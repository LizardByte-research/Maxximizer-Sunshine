using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject appListWindow;
    public GameObject settingsWindow;
    public GameObject restartWindow;

    private void Awake()
    {
        Screen.SetResolution(1280, 720,false);
    }
    private void OnEnable()
    {
        Application.targetFrameRate = 60;
    }

    public void SelectAppList()
    {
        appListWindow.SetActive(true);
        settingsWindow.SetActive(false);
        this.gameObject.SetActive(false);
        restartWindow.SetActive(false);
    }

    public void SelectMainMenu()
    {
        this.gameObject.SetActive(true);
        appListWindow.SetActive(false);
        settingsWindow.SetActive(false);
        restartWindow.SetActive(false);
    }

    public void SelectSettingsMenu()
    {
        appListWindow.SetActive(false);
        settingsWindow.SetActive(true);
        this.gameObject.SetActive(false);
        restartWindow.SetActive(false);
    }

    public void SelectRestartMenu()
    {
        appListWindow.SetActive(false);
        settingsWindow.SetActive(false);
        this.gameObject.SetActive(false);
        restartWindow.SetActive(true);
    }

    public void RestartServices()
    {
        Process[] pname2 = Process.GetProcessesByName("moonlighthost");
        if (pname2.Length != 0) pname2[0].Kill();

        SelectMainMenu();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
