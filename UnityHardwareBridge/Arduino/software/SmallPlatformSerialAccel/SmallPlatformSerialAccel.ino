#include <AccelStepper.h>

/*
 * Variables -----------------------------------------------------------------------------------
 */

int XLeftStop = 4; // old:  = 5
int XRightStop = 5; // old: = 4
int YTopStop = 3;
int YBottomStop = 2;

int XPUL = 9; //define Pulse pin
int XDIR = 8; //define Direction pin
int XENA = 7; //define Enable Pin

int YPUL = 10; //define Pulse pin
int YDIR = 11; //define Direction pin
int YENA = 12; //define Enable Pin

int XL = 1, XR = 1, YT = 1, YB = 1;

AccelStepper stepperX(AccelStepper::DRIVER, XPUL, XDIR);
AccelStepper stepperY(AccelStepper::DRIVER, YPUL, YDIR);

bool errorstate = false; //errorstate means no more movement allowed
const byte numChars = 32;
char receivedChars[numChars];
char tempChars[numChars];        // temporary array for use when parsing

boolean newData = false;

const bool moveWithAcceleration = false;

const float stepperXSpeed = 600.0; //2000.0; //1500.0
// 800 works, 1000 doesn't, 900 doesn't
const float stepperYSpeed = 800.0; //1200.0; //1500.0


/* ------ stepper Y : 4 microsteps --> 800 full rotation------ */
const int STEPPER_Y_BOTTOM_LIMIT = 0; // 100
const int STEPPER_Y_UPPER_LIMIT = 1125; // 9100

///* ------ stepper X : 1 microsteps --> 200 full rotation------ */
//const int STEPPER_X_BOTTOM_LIMIT = 0; // 200
//const int STEPPER_X_UPPER_LIMIT = 500; // 8800
/* ------ stepper X : 2 microsteps --> 400 full rotation------ */
const int STEPPER_X_BOTTOM_LIMIT = 0; // 200
const int STEPPER_X_UPPER_LIMIT = 1000; // 8800

//const float stepperXAcceleration = 375.0; // 200
const float stepperXAcceleration = 1000.0; // 400 steps
const float stepperYAcceleration = 1500.0; 

float lastSavedPositionX = 0;
float lastSavedPositionY = 0;

/*
 * Extra functions -----------------------------------------------------------------------------------
 */

/* Stepper startup functions */
void home_x() {
  long initial_homing = -1;
  stepperX.setMaxSpeed(stepperXSpeed);
  stepperX.setAcceleration(102.0); // 102

  while (digitalRead(XRightStop)) {
    stepperX.moveTo(initial_homing);
    initial_homing--;
    stepperX.run();
  }
  stepperX.setCurrentPosition(0);
  stepperX.setMaxSpeed(200);
  initial_homing = 1;
  while (!digitalRead(XRightStop)) {
    stepperX.moveTo(initial_homing);
    stepperX.run();
    initial_homing++;
  }

  stepperX.setCurrentPosition(0);

}

void home_y() {
  long initial_homing = -1;
  stepperY.setMaxSpeed(stepperYSpeed);
  stepperY.setAcceleration(102.0); 

  while (digitalRead(YBottomStop)) {
    stepperY.moveTo(initial_homing);
    initial_homing--;
    stepperY.run();
  }
  stepperY.setCurrentPosition(0);
  stepperY.setMaxSpeed(200);
  initial_homing = 1;

  while (!digitalRead(YBottomStop)) {
    stepperY.moveTo(initial_homing);
    stepperY.run();
    initial_homing++;
  }
  stepperY.setCurrentPosition(0);
}

void MoveSteppersAccel(long goToPositionX, long goToPositionY){
  // X AXIS
    if (goToPositionX >= STEPPER_X_BOTTOM_LIMIT && goToPositionX <= STEPPER_X_UPPER_LIMIT) 
    {
      lastSavedPositionX = goToPositionX;
      stepperX.moveTo(goToPositionX);
    }
    // outside range, move to last saved position
    else
    {
      stepperX.moveTo(lastSavedPositionX);
    }
    // Y AXIS
    if (goToPositionY >= STEPPER_Y_BOTTOM_LIMIT && goToPositionY <= STEPPER_Y_UPPER_LIMIT) 
    {
      lastSavedPositionY = goToPositionY;
      stepperY.moveTo(goToPositionY);
    }
    // outside range, move to last saved position
    else
    {
      stepperY.moveTo(lastSavedPositionY);
    }
}

void MoveSteppersConstant(long goToPositionX, long goToPositionY){
  // X AXIS
    if (goToPositionX >= STEPPER_X_BOTTOM_LIMIT && goToPositionX <= STEPPER_X_UPPER_LIMIT) 
    {
      lastSavedPositionX = goToPositionX;
      stepperX.moveTo(goToPositionX);
      // Move with constant speed
      stepperX.setSpeed(stepperXSpeed);
    }
    // outside range, move to last saved position
    else
    {
      stepperX.moveTo(lastSavedPositionX);
      // Move with constant speed
      stepperX.setSpeed(stepperXSpeed);
    }
    // Y AXIS
    if (goToPositionY >= STEPPER_Y_BOTTOM_LIMIT && goToPositionY <= STEPPER_Y_UPPER_LIMIT) 
    {
      lastSavedPositionY = goToPositionY;
      stepperY.moveTo(goToPositionY);
      // Move with constant speed
      stepperY.setSpeed(stepperYSpeed);
    }
    // outside range, move to last saved position
    else
    {
      stepperY.moveTo(lastSavedPositionY);
      // Move with constant speed
      stepperY.setSpeed(stepperYSpeed);
    }
}


