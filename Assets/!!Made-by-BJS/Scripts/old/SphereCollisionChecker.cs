using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    private bool isColliding = false;

    private void OnCollisionStay(Collision collision)
    {
        isColliding = true;
    }

    public bool IsColliding()
    {
        return isColliding;
    }
}