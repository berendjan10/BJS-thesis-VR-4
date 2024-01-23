using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;

public class AvatarHeadMovement : MonoBehaviour
{
    public Transform hmdTarget;
    public Transform goal;
    private float lerpValue = 0;
    private float currentTime = 0; // clock

    private Vector3 _goalPosition;
    private Quaternion _goalRotation;
    private bool _isPlaying = false;
    public bool deviate = false;
    public float duration = 2.0f; // duration of deviation
    public bool thirdPersonPerspective = false;
    public GameObject cameraOffset;
    public GameObject mirror;
    private Vector3 standardCameraOffsetPosition = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 standardCameraOffsetRotation = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 thirdPersonPerspectiveOffsetPosition = new Vector3(0.0f, 0.0f, -0.5f);
    public Vector3 thirdPersonPerspectiveOffsetRotation = new Vector3(0.0f, 0.0f, 0.0f);


    // standard position
    // offset add to cameraoffset
    // offset subtract from head target!!!


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
        if (!thirdPersonPerspective)
        {
            mirror.SetActive(true);
            cameraOffset.transform.position = standardCameraOffsetPosition;
            cameraOffset.transform.rotation = Quaternion.Euler(standardCameraOffsetRotation); // 0,0,0
        }
        else if (thirdPersonPerspective)
        {
            mirror.SetActive(false);
            if (cameraOffset != null)
            {
                cameraOffset.transform.position = standardCameraOffsetPosition + thirdPersonPerspectiveOffsetPosition;
                cameraOffset.transform.rotation = Quaternion.Euler(standardCameraOffsetRotation) * Quaternion.Euler(thirdPersonPerspectiveOffsetRotation);
            }
            else
            {
                Debug.LogError("cameraOffset reference not set in the inspector.");
            }
        }




        if (!deviate) 
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
                transform.rotation = hmdTarget.rotation * Quaternion.Euler(-thirdPersonPerspectiveOffsetRotation);
            }
        }
        else if (deviate)
        {
            currentTime += Time.deltaTime; // add the time since last frame to the total duration

            if (currentTime < 0 || currentTime > 2) // no deviation
            {
                _isPlaying = false;
                // make sure that if simulation is not playing that avatar follows HMD. SELF = HMD
                if (!thirdPersonPerspective)
                {
                    transform.position = hmdTarget.position;
                    transform.rotation = hmdTarget.rotation;
                }
                else if (thirdPersonPerspective)
                {
                    // subtract offset so avatar stayus in place
                    transform.position = hmdTarget.position - thirdPersonPerspectiveOffsetPosition;
                    transform.rotation = hmdTarget.rotation * Quaternion.Euler(-thirdPersonPerspectiveOffsetRotation);
                }
            }
            else if (currentTime >= 0 && currentTime <= 2)  // Simulation
            {
                _isPlaying = true;
            }

            if (_isPlaying) // je wil in 2 seconden 1 hele sinus doorlopen
            {
                if (!thirdPersonPerspective)
                {
                    float A = 0.5f; // amplitude
                    float period = duration; // period of the sine wave (how many seconds for 1 full cycle)
                    float B = 2 * Mathf.PI / Mathf.Abs(period); // frequency
                    float C = duration / 4; // phase shift of sine wave (horizontal shift)
                    float D = 0.5f; // vertical shift of the sine wave (0 to 1)
                    // Update the lerpValue calculation with the new amplitude
                    lerpValue = A * Mathf.Sin(B * (currentTime - C)) + D; // 0.5 * sin(pi * (x-0.5))+ 0.5 goes from 0 to 1 to 0 in 2s
                    transform.position = Vector3.Lerp(hmdTarget.position, _goalPosition, lerpValue); // position linear interpolation between HMD & target
                    // transform.rotation = Quaternion.Slerp(transform.rotation, hmdTarget.rotation, lerpValue); // rotation linear interpolation between HMD & target
                }
                else if (thirdPersonPerspective)
                {
                    float A = 0.5f; // amplitude
                    float period = duration; // period of the sine wave (how many seconds for 1 full cycle)
                    float B = 2 * Mathf.PI / Mathf.Abs(period); // frequency
                    float C = duration / 4; // phase shift of sine wave (horizontal shift)
                    float D = 0.5f; // vertical shift of the sine wave (0 to 1)
                    // Update the lerpValue calculation with the new amplitude
                    lerpValue = A * Mathf.Sin(B * (currentTime - C)) + D; // 0.5 * sin(pi * (x-0.5))+ 0.5 goes from 0 to 1 to 0 in 2s
                    transform.position = Vector3.Lerp(hmdTarget.position - thirdPersonPerspectiveOffsetPosition, _goalPosition, lerpValue); // position linear interpolation between HMD & target
                    // transform.rotation = Quaternion.Slerp(transform.rotation, hmdTarget.rotation, lerpValue); // rotation linear interpolation between HMD & target
                }
            }
        }
    }

    public void TriggerAnimation()
    {
        currentTime = 0; // reset clock
        deviate = true;
        _isPlaying = true;
    }

}
