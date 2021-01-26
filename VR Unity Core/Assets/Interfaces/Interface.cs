using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.IO;

//Scale and position are in millimeters
//these are not unity positions! Unity is not a CAD program
namespace HardwareInterface {

	[Serializable]
	public class Interface {
		public string name;
		public List<Panel> panels = new List<Panel>();

		public string fromPortToHardwareID(int port, out bool found) {
			bool foundchild = false;
			foreach(Panel p in panels) {
				string id = p.fromPortToHardwareID(port, out foundchild);
				if (foundchild) {
					found = true;
					return id;
				}
			}
			found = false;
			return "";
		}

		public bool GetPosRotOfHardwareID(string HardwareID, out Vector3 position, out Quaternion rotation) {
			foreach(Panel p in panels) {
				if(p.GetPosRotOfID(HardwareID, out position, out rotation)) 
					return true;
			}
			position = Vector3.zero;
			rotation = Quaternion.identity;
			return false;
		}
	}

	[Serializable]
	public class Panel {
		public Vector3 position = new Vector3(); //the position of the center of the panel on which the components are placed (on the surface) (todo, now middle point)
		public Quaternion rotation = Quaternion.identity;
		public Vector3 size = new Vector3(1, 1, 1); //x, y is the dimensions of the workable panel, z the height
		public List<PanelElement> elements = new List<PanelElement>();

		public string fromPortToHardwareID(int port, out bool found) {
			bool foundchild = false;
			foreach (PanelElement p in elements) {
				string id = p.fromPortToHardwareID(port, out foundchild);
				if (foundchild) {
					found = true;
					return id;
				}
			}
			found = false;
			return "";
		}

		public bool GetPosRotOfID(string ID, out Vector3 position, out Quaternion rotation) {
			foreach(PanelElement elem in elements) {
				if(elem.GetPosRotOfID(ID, out position, out rotation)) {
					position += this.position;
					rotation *= this.rotation;
					return true;
				}
			}
			position = Vector3.zero;
			rotation = Quaternion.identity;
			return false;
		}
	}

	[Serializable]
	public class PanelElement {
		public string id = "";
		public string type = "none";
		public Vector3 position = new Vector2(); //2D position on the panel, center in the middle
		public Quaternion rotation = Quaternion.identity;
		public Vector3 size; //Size with x, y the dimensions according to the panel, z the height
		public string color = "ffffff";
		public int port;

		public string fromPortToHardwareID(int port, out bool found) {
			if (port == this.port) {
				found = true;
				return id;
			} else {
				found = false;
				return "";
			}
		}

		public bool GetPosRotOfID(string ID, out Vector3 position, out Quaternion rotation) {
			if (ID == this.id) {
				position = new Vector3(this.position.x, this.position.y, size.z);
				rotation = this.rotation;
				return true;
			} else {
				position = Vector3.zero;
				rotation = Quaternion.identity;
				return false;
			}
		}
	}
}
