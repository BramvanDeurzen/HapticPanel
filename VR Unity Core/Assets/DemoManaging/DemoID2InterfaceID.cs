using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Demo/New Demo Mapping")]
public class DemoID2InterfaceID : ScriptableObject {
	[System.Serializable]
	public class map_t {
		public string demoID;
		public string interfaceID;
	}
	
	public List<map_t> mapping;

	public string getMappedID(string demoID) {
		foreach(map_t map in mapping) {
			if (map.demoID == demoID)
				return map.interfaceID;
		}
		UILogging.Error("ID Not found: " + demoID);
		return demoID;
	}
}
