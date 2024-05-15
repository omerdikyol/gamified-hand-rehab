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
    public string portName = "COM8";
    public int baudRate = 9600;
    private SerialPort serialPort;
    private Thread serialThread;
    private bool isRunning = true;

    [Header("Data Queue")]
    public ConcurrentQueue<string> DataQueue = new ConcurrentQueue<string>();
    public bool isCalibrated = false; // Track if the system has been calibrated

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
