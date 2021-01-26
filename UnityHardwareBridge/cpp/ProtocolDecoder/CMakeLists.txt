cmake_minimum_required (VERSION 2.6) 
project (ProtocolTester) 

set (PROGRAM_VERSION_MAJOR 1)
set (PROGRAM_VERSION_MINOR 0)

SET(main_SOURCES 
	Util.h
	Util.cpp
	MessageBuilder.h
	MessageBuilder.cpp
	ProtocolParser.cpp
	ProtocolParser.h
	SerialPort.cpp
	SerialPort.h
)

file(GLOB_RECURSE INCS "*.h")

add_executable(ProtocolTester main.cpp ${main_SOURCES} )
add_library(HardwareInterfaceToUnityBridgePlugin SHARED UnityPlugin.cpp ${main_SOURCES} )