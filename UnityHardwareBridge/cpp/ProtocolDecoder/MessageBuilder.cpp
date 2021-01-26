#include "MessageBuilder.h"
#include "ProtocolParser.h"

char* MessageBuilder::ActivateButton(unsigned char port) {
	static char buf[] = "{CA_B}";
	buf[3] = port;
	return buf;
}

char* MessageBuilder::DeactivateButton(unsigned char port) {
	static char buf[] = "{CA_I}";
	buf[3] = port;
	return buf;
}

char* MessageBuilder::ActivateRotaryEncoder(unsigned char port1, unsigned char port2) {
	static char buf[] = "{CR__R}";
	buf[3] = port1;
	buf[4] = port2;
	return buf;
}

char* MessageBuilder::DeactivateRotaryEncoder(unsigned char port1, unsigned char port2) {
	static char buf[] = "{CR__I}";
	buf[3] = port1;
	buf[4] = port2;
	return buf;
}

char* MessageBuilder::ActivateSlider(unsigned char port) {
	static char buf[] = "{CS_S}";
	buf[3] = port;
	return buf;
}

char* MessageBuilder::GetDeviceInfo() {
	char buf[] = "{CI}";
	return buf;
}
