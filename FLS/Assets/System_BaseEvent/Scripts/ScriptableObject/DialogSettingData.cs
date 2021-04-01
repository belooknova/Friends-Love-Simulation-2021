using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

//[Serializable]
[CreateAssetMenu(menuName = "Scriptable/Create DialogSetingData")]
public class DialogSettingData : ScriptableObject
{

    [SerializeField]
    public List<Dialog_Prefab> prefabs = new List<Dialog_Prefab>();

}
