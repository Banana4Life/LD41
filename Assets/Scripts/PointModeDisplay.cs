using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class PointModeDisplay : MonoBehaviour
{

	public Game Game;
	private Text text;
	
	void Start()
	{
		text = GetComponent<Text>();
//		text.text = "➤";
	}

	// Update is called once per frame
	void Update ()
	{

		var rot = transform.localEulerAngles;
		switch (Game.SelectedPointMode)
		{
			case PointMode.SPEEDUP:
				rot.z = 40f;
				break;
			case PointMode.SUSTAIN:
				rot.z = 0f; 
				break;
			case PointMode.SLOWDOWN:
				rot.z = -40f;
				break;
		}

		transform.localEulerAngles = rot;
	}
}
