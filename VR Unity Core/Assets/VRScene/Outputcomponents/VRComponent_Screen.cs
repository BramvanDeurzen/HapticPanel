using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Attach this to a gui textbox, and set the options there (eg monospace, lines, ...)
public class VRComponent_Screen : VROutputComponent {

	TextMesh textbox;

	// Use this for initialization
	public new void Awake () {
		base.Awake();
		noRendering = false;
		if(textbox == null) {
			textbox = GetComponent<TextMesh>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override void setText(string ID, string text) {
		if (ID == this.ID)
			textbox.text = text;
	}
}
