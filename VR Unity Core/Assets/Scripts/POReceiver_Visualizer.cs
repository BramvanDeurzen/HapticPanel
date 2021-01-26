using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Receiver that only visualizes, does not run any interaction*/

public class POReceiver_Visualizer : IPOReceiver {

	Quaternion offsetRotation;

	[System.Serializable]
	public class keyToColorMapping {
		public string key;
		public POBehaviour visualization;
	}

	public POBehaviour visualizationInvalid;
	public keyToColorMapping[] keyToColorMappings;
	Dictionary<string, POBehaviour> keyToColorMappingsDict = new Dictionary<string, POBehaviour>();

	[Tooltip("Maximum speet at which the tracker can move. Use for simulation")]
	public float maximumSpeed = 1.0f;

	Material material;
	Vector3 desiredPosition;

	void Start () {
		desiredPosition = transform.position;
		offsetRotation = transform.rotation;

		material = new Material(Shader.Find("Standard"));
		material.color = Color.gray;
		GetComponent<Renderer>().material = material;

		foreach(keyToColorMapping m in keyToColorMappings) {
			keyToColorMappingsDict[m.key] = m.visualization;
		}
	}
	
	void Update () {
		float maxdist = maximumSpeed * Time.deltaTime;
		//Simple linear movement
		if(Vector3.Distance(desiredPosition, transform.position) <= maxdist) {
			transform.position = desiredPosition;
		} else {
			transform.position = Vector3.ClampMagnitude(desiredPosition - transform.position, maxdist) + transform.position;
		}
	}

	public override void MoveToPO(Vector3 pos, Quaternion orientation, string type) {
		desiredPosition = pos;
		transform.rotation = offsetRotation * orientation;

		if(keyToColorMappingsDict.ContainsKey(type)) {
			keyToColorMappingsDict[type].Visualize(gameObject);
		}
	}
}
