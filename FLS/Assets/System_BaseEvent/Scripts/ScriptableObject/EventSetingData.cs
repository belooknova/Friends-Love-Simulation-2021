using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

//[Serializable]
[CreateAssetMenu(menuName = "Scriptable/Create EventSetingData")]
public class EventSetingData : ScriptableObject
{

    [SerializeField]
    public List<string> eventPaths = new List<string>();

    [SerializeField]
    public List<int> date = new List<int>();

    [SerializeField]
    public List<string> formales = new List<string>();



}

