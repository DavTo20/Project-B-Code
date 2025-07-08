using UnityEngine;

public class PlayerCrouch
{
    private readonly CharacterController controller;
    private readonly PlayerController config;
    private readonly Transform cameraTransform;

    public bool IsCrouching { get; private set; }

    private bool crouchHeld = false;
    private bool targetCrouchState;
    private float transitionSpeed = 8f;

    private float standingCameraY;
    private float crouchingCameraY;

    public PlayerCrouch(CharacterController controller, PlayerController config)
    {
        this.controller = controller;
        this.config = config;
        this.cameraTransform = config.cameraTransform;

        standingCameraY = cameraTransform.localPosition.y;
        crouchingCameraY = standingCameraY * 0.5f;
    }

    public void SetCrouching(bool value)
    {
        crouchHeld = value;

        if (value)
        {
            targetCrouchState = true;
        }
    }

    public void Update()
    {
        if (!crouchHeld && targetCrouchState)
        {
            if (CanStandUp())
            {
                targetCrouchState = false;
            }
        }

        float targetHeight = targetCrouchState ? config.crouchHeight : config.standingHeight;
        Vector3 targetCenter = targetCrouchState ? config.crouchCenter : config.standingCenter;

        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * transitionSpeed);
        controller.center = Vector3.Lerp(controller.center, targetCenter, Time.deltaTime * transitionSpeed);

        
        float targetCamY = targetCrouchState ? crouchingCameraY : standingCameraY;
        Vector3 camLocalPos = cameraTransform.localPosition;
        camLocalPos.y = Mathf.Lerp(camLocalPos.y, targetCamY, Time.deltaTime * transitionSpeed);
        cameraTransform.localPosition = camLocalPos;

        
        IsCrouching = Mathf.Abs(controller.height - config.crouchHeight) < 0.1f && targetCrouchState;

    if (config.playerModel != null)
    {
        float targetScaleY = targetCrouchState ? 0.5f : 1f;
        Vector3 scale = config.playerModel.localScale;
        scale.y = Mathf.Lerp(scale.y, targetScaleY, Time.deltaTime * transitionSpeed);
        config.playerModel.localScale = scale;
    }

    }

    private bool CanStandUp()
    {
        float checkDistance = config.standingHeight - controller.height;
        Vector3 start = controller.transform.position + Vector3.up * controller.height / 2f;
        float radius = controller.radius * 0.9f;

        return !Physics.SphereCast(start, radius, Vector3.up, out _, checkDistance, ~0, QueryTriggerInteraction.Ignore);
    }
}
