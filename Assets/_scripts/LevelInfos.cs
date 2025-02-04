using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum LevelType
{
    qcm, puzzle
}
[CreateAssetMenu]
public class LevelInfos : ScriptableObject
{
    [Header("General")]
    public int LevelId = 1;
    public LevelType MyLevelType;
    public string Description = "Careful!";

    [Header("In Game")]
    [MinValue(0), MaxValue(100)] public int PourcentageToPass = 80;
    public List<ContentCreation> ContentCreationList;
}

public enum QuestionType
{
    MultipleChoice, UniqueChoice, Input
}
public enum AnswerType
{
    Vrai, Faux
}

[Serializable]
public class Answer
{
    public string MyValue;
    public AnswerType MyAnswerType = AnswerType.Faux;
}

[Serializable]
public class ContentCreation
{
    [TextArea(3, 10)]
    public string MyQuestion = " ?";
    public string MyDescriptionHelp = "Think faster!";
    public QuestionType MyQuestionType;

    [Tooltip("Si c'est en unique choice, mettre 1 seule valeur dans la liste et en Vraie.")]
    public List<Answer> MyAnswers;
    public List<Answer> GetRightAnswers()
    {
        List<Answer> rightAnswers = new List<Answer>();
        foreach (Answer answer in MyAnswers)
        {
            if (answer.MyAnswerType == AnswerType.Vrai)
            {
                rightAnswers.Add(answer);
            }
        }
        return rightAnswers;
    }

    public string GetRightAnswersString()
    {
        string answer = "";
        foreach (Answer item in GetRightAnswers())
        {
            answer += item.MyValue.ToString() + "\n";
        }
        return answer;
    }
}

[Serializable]
public class LevelDatasPlayer
{
    public int LevelId;
    public bool IsFinished = false;
    public float PourcentagePass { get { return ComputePourcentage(); } }
    [Tooltip("Resultats pour chaque question dans l'odre si c'est reussi ou non.")]
    public List<bool> HasPassedQuestions = new List<bool>();

    private float ComputePourcentage()
    {
        if (HasPassedQuestions == null || HasPassedQuestions.Count == 0)
            return 0; // Éviter une division par zéro

        int trueCount = HasPassedQuestions.Count(i => i);
        return (trueCount / (float)HasPassedQuestions.Count);
    }

    public LevelDatasPlayer(LevelInfos levelInfos)
    {
        LevelId = levelInfos.LevelId;
    }

    public void AddHasPassedQuestion(bool hasPassed)
    {
        HasPassedQuestions.Add(hasPassed);
    }
}