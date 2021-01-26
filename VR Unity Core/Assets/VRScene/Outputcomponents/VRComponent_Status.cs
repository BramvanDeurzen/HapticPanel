using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRComponent_Status : VROutputComponent {

	public Material on;
	public Material off;
	Renderer thisrenderer;

	public new void Awake() {
		base.Awake();
		noRendering = false;
		thisrenderer = GetComponent<Renderer>();
		setStatus(ID, false);
	}
	public override void setStatus(string ID, bool status) {
		if (this.ID == ID) {
			if (status)
				thisrenderer.material = on;
			else
				thisrenderer.material = off;
		}
	}
}
