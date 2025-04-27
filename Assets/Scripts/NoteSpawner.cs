using UnityEngine;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    [System.Serializable]
    public class GridPosition // this works dont touch
    {
        public Vector3 position;
        public Vector3 rotation;

        public GridPosition(Vector3 pos, Vector3 rot) // this works dont touch
        {
            position = pos;
            rotation = rot;
        }
    }

    [System.Serializable]
    public class GridRow // this works dont touch
    {
        public GridPosition[] columns = new GridPosition[5];
    }

    public GridRow[] gridRows = new GridRow[4];
    public GameObject leftDrumPrefab;
    public GameObject rightDrumPrefab;
    public float spawnDistance = 20f;
    public string levelName = "DefaultLevel";
    private List<DrumNoteData> originalNoteList = new List<DrumNoteData>();
    public List<DrumNoteData> noteDataList = new List<DrumNoteData>();
    
    public bool showGridGizmos = false;
    public bool showDeathZone = false;
    public Vector3 deathZonePosition = new Vector3(0f, 0f, 2f);
    public Vector3 deathZoneSize = new Vector3(5f, 4f, 0.5f);
    public Color deathZoneColor = Color.red;
    public float gizmoSize = 0.3f;
    public Color leftDrumGizmoColor = Color.blue;
    public Color rightDrumGizmoColor = Color.red;
    
    [HideInInspector]
    public DrumNoteData lastAddedNote;

    private SongManager songManager;

    [Header("Note Timing Settings")]
    public float noteSpawnOffset = 2f; 
    public float noteSpawnDistance = 30f;
    public float songDelay = 0f; // Delay in seconds to offset note spawning
    public float noteSpawningDelay = 0f; // Additional delay for note spawning timing

    private GameManager gameManager;

    public bool randomBoolOne = false;
    
    private int lastLoggedTime = -1;
    private float lastBeatTime = -1f;
    private float beatInterval = 60f / 171f; // 171 BPM = 60/171 seconds per beat

    void OnEnable()
    {
        InitializeGridPositions();
    }

    void Start()
    {
        songManager = FindObjectOfType<SongManager>();
        gameManager = FindObjectOfType<GameManager>();
        Debug.Log("NoteSpawner Start: Loading level...");
        LoadLevel(); // Load level immediately on start
    }

    void Update()
    {
        
        if (songManager == null || noteDataList == null || noteDataList.Count == 0)
        {
            return;
        }

        float currentTime = songManager.GetCurrentSongTime();
        if (currentTime == float.MinValue) return; // Song not playing
        
        // Apply song delay to current time
        float adjustedTime = currentTime - songDelay;
        
        // Log current time every 5 seconds for debugging
        int currentSecond = Mathf.FloorToInt(adjustedTime);
        if (currentSecond % 5 == 0 && currentSecond != lastLoggedTime)
        {
            lastLoggedTime = currentSecond;
            Debug.Log($"Current song time: {adjustedTime:F2}s (with {songDelay:F2}s delay), Next note at: {noteDataList[0].timing:F2}s");
            Debug.Log($"Notes remaining: {noteDataList.Count}, Song length: {songManager.musicSource.clip.length:F2}s");
        }

        // Print beat markers (every 1/4 beat)
        float currentBeat = adjustedTime / beatInterval;
        float quarterBeat = Mathf.Floor(currentBeat * 4) / 4;
        
        if (quarterBeat > lastBeatTime)
        {
            lastBeatTime = quarterBeat;
            Debug.Log($"[BEAT] Time: {adjustedTime:F2}s, Beat: {quarterBeat:F2}");
        }

        // Spawn notes ahead of time based on current song time, including the note spawning delay
        while (noteDataList.Count > 0 && noteDataList[0].timing <= adjustedTime + noteSpawnOffset + noteSpawningDelay)
        {
            var noteData = noteDataList[0];
            Debug.Log($"[NOTE SPAWN] Time: {adjustedTime:F2}s, Note timing: {noteData.timing:F2}s, Row: {noteData.row}, Col: {noteData.column}, Right: {noteData.isRightDrum}");
            SpawnNote(noteData.row, noteData.column, noteData.isRightDrum);
            noteDataList.RemoveAt(0);
        }

        // Check if all notes have been spawned and processed
        if (noteDataList.Count == 0 && (adjustedTime >= songManager.musicSource.clip.length - 5f || !songManager.musicSource.isPlaying))
        {
            Debug.Log($"[LEVEL COMPLETE] Notes remaining: {noteDataList.Count}, Song time: {adjustedTime:F2}s, Song length: {songManager.musicSource.clip.length:F2}s, Is playing: {songManager.musicSource.isPlaying}");
            gameManager.LevelComplete();
        }
        
        // Safety check - if we're very close to the end of the song (last 1 second), force level completion
        if (adjustedTime >= songManager.musicSource.clip.length - 1f)
        {
            Debug.Log($"[FORCE LEVEL COMPLETE] Very close to end of song. Notes remaining: {noteDataList.Count}, Song time: {adjustedTime:F2}s, Song length: {songManager.musicSource.clip.length:F2}s");
            gameManager.LevelComplete();
        }
    }

    public void InitializeGridPositions()
    {
        bool needsInitialization = false;
        
        for (int row = 0; row < 4; row++)
        {
            if (gridRows[row] == null)
            {
                needsInitialization = true;
                break;
            }
            
            for (int col = 0; col < 5; col++)
            {
                if (gridRows[row].columns[col] == null)
                {
                    needsInitialization = true;
                    break;
                }
            }
        }

        if (needsInitialization) // good
        {
            float startX = -2f; 
            float startY = 1f;
            float spacingX = 1f;
            float spacingY = 1f;

            for (int row = 0; row < 4; row++)
            {
                if (gridRows[row] == null)
                    gridRows[row] = new GridRow();
                
                for (int col = 0; col < 5; col++)
                {
                    if (gridRows[row].columns[col] == null)
                    {
                        Vector3 position = new Vector3(startX + (col * spacingX), startY + (row * spacingY), 0);
                        gridRows[row].columns[col] = new GridPosition(position, Vector3.zero);
                    }
                }
            }
        }
    }

    public void SpawnNote(int row, int col, bool isRightDrum)
    {
        if (row < 0 || row >= 4 || col < 0 || col >= 5 || gridRows[row]?.columns[col] == null)
        {
            Debug.LogError($"Invalid grid position: row {row}, col {col}");
            return;
        }

        GameObject prefab = isRightDrum ? rightDrumPrefab : leftDrumPrefab;
        if (prefab == null)
        {
            Debug.LogError("Drum prefab is not assigned!");
            return;
        }

        Vector3 gridPosition = gridRows[row].columns[col].position;
        Quaternion gridRotation = Quaternion.Euler(gridRows[row].columns[col].rotation);
        
        Vector3 spawnPos = gridPosition;
        spawnPos.z = noteSpawnDistance;
        
        Vector3 targetPos = gridPosition;
        targetPos.z = deathZonePosition.z - spawnDistance;

        // Get the note data for rotation
        var noteData = noteDataList[0];
        Quaternion noteRotation = Quaternion.Euler(noteData.rotation);

        // Combine grid rotation with note rotation
        Quaternion finalRotation = gridRotation * noteRotation;

        // use combined rotation
        GameObject noteObj = Instantiate(prefab, spawnPos, finalRotation);
        DrumNote note = noteObj.AddComponent<DrumNote>();
        note.isRightDrum = isRightDrum;
        note.targetPosition = targetPos;
        
        // calculate speed based on distance and time to reach grid (probably works)
        float distanceToGrid = noteSpawnDistance;
        note.speed = distanceToGrid / noteSpawnOffset;
    }

    public void AddNote(float timing, int row, int col, bool isRightDrum)
    {
        DrumNoteData newNote = new DrumNoteData
        {
            timing = timing,
            row = row,
            column = col,
            isRightDrum = isRightDrum
        };

        noteDataList.Add(newNote);
        noteDataList.Sort((a, b) => a.timing.CompareTo(b.timing));
        lastAddedNote = newNote;
        
        // auto-save when note added
        SaveLevel();
        //Debug.Log("done");
    }

    public void SaveLevel()
    {
        LevelData levelData = new LevelData
        {
            songName = levelName,
            notes = noteDataList.ToArray()
        };

        SongFileHandler.SaveLevel(levelName, levelData);
        //Debug.Log("done");
    }

    public void LoadLevel(string songName = null)
    {
        if (songName != null)
        {
            levelName = songName;
        }

        LevelData levelData = SongFileHandler.LoadLevel(levelName);
        if (levelData != null)
        {
            // store in original list and sort
            originalNoteList = new List<DrumNoteData>(levelData.notes);
            originalNoteList.Sort((a, b) => a.timing.CompareTo(b.timing));
            
            // initialize spawn list
            ResetNoteSpawnList();
            
            Debug.Log($"Loaded {originalNoteList.Count} notes for song: {levelName}");

            // Save the level back to the file to ensure it's in the correct format
            SaveLevel();
        }
        else
        {
            originalNoteList.Clear();
            noteDataList.Clear();
            Debug.Log($"No existing data found for song: {levelName}");
        }
    }

    public void ResetNoteSpawnList()
    {
        // create new spawn list from original data
        noteDataList = new List<DrumNoteData>(originalNoteList);
    }

    // get available songs useless for now, needs more songs
    public string[] GetAvailableSongs()
    {
        return SongFileHandler.GetAllLevelNames();
        //Debug.Log("got");
    }

    public void ClearNotes()
    {
        noteDataList.Clear();
        SaveLevel();
        //Debug.Log("cleared");
    }

    private void OnDrawGizmos()
    {
        if (gridRows[0]?.columns[0] == null)
        {
            InitializeGridPositions();
        }

        if (showGridGizmos)
        {
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    if (gridRows[row]?.columns[col] != null)
                    {
                        Vector3 pos = gridRows[row].columns[col].position;
                        Quaternion rot = Quaternion.Euler(gridRows[row].columns[col].rotation);
                        
                        // draw grid cube
                        Gizmos.color = Color.green;
                        Gizmos.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
                        Gizmos.DrawWireCube(Vector3.zero, Vector3.one * gizmoSize);
                        
                        // draw forward line
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(Vector3.zero, Vector3.forward * gizmoSize);
                    }
                }
            }
            
            // reset gizmos matrix
            Gizmos.matrix = Matrix4x4.identity;
        }

        if (showDeathZone)
        {
            Gizmos.color = deathZoneColor;
            Gizmos.DrawWireCube(deathZonePosition, deathZoneSize);
        }

        if (lastAddedNote != null && lastAddedNote.row >= 0 && lastAddedNote.row < 4 && lastAddedNote.column >= 0 && lastAddedNote.column < 5)
        {
            Vector3 gridPos = gridRows[lastAddedNote.row].columns[lastAddedNote.column].position;
            
            Vector3 spawnPos = gridPos;
            spawnPos.z = noteSpawnDistance;
            
            Vector3 targetPos = gridPos;
            targetPos.z = deathZonePosition.z - spawnDistance;
            
            Gizmos.color = lastAddedNote.isRightDrum ? rightDrumGizmoColor : leftDrumGizmoColor;
            
            Gizmos.DrawWireSphere(spawnPos, gizmoSize);
            Gizmos.DrawWireSphere(targetPos, gizmoSize);
            Gizmos.DrawLine(spawnPos, targetPos);
        }
        //Debug.Log("done gizzmosss");
    }
}