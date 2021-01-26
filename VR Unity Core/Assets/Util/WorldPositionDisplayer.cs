using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPositionDisplayer : MonoBehaviour {

    public Transform localTransform;

    public Vector3 worldPosition;

    // Use this for initialization
    void Start () {
	    	
	}
	
	// Update is called once per frame
	void Update () {
        if (localTransform != null)
        {
            worldPosition = localTransform.position;
        }
		
	}
}
