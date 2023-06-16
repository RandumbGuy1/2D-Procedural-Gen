using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileData : ScriptableObject {
    [SerializeField] TileBase[] associatedTiles = new TileBase[0];

    [SerializeField] private string tileName;
    [SerializeField] private float miningSpeed;
    [SerializeField] private float strength;
    [SerializeField] private float rarity;
    [SerializeField] private float friction;
    [SerializeField] private GameObject drop;

    public TileBase[] AssociatedTiles => associatedTiles;
    public string TileName => tileName;
    public float MiningSpeed => miningSpeed;
    public float Rarity => rarity;
    public float Strength => strength;
    public float Friction => friction;
    public GameObject Drop => drop;
}
