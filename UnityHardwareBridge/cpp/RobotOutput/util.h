#pragma once

#include <cassert>
#include <cmath>
#include <cstdio>
#include <vector>
#include <string>
#include <sstream>
#include <iostream>
#include <fstream>
#include <mutex>
#include <iostream>
#include <fstream>
#include <string>
#include <tuple>
#include <algorithm>
#include <set>
using namespace std;

//std::set<char> delims{ '\\', '/' };
//std::vector<std::string> path = splitpath("C:\\MyDirectory\\MyFile.bat", delims);
//cout << path.back() << endl;


class Logging {
	static Logging* log;
	
	bool printdebug;
	//boost::mutex mtx_file;
	std::stringstream errorMessages; //to unity
	std::stringstream debugMessages; //to unity

	std::streambuf* stdout_old;
	std::streambuf* stderr_old;

	std::mutex mtx;

	Logging(): printdebug(true) {
		//redirect stdout to debug
		stdout_old = std::cout.rdbuf();
		std::cout.rdbuf(debugMessages.rdbuf());

		//stderr_old = std::cerr.rdbuf();
		//std::cerr.rdbuf(debugMessages.rdbuf());
	}

	~Logging() {
		std::cout.flush();
		std::cout.rdbuf(stdout_old);

		//std::cerr.flush();
		//std::cerr.rdbuf(stderr_old);
	}

public:
	static Logging* ins() {
		if(log == 0)
			log = new Logging();
		return log;
	}

	void setPrintDebug(bool pd) {
		if(printdebug && !pd)
			WriteDebug("Debug now turned off");
		printdebug = pd;
	}

	void WriteDebug(int id, const char* message) {
		std::lock_guard<std::mutex> lock(mtx);
		if(printdebug) 
		{
			//boost::unique_lock<boost::mutex> guardf(mtx_file);
			FILE* debugFile;
#ifdef WIN32
			errno_t err;
			if ((err = fopen_s(&debugFile, "debugRenderPlugin.log", "a")) != 0) {
#else
			if ((debugFile = fopen("debugRenderPlugin.log", "a")) == NULL) {
#endif
				fprintf(stderr, "Cannot open config file %s!\n", "debugRenderPlugin.log");
			} else {
				fprintf(debugFile, "[%d] %s\n", id, message);
				fclose(debugFile);
			}
			debugMessages << message << "\n";
		}
	}

	void WriteDebug(const char* message) {
		std::lock_guard<std::mutex> lock(mtx);
		if(printdebug) 
		{
			//boost::unique_lock<boost::mutex> guardf(mtx_file);
			FILE* debugFile;
#ifdef WIN32
			errno_t err;
			if ((err = fopen_s(&debugFile, "debugRenderPlugin.log", "a")) != 0) {
#else
			if ((debugFile = fopen("debugRenderPlugin.log", "a")) == NULL) {
#endif
				fprintf(stderr, "Cannot open config file %s!\n", "debugRenderPlugin.log");
			} else {
				fprintf(debugFile, "%s\n", message);
				fclose(debugFile);
			}
			debugMessages << message << "\n";
		}
	}

	void WriteError(int id, const char* message) {
		std::lock_guard<std::mutex> lock(mtx);
		//boost::unique_lock<boost::mutex> guardf(mtx_file);
		FILE* debugFile;
#ifdef WIN32
		errno_t err;
		if ((err = fopen_s(&debugFile, "debugRenderPlugin.log", "a")) != 0) {
#else
		if ((debugFile = fopen("debugRenderPlugin.log", "a")) == NULL) {
#endif
			fprintf(stderr, "Cannot open log file %s\n", "debugRenderPlugin.log");
		} else {
			fprintf(debugFile, "[%d] *** ERROR *** %s\n", id, message);
			fclose(debugFile);
		}
		errorMessages << message << "\n";
	}

	void WriteError(const char* message) {
		std::lock_guard<std::mutex> lock(mtx);
		//boost::unique_lock<boost::mutex> guardf(mtx_file);
		FILE* debugFile;
#ifdef WIN32
		errno_t err;
		if ((err = fopen_s(&debugFile, "debugRenderPlugin.log", "a")) != 0) {
#else
		if ((debugFile = fopen("debugRenderPlugin.log", "a")) == NULL) {
#endif
			fprintf(stderr, "Cannot open log file %s\n", "debugRenderPlugin.log");
		} else {
			fprintf(debugFile, "*** ERROR *** %s\n", message);
			fclose(debugFile);
		}
		errorMessages << message << "\n";
	}
	char* getDebugMessages() {
		//boost::unique_lock<boost::mutex> guardf(mtx_file);
		if(debugMessages.str().length() == 0) return "";
		char* msg = new char[strlen(debugMessages.str().c_str()) + 1];
		strcpy(msg, debugMessages.str().c_str());
		debugMessages.clear();
		debugMessages.str(std::string());
		return msg;
	}

	char* getErrorMessages() {
		//boost::unique_lock<boost::mutex> guardf(mtx_file);
		if(errorMessages.str().length() == 0) return "";
		char* msg = new char[strlen(errorMessages.str().c_str()) + 1];
		strcpy(msg, errorMessages.str().c_str());
		errorMessages.clear();
		errorMessages.str(std::string());
		return msg;
	}
};
