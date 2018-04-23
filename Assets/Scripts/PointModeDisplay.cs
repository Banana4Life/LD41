using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class PointModeDisplay : MonoBehaviour
{

	public Game Game;

	// Update is called once per frame
	void Update ()
	{
		var text = GetComponent<Text>();

		if (Game.runSimulation)
		{
			text.text = "";
		}
		else
		{
			switch (Game.SelectedPointMode)
			{
				case PointMode.SPEEDUP:
					text.text = "Getting faster!";
					break;
				case PointMode.SLOWDOWN:
					text.text = "Getting slower!";
					break;
				case PointMode.SUSTAIN:
					text.text = "Keeping speed!";
					break;
			}
		}
	}
}
