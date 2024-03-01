// For USB connection 

#include <Wire.h>
#include "I2Cdev.h"
#include "MPU6050.h"

MPU6050 accelgyro;

// Analog pins for the potentiometers
const int fingerPins[] = {A0, A1, A2, A3, A6};
const int numFingers = 5;
int fingerValues[numFingers];

void setup() {
  Serial.begin(9600);
  Wire.begin();

  // Initialize the MPU6050 sensor
  accelgyro.initialize();
  accelgyro.setFullScaleGyroRange(MPU6050_GYRO_FS_250);
  accelgyro.setFullScaleAccelRange(MPU6050_ACCEL_FS_2);

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

  // Read finger potentiometer values
  for (int i = 0; i < numFingers; i++) {
    fingerValues[i] = analogRead(fingerPins[i]);
  }

  // Send accelerometer, gyroscope, and finger potentiometer data in CSV format
  Serial.print(ax); Serial.print(",");
  Serial.print(ay); Serial.print(",");
  Serial.print(az); Serial.print(",");
  Serial.print(gx); Serial.print(",");
  Serial.print(gy); Serial.print(",");
  Serial.print(gz);
  
  // Send finger values
  for (int i = 0; i < numFingers; i++) {
    Serial.print(",");
    Serial.print(fingerValues[i]);
  }
  Serial.println();

  delay(100); // Delay before the next reading
}


/*

// For Bluetooth Connection

// Coming soon..
