using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    [Tooltip("El objeto (Enemigo, Item, etc.) que quieres generar. ¡Recuerda usar un Prefab!")]
    public GameObject prefabToSpawn;
    
    [Tooltip("Cantidad de objetos a generar en esta zona.")]
    public int spawnCount = 5;
    
    [Tooltip("Tamaño de la zona de aparición (X, Y, Z).")]
    public Vector3 spawnArea = new Vector3(10, 0, 10);
    
    [Tooltip("¿Generar los objetos automáticamente al iniciar el juego?")]
    public bool spawnOnStart = true;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnObjects();
        }
    }

    public void SpawnObjects()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("Cuidado: No has asignado ningún Prefab en el Spawner " + gameObject.name);
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            // Calcula una posición aleatoria basándose en el tamaño del área
            Vector3 randomPosition = new Vector3(
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                Random.Range(-spawnArea.y / 2, spawnArea.y / 2), // Usar 0 en la Y del area si quieres que spawneen todos al mismo nivel
                Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
            );

            // Se suma la posición del propio Spawner para que aparezcan a su alrededor
            Vector3 spawnPosition = transform.position + randomPosition;
            
            // Instancia el objeto
            Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        }
    }

    // ==========================================
    // Esto es magia para el Editor de Unity
    // Dibuja una caja verde para que puedas VERICAMENTE confirmar dónde está tu zona
    // ==========================================
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f); // Verde transparente por dentro
        Gizmos.DrawCube(transform.position, spawnArea);
        Gizmos.color = Color.green;              // Borde verde fuerte
        Gizmos.DrawWireCube(transform.position, spawnArea);
    }
}
