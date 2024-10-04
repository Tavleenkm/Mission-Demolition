using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public GameObject gameOverPanel; // Reference to the Game Over UI panel

    void Start() {
        // Hide the Game Over panel initially
        gameOverPanel.SetActive(false);
    }

    // Function to show the Game Over screen
    public void ShowGameOver() {
        gameOverPanel.SetActive(true);
    }

    // Function to restart the game
    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // Reload the current scene
    }
}
