using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoteSpawner))]
public class DrumLevelEditor : Editor // note to self: never doing editor again
{
    // variables
    private int selectedRow = -1;
    private int selectedCol = -1;
    private bool isRightDrum = false;
    private string minutesInput = "0";
    private string secondsInput = "0";
    private bool showGridPositions = false;
    private bool showDeathZoneSettings = false;
    private string[] availableSongs;
    private int selectedSongIndex = -1;

    void OnEnable()
    {
        RefreshSongList();
    }

    // flipping resets the song list
    private void RefreshSongList()
    {
        NoteSpawner spawner = (NoteSpawner)target;
        availableSongs = spawner.GetAvailableSongs();
        selectedSongIndex = System.Array.IndexOf(availableSongs, spawner.levelName);
    }

    // convert time to seconds
    private float ParseTimeInput(string minutes, string seconds)
    {
        float totalSeconds = 0f;
        if (float.TryParse(minutes, out float mins))
        {
            totalSeconds += mins * 60f;
        }
        if (float.TryParse(seconds, out float secs))
        {
            totalSeconds += secs;
        }
        return totalSeconds;
    }

    // convert seconds to minutes
    private void TimeFromSeconds(float totalSeconds, out string minutes, out string seconds)
    {
        int mins = Mathf.FloorToInt(totalSeconds / 60f);
        float secs = totalSeconds % 60f;
        minutes = mins.ToString();
        seconds = secs.ToString("F2");
        //Debug.Log("bing");
    }

    // omfg this took way too long but working
    public override void OnInspectorGUI()
    {
        //Debug.Log("start");
        NoteSpawner spawner = (NoteSpawner)target;
        
        // song selection
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Song Selection", EditorStyles.boldLabel);
        
        if (availableSongs != null && availableSongs.Length > 0)
        {
            int newSelectedIndex = EditorGUILayout.Popup("Select Song", selectedSongIndex, availableSongs);
            if (newSelectedIndex != selectedSongIndex)
            {
                selectedSongIndex = newSelectedIndex;
                spawner.LoadLevel(availableSongs[selectedSongIndex]);
                //Debug.Log("A");
            }
        }

        EditorGUILayout.BeginHorizontal();
        spawner.levelName = EditorGUILayout.TextField("New Song Name", spawner.levelName);
        if (GUILayout.Button("Create/Load", GUILayout.Width(100)))
        {
            spawner.LoadLevel();
            RefreshSongList();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // grid visual toggle
        EditorGUI.BeginChangeCheck();
        bool showGrid = EditorGUILayout.Toggle("Show Grid in Scene", spawner.showGridGizmos);
        if (EditorGUI.EndChangeCheck())
        {
            spawner.showGridGizmos = showGrid;
            SceneView.RepaintAll();
        }

        // death zone visual toggle and settings
        EditorGUI.BeginChangeCheck();
        bool showDeathZone = EditorGUILayout.Toggle("Show Death Zone in Scene", spawner.showDeathZone);
        if (EditorGUI.EndChangeCheck())
        {
            spawner.showDeathZone = showDeathZone;
            SceneView.RepaintAll();
        }

        showDeathZoneSettings = EditorGUILayout.Foldout(showDeathZoneSettings, "Death Zone Settings", true);
        if (showDeathZoneSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            
            Vector3 deathPos = EditorGUILayout.Vector3Field("Position", spawner.deathZonePosition);
            Vector3 deathSize = EditorGUILayout.Vector3Field("Size", spawner.deathZoneSize);
            Color deathColor = EditorGUILayout.ColorField("Color", spawner.deathZoneColor);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spawner, "Modified Death Zone");
                spawner.deathZonePosition = deathPos;
                spawner.deathZoneSize = deathSize;
                spawner.deathZoneColor = deathColor;
                EditorUtility.SetDirty(spawner);
                SceneView.RepaintAll();
            }
            EditorGUI.indentLevel--;
        }

