using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Tracker interface
 * Trackers must inherit this class and 
 * - call Restrict and apply the changed position and rotation, when supported. This must be called first, and assures the data provided to Notify is within range
 * - call NotifyChange every time the tracker data changes, and the first frame of the tracking
 */

public class ITracker : MonoBehaviour {

	public enum ButtonState { Press, Release };

	public delegate void TrackerChange(Vector3 pos, Quaternion orientation);
	private List<TrackerChange> callbacks = new List<TrackerChange>();

	public delegate void Restrictor(ref Vector3 pos, ref Quaternion orientation);
	private List<Restrictor> restrictorCallbacks = new List<Restrictor>();

	public void Register(TrackerChange f) {
		callbacks.Add(f);
	}

	public void RegisterRestrictor(Restrictor f) {
		restrictorCallbacks.Add(f);
	}

	protected void NotifyChange(Vector3 pos, Quaternion orientation) {
		foreach(TrackerChange f in callbacks) {
			f(pos, orientation);
		}
	}

	protected void Restrict(ref Vector3 pos, ref Quaternion orientation) {
		foreach(Restrictor f in restrictorCallbacks) {
			f(ref pos, ref orientation);
		}
	}
}
