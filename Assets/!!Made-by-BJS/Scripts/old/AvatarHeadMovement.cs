using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;
using UnityEngine.UIElements;

public class AvatarHeadMovement : MonoBehaviour
{
    [SerializeField] private Transform hmdTarget;
    [SerializeField] private Transform goalLeft;
    [SerializeField] private Transform goalRight;
    [SerializeField] private Transform goalFront;
    [SerializeField] private Transform goalBack;
    [SerializeField] private GameObject goalLeftG; // to do: change later (merge with 4 above wgen merging scripts)
    [SerializeField] private GameObject goalRightG;
    [SerializeField] private GameObject goalFrontG;
    [SerializeField] private GameObject goalBackG;

    [SerializeField] private GameObject hipAnchor;
    private float transitionLerpValue;
    private float deviationLerpValue = 0;
    private float currentTime; // clock
    private float transitionTime = 0; // clock
    private float deviationCurrentTime = 3600;
    private float deviationTimer2;

    private Vector3 goalPosition;
    private Quaternion goalRotation;
    [SerializeField] private bool thirdPersonPerspective = false;
    [SerializeField] private bool smoothTransition = false;
    [SerializeField] private float transitionStart;
    [SerializeField] private float transitionDuration; // duration of transition
    public float deviationDuration; // duration of deviation
    public float pauseAtGoal;
    [SerializeField] private GameObject cameraOffset;
    [SerializeField] private GameObject mirror;
    [SerializeField] private GameObject gameInstructions;
    private Vector3 standardCameraOffsetPosition = new Vector3();
    private Vector3 standardCameraOffsetRotation = new Vector3();
    [SerializeField] private Vector3 thirdPersonPerspectiveOffsetPosition = new Vector3();
    [SerializeField] private GameObject alienAntenna;
    //public Vector3 thirdPersonPerspectiveOffsetRotation = new Vector3(15.0f, 0.0f, 0.0f);

    public GameObject leftHandTarget;
    public GameObject leftHandTargetFollow;
    public GameObject rightHandTarget;
    public GameObject rightHandTargetFollow;

    private int deviationDirection;

    // dropdown
    public enum devType { sineWave, forthPauseBack, waitForUser }
    public devType deviationType;
    public int deviationTypeInt; // TO DO: remove (merge scripts)


    // Start is called before the first frame update
    void Start()
    {
        // Get a reference to the GameManager instance/////////////////////////////////////////////////////////////////////////////////////////
        GameManagerExp1 GameManager = GetComponent<GameManagerExp1>();
        GameManager.MoveDown(goalLeftG);
        GameManager.MoveDown(goalRightG);
        GameManager.MoveDown(goalFrontG);
        GameManager.MoveDown(goalBackG);////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
     
    // Update is called once per frame
    void Update()
    {
        if (deviationType == devType.sineWave)
        {
            deviationTypeInt = 0;
        } else if (deviationType == devType.forthPauseBack)
        {
            deviationTypeInt = 1;
        } else if (deviationType == devType.waitForUser)
        {
            deviationTypeInt = 2;
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
        } else if (deviationType == devType.forthPauseBack)
        {
            if (deviationCurrentTime >= 0 && deviationCurrentTime <= deviationDuration/2)
            {
                // movement speed function
                deviationLerpValue = sineWave(deviationCurrentTime);
            }
            else if (deviationCurrentTime > deviationDuration/2 && deviationCurrentTime <= (deviationDuration/2 + pauseAtGoal)) // pause at goal
            {
                // movement speed function
                deviationLerpValue = 1;

                // set next loop timer to timestamp top of sine wave
                deviationTimer2 = deviationDuration/2;
            }
            else if (deviationCurrentTime > (deviationDuration/2 + pauseAtGoal) && deviationCurrentTime <= (deviationDuration + pauseAtGoal))
            {
                deviationTimer2 += Time.deltaTime;
                // movement speed function
                deviationLerpValue = sineWave(deviationTimer2);
            }
        } else if (deviationType == devType.waitForUser)
        {
            if (deviationCurrentTime >= 0 && deviationCurrentTime <= deviationDuration/2)
            {
                // movement speed function
                deviationLerpValue = sineWave(deviationCurrentTime);
            }
            else if (deviationCurrentTime > deviationDuration/2 && deviationCurrentTime <= (deviationDuration/2 + pauseAtGoal)) // pause at goal
            {
                // movement speed function
                deviationLerpValue = 1;

                // set next loop timer to timestamp top of sine wave
                deviationTimer2 = deviationDuration/2;
            } else if (deviationCurrentTime == (deviationDuration/2 + pauseAtGoal) && deviationCurrentTime < (deviationDuration / 2 + pauseAtGoal + 0.2)) // werkt niet
            {
                // if nog niet touched
                deviationLerpValue = 1;
                GameManagerExp1 GameManager = GetComponent<GameManagerExp1>();//////////////////////////////////////////////////////////////////////
                GameManager.ActivateDisk(deviationDirection); //activate goal disk (check out fcn TriggerAnimation()) JUST ONCE
                // zodra touched klaar ////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
                goalPosition = goalLeft.position;
                goalRotation = goalLeft.rotation;
                deviationDirection = randomDirection;
                break;
            case 1:
                goalPosition = goalRight.position;
                goalRotation = goalRight.rotation;
                deviationDirection = randomDirection;
                break;
            case 2:
                goalPosition = goalFront.position;
                goalRotation = goalFront.rotation;
                deviationDirection = randomDirection;
                break;
            case 3:
                goalPosition = goalBack.position;
                goalRotation = goalBack.rotation;
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

    //// forth
    //private float sineForth(float localCurrentTime)
    //{
    //    float B = Mathf.PI / Mathf.Abs(deviationDuration); // frequency
    //    float C = deviationDuration / 4; // phase shift of sine wave (horizontal shift)
    //    deviationLerpValue = 0.5f * Mathf.Sin(B * (localCurrentTime - C)) + 0.5f; // Update the lerpValue calculation with the new amplitude. 0.5 * sin(pi * (x-0.5))+ 0.5 goes from 0 to 1 to 0 in 2s
    //    return deviationLerpValue;
    //}
    // // back
    // private float lerpDeviateBack(float localCurrentTime)
    // {
    //     float B = 2 * Mathf.PI / Mathf.Abs(deviationDuration); // frequency
    //     float C = deviationDuration / 4; // phase shift of sine wave (horizontal shift)
    //     deviationLerpValue = 0.5f * Mathf.Sin(B * (localCurrentTime - C)) + 0.5f; // Update the lerpValue calculation with the new amplitude. 0.5 * sin(pi * (x-0.5))+ 0.5 goes from 0 to 1 to 0 in 2s
    //     return deviationLerpValue;
    // }


}

