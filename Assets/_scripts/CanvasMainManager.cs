using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasMainManager : MonoBehaviour
{
    public static CanvasMainManager Instance;

    public Wizard Wizard;

    void Awake()
    {
        Instance = this;
        Debug.Log("CanvasMainManager is awake");

        Wizard.Close();
    }


    public void SetWizard(LevelInfos _levelInfos)
    {
        Wizard.SetInfos(_levelInfos);
    }
}
