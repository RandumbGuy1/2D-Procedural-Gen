using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainGeneration : MonoBehaviour {

    [Header("Block Settings")]
    [SerializeField] private int grassLayer;
    [SerializeField] private Tile grass;
    [SerializeField] private int dirtLayer;
    [SerializeField] private Tile dirt;
    [SerializeField] private Tile stone;
    [SerializeField] private Tile tallGrass;
    [SerializeField] private int tallGrassChance;

    [Header("Tree Settings")]
    [SerializeField] private Tile treeTrunk;
    [SerializeField] private Tile treeLeaf;
    [SerializeField] private int treeChance;
    [SerializeField] private int maxTreeHeight;
    [SerializeField] private int minTreeHeight;

    [Header("Refrences Settings")]
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap mapPrefab;
    [SerializeField] private Tilemap treeMap;

    [Header("Ground Generation Settings")]
    [SerializeField] private int chunkSize;
    [SerializeField] private float heightMultiplier;
    [SerializeField] private float heightAddition;
    [SerializeField] private Vector2Int worldSize;
    [SerializeField] private float caveFrequency;
    [SerializeField] private float surfaceFrequency;
    [SerializeField] private float seed;
    [SerializeField] private Texture2D noiseTexture;
    [SerializeField] private Tilemap[] worldChunks;

    [System.Serializable]
    public struct OreSettings {
        [SerializeField] private Tile oreTile;
        [SerializeField] private float oreFrequency;
        [SerializeField] private float oreSize;
        [SerializeField] private float oreSpawnHeight;
        [SerializeField] private Texture2D oreNoiseTexture;

        public Tile OreTile => oreTile;
        public float OreFrequency => oreFrequency;
        public float OreSize => oreSize;
        public float OreSpawnHeight => oreSpawnHeight;
        public Texture2D OreNoiseTexture { get => oreNoiseTexture; set { oreNoiseTexture = value; } }
    }

    [Header("Ore Generation Settings")]
    [SerializeField] private OreSettings[] ores = new OreSettings[0];

    void OnValidate() {
        for (int i = 0; i < ores.Length; i++) 
            ores[i].OreNoiseTexture = GenerateNoiseTexture(ores[i].OreFrequency, ores[i].OreSize);
    }

    void Start() {
        for (int i = 0; i < ores.Length; i++) {
            seed = Random.Range(-1000000, 100000);
            ores[i].OreNoiseTexture = GenerateNoiseTexture(ores[i].OreFrequency, ores[i].OreSize);
        }

        seed = Random.Range(-1000000, 100000);
        CreateChunks();
        GenerateTerrain();

        grid.transform.position -= new Vector3(worldSize.x * 0.5f, heightAddition, 0);
    }

    private void CreateChunks() {
        int numberOfChunks = worldSize.x / chunkSize;
        worldChunks = new Tilemap[numberOfChunks];

        for (int i = 0; i < numberOfChunks; i++) {
            Tilemap newChunk = Instantiate(mapPrefab.gameObject).GetComponent<Tilemap>();
            newChunk.transform.SetParent(grid.transform);
            newChunk.transform.localPosition = Vector3.zero;

            worldChunks[i] = newChunk;
        }
    }

    private Texture2D GenerateNoiseTexture(float frequency) {
        Texture2D noiseTexture = new Texture2D(worldSize.x, worldSize.y);

        for (int x = 0; x < noiseTexture.width; x++) {
            for (int y = 0; y < noiseTexture.height; y++) {
                float v = Mathf.PerlinNoise(x * frequency, y * frequency);
                noiseTexture.SetPixel(x, y, new Color(v, v, v));
            }
        }

        noiseTexture.Apply();
        return noiseTexture;
    }

    private Texture2D GenerateNoiseTexture(float frequency, float threshold)
    {
        Texture2D noiseTexture = new Texture2D(worldSize.x, worldSize.y);

        for (int x = 0; x < noiseTexture.width; x++) {
            for (int y = 0; y < noiseTexture.height; y++) {
                float v = Mathf.PerlinNoise(x * frequency, y * frequency);
                
                if (v > threshold) noiseTexture.SetPixel(x, y, Color.black);
                else noiseTexture.SetPixel(x, y, Color.white);
            }
        }

        noiseTexture.Apply();
        return noiseTexture;
    }

    private void GenerateTerrain() {
        noiseTexture = GenerateNoiseTexture(caveFrequency);

        for (int x = 0; x < noiseTexture.width; x++) {
            float height = Mathf.PerlinNoise((x + seed) * surfaceFrequency, seed * surfaceFrequency) * heightMultiplier + heightAddition;
            for (int y = 0; y < height; y++) {
                if (noiseTexture.GetPixel(x, y).r > 0.8f) continue;

                Tile tileToChoose;
                if (y > height - grassLayer) {
                    tileToChoose = grass;

                    int roll = Random.Range(1, 100);
                    if (roll < treeChance)
                    {
                        GenerateTree(new Vector2Int(x, y));
                    }
                    else if (roll < tallGrassChance) PlaceTile(treeMap, tallGrass, new Vector2Int(x, y + 1));
                }
                else if (y > height - dirtLayer) tileToChoose = dirt;
                else {
                    tileToChoose = stone;

                    foreach (OreSettings ore in ores) {
                        if (y > height - ore.OreSpawnHeight) continue;
                        if (ore.OreNoiseTexture.GetPixel(x, y) == Color.white) tileToChoose = ore.OreTile;
                    }
                }

                float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize) / chunkSize;
                PlaceTile(worldChunks[(int) chunkCoord], tileToChoose, new Vector2Int(x, y));
            }
        }
    }

    public void GenerateTree(Vector2Int pos) {
        int height = Random.Range(minTreeHeight, maxTreeHeight);

        for (int i = 1; i <= height; i++) PlaceTile(treeMap, treeTrunk, new Vector2Int(pos.x, pos.y + i));

        for (int i = height + 1; i < height + 4; i++) {
            PlaceTile(treeMap, treeLeaf, new Vector2Int(pos.x, pos.y + i));

            if (Random.Range(1, 10) < 9) PlaceTile(treeMap, treeLeaf, new Vector2Int(pos.x - 1, pos.y + i));
            if (Random.Range(1, 10) < 9) PlaceTile(treeMap, treeLeaf, new Vector2Int(pos.x + 1, pos.y + i));
        }
    }

    public void PlaceTile(Tilemap map, Tile tile, Vector2Int pos) {
        map.SetTile(new Vector3Int(pos.x, pos.y), tile);
    }
}
