using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DustTrail : MonoBehaviour
{
    [Header("PacStudent Animator (with IsMoving + State)")]
    [SerializeField] private Animator pacAnimator;

    [Header("Emission Rates (particles/sec)")]
    [SerializeField] private float movingRate = 32f;
    [SerializeField] private float idleRate = 0f;

    [Header("Look")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private float offsetBehind = 0.18f; // spawn behind
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
        int state = pacAnimator.GetInteger("State"); // 0=R, 1=D, 2=L, 3=U, 4=Death

        // Don't emit during death
        if (state == 4)
        {
            emission.rateOverTime = 0f;
            return;
        }

        Vector2 v = Vector2.zero;
        switch (state)
        {
            case 0: v = Vector2.right;  break; // right
            case 1: v = Vector2.down;   break; // down
            case 2: v = Vector2.left;   break; // left
            case 3: v = Vector2.up;     break; // up
        }

        // slightly behind the player
        if (followTarget)
            transform.position = followTarget.position - (Vector3)(v * offsetBehind);

        // Push particles opposite to movement
        vol.x = -v.x * trailVelocity;
        vol.y = -v.y * trailVelocity;
        
        emission.rateOverTime = moving ? movingRate : idleRate;
    }
}