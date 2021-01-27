using HardwareInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInterfaceMoverAlgorithm  {
    

    void Init(VRComponentCollection VRComponents, Interface @interface);

	void GetNewPositionToSet(
		Vector3 trackerPosition, Quaternion trackerRotation,
		Vector3 currentPosition, Quaternion currentRotation, 
        InterfaceMover.MoverAxisOrientation platformAxisOrientation,
		out Vector3 gotoPosition, out Quaternion gotoRotation);

    Vector3 GetVRComponentsWorldPosition();

	// The trackerWorldPosition is used to find the closest component, 
	// The trackerLocalPosition is used to set the goToPosition.
	//void GetNewPositionToSet(Vector3 trackerWorldPosition, Quaternion trackerWorldRotation,
	//	Vector3 trackerLocalPosition, Quaternion trackerLocalRotation,
	//	Vector3 currentPosition, Quaternion currentRotation,
	//	out Vector3 goToPosition, out Quaternion goToRotation);
}
