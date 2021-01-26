using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCoordinateInspector : MonoBehaviour {
    public Vector3 worldPosition;

	// Use this for initialization
	void Start () {
        worldPosition = this.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        worldPosition = this.transform.position;
    }
}
