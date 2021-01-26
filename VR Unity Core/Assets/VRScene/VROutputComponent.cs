using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VROutputComponent : VRComponent, OutputReceiver {
	public virtual void setText(string ID, string text) {
	}

	public virtual void setStatus(string ID, bool status) {
	}
}
