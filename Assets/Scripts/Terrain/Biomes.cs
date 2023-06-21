using UnityEngine;
using UnityEngine.Tilemaps;

public interface IBiome {
    bool UpdateInInspector { get; }

    Texture2D NoiseTexture { get; }
    void GenerateTerrain(float seed);
    void GenerateTree(Vector2Int pos);
    void Start();
}

[System.Serializable]
public class BasicBiome : IBiome {
    [SerializeField] private string name;
    [SerializeField] private TerrainGeneration generator;
    [SerializeField] private Color biomeValidation;

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
    [SerializeField] private Tilemap treeMap;

    [Header("Generation Settings")]
    [SerializeField] private bool updateInInspector;
    public bool UpdateInInspector => updateInInspector;

    [SerializeField] private float heightMultiplier;
    [SerializeField] private float heightAddition;
    [SerializeField] private float caveFrequency;
    [SerializeField] private float caveThreshold;
    [SerializeField] private float surfaceFrequency;
    [SerializeField] private Texture2D noiseTexture;
    public Texture2D NoiseTexture => noiseTexture;

    [Header("Ore Generation Settings")]
    [SerializeField] private OreSettings[] extraOres = new OreSettings[0];

    public void Start() {
        for (int i = 0; i < extraOres.Length; i++)
            extraOres[i].OreNoiseTexture = generator.GenerateNoiseTexture(new NoiseTextureParams(extraOres[i].OreFrequency, extraOres[i].OreSize, Color.white, true));
        
        noiseTexture = generator.GenerateNoiseTexture(caveFrequency);
    }

    public void GenerateTerrain(float seed) {
        noiseTexture = generator.GenerateNoiseTexture(caveFrequency);

        for (int x = 0; x < noiseTexture.width; x++) {
            //if (generator.BiomeColors.Evaluate(((float) x / (float) noiseTexture.width)) != biomeValidation) continue;
            //if (generator.BiomeNoiseTexture.GetPixel(x, 0) != biomeValidation) continue;

            float height = Mathf.PerlinNoise((x + seed) * surfaceFrequency, seed * surfaceFrequency) * heightMultiplier + heightAddition;
            for (int y = 0; y < height; y++) {
                if (generator.BiomeNoiseTexture.GetPixel(x, y) != biomeValidation) continue;

                if (noiseTexture.GetPixel(x, y).r > caveThreshold) continue;

                Tile tileToChoose;
                if (y > height - grassLayer) {
                    tileToChoose = grass;

                    int roll = Random.Range(1, 100);
                    if (roll < treeChance) GenerateTree(new Vector2Int(x, y));
                    else if (roll < tallGrassChance) generator.PlaceTile(treeMap, tallGrass, new Vector2Int(x, y + 1));
                }
                else if (y > height - dirtLayer) tileToChoose = dirt;
                else tileToChoose = stone;

                foreach (OreSettings ore in extraOres) {
                    if (y > height - ore.OreSpawnHeight) continue;
                    if (ore.OreNoiseTexture.GetPixel(x, y) == Color.white) tileToChoose = ore.OreTile;
                }

                foreach (OreSettings ore in generator.BaseOres) {
                    if (y > height - ore.OreSpawnHeight) continue;
                    if (ore.OreNoiseTexture.GetPixel(x, y) == Color.white) tileToChoose = ore.OreTile;
                }

                generator.PlaceTile(generator.GetChunkMap(x), tileToChoose, new Vector2Int(x, y));
            }
        }
    }

    public void GenerateTree(Vector2Int pos) {
        if (!treeTrunk) return;
        int height = Random.Range(minTreeHeight, maxTreeHeight);

        for (int i = 1; i <= height; i++) generator.PlaceTileWithShadow(treeMap, treeTrunk, new Vector2Int(pos.x, pos.y + i));

        if (!treeLeaf) return;

        for (int i = height + 1; i < height + 4; i++) {
            generator.PlaceTileWithShadow(treeMap, treeLeaf, new Vector2Int(pos.x, pos.y + i));

            if (Random.Range(1, 10) < 9) generator.PlaceTileWithShadow(treeMap, treeLeaf, new Vector2Int(pos.x - 1, pos.y + i));
            if (Random.Range(1, 10) < 9) generator.PlaceTileWithShadow(treeMap, treeLeaf, new Vector2Int(pos.x + 1, pos.y + i));
        }
    }
}
