using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRef : MonoBehaviour {
    [Header("Central Player Refrences")]
    [SerializeField] private PlayerMovement2D playerMovement;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Camera _camera;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private CapsuleCollider2D capsuleCol;

    public CapsuleCollider2D CapsuleCol => capsuleCol;
    public MeshRenderer Rendering => meshRenderer;
    public PlayerMovement2D PlayerMovement => playerMovement;
    public PlayerInput PlayerInput => playerInput;
    public Camera PlayerCam => _camera;
}
