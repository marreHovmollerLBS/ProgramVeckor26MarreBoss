using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ExitScript : MonoBehaviour
{
    public bool SwitchActivated;
    public bool SwitchAllowed;
    public bool IsGoodDream;
    TextMeshPro Text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SwitchActivated = false;
        SwitchAllowed = true;
        Text = "";
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
