using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLedgeClimb
{
    private readonly CharacterController controller;
    private readonly PlayerController config;
    private readonly Transform cameraTransform;

    private bool isClimbing = false;
    private float climbDuration = 0.3f;
    private float ledgeCheckDistance = 1.2f;
    private float ledgeCheckHeight = 2f;
    private float minClimbHeight = 1.0f;
    private float maxClimbHeight = 2.7f;
    private LayerMask ledgeLayer;


    public PlayerLedgeClimb(CharacterController controller, PlayerController config)
    {
        this.controller = controller;
        this.config = config;
        this.cameraTransform = config.cameraTransform;
        this.ledgeLayer = config.groundLayer;
    }

    public void TryClimb()
    {
        if (isClimbing || !Keyboard.current.spaceKey.isPressed)
            return;

        if (DetectLedge(out Vector3 targetPoint))
        {
            config.StartCoroutine(ClimbToLedge(targetPoint));
        }
    }

    public bool TryClimbFromJump()
    {
        if (isClimbing || !Keyboard.current.spaceKey.isPressed)
            return false;

        if (DetectLedge(out Vector3 targetPoint))
        {
            config.StartCoroutine(ClimbToLedge(targetPoint));
            return true;
        }

        return false;
    }


    private bool DetectLedge(out Vector3 ledgePoint)
    {
        ledgePoint = Vector3.zero;

        Vector3 origin = cameraTransform.position;
        Vector3 forward = cameraTransform.forward;

        // Forward check to find wall
        if (Physics.Raycast(origin, forward, out RaycastHit wallHit, ledgeCheckDistance, ledgeLayer))
        {
            // check if player is looking at an edge
            float lookDot = Vector3.Dot(forward.normalized, -wallHit.normal);
            if (lookDot < 0.7f) // not facing wall enough
                return false;

            Vector3 topRayOrigin = wallHit.point + Vector3.up * ledgeCheckHeight;

            // downward check to find ledge
            if (Physics.Raycast(topRayOrigin, Vector3.down, out RaycastHit ledgeHit, ledgeCheckHeight + 0.5f, ledgeLayer))
            {
                float heightDiff = ledgeHit.point.y - config.transform.position.y;

                if (heightDiff > minClimbHeight && heightDiff < maxClimbHeight)
                {
                    ledgePoint = ledgeHit.point;
                    return true;
                }
            }
        }

        return false;
    }


    private System.Collections.IEnumerator ClimbToLedge(Vector3 targetPoint)
    {
        isClimbing = true;
        config.IsClimbing = true;

        Vector3 start = config.transform.position;
        Vector3 end = targetPoint + config.transform.forward * 0.5f;

        float elapsed = 0f;
        var rb = config.GetComponent<Rigidbody>();
        if (rb != null) rb.useGravity = false;

        while (elapsed < climbDuration)
        {
            config.controller.enabled = false;
            config.transform.position = Vector3.Lerp(start, end, elapsed / climbDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        config.transform.position = end;
        config.controller.enabled = true;
        if (rb != null) rb.useGravity = true;

        isClimbing = false;
        config.IsClimbing = false;
    }
}
