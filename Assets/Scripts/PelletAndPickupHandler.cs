using UnityEngine;
using UnityEngine.Tilemaps;

public class PelletAndPickupHandler : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap pelletTilemap;
    
    [Header("Tags")]
    public string powerPillTag = "PowerPill";
    public string cherryTag = "Cherry";
    
    Tweener tweener;
    bool wasLerping;
    GameManager gm;

    void Awake()
    {
        tweener = FindAnyObjectByType<Tweener>();
        gm = FindAnyObjectByType<GameManager>();
    }

    void LateUpdate()
    {
        bool isLerping = tweener && tweener.TweenExists(transform);
        
        if (!isLerping && wasLerping)
        {
            CheckPelletAtPosition();
        }
        
        wasLerping = isLerping;
    }

    void CheckPelletAtPosition()
    {
        if (!pelletTilemap) return;
        
        var cell = pelletTilemap.WorldToCell(transform.position);
        
        if (pelletTilemap.HasTile(cell))
        {
            pelletTilemap.SetTile(cell, null);
            if (gm) gm.AddPellet();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(powerPillTag))
        {
            if (gm) gm.AddPowerPill();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag(cherryTag))
        {
            if (gm) gm.AddCherry();
            Destroy(other.gameObject);
        }
    }
}