#pragma once
#include <memory>
#include <string>
#include <sstream>
#include <vector>
#include <map>
#include <iomanip>

class Message {
public:
	Message() : size(0), sizeread(0), messagetype(UNSET), payload(nullptr) {}
	typedef enum {UNSET = 0, MESSAGE, COMMAND, DATA_BUTTON, DATA_ROTATION, DATA_SLIDER, DEVICE_INFO, TYPEDDATA, ERRORMESSAGE} messagetype_t;
	std::vector<std::string> messagetypestring = {"Unset", "Message from hardware", "Command", "Data:Button", "Data:Rotation", "Data:Slider", "Extended data", "Error"};

	messagetype_t messagetype;
	int size;
	int sizeread;
	unsigned char* payload;

	~Message() { if (payload != nullptr) delete[] payload; }
	std::string ToString() {
		std::stringstream ss;
		ss << "Message: Type=";
		ss << messagetypestring[int(messagetype)];
		if (messagetype == DATA_BUTTON) {
			ss << "; Port=" << int(payload[0]) << "; Value=" << int(payload[1]);
		} else if (messagetype == DATA_ROTATION) {
			ss << "; Port=" << int(payload[0]) << "; Rotation Value=" << int(payload[1]);
		} else if (messagetype == DATA_SLIDER) {
			ss << "; Port=" << int(payload[0]) << "; Slider Value=" << int(payload[1]);
		} else {
			if (size > 0) {
				ss << "; Payload size=" << size;
				bool ascii = true;
				for (int i = 0; ascii && i < size; ++i) {
					if (payload[i] < 32)
						ascii = false;
				}
				//if (ascii)
				//	ss << "; Payload=" << std::string(payload, size);
				ss << "; Payload (hex)=";
				for (int i = 0; i < size; ++i)
					ss << "0x" << std::uppercase << std::setfill('0') << std::setw(2) << std::hex << int(payload[i]);
			}
		}

		return ss.str();
	}

	std::string MessageString() {
		if (messagetype == MESSAGE || messagetype == ERRORMESSAGE) {
			//return std::string(payload, size);
			return "";
		} else {
			ToString();
		}
	}

	int getPort() {
		if (messagetype == DATA_BUTTON || messagetype == DATA_ROTATION || messagetype == DATA_SLIDER) {
			if (size != 2)
				throw std::exception("Bad message");
			return int(payload[0]);
		} else 
			throw std::exception("Bad message");
	}

	//For the data command
	int getData() {
		if (messagetype == DATA_BUTTON || messagetype == DATA_ROTATION || messagetype == DATA_SLIDER) {
			if (size != 2)
				throw std::exception("Bad message");
			return int(payload[1]);
		} else
			throw std::exception("Bad message");
	}
};

class ProtocolParser {
public:
	ProtocolParser() : message(nullptr), state(0) {}
	std::shared_ptr<Message> Feed(unsigned char c);
private:
	int state; //0: waiting, 1: message started, 2 type determined next size, 3 type+size determined next data, 4 fields filled
	std::shared_ptr<Message> message;
};
