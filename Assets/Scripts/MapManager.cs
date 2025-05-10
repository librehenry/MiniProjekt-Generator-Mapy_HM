using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform tileParent;
    public int width = 10;
    public int height = 10;
    private MapTile[,] map;
    private CancellationTokenSource cts;
    private Color colorObstacle = new Color(0.5f, 0.5f, 0.5f);
    private Color colorResource = new Color(0f, 1f, 0f);
    private Color colorEmpty = new Color(1f, 1f, 1f);
    private Color colorError = new Color(1f, 0f, 1f);




    public Texture2D CurrentTexture { get; private set; }

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
        map = new MapTile[width, height];
        onLog?.Invoke("Inicjalizacja mapy...");

        for (int y = 0; y < height; y++)
        {
            token.ThrowIfCancellationRequested();

            for (int x = 0; x < width; x++)
                map[x, y] = new MapTile();

            onProgress?.Invoke((float)(y + 1) / height * 0.2f); // 20% paska postępu na inicjalizację
            await Task.Yield();
        }

        int totalTiles = width * height;
        int obstacleCount = Mathf.RoundToInt(totalTiles * obstaclePercent);
        int resourceCount = Mathf.RoundToInt(totalTiles * resourcePercent);
        System.Random rand = new();

        onLog?.Invoke("Rozpoczynam rozmieszczanie przeszkód i zasobów...");

        var obstacleTask = PlaceRandomTiles(TileType.Obstacle, obstacleCount, rand, token, onLog, 0.2f, 0.5f, onProgress);
        var resourceTask = PlaceRandomTiles(TileType.Resource, resourceCount, rand, token, onLog, 0.5f, 0.8f, onProgress);

        await Task.WhenAll(obstacleTask, resourceTask);

        onLog?.Invoke("Renderowanie mapy...");
        onProgress?.Invoke(0.9f);
        RenderMap();

        onLog?.Invoke("Generowanie tekstury...");
        CurrentTexture = GenerateTexture();
        onProgress?.Invoke(1f);
        onLog?.Invoke("Generowanie zakończone.");
    }

    private async Task PlaceRandomTiles(
        TileType type,
        int count,
        System.Random rand,
        CancellationToken token,
        Action<string> onLog,
        float progressStart,
        float progressEnd,
        Action<float> onProgress
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

                if (placed % 100 == 0 || placed == count)
                {
                    onLog?.Invoke($"[{type}] Umieszczono {placed}/{count}");
                    float t = progressStart + (progressEnd - progressStart) * (placed / (float)count);
                    onProgress?.Invoke(t);
                }

                await Task.Yield();
            }
        }

        onLog?.Invoke($"Zakończono rozmieszczanie: {type}");
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
                if (sr != null)
                {
                    sr.color = map[x, y].Type switch
                    {
                        TileType.Obstacle => colorObstacle,
                        TileType.Resource => colorResource,
                        TileType.Empty => colorEmpty,
                        _ => colorError
                    };
                }
            }
        }
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
                    TileType.Obstacle => colorObstacle,
                    TileType.Resource => colorResource,
                    TileType.Empty => colorEmpty,
                    _ => colorError
                };
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    public void CancelGeneration()
    {
        cts?.Cancel();
    }

    public void StartGeneration(
        Action<float> onProgress,
        Action<string> onLog,
        float obstaclePercent,
        float resourcePercent,
        Action onCompleted
    )
    {
        cts = new CancellationTokenSource();
        _ = GenerateMapAsync(width, height, obstaclePercent, resourcePercent, onProgress, onLog, cts.Token)
            .ContinueWith(task =>
            {
                if (task.IsCanceled)
                    onLog?.Invoke("Generowanie anulowane.");
                else if (task.Exception != null)
                    onLog?.Invoke($"Błąd: {task.Exception.InnerException?.Message}");
                else
                    onCompleted?.Invoke();
            }, TaskScheduler.FromCurrentSynchronizationContext());
    }
}
