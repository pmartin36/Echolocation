using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InputActions;

public class PlayerController : MonoBehaviour, IInputReceiver
{
	private CharacterController controller;
	private Vector3 playerVelocity;
	private bool groundedPlayer;

	public float playerSpeed = 2.0f;

	public float jumpHeight = 1.0f;
	public float gravityValue = -9.81f;

	public float MouseSensitivity { get; set; } = 0.2f;

	[SerializeField]
	private Rigidbody Shoulder;
	public Cane Equipment;

	private MinMax verticalAngleMinMax;

	private void Start()
	{
		controller = gameObject.GetComponent<CharacterController>();
		(GameManager.Instance.ContextManager as LevelManager).Player = this;
		Equipment.Bind(this.transform);
		verticalAngleMinMax = new MinMax(40, 170);
	}

	void Update()
	{
		groundedPlayer = controller.isGrounded;
		if (groundedPlayer && playerVelocity.y < 0)
		{
			playerVelocity.y = 0f;
		}

		// Changes the height position of the player..
		if (Input.GetButtonDown("Jump") && groundedPlayer)
		{
			playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
		}

		playerVelocity.y += gravityValue * Time.deltaTime;
		controller.Move(playerVelocity * Time.deltaTime);
	}

	public void HandleInput(PlayerActions p)
	{
		Vector2 moveInput = p.Movement.ReadValue<Vector2>();
		Vector3 move =  moveInput.y * transform.forward + moveInput.x * transform.right;
		Vector3 diff = move * Time.deltaTime * playerSpeed;
		//Equipment.TryMove(diff);
		controller.Move(diff);

		Vector2 mouseDelta = p.MouseDelta.ReadValue<Vector2>() * MouseSensitivity;
		Vector2 gamePadPosition = p.RightStick.ReadValue<Vector2>();
		Vector2 m = mouseDelta + gamePadPosition;

		transform.eulerAngles += Vector3.up * m.x;
		RotateShoulderVertical(-m.y);
	}

	public bool RotateShoulderVertical(float angle)
	{
		float currentAngle = Vector3.SignedAngle(transform.up, Shoulder.transform.forward, transform.right);
		angle = verticalAngleMinMax.ClampAngle(currentAngle + angle) - currentAngle;
		Shoulder.transform.RotateAround(transform.position, transform.right, angle);
		return true;
	}
}
