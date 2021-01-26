using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TablePositioner : MonoBehaviour
{
    public Transform rightBack;
    public Transform rightFront;
    public Transform leftBack;
    public Transform leftFront;
    public GameObject table;
    public InterfaceMover.MoverAxisOrientation axisOrientation;

    private float xSize, ySize, zSize;
    private float xPosition, yPosition, zPosition;
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        // Front row is 6mm higher because of the mdf plate
        Vector3 tempPosition = rightFront.position;
        tempPosition.y -= 0.006f;
        rightFront.position = tempPosition;

        tempPosition = leftFront.position;
        tempPosition.y -= 0.006f;
        leftFront.position = tempPosition;


        SetTableSize();
        SetTablePosition();

    }


    private void SetTableSize()
    {
        // Set the table to the correct size
        // ZY coordinates
        if (axisOrientation.Equals(InterfaceMover.MoverAxisOrientation.ZY_AXIS))
        {
            xSize = (leftBack.position.x + rightBack.position.x) / 2 - (leftFront.position.x + leftFront.position.x) / 2;
            ySize = (leftBack.position.y + leftFront.position.y + rightBack.position.y + rightFront.position.y) / 4;
            zSize = (leftBack.position.z + leftFront.position.z) / 2 - (rightBack.position.z + rightFront.position.z) / 2;
        }
        else if (axisOrientation.Equals(InterfaceMover.MoverAxisOrientation.XY_AXIS))
        {
            xSize = ((leftBack.position.x - rightBack.position.x) + (leftFront.position.x - leftFront.position.x)) / 2;
            ySize = (leftBack.position.y + leftFront.position.y + rightBack.position.y + rightFront.position.y) / 4;
            zSize = ((leftBack.position.z - leftFront.position.z) + (rightBack.position.z - rightFront.position.z)) / 2;
        }


        table.transform.localScale = new Vector3(xSize, ySize, zSize);
    }

    private void SetTablePosition()
    {
        // Set the table to the correct position (Should stay the same during the program)
        xPosition = (leftBack.position.x + leftFront.position.x + rightBack.position.x + rightFront.position.x) / 4;
        yPosition = (leftBack.position.y + leftFront.position.y + rightBack.position.y + rightFront.position.y) / 4;
        zPosition = (leftBack.position.z + leftFront.position.z + rightBack.position.z + rightFront.position.z) / 4;

        // Since the position is the centre of the table in Unity and we set the top value, set table position to counter this
        table.transform.localPosition = new Vector3(0, -ySize / 2, 0);

        this.transform.position = new Vector3(xPosition, yPosition, zPosition);
    }
}
