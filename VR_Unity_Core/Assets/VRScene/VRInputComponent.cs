using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInputComponent : VRComponent {

	[HideInInspector]
	public List<InputReceiver> receiver = new List<InputReceiver>();
	public string hardwareComponentID;

	public virtual void setButtonStatus(bool status) {
	}

    public virtual void setRotationValue(int value)
    {

    }

    public virtual void setSliderValue(int sliderValue)
    {

    }

	void Update() {
	}
}
