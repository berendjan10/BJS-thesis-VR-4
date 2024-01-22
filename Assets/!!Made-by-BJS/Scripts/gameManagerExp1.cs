using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManagerExp1 : MonoBehaviour
{

    // ========== Hyperparameters: ==========

    // wait time to sit straight between instructions
    private float wait_time_lower_limit = 1.0f;
    private float wait_time_upper_limit = 2.0f; // TODO: change waittime back to 2-5s

    // amount of instructions in phase 1 (no deviation)
    private int phase1_instructions = 1;

    // amount of instructions in phase 2 (with deviation)
    private int phase2_instructions = 6; // [2, 4, 6] uit [2, 3, 4, 5, 6, 7]

    // percentage that deviates in phase 2
    private int deviate_percentage = 33; // [%]

    // ========== Variables: ==========

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
    private List<int> deviatingTrials = new List<int> { 2, 4, 6 }; // The trials in which avatar deviates from user

    void Start()
    {
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

        //// Calculate the number of trials that should deviate based on deviate_percentage
        //int deviatingTrialsCount = Mathf.RoundToInt((float)(deviate_percentage * phase2_instructions) / 100);

        //// Generate a collection of instruction counters for deviating trials
        //GenerateDeviatingTrials(deviatingTrialsCount);

        // Call the function to set up the initial game instructions
        SetRandomGameInstruction();
    }

    void OnTriggerEnter(Collider other) // when the head touches a sphere
    {
        // Check if the collided object has the tag "GameTarget"
        if (other.gameObject.CompareTag("GameTarget"))
        {
            // Disable the sphere that was touched
            other.gameObject.SetActive(false);

            // Call the function to handle the logic after touching a sphere
            HandleSphereTouched();
        }
        else if (other.gameObject.CompareTag("GameTargetTop"))
        {
            // Disable the sphere that was touched
            other.gameObject.SetActive(false);
            textScript.ChangeTextFcn("Good job! Please keep sitting straight until further instructions.");

            // Set a random wait time between 1s and 2s
            waitTime = Random.Range(wait_time_lower_limit, wait_time_upper_limit);

            // Call the function to generate a new random game instruction after the wait period
            StartCoroutine(WaitAndSetRandomInstruction());
        }
    }

    // Coroutine to wait for a random period and then set a new random game instruction
    IEnumerator WaitAndSetRandomInstruction()
    {
        float elapsedTime = 0f;

        // Continue waiting until the elapsed time reaches the randomly chosen wait time
        while (elapsedTime < waitTime)
        {
            // Increment the elapsed time using Time.deltaTime
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Call the function to generate a new random game instruction
        SetRandomGameInstruction();
    }

    // Coroutine (used when avatar deviates from user) to wait for a random period and then call the function to handle the logic after touching a sphere 
    IEnumerator WaitAndHandleSphereTouched()
    {
        float elapsedTime = 0f;

        // Continue waiting until the elapsed time reaches the randomly chosen wait time
        while (elapsedTime < waitTime)
        {
            // Increment the elapsed time using Time.deltaTime
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Call the function to handle the logic after touching a sphere
        HandleSphereTouched();
    }

    void HandleSphereTouched()
    {
        if (instructionCounter > (phase1_instructions + phase2_instructions))
        {
            textScript.ChangeTextFcn("Thanks for playing!");
        }
        else
        {
            textScript.ChangeTextFcn("Good job! Please sit straight up now.");
            topSphere.SetActive(true);
        }

    }

    void SetRandomGameInstruction()
    {
        // Increment the instruction counter
        instructionCounter++;
        print("instructionCounter: " + instructionCounter);

        deviate = deviatingTrials.Contains(instructionCounter);
        if (deviate)
        {
            LerpHmd lerpHmdInstance = GetComponent<LerpHmd>(); // Get a reference to the LerpHmd instance
            lerpHmdInstance.TriggerAnimation(); // Call the TriggerAnimation() function
            waitTime = lerpHmdInstance.duration;
            StartCoroutine(WaitAndHandleSphereTouched());

            // now. there is no golden sphere to activate a trigger.
        }
        else
        {
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

    // Function to generate a collection of instruction counters for deviating trials
    void GenerateDeviatingTrials(int count)
    {
        deviatingTrials = new List<int>();

        // Generate 'count' unique random instruction counters in the range [11, 20]
        while (deviatingTrials.Count < count)
        {
            int trial = Random.Range((phase1_instructions + 1), (phase1_instructions + phase2_instructions + 1));
            if (!deviatingTrials.Contains(trial))
            {
                deviatingTrials.Add(trial);
            }
        }
        foreach (var item in deviatingTrials)
        {
            print(item);
        }
    }
}
