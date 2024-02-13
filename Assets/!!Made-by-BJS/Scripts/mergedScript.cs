using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;


public class mergedScript : MonoBehaviour
{
    public GameObject headReference;
    public enum gendr { male, female } // dropdown menu
    public gendr gender;
    [SerializeField] private float _height; // [cm]
    public float scalingIntensity = 1.03f; // % =1 scale exactly, <1 scale more, >1 scale less
    [SerializeField] private bool thirdPersonPerspective = false;
    [SerializeField] private bool smoothTransition = false;
    [SerializeField] private float transitionStart;
    [SerializeField] private float transitionDuration; // duration of transition
    public int phaseOneInstructions = 10;    // amount of instructions in phase 1 (no deviation)
    public int phaseTwoInstructions = 10; // [2, 4, 6] uit [2, 3, 4, 5, 6, 7]    // amount of instructions in phase 2 (with deviation)
    public int deviatePercentage = 40; // [%]    // percentage that deviates in phase 2
    public float deviationDuration; // duration of deviation
    public float pauseAtGoal;
    public float waitTimeLowerLimit = 1.0f;    // wait time to sit straight between instructions
    public float waitTimeUpperLimit = 2.0f; // TODO: change waittime back to 2-5s
    [SerializeField] private Vector3 thirdPersonPerspectiveOffsetPosition = new Vector3();
    public enum devType { sineWave, forthPauseBack, waitForUser }
    public devType deviationType;
    public float instructionDuration = 10f;

    public GameObject rightSphere;
    public GameObject leftSphere;
    public GameObject frontSphere;
    public GameObject backSphere;
    public GameObject topSphere;
    public GameObject leftHandSphere;
    public GameObject middleHandSphere;
    public GameObject rightHandSphere;
    [SerializeField] private GameObject avatar;

    private float scale;

    private List<GameObject> spheres; // List to store all the sphere game objects
    private ChangeText textScript; // Reference to the ChangeText script
    private float waitTime;
    private int instructionCounter = 0; // Counter to keep track of the number of instructions given
    private bool deviate = false;
    private List<int> deviatingTrials;
    private string reach;
    private GameObject previousSphere;


    [SerializeField] private Transform hmdTarget;
    [SerializeField] private GameObject goalLeft; // to do: change later (merge with 4 above wgen merging scripts)
    [SerializeField] private GameObject goalRight;
    [SerializeField] private GameObject goalFront;
    [SerializeField] private GameObject goalBack;

    [SerializeField] private GameObject hipAnchor;
    private float transitionLerpValue;
    private float deviationLerpValue = 0;
    private float currentTime; // clock
    private float transitionTime = 0; // clock
    private float deviationCurrentTime = 3600;
    private float deviationTimer2;

    private Vector3 goalPosition;
    private Quaternion goalRotation;
    [SerializeField] private GameObject cameraOffset;
    [SerializeField] private GameObject mirror;
    [SerializeField] private GameObject gameInstructions;
    private Vector3 standardCameraOffsetPosition = new Vector3();
    private Vector3 standardCameraOffsetRotation = new Vector3();
    [SerializeField] private GameObject alienAntenna;
    //public Vector3 thirdPersonPerspectiveOffsetRotation = new Vector3(15.0f, 0.0f, 0.0f);

    public GameObject leftHandTarget;
    public GameObject leftHandTargetFollow;
    public GameObject rightHandTarget;
    public GameObject rightHandTargetFollow;

    private int deviationDirection;

    public InputActionProperty thumbButtonA;



    bool isPaused = false;

    void Pause()
    {
        Time.timeScale = 0.0f;
        isPaused = true;
        // Optionally, you can do other things when the game is paused
    }

    void ResumeGame()
    {
        Time.timeScale = 1.0f;
        isPaused = false;
        // Optionally, you can do other things when the game is resumed
    }


