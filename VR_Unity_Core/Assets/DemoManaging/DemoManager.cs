using HardwareInterface;
using UnityEngine;
using UnityEngine.UI;

//The global demo manager
public class DemoManager : MonoBehaviour {

	[Tooltip("The demo description asset")]
	public Demo currentDemo;
	//[Tooltip("The parent of the visualization, gameobjects will be added here")]
	//public GameObject visualizerNode;
	//[Tooltip("Textbox to put info in on hover of the visualizer. Set to none to not use this functionality")]
	//public Text infotext;
	[Tooltip("The input device object")]
	public DeviceConnector device;
	[Tooltip("The interface mover, which moves the physical platform")]
	public InterfaceMover mover;
	[Tooltip("The tracker selector, which provides the tracking info")]
	public TrackerSelector tracker;
	[Tooltip("The parent object, of which all VR components are direct descendants")]
	public VRComponentCollection componentsParent;

	//Use start, so all ids are remapped (in awake)
	void Start () {
		if (currentDemo == null /*|| visualizerNode == null*/ || device == null || tracker == null || componentsParent == null)
			UILogging.Warning("The demo contains unreferenced objects and might not work as expected");

		Interface @interface = JsonUtility.FromJson<Interface>(currentDemo.interfaceAsset.text);
		UILogging.Info("Initializing '{0}', interface '{1}'", currentDemo.demoname, @interface.name);

		//Visualizer
		//showInfoDelegate showinfo = (t) => { };
		//if(visualizerNode != null) {
		//	showinfo = (t) => { infotext.text = t; };
		//}
		//InterfaceVisualizer visualizer = new InterfaceVisualizer(visualizerNode, @interface, showinfo);
		//visualizer.Visualize();

		//Assign visualizer and interface to the mover
		mover.SetVisualizerAndTracker(/*visualizer,*/ tracker);

		GameObject positioner = new GameObject();
		positioner.AddComponent<InterfacePositioner>().Init(componentsParent, tracker, mover.GetComponent<InterfaceMover>(), @interface);
		HardwareEventDispatcher dispatcher = new HardwareEventDispatcher(componentsParent, positioner.GetComponent< InterfacePositioner>());

		//Connect to input device
		InterfaceDeviceConnector connector = new InterfaceDeviceConnector(@interface, device, 
			//Button
			(port, status) =>
            {
                //visualizer.setButtonStatus(port, status);
                bool found;
                string id = @interface.fromPortToHardwareID(port, out found);
                if (found)
                    dispatcher.setButtonStatus(id, status);
                else
                    UILogging.Error("DemoManager: Unregistered input hardware port {0} for button", port);
            },
            // Rotary encoder
            (port, rotationValue) =>
            {
                bool found;
                string id = @interface.fromPortToHardwareID(port, out found);
                if (found)
                    dispatcher.setRotationValue(id, rotationValue);
                else
                    UILogging.Error("DemoManager: Unregistered input hardware port {0} for rotary encoder", port);
            },
            // Slider
            (port, sliderValue) =>
            {
                bool found;
                string id = @interface.fromPortToHardwareID(port, out found);
                if (found)
                    dispatcher.setSliderValue(id, sliderValue);
                else
                    UILogging.Error("DemoManager: Unregistered input hardware port {0} for slider", port);
            }
            );
		connector.ConnectComponents();

		//Done
		UILogging.Info("demo set up!");
	}
}
