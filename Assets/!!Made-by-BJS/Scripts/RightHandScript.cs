using OVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Righthandscript : MonoBehaviour
{
    private GameManagerExp1 gameManagerExp1; // Reference to the GameManagerExp1 script
    private ChangeText textScript; // Reference to the ChangeText script


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other) // when the head touches a sphere
    {
        // Check if the collided object has the tag "GameTarget"
        if (other.gameObject.CompareTag("GameTarget"))
        {
            // Disable the sphere that was touched
            other.gameObject.SetActive(false);

            // Call the function to handle the logic after touching a sphere
            gameManagerExp1.HandleSphereTouched();
        }
        else if (other.gameObject.CompareTag("GameTargetTop"))
        {
            // Disable the sphere that was touched
            other.gameObject.SetActive(false);
            gameManagerExp1.HandleTopSphereTouched();
        }
    }
}