    void Start()
    {
        print("headReference" + headReference.transform.position); // SAVE HEAD POSITION OF AVATAR (M/F) FOR LATER CALCS then read out head position when "calibrate" is pressed.
        WaitAndRemoveInstruction();

        // scale down avatar & targets based on participant height
        if (gender == gendr.male)
        {
            scale = _height / 185f * scalingIntensity;
        }
        else if (gender == gendr.female)
        {
            scale = _height / 173.6281f * scalingIntensity;
        }


        avatar.transform.localScale = new Vector3(scale, scale, scale);
        MoveDown(topSphere);
        MoveDown(leftSphere);
        MoveDown(rightSphere);
        MoveDown(frontSphere);
        MoveDown(backSphere);


        MoveDown(goalLeft);
        MoveDown(goalRight);
        MoveDown(goalFront);
        MoveDown(goalBack);


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

    // Update is called once per frame
    void Update()
    {


        float triggerValue = thumbButtonA.action.ReadValue<float>();
        //print("triggerValue: " + triggerValue);
        // yaaayyy right index finger trigger value will be 0 or 1

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (triggerValue == 1)
            {
                Pause();
            }
            else
            {
                ResumeGame();
            }
        }


        currentTime += Time.deltaTime;

        // Match head & hand target rotations with controllers (same for 1PP & 3PP)
        transform.rotation = hmdTarget.rotation;
        leftHandTargetFollow.transform.rotation = leftHandTarget.transform.rotation;
        rightHandTargetFollow.transform.rotation = rightHandTarget.transform.rotation;


        // First Person Perspective
        if (!thirdPersonPerspective) //////////// FOLLOW HMD
        {
            firstPersonPerspective(); /////////////////////////////////////////////////////////////////////////////////// HMD

            // mirror is needed in 1PP
            mirror.SetActive(true);
            gameInstructions.transform.localPosition = new Vector3(0f, 1.189f, 1.042f);

        }

        // Third Person Perspective
        else if (thirdPersonPerspective) //////////// FOLLOW HMD
        {
            if (!smoothTransition)
            {
                thirdPersonPerspectiveFcn(); /////////////////////////////////////////////////////////////////////////////////// HMD
            }
            else if (smoothTransition)
            {
                if (currentTime < transitionStart)
                {
                    firstPersonPerspective(); /////////////////////////////////////////////////////////////////////////////////// HMD
                }
                else if (currentTime >= transitionStart && currentTime <= (transitionStart + transitionDuration))
                {
                    transitionTime += Time.deltaTime;
                    transitionLerpValue = lerpTransition(transitionTime);
                    // transition camera view
                    cameraOffset.transform.position = Vector3.Lerp(standardCameraOffsetPosition, (standardCameraOffsetPosition + thirdPersonPerspectiveOffsetPosition), transitionLerpValue);
                    // transition avatar head & hands positions, compensate for camera offset
                    transform.position = Vector3.Lerp(hmdTarget.position, (hmdTarget.position - thirdPersonPerspectiveOffsetPosition), transitionLerpValue); //// HMD
                    leftHandTargetFollow.transform.position = Vector3.Lerp(leftHandTarget.transform.position, (leftHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition), transitionLerpValue);
                    rightHandTargetFollow.transform.position = Vector3.Lerp(rightHandTarget.transform.position, (rightHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition), transitionLerpValue);


                }
                else if (currentTime > (transitionStart + transitionDuration))
                {
                    thirdPersonPerspectiveFcn(); /////////////////////////////////////////////////////////////////////////////////// HMD
                }
            }

            // mirror is not needed in 1PP
            mirror.SetActive(false);
            gameInstructions.transform.localPosition = new Vector3(0f, 1.442f, 0.394f);

        }

        //////////////////////////////////// DEVIATION ////////////////////////////////////
        // deviation timing
        deviationCurrentTime += Time.deltaTime;

        if (deviationType == devType.sineWave)
        {
            if (deviationCurrentTime >= 0 && deviationCurrentTime <= deviationDuration)
            {
                // movement speed function
                deviationLerpValue = sineWave(deviationCurrentTime);
            }
        }
        else if (deviationType == devType.forthPauseBack)
        {
            if (deviationCurrentTime >= 0 && deviationCurrentTime <= deviationDuration / 2)
            {
                // movement speed function
                deviationLerpValue = sineWave(deviationCurrentTime);
            }
            else if (deviationCurrentTime > deviationDuration / 2 && deviationCurrentTime <= (deviationDuration / 2 + pauseAtGoal)) // pause at goal
            {
                // movement speed function
                deviationLerpValue = 1;

                // set next loop timer to timestamp top of sine wave
                deviationTimer2 = deviationDuration / 2;
            }
            else if (deviationCurrentTime > (deviationDuration / 2 + pauseAtGoal) && deviationCurrentTime <= (deviationDuration + pauseAtGoal))
            {
                deviationTimer2 += Time.deltaTime;
                // movement speed function
                deviationLerpValue = sineWave(deviationTimer2);
            }
        }
        else if (deviationType == devType.waitForUser)
        {
            if (deviationCurrentTime >= 0 && deviationCurrentTime <= deviationDuration / 2)
            {
                // movement speed function
                deviationLerpValue = sineWave(deviationCurrentTime);
            }
            else if (deviationCurrentTime > deviationDuration / 2 && deviationCurrentTime <= (deviationDuration / 2 + pauseAtGoal)) // pause at goal
            {
                // movement speed function
                deviationLerpValue = 1;

                // set next loop timer to timestamp top of sine wave
                deviationTimer2 = deviationDuration / 2;
            }
            else if (deviationCurrentTime == (deviationDuration / 2 + pauseAtGoal) && deviationCurrentTime < (deviationDuration / 2 + pauseAtGoal + 0.2)) // werkt niet
            {
                // if nog niet touched
                deviationLerpValue = 1;
                ActivateDisk(deviationDirection); //activate goal disk (check out fcn TriggerAnimation()) JUST ONCE
                // zodra touched klaar
            }

        }

        // calculate position current frame
        Vector3 center = hipAnchor.transform.position; // hip pivot point
        Vector3 end = goalPosition - center;
        Vector3 start = new Vector3();
        // first person perspective
        if (!thirdPersonPerspective)
        {
            start = hmdTarget.position - center;
        }
        // third person perspective
        else if (thirdPersonPerspective)
        {
            start = hmdTarget.position - thirdPersonPerspectiveOffsetPosition - center;
        }

        transform.position = Vector3.Slerp(start, end, deviationLerpValue) + center; /////////////////////////////////////////////////// slerp overwrites position? 

        // both perspectives
        transform.rotation = Quaternion.Slerp(hmdTarget.rotation, goalRotation, deviationLerpValue); // rotation linear interpolation between HMD & target

    }

