/*
* Author: Manash Kumar Mandal
* Modified Library introduced in Arduino Playground which does not work
* This works perfectly
* LICENSE: MIT
*/

#include "SerialPort.h"
#include <string>
#include <list>
#include <algorithm>

void WriteData(const char* message, size_t size) {
	FILE* debugFile;
#ifdef WIN32
	errno_t err;
	if ((err = fopen_s(&debugFile, "COMPort.log", "a")) != 0) {
#else
	if ((debugFile = fopen("COMPort.log", "a")) == NULL) {
#endif
		fprintf(stderr, "Cannot open log file %s\n", "COMPort.log");
	} else {
		for(int i = 0; i < size; ++i)
			fprintf(debugFile, "%c", message[i]);
		fclose(debugFile);
	}
	}

SerialPort::SerialPort(const char *portName, int baudrate) {
	//Todo: arduino defines 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200
	std::list<int> allowedBaudrates = { 110, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200, 128000, 256000 };
	if(std::find(allowedBaudrates.begin(), allowedBaudrates.end(), baudrate) == allowedBaudrates.end())
		throw std::exception("Cannot open COM port: baud rate is not a valid value");

	this->connected = false;

	this->handler = CreateFileA(static_cast<LPCSTR>(portName),
		GENERIC_READ | GENERIC_WRITE,
		0,
		NULL,
		OPEN_EXISTING,
		FILE_ATTRIBUTE_NORMAL,
		NULL);
	if (this->handler == INVALID_HANDLE_VALUE) {
		if (GetLastError() == ERROR_FILE_NOT_FOUND) {
			throw std::exception(("Cannot open COM port: " + std::string(portName) + " not available").c_str());
		} else {
			throw std::exception("Cannot open COM port: uknown error");
		}
	} else {
		DCB dcbSerialParameters = { 0 };

		if (!GetCommState(this->handler, &dcbSerialParameters)) {
			throw std::exception("Failed to get current serial parameters");
		} else {
			dcbSerialParameters.BaudRate = baudrate;
			dcbSerialParameters.ByteSize = 8;
			dcbSerialParameters.StopBits = ONESTOPBIT;
			dcbSerialParameters.Parity = NOPARITY;
			dcbSerialParameters.fDtrControl = DTR_CONTROL_ENABLE;

			if (!SetCommState(handler, &dcbSerialParameters)) {
				throw std::exception("Could not set Serial port parameters");
			} else {
				this->connected = true;
				PurgeComm(this->handler, PURGE_RXCLEAR | PURGE_TXCLEAR);
				Sleep(ARDUINO_WAIT_TIME);
			}
		}
	}
}

SerialPort::~SerialPort() {
	if (this->connected) {
		this->connected = false;
		CloseHandle(this->handler);
	}
}

int SerialPort::readSerialPort(char *buffer, unsigned int buf_size) {
	std::lock_guard<std::mutex> lock_arduino(device_mutex);
	DWORD bytesRead;
	unsigned int toRead = 0;

	ClearCommError(this->handler, &this->errors, &this->status);

	if (this->status.cbInQue > 0) {
		if (this->status.cbInQue > buf_size) {
			toRead = buf_size;
		} else {
			toRead = this->status.cbInQue;
		}
	}

	if (ReadFile(this->handler, buffer, toRead, &bytesRead, NULL)) {
		WriteData(buffer, bytesRead);
		return bytesRead;
	}  else
		throw std::exception(("Cannot read from COM port: error code = " + std::to_string(GetLastError())).c_str());

	return 0;
}

bool SerialPort::writeSerialPort(const char *buffer, unsigned int buf_size) {
	std::lock_guard<std::mutex> lock_arduino(device_mutex);
	DWORD bytesSend;

	ClearCommError(this->handler, &this->errors, &this->status);

	if (!WriteFile(this->handler, (void*)buffer, buf_size, &bytesSend, 0)) {
		throw std::exception(("Cannot read from COM port: error code = " + std::to_string(GetLastError())).c_str());
	} else {
		return true;
	}
}

bool SerialPort::isConnected() {
	return this->connected;
}
