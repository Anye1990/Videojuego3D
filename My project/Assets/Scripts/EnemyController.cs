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
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > stoppingDistance)
        {
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
            {
                // Movimiento usando NavMeshAgent
                navMeshAgent.SetDestination(playerTarget.position);
            }
            else
            {
                // Movimiento manual básico si no hay NavMesh
                Vector3 direction = (playerTarget.position - transform.position).normalized;
                // Ignorar el eje Y para no rotar hacia arriba/abajo
                direction.y = 0; 
                
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }
                
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
            // Está quieto o atacando
            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
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
        
        // Girar hacia el jugador suavemente mientras ataca
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);

        // Disparar la animación de ataque (usar un Trigger)
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Simular que le bajamos vida al jugador (luego conectaremos esto con la vida real del Player)
        Debug.Log("¡El enemigo atacó al jugador y le hizo " + attackDamage + " de daño!");
        
        // Aquí llamaríamos al script del jugador, por ejemplo:
        // playerTarget.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log("Enemigo recibió daño: " + damageAmount + ". Salud actual: " + currentHealth);

        // Puedes añadir aquí efectos visuales o de sonido de recibir daño

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemigo destruido");
        // Puedes instanciar un efecto de partículas aquí antes de destruir el objeto
        Destroy(gameObject);
    }
}
