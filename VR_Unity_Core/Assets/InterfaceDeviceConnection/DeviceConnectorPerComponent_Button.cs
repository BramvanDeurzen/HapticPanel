using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class DeviceConnectorPerComponent_Button: DeviceConnectorPerComponent {
	const int maxButtons = 50;
	bool[] buttonstatus = new bool[maxButtons];
	public buttonPushCallbackDelegate buttonPushCallback; //has to be saved or the callback gets garbage collected

	public enum buttonCallbackMethod_t { ALWAYS, CHANGE, PRESS, RELEASE }; // Used to determine the button type? 
	class componentSettings {
		public buttonCallbackMethod_t callbackMethod;
		public int callInNextUpdate;
		public bool status;
	}
	//buttons
	Dictionary<int, buttonPushCallbackDelegate> buttonCallbacks = new Dictionary<int, buttonPushCallbackDelegate>();
	Dictionary<int, componentSettings> buttonSettings = new Dictionary<int, componentSettings>();

	public void Update() {
		lock (buttonSettings) {
			foreach (KeyValuePair<int, componentSettings> setting in buttonSettings) {
				for (int call = 0; call < setting.Value.callInNextUpdate; ++call) {
					buttonCallbacks[setting.Key](setting.Key, setting.Value.status);
				}
				setting.Value.callInNextUpdate = 0;
			}
		}
	}

	//register, but do not activate on the hardware. Init will do that
	public void RegisterButton(int port, buttonPushCallbackDelegate callback, buttonCallbackMethod_t callbackMethod) {
		lock (buttonSettings) {
			buttonCallbacks[port] = callback;
			buttonSettings[port] = new componentSettings();
			buttonSettings[port].callbackMethod = callbackMethod;
			buttonSettings[port].callInNextUpdate = 1;
		}
	}

	void onHardwarePress(int port, bool status) {
		try {
			lock (buttonSettings) {
				if (buttonSettings[port].callbackMethod == buttonCallbackMethod_t.CHANGE && buttonSettings[port].status != status) {
					buttonSettings[port].callInNextUpdate += 1;
				} else if (buttonSettings[port].callbackMethod == buttonCallbackMethod_t.PRESS && buttonSettings[port].status != status && status == true) {
					buttonSettings[port].callInNextUpdate += 1;
				} else if (buttonSettings[port].callbackMethod == buttonCallbackMethod_t.RELEASE && buttonSettings[port].status != status && status == false) {
					buttonSettings[port].callInNextUpdate += 1;
				} else if (buttonSettings[port].callbackMethod == buttonCallbackMethod_t.ALWAYS) {
					buttonSettings[port].callInNextUpdate = 1;
				}
				buttonSettings[port].status = status;
			}
			buttonstatus[port] = status;
		} catch (Exception ex) {
			UILogging.Error("onHardwarePress:Callback exception {0}", ex.ToString());
		}
	}

	public void Init() {
		buttonPushCallback = new buttonPushCallbackDelegate(onHardwarePress);

		foreach (KeyValuePair<int, componentSettings> setting in buttonSettings) {
			if (!ActivateButton(setting.Key, buttonPushCallback))
				UILogging.Error("Cannot activate button {0}", setting);
		}
	}

	[DllImport("HardwareInterfaceToUnityBridgePlugin")]
	private static extern bool ActivateButton(int port, buttonPushCallbackDelegate callback);

}