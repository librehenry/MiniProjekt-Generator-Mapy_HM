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
    public UIManager uiManager; // Referencja do UIManagera

    public async Task GenerateMapAsync(
        int width, 
        int height, 
        float obstaclePercent, 
        float resourcePercent,
        Action<float> onProgress, 
        Action<string> onLog, 
        CancellationToken token
        )
    {
        Debug.Log("Przypisany UIManager: " + uiManager.name);
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
        int totalTiles = width * height;

        int obstacleCount = Mathf.RoundToInt(totalTiles * obstaclePercent); //(int)(width * height * 0.1f);
        int resourceCount = Mathf.RoundToInt(totalTiles * resourcePercent); //(int)(width * height * 0.02f);

        onLog?.Invoke("Generuję przeszkody...");
        await PlaceRandomTiles(TileType.Obstacle, obstacleCount, rand, token, onLog);

        onLog?.Invoke("Generuję zasoby...");
        await PlaceRandomTiles(TileType.Resource, resourceCount, rand, token, onLog);

        onLog?.Invoke("Renderuję mapę...");
        RenderMap();

        
        if (uiManager != null)
        {
            Texture2D tex = GenerateTexture();
            uiManager.ShowMapTexture(tex);
        }
        

        onLog?.Invoke("Generowanie zakończone.");
    }

    private async Task PlaceRandomTiles(
        TileType type, 
        int count, 
        System.Random rand,
        CancellationToken token,
        Action<string> onLog
        )
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
                if (placed % 100 == 0) // loguj co 100 elementów, żeby nie spamować
                {
                    onLog?.Invoke($"Umieszczono {placed}/{count} {type}");
                }
                await Task.Yield();
            }
        }
        onLog?.Invoke($"{type} rozmieszczone: {placed}/{count}");
    }

    public void RenderMap()
    {
        Debug.Log("RenderMap start");
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
                if (sr == null)
                {
                    Debug.LogError("Tile prefab nie ma SpriteRenderera!");
                    continue;
                }
                switch (map[x, y].Type)
                {
                    case TileType.Obstacle: sr.color = new Color(0.5f, 0.5f, 0.5f); break;
                    case TileType.Resource: sr.color = new Color(0f, 1f, 0f); break;
                    case TileType.Empty: sr.color = new Color(0f, 0f, 0f); break;
                }
            }
        }
        Debug.Log("RenderMap done");
    }

    public void CancelGeneration()
    {
        cts?.Cancel();
    }

    public void StartGeneration(
        UIManager ui,
        float obstaclePercent,
        float resourcePercent
        )
    {
        cts = new CancellationTokenSource();
        _ = GenerateMapAsync(width, height, obstaclePercent, resourcePercent, ui.UpdateProgress, ui.LogMessage, cts.Token);
    }

    public Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = map[x, y].Type switch
                {
                    TileType.Obstacle => Color.gray,
                    TileType.Resource => Color.green,
                    TileType.Empty => Color.black,
                    _ => Color.magenta // Błąd
                };
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    public void SaveTextureAsPNG(Texture2D texture, string path)
    {
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log($"Mapa zapisana jako {path}");
    }

}
