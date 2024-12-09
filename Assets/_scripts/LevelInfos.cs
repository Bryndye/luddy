using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using NaughtyAttributes;
using System;
using JetBrains.Annotations;
using Unity.VisualScripting;

public enum LevelType
{
    qcm, puzzle
}
[CreateAssetMenu]
public class LevelInfos : ScriptableObject
{
    [Header("General")]
    public string Name = "1 - 1";
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