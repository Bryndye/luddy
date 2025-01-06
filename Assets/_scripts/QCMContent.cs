using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QCMContent : MonoBehaviour
{
    Action OnEndQCM;
    AuthManager authManager;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI question;
    [SerializeField] private GameObject answersParent;

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

    private void Start()
    {
        authManager = AuthManager.Instance;
    }

    public void ActiveQCM(Action action, LevelInfos levelInfos)
    {
        this.levelInfos = levelInfos;
        OnEndQCM = action;

        gameObject.SetActive(true);
        questionIndex = 0;
        // Permet d'effacer les anciennes datas
        levelDatasPlayer = new LevelDatasPlayer(levelInfos);
        NextQuestion();
    }

    public void NextQuestion()
    {
        // questionIndex 
        if (questionIndex >= levelInfos.ContentCreationList.Count)
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
            else
            {
                levelDatasPlayer.IsFinished = IsLevelPassed();
                authManager.MyCurrentSubAccount.AddLevelDataPlayer(levelDatasPlayer);
                Debug.Log("Pire/Pas de score");
            }
            // MAJ des datas
            authManager.MyAccount.SetDatas();

            // cloturer le level
            gameObject.SetActive(false);
            OnEndQCM?.Invoke();
            return;
        }
        currentQuestion = levelInfos.ContentCreationList[questionIndex];
        SetDatasInContent();

        questionIndex++;
    }

    private void SetDatasInContent()
    {
        question.text = currentQuestion.MyQuestion;

        InstantiateContents();
    }

    private void InstantiateContents()
    {
        ClearAnswersGO();

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
                    }
                }
                break;

            case QuestionType.Input:
                string validateAnswer = inputFieldAnswer.text.ToLower();
                string currentAnswer = currentQuestion.MyAnswers[0].MyValue.ToLower();
                isGoodAnswer = validateAnswer == currentAnswer;
                break;

            default:
                break;
        }

        Debug.Log("Bonne réponse : " + isGoodAnswer);
        levelDatasPlayer.AddHasPassedQuestion(isGoodAnswer);

        NextQuestion();
    }

    private bool IsLevelPassed()
    {
        return levelDatasPlayer.PourcentagePass >= (levelDatasPlayer.PourcentagePass / 100);
    }
}