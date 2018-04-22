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

	private void Start()
	{
		text = GetComponent<Text>();
		baseText = text.text;
	}

	void Update ()
	{
		var gameCheckPoint = game.checkPoints[player.checkPoint];
		var center = gameCheckPoint.GetComponentInChildren<Renderer>().bounds.center;

		text.gameObject.transform.position = center + Vector3.up * HeightOffset;
		text.gameObject.transform.eulerAngles = gameCheckPoint.transform.eulerAngles;


		var playerPlacing = 0;
		for (var i = 0; i < game.placing.Count; i++)
		{
			if (game.placing[i].gameObject == player.gameObject)
			{
				playerPlacing = i + 1;
				break;
			}
		}

		if (playerPlacing > 0)
		{
			text.text = baseText + " " + playerPlacing;
		}
	}
}
