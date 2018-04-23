﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CarAgent : MonoBehaviour
{
    public GameObject GameObj;
    private Game game;
    private NavMeshAgent agent;
    public int checkPoint;
    public Vector3 targetPoint;
    public bool IsFinished;

    private Vector3 lastAgentVelocity;
    private NavMeshPath lastAgentPath;

    private bool paused;

    private bool updateAgent = true;
    public bool playerControlled;
    public bool isPlayer;

    private float delta = 0f;
    public int Round = 0;
    public float placePoints;
    
    private int maxSpeed = 30;

    private float overDrive = 0f;
    public int FinishedAt;

    // Use this for initialization
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        game = GameObj.GetComponent<Game>();
        targetPoint = transform.position;
    }

    void Start()
    {
        game.placing.Add(this);
        checkPoint = game.StartCheckpoint;
        NextTarget();

        FinishedAt = 11; // shit code - no of vehicles + 1
    }

    // Update is called once per frame
    void Update()
    {
        if (game.runSimulation)
        {
            overDrive -= Time.deltaTime;
        }
        if (overDrive < 0)
        {
            agent.acceleration = 35f;
            maxSpeed = game.maxSpeed;
        }
        if (playerControlled)
        {
            UpdatePlayerInput();
            game.ghostCar.active = true;
        }
        if (isPlayer)
        {
            UpdatePlayerCam();
        }
        DrawPath();
        
        if (game.runSimulation)
        {
            resume();
            UpdateSimulation();
            game.ghostCar.active = false;
            int i = 0;
            foreach (Transform child in transform.parent)
            {
                i++;
                var ag = child.gameObject.GetComponent<CarAgent>();
                child.gameObject.name = "Car " + i + " R " + ag.Round + " CP " + ag.checkPoint;
            }
        }
        else
        {
            pause();
        }
    }

    private void UpdatePlayerCam()
    {
        var cameraController = Camera.main.GetComponent<CameraController>();
        var ufoController = Camera.main.transform.parent.GetComponent<CameraUfoController>();
        if (game.runSimulation)
        {
            cameraController.Planning = false;

            ufoController.SetTarget(transform);
        }
        else
        {
            cameraController.Planning = true;

            var camPoint = transform.position;
            if (game.queued.Count > 0)
            {
                camPoint = game.queued.Last.Value;
            }

            var offset = game.RaceTrack.transform.position;

            float minMag = float.PositiveInfinity;
            Vector3 min = camPoint;
            Vector3 dir = Vector3.zero;
            for (var index = 0; index < game.splinePath.Length; index++)
            {
                var splinePoint = game.splinePath[index] + offset;
                var mag = (splinePoint - camPoint).sqrMagnitude;
                if (mag < minMag)
                {
                    minMag = mag;
                    min = splinePoint;
                    dir = game.splineVelocity[index];
                }
            }

            ufoController.SetTarget(min, Quaternion.LookRotation(-dir));
        }
    }

    private void UpdatePlayerInput()
    {

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        float lastYPos = agent.transform.position.y - 1;
        if (game.queued.Count != 0)
        {
            lastYPos = game.queued.Last.Value.y - 1;
        }


        if (!game.runSimulation)
        {
            if (Input.GetMouseButtonUp(0))
            {
                var trackMask = LayerMask.GetMask("Track");
                if (Physics.Raycast(ray, out hit, trackMask)) // Only hit Track
                {
                    var point = hit.point + hit.normal / 10f;

                    if (hit.point.y > lastYPos)
                    {
                        var ray2 = new Ray(hit.point + Vector3.down / 50, ray.direction);
                        if (Physics.Raycast(ray2, out hit, trackMask) && hit.point.y > lastYPos)
                        {
                            point = hit.point;
                        }
                    }

                    var lastPoint = transform.position;
                    if (game.queued.Count > 0)
                    {
                        lastPoint = game.queued.Last.Value;
                    }

                    var path = new NavMeshPath();
                    NavMesh.CalculatePath(lastPoint, point, NavMesh.AllAreas, path);
                    if (GetPathLength(path, lastPoint) > 70)
                    {
                        Debug.LogWarning("Path too long. Choose smaller steps,");
                    }
                    else
                    {
                        game.queued.AddLast(point);
                        var total = GetPathLength(agent, game.queued);

                        if (total > game.maxCost && !game.testing)
                        {
                            Debug.LogWarning("Path cost: " + total);
                            game.queued.RemoveLast();
                        }
                    }
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                game.queued.RemoveLast();
            }
        }


        if (Input.GetAxis("Submit") > 0)
        {
            if (game.queued.Count == 0)
            {
                return;
            }

            game.runSimulation = true;

            game.sleepyTime = game.maxSleepyTime;


            if (agent.isStopped)
            {
                agent.ResetPath();

                agent.SetDestination(game.queued.First.Value);
                game.queued.RemoveFirst();
            }
        }


       
    }

    private void DrawPath()
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> triangles = new List<int>();

        Vector3 last = transform.position;

        if (game.drawPaths)
        {
            
            if (!agent.isStopped)
            {
                Debug.DrawLine(agent.destination, last, Color.cyan);
                last = agent.destination;
                
                foreach (var v3 in agent.path.corners)
                {
                    var next = new Vector3(v3.x, v3.y, v3.z);

                    Debug.DrawLine(next, last, Color.yellow);
                    last = next;
                }

                last = agent.destination;
            }
            
            
        }
        
        if (playerControlled)
        {
            foreach (var v3 in game.queued)
            {
                var next = new Vector3(v3.x, v3.y, v3.z);

                drawArc(last, next, verts, triangles);
                Debug.DrawLine(next, last, Color.red);
                last = next;
            }  
            
            var meshy = new Mesh();

            var mf = game.trailmesh.GetComponent<MeshFilter>();
            meshy.vertices = verts.ToArray();
            meshy.triangles = triangles.ToArray();
            var ps = game.trailmesh.GetComponentInChildren<ParticleSystem>();
            if (meshy.triangles.Length > 0)
            {
                ps.Play();
            }
            else
            {
                ps.Clear();
                ps.Stop();
            }
            mf.mesh = meshy;

            // GhostCar
            if (!game.runSimulation && game.queued.Count > 1)
            {
                var lastPoint = game.queued.Last;
                var prevPoint = lastPoint.Previous;
                if (prevPoint != null)
                {
                    game.ghostCar.transform.position = prevPoint.Value;
                    game.ghostCar.transform.LookAt(lastPoint.Value);
                }
            }
            else
            {
                game.ghostCar.active = false;
            }
        }
        
        
    }

    private void drawArc(Vector3 last, Vector3 next, List<Vector3> verts, List<int> triangles)
    {
        var magnitude = (last - next).magnitude;
        var f = 1f / (magnitude * 10f);
        for (var l = 0f; l <= 1; l += f)
        {
            var nextP = SampleParabola(last, next, 4, l);
            //Debug.DrawLine(lastP, nextP, Color.magenta);

            verts.Add(nextP + Vector3.left / 20);
            verts.Add(nextP + Vector3.right / 20);
            var i = verts.Count;
            if (i > 2)
            {
                triangles.Add(i - 4);
                triangles.Add(i - 3);
                triangles.Add(i - 2);

                triangles.Add(i - 1);
                triangles.Add(i - 2);
                triangles.Add(i - 3);

                triangles.Add(i - 2);
                triangles.Add(i - 3);
                triangles.Add(i - 4);

                triangles.Add(i - 3);
                triangles.Add(i - 2);
                triangles.Add(i - 1);
            }
        }
    }

    Vector3 SampleParabola(Vector3 start, Vector3 end, float height, float pCent)
    {
        if (Mathf.Abs(start.y - end.y) < 0.1f)
        {
            //start and end are roughly level, pretend they are - simpler solution with less steps

            Vector3 travelDirection = end - start;
            Vector3 result = start + pCent * travelDirection;
            result.y += Mathf.Sin(pCent * Mathf.PI) * height;
            return result;
        }
        else
        {
            //start and end are not level, gets more complicated
            Vector3 travelDirection = end - start;
            Vector3 levelDirecteion = end - new Vector3(start.x, end.y, start.z);
            Vector3 right = Vector3.Cross(travelDirection, levelDirecteion);
            Vector3 up = Vector3.Cross(right, travelDirection);
            if (end.y > start.y) up = -up;
            Vector3 result = start + pCent * travelDirection;
            result += (Mathf.Sin(pCent * Mathf.PI) * height) * up.normalized;
            return result;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((CompareTag("Player") || CompareTag("Enemy")) && (other.CompareTag("Player") || other.CompareTag("Enemy")))
        {
            var otherAgent = other.gameObject.GetComponent<NavMeshAgent>();
            var dir = (transform.position - other.transform.position).normalized * game.knockBack * -1;
            otherAgent.velocity = new Vector3(dir.x, dir.y / 20, dir.z);
        }
        else
        {
            if (other.CompareTag("Checkpoint") && other.gameObject == game.checkPoints[checkPoint])
            {
                if (checkPoint == game.StartCheckpoint)
                {
                    Round++;
                    IsFinished = (Round - 1) >= game.NumberOfRounds;
                    if (Round -1 == game.NumberOfRounds)
                    {
                        FinishedAt = game.GetPlacing(gameObject);
                    } 
                       
                    if (IsFinished && playerControlled)
                    {
                        game.queued.Clear();
                        playerControlled = false;
                        game.runSimulation = true;
                        NextTarget();
                    }
                }
                NextCheckpoint();
            }

            if (other.CompareTag("Pickup"))
            {
                maxSpeed = game.overDriveSpeed;
                agent.speed = game.overDriveSpeed;
                agent.acceleration = 55f;
                overDrive = 5f;
                var sound = other.GetComponent<AudioSource>();
                if (sound)
                {
                    sound.Play();
                }
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

    private void NextCheckpoint()
    {
        checkPoint = (checkPoint + 1) % game.checkPoints.Length;
        NextTarget();
    }

    private void NextTarget()
    {
        var target = game.checkPoints[checkPoint];

        targetPoint = target.transform.position;
        targetPoint = target.transform.right.normalized * Random.Range(-7f, 7f) + targetPoint;

        updateAgent = true;
    }

    private void UpdateSimulation()
    {
        UpdateSpeed();

        if (playerControlled)
        {
            game.sleepyTime -= Time.deltaTime;
            if (DidAgentReachDestination(agent.gameObject.transform.position, agent.destination, 8f))
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
            if (game.sleepyTime < 0)
            {
                game.runSimulation = false;
            }
            
            if (!game.runSimulation)
            {
                foreach (var particleSystem in agent.GetComponentsInChildren<ParticleSystem>())
                {
                    particleSystem.Pause();
                }
            }
            else
            {
                foreach (var particleSystem in agent.GetComponentsInChildren<ParticleSystem>())
                {
                    particleSystem.Play();
                }
            }
        }
        else if (updateAgent)
        {
            updateAgent = false;
            agent.ResetPath();
            agent.SetDestination(targetPoint);
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

    private void UpdateSpeed()
    {
        var deltaSpeed = Random.Range(-1.1f, 1f); // Biased on purpose
        agent.speed = Mathf.Clamp(agent.speed + deltaSpeed / 10, 20, maxSpeed);

        var deltaPlacePoints = game.placing.First().placePoints - placePoints;
        agent.speed = Math.Max(0, deltaPlacePoints - 100) / 1000f + agent.speed;

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