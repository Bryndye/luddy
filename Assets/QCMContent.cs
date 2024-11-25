using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QCMContent : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI question;
    [SerializeField] private GameObject answersParent;

    [Header("Prefabs réponse")]
    [SerializeField] private Toggle togglePrefab;
    [SerializeField] private TMP_InputField inputFieldPrefab;

    [Header("Current QCM")]
    [SerializeField] private LevelInfos levelInfos;
    private ContentCreation currentQuestion;
    [SerializeField] private int questionIndex = 0;

    public void ActiveQCM(LevelInfos levelInfos)
    {
        this.levelInfos = levelInfos;

        gameObject.SetActive(true);
    }

    public void SetNextQuestion()
    {
        // questionIndex 
        currentQuestion = levelInfos.ContentCreationList[questionIndex];
        SetDatasInContent();

        questionIndex++;
    }

    private void SetDatasInContent()
    {
        question.text = currentQuestion.MyQuestion;

        switch (currentQuestion.MyQuestionType)
        {
            case QuestionType.MultipleChoice:
                // foreach
                break;
            case QuestionType.UniqueChoice:
                // foreach
                // Add listener, annule le precedent choisi
                break;
            case QuestionType.Input:
                // Instantiate inputField
                break;
            default:
                break;
        }
    }
}
