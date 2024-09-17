using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    public static MenuManager InstanceMM;

    [SerializeField] Menu[] menus;

    private void Awake()
    {
        InstanceMM = this;
        for (int i = 0; i < menus.Length; i++)
            CloseMenu(menus[i]);
    }

    public void OpenMenu(Menu menu)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].Openned)
            {
                CloseMenu(menus[i]);
            }
        }
        menu.Open();
    }

    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
