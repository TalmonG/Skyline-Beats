using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int score = 0;
    public int lives = 5;
    public int combo = 0;
    public int multiplier = 1;
    private int multiplierCounter = 0;

    public bool shouldGameContinue = false;
    public bool isGameResetting = false;
    public bool STARTGAME = false;
    public bool isGameStarted = false;
    public bool isLevelComplete = false;
    public bool isInvincible = false;

    [SerializeField] 
    private TextMeshPro scoreText;
    [SerializeField]
    private TextMeshPro livesText;
    [SerializeField]
    private TextMeshPro comboText;
    [SerializeField]
    private TextMeshPro multiplierText;

    [SerializeField]
    private TextMeshProUGUI scoreLevelCompleteText; // Keep it private but visible in Inspector    
    [SerializeField]
    private TextMeshProUGUI maxComboLevelCompleteText;

    private SongManager songManager;
    public AudioManager audioManager;
    private UIManager uIManager;

    void Start()
    {
        songManager = FindObjectOfType<SongManager>();
        audioManager = FindObjectOfType<AudioManager>();
        uIManager = FindObjectOfType<UIManager>();
        // Try to find texts if not assigned in inspector
        if (scoreText == null)
            scoreText = GameObject.FindGameObjectWithTag("ScoreText")?.GetComponent<TextMeshPro>();
        if (livesText == null) 
            livesText = GameObject.FindGameObjectWithTag("LivesText")?.GetComponent<TextMeshPro>();
        if (comboText == null)
            comboText = GameObject.FindGameObjectWithTag("ComboText")?.GetComponent<TextMeshPro>();
        if (multiplierText == null) 
            multiplierText = GameObject.FindGameObjectWithTag("MultiplierText")?.GetComponent<TextMeshPro>();
        if (scoreLevelCompleteText == null)
        {
            scoreLevelCompleteText = GameObject.FindGameObjectWithTag("ScoreLevelCompleteText")?.GetComponent<TextMeshProUGUI>();
            //Debug.Log("part one done");
        }
        else
        {
            //Debug.Log("RAGASF");
        }
        if (maxComboLevelCompleteText == null)
            maxComboLevelCompleteText = GameObject.FindGameObjectWithTag("MaxComboLevelCompleteText")?.GetComponent<TextMeshProUGUI>();

        if (scoreText == null || livesText == null || comboText == null || multiplierText == null || scoreLevelCompleteText == null || maxComboLevelCompleteText == null)
        {
            Debug.LogError("Could not find score or lives text components!");
            return;
        }
        // Initialize UI
        scoreText.text = "" + score;
        livesText.text = "Lives: " + lives;
        comboText.text = "Combo\n" + combo;
        multiplierText.text = "X" + multiplier;

        // Find and initialize toggle state
        Toggle invincibilityToggle = GameObject.FindObjectOfType<Toggle>();
        if (invincibilityToggle != null)
        {
            isInvincible = invincibilityToggle.isOn;
            invincibilityToggle.onValueChanged.AddListener(ToggleInvincibility);
        }
    }

    void Update()
    {
        if (STARTGAME == true && isGameStarted == false)
        {
            isGameStarted = true;

            StartGame();
        }

        if (lives <= 0 && isGameResetting == false) // Player loses
        {
            Debug.Log("Game Over");
            shouldGameContinue = false;
            isGameResetting = true;
            songManager.SuddenStopSong();
            audioManager.Play("PowerDown");
            GameOver();
        }

    }

    public void LevelComplete()
    {
        //isLevelComplete = true; // Useless for now but might be useful in future
        // coming from noteSpawner script
        uIManager.EnableLevelCompleteCanvas();
        songManager.FadeOutSong();
        scoreLevelCompleteText.text = "Score: " + score;
        //maxComboLevelCompleteText.text = "Max Combo: " + combo;
    }

    public void StartGame()
    {
        shouldGameContinue = true;
        songManager.StartSong();
    }

    private void GameOver()
    {
        Debug.Log("Game Over");
        songManager.FadeOutSong();
        // TODO: Add game over screen
        uIManager.EnableLevelFailedCanvas();
        //ResetGame();
    }

    public void ResetGame()
    {
        // Destroy all existing DrumNote objects in the scene
        DrumNote[] existingNotes = FindObjectsOfType<DrumNote>();
        foreach (DrumNote note in existingNotes)
        {
            Destroy(note.gameObject);
        }

        uIManager.DisableLevelCompleteCanvas();
        uIManager.EnableLevelSelectorCanvas();
        uIManager.EnableHowToPlayCanvas();
        uIManager.EnableLeaderboardCanvas();

        score = 0;
        lives = 5;
        combo = 0;
        multiplier = 1;
        multiplierCounter = 0;
        shouldGameContinue = false;  // Changed from true to false
        isGameResetting = false;
        isGameStarted = false;       // Added this line
        STARTGAME = false;          // Added this line
        isInvincible = false;  // Also reset invincibility when game resets

        scoreText.text = "" + score;
        livesText.text = "Lives: " + lives;
        comboText.text = "Combo\n" + combo;
        multiplierText.text = "X" + multiplier;
        multiplierText.color = new Color32(255, 255, 255, 255);

        Debug.Log("Game Reset");
    }

    public void UpdateCombo(string status)
    {
        switch(status)
        {
            case "Hit":
                combo++;
                break;
            case "Miss":
                combo = 0;
                break;
        }
        comboText.text = "" + combo;
    }
    public void UpdateMultiplier(string status)
    {
        switch(status)
        {
            case "Hit":
                multiplierCounter++;
                if (multiplierCounter >= 1 && multiplierCounter <= 2)
                {
                    multiplier = 2;
                    multiplierText.color = new Color32(0, 255, 59, 255);
                }
                else if (multiplierCounter >= 4 && multiplierCounter <= 8)
                {
                    multiplier = 4;
                    multiplierText.color = new Color32(255, 227, 0, 255);
                }
                else if (multiplierCounter >= 12 && multiplierCounter <= 16)
                {
                    multiplier = 8;
                    multiplierText.color = new Color32(255, 0, 155, 255);
                }

                break;
            case "Miss":
                multiplierCounter = 0;
                multiplier = 1;
                multiplierText.color = new Color32(255, 255, 255, 255);
                break;
        }
        multiplierText.text = "X" + multiplier;
    }

    public void AddScore()
    {

        score += multiplier;
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void LoseLife()
    {
        if (isInvincible) return;
        
        if (lives > 0)  
        {
            lives--;
            if (livesText != null)
                livesText.text = "Lives: " + lives;
        }
    }


    public void AddLife()
    {
        if (lives < 3)  
        {
            lives++;
            if (livesText != null)
                livesText.text = "Lives: " + lives;
        }
    }

    public void ToggleInvincibility(bool value)
    {
        isInvincible = value;
        Debug.Log($"Invincibility {(value ? "enabled" : "disabled")}");
    }

}
