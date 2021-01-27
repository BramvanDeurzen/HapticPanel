using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DemoLoader : MonoBehaviour {

	public Dropdown demochooser;
	public Demo[] demos = new Demo[0];

	public void load() {
		string name = demochooser.options[demochooser.value].text;

		Debug.Log("Finding " + name);

		foreach (Demo demo in demos) {
			if(demo.name == name) {
				Debug.Log("Loading " + demo.scenename);
				SceneManager.LoadScene(demo.scenename, LoadSceneMode.Single);
				return;
			}
		}
	}
}
