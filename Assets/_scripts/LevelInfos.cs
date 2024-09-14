using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using NaughtyAttributes;
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
    public string DescriptionHelp = "Think faster!";
}
