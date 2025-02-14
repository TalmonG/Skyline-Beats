using UnityEngine;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    [System.Serializable]
    public class GridPosition
    {
        public Vector3 position;
        public Vector3 rotation;

        public GridPosition(Vector3 pos, Vector3 rot)
        {
            position = pos;
            rotation = rot;
        }
    }

    [System.Serializable]
    public class GridRow
    {
        public GridPosition[] columns = new GridPosition[5];
    }

    public GridRow[] gridRows = new GridRow[4];
    public GameObject leftDrumPrefab;
    public GameObject rightDrumPrefab;
    public float spawnDistance = 20f;
    public string levelName = "DefaultLevel";
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
    public float noteSpawnOffset = 2f; // How many seconds before the beat to spawn the note
    public float noteSpawnDistance = 30f; // How far up in Z axis to spawn the note

    private GameManager gameManager;

    public bool randomBoolOne = false;
    
    void OnEnable()
    {
        InitializeGridPositions();
    }

    void Start()
    {
        songManager = FindObjectOfType<SongManager>();
        gameManager = FindObjectOfType<GameManager>();
        //LoadLevel();
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

        if (needsInitialization)
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

    void Update()
    {
        if (gameManager.shouldGameContinue)
        {
            if (songManager != null && noteDataList.Count > 0)
            {
                float currentTime = songManager.GetCurrentSongTime();
            
                // Spawn notes ahead of time using noteSpawnOffset
                while (noteDataList.Count > 0 && currentTime >= noteDataList[0].timing - noteSpawnOffset)
                {
                    var noteData = noteDataList[0];
                    SpawnNote(noteData.row, noteData.column, noteData.isRightDrum);
                    noteDataList.RemoveAt(0);
                }
            }
            
            // Check if all notes have been spawned and processed
            if (noteDataList.Count == 0 && FindObjectsOfType<DrumNote>().Length == 0 && randomBoolOne == false)
            {
                gameManager.LevelComplete();
                randomBoolOne = true;
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

        // Use the grid's rotation directly
        GameObject noteObj = Instantiate(prefab, spawnPos, gridRotation);
        DrumNote note = noteObj.AddComponent<DrumNote>();
        note.isRightDrum = isRightDrum;
        note.targetPosition = targetPos;
        
        // Calculate speed based on distance and time to reach grid
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
        
        // Auto-save when note is added
        SaveLevel();
    }

    public void SaveLevel()
    {
        LevelData levelData = new LevelData
        {
            songName = levelName,
            notes = noteDataList.ToArray()
        };

        SongFileHandler.SaveLevel(levelName, levelData);
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
            noteDataList = new List<DrumNoteData>(levelData.notes);
            noteDataList.Sort((a, b) => a.timing.CompareTo(b.timing));
            Debug.Log($"Loaded {noteDataList.Count} notes for song: {levelName}");
        }
        else
        {
            noteDataList.Clear();
            Debug.Log($"No existing data found for song: {levelName}");
        }
    }

    // Get all available songs
    public string[] GetAvailableSongs()
    {
        return SongFileHandler.GetAllLevelNames();
    }

    public void ClearNotes()
    {
        noteDataList.Clear();
        SaveLevel();
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
                        
                        // Draw the grid cube
                        Gizmos.color = Color.green;
                        Gizmos.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
                        Gizmos.DrawWireCube(Vector3.zero, Vector3.one * gizmoSize);
                        
                        // Draw simple forward line
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(Vector3.zero, Vector3.forward * gizmoSize);
                    }
                }
            }
            
            // Reset Gizmos matrix
            Gizmos.matrix = Matrix4x4.identity;
        }

        if (showDeathZone)
        {
            Gizmos.color = deathZoneColor;
            Gizmos.DrawWireCube(deathZonePosition, deathZoneSize);
        }

        if (lastAddedNote != null && 
            lastAddedNote.row >= 0 && lastAddedNote.row < 4 && 
            lastAddedNote.column >= 0 && lastAddedNote.column < 5)
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
    }
}