using System;
using UnityEngine;

public class WizardLevel : Wizard
{
    [Header("Level")]
    [SerializeField] private LevelInfos levelInfos;

    public void ActiveWizardLevel(Action action, LevelInfos levelInfos)
    {
        // Set infos
        this.levelInfos = levelInfos;

        ActiveWizard(action);

        SetContents();
    }

    private void SetContents()
    {
        title.text = levelInfos.LevelId.ToString();
        content.text = levelInfos.Description;
    }
}