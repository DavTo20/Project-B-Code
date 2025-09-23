using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump
{
    private readonly CharacterController controller;
    private readonly PlayerController config;
    private readonly PlayerWallKick wallKick;
    private readonly PlayerLedgeClimb ledgeClimb;

    private float verticalVelocity = 0f;
    private Vector3 airMomentum = Vector3.zero;

    private float airDrag = 1f;
    private float slideSpeed = 5f;



    public PlayerJump(CharacterController controller, PlayerController config, PlayerLedgeClimb ledgeClimb)
    {
        this.controller = controller;
        this.config = config;
        this.wallKick = new PlayerWallKick(controller, config);
        this.ledgeClimb = ledgeClimb;
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(config.GetJumpHeight() * -2f * config.GetGravity());
            wallKick.ResetWallKicks();
        }
        else
        {
            if (ledgeClimb.TryClimbFromJump())
                return;

            if (wallKick.CanWallKick())
            {
                airMomentum = wallKick.PerformWallKick(out verticalVelocity);
            }
        }
    }






    public void Update(Vector3 moveDir, bool isSprinting)
    {
        bool grounded = config.IsGrounded(out Vector3 surfaceNormal);
        float slopeAngle = Vector3.Angle(surfaceNormal, Vector3.up);


        //Wall Kick
        if (grounded)
        {
            wallKick.ResetWallKicks();
        }

        //Slope Detection
        if (grounded && verticalVelocity < 0f)
        {
            if (slopeAngle <= config.GetMaxSlopeAngle())
            {
                verticalVelocity = -5f;
                airMomentum = Vector3.zero;
            }
            else
            {
                verticalVelocity += config.GetGravity() * 0.5f * Time.deltaTime;
            }
        }
        else
        {
            verticalVelocity += config.GetGravity() * Time.deltaTime;
        }

        Vector3 gravityMove = Vector3.up * verticalVelocity;
        Vector3 move = Vector3.zero;

        if (!grounded)
        {
            // Airborne movement
            airMomentum *= airDrag;
            airMomentum += moveDir * 0.3f * config.GetSpeed(isSprinting);
            airMomentum = Vector3.ClampMagnitude(airMomentum, config.GetSpeed(isSprinting));
            move = airMomentum;
        }
        else
        {
            
            if (slopeAngle > controller.slopeLimit)
            {
                
                Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, surfaceNormal).normalized;
                move = slideDir * slideSpeed;
            }
            else
            {
                
                Vector3 slopeDir = Vector3.ProjectOnPlane(moveDir, surfaceNormal).normalized;
                float dotUp = Vector3.Dot(slopeDir, Vector3.up); 
                float dotForward = Vector3.Dot(slopeDir, config.transform.forward);

                if (slopeAngle > config.GetMaxSlopeAngle())
                {
                    if (dotForward > 0.3f)
                    {
                        
                        slopeDir = Vector3.zero;
                    }
                    else
                    {
                        
                        slopeDir *= 0.5f;
                    }
                }

                move = slopeDir * config.GetSpeed(isSprinting);

                
            }
        }

        controller.Move((move + gravityMove) * Time.deltaTime);
    }

    public void ResetMomentum()
    {
        verticalVelocity = -5f;
        airMomentum = Vector3.zero;
    }
}
