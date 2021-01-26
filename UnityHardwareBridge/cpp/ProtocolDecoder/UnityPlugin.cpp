/*
The code is multithreaded. The thread handles the recieving of messages and sends callbacks to unity. 
*/

#include "util.h"
#include <iostream>
#include "SerialPort.h"
#include <stdio.h>
#include <string.h>
#include "ProtocolParser.h"
#include "MessageBuilder.h"

#include <assert.h>
#include <math.h>
#include <vector>
#include <mutex>
#include <future>
#include <list>

#include "Unity/IUnityGraphics.h"

const static size_t messagesize = 2048;
char message[messagesize];
std::mutex device_mutex;
SerialPort *device = nullptr;

bool runListening = false;
std::thread listener;

//https://en.cppreference.com/w/cpp/thread/condition_variable
std::mutex canIssueCallbackMutex;
std::condition_variable canIssueCallbackCV;
bool canIssueCallback;
bool CallbacksProcessed = false;


//buttons
typedef void(UNITY_INTERFACE_API *buttonpush_callback_t)(int /*port*/, bool /*value*/);
static std::map<int /*port*/, buttonpush_callback_t> buttonpush_callbacks;
void buttonpush_callback_donothing(int, bool) {}
std::mutex buttonpush_callbacks_mutex;

//Rotary Encoder
typedef void(UNITY_INTERFACE_API *rotaryEncoder_callback_t)(int /* port 1*/, int /*rotation value*/);
static std::map<int /*port1*/, rotaryEncoder_callback_t> rotaryEncoder_callbacks;
void rotaryEncoder_callback_donothing(int, int) {}
std::mutex rotaryEncoder_callbacks_mutex;

//Slider
typedef void(UNITY_INTERFACE_API *slider_callback_t)(int /* port 1*/, int /*slider value*/);
static std::map<int /*port1*/, slider_callback_t> slider_callbacks;
void slider_callback_donothing(int, int) {}
std::mutex slider_callbacks_mutex;


//!Return all saved debug messages. After return, all mesages are deleted
extern "C" UNITY_INTERFACE_EXPORT char* UNITY_INTERFACE_API GetDebugMessages() {
	return Logging::ins()->getDebugMessages();
}

//!Return all saved error messages. After return, all mesages are deleted
extern "C" UNITY_INTERFACE_EXPORT char* UNITY_INTERFACE_API GetErrorMessages() {
	return Logging::ins()->getErrorMessages();
}

//!Deactivate a button on a port. Also safe to call when the button was not activated
extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API DectivateButton(int port) {
	try {
		Logging::ins()->WriteDebug("Deactivating button");
		if (port > 255 || port < 0)
			throw std::exception("Invalid input port number, must be positive and less than 255");

		if (device == nullptr || !device->isConnected())
			throw std::exception("There is no device available. Did you start listening?");

		bool go;
		{
			std::lock_guard<std::mutex> lock_button(buttonpush_callbacks_mutex);
			go = (buttonpush_callbacks.find(port) != buttonpush_callbacks.end());
		}

		if (go) {
			char * msg = MessageBuilder::DeactivateButton(port);
			device->writeSerialPort(msg, MessageBuilder::DeactivateButtonLength());
			{
				std::lock_guard<std::mutex> lock_button(buttonpush_callbacks_mutex);
				buttonpush_callbacks.erase(port);
			}
		}

		return true;
	} catch (std::exception& ex) {
		Logging::ins()->WriteError(ex.what());
		device->closeSerialPort();
		return false;
	} catch (...) {
		Logging::ins()->WriteError("Undefined error");
		return false;
	}
}

