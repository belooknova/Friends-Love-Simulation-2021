using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "SrotData", menuName = "Editor/Create_Setting_SrotData")]

public class MemorySrot : ScriptableObject
{
    public void Set(float[] v1, string[] v2)
    {
        values = new float[v1.Length];
        texts = new string[v2.Length];
        for (int i=0; i < values.Length; i++)
        {
            values[i] = 0;
        }
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i] = "";
        }

        isSaved = true;
        v1.CopyTo(values,0);
        v2.CopyTo(texts, 0);
    }

    public void Reset()
    {
        isSaved = false;
    }

    public bool isSaved = false;
    public float[] values;
    public string[] texts;
}
