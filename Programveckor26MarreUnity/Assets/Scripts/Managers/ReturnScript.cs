using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ReturnScript : MonoBehaviour
{
    public TextMeshProUGUI Text;
    [SerializeField] private KeyCode returnButton = KeyCode.E;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Text.text = $"Press {returnButton} to return to dream";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(returnButton))
        {
            Debug.Log("returning");
            SceneManager.LoadScene("Main Scene");
        }
    }
}