//!Deactivate a rotary encoder on a port combination. Also safe to call when the button was not activated
// precond: port1 + 1 == port2
extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API DeactivateRotaryEncoder(int port1, int port2) {
	try {
		Logging::ins()->WriteDebug("Deactivating rotary encoder");
		if (port1 > 255 || port1 < 0)
			throw std::exception("Invalid input port 1 number, must be positive and less than 255");
		if (port2 > 255 || port2 < 0)
			throw std::exception("Invalid input port 1 number, must be positive and less than 255");
		if (port1 + 1 != port2)
			throw std::exception("Pre condition not satisfied: port1 + 1 == port2 must be true.");

		if (device == nullptr || !device->isConnected())
			throw std::exception("There is no device available. Did you start listening?");

		bool go;
		{
			//std::lock_guard<std::mutex> lock_button(buttonpush_callbacks_mutex);
			//go = (buttonpush_callbacks.find(port) != buttonpush_callbacks.end());
			std::lock_guard<std::mutex> lock(rotaryEncoder_callbacks_mutex);
			go = (rotaryEncoder_callbacks.find(port1) != rotaryEncoder_callbacks.end());
		}

		if (go) {
			char* msg = MessageBuilder::DeactivateRotaryEncoder(port1, port2);
			device->writeSerialPort(msg, MessageBuilder::DeactivateRotaryEncoderLength());
			{
				//std::lock_guard<std::mutex> lock_button(buttonpush_callbacks_mutex);
				//buttonpush_callbacks.erase(port);
				std::lock_guard<std::mutex> lock(rotaryEncoder_callbacks_mutex);
				rotaryEncoder_callbacks.erase(port1);
			}
		}

		return true;
	}
	catch (std::exception& ex) {
		Logging::ins()->WriteError(ex.what());
		device->closeSerialPort();
		return false;
	}
	catch (...) {
		Logging::ins()->WriteError("Undefined error");
		return false;
	}
}

//!Deactivate all inputs on this port
bool DeactivateButton(unsigned int port) {
	return DectivateButton(port) && true;
}

//!Deactive the rotary encoder on the ports
bool DeactivateRotaryEncoder(unsigned int port1, unsigned int port2) {
	return DeactivateRotaryEncoder(port1, port2) && true;
}

//!Deactivate all input ports
bool DeactivateAll() {
	std::lock_guard<std::mutex> lock_button(buttonpush_callbacks_mutex);
	buttonpush_callbacks.clear();

	std::lock_guard<std::mutex> lock_rotaryEncoder(rotaryEncoder_callbacks_mutex);
	rotaryEncoder_callbacks.clear();

	/*//This needs testing. deactivatebutton erases also its callback, not sure what happens with the iterator
	for (auto it = buttonpush_callbacks.begin(); it != buttonpush_callbacks.end(); ++it) {
		DectivateButton(it->first);
	}
	*/

	return true;
}

/*!Activate a button on a port on the device
\param port The port number on the actual hardware
\param onStatusChange Callback function when a message arrives on this port
\todo Handle error messages from the device
*/
extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ActivateButton(int port, buttonpush_callback_t onStatusChange) {
	try {
		if (port > 255 || port < 0)
			throw std::exception("Invalid input port number, must be positive and less than 255");

		if (device == nullptr || !device->isConnected())
			throw std::exception("There is no device available. Did you start listening?");

		//Deactivate all types
		DeactivateButton(port);
		//Activate button
		{
			std::lock_guard<std::mutex> lock(buttonpush_callbacks_mutex);
			buttonpush_callbacks[port] = onStatusChange;
		}
		char* msg = MessageBuilder::ActivateButton(port);
		device->writeSerialPort(msg, MessageBuilder::ActivateButtonLength());

		return true;
	} catch (std::exception& ex) {
		Logging::ins()->WriteError(ex.what());
		device->closeSerialPort();
		return false;
	} catch (...) {
		Logging::ins()->WriteError("Undefined error");
		return false;
	}
}

/*!Activate a rotary encoder on the device. Uses 2 ports for the rotation.
\param port1 The first port number on the actual hardware. Pre-cond: port1 + 1 == port2!
\param port2 The second port number on the actual hardware. Pre-cond: port1 + 1 == port2!
\param onStatusChange Callback function when a message arrives on this port
\todo Handle error messages from the device
*/
extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ActivateRotaryEncoder(int port1, int port2, rotaryEncoder_callback_t onRotationChange) {
	try {
		if (port1 > 255 || port1 < 0)
			throw std::exception("Invalid input port1 number, must be positive and less than 255");
		if (port1 > 255 || port1 < 0)
			throw std::exception("Invalid input port2 number, must be positive and less than 255");
		if (port1 + 1 != port2)
			throw std::exception("Pre condition not satisfied: port1 + 1 == port2 must be true.");

		if (device == nullptr || !device->isConnected())
			throw std::exception("There is no device available. Did you start listening?");

		//Deactivate all types
		DeactivateRotaryEncoder(port1, port2); // TODO: change to deactive rotary encoder.


		//Activate rotary encoder
		{
			//std::lock_guard<std::mutex> lock(buttonpush_callbacks_mutex); // TODO: figure out what to do with this
			//buttonpush_callbacks[port] = onStatusChange;
			std::lock_guard<std::mutex> lock(rotaryEncoder_callbacks_mutex);
			rotaryEncoder_callbacks[port1] = onRotationChange;

		}
		char* msg = MessageBuilder::ActivateRotaryEncoder(port1, port2);
		device->writeSerialPort(msg, MessageBuilder::ActivateRotaryEncoderLength());

		return true;
	}
	catch (std::exception& ex) {
		Logging::ins()->WriteError(ex.what());
		device->closeSerialPort();
		return false;
	}
	catch (...) {
		Logging::ins()->WriteError("Undefined error");
		return false;
	}
}

