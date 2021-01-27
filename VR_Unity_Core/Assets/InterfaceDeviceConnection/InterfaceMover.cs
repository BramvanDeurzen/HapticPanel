using HardwareInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

//This class does the actual moving, and all connections with the device
//Tracker is required to handle movement safely
public class InterfaceMover : MonoBehaviour {
	// Platform offsets and step factors. 
	// Only placed here for testing, could move to the plugin when the correct values are found
	public float MM_TO_STEPS_FACTOR_X; // 2000/45 - big platform
	public float MM_TO_STEPS_FACTOR_Y; // 100 - bif platform
	public float X_OFFSET_MM;
	public float Y_OFFSET_MM;
    public float interfacePlatformWidthInMm;
    public float interfacePlatformHeightInMm;
    public MoverAxisOrientation platformAxisOrientation;



    //HardwareInterface.InterfaceVisualizer visualizer;
    TrackerSelector tracker;

    public Vector3 desiredPosition; // Set as public for testing, TODO : remove
	Quaternion desiredRotation;

	public Vector3 currentPosition { get; /*todo, replace with hardware calls*/ private set; }
	public Quaternion currentRotation { get; /*todo, replace with hardware calls*/ private set; }

	public int comPort;
	public string comPortString {
		set {
			comPort = int.Parse(value);
		}
	}

	public MoverStatus Status { get; private set; }
	private bool DeviceIsReady = false;
    private bool demoRunning = false;
    private bool PlatformPositionAdjusted = false;
    public bool temporaryStopPlatform = false; // Can be stopped from other functions, for example when rotating the rotary encoder.
    private bool fingerNearPlatform = false;

    private Vector3 VRComponentsWorldPosition;
    private Transform VRComponentsModelTransform;
    private Vector3 VRComponentsModelSizeInMm;
    private Vector3 VRComponentsPositionInPlatformSpace;

    private Vector3 previousPositionPlatform;
    private int previousPositionMatchCounter = 0;
    private const int REQUIRED_NUMBER_OF_MATCHING_FRAMES = 2;
    private int previousStepPositionPlatformHorizontal;
    private int previousStepPositionPlatformVertical;

	[System.Flags]
	public enum MovementCapabilities {
		//Rotation axes are non-moving
		XY = 1,         //Can move in a plane
		Depth = 2,      //Can move forward and backwards
		Rotate_Z = 4    //Can rotate over the depth axis
	};
	public enum MoverStatus {
		Off,            //Interfacemover is turned off
		Arrived,        //Interface is where it should be, allow interaction
		Moving,         //Moving to the right position
		SafetyStop,     //Something unsafe happened, and all movement stopped
	};
    public enum MoverAxisOrientation
    {
        XY_AXIS, // the x and y match the motive axis
        ZY_AXIS //  the x axis of the platform is the z axis in motive.
    };

	public MovementCapabilities Capabilities { 
		get {
			/*todo, replace with hardware calls*/
			return MovementCapabilities.XY;
		}
	}

	public void Init() {
		if (comPort < 0)
		{
			UILogging.Error("InterfaceMover: Please enter a valid com port number");
			return;
		}

		if (!StartDevice(comPort))
			UILogging.Error("InterfaceMover: Cannot activate the hardware interface (output) on COM port {0}", comPort);
		else
		{
            // Start by moving the platform to the middle of the alarm

            if (!MovePlatform(VRComponentsPositionInPlatformSpace.x, VRComponentsPositionInPlatformSpace.y, MM_TO_STEPS_FACTOR_X, MM_TO_STEPS_FACTOR_Y))
                UILogging.Error("InterfaceMover: Cannot move the platform to position: ({0:F1} ; {1:F1})", VRComponentsPositionInPlatformSpace.x, VRComponentsPositionInPlatformSpace.y);
            else
            {
                UILogging.Info("InterfaceMover: Init moved the platform to position: ({0:F1} ; {1:F1})", VRComponentsPositionInPlatformSpace.x, VRComponentsPositionInPlatformSpace.y);
                demoRunning = true;
                DeviceIsReady = true;
            }
                
        }
    }

