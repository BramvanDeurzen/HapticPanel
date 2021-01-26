using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class DeviceConnectorPerComponent_Slider: DeviceConnectorPerComponent {
	public sliderCallbackDelegate sliderCallback; //has to be saved or the callback gets garbage collected


	class componentSettings {
		public int callInNextUpdate;
		public int sliderValue;
	}
	//buttons
	Dictionary<int, sliderCallbackDelegate> sliderCallbacks = new Dictionary<int, sliderCallbackDelegate>();
	Dictionary<int, componentSettings> sliderSettings = new Dictionary<int, componentSettings>();

	public void Update() {
		lock (sliderSettings) {
			foreach (KeyValuePair<int, componentSettings> setting in sliderSettings) {
                //Debug.Log("DeviceConnectorPerComponent_Slider::Update: foreach setting.key:" +  setting.Key + " in slidersettings");
                for (int call = 0; call < setting.Value.callInNextUpdate; ++call) {
                    //Debug.Log("DeviceConnectorPerComponent_Slider::Update: In Call next update, slidervalue = " + setting.Value.sliderValue);
					sliderCallbacks[setting.Key](setting.Key, setting.Value.sliderValue);
				}
				setting.Value.callInNextUpdate = 0;
			}
		}
	}

	//register, but do not activate on the hardware. Init will do that
	public void RegisterSlider(int port, sliderCallbackDelegate callback) {
		lock (sliderSettings) {
            Debug.Log("DeviceConnectorPerComponent_Slider:: RegisterSlider on port:" + port);
            sliderCallbacks[port] = callback;
			sliderSettings[port] = new componentSettings();
			sliderSettings[port].callInNextUpdate = 1;
		}
	}

	void onSliderValueChange(int port, int value) {
		try {
			lock (sliderSettings) {
                // Only update when the value changed
                if (sliderSettings[port].sliderValue != value)
                {
                    //Debug.Log("DeviceConnectorPerComponent_Slider::OnSliderValueChange: on port:" + port + " value: " + value);
                    sliderSettings[port].sliderValue = value;
                    sliderSettings[port].callInNextUpdate += 1;
                }
			}
			//buttonstatus[port] = status;
		} catch (Exception ex) {
			UILogging.Error("DeviceConnectorPerComponent_Slider::onSliderValueChange::Callback exception {0}", ex.ToString());
		}
	}

	public void Init() {
        sliderCallback = new sliderCallbackDelegate(onSliderValueChange);

		foreach (KeyValuePair<int, componentSettings> setting in sliderSettings) {
            if (!ActivateSlider(setting.Key, sliderCallback))
                UILogging.Error("Cannot activate slider {0}", setting);
            else
                Debug.Log("DeviceConnectorPerComponent_Slider::Init: Slider " + setting.Key + " activiated");
		}
	}

	[DllImport("HardwareInterfaceToUnityBridgePlugin")]
	private static extern bool ActivateSlider(int port, sliderCallbackDelegate callback);

}