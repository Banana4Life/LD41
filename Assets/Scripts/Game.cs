using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	public GameObject RaceTrack;
	public GameObject CheckpointTrack;
	public int StartCheckpoint = 0;

	public float knockBack = 30;

	public float maxCost = 150;

	public GameObject[] checkPoints;
	
	public LinkedList<Vector3> queued = new LinkedList<Vector3>();

	public bool runSimulation = true;

	public Vector3 camOffset1 = new Vector3(0,5,-4);
	public Vector3 camRot1 = new Vector3(30,0,0);
	
	public Vector3 camOffset2 = new Vector3(0,5,-4);
	public Vector3 camRot2 = new Vector3(30,0,0);

	public GameObject trailmesh;
	public Vector3[] splinePath;
	public Vector3[] splineVelocity;
	public bool testing;

	public GameObject ghostCar;
	
	void Awake()
	{
		var wrapper = CheckpointTrack.GetComponent<BezierSpline>().transform;
		
		checkPoints = new GameObject[wrapper.childCount];
		var i = 0;
		foreach (Transform t in wrapper)
		{
			checkPoints[i++] = t.gameObject;
		}

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
}
