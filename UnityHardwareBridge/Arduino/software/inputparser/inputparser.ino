/*
 * TODO
 * - Make reset command
 * - Create error message
 * - Fix memory limitations
 */

// Used for rotary encoder
#include "Rotary.h"

const int numberOfDigitalPins = 14;
const int numberOfAnalogPins = 5;

typedef enum { UNSET = 0, MESSAGE, COMMAND, DATA, EXTENDEDDATA, ERRORMESSAGE } messagetype_t;

int command = 0;
int commanddata = -1;
int state = 0; //0 = start, 1 = wait command, 2 = wait command data, -1 error
int messagesize;
int messagesizeread;
messagetype_t messagetype;
char messagepayload[128]; // Reduced from 1024 to save some program memory
bool messagedone = false;

typedef enum { INACTIVE = 0, BUTTON, ROTARY_ENCODER, NUMBEROFDIGITALTYPES } digitaltype_t;
typedef enum analogtype_t { INACTIVE_A = 0, SLIDER, NUMBEROFANALOGTYPES };
int digitaltype[numberOfDigitalPins];
int analogtype[numberOfAnalogPins];
int analogInputPins[] = { A0, A1, A2, A3, A4};
Rotary* rotaryEncoderList[numberOfDigitalPins];

void writeSizedMessage(char code, const char* reply) {
	Serial.print(F("{")); // F("...") used to save some program memory
	Serial.write(code);
	Serial.write(strlen(reply));
	Serial.write(reply, strlen(reply));
	Serial.print(F("}"));
}

void setup() {
	Serial.begin(9600);
	writeSizedMessage('M', "Starting device...");

	for (int i = 0; i < numberOfDigitalPins; ++i) {
		digitaltype[i] = digitaltype_t::INACTIVE;
    rotaryEncoderList[i] = nullptr;
    analogtype[i] = analogtype_t::INACTIVE_A;
	}
}

void processmessage() {
	if (messagetype == messagetype_t::COMMAND) {
		if (messagepayload[0] == 'A') { 
			if (messagepayload[2] == 'I')
				messagepayload[2] = digitaltype_t::INACTIVE;
			if (messagepayload[2] == 'B') // Button case
				messagepayload[2] = digitaltype_t::BUTTON;
//      Serial.print(messagepayload[1]); Serial.print(" payload < numberOFPins "); Serial.println(numberOfDigitalPins); Serial.println(3 < 14);
//      Serial.println(messagepayload[1] < numberOfDigitalPins);
//      Serial.println(messagepayload[2] < digitaltype_t::NUMBEROFDIGITALTYPES);
			if (messagepayload[1] < numberOfDigitalPins && messagepayload[2] < digitaltype_t::NUMBEROFDIGITALTYPES) {
				pinMode(messagepayload[1], INPUT);
				digitaltype[messagepayload[1]] = messagepayload[2];

				Serial.print(F("{M"));
				Serial.write(37);
				Serial.print(F("Command executed: "));
				Serial.write(messagepayload[0]);
				Serial.print(F(": type "));
				Serial.write(messagepayload[2] + '0');
				Serial.print(F(" on port "));
				Serial.write(messagepayload[1] + '0');
				Serial.print(F("}"));
			} else {
				//todo, error info
        Serial.println(F("Invalid port number given."));
			}
		} else if(messagepayload[0] == 'R'){ // Rotary encoder case --> 2 ports
      if (messagepayload[3] == 'I')
        messagepayload[3] = digitaltype_t::INACTIVE;
      if (messagepayload[3] == 'R')
        messagepayload[3] = digitaltype_t::ROTARY_ENCODER;
      
      if (messagepayload[1] < numberOfDigitalPins && messagepayload[2] < numberOfDigitalPins && messagepayload[3] < digitaltype_t::NUMBEROFDIGITALTYPES && messagepayload[4] < digitaltype_t::NUMBEROFDIGITALTYPES) {
        // Assume that the pin1 + 1 == pin2
        digitaltype[messagepayload[1]] = messagepayload[3];
        digitaltype[messagepayload[2]] = messagepayload[3];
        // Setup rotary encoder in the list position of port 1
        rotaryEncoderList[messagepayload[1]] = new Rotary(messagepayload[1],messagepayload[2]);

        // TODO Fix this.
        Serial.print(F("{M"));
        Serial.write(37);
        Serial.print(F("Command executed: "));
        Serial.write(messagepayload[0]);
        Serial.print(F(": type "));
        Serial.write(messagepayload[2] + '0');
        Serial.print(F(" on port "));
        Serial.write(messagepayload[1] + '0');
        Serial.print(F("}"));
      } else {
       //todo, error info
      }
		} else if (messagepayload[0] == 'S'){ // Slider case: Analog 1 port
        if (messagepayload[2] == 'I')
          messagepayload[2] = analogtype_t::INACTIVE_A;
        if (messagepayload[2] == 'S')
          messagepayload[2] = analogtype_t::SLIDER;
        // Process information
        if (messagepayload[1] < numberOfAnalogPins && messagepayload[2] < analogtype_t::NUMBEROFANALOGTYPES) {

          analogtype[messagepayload[1]] = messagepayload[2];
  
          Serial.print(F("{M"));
          Serial.write(3);
//          Serial.print(F("Command executed: "));
          Serial.write(messagepayload[0]);
//          Serial.print(F(": type "));
          Serial.write('A');
//          Serial.print(F(" on port "));
          Serial.write(messagepayload[1]);
          Serial.print(F("}"));
        } else {
          //todo, error info
          Serial.println(F("Invalid port number given."));
      }
		}	else if (messagepayload[0] == 'I') {
			writeSizedMessage('I', "{\"devicetype\":\"inputpasser\",\"version\":2}");
		} else {
      // no message payload matched
		}
	} else if (messagetype == messagetype_t::ERRORMESSAGE) {
		Serial.print(F("{X"));
		Serial.write(7 + messagesize);
		Serial.print(F("Error: "));
		Serial.write(messagepayload, messagesize);
		Serial.print(F("}"));
	} else {
		writeSizedMessage('M', "Message received");
	}
	command = 0;
	messagesize = 0;
	messagesizeread = 0;
	messagetype = messagetype_t::UNSET;
	messagedone = false;
}

