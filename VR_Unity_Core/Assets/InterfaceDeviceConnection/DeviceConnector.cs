using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public delegate void buttonPushCallbackDelegate(int port, bool status);
public delegate void rotaryEncoderCallbackDelegate(int port, int rotationValue);
public delegate void sliderCallbackDelegate(int port, int sliderValue);

interface DeviceConnectorPerComponent {
	void Init();
	void Update();
}

//WARNING: Callbacks are out of the main thread and should not edit any unity-owned object
public class DeviceConnector : MonoBehaviour {

	Dictionary<InputHardwareComponentType, DeviceConnectorPerComponent> componentTypeHandlers = new Dictionary<InputHardwareComponentType, DeviceConnectorPerComponent>();

	public enum InputHardwareComponentType {
		BUTTON,
        ROTARY_ENCODER,
        SLIDER
	}

	
	public int comPort;
	public string comPortString {
		set {
			comPort = int.Parse(value);
		}
	}

	public DeviceConnector() {
		//comPort = -1;
		componentTypeHandlers[InputHardwareComponentType.BUTTON] = new DeviceConnectorPerComponent_Button();
        componentTypeHandlers[InputHardwareComponentType.ROTARY_ENCODER] = new DeviceConnectorPerComponent_RotaryEncoder();
        componentTypeHandlers[InputHardwareComponentType.SLIDER] = new DeviceConnectorPerComponent_Slider();
    }

	void Start() {
	}

	void OnDestroy() {
		StopDevice();
	}

	public void RegisterButton(int port, buttonPushCallbackDelegate callback, DeviceConnectorPerComponent_Button.buttonCallbackMethod_t callbackMethod) {
		(componentTypeHandlers[InputHardwareComponentType.BUTTON] as DeviceConnectorPerComponent_Button).RegisterButton(port, callback, callbackMethod);
	}

    public void RegisterRotaryEncoder(int port, rotaryEncoderCallbackDelegate callback, buttonPushCallbackDelegate callbackButton)
    {
        (componentTypeHandlers[InputHardwareComponentType.ROTARY_ENCODER] as DeviceConnectorPerComponent_RotaryEncoder).RegisterRotaryEncoder(port, callback, callbackButton);
    }

    public void RegisterSlider(int port, sliderCallbackDelegate callback)
    {
        (componentTypeHandlers[InputHardwareComponentType.SLIDER] as DeviceConnectorPerComponent_Slider).RegisterSlider(port, callback);
    }

	void Update() {
		foreach(KeyValuePair<InputHardwareComponentType, DeviceConnectorPerComponent> comptype in componentTypeHandlers) {
			comptype.Value.Update();
		}
        try
        {
            IntPtr errorStringPtr = GetErrorMessages();
            string errorString = Marshal.PtrToStringAnsi(errorStringPtr);

            if (!string.IsNullOrEmpty(errorString))
                UILogging.Error(errorString);

            IntPtr debugStringPtr = GetDebugMessages();
            string debugString = Marshal.PtrToStringAnsi(debugStringPtr);

            if (!string.IsNullOrEmpty(debugString))
                UILogging.Info(debugString);
        }
        catch (ExecutionEngineException e)
        {
            UILogging.Error("DeviceConnector::Update: PtrToString ExecutionEngineException: {0}", e.ToString());
        }
        catch (Exception e)
        {
            UILogging.Error("DeviceConnector::Update: PtrToString generic Exception: {0}", e.ToString());
        }
		
	}
	
	public void Init() {

		if (comPort < 0) {
			UILogging.Error("Please enter a valid com port number");
			return;
		}

		if (!StartDevice(comPort))
			UILogging.Error("Cannot activate the hardware interface (input) on  COM port {0}", comPort);
		else {
            foreach (KeyValuePair<InputHardwareComponentType, DeviceConnectorPerComponent> comptype in componentTypeHandlers)
            {
                comptype.Value.Init();
            }

        }

	}

    [DllImport("HardwareInterfaceToUnityBridgePlugin")] private static extern bool StartDevice(int comPortNumber);
	[DllImport("HardwareInterfaceToUnityBridgePlugin")] private static extern void StopDevice();
	[DllImport("HardwareInterfaceToUnityBridgePlugin")] private static extern IntPtr GetDebugMessages();
	[DllImport("HardwareInterfaceToUnityBridgePlugin")] private static extern IntPtr GetErrorMessages();
	[DllImport("HardwareInterfaceToUnityBridgePlugin")] private static extern IntPtr GetRenderEventFunc();
}
