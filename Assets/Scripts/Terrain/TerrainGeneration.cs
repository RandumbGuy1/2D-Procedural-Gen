using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class TerrainGeneration : MonoBehaviour {

    [Header("Refrences Settings")]
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap mapPrefab;
    public Grid Grid => grid;

    [Header("World Settings")]
    [SerializeField] private Tilemap[] worldChunks;

    [SerializeField] private float seed;
    [SerializeField] private int chunkSize;
    [SerializeField] private Vector2Int worldSize;
    [SerializeField] private Vector2 worldOffset;
    public Vector2Int WorldSize => worldSize;

    [Header("Shadow Settings")]
    [SerializeField] private GameObject shadowPrefab;
    [SerializeField] private bool enableShadows = false;

    [Header("Biomes")]
    [SerializeField] private BasicBiome forestBiome = new BasicBiome();
    [SerializeField] private BasicBiome snowBiome = new BasicBiome();
    [SerializeField] private BasicBiome desertBiome = new BasicBiome();

    public IBiome[] Biomes { get; private set; }

    [Header("Biome Generation Settings")]
    [SerializeField] private bool validateBiomes = false;
    [SerializeField] private Texture2D biomeNoiseTexture;
    [SerializeField] private float biomeFrequency;
    [SerializeField] private Gradient biomeColors;
    public Gradient BiomeColors => biomeColors;
    public Texture2D BiomeNoiseTexture => biomeNoiseTexture;

    [Header("Ore Generation Settings")]
    [SerializeField] private bool validateOres = false;
    [SerializeField] private OreSettings[] baseOres = new OreSettings[0];
    public OreSettings[] BaseOres => baseOres;

    public static TerrainGeneration Instance { get; private set; }

    void Awake() => Instance = this;

    void OnValidate() {
        Biomes = new IBiome[3] { forestBiome, snowBiome, desertBiome };

        if (validateBiomes) 
            biomeNoiseTexture = GenerateNoiseTexture(biomeColors, new NoiseTextureParams(biomeFrequency, 0f, Color.black, true, false));

        if (validateOres) 
            for (int i = 0; i < baseOres.Length; i++)
                baseOres[i].OreNoiseTexture = GenerateNoiseTexture(new NoiseTextureParams(baseOres[i].OreFrequency, baseOres[i].OreSize, Color.white, true));

        foreach (IBiome biome in Biomes) 
            if (biome.UpdateInInspector) biome.Start();
    }

    void Start() {
        Biomes = new IBiome[3] { forestBiome, snowBiome, desertBiome };

        for (int i = 0; i < baseOres.Length; i++)
            baseOres[i].OreNoiseTexture = GenerateNoiseTexture(new NoiseTextureParams(baseOres[i].OreFrequency, baseOres[i].OreSize, Color.white, true));

        biomeNoiseTexture = GenerateNoiseTexture(biomeColors, new NoiseTextureParams(biomeFrequency, 0f, Color.black, true, false));
        foreach (IBiome biome in Biomes) biome.Start();

        seed = Random.Range(-1000000, 100000);
        CreateChunks();

        foreach (IBiome biome in Biomes) biome.GenerateTerrain(seed);

        grid.transform.position += (Vector3) worldOffset;
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

    public Texture2D GenerateNoiseTexture(float frequency) {
        Texture2D noiseTexture = new Texture2D(worldSize.x, worldSize.y);

        for (int x = 0; x < noiseTexture.width; x++) {
            for (int y = 0; y < noiseTexture.height; y++) {
                float v = Mathf.PerlinNoise((seed + x) * frequency, (seed + y) * frequency);
                noiseTexture.SetPixel(x, y, new Color(v, v, v));
            }
        }

        noiseTexture.Apply();
        return noiseTexture;
    }

    public Texture2D GenerateNoiseTexture(NoiseTextureParams parameters) {
        float seed = this.seed;
        if (parameters.NewSeed) seed = Random.Range(-1000000, 100000);

        Texture2D noiseTexture = new Texture2D(worldSize.x, worldSize.y);

        for (int x = 0; x < noiseTexture.width; x++) {
            for (int y = 0; y < noiseTexture.height; y++) {
                float v = Mathf.PerlinNoise((seed + x) * parameters.Frequency, (parameters.IgnoreY ? seed : seed + y) * parameters.Frequency);
                
                if (v > parameters.Threshold) noiseTexture.SetPixel(x, y, Color.black);
                else noiseTexture.SetPixel(x, y, parameters.Color);
            }
        }

        noiseTexture.Apply();
        return noiseTexture;
    }

    public Texture2D GenerateNoiseTexture(Gradient gradient, NoiseTextureParams parameters) {
        float seed = this.seed;
        if (parameters.NewSeed) seed = Random.Range(-1000000, 100000);

        Texture2D noiseTexture = new Texture2D(worldSize.x, worldSize.y);

        for (int x = 0; x < noiseTexture.width; x++) {
            for (int y = 0; y < noiseTexture.height; y++) {
                float v = Mathf.PerlinNoise((seed + x) * parameters.Frequency, (parameters.IgnoreY ? seed : seed + y) * parameters.Frequency);
                noiseTexture.SetPixel(x, y, gradient.Evaluate(v));
            }
        }

        noiseTexture.Apply();
        return noiseTexture;
    }

    public Tilemap GetChunkMap(float x) {
        int coords = (int)Mathf.Floor(x / chunkSize);

        return worldChunks[coords];
    }

    public void PlaceTile(Tilemap map, Tile tile, Vector2Int pos) {
        map.SetTile(new Vector3Int(pos.x, pos.y), tile);
    }

    private bool startShadows = true;
    public void PlaceTileWithShadow(Tilemap map, Tile tile, Vector2Int pos)
    {
        map.SetTile(new Vector3Int(pos.x, pos.y), tile);

        if (!enableShadows) return;

        Transform shadow = Instantiate(shadowPrefab, grid.transform).transform;
        shadow.localPosition = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f) * 0.5f;

        if (startShadows) {
            grid.gameObject.AddComponent<CompositeShadowCaster2D>();
            startShadows = false;
        }
    }

    public Vector3Int GetClosestTilePosInMap(Vector3 pos, Tilemap map) {
        Vector3Int closestTilePos = new Vector3Int(0, 0);
        for (int x = map.cellBounds.min.x; x < map.cellBounds.max.x; x++) {
            for (int y = map.cellBounds.min.y; y < map.cellBounds.max.y; y++) {
                Vector3Int testPos = new Vector3Int(x, y, 0);
                TileBase tile = map.GetTile(testPos);
                if (tile == null) continue;

                closestTilePos = (pos - testPos).sqrMagnitude < (pos - closestTilePos).sqrMagnitude ? testPos : closestTilePos;
            }
        }

        return closestTilePos;
    }
}

[System.Serializable]
public struct OreSettings
{
    [SerializeField] private string oreName;
    [SerializeField] private Tile oreTile;
    [SerializeField] private float oreFrequency;
    [SerializeField] private float oreSize;
    [SerializeField] private float oreSpawnHeight;
    [SerializeField] private Texture2D oreNoiseTexture;

    public string OreName => oreName;
    public Tile OreTile => oreTile;
    public float OreFrequency => oreFrequency;
    public float OreSize => oreSize;
    public float OreSpawnHeight => oreSpawnHeight;
    public Texture2D OreNoiseTexture { get => oreNoiseTexture; set { oreNoiseTexture = value; } }
}

public struct NoiseTextureParams
{
    public float Frequency { get; private set; }
    public float Threshold { get; private set; }
    public Color Color { get; private set; }
    public bool NewSeed { get; private set; }
    public bool IgnoreY { get; private set; }

    public NoiseTextureParams(float frequency, float threshold, Color color, bool newSeed = false, bool ignoreY = false)
    {
        Frequency = frequency;
        Threshold = threshold;
        Color = color;
        NewSeed = newSeed;
        IgnoreY = ignoreY;
    }
}
