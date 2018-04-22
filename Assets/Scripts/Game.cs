using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	public GameObject RaceTrack;
	public GameObject CheckpointTrack;

	public GameObject[] checkPoints;
	
	public LinkedList<Vector3> queued = new LinkedList<Vector3>();

	public bool runSimulation = true;

	public Vector3 camOffset1 = new Vector3(0,5,-4);
	public Vector3 camRot1 = new Vector3(30,0,0);
	
	public Vector3 camOffset2 = new Vector3(0,5,-4);
	public Vector3 camRot2 = new Vector3(30,0,0);

	public GameObject trailmesh;

	void Awake()
	{
		checkPoints = CheckpointTrack.GetComponent<BezierMaster>().instantiatedObjects;
	}
}
