/*
* Author: Manash Kumar Mandal
* Modified Library introduced in Arduino Playground which does not work
* This works perfectly
* LICENSE: MIT
* https://github.com/manashmndl/SerialPort
*/

//Thread-safe for read and write, not to create and to have multiple instances on the same port


#ifndef SERIALPORT_H
#define SERIALPORT_H

#define ARDUINO_WAIT_TIME 2000
#define MAX_DATA_LENGTH 255

#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <mutex>

class SerialPort
{
private:
    HANDLE handler;
    bool connected;
    COMSTAT status;
    DWORD errors;
	std::mutex device_mutex;
public:
    SerialPort(const char *portName, int baudrate = 9600);
    ~SerialPort();

	void closeSerialPort();
    int readSerialPort(char *buffer, unsigned int buf_size);
    bool writeSerialPort(const char *buffer, unsigned int buf_size);
    bool isConnected();
};

#endif // SERIALPORT_H
