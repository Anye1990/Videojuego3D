using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Ammo")]
    public int maxAmmo = 100;
    private int currentAmmo;

    private void Start()
    {
        currentHealth = maxHealth;
        currentAmmo = maxAmmo;

        // Call UpdateHUD via Invoke slightly later to ensure the GameManager & UIManager 
        // have properly finished initializing the new UI Panel text elements 
        Invoke(nameof(UpdateHUD), 0.1f);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        
        UpdateHUD();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public bool Heal(int amount)
    {
        if (currentHealth >= maxHealth)
        {
            return false; // Ya tiene la vida al máximo
        }

        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHUD();
        return true;
    }

    public bool UseAmmo(int amount)
    {
        if (currentAmmo >= amount)
        {
            currentAmmo -= amount;
            UpdateHUD();
            return true;
        }
        return false;
    }

    public bool AddAmmo(int amount)
    {
        if (currentAmmo >= maxAmmo)
        {
            return false; // Ya tiene la munición al máximo
        }

        currentAmmo += amount;
        if (currentAmmo > maxAmmo) currentAmmo = maxAmmo;
        UpdateHUD();
        return true;
    }

    private void UpdateHUD()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth);
            UIManager.Instance.UpdateAmmo(currentAmmo, maxAmmo);
        }
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        // Notify Game Manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateGameState(GameManager.GameState.GameOver);
        }
        else
        {
            // Fallback just in case testing without Game Manager
            Destroy(gameObject); 
        }
    }
}
