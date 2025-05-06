using UnityEngine;
using System;

// DO NOT TOUCH THIS CODE OR EVERYTHING WILL BREAK `^`
[System.Serializable]
public class DrumNoteData
{
    public float timing;
    public int row;
    public int column;
    public bool isRightDrum;
    public Vector3 rotation;
}

[System.Serializable]
public class LevelData
{
    public string songName;
    public DrumNoteData[] notes;
    public float songLength; // length of the song in seconds
}