using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum LevelState { Lock, Unlock, Pass}
public class LevelButton : MonoBehaviour
{
    private AuthManager authManager;
    private Button myButton;
    [SerializeField] private LevelState levelState;
    public LevelInfos Infos;
    [SerializeField] private TextMeshProUGUI m_TextMeshPro;

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
        // Si on retrouve le level dans les datas, il est débloque => Unlock
        var hasExistLevels = authManager.MyCurrentSubAccount.MyLevelDatasPlayer.Where(i => i.LevelId == Infos.LevelId).FirstOrDefault();
        if (hasExistLevels != null)
        {
            // Si IsFinished = true, alors => Pass
            if (hasExistLevels.IsFinished)
                ChangeLevelState(LevelState.Pass);
            else 
                ChangeLevelState(LevelState.Unlock);
        }
        // Si on ne le retrouve pas, il n'a jamais ete joue => Lock
        else
        {
            // Si le dernier level est son -1, alors il est le suivant => Unlock
            if (authManager.MyCurrentSubAccount.MyLevelDatasPlayer.Count + 1 == Infos.LevelId)
                ChangeLevelState(LevelState.Unlock);
            else
                ChangeLevelState(LevelState.Lock);
        }
    }

    private void ChangeLevelState(LevelState levelState)
    {
        this.levelState = levelState;
        switch (levelState)
        {
            case LevelState.Lock:
                myButton.interactable = false;
                break;
            case LevelState.Unlock:
                myButton.interactable = true;
                break;
            case LevelState.Pass:
                myButton.interactable= true;
                break;
            default:
                break;
        }
    }

    public void OnClick()
    {
        // Do something
        CanvasMainManager.Instance?.OpenLevel(Infos);
    }
}