using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerCollider : MonoBehaviour
{
    public InterfaceMover mover;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        // Enable mover
        mover.SetMovementIfFingerIsNearPlatform(true);

    }

    private void OnTriggerExit(Collider other)
    {
        // Disable mover
        mover.SetMovementIfFingerIsNearPlatform(false);
    }


}
