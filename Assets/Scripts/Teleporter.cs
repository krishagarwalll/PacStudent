using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Teleporter : MonoBehaviour
{
    public Transform exit;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!exit) return;
        var ps = other.GetComponent<PacStudentController>();
        if (!ps) return;
        if (!ps.CanTeleport()) return; // cooldown
        ps.TeleportTo(exit.position);
    }
}