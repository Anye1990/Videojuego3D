using UnityEngine;

public class ItemCollectible : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Registrar el item en el Game Manager
            if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Playing)
            {
                GameManager.Instance.ItemCollected();
                Destroy(gameObject);
            }
            else
            {
                // Fallback (solo destruirlo si no hay GM para no dejar basura)
                Destroy(gameObject);
            }
        }
    }
}
