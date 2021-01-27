using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HardwareInterface {
	public delegate void showInfoDelegate(string info);
	public delegate void triggerDelegate();

	public class InfoShower : MonoBehaviour {
		public triggerDelegate enter;
		public triggerDelegate leave;
		public void OnMouseOver() {
			enter();
		}

		public void OnMouseExit() {
			leave();
		}
	}
	
	public class InterfaceVisualizer {
		GameObject globalParent;
		GameObject movableParent;
		Interface @interface;
		showInfoDelegate showInfo = (string s) => { };

		Dictionary<int, Renderer> buttonRenderers = new Dictionary<int, Renderer>();
		Dictionary<int, Material> buttonMaterialReleased = new Dictionary<int, Material>();
		Dictionary<int, Material> buttonMaterialPressed = new Dictionary<int, Material>();

		public InterfaceVisualizer(GameObject parent, Interface @interface, showInfoDelegate showInfo) {
			globalParent = parent;
			this.@interface = @interface;
			this.showInfo = showInfo;
		}

		public void setButtonStatus(int port, bool status) {
			if (status)
				buttonRenderers[port].material = buttonMaterialPressed[port];
			else
				buttonRenderers[port].material = buttonMaterialReleased[port];
		}

		internal void setPosition(Vector3 pos, Quaternion rot) {
			//todo: is a scaling required here?
			movableParent.transform.localPosition = pos;
			movableParent.transform.localRotation = rot;
		}

		public void Visualize() {
			movableParent = new GameObject();
			movableParent.name = "Global Movable Parent";
			movableParent.transform.parent = globalParent.transform;
			movableParent.transform.localPosition = new Vector3();
			movableParent.transform.localRotation = Quaternion.identity;

			GameObject obj = new GameObject();
			obj.name = "Visualizer Collector";
			obj.transform.parent = movableParent.transform;
			obj.transform.localPosition = new Vector3();
			obj.transform.localRotation = Quaternion.identity;

			foreach (Panel p in @interface.panels) {
				VisualizePanel(p, obj);
			}
		}

		protected void VisualizePanel(Panel p, GameObject parent) {
			GameObject obj = new GameObject();
			obj.name = "Panel";
			obj.transform.parent = parent.transform;
			obj.transform.localPosition = p.position;
			obj.transform.localRotation = p.rotation;

			GameObject obj_scaled = GameObject.CreatePrimitive(PrimitiveType.Cube);
			obj_scaled.name = "Panel visualizer";
			obj_scaled.transform.parent = obj.transform;
			obj_scaled.transform.localScale = p.size;
			Material material = new Material(Shader.Find("Transparent/Diffuse"));
			material.color = Color.gray;
			obj_scaled.GetComponent<Renderer>().material = material;
			obj_scaled.transform.localPosition = new Vector3();
			obj_scaled.transform.localRotation = Quaternion.identity;

			foreach (PanelElement pe in p.elements) {
				VisualizePanelElement(pe, obj, p.size);
			}
		}

		protected void VisualizePanelElement(PanelElement p, GameObject parent, Vector3 panelsize) {
			GameObject obj = new GameObject();
			obj.name = "Panel element";
			obj.transform.parent = parent.transform;
			obj.transform.localPosition = new Vector3(p.position.x, p.position.y, -(p.size.z + panelsize.z) / 2);
			obj.transform.localRotation = p.rotation;

			GameObject obj_scaled = GameObject.CreatePrimitive(PrimitiveType.Cube);
			obj.name = "Panel Element Visualizer";
			obj_scaled.transform.parent = obj.transform;
			obj_scaled.transform.localPosition = Vector3.zero;
			obj_scaled.transform.localScale = p.size;
			Material material = new Material(Shader.Find("Transparent/Diffuse"));
			Color parsedcolor = Color.white;
			ColorUtility.TryParseHtmlString("#" + p.color, out parsedcolor);
			material.color = parsedcolor;
			obj_scaled.GetComponent<Renderer>().material = material;

			

			if (p.type.ToLower() == "button") {
				buttonRenderers[p.port] = obj_scaled.GetComponent<Renderer>();
				Material materialpress = new Material(Shader.Find("Transparent/Diffuse"));
				materialpress.color = Color.blue;
				buttonMaterialPressed[p.port] = materialpress;
				buttonMaterialReleased[p.port] = material;
			}

			InfoShower shower = obj_scaled.AddComponent<InfoShower>();
			shower.enter = () => { showInfo("ID: " + p.id + "; port: " + p.port); };
			shower.leave = () => { showInfo(""); };
		}
	}
}
