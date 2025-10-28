using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SpriteRenderer))]
public class PacStudentController : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] Tilemap walkableTilemap;
    [SerializeField] Tilemap pelletTilemap;
    [SerializeField] Tilemap wallTilemap;
    [SerializeField] Tilemap ghostWallTilemap;

    [Header("Movement")]
    [SerializeField] float tilesPerSecond = 8f;
    [SerializeField] Tweener tweener;

    [Header("Animation")]
    [SerializeField] Animator animator;          // expects: IsMoving(bool), Dir(int 0=R,1=D,2=L,3=U)

    AudioManager audioManager;
    Vector3Int gridPos, lastInput, currentInput;
    Vector3Int Up = new(0, 1, 0), Down = new(0, -1, 0), Left = new(-1, 0, 0), Right = new(1, 0, 0);
    Vector3Int lastBumpCell = new(int.MinValue, int.MinValue, 0);
    float nextBumpTime, bumpCooldown = 0.05f;
    bool IsLerping => tweener && tweener.TweenExists(transform);

    void Awake()
    {
                    audioManager = Object.FindFirstObjectByType<AudioManager>();   // change to direct reference if you prefer
    }

    void Start()
    {
        gridPos = walkableTilemap.WorldToCell(transform.position);
        transform.position = walkableTilemap.GetCellCenterWorld(gridPos);
        transform.rotation = Quaternion.identity;
        lastInput = currentInput = Vector3Int.zero;
        SetAnimDir(Right); SetAnimMoving(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) lastInput = Up;
        else if (Input.GetKeyDown(KeyCode.S)) lastInput = Down;
        else if (Input.GetKeyDown(KeyCode.A)) lastInput = Left;
        else if (Input.GetKeyDown(KeyCode.D)) lastInput = Right;

        if (IsLerping) return;

        if (TryStartMove(lastInput)) return;
        if (TryStartMove(currentInput)) return;

        currentInput = Vector3Int.zero;
        if (audioManager) audioManager.StopMoveLoop();
        SetAnimMoving(false);
    }

    void OnDisable() { if (audioManager) audioManager.StopMoveLoop(); SetAnimMoving(false); }

    bool TryStartMove(Vector3Int dir)
    {
        if (dir == Vector3Int.zero) return false;
        var nextCell = gridPos + dir;

        if (!Walkable(nextCell))
        {
            if (Time.time >= nextBumpTime && nextCell != lastBumpCell)
            {
                audioManager?.PlayWallHit();
                lastBumpCell = nextCell; nextBumpTime = Time.time + bumpCooldown;
            }
            return false;
        }

        currentInput = dir;
        Vector3 a = walkableTilemap.GetCellCenterWorld(gridPos);
        Vector3 b = walkableTilemap.GetCellCenterWorld(nextCell);
        float dur = Vector3.Distance(a, b) / Mathf.Max(0.0001f, tilesPerSecond);

        if (tweener.AddTween(transform, a, b, dur))
        {
            gridPos = nextCell;
            bool aboutToEat = pelletTilemap && pelletTilemap.HasTile(nextCell);
            audioManager?.StartMoveLoop(aboutToEat);
            SetAnimDir(currentInput); SetAnimMoving(true);
            return true;
        }
        return false;
    }

    bool Walkable(Vector3Int cell)
    {
        if (wallTilemap && wallTilemap.HasTile(cell)) return false;
        if (ghostWallTilemap && ghostWallTilemap.HasTile(cell)) return false; // treat ghost wall as solid from outside
        return true;
    }

    void SetAnimMoving(bool moving)
    {
        if (!animator) return;
        animator.SetBool("IsMoving", moving);
        animator.speed = moving ? 1f : 0f;   // freeze when idle
    }

    void SetAnimDir(Vector3Int dir)
    {
        if (!animator) return;
        int d = (dir.x > 0) ? 0 : (dir.y < 0) ? 1 : (dir.x < 0) ? 2 : 3;
        animator.SetInteger("Dir", d);
    }

    // call this when implementing death later
    public void PlayDeath()
    {
        audioManager?.PlayDeath();
        if (animator) { animator.speed = 1f; animator.SetTrigger("Death"); }
    }
}