void setErrorInParsing(const char* message) {
	messagetype = messagetype_t::ERRORMESSAGE;
	strcpy(messagepayload, message);
	messagesize = strlen(message);
	messagesizeread = messagesize;
	messagedone = true;
	state = 0;
}

void loop() {
	while (Serial.available() > 0) {
		int c = Serial.read();
		if (state == 0) { //Start of message
			if (c == '{') {
				command = 0;
				messagesize = 0;
				messagesizeread = 0;
				messagetype = messagetype_t::UNSET;
				messagedone = false;
				state = 1;
			}
		} else if (state == 1) { //Type
			if (c == 'C') {
				messagetype = messagetype_t::COMMAND;
				state = 5;
			} else if (c == 'M') {
				messagetype = messagetype_t::MESSAGE;
				state = 2;
			} else if (c == 'X') {
				messagetype = messagetype_t::ERRORMESSAGE;
				state = 2;
			} else {
				//This is not working as expected yet
				setErrorInParsing("Cannot parse message: Unknown message type:" + c);
			}
		} else if (state == 2) { //Payload size
			messagesize = (int)(c);
			messagesizeread = 0;
			if (messagesize == 0) {
				state = 4;
			} else if (messagesize > 1024) {
				setErrorInParsing("Cannot parse message: payload too large");
			} else {
				state = 3;
			}
		} else if (state == 3) { //Payload
			messagepayload[messagesizeread] = c;
			messagesizeread++;
			if (messagesizeread >= messagesize)
				state = 4;
		} else if (state == 4) { //End of message
			if (c == '}') {
				messagedone = true;
				state = 0;
			} else {
				setErrorInParsing("Cannot parse message: trailing data");
			}
		} else if (state == 5) { //Command
			messagepayload[0] = c;
			if (c == 'A') {
				messagesize = 3; // A_B
				messagesizeread = 1;
				state = 3;
			} else if (c == 'I') {
				messagesize = 1;
				messagesizeread = 1;
				state = 4;
			} else if (c == 'R') {
			  messagesize = 4; // R__R
        messagesizeread = 1;
        state = 3;
			} else if (c == 'S') {
        messagesize = 3; // S_S
        messagesizeread = 1;
        state = 3;
      } else {
				setErrorInParsing("Unknown command");
			}
		}
		if (messagedone){
			processmessage();
		}
	}

	//Digital pins
	for (int i = 0; i < numberOfDigitalPins; ++i) {
		//Buttons
		if (digitaltype[i] == digitaltype_t::BUTTON) {
			int buttonState = digitalRead(i);
			Serial.print(F("{DB"));
			Serial.write((unsigned char)(i));
			Serial.write((unsigned char)(buttonState));
			Serial.print(F("}"));
		}
    // Rotary encoder
    else if (digitaltype[i] == digitaltype_t::ROTARY_ENCODER){
      // TODO: handle reading rotary encoder rotation
      // We can asume the next digitalpin is also a rotary encoder pin. 
      if(rotaryEncoderList[i] != nullptr){
        // Get direction of movement from rotary encoder (if changed)
        unsigned char result = rotaryEncoderList[i]->process();
        // result is Clock Wise or Counter Clock Wise
        // Handling of the result is done in Unity.
        int rotationResult = 0;
        if (result == DIR_CW) {
          rotationResult = 1;
        } else if (result == DIR_CCW) {
          rotationResult = -1;
        }
        if(rotationResult != 0){
          Serial.print(F("{DR"));
          Serial.write((unsigned char)(i));
          Serial.write((unsigned char)(rotationResult));
          Serial.print(F("}"));
        }  
      }
    }
  }
  // Analog pins
  for(int i = 0; i < numberOfAnalogPins; ++i){
    // Slider
    if(analogtype[i] == analogtype_t::SLIDER){
      int analogValue = analogRead(analogInputPins[i]);
      // Tranform from 0-1023 value to 0-255 value so it fits into a byte
      analogValue = analogValue / 4;
      Serial.print(F("{DS"));
      Serial.write((unsigned char)(i));
      Serial.write((unsigned char)(analogValue));
      Serial.print(F("}"));
      Serial.println(analogValue);
    }
  }

	delay(2);
}
