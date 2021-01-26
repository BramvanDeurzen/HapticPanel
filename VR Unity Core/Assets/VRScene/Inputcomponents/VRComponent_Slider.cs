using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRComponent_Slider : VRInputComponent {

	[Tooltip("Set when you also want keyboard bindings")]
	public KeyCode keyboardKey = KeyCode.None;

	public override void setButtonStatus(bool status) {
        // do nothing

	}

    public override void setRotationValue(int value)
    {
        // do nothing
    }

    public override void setSliderValue(int sliderValue)
    {
        foreach (InputReceiver r in receiver)
            r.onSliderChange(ID, sliderValue);
    }

    // Update is called once per frame
    void Update() {

	}
}
