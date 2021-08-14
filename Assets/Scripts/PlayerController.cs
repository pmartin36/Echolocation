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
	private Transform Shoulder;
	public Equipment Equipment;

	private Animator anim;

	private MinMax verticalAngleMinMax;

	private Vector3 LastFramePosition;
	private Vector3 Speed;
	private float HorizontalSpeed => Speed.ScaleInline(new Vector3(1, 0, 1)).magnitude;
	private float VerticalSpeed => Speed.ScaleInline(Vector3.up).magnitude;

	private LayerMask walkableLayerMask;

	private void Start()
	{
		controller = GetComponent<CharacterController>();
		anim = GetComponent<Animator>();

		(GameManager.Instance.ContextManager as LevelManager).Player = this;

		Equipment.Bind(this.transform);
		verticalAngleMinMax = new MinMax(40, 170);

		walkableLayerMask = 1 << LayerMask.NameToLayer("Default");

		LastFramePosition = transform.position;
		Speed = Vector3.zero;
	}

	void Update()
	{
		groundedPlayer = controller.isGrounded;
		if (groundedPlayer && playerVelocity.y < 0)
		{
			playerVelocity.y = 0f;
		}

		if (Input.GetButtonDown("Jump") && groundedPlayer)
		{
			playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
		}

		playerVelocity.y += gravityValue * Time.deltaTime;
		controller.Move(playerVelocity * Time.deltaTime);

		Speed = (LastFramePosition - transform.position) / Time.deltaTime * 0.3f
				+ Speed * 0.7f;
		anim.SetFloat("Speed", HorizontalSpeed / playerSpeed);
		LastFramePosition = transform.position;
	}

	public void HandleInput(PlayerActions p)
	{
		Vector2 moveInput = p.Movement.ReadValue<Vector2>();
		Vector3 move =  moveInput.y * transform.forward + moveInput.x * transform.right;
		if (move.sqrMagnitude > 1) move = move.normalized;
		Vector3 diff = move * Time.deltaTime * playerSpeed;
		controller.Move(diff);

		Vector2 mouseDelta = p.MouseDelta.ReadValue<Vector2>() * MouseSensitivity;
		Vector2 gamePadPosition = p.RightStick.ReadValue<Vector2>();
		Vector2 m = mouseDelta + gamePadPosition;

		transform.eulerAngles += Vector3.up * m.x;
		RotateShoulderVertical(-m.y);

		Equipment.HandleInput(p);
	}

	public bool RotateShoulderVertical(float angle)
	{
		float currentAngle = Vector3.SignedAngle(transform.up, Shoulder.transform.forward, transform.right);
		angle = verticalAngleMinMax.ClampAngle(currentAngle + angle) - currentAngle;
		Shoulder.transform.RotateAround(transform.position, transform.right, angle);
		return true;
	}

	public void Step(float offset)
	{
		float echoSize = HorizontalSpeed / playerSpeed;
		bool detailedEcho = echoSize > 0.75f;

		RaycastHit hit;
		if (Physics.Raycast(transform.position + transform.right * offset * 0.35f, Vector3.down, out hit, controller.height / 2f + 0.1f, walkableLayerMask, QueryTriggerInteraction.Ignore))
		{
			Echo e = PoolManager.Instance.Next<Echo>("Echo");
			e.Init(hit, echoSize, detailedEcho);
		}
	}
}
