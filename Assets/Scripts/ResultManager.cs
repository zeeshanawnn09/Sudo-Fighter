using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    public GameObject ResultPanel;
    public Text Result_txt;

    public FightingController[] fightingControllers;
    public OpponentAIController[] opponentAIControllers;

    void DisplayResult(string result)
    {
        Result_txt.text = result;
        ResultPanel.SetActive(true);
        Time.timeScale = 0f; //To Pause the game
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void FightResult()
    {
        foreach (FightingController fightingControllers in fightingControllers)
        {
            if (fightingControllers.gameObject.activeSelf && fightingControllers.currHP <= 0)
            {
                DisplayResult("You Lost");
            }
        }
        foreach (OpponentAIController opponentAIControllers in opponentAIControllers)
        {
            if (opponentAIControllers.gameObject.activeSelf && opponentAIControllers.currHP <= 0)
            {
                DisplayResult("You Won!");
            }
        }
    }

    void Update()
    {
        FightResult();
    }

}
