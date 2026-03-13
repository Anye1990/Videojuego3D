using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    static void ClearSelectionOnPlay()
    {
        UnityEditor.EditorApplication.playModeStateChanged += (state) =>
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode || state == UnityEditor.PlayModeStateChange.EnteredPlayMode)
            {
                UnityEditor.Selection.activeObject = null;
            }
        };
    }
#endif

    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState currentState;

    [Header("Level Progress")]
    public int totalEnemiesInLevel = 0;
    public int enemiesKilled = 0;
    public int itemsCollected = 0;
    public int itemsToCollectForLevel = 0;

    [Header("Player Settings")]
    public Transform playerSpawnPoint;
    public GameObject currentPlayerInstance;

    public enum GameState
    {
        MainMenu,
        CharacterSelection,
        Playing,
        GameOver,
        LevelComplete
    }

    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep the GameManager alive across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateGameState(GameState.MainMenu);
    }

    public void UpdateGameState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.MainMenu:
                Time.timeScale = 0f; // Pausar el juego para que no pase nada de fondo
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                UIManager.Instance.ShowMainMenu();
                break;
            case GameState.CharacterSelection:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                UIManager.Instance.ShowCharacterSelection();
                break;
            case GameState.Playing:
                Time.timeScale = 1f; // Reanudar el juego
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                UIManager.Instance.ShowPlayerHUD();
                CountLevelObjectives();
                break;
            case GameState.GameOver:
                Time.timeScale = 0f; // Pausar juego
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                UIManager.Instance.ShowGameOver();
                break;
            case GameState.LevelComplete:
                Time.timeScale = 0f; // Pausar juego
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                UIManager.Instance.ShowLevelComplete();
                break;
        }
    }

    public void StartGameWithCharacter(GameObject characterPrefab)
    {
        // 1. Destroy any existing players in the scene to avoid duplicates (usando el script de ThirdPersonController en lugar del Tag)
        // El bug del MissingReferenceException ya se manejó con el UnityEditor.InitializeOnLoadMethod arriba.
        StarterAssets.ThirdPersonController[] existingPlayers = Object.FindObjectsByType<StarterAssets.ThirdPersonController>(FindObjectsSortMode.None);
        foreach (var player in existingPlayers)
        {
            if (player.gameObject != currentPlayerInstance)
            {
                Destroy(player.gameObject);
            }
        }

        if (currentPlayerInstance != null)
        {
            Destroy(currentPlayerInstance);
        }

        // 2. Spawn character at spawn point (or 0,0,0 if not set)
        Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
        Quaternion spawnRot = playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;

        currentPlayerInstance = Instantiate(characterPrefab, spawnPos, spawnRot);

        // 2.5 Reconectar la cámara al nuevo jugador
        Cinemachine.CinemachineVirtualCamera vcam = Object.FindAnyObjectByType<Cinemachine.CinemachineVirtualCamera>();
        if (vcam != null)
        {
            // StarterAssets usa un objeto vacío adentro del jugador llamado "PlayerCameraRoot"
            Transform cameraRoot = currentPlayerInstance.transform.Find("PlayerCameraRoot");
            if (cameraRoot != null)
            {
                vcam.Follow = cameraRoot;
            }
            else
            {
                vcam.Follow = currentPlayerInstance.transform; // Fallback
            }
        }

        // 3. Set state to playing
        UpdateGameState(GameState.Playing);
    }

    public void EnemyKilled()
    {
        enemiesKilled++;
        CheckLevelComplete();
    }

    public void ItemCollected()
    {
        itemsCollected++;
        UIManager.Instance.UpdateItems(itemsCollected);
        CheckLevelComplete();
    }

    private void CountLevelObjectives()
    {
        // Find all enemies in the scene
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        totalEnemiesInLevel = enemies.Length;
        enemiesKilled = 0;

        // Find all items in the scene and just count the GoldBars
        ItemCollectible[] items = Object.FindObjectsByType<ItemCollectible>(FindObjectsSortMode.None);
        itemsToCollectForLevel = 0;
        foreach (var item in items)
        {
            if (item.itemType == ItemCollectible.ItemType.GoldBar)
            {
                itemsToCollectForLevel++;
            }
        }
        
        itemsCollected = 0;
        UIManager.Instance.UpdateItems(0); // initialize HUD text
    }

    private void CheckLevelComplete()
    {
        if (currentState != GameState.Playing) return;

        // Only checking if you collected all gold bars to win the level (ignoring enemies due to spawner)
        // Ensure there is actually a goal, to prevent auto-winning on empty scenes
        if (itemsToCollectForLevel > 0 && itemsCollected >= itemsToCollectForLevel)
        {
            // Level is complete!
            UpdateGameState(GameState.LevelComplete);
        }
    }

    #region Scene Transitions
    public void LoadNextLevel()
    {
        // Example: load next scene in build index
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
            UpdateGameState(GameState.Playing); // Once scene loads, we are playing
        }
        else
        {
            Debug.Log("No more levels! You win the entire game.");
            // You could load a Credits scene here or go back to main menu
            UpdateGameState(GameState.MainMenu);
        }
    }

    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        UpdateGameState(GameState.Playing);
    }

    public void GoToMainMenu()
    {
        // Assuming Scene 0 is main menu, or just reset state
        // SceneManager.LoadScene(0);
        UpdateGameState(GameState.MainMenu);
    }
    #endregion
}
