using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class VectorHelpers {
	public static Vector2 SnapToAxis(this Vector2 v) {
		float x = Mathf.Abs(v.x);
		float y = Mathf.Abs(v.y);
		if(x > y) {
			v *= Vector2.right;
		}
		else {
			v *= Vector2.up;
		}
		return v;
	}

	public static Vector3 Rotate(this Vector3 v, float angle) {
		return Quaternion.Euler(0,0,angle) * v;
	}

	public static Vector2 Rotate(this Vector2 v, float angle) {
		return Quaternion.Euler(0, 0, angle) * v;
	}

	public static Vector2 RotateAround(this Vector2 v, float angle, Vector2 position) {
		Vector2 vr = v - position;
		vr = vr.Rotate(angle);
		vr += position;
		return vr;
	}

	public static Vector2 Abs(this Vector2 v) {
		return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
	}

	public static Vector3 Abs(this Vector3 v) {
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	}

	public static float Clamp(this float x, MinMax m)
	{
		return m.Clamp(x);
	}
}

public static class GeneralHelpers {
	public static void Destroy(this GameObject g, float time = 0) {
		UnityEngine.Object.Destroy(g, time);
	}
}

public struct MinMax
{
	public float Min { get; set; }
	public float Max { get; set; }
	public MinMax(float min, float max)
	{
		Min = min;
		Max = max;
	}
	public float Clamp(float x)
	{
		return Mathf.Clamp(x, Min, Max);
	}

	public float ClampAngle(float x)
	{
		float sign = Mathf.Sign(x);
		while (Math.Abs(x) > 180)
		{
			x -= sign * 360;
		}
		return Clamp(x);
	}
}