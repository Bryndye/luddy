using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Luddy.Children;

public class QCMContent : MonoBehaviour
{
    Action OnEndQCM;
    AuthManager authManager;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI question;
    [SerializeField] private GameObject answersParent;
    [SerializeField] private Image illustration;

    [Header("Prefabs réponse")]
    [SerializeField] private Toggle togglePrefab;
    [SerializeField] private Toggle toggleUniquePrefab;
    [SerializeField] private TMP_InputField inputFieldPrefab;

    [Header("Current QCM")]
    [SerializeField] private LevelInfos levelInfos;
    private ContentCreation currentQuestion;
    [SerializeField] private int questionIndex = 0;
    public List<GameObject> answers = new List<GameObject>();
    public List<Toggle> toggles = new List<Toggle>();
    [Tooltip("Laisser vide, champ automatique")] private TMP_InputField inputFieldAnswer;
    [SerializeField] private LevelDatasPlayer levelDatasPlayer;
    private DateTime questionStarted;

    [Header("Final/ Reveal")]
    [SerializeField] private GameObject revealParent;
    [SerializeField] private AnswerReveal AnswerRevealPrefab;
    [SerializeField] private Transform contentAnswerReveal1;
    [SerializeField] private Transform contentAnswerReveal2;
    [SerializeField] private Transform contentAnswerReveal3;
    [SerializeField] private TextMeshProUGUI textEndReveal;
    [SerializeField] private TextMeshProUGUI textMonTemps;
    [SerializeField] private TextMeshProUGUI textTempsReach;
    [SerializeField] private TextMeshProUGUI textDys;
    public List<List<string>> currentAnswers = new List<List<string>>();

    private void Start()
    {
        authManager = AuthManager.Instance;
    }

    public void ActiveQCM(Action action, LevelInfos levelInfos)
    {
        this.levelInfos = levelInfos;
        OnEndQCM = action;

        gameObject.SetActive(true);
        revealParent.SetActive(false);
        questionIndex = 0;
        // Permet d'effacer les anciennes datas
        levelDatasPlayer = new LevelDatasPlayer(levelInfos);
        currentAnswers.Clear();
        NextQuestion();
    }

    public void NextQuestion()
    {
        // questionIndex 
        if (questionIndex >= levelInfos.ContentCreationList.Count)
        {
            ActiveAnswerReveal();
            return;
        }
        currentQuestion = levelInfos.ContentCreationList[questionIndex];
        SetDatasInContent();

        questionIndex++;
    }

