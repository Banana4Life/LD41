using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CarAgent : MonoBehaviour
{
	public GameObject GameObj;
	private Game game;
	private NavMeshAgent agent;
	public int checkPoint;

	private Vector3 lastAgentVelocity;
	private NavMeshPath lastAgentPath;

	private bool paused;

	public bool playerControlled;

	// Use this for initialization
	void Awake() {
		agent = GetComponent<NavMeshAgent>();
		game = GameObj.GetComponent<Game>();
	}

	void Start()
	{
		var target = game.checkPoints[checkPoint];
		if (!playerControlled)
		{
			agent.SetDestination(target.transform.position);
		}
	}

	// Update is called once per frame
	void Update () {
		var agent = GetComponent<NavMeshAgent>();
		
		if (playerControlled)
		{
			UpdatePlayerInput(agent);
						
			if (game.runSimulation)
			{
				Camera.main.transform.localPosition = game.camOffset1;
				Camera.main.transform.localEulerAngles = game.camRot1;
			}
			else
			{
				Camera.main.transform.localPosition = game.camOffset2;
				Camera.main.transform.localEulerAngles = game.camRot2;
			}
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
		if (Physics.Raycast(ray, out hit, 1 << 8)) // Only hit Track
		{
			var point = hit.point + hit.normal / 10f;
			
			if (!game.runSimulation)
			{
				if (Input.GetMouseButtonUp(0))
				{
					game.queued.AddLast(point);
					var total = GetPathLength(agent, game.queued);
					Debug.LogWarning("Path cost: " + total);
				}
				else if (Input.GetMouseButtonUp(1))
				{
					game.queued.RemoveLast();
				}
				
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
			if (game.queued.Count == 0)
			{
				return;
			}
			
			game.runSimulation = true;
				
			
			if (agent.isStopped)
			{
				agent.ResetPath();
				
				agent.SetDestination(game.queued.First.Value);
				game.queued.RemoveFirst();
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

	private void UpdateSimulation(NavMeshAgent agent)
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
					agent.SetDestination(game.queued.First.Value);
					game.queued.RemoveFirst();
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

		/*
		if (game.runSimulation)
		{
			var ray = new Ray(agent.transform.position, agent.transform.up * -1);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				//agent.transform.LookAt(agent.transform.position + agent.velocity);
		
				agent.transform.rotation = Quaternion.LookRotation(lastAgentVelocity, hit.normal);
				lastAgentVelocity = agent.velocity;
				//agent.transform.up = hit.normal;
				//var forward = new Vector3(euler.x, agent.transform.eulerAngles.y, euler.z);

				//agent.transform.Rotate(forward);
			
				//agent.transform.eulerAngles = forward;

				Debug.DrawLine(hit.point, hit.point + hit.normal * 10, Color.yellow);
			}	
		}
		*/
		
		
	}

	private static float GetPathLength(NavMeshAgent agent, IEnumerable<Vector3> plannedPath)
	{
		float length;
		Vector3 last;
		if (agent.hasPath)
		{
			length = GetPathLength(agent.path, agent.transform.position);
			last = agent.destination;
		}
		else
		{
			length = 0f;
			last = agent.transform.position;
		}
		
		var path = new NavMeshPath();
		foreach (var pos in plannedPath)
		{
			NavMesh.CalculatePath(last, pos, NavMesh.AllAreas, path);
			length += GetPathLength(path, last);
			last = pos;
		}

		return length;
	}

	private static float GetPathLength(NavMeshPath path, Vector3 from)
	{
		var length = 0f;
		var lastCorner = from;
		foreach (var corner in path.corners)
		{
			length += (corner - lastCorner).magnitude;
			lastCorner = corner;
		}
		path.ClearCorners();
		return length;
	}

	public static bool DidAgentReachDestination(Vector3 pos, Vector3 dest, float targetDistance)
	{
		var distance = Vector3.SqrMagnitude(pos - dest);
		return distance <= targetDistance * targetDistance;
	}
}
