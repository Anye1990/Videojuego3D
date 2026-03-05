using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 1.5f;

    [Header("Attack Settings")]
    public int attackDamage = 15;
    public float attackCooldown = 2f;
    private float lastAttackTime = -9999f; // Empezamos en un número muy negativo para que el primer golpe sea instantáneo

    private Transform playerTarget;
    private NavMeshAgent navMeshAgent;
    private Animator animator;

    private void Start()
    {
        currentHealth = maxHealth;
        
        // Asignamos el jugador buscando su tag "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }

        // Si el enemigo tiene un NavMeshAgent, lo usamos. Si no, usaremos movimiento manual.
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
        }

        // Obtener el Animator (buscamos también en los hijos por si el modelo 3D está anidado)
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
            else
            {
                // Si no hay jugador, detener animación de caminar
                if (animator != null) animator.SetBool("IsWalking", false);
                return;
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > stoppingDistance)
        {
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
            {
                // Movimiento usando NavMeshAgent
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(playerTarget.position);

                // Forzar rotación hacia el jugador incluso si el NavMesh patina
                Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
                directionToPlayer.y = 0;
                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }
            }
            else
            {
                // Si tiene el componente pero no hay "NavMesh" calculado en el nivel, usar movimiento manual
                if (navMeshAgent != null) navMeshAgent.enabled = false; 

                // Movimiento manual básico
                Vector3 direction = (playerTarget.position - transform.position).normalized;
                // Ignorar el eje Y para no rotar hacia arriba/abajo
                direction.y = 0; 
                
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }
                
                // Mover al enemigo hacia adelante de forma manual sin físicas complejas
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
            }

            // Animación: Activamos el boolean IsWalking
            if (animator != null)
            {
                animator.SetBool("IsWalking", true);
            }
        }
        else
        {
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
            {
                // Frenar al agente
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
            }

            // Está quieto o atacando
            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
            }

            // Rotar para mirar al jugador al atacar
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }

            // Lógica de Ataque
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
    }

    private void AttackPlayer()
    {
        lastAttackTime = Time.time;
        
        // Disparar la animación de ataque (usar un Trigger)
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Simular que le bajamos vida al jugador (luego conectaremos esto con la vida real del Player)
        Debug.Log("¡El enemigo atacó al jugador y le hizo " + attackDamage + " de daño!");
        
        PlayerStats stats = playerTarget.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        // Si ya está muerto, no hacer nada más
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;
        Debug.Log("Enemigo recibió daño: " + damageAmount + ". Salud actual: " + currentHealth);

        // Disparar animación de recibir daño
        if (animator != null && currentHealth > 0)
        {
            animator.SetTrigger("Hit");
        }

        // Puedes añadir aquí efectos visuales o de sonido de recibir daño

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemigo destruido");
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Playing)
        {
            GameManager.Instance.EnemyKilled();
        }
        
        // Disparar animación de muerte
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Desactivar movimiento y colisiones para que no atraviese pisos o siga atacando como fantasma
        if (navMeshAgent != null) navMeshAgent.enabled = false;
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        // Desactivar este script para que deje de perseguir al jugador
        this.enabled = false;

        // Limpiar animador para detener animaciones raras si estaba caminando
        if (animator != null) animator.SetBool("IsWalking", false);

        // Destruir después de 3 segundos para que la animación termine de verse
        Destroy(gameObject, 3f);
    }
}
