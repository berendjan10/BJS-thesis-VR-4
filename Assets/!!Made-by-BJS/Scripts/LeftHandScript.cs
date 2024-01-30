using OVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lefthandscript : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerOwner;
    private GameManagerExp1 gameManagerExp1; // Reference to the GameManagerExp1 script

    void Start()
    {
        gameManagerExp1 = gameManagerOwner.GetComponent<GameManagerExp1>();
    }

    void OnTriggerEnter(Collider other) // when the hand touches a sphere
    {
        print("Left trigger!");
        print("Reach = " + gameManagerExp1.GetReach());
        print("Other tag = " + other.gameObject.tag);
        if (gameManagerExp1.GetReach() == "left")
        {
            // Check if the collided object has the tag "GameTarget"
            if (other.gameObject.CompareTag("GameTarget"))
            {
                // Disable the sphere that was touched
                other.gameObject.SetActive(false);

                // Call the function to handle the logic after touching a sphere
                gameManagerExp1.HandleDiskTouched();
            }
        }
    }
}
