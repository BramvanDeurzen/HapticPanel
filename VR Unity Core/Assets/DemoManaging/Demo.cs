using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "VR Hardware Interface/New Scene")]
public class Demo : ScriptableObject {

	public string demoname;
	public string scenename;
	public TextAsset interfaceAsset;
	public DemoID2InterfaceID IDMapping;
}
