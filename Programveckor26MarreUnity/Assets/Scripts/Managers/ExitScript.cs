using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ExitScript : MonoBehaviour
{
    public bool IsGoodDream;
    public TextMeshProUGUI Text;

    private bool SwitchActivated;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SwitchActivated = false;
        Text.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGoodDream)
        {
            string onOrOff = SwitchActivated ? "On" : "Off";
            Text.text = $"E: Toggle Exit ({onOrOff})";

            if (Input.GetKey(KeyCode.E))
            {
                SwitchActivated = !SwitchActivated;
                Debug.Log($"SwitchActivated is {SwitchActivated}");
            }
        }
        else
        {
            Text.text = ("");
            SwitchActivated = false;
        }
    }
    public void OnSwitchToBadDream()
    {
        if (SwitchActivated)
        {
            SceneManager.LoadScene("Bedroom");
        }
    }
}
