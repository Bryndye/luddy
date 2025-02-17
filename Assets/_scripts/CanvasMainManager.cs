using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasMainManager : MonoBehaviour
{
    public static CanvasMainManager Instance;

    public WizardLevel Wizard;

    [Header("World Map")]
    [SerializeField] private Transform worldMapContent;

    [Header("Level Type")]
    [SerializeField] private GameObject levelsParent;
    [SerializeField] private QCMContent qcmContent;

    void Awake()
    {
        Instance = this;

        Wizard.Close();

        levelsParent.SetActive(false);
    }

    private void Start()
    {
        // Force la position de la world map a etre au debut
        if (worldMapContent)
        {
            worldMapContent.position = new Vector3(
                0, 
                worldMapContent.position.y, 
                worldMapContent.position.z);
        }
    }


    public void OpenLevel(LevelInfos levelInfos)
    {
        // Activation du Wizard pour annoncer le niveau
        Wizard.ActiveWizardLevel(() =>
        {
            levelsParent.SetActive(true);

            // QCM BY DEFAULT
            qcmContent.ActiveQCM(()=> { 
                CloseLevel();
                FindAllLevelButtons();
            },
            levelInfos);

        }, levelInfos);
    }

    public void CloseLevel()
    {
        levelsParent.SetActive(false);
    }

    void FindAllLevelButtons()
    {
        // Récupère tous les objets qui ont un LevelButton
        LevelButton[] levelButtons = GameObject.FindObjectsByType<LevelButton>(FindObjectsSortMode.None);

        // Convertit en liste de GameObjects
        //List<GameObject> levelButtonObjects = new List<GameObject>();

        foreach (LevelButton btn in levelButtons)
        {
            btn.RefreshMyState();
        }

        // Affiche les noms des objets trouvés
        //foreach (GameObject go in levelButtonObjects)
        //{
        //    Debug.Log("LevelButton trouvé sur : " + go.name);
        //}
    }
}
