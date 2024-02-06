using UnityEngine;

public class Height : MonoBehaviour
{
    [SerializeField] private float _height; // [cm]
    [SerializeField] private GameObject topDisk; // [kg]

    void Start()
    {
        float scale = _height / 185;
        transform.localScale = new Vector3(scale, scale, scale);
        topDisk.transform.localPosition = new Vector3(0f, 1.4f * scale, -0.089f);

    }
}