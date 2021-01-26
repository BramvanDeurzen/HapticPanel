using HardwareInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Given a tracking position and a list of VR Components, determine the position the interface mover should go to
//Does the prediction, and send to moving
public class InterfacePositioner : MonoBehaviour{

	VRComponentCollection components;
	TrackerSelector tracker;
	InterfaceMover mover;
	IInterfaceMoverAlgorithm algorithm;

	public void Init(VRComponentCollection components, TrackerSelector tracker, InterfaceMover mover, Interface @interface) {
		this.components = components;
		this.tracker = tracker;
		this.mover = mover;

		if((mover.Capabilities & InterfaceMover.MovementCapabilities.XY) == InterfaceMover.MovementCapabilities.XY) {
            //algorithm = new InterfaceMoverAlgorithm_PlanarShortestPath();
            //algorithm = new InterfaceMoverAlgorithm_PlanarShortestPathImproved();
            algorithm = new InterfaceMoverAlgorithm_ClosestComponentPosition();
            algorithm.Init(components, @interface);
            this.mover.SetVRComponentsWorldPosition(algorithm.GetVRComponentsWorldPosition());
            this.mover.SetVRComponentsModelTransform(components.GetModelTransform());
        } else {
			UILogging.Error("There is no method to use the current hardware");
		}
	}

	public string getCurrentVRComponentID(out bool currentExists) {
		if(mover.Status == InterfaceMover.MoverStatus.Arrived) {
			currentExists = true;
			//TODO, this can be cached
			return components.GetClosestInWorldspace(tracker.VRTrackedObject.transform.position).GetComponent<VRComponent>().ID;
		} else if (mover.Status == InterfaceMover.MoverStatus.Moving) {
			currentExists = false;
            // Mover desiredPosition is in platform space, tracker is in optitrack space
            if (mover.platformAxisOrientation.Equals(InterfaceMover.MoverAxisOrientation.XY_AXIS))
            {
                Vector3 diff = mover.desiredPosition - tracker.PlatformTrackedObject.transform.position * 1000;
                UILogging.Warning("InterfacePositioner::getCurrentVRComponentID:: moverstatus == moving, diff = X:{0},Y:{1}", System.Math.Abs(diff.x), System.Math.Abs(diff.y));
            }
            else if (mover.platformAxisOrientation.Equals(InterfaceMover.MoverAxisOrientation.ZY_AXIS))
            {
                // Mover desiredPosition is in OptiTrack space, not platform space.
                // Create a temp copy of the platform tracker with X and Z switched for the comparison and debugging.
                Vector3 tempPosition = new Vector3(tracker.PlatformTrackedObject.transform.position.z, tracker.PlatformTrackedObject.transform.position.y, tracker.PlatformTrackedObject.transform.position.x);
                Vector3 diff = mover.desiredPosition - tempPosition * 1000;
                UILogging.Warning("InterfacePositioner::getCurrentVRComponentID:: moverstatus == moving, diff = X:{0},Y:{1}", System.Math.Abs(diff.x), System.Math.Abs(diff.y));
            }
            return "";
		} else {
			currentExists = false;
			return "";
		}
	}

	void Update() {
		Vector3 gotopos;
		Quaternion gotorot;

        // Tracking and VR scene is in meters, platform is in mm.
        Vector3 trackerWorldMm = tracker.VRTrackedObject.transform.position * 1000;


        /* ALGORITHM: 
         * Gets the closest component from the finger tracker and sets the gotopos in OptiTrack space.
         * Closest component is based on Unitys VRCompontents world coordinate
         * Distance to physical button already subtracted from the gotopos 
         * Switching between X - Z coordinate is done for the desiredPosition!!!
         */
        algorithm.GetNewPositionToSet(trackerWorldMm, tracker.VRTrackedObject.transform.rotation,
            mover.currentPosition, mover.currentRotation, mover.platformAxisOrientation, out gotopos, out gotorot);
        
		mover.Move(gotopos, gotorot);
	}
}