    private void ActiveAnswerReveal()
    {
        Debug.Log("Fin du QCM");
        // Ajouter ecran de fin avec les stats (levelDatasPlayer)

        // Gestion des datas du level pour le player
        var level = authManager.MyCurrentSubAccount.MyLevelDatasPlayer.Where(i => i.LevelId == levelInfos.LevelId).FirstOrDefault();
        if (level != null && level.PourcentagePass < levelDatasPlayer.PourcentagePass)
        {
            levelDatasPlayer.IsFinished = IsLevelPassed();
            authManager.MyCurrentSubAccount.AddLevelDataPlayer(levelDatasPlayer);
            Debug.Log("Meilleur score");
        }
        else if (level != null)
        {
            Debug.Log("Pire score");
        }
        else if (level == null)
        {
            levelDatasPlayer.IsFinished = IsLevelPassed();
            authManager.MyCurrentSubAccount.AddLevelDataPlayer(levelDatasPlayer);
            // time deja enregistre
        }
            // MAJ des datas
            authManager.MyAccount.SetDatas();

        // Création des réponses pour le joueur
        for (int i = 0; i < contentAnswerReveal1.childCount; i++)
        {
            Destroy(contentAnswerReveal1.GetChild(i).gameObject);
        }
        for (int i = 0; i < contentAnswerReveal2.childCount; i++)
        {
            Destroy(contentAnswerReveal2.GetChild(i).gameObject);
        }
        for (int i = 0; i < contentAnswerReveal3.childCount; i++)
        {
            Destroy(contentAnswerReveal3.GetChild(i).gameObject);
        }

        int _indexTempColumn = 0;
        for (int i = 0; i < levelInfos.ContentCreationList.Count; i++)
        {
            // Passer la réponse quand c'est une question Void
            if (levelInfos.ContentCreationList[i].MyQuestionType != QuestionType.Void)
            {
                int columnIndex = _indexTempColumn % 3;
                Transform _parent = null;
                if (columnIndex == 0)
                {
                    _parent = contentAnswerReveal1;
                }
                else if (columnIndex == 1)
                {
                    _parent = contentAnswerReveal2;
                }
                else if (columnIndex == 2)
                {
                    _parent = contentAnswerReveal3;
                }
                AnswerReveal _a = Instantiate(AnswerRevealPrefab, _parent);
                _a.SetAnswer(levelDatasPlayer.HasPassedQuestions[i], currentAnswers[i]);
                _indexTempColumn++;
            }
        }

        textEndReveal.text = IsLevelPassed() ? "Bravo ! \n Tu as réussi le niveau !" : 
            "Dommage...\n Retente et tu réussira !";

        // Average time for this level
        // TEMPS AFFICHAGE TEXTE
        textMonTemps.text = "Votre temps moyen : "+ AverageTimeForThisLevel() + "s\r\nTemps Total : "+ TotalTimeForThisLevel() + "s";
        textTempsReach.text = levelInfos.AverageTimeToFinish() + "s : Temps moyen à avoir ";

        textDys.gameObject.SetActive(AverageTimeForThisLevel() > levelInfos.AverageTimeToFinish());
        
        revealParent.SetActive(true);
    }

    private void SetDatasInContent()
    {
        question.text = currentQuestion.MyQuestion;
        if (currentQuestion.MyIllustration)
        {
            illustration.gameObject.SetActive(true);
            illustration.sprite = currentQuestion.MyIllustration;
        }
        else
        {
            illustration.gameObject.SetActive(false);
            illustration.sprite = null;
        }

        questionStarted = DateTime.Now;

        InstantiateContents();
    }

    private void InstantiateContents()
    {
        ClearAnswersGO();
        
        // Void Section
        answersParent.SetActive(true);

        if (currentQuestion.MyQuestionType == QuestionType.Void)
        {
            answersParent.SetActive(false);
        }

        foreach (Answer item in currentQuestion.MyAnswers)
        {
            switch (currentQuestion.MyQuestionType)
            {
                case QuestionType.MultipleChoice:
                    Toggle answerGO = Instantiate(togglePrefab, answersParent.transform);

                    answerGO.GetComponentInChildren<TextMeshProUGUI>().text = item.MyValue;
                    answerGO.isOn = false;

                    answers.Add(answerGO.gameObject);
                    toggles.Add(answerGO);
                    break;

                case QuestionType.UniqueChoice:
                    Toggle answerUniqueGO = Instantiate(toggleUniquePrefab, answersParent.transform);

                    answerUniqueGO.name = "Toggle Unique : " + item.MyValue;
                    answerUniqueGO.GetComponentInChildren<TextMeshProUGUI>().text = item.MyValue;
                    answerUniqueGO.isOn = false;

                    // Ajoutez un flag pour éviter les boucles
                    bool isUpdating = false;

                    answerUniqueGO.onValueChanged.AddListener((value) =>
                    {
                        if (isUpdating) return; // Évite une boucle infinie

                        isUpdating = true;

                        // Désactiver les autres toggles
                        foreach (Toggle toggle in toggles)
                        {
                            if (toggle != answerUniqueGO) // Ne pas modifier le Toggle courant
                            {
                                toggle.isOn = false;
                            }
                        }

                        // Synchroniser l'état du toggle actuel
                        answerUniqueGO.isOn = value;

                        isUpdating = false;
                    });

                    answers.Add(answerUniqueGO.gameObject);
                    toggles.Add(answerUniqueGO);
                    break;

                case QuestionType.Input:
                    TMP_InputField answerInputGO = Instantiate(inputFieldPrefab, answersParent.transform);
                    answers.Add(answerInputGO.gameObject);
                    inputFieldAnswer = answerInputGO;
                    break;

                default:
                    break;
            }
        }
    }

