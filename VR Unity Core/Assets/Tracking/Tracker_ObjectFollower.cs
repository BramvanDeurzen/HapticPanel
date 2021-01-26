using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Tracker that follows 'this'*/

public class Tracker_ObjectFollower : ITracker {

	bool first = true;
	Vector3 oldPos;
	Quaternion oldRot;

	void Start () {
		
	}
	
	void Update () {
		if(first || oldPos != transform.localPosition || oldRot != transform.localRotation) {
			Vector3 pos = transform.localPosition;
			Quaternion rot = transform.localRotation;

			Restrict(ref pos, ref rot);
			NotifyChange(pos, rot);

			transform.localPosition = pos;
			transform.localRotation = rot;

			first = false;
			oldPos = pos;
			oldRot = rot;
		}
	}
}
