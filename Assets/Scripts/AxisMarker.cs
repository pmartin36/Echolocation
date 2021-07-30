using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisMarker : MonoBehaviour
{
    private new CameraController camera;
	private Vector3 initial;

	[SerializeField]
	private Transform pitchMarker;
	[SerializeField]
	private Transform yawMarker;

    public void Start()
    {
        camera = GameManager.Instance.LevelManager.MainCamera;
        initial = Vector3.forward;
    }

    public void SetRotation(Vector3 lookAt)
    {
		// transform.rotation * rotation -- local
		// rotation * transform.rotation -- global
		//var q = Quaternion.FromToRotation(initial, lookAt);
		//var r = new Rotation(q);
		//transform.rotation = Quaternion.AngleAxis(r.Pitch * Mathf.Rad2Deg, Vector3.up) * transform.rotation * Quaternion.AngleAxis(r.Yaw * Mathf.Rad2Deg, Vector3.right);
		//initial = camera.transform.forward;

		var q = Quaternion.FromToRotation(initial, lookAt);
		var r = new Rotation(q);
		pitchMarker.rotation = Quaternion.AngleAxis(r.Pitch * Mathf.Rad2Deg, Vector3.left);
		yawMarker.rotation = Quaternion.AngleAxis(r.Yaw * Mathf.Rad2Deg, Vector3.up);
	}

	private void Update()
	{
		SetRotation(camera.transform.forward);
	}
}
