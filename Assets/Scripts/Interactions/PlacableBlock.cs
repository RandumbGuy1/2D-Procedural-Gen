using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class PlacableBlock : MonoBehaviour, IInteractable, IItem
{
    [SerializeField] private int itemId;
    public int ItemID => itemId;

    [SerializeField] private AudioClip placeClip;
    [SerializeField] private Tile toPlace;
    [SerializeField] private int placeRange;
    [SerializeField] private Transform placeHighlight;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] HoverEffects hoverEffects;

    public PlayerRef Player { get; set; }
    public GameObject GameObject => gameObject;

    public ItemHoldSettings HoldSettings => holdSettings;
    public ItemArtSettings SpriteSettings => clubSpriteSettings;
    public List<ItemArtSettings> AbilitySpriteSettings => clubAbilitySpriteSettings;

    [SerializeField] private ItemHoldSettings holdSettings = new ItemHoldSettings();
    [SerializeField] private ItemArtSettings clubSpriteSettings = new ItemArtSettings();
    [SerializeField] private List<ItemArtSettings> clubAbilitySpriteSettings = new List<ItemArtSettings>();

    [SerializeField] private Rigidbody2D itemRb;

    public void OnDrop(PlayerRef player, Action<Rigidbody2D> DropForce) {
        player.PlayerInput.OnMouseButtonInput -= Place;
        placeHighlight.gameObject.SetActive(false);

        itemRb.simulated = true;
        DropForce(itemRb);
        Player = null;
    }

    public void ItemUpdate(PlayerRef player) {

    }

    private Tilemap chunk = null;
    private void Place(int mouseButton) {
        if (!gameObject.activeInHierarchy) {
            placeHighlight.gameObject.SetActive(false);
            return;
        }

        Vector2 mousePos = Player.PlayerCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPos = TerrainGeneration.Instance.Grid.WorldToCell(mousePos);
        chunk = TerrainGeneration.Instance.GetChunkMap(gridPos.x);

        TileBase clickedTile = chunk.GetTile(gridPos);

        if (clickedTile != null) {
            placeHighlight.gameObject.SetActive(false);
            return;
        }

        placeHighlight.gameObject.SetActive(true);
        placeHighlight.position = TerrainGeneration.Instance.Grid.CellToWorld(gridPos) + new Vector3(0.25f, 0.25f);

        if (mouseButton != 0) return;

        AudioManager.Instance.PlayOnce(placeClip);
        chunk.SetTile(gridPos, toPlace);

        Player.ItemController.DropItem();
        GameObject.SetActive(false);
    }

    public void OnPickup(PlayerRef player) {
        print(this);
        itemRb.simulated = false;
        player.PlayerInput.OnMouseButtonInput += Place;
        Player = player;
    }

    public string GetDescription(PlayerRef player) {
        return null;
    }

    public void OnHover(PlayerRef player) {
        hoverEffects.HoverFrames++;
    }

    void Update(){
        hoverEffects.UpdateHover();
    }

    public void OnInteract(PlayerRef player) {
        AudioManager.Instance.PlayOnce(pickupClip);
        player.ItemController.AddItem(gameObject);
    }
}
