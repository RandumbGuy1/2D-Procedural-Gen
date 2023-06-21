using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class MiningTool : MonoBehaviour, IInteractable, IItem
{
    [SerializeField] private int itemId;
    public int ItemID => itemId;
    public int Stacked { get; set; } = 1;

    [Header("Mining Settings")]
    [SerializeField] private float miningSpeed = 1f;
    [SerializeField] private int miningRange;
    [SerializeField] private int miningStrength;
    [SerializeField] private LayerMask mineables;
    [SerializeField] private Transform mineHighlight;
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
        player.PlayerInput.OnMouseButtonInput -= Mine;
        mineHighlight.gameObject.SetActive(false);

        itemRb.simulated = true;
        DropForce(itemRb);
        Player = null;
    }

    public void ItemUpdate(PlayerRef player) {

    }

    private float[] soundQueues = { 0.1f, 0.3f };
    int soundIndex = 0;

    private Tilemap chunk = null;
    private Vector2 mineProgress = Vector2.zero;

    private void Mine(int mouseButton)
    {
        if (!gameObject.activeInHierarchy) {
            mineHighlight.gameObject.SetActive(false);
            return;
        }

        Vector2 mousePos = Player.PlayerCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPos = TerrainGeneration.Instance.Grid.WorldToCell(mousePos);

        chunk = TerrainGeneration.Instance.GetChunkMap(gridPos.x);

        if (Player.ItemController.SmartCursor) {
            Vector2 playerToMouse = mousePos - (Vector2)Player.transform.position;
            RaycastHit2D hit = Physics2D.CircleCast(Player.transform.position, 0.45f, playerToMouse, miningRange, mineables);
            if (hit.collider) {
                Vector3 worldPos = hit.point + playerToMouse.normalized * 0.1f;
                chunk = TerrainGeneration.Instance.GetChunkMap(TerrainGeneration.Instance.Grid.WorldToCell(worldPos).x);
                gridPos = TerrainGeneration.Instance.GetClosestTilePosInMap(TerrainGeneration.Instance.Grid.WorldToCell(worldPos), chunk);
            }
        }

        TileBase clickedTile = chunk.GetTile(gridPos);

        if (clickedTile == null || !TileManager.Instance.DataFromTiles.ContainsKey(clickedTile)) {
            mineHighlight.gameObject.SetActive(false);
            soundIndex = 0;
            mineProgress = Vector2.zero;
            return;
        }

        mineHighlight.gameObject.SetActive(true);
        Vector3 cellToWorldPos = TerrainGeneration.Instance.Grid.CellToWorld(gridPos);
        mineHighlight.position = cellToWorldPos + new Vector3(0.25f, 0.25f);

        if (mouseButton != 0) return;

        TileData data = TileManager.Instance.DataFromTiles[clickedTile];

        mineProgress += miningSpeed * Time.deltaTime * Vector2.one;

        if (mineProgress.x >= soundQueues[soundIndex]) {
            AudioManager.Instance.PlayOnce(data.MineClips, transform.position);
            mineProgress.x = 0f;

            ObjectPooler.Instance.Spawn(data.BreakFX.name, true, cellToWorldPos, Quaternion.identity);

            if (soundIndex < soundQueues.Length - 1) soundIndex++;
        }

        if (mineProgress.y >= data.TimeToMine) {
            AudioManager.Instance.PlayOnce(data.BreakClip);
            ObjectPooler.Instance.Spawn(data.BreakFX.name, true, cellToWorldPos, Quaternion.identity);
            if (data.Drop) ObjectPooler.Instance.Spawn(data.Drop.name, true, cellToWorldPos, Quaternion.identity);

            chunk.SetTile(gridPos, null);
            chunk.RefreshAllTiles();
            mineProgress = Vector2.zero;
        }
    }

    public void OnPickup(PlayerRef player) {
        itemRb.simulated = false;
        player.PlayerInput.OnMouseButtonInput += Mine;
        Player = player;
    }

    public string GetDescription(PlayerRef player) {
        return null;
    }

    public void OnHover(PlayerRef player) {
        hoverEffects.HoverFrames++;
    }

    void Update() {
        hoverEffects.UpdateHover();
    }

    public void OnInteract(PlayerRef player) {
        AudioManager.Instance.PlayOnce(pickupClip);
        player.ItemController.AddItem(gameObject);
    }
}
