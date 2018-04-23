using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Outline))]
public class PointModeHighlighter : MonoBehaviour
{
	public PointMode Mode;
	public Game Game;
	
	void Update ()
	{
		GetComponent<Outline>().enabled = Game.SelectedPointMode == Mode;
	}
}
