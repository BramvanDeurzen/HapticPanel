cmake_minimum_required (VERSION 2.6) 
project (RobotOutput) 

set (PROGRAM_VERSION_MAJOR 1)
set (PROGRAM_VERSION_MINOR 0)

SET(main_SOURCES 
	Util.h
	Util.cpp
	SerialPort.cpp
	SerialPort.h
)

file(GLOB_RECURSE INCS "*.h")

add_executable(RobotOutput main.cpp ${main_SOURCES} )
add_library(UnityToHardwareMoverBridgePlugin SHARED UnityPlugin.cpp ${main_SOURCES} )