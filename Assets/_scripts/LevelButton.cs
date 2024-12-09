using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelButton : MonoBehaviour
{
    public LevelInfos Infos;
    [SerializeField] private TextMeshProUGUI m_TextMeshPro;

    private void Awake()
    {
        m_TextMeshPro.text = Infos.name;
    }

    public void OnClick()
    {
        // Do something
        CanvasMainManager.Instance?.OpenLevel(Infos);
    }
}