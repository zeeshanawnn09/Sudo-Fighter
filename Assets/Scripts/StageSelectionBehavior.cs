using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectionBehavior : MonoBehaviour
{
    public void StageSelection(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
}
