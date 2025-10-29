using UnityEngine;
using UnityEngine.Tilemaps;

public class PelletAndPickupHandler : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap walkableTilemap; // Background_Tilemap
    public Tilemap pelletTilemap;   // Pellet_Tilemap
    
    [Header("Game")]
    public GameManager gameManager;
    
    Vector3Int lastWalkableCell = new Vector3Int(int.MinValue, int.MinValue, 0);
    
    void Reset()
    {
        if (!walkableTilemap) walkableTilemap = GameObject.Find("Background_Tilemap")?.GetComponent<Tilemap>();
        if (!pelletTilemap)   pelletTilemap   = GameObject.Find("Pellet_Tilemap")?.GetComponent<Tilemap>();
#if UNITY_2023_1_OR_NEWER
        if (!gameManager) gameManager = FindFirstObjectByType<GameManager>();
#else
        if (!gameManager) gameManager = FindObjectOfType<GameManager>();
#endif
    }
    
    void Update()
    {
        if (!walkableTilemap || !pelletTilemap) return;
        
        var nowCell = walkableTilemap.WorldToCell(transform.position);
        if (nowCell == lastWalkableCell) return; 
        lastWalkableCell = nowCell;

        Vector3 worldCenter   = walkableTilemap.GetCellCenterWorld(nowCell);
        Vector3Int pelletCell = pelletTilemap.WorldToCell(worldCenter);

        if (pelletTilemap.HasTile(pelletCell))
        {
            pelletTilemap.SetTile(pelletCell, null);
            gameManager?.AddPellet();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PowerPill"))
        {
            gameManager?.AddPowerPill();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Cherry"))
        {
            gameManager?.AddCherry();
            Destroy(other.gameObject);
        }
    }
}