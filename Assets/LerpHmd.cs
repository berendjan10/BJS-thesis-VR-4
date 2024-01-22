using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpHmd : MonoBehaviour
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


    // Start is called before the first frame update
    void Start()
    {
        _goalPosition = goal.position; // reference to object script is attached to. Save the target position
        _goalRotation = goal.rotation; // reference to object script is attached to
    }

    // Update is called once per frame
    void Update()
    {
        if (!deviate) 
        {
            transform.position = hmdTarget.position;
            transform.rotation = hmdTarget.rotation;
        }
        else
        {
            currentTime += Time.deltaTime; // add the time since last frame to the total duration

            if (currentTime < 0 || currentTime > 2) // no deviation
            {
                _isPlaying = false;
                transform.position = hmdTarget.position; // make sure that if simulation is not playing that avatar follows HMD. SELF = HMD
                transform.rotation = hmdTarget.rotation; // make sure that if simulation is not playing that avatar follows HMD.
            }
            else if (currentTime >= 0 && currentTime <= 2)  // Simulation
            {
                _isPlaying = true;
            }

            if (_isPlaying) // je wil in 2 seconden 1 hele sinus doorlopen
            {
                float A = 0.5f; // amplitude
                float period = duration ; // period of the sine wave (how many seconds for 1 full cycle)
                float B = 2*Mathf.PI / Mathf.Abs(period); // frequency
                float C = duration/4; // phase shift of sine wave (horizontal shift)
                float D = 0.5f; // vertical shift of the sine wave (0 to 1)
                // Update the lerpValue calculation with the new amplitude
                lerpValue = A * Mathf.Sin(B * (currentTime - C)) + D; // 0.5 * sin(pi * (x-0.5))+ 0.5 goes from 0 to 1 to 0 in 2s
                transform.position = Vector3.Lerp(hmdTarget.position, _goalPosition,lerpValue); // position linear interpolation between HMD & target
                // transform.rotation = Quaternion.Slerp(transform.rotation, hmdTarget.rotation, lerpValue); // rotation linear interpolation between HMD & target
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
