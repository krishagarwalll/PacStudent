using System.Collections;
using UnityEngine;

public class CherrySpawner : MonoBehaviour
{
    public BoxCollider2D levelBounds;
    public GameObject cherryPrefab;
    public SpriteMask levelMask;           // assign if you want "visible only inside" masking
    public float speed = 3f;               // ← change this to make the cherry slower/faster
    public float edgeMargin = 0.5f;
    public float respawnDelay = 5f;
    public int sortingOrder = 999;         // ensure it draws on top

    GameObject current;

    void Start() { StartCoroutine(Loop()); }

    IEnumerator Loop()
    {
        // First spawn: wait 10s (countdown ~5s + delay 5s = spawns 5s after game starts)
        yield return new WaitForSeconds(respawnDelay * 2f);
        
        while (true)
        {
            Spawn();
            while (current != null) yield return null;
            // Subsequent spawns: wait only 5s after destroyed
            yield return new WaitForSeconds(respawnDelay);
        }
    }

    void Spawn()
    {
        var b = levelBounds.bounds;
        var c = (Vector2)b.center;

        int edge = Random.Range(0, 4);
        Vector2 start = edge == 0 ? new Vector2(b.min.x - edgeMargin, Random.Range(b.min.y, b.max.y)) :
                         edge == 1 ? new Vector2(b.max.x + edgeMargin, Random.Range(b.min.y, b.max.y)) :
                         edge == 2 ? new Vector2(Random.Range(b.min.x, b.max.x), b.min.y - edgeMargin) :
                                     new Vector2(Random.Range(b.min.x, b.max.x), b.max.y + edgeMargin);

        Vector2 dirOut = (c - start).normalized * -1f;
        Vector2 end = RayAABB(c, dirOut, b) + dirOut * edgeMargin;

        current = Instantiate(cherryPrefab, start, Quaternion.identity);

        var sr = current.GetComponentInChildren<SpriteRenderer>();
        if (sr)
        {
            sr.sortingOrder = sortingOrder;
            sr.maskInteraction = levelMask ? SpriteMaskInteraction.VisibleInsideMask
                                           : SpriteMaskInteraction.None;
        }

        var cc = current.GetComponent<CherryController>();
        if (!cc) cc = current.AddComponent<CherryController>();
        cc.Init(start, c, end, speed, () => current = null);
    }

    static Vector2 RayAABB(Vector2 o, Vector2 d, Bounds b)
    {
        Vector2 inv = new Vector2(1f / (Mathf.Abs(d.x) < 1e-6f ? 1e-6f : d.x),
                                  1f / (Mathf.Abs(d.y) < 1e-6f ? 1e-6f : d.y));
        Vector2 t1 = (new Vector2(b.min.x, b.min.y) - o) * inv;
        Vector2 t2 = (new Vector2(b.max.x, b.max.y) - o) * inv;
        float tmax = Mathf.Min(Mathf.Max(t1.x, t2.x), Mathf.Max(t1.y, t2.y));
        return o + d * tmax;
    }
}