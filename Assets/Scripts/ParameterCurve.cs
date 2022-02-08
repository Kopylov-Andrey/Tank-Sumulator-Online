using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParameterCurve
{
    [SerializeField] private AnimationCurve curve;

    [SerializeField] private float duration = 1;

    private float expiredTime;


    public float MoveTowards(float deltaTime)
    {
        expiredTime += deltaTime;
        return curve.Evaluate(expiredTime/duration);
    }

    public float Reset()
    {
        expiredTime = 0;

        return curve.Evaluate(0);
    }
}
