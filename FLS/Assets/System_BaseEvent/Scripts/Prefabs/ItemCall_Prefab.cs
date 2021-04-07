using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FLS.Item
{
    public class ItemCall_Prefab : MonoBehaviour
    {
        [SerializeField]
        public int index = 0;
        [SerializeField]
        private Text nameText;
        [SerializeField]
        private Text descriptionText;
        [SerializeField]
        private Text countText;
        [SerializeField]
        private int itemCount = 0;
        private ItemManager im;
        private string eventPath;
        

        public void StartUp(ItemManager im, int index)
        {
            ItemStaticData data = im.Get_ItemData(index);
            nameText.text = data.itemName;
            descriptionText.text = data.description;
            eventPath = data.eventPath;
            this.im = im;
        }

        public void UpdateCount(int itemCount)
        {
            this.itemCount = itemCount;
        }

        public void UseButton()
        {
            var talk = TalkEventManager.instance;
            if (!talk.IsReservation)
            {
                TalkEventManager.instance.EventReservation(eventPath);
            }
        }

        public void DeleteButton()
        {
            var talk = TalkEventManager.instance;
            ValuesManager.instance.Set_Value(410, index);
            ValuesManager.instance.Set_Text(100, nameText.text);

            if (!talk.IsReservation)
            {
                TalkEventManager.instance.EventReservation(im.itemDeletePath);
            }
        }
    }
}
