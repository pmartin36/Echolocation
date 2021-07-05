using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InputActions;

[RequireComponent(typeof(DarkInitializer))]
public class LevelManager : ContextManager
{
	private Camera main;
	public PlayerController Player { get; set; }

	public override void Awake() {
		base.Awake();
		main = Camera.main;
	}

	public override void Start()
    {
		
    }

	public override void HandleInput(PlayerActions p) {
		Player.HandleInput(p);
	}
}
