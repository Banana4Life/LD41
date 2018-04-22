using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialSelector : MonoBehaviour {
	
	public Material[] Materials;
	public bool RendererInChildren = false;

	void Start () {
		var selection = Materials[Random.Range(0, Materials.Length)];
		Debug.LogWarning("Selection:");
		Debug.LogWarning(selection);
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
