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

	private bool paused;

	public bool playerControlled;

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

		if (playerControlled)
		{
			UpdatePlayerInput(agent, game);
		}
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

	private void UpdatePlayerInput(NavMeshAgent agent, Game game)
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			var point = hit.point;
			
			if (Input.GetMouseButtonUp(0))
			{
				agent.ResetPath();
				agent.SetDestination(point);
				game.runSimulation = true;
			}
			Debug.DrawLine(agent.transform.position, point, Color.black);
			Debug.DrawLine(agent.destination, transform.position, Color.red);

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
			if (!playerControlled)
			{
				agent.SetPath(lastAgentPath);
			}
		}		
	}

	private void  UpdateSimulation(NavMeshAgent agent, Game game)
	{
		var deltaSpeed = (float) Random.Range(-1, 2);
		agent.speed = Mathf.Clamp(agent.speed + deltaSpeed / 10, 7, 13);

		if (playerControlled)
		{
			Camera.main.transform.parent.transform.position = transform.position;
			Camera.main.transform.parent.eulerAngles = transform.eulerAngles;
			
			
			if (DidAgentReachDestination(agent.gameObject.transform.position, agent.destination, 3f))
			{
				Debug.Log("Stopping");
				game.runSimulation = false;
			}
			 
		}
		else
		{
			if (DidAgentReachDestination(agent.gameObject.transform.position, agent.destination, agent.stoppingDistance))
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

	public static bool DidAgentReachDestination(Vector3 pos, Vector3 dest, float targetDistance)
	{
		var distance = Vector3.SqrMagnitude(pos - dest);
		Debug.Log(pos + " " + dest);
		Debug.Log(distance);
		return distance <= targetDistance * targetDistance;
	}
}
