using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using TMPro;


public class SerialPortManager : MonoBehaviour
{
    [Header("Serial Port Settings")]
    public string portName = "COM3";
    public int baudRate = 9600;
    private SerialPort serialPort;
    private Thread serialThread;
    private bool isRunning = true;

    [Header("Data Queue")]
    public ConcurrentQueue<string> DataQueue = new ConcurrentQueue<string>();

    [Header("Threshold Values")]
    public float quaternionThreshold = 0.08f; // Threshold for avoiding the unnecessary rotation of the hand model

    public bool isCalibrated = false; // Track if the system has been calibrated

    [Header("Calibration Values")]
    private float[] fingerMinValues = new float[5]; // Minimum calibration values for fingers
    private float[] fingerMaxValues = new float[5]; // Maximum calibration values for fingers
    private float[] tempCalibrationValues = new float[5]; // Temporary storage for calibration values
    private Vector3 initialPosition;  // Store the initial position of the model
    private Quaternion initialRotation; // Store the initial rotation of the model
    private float qw, qx, qy, qz; // Quaternion values for the model rotation
    private float prevQw, prevQx, prevQy, prevQz; // Previous quaternion values
    private float[] prevFingerValues = new float[5]; // Previous finger values
    private float[] fingerNormalizedValues = new float[5]; // Normalized finger values

    void Start()
    {
        OpenSerialPort();
        StartSerialThread();
    }

    void OpenSerialPort()
    {
        serialPort = new SerialPort(portName, baudRate);
        try
        {
            serialPort.Open();
            Debug.Log("Serial port opened successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to open serial port: {ex.Message}");
        }
    }

    void StartSerialThread()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialThread = new Thread(ReadSerialPort);
            isRunning = true;
            serialThread.Start();
        }
    }

    void ReadSerialPort()
    {
        while (isRunning)
        {
            try
            {
                if (serialPort.IsOpen && serialPort.BytesToRead > 0)
                {
                    string data = serialPort.ReadLine();
                    // Debug.Log($"Enqueuing data: {data}");
                    DataQueue.Enqueue(data);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading from serial port: {ex.Message}");
            }
        }
    }

    void OnDisable()
    {
        if (serialThread != null)
        {
            isRunning = false;
            serialThread.Join();
        }

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial port closed");
        }
    }
}
