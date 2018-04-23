using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WinMusic : MonoBehaviour
{
	public Game Game;
	private AudioSource source;
	public AudioClip FinishClip;

	// Use this for initialization
	void Start ()
	{
		source = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Game.gameOver)
		{
			source.Stop();
			source.clip = FinishClip;
			source.Play();
			Destroy(this);
		}
	}
}