    void OnTriggerEnter(Collider other) // when the hand touches a sphere   
    {
        // Check if the collided object has the tag "GameTarget"
        if (other.gameObject.CompareTag("GameTarget")) //  && gameManagerExp1.GetReach() == "head")
        {
            // Disable the sphere that was touched
            other.gameObject.SetActive(false);

            // Call the function to handle the logic after touching a sphere
            HandleDiskTouched();
        }
        else if (other.gameObject.CompareTag("GameTargetTop"))
        {
            // Disable the sphere that was touched
            other.gameObject.SetActive(false);
            HandleTopSphereTouched();

        }
    }

    public void MoveDown(GameObject moveThis)
    {
        float scale1 = _height / 185 * scalingIntensity;
        Vector3 newPosition = moveThis.transform.localPosition;
        newPosition.y *= scale1;
        moveThis.transform.localPosition = newPosition;
    }

    // Coroutine to wait for a random period and then set a new random game instruction
    IEnumerator WaitAndRemoveInstruction()
    {
        float elapsedTime = 0f;

        // Continue waiting until the elapsed time reaches the randomly chosen wait time
        while (elapsedTime < instructionDuration)
        {
            // Increment the elapsed time using Time.deltaTime
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Call the function to generate a new random game instruction
        gameInstructions.SetActive(false);
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
            gameInstructions.SetActive(true);
            textScript.ChangeTextFcn("Thanks for playing!");
        }
        else
        {
            //textScript.ChangeTextFcn("Good job! Please sit straight up now.");
            topSphere.SetActive(true);
        }
    }
    public void HandleSphereTouched()
    {
        if (instructionCounter > (phaseOneInstructions + phaseTwoInstructions))
        {
            gameInstructions.SetActive(true);
            textScript.ChangeTextFcn("Thanks for playing!");
        }
        else
        {
            HandleTopSphereTouched(); // because I skip the "sit up straight now" part
        }
    }

