using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerExp1 : MonoBehaviour
{
    public GameObject rightSphere;
    public GameObject leftSphere;
    public GameObject frontSphere;
    public GameObject backSphere;
    public GameObject topSphere;

    private List<GameObject> spheres; // List to store all the sphere game objects

    private ChangeText textScript; // Reference to the ChangeText script

    void Start()
    {
        // Initialize the list with all sphere game objects
        spheres = new List<GameObject> { rightSphere, leftSphere, frontSphere, backSphere, topSphere };

        // Find the game object with the ChangeText script
        GameObject textGameObject = GameObject.FindWithTag("gameInstructions");
        textScript = textGameObject.GetComponent<ChangeText>();

        // // Call the function to set up the initial game instructions
        // SetGameInstructions();
        // spheres[0].GetComponent<MeshRenderer>().enabled = true;
        SetRandomGameInstruction();
    }

    void SetGameInstructions()
    {
        // Set the initial game instruction
        textScript.ChangeTextFcn("INITIAL Lean to the right - touch the golden sphere with your head");
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has the tag "GameTarget"
        if (other.gameObject.CompareTag("GameTarget"))
        {
            // Check if the touched sphere is the correct one
            if (spheres.Contains(other.gameObject))
            {
                print("correct one");
                // Disable the entire GameObject
                other.gameObject.SetActive(false);

                // Call the function to handle the logic after touching a sphere
                HandleSphereTouched();

                // Call the function to generate a new random game instruction
                SetRandomGameInstruction();
            }
        }
    }

    void HandleSphereTouched()
    {
        // Add logic here to handle anything specific when a sphere is touched
        // For example, updating the score or triggering other events.
        textScript.ChangeTextFcn("yaayyy success!");
    }

    void SetRandomGameInstruction()
    {
        // Generate a random index to choose the next sphere
        int randomIndex = Random.Range(0, spheres.Count);

        // Enable the selected sphere
        spheres[randomIndex].SetActive(true);

        // Set the game instruction based on the selected sphere
        switch (randomIndex)
        {
            case 0:
                textScript.ChangeTextFcn("Lean to the right - touch the golden sphere with your head");
                break;
            case 1:
                textScript.ChangeTextFcn("Lean to the left - touch the golden sphere with your head");
                break;
            case 2:
                textScript.ChangeTextFcn("Lean forward - touch the golden sphere with your head");
                break;
            case 3:
                textScript.ChangeTextFcn("Lean backward - touch the golden sphere with your head");
                break;
            case 4:
                textScript.ChangeTextFcn("Sit straight up - touch the golden sphere with your head");
                break;
            default:
                break;
        }
    }
}
