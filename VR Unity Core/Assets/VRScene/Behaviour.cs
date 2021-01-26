using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behaviour : MonoBehaviour, InputReceiver {

	public OutputReceiver receiver;

	public virtual void onPress(string ID, bool status) {
	}

    public virtual void onValueChange(string ID, int rotationValue)
    {

    }

    public virtual void onSliderChange(string ID, int sliderValue)
    {

    }
}
