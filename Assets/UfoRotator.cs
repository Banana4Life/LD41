using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UfoRotator : MonoBehaviour
{
	public float Scaler = 1;
	
	void Update ()
	{
		var agent = GetComponentInParent<NavMeshAgent>();
		if (agent)
		{
			transform.RotateAround(Vector3.up, agent.velocity.magnitude * Scaler * Time.deltaTime);
		}
	}
}
