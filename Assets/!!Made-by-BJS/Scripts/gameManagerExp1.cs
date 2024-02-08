using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

// to do: merge scripts

public class GameManagerExp1 : MonoBehaviour
{
    public GameObject rightSphere;
    public GameObject leftSphere;
    public GameObject frontSphere;
    public GameObject backSphere;
    public GameObject topSphere;
    public GameObject leftHandSphere;
    public GameObject middleHandSphere;
    public GameObject rightHandSphere;
    [SerializeField] private GameObject avatar;

    [SerializeField] private float _height; // [cm]
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
    private GameObject previousSphere;

    //private List<int> deviatingTrials = new List<int> { 2, 4, 6 }; // The trials in which avatar deviates from user

    void Start()
    {
        // scale down avatar & targets based on participant height
        float scale = _height / 185;

        avatar.transform.localScale = new Vector3(scale, scale, scale);
        MoveDown(topSphere);
        MoveDown(leftSphere);
        MoveDown(rightSphere);
        MoveDown(frontSphere);
        MoveDown(backSphere);
        
        // Initialize the list with all sphere game objects
        spheres = new List<GameObject> { leftSphere, rightSphere, frontSphere, backSphere, leftHandSphere, middleHandSphere, middleHandSphere, rightHandSphere };
        

        // Find the game object with the ChangeText script
        GameObject textGameObject = GameObject.FindWithTag("gameInstructions");
        textScript = textGameObject.GetComponent<ChangeText>();

        // Disable all spheres (just to be sure)
        foreach (GameObject sphere in spheres)
        {
            sphere.SetActive(false);
        }

        if (deviatePercentage < 0)
        {
            deviatePercentage = 0;
            print("Deviate Percentage cannot be lower than 0. It is set to 0");
        }
        else if (deviatePercentage > 100)
        {
            deviatePercentage = 100;
            print("Deviate Percentage cannot be greater than 100. It is set to 100");
        }
        // Calculate the number of trials that should deviate based on deviate_percentage
        int deviatingTrialsCount = Mathf.RoundToInt((float)(deviatePercentage * phaseTwoInstructions) / 100);

        // Generate a collection of instruction counters for deviating trials
        GenerateDeviatingTrials(deviatingTrialsCount);

        // Call the function to set up the initial game instructions
        SetRandomGameInstruction();
    }

    public void MoveDown(GameObject moveThis)
    {
        Vector3 newPosition = moveThis.transform.localPosition;
        float px = 1.03f; // % =1 scale exactly, <1 scale more, >1 scale less
        float scale = _height / 185 * px;
        newPosition.y *= scale;
        moveThis.transform.localPosition = newPosition;
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
    IEnumerator WaitAndHandleDiskTouched()
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
        HandleDiskTouched();
    }

    public void HandleDiskTouched()
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
    public void HandleSphereTouched()
    {
        if (instructionCounter > (phaseOneInstructions + phaseTwoInstructions))
        {
            textScript.ChangeTextFcn("Thanks for playing!");
        }
        else
        {
            HandleTopSphereTouched(); // because I skip the "sit up straight now" part
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
            // Get a reference to the AvatarHeadMovement instance
            AvatarHeadMovement AvatarHeadMovementInstance = GetComponent<AvatarHeadMovement>();

            // random direction generator 
            int randomDirection = Random.Range(0, 2); // only left and right!!!
            AvatarHeadMovementInstance.TriggerAnimation(randomDirection);
            if (AvatarHeadMovementInstance.deviationTypeInt == 0)
            {
                waitTime = AvatarHeadMovementInstance.deviationDuration;
                StartCoroutine(WaitAndHandleDiskTouched());
            } else if (AvatarHeadMovementInstance.deviationTypeInt == 1)
            {
                waitTime = AvatarHeadMovementInstance.deviationDuration + AvatarHeadMovementInstance.pauseAtGoal;
                StartCoroutine(WaitAndHandleDiskTouched());
            }

            // now. there is no golden sphere to activate a trigger.
        }
        else
        {
            // Generate a random index to choose the next sphere
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, spheres.Count); /////////////////////////////////////////////////////////////////////////
            } while (spheres[randomIndex] == previousSphere);

            previousSphere = spheres[randomIndex];

            // Enable the selected sphere
            spheres[randomIndex].SetActive(true);

            // Set the game instruction based on the selected sphere
            switch (randomIndex)
            {
                case 0:
                    reach = "head";
                    textScript.ChangeTextFcn("Please lean to the left - touch the blue disk with your head");
                    break;
                case 1:
                    reach = "head";
                    textScript.ChangeTextFcn("Please lean to the right - touch the blue disk with your head");
                    break;
                case 2:
                    reach = "head";
                    textScript.ChangeTextFcn("Please lean forward - touch the blue disk with your head");
                    break;
                case 3:
                    reach = "head";
                    textScript.ChangeTextFcn("Please lean backward - touch the blue disk with your head");
                    break;
                case 4:
                    reach = "left";
                    textScript.ChangeTextFcn("Please touch the golden sphere with your " + reach + " hand");
                    break;
                case 5:
                    reach = "left";
                    textScript.ChangeTextFcn("Please touch the golden sphere with your " + reach + " hand");
                    break;
                case 6:
                    reach = "right";
                    textScript.ChangeTextFcn("Please touch the golden sphere with your " + reach + " hand");
                    break;
                case 7:
                    reach = "right";
                    textScript.ChangeTextFcn("Please touch the golden sphere with your " + reach + " hand");
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

        while (deviatingTrials.Count < count)
        {
            int trial = Random.Range((phaseOneInstructions + 1), (phaseOneInstructions + phaseTwoInstructions + 1));
            if (0 <= deviatePercentage && deviatePercentage <= 50)
            {
                if (!deviatingTrials.Contains(trial) && !deviatingTrials.Contains(trial - 1)) // second argument avoids 2 consecutive deviating trials
                {
                    deviatingTrials.Add(trial);
                }
            } else
            {
                if (!deviatingTrials.Contains(trial))
                {
                    deviatingTrials.Add(trial);
                }
            }
        }
    }
    public string GetReach()
    {
        return reach;
    }

    public void ActivateDisk(int nr)
    {
        spheres[nr].SetActive(true);
        switch (nr)
        {
            case 0:
                reach = "head";
                textScript.ChangeTextFcn("Please lean to the left - touch the blue disk with your head");
                break;
            case 1:
                reach = "head";
                textScript.ChangeTextFcn("Please lean to the right - touch the blue disk with your head");
                break;
        }
    }


}
