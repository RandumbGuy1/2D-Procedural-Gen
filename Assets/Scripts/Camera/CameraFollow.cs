using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    [SerializeField] private Vector2 offset;
    [SerializeField] private float smoothTime;
    [SerializeField] private Transform target;
    private Vector2 vel = Vector2.zero;

    void LateUpdate() {
        Vector2 newPos = Vector2.SmoothDamp(transform.position, (Vector2) target.position + offset, ref vel, smoothTime);
        transform.position = newPos;
    }
}
