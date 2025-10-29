using UnityEngine;

public class PacDeathFX : MonoBehaviour
{
    [Tooltip("Assign a ParticleSystem prefab (not looping).")]
    public ParticleSystem fxPrefab;

    [Tooltip("Optional extra lifetime if your prefab has no StopAction=Destroy.")]
    public float extraLifetime = 0.25f;

    public void Play()
    {
        if (!fxPrefab) return;

        var fx = Instantiate(fxPrefab, transform.position, Quaternion.identity);

        // If the prefab isn't set to auto-destroy, clean it up safely.
        var main = fx.main;
        bool willAutoDestroy =
            main.stopAction == ParticleSystemStopAction.Destroy ||
            main.stopAction == ParticleSystemStopAction.Callback;

        fx.Clear();
        fx.Play();

        if (!willAutoDestroy)
        {
            float life = main.duration + main.startLifetime.constantMax + extraLifetime;
            Destroy(fx.gameObject, life);
        }
    }
}