using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PacStudentController : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] Tilemap walkableTilemap;
    [SerializeField] Tilemap pelletTilemap;
    [SerializeField] Tilemap wallTilemap;
    [SerializeField] Tilemap ghostWallTilemap;

    [Header("Movement")]
    [SerializeField] float tilesPerSecond = 3f;
    [SerializeField] Tweener tweener;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] string idleStateName = "Right";

    [Header("FX")]
    [SerializeField] GameObject bumpPuffPrefab;

    AudioManager audioManager;

    Vector3Int gridPos, lastInput, currentInput;
    readonly Vector3Int Up = new(0, 1, 0), Down = new(0, -1, 0), Left = new(-1, 0, 0), Right = new(1, 0, 0);

    float teleportCoolUntil = -999f;
    [SerializeField] float teleportCooldown = 0.2f;

    bool inputEnabled = true;
    bool isDying = false;

    bool IsLerping => tweener && tweener.TweenExists(transform);

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
#if UNITY_2023_1_OR_NEWER
        audioManager = Object.FindFirstObjectByType<AudioManager>();
#else
        audioManager = FindObjectOfType<AudioManager>();
#endif
        var rb = GetComponent<Rigidbody2D>();
        if (rb) { rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0f; }
    }

    void Start()
    {
        gridPos = walkableTilemap.WorldToCell(transform.position);
        transform.position = walkableTilemap.GetCellCenterWorld(gridPos);
        transform.rotation = Quaternion.identity;

        lastInput = currentInput = Vector3Int.zero;
        SetAnimState(Right);
        SetAnimMoving(false);
    }

    void Update()
    {
        if (isDying) return;

        if (inputEnabled)
        {
            if (Input.GetKeyDown(KeyCode.W)) lastInput = Up;
            else if (Input.GetKeyDown(KeyCode.S)) lastInput = Down;
            else if (Input.GetKeyDown(KeyCode.A)) lastInput = Left;
            else if (Input.GetKeyDown(KeyCode.D)) lastInput = Right;
        }

        if (!IsLerping && currentInput != Vector3Int.zero && !Walkable(gridPos + currentInput))
        {
            audioManager?.PlayWallHit();
            SpawnBumpFX(walkableTilemap.GetCellCenterWorld(gridPos));
            currentInput = Vector3Int.zero;
            SetAnimMoving(false);
        }

        if (IsLerping) return;

        if (TryStartMove(lastInput)) return;
        if (TryStartMove(currentInput)) return;

        currentInput = Vector3Int.zero;
        audioManager?.StopMoveLoop();
        SetAnimMoving(false);
    }

    bool TryStartMove(Vector3Int dir)
    {
        if (dir == Vector3Int.zero) return false;

        var nextCell = gridPos + dir;
        if (!Walkable(nextCell)) return false;

        currentInput = dir;

        Vector3 a = walkableTilemap.GetCellCenterWorld(gridPos);
        Vector3 b = walkableTilemap.GetCellCenterWorld(nextCell);
        float dur = Vector3.Distance(a, b) / Mathf.Max(0.0001f, tilesPerSecond);

        if (tweener.AddTween(transform, a, b, dur))
        {
            gridPos = nextCell;

            bool aboutToEat = pelletTilemap && pelletTilemap.HasTile(AlignedCell(pelletTilemap, nextCell));
            audioManager?.StartMoveLoop(aboutToEat);

            SetAnimState(currentInput);
            SetAnimMoving(true);
            return true;
        }
        return false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Cherry"))
        {
            GameManager.I?.AddCherry();
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("Ghost"))
        {
            if (GameManager.I == null) return;

            // Check if ghost is scared/recovering
            if (GameManager.I.IsScaredActive)
            {
                // Try to get the ghost's index
                var ghostCtrl = other.GetComponent<GhostController>();
                if (ghostCtrl != null)
                {
                    GameManager.I.GhostEaten(ghostCtrl.ghostIndex);
                }
            }
            else
            {
                // Ghost kills PacStudent
                GameManager.I.PlayerDied();
            }
        }
    }
    
    public bool CanTeleport() => Time.time >= teleportCoolUntil;

    public void TeleportTo(Vector3 worldPos)
    {
        var cell = walkableTilemap.WorldToCell(worldPos);
        gridPos = cell;
        transform.position = walkableTilemap.GetCellCenterWorld(cell);
        teleportCoolUntil = Time.time + teleportCooldown;
    }

    public void SetInputEnabled(bool enabled) => inputEnabled = enabled;

    public void FaceRightIdle()
    {
        SetAnimState(Right);
        SetAnimMoving(false);
        if (animator && !string.IsNullOrEmpty(idleStateName))
            animator.Play(idleStateName, 0, 0f);
    }

    public void PlayDeath()
    {
        isDying = true;
        audioManager?.StopMoveLoop();

        if (animator)
        {
            animator.enabled = true;
            animator.speed = 1f;
            animator.SetInteger("State", 4); // Death state
        }
    }

    public void EndDeath()
    {
        isDying = false;
        
        if (animator && !string.IsNullOrEmpty(idleStateName))
        {
            animator.Play(idleStateName, 0, 0f);
        }
        
        SetAnimMoving(false);
    }

    bool Walkable(Vector3Int cellOnWalkable)
    {
        if (HasTileAligned(wallTilemap, cellOnWalkable)) return false;
        if (HasTileAligned(ghostWallTilemap, cellOnWalkable)) return false;
        return true;
    }

    bool HasTileAligned(Tilemap map, Vector3Int cellOnWalkable)
    {
        if (!map || !walkableTilemap) return false;
        Vector3 world = walkableTilemap.GetCellCenterWorld(cellOnWalkable);
        Vector3Int cellOnThatMap = map.WorldToCell(world);
        return map.HasTile(cellOnThatMap);
    }

    Vector3Int AlignedCell(Tilemap map, Vector3Int cellOnWalkable)
    {
        Vector3 world = walkableTilemap.GetCellCenterWorld(cellOnWalkable);
        return map.WorldToCell(world);
    }

    void SetAnimMoving(bool moving)
    {
        if (!animator) return;
        if (isDying) return;
        
        animator.SetBool("IsMoving", moving);
        animator.speed = moving ? 1f : 0f;
    }

    void SetAnimState(Vector3Int dir)
    {
        if (!animator) return;
        // State: 0=Right, 1=Down, 2=Left, 3=Up, 4=Death
        int state = (dir.x > 0) ? 0 :
                    (dir.y < 0) ? 1 :
                    (dir.x < 0) ? 2 : 3;
        animator.SetInteger("State", state);
    }

    void SpawnBumpFX(Vector3 at)
    {
        if (!bumpPuffPrefab) return;
        var go = Instantiate(bumpPuffPrefab, at, Quaternion.identity);
        var ps = go.GetComponent<ParticleSystem>();
        if (ps)
        {
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;
            ps.Clear(); ps.Play();
        }
        else Destroy(go, 0.5f);
    }
}