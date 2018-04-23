using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckpointMeta : MonoBehaviour
{

	public Game game;
	public CarAgent player;
	private Text text;
	public float HeightOffset = 5f;

	private string baseText;

	public int offset;

	private void Start()
	{
		text = GetComponent<Text>();
		baseText = text.text;
	}

	void Update ()
	{
		var checkpt = player.checkPoint + offset;
		if (checkpt < 0)
		{
			checkpt = checkpt + game.checkPoints.Length;
		}
		
		var gameCheckPoint = game.checkPoints[checkpt];
		var center = gameCheckPoint.GetComponentInChildren<Renderer>().bounds.center;

		text.gameObject.transform.position = center + Vector3.up * HeightOffset;
		text.gameObject.transform.eulerAngles = gameCheckPoint.transform.eulerAngles;

		var playerPlacing = game.GetPlacing(player.gameObject);

		if (playerPlacing > 0)
		{
			text.text = baseText + " " + playerPlacing;
		}
	}
}
