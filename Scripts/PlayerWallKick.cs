using UnityEngine;

public class PlayerWallKick
{
    private readonly CharacterController controller;
    private readonly PlayerController config;

    private int wallKickCount = 0;
    private readonly int maxWallKicks = 1;

    private readonly float baseKickForce = 9f;
    private readonly float kickForceIncrement = 0f;
    private readonly float horizontalPush = 8f;
    private readonly float wallCheckDistance = 1f;

    private Vector3? lastWallNormal = null;
    private const float normalTolerance = 0.1f;

    private readonly LayerMask wallLayer;

    public PlayerWallKick(CharacterController controller, PlayerController config)
    {
        this.controller = controller;
        this.config = config;
        wallLayer = config.groundLayer;
    }

    public bool CanWallKick()
    {
        return wallKickCount < maxWallKicks && IsNearWall(out _);
    }

    public Vector3 PerformWallKick(out float newVerticalVelocity)
    {
        if (!IsNearWall(out RaycastHit hit))
        {
            newVerticalVelocity = 0f;
            return Vector3.zero;
        }

        lastWallNormal = hit.normal;

        // DO NOT USE
        // Increase upwards velocity with each kick currently set to 0 maybe for a power up later on 
        newVerticalVelocity = baseKickForce + (wallKickCount * kickForceIncrement);
        wallKickCount++;

        // Kick away from wall surface
        Vector3 awayFromWall = hit.normal * horizontalPush;
        return awayFromWall;
    }

    public void ResetWallKicks()
    {
        wallKickCount = 0;
        lastWallNormal = null;
    }

    private bool IsNearWall(out RaycastHit bestHit)
    {
        Vector3 origin = config.transform.position + Vector3.up * 1f;
        Vector3[] directions = new Vector3[]
        {
            -config.transform.right,
            config.transform.right,
            -config.transform.forward,
            config.transform.forward
        };

        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(origin, dir, out RaycastHit hit, wallCheckDistance, wallLayer))
            {
                if (lastWallNormal.HasValue && Vector3.Angle(lastWallNormal.Value, hit.normal) < normalTolerance * 90f)
                {
                    continue;
                }

                bestHit = hit;
                Debug.DrawRay(origin, dir * wallCheckDistance, Color.cyan, 0.2f);
                return true;
            }
        }

        bestHit = default;
        return false;
    }

}
