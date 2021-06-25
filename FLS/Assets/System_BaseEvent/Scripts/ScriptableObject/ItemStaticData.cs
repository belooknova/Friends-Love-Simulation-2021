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
    [CreateAssetMenu(menuName = "Item/Create ItemStaticData")]
    public class ItemStaticData : ScriptableObject
    {
        [SerializeField]
        /// <summary> アイテムの名前 </summary>
        public string itemName = "Item";

        [SerializeField]
        /// <summary> アイテムの説明 </summary>
        public string description = "Description";

        [SerializeField]
        /// <summary> アイテムイベントのファイル名 </summary>
        public string eventPath = "";

        /// <summary> アイテムを捨てるイベントのファイル名(設定しない場合は汎用イベント) </summary>
        public string deletePath = "NONE";

        /// <summary> アイテムの価値 </summary>
        public int price = 0;

        /// <summary> アイテム購入禁止条件 </summary>
        public string BunbuyFormale = "False";

    }
}