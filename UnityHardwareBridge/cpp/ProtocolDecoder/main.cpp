#include <iostream>
#include "SerialPort.h"
#include <stdio.h>
#include <string.h>
#include "ProtocolParser.h"
#include <thread>         
#include <chrono>   

#include "MessageBuilder.h"

using namespace std;

char* portName = "\\\\.\\COM3";
#define MAX_DATA_LENGTH 255
char incomingData[MAX_DATA_LENGTH];
SerialPort *device;
std::shared_ptr<ProtocolParser> parser = std::shared_ptr<ProtocolParser>(new ProtocolParser());


int main()
{

    device = new SerialPort(portName, 9600 /* baudrate */);
	MessageBuilder builder;
    //Checking if device is connected or not
    if (device->isConnected()){
        std::cout << "Connection established at port " << portName << endl;
    }
	int port = 5;

	char* msgArray = builder.ActivateRotaryEncoder(port, port + 1);

	cout << "msg: ";
	cout << msgArray << endl;
	device->writeSerialPort(msgArray, builder.ActivateRotaryEncoderLength());

	
	while (true) {
		int readResult = device->readSerialPort(incomingData, MAX_DATA_LENGTH);
		for (int i = 0; i < readResult; ++i) {
			std::shared_ptr<Message> msg = parser->Feed(incomingData[i]);
			if (msg != nullptr)
				std::cout << msg->ToString() << std::endl;
		}
	}
}
