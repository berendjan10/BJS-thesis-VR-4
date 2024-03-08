using System;
using System.Collections;
using System.Collections.Generic;
//using static OVRTask<TResult>;
using System.IO;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

// tweak middle deviation timing

public class GameScript : MonoBehaviour
{
    public GameObject headReference;
    public enum Gender { male, female } // dropdown menu
    public Gender gender;
    [SerializeField] private float _height; // [cm]
    [SerializeField] private bool thirdPersonPerspective = false;
    [SerializeField] private bool smoothTransition = false;
    [SerializeField] private float transitionStart;
    [SerializeField] private float transitionDuration; // duration of transition
    public int phaseOneInstructions = 50;    // amount of instructions in phase 1 (no deviation) // ongeveer 2.42s per goal. ongeveer 50 goals in 2 seconden
    public int phaseTwoInstructions = 50; // [2, 4, 6] uit [2, 3, 4, 5, 6, 7]    // amount of instructions in phase 2 (with deviation)
    public int deviatePercentage = 40; // [%]    // percentage that deviates in phase 2
    public float deviationDuration; // duration of deviation
    public float pauseAtGoal;
    public float waitTimeLowerLimit = 1.0f;    // wait time to sit straight between instructions
    public float waitTimeUpperLimit = 2.0f; // TODO: change waittime back to 2-5s
    [SerializeField] private Vector3 thirdPersonPerspectiveOffsetPosition = new Vector3();
    public enum DeviationType { sineWave, forthPauseBack }
    public DeviationType deviationType;
    public enum CenterDeviationPrevalence { noMiddleDeviations, allTypesOfDeviations, onlyMiddleDeviations }
    public CenterDeviationPrevalence centerDeviationPrevalence;
    public enum CenterDeviationSpeed { slowAndFast, onlySlow, onlyFast }
    public CenterDeviationSpeed centerDeviationSpeed;

    public float flashTime = 0.2f; // [s]
    public bool useGhost = false;

    public GameObject instructionZero;
    public GameObject three3DObjects;

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

    private List<GameObject> goals; // List to store all the sphere game objects
    private List<GameObject> goalsPhase2; // List to store all the sphere game objects
    private ChangeText textScript; // Reference to the ChangeText script
    public GameObject addedScoreGameObject;
    private ChangeText addedScoreScript;
    public GameObject totalScoreGameObject;
    private ChangeText totalScoreScript;
    public GameObject remainingGameObject;
    private ChangeText remainingScript;
    private float waitTime;
    private float waitBeforeNextInstruction;
    private float waitDeviationDuration;
    private int instructionCounter = 0; // Counter to keep track of the number of instructions given
    private bool thisTrialContainsDeviation = false;
    private List<int> deviatingTrials;
    private string reach;
    private GameObject previousGoal;
    private GameObject currentGoal;
    private GameObject goalPreviousFrame;


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
    public GameObject textObjects;
    //private Vector3 standardCameraOffsetPosition = new Vector3();
    //private Vector3 standardCameraOffsetRotation = new Vector3();
    //public Vector3 thirdPersonPerspectiveOffsetRotation = new Vector3(15.0f, 0.0f, 0.0f);

    public GameObject leftHandTarget;
    public GameObject leftHandTargetFollow;
    public GameObject rightHandTarget;
    public GameObject rightHandTargetFollow;

    private int deviationDirection; // 0 = less far 1 = further

    private bool scoreSaved;

    public InputActionProperty thumbButtonA;
    public InputActionProperty thumbButtonB;

    //[SerializeField] private GameObject CameraOffsetRef;

    [SerializeField]
    private GameObject MainCamera;

    private int totalScore = 0;
    private int roundScore = 0;

    private TextMeshPro textMeshProToChange;

    private string gameState; // "waiting for user to recenter" "Waiting for user to read instruction" "Waiting for user to touch goal" "Waiting for user to sit straight" "Sudden deviation SLOW" "Waiting before next instruction is given"

    public Transform myMeasuredPivotPoint;

    private bool currentlyDeviating = false;

