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
        /// <summary> �A�C�e���̖��O </summary>
        public string itemName = "Item";

        [SerializeField]
        /// <summary> �A�C�e���̐��� </summary>
        public string description = "Description";

        [SerializeField]
        /// <summary> �A�C�e���C�x���g�̃t�@�C���� </summary>
        public string eventPath = "";

        /// <summary> �A�C�e�����̂Ă�C�x���g�̃t�@�C����(�ݒ肵�Ȃ��ꍇ�͔ėp�C�x���g) </summary>
        public string deletePath = "NONE";

        /// <summary> �A�C�e���̉��l </summary>
        public int price = 0;

        /// <summary> �A�C�e���w���֎~���� </summary>
        public string BunbuyFormale = "False";

    }
}