/*!Activate a slider on a analog port on the device
\param port The port number on the actual hardware
\param onStatusChange Callback function when a message arrives on this port
\todo Handle error messages from the device
*/
extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ActivateSlider(int port, slider_callback_t onStatusChange) {
	try {
		if (port > 5 || port < 0)
			throw std::exception("Invalid input analog port number, must be positive and less than 5");

		if (device == nullptr || !device->isConnected())
			throw std::exception("There is no device available. Did you start listening?");

		//Deactivate all types
		//DeactivateSlider(port);
		//Activate slider
		{
			std::lock_guard<std::mutex> lock(slider_callbacks_mutex);
			slider_callbacks[port] = onStatusChange;
		}
		char* msg = MessageBuilder::ActivateSlider(port);
		device->writeSerialPort(msg, MessageBuilder::ActivateSliderLength());

		return true;
	}
	catch (std::exception& ex) {
		Logging::ins()->WriteError(ex.what());
		device->closeSerialPort();
		return false;
	}
	catch (...) {
		Logging::ins()->WriteError("Undefined error");
		return false;
	}
}


/*!The thread that listens for messages of the device, and that calls the callbacks in unity
\todo Handle read errors more gracefully
*/
void ListeningThread() {
	const int datalength = 255;
	char incomingData[datalength];
	std::shared_ptr<ProtocolParser> parser = std::shared_ptr<ProtocolParser>(new ProtocolParser());
	Logging::ins()->WriteDebug("HardwareToUnityPlugin: Listener about to start");

	{
		//Arduino object keeps locked here, and will unlock when runListening is set to false
		std::lock_guard<std::mutex> lock_arduino(device_mutex);
		if (device == nullptr || !device->isConnected())
			throw std::exception("HardwareToUnityPlugin: There is no device available. Did you initialize?");

		while (runListening && device != nullptr) {
			int totalDataRead = 0;
			try {
				totalDataRead = device->readSerialPort(incomingData, MAX_DATA_LENGTH);
			} catch (std::exception& ex) {
				Logging::ins()->WriteError("HardwareToUnityPlugin: Read failed, shutting down the listener. Please re-initialize the device and application");
				Logging::ins()->WriteError(ex.what());
				device->closeSerialPort();
				return;
			}

			//std::unique_lock<std::mutex> lk(canIssueCallbackMutex
			//canIssueCallbackCV.wait(lk, [] {return canIssueCallback; });

			for (int i = 0; i < totalDataRead; ++i) {
				std::shared_ptr<Message> msg = nullptr;
				try {
					msg = parser->Feed(incomingData[i]);
				} catch (std::exception& ex) {
					Logging::ins()->WriteError("HardwareToUnityPlugin: Parsing data failed, shutting down the listener. Please re-initialize the device and application");
					Logging::ins()->WriteError(ex.what());
					device->closeSerialPort();
					return;
				}
				if (msg != nullptr) {
					//handle
					if (msg->messagetype == Message::DATA_BUTTON) {
						try {
							int port = msg->getPort();
							buttonpush_callback_t f_button = (buttonpush_callback_t) buttonpush_callback_donothing;
							{
								std::lock_guard<std::mutex> lock(buttonpush_callbacks_mutex);
								if (buttonpush_callbacks.find(port) != buttonpush_callbacks.end()) {
									f_button = buttonpush_callbacks[port];
								}
							}
							f_button(port, msg->getData() == 1); // button data is 0 or 1 .
						} catch (std::exception& ex) {
							Logging::ins()->WriteError(ex.what());
							Logging::ins()->WriteDebug(msg->ToString().c_str());
						} catch (...) {
							Logging::ins()->WriteError("HardwareToUnityPlugin: Undefined error");
							Logging::ins()->WriteDebug(msg->ToString().c_str());
						}
					} else if (msg->messagetype == Message::DATA_ROTATION) {
						try {
							int port = msg->getPort();
							rotaryEncoder_callback_t f_rotary = (rotaryEncoder_callback_t) rotaryEncoder_callback_donothing;
							{
								std::lock_guard<std::mutex> lock(rotaryEncoder_callbacks_mutex);
								if (rotaryEncoder_callbacks.find(port) != rotaryEncoder_callbacks.end()) {
									f_rotary = rotaryEncoder_callbacks[port];
								}
							}
							f_rotary(port, msg->getData()); // RotaryEncoder data is -1 or +1 that indicates CW or CCW rotation
						}
						catch (std::exception& ex) {
							Logging::ins()->WriteError(ex.what());
							Logging::ins()->WriteDebug(msg->ToString().c_str());
						}
						catch (...) {
							Logging::ins()->WriteError("HardwareToUnityPlugin: Undefined error");
							Logging::ins()->WriteDebug(msg->ToString().c_str());
						}
					} else if (msg->messagetype == Message::DATA_SLIDER) {
						try {
							int port = msg->getPort();
							slider_callback_t f_slider = (slider_callback_t)slider_callback_donothing;
							{
								std::lock_guard<std::mutex> lock(slider_callbacks_mutex);
								if (slider_callbacks.find(port) != slider_callbacks.end()) {
									f_slider = slider_callbacks[port];
								}
							}
							f_slider(port, msg->getData()); // Slider data is 0 - 255 slider value.
						}
						catch (std::exception& ex) {
							Logging::ins()->WriteError(ex.what());
							Logging::ins()->WriteDebug(msg->ToString().c_str());
						}
						catch (...) {
							Logging::ins()->WriteError("HardwareToUnityPlugin: Undefined error");
							Logging::ins()->WriteDebug(msg->ToString().c_str());
						}
					} else if (msg->messagetype == Message::MESSAGE) {
						Logging::ins()->WriteDebug(msg->MessageString().c_str());
					} else if (msg->messagetype == Message::DEVICE_INFO) {
						//TODO: parse version and device type
						Logging::ins()->WriteDebug(msg->MessageString().c_str());
					} else if (msg->messagetype == Message::ERRORMESSAGE) {
						Logging::ins()->WriteError(msg->MessageString().c_str());
					} else {
						Logging::ins()->WriteError("HardwareToUnityPlugin: Unsupported message type");
						Logging::ins()->WriteDebug(msg->ToString().c_str());
					}
				}
			}

			CallbacksProcessed = true;
			canIssueCallback = false;
			//lk.unlock();
			//canIssueCallbackCV.notify_all();
		}
	}

	Logging::ins()->WriteDebug("HardwareToUnityPlugin: Listener stopped");
}

