using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Wizard : MonoBehaviour, IPointerClickHandler
{

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI content;

    public void SetInfos(LevelInfos _levelInfos)
    {
        // Set infos
        title.text = _levelInfos.Name;

        // Active GO wizard
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerPress == gameObject)
        {
            //Debug.Log(gameObject.name);
            Close();
        }
        else
        {
            // Do nothing
        }
    }
}