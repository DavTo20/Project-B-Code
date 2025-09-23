using UnityEngine;
using UnityEngine.InputSystem;

//Fucking magic

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float gravity = -40f;
    public float jumpHeight = 2f;
    public float lookSensitivity = 1f;
    public float minPitch = -70f;
    public float maxPitch = 80f;

    public float crouchSpeed = 2f;
    public float standingHeight = 2f;
    public float crouchHeight = 1f;
    public Vector3 standingCenter = new Vector3(0, 1f, 0);
    public Vector3 crouchCenter = new Vector3(0,  1f, 0);

    private float jumpHoldTime = 0f;
    private bool jumpHeld = false;
    private const float ledgeGrabHoldThreshold = 0.1f;

    public Transform cameraTransform;
    public Transform playerModel;

    public CharacterController controller { get; private set; }
    private InputSystem_Actions inputActions;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting;
    public bool IsSprinting() => isSprinting;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.3f;

    private PlayerMovement movement;
    private PlayerCamera playerCamera;
    private PlayerJump jump;
    private PlayerCrouch crouch;

    public float slideSpeed = 5f;

    public float maxSlopeAngle = 45f;
    public float GetMaxSlopeAngle() => maxSlopeAngle;

    private PlayerLedgeClimb ledgeClimb;
    public bool IsClimbing { get; set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new InputSystem_Actions();

        ledgeClimb = new PlayerLedgeClimb(controller, this);

        movement = new PlayerMovement(controller, this);
        playerCamera = new PlayerCamera(cameraTransform, this);
        jump = new PlayerJump(controller, this, ledgeClimb);
        crouch = new PlayerCrouch(controller, this);
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;

        inputActions.Player.Jump.started += ctx => {
            jumpHeld = true;
            jump.OnJump(ctx);
        };
        inputActions.Player.Jump.canceled += ctx => {
            jumpHeld = false;
            jumpHoldTime = 0f;
        };


        inputActions.Player.Sprint.started += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;

        inputActions.Player.Crouch.started += ctx => crouch.SetCrouching(true);
        inputActions.Player.Crouch.canceled += ctx => crouch.SetCrouching(false);
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();

        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;

        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;

        inputActions.Player.Jump.performed -= jump.OnJump;

        inputActions.Player.Crouch.started -= ctx => crouch.SetCrouching(true);
        inputActions.Player.Crouch.canceled -= ctx => crouch.SetCrouching(false);
    }

    private void Update()
    {
        if(IsClimbing)
            return;

        playerCamera.UpdateCamera(lookInput, moveInput);

        if (IsGrounded(out _))
        {
            if (moveInput.magnitude < 0.01f)
            {
                controller.Move(Vector3.zero);
            }
            else
            {
                movement.Move(moveInput, IsSprinting());
            }
        }

        if (!IsClimbing)
        {
            Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;

            jump.Update(moveDir, IsSprinting());

            if (jumpHeld)
            {
                jumpHoldTime += Time.deltaTime;

                if (jumpHoldTime >= ledgeGrabHoldThreshold)
                {
                    ledgeClimb.TryClimb();
                }
            }
        }


        crouch.Update();

        lookInput = Vector2.zero;
    }

    private void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnLook(InputAction.CallbackContext ctx) => lookInput = ctx.ReadValue<Vector2>();

    public float GetSpeed(bool sprinting)
    {
        if (crouch != null && crouch.IsCrouching)
            return crouchSpeed;

        return sprinting ? sprintSpeed : walkSpeed;
    }

    public bool IsGrounded(out Vector3 surfaceNormal)
    {
        surfaceNormal = Vector3.up;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float rayDistance = groundCheckDistance + 0.1f;
        float rayOffset = controller.radius * 0.5f;

        Vector3[] rayOrigins = new Vector3[]
        {
            origin,
            origin + transform.forward * rayOffset,
            origin - transform.forward * rayOffset,
            origin + transform.right * rayOffset,
            origin - transform.right * rayOffset,
        };

        foreach (Vector3 rayOrigin in rayOrigins)
        {
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, groundLayer))
            {
                surfaceNormal = hit.normal;

                // Optional: visualize ray
                Debug.DrawRay(rayOrigin, Vector3.down * rayDistance, Color.green);

                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle < controller.slopeLimit)
                {
                    return true;
                }
            }
            else
            {
                Debug.DrawRay(rayOrigin, Vector3.down * rayDistance, Color.red);
            }
        }

        return false;
    }


    public float GetGravity() => gravity;
    public float GetJumpHeight() => jumpHeight;
    public float GetLookSensitivity() => lookSensitivity;
    public float GetMinPitch() => minPitch;
    public float GetMaxPitch() => maxPitch;

}
