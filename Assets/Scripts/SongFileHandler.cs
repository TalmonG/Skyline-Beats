using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SongFileHandler // DO NOT TOUCH THIS CODE OR EVERYTHING WILL BREAK
{
    private static readonly string LEVELS_DIRECTORY = "Levels";
    private static readonly string FILE_EXTENSION = ".json";

    public static void SaveLevel(string levelName, LevelData levelData)
    {
        string directory = Path.Combine(Application.dataPath, LEVELS_DIRECTORY);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonUtility.ToJson(levelData, true); 
        string filePath = Path.Combine(directory, levelName + FILE_EXTENSION);
        File.WriteAllText(filePath, json);

        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif

        Debug.Log($"Saved level to: {filePath}");
    }

    public static LevelData LoadLevel(string levelName)
    {
        string filePath = Path.Combine(Application.dataPath, LEVELS_DIRECTORY, levelName + FILE_EXTENSION);
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<LevelData>(json);
        }
        return null;
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