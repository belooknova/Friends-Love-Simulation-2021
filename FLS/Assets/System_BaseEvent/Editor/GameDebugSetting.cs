using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// セーブするセッティングデータ
/// </summary>

[Serializable]
[CreateAssetMenu(fileName = "GameDebugSetting", menuName = "Editor/Create_GameDebugSetting")]

public sealed class GameDebugSetting : ScriptableObject
{
    public int valueCount = 513;
    public int textValueCount = 255;
    public float[] dumpValue;
    public string[] dumpText;
    public float[] values;
    public string[] texts;

    public List<bool> foldList = new List<bool>();
    public Dictionary<string, string> memoDict = new Dictionary<string, string>();
    public List<MemorySrot> memorySrots = new List<MemorySrot>();
    //public List<MemorySrot> memorySrots__ = new List<MemorySrot>();

    //public ItemInventryManager manager;
    //public ItemDataBase itemDataBase = null;
    public int tabs_index = 0;
    public int tabs_value_index = 0;
    public int folderCount = 10;
    public int folderCount_dmp = 10;
    public bool memorySwitch = true;
    //public int memorySrotCount = 0;
}



public class Foldout_Editor
{
    public bool foldout = false;
    public Dictionary<string, bool> foldDict = new Dictionary<string, bool>();
}