    private float goalRotation;

    public float closerDeviationCutoff = 0.6f;
    public float fartherDeviationCutoff = 0.8f;
    private float deviationCutoff;
    private GameObject deviationGoal;

    private bool animationTriggered = false;

    private float originalZ;

    public GameObject hmdResetView;

    public Transform mainCamera;
    public Transform origin;
    public Transform target;

    public XROrigin XRORIGINN;

    public GameObject ghost;

    public Transform three3PPPlaceholder;

    private Vector3 instruction1PPPosition;

    private float overshoot = 0;

    private float instructionGivenTimestamp;

    private float timeToReachGoal;

    public float centerFastDeviationDuration = 0.5f;
    public float centerSlowDeviationDuration = 3.0f;

    public GameObject closeGoalLeft;
    public GameObject closeGoalRight;

    private float progress0debug;

    public float waitForCenterDeviation = 3.0f;

    private bool WaitAndSetRandomGoalCalled;

    public bool record;

    private string startGameTimestamp;

    private float farthestX;

    private float farthestY;

    void Start()
    {
        // Initialize the list with all sphere game objects
        goals = new List<GameObject> { goal1, goal2, goal3, goal4, goalmin1, goalmin2, goalmin3, goalmin4 };
        goalsPhase2 = new List<GameObject> { goal1, goal2, goal3, goal4, goalmin1, goalmin2, goalmin3, goalmin4, topGoal, topGoal, topGoal, topGoal };
        // goals = new List<GameObject> { goal1 };

        // Find the game object with the ChangeText script
        textScript = gameInstructions.GetComponent<ChangeText>();
        addedScoreScript = addedScoreGameObject.GetComponent<ChangeText>();
        totalScoreScript = totalScoreGameObject.GetComponent<ChangeText>();
        remainingScript = remainingGameObject.GetComponent<ChangeText>();

        instruction1PPPosition = textObjects.transform.position;

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

        thumbButtonB.action.performed += OnThumbB;
        thumbButtonA.action.performed += OnThumbA;

        if (!useGhost) { ghost.SetActive(false); }


        // initial game state just prompts the user to recenter
        three3DObjects.SetActive(false);
        instructionZero.SetActive(true);
        gameState = "waiting for user to recenter";

        remainingScript.ChangeTextFcn("Remaining: " + (phaseOneInstructions + phaseTwoInstructions));

        startGameTimestamp = System.DateTime.Now.ToString("yyyyMMddHHmmss");

        
        if (record)
        {
            // header of hmd tracking log
            string header = "Timestamp; x; y; z; rx; ry; rz; instruction ID; Game state";
            string filePath = Path.Combine(Application.dataPath, "!!Made-by-BJS", "Logs", "Headset_tracking_log_" + startGameTimestamp + ".csv");
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(header);
            }

            // tracking summary log header
            string summaryHeader = "Instruction ID; goal; goal x-position; goal y-position; farthest reached x; farthest reached y; overshoot; time to reach goal; trial contains deviation; deviation goal; deviation goal x; deviation goal y";
            string filePath2 = Path.Combine(Application.dataPath, "!!Made-by-BJS", "Logs", "Tracking_summary_" + startGameTimestamp + ".csv");
            using (StreamWriter writer = new StreamWriter(filePath2, true))
            {
                writer.WriteLine(summaryHeader);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //totalScoreScript.ChangeTextFcn(gameState + "\nContains Deviation: ");// + thisTrialContainsDeviation + "\nAnimation Triggered: " + animationTriggered + "\n" + "progress >= deviationcutoff:\n" + progress0debug + "\n" + deviationCutoff);

        if (Input.GetKeyDown(KeyCode.B))
        {
            OnThumbB(default);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            OnThumbA(default);
        }
        if (gameState == "waiting for user to recenter")
        {
            InstructionInFrontOfCamera();
        }

        // check head position relative to goal position, start deviating at 80%
        if (gameState == "Waiting for user to touch goal" || gameState == "Waiting for user to sit straight" || gameState == "Sudden deviation SLOW" || gameState == "Sudden deviation FAST")
        {
            // calculate the progress to the goal and overshoot
            if (goalPreviousFrame != currentGoal) { overshoot = 0; } // reset overshoot when new goal is set
            goalPreviousFrame = currentGoal;
            // if (goalRotation != 0) // if (currentGoal != topGoal)

            float angle = (float)(Mathf.Atan2(hmdTarget.position.x - myMeasuredPivotPoint.position.x, hmdTarget.position.y - myMeasuredPivotPoint.position.y) * 360 / (2 * Math.PI) * (-1)); // radians!!!
            float progress = angle / goalRotation;
            progress0debug = progress;
            if (progress > overshoot)
            {
                // farthest reached hmd position
                farthestX = hmdTarget.position.x;
                farthestY = hmdTarget.position.y;
                overshoot = progress; // in percentage
            }
            if (thisTrialContainsDeviation)
            {
                if (progress >= deviationCutoff || gameState == "Sudden deviation SLOW" || gameState == "Sudden deviation FAST")
                {
                    if (!animationTriggered && gameState != "Waiting for user to sit straight")
                    {
                        // Trigger deviation
                        currentlyDeviating = true;
                        deviationClock = 0;
                        animationTriggered = true;
                        if (gameState == "Waiting for user to touch goal") { StartCoroutine(WaitForDeviationAndActAsIfGoalTouched()); } // skip that (user has to touch goal)
                    }
                }
            }
        }

        if (!thisTrialContainsDeviation)
        {
            currentlyDeviating = false;
        }

        //// controller trigger button press
        //float triggerValue = thumbButtonA.action.ReadValue<float>(); // 0 or 1


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
                    cameraOffset.transform.position = Vector3.Lerp(Vector3.zero, thirdPersonPerspectiveOffsetPosition, transitionLerpValue);
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
            float deviationDurationLocal;
            DeviationType deviationTypeLocal;

            if (gameState == "Sudden deviation SLOW")
            {
                deviationTypeLocal = DeviationType.forthPauseBack; // IF change back to sineWave ALSO CHANGE line in CenterDeviation(): while (localTimer < localDeviationDuration+pauseAtGoal)
                deviationDurationLocal = centerSlowDeviationDuration;
            }
            else if (gameState == "Sudden deviation FAST")
            {
                deviationTypeLocal = DeviationType.forthPauseBack;
                deviationDurationLocal = centerFastDeviationDuration;
            }
            else
            {
                deviationTypeLocal = deviationType;
                deviationDurationLocal = deviationDuration;
            }
            // deviation timing
            deviationClock += Time.deltaTime;

            if (deviationTypeLocal == DeviationType.sineWave)
            {
                if (deviationClock >= 0 && deviationClock <= deviationDurationLocal)
                {
                    // movement speed function
                    deviationLerpValue = sineWave(deviationClock, deviationDurationLocal);
                }
                else if (deviationClock > deviationDurationLocal)
                {
                    currentlyDeviating = false;
                }
            }
            else if (deviationTypeLocal == DeviationType.forthPauseBack)
            {
                if (deviationClock >= 0 && deviationClock <= deviationDurationLocal / 2)
                {
                    // movement speed function
                    deviationLerpValue = sineWave(deviationClock, deviationDurationLocal);
                }
                else if (deviationClock > deviationDurationLocal / 2 && deviationClock <= (deviationDurationLocal / 2 + pauseAtGoal)) // pause at goal
                {
                    // movement speed function
                    deviationLerpValue = 1;

                    // set next loop timer to timestamp top of sine wave
                    deviationClock2 = deviationDurationLocal / 2;
                }
                else if (deviationClock > (deviationDurationLocal / 2 + pauseAtGoal) && deviationClock <= (deviationDurationLocal + pauseAtGoal))
                {
                    deviationClock2 += Time.deltaTime;
                    // movement speed function
                    deviationLerpValue = sineWave(deviationClock2, deviationDurationLocal);
                }
                else if (deviationClock > (deviationDurationLocal + pauseAtGoal))
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
                originalZ = hmdTarget.position.z;
            }
            // third person perspective
            else if (thirdPersonPerspective)
            {
                Vector3 noCompensation = hmdTarget.position - thirdPersonPerspectiveOffsetPosition;
                start = noCompensation - center;
                originalZ = noCompensation.z;
            }

            // determine deviation position
            Vector3 lerpielerpie = Vector3.Slerp(start, end, deviationLerpValue) + center;
            //// only alter X & Y. Z-position (fore-aft) is still tracked.
            lerpielerpie.z = originalZ;
            transform.position = lerpielerpie;

            // both perspectives
            transform.rotation = Quaternion.Slerp(hmdTarget.rotation, deviationGoalRotation, deviationLerpValue); // rotation linear interpolation between HMD & target
        }

        if (useGhost)
        {
            if (currentlyDeviating) { ghost.SetActive(true); }
            else { ghost.SetActive(false); }
        }




        // Store headset movement
        if (record)
        {
            string log = System.DateTime.Now.ToString() + ";" + mainCamera.transform.position.x + ";" + mainCamera.transform.position.y + ";" + mainCamera.transform.position.z
                + ";" + mainCamera.transform.eulerAngles.x + ";" + mainCamera.transform.eulerAngles.y + ";" + mainCamera.transform.eulerAngles.z + ";" + instructionCounter
                + ";" + gameState;
            string filePath = Path.Combine(Application.dataPath, "!!Made-by-BJS", "Logs", "Headset_tracking_log_" + startGameTimestamp + ".csv");
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(log);
            }
        }
    } // Update() end


    // pressing B resets the view, scales it to participant
    void OnThumbB(InputAction.CallbackContext context)
    {
        // switch from start view to forest with mirror
        if (thirdPersonPerspective) { firstPersonPerspective(); }
        if (gameState == "waiting for user to recenter")
        {
            three3DObjects.SetActive(true);
            if (thirdPersonPerspective) { textObjects.transform.position = three3PPPlaceholder.position; }
            else { textObjects.transform.position = instruction1PPPosition; }
            instructionZero.SetActive(false);
            gameState = "Waiting for user to read instruction";
            addedScoreGameObject.SetActive(false);
        }

        // Recenter the view
        target.position = new Vector3(target.position.x, mainCamera.position.y, target.position.z);
        XRORIGINN.MoveCameraToWorldLocation(target.position);
        XRORIGINN.MatchOriginUpCameraForward(target.up, target.forward);



        // Scale goals & avatar
        float scaleDeviationGoals = (transform.position.y - myMeasuredPivotPoint.transform.position.y) / (topGoal.transform.position.y - myMeasuredPivotPoint.transform.position.y) * myMeasuredPivotPoint.localScale.x; // scales goals so that topGoal aligns with HMD
        float avatarScale;
        if(gender == Gender.male)
        {
            avatarScale = scaleDeviationGoals / 0.02f;
        }
        else
        {
            float femaleAvatarHeight = 1.736281f;
            float maleAvatarHeight = 1.86452f;
            float maleToFemaleScaling = femaleAvatarHeight / maleAvatarHeight;
            avatarScale = scaleDeviationGoals / 0.02f / maleToFemaleScaling;
        }
        if (avatarScale > 1.125f) { avatarScale = 1.125f; }
        else if (avatarScale < 0.88f) { avatarScale = 0.88f; }
        avatarScale *= 1.0f; // scaling intensity scalingIntensity
        // scale goals which are a child of myMeasuredPivotPoint
        myMeasuredPivotPoint.localScale = new Vector3(scaleDeviationGoals, scaleDeviationGoals, scaleDeviationGoals);
        // Scale avatar
        avatar.transform.localScale = new Vector3(avatarScale, avatarScale, avatarScale);
        if (thirdPersonPerspective) { thirdPersonPerspectiveFcn(); }
    }

    void OnThumbA(InputAction.CallbackContext context)
    {
        if (gameState == "Waiting for user to read instruction")
        {
            gameInstructions.SetActive(false);
            SetRandomGoal();
        }
        else
        {
            gameInstructions.SetActive(!gameInstructions.activeInHierarchy);
            if (thirdPersonPerspective) { textObjects.transform.position = three3PPPlaceholder.position; }
            else { textObjects.transform.position = instruction1PPPosition; }
        }
    }

    private void InstructionInFrontOfCamera()
    {
        if (mainCamera != null)
        {
            // Calculate the position in front of the main camera
            Vector3 newPosition = mainCamera.transform.position + mainCamera.transform.forward * 8.0f;

            // Update the position of the object
            instructionZero.transform.position = newPosition;

            // Calculate the direction from the object to the main camera
            Vector3 lookDir = mainCamera.transform.position - instructionZero.transform.position;

            // Ensure the object is not rotated in the x or z axis
            lookDir.y = 0;

            // Rotate the object to face the main camera
            instructionZero.transform.rotation = Quaternion.LookRotation(lookDir);

            // Flip the object 180 degrees around its vertical Y-axis
            instructionZero.transform.Rotate(Vector3.up, 180f);
        }
    }

    IEnumerator ShowScore()
    {
        float localTime = 0f;
        addedScoreGameObject.SetActive(true);

        while (localTime < 1) // 1 second visible
        {
            localTime += Time.deltaTime;
            yield return null;
        }

        addedScoreGameObject.SetActive(false);
    }

    // Collision collider
    void OnTriggerStay(Collider other) // when the head touches a goal
    {
        if (gameState == "Waiting for user to touch goal" && other.gameObject == currentGoal & !currentlyDeviating)
        {
            HandleGoalTouched();
        }
        else if (gameState == "Waiting for user to sit straight" && other.gameObject == topGoal)
        {
            ChangeTextColor(topGoal, Color.black);
            waitBeforeNextInstruction = Random.Range(waitTimeLowerLimit, waitTimeUpperLimit);
            gameState = "Waiting before next instruction is given";
            logSummary();
            calculateScore();
            StartCoroutine(WaitAndSetRandomGoal(waitBeforeNextInstruction));
        }
    }

    void logSummary()
    {
        if (record)
        {
            string log;
            if (!thisTrialContainsDeviation)
            {
                log = instructionCounter + ";" + currentGoal.name + ";" + currentGoal.transform.position.x + ";" + currentGoal.transform.position.y
                    + ";" + farthestX + ";" + farthestY + ";" + overshoot + ";" + timeToReachGoal + ";" + thisTrialContainsDeviation + ";;;";
            }
            else
            {
                log = instructionCounter + ";" + currentGoal.name + ";" + currentGoal.transform.position.x + ";" + currentGoal.transform.position.y
                    + ";" + farthestX + ";" + farthestY + ";" + overshoot + ";" + timeToReachGoal + ";" + thisTrialContainsDeviation + ";" + deviationGoal.name
                    + ";" + deviationGoal.transform.position.x + ";" + deviationGoal.transform.position.y;
            }

            string filePath = Path.Combine(Application.dataPath, "!!Made-by-BJS", "Logs", "Tracking_summary_" + startGameTimestamp + ".csv");
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(log);
            }
        }
    }

    void calculateScore()
    {
        // CALCULATE AND SHOW SCORE
        // previous overshoot
        // print("Overshoot = " + overshoot);
        //print("overshoot difference : " + Mathf.Abs(overshoot - 1));
        // print("Accuracy = " + ((1-Mathf.Abs(overshoot - 1)) * 100));
        roundScore = (int)(100 - timeToReachGoal * 20 - Mathf.Abs(overshoot - 1) * 150);
        if (roundScore < 0) { roundScore = 0; }
        //print("Score = 100 - " + (timeToReachGoal * 20) + " - " + (Mathf.Abs(overshoot - 1) * 150) + " = " + roundScore);
        totalScore += roundScore;
        addedScoreScript.ChangeTextFcn("+" + roundScore + "!");
        totalScoreScript.ChangeTextFcn("Score: " + totalScore);
        remainingScript.ChangeTextFcn("Remaining: " + (phaseOneInstructions + phaseTwoInstructions - instructionCounter));
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

    // Coroutine used when avatar deviates from user to act as if goal is touched
    IEnumerator WaitForDeviationAndActAsIfGoalTouched()
    {
        float elapsedTime = 0f;

        // Continue waiting until the elapsed time reaches the wait time
        while (elapsedTime < (deviationDuration*0.5f)+pauseAtGoal)
        {
            // Increment the elapsed time using Time.deltaTime
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Call the function to handle the logic after touching a sphere
        HandleGoalTouched();
    }

    public void HandleGoalTouched()
    {
        timeToReachGoal = Time.time - instructionGivenTimestamp;
        print("Time to reach goal" + timeToReachGoal);

        if (instructionCounter > (phaseOneInstructions + phaseTwoInstructions))
        {
            gameInstructions.SetActive(true);
            textScript.ChangeTextFcn("Thanks for playing!");
        }
        else
        {
            ChangeTextColor(currentGoal, Color.black);
            gameState = "Waiting for user to sit straight";
            ChangeTextColor(topGoal, Color.red);
            // print(gameState);
        }
    }

    void SetRandomGoal()
    {
        //print("SetRandomGoal() entered.");
        // Increment the instruction counter
        instructionCounter++;
        print("instructionCounter: " + instructionCounter);
        scoreSaved = false;

        thisTrialContainsDeviation = deviatingTrials.Contains(instructionCounter);
        print("thisTrialContainsDeviation" + thisTrialContainsDeviation);
        // Generate a random index to choose the next sphere
        int randomIndex;

        if (instructionCounter > phaseOneInstructions)
        {
            print("dit bericht moet pas in phase 2 verschijnen!");
            goals = goalsPhase2;
        }

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
        print("randomIndex: " + randomIndex);

        previousGoal = goals[randomIndex];
        currentGoal = goals[randomIndex];
        // print("currentGoal: " + currentGoal);

        goalRotation = currentGoal.transform.eulerAngles.z;
        if (goalRotation > 180) { goalRotation -= 360; }

        //////////////////////////////////////////////////////////////// choose type of deviation //////////////////////////////////////////////////////////////////
        // Determine deviation goal
        if (thisTrialContainsDeviation) 
        {
            animationTriggered = false;
            // get position of currentGoal in Goals list (length 12. idxs 0 t/m 11)
            if (currentGoal == goal1 || currentGoal == goalmin1)
            {
                deviationDirection = 1; // further
                gameState = "Waiting for user to touch goal";
            }
            else if (currentGoal == goal4 || currentGoal == goalmin4)
            {
                deviationDirection = 0; // closer
                gameState = "Waiting for user to touch goal";
            }
            // topGoalDeviationSlow
            else if (currentGoal == topGoal)
            {
                StartCoroutine(waiBeforeCenterDeviation(false)); // (bool fast)
                deviationDirection = Random.Range(3, 5); // 3 = L, 4 = R
            }
            // topGoalDeviationFast
            else if (randomIndex >= 10 && randomIndex <= 11)
            {
                StartCoroutine(waiBeforeCenterDeviation(true)); // (bool fast)
                deviationDirection = Random.Range(3, 5); // 3 = L, 4 = R
            }
            //else if (randomIndex >= 8 && randomIndex <= 9)
            //{
            //    StartCoroutine(waiBeforeCenterDeviation(false));
            //    deviationDirection = Random.Range(3, 5); // 3 = L, 4 = R
            //}
            //// topGoalDeviationFast
            //else if (randomIndex >= 10 && randomIndex <= 11)
            //{
            //    StartCoroutine(waiBeforeCenterDeviation(true));
            //    deviationDirection = Random.Range(3, 5); // 3 = L, 4 = R
            //}
            else
            {
                deviationDirection = Random.Range(0, 2); // 0 = closer 1 = farther
                gameState = "Waiting for user to touch goal";
            }
            // print("deviation direction: " + deviationDirection);

            if (deviationDirection == 0) // closer
            {
                deviationCutoff = closerDeviationCutoff;
                try { deviationGoal = goals[randomIndex - 1]; }
                catch (IndexOutOfRangeException e)
                {
                    Debug.LogError("Deviation aborted. Index is out of range: " + e.Message);
                    thisTrialContainsDeviation = false;
                }
            }
            else if (deviationDirection == 1) // farther
            {
                deviationCutoff = fartherDeviationCutoff;
                try { deviationGoal = goals[randomIndex + 1]; }
                catch (IndexOutOfRangeException e)
                {
                    Debug.LogError("Deviation aborted. Index is out of range: " + e.Message);
                    thisTrialContainsDeviation = false;
                }
            }
            else if (deviationDirection == 3) // L
            {
                deviationCutoff = -1;
                deviationGoal = closeGoalLeft;
            }
            else if (deviationDirection == 4) // R
            {
                deviationCutoff = -1;
                deviationGoal = closeGoalRight;
            }
            else
            {
                Debug.LogError("Unexpected deviationDirection vaLue: " + deviationDirection);
            }
            deviationGoalPosition = deviationGoal.transform.position;
            deviationGoalRotation = deviationGoal.transform.rotation;
            // print("deviationGoal: " + deviationGoal);
            // print("deviationGoalPosition: " + deviationGoalPosition);
            // print("deviationGoalRotation: " + deviationGoalRotation.eulerAngles);
        }

        else { gameState = "Waiting for user to touch goal"; }
        // print(gameState);

        // Make selected number red
        if (currentGoal != topGoal) { ChangeTextColor(currentGoal, Color.red); }

        instructionGivenTimestamp = Time.time;

        
    }

    IEnumerator waiBeforeCenterDeviation(bool fast)
    {
        float localTimer = 0;
        while (localTimer < waitForCenterDeviation)
        {
            localTimer += Time.deltaTime;
            yield return null;
        }
        if (!fast)
        {
            gameState = "Sudden deviation SLOW";
            StartCoroutine(CenterDeviation(centerSlowDeviationDuration));
        }
        else 
        {
            gameState = "Sudden deviation FAST";
            StartCoroutine(CenterDeviation(centerFastDeviationDuration));
        }
    }

    // coroutine. waits as long as the deviation duurt then: 
    IEnumerator CenterDeviation(float localDeviationDuration)
    {
        float localTimer = 0;
        while (localTimer < localDeviationDuration+pauseAtGoal)
        {
            localTimer += Time.deltaTime;
            yield return null;
        }
        // after timer
        ChangeTextColor(topGoal, Color.black);

        // stop deviation
        currentlyDeviating = false;
        gameState = "Waiting before next instruction is given";

        // make game continue
        waitBeforeNextInstruction = Random.Range(waitTimeLowerLimit, waitTimeUpperLimit);
        StartCoroutine(WaitAndSetRandomGoal(waitBeforeNextInstruction));

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

    private void firstPersonPerspective()
    {
        // 1PP camera view
        cameraOffset.transform.localPosition = Vector3.zero;

        // 1PP avatar hand targets match controllers
        leftHandTargetFollow.transform.position = leftHandTarget.transform.position;
        rightHandTargetFollow.transform.position = rightHandTarget.transform.position;

        // 1PP avatar head no deviation
        if (!currentlyDeviating) { transform.position = hmdTarget.position; }
    }

    private void thirdPersonPerspectiveFcn()
    {
        // 3PP camera view
        cameraOffset.transform.position = XRORIGINN.transform.position + thirdPersonPerspectiveOffsetPosition;

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
    private float sineWave(float localCurrentTime, float deviationDuration)
    {
        float B = 2 * Mathf.PI / Mathf.Abs(deviationDuration); // frequency
        float C = deviationDuration / 4; // phase shift of sine wave (horizontal shift)
        deviationLerpValue = 0.5f * Mathf.Sin(B * (localCurrentTime - C)) + 0.5f; // Update the lerpValue calculation with the new amplitude. 0.5 * sin(pi * (x-0.5))+ 0.5 goes from 0 to 1 to 0 in 2s
        return deviationLerpValue;
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
