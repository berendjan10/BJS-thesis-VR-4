using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// hyperparameter: wait time TODO: change waittime back to 2-5s
// hyperparameter: amount of instructions in phase 1 (no deviation)
// hyperparameter: amount of instructions in phase 2 (with deviation)
// hyperparameter: deviate_percentage: percentage that deviates in phase 2


public class GameManagerExp1 : MonoBehaviour
{
    public GameObject rightSphere;
    public GameObject leftSphere;
    public GameObject frontSphere;
    public GameObject backSphere;
    public GameObject topSphere;

    private List<GameObject> spheres; // List to store all the sphere game objects

    private ChangeText textScript; // Reference to the ChangeText script

    private float currentTime = 0f;
    private float waitTime;
    private int instructionCounter = 0; // Counter to keep track of the number of instructions given
    private bool deviate = false;
    private int deviate_percentage = 50; // percentage that deviates in phase 2


    void Start()
    {
        //print("entered function: start");
        // Initialize the list with all sphere game objects
        spheres = new List<GameObject> { rightSphere, leftSphere, frontSphere, backSphere };

        // Find the game object with the ChangeText script
        GameObject textGameObject = GameObject.FindWithTag("gameInstructions");
        textScript = textGameObject.GetComponent<ChangeText>();

        // Disable all spheres (just to be sure)
        foreach (GameObject sphere in spheres)
        {
            sphere.SetActive(false);
        }

        // // Call the function to set up the initial game instructions
        // SetGameInstructions();
        SetRandomGameInstruction();

    }

    // void SetGameInstructions()
    // {
    //     // Set the initial game instruction
    //     textScript.ChangeTextFcn("INITIAL Lean to the right - touch the golden sphere with your head");
    // }

    void OnTriggerEnter(Collider other)
    {
        //print("entered function: OnTriggerEnter");
        // Check if the collided object has the tag "GameTarget"
        if (other.gameObject.CompareTag("GameTarget"))
        {
            //print("other gameobject == GameTarget");
            // Disable the sphere that was touched
            other.gameObject.SetActive(false);

            // Call the function to handle the logic after touching a sphere
            HandleSphereTouched();
        }
        else if (other.gameObject.CompareTag("GameTargetTop"))
        {
            //print("other gameobject == GameTargetTop");
            // Disable the sphere that was touched
            other.gameObject.SetActive(false);
            textScript.ChangeTextFcn("Good job! Please keep sitting straight until further instructions.");

            // Set a random wait time between 1s and 5s
            waitTime = Random.Range(1f, 2f);
            print("waitTime: " + waitTime);

            // Call the function to generate a new random game instruction after the wait period
            StartCoroutine(WaitAndSetRandomInstruction());
        }
    }

    // Coroutine to wait for a random period and then set a new random game instruction
    IEnumerator WaitAndSetRandomInstruction()
    {
        //print("entered function: WaitAndSetRandomInstruction (CoRoutine)");
        float elapsedTime = 0f;

        // Continue waiting until the elapsed time reaches the randomly chosen wait time
        while (elapsedTime < waitTime)
        {
            // Increment the elapsed time using Time.deltaTime
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }
        //print("timer done!");

        // Call the function to generate a new random game instruction
        SetRandomGameInstruction();
    }

    void HandleSphereTouched()
    {
        //print("entered function: HandleSphereTouched");

        // Increment the instruction counter
        instructionCounter++;
        print("Instruction Counter: " + instructionCounter);

        textScript.ChangeTextFcn("Good job! Please sit straight up now.");
        topSphere.SetActive(true);

    }

    void SetRandomGameInstruction()
    {
        if (instructionCounter <= 10) // Phase 1
        {
            deviate = false;
        }
        else if (instructionCounter > 10 && instructionCounter <= 20) // Phase 2
        {
            deviate = true; // I want, in phase 2, for 'deviate_percentage'% of the trials to have the parameter 'deviate' set to true, but this should occur within a randomly selected collection of phase 2 trials.
        }
        else if (instructionCounter > 20) // Phase 3
        {
            deviate = false;
        }

        // Generate a random index to choose the next sphere
        int randomIndex = Random.Range(0, spheres.Count);

        // Enable the selected sphere
        spheres[randomIndex].SetActive(true);

        // Set the game instruction based on the selected sphere
        switch (randomIndex)
        {
            case 0:
                textScript.ChangeTextFcn("Please lean to the right - touch the golden sphere with your head");
                break;
            case 1:
                textScript.ChangeTextFcn("Please lean to the left - touch the golden sphere with your head");
                break;
            case 2:
                textScript.ChangeTextFcn("Please lean forward - touch the golden sphere with your head");
                break;
            case 3:
                textScript.ChangeTextFcn("Please lean backward - touch the golden sphere with your head");
                break;
            default:
                break;
           }

    }
}
