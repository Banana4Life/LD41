using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

	public Vector3 Offset;
	public Vector3 Angle;
	public GameObject Target;
	
	// Update is called once per frame
	void Update ()
	{
		transform.rotation = Target.transform.rotation;
		transform.position = Target.transform.position;
		transform.position += Offset;
	}
}
