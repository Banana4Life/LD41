using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public class CarAgent : MonoBehaviour
{
	public GameObject checkPoints;
	public int checkPoint;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		var agent = GetComponent<NavMeshAgent>();
		var checkPts = checkPoints.GetComponent<CheckPoints>();
		
		
		if (DidAgentReachDestination(agent))
		{
			agent.ResetPath();
			checkPoint++;
			if (checkPoint >= checkPts.checkPoints.Length)
			{
				checkPoint = 0;
			}
		}
		
		var target = checkPts.checkPoints[checkPoint];
		
		agent.SetDestination(target.transform.position);
		
		NavMeshHit navMeshHit;
		Debug.Log("...");
		if(agent.SamplePathPosition(NavMesh.AllAreas, 0f, out navMeshHit)) {
			Debug.Log(navMeshHit.mask);
		}
		
	}
	
	public static bool DidAgentReachDestination(NavMeshAgent agent)
	{
		var distance = Vector3.Distance(agent.gameObject.transform.position, agent.destination);
		return distance <= agent.stoppingDistance;
	}
}