    public void HandleTopSphereTouched()
    {
        //textScript.ChangeTextFcn("Good job! Please keep sitting straight until further instructions.");

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

            // random direction generator 
            int randomDirection = Random.Range(0, 2); // only left and right!!!
            TriggerAnimation(randomDirection);
            if (deviationType == devType.sineWave)
            {
                waitTime = deviationDuration;
                StartCoroutine(WaitAndHandleDiskTouched());
            }
            else if (deviationType == devType.forthPauseBack)
            {
                waitTime = deviationDuration + pauseAtGoal;
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
                randomIndex = Random.Range(0, spheres.Count); 
            } while (spheres[randomIndex] == previousSphere);

            previousSphere = spheres[randomIndex];

            // Enable the selected sphere
            spheres[randomIndex].SetActive(true);

            // Set the game instruction based on the selected sphere
            //switch (randomIndex)
            //{
            //    case 0:
            //        reach = "head";
            //        textScript.ChangeTextFcn("Please lean to the left - touch the blue disk with your head");
            //        break;
            //    case 1:
            //        reach = "head";
            //        textScript.ChangeTextFcn("Please lean to the right - touch the blue disk with your head");
            //        break;
            //    case 2:
            //        reach = "head";
            //        textScript.ChangeTextFcn("Please lean forward - touch the blue disk with your head");
            //        break;
            //    case 3:
            //        reach = "head";
            //        textScript.ChangeTextFcn("Please lean backward - touch the blue disk with your head");
            //        break;
            //    case 4:
            //        reach = "left";
            //        textScript.ChangeTextFcn("Please touch the golden sphere with your " + reach + " hand");
            //        break;
            //    case 5:
            //        reach = "left";
            //        textScript.ChangeTextFcn("Please touch the golden sphere with your " + reach + " hand");
            //        break;
            //    case 6:
            //        reach = "right";
            //        textScript.ChangeTextFcn("Please touch the golden sphere with your " + reach + " hand");
            //        break;
            //    case 7:
            //        reach = "right";
            //        textScript.ChangeTextFcn("Please touch the golden sphere with your " + reach + " hand");
            //        break;
            //    default:
            //        break;
            //}

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
            }
            else
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

    private void firstPersonPerspective()
    {
        // 1PP camera view
        cameraOffset.transform.position = standardCameraOffsetPosition;

        // 1PP avatar hand targets match controllers
        leftHandTargetFollow.transform.position = leftHandTarget.transform.position;
        rightHandTargetFollow.transform.position = rightHandTarget.transform.position;

        // 1PP avatar head no deviation
        transform.position = hmdTarget.position; ////////////////////////////////////////////////////////////////////////////////////// HMD
    }

    private void thirdPersonPerspectiveFcn()
    {
        // 3PP camera view
        cameraOffset.transform.position = standardCameraOffsetPosition + thirdPersonPerspectiveOffsetPosition;

        // 3PP avatar hand targets, compensate for camera offset
        leftHandTargetFollow.transform.position = leftHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition;
        rightHandTargetFollow.transform.position = rightHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition;

        // 3PP avatar head no deviation
        transform.position = hmdTarget.position - thirdPersonPerspectiveOffsetPosition; ///////////////////////////////////////////////////// HMD
    }

    public void TriggerAnimation(int randomDirection)
    {
        switch (randomDirection)
        {
            case 0:
                goalPosition = goalLeft.transform.position;
                goalRotation = goalLeft.transform.rotation;
                deviationDirection = randomDirection;
                break;
            case 1:
                goalPosition = goalRight.transform.position;
                goalRotation = goalRight.transform.rotation;
                deviationDirection = randomDirection;
                break;
            case 2:
                goalPosition = goalFront.transform.position;
                goalRotation = goalFront.transform.rotation;
                deviationDirection = randomDirection;
                break;
            case 3:
                goalPosition = goalBack.transform.position;
                goalRotation = goalBack.transform.rotation;
                deviationDirection = randomDirection;
                break;
            default:
                break;
        }
        deviationCurrentTime = 0; // reset deviation clock
        // _isPlaying = true;
    }

    // one direction
    private float lerpTransition(float localCurrentTime)
    {
        float period = transitionDuration * 2; // period of the sine wave (how many seconds for 1 full cycle)
        float B = 2 * Mathf.PI / Mathf.Abs(period); // frequency
        float C = period / 4; // phase shift of sine wave (horizontal shift)
        deviationLerpValue = 0.5f * Mathf.Sin(B * (localCurrentTime - C)) + 0.5f; // Update the lerpValue calculation with the new amplitude. 0.5 * sin(pi * (x-0.5))+ 0.5 goes from 0 to 1 to 0 in 2s
        return deviationLerpValue;
    }

    // forth & back
    private float sineWave(float localCurrentTime)
    {
        float B = 2 * Mathf.PI / Mathf.Abs(deviationDuration); // frequency
        float C = deviationDuration / 4; // phase shift of sine wave (horizontal shift)
        deviationLerpValue = 0.5f * Mathf.Sin(B * (localCurrentTime - C)) + 0.5f; // Update the lerpValue calculation with the new amplitude. 0.5 * sin(pi * (x-0.5))+ 0.5 goes from 0 to 1 to 0 in 2s
        return deviationLerpValue;
    }


}
