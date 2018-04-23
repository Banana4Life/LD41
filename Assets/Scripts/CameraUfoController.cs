using UnityEngine;

public class CameraUfoController : MonoBehaviour
{

	public Vector3 TargetPosition;
	public Quaternion TargetRotation;
	private Vector3? startPos;
	private Quaternion? startRot;
	
	public float LerpDuration = 3;

	private float lerpBegin;

	public void SetTarget(Vector3 position, Quaternion rotation)
	{
		if ((position - TargetPosition).sqrMagnitude > .001 || Mathf.Abs(Quaternion.Dot(rotation, TargetRotation)) < .9)
		{
			TargetPosition = position;
			TargetRotation = rotation;
			startPos = transform.position;
			startRot = transform.rotation;
			lerpBegin = Time.time;
		}
	}

	public void SetTarget(Transform transform)
	{
		SetTarget(transform.position, transform.rotation);
	}
	
	void Update () {
		var progress = (Time.time - lerpBegin) / LerpDuration;
		if (progress >= 1)
		{
			transform.position = TargetPosition;
			transform.rotation = TargetRotation;
		}
		else
		{
			if (startPos != null)
			{
				transform.position = Vector3.Lerp(startPos.Value, TargetPosition, progress);
			}
			if (startRot != null)
			{
				transform.rotation = Quaternion.Lerp(startRot.Value, TargetRotation, progress);
			}
		}
	}
}
