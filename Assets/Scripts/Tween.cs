using System;
using UnityEngine;

public class Tween
{
    public Transform Target { get; private set; }
    public Vector3 StartPos { get; private set; }
    public Vector3 EndPos   { get; private set; }
    public float StartTime  { get; private set; }
    public float Duration   { get; private set; }
    public Action OnComplete { get; private set; }

    public Tween(Transform target, Vector3 startPos, Vector3 endPos, float startTime, float duration, Action onComplete = null)
    {
        Target = target;
        StartPos = startPos;
        EndPos = endPos;
        StartTime = startTime;
        Duration = duration;
        OnComplete = onComplete;
    }
}