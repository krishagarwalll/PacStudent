using System;
using System.Collections.Generic;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    List<Tween> queue = new List<Tween>();

    public bool TweenExists(Transform target)
    {
        for (int i = 0; i < queue.Count; i++)
            if (queue[i].Target == target) return true;
        return false;
    }

    public bool AddTween(Transform target, Vector3 startPos, Vector3 endPos, float duration)
    {
        return AddTween(target, startPos, endPos, duration, null);
    }

    public bool AddTween(Transform target, Vector3 startPos, Vector3 endPos, float duration, Action onComplete)
    {
        if (TweenExists(target)) return false;
        queue.Add(new Tween(target, startPos, endPos, Time.time, duration, onComplete));
        return true;
    }

    void Update()
    {
        for (int i = queue.Count - 1; i >= 0; i--)
        {
            var twn = queue[i];
            float u = Mathf.Clamp01((Time.time - twn.StartTime) / twn.Duration);
            twn.Target.position = Vector3.Lerp(twn.StartPos, twn.EndPos, u);

            if ((twn.Target.position - twn.EndPos).sqrMagnitude <= 0.000001f)
            {
                twn.Target.position = twn.EndPos;
                twn.OnComplete?.Invoke();
                queue.RemoveAt(i);
            }
        }
    }
}