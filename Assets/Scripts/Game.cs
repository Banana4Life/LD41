using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	public GameObject RaceTrack;
	public GameObject CheckpointTrack;

	public GameObject[] checkPoints;
	
	public Queue<Vector3> queued = new Queue<Vector3>();

	public bool runSimulation = true;

	// Use this for initialization
	void Start ()
	{
		checkPoints = CheckpointTrack.GetComponent<BezierMaster>().instantiatedObjects;
	}
}
