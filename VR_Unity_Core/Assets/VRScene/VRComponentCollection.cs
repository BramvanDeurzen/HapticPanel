using System.Collections.Generic;
using UnityEngine;

//ID-based receiver, hardware-agnostic
public interface InputReceiver {
	void onPress(string ID, bool status);
    void onValueChange(string ID, int value);
    void onSliderChange(string ID, int sliderValue);
}

public interface OutputReceiver {
	void setText(string ID, string text);
	void setStatus(string ID, bool status);
}

//The direct and only parent of all the components
//Todo: remove these restrictions
public class VRComponentCollection : MonoBehaviour, OutputReceiver {

	public DemoID2InterfaceID idmapper;
	public Behaviour behaviour;
    [Tooltip("The model of the VRComponents represents the size in which the components can be placed")]
    //public VRComponent model;

	private List<OutputReceiver> outputreceivers = new List<OutputReceiver>();

	//Use awake, so that all mapped ids are correct when other objects use start
	void Awake () {
		behaviour.receiver = this;
		foreach (Transform t in transform) {
			VRComponent vrc = t.gameObject.GetComponent<VRComponent>();
			if (vrc != null) {
				if (vrc is VRInputComponent) {
					(vrc as VRInputComponent).hardwareComponentID = idmapper.getMappedID((vrc as VRInputComponent).hardwareComponentID);
					(vrc as VRInputComponent).receiver.Add(behaviour);
				}

				if (vrc is OutputReceiver) {
					outputreceivers.Add(vrc as OutputReceiver);
				}
			}

			Renderer r = t.gameObject.GetComponent<Renderer>();
			if(r != null && vrc.noRendering) {
				r.enabled = false;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void setText(string ID, string text) {
		foreach(OutputReceiver oc in outputreceivers) {
			oc.setText(ID, text);
		}
	}

	public void setStatus(string ID, bool status) {
		foreach (OutputReceiver oc in outputreceivers) {
			oc.setStatus(ID, status);
		}
	}

	public VRInputComponent GetClosestInWorldspace(Vector3 from_worldspacePosition, bool inputonly = true) {
		//Todo, use quad tree or similar
		VRInputComponent closest = null;
		foreach (Transform t in transform) {
			VRComponent vrc = t.gameObject.GetComponent<VRComponent>();
			if (vrc != null && (inputonly && vrc is VRInputComponent)) {
				if (closest == null || Vector3.Distance(from_worldspacePosition, t.position) < Vector3.Distance(from_worldspacePosition, closest.transform.position)) {
					closest = vrc as VRInputComponent;
				}
			}
		}

		return closest;
	}

    public Transform GetModelTransform()
    {
        //return model.transform;
        return this.transform;
    }
}
