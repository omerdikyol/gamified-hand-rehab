using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using TMPro;

[Serializable]
public class SerialPortConfig
{
    public string portName;
    public int baudRate;
}

public class SerialPortManager : MonoBehaviour
{
    [Header("Serial Port Settings")]
    public string portName = "COM3"; // Default port name
    public int baudRate = 115200; // Default baud rate
    private SerialPort serialPort;
    private Thread serialThread;
    private bool isRunning = true;

    [Header("Data Queue")]
    public ConcurrentQueue<string> DataQueue = new ConcurrentQueue<string>();
    public bool isCalibrated = false; // Track if the system has been calibrated

    void Start()
    {
        LoadConfig();
        OpenSerialPort();
        StartSerialThread();
    }

    void LoadConfig()
    {
        string configPath = Path.Combine(Application.persistentDataPath, "SerialPortConfig.json");

        // Check if the file exists in persistent data path
        if (!File.Exists(configPath))
        {
            // If not, create a new config file with default values
            SerialPortConfig config = new SerialPortConfig
            {
                portName = portName,
                baudRate = baudRate
            };
            string configJson = JsonUtility.ToJson(config, true);
            File.WriteAllText(configPath, configJson);
            Debug.Log($"Created config file: {configPath}");
        }

        // Read and parse the config file from persistent data path
        try
        {
            string configJson = File.ReadAllText(configPath);
            SerialPortConfig config = JsonUtility.FromJson<SerialPortConfig>(configJson);
            portName = config.portName;
            baudRate = config.baudRate;
            Debug.Log($"Loaded config: portName={portName}, baudRate={baudRate}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to read config: {ex.Message}");
        }
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
