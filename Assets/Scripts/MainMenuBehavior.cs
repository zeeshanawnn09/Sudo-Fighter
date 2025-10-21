using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.UI;
using System;

public class MainMenuBehavior : MonoBehaviour
{
    public GameObject MainMenu_Panel;
    public GameObject Player_MapSelection_Panel;
    public GameObject Options_Panel;
    public GameObject Controls_Panel;

    public 

    void Start()
    {
        MainMenu_Panel.SetActive(true);
        Player_MapSelection_Panel.SetActive(false);
        Options_Panel.SetActive(false);
        Controls_Panel.SetActive(false);
    }

    public void OnStartButtonClicked()
    {
        MainMenu_Panel.SetActive(false);
        Player_MapSelection_Panel.SetActive(true);
    }

    public void PlayerSelection(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    public void MapSelection(string SceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneName);
    }

    public void OnOptionsButtonClicked()
    {
        MainMenu_Panel.SetActive(false);
        Options_Panel.SetActive(true);
    }

    public void OnControlButtonClicked()
    {
        Options_Panel.SetActive(false);
        Controls_Panel.SetActive(true);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void OnBackButtonClicked()
    {
        MainMenu_Panel.SetActive(true);
        Player_MapSelection_Panel.SetActive(false);
        Options_Panel.SetActive(false);
        Controls_Panel.SetActive(false);
    }


}
