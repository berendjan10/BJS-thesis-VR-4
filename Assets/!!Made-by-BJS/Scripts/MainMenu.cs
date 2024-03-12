using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Toggle avatar1Toggle;
    public Toggle thirdPersonPerspective;

    // Start is called before the first frame update
    public void PlayGame()
    {
        PlayerPrefs.SetInt("3PP", thirdPersonPerspective.isOn ? 1 : 0);
        PlayerPrefs.Save();
        if (avatar1Toggle.isOn)
        {
            SceneManager.LoadScene("NatureMale");
        }
        else
        {
            SceneManager.LoadScene("NatureFemale");
        }
    }


    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
