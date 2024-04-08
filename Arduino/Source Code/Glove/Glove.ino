// For USB connection 

#include <Wire.h>
#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps20.h"

MPU6050 accelgyro;

// Analog pins for the potentiometers
const int fingerPins[] = {A0, A1, A2, A3, A6};
const int numFingers = 5;
int fingerValues[numFingers];

// MPU control/status vars
uint8_t devStatus;      // return status after each device operation (0 = success, !0 = error)
uint16_t packetSize;    // expected DMP packet size (default is 42 bytes)
uint8_t fifoBuffer[64]; // FIFO storage buffer

// orientation/motion vars
Quaternion q;           // [w, x, y, z]         quaternion container

void setup() {
  Serial.begin(115200);
  Wire.begin();

  // Initialize the MPU6050 sensor
  accelgyro.initialize();
  accelgyro.setFullScaleGyroRange(MPU6050_GYRO_FS_250);
  accelgyro.setFullScaleAccelRange(MPU6050_ACCEL_FS_2);

  devStatus = accelgyro.dmpInitialize();

  // supply your own gyro offsets here, scaled for min sensitivity
  accelgyro.setXGyroOffset(220);
  accelgyro.setYGyroOffset(76);
  accelgyro.setZGyroOffset(-85);
  accelgyro.setZAccelOffset(2000); // 1688 factory default for my test chip

  // make sure it worked (returns 0 if so)
  if (devStatus == 0) {
    accelgyro.setDMPEnabled(true);

    // Calibration Time: generate offsets and calibrate our MPU6050
    accelgyro.CalibrateAccel(6);
    accelgyro.CalibrateGyro(6);
    accelgyro.PrintActiveOffsets();
    accelgyro.setDMPEnabled(true);

    // get expected DMP packet size for later comparison
    packetSize = accelgyro.dmpGetFIFOPacketSize();
  }

  // Initialize finger potentiometer pins
  for (int i = 0; i < numFingers; i++) {
    pinMode(fingerPins[i], INPUT);
  }
}

void loop() {
  // Read the accelerometer and gyroscope values
  int16_t ax, ay, az;
  int16_t gx, gy, gz;
  accelgyro.getMotion6(&ax, &ay, &az, &gx, &gy, &gz);

  if (accelgyro.dmpGetCurrentFIFOPacket(fifoBuffer)) { // Get the Latest packet 
      // display quaternion values in easy matrix form: w x y z
      accelgyro.dmpGetQuaternion(&q, fifoBuffer);
      // Send accelerometer and gyroscope values
      Serial.print(q.w);
      Serial.print(",");
      Serial.print(q.x);
      Serial.print(",");
      Serial.print(q.y);
      Serial.print(",");
      Serial.print(q.z);
  }

  // Read finger potentiometer values
  for (int i = 0; i < numFingers; i++) {
    fingerValues[i] = analogRead(fingerPins[i]);
  }
  
  // Send finger values
  for (int i = 0; i < numFingers; i++) {
    Serial.print(",");
    Serial.print(fingerValues[i]);
  }
  Serial.println();

  delay(100); // Delay before the next reading
}

// For Bluetooth Connection

// Coming soon..
