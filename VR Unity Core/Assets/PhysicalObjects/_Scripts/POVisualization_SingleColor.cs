using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Physical Objects/No Interaction/Single Color")]
public class POVisualization_SingleColor : POBehaviour {
	public Color color;

	public override void Visualize(GameObject obj) {
		Material mat = obj.GetComponent<Renderer>().material;
		if (mat != null) {
			mat.color = color;
		} else {
			Debug.LogError("No material!");
		}
	}
}
