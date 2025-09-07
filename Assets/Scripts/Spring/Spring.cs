using System;
using System.Collections;
using UnityEngine;

public enum SpringState
{
    Negative = -1,
    Normal = 0,
    Positive = 1
}

public class Spring : MonoBehaviour
{
    [Header("Take")]
    [SerializeField] private BaseSpringEffect[] springEffects;

    [Header("Spring")]
    [SerializeField] private float springStrength = 15.0f;
    [SerializeField] private float springDamper = 0.3f;
    [SerializeField] private float overVelocity = 2.0f;

    [Header("Start")]
    [SerializeField] private SpringState startState = SpringState.Normal;

    private Coroutine springCorotine;

    private ReactiveVariable<float> currentValue;
    private float targetValue;

    private DampedSpringMotionParams springParam;
    private float springVelocity;

    private void Awake()
    {
        currentValue = new ReactiveVariable<float>();
        currentValue.OnValueChanged += CurrentValue_OnValueChanged;
    }

    private void Start()
    {
        SetNewState(startState);
    }

    public void SetNewState(SpringState newState)
    {
        int newValue = (int)newState;

        if (newValue != targetValue)
        {
            targetValue = newValue;

            if (springCorotine == null)
            {
                springCorotine = StartCoroutine(StartSpring());
            }
        }
        else
        {
            if (newValue == currentValue.Value)
            {
                springVelocity = overVelocity;

                if (springCorotine == null)
                {
                    springCorotine = StartCoroutine(StartSpring());
                }
            }
        }
    }

    private void CurrentValue_OnValueChanged(float priviouse, float newValue)
    {
        foreach (var effect in springEffects)
        {
            effect.OnSpringUpdate(newValue, springVelocity);
        }
    }

    private IEnumerator StartSpring()
    {
        while (springVelocity != 0.0f || targetValue != currentValue.Value)
        {
            MathSpring.CalculateSpringParams(ref springParam, Time.deltaTime, springStrength, springDamper);

            float curVal = currentValue.Value;
            MathSpring.UpdateSpring(ref curVal, ref springVelocity, targetValue, ref springParam);
            currentValue.Value = curVal;

            yield return null;
        }
    }
}
