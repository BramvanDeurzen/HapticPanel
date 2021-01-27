using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Disable all children, except the selected one
 * The tracked object has local position and orientation in meters
 * All transformations to VR are done here
 */

public class TrackerSelector : MonoBehaviour {
    // OptiTrack finger tracker
	public GameObject activeTrackerObject;
	public GameObject getTrackedObject { get { return activeTrackerObject; } }
	public GameObject VRTrackedObject;
    public GameObject PlatformTrackedObject;
    // OptiTrack platform tracker left
    //public GameObject activePlatformLeftObject;
    //public GameObject getactivePlatformLeftObject { get { return activePlatformLeftObject; } }
    //public GameObject VRPlatformLeft;
    // OptiTrack platform tracker left

    void Start () {
		foreach(Transform t in transform) {
			t.gameObject.SetActive(t.gameObject == activeTrackerObject);
            //t.gameObject.SetActive(t.gameObject == activePlatformLeftObject);
		}
	}	
}
