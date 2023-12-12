using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class embodiment_questionnaire_script : MonoBehaviour
{
    public GameObject questionnaireTarget;
    public List<Toggle> answerToggles; // assign toggles in Unity editor
    public Button doneButton; // assign button in Unity editor

    void Start()
    {
        questionnaireTarget = gameObject; // self
        questionnaireTarget.GetComponentsInChildren<Toggle>(answerToggles);
        doneButton.onClick.AddListener(ReadSurveyAnswers);
    }

    void ReadSurveyAnswers()
    {
        foreach (Toggle toggle in answerToggles)
        {
            if (toggle.isOn)
            {
                string log = "Answer: " + toggle.name + ", Timestamp: " + System.DateTime.Now.ToString();
                Debug.Log(log);
                WriteToCSV(log);
            }
        }
    }

    void WriteToCSV(string log)
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        string filePath = Path.Combine(Application.dataPath, "!Made-by-BJS", "Logs", "log_" + timestamp + ".csv");
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine(log);
        }
    }
}
