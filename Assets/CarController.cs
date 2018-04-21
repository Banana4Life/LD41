using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class CarController : MonoBehaviour
{

	public GameObject FrontLeft;
	public GameObject FrontRight;
	public GameObject RearLeft;
	public GameObject RearRight;

	public float EnginePower = 150;
	public float MaxTurn = 25;

	private WheelCollider frontLeftCollider;
	private WheelCollider frontRightCollider;
	private WheelCollider rearLeftCollider;
	private WheelCollider rearRightCollider;

	// Use this for initialization
	void Start ()
	{
		frontLeftCollider = FrontLeft.GetComponent<WheelCollider>();
		frontRightCollider = FrontRight.GetComponent<WheelCollider>();
		rearLeftCollider = RearLeft.GetComponent<WheelCollider>();
		rearRightCollider = RearRight.GetComponent<WheelCollider>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		var power = Input.GetAxis("Vertical") * EnginePower * Time.deltaTime;
		var turn = Input.GetAxis("Horizontal") * MaxTurn;
		var brake = Input.GetKey("space") ? GetComponent<Rigidbody>().mass * .1f : 0;

		frontLeftCollider.steerAngle = turn;
		frontRightCollider.steerAngle = turn;

		var leftAngle = FrontLeft.transform.localEulerAngles;
		FrontLeft.transform.localEulerAngles = new Vector3(leftAngle.x, turn - leftAngle.z, leftAngle.z);
		
		var rightAngle = FrontLeft.transform.localEulerAngles;
		FrontRight.transform.localEulerAngles = new Vector3(rightAngle.x, turn - rightAngle.z, rightAngle.z);
		
		FrontLeft.transform.Rotate(frontLeftCollider.rpm / 60 * 360 * Time.deltaTime, 0, 0);
		FrontRight.transform.Rotate(frontRightCollider.rpm / 60 * 360 * Time.deltaTime, 0, 0);
		RearLeft.transform.Rotate(rearLeftCollider.rpm / 60 * 360 * Time.deltaTime, 0, 0);
		RearRight.transform.Rotate(rearRightCollider.rpm / 60 * 360 * Time.deltaTime, 0, 0);

		if (brake > 0)
		{
			frontLeftCollider.brakeTorque = brake;
			frontRightCollider.brakeTorque = brake;
			rearLeftCollider.brakeTorque = brake;
			rearRightCollider.brakeTorque = brake;
			frontLeftCollider.motorTorque = 0;
			frontRightCollider.motorTorque = 0;
		}
		else
		{
			frontLeftCollider.brakeTorque = 0;
			frontRightCollider.brakeTorque = 0;
			rearLeftCollider.brakeTorque = 0;
			rearRightCollider.brakeTorque = 0;
			frontLeftCollider.motorTorque = power;
			frontRightCollider.motorTorque = power;
		}
	}
}
