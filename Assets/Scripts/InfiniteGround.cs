using UnityEngine;
using System.Collections.Generic;

public class InfiniteGround : MonoBehaviour
{
    [Header("Ground Generation")]
    public float tileLength = 20f;
    public int tilesAhead = 5;
    public int tilesBehind = 2;
    
    private Transform player;
    private List<GameObject> groundTiles = new List<GameObject>();
    private float nextTileZ = 0f;
    
    void Start()
    {
        player = FindObjectOfType<PlayerController>()?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found for infinite ground system!");
            return;
        }
        
        // Generate initial ground tiles
        for (int i = 0; i < tilesAhead + tilesBehind; i++)
        {
            CreateGroundTile(nextTileZ);
            nextTileZ += tileLength;
        }
        
        Debug.Log("✓ Infinite ground system initialized");
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Generate new tiles ahead of player
        float playerZ = player.position.z;
        float frontTileZ = nextTileZ - tileLength;
        
        if (playerZ > frontTileZ - (tilesAhead * tileLength))
        {
            CreateGroundTile(nextTileZ);
            nextTileZ += tileLength;
        }
        
        // Remove tiles behind player
        RemoveOldTiles(playerZ);
    }
    
    void CreateGroundTile(float zPosition)
    {
        GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tile.name = $"GroundTile_{zPosition}";
        tile.transform.position = new Vector3(0, -0.5f, zPosition + tileLength * 0.5f);
        tile.transform.localScale = new Vector3(6, 1, tileLength);
        tile.tag = "Ground";
        
        // Set layer for ground detection
        tile.layer = 0; // Default layer
        
        // Use Built-in material
        Material groundMat = new Material(Shader.Find("Standard"));
        groundMat.color = new Color(0.8f, 0.7f, 0.6f); // Sandy color
        tile.GetComponent<MeshRenderer>().material = groundMat;
        
        groundTiles.Add(tile);
    }
    
    void RemoveOldTiles(float playerZ)
    {
        for (int i = groundTiles.Count - 1; i >= 0; i--)
        {
            if (groundTiles[i] != null)
            {
                float tileZ = groundTiles[i].transform.position.z;
                if (tileZ < playerZ - (tilesBehind * tileLength))
                {
                    Destroy(groundTiles[i]);
                    groundTiles.RemoveAt(i);
                }
            }
        }
    }
}