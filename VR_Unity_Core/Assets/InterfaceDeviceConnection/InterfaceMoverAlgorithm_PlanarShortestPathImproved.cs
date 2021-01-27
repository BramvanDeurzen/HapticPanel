using HardwareInterface;
using UnityEngine;

public class InterfaceMoverAlgorithm_PlanarShortestPathImproved : IInterfaceMoverAlgorithm {
    VRComponentCollection VRComponents;
    Interface @interface;

    private Vector3 VRComponentsWorldPosition;

    public void Init(VRComponentCollection VRComponents, Interface @interface)
    {
        this.VRComponents = VRComponents;
        this.@interface = @interface;
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

        gotoPosition = trackerPosition; // -> Platform moves to where finger is, should stay within the position limits of the alarm (boundary box so the movement isn't to slow)
        gotoPosition.z = 0.0f;
        gotoRotation = Quaternion.identity;

        Vector3 interfacePosition;
        Quaternion interfaceRotation;
        if (@interface.GetPosRotOfHardwareID(closest.hardwareComponentID, out interfacePosition, out interfaceRotation))
        {
            gotoPosition -= interfacePosition;
            gotoRotation *= interfaceRotation;
        }
    }

    //public void GetNewPositionToSet(Vector3 trackerWorldPosition, Quaternion trackerWorldRotation,
    //    Vector3 trackerLocalPosition, Quaternion trackerLocalRotation,
    //    Vector3 currentPosition, Quaternion currentRotation,
    //    out Vector3 goToPosition, out Quaternion goToRotation)
    //{
    //    VRInputComponent closest = VRComponents.GetClosestInWorldspace(trackerWorldPosition);
    //    //gotoPosition = /*VRComponentsPosition -*/ closest.transform.localPosition; // Why localposition? Should move to componentCollection - localposition. - because componentCollection.X is inversed
    //    goToPosition = trackerLocalPosition; // -> Platform moves to where finger is, should stay within the position limits of the alarm (boundary box so the movement isn't to slow)
    //    goToPosition.x = -trackerLocalPosition.x; //  x axis of optitrack is reverse of robot x
    //    goToPosition.z = 0.0f;
    //    goToRotation = Quaternion.identity;

    //    //Apply the interface transform (untested)
    //    Vector3 interfacePosition;
    //    Quaternion interfaceRotation;
    //    Vector3 componentsLocalPos = VRComponents.transform.localPosition;
    //    Vector3 closestLocalPos = closest.transform.localPosition;
    //    if (@interface.GetPosRotOfHardwareID(closest.hardwareComponentID, out interfacePosition, out interfaceRotation))
    //    {
    //        //interfacePosition.x -= 10; // testing purpose
    //        //goToPosition -= interfacePosition;
    //        goToPosition.x = -componentsLocalPos.x - closestLocalPos.x - interfacePosition.x;
    //        goToPosition.y = componentsLocalPos.y + closestLocalPos.y + interfacePosition.y;
    //        goToPosition.z = 0;
    //        goToRotation *= interfaceRotation;
    //        //UILogging.Info("Move platform to {0} position: ({1:F1} ; {2:F1})", closest.hardwareComponentID, interfacePosition.x, interfacePosition.y);
    //        //int i = 0;
    //    }
    //}
}
