using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRComponent : MonoBehaviour {

	[Tooltip("Leave empty to use the name of the component")]
	public string ID;
	public bool noRendering = true;

	// Use this for initialization
	public void Awake() {
		if (ID == "") {
			ID = transform.name;
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