/*!Stop the device, and the listening thread
*/
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API StopDevice() {
	if (runListening) {
		Logging::ins()->WriteDebug("HardwareToUnityPlugin: Stopping the device");

		//Stop listening thread
		runListening = false;
		listener.join();

		//The device is held by the thread, so we can lock after the thread finished
		std::lock_guard<std::mutex> lock_arduino(device_mutex);

		//Stop all listening on the hardware
		DeactivateAll();

		//Stop the hardware link
		if (device != nullptr)
			delete device;
		device = nullptr;
	}
}

/*!Start the device on a COM port, and the listening thread
\param comPortNumber The number of the com port
*/
extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API StartDevice(int comPortNumber) {
	StopDevice();
	std::lock_guard<std::mutex> lock_arduino(device_mutex);
	try {
		Logging::ins()->WriteDebug("HardwareToUnityPlugin: Starting the device");
		if (comPortNumber < 0)
			throw std::exception("HardwareToUnityPlugin: Invalid comport number");
		std::string portName = "\\\\.\\COM" + std::to_string(comPortNumber);
		device = new SerialPort(portName.c_str());
		runListening = true;
		listener = std::thread(ListeningThread);
		char* msg = MessageBuilder::GetDeviceInfo();
		//device->writeSerialPort(msg.c_str(), unsigned int(msg.length()));
		return true;
	} catch (std::exception& ex) {
		Logging::ins()->WriteError("HardwareToUnityPlugin: Cannot start device: ");
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
