using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance;

    public Wizard Wizard;

    private void Awake()
    {
        Instance = this;

        Wizard.Close();
    }


    public void SetWizard(LevelInfos _levelInfos)
    {
        Wizard.SetInfos(_levelInfos);
    }
}
