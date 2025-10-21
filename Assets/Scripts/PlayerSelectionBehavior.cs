using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class PlayerSelectionBehavior : MonoBehaviour
{
    public GameObject PlyrCharacter;

    private GameObject[] _Characters;
    private int _CurrIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Initialize the character array
        _Characters = new GameObject[PlyrCharacter.transform.childCount];

        //Populate the character array and disable all characters
        for (int i = 0; i < PlyrCharacter.transform.childCount; i++)
        {
            _Characters[i] = PlyrCharacter.transform.GetChild(i).gameObject;
            _Characters[i].SetActive(false);
        }

        //Load the selected character index from PlayerPrefs
        if (PlayerPrefs.HasKey("SelectedCharacterIndex"))
        {
            _CurrIndex = PlayerPrefs.GetInt("SelectedCharacterIndex");
        }

        SelectedCharacter();
    }

    void SelectedCharacter()
    {
        //Disable every other characters
        foreach (GameObject Character in _Characters)
        {
            Character.SetActive(false);
        }
        //Only show the selected character
        _Characters[_CurrIndex].SetActive(true);
    }

    public void NextCharacter()
    {
        _CurrIndex = (_CurrIndex + 1) % _Characters.Length;
        SelectedCharacter();

    }

    public void PrevCharacter()
    {
        _CurrIndex = (_CurrIndex - 1 + _Characters.Length) % _Characters.Length;
        SelectedCharacter();
    }
    
    public void OnConfirmedPlyrSelection(string SceneName)
    {
        //Save the selected character index to PlayerPrefs
        PlayerPrefs.SetInt("SelectedCharcterIndex", _CurrIndex);
        PlayerPrefs.Save();

        SceneManager.LoadScene(SceneName);
    }
    
}
