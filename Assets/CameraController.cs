using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

	public Game Game;
	public bool Planning = false;
	
	// Update is called once per frame
	void Update ()
	{
		if (Planning)
		{
            transform.localPosition = Game.camOffset2;
            transform.localEulerAngles = Game.camRot2;
		}
		else
		{
			transform.localPosition = Game.camOffset1;
			transform.localEulerAngles = Game.camRot1;
		}
	}
}
