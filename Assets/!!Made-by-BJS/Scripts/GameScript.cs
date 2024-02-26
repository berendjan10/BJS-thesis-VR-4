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
    public GameObject goalmin1;
    public GameObject goalmin2;
    public GameObject goalmin3;

    [SerializeField] private GameObject avatar;

    private float scale;

    private List<GameObject> goals; // List to store all the sphere game objects
    private ChangeText textScript; // Reference to the ChangeText script
    private ChangeText scoreScript;
    private float waitTime;
    private float waitBeforeNextInstruction;
    private float waitDeviationDuration;
    private int instructionCounter = 0; // Counter to keep track of the number of instructions given
    private bool deviate = false;
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

    public GameObject scoreText;

    private int deviationDirection;

    private bool scoreSaved;

    public InputActionProperty thumbButtonA;

    private int totalScore = 0;

    public TextMeshPro textMeshProToChange;

    void Start()
    {
        // for later calibration: SAVE HEAD POSITION OF AVATAR (M/F) FOR LATER CALCS then read out head position when "calibrate" is pressed.
        print("headReference" + headReference.transform.position); 

        // scale down avatar & targets based on participant height
        if (gender == gendr.male)
        {
            scale = Mathf.Pow(_height / 185f, scalingIntensity);
        }
        else if (gender == gendr.female)
        {
            scale = Mathf.Pow(_height / 185f, scalingIntensity);
        }

        // scale avatar
        avatar.transform.localScale = new Vector3(scale, scale, scale);

        // Initialize the list with all sphere game objects
        goals = new List<GameObject> { goal1, goal2, goal3, goalmin1, goalmin2, goalmin3 };
        // goals = new List<GameObject> { goal1 };

        // Find the game object with the ChangeText script
        textScript = gameInstructions.GetComponent<ChangeText>();

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

        // Choose which trials wil deviate
        GenerateDeviatingTrials(deviatingTrialsCount);

        // wait, remove text and SetRandomGoal
        StartCoroutine(WaitAndStartGame());

    }

    // Update is called once per frame
    void Update()
    {
        // controller trigger button press
        float triggerValue = thumbButtonA.action.ReadValue<float>(); // 0 or 1

        scoreScript = scoreText.GetComponent<ChangeText>();

        /////////////////////////////////////////////////////////////////////////////////// HeadMovement ///////////////////////////////////////////////////////////////////////////////////
        currentTime += Time.deltaTime;

        // Match head & hand target rotations with controllers (same for 1PP & 3PP)
        transform.rotation = hmdTarget.rotation;
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

        transform.position = Vector3.Slerp(start, end, deviationLerpValue) + center; /////////////////////////////////////////////////// slerp overwrites position? Keertje met debug functie op de tu uitzoeken

        // both perspectives
        transform.rotation = Quaternion.Slerp(hmdTarget.rotation, goalRotation, deviationLerpValue); // rotation linear interpolation between HMD & target

    } /////////////////////////////////////////////////////////////////////////////////// end of DEVIATION ///////////////////////////////////////////////////////////////////////////////////

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
        print("collision with " + other.gameObject);
        if (other.gameObject == currentGoal)
        {
            print("collision with currentGoal!");
            ChangeTextColor(other.gameObject, Color.black);
            ChangeTextColor(topGoal, Color.red);
        }
        else if (other.gameObject == topGoal)
        {
            // Disable the sphere that was touched
            ChangeTextColor(topGoal, Color.black);

            waitBeforeNextInstruction = Random.Range(waitTimeLowerLimit, waitTimeUpperLimit);

            // Call the function to generate a new random game instruction after the wait period
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
        print("coroutine wait successfully finished. Now calling SetRandomGoal...");

        // Call the function to generate a new random game instruction
        SetRandomGoal(); // en deze heeft die eerder al succesvol gebruikt...
    }

    // Coroutine (used when avatar deviates from user) to wait for a random period and then call the function to handle the logic after touching a sphere 
    IEnumerator WaitAndHandleDiskTouched(float waitTime1)
    {
        print("waitTime = " + waitTime1);
        float elapsedTime = 0f;

        // Continue waiting until the elapsed time reaches the wait time
        while (elapsedTime < waitTime1)
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
            ChangeTextColor(topGoal, Color.red);
        }
    }

    void SetRandomGoal()
    {
        // Increment the instruction counter
        instructionCounter++;
        print("instructionCounter: " + instructionCounter);
        scoreSaved = false;

        deviate = deviatingTrials.Contains(instructionCounter);
        if (deviate) // TODO: nog aanpassen.
        {
            // Get a reference to the AvatarHeadMovement instance

            // random direction generator 
            int randomDirection = Random.Range(0, 2); // only left and right!!!??   TODO: change back
            if (deviationType == devType.sineWave)
            {
                waitDeviationDuration = deviationDuration;
                StartCoroutine(WaitAndHandleDiskTouched(waitDeviationDuration));
            }
            else if (deviationType == devType.forthPauseBack)
            {
                waitDeviationDuration = deviationDuration + pauseAtGoal;
                StartCoroutine(WaitAndHandleDiskTouched(waitDeviationDuration));
            } 

            // now. there is no golden sphere to activate a trigger.
        }
        else
        {
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
            print("randomIndex: " + randomIndex);
            print("previousGoal: " + previousGoal);
            print("currectGoal: " + currentGoal);

            // Make selected number red
            ChangeTextColor(currentGoal, Color.red);

        }
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

        print("textMeshProToChange: " + textMeshProToChange);
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
