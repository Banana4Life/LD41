using System.Collections.Generic;
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
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        game = GameObj.GetComponent<Game>();
    }

    void Start()
    {
        checkPoint = game.StartCheckpoint;
        var target = game.checkPoints[checkPoint];
        if (!playerControlled)
        {
            agent.SetDestination(target.transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerControlled)
        {
            UpdatePlayerInput();
            UpdatePlayerCam();
            game.ghostCar.active = true;
            DrawPlayerPath();
        }

        if (game.runSimulation)
        {
            resume();
            UpdateSimulation();
            game.ghostCar.active = false;
        }
        else
        {
            pause();
        }
    }

    private void UpdatePlayerCam()
    {
        if (game.runSimulation)
        {
            Camera.main.transform.localPosition = game.camOffset1;
            Camera.main.transform.localEulerAngles = game.camRot1;

            Camera.main.transform.parent.transform.position = transform.position;
            Camera.main.transform.parent.eulerAngles = transform.eulerAngles;
        }
        else
        {
            Camera.main.transform.localPosition = game.camOffset2;
            Camera.main.transform.localEulerAngles = game.camRot2;

            var camPoint = transform.position;
            if (game.queued.Count > 1)
            {
                camPoint = game.queued.Last.Previous.Value;
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

            var target = dir + min;


            Camera.main.transform.parent.transform.position = min;
            Camera.main.transform.parent.LookAt(target);
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
                if (Physics.Raycast(ray, out hit, 1 << 8)) // Only hit Track
                {
                    var point = hit.point + hit.normal / 10f;

                    if (hit.point.y > lastYPos)
                    {
                        var ray2 = new Ray(hit.point + Vector3.down / 50, ray.direction);
                        if (Physics.Raycast(ray2, out hit, 1 << 8) && hit.point.y > lastYPos)
                        {
                            point = hit.point;
                        }
                    }

                    game.queued.AddLast(point);
                    var total = GetPathLength(agent, game.queued);

                    if (total > 50 && !game.testing)
                    {
                        Debug.LogWarning("Path cost: " + total);
                        game.queued.RemoveLast();
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


            if (agent.isStopped)
            {
                agent.ResetPath();

                agent.SetDestination(game.queued.First.Value);
                game.queued.RemoveFirst();
            }
        }


       
    }

    private void DrawPlayerPath()
    {
        var meshy = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> triangles = new List<int>();
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
        var ps = game.trailmesh.GetComponentInChildren<ParticleSystem>();
        if (meshy.triangles.Length > 0)
        {
            ps.Play();
        }
        else
        {
            ps.Stop();
        }


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

    private void drawArc(Vector3 last, Vector3 next, List<Vector3> verts, List<int> triangles)
    {
        var lastP = last;

        var magnitude = (last - next).magnitude;
        var f = 1f / (magnitude * 10f);
        for (var l = 0f; l <= 1.2; l += f)
        {
            var nextP = SampleParabola(last, next, 4, l);
            //Debug.DrawLine(lastP, nextP, Color.magenta);
            lastP = nextP;

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
        if ((CompareTag("Player") || CompareTag("Ememy")) && (other.CompareTag("Player") || other.CompareTag("Enemy")))
        {
            var otherAgent = other.gameObject.GetComponent<NavMeshAgent>();
            otherAgent.velocity = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)).normalized * 30;
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

    private void UpdateSimulation()
    {
        var deltaSpeed = (float) Random.Range(-1, 2);
        agent.speed = Mathf.Clamp(agent.speed + deltaSpeed / 10, 20, 30);

        if (playerControlled)
        {
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
            if (DidAgentReachDestination(agent.gameObject.transform.position, agent.destination,
                agent.stoppingDistance))
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