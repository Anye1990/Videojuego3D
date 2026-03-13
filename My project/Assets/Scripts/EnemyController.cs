using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    [Tooltip("Distancia a la que se detiene a golpear sin chocar contigo")]
    public float stoppingDistance = 1.6f; 

    [Header("Attack Settings")]
    public int attackDamage = 15;
    public float attackCooldown = 2f;
    private float lastAttackTime = -9999f; 

    private Transform playerTarget;
    private NavMeshAgent navMeshAgent;
    private Animator animator;

    private void Start()
    {
        currentHealth = maxHealth;
        
        // Buscar al jugador basándonos en si tiene el script de vida (PlayerStats)
        PlayerStats pStats = FindObjectOfType<PlayerStats>();
        GameObject player = pStats != null ? pStats.gameObject : null;
        
        if (player != null)
        {
            playerTarget = player.transform;
            Debug.Log("ENEMIGO: ¡Encontré al jugador! Su nombre es: " + player.name);
        }
        else
        {
            Debug.LogError("ENEMIGO: No pude encontrar al jugador al iniciar. Asegúrate de que se llame 'PlayerArmature(Clone)'.");
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
        }

        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (playerTarget == null)
        {
            // Intentar buscar de nuevo si el jugador se genera unos segundos tarde
            PlayerStats pStats = FindObjectOfType<PlayerStats>();
            GameObject player = pStats != null ? pStats.gameObject : null;
            
            if (player != null)
            {
                playerTarget = player.transform;
                Debug.Log("ENEMIGO: ¡Encontré al jugador con retraso! Su nombre es: " + player.name);
            }
            else
            {
                // Si no hay jugador, detener animación de caminar
                if (animator != null) animator.SetBool("IsWalking", false);
                return;
            }
        }

        // Calculamos la distancia plana (ignorando alturas) para evitar bugs de Navmesh
        Vector3 flatEnemyPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatPlayerPos = new Vector3(playerTarget.position.x, 0, playerTarget.position.z);
        float distanceToPlayer = Vector3.Distance(flatEnemyPos, flatPlayerPos);

        // Si estamos lejos, perseguimos
        if (distanceToPlayer > stoppingDistance)
        {
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(playerTarget.position);
            }
            else
            {
                // Respaldo manual si falla el suelo
                Vector3 direction = (playerTarget.position - transform.position).normalized;
                direction.y = 0; 
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
            }

            if (animator != null) animator.SetBool("IsWalking", true);
        }
        else
        {
            // ¡Ya llegamos a ti! Frenamos por completo
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.velocity = Vector3.zero; // Frenado seco NavMesh
            }

            // [NUEVO] Si el enemigo tiene Rigidbody (que seguro lo tiene), le matamos cualquier inercia
            // para que deje de patinar hacia adelante y empujarte
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (animator != null) animator.SetBool("IsWalking", false);

            // Giramos el muñeco para mirarte a los ojos
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f); 
            }

            // Atacamos si ya pasó el cooldown
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
    }

    private void AttackPlayer()
    {
        lastAttackTime = Time.time;
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        Debug.Log("¡El enemigo atacó al jugador y le hizo " + attackDamage + " de daño!");
        
        // Daño real a la barra de vida del jugador
        PlayerStats stats = playerTarget.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;
        Debug.Log("Enemigo recibió daño: " + damageAmount + ". Salud actual: " + currentHealth);

        if (animator != null && currentHealth > 0) animator.SetTrigger("Hit");

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Debug.Log("Enemigo destruido");
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Playing) GameManager.Instance.EnemyKilled();
        if (animator != null) animator.SetTrigger("Die");

        if (navMeshAgent != null) navMeshAgent.enabled = false;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        this.enabled = false;
        if (animator != null) animator.SetBool("IsWalking", false);

        Destroy(gameObject, 3f);
    }
}
