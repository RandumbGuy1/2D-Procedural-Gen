using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeDummy : MonoBehaviour
{
    [SerializeField] private CameraShaker cameraShaker;
    private Quaternion startRotation = Quaternion.identity;

    void Start()
    {
        startRotation = transform.rotation;
    }

    void Update()
    {
        transform.rotation = startRotation * Quaternion.Euler(cameraShaker.Offset);
    }
}
