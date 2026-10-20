using System;
using System.Collections.Generic;
using UnityEngine;

public class IntroController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject introPanel;

    [Tooltip("Assign the text pages in their display order.")]
    [SerializeField]
    private GameObject[] introTextObjects =
        Array.Empty<GameObject>();

    [Header("Gameplay During Introduction")]
    [Tooltip(
        "Assign LookAtPromptController here. " +
        "Optionally also assign movement and world-interaction scripts. " +
        "Do not assign the UI EventSystem or this controller.")]
    [SerializeField]
    private Behaviour[] gameplayScriptsToPause =
        Array.Empty<Behaviour>();

    private readonly Dictionary<Behaviour, bool> previousEnabledStates =
        new Dictionary<Behaviour, bool>();

    private int currentIndex;
    private bool finished;

    private void Start()
    {
        currentIndex = 0;

        PauseGameplayScripts();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerState(
                EnumPlayerState.Intro);
        }

        if (introPanel != null)
            introPanel.SetActive(true);

        ShowCurrentPage();
    }

    private void PauseGameplayScripts()
    {
        foreach (Behaviour script in gameplayScriptsToPause)
        {
            if (script == null ||
                script == this ||
                previousEnabledStates.ContainsKey(script))
            {
                continue;
            }

            previousEnabledStates.Add(script, script.enabled);
            script.enabled = false;
        }
    }

    private void RestoreGameplayScripts()
    {
        foreach (var entry in previousEnabledStates)
        {
            if (entry.Key != null)
                entry.Key.enabled = entry.Value;
        }

        previousEnabledStates.Clear();
    }

    private void ShowCurrentPage()
    {
        for (int i = 0; i < introTextObjects.Length; i++)
        {
            if (introTextObjects[i] != null)
                introTextObjects[i].SetActive(i == currentIndex);
        }
    }

    public void OnNextButtonClicked()
    {
        if (finished)
            return;

        currentIndex++;

        if (currentIndex < introTextObjects.Length)
        {
            ShowCurrentPage();
            return;
        }

        FinishIntro();
    }

    private void FinishIntro()
    {
        finished = true;

        if (introPanel != null)
            introPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerState(EnumPlayerState.Walking);
        }
        else
        {
            Debug.LogError(
                "Cannot start the workday: GameManager is missing.",
                this);
        }

        RestoreGameplayScripts();
    }
}