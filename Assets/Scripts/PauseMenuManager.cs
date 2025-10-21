using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{

    public GameObject PauseMenu_Panel;
    private bool _isPaused = false;
    public ResultManager resultManager;

    void Awake()
    {
        resultManager = GetComponent<ResultManager>();
    }

    public void PauseGame()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        PauseMenu_Panel.SetActive(true);

        //To make cursor active and visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        PauseMenu_Panel.SetActive(false);

        //To make cursor active and visible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void LoadMainMenu()
    {
        resultManager.LoadMainMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

    }
}
