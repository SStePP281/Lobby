using UnityEngine;

public class SpringMovementEffect : BaseSpringEffect
{
    [SerializeField] private Vector3 moveDirection = Vector3.right;
    [SerializeField] private float moveStrength = 300.0f;

    private Vector3 defaultPosition;

    private void Awake()
    {
        defaultPosition = transform.localPosition;
    }

    public override void OnSpringUpdate(float springValue, float springVelocity)
    {
        float finalSpringValue = springValue * moveStrength;
        transform.localPosition = defaultPosition + moveDirection * finalSpringValue;
    }
}