/* Serial communication */

void SerialCommandListener() {
  recvWithStartEndMarkers();
  if (newData == true) {
    strcpy(tempChars, receivedChars);
    char * strtokIndx; // this is used by strtok() as an index
    long positionX = 0;
    long positionY = 0;

    strtokIndx = strtok(tempChars, ",");     // get the first part
    positionX = atol(strtokIndx); // Convert to travelX

    strtokIndx = strtok(NULL, ","); // this continues where the previous call left off
    positionY = atol(strtokIndx);     // Convert to travelY

    strtokIndx = strtok(NULL, ","); // this continues where the previous call left off
    if(strtokIndx != NULL){
      if(*strtokIndx == 'P'){
        // Set the new position of the platform.
        stepperX.setCurrentPosition(positionX);
        stepperX.setMaxSpeed(stepperXSpeed);
        stepperY.setCurrentPosition(positionY);
        stepperY.setMaxSpeed(stepperYSpeed);
      }
      else {
        Serial.print("Wrong command, should be P: ");
      }
    }
    else {
      if(moveWithAcceleration)
        MoveSteppersAccel(positionX, positionY);
      else
        MoveSteppersConstant(positionX, positionY);
    }


    /* ------ Steppers Control ------- */

    

    newData = false;
  }

  if (!errorstate) {
    if(moveWithAcceleration)
    {
      // Move with acceleration
      stepperX.run();
      stepperY.run();
    }
    else {
      // Move with constant speed
      stepperX.runSpeedToPosition();
      stepperY.runSpeedToPosition();
    }
  }
}

void recvWithStartEndMarkers() {
  static boolean recvInProgress = false;
  static byte ndx = 0;
  char startMarker = '<';
  char endMarker = '>';
  char rc;

  while (Serial.available() > 0 && newData == false) {
    rc = Serial.read();

    if (recvInProgress == true) {
      if (rc != endMarker) {
        receivedChars[ndx] = rc;
        ndx++;
        if (ndx >= numChars) {
          ndx = numChars - 1;
        }
      }
      else {
        receivedChars[ndx] = '\0'; // terminate the string
        recvInProgress = false;
        ndx = 0;
        newData = true;
      }
    }

    else if (rc == startMarker) {
      recvInProgress = true;
    }
  }
}

/* 
 *  Main setup and loop -----------------------------------------------------------------------------------
 */

void setup() {
  // put your setup code here, to run once:
  pinMode(XENA, OUTPUT);
  pinMode(YENA, OUTPUT);
  pinMode(13, OUTPUT);

  pinMode(XLeftStop, INPUT_PULLUP);
  pinMode(XRightStop, INPUT_PULLUP);
  pinMode(YTopStop, INPUT_PULLUP);
  pinMode(YBottomStop, INPUT_PULLUP);

  digitalWrite(XENA, HIGH);
  digitalWrite(YENA, HIGH);

  Serial.begin(9600); // 115200
  
//  Serial.println("Startinig up!");
  delay(3);

  home_x();

//  Serial.println("Home x finished");
  
  home_y();
  
  Serial.println("homing finished");

  stepperX.moveTo(10);
  stepperY.moveTo(10);

  stepperX.setMaxSpeed(stepperXSpeed);
  stepperX.setAcceleration(stepperXAcceleration);

  stepperY.setMaxSpeed(stepperYSpeed);
  stepperY.setAcceleration(stepperYAcceleration);
}

void loop() {
  // REad input from the stops
  XL = digitalRead(XLeftStop);
  XR = digitalRead(XRightStop);
  YT = digitalRead(YTopStop);
  YB = digitalRead(YBottomStop);

//  Serial.print("XL: ");
//  Serial.print(XL);
//  Serial.print(" XR: ");
//  Serial.print(XR);
//  Serial.print(" YT: ");
//  Serial.print(YT);
//  Serial.print(" YB: ");
//  Serial.println(YB);

  SerialCommandListener();


  // When a end stop is hit, home back to 0 position for that axis.
//  if (!(XL)) {
//    Serial.println("Home X Left  triggered");
//    stepperX.stop();
//    home_x();
//    stepperX.setMaxSpeed(stepperXSpeed);
//    stepperX.setAcceleration(stepperXAcceleration);
//    
//  }
//  if (!(XR)) {
//    Serial.println("Home X Right triggered");
//    stepperX.stop();
//    home_x();
//    stepperX.setMaxSpeed(stepperXSpeed);
//    stepperX.setAcceleration(stepperXAcceleration);
//    
//  }
//
//  if (!(YT)) {
//    Serial.println("Home -Y- Top triggered");
//    stepperY.stop();
//    home_y();
//    stepperY.setMaxSpeed(stepperYSpeed);
//    stepperY.setAcceleration(stepperYAcceleration);
//    
//  }
//
//  if (!(YB)) {
//    Serial.println("Home -Y- Bottom triggered");
//    stepperY.stop();
//    home_y();
//    stepperY.setMaxSpeed(stepperYSpeed);
//    stepperY.setAcceleration(stepperYAcceleration);
//    
//  }
  
}
