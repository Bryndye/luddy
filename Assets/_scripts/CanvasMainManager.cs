using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasMainManager : MonoBehaviour
{
    public static CanvasMainManager Instance;

    public WizardLevel Wizard;

    [Header("Level Type")]
    [SerializeField] private GameObject levelsParent;
    [SerializeField] private QCMContent qcmContent;

    void Awake()
    {
        Instance = this;

        Wizard.Close();

        levelsParent.SetActive(false);
    }


    public void OpenLevel(LevelInfos levelInfos)
    {
        // Activation du Wizard pour annoncer le niveau
        Wizard.ActiveWizardLevel(() =>
        {
            Debug.Log("Lancement de la partie : " + levelInfos.Name);
            levelsParent.SetActive(true);

            // QCM BY DEFAULT
            qcmContent.ActiveQCM(()=> { 
                CloseLevel();
            },
            levelInfos);

        }, levelInfos);
    }

    public void CloseLevel()
    {
        levelsParent.SetActive(false);
    }
}
