using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;

public class AvatarHeadMovement : MonoBehaviour
{
    public Transform hmdTarget;
    public Transform goal;
    private float lerpValue = 0;
    private float deviationCurrentTime = 0; // clock

    private Vector3 _goalPosition;
    private Quaternion _goalRotation;
    private bool _isPlaying = false;
    public bool deviate = false;
    public float duration = 2.0f; // duration of deviation
    public bool thirdPersonPerspective = false;
    public bool smoothTransition = false;
    public GameObject cameraOffset;
    public GameObject mirror;
    private Vector3 standardCameraOffsetPosition = new Vector3();
    private Vector3 standardCameraOffsetRotation = new Vector3();
    public Vector3 thirdPersonPerspectiveOffsetPosition = new Vector3();
    //public Vector3 thirdPersonPerspectiveOffsetRotation = new Vector3(15.0f, 0.0f, 0.0f);

    public GameObject leftHandTarget;
    public GameObject leftHandTargetFollow;
    public GameObject rightHandTarget;
    public GameObject rightHandTargetFollow;

    // Start is called before the first frame update
    void Start()
    {
        _goalPosition = goal.position; // reference to object script is attached to. Save the target position
        _goalRotation = goal.rotation; // reference to object script is attached to
    }

    // Update is called once per frame
    void Update()
    {
        // Set camera offset
        leftHandTargetFollow.transform.rotation = leftHandTarget.transform.rotation;
        rightHandTargetFollow.transform.rotation = rightHandTarget.transform.rotation;

        if (!thirdPersonPerspective)
        {
            mirror.SetActive(true);
            cameraOffset.transform.position = standardCameraOffsetPosition;
            //cameraOffset.transform.rotation = Quaternion.Euler(standardCameraOffsetRotation); // 0,0,0

            leftHandTargetFollow.transform.position = leftHandTarget.transform.position;

            rightHandTargetFollow.transform.position = rightHandTarget.transform.position;
        }
        else if (thirdPersonPerspective)
        {
            mirror.SetActive(false);

            cameraOffset.transform.position = standardCameraOffsetPosition + thirdPersonPerspectiveOffsetPosition;
            //cameraOffset.transform.rotation = Quaternion.Euler(standardCameraOffsetRotation + thirdPersonPerspectiveOffsetRotation);

            leftHandTargetFollow.transform.position = leftHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition;
            //leftHandTargetFollow.transform.rotation = leftHandTarget.transform.rotation * Quaternion.Euler(-thirdPersonPerspectiveOffsetRotation);

            rightHandTargetFollow.transform.position = rightHandTarget.transform.position - thirdPersonPerspectiveOffsetPosition;
            //rightHandTargetFollow.transform.rotation = rightHandTarget.transform.rotation * Quaternion.Euler(-thirdPersonPerspectiveOffsetRotation);
        }

        if (!deviate)
        {
            noDeviation();
        }
        else if (deviate)
        {
            deviationCurrentTime += Time.deltaTime; // add the time since last frame to the total duration

            if (deviationCurrentTime >= 0 && deviationCurrentTime <= 2)  // Simulation
            {
                _isPlaying = true;
            }

            else if (deviationCurrentTime < 0 || deviationCurrentTime > 2) // no deviation
            {
                _isPlaying = false;

                noDeviation();
            }

            if (_isPlaying) // je wil in 2 seconden 1 hele sinus doorlopen
            {
                lerpValue = sinusoidLerp(deviationCurrentTime);
                if (!thirdPersonPerspective)
                {
                    transform.position = Vector3.Lerp(hmdTarget.position, _goalPosition, lerpValue); // position linear interpolation between HMD & target
                }
                else if (thirdPersonPerspective)
                {
                    transform.position = Vector3.Lerp(hmdTarget.position - thirdPersonPerspectiveOffsetPosition, _goalPosition, lerpValue); // position linear interpolation between HMD & target
                }
                     transform.rotation = Quaternion.Slerp(hmdTarget.rotation, _goalRotation, lerpValue); // rotation linear interpolation between HMD & target
           }
        }
    }

    public void TriggerAnimation()
    {
        deviationCurrentTime = 0; // reset deviation clock
        deviate = true;
        _isPlaying = true;
    }

    private float sinusoidLerp(float localCurrentTime)
    {
        float A = 0.5f; // amplitude
        float period = duration; // period of the sine wave (how many seconds for 1 full cycle)
        float B = 2 * Mathf.PI / Mathf.Abs(period); // frequency
        float C = duration / 4; // phase shift of sine wave (horizontal shift)
        float D = 0.5f; // vertical shift of the sine wave (0 to 1)
        lerpValue = A * Mathf.Sin(B * (localCurrentTime - C)) + D; // Update the lerpValue calculation with the new amplitude. 0.5 * sin(pi * (x-0.5))+ 0.5 goes from 0 to 1 to 0 in 2s
        return lerpValue;
    }
    private void noDeviation()
    {
        if (!thirdPersonPerspective)
        {
            transform.position = hmdTarget.position;
            transform.rotation = hmdTarget.rotation;
        }
        else if (thirdPersonPerspective)
        {
            // subtract offset so avatar stayus in place
            transform.position = hmdTarget.position - thirdPersonPerspectiveOffsetPosition;
            transform.rotation = hmdTarget.rotation; //* Quaternion.Euler(-thirdPersonPerspectiveOffsetRotation);
        }
    }
}
