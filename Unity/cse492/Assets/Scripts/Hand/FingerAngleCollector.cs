using System.IO;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class FingerAngleCollector : MonoBehaviour
{
    private StreamWriter streamWriter;
    private bool isLogging = false;
    private string currentLogFilePath;

    public void ToggleLogging()
    {
        if (isLogging)
        {
            // Close the current file and stop logging
            streamWriter.Close();
            isLogging = false;
            Debug.Log($"Logging stopped. Data saved to {currentLogFilePath}");
        }
        
        // Start a new logging session with a new file
        StartNewLogFile();
    }

    private void StartNewLogFile()
    {
        // Create a new file with the current date and time as its name
        currentLogFilePath = Path.Combine(Application.persistentDataPath, $"FingerMovements_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv");
        streamWriter = new StreamWriter(currentLogFilePath, append: true);
        streamWriter.WriteLine("Timestamp,Scene,Thumb,Index,Middle,Ring,Pinky");
        isLogging = true;
        // Optionally, update button text or UI state here if needed
        Debug.Log($"Logging started. Data will be saved to {currentLogFilePath}");
    }

    public void LogData(float[] angles)
    {
        if (isLogging)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string sceneName = SceneManager.GetActiveScene().name;
            streamWriter.WriteLine($"{timestamp},{sceneName},{angles[0]:F2},{angles[1]:F2},{angles[2]:F2},{angles[3]:F2},{angles[4]:F2}");
            streamWriter.Flush(); // Ensure data is written to the file immediately
        }
    }

    void OnDisable()
    {
        // Close the StreamWriter if it's still open when the game closes
        if (streamWriter != null)
        {
            streamWriter.Close();
            Debug.Log("Logging stopped. Data saved to " + currentLogFilePath);
        }
    }
}
