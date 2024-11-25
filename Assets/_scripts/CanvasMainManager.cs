using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasMainManager : MonoBehaviour
{
    public static CanvasMainManager Instance;

    public WizardLevel Wizard;

    void Awake()
    {
        Instance = this;

        Wizard.Close();
    }


    public void SetWizard(LevelInfos levelInfos)
    {
        Wizard.ActiveWizardLevel(() =>
        {
            Debug.Log("Lancement de la partie : " + levelInfos.Name);
        }, levelInfos);
    }
}
