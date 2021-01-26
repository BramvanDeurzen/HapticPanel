using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Only send hardware events to the right VRComponent, based on position and type
//InterfacePositioner knows which button is active
//Use the hardware ID, there is no info about ports, etc
public class HardwareEventDispatcher {
	InterfacePositioner positioner;
	Dictionary<string /*VRComponent id*/, VRInputComponent> componentmapper = new Dictionary<string, VRInputComponent>();

    private int previousSliderValue = 0;

	public HardwareEventDispatcher(VRComponentCollection components, InterfacePositioner positioner) {
		this.positioner = positioner;

		foreach (Transform t in components.transform) {
			VRComponent vrc = t.gameObject.GetComponent<VRComponent>();
			if (vrc != null) {
				if (vrc is VRInputComponent) {
					componentmapper[(vrc as VRInputComponent).ID] = (vrc as VRInputComponent);
				}
			}
		}
	}

	public void setButtonStatus(string hardwareID, bool status) {
		UILogging.Info("HardwareEventDispatcher: setButtonStatus:Dispach request for {0} -> {1}", hardwareID, status);
		bool exists;
		string VRComponentID = positioner.getCurrentVRComponentID(out exists);
		if (exists) {
			if (componentmapper.ContainsKey(VRComponentID)) {
				if (componentmapper[VRComponentID].hardwareComponentID == hardwareID)
					componentmapper[VRComponentID].setButtonStatus(status);
				else
					UILogging.Warning("HardwareEventDispatcher:setButtonStatus: Pressed HID {0}, but requested {1} based on VR Component. Ignoring event", hardwareID, componentmapper[VRComponentID].hardwareComponentID);
			} else {
				UILogging.Warning("HardwareEventDispatcher:setButtonStatus: Pressed HID {0}, but VR Component {1} does not exist. Ignoring event", hardwareID, VRComponentID);
			}
		} else {
			UILogging.Warning("HardwareEventDispatcher:setButtonStatus: Pressed HID {0}, but no active VR Component. Ignoring event. This is normal at startup", hardwareID, VRComponentID);
		}
	}

    public void setRotationValue(string hardwareID, int rotationValue)
    {
        UILogging.Info("HardwareEventDispatcher:setRotationValue: Dispach request for {0} -> {1}", hardwareID, rotationValue);
        bool exists;
        string VRComponentID = positioner.getCurrentVRComponentID(out exists);
        if (exists)
        {
            if (componentmapper.ContainsKey(VRComponentID))
            {
                if (componentmapper[VRComponentID].hardwareComponentID == hardwareID)
                    componentmapper[VRComponentID].setRotationValue(rotationValue);
                else
                    UILogging.Warning("HardwareEventDispatcher:setRotationValue: Pressed HID {0}, but requested {1} based on VR Component. Ignoring event", hardwareID, componentmapper[VRComponentID].hardwareComponentID);
            }
            else
            {
                UILogging.Warning("HardwareEventDispatcher:setRotationValue: Pressed HID {0}, but VR Component {1} does not exist. Ignoring event", hardwareID, VRComponentID);
            }
        }
        else
        {
            UILogging.Warning("HardwareEventDispatcher:setRotationValue: Pressed HID {0}, but no active VR Component. Ignoring event. This is normal at startup", hardwareID, VRComponentID);
        }
    }

    public void setSliderValue(string hardwareID, int sliderValue)
    {
        if ((previousSliderValue + 1) == sliderValue || (previousSliderValue - 1) == sliderValue || previousSliderValue == sliderValue)
            return;
        previousSliderValue = sliderValue;
        UILogging.Info("HardwareEventDispatcher:setSliderValue: Dispach request for {0} -> {1}", hardwareID, sliderValue);
        bool exists;
        string VRComponentID = positioner.getCurrentVRComponentID(out exists);
        if (exists)
        {
            if (componentmapper.ContainsKey(VRComponentID))
            {
                if (componentmapper[VRComponentID].hardwareComponentID == hardwareID)
                    componentmapper[VRComponentID].setSliderValue(sliderValue);
                else
                    UILogging.Warning("HardwareEventDispatcher:setSliderValue: Pressed HID {0}, but requested {1} based on VR Component. Ignoring event", hardwareID, componentmapper[VRComponentID].hardwareComponentID);
            }
            else
            {
                UILogging.Warning("HardwareEventDispatcher:setSliderValue: Pressed HID {0}, but VR Component {1} does not exist. Ignoring event", hardwareID, VRComponentID);
            }
        }
        else
        {
            UILogging.Warning("HardwareEventDispatcher:setSliderValue: Pressed HID {0}, but no active VR Component. Ignoring event. This is normal at startup", hardwareID, VRComponentID);
        }
    }
}
