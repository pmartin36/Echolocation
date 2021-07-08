using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisMarker : MonoBehaviour
{
    private new CameraController camera;
	private Quaternion initial;

    public void Start()
    {
        var axisBody = GameManager.Instance.LevelManager.Player.transform;
        camera = GameManager.Instance.LevelManager.MainCamera;
        initial = Quaternion.FromToRotation(
            axisBody.forward, 
            camera.transform.forward
        );
    }

    public void SetRotation(Quaternion q)
    {
        transform.rotation = Quaternion.RotateTowards(initial, q, float.MaxValue);
    }

	private void Update()
	{
        SetRotation(camera.transform.rotation);
	}
}
