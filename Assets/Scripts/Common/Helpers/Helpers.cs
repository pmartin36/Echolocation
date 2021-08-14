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

	public static Vector3 ScaleInline(this Vector3 x, Vector3 y) {
		return new Vector3(x.x * y.x, x.y * y.y, x.z * y.z);
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

public struct Rotation
{
	private Vector3 PitchYawRoll;
	public float Yaw => PitchYawRoll.y;
	public float Pitch => PitchYawRoll.x;
	public float Roll => PitchYawRoll.z;

	public Rotation(Quaternion q)
	{
		PitchYawRoll = q.GetPitchYawRoll();
	}
}

public static class QuaternionExtensions
{
	public static Vector3 GetPitchYawRoll(this Quaternion q)
	{
		return new Vector3(q.GetPitch(), q.GetYaw(), q.GetRoll());
	}

	public static float GetPitch(this Quaternion q)
	{
		return q.GetK().GetPitch();
	}

	public static float GetYaw(this Quaternion q)
	{
		return q.GetK().GetYaw();
	}

	public static float GetRoll(this Quaternion q)
	{
		// This is M12 * M22 of rotation matrix
		float xx = q.x * q.x;
		float xy = q.x * q.y;
		float zz = q.z * q.z;
		float wz = q.w * q.z;
		return (float)Math.Atan2(2f * (xy - wz), 1f - 2f * (xx + zz));
	}

	public static Vector3 GetK(this Quaternion q)
	{
		float xz = q.x * q.z;
		float wy = q.w * q.y;
		float yz = q.y * q.z;
		float wx = q.w * q.x;
		float xx = q.x * q.x;
		float yy = q.y * q.y;
		return new Vector3(
			2f * (xz - wy),
			2f * (yz + wx),
			1f - 2f * (xx + yy));
	}
}

public static class Vector3Extensions
{
	public static float GetPitch(this Vector3 v)
	{
		return (float)-Math.Atan2(v.y, Math.Sqrt(v.x * v.x + v.z * v.z));
	}

	public static float GetYaw(this Vector3 v)
	{
		return (float)-Math.Atan2(v.x, v.z);
	}
}