using UnityEngine;
using UnityEngine.Rendering;

// helper class
[System.Serializable]
public class MapTransforms
{
    public Transform vrTarget;
    public Transform ikTarget;

    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;


    public void VRMapping()
    {
        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class AvatarController : MonoBehaviour
{
    [SerializeField] private MapTransforms head;
    [SerializeField] private MapTransforms leftHand;
    [SerializeField] private MapTransforms rightHand;

    [SerializeField] private float turnSmoothness;
    [SerializeField] Transform ikHead;
    [SerializeField] Vector3 headBodyOffset;

    [SerializeField] Transform hipAnchor;
    [SerializeField] Transform hipPosition;
    [SerializeField] private Transform spine;
    [SerializeField] Vector3 hipRotationOffset;
    [SerializeField] Transform headReference;


    private void LateUpdate()
    {
        // calculate & set spine rotation
        // Vector3 spineDirection = head.ikTarget.position - hipAnchor.position;
        //Vector3 spineDirection = headReference.position - hipAnchor.position;
        Vector3 spineDirection = headReference.position - hipAnchor.position;
        // spineDirection = Quaternion.Euler(hipRotationOffset) * spineDirection;
        Quaternion spineRotation = Quaternion.LookRotation(spineDirection, Vector3.forward);
        spine.rotation = spineRotation * Quaternion.Euler(hipRotationOffset);

        // The transform keyword in Unity refers to the transform component of the game object that the script is attached to. In this case, the transform keyword is used to set the position of the game object that the AvatarController script is attached to
        transform.position = ikHead.position + headBodyOffset;
        transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(ikHead.forward, Vector3.up).normalized, Time.deltaTime * turnSmoothness);

        head.VRMapping();
        leftHand.VRMapping();
        rightHand.VRMapping();


        hipPosition.position = hipAnchor.position;
        hipPosition.rotation = hipAnchor.rotation;



    }
}
