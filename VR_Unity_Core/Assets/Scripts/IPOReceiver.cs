using UnityEngine;

/*Abstract reciever for PO (physical objects) information
 * The receiver just gets the information, the sender has to know if there is actually something in the virtual world
 * Type is a string describing the object, and must be understood by the different implementations of this class. Empty string means 'not important now'
 * This class must be inherited by eg the robot, visualization, ...
 * 
 * Glossary: 
 * - PO: Physical object: the object that will be represented in the real world
 */

public abstract class IPOReceiver : MonoBehaviour {

	public abstract void MoveToPO(Vector3 pos, Quaternion orientation, string type);
}
