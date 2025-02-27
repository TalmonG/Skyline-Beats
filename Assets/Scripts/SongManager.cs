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
    public float skipAmount = 5f; // Amount of seconds to skip forward/backward
    public KeyCode skipForwardKey = KeyCode.RightArrow;
    public KeyCode skipBackwardKey = KeyCode.LeftArrow;

    // Add reference to NoteSpawner
    private NoteSpawner noteSpawner;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        noteSpawner = FindObjectOfType<NoteSpawner>();
    }

    void Update()
    {
        if (!isSongPlaying) return;

        // Handle time control input
        if (Input.GetKeyDown(skipForwardKey))
        {
            SkipForward();
        }
        else if (Input.GetKeyDown(skipBackwardKey))
        {
            SkipBackward();
        }
    }

    public void ResetSong()
    {
        musicSource.Stop();
        musicSource.time = 0;  // Reset the song position to start
        isSongPlaying = false;
        musicSource.volume = 1f;  // Reset volume in case it was faded out
    }

    public void StartSong()
    {
        ResetSong();  // Add this line to ensure clean state
        Invoke(nameof(PlayMusic), songStartDelay);
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

    // dont use this directly, only use FadeOutSong()
    public IEnumerator FadeOutSongIEnumerator()
    {
        // fade the song out over 2 seconds WITHOUT using DOFade
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
    }

    public float GetCurrentSongTime()
    {
        if (!isSongPlaying)
            return float.MinValue;  // Return a very low value to prevent note spawning before song starts
        return currentSongTime;
    }

    public void SkipForward()
    {
        float newTime = Mathf.Clamp(musicSource.time + skipAmount, 0f, musicSource.clip.length);
        Debug.Log($"Skipping forward from {musicSource.time:F2}s to {newTime:F2}s");
        musicSource.time = newTime;
        
        if (noteSpawner != null)
        {
            CleanupAndResetNotes();
        }
    }

    public void SkipBackward()
    {
        float newTime = Mathf.Clamp(musicSource.time - skipAmount, 0f, musicSource.clip.length);
        Debug.Log($"Skipping backward from {musicSource.time:F2}s to {newTime:F2}s");
        musicSource.time = newTime;
        
        if (noteSpawner != null)
        {
            CleanupAndResetNotes();
        }
    }

    private void CleanupAndResetNotes()
    {
        // Destroy all existing notes
        DrumNote[] existingNotes = FindObjectsOfType<DrumNote>();
        foreach (DrumNote note in existingNotes)
        {
            Destroy(note.gameObject);
        }
        
        // Reset the spawn list
        noteSpawner.ResetNoteSpawnList();
    }
}