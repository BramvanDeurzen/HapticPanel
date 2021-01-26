#pragma once
#include <string>
#include <iostream>

class MessageBuilder {
public: 
	static char* ActivateButton(unsigned char port);
	static int ActivateButtonLength() { return 6; }
	static char* DeactivateButton(unsigned char port);
	static int DeactivateButtonLength() { return 6; }
	static char* ActivateRotaryEncoder(unsigned char port1, unsigned char port2);
	static int ActivateRotaryEncoderLength() { return 7; }
	static char* DeactivateRotaryEncoder(unsigned char port1, unsigned char port2);
	static int DeactivateRotaryEncoderLength() { return 7; }
	static char* ActivateSlider(unsigned char port);
	static int ActivateSliderLength() { return 6; }
	static char* GetDeviceInfo();
	static int GetDeviceInfoLength() { return 5; }
};
