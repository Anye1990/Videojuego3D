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

    [Header("Settings")]
    [Tooltip("Rotation speed when aiming")]
    public float AimRotationSpeed = 20f;
    [Tooltip("Max distance for the bullet")]
    public float Range = 100f;

    private ThirdPersonController _thirdPersonController;
    private StarterAssetsInputs _starterAssetsInputs;

    private void Awake()
    {
        _thirdPersonController = GetComponent<ThirdPersonController>();
        _starterAssetsInputs = GetComponent<StarterAssetsInputs>();
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

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * AimRotationSpeed);
        }
        else
        {
            if(AimCamera) AimCamera.gameObject.SetActive(false);
            _thirdPersonController.RotateOnMove = true;
        }

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
            
            // Visual Debug
            Debug.DrawRay(SpawnBulletPosition.position, aimDir * Range, Color.red, 2f);
            
            // Raycast Shoot
            if(Physics.Raycast(SpawnBulletPosition.position, aimDir, out RaycastHit hit, Range, AimColliderLayerMask)) {
                 Debug.Log("Hit: " + hit.collider.name);
                 // Add logic here to damage enemy or destroy object
            }
        }
    }
}
