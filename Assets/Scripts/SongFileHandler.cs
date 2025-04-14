using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text;

public static class SongFileHandler // DO NOT TOUCH THIS CODE OR EVERYTHING WILL BREAK
{
    private static readonly string LEVELS_DIRECTORY = "Levels";
    private static readonly string FILE_EXTENSION = ".dat";

    [System.Serializable]
    public class BeatSaberLevelData
    {
        public string _version;
        public CustomData _customData;
        public List<Event> _events;
        public List<Note> _notes;
    }

    [System.Serializable]
    public class CustomData
    {
        public float _time;
        public List<object> _BPMChanges;
        public List<object> _bookmarks;
    }

    [System.Serializable]
    public class Event
    {
        public float _time;
        public int _type;
        public int _value;
    }

    [System.Serializable]
    public class Note
    {
        public float _time;
        public int _lineIndex;
        public int _lineLayer;
        public int _type;
        public int _cutDirection;
    }

    public static void SaveLevel(string levelName, LevelData levelData)
    {
        string directory = Path.Combine(Application.dataPath, LEVELS_DIRECTORY);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string filePath = Path.Combine(directory, levelName + FILE_EXTENSION);
        using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            // Write song name
            writer.Write(levelData.songName);

            // Write number of notes
            writer.Write(levelData.notes.Length);

            // Write each note
            foreach (var note in levelData.notes)
            {
                writer.Write(note.timing);
                writer.Write(note.row);
                writer.Write(note.column);
                writer.Write(note.isRightDrum);
            }
        }

        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif

        Debug.Log($"Saved level to: {filePath}");
    }

    public static LevelData LoadLevel(string levelName)
    {
        string filePath = Path.Combine(Application.dataPath, LEVELS_DIRECTORY, levelName + FILE_EXTENSION);
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Level file not found: {filePath}");
            return null;
        }

        try
        {
            // Read the JSON file
            string jsonContent = File.ReadAllText(filePath);
            Debug.Log($"Loading level file: {filePath} (Size: {new FileInfo(filePath).Length} bytes)");

            // Parse the Beat Saber format
            BeatSaberLevelData beatSaberData = JsonUtility.FromJson<BeatSaberLevelData>(jsonContent);
            
            if (beatSaberData == null)
            {
                Debug.LogError("Failed to parse Beat Saber level data");
                return null;
            }

            // Convert to our format
            LevelData levelData = new LevelData();
            levelData.songName = levelName;

            if (beatSaberData._notes != null)
            {
                List<DrumNoteData> convertedNotes = new List<DrumNoteData>();
                
                // Get the first note time to use as offset
                float timeOffset = beatSaberData._notes.Count > 0 ? beatSaberData._notes[0]._time : 0;
                
                foreach (var note in beatSaberData._notes)
                {
                    // Convert Beat Saber note format to our format
                    // Beat Saber uses 4 lanes (0-3) and 3 layers (0-2)
                    // We use 5 columns (0-4) and 4 rows (0-3)
                    
                    // Map lineIndex (0-3) to our columns (0-4)
                    int column = Mathf.Clamp(note._lineIndex, 0, 4);
                    
                    // Map lineLayer (0-2) to our rows (0-3)
                    int row = Mathf.Clamp(note._lineLayer, 0, 3);
                    
                    // In Beat Saber, type 0 is left hand, type 1 is right hand
                    bool isRightDrum = note._type == 1;
                    
                    // Convert timing from beats to seconds and adjust offset
                    float timing = (note._time - timeOffset) * 0.5f; // Assuming 120 BPM, adjust multiplier if needed
                    
                    DrumNoteData drumNote = new DrumNoteData
                    {
                        timing = timing,
                        row = row,
                        column = column,
                        isRightDrum = isRightDrum
                    };
                    
                    convertedNotes.Add(drumNote);
                }

                levelData.notes = convertedNotes.ToArray();
                Debug.Log($"Converted {levelData.notes.Length} notes from Beat Saber format. First note at {levelData.notes[0].timing}s, last note at {levelData.notes[levelData.notes.Length-1].timing}s");
            }
            else
            {
                Debug.LogWarning("No notes found in Beat Saber level data");
                levelData.notes = new DrumNoteData[0];
            }

            return levelData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading level file: {e.Message}\nStack trace: {e.StackTrace}");
            return null;
        }
    }

    public static string[] GetAllLevelNames()
    {
        string directory = Path.Combine(Application.dataPath, LEVELS_DIRECTORY);
        if (!Directory.Exists(directory))
        {
            return new string[0];
        }

        string[] files = Directory.GetFiles(directory, "*" + FILE_EXTENSION);
        List<string> levelNames = new List<string>();
        
        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            levelNames.Add(fileName);
        }

        return levelNames.ToArray();
    }
} 