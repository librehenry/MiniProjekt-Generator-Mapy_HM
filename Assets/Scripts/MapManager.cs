using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform tileParent;
    public int width = 10;
    public int height = 10;

    private MapTile[,] map;
    private CancellationTokenSource cts;

    public async Task GenerateMapAsync(int width, int height, Action<float> onProgress, Action<string> onLog, CancellationToken token)
    {
        map = new MapTile[width, height];

        for (int y = 0; y < height; y++)
        {
            token.ThrowIfCancellationRequested();

            for (int x = 0; x < width; x++)
            {
                map[x, y] = new MapTile();
            }

            onProgress?.Invoke((float)(y + 1) / height);
            onLog?.Invoke($"Zainicjalizowano wiersz {y}");
            await Task.Yield(); // pozwala Unity zaktualizowa? GUI
        }

        System.Random rand = new();
        int obstacleCount = (int)(width * height * 0.1f);
        int resourceCount = (int)(width * height * 0.02f);

        await PlaceRandomTiles(TileType.Obstacle, obstacleCount, rand, token);
        await PlaceRandomTiles(TileType.Resource, resourceCount, rand, token);

        RenderMap();
    }

    private async Task PlaceRandomTiles(TileType type, int count, System.Random rand, CancellationToken token)
    {
        int placed = 0;
        while (placed < count)
        {
            token.ThrowIfCancellationRequested();

            int x = rand.Next(width);
            int y = rand.Next(height);

            if (map[x, y].Type == TileType.Empty)
            {
                map[x, y].Type = type;
                placed++;
                await Task.Yield();
            }
        }
    }

    public void RenderMap()
    {
        foreach (Transform child in tileParent)
        {
            Destroy(child.gameObject);
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject tile = Instantiate(tilePrefab, tileParent);
                tile.transform.localPosition = new Vector3(x, -y, 0);

                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                switch (map[x, y].Type)
                {
                    case TileType.Obstacle: sr.color = Color.gray; break;
                    case TileType.Resource: sr.color = Color.green; break;
                    case TileType.Empty: sr.color = Color.black; break;
                }
            }
        }
    }

    public void CancelGeneration()
    {
        cts?.Cancel();
    }

    public void StartGeneration(UIManager ui)
    {
        cts = new CancellationTokenSource();
        _ = GenerateMapAsync(width, height, ui.UpdateProgress, ui.LogMessage, cts.Token);
    }
    /*
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
