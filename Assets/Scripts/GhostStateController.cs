using UnityEngine;
using System.Collections;

public class GhostStateController : MonoBehaviour
{
    [Tooltip("Assign all 4 ghost Animators.")]
    public Animator[] ghosts;

    [Header("Animator state names (must match your controller)")]
    public string walkingState    = "Walking";
    public string scaredState     = "Scared";
    public string recoveringState = "Recovering";
    public string deadState       = "Dead";

    bool subscribed;

    void OnEnable()
    {
        StartCoroutine(WaitAndSubscribe());
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    IEnumerator WaitAndSubscribe()
    {
        while (GameManager.I == null) yield return null;
        if (subscribed) yield break;

        GameManager.I.OnGhostScaredStart += HandleScared;
        GameManager.I.OnGhostRecovering  += HandleRecovering;
        GameManager.I.OnGhostNormal      += HandleNormal;
        subscribed = true;
    }

    void Unsubscribe()
    {
        if (!subscribed || GameManager.I == null) { subscribed = false; return; }
        GameManager.I.OnGhostScaredStart -= HandleScared;
        GameManager.I.OnGhostRecovering  -= HandleRecovering;
        GameManager.I.OnGhostNormal      -= HandleNormal;
        subscribed = false;
    }

    void HandleScared(float _)  => SetAll(1, scaredState);       // Scared now
    void HandleRecovering()     => SetAll(2, recoveringState);   // last 3s
    void HandleNormal()         => SetAll(0, walkingState);      // back to normal

    void SetAll(int state, string stateName)
    {
        if (ghosts == null) return;
        for (int i = 0; i < ghosts.Length; i++)
        {
            var a = ghosts[i];
            if (!a) continue;
            
            // Don't change state if ghost is dead (State 3)
            if (a.GetInteger("GhostState") == 3) continue;
            
            a.SetInteger("GhostState", state);
            a.CrossFade(stateName, 0f, 0, 0f);
        }
    }

    // Call this when a specific ghost dies
    public void SetGhostDead(int ghostIndex)
    {
        if (ghosts == null || ghostIndex < 0 || ghostIndex >= ghosts.Length) return;
        var a = ghosts[ghostIndex];
        if (!a) return;

        a.SetInteger("GhostState", 3);
        a.CrossFade(deadState, 0f, 0, 0f);
    }

    // Call this to revive a specific ghost to a specific state
    public void ReviveGhost(int ghostIndex, int newState)
    {
        if (ghosts == null || ghostIndex < 0 || ghostIndex >= ghosts.Length) return;
        var a = ghosts[ghostIndex];
        if (!a) return;

        string stateName = newState switch
        {
            0 => walkingState,
            1 => scaredState,
            2 => recoveringState,
            _ => walkingState
        };

        a.SetInteger("GhostState", newState);
        a.CrossFade(stateName, 0f, 0, 0f);
    }

    // Get the current state of a ghost
    public int GetGhostState(int ghostIndex)
    {
        if (ghosts == null || ghostIndex < 0 || ghostIndex >= ghosts.Length) return 0;
        var a = ghosts[ghostIndex];
        if (!a) return 0;
        return a.GetInteger("GhostState");
    }
}