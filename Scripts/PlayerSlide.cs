using UnityEngine;

public class PlayerSlide
{
    private readonly CharacterController controller;
    private readonly PlayerController config;
    private readonly PlayerCrouch crouch;

    private bool isSliding = false; // Checks if the player is sliding or not
    private Vector3 slideDirection; // Locks the player into a single direction
    private float currentSpeed; // current slide speeds

    // Configurable parameters
    private readonly float slideStartBoost = 1.3f; // multiplier of sprint speed
    private readonly float slideDecayRate = 10f; // how fast it slows down
    private readonly float slideDuration = 1.2f; // seconds
    private readonly float minSlideSpeed = 1.5f; // auto end when slower than this speed

    private float slideTimer;

    public bool IsSliding => isSliding;

    //Contsructor
    public PlayerSlide(CharacterController controller, PlayerController config)
    {
        this.controller = controller;
        this.config = config;
        this.crouch = new PlayerCrouch(controller, config);
    }

    public void StartSlide()
    {
        if (isSliding || !config.IsGrounded(out _) || !config.IsSprinting())
            return;

        isSliding = true;
        slideTimer = 0f;

        //Slide only in current facing direction
        slideDirection = config.transform.forward;

        // Initial speed boost
        currentSpeed = config.sprintSpeed * slideStartBoost;

        // Force crouch while sliding
        crouch.SetCrouching(true);
    }

    public void Update()
    {
        if (!isSliding)
            return;
        
        slideTimer += Time.deltaTime;

        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, slideDecayRate * Time.deltaTime);

        Vector3 move = slideDirection * currentSpeed;
        move.y += config.GetGravity() * 0.1f * Time.deltaTime;

        controller.Move(move * Time.deltaTime);

        if (currentSpeed <= minSlideSpeed || slideTimer >= slideDuration || !config.IsGrounded(out _))
        {
            StopSlide();
        }
    }

    public void StopSlide()
    {
        if (!isSliding) return;
        
        isSliding = false;

        crouch.SetCrouching(true);
    }
}