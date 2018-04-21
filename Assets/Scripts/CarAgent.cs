using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CarAgent : MonoBehaviour
{
	public GameObject checkPoints;
	public int checkPoint;
	// Use this for initialization
	void Start () {
		
		var agent = GetComponent<NavMeshAgent>();
		var checkPts = checkPoints.GetComponent<CheckPoints>();
		var target = checkPts.checkPoints[checkPoint];
		
		agent.SetDestination(target.transform.position);
	}
	
	// Update is called once per frame
	void Update () {
		var agent = GetComponent<NavMeshAgent>();
		var checkPts = checkPoints.GetComponent<CheckPoints>();


		var deltaSpeed = (float)Random.Range(-1, 2);
		agent.speed = Mathf.Clamp(agent.speed + deltaSpeed / 10, 7, 13);

		if (DidAgentReachDestination(agent))
		{
			agent.ResetPath();
			checkPoint++;
			if (checkPoint >= checkPts.checkPoints.Length)
			{
				checkPoint = 0;
			}
			var target = checkPts.checkPoints[checkPoint];
			
			agent.SetDestination(target.transform.position);
		}
		
		
		NavMeshHit navMeshHit;
		if (agent.FindClosestEdge(out navMeshHit))
		{
			if (navMeshHit.mask == 5)
			{
				agent.speed = 7;
			}
		}
		
	}
	
	public static bool DidAgentReachDestination(NavMeshAgent agent)
	{
		var distance = Vector3.Distance(agent.gameObject.transform.position, agent.destination);
		return distance <= agent.stoppingDistance;
	}
}
