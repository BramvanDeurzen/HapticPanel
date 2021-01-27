using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Connect the device with the interface description
//todo: remove delegate, replace with responsibiliy in visualizer
public class InterfaceDeviceConnector {
	HardwareInterface.Interface @interface;
	DeviceConnector device;
	buttonPushCallbackDelegate setbuttonstatus;
    rotaryEncoderCallbackDelegate setRotationValue;
    sliderCallbackDelegate setSliderValue;

	public InterfaceDeviceConnector(HardwareInterface.Interface @interface, DeviceConnector device, buttonPushCallbackDelegate setbuttonstatus, rotaryEncoderCallbackDelegate setRotationValue, sliderCallbackDelegate setSliderValue) {
		this.@interface = @interface;
		this.device = device;
		this.setbuttonstatus = setbuttonstatus;
        this.setRotationValue = setRotationValue;
        this.setSliderValue = setSliderValue;
	}

	void onHardwarePress(int port, bool status) {
		setbuttonstatus(port, status);
	}

    void onRotationChange(int port, int rotationValue)
    {
        setRotationValue(port, rotationValue);
    }

    void onSliderValueChange(int port, int sliderValue)
    {
        setSliderValue(port, sliderValue);
    }

	public void ConnectComponents() {
		foreach (HardwareInterface.Panel panel in @interface.panels) {
			foreach(HardwareInterface.PanelElement elem in panel.elements) {
				if(elem.type.ToLower() == "button") {
					device.RegisterButton(elem.port, new buttonPushCallbackDelegate(onHardwarePress), DeviceConnectorPerComponent_Button.buttonCallbackMethod_t.CHANGE);
				}
                else if(elem.type.ToLower() == "rotaryencoder")
                {
                    device.RegisterRotaryEncoder(elem.port, new rotaryEncoderCallbackDelegate(onRotationChange), new buttonPushCallbackDelegate(onHardwarePress));
                }
                if(elem.type.ToLower() == "slider")
                {
                    device.RegisterSlider(elem.port, new sliderCallbackDelegate(onSliderValueChange));
                }
			}
		}
	}
}
