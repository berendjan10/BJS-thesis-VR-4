using UnityEngine;

public class Height : MonoBehaviour
{
    [SerializeField] private float _height; // [cm]

    void Start()
    {
        float scale = _height / 185;
        transform.localScale = new Vector3(scale, scale, scale);
    }
}