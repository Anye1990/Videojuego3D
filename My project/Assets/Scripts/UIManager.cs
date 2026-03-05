using UnityEngine;
using UnityEngine.UI;
using TMPro; // Assuming using TextMeshPro for text
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject characterSelectionPanel;
    public GameObject playerHUDPanel;
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;

    [Header("Player HUD Elements")]
    public Slider healthSlider;
    public Image characterIconImage;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI itemsText;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // No DontDestroyOnLoad here. It's usually better to have UI in each scene 
            // or put it under a root that doesn't destroy. 
            // We'll leave it as a per-scene singleton for simplicity right now.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (characterSelectionPanel != null) characterSelectionPanel.SetActive(false);
        if (playerHUDPanel != null) playerHUDPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void ShowCharacterSelection()
    {
        HideAllPanels();
        if (characterSelectionPanel != null) characterSelectionPanel.SetActive(true);
    }

    public void ShowPlayerHUD()
    {
        HideAllPanels();
        if (playerHUDPanel != null) playerHUDPanel.SetActive(true);
    }

    public void ShowGameOver()
    {
        HideAllPanels();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void ShowLevelComplete()
    {
        HideAllPanels();
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
    }

    #region HUD Updates
    public void SetupHUD(Sprite charIcon, int maxHealth)
    {
        if (characterIconImage != null) characterIconImage.sprite = charIcon;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
        UpdateItems(0);
        UpdateAmmo(0, 0); // Temporary 0, depends on implementation
    }

    public void UpdateHealth(int currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    public void UpdateAmmo(int currentAmmo, int maxAmmo)
    {
        if (ammoText != null)
        {
            ammoText.text = "Ammo: " + currentAmmo + " / " + maxAmmo;
        }
    }

    public void UpdateItems(int itemsCount)
    {
        if (itemsText != null)
        {
            itemsText.text = "Items: " + itemsCount;
            
            // Si el GameManager ya calculó el total
            if (GameManager.Instance != null && GameManager.Instance.itemsToCollectForLevel > 0)
            {
                itemsText.text = "Items: " + itemsCount + " / " + GameManager.Instance.itemsToCollectForLevel;
            }
        }
    }
    #endregion
    
    #region Button Callbacks (For Unity Editor)
    // Link to 'Play' button in Main Menu
    public void BTN_ClickPlay()
    {
        GameManager.Instance.UpdateGameState(GameManager.GameState.CharacterSelection);
    }

    // Link to 'Quit' button in Main Menu
    public void BTN_ClickQuit()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // Link to 'Restart' button in GameOver
    public void BTN_ClickRestart()
    {
        GameManager.Instance.RestartCurrentLevel();
    }

    // Link to 'Next Level' button in LevelComplete
    public void BTN_ClickNextLevel()
    {
        GameManager.Instance.LoadNextLevel();
    }
    #endregion
}
