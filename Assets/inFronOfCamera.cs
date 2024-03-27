using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class inFronOfCamera : MonoBehaviour
{
    public Transform mainCamera;
    public InputActionProperty thumbButtonB;
    //public GameObject readyText;
    public TextMeshProUGUI textMeshProToChange;
    private ChangeText textScript;
    public InputActionProperty thumbButtonX;
    public bool ready;
    public GameObject shortRecenterMessage;
    public float distance;


    private void Start()
    {
        thumbButtonB.action.performed += OnThumbB;
        thumbButtonX.action.performed += OnThumbX;
        //textMeshProToChange = readyText.GetComponentInChildren<TextMeshProUGUI>();
        textMeshProToChange.color = Color.red;
        textScript = GetComponent<ChangeText>();
        textScript.ChangeTextFcn("NOT READY");

        // recenter 
        OnThumbB(default);
    }

    private void Update()
    {
        if (mainCamera != null)
        {
            // Calculate the position in front of the main camera
            Vector3 newPosition = mainCamera.transform.position + mainCamera.transform.forward * 8.0f;

            // Update the position of the object
            //transform.position = newPosition;
            shortRecenterMessage.transform.position = new Vector3(newPosition.x, 5.2f, newPosition.z);


            // Calculate the direction from the object to the main camera
            Vector3 lookDir = mainCamera.transform.position - shortRecenterMessage.transform.position;

            // Ensure the object is not rotated in the x or z axis
            lookDir.y = 0;

            // Rotate the object to face the main camera
            shortRecenterMessage.transform.rotation = Quaternion.LookRotation(lookDir);

            // Flip the object 180 degrees around its vertical Y-axis
            shortRecenterMessage.transform.Rotate(Vector3.up, 180f);
        }
        distance = Vector3.Distance(transform.position, shortRecenterMessage.transform.position);
        print(distance);
        if (distance > 3 )
        {
            shortRecenterMessage.SetActive(true);
            // center message in front of participant

        }
        else
        {
            shortRecenterMessage.SetActive(false);
        }



    }


    // Update is called once per frame
    void OnThumbX(InputAction.CallbackContext context)
    {
        if (textMeshProToChange.color == Color.green)
        {
            ready = false;
            textScript.ChangeTextFcn("NOT READY");
            textMeshProToChange.color = Color.red;
        }
        else if (textMeshProToChange.color == Color.red)
        {
            ready = true;
            textScript.ChangeTextFcn("READY");
            textMeshProToChange.color = Color.green;
        }
    }

    void OnThumbB(InputAction.CallbackContext context) // recenter instruction
    {
        // center instruction in front of participant
        if (mainCamera != null)
        {
            // Calculate the position in front of the main camera
            Vector3 newPosition = mainCamera.transform.position + mainCamera.transform.forward * 8.0f;

            // Update the position of the object
            //transform.position = newPosition;
            transform.position = new Vector3(newPosition.x, 5.2f, newPosition.z);


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
