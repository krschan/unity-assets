using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject buttonPause;
    [SerializeField] private GameObject menuPause;

    [Header("Health UI")]
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Audio Settings")]
    [SerializeField] private Slider volumeSlider;

    [Header("Slots UI")]
    [SerializeField] private Button buttonSlot1;
    [SerializeField] private Button buttonSlot2;
    [SerializeField] private TextMeshProUGUI textLevelSlot1;
    [SerializeField] private TextMeshProUGUI textLevelSlot2;

    private static CanvasManager instance;
    public static CanvasManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        InitializePauseMenu();
        LoadVolumeSettings();
        UpdateSlotUI();
        GameManager.Instance.OnGameDataUpdated += UpdateSlotUI;
        Invoke("UpdateSlotUI", 0.5f); // Ensure UI updates after loading
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameDataUpdated -= UpdateSlotUI;
        }
    }

    // Initializes the pause menu state
    private void InitializePauseMenu()
    {
        Time.timeScale = 1f;

        if (buttonPause != null)
        {
            buttonPause.SetActive(true);
        }

        if (menuPause != null)
        {
            menuPause.SetActive(false);
        }
    }

    // Updates the health display
    public void UpdateHealthUI(int currentHealth)
    {
        if (healthText != null)
        {
            healthText.SetText(currentHealth.ToString());
        }
    }

    // Handles pausing the game
    public void ClickPause()
    {
        Time.timeScale = 0f;
        buttonPause.SetActive(false);
        menuPause.SetActive(true);
    }

    // Resumes the game
    public void ClickResume()
    {
        Time.timeScale = 1f;
        buttonPause.SetActive(true);
        menuPause.SetActive(false);
    }

    // Restarts the current level
    public void ClickRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Loads saved volume settings
    private void LoadVolumeSettings()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        }
    }

    // Saves volume settings
    public void SetVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    // Exits the level and returns to the main menu
    public void ExitLevel()
    {
        SceneManager.LoadScene("MenuLobby");
        Time.timeScale = 1f;
    }

    // Saves game progress and exits to the main menu
    public void ClickSaveGameAndExitGame()
    {
        float gameDuration = Time.time - GameManager.Instance.GetGameStartTime();
        int playtime = Mathf.FloorToInt(gameDuration);
        
        // Guardar TODO en un solo método
        GameManager.Instance.SaveAllProgress(playtime);
        SceneManager.LoadScene("MenuLobby");
        Time.timeScale = 1f;
    }

    // Updates the UI elements related to save slots
    public void UpdateSlotUI()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance es null en UpdateSlotUI()");
            return;
        }

        if (textLevelSlot1 != null)
        {
            textLevelSlot1.text = GameManager.Instance.GetSlotLevel(1).ToString();
        }

        if (textLevelSlot2 != null)
        {
            textLevelSlot2.text = GameManager.Instance.GetSlotLevel(2).ToString();
        }
    }

    public void OnSlot1Clicked()
    {
        GameManager.Instance.SelectSlot(1);
    }

    public void OnSlot2Clicked()
    {
        GameManager.Instance.SelectSlot(2);
    }
} 
