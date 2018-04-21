﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CarAgent : MonoBehaviour
{
	public GameObject GameObj;
	private Game game;
	public int checkPoint;

	private Vector3 lastAgentVelocity;
	private NavMeshPath lastAgentPath;

	private bool paused;

	public bool playerControlled;

	// Use this for initialization
	void Start () {
		
		var agent = GetComponent<NavMeshAgent>();
		game = GameObj.GetComponent<Game>();
		var target = game.checkPoints[checkPoint];
		
		agent.SetDestination(target.transform.position);
	}
	
	// Update is called once per frame
	void Update () {
		var agent = GetComponent<NavMeshAgent>();
		
		if (playerControlled)
		{
			UpdatePlayerInput(agent);
		}
		if (game.runSimulation)
		{
			resume();
			UpdateSimulation(agent);
		}
		else
		{
			pause();
		}
	}

	private void UpdatePlayerInput(NavMeshAgent agent)
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			var point = hit.point + hit.normal / 10f;
			
			if (!game.runSimulation && Input.GetMouseButtonUp(0))
			{
				game.queued.Enqueue(point);
			}

			Vector3 last = transform.position;
			if (!agent.isStopped)
			{
				Debug.DrawLine(agent.destination, last, Color.cyan);
				last = agent.destination;
			}
			foreach (var v3 in game.queued)
			{
				var next = new Vector3(v3.x, v3.y, v3.z);
				Debug.DrawLine(next, last, Color.red);
				last = next;
			}

			Debug.DrawLine(agent.transform.position, point, Color.black);

		}
		
		if (Input.GetAxis("Submit") > 0)
		{
			game.runSimulation = true;
				
			if (agent.isStopped)
			{
				agent.ResetPath();
				agent.SetDestination(game.queued.Dequeue());	
			}
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

	private void  UpdateSimulation(NavMeshAgent agent)
	{
		var deltaSpeed = (float) Random.Range(-1, 2);
		agent.speed = Mathf.Clamp(agent.speed + deltaSpeed / 10, 7, 13);

		if (playerControlled)
		{
			Camera.main.transform.parent.transform.position = transform.position;
			Camera.main.transform.parent.eulerAngles = transform.eulerAngles;
			
			
			if (DidAgentReachDestination(agent.gameObject.transform.position, agent.destination, 3f))
			{
				if (game.queued.Count > 0)
				{
					agent.SetDestination(game.queued.Dequeue());
				}
				else
				{
					game.runSimulation = false;
				}
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

		var ray = new Ray(agent.transform.position, agent.transform.up * -1);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			//agent.transform.LookAt(agent.transform.position + agent.velocity);

			agent.transform.rotation = Quaternion.LookRotation(agent.desiredVelocity, hit.normal);
			
			//agent.transform.up = hit.normal;
			//var forward = new Vector3(euler.x, agent.transform.eulerAngles.y, euler.z);

			//agent.transform.Rotate(forward);
			
			//agent.transform.eulerAngles = forward;

			Debug.DrawLine(hit.point, hit.point + hit.normal * 10, Color.yellow);
		}
	}

	public static bool DidAgentReachDestination(Vector3 pos, Vector3 dest, float targetDistance)
	{
		var distance = Vector3.SqrMagnitude(pos - dest);
		return distance <= targetDistance * targetDistance;
	}
}
