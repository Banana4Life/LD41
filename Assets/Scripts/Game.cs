using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
	public GameObject RaceTrack;
	public GameObject CheckpointTrack;
	
	public bool runSimulation = true;
	public bool testing;
	public bool drawPaths;

	public PointMode SelectedPointMode = PointMode.SUSTAIN;

	public int StartCheckpoint;
	
	public float knockBack = 30;

	public float maxCost = 150;


	public GameObject[] checkPoints;
	
	public LinkedList<Waypoint> queued = new LinkedList<Waypoint>();


	public Vector3 camOffset1 = new Vector3(0,5,-4);
	public Vector3 camRot1 = new Vector3(30,0,0);
	
	public Vector3 camOffset2 = new Vector3(0,5,-4);
	public Vector3 camRot2 = new Vector3(30,0,0);

	public GameObject trailmesh;
	public Vector3[] splinePath;
	public Vector3[] splineVelocity;

	public GameObject ghostCar;

	public List<CarAgent> placing;
	public int maxSpeed = 30;
	public int overDriveSpeed = 50;

	public GameObject placingTexts;

	public float sleepyTime = 0;
	public float maxSleepyTime = 5f;
	public int NumberOfRounds = 3;

	void Awake()
	{
		var wrapper = CheckpointTrack.GetComponent<BezierSpline>().transform;
		
		checkPoints = new GameObject[wrapper.childCount];
		var i = 0;
		foreach (Transform t in wrapper)
		{
			checkPoints[i++] = t.gameObject;
		}

		checkPoints = checkPoints.Reverse().ToArray();
		
		GetPath(RaceTrack.GetComponent<BezierSpline>(), 500);
		
		
	}
	
	
	private void GetPath(BezierSpline spline, int pointsCount)
	{
		splinePath = new Vector3[pointsCount];
		splineVelocity = new Vector3[pointsCount];

		for (int i = 0; i < pointsCount; i++)
		{
			float t = i / (float)(pointsCount - 1);

			if (spline.Loop)
				t = i / (float)(pointsCount);

			splinePath[i] = spline.GetPoint(t);
			splineVelocity[i] = spline.GetVelocity(t);
		}
	}

	private void Update()
	{
		//placing = placing.OrderBy(ag => pathIndex(ag.transform.position) + splinePath.Length * ag.Round).ToList();
		placing = placing.OrderBy(ag => -ag.FinishedAt).ThenBy(ag =>
		{
			var pathIdx = pathIndex(ag.transform.position);
			if (ag.checkPoint == 0)
			{
				if (pathIdx > 250)
				{
					pathIdx = pathIdx - 500;
				}
			}

			ag.pathIndex = pathIdx;
			var placePoints = ag.Round * checkPoints.Length + ag.checkPoint + pathIdx / 1000f;
			ag.placePoints = placePoints;
			return placePoints;
		}).ThenBy(ag => (ag.transform.position - ag.targetPoint).sqrMagnitude)
			.Reverse().ToList();

		int i = 0;
		foreach (Transform child in placingTexts.transform)
		{
			var placed = placing[i].transform;
			child.position = placed.position + Vector3.up;
			child.LookAt(Camera.main.transform.position);
			i++;
		}
		
	}

	private void FixedUpdate()
	{
		var modes = Enum.GetValues(typeof(PointMode)).Cast<PointMode>().ToArray();
		var i = 0;
		for (; i < modes.Length; ++i)
		{
			if (modes[i] == SelectedPointMode) break;
		}
		
		var scroll = Input.GetAxisRaw("Mouse ScrollWheel");
		if (scroll > 0)
		{
			SelectedPointMode = modes[Math.Min(i + 1, modes.Length - 1)];
		}
		else if (scroll < 0)
		{
			SelectedPointMode = modes[Math.Max(i - 1, 0)];
		}

		if (Input.GetKeyUp(KeyCode.R))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		if (Input.GetKeyUp(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	public int GetPlacing(GameObject obj)
	{
		for (var i = 0; i < placing.Count; i++)
		{
			if (placing[i].gameObject == obj)
			{
				return i + 1;
			}
		}

		return 0;
	}

	public int pathIndex(Vector3 position)
	{
		int mindex = 0;
		float mag = float.PositiveInfinity;
		for (var index = 0; index < splinePath.Length; index++)
		{
			Vector3 vector3 = splinePath[index] + RaceTrack.transform.position;
			var nextMag = (position - vector3).sqrMagnitude;
			if (nextMag <= mag)
			{
				mag = nextMag;
				mindex = index;
			}
		}

		return 500 - mindex;
	}
}
