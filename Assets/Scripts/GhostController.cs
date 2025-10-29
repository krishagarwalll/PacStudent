using UnityEngine;

public class GhostController : MonoBehaviour
{
    [Header("Ghost Identity")]
    [Tooltip("Set this to 0, 1, 2, or 3 for each ghost")]
    public int ghostIndex = 0;

    // You can add more ghost-specific logic here later
    // For now, this just holds the ghost's index for identification
}