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
        // Try to get max health from UIManager slider if it's already set 
        // by the CharacterSelector, otherwise use default
        currentHealth = maxHealth;
        currentAmmo = maxAmmo;

        UpdateHUD();
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

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHUD();
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

    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
        if (currentAmmo > maxAmmo) currentAmmo = maxAmmo;
        UpdateHUD();
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
