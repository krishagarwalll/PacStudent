using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DustTrailMinimal : MonoBehaviour
{
    [Header("PacStudent Animator (with IsMoving + Dir)")]
    [SerializeField] private Animator pacAnimator;

    [Header("Emission Rates (particles/sec)")]
    [SerializeField] private float movingRate = 32f;
    [SerializeField] private float idleRate = 0f;

    [Header("Look")]
    [SerializeField] private Transform followTarget; // PacStudent transform
    [SerializeField] private float offsetBehind = 0.18f; // spawn slightly behind
    [SerializeField] private float trailVelocity = 0.75f; // particle push opposite direction

    ParticleSystem ps;
    ParticleSystem.EmissionModule emission;
    ParticleSystem.VelocityOverLifetimeModule vol;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        emission = ps.emission;
        vol = ps.velocityOverLifetime;
        
        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;

        vol.enabled = true;
    }

    void LateUpdate()
    {
        if (!pacAnimator) return;

        bool moving = pacAnimator.GetBool("IsMoving");
        int dir = pacAnimator.GetInteger("Dir"); // 0=R,1=D,2=L,3=U

        Vector2 v = Vector2.zero;
        switch (dir)
        {
            case 0: v = Vector2.right;  break; // Right
            case 1: v = Vector2.down;   break; // Down
            case 2: v = Vector2.left;   break; // Left
            default:v = Vector2.up;     break; // Up
        }

        // Position the emitter slightly behind the player
        if (followTarget)
            transform.position = followTarget.position - (Vector3)(v * offsetBehind);

        // Push particles opposite to movement for a trailing effect
        vol.x = -v.x * trailVelocity;
        vol.y = -v.y * trailVelocity;

        // change emission rate by movement state
        emission.rateOverTime = moving ? movingRate : idleRate;
    }
}
