using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitScript : MonoBehaviour
{
    public bool SwitchActivated;
    public bool SwitchAllowed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SwitchActivated = false;
        SwitchAllowed = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            SwitchActivated = !SwitchActivated;
            Debug.Log($"SwitchActivated is {SwitchActivated}");
        }
        if (SwitchActivated && SwitchAllowed)
        {
            SceneManager.LoadScene("Bedroom");
        }
    }
}
