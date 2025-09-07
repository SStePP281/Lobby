using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10.0f;

    private Rigidbody rb;
    private Vector2 movementInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        movementInput = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) { movementInput.y = 1.0f; }
        if (Input.GetKey(KeyCode.S)) { movementInput.y = -1.0f; }
        if (Input.GetKey(KeyCode.A)) { movementInput.x = -1.0f; }
        if (Input.GetKey(KeyCode.D)) { movementInput.x = 1.0f; }

        movementInput.Normalize();
    }

    private void FixedUpdate()
    {
        Vector3 movementDirection = Vector3.forward * movementInput.y + Vector3.right * movementInput.x;
        rb.velocity = movementDirection * moveSpeed;
    }
}
