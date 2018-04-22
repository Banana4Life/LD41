using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	public GameObject RaceTrack;
	public GameObject CheckpointTrack;
	
	public bool runSimulation = true;
	public bool testing;
	public bool drawPaths;

	public int StartCheckpoint = 0;
	
	public float knockBack = 30;

	public float maxCost = 150;


	public GameObject[] checkPoints;
	
	public LinkedList<Vector3> queued = new LinkedList<Vector3>();


	public Vector3 camOffset1 = new Vector3(0,5,-4);
	public Vector3 camRot1 = new Vector3(30,0,0);
	
	public Vector3 camOffset2 = new Vector3(0,5,-4);
	public Vector3 camRot2 = new Vector3(30,0,0);

	public GameObject trailmesh;
	public Vector3[] splinePath;
	public Vector3[] splineVelocity;

	public GameObject ghostCar;

	public List<CarAgent> placing;
	
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
		placing = placing.OrderBy(ag =>
		{
			var placePoints = ag.Round * checkPoints.Length + ag.checkPoint + pathIndex(ag.transform.position) / 1000f;
			ag.placePoints = placePoints;
			return placePoints;
		}).Reverse().ToList();
	}

	private int pathIndex(Vector3 position)
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
