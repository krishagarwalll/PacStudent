using UnityEngine;

public class PacStudentMovement : MonoBehaviour
{
    public Tweener tweener;
    public Transform[] waypoints;
    public float unitsPerSecond = 4f;
    public AudioManager audioManager;

    int index;

    void Start()
    {
        if (waypoints != null && waypoints.Length >= 2)
        {
            transform.position = waypoints[0].position;
            index = 0;
        }
    }

    void Update()
    {
        if (audioManager != null) audioManager.StartMoveLoop(false);
        if (tweener == null || waypoints == null || waypoints.Length < 2) return;
        if (!tweener.TweenExists(transform))
        {
            int next = (index + 1) % waypoints.Length;
            Vector3 a = waypoints[index].position;
            Vector3 b = waypoints[next].position;
            float dist = Vector3.Distance(a, b);
            float dur = dist / Mathf.Max(0.0001f, unitsPerSecond);
            tweener.AddTween(transform, a, b, dur);
            index = next;
        }
    }

    void OnDisable()
    {
        if (audioManager != null) audioManager.StopMoveLoop();
    }

    void OnDestroy()
    {
        if (audioManager != null) audioManager.StopMoveLoop();
    }
}