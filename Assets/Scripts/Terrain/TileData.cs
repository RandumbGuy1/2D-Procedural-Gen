using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileData : ScriptableObject {
    [SerializeField] TileBase[] associatedTiles = new TileBase[0];

    [SerializeField] private string tileName;
    [SerializeField] private float timeToMine;
    [SerializeField] private float strengthRequred;
    [SerializeField] private AudioClip[] mineClips;
    [SerializeField] private AudioClip breakClip;
    [SerializeField] private GameObject breakFX;
    [SerializeField] private float rarity;
    [SerializeField] private float friction;
    [SerializeField] private GameObject drop;

    public TileBase[] AssociatedTiles => associatedTiles;
    public string TileName => tileName;
    public float TimeToMine => timeToMine;
    public AudioClip[] MineClips => mineClips;
    public AudioClip BreakClip => breakClip;
    public GameObject BreakFX => breakFX;
    public float Rarity => rarity;
    public float StrengthRequired => strengthRequred;
    public float Friction => friction;
    public GameObject Drop => drop;
}
