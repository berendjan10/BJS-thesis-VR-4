using UnityEngine;

public class PositionInFrontOfMainCamera : MonoBehaviour
{
    public float distance = 3.0f; // Distance from the camera to place the object

    public Camera mainCamera;

    private void Start()
    {
        // Find the main camera in the scene
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found in the scene!");
            return;
        }

        // Set the initial position of the object
        InstructionInFrontOfCamera();
    }

    private void Update()
    {
        // Update the position of the object every frame
        InstructionInFrontOfCamera();
    }

    private void InstructionInFrontOfCamera()
    {
        if (mainCamera != null)
        {
            // Calculate the position in front of the main camera
            Vector3 newPosition = mainCamera.transform.position + mainCamera.transform.forward * distance;

            // Update the position of the object
            transform.position = newPosition;

            // Calculate the direction from the object to the main camera
            Vector3 lookDir = mainCamera.transform.position - transform.position;

            // Ensure the object is not rotated in the x or z axis
            lookDir.y = 0;

            // Rotate the object to face the main camera
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }
}
