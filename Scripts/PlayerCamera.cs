using UnityEngine;

public class PlayerCamera
{
    private readonly Transform cameraTransform;
    private readonly PlayerController config;
    private float pitch = 0f;

    // camera nod variables
    private float landingBump = 0f;         // Current downward offset from landing impact
    private float bumpVelocity = 0f;        // Velocity reference for SmoothDamp smoothing in line 51
    private float previousY = 0f;           // Player Y position tracker


    // camera tilt when moving
    private float targetTilt = 0f;
    private float currentTilt = 0f;
    private float tiltSmoothVelocity = 0f;
    private float targetForwardTilt = 0f;
    private float currentForwardTilt = 0f;
    private float forwardTiltVelocity = 0f;

    private float maxForwardTiltAngle = 1f;
    private float maxTiltAngle = 1f;     // Maximum roll angle in degrees
    private float tiltSmoothTime = 0.2f;  // Smoothing time
    
    // camera titl when jumping
    private float jumpFallTilt = 0f;
    private float tiltVelocityY = 0f;

    private float maxJumpTilt = 2f;     // Camera tilts up to this when rising
    private float maxFallTilt = -2f;    // Camera tilts down to this when falling
    private float jumpFallTiltSmooth = 0.25f; // Smoothing time

    private float maxBump = 0.3f;           // Maximum camera bump

    // Head bob
    private float headBobTimer = 0f;
    private Vector3 defaultCamLocalPos;
    private float walkBobSpeed = 10f;
    private float sprintBobSpeed = 25f;

    private float walkBobAmount = 0.1f;
    private float sprintBobAmount = 0.15f;

    private float bobTransitionSpeed = 5f; // Smooth transition


    public PlayerCamera(Transform camTransform, PlayerController config)
    {
        this.cameraTransform = camTransform;
        this.config = config;

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize previous Y position to current player Y at start
        previousY = config.transform.position.y;
        defaultCamLocalPos = cameraTransform.localPosition;
    }

    // Too lazy to comment code here
    public void UpdateCamera(Vector2 lookInput, Vector2 moveInput)
    {
        // process mouse input
        float mouseX = lookInput.x * config.GetLookSensitivity();
        float mouseY = lookInput.y * config.GetLookSensitivity();

        float strafeInput = moveInput.x;
        targetTilt = -strafeInput * maxTiltAngle;

        // Rotate the player with the camera
        config.transform.Rotate(Vector3.up * mouseX);

        // Adjust pitch and clamp vertical toggle
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, config.GetMinPitch(), config.GetMaxPitch());

        currentTilt = Mathf.SmoothDamp(currentTilt, targetTilt, ref tiltSmoothVelocity, tiltSmoothTime);

        targetTilt = -strafeInput * maxTiltAngle;
        currentTilt = Mathf.SmoothDamp(currentTilt, targetTilt, ref tiltSmoothVelocity, tiltSmoothTime);

        float forwardInput = moveInput.y;
        targetForwardTilt = forwardInput * maxForwardTiltAngle;
        currentForwardTilt = Mathf.SmoothDamp(currentForwardTilt, targetForwardTilt, ref forwardTiltVelocity, tiltSmoothTime);


        // Final camera position base
        Vector3 targetPos = defaultCamLocalPos;

        // Only apply head bob if moving and grounded
        if (config.IsGrounded(out _) && moveInput.magnitude > 0.1f)
        {
            bool sprinting = config.IsSprinting(); // Make sure this exists in PlayerController
            float bobSpeed = sprinting ? sprintBobSpeed : walkBobSpeed;
            float bobAmount = sprinting ? sprintBobAmount : walkBobAmount;

            headBobTimer += Time.deltaTime * bobSpeed;

            // Sin wave for vertical and horizontal bob
            float bobOffsetY = Mathf.Sin(headBobTimer) * bobAmount;
            float bobOffsetX = Mathf.Cos(headBobTimer * 0.5f) * bobAmount * 0.5f;

            targetPos += new Vector3(bobOffsetX, bobOffsetY, 0f);
        }
        else
        {
            // Reset timer to avoid janky resuming
            headBobTimer = 0f;
        }

        // Combine with landing bump and jump/fall tilt
        targetPos.y -= landingBump;
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetPos, Time.deltaTime * bobTransitionSpeed);

        float deltaY = config.transform.position.y - previousY;

        // Landing detection
        if (config.IsGrounded(out _) && deltaY < -0.1f)
        {
            float fallDistance = Mathf.Abs(deltaY);
            float bumpStrength = Mathf.Clamp(fallDistance / 3f, 0.05f, maxBump);
            landingBump = bumpStrength;
        }

        // Jump/Fall tilt logic
        if (!config.IsGrounded(out _))
        {
            if (deltaY > 0.01f)
            {
                // Rising (jumping)
                jumpFallTilt = Mathf.SmoothDamp(jumpFallTilt, maxJumpTilt, ref tiltVelocityY, jumpFallTiltSmooth);
            }
            else if (deltaY < -0.01f)
            {
                // Falling
                jumpFallTilt = Mathf.SmoothDamp(jumpFallTilt, maxFallTilt, ref tiltVelocityY, jumpFallTiltSmooth);
            }
        }
        else
        {
            // Reset to 0 when grounded
            jumpFallTilt = Mathf.SmoothDamp(jumpFallTilt, 0f, ref tiltVelocityY, jumpFallTiltSmooth);
        }


        // camera tilt when moving (i think) ((FUCKING MAGIC))
        cameraTransform.localRotation = Quaternion.Euler(pitch + jumpFallTilt + currentForwardTilt, 0f, currentTilt);

        // Landing detection
        if (config.IsGrounded(out _) && deltaY < -0.1f)
        {
            float fallDistance = Mathf.Abs(deltaY);
            float bumpStrength = Mathf.Clamp(fallDistance / 3f, 0.05f, maxBump);
            landingBump = bumpStrength;
        }

        previousY = config.transform.position.y;

        // Smooth Camera Recovery
        landingBump = Mathf.SmoothDamp(landingBump, 0f, ref bumpVelocity, 0.15f);

        // Camera Bump
        Vector3 camLocalPos = cameraTransform.localPosition;
        camLocalPos.y -= landingBump;
        cameraTransform.localPosition = camLocalPos;
        


    }
}
