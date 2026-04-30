using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TileManager : MonoBehaviour
{
    [Header("Setup")]
    public Transform tileGridRoot;
    public Camera mainCamera;

    [Header("Resolution Settings")]
    public int lowResValue = 128;
    // We keep this for logic, but we will send "max" to the server for HighRes
    public int highResValue = 1024;
    [Range(0.1f, 20f)]
    public float highResDistance = 2.0f;

    [Header("Materials")]
    public Material tileBaseMaterial;

    [Header("IIIF Settings")]
    public string baseUrl = "https://iiif.micr.io/PJEZO";

    private Tile[,] tiles;
    private const int gridRows = 16;
    private const int gridCols = 16;

    private int[] currentLoadedRes;
    private float[] lastFailTime;

    private int activeHighResDownloads = 0;
    private const int MAX_CONCURRENT_HIGHRES = 4;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        currentLoadedRes = new int[gridRows * gridCols];
        lastFailTime = new float[gridRows * gridCols];

        InitializeGridByNaming();
        StartCoroutine(InitialLowResLoad());
        StartCoroutine(UpdateTilesRoutine());

        Debug.Log($"<color=cyan>[TileManager]</color> Initialized. Grid: {gridRows}x{gridCols}.");
    }

    void InitializeGridByNaming()
    {
        tiles = new Tile[gridRows, gridCols];
        int foundCount = 0;

        for (int r = 1; r <= gridRows; r++)
        {
            for (int c = 1; c <= gridCols; c++)
            {
                // Matches naming convention Quad_01_01, Quad_01_02, etc.
                string targetName = $"Quad_{r:D2}_{c:D2}";
                Transform quadTransform = tileGridRoot.Find(targetName);

                if (quadTransform != null)
                {
                    Tile tile = quadTransform.GetComponent<Tile>() ?? quadTransform.gameObject.AddComponent<Tile>();
                    tile.row = r - 1;
                    tile.col = c - 1;

                    // Assign a unique material instance so we don't overwrite other tiles
                    MeshRenderer mr = quadTransform.GetComponent<MeshRenderer>();
                    if (mr != null) mr.material = new Material(tileBaseMaterial);

                    tiles[r - 1, c - 1] = tile;
                    foundCount++;
                }
            }
        }
        Debug.Log($"<color=cyan>[TileManager]</color> Found {foundCount} / {gridRows * gridCols} quads.");
    }

    IEnumerator InitialLowResLoad()
    {
        Debug.Log("<color=white>[TileManager]</color> Starting Initial Low-Res Load...");
        foreach (Tile tile in tiles)
        {
            if (tile != null) StartCoroutine(LoadTile(tile, lowResValue, false));
        }
        yield return null;
    }

    IEnumerator UpdateTilesRoutine()
    {
        while (true)
        {
            Vector3 camPos = mainCamera.transform.position;

            foreach (Tile tile in tiles)
            {
                if (tile == null) continue;

                float dist = Vector3.Distance(camPos, tile.transform.position);
                int index = tile.row * gridCols + tile.col;

                // Frustum Culling: Only update if the tile is actually on screen
                Vector3 screenPoint = mainCamera.WorldToViewportPoint(tile.transform.position);
                bool inView = screenPoint.z > 0 && screenPoint.x > -0.1f && screenPoint.x < 1.1f && screenPoint.y > -0.1f && screenPoint.y < 1.1f;

                if (dist <= highResDistance && inView)
                {
                    // Trigger upgrade if: 
                    // 1. Not already high-res
                    // 2. We have download slots open
                    // 3. It hasn't failed in the last 5 seconds
                    if (currentLoadedRes[index] < highResValue &&
                        activeHighResDownloads < MAX_CONCURRENT_HIGHRES &&
                        Time.time > lastFailTime[index] + 5f)
                    {
                        StartCoroutine(LoadTile(tile, highResValue, true));
                    }
                }
                // Downgrade if we move away
                else if (dist > (highResDistance + 1.0f) && currentLoadedRes[index] == highResValue)
                {
                    StartCoroutine(LoadTile(tile, lowResValue, false));
                }
            }
            // Check 4 times per second
            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator LoadTile(Tile tile, int resolution, bool isHighResRequest)
    {
        if (isHighResRequest)
        {
            activeHighResDownloads++;
        }

        int index = tile.row * gridCols + tile.col;
        int previousRes = currentLoadedRes[index];
        currentLoadedRes[index] = resolution;

        float pw = 100f / gridCols;
        float ph = 100f / gridRows;
        float px = tile.col * pw;
        float py = tile.row * ph;

        // --- THE OVERLAP HACK ---
        // We request a 0.05% larger area than the quad actually needs.
        // This provides "gutter" pixels so the GPU doesn't sample empty space at the edges.
        float buffer = 0.05f;
        string region = $"pct:{px:F2},{py:F2},{pw + buffer:F2},{ph + buffer:F2}";

        string resString = isHighResRequest ? "max" : $"{resolution},";
        string url = $"{baseUrl}/{region}/{resString}/0/default.jpg";

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            uwr.SetRequestHeader("User-Agent", "Mozilla/5.0");
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                // We get the texture from the download handler
                Texture2D newTex = DownloadHandlerTexture.GetContent(uwr);

                // --- EDGE CLAMPING & FILTERING ---
                // Clamp prevents the texture from trying to "wrap" pixels from the opposite side
                newTex.wrapMode = TextureWrapMode.Clamp;

                // Bilinear is often safer for tiled seams than Trilinear
                newTex.filterMode = FilterMode.Bilinear;

                // MipMap Bias can help if seams only appear when moving away. 
                // -0.5 makes it stay "sharper" longer.
                newTex.mipMapBias = -0.5f;
                newTex.Apply();

                MeshRenderer mr = tile.GetComponent<MeshRenderer>();
                Texture oldTex = mr.material.mainTexture;

                tile.ApplyTexture(newTex);

                if (oldTex != null && oldTex != newTex) Destroy(oldTex);
            }
            else
            {
                lastFailTime[index] = Time.time;
                currentLoadedRes[index] = previousRes;
                Debug.LogWarning($"<color=orange>[Server Reject]</color> {tile.name} failed. URL: {url}");
            }
        }

        if (isHighResRequest) activeHighResDownloads--;
    }
}