using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private GameManager gameManager;
    private AudioManager audioManager;

    private NoteSpawner noteSpawner;
    private GameObject canvasGroup;
    private GameObject levelSelectorCanvas;
    private GameObject howToPlayCanvas;
    private GameObject leaderboardCanvas;
    private GameObject levelCompleteCanvas;
    private GameObject levelFailedCanvas;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioManager = FindObjectOfType<AudioManager>();
        noteSpawner = FindObjectOfType<NoteSpawner>();

        // Find with tags
        //canvasGroup = GameObject.FindGameObjectWithTag("CanvasGroup"); // could use but i like controlling all canvas seperately
        levelSelectorCanvas = GameObject.FindGameObjectWithTag("LevelSelectorCanvas");
        howToPlayCanvas = GameObject.FindGameObjectWithTag("HowToPlayCanvas");
        leaderboardCanvas = GameObject.FindGameObjectWithTag("LeaderboardCanvas");
        levelCompleteCanvas = GameObject.FindGameObjectWithTag("LevelCompleteCanvas");
        levelFailedCanvas = GameObject.FindGameObjectWithTag("LevelFailedCanvas");

        levelSelectorCanvas.SetActive(true);
        howToPlayCanvas.SetActive(true);
        leaderboardCanvas.SetActive(true);
        levelCompleteCanvas.SetActive(false);
        levelFailedCanvas.SetActive(false);
    }

    // Update is called once per frame
    // void Update()
    // {
        
    // }

    public void PlayButton()
    {
        // canvasGroup.SetActive(false);
        levelSelectorCanvas.SetActive(false);
        howToPlayCanvas.SetActive(false);
        leaderboardCanvas.SetActive(false);
        levelCompleteCanvas.SetActive(false);
        
        // Add these lines to reload the level
        if (noteSpawner != null)
        {
            noteSpawner.LoadLevel(noteSpawner.levelName);
        }
        
        noteSpawner.randomBoolOne = false;
        gameManager.StartGame();
    }

    public void EnableLevelSelectorCanvas()
    {
        levelSelectorCanvas.SetActive(true);
    }

    public void EnableHowToPlayCanvas()
    {
        howToPlayCanvas.SetActive(true);
    }

    public void EnableLeaderboardCanvas()
    {
        leaderboardCanvas.SetActive(true);
    }

    public void EnableLevelCompleteCanvas()
    {
        levelCompleteCanvas.SetActive(true);
    }

    public void EnableLevelFailedCanvas()
    {
        levelFailedCanvas.SetActive(true);
        Debug.Log("Level Failed Canvas Enabled");
    }

    public void DisableLevelSelectorCanvas()
    {
        levelSelectorCanvas.SetActive(false);
    } 

    public void DisableHowToPlayCanvas()
    {
        howToPlayCanvas.SetActive(false);
    }
    
    public void DisableLeaderboardCanvas()
    {
        leaderboardCanvas.SetActive(false);
    }

    public void DisableLevelCompleteCanvas()
    {
        levelCompleteCanvas.SetActive(false);
    }

    public void DisableLevelFailedCanvas()
    {
        levelFailedCanvas.SetActive(false);
        Debug.Log("Level Failed Canvas Disabled");
    }

    public void LevelCompleteContinueButton()
    {
        gameManager.ResetGame();
    }
    
}
