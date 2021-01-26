using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRComponent_Button : VRInputComponent {

	[Tooltip("Set when you also want keyboard bindings")]
	public KeyCode keyboardKey = KeyCode.None;

	public override void setButtonStatus(bool status) {
        if (status) {
            foreach (InputReceiver r in receiver)
            {
                r.onPress(ID, status);
            }
        }

	}

    // TEST
    public override void setRotationValue(int value)
    {
        // do nothing
    }

    public override void setSliderValue(int sliderValue)
    {
        // do nothing
    }

    // Update is called once per frame
    void Update() {
		if (Input.GetKeyDown(keyboardKey))
			setButtonStatus(true);
		if (Input.GetKeyUp(keyboardKey))
			setButtonStatus(false);
	}
}