	public void SetVisualizerAndTracker(/*HardwareInterface.InterfaceVisualizer visualizer,*/ TrackerSelector tracker) {
        //this.visualizer = visualizer;
        this.tracker = tracker;
        //currentPosition = new Vector3(x_positionPlatformInOptiSpace - X_OFFSET_MM, y_positionPlatformInOptiSpace - Y_OFFSET_MM, 0);
        currentPosition = new Vector3(X_OFFSET_MM, Y_OFFSET_MM, 0);
		currentRotation = Quaternion.identity;
		Status = MoverStatus.Off;
	}

	public void Move(Vector3 pos, Quaternion rot) {
        //UILogging.Error("InterfaceMover::Move desiredPosition: " + desiredPosition);
        desiredPosition = pos;
		desiredRotation = rot;
	}

    public void SetVRComponentsWorldPosition(Vector3 VRComponentsPosition)
    {
        VRComponentsWorldPosition = VRComponentsPosition;
        // Save a mm scale version (for the platform controls)
        VRComponentsPositionInPlatformSpace = VRComponentsWorldPosition * 1000;
        // Switch x and z around
        if (platformAxisOrientation.Equals(MoverAxisOrientation.ZY_AXIS))
        {
            float temp = VRComponentsPositionInPlatformSpace.z;
            VRComponentsPositionInPlatformSpace.z = VRComponentsPositionInPlatformSpace.x;
            VRComponentsPositionInPlatformSpace.x = temp;
        }
        VRComponentsPositionInPlatformSpace.x -= X_OFFSET_MM;
        VRComponentsPositionInPlatformSpace.y -= Y_OFFSET_MM;


    }

    public void SetVRComponentsModelTransform(Transform modelTransform)
    {
        VRComponentsModelTransform = modelTransform;
        // Set the size in mm
        VRComponentsModelSizeInMm = VRComponentsModelTransform.localScale * 1000;
    }

    public void SwitchTemporaryPlatformMovementStop(bool platfromStatus)
    {
        // Stop moving
        if (platfromStatus)
        {
            temporaryStopPlatform = true;
            Status = MoverStatus.Arrived;
        }
        else
        {
            temporaryStopPlatform = false;
            Status = MoverStatus.Moving;
        }
    }

    public void SetMovementIfFingerIsNearPlatform(bool triggerStatus)
    {
        fingerNearPlatform = triggerStatus;
    }

    void OnDestroy()
	{
		StopDevice();
	}

