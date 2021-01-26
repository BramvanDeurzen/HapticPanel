using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickandPlaceController : MonoBehaviour {

    public float rightLimit;
    public float leftLimit;
    public float percentage = 0;
    public GameObject movingArm;
    public GameObject gripperArm;

    private float percentageSetup;
    
    private float totalMovementLimit;
	// Use this for initialization
	void Start () {
        // Calculate the total movement possible
        totalMovementLimit = leftLimit + rightLimit;
    }

    // Update is called once per frame
    void Update () {
	}

    public void MoveArmToPosPercentage(float percentage)
    {
        if (percentage < 0 || percentage > 1)
            return;
        // Calculate new position
        float newPosition = leftLimit + totalMovementLimit * percentage;

        // Set new position
        movingArm.transform.localPosition = new Vector3(movingArm.transform.localPosition.x, movingArm.transform.localPosition.y, newPosition);
    }
}
