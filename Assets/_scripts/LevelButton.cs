using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum LevelState { Lock, Unlock,UnlockFirst, Pass}
public class LevelButton : MonoBehaviour
{
    private AuthManager authManager;
    private Button myButton;
    [SerializeField] private LevelState levelState;
    public LevelInfos Infos;
    [SerializeField] private TextMeshProUGUI m_TextMeshPro;
    [SerializeField] private Image Mongol;

    private void Awake()
    {
        myButton = GetComponent<Button>();

        m_TextMeshPro.text = Infos.name;
    }

    private void Start()
    {
        authManager = AuthManager.Instance;

        RefreshMyState();
    }

    public void RefreshMyState()
    {
        var levels = authManager.MyCurrentSubAccount.MyLevelDatasPlayer;

        // Vérifier si le niveau actuel existe
        LevelDatasPlayer currentLevelData = levels.FirstOrDefault(l => l.LevelId == Infos.LevelId);

        if (currentLevelData != null)
        {
            // Si le niveau est terminé => Pass
            if (currentLevelData.IsFinished)
            {
                ChangeLevelState(LevelState.Pass);
            }
            // Si le niveau a été commencé mais pas terminé => Unlock
            else
            {
                ChangeLevelState(LevelState.Unlock);
            }
        }
        else
        {
            if (levels.Count >= 0 && Infos.LevelId == levels.Count +1)
            {
                ChangeLevelState(LevelState.Unlock);
                return;
            }

            // Trouver le dernier niveau terminé
            var lastFinishedLevel = levels
                .Where(l => l.IsFinished)
                .OrderByDescending(l => l.LevelId)
                .FirstOrDefault();

            // Si le dernier niveau terminé est juste avant le niveau actuel => Unlock sinon Lock
            if (lastFinishedLevel != null && lastFinishedLevel.LevelId + 1 == Infos.LevelId)
            {
                ChangeLevelState(LevelState.Unlock);
            }
            else
            {
                ChangeLevelState(LevelState.Lock);
            }
        }
    }


    private void ChangeLevelState(LevelState levelState)
    {
        this.levelState = levelState;
        switch (levelState)
        {
            case LevelState.Lock:
                myButton.interactable = false;
                Mongol.gameObject.SetActive(false);
                break;
            case LevelState.Unlock:
                myButton.interactable = true;
                Mongol.gameObject.SetActive(true);
                break;
            case LevelState.UnlockFirst:
                myButton.interactable = true;
                Mongol.gameObject.SetActive(false);
                break;
            case LevelState.Pass:
                myButton.interactable= true;
                Mongol.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    public void OnClick()
    {
        CanvasMainManager.Instance?.OpenLevel(Infos);
    }
}