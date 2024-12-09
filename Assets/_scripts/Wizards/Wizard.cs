using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Wizard : MonoBehaviour
{
    protected Action onFinished;

    [Header("UI")]
    [SerializeField] protected TextMeshProUGUI title;
    [SerializeField] protected TextMeshProUGUI content;

    [SerializeField] protected Button confirmationButton;
    [SerializeField] protected Button cancelButton;

    private void Awake()
    {
        Close();
    }

    public void ActiveWizard(Action action)
    {
        gameObject.SetActive(true);

        onFinished = action;

        // FIX et je ne sais pas comment 
        gameObject.SetActive(true);

        confirmationButton.onClick.RemoveAllListeners();
        confirmationButton.onClick.AddListener(OnConfirmation);

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(OnCancel);
    }

    public void ActiveWizard(Action action, string title, string content = null)
    {
        ActiveWizard(action);

        this.title.text = title;
        this.content.text = content;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    #region Buttons
    private void OnConfirmation()
    {
        onFinished?.Invoke();
        Close();
    }

    private void OnCancel()
    {
        Close();
    }
    #endregion
}