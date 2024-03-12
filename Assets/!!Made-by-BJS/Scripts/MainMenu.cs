using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Toggle maleAvatarToggle;
    public Toggle thirdPersonPerspective;

    // Start is called before the first frame update
    public void PlayGame()
    {
        PlayerPrefs.SetInt("3PP", thirdPersonPerspective.isOn ? 1 : 0);
        PlayerPrefs.Save();
        if (maleAvatarToggle.isOn)
        {
            PlayerPrefs.SetInt("gender", 0);
            PlayerPrefs.Save();

            SceneManager.LoadScene("NatureMale");
        }
        else
        {
            PlayerPrefs.SetInt("gender", 1);
            PlayerPrefs.Save();

            SceneManager.LoadScene("NatureFemale");
        }
    }


    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
