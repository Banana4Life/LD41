using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using JetBrains.Annotations;
using UnityEditor;
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

	private float delta = 0f;

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
		var meshy = new Mesh();
		List<Vector3> verts = new List<Vector3>();
		List<int> triangles = new List<int>();
		
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

			Debug.DrawLine(agent.transform.position, point, Color.black);

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

			drawArc(last, next, verts, triangles);
			Debug.DrawLine(next, last, Color.red);
			last = next;
		}
		
		
		var mf = game.trailmesh.GetComponent<MeshFilter>();
		mf.mesh = meshy;
		meshy.vertices = verts.ToArray();
		meshy.triangles = triangles.ToArray();
		
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

	private void drawArc(Vector3 last, Vector3 next, List<Vector3> verts, List<int> triangles)
	{
		var lastP = last;

		for (var l = 0f; l <= 1.2; l += 0.1f)
		{
			var nextP = SampleParabola(last, next, 4, l);
			//Debug.DrawLine(lastP, nextP, Color.magenta);
			lastP = nextP;
					
			verts.Add(nextP + Vector3.left / 20);
			verts.Add(nextP + Vector3.right / 20);
			var i = verts.Count;
			if (i > 2)
			{
				triangles.Add(i-4);
				triangles.Add(i-3);
				triangles.Add(i-2);
				
				triangles.Add(i-1);
				triangles.Add(i-2);
				triangles.Add(i-3);

				triangles.Add(i-2);
				triangles.Add(i-3);
				triangles.Add(i-4);

				triangles.Add(i-3);
				triangles.Add(i-2);
				triangles.Add(i-1);
			}
		}
	}
	
	Vector3 SampleParabola ( Vector3 start, Vector3 end, float height, float pCent ) {
		if ( Mathf.Abs( start.y - end.y ) < 0.1f ) {
			//start and end are roughly level, pretend they are - simpler solution with less steps
			Vector3 travelDirection = end - start;
			Vector3 result = start + pCent * travelDirection;
			result.y += Mathf.Sin( pCent * Mathf.PI ) * height;
			return result;
		} else {
			//start and end are not level, gets more complicated
			Vector3 travelDirection = end - start;
			Vector3 levelDirecteion = end - new Vector3( start.x, end.y, start.z );
			Vector3 right = Vector3.Cross( travelDirection, levelDirecteion );
			Vector3 up = Vector3.Cross( right, travelDirection );
			if ( end.y > start.y ) up = -up;
			Vector3 result = start + pCent * travelDirection;
			result += ( Mathf.Sin( pCent * Mathf.PI ) * height ) * up.normalized;
			return result;
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
		agent.speed = Mathf.Clamp(agent.speed + deltaSpeed / 10, 20, 30);

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
