using UnityEngine;

public class PlayerMovement
{
    private readonly CharacterController controller;
    private readonly PlayerController config;

    public PlayerMovement(CharacterController controller, PlayerController config)
    {
        this.controller = controller;
        this.config = config;
    }

    public void Move(Vector2 input, bool isSprinting)
    {
        if (input.sqrMagnitude < 0.01f)
            return;

        float speed = config.GetSpeed(isSprinting);
        Vector3 move = (config.transform.right * input.x + config.transform.forward * input.y).normalized * speed;

        controller.Move(move * Time.deltaTime);
    }
}
