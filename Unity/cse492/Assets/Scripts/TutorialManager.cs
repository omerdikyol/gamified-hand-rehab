using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;       // Reference to the Tutorial Panel
    public Button startGameButton;         // Reference to the Start Game Button
    public VideoPlayer[] videoPlayers;     // Array of VideoPlayers

    void Start()
    {
        ShowTutorial();  // Show the tutorial panel
    }

    void ShowTutorial()
    {
        tutorialPanel.SetActive(true);  // Show the tutorial panel
        Time.timeScale = 0;  // Pause the game
        PlayAllVideos();  // Start playing all videos
    }

    void PlayAllVideos()
    {
        foreach (var player in videoPlayers)
        {
            if (!player.isPlaying) // Check if the player is not already playing
            {
                player.Play();  // Play each video
            }
        }
    }

    public void StartGame()
    {
        tutorialPanel.SetActive(false);  // Hide the tutorial panel
        Time.timeScale = 1;  // Resume the game
        StopAllVideos();  // Stop all videos
    }

    void StopAllVideos()
    {
        foreach (var player in videoPlayers)
        {
            player.Stop();  // Stop each video
        }
    }
}
