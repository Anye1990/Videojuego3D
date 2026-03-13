using UnityEngine;

public class ItemCollectible : MonoBehaviour
{
    public enum ItemType
    {
        GoldBar,
        Strawberry,
        AmmoCrate
    }

    [Header("Item Settings")]
    public ItemType itemType;
    public int amount = 20; // Default amount for ammo or health

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats == null) return;

            bool wasCollected = false;

            switch (itemType)
            {
                case ItemType.GoldBar:
                    if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Playing)
                    {
                        GameManager.Instance.ItemCollected();
                        wasCollected = true;
                    }
                    break;
                case ItemType.Strawberry:
                    wasCollected = stats.Heal(amount);
                    break;
                case ItemType.AmmoCrate:
                    wasCollected = stats.AddAmmo(amount);
                    break;
            }

            if (wasCollected)
            {
                Destroy(gameObject);
            }
        }
    }
}
