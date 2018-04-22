using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

	public Game Game;
	public bool Planning;
	public float LerpDuration = 5;

	private bool previousPlanningState;
	private Vector3 targetEulerAngles;
	private Vector3 targetPosition;

	private float lerpBegin = -1;
	
	// Update is called once per frame
	void Update ()
	{
		if (Planning)
		{
			targetPosition = Game.camOffset2;
			targetEulerAngles = Game.camRot2;
		}
		else
		{
			targetPosition = Game.camOffset1;
			targetEulerAngles = Game.camRot1;
		}

		if (previousPlanningState != Planning)
		{
			previousPlanningState = Planning;
			lerpBegin = Time.time;	
		}

		var progess = (Time.time - lerpBegin) / LerpDuration;
		if (progess >= 1)
		{
			transform.localPosition = targetPosition;
			transform.localEulerAngles = targetEulerAngles;
		}
		else
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, progess);			
			transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(targetEulerAngles), progess);			
		}
	}
}
