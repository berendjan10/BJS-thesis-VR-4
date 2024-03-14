using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inFronOfCamera : MonoBehaviour
{
    public Transform mainCamera;


    // Update is called once per frame
    void Update()
    {

        if (mainCamera != null)
        {
            // Calculate the position in front of the main camera
            Vector3 newPosition = mainCamera.transform.position + mainCamera.transform.forward * 8.0f;

            // Update the position of the object
            //transform.position = newPosition;
            transform.position = new Vector3(newPosition.x, 4f, newPosition.z);


            // Calculate the direction from the object to the main camera
            Vector3 lookDir = mainCamera.transform.position - transform.position;

            // Ensure the object is not rotated in the x or z axis
            lookDir.y = 0;

            // Rotate the object to face the main camera
            transform.rotation = Quaternion.LookRotation(lookDir);

            // Flip the object 180 degrees around its vertical Y-axis
            transform.Rotate(Vector3.up, 180f);
        }
    }
}
