using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour {
    [SerializeField] private TileData[] tileData = new TileData[0];
    public Dictionary<TileBase, TileData> DataFromTiles { get; private set; } = new Dictionary<TileBase, TileData>();

    public static TileManager Instance { get; private set; }

    void Awake() {
        foreach (TileData data in tileData)
            foreach (TileBase tile in data.AssociatedTiles)
                DataFromTiles.Add(tile, data);

        Instance = this;
    }
}
