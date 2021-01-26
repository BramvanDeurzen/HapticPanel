using UnityEngine;

/*Moving around the POs, based on some mode
 * TODO
 * - Modes should be moved to some behaviour module, not all in 1 class
 */

public class POPlacer : MonoBehaviour {
	public enum POPlacerModes {
		TesterPlane,			//Assume there is a plane at z = -5
		TesterObjectsOnPlane,	//Assume there are objects at z = -5
	};
	public enum PlacerStatus {
		FreeSpace,				//The tracker is in free moving space
		BreakDistance,			//The tracker is in the space where the PO must stop
		Touch,					//The tracker is touching the PO
		Through,				//The tracker seemed to be through the PO. This is an error condition. PO cannot move here to avoid danger
	}

	[Header("Send and receive")]
	public IPOReceiver[] receivers;
	public ITracker tracker;

	[Header("Placer settings")]
	public POPlacerModes mode;
	[Tooltip("Distance from the tracker to the POs where the placer will not move anymore")]
	public float breakDistance = 0.1f;
	[Tooltip("Distance from the tracker to the POs where the placer will move the PO to the desired place")]
	public float placerDistance = 1.0f;
	[Tooltip("Distance to backup for the placer. The placer will move backwards to move around when the tracker is far away enough")]
	public float backupdistance = 0.5f;

	// Use this for initialization
	void Start () {
		tracker.Register((Vector3 p, Quaternion r) => TrackerUpdate(p, r, 0));
		tracker.RegisterRestrictor((ref Vector3 p, ref Quaternion r) => TrackerRestrictor(ref p, ref r, 0));
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void TrackerUpdate(Vector3 pos, Quaternion rot, int trackernumber) {
		switch(mode) {
			case POPlacerModes.TesterPlane: 
				{
					if(pos.z <= -5 + breakDistance) {
						//break
					} else {
						Vector3 newpos = new Vector3(pos.x, pos.y, -5);
						foreach(IPOReceiver r in receivers) {
							r.MoveToPO(newpos, Quaternion.identity, "none");
						}
					}
					break;
				}
			case POPlacerModes.TesterObjectsOnPlane: {
					if (pos.z <= -5 + breakDistance) {
						//break
					} else {
						//just the closest int*factor
						const int factor = 2;

						Vector3 newpos = new Vector3(Mathf.RoundToInt(pos.x / (float)factor) * factor, Mathf.RoundToInt(pos.y / (float)factor) * factor, -5);
						foreach (IPOReceiver r in receivers) {
							r.MoveToPO(newpos, Quaternion.identity, ((int)(newpos.x) / factor) % 2 == 0 ? "A" : "B");
						}
					}
					break;
				}
		}
	}

	void TrackerRestrictor(ref Vector3 pos, ref Quaternion rot, int trackernumber) {
		switch (mode) {
			case POPlacerModes.TesterPlane:
			case POPlacerModes.TesterObjectsOnPlane:
				{
					pos.z = Mathf.Max(pos.z, -5);
					break;
				}
		}
	}
}
