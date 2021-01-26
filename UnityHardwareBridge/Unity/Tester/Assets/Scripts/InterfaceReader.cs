using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;



//Scale and position are in meters
//these are not unity positions! Unity is not a CAD program
namespace HardwareInterface {


	public delegate void showInfoDelegate(string info);


	[Serializable]
	public class Interface {
		public List<Panel> panels = new List<Panel>();

		public void Visualize(GameObject parent, showInfoDelegate showInfo) {
			GameObject obj = new GameObject();
			obj.transform.parent = parent.transform;

			foreach(Panel p in panels) {
				p.Visualize(obj, showInfo);
			}
		}
	}

	[Serializable]
	public class Panel {
		public Vector3 position = new Vector3(); //the position of the center of the panel on which the components are placed (todo, now middle point)
		public Quaternion rotation = Quaternion.identity;
		public Vector3 size = new Vector3(1, 1, 1); //x, y is the dimensions of the workable panel, z the height
		public List<PanelElement> elements = new List<PanelElement>();

		public void Visualize(GameObject parent, showInfoDelegate showInfo) {
			GameObject obj = new GameObject();
			obj.transform.parent = parent.transform;
			obj.transform.localPosition = position;
			obj.transform.localRotation = rotation;

			GameObject obj_scaled = GameObject.CreatePrimitive(PrimitiveType.Cube);
			obj_scaled.transform.parent = obj.transform;
			obj_scaled.transform.localScale = size;
			Material material = new Material(Shader.Find("Transparent/Diffuse"));
			material.color = Color.gray;
			obj_scaled.GetComponent<Renderer>().material = material;

			foreach (PanelElement p in elements) {
				p.Visualize(obj, size, showInfo);
			}
		}
	}

	[Serializable]
	public class PanelElement {
		public string id = "";
		public string type = "none";
		public Vector3 position = new Vector2(); //2D position on the panel
		public Quaternion rotation = Quaternion.identity;
		public Vector3 size; //Size with x, y the dimensions according to the panel, z the height
		public string color = "ffffff";

		public void Visualize(GameObject parent, Vector3 panelsize, showInfoDelegate showInfo) {
			GameObject obj = new GameObject();
			obj.transform.parent = parent.transform;
			obj.transform.localPosition = new Vector3(position.x, position.y, (size.z + panelsize.z) / 2);
			obj.transform.localRotation = rotation;

			GameObject obj_scaled = GameObject.CreatePrimitive(PrimitiveType.Cube);
			obj_scaled.transform.parent = obj.transform;
			obj_scaled.transform.localPosition = Vector3.zero;
			obj_scaled.transform.localScale = size;
			Material material = new Material(Shader.Find("Transparent/Diffuse"));
			Color parsedcolor = Color.white;
			ColorUtility.TryParseHtmlString("#" + color, out parsedcolor);
			material.color = parsedcolor;
			obj_scaled.GetComponent<Renderer>().material = material;

			//Not working yet
			EventTrigger trigger = obj_scaled.AddComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((data) => { Debug.LogError("Not working yet"); showInfo("Name: " + id); });
			trigger.triggers.Add(entry);
		}
	}
}

public class InterfaceReader : MonoBehaviour {

	public GameObject visualizerNode;
	public Text infobox;

	void Start () {
		string json = @"
{
    ""panels"": [
        {
            ""position"": {
                ""x"": 0.0,
                ""y"": 0.0,
                ""z"": 0.0
            },
            ""rotation"": {
                ""x"": 0.0,
                ""y"": 0.0,
                ""z"": 0.0,
                ""w"": 1.0
            },
			""size"": {
                ""x"": 0.08,
                ""y"": 0.053,
                ""z"": 0.008
		    },
			""elements"": [
                {
                    ""id"": ""smallbutton"",
                    ""type"": ""button"",
                    ""position"": {
                        ""x"": 0.034,
                        ""y"": -0.001
                    },
                    ""rotation"": {
                        ""x"": 0.0,
                        ""y"": 0.0,
                        ""z"": 0.0,
                        ""w"": 1.0
                    },
                    ""color"": ""ffffff"",
                    ""size"": {
                        ""x"": 0.012,
                        ""y"": 0.012,
                        ""z"": 0.012
                    }
                },
                {
                    ""id"": ""smallbutton2"",
                    ""type"": ""button"",
                    ""position"": {
                        ""x"": 0.019,
                        ""y"": -0.001
                    },
                    ""rotation"": {
                        ""x"": 0.0,
                        ""y"": 0.0,
                        ""z"": 0.0,
                        ""w"": 1.0
                    },
                    ""color"": ""00ff00"",
                    ""size"": {
                        ""x"": 0.012,
                        ""y"": 0.012,
                        ""z"": 0.012
                    }
                }
            ]
        }
    ]
}";


		HardwareInterface.Interface intf = JsonUtility.FromJson<HardwareInterface.Interface>(json);
		intf.Visualize(visualizerNode, (string s) => { infobox.text = s; });
	}
	
	void Update () {
		
	}
}
