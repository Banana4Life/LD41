using UnityEngine;

public class CameraController : MonoBehaviour
{

	public Game Game;
	public bool Planning;
	public float LerpDuration = 5;

	private bool previousPlanningState;
	private Vector3 targetPosition;
	private Quaternion targetRotation;

	private Vector3 startPos;
	private Quaternion startRot;

	private float lerpBegin = -1;
	
	// Update is called once per frame
	void Update ()
	{
		if (Planning)
		{
			targetPosition = Game.camOffset2;
			targetRotation = Quaternion.Euler(Game.camRot2);
		}
		else
		{
			targetPosition = Game.camOffset1;
			targetRotation = Quaternion.Euler(Game.camRot1);
		}

		if (previousPlanningState != Planning)
		{
			previousPlanningState = Planning;
			lerpBegin = Time.time;
			startPos = transform.localPosition;
			startRot = transform.localRotation;
		}

		var progess = (Time.time - lerpBegin) / LerpDuration;
		if (progess >= 1)
		{
			transform.localPosition = targetPosition;
			transform.localRotation = targetRotation;
		}
		else
		{
			transform.localPosition = Vector3.Lerp(startPos, targetPosition, progess);			
			transform.localRotation = Quaternion.Lerp(startRot, targetRotation, progess);			
		}
	}
}
