using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerCalibration : MonoBehaviour
{
    public GameObject fingerRepresentation;
    public Transform calibrationBoxTransform;

    private bool calibrationStarted = false;
    private bool marker1Set = false;
    private bool marker2Set = false;


    // New calibration procedure
    private Vector3 positionMarker1;
    private Vector3 position1FromFinger;
    private Vector3 positionMarker2;
    private Vector3 position2FromFinger;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartCalibration()
    {
        if (!calibrationStarted)
        {
            calibrationStarted = true;
            Debug.Log("Start calibration by placing your finger on marker 1");
        }
        else
        {
            calibrationStarted = true;
            Debug.Log("Redo calibration: place you finger on marker 1");
        }

    }

    public void GetPositionMarker1()
    {
        if (calibrationStarted)
        {
            position1FromFinger = this.transform.position;
            positionMarker1 = calibrationBoxTransform.position;
            Debug.Log("Marker 1 saved with position: ");
            marker1Set = true;
        }

    }

    public void GetPositionMarker2()
    {
        if (calibrationStarted)
        {
            position2FromFinger = this.transform.position;
            positionMarker2 = calibrationBoxTransform.position;
            Debug.Log("Marker 2 saved with position: ");
            marker2Set = true;
        }
    }

    public void ExecuteCalibration()
    {
        if (!calibrationStarted)
        {
            Debug.LogWarning("Start calibration first!");
            return;
        }
        else if (!marker1Set)
        {
            Debug.LogWarning("Marker 1 not set");
            return;
        }

        else if (!marker2Set)
        {
            Debug.LogWarning("Marker 2 not set");
            return;
        }
        else
        {
            Vector3 differenceMarker1, differenceMarker2;

            differenceMarker1 = position1FromFinger - positionMarker1;
            differenceMarker2 = position2FromFinger - positionMarker2;


            // Marker 1 is Y position, finger placed flat
            // Marker 2 is X position, finger placed perpendicular
            // z is unused
            fingerRepresentation.transform.localPosition = new Vector3(differenceMarker2.x, -differenceMarker1.y, 0);
            Debug.Log("Calbiration is set!");
        }
    }
}
