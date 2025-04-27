using UnityEngine;
using System.Collections;
public class SongManager : MonoBehaviour
{
    public AudioSource musicSource;
    public float songStartDelay = 3f;
    public float currentSongTime => musicSource.time;

    private bool isSongPlaying = false;

    private GameManager gameManager;
    private AudioManager audioManager;

    [Header("Time Control")]
    public float skipAmount = 5f;
    public KeyCode skipForwardKey = KeyCode.RightArrow;
    public KeyCode skipBackwardKey = KeyCode.LeftArrow;

    // references
    private NoteSpawner noteSpawner;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        noteSpawner = FindObjectOfType<NoteSpawner>();
    }

    void Update()
    {
        if (!isSongPlaying) return; // cancel if song not playing

        if (Input.GetKeyDown(skipForwardKey)) // skip forward
        {
            SkipForward();
        }
        else if (Input.GetKeyDown(skipBackwardKey)) // skip backward
        {
            SkipBackward();
        }
        
        // Check if song has finished playing
        if (musicSource != null && musicSource.clip != null && !musicSource.isPlaying && isSongPlaying)
        {
            Debug.Log("[SONG MANAGER] Song has finished playing naturally");
            isSongPlaying = false;
            
            // Find the GameManager and call LevelComplete
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                Debug.Log("[SONG MANAGER] Calling LevelComplete from SongManager");
                gameManager.LevelComplete();
            }
        }
    }

    public void ResetSong()
    {
        musicSource.Stop();
        musicSource.time = 0;  
        isSongPlaying = false;
        musicSource.volume = 0.1f; 
    }

    public void StartSong()
    {
        ResetSong();  // just incase
        Invoke(nameof(PlayMusic), songStartDelay); // just gonna have 1 song for now but this can be used for multiple
    }

    private void PlayMusic()
    {
        musicSource.Play();
        isSongPlaying = true;
    }

    public void SuddenStopSong()
    {
        musicSource.Stop();
        isSongPlaying = false;
    }

    public void FadeOutSong()
    {
        StartCoroutine(FadeOutSongIEnumerator());
    }

    // dont use directly, only use FadeOutSong()
    public IEnumerator FadeOutSongIEnumerator()
    {
        // fade song over 2 seconds
        float startVolume = musicSource.volume;
        float elapsedTime = 0f;
        while (elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / 2f);
            yield return null;
        }
        musicSource.Stop();
        audioManager.Play("LevelComplete");
        isSongPlaying = false;
        //Debug.Log("done");
    }

    public float GetCurrentSongTime()
    {
        if (!isSongPlaying)
            return float.MinValue;
        return currentSongTime;
    }

    public void SkipForward()
    {
        float newTime = Mathf.Clamp(musicSource.time + skipAmount, 0f, musicSource.clip.length);
        //Debug.Log($"Skipping forward from {musicSource.time:F2}s to {newTime:F2}s");
        musicSource.time = newTime;
        
        if (noteSpawner != null)
        {
            CleanupAndResetNotes();
        }
        //Debug.Log("SKIPPEDDDDDD");
    }

    public void SkipBackward()
    {
        float newTime = Mathf.Clamp(musicSource.time - skipAmount, 0f, musicSource.clip.length);
        //Debug.Log($"Skipping backward from {musicSource.time:F2}s to {newTime:F2}s");
        musicSource.time = newTime;
        
        if (noteSpawner != null)
        {
            CleanupAndResetNotes();
        }
    }

    private void CleanupAndResetNotes()
    {
        // destroy all notes
        DrumNote[] existingNotes = FindObjectsOfType<DrumNote>();
        foreach (DrumNote note in existingNotes)
        {
            Destroy(note.gameObject);
        }
        
        // reset note spawn list
        noteSpawner.ResetNoteSpawnList();
    }
}