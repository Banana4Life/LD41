using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckpointMeta : MonoBehaviour
{

	public Game game;
	public CarAgent player;
	private Text text;

	private string baseText;

	private void Start()
	{
		text = GetComponent<Text>();
		baseText = text.text;
	}

	void Update ()
	{
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
