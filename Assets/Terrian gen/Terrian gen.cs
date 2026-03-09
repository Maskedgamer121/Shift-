using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Infinite terrain generator - spawns tiles near the camera and keeps them loaded.
/// Attach to an empty GameObject in your scene.
/// </summary>
public class TerrainGenerator : MonoBehaviour
{
    [Header("Tile Sprites")]
    [SerializeField] private Sprite plainTile;
    [SerializeField] private Sprite decorativeTile;

    [Header("Settings")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private int bufferTiles = 2;
    [Range(0f, 1f)]
    [SerializeField] private float decorativeChance = 0.1f;

    private Camera mainCamera;
    private Dictionary<Vector2Int, GameObject> activeTiles = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, bool> tileTypes = new Dictionary<Vector2Int, bool>();
    private Transform tileParent;
    private int randomSeed;

    private void Start()
    {
        mainCamera = Camera.main;
        tileParent = new GameObject("Tiles").transform;
        randomSeed = Random.Range(0, 999999);
        GenerateVisibleTiles();
    }

    private void Update()
    {
        GenerateVisibleTiles(); // Only spawns new tiles, never removes
    }

    private void GenerateVisibleTiles()
    {
        float camHeight = mainCamera.orthographicSize;
        float camWidth  = camHeight * mainCamera.aspect;
        Vector3 camPos  = mainCamera.transform.position;

        int minX = Mathf.FloorToInt((camPos.x - camWidth)  / tileSize) - bufferTiles;
        int maxX = Mathf.FloorToInt((camPos.x + camWidth)  / tileSize) + bufferTiles;
        int minY = Mathf.FloorToInt((camPos.y - camHeight) / tileSize) - bufferTiles;
        int maxY = Mathf.FloorToInt((camPos.y + camHeight) / tileSize) + bufferTiles;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int tileCoord = new Vector2Int(x, y);
                if (!activeTiles.ContainsKey(tileCoord))
                    SpawnTile(tileCoord);
            }
        }
    }

    private void SpawnTile(Vector2Int coord)
    {
        Vector3 worldPos = new Vector3(coord.x * tileSize, coord.y * tileSize, 1f);

        GameObject tile = new GameObject("Tile_" + coord.x + "_" + coord.y);
        tile.transform.position = worldPos;
        tile.transform.parent = tileParent;

        SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();

        if (!tileTypes.ContainsKey(coord))
        {
            int hash = (coord.x * 73856093 ^ coord.y * 19349663 ^ randomSeed) & 0x7fffffff;
            tileTypes[coord] = (hash % 100) < (int)(decorativeChance * 100);
        }

        sr.sprite = tileTypes[coord] ? decorativeTile : plainTile;
        sr.sortingOrder = -10;

        float spriteWidth  = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;
        tile.transform.localScale = new Vector3(
            tileSize / spriteWidth,
            tileSize / spriteHeight,
            1f
        );

        activeTiles[coord] = tile;
    }
}