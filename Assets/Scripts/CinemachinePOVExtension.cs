using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachinePOVExtension : CinemachineExtension
{
	//private InputActions input
	private Vector3 startingRotation;

	protected void Awake()
	{
		base.Awake();
		startingRotation = transform.localRotation.eulerAngles;
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		//if (vcam.Follow)
		//{
		//	if (stage == CinemachineCore.Stage.Aim)
		//	{
		//		Vector3 deltaInput = Vector3.zero;// InputManagerEntry.GetMouseDelta();
		//		startingRotation += deltaInput * Time.deltaTime;
		//		state.RawOrientation = Quaternion.Euler(startingRotation.y, startingRotation.x, 0f);
		//	}
		//}
	}
}
