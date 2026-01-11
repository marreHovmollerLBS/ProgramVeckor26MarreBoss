using UnityEngine;

public class TheodorPlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    public Vector2 Direction { get; private set; }
    public float Speed;
    bool MovementAllowed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Direction = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        SetDirection();
        ApplyMovement();
    }
    private void SetDirection()
    {
        Vector2 temp = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            temp.y += 1;
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            temp.y -= 1;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            temp.x += 1;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            temp.x -= 1;
        }
        temp.Normalize();
        Direction = temp;
    }
    private void ApplyMovement()
    {
        rb.linearVelocity = Direction * Speed;
    }
}
