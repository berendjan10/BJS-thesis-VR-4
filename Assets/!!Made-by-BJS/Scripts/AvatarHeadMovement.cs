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

    private Vector3 goalPosition;
    private Quaternion goalRotation;
    private bool _isPlaying = false;
    [SerializeField] private bool thirdPersonPerspective = false;
    [SerializeField] private bool smoothTransition = false;
    [SerializeField] private float transitionStart;
    [SerializeField] private float transitionDuration; // duration of transition
    public float deviationDuration = 2.0f; // duration of deviation
    [SerializeField] private GameObject cameraOffset;
    [SerializeField] private GameObject mirror;
    [SerializeField] private GameObject gameInstructions;
    private Vector3 standardCameraOffsetPosition = new Vector3();
    private Vector3 standardCameraOffsetRotation = new Vector3();
    [SerializeField] private Vector3 thirdPersonPerspectiveOffsetPosition = new Vector3();
    //public Vector3 thirdPersonPerspectiveOffsetRotation = new Vector3(15.0f, 0.0f, 0.0f);

    public GameObject leftHandTarget;
    public GameObject leftHandTargetFollow;
    public GameObject rightHandTarget;
    public GameObject rightHandTargetFollow;

    // Start is called before the first frame update
    void Start()
    {
        // Get a reference to the GameManager instance
        GameManagerExp1 GameManager = GetComponent<GameManagerExp1>();
        GameManager.MoveDown(goalLeftG);
        GameManager.MoveDown(goalRightG);
        GameManager.MoveDown(goalFrontG);
        GameManager.MoveDown(goalBackG);
    }
     
    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        // Match head & hand target rotations with controllers (same for 1PP & 3PP)
        transform.rotation = hmdTarget.rotation;
        leftHandTargetFollow.transform.rotation = leftHandTarget.transform.rotation;
        rightHandTargetFollow.transform.rotation = rightHandTarget.transform.rotation;


        // First Person Perspective
        if (!thirdPersonPerspective)
        {
            firstPersonPerspective();

            // mirror is needed in 1PP
            mirror.SetActive(true);
            gameInstructions.transform.localPosition = new Vector3(0f, 1.189f, 1.042f);

        }

        // Third Person Perspective
        else if (thirdPersonPerspective)
        {
            if (!smoothTransition)
            {
                thirdPersonPerspectiveFcn();
            }
            else if (smoothTransition)
            {
                if (currentTime < transitionStart)
                {
                    firstPersonPerspective();
                }
                else if (currentTime >= transitionStart && currentTime <= (transitionStart + transitionDuration))
                {
                    transitionTime += Time.deltaTime;
                    transitionLerpValue = lerpTransition(transitionTime);
                    // transition camera view
                    cameraOffset.transform.position = Vector3.Lerp(standardCameraOffsetPosition, (standardCameraOffsetPosition + thirdPersonPerspectiveOffsetPosition), transitionLerpValue);
                    // transition avatar head & hands positions, compensate for camera offset
                    transform.position = Vector3.Lerp(hmdTarget.position, (hmdTarget.position - thirdPersonPerspectiveOffsetPosition), transitionLerpValue);
                    leftHandTargetFollow.transform.position = Vector3.Lerp(leftHandTarget.transform.position, (leftHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition), transitionLerpValue);
                    rightHandTargetFollow.transform.position = Vector3.Lerp(rightHandTarget.transform.position, (rightHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition), transitionLerpValue);


                }
                else if (currentTime > (transitionStart + transitionDuration))
                {
                    thirdPersonPerspectiveFcn();
                }
            }

            // mirror is not needed in 1PP
            mirror.SetActive(false);
            gameInstructions.transform.localPosition = new Vector3(0f, 1.442f, 0.394f);

        }

        // deviation
        deviationCurrentTime += Time.deltaTime;

        if (deviationCurrentTime >= 0 && deviationCurrentTime <= deviationDuration)  // Simulation
        {
            _isPlaying = true;
        }

        else if (deviationCurrentTime < 0 || deviationCurrentTime > deviationDuration) // no deviation
        {
            _isPlaying = false;
        }

        if (_isPlaying) // je wil in 2 seconden 1 hele sinus doorlopen
        {
            deviationLerpValue = lerpDeviate(deviationCurrentTime);

            // first person perspective
            if (!thirdPersonPerspective)
            {
                // The center of the arc
                Vector3 center = hipAnchor.transform.position; // hip pivot point

                // Interpolate over the arc relative to center
                Vector3 start = hmdTarget.position - center;
                Vector3 end = goalPosition - center;

                transform.position = Vector3.Slerp(start, end, deviationLerpValue) + center;
            }

            // third person perspective
            else if (thirdPersonPerspective)
            {
                transform.position = Vector3.Lerp(hmdTarget.position - thirdPersonPerspectiveOffsetPosition, goalPosition, deviationLerpValue); // position linear interpolation between HMD & target
            }

            // both perspectives
            transform.rotation = Quaternion.Slerp(hmdTarget.rotation, goalRotation, deviationLerpValue); // rotation linear interpolation between HMD & target
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
        transform.position = hmdTarget.position;
    }

    private void thirdPersonPerspectiveFcn()
    {
        // 3PP camera view
        cameraOffset.transform.position = standardCameraOffsetPosition + thirdPersonPerspectiveOffsetPosition;

        // 3PP avatar hand targets, compensate for camera offset
        leftHandTargetFollow.transform.position = leftHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition;
        rightHandTargetFollow.transform.position = rightHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition;

        // 3PP avatar head no deviation
        transform.position = hmdTarget.position - thirdPersonPerspectiveOffsetPosition;
    }

    public void TriggerAnimation(int randomDirection)
    {
        switch (randomDirection)
        {
            case 0:
                goalPosition = goalLeft.position;
                goalRotation = goalLeft.rotation;
                break;
            case 1:
                goalPosition = goalRight.position;
                goalRotation = goalRight.rotation;
                break;
            case 2:
                goalPosition = goalFront.position;
                goalRotation = goalFront.rotation;
                break;
            case 3:
                goalPosition = goalBack.position;
                goalRotation = goalBack.rotation;
                break;
            default:
                break;
            }
            deviationCurrentTime = 0; // reset deviation clock
        _isPlaying = true;
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
    private float lerpDeviate(float localCurrentTime)
    {
        float B = 2 * Mathf.PI / Mathf.Abs(deviationDuration); // frequency
        float C = deviationDuration / 4; // phase shift of sine wave (horizontal shift)
        deviationLerpValue = 0.5f * Mathf.Sin(B * (localCurrentTime - C)) + 0.5f; // Update the lerpValue calculation with the new amplitude. 0.5 * sin(pi * (x-0.5))+ 0.5 goes from 0 to 1 to 0 in 2s
        return deviationLerpValue;
    }



}

