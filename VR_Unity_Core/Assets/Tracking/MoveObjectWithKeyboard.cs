using UnityEngine;

public class MoveObjectWithKeyboard : MonoBehaviour {

	float speed = 1.0f;

	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKey(KeyCode.LeftArrow)) {
			Vector3 position = this.transform.localPosition;
			position.x -= speed;
			this.transform.localPosition = position;
		}
		if (Input.GetKey(KeyCode.RightArrow)) {
			Vector3 position = this.transform.localPosition;
			position.x += speed;
			this.transform.localPosition = position;
		}
		if (Input.GetKey(KeyCode.UpArrow)) {
			Vector3 position = this.transform.localPosition;
			position.y += speed;
			this.transform.localPosition = position;
		}
		if (Input.GetKey(KeyCode.DownArrow)) {
			Vector3 position = this.transform.localPosition;
			position.y -= speed;
			this.transform.localPosition = position;
		}
		if (Input.GetKey(KeyCode.PageUp)) {
			Vector3 position = this.transform.localPosition;
			position.z -= speed;
			this.transform.localPosition = position;
		}
		if (Input.GetKey(KeyCode.PageDown)) {
			Vector3 position = this.transform.localPosition;
			position.z += speed;
			this.transform.localPosition = position;
		}
	}
}
