using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public bool Openned;

    public void Open()
    {
        Openned = true;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        Openned = false;
        gameObject.SetActive(false);
    }
}
