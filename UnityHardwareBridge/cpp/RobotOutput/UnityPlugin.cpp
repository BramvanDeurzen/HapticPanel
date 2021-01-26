/*
The code is multithreaded. The thread handles the recieving of messages and sends callbacks to unity. 
*/

#include "util.h"
#include <iostream>
#include "SerialPort.h"
#include <stdio.h>
#include <string.h>

#include <assert.h>
#include <math.h>
#include <vector>
#include <mutex>
#include <future>
#include <list>
#include <map>

#include "Unity/IUnityGraphics.h"

const static size_t messagesize = 2048;
char message[messagesize];
std::mutex device_mutex;
SerialPort *device = nullptr;

bool runListening = false;

//https://en.cppreference.com/w/cpp/thread/condition_variable
std::mutex canIssueCallbackMutex;
std::condition_variable canIssueCallbackCV;
bool canIssueCallback;
bool CallbacksProcessed = false;

const static int X_LIMIT_MM = 700; // TODO: define the correct limits and change them in the error handling
const static int Y_LIMIT_MM = 700;

typedef void(UNITY_INTERFACE_API *movePlatform_callback_t)(int /*x Position*/, int /* y position*/);
static std::map<int /*port*/, movePlatform_callback_t> movePlatform_callbacks;
void movePlatform_callback_donothing(int, bool) {}
std::mutex movePlatform_callbacks_mutex;



//!Return all saved debug messages. After return, all mesages are deleted
extern "C" UNITY_INTERFACE_EXPORT char* UNITY_INTERFACE_API GetDebugMessages() {
	return Logging::ins()->getDebugMessages();
}

//!Return all saved error messages. After return, all mesages are deleted
extern "C" UNITY_INTERFACE_EXPORT char* UNITY_INTERFACE_API GetErrorMessages() {
	return Logging::ins()->getErrorMessages();
}


/*!Move the platform to the desired position
\param xPosition The X position
\param yPosition The Y position
\param onStatusChange Callback function when a message arrives for the mover
\todo Handle error messages from the device
*/
extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API MovePlatform(float xPositionInMM, float yPositionInMM, float mmToStepsFactorX, float mmToStepsFactorY/*, float xOffsetInMM, float yOffsetInMM*/) {
	try {
		// Apply the offsets before checking the limits.
		//xPositionInMM -= xOffsetInMM;
		//yPositionInMM += yOffsetInMM;

		if (xPositionInMM >= X_LIMIT_MM || xPositionInMM <= 0)
			throw std::exception("Invalid input port number, must be positive and less than 700mm"); // X Limit

		if (yPositionInMM >= Y_LIMIT_MM || yPositionInMM <= 0)
			throw std::exception("Invalid input port number, must be positive and less than 700mm"); // Y Limit

		if (device == nullptr || !device->isConnected())
			throw std::exception("There is no device available. Did you start listening?");


		Logging::ins()->WriteDebug("Moving platform");
		{
			std::lock_guard<std::mutex> lock(movePlatform_callbacks_mutex);
		}

		// Transform given position in mm to steps for the stepper motors of the platform
		int xPositionSteps = xPositionInMM * mmToStepsFactorX;
		int yPositionSteps = yPositionInMM * mmToStepsFactorY;

		// Create the message
		// Message format: <TravelX,TravelY>
		std::string msg = "<";
		msg += to_string(xPositionSteps);
		msg.append(",");
		msg += to_string(yPositionSteps);	
		msg.append(">");

		device->writeSerialPort(msg.c_str(), unsigned int(msg.length()));

		return true;
	}
	catch (std::exception& ex) {
		Logging::ins()->WriteError(ex.what());
		return false;
	}
	catch (...) {
		Logging::ins()->WriteError("Undefined error");
		return false;
	}
}

/*!Update the current position (in steps) of the platform
\param stepPositionX The amount of steps in X direction
\param stepPositionY The amount of steps in Y direction
\todo Handle error messages from the device
*/
extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetPlatformStepPosition(int stepPositionX, int stepPositionY) {
	try {
		// Create the message
		// Message format: <TravelX,TravelY>
		std::string msg = "<";
		msg += to_string(stepPositionX);
		msg.append(",");
		msg += to_string(stepPositionY);
		msg.append(",P");
		msg.append(">");

		device->writeSerialPort(msg.c_str(), unsigned int(msg.length()));

		return true;
	}
	catch (std::exception& ex) {
		Logging::ins()->WriteError(ex.what());
		return false;
	}
	catch (...) {
		Logging::ins()->WriteError("Undefined error");
		return false;
	}
}

/*!Stop the device, and the listening thread
*/
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API StopDevice() {
	if (runListening) {
		Logging::ins()->WriteDebug("Stopping the device");

		//Stop listening thread
		runListening = false;

		//The device is held by the thread, so we can lock after the thread finished
		std::lock_guard<std::mutex> lock_arduino(device_mutex);

		//Stop all listening on the hardware
		//DeactivateAll();

		//Stop the hardware link
		if (device != nullptr)
			delete device;
		device = nullptr;

		Logging::ins()->WriteDebug("Hardware device");
	}
}

/*!Start the device on a COM port, and the listening thread
\param comPortNumber The number of the com port
*/
extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API StartDevice(int comPortNumber) {
	StopDevice();
	std::lock_guard<std::mutex> lock_arduino(device_mutex);
	try {
		Logging::ins()->WriteDebug("Starting the device");
		if (comPortNumber < 0)
			throw std::exception("Invalid comport number");
		std::string portName = "\\\\.\\COM" + std::to_string(comPortNumber);
		device = new SerialPort(portName.c_str());
		runListening = true;

		return true;
	} catch (std::exception& ex) {
		Logging::ins()->WriteError("Cannot start device: ");
		Logging::ins()->WriteError(ex.what());
		return false;
	}
}

//!Called to load plugin. Currently not doing anything. 
extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces) {
	Logging::ins()->WriteDebug("LOAD PLUGIN");
}

//!Called to unload plugin. Currently not doing anything. 
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload() {
	Logging::ins()->WriteDebug("UNLOAD PLUGIN");
}

//!Called on every render interation. Can be ignored in this version. Currently not doing anything. 
static void UNITY_INTERFACE_API OnRenderEvent(int eventID) { 
	if (!runListening)
		return;

	/*
	Logging::ins()->WriteDebug("Enabling callbacks");
	{
		std::lock_guard<std::mutex> lk(canIssueCallbackMutex);
		canIssueCallback = true;
	}
	canIssueCallbackCV.notify_all();
	Logging::ins()->WriteDebug("Wait for processing");
	{
		std::unique_lock<std::mutex> lk(canIssueCallbackMutex);
		canIssueCallbackCV.wait(lk, [] {return CallbacksProcessed; });
	}
	*/
}

//!Return the function that has to be called every render iteration
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc() {
	return OnRenderEvent;
}
