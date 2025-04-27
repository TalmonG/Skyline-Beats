using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // references
    private GameManager gameManager;
    private AudioManager audioManager;

    private NoteSpawner noteSpawner;
    private GameObject canvasGroup;
    private GameObject levelSelectorCanvas;
    private GameObject howToPlayCanvas;
    private GameObject leaderboardCanvas;
    private GameObject levelCompleteCanvas;
    private GameObject levelFailedCanvas;

    [SerializeField]
    private Slider songPositionSlider;
    private SongManager songManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioManager = FindObjectOfType<AudioManager>();
        noteSpawner = FindObjectOfType<NoteSpawner>();
        songManager = FindObjectOfType<SongManager>();

        // find tags
        //canvasGroup = GameObject.FindGameObjectWithTag("CanvasGroup"); // dont need, delete when done
        levelSelectorCanvas = GameObject.FindGameObjectWithTag("LevelSelectorCanvas");
        howToPlayCanvas = GameObject.FindGameObjectWithTag("HowToPlayCanvas");
        leaderboardCanvas = GameObject.FindGameObjectWithTag("LeaderboardCanvas");
        levelCompleteCanvas = GameObject.FindGameObjectWithTag("LevelCompleteCanvas");
        levelFailedCanvas = GameObject.FindGameObjectWithTag("LevelFailedCanvas");

        // set active
        levelSelectorCanvas.SetActive(true);
        howToPlayCanvas.SetActive(true);
        leaderboardCanvas.SetActive(true);
        levelCompleteCanvas.SetActive(false);
        levelFailedCanvas.SetActive(false);

        // add slider
        if (songPositionSlider != null && songManager != null && songManager.musicSource.clip != null)
        {
            songPositionSlider.minValue = 0f;
            songPositionSlider.maxValue = songManager.musicSource.clip.length;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (songManager != null && songManager.musicSource.clip != null && songPositionSlider != null)
        {
            songPositionSlider.value = songManager.currentSongTime;
        }
    }

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
        //Debug.Log("play pressed");
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
        Debug.Log("[UI MANAGER] Enabling LevelCompleteCanvas");
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
