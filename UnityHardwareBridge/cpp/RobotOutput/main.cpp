#include <iostream>
#include "SerialPort.h"
#include <stdio.h>
#include <string.h>

using namespace std;

char* portName = "\\\\.\\COM4";
#define MAX_DATA_LENGTH 255
char incomingData[MAX_DATA_LENGTH];
SerialPort *device;

int main()
{
    device = new SerialPort(portName);

    //Checking if device is connected or not
    if (device->isConnected()){
        std::cout << "Connection established at port " << portName << endl;
    }
	

	//char buf[] = "{CA_B}{CA_B}";
	//buf[3] = 2;
	//buf[9] = 3;
	//device->writeSerialPort(buf, 12);

	//while (true) {
	//	int readResult = device->readSerialPort(incomingData, MAX_DATA_LENGTH);
	//	for (int i = 0; i < readResult; ++i) {
	//		std::shared_ptr<Message> msg = parser->Feed(incomingData[i]);
	//		if (msg != nullptr)
	//			std::cout << msg->ToString() << std::endl;
	//	}
	//}
}
