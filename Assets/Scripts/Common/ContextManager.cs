using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InputActions;

public abstract class ContextManager : MonoBehaviour, IInputReceiver
{
	protected InputActions InputActions { get; set; }

	public virtual void Awake() {
		GameManager.Instance.ContextManager = this;
		InputActions = new InputActions();
		InputActions.Enable();
	}

	public virtual void Start() {

	}

	public virtual void Update()
	{
		HandleInput(InputActions.Player);
	}

	public abstract void HandleInput(PlayerActions p);
}
