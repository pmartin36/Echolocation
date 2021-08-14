using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightGun : Equipment
{
	private Vector3 MuzzlePosition => transform.position + transform.forward;

	private float cooldown = 1f;
	private float lastFireTime = -1f;

	public override void HandleInput(InputActions.PlayerActions p)
	{
		base.HandleInput(p);
		if (p.Fire.triggered && Time.time - lastFireTime > cooldown)
		{
			HighlightBullet bullet = PoolManager.Instance.Next<HighlightBullet>("HighlightBullet");
			bullet.Init(MuzzlePosition, Quaternion.AngleAxis(2f, Vector3.up) * transform.forward);
		}

		if (p.AltFire.triggered)
		{

		}
	}
}
