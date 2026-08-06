using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class SwipeMovement : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private GameManager gameManager;
    
    [Header("Movement")]
    [SerializeField, Min(0.01f)] private float moveSpeed = 8f;

    [Header("Swipe")]
    [SerializeField, Min(1f)] private float minimumSwipeDistance = 50f;
    [SerializeField] private Camera inputCamera;

    [Header("Wall Detection")]
    [SerializeField] private LayerMask obstacleLayers = ~0;
    [SerializeField, Min(0.001f)] private float collisionSkin = 0.02f;
    [SerializeField, Range(0f, 1f)] private float minimumWallFacing = 0.5f;

    private readonly RaycastHit[] wallHits = new RaycastHit[32];

    private BoxCollider boxCollider;
    private Vector2 swipeStartPosition;
    private Vector3 moveDirection;
    private bool isTrackingSwipe;

    public bool IsMoving { get; private set; }

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.EndGame += StopMoving;
        }

        if (inputCamera == null)
        {
            inputCamera = Camera.main;
        }
    }

    private void Update()
    {
        ReadSwipeInput();
    }

    private void FixedUpdate()
    {
        if (!IsMoving)
        {
            return;
        }

        float desiredDistance = moveSpeed * Time.fixedDeltaTime;
        float allowedDistance = GetAllowedDistance(desiredDistance, out bool wallFound);

        transform.position += moveDirection * allowedDistance;

        if (wallFound)
        {
            StopMoving();
        }
    }

    private void ReadSwipeInput()
    {
        Pointer pointer = Pointer.current;

        if (pointer == null)
        {
            return;
        }

        if (pointer.press.wasPressedThisFrame)
        {
            if (IsMoving)
            {
                isTrackingSwipe = false;
                return;
            }

            swipeStartPosition = pointer.position.ReadValue();
            isTrackingSwipe = true;
        }

        if (!pointer.press.wasReleasedThisFrame || !isTrackingSwipe)
        {
            return;
        }

        isTrackingSwipe = false;

        Vector2 swipe = pointer.position.ReadValue() - swipeStartPosition;
        if (swipe.magnitude < minimumSwipeDistance)
        {
            return;
        }

        BeginMovement(GetWorldDirection(swipe));
    }

    private Vector3 GetWorldDirection(Vector2 swipe)
    {
        bool isHorizontalSwipe = Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y);
        float swipeSign = Mathf.Sign(isHorizontalSwipe ? swipe.x : swipe.y);

        Vector3 screenAxis;
        if (inputCamera != null)
        {
            screenAxis = isHorizontalSwipe
                ? inputCamera.transform.right
                : inputCamera.transform.up;
        }
        else
        {
            screenAxis = isHorizontalSwipe ? Vector3.right : Vector3.forward;
        }

        screenAxis.y = 0f;
        screenAxis *= swipeSign;

        if (screenAxis.sqrMagnitude < 0.001f)
        {
            screenAxis = isHorizontalSwipe
                ? Vector3.right * swipeSign
                : Vector3.forward * swipeSign;
        }

        return Mathf.Abs(screenAxis.x) > Mathf.Abs(screenAxis.z)
            ? Vector3.right * Mathf.Sign(screenAxis.x)
            : Vector3.forward * Mathf.Sign(screenAxis.z);
    }

    private void BeginMovement(Vector3 direction)
    {
        moveDirection = direction.normalized;
        IsMoving = true;

        if (gameManager != null)
        {
            gameManager.RegisterSwipe();
        }
        else
        {
            GameSessionData.GetOrCreate().RegisterSwipe();
        }
    }

    private float GetAllowedDistance(float desiredDistance, out bool wallFound)
    {
        Vector3 scale = transform.lossyScale;
        Vector3 absoluteScale = new Vector3(
            Mathf.Abs(scale.x),
            Mathf.Abs(scale.y),
            Mathf.Abs(scale.z));

        Vector3 halfExtents = Vector3.Scale(boxCollider.size * 0.5f, absoluteScale);
        halfExtents = Vector3.Max(
            halfExtents - Vector3.one * collisionSkin,
            Vector3.one * 0.001f);

        Vector3 castOrigin = transform.TransformPoint(boxCollider.center);
        float castDistance = desiredDistance + collisionSkin;
        int hitCount = Physics.BoxCastNonAlloc(
            castOrigin,
            halfExtents,
            moveDirection,
            wallHits,
            transform.rotation,
            castDistance,
            obstacleLayers,
            QueryTriggerInteraction.Ignore);

        float allowedDistance = desiredDistance;
        wallFound = false;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = wallHits[i];

            if (hit.collider == boxCollider)
            {
                continue;
            }

            float wallFacing = Vector3.Dot(hit.normal, -moveDirection);
            if (wallFacing < minimumWallFacing)
            {
                continue;
            }

            float distanceBeforeWall = Mathf.Max(0f, hit.distance - collisionSkin);
            allowedDistance = Mathf.Min(allowedDistance, distanceBeforeWall);
            wallFound = true;
        }

        return allowedDistance;
    }

    private void StopMoving()
    {
        IsMoving = false;
        moveDirection = Vector3.zero;
    }

    private void OnDisable()
    {
        isTrackingSwipe = false;
        StopMoving();
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.EndGame -= StopMoving;
        }
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0.01f, moveSpeed);
        minimumSwipeDistance = Mathf.Max(1f, minimumSwipeDistance);
        collisionSkin = Mathf.Max(0.001f, collisionSkin);
    }
}
