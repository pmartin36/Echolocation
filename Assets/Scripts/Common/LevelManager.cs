using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InputActions;

[RequireComponent(typeof(DarkInitializer))]
public class LevelManager : ContextManager
{
	public CameraController MainCamera { get; set; }
	public PlayerController Player { get; set; }

	public override void Awake() {
		base.Awake();
		Player = GameManager.FindObjectOfType<PlayerController>();
		MainCamera = GameManager.FindObjectOfType<CameraController>();
	}

	public override void Start()
    {
		
    }

	public override void HandleInput(PlayerActions p) {
		Player.HandleInput(p);
	}
}
