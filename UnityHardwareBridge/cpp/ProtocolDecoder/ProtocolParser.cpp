#include "ProtocolParser.h"
#include <iostream>

std::shared_ptr<Message> ProtocolParser::Feed(unsigned char c) {
	//std::cout << "Feed = " << c << "; State = " << state << std::endl;
	std::shared_ptr<Message> ret = nullptr;
	if (state == 0) { //Start of message
		if (c == '{') {
			message = std::shared_ptr<Message>(new Message);
			state = 1;
		}
	} else if (state == 1) { //Type
		if (c == 'C') {
			message->messagetype = Message::COMMAND;
		} else if (c == 'D') { // DATA, type not determined yet.
			state = 5;
		} else if (c == 'M') {
			message->messagetype = Message::MESSAGE;
			state = 2;
/*		} else if (c == 'I') {
			message->messagetype = Message::DEVICE_INFO;
			state = 2;*/
		} else if (c == 'X') {
			message->messagetype = Message::ERRORMESSAGE;
			state = 2;
		} else {
			message = nullptr;
			state = 0;
			char strerr[80];
			sprintf(strerr, "ProtocolParser: Parse error: unknown message type: %c", c);
			throw std::exception(strerr);
		}
	} else if (state == 2) { //Payload size
		message->size = (int)(c);
		message->sizeread = 0;
		if (message->size == 0) {
			state = 4;
		} else {
			state = 3;
			message->payload = new unsigned char[message->size];
		}
	} else if (state == 3) { //Payload
		message->payload[message->sizeread] = c;
		message->sizeread++;
		if (message->sizeread >= message->size)
			state = 4;
	} else if (state == 4) { //End of message
		if (c == '}') {
			ret = message;
			message = nullptr;
			state = 0;
		} else {
			message = nullptr;
			state = 0;
			throw std::exception("ProtocolParser: Parse error: trailing data");
		}
	} else if (state == 5) { // Checking data type
		if (c == 'B') {
			message->messagetype = Message::DATA_BUTTON;
			message->size = 2;
			message->payload = new unsigned char[2];
			state = 3;
		} else if (c == 'R') {
			message->messagetype = Message::DATA_ROTATION;
			message->size = 2;
			message->payload = new unsigned char[2];
			state = 3;
		} else if (c == 'S') {
			message->messagetype = Message::DATA_SLIDER;
			message->size = 2;
			message->payload = new unsigned char[2];
			state = 3;
		} else {
			message = nullptr;
			state = 0;
			char strerr[80];
			sprintf(strerr, "ProtocolParser: Parse error: unknown data type: %c", c);
			throw std::exception(strerr);
		}
	}

	return ret;
}
