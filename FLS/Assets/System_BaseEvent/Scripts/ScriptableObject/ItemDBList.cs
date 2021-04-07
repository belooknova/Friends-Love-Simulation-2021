using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace FLS.Item
{
    //[Serializable]
    [CreateAssetMenu(menuName = "Item/Create ItemDBList")]
    public class ItemDBList : ScriptableObject
    {
        [SerializeField]
        private List<ItemStaticData> itemStaticDatas = new List<ItemStaticData>();


        public List<ItemStaticData> Get_ItemList()
        {
            return itemStaticDatas;
        } 
    }
}
