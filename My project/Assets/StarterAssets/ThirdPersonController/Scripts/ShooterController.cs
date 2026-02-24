using UnityEngine;
using Cinemachine;
using StarterAssets;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

public class ShooterController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Virtual Camera to use when aiming")]
    public CinemachineVirtualCamera AimCamera;
    
    [Tooltip("The point where the bullet is spawned (Barrel tip)")]
    public Transform SpawnBulletPosition;
    
    [Tooltip("LayerMask for what the bullet can hit (everything except Player)")]
    public LayerMask AimColliderLayerMask;
    
    [Tooltip("The actual weapon object in the character's hand to rotate towards the target")]
    public Transform WeaponTransform;

    [Tooltip("Rotación extra para corregir el modelo 3D del arma si está mirando hacia atrás o de lado")]
    public Vector3 WeaponRotationOffset = new Vector3(0, 0, 0);

    [Header("Settings")]
    [Tooltip("Rotation speed when aiming")]
    public float AimRotationSpeed = 20f;
    [Tooltip("Max distance for the bullet")]
    public float Range = 100f;

    private ThirdPersonController _thirdPersonController;
    private StarterAssetsInputs _starterAssetsInputs;
    private Animator _animator;

    private void Awake()
    {
        _thirdPersonController = GetComponent<ThirdPersonController>();
        _starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        _animator = GetComponent<Animator>();

        // Hacer que el cambio de cámara (Zoom) sea más rápido
        if (Camera.main != null && Camera.main.TryGetComponent(out CinemachineBrain brain))
        {
            var blend = brain.m_DefaultBlend;
            blend.m_Time = 0.1f; // Transición de cámara súper rápida de 0.1 segundos
            brain.m_DefaultBlend = blend;
        }
    }

    private void Update()
    {
        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, AimColliderLayerMask))
        {
            mouseWorldPosition = raycastHit.point;
        }
        else
        {
             mouseWorldPosition = ray.GetPoint(999f); 
        }

        // Handle Aiming
        bool isAiming = false;
#if ENABLE_INPUT_SYSTEM
        if(Mouse.current != null) isAiming = Mouse.current.rightButton.isPressed;
#else
        isAiming = Input.GetMouseButton(1);
#endif

        if (isAiming)
        {
            if(AimCamera) AimCamera.gameObject.SetActive(true);
            _thirdPersonController.RotateOnMove = false;

            // Indicarle al Animator que estamos apuntando para que cambie a la postura de apuntado
            if (_animator != null)
            {
                _animator.SetBool("IsAiming", true);
            }

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            // Evitar que el personaje se incline hacia adelante al apuntar
            if (aimDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(aimDirection), Time.deltaTime * AimRotationSpeed);
            }
        }
        else
        {
            if(AimCamera) AimCamera.gameObject.SetActive(false);
            _thirdPersonController.RotateOnMove = true;

            // Indicarle al Animator que dejamos de apuntar
            if (_animator != null)
            {
                _animator.SetBool("IsAiming", false);
            }
            
            // Si el jugador soltó el botón de apuntar, cancelar la animación de disparo si estaba en medio de una
            if (_animator != null)
            {
                _animator.ResetTrigger("Shoot");
            }
        }

        // Forzar al personaje a mantenerse completamente recto y evitar que se siga hundiendo
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        // Handle Shooting
        bool isShooting = false;
#if ENABLE_INPUT_SYSTEM
        if(Mouse.current != null) isShooting = Mouse.current.leftButton.wasPressedThisFrame;
#else
        isShooting = Input.GetMouseButtonDown(0);
#endif

        if (isShooting && isAiming && SpawnBulletPosition != null)
        {
            Vector3 aimDir = (mouseWorldPosition - SpawnBulletPosition.position).normalized;
            Vector3 hitPoint = SpawnBulletPosition.position + aimDir * Range;
            
            // Visual Debug
            Debug.DrawRay(SpawnBulletPosition.position, aimDir * Range, Color.red, 2f);
            
            // Raycast Shoot
            if(Physics.Raycast(SpawnBulletPosition.position, aimDir, out RaycastHit hit, Range, AimColliderLayerMask)) {
                 hitPoint = hit.point;
                 Debug.Log("Hit: " + hit.collider.name);
                 
                 // Intentar obtener el componente EnemyController si existe
                 EnemyController enemy = hit.collider.GetComponent<EnemyController>() ?? hit.collider.GetComponentInParent<EnemyController>();
                 
                 if (enemy != null)
                 {
                     enemy.TakeDamage(25); // Cantidad de daño por disparo
                 }
                 else if (hit.collider.gameObject.name.ToLower().Contains("pet_rock") || hit.collider.CompareTag("Enemy"))
                 {
                     // Si no tiene el script pero es enemigo o piedra, lo destruimos de inmediato (comportamiento anterior)
                     Destroy(hit.collider.gameObject);
                 }
                 else if (!hit.collider.CompareTag("Player") && hit.collider.gameObject.name != "Plane" && hit.collider.gameObject.name != "Terrain" && hit.collider.gameObject.name.ToLower().Contains("floor") == false)
                 {
                     // Si quieres que destruya cualquier cubo de prueba sin tag, 
                     // descomenta la siguiente línea pero ten cuidado de no apuntar al suelo base:
                     // Destroy(hit.collider.gameObject);
                 }
            }

            // Reproducir efecto visual del rayo
            CreateVisualRay(SpawnBulletPosition.position, hitPoint);
            
            // Reproducir animación (Requiere un parámetro de tipo Trigger llamado "Shoot" en el Animator)
            if (_animator != null)
            {
                _animator.SetTrigger("Shoot");
            }
        }
    }

    private void CreateVisualRay(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("ShootRay");
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = 0.02f;
        line.endWidth = 0.02f;
        
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = Color.yellow;
        line.material = lineMaterial;
        
        Destroy(lineObj, 0.05f); // Destruir la línea rápidamente para dar efecto de destello de disparo
    }

    private void LateUpdate()
    {
        // Si tenemos un transform del arma asignado y estamos apuntando
        bool isAiming = false;
#if ENABLE_INPUT_SYSTEM
        if(Mouse.current != null) isAiming = Mouse.current.rightButton.isPressed;
#else
        isAiming = Input.GetMouseButton(1);
#endif

        if (isAiming && WeaponTransform != null)
        {
            Vector3 mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, AimColliderLayerMask))
            {
                mouseWorldPosition = raycastHit.point;
            }
            else
            {
                 mouseWorldPosition = ray.GetPoint(999f); 
            }

            // Forzar al Transform del arma a mirar exactamente al punto donde apunta la cámara
            // Esto se hace en LateUpdate para sobreescribir la rotación que le impone la animación
            WeaponTransform.LookAt(mouseWorldPosition);
            
            // Añadir compensación de rotación si el modelo 3D del arma vino torcido por defecto
            WeaponTransform.Rotate(WeaponRotationOffset, Space.Self);
        }
    }
}
