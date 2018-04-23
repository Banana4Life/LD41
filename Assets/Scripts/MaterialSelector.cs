using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialSelector : MonoBehaviour {
	
	public Material[] Materials;
	public bool RendererInChildren;

	void Start () {
		var selection = Materials[Random.Range(0, Materials.Length)];
		Renderer target;
		if (RendererInChildren)
		{
			target = GetComponentInChildren<Renderer>();
		}
		else
		{
			target = GetComponent<Renderer>();
		}
		target.material = selection;
	}
}
