using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerInteractions : MonoBehaviour
{
    [Header("Find Settings")]
    [SerializeField] private LayerMask interactables;
    [SerializeField] private float searchRadius;
    [SerializeField] private float maxSearchDistance;

    [Header("Refrences")]
    [SerializeField] private PlayerRef player;
    [SerializeField] private TerrainGeneration generator;

    void Awake()
    {
        player.PlayerInput.OnInteractInput += CheckForInteraction;
    }

    private void CheckForInteraction(bool interact)
    {
        Vector2 mousePos = player.PlayerCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 fromTo = mousePos - (Vector2) transform.position;

        RaycastHit2D hit = Physics2D.CircleCast(transform.position, searchRadius, fromTo, maxSearchDistance, interactables);
        if (!hit.collider) return;

        IInteractable interactable = hit.collider.GetComponent<IInteractable>();
        if (interactable == null) return;

        interactable.OnHover(player);

        if (interact) interactable.OnInteract(player);
    }
}