    private void ClearAnswersGO()
    {
        foreach (Transform item in answersParent.transform)
        {
            Destroy(item.gameObject);
        }

        answers.Clear();
        toggles.Clear();
    }

    public void ValidateAnswer()
    {
        bool isGoodAnswer = false;
        List<string> currentAnswersThisQuestion = new List<string>();

        // Vérifier si la réponse est bonne
        switch (currentQuestion.MyQuestionType)
        {
            case QuestionType.MultipleChoice:
                int count = 0;
                int countGoodAnswer = 0;
                foreach (Answer anwser in currentQuestion.MyAnswers)
                {
                    if (anwser.MyAnswerType == AnswerType.Vrai)
                    {
                        countGoodAnswer++;
                    }
                    currentAnswersThisQuestion.Add(anwser.MyValue);
                }
                for (int i = 0; i < toggles.Count; i++)
                {
                    if (toggles[i].isOn)
                    {
                        isGoodAnswer = currentQuestion.MyAnswers[i].MyAnswerType == AnswerType.Vrai;
                        if (isGoodAnswer)
                            count++;
                    }
                }
                if (isGoodAnswer)
                    isGoodAnswer = count == countGoodAnswer;
                break;

            case QuestionType.UniqueChoice:
                for (int i = 0; i < toggles.Count; i++)
                {
                    if (toggles[i].isOn)
                    {
                        isGoodAnswer = currentQuestion.MyAnswers[i].MyAnswerType == AnswerType.Vrai;
                        currentAnswersThisQuestion.Add(currentQuestion.MyAnswers[i].MyValue);
                    }
                }
                break;

            case QuestionType.Input:
                string validateAnswer = inputFieldAnswer.text.ToLower();
                string currentAnswer = currentQuestion.MyAnswers[0].MyValue.ToLower();
                isGoodAnswer = validateAnswer == currentAnswer;
                currentAnswersThisQuestion.Add(currentAnswer);
                break;

            case QuestionType.Void:
                isGoodAnswer = true;
                break;

            default:
                break;
        }

        //Debug.Log("Bonne réponse : " + isGoodAnswer);
        levelDatasPlayer.AddHasPassedQuestion(isGoodAnswer);

        TimeSpan difference = DateTime.Now - questionStarted;
        float seconds = Mathf.Round((float)difference.TotalSeconds);
        levelDatasPlayer.AddTime(seconds);
        //Debug.Log("Time to finish this question : " + seconds);

        currentAnswers.Add(currentAnswersThisQuestion);

        NextQuestion();
    }

    private bool IsLevelPassed()
    {
        return levelDatasPlayer.PourcentagePass >= ((float)levelInfos.PourcentageToPass / 100);
    }

    private float AverageTimeForThisLevel()
    {
        float _averageTime = 0f;
        foreach (float _time in levelDatasPlayer.TimeForEachQuestions)
        {
            _averageTime += _time;
        }
        return _averageTime / levelDatasPlayer.TimeForEachQuestions.Count;
    }

    private float TotalTimeForThisLevel()
    {
        float _timeTotal = 0;
        foreach (float _time in levelDatasPlayer.TimeForEachQuestions)
        {
            _timeTotal += _time;
        }

        return _timeTotal;
    }

    public void EndQCM()
    {
        // cloturer le level
        gameObject.SetActive(false);
        OnEndQCM?.Invoke();
    }
}