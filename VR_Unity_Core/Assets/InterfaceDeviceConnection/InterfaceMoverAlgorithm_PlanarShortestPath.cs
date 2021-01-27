using HardwareInterface;
using UnityEngine;

public class InterfaceMoverAlgorithm_PlanarShortestPath : IInterfaceMoverAlgorithm {
	VRComponentCollection VRComponents;
	Interface @interface;

	private Vector3 VRComponentsPosition;
    private Vector3 VRComponentsWorldPosition;

    public void Init(VRComponentCollection VRComponents, Interface @interface) {
		this.VRComponents = VRComponents;
		this.@interface = @interface;
        VRComponentsWorldPosition = this.VRComponents.transform.position;
        VRComponentsPosition = this.VRComponents.transform.localPosition;
		VRComponentsPosition.x = -VRComponentsPosition.x; // X value is negative in Unity, we need positive value
	}

    public Vector3 GetVRComponentsWorldPosition() { return VRComponentsWorldPosition; }

    public void GetNewPositionToSet(
		Vector3 trackerPosition, Quaternion trackerRotation,
		Vector3 currentPosition, Quaternion currentRotation,
        InterfaceMover.MoverAxisOrientation platformAxisOrientation,
        out Vector3 gotoPosition, out Quaternion gotoRotation) {

		VRInputComponent closest = VRComponents.GetClosestInWorldspace(trackerPosition);
		//gotoPosition = /*VRComponentsPosition -*/ closest.transform.localPosition; // Why localposition? Should move to componentCollection - localposition. - because componentCollection.X is inversed
		gotoPosition = trackerPosition; // -> Platform moves to where finger is, should stay within the position limits of the alarm (boundary box so the movement isn't to slow)
		gotoPosition.x = - trackerPosition.x; //  x axis of optitrack is reverse of robot x
		gotoPosition.z = 0.0f;
		gotoRotation = Quaternion.identity;

		//Apply the interface transform (untested)
		Vector3 interfacePosition;
		Quaternion interfaceRotation;
		if(@interface.GetPosRotOfHardwareID(closest.hardwareComponentID, out interfacePosition, out interfaceRotation)) {
			//interfacePosition.x -= 10; // testing purpose
			gotoPosition -= interfacePosition; 
			gotoRotation *= interfaceRotation;
			//int i = 0;
		}
	}

	//public void GetNewPositionToSet(Vector3 trackerWorldPosition, Quaternion trackerWorldRotation,
	//	Vector3 trackerLocalPosition, Quaternion trackerLocalRotation,
	//	Vector3 currentPosition, Quaternion currentRotation,
	//	out Vector3 goToPosition, out Quaternion goToRotation)
	//{
	//	VRInputComponent closest = VRComponents.GetClosestInWorldspace(trackerWorldPosition);
	//	//gotoPosition = /*VRComponentsPosition -*/ closest.transform.localPosition; // Why localposition? Should move to componentCollection - localposition. - because componentCollection.X is inversed
	//	goToPosition = trackerLocalPosition; // -> Platform moves to where finger is, should stay within the position limits of the alarm (boundary box so the movement isn't to slow)
	//	goToPosition.x = -trackerLocalPosition.x; //  x axis of optitrack is reverse of robot x
	//	goToPosition.z = 0.0f;
	//	goToRotation = Quaternion.identity;

	//	//Apply the interface transform (untested)
	//	Vector3 interfacePosition;
	//	Quaternion interfaceRotation;
	//	if (@interface.GetPosRotOfHardwareID(closest.hardwareComponentID, out interfacePosition, out interfaceRotation))
	//	{
 //           interfacePosition.x -= 10; // testing purpose
 //           goToPosition -= interfacePosition;
	//		goToRotation *= interfaceRotation;
	//		//UILogging.Info("Move platform to {0} position: ({1:F1} ; {2:F1})", closest.hardwareComponentID, interfacePosition.x, interfacePosition.y);
	//		//int i = 0;
	//	}
	//}
}