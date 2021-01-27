using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRComponent_RotaryEncoder : VRInputComponent {

	[Tooltip("Set when you also want keyboard bindings")]
	public KeyCode keyboardKey = KeyCode.None;

    public override void setRotationValue(int value)
    {
        foreach (InputReceiver r in receiver)
        {
            // Transform value == 255 to -1 (is 255 because of the unsigned byte that is used)
            if (value == 255)
                value = -1;
            r.onValueChange(ID, value);
        }
            
    }

    // TEST
    public override void setButtonStatus(bool status)
    {
        foreach (InputReceiver r in receiver)
        {
            Debug.Log("VRComponent_RotaryEncoder::SetButtonStatus:: ID = " + ID + " status = " + status);
            r.onPress(ID, status);
        }
    }

    public override void setSliderValue(int sliderValue)
    {
        // do nothing with this
    }


    // Update is called once per frame
    void Update() {
        
	}
}
