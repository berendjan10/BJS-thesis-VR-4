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

    // Start is called before the first frame update
    void Start()
    {
        _goalPosition = transform.position; // reference to object script is attached to
        _goalRotation = transform.rotation; // reference to object script is attached to
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime; // add the time since last frame to the total duration

        if(currentTime <= 2 || currentTime > 5) // no deviation
        {
            _isPlaying = false;
            transform.position = hmdTarget.position; // make sure that if simulation is not playing that avatar follows HMD.
            transform.rotation = hmdTarget.rotation; // make sure that if simulation is not playing that avatar follows HMD.
        }
        else if(currentTime > 2 && currentTime <= 3) // moving towards goal
        {
            _isPlaying = true;
        }
        else if(currentTime > 3 && currentTime <= 4) // at goal
        {
            _isPlaying = false;
            transform.position = _goalPosition; 
            transform.rotation = _goalRotation; 
        }
        else if(currentTime > 4 && currentTime <= 5) // moving back towards HMD
        {
            _isPlaying = true;
        }
        

        if (_isPlaying) // je wil in 2 seconden 1 hele sinus doorlopen
        {
            transform.position = Vector3.Lerp(_goalPosition, hmdTarget.position, lerpValue); // position linear interpolation between HMD & target
            transform.rotation = Quaternion.Slerp(transform.rotation, hmdTarget.rotation, lerpValue); // rotation linear interpolation between HMD & target
            lerpValue = Mathf.Sin(2 * Mathf.PI * (currentTime - 2) / 2); // calc lerpValue used in 2 lines above. a sinus of the time, one full cycle in 2s.        }
        }
    }
}
