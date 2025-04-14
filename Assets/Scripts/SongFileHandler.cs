using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text;
using System.Text.RegularExpressions;

public static class SongFileHandler // DO NOT TOUCH THIS CODE OR EVERYTHING WILL BREAK
{
    private static readonly string LEVELS_DIRECTORY = "Levels";
    private static readonly string FILE_EXTENSION = ".dat";
    private static readonly float BPM = 171f; // Beat Saber song BPM
    private static readonly float SECONDS_PER_BEAT = 60f / BPM; // Convert beats to seconds

    // Beat Saber note types - we only care about actual notes
    private const int NOTE_TYPE_RED = 1;    // Right hand
    private const int NOTE_TYPE_BLUE = 0;   // Left hand

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
        public float _time;  // Song length in beats
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
        public float _time;          // Raw time value from file
        public int _lineIndex;       // Lane position (0-3)
        public int _lineLayer;       // Layer height (0-2)
        public int _type;           // 0 = red (right), 1 = blue (left)
        public int _cutDirection;    // 0-7 for cut directions
    }

    public static void SaveLevel(string levelName, LevelData levelData)
    {
        string filePath = Path.Combine(Application.dataPath, LEVELS_DIRECTORY, levelName + FILE_EXTENSION);
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Level file not found: {filePath}");
            return;
        }

        try
        {
            Debug.Log("Starting to save level...");
            
            // Read the original file content
            string fileContent = File.ReadAllText(filePath);
            
            // Find the notes section
            int notesStartIndex = fileContent.IndexOf("\"_notes\":[");
            if (notesStartIndex == -1)
            {
                Debug.LogError("Could not find notes section in file");
                return;
            }
            Debug.Log($"Found notes section at character {notesStartIndex}");

            // Find the end of the notes array
            int notesEndIndex = fileContent.IndexOf("],\"_obstacles\"", notesStartIndex);
            if (notesEndIndex == -1)
            {
                notesEndIndex = fileContent.IndexOf("],\"_sliders\"", notesStartIndex);
            }
            if (notesEndIndex == -1)
            {
                notesEndIndex = fileContent.IndexOf("],\"_customData\"", notesStartIndex);
            }
            if (notesEndIndex == -1)
            {
                Debug.LogError("Could not find end of notes section");
                return;
            }
            Debug.Log($"Notes section ends at character {notesEndIndex}");

            // Convert our notes to Beat Saber format
            List<string> beatSaberNotes = new List<string>();
            foreach (var note in levelData.notes)
            {
                // Convert timing from seconds back to beats
                float beatTime = note.timing / SECONDS_PER_BEAT;
                
                // Convert our format back to Beat Saber format
                string beatSaberNote = $"{{\"_time\":{beatTime:F2},\"_lineIndex\":{note.column},\"_lineLayer\":{note.row},\"_type\":{(note.isRightDrum ? 0 : 1)},\"_cutDirection\":1}}";
                beatSaberNotes.Add(beatSaberNote);
            }

            // Create the new notes section
            string newNotesSection = "\"_notes\":[" + string.Join(",", beatSaberNotes) + "]";
            
            // Replace the old notes section with the new one
            string newContent = fileContent.Substring(0, notesStartIndex) + 
                              newNotesSection + 
                              fileContent.Substring(notesEndIndex + 1);

            // Write back to the file
            File.WriteAllText(filePath, newContent);
            Debug.Log($"Successfully saved {levelData.notes.Length} notes back to the file!");

            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            #endif
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving level file: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }

    public static LevelData LoadLevel(string levelName)
    {
        string filePath = Path.Combine(Application.dataPath, LEVELS_DIRECTORY, levelName + FILE_EXTENSION);
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Level file not found: {filePath}");
            return null;
        }

        try
        {
            Debug.Log("Starting to load level file...");
            
            // Read the file content
            string fileContent = File.ReadAllText(filePath);
            Debug.Log("File content loaded successfully");

            // Find the notes section immediately
            int notesStartIndex = fileContent.IndexOf("\"_notes\":[");
            if (notesStartIndex != -1)
            {
                Debug.Log("=== FOUND NOTES SECTION! ===");
                Debug.Log($"Notes section starts at character {notesStartIndex}");
            }
            else
            {
                Debug.LogError("Could not find notes section in file");
                return null;
            }

            // Find the end of the notes array
            int notesEndIndex = fileContent.IndexOf("],\"_obstacles\"", notesStartIndex);
            if (notesEndIndex == -1)
            {
                notesEndIndex = fileContent.IndexOf("],\"_sliders\"", notesStartIndex);
            }
            if (notesEndIndex == -1)
            {
                notesEndIndex = fileContent.IndexOf("],\"_customData\"", notesStartIndex);
            }
            if (notesEndIndex == -1)
            {
                Debug.LogError("Could not find end of notes section");
                return null;
            }
            Debug.Log($"Notes section ends at character {notesEndIndex}");

            // Extract just the notes JSON array
            string notesJson = fileContent.Substring(notesStartIndex + 9, notesEndIndex - (notesStartIndex + 9));
            notesJson = "{\"_notes\":" + notesJson + "]}";
            
            // Print the first 100 characters of the notes section
            string preview = notesJson.Length > 100 ? notesJson.Substring(0, 100) + "..." : notesJson;
            Debug.Log($"First part of notes section:\n{preview}");

            // Parse just the notes
            BeatSaberLevelData beatSaberData = JsonUtility.FromJson<BeatSaberLevelData>(notesJson);
            
            if (beatSaberData == null || beatSaberData._notes == null)
            {
                Debug.LogError("Failed to parse Beat Saber notes data");
                return null;
            }

            Debug.Log($"Successfully parsed {beatSaberData._notes.Count} notes!");

            // Get song length from custom data
            float songLengthInBeats = 0f;
            if (beatSaberData._customData != null)
            {
                songLengthInBeats = beatSaberData._customData._time;
                Debug.Log($"Song length in beats: {songLengthInBeats}, which is approximately {songLengthInBeats * SECONDS_PER_BEAT:F2} seconds");
            }

            // Convert to our format
            LevelData levelData = new LevelData();
            levelData.songName = levelName;
            levelData.songLength = songLengthInBeats * SECONDS_PER_BEAT; // Store song length in seconds

            List<DrumNoteData> convertedNotes = new List<DrumNoteData>();
            
            // Sort notes by time to ensure proper order
            beatSaberData._notes.Sort((a, b) => a._time.CompareTo(b._time));
            
            foreach (var note in beatSaberData._notes)
            {
                // Only process actual notes (red and blue)
                if (note._type != NOTE_TYPE_RED && note._type != NOTE_TYPE_BLUE)
                {
                    continue;
                }
                
                // Convert Beat Saber note format to our format
                // Beat Saber uses 4 lanes (0-3) and 3 layers (0-2)
                // We use 5 columns (0-4) and 4 rows (0-3)
                
                // Map lineIndex (0-3) to our columns (0-4)
                int column = Mathf.Clamp(note._lineIndex, 0, 4);
                
                // Map lineLayer (0-2) to our rows (0-3)
                int row = Mathf.Clamp(note._lineLayer, 0, 3);
                
                // In Beat Saber, type 0 is right hand (red), type 1 is left hand (blue)
                bool isRightDrum = note._type == NOTE_TYPE_RED;
                
                // Convert timing from beats to seconds using BPM
                float timing = note._time * SECONDS_PER_BEAT;
                
                // Convert cut direction to rotation
                Vector3 rotation = Vector3.zero;
                switch (note._cutDirection)
                {
                    case 0: // Up
                        rotation = new Vector3(0, 0, 0);
                        break;
                    case 1: // Down
                        rotation = new Vector3(180, 0, 0);
                        break;
                    case 2: // Left
                        rotation = new Vector3(0, 0, 90);
                        break;
                    case 3: // Right
                        rotation = new Vector3(0, 0, -90);
                        break;
                    case 4: // UpLeft
                        rotation = new Vector3(0, 0, 45);
                        break;
                    case 5: // UpRight
                        rotation = new Vector3(0, 0, -45);
                        break;
                    case 6: // DownLeft
                        rotation = new Vector3(180, 0, 90);
                        break;
                    case 7: // DownRight
                        rotation = new Vector3(180, 0, -90);
                        break;
                }
                
                DrumNoteData drumNote = new DrumNoteData
                {
                    timing = timing,
                    row = row,
                    column = column,
                    isRightDrum = isRightDrum,
                    rotation = rotation
                };
                
                convertedNotes.Add(drumNote);
            }

            levelData.notes = convertedNotes.ToArray();
            Debug.Log($"Converted {levelData.notes.Length} notes from Beat Saber format. First note at {levelData.notes[0].timing:F2}s, last note at {levelData.notes[levelData.notes.Length-1].timing:F2}s");

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