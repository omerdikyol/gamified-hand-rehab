# Gamified hand rehabilitation

An Arduino glove connected to Unity games through USB serial input. Finger potentiometers and an MPU6050 provide hand measurements; calibration and gesture handling turn those readings into game interactions.

The project combines a physical input device, an Arduino data protocol, and a Unity application with rehabilitation-themed activities. It is a research prototype; the repository demonstrates the device/software integration rather than establishing clinical treatment outcomes.

[Related publication: IEEE ASYU 2024](https://ieeexplore.ieee.org/document/10757096) · [Download a release](https://github.com/omerdikyol/gamified-hand-rehab/releases)

## How the pieces connect

1. [Glove.ino](Arduino/Source%20Code/Glove/Glove.ino) samples the finger sensors and orientation and emits serial records at 115200 baud.
2. [SerialPortManager](Unity/cse492/Assets/Scripts/Hand/SerialPortManager.cs) receives device data in Unity.
3. [InputController](Unity/cse492/Assets/Scripts/Hand/InputController.cs) interprets calibrated input for the hand/game state.
4. The Unity scenes use those states for game interaction and feedback.

## Open the projects

- Arduino: open `Arduino/Source Code/Glove/Glove.ino` in the Arduino IDE. The supporting MPU6050/I2Cdev source is included in `Arduino/Source Code`; configure the libraries for your board before uploading.
- Unity: open `Unity/cse492` in **Unity 2022.3.20f1**, as recorded in [ProjectVersion.txt](Unity/cse492/ProjectSettings/ProjectVersion.txt).
- Connect the glove over USB, configure its serial port in the Unity application, and calibrate the sensors before playing.

The sketch uses `A0`, `A1`, `A2`, `A3`, and `A6` for finger input. Choose a compatible board and check the wiring against the sketch. The physical glove is required for the complete interaction path.


## Screenshots

![app_ss](https://github.com/user-attachments/assets/821c5339-bccd-4b32-ae77-92d48e22c3cd)

![calibration_scene](https://github.com/user-attachments/assets/9075e2cf-103d-4b0e-bd2b-67a339b1c622)

![bird_jump_gameplay](https://github.com/user-attachments/assets/b8e3ef1d-0ecc-42b4-9eb0-be3e34cf0c2c)

![space_shooter_gameplay](https://github.com/user-attachments/assets/d4483456-1d75-4180-89eb-00e9c22285e9)
