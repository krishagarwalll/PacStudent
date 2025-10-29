using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostController : MonoBehaviour
{
    [Header("Ghost Identity")]
    public int ghostIndex = 0;
    
    [Header("Movement")]
    public float normalSpeed = 3.6f;
    public Tilemap walkableTilemap;
    public Tilemap wallTilemap;
    public Tilemap ghostWallTilemap;
    
    [Header("References")]
    public Transform player;
    public Animator animator;
    
    Vector3Int gridPos;
    Vector3Int lastDir;
    bool isLerping = false;
    float currentSpeed;
    int currentState;
    bool movementEnabled = false;
    
    readonly Vector3Int Up = new(0, 1, 0);
    readonly Vector3Int Down = new(0, -1, 0);
    readonly Vector3Int Left = new(-1, 0, 0);
    readonly Vector3Int Right = new(1, 0, 0);

    void Start()
    {
        if (!walkableTilemap) walkableTilemap = GameObject.Find("Walkable")?.GetComponent<Tilemap>();
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (!animator) animator = GetComponent<Animator>();
        
        gridPos = walkableTilemap.WorldToCell(transform.position);
        transform.position = walkableTilemap.GetCellCenterWorld(gridPos);
        lastDir = Vector3Int.zero;
        currentSpeed = normalSpeed;
        currentState = 0;
    }

    void Update()
    {
        if (!movementEnabled) return;
        if (isLerping) return;
        
        Vector3Int nextDir = DecideNextMove();
        
        if (nextDir != Vector3Int.zero)
        {
            Vector3Int nextCell = gridPos + nextDir;
            // dead ghosts ignore walls, alive ghosts respect them
            if (currentState == 3 || IsWalkable(nextCell))
            {
                StartCoroutine(LerpToCell(nextCell, nextDir));
            }
        }
    }

    Vector3Int DecideNextMove()
    {
        if (currentState == 3)
        {
            Vector3 spawnPos = new Vector3(13.5f, 11f, 0f);
            Vector3 toSpawn = spawnPos - transform.position;
            
            if (Mathf.Abs(toSpawn.x) > Mathf.Abs(toSpawn.y))
                return toSpawn.x > 0 ? Right : Left;
            else
                return toSpawn.y > 0 ? Up : Down;
        }
        
        var validDirs = GetValidDirections();
        if (validDirs.Count == 0) return lastDir * -1;
        
        if (currentState == 1 || currentState == 2)
        {
            return PickAwayFromPlayer(validDirs);
        }
        
        switch (ghostIndex)
        {
            case 0:
                return PickAwayFromPlayer(validDirs);
            case 1:
                return PickTowardPlayer(validDirs);
            case 2:
                return validDirs[Random.Range(0, validDirs.Count)];
            case 3:
                return PickClockwise(validDirs);
            default:
                return validDirs[Random.Range(0, validDirs.Count)];
        }
    }

    System.Collections.Generic.List<Vector3Int> GetValidDirections()
    {
        var dirs = new System.Collections.Generic.List<Vector3Int> { Up, Down, Left, Right };
        var valid = new System.Collections.Generic.List<Vector3Int>();
        
        foreach (var dir in dirs)
        {
            if (dir == lastDir * -1) continue;
            
            Vector3Int nextCell = gridPos + dir;
            if (IsWalkable(nextCell))
            {
                valid.Add(dir);
            }
        }
        
        return valid;
    }

    bool IsWalkable(Vector3Int cell)
    {
        if (!walkableTilemap) return false;
        
        if (HasTile(wallTilemap, cell)) return false;
        
        
        return true;
    }

    bool HasTile(Tilemap map, Vector3Int cellOnWalkable)
    {
        if (!map || !walkableTilemap) return false;
        Vector3 world = walkableTilemap.GetCellCenterWorld(cellOnWalkable);
        Vector3Int cellOnMap = map.WorldToCell(world);
        return map.HasTile(cellOnMap);
    }

    Vector3Int PickAwayFromPlayer(System.Collections.Generic.List<Vector3Int> validDirs)
    {
        if (!player) return validDirs[Random.Range(0, validDirs.Count)];
        
        float currentDist = Vector3.Distance(transform.position, player.position);
        var goodDirs = new System.Collections.Generic.List<Vector3Int>();
        
        foreach (var dir in validDirs)
        {
            Vector3 nextPos = walkableTilemap.GetCellCenterWorld(gridPos + dir);
            float nextDist = Vector3.Distance(nextPos, player.position);
            
            if (nextDist >= currentDist)
            {
                goodDirs.Add(dir);
            }
        }
        
        if (goodDirs.Count == 0) return validDirs[Random.Range(0, validDirs.Count)];
        return goodDirs[Random.Range(0, goodDirs.Count)];
    }

    Vector3Int PickTowardPlayer(System.Collections.Generic.List<Vector3Int> validDirs)
    {
        if (!player) return validDirs[Random.Range(0, validDirs.Count)];
        
        float currentDist = Vector3.Distance(transform.position, player.position);
        var goodDirs = new System.Collections.Generic.List<Vector3Int>();
        
        foreach (var dir in validDirs)
        {
            Vector3 nextPos = walkableTilemap.GetCellCenterWorld(gridPos + dir);
            float nextDist = Vector3.Distance(nextPos, player.position);
            
            if (nextDist <= currentDist)
            {
                goodDirs.Add(dir);
            }
        }
        
        if (goodDirs.Count == 0) return validDirs[Random.Range(0, validDirs.Count)];
        return goodDirs[Random.Range(0, goodDirs.Count)];
    }

    Vector3Int PickClockwise(System.Collections.Generic.List<Vector3Int> validDirs)
    {
        // prefer right > down > left > up for clockwise movement
        if (validDirs.Contains(Right)) return Right;
        if (validDirs.Contains(Down)) return Down;
        if (validDirs.Contains(Left)) return Left;
        if (validDirs.Contains(Up)) return Up;
        return validDirs[0];
    }

    System.Collections.IEnumerator LerpToCell(Vector3Int nextCell, Vector3Int dir)
    {
        isLerping = true;
        lastDir = dir;
        
        Vector3 start = walkableTilemap.GetCellCenterWorld(gridPos);
        Vector3 end = walkableTilemap.GetCellCenterWorld(nextCell);
        
        float distance = Vector3.Distance(start, end);
        float duration = distance / currentSpeed;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        
        transform.position = end;
        gridPos = nextCell;
        isLerping = false;
    }

    public void SetState(int newState)
    {
        currentState = newState;
        
        if (currentState == 0)
            currentSpeed = normalSpeed;
        else
            currentSpeed = normalSpeed * 0.5f;
    }

    public void EnableMovement()
    {
        movementEnabled = true;
    }

    public void DisableMovement()
    {
        movementEnabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        // scared or recovering
        if (currentState == 1 || currentState == 2)
        {
            if (GameManager.I) GameManager.I.GhostEaten(ghostIndex);
        }
        // normal state
        else if (currentState == 0)
        {
            if (GameManager.I) GameManager.I.PlayerDied();
        }
        // state 3 (dead)
    }
}