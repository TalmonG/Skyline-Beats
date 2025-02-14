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


    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
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
}