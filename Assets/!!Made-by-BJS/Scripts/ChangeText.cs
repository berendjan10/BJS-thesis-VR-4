using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class ChangeText : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    
    public void ChangeTextFcn(string newText)
    {
        textMeshPro.text = newText;
    }
}

