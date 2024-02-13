using OVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lefthandscript : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerOwner;
    private GameScript gameManagerExp1; // Reference to the GameManagerExp1 script
    
    void Start()
    {
        gameManagerExp1 = gameManagerOwner.GetComponent<GameScript>();
    }

    void OnTriggerEnter(Collider other) // when the hand touches a sphere
    {
        if (gameManagerExp1.GetReach() == "left")
        {
            // Check if the collided object has the tag "GameTarget"
            if (other.gameObject.CompareTag("GameTarget"))
            {
                // Disable the sphere that was touched
                other.gameObject.SetActive(false);

                // Call the function to handle the logic after touching a sphere
                gameManagerExp1.HandleSphereTouched();
            }
        }
    }
}
