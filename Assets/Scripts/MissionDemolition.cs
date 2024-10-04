using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameMode { 
    idle,
    playing,
    levelEnd,
    gameOver
}

public class MissionDemolition : MonoBehaviour { 
    static private MissionDemolition S;  // a private Singleton
    public GameController gameController;

    [Header("Inscribed")]
    public TMP_Text uitLevel;  // The UIText_Level Text
    public TMP_Text uitShots;  // The UIText_Shots Text
    public TMP_Text uitTimer;  // The UIText_Timer Text

    public Vector3 castlePos;  // The place to put castles
    public GameObject[] castles;  // An array of the castles
    public float levelTime = 60f;  // Total allowed time per level (60 seconds)

    [Header("Dynamic")]
    public int level;  // The current level
    public int levelMax;  // The number of levels
    public int shotsTaken;  // The number of shots taken
    public GameObject castle;  // The current castle
    public GameMode mode = GameMode.idle;  // FollowCam mode
    private float timeRemaining;   // Tracks remaining time during the level

    void Start() { 
        S = this;  // Define the Singleton

        level = 0;
        shotsTaken = 0;
        levelMax = castles.Length;

        StartLevel();
    }

    void StartLevel() {
        // Reset the timer
        timeRemaining = levelTime;

        // Get rid of the old castle if one exists
        if (castle != null) {
            Destroy(castle);
        }

        // Destroy old projectiles if they exist
        Projectile.DESTROY_PROJECTILES();

        // Instantiate the new castle
        castle = Instantiate<GameObject>(castles[level]);
        castle.transform.position = castlePos;

        // Reset the goal
        Goal.goalMet = false;

        // Update the GUI
        UpdateGUI();

        // Set mode to playing
        mode = GameMode.playing;

        // Zoom out to show both
        FollowCam.SWITCH_VIEW(FollowCam.eView.both);
    }

    void UpdateGUI() {
        // Show the data in the GUITexts
        uitLevel.text = "Level: " + (level + 1) + " of " + levelMax;
        uitShots.text = "Shots Taken: " + shotsTaken;
        uitTimer.text = "Time Left: " + Mathf.Ceil(timeRemaining) + "s";
    }

    void Update() {
        if (mode == GameMode.playing) {
            // Update the timer
            timeRemaining -= Time.deltaTime;

            // Clamp the timer to ensure it doesn't go below zero
            if (timeRemaining <= 0) {
                timeRemaining = 0;
                EndGame();  // Trigger Game Over if time runs out
            }

            // Update the GUI to reflect changes in time, shots, etc.
            UpdateGUI();

            // Check if the goal is met or shots exceed limit to end level
            if (Goal.goalMet || shotsTaken >= 10) {
                // Change mode to levelEnd to stop further checks
                mode = GameMode.levelEnd;

                // Zoom out to show the full view
                FollowCam.SWITCH_VIEW(FollowCam.eView.both);

                // Start the next level in 2 seconds
                Invoke("NextLevel", 2f);
            }
        }
    }

    void NextLevel() {
        if (mode != GameMode.levelEnd) {
            return;  // Prevent multiple calls to NextLevel
        }

        level++;
        if (level >= levelMax) { 
            level = 0;  // Loop back to the first level if all levels are completed
        }

        shotsTaken = 0;
        StartLevel();
    }

    void EndGame() {
        mode = GameMode.gameOver;  // Set mode to gameOver to prevent further actions
        gameController.ShowGameOver();  // Show the Game Over screen
    }

    // Static method that allows code anywhere to increment shotsTaken
    static public void SHOT_FIRED() {
        if (S != null) {
            S.shotsTaken++;
            S.UpdateGUI();
        }
    }

    // Static method that allows code anywhere to get a reference to S.castle
    static public GameObject GET_CASTLE() {
        return S.castle;
    }
}