	void Update() {
        // new version:
        // Get currentPosition from tracker
        // Compare desiredPosition versus CurrentPosition
        // If difference is not neglectible, move to the desired position. (bigger then 2mm? ) 
        if (demoRunning && !temporaryStopPlatform && fingerNearPlatform)
        {
            /* ALGORITHM: 
             * The axis orientation is adjusted in the interfaceComponent position algorithm.
             * We switch the current position x and y
             * So we can assume we can work in the x-y axis for the calculations.
             * 
             * 
             */
            currentPosition = (tracker.PlatformTrackedObject.transform.position);
            currentPosition *= 1000; // transform from mm to meter
            if (platformAxisOrientation.Equals(MoverAxisOrientation.ZY_AXIS))
            {
                currentPosition = new Vector3(currentPosition.z, currentPosition.y, currentPosition.x);
            }
            float movementDriftCompensation = 8f; //8f

            // We only care about X and Y difference for the platform. 
            float horizontalDifference = Math.Abs(desiredPosition.x - currentPosition.x);                
            float verticalDifference = Math.Abs(desiredPosition.y - currentPosition.y);
            
            // CHECK IF PLATFORM IS ARRIVED
            if (horizontalDifference <= movementDriftCompensation && verticalDifference <= movementDriftCompensation)
            {
                //currentPosition = desiredPosition;
                Status = MoverStatus.Arrived;
                //UILogging.Warning("MoverStatus: Arrived!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
            else
            {
                //UILogging.Warning("DEBUG: desired: {0} - current: {1}", desiredPosition, currentPosition);
                //UILogging.Warning("InterfaceMover:: (moving) desired - current pos: ({0}, {1})", horizontalDifference, verticalDifference);
                Status = MoverStatus.Moving;
                PlatformPositionAdjusted = false;
            }

            // Compare value of previous frames, to know the platform is in 1 place (which indicates it is in position)
            // Because we only want to send reposition message to the steppers when it stopped moving
            if (CompareCurrentAndPreviousPlatformPosition())
            {
                // Stepper in one place
                //Debug.Log("Platform position unchanged since last frame at position: " + Vector3Int.RoundToInt(currentPosition).ToString());
                CheckAndAdjustPlatformPosition();
            }
            previousPositionPlatform = currentPosition;

            // Move to the current position if MoverStatus is moving
            if (Status.Equals(MoverStatus.Moving) && DeviceIsReady)
            {
                // Subtract offsets
                float tempPositionX = desiredPosition.x - X_OFFSET_MM;
                float tempPositionY = desiredPosition.y - Y_OFFSET_MM;

                // The model represents the virtual interface with which the user can interact. No need to move the platform outside of it.
                //if (IsTrackerWithinVirtualModel(tempPositionX, tempPositionY))
                //{
                    //UILogging.Error("InterfaceMover::MovePlatform move the platform to position: ({0:F1};{1:F1})", tempPositionX, tempPositionY);
                    // TESTING: Disabled for testing
                    if (!MovePlatform(tempPositionX, tempPositionY, MM_TO_STEPS_FACTOR_X, MM_TO_STEPS_FACTOR_Y))
                        UILogging.Error("InterfaceMover::MovePlatform Cannot move the platform to position: ({0:F1} ; {1:F1})", tempPositionX, tempPositionY);
                    currentRotation = desiredRotation;

                //}
            }
        }
        else if (demoRunning && temporaryStopPlatform)
        {
            Status = MoverStatus.Arrived; 
        }
    }

    private bool IsTrackerWithinVirtualModel(float trackerPositionX, float trackerPositionY)
    {
        //// Also incorporate the physical size of the platform.
        //// Test x direction
        //if (trackerPositionX <= VRComponentsPositionInPlatformSpace.x + VRComponentsModelSizeInMm.x / 2 + interfacePlatformWidthInMm * 2
        //    && trackerPositionX >= VRComponentsPositionInPlatformSpace.x - VRComponentsModelSizeInMm.x / 2 - interfacePlatformWidthInMm * 2)
        //    // Test y direction
        //    if (trackerPositionY <= VRComponentsPositionInPlatformSpace.y + VRComponentsModelSizeInMm.y / 2 + interfacePlatformHeightInMm * 2
        //        && trackerPositionY >= VRComponentsPositionInPlatformSpace.y - VRComponentsModelSizeInMm.y / 2 - interfacePlatformHeightInMm * 2)
        //        // Within boundary of the virtual model of the interface
        //        return true;
        //    else
        //    // Outside in the y direction
        //    {
        //        UILogging.Warning("InterfaceMover::IsTrackerWithinVIrtualModel: outside of model in Y direction");
        //        return false;
        //    }
        //else
        //{
        //    // Outside in the x direction
        //    UILogging.Warning("InterfaceMover::IsTrackerWithinVIrtualModel: outside of model in X direction");
        //    return false;
        //}
        return false;
    }

    private bool CompareCurrentAndPreviousPlatformPosition()
    {
        // temp convert both to vector3int
        Vector3Int roundedPreviousPlatformPosition = Vector3Int.RoundToInt(previousPositionPlatform);
        Vector3Int roundedCurrentPlatformPosition = Vector3Int.RoundToInt(currentPosition);
        if (roundedPreviousPlatformPosition.Equals(roundedCurrentPlatformPosition))
        {
            previousPositionMatchCounter++;
            if (previousPositionMatchCounter.Equals(REQUIRED_NUMBER_OF_MATCHING_FRAMES))
                return true;
            else
                return false;
        }
        else
        {
            previousPositionMatchCounter = 0;
            return false;
        }
    }


    private void CheckAndAdjustPlatformPosition()
    {
        /* ALGORITHM: 
         * Compare current and desired position.
         * 
         */
        float movementDriftCompensation = 5f;
        float horizontalDifference = Math.Abs(desiredPosition.x - currentPosition.x); ;
        // Compensation for axis switch in certain orientations. 
        float verticalDifference = Math.Abs(desiredPosition.y - currentPosition.y);

        // Only adjust if the offset is bigger then 5 mm.
        if (horizontalDifference >= movementDriftCompensation || verticalDifference >= movementDriftCompensation)
        {
            // Calculate current position of the platform in steps
            Vector3 currentPositionInPlatformMM = new Vector3(currentPosition.x - X_OFFSET_MM, currentPosition.y - Y_OFFSET_MM, 0); ;
            int currentPositionStepsHorizontal = (int)Math.Round(currentPositionInPlatformMM.x * MM_TO_STEPS_FACTOR_X); ;
            int currentPositionStepsVertical = (int)Math.Round(currentPositionInPlatformMM.y * MM_TO_STEPS_FACTOR_Y);

            // Only chance if new step position > 0 and the position difference is >= 2
            int stepDifferenceHorizontal = previousStepPositionPlatformHorizontal - currentPositionStepsHorizontal;
            int stepDifferenceVertical = previousStepPositionPlatformVertical - currentPositionStepsVertical;
            if (currentPositionStepsHorizontal > 0 && currentPositionStepsVertical > 0 && stepDifferenceHorizontal >= 2 || stepDifferenceVertical >= 2)
            {
                // TESTING:
                UILogging.Info("InterfaceMover::AdjustPosition: Position updated: ({0} ; {1}) from previousStepPosition: ({2} ; {3}) based on current position: {4}", currentPositionStepsHorizontal, currentPositionStepsVertical, previousStepPositionPlatformHorizontal, previousStepPositionPlatformVertical, currentPosition.ToString());

                // Set the platform step position
                if (!SetPlatformStepPosition(currentPositionStepsHorizontal, currentPositionStepsVertical))
                {
                    UILogging.Error("InterfaceMover::AdjustPosition: Cannot update the platform step position: ({0} ; {1})", currentPositionStepsHorizontal, currentPositionStepsVertical);
                }
                else
                {
                    PlatformPositionAdjusted = true; // only change the position once per arrived movement.
                    UILogging.Info("InterfaceMover::AdjustPosition: Position updated: ({0} ; {1}) from position: {2}", currentPositionStepsHorizontal, currentPositionStepsVertical, currentPosition.ToString());
                }
            }

            previousStepPositionPlatformHorizontal = currentPositionStepsHorizontal;
            previousStepPositionPlatformVertical = currentPositionStepsVertical;
        }
    }

    [DllImport("UnityToHardwareMoverBridgePlugin")] private static extern bool StartDevice(int comPortNumber);
	[DllImport("UnityToHardwareMoverBridgePlugin")] private static extern void StopDevice();
	[DllImport("UnityToHardwareMoverBridgePlugin")] private static extern IntPtr GetDebugMessages();
	[DllImport("UnityToHardwareMoverBridgePlugin")] private static extern IntPtr GetErrorMessages();
	[DllImport("UnityToHardwareMoverBridgePlugin")] private static extern IntPtr GetRenderEventFunc();

	[DllImport("UnityToHardwareMoverBridgePlugin")]
	private static extern bool MovePlatform(float xPositionInMM, float yPositionInMM, float mmToStepsFactorX, float mmToStepsFactorY/*, float xOffsetInMM, float yOffsetInMM*/);
    [DllImport("UnityToHardwareMoverBridgePlugin")]
    private static extern bool SetPlatformStepPosition(int stepPositionX, int stepPositionY);

}