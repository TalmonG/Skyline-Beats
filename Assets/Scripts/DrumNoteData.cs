using UnityEngine;
using System;

[System.Serializable]
public class DrumNoteData
{
    public float timing;
    public int row;
    public int column;
    public bool isRightDrum;
}

[System.Serializable]
public class LevelData
{
    public string songName;
    public DrumNoteData[] notes;
} 