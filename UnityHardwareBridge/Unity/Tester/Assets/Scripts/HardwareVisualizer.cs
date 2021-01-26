using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class HardwareVisualizer : MonoBehaviour {

	public delegate void buttonPushCallbackDelegate(int port, bool status);

	bool[] buttonstatus = new bool[10];
	GameObject[] visualizers = new GameObject[10];
	Renderer[] visualizerrenderers = new Renderer[10];
	buttonPushCallbackDelegate buttonPushCallback; //has to be saved or the callback gets garbage collected
	public int comPort { get; set; }
	public string comPortString { set { comPort = int.Parse(value); } }

	public HardwareVisualizer() {
		comPort = -1;
	}

	void Start() {
		for (int i = 0; i < 10; ++i) {
			GameObject o = GameObject.CreatePrimitive(PrimitiveType.Cube);
			o.transform.position = new Vector3(i * 1.2f - (10 / 2.0f) * 1.2f, 0, 0);
			o.transform.localScale = new Vector3(1, 1, 1);
			visualizers[i] = o;
			visualizerrenderers[i] = o.GetComponent<Renderer>();
		}

		//yield return StartCoroutine("CallPluginAtEndOfFrames");
	}

	void OnDestroy() {
		StopDevice();
	}

	void Update() {
		{
			IntPtr errorStringPtr = GetErrorMessages();
			string errorString = Marshal.PtrToStringAnsi(errorStringPtr);

			if (!string.IsNullOrEmpty(errorString))
				Debug.LogError(errorString);
		}

		{
			IntPtr errorStringPtr = GetDebugMessages();
			string errorString = Marshal.PtrToStringAnsi(errorStringPtr);

			if (!string.IsNullOrEmpty(errorString))
				Debug.LogWarning(errorString);
		}

		for (int i = 0; i < 10; ++i) {
			if (buttonstatus[i]) {
				visualizerrenderers[i].material.color = Color.green;
			} else {
				visualizerrenderers[i].material.color = Color.red;
			}
		}
	}

	private IEnumerator CallPluginAtEndOfFrames() {
		while (true) {
			// Wait until all frame rendering is done
			yield return new WaitForEndOfFrame();
			//GL.IssuePluginEvent(GetRenderEventFunc(), 1);

			{
				IntPtr errorStringPtr = GetErrorMessages();
				string errorString = Marshal.PtrToStringAnsi(errorStringPtr);

				if (!string.IsNullOrEmpty(errorString))
					Debug.LogError(errorString);
			}

			{
				IntPtr errorStringPtr = GetDebugMessages();
				string errorString = Marshal.PtrToStringAnsi(errorStringPtr);

				if (!string.IsNullOrEmpty(errorString))
					Debug.LogWarning(errorString);
			}
		}
	}

	void onHardwarePress(int port, bool status) {
		try {
			buttonstatus[port] = status;
		} catch(Exception ex) {
			Debug.LogErrorFormat("Callback exception {0}", ex.ToString());
		}
	}
	
	public void Init() {
		if (comPort < 0) {
			Debug.LogError("Please enter a valid com port number");
			return;
		}

		buttonPushCallback = new buttonPushCallbackDelegate(onHardwarePress);

		if (!StartDevice(comPort))
			Debug.LogError("Cannot activate the hardware interface");
		else {
			if (!ActivateButton(2, buttonPushCallback))
				Debug.LogError("Cannot activate button 2");
			if (!ActivateButton(3, buttonPushCallback))
				Debug.LogError("Cannot activate button 3");
		}
	}

	[DllImport("HardwareInterfaceToUnityBridgePlugin")] private static extern bool StartDevice(int comPortNumber);
	[DllImport("HardwareInterfaceToUnityBridgePlugin")] private static extern void StopDevice();
	[DllImport("HardwareInterfaceToUnityBridgePlugin")] private static extern bool ActivateButton(int port, buttonPushCallbackDelegate callback);
	[DllImport("HardwareInterfaceToUnityBridgePlugin")] private static extern IntPtr GetDebugMessages();
	[DllImport("HardwareInterfaceToUnityBridgePlugin")] private static extern IntPtr GetErrorMessages();
	[DllImport("HardwareInterfaceToUnityBridgePlugin")] private static extern IntPtr GetRenderEventFunc();

}
