using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using NaughtyAttributes;
using System;
using JetBrains.Annotations;
using Unity.VisualScripting;
using System.Linq;

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
    public int Level = 0;
    public string Description = "Careful!";
    [Scene]
    public string LevelIdScene;

    [Header("In Game")]
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
    public string MyQuestion = " ?";
    public string MyDescriptionHelp = "Think faster!";
    public QuestionType MyQuestionType;

    [Tooltip("Si c'est en unique choice, mettre 1 seule valeur dans la liste et en Vraie.")]
    public List<Answer> MyAnswers;

}

[Serializable]
public class LevelDatasPlayer
{
    public int LevelId;
    public bool IsFinished = false;
    public float pourcentagePass { get {return ComputePourcentage(); } }
    [Tooltip("Resultats pour chaque question dans l'odre si c'est reussi ou non.")]
    public List<bool> HasPassedQuestions = new List<bool>();

    private float ComputePourcentage()
    {
        return HasPassedQuestions.Count(i => i);
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