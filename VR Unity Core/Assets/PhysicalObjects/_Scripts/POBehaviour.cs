using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Behaviour of a physical object. Inherit to do something else
 * For PO without any interaction (blocks on screen, raw material, ...), just visualization, use the name POVisualization_Something
 * For PO with interaction (buttons), use the name POBehaviour_Something
 */

public abstract class POBehaviour : ScriptableObject {
	public abstract void Visualize(GameObject obj);
}
