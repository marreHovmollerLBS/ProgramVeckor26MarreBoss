using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    public Vector2 Direction { get; private set; }
    public float Speed { get; set; }
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
        if (Input.GetKey(KeyCode.UpArrow))
        {
            temp.y += 1; 
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            temp.y -= 1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            temp.x += 1;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
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
