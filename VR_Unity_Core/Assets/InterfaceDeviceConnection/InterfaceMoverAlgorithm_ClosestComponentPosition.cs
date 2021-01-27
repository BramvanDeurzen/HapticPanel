using HardwareInterface;
using UnityEngine;

public class InterfaceMoverAlgorithm_ClosestComponentPosition : IInterfaceMoverAlgorithm {
    VRComponentCollection VRComponents;
    Interface @interface;

    private Vector3 VRComponentsWorldPosition;

    public void Init(VRComponentCollection VRComponents, Interface @interface)
    {
        this.VRComponents = VRComponents;
        this.@interface = @interface;
        VRComponentsWorldPosition = this.VRComponents.transform.localPosition;
        VRComponentsWorldPosition = this.VRComponents.transform.position;
    }

    public Vector3 GetVRComponentsWorldPosition() { return VRComponentsWorldPosition; }

    public void GetNewPositionToSet(
        Vector3 trackerPosition, Quaternion trackerRotation,
        Vector3 currentPosition, Quaternion currentRotation,
        InterfaceMover.MoverAxisOrientation platformAxisOrientation,    
        out Vector3 gotoPosition, out Quaternion gotoRotation)
    {
        // Tracker position is in millimeter, so transform to meter world space.
        VRInputComponent closest = VRComponents.GetClosestInWorldspace(trackerPosition / 1000);
        Vector3 closestComponentPosition = closest.transform.position; // Meter scale
        // Transform to millimeter scale, because that's the scale of the robot platform
        closestComponentPosition *= 1000;
        // Move platform to where the closest component is.
        gotoPosition = closestComponentPosition;
        gotoRotation = Quaternion.identity;

        if (platformAxisOrientation.Equals(InterfaceMover.MoverAxisOrientation.XY_AXIS)) // In this case the X,Y of the tracker matches the platform axis
        {
            gotoPosition.z = 0.0f;
        }
        else if(platformAxisOrientation.Equals(InterfaceMover.MoverAxisOrientation.ZY_AXIS)) // In this case the Z of the tracker is the X of the platform (different orientation)
        {
            // Switch the z axis of the component to the x axis (because of the orientation)
            gotoPosition.x = gotoPosition.z;
            gotoPosition.z = 0.0f;
        }


        Vector3 interfacePosition;
        Quaternion interfaceRotation;
        if (@interface.GetPosRotOfHardwareID(closest.hardwareComponentID, out interfacePosition, out interfaceRotation))
        {
            gotoPosition -= interfacePosition;
            gotoRotation *= interfaceRotation;
        }
    }
}