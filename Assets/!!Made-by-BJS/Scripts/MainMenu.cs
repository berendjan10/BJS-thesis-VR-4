using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Toggle maleAvatarToggle;
    public Toggle thirdPersonPerspective;
    public GameObject instruction0;
    private inFronOfCamera instruction0script;
    public TextMeshProUGUI textMeshProToChange;
    public GameObject playerIsNotReady;
    public GameObject NO;
    public GameObject YES;


    private void Start()
    {
        instruction0script = instruction0.GetComponent<inFronOfCamera>();
        textMeshProToChange.color = Color.red;
        playerIsNotReady.SetActive(false);
    }
    public void Update()
    {
        if (!instruction0script.ready) { NO.SetActive(true); YES.SetActive(false); }
        else { NO.SetActive(false); YES.SetActive(true); }
    }

    public void PlayGame() // runs when START is pressed
    {
        if (!instruction0script.ready)
        {
            StartCoroutine(notReady());
        }
        else
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
    }


    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }

    IEnumerator notReady()
    {
        float clock = 0.0f;
        playerIsNotReady.SetActive(true);
        while (clock < 0.5f)
        {
            clock += Time.deltaTime;
            yield return null;
        }
        playerIsNotReady.SetActive(false);
    }

}
