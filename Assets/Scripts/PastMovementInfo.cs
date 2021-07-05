using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct PastMovementInfo
{
	public float Alpha { get; set; }
	public float OneMinusAlpha => 1 - Alpha;

	public Vector3 LastPosition { get; set; }
	public Vector3 LastPositionAverage { get; set; }

	public Quaternion LastRotation { get; set; }
	public Quaternion LastRotationAverage { get; set; }

	public PastMovementInfo(float a, Transform t)
	{
		Alpha = a;
		LastPosition =  t.position;
		LastPositionAverage = Vector3.zero;
		LastRotation = t.rotation;
		LastRotationAverage = Quaternion.identity;
	}

	public void Update(Transform t)
	{
		LastPositionAverage = (t.position - LastPosition) * Alpha + LastPosition * OneMinusAlpha;
		LastPosition = t.position;

		Quaternion diff = t.rotation * Quaternion.Inverse(LastRotation);
		LastRotationAverage = Quaternion.Lerp(LastRotationAverage, diff, Alpha);
		LastRotation = t.rotation;
	}
}