        // grid positions editor
        showGridPositions = EditorGUILayout.Foldout(showGridPositions, "Grid Positions", true);
        if (showGridPositions)
        {
            EditorGUI.indentLevel++;
            
            for (int row = 0; row < 4; row++)
            {
                if (spawner.gridRows[row] == null)
                    spawner.gridRows[row] = new NoteSpawner.GridRow();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Row {row}", EditorStyles.boldLabel);
                
                for (int col = 0; col < 5; col++)
                {
                    if (spawner.gridRows[row].columns[col] == null)
                        spawner.gridRows[row].columns[col] = new NoteSpawner.GridPosition(Vector3.zero, Vector3.zero);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"Column {col}", EditorStyles.boldLabel);
                    
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPos = EditorGUILayout.Vector3Field("Position", spawner.gridRows[row].columns[col].position);
                    Vector3 newRot = EditorGUILayout.Vector3Field("Rotation", spawner.gridRows[row].columns[col].rotation);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(spawner, "Modified Grid Position");
                        spawner.gridRows[row].columns[col].position = newPos;
                        spawner.gridRows[row].columns[col].rotation = newRot;
                        EditorUtility.SetDirty(spawner);
                        SceneView.RepaintAll();
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            if (GUILayout.Button("Reset Grid to Default"))
            {
                if (EditorUtility.DisplayDialog("Reset Grid", 
                    "Are you sure you want to reset all grid positions to default?", "Yes", "No"))
                {
                    Undo.RecordObject(spawner, "Reset Grid");
                    spawner.gridRows = new NoteSpawner.GridRow[4];
                    spawner.InitializeGridPositions();
                    EditorUtility.SetDirty(spawner);
                    SceneView.RepaintAll();
                }
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // draw default inspector excluding grid
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "gridRows");
        serializedObject.ApplyModifiedProperties();

        SongManager songManager = FindObjectOfType<SongManager>();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Note Placement", EditorStyles.boldLabel);

        // grid selection
        EditorGUILayout.LabelField("Grid Selection (Green = Selected)");
        for (int row = 3; row >= 0; row--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int col = 0; col < 5; col++)
            {
                GUI.backgroundColor = (row == selectedRow && col == selectedCol) ? Color.green : Color.white;
                if (GUILayout.Button($"[{row},{col}]", GUILayout.Width(50)))
                {
                    selectedRow = row;
                    selectedCol = col;
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(10);
        
        // drum type toggle
        isRightDrum = EditorGUILayout.Toggle("Right Drum", isRightDrum);

        // note timing input (right after drum type)
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Time (mm:ss)", GUILayout.Width(100));
        minutesInput = EditorGUILayout.TextField(minutesInput, GUILayout.Width(30));
        EditorGUILayout.LabelField(":", GUILayout.Width(10));
        secondsInput = EditorGUILayout.TextField(secondsInput, GUILayout.Width(50));
        float totalSeconds = ParseTimeInput(minutesInput, secondsInput);
        EditorGUILayout.LabelField($"({totalSeconds:F2} seconds)", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // add note button
        GUI.enabled = selectedRow != -1 && selectedCol != -1;
        if (GUILayout.Button("Add Note"))
        {
            spawner.AddNote(totalSeconds, selectedRow, selectedCol, isRightDrum);
        }
        GUI.enabled = true;

        EditorGUILayout.Space(10);

        // display current notes
        EditorGUILayout.LabelField("Current Notes", EditorStyles.boldLabel);
        foreach (var note in spawner.noteDataList)
        {
            TimeFromSeconds(note.timing, out string mins, out string secs);
            EditorGUILayout.LabelField(
                $"Time: {mins}:{secs}, Row: {note.row}, Col: {note.column}, " +
                $"Drum: {(note.isRightDrum ? "Right" : "Left")}");
        }

        if (songManager != null && songManager.musicSource.clip != null)
        {
            EditorGUILayout.LabelField($"Song Duration: {songManager.musicSource.clip.length}s");
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField($"Current Time: {songManager.GetCurrentSongTime()}s");
            }
        }

        if (Application.isPlaying)
        {
            Repaint();
        }

        // add save button at bottom
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Save Level"))
        {
            spawner.SaveLevel();
            RefreshSongList();
        }
    }
} 