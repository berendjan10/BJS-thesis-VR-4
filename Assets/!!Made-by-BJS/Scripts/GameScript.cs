using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;
//using static OVRTask<TResult>;
using System.IO;
using UnityEditor.ShaderGraph.Drawing;
using TMPro;
using OVR.OpenVR;
using UnityEngine.UI;

// DONE: deviation begint al in het midden. percentage berekening is niet goed.
// DOING: deviation blijft vast in een loop en hij gaat ook omhoog. zoek waarom ie niet uitgaat
// DONE: en scale de goals terug.
// TODO: hoofd schiet tijdens deviation verder naar voren (Z). na deviation is er een offset. check welke z waarde die pakt. haal evt eerst de lines weg waarin ie Z negeert

public class GameScript : MonoBehaviour
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
    public enum devType { sineWave, forthPauseBack }
    public devType deviationType;
    public float instructionDuration = 10f;
    public float flashTime = 0.2f; // [s]

    public GameObject topGoal;
    public GameObject goal1;
    public GameObject goal2;
    public GameObject goal3;
    public GameObject goal4;
    public GameObject goalmin1;
    public GameObject goalmin2;
    public GameObject goalmin3;
    public GameObject goalmin4;

    [SerializeField] private GameObject avatar;

    private float scale;

    private List<GameObject> goals; // List to store all the sphere game objects
    private ChangeText textScript; // Reference to the ChangeText script
    private ChangeText scoreScript;
    private float waitTime;
    private float waitBeforeNextInstruction;
    private float waitDeviationDuration;
    private int instructionCounter = 0; // Counter to keep track of the number of instructions given
    private bool thisTrialContainsDeviation = false;
    private List<int> deviatingTrials;
    private string reach;
    private GameObject previousGoal;
    private GameObject currentGoal;


    [SerializeField] private Transform hmdTarget;


    [SerializeField] private GameObject hipAnchor;
    private float transitionLerpValue;
    private float deviationLerpValue = 0;
    private float currentTime; // clock
    private float transitionTime = 0; // clock
    private float deviationClock = 3600;
    private float deviationClock2;

    private Vector3 deviationGoalPosition;
    private Quaternion deviationGoalRotation;
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

    public GameObject scoreText;

    private int deviationDirection; // 0 = less far 1 = further

    private bool scoreSaved;

    public InputActionProperty thumbButtonA;

    private int totalScore = 0;

    private TextMeshPro textMeshProToChange;

    private string gameState; // "Waiting for user to touch goal" "Waiting for user to sit straight"

    public Transform myMeasuredPivotPoint;

    private bool currentlyDeviating = false;

    private float goalRotation;

    public float closerDeviationCutoff = 0.6f;
    public float fartherDeviationCutoff = 0.8f;
    private float deviationCutoff;
    private GameObject deviationGoal;

    private bool animationTriggered = false;

    void Start()
    {
        // for later calibration: SAVE HEAD POSITION OF AVATAR (M/F) FOR LATER CALCS then read out head position when "calibrate" is pressed.
        //print("headReference" + headReference.transform.position); 

        // scale down avatar & targets based on participant height
        if (gender == gendr.male)
        {
            scale = Mathf.Pow(_height / 185f, scalingIntensity);
        }
        else if (gender == gendr.female)
        {
            scale = Mathf.Pow(_height / 185f, scalingIntensity);
        }
        else
        {
            Debug.LogError("Unexpected gender vaLue:" + gender);
        }

        // scale avatar
        avatar.transform.localScale = new Vector3(scale, scale, scale);

        // Initialize the list with all sphere game objects
        goals = new List<GameObject> { goal1, goal2, goal3, goal4, goalmin1, goalmin2, goalmin3, goalmin4 };
        // goals = new List<GameObject> { goal1 };

        // Find the game object with the ChangeText script
        textScript = gameInstructions.GetComponent<ChangeText>();

        if (deviatePercentage < 0)
        {
            deviatePercentage = 0;
            Debug.LogWarning("Deviate Percentage cannot be lower than 0. It is set to 0");
        }
        else if (deviatePercentage > 100)
        {
            deviatePercentage = 100;
            Debug.LogWarning("Deviate Percentage cannot be greater than 100. It is set to 100");
        }
        // Calculate the number of trials that should deviate based on deviate_percentage
        int deviatingTrialsCount = Mathf.RoundToInt((float)(deviatePercentage * phaseTwoInstructions) / 100);

        // Choose which trials wil deviate
        GenerateDeviatingTrials(deviatingTrialsCount);

        // wait, remove text and SetRandomGoal
        StartCoroutine(WaitAndStartGame());

    }

    // Update is called once per frame
    void Update()
    {
        // check head position relative to goal position, start deviating at 80%
        if (thisTrialContainsDeviation)
        {
            float angle = (float)(Mathf.Atan2(hmdTarget.position.x - myMeasuredPivotPoint.position.x, hmdTarget.position.y - myMeasuredPivotPoint.position.y) * 360 / (2 * Math.PI) * (-1)); // radians!!!
            //print("angle / goalRotation >= deviationCutoff...?");
            //print("HMD position angle:" + angle);
            //print("goalRotation: " + goalRotation);
            //print("angle / goalRotation: " + angle / goalRotation);
            //print("deviationCutoff" + deviationCutoff);
            //print("animationTriggered" + animationTriggered);
            //print("currentlyDeviating" + currentlyDeviating);
            if (angle / goalRotation >= deviationCutoff)
            {
                if (!animationTriggered)
                {
                    currentlyDeviating = true;
                    deviationClock = 0;
                    animationTriggered = true;
                }
            }
        }
        else if (!thisTrialContainsDeviation)
        {
            currentlyDeviating = false;
        }

        // controller trigger button press
        float triggerValue = thumbButtonA.action.ReadValue<float>(); // 0 or 1

        scoreScript = scoreText.GetComponent<ChangeText>();

        /////////////////////////////////////////////////////////////////////////////////// HeadMovement ///////////////////////////////////////////////////////////////////////////////////
        currentTime += Time.deltaTime;

        // Match head & hand target rotations with controllers (same for 1PP & 3PP)
        if (!currentlyDeviating) { transform.rotation = hmdTarget.rotation; }
        leftHandTargetFollow.transform.rotation = leftHandTarget.transform.rotation;
        rightHandTargetFollow.transform.rotation = rightHandTarget.transform.rotation;


        // First Person Perspective
        if (!thirdPersonPerspective) // HMD
        {
            firstPersonPerspective(); // HMD

            // mirror is needed in 1PP
            mirror.SetActive(true);
            //gameInstructions.transform.localPosition = new Vector3(0f, 1.189f, 1.042f); // move gameinstructions

        }

        // Third Person Perspective
        else if (thirdPersonPerspective) // HMD
        {
            if (!smoothTransition)
            {
                thirdPersonPerspectiveFcn(); // HMD
            }
            else if (smoothTransition)
            {
                if (currentTime < transitionStart)
                {
                    firstPersonPerspective(); // HMD
                }
                else if (currentTime >= transitionStart && currentTime <= (transitionStart + transitionDuration))
                {
                    transitionTime += Time.deltaTime;
                    transitionLerpValue = lerpTransition(transitionTime);
                    // transition camera view
                    cameraOffset.transform.position = Vector3.Lerp(standardCameraOffsetPosition, standardCameraOffsetPosition + thirdPersonPerspectiveOffsetPosition, transitionLerpValue);
                    // transition avatar head & hands positions, compensate for camera offset
                    transform.position = Vector3.Lerp(hmdTarget.position, hmdTarget.position - thirdPersonPerspectiveOffsetPosition, transitionLerpValue); //// HMD
                    leftHandTargetFollow.transform.position = Vector3.Lerp(leftHandTarget.transform.position, leftHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition, transitionLerpValue);
                    rightHandTargetFollow.transform.position = Vector3.Lerp(rightHandTarget.transform.position, rightHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition, transitionLerpValue);


                }
                else if (currentTime > (transitionStart + transitionDuration))
                {
                    thirdPersonPerspectiveFcn(); // HMD
                }
            }

            // mirror is not needed in 1PP
            mirror.SetActive(false);
            //gameInstructions.transform.localPosition = new Vector3(0f, 1.442f, 0.394f); // move gameinstructions

        }

        /////////////////////////////////////////////////////////////////////////////////// DEVIATION ///////////////////////////////////////////////////////////////////////////////////
        if (currentlyDeviating)
        {
            // deviation timing
            deviationClock += Time.deltaTime;

            if (deviationType == devType.sineWave)
            {
                if (deviationClock >= 0 && deviationClock <= deviationDuration)
                {
                    // movement speed function
                    deviationLerpValue = sineWave(deviationClock);
                }
                else if (deviationClock > deviationDuration)
                {
                    currentlyDeviating = false;
                }
            }
            else if (deviationType == devType.forthPauseBack)
            {
                if (deviationClock >= 0 && deviationClock <= deviationDuration / 2)
                {
                    // movement speed function
                    deviationLerpValue = sineWave(deviationClock);
                }
                else if (deviationClock > deviationDuration / 2 && deviationClock <= (deviationDuration / 2 + pauseAtGoal)) // pause at goal
                {
                    // movement speed function
                    deviationLerpValue = 1;

                    // set next loop timer to timestamp top of sine wave
                    deviationClock2 = deviationDuration / 2;
                }
                else if (deviationClock > (deviationDuration / 2 + pauseAtGoal) && deviationClock <= (deviationDuration + pauseAtGoal))
                {
                    deviationClock2 += Time.deltaTime;
                    // movement speed function
                    deviationLerpValue = sineWave(deviationClock2);
                }
                else if (deviationClock > (deviationDuration + pauseAtGoal))
                {
                    currentlyDeviating = false;
                }
            }

            // calculate position current frame
            Vector3 center = hipAnchor.transform.position; // hip pivot point
            Vector3 end = deviationGoalPosition - center;
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

            // determine deviation position
            Vector3 lerpielerpie = Vector3.Slerp(start, end, deviationLerpValue) + center;
            //// only alter X & Y. Z-position (fore-aft) is still tracked.
            //lerpielerpie.z = start.z;
            transform.position = lerpielerpie;

            // both perspectives
            transform.rotation = Quaternion.Slerp(hmdTarget.rotation, deviationGoalRotation, deviationLerpValue); // rotation linear interpolation between HMD & target
        }
    } // Update() end-

    IEnumerator ShowScore()
    {
        float localTime = 0f;
        scoreText.SetActive(true);
        
        while (localTime < 2) // 2 seconds visible
        {
            localTime += Time.deltaTime;
            yield return null;
        }
        
        scoreText.SetActive(false);
    }

    // Collision collider
    void OnTriggerEnter(Collider other) // when the head touches a goal
    {
        if (gameState == "Waiting for user to touch goal" && other.gameObject == currentGoal)
        {
            print("yay goal touched!");
            ChangeTextColor(currentGoal, Color.black);
            gameState = "Waiting for user to sit straight";
            ChangeTextColor(topGoal, Color.red);
            print(gameState);
        }
        else if (gameState == "Waiting for user to sit straight" && other.gameObject == topGoal)
        {
            print("yay sitting straight!");

            ChangeTextColor(topGoal, Color.black);

            waitBeforeNextInstruction = Random.Range(waitTimeLowerLimit, waitTimeUpperLimit);
            gameState = "Waiting before next instruction is given";
            StartCoroutine(WaitAndSetRandomGoal(waitBeforeNextInstruction));
        }
    }

    public void MoveDown(GameObject moveThis)
    {
        float scale1 = Mathf.Pow(_height / 185f, scalingIntensity);
        Vector3 newPosition = moveThis.transform.localPosition;
        newPosition.y *= scale1;
        moveThis.transform.localPosition = newPosition;
    }

    // Coroutine to wait for a random period and then remove instructions and let the first goal appear
    IEnumerator WaitAndStartGame()
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
        SetRandomGoal();
    }

    // Coroutine to wait for a random period and then set a new random game instruction
    IEnumerator WaitAndSetRandomGoal(float waitTime0)
    {
        // Set a random wait time between 1s and 2s
        float elapsedTime = 0f;

        // Continue waiting until the elapsed time reaches the randomly chosen wait time
        while (elapsedTime < waitTime0)
        {
            // Increment the elapsed time using Time.deltaTime
            elapsedTime += Time.deltaTime;
            // Wait for the next frame
            yield return null;
        }

        // Call the function to generate a new random game instruction
        SetRandomGoal();
    }

    //// Coroutine (used when avatar deviates from user) to wait for a random period and then call the function to handle the logic after touching a sphere 
    //IEnumerator WaitAndHandleDiskTouched(float waitTime1)
    //{
    //    float elapsedTime = 0f;

    //    // Continue waiting until the elapsed time reaches the wait time
    //    while (elapsedTime < waitTime1)
    //    {
    //        // Increment the elapsed time using Time.deltaTime
    //        elapsedTime += Time.deltaTime;

    //        // Wait for the next frame
    //        yield return null;
    //    }

    //    // Call the function to handle the logic after touching a sphere
    //    HandleDiskTouched();
    //}

    //public void HandleDiskTouched()
    //{
    //    if (instructionCounter > (phaseOneInstructions + phaseTwoInstructions))
    //    {
    //        gameInstructions.SetActive(true);
    //        textScript.ChangeTextFcn("Thanks for playing!");
    //    }
    //    else
    //    {
    //        ChangeTextColor(topGoal, Color.red);
    //    }
    //}

    void SetRandomGoal()
    {
        // Increment the instruction counter
        instructionCounter++;
        print("instructionCounter: " + instructionCounter);
        scoreSaved = false;

        thisTrialContainsDeviation = deviatingTrials.Contains(instructionCounter);
        // Generate a random index to choose the next sphere
        int randomIndex;

        if (goals.Count <= 1)
        {
            randomIndex = 0;
        }
        else
        {
            do
            {
                randomIndex = Random.Range(0, goals.Count);
            } while (goals[randomIndex] == previousGoal);
        }
        previousGoal = goals[randomIndex];
        currentGoal = goals[randomIndex];
        print("currentGoal: " + currentGoal);

        // Determine deviation goal
        if (thisTrialContainsDeviation)
        {
            animationTriggered = false;
            goalRotation = currentGoal.transform.eulerAngles.z;
            if (goalRotation > 180) { goalRotation -= 360; }
            // get position of currentGoal in Goals list { goal1, goal2, goal3, goal4, goalmin1, goalmin2, goalmin3, goalmin4 };
            if (randomIndex == 0 || randomIndex == 4)
            {
                deviationDirection = 1; // farther
            }
            else if (randomIndex == 3 || randomIndex == 7)
            {
                deviationDirection= 0; // closer
            }
            else
            {
                deviationDirection = Random.Range(0, 2); // 0 = less far 1 = farther
            }
            print("deviation direction: " + deviationDirection);

            if (deviationDirection == 0)
            {
                deviationCutoff = closerDeviationCutoff;
                try { deviationGoal = goals[randomIndex - 1]; }
                catch (IndexOutOfRangeException e)
                {
                    Debug.LogError("Deviation aborted. Index is out of range: " + e.Message);
                    thisTrialContainsDeviation = false;
                }
            }
            else if (deviationDirection == 1)
            {
                deviationCutoff = fartherDeviationCutoff;
                try { deviationGoal = goals[randomIndex + 1]; }
                catch (IndexOutOfRangeException e)
                {
                    Debug.LogError("Deviation aborted. Index is out of range: " + e.Message);
                    thisTrialContainsDeviation = false;
                }
            }
            else
            {
                Debug.LogError("Unexpected deviationDirection vaLue: " + deviationDirection);
            }
            deviationGoalPosition = deviationGoal.transform.position;
            deviationGoalRotation = deviationGoal.transform.rotation;
            print("deviationGoal: " + deviationGoal);
            print("deviationGoalPosition: " + deviationGoalPosition);
            print("deviationGoalRotation: " + deviationGoalRotation.eulerAngles);
        }


        gameState = "Waiting for user to touch goal";
        print(gameState);

        // Make selected number red
        ChangeTextColor(currentGoal, Color.red);



    }



    // Function to generate a collection of instruction counters for deviating trials
    void GenerateDeviatingTrials(int count)
    {
        deviatingTrials = new List<int>();

        while (deviatingTrials.Count < count)
        {
            int trial = Random.Range(phaseOneInstructions + 1, phaseOneInstructions + phaseTwoInstructions + 1);
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
        goals[nr].SetActive(true);
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
        if (!currentlyDeviating) { transform.position = hmdTarget.position; }
    }

    private void thirdPersonPerspectiveFcn()
    {
        // 3PP camera view
        cameraOffset.transform.position = standardCameraOffsetPosition + thirdPersonPerspectiveOffsetPosition;

        // 3PP avatar hand targets, compensate for camera offset
        leftHandTargetFollow.transform.position = leftHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition;
        rightHandTargetFollow.transform.position = rightHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition;

        // 3PP avatar head no deviation
        if (!currentlyDeviating) { transform.position = hmdTarget.position - thirdPersonPerspectiveOffsetPosition; }
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


        // Co-routine
    IEnumerator FlashGoal(GameObject goal)
    {
        float elapsedTime = 0f;
        goal.SetActive(true);

        // Wait
        while (elapsedTime < flashTime)
        {
            // Increment the elapsed time using Time.deltaTime
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Call the function to handle the logic after touching a sphere
        goal.SetActive(false);
    }

    // Call this method to change the text color
    public void ChangeTextColor(GameObject gameobjectt, Color newColor)
    {
        textMeshProToChange = gameobjectt.GetComponentInChildren<TextMeshPro>();

        if (textMeshProToChange != null)
        {
            textMeshProToChange.color = newColor;
        }
        else
        {
            Debug.LogWarning("TextMeshPro reference is missing!");
        }
    }

}
