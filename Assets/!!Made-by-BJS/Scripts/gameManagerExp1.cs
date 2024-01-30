using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManagerExp1 : MonoBehaviour
{
    public GameObject rightSphere;
    public GameObject leftSphere;
    public GameObject frontSphere;
    public GameObject backSphere;
    public GameObject topSphere;


    public float waitTimeLowerLimit = 1.0f;    // wait time to sit straight between instructions
    public float waitTimeUpperLimit = 2.0f; // TODO: change waittime back to 2-5s
    public int phaseOneInstructions = 1;    // amount of instructions in phase 1 (no deviation)
    public int phaseTwoInstructions = 6; // [2, 4, 6] uit [2, 3, 4, 5, 6, 7]    // amount of instructions in phase 2 (with deviation)
    public int deviatePercentage = 33; // [%]    // percentage that deviates in phase 2

    private List<GameObject> spheres; // List to store all the sphere game objects
    private ChangeText textScript; // Reference to the ChangeText script
    private float currentTime = 0f;
    private float waitTime;
    private int instructionCounter = 0; // Counter to keep track of the number of instructions given
    private bool deviate = false;
    private List<int> deviatingTrials;
    private string reach;

    //private List<int> deviatingTrials = new List<int> { 2, 4, 6 }; // The trials in which avatar deviates from user

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

        // Calculate the number of trials that should deviate based on deviate_percentage
        int deviatingTrialsCount = Mathf.RoundToInt((float)(deviatePercentage * phaseTwoInstructions) / 100);

        // Generate a collection of instruction counters for deviating trials
        GenerateDeviatingTrials(deviatingTrialsCount);

        // Call the function to set up the initial game instructions
        SetRandomGameInstruction();
    }

    void OnTriggerEnter(Collider other) // when the head touches a sphere
    {
        // Check if the collided object has the tag "GameTarget"
        if (other.gameObject.CompareTag("GameTarget") && reach == "head")
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
            HandleTopSphereTouched();
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

    public void HandleSphereTouched()
    {
        if (instructionCounter > (phaseOneInstructions + phaseTwoInstructions))
        {
            textScript.ChangeTextFcn("Thanks for playing!");
        }
        else
        {
            textScript.ChangeTextFcn("Good job! Please sit straight up now.");
            topSphere.SetActive(true);
        }

    }

    public void HandleTopSphereTouched()
    {
        textScript.ChangeTextFcn("Good job! Please keep sitting straight until further instructions.");

        // Set a random wait time between 1s and 2s
        waitTime = Random.Range(waitTimeLowerLimit, waitTimeUpperLimit);

        // Call the function to generate a new random game instruction after the wait period
        StartCoroutine(WaitAndSetRandomInstruction());

    }

    void SetRandomGameInstruction()
    {
        // Increment the instruction counter
        instructionCounter++;
        print("instructionCounter: " + instructionCounter);

        deviate = deviatingTrials.Contains(instructionCounter);
        if (deviate)
        {
            AvatarHeadMovement AvatarHeadMovementInstance = GetComponent<AvatarHeadMovement>(); // Get a reference to the LerpHmd instance
            AvatarHeadMovementInstance.TriggerAnimation(); // Call the TriggerAnimation() function
            waitTime = AvatarHeadMovementInstance.duration;
            StartCoroutine(WaitAndHandleSphereTouched());

            // now. there is no golden sphere to activate a trigger.
        }
        else
        {
            // Generate a random index to choose the next sphere
            int randomIndex = Random.Range(0, spheres.Count);

            reach = "head";

            //int id = Random.Range(0, 3);
            //switch (id)
            //{
            //    case 0:
            //        reach = "head";
            //        break;
            //    case 1:
            //        reach = "left";
            //        break;
            //    case 2:
            //        reach = "right";
            //        break;
            //};

            // Enable the selected sphere
            spheres[randomIndex].SetActive(true);

            // Set the game instruction based on the selected sphere
            switch (randomIndex)
            {
                case 0:
                    textScript.ChangeTextFcn("Please lean to the right - touch the golden sphere with your "+ reach);
                    break;
                case 1:
                    textScript.ChangeTextFcn("Please lean to the left - touch the golden sphere with your " + reach);
                    break;
                case 2:
                    textScript.ChangeTextFcn("Please lean forward - touch the golden sphere with your " + reach);
                    break;
                case 3:
                    textScript.ChangeTextFcn("Please lean backward - touch the golden sphere with your " + reach);
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
            int trial = Random.Range((phaseOneInstructions + 1), (phaseOneInstructions + phaseTwoInstructions + 1));
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
    public string GetReach()
    {
        return reach;
    }


}
