using System;
using System.Collections;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    Vector3 a, m, b;
    float spd;
    Action onDone;

    // call from spawner
    public void Init(Vector3 start, Vector3 center, Vector3 end, float speed, Action finished)
    {
        a = start; m = center; b = end; spd = Mathf.Max(0.01f, speed); onDone = finished;
        transform.position = a;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        yield return Move(a, m);
        yield return Move(m, b);
        onDone?.Invoke();
        Destroy(gameObject);
    }

    IEnumerator Move(Vector3 from, Vector3 to)
    {
        float dist = Vector3.Distance(from, to);
        float dur = dist / spd;
        for (float t = 0f; t < 1f; t += Time.deltaTime / dur)
        {
            transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        transform.position = to;
    }
}