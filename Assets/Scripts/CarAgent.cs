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

	private Vector3 lastAgentVelocity;
	private NavMeshPath lastAgentPath;

	private bool paused = false;

	// Use this for initialization
	void Start () {
		
		var agent = GetComponent<NavMeshAgent>();
		var game = checkPoints.GetComponent<Game>();
		var target = game.checkPoints[checkPoint];
		
		agent.SetDestination(target.transform.position);
	}
	
	// Update is called once per frame
	void Update () {
		var agent = GetComponent<NavMeshAgent>();
		
		var game = checkPoints.GetComponent<Game>();

		if (game.runSimulation)
		{
			resume();
			UpdateSimulation(agent, game);
		}
		else
		{
			pause();
		}
	}
	
	void pause() 
	{
		
		if (paused)
		{
			return;
		}

		paused = true;
		
		var agent = GetComponent<NavMeshAgent>();

		lastAgentVelocity = agent.velocity;
		lastAgentPath = agent.path;
		agent.velocity = Vector3.zero;
		agent.ResetPath();
	}
     
	void resume() 
	{
		if (paused)
		{
			paused = false;
			var agent = GetComponent<NavMeshAgent>();
		
			agent.velocity = lastAgentVelocity;
			agent.SetPath(lastAgentPath);
		}		
	}

	private void UpdateSimulation(NavMeshAgent agent, Game game)
	{
		var deltaSpeed = (float) Random.Range(-1, 2);
		agent.speed = Mathf.Clamp(agent.speed + deltaSpeed / 10, 7, 13);

		if (DidAgentReachDestination(agent))
		{
			agent.ResetPath();
			checkPoint++;
			if (checkPoint >= game.checkPoints.Length)
			{
				checkPoint = 0;
			}

			var target = game.checkPoints[checkPoint];

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
