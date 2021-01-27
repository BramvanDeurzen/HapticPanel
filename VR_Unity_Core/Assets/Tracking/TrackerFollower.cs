using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HardwareInterface;
using System;
using System.Runtime.InteropServices;

//Follow a tracker object (locally, don't hang on a moving parent unless intended to move along). 
//Useful for visualization. 
//Place initially at the zero position
public class TrackerFollower : MonoBehaviour {

    
	public Vector3 offsetpos; // standard is 0,0,0
    public Quaternion offsetrot;
    public GameObject OptiTracker;


    void Awake () {

    }

	
	void Update () {
        // Scale from meter to millimeter
        Vector3 localPos = OptiTracker.transform.localPosition /* + offsetpos / 1000*/; 
        // apply offset
        localPos += offsetpos;
        transform.localPosition = localPos;
        // Apply offset rotation
        transform.localRotation = offsetrot * OptiTracker.transform.localRotation; // Inverse by the value of offsetRot

    }
}
