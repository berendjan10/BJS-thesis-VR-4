using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpHmd : MonoBehaviour
{
    public Transform hmdTarget;
    public float lerpValue = 0;
    public float playDuration = 5; // 5 seconds

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private bool _isPlaying = true;

    // Start is called before the first frame update
    void Start()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        playDuration -= Time.deltaTime;

        if(playDuration <= 0)
        {
            _isPlaying = false;
            transform.position = hmdTarget.position;
            transform.rotation = hmdTarget.rotation;
        }

        if (_isPlaying)
        {
            transform.position = Vector3.Lerp(_startPosition, hmdTarget.position, lerpValue);
            transform.rotation = Quaternion.Slerp(transform.rotation, hmdTarget.rotation, lerpValue);
            lerpValue = Mathf.Abs(Mathf.Sin(Mathf.PI * Time.time/3));

        }

    }
}
