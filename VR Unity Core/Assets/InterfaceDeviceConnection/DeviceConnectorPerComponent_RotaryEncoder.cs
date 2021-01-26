using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class DeviceConnectorPerComponent_RotaryEncoder : DeviceConnectorPerComponent
{
    //const int maxButtons = 50;
    //bool[] buttonstatus = new bool[maxButtons];
    public rotaryEncoderCallbackDelegate rotaryEncoderCallback; //has to be saved or the callback gets garbage collected
    public buttonPushCallbackDelegate rotaryEncoderButtonCallback;

    //public enum rotaryEncoderCallbackMethod_t { ALWAYS, CHANGE, PRESS, RELEASE };
    class componentSettings 
    {
        //public rotaryEncoderCallbackMethod_t callbackMethod;
        public int callInNextUpdate;
        public int rotationValue;
        public bool status;
    }
    //RotaryEncoder
    Dictionary<int, rotaryEncoderCallbackDelegate> rotaryEncoderCallbacks = new Dictionary<int, rotaryEncoderCallbackDelegate>();
    Dictionary<int, componentSettings> rotaryEncoderSettings = new Dictionary<int, componentSettings>();
    //Button from rotary encoder
    Dictionary<int, buttonPushCallbackDelegate> rotaryEncoderButtonCallbacks = new Dictionary<int, buttonPushCallbackDelegate>();
    Dictionary<int, componentSettings> rotaryEncoderButtonSettings = new Dictionary<int, componentSettings>();

    public void Update()
    {
        lock (rotaryEncoderSettings)
        {
            // Do all the rotary encoder rotation
            foreach (KeyValuePair<int, componentSettings> setting in rotaryEncoderSettings)
            {
                for (int call = 0; call < setting.Value.callInNextUpdate; ++call)
                {
                    rotaryEncoderCallbacks[setting.Key](setting.Key, setting.Value.rotationValue);
                }
                setting.Value.callInNextUpdate = 0;
            }
            
            // Do all the rotary encoder buttons
            foreach (KeyValuePair<int, componentSettings> setting in rotaryEncoderButtonSettings)
            {
                for (int call = 0; call < setting.Value.callInNextUpdate; ++call)
                {
                    rotaryEncoderButtonCallbacks[setting.Key](setting.Key, setting.Value.status);
                }
                setting.Value.callInNextUpdate = 0;
            }
        }
    }

    //register, but do not activate on the hardware. Init will do that
    /**
     * Register a rotary encoder
     * @Pre: port, port + 1 == rotation, port + 2 == rotary encoder button
     */
    public void RegisterRotaryEncoder(int port, rotaryEncoderCallbackDelegate callback, buttonPushCallbackDelegate callbackButton)
    {
        lock (rotaryEncoderSettings)
        {
            rotaryEncoderCallbacks[port] = callback;
            rotaryEncoderSettings[port] = new componentSettings();
            rotaryEncoderSettings[port].callInNextUpdate = 1;
        }
        lock (rotaryEncoderButtonSettings)
        {
            rotaryEncoderButtonCallbacks[port] = callbackButton;
            rotaryEncoderButtonSettings[port] = new componentSettings();
            rotaryEncoderButtonSettings[port].callInNextUpdate = 1;
        }
    }

    void onRotationChange(int port, int rotationValue)
    {
        try
        {
            lock (rotaryEncoderSettings)
            {
                if (rotaryEncoderSettings[port].rotationValue != 0) // Only update when value has changed. So not zero
                    rotaryEncoderSettings[port].callInNextUpdate += 1;
                // Update the value.
                rotaryEncoderSettings[port].rotationValue = rotationValue;
            }
            //buttonstatus[port] = status;
        }
        catch (Exception ex)
        {
            UILogging.Error("OnRotationChange:Callback exception {0}", ex.ToString());
        }
    }

    /**
     * Because the rotary encoder is indicitated by its first port, but uses 3 in total. The first port and the next 2 are always used.
     * So the rotary encoder is saved on its first port.
     * But given port is the real port number of the button. So this is port + 2. To find the port in settings port - 2 is done.
     */
    void onRotaryButtonPress(int port, bool status)
    {
        try
        {
            lock (rotaryEncoderButtonSettings)
            {
                if (rotaryEncoderButtonSettings[port - 2].status != status) // Only update when value has changed.
                {
                    //UILogging.Info("RotaryButton status: {0}, status: {1}", rotaryEncoderButtonSettings[port - 2].status, status);
                    rotaryEncoderButtonSettings[port - 2].callInNextUpdate += 1;
                }
                // Update the value.
                rotaryEncoderButtonSettings[port - 2].status = status;
            }
            //buttonstatus[port] = status;
        }
        catch (Exception ex)
        {
            UILogging.Error("OnRotaryButtonPress:Callback exception: port ( -2 in function, debugs real port number): {0}, status: {1}\n. Error: {2}", port, status, ex.ToString());
        }
    }

    public void Init()
    {
        // Init rotary encoder rotation callback
        rotaryEncoderCallback = new rotaryEncoderCallbackDelegate(onRotationChange);

        foreach (KeyValuePair<int, componentSettings> setting in rotaryEncoderSettings)
        {
            if (!ActivateRotaryEncoder(setting.Key, setting.Key+1, rotaryEncoderCallback))
                UILogging.Error("Cannot activate rotary encoder {0}", setting);
        }

        // Init rotary encoder button callback

        rotaryEncoderButtonCallback = new buttonPushCallbackDelegate(onRotaryButtonPress);

        foreach (KeyValuePair<int, componentSettings> setting in rotaryEncoderButtonSettings)
        {
            if (!ActivateButton(setting.Key + 2, rotaryEncoderButtonCallback))
                UILogging.Error("Cannot activate rotary encoder button {0}", setting);
        }
    }

    [DllImport("HardwareInterfaceToUnityBridgePlugin")]
    private static extern bool ActivateRotaryEncoder(int port1, int port2, rotaryEncoderCallbackDelegate onRotationChange);
    [DllImport("HardwareInterfaceToUnityBridgePlugin")]
    private static extern bool ActivateButton(int port, buttonPushCallbackDelegate callback);
}