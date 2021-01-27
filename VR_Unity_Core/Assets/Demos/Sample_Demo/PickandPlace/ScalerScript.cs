using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalerScript : MonoBehaviour {

    public GameObject picker;
    public float minScale;
    public float maxScale;
    public float scaleStep;
    public ScaleDirection scaleDirection;
    public enum ScaleDirection {  UP, DOWN };

    private float currentScale;
    private float initialPositionY;

	// Use this for initialization
	void Start () {
        currentScale = picker.transform.localScale.y;
        initialPositionY = picker.transform.position.y;

	}
	
	// Update is called once per frame
	void Update () {
        //Move Down
        if (scaleDirection.Equals(ScaleDirection.DOWN) && currentScale < maxScale)
        {
            currentScale += scaleStep;
            picker.transform.localScale = new Vector3(picker.transform.localScale.x, currentScale, picker.transform.localScale.z);

            float newPositionOffset = (currentScale - minScale) / 2;
            picker.transform.position = new Vector3(picker.transform.position.x, initialPositionY - newPositionOffset, picker.transform.position.z);

        }
        // Move up
        else if (scaleDirection.Equals(ScaleDirection.UP) && currentScale > minScale)
        {
            currentScale -= scaleStep;
            picker.transform.localScale = new Vector3(picker.transform.localScale.x, currentScale, picker.transform.localScale.z);


            //float newPositionOffset = (currentScale - minScale) / 2;
            //picker.transform.position = new Vector3(picker.transform.position.x, initialPositionY + newPositionOffset, picker.transform.position.z);

            float newPositionOffset = (currentScale - minScale) / 2;
            picker.transform.position = new Vector3(picker.transform.position.x, picker.transform.position.y + newPositionOffset, picker.transform.position.z);
        }
	}
}
