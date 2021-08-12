# HapticPanel: An Open System to Render Haptic Interfaces in Virtual Reality for Manufacturing Industry

## Quick overview
* The files can be found in the 'Platform files' directory.
* The assembly steps can be found in the wiki of this repository.
* The VR framework software can be found in the 'VR Unity core' directory.
* The Arduino software and serial parser code can be found in the 'UnityHardwareBridge' directory.

## HapticPanel Haptic Platform:
![Simulated Reality Haptic Platform](https://github.com/BramvanDeurzen/SimulatedReality/blob/master/Images/final_platform_result_compressed.jpg?raw=true)
Picture showing the Simulated Reality Haptic Platform for which the build file and instructions are shared in this repository.


## Installation instruction:
1. Include SteamVR from the Unity asset store.
2. Include the OptiTrack Unity plugin, found here: https://optitrack.com/support/downloads/plugins.html
3. Deploy the Arduino software on the Arduino's controlling the stepper motors and the Arduino handling the physical input.
4. Open the Sample demo scene in the Demos subdirectory.
5. Align the SteamVR and OptiTrack tracking.
6. Assign the correct OptiTrack id's to the finger tracker and the input panel tracker.
7. Assign the correct COM ports.
8. Update the 'sample interface.json' to match:
	1. The Arduino port used for each physical input element
	2. The physical position of the input element on the input panel. The centre of the input panel is 0,0. 



