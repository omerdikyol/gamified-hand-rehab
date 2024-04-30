using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;       // Reference to the Tutorial Panel

    public GameObject[] tutorialPages;      // Array of Tutorial Pages
    public Button startGameButton;         // Reference to the Start Game Button
    public VideoPlayer[] videoPlayers;     // Array of VideoPlayers

    public int index = 0;

    void Start()
    {
        ShowTutorial();  // Show the tutorial panel
    }

    void ShowTutorial()
    {
        tutorialPanel.SetActive(true);  // Show the tutorial panel
        // Time.timeScale = 0;  // Pause the game
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

    void StopAllVideos()
    {
        foreach (var player in videoPlayers)
        {
            player.Stop();  // Stop each video
        }
    }

    public void NextPage()
    {
        if (index == tutorialPages.Length - 1)  // Check if the current page is the last page
        {
            return;  // Exit the method
        }
        tutorialPages[index].SetActive(false);  // Hide the current page
        tutorialPages[index + 1].SetActive(true);  // Show the next page
        index++;  // Increment the index
    }

    public void PreviousPage()
    {
        if (index == 0)  // Check if the current page is the first page
        {
            return;  // Exit the method
        }
        tutorialPages[index].SetActive(false);  // Hide the current page
        tutorialPages[index - 1].SetActive(true);  // Show the previous page
        index--;  // Decrement the index
    }
}
