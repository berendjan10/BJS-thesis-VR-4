using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameExperiment1 : MonoBehaviour
{
    public GameObject rightSphere;
    private CollisionChecker collisionChecker;
    public TextMeshProUGUI textMeshPro;

    // Start is called before the first frame update
    void Start()
    {
    }



    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Toggle the component on or off
            rightSphere.SetActive(!rightSphere.activeSelf);
        }

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    // Change the text
        //    textMeshPro.text = "New Text";
        //}
        collisionChecker = rightSphere.GetComponent<CollisionChecker>();
        if (collisionChecker != null)
        {
            // Change the text
            textMeshPro.text = "Collision";
        }
        else
        {
            textMeshPro.text = "No Collision";
        }


    }
}



public class TriggerHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered: " + other.gameObject.name);
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Trigger stay: " + other.gameObject.name);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger exited: " + other.gameObject.name);
    }
}