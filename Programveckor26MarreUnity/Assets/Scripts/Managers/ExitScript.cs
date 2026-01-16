using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ExitScript : MonoBehaviour
{
    public bool IsGoodDream;
    public bool IsGameActive;
    public TextMeshProUGUI Text;
    public TextMeshProUGUI SkipRoundText;

    [SerializeField] private KeyCode exitButton = KeyCode.E;
    [SerializeField] private KeyCode skipRoundButton = KeyCode.N;
    [SerializeField] private GameManager gameManager;

    public bool SwitchActivated;
    private bool noEnemiesInScene = false;

    void Start()
    {
        SwitchActivated = false;
        Text.text = "";
        if (SkipRoundText != null)
        {
            SkipRoundText.text = "";
        }
    }

    void Update()
    {
        // Hide all text during countdown
        if (!IsGameActive)
        {
            Text.text = "";
            if (SkipRoundText != null)
            {
                SkipRoundText.text = "";
            }
            return;
        }

        // Check if there are any enemies in the scene
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        noEnemiesInScene = (enemies.Length == 0);

        // Handle good dream UI
        if (IsGoodDream)
        {
            string onOrOff = SwitchActivated ? "On" : "Off";
            Text.text = $"E: Toggle Exit ({onOrOff})";

            if (Input.GetKeyDown(KeyCode.E))
            {
                SwitchActivated = !SwitchActivated;
                Debug.Log($"SwitchActivated is {SwitchActivated}");
            }
        }
        else
        {
            Text.text = "";
            SwitchActivated = false;
        }

        // Handle skip round prompt
        if (noEnemiesInScene && SkipRoundText != null)
        {
            SkipRoundText.text = $"Press {skipRoundButton} to skip round";

            if (Input.GetKeyDown(skipRoundButton))
            {
                SkipRound();
            }
        }
        else if (SkipRoundText != null)
        {
            SkipRoundText.text = "";
        }
    }

    public void SkipRound()
    {
        if (gameManager != null)
        {
            gameManager.currentTime = 99999f;
            Debug.Log("Round skipped!");
        }
        else
        {
            Debug.LogError("GameManager reference is missing in ExitScript!");
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