using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: rechts maken.

public class LerpHmd : MonoBehaviour
{
    public Transform hmdTarget;
    private float lerpValue = 0;
    private float currentTime = 0; // clock

    private Vector3 _goalPosition;
    private Quaternion _goalRotation;
    private bool _isPlaying = true;
    public bool switchOnOff = false;

    // Start is called before the first frame update
    void Start()
    {
        _goalPosition = transform.position; // reference to object script is attached to. Save the target position
        _goalRotation = transform.rotation; // reference to object script is attached to
    }

    // Update is called once per frame
    void Update()
    {
        if (!switchOnOff) 
        {
            transform.position = hmdTarget.position; // make sure that if simulation is not playing that avatar follows HMD. SELF = HMD
            transform.rotation = hmdTarget.rotation; // make sure that if simulation is not playing that avatar follows HMD.
        }
        else if (switchOnOff)
        {
            currentTime += Time.deltaTime; // add the time since last frame to the total duration

            if (currentTime <= 2 || currentTime > 4) // no deviation
            {
                _isPlaying = false;
                transform.position = hmdTarget.position; // make sure that if simulation is not playing that avatar follows HMD. SELF = HMD
                transform.rotation = hmdTarget.rotation; // make sure that if simulation is not playing that avatar follows HMD.
            }
            else if (currentTime > 2 && currentTime <= 4) // moving towards goal
            {
                _isPlaying = true;
            }
            //else if (currentTime > 3 && currentTime <= 5) // at goal
            //{
            //    _isPlaying = false;
            //    transform.position = _goalPosition;
            //    transform.rotation = _goalRotation;
            //}
            //else if (currentTime > 5 && currentTime <= 6) // moving back towards HMD
            //{
            //    _isPlaying = true;
            //}


            if (_isPlaying) // je wil in 2 seconden 1 hele sinus doorlopen
            {
                transform.position = Vector3.Lerp(_goalPosition, hmdTarget.position, lerpValue); // position linear interpolation between HMD & target
                transform.rotation = Quaternion.Slerp(transform.rotation, hmdTarget.rotation, lerpValue); // rotation linear interpolation between HMD & target
                float amplitude = 0.5f; // Set the amplitude to 0.5 to make the cosine function go from 1 to 0 to 1
                lerpValue = 0.5f + amplitude * Mathf.Cos(Mathf.PI * (currentTime - 2)); // Update the lerpValue calculation with the new amplitude. FROM 1 TO 0 TO 1!
                lerpValue = Mathf.Cos(Mathf.PI * (currentTime - 2)); // calc lerpValue used in 2 lines above. a sinus of the time, one full cycle in 2s.
            }
        }
    }

}
