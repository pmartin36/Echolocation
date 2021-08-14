using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InputActions;

public abstract class Equipment : MonoBehaviour, IInputReceiver
{
	protected Transform holder;
	protected LayerMask interactableLayerMask;
	public virtual void Start()
	{
		interactableLayerMask = ~(1 >> LayerMask.NameToLayer("Equipment") | 1 >> LayerMask.NameToLayer("Player"));
	}

	public virtual void HandleInput(PlayerActions p)
	{
		
	}

	public virtual void Bind(Transform t)
	{
		holder = t;
	}

}
