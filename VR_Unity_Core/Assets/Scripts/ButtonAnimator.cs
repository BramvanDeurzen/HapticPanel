using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAnimator : MonoBehaviour
{
    public float buttonDisplacement = 0.001f;
    
    public Material buttonEnabledMaterial;
    public Material buttonDisabledMaterial;
    public float flashTime;

    private Vector3 startPosition;
    private MeshRenderer buttonRenderer;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = this.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AnimateButton(bool inputState)
    {

        // Get buttonRenderer from child gameobject
        buttonRenderer = this.GetComponentInChildren<MeshRenderer>();

        if (inputState)
        {
            buttonRenderer.material = buttonEnabledMaterial;
            Vector3 tempPosition = startPosition;
            tempPosition.x -= buttonDisplacement;
            this.transform.localPosition = tempPosition;
            StartCoroutine(FlashButtonTimed());
        }
        else
        {
            buttonRenderer.material = buttonDisabledMaterial;
            this.transform.localPosition = startPosition;
        }

        
    }

    IEnumerator FlashButtonTimed()
    {
        yield return new WaitForSeconds(flashTime);
        AnimateButton(false);
    }
}
