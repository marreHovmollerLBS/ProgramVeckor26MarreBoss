using UnityEngine;

public class CursorController : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTextureDefault;

    [SerializeField] private Vector2 clickPosition = Vector2.zero;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.SetCursor(cursorTextureDefault, clickPosition, CursorMode.Auto);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
