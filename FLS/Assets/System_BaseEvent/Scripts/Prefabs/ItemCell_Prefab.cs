using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FLS.Item
{
    public class ItemCell_Prefab : MonoBehaviour
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

        private void Update()
        {
            countText.text = ValuesManager.instance.Get_Value(im.IndexForValue(index)).ToString();
        }

        public void StartUp(ItemManager im, int index)
        {
            this.index = index;
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
                TalkEventManager.instance.EventReservation(im.itemInventryUpdatePath);
            }
        }

        public void DeleteButton()
        {
            var talk = TalkEventManager.instance;
            ValuesManager.instance.Set_Value(410, index);
            ValuesManager.instance.Set_Text(100, nameText.text);

            if (!talk.IsReservation)
            {
                ItemStaticData data = im.Get_ItemData(index);
                if (data.deletePath == "" || data.deletePath == "NONE")
                {
                    TalkEventManager.instance.EventReservation(im.itemDeletePath);
                }
                else
                {
                    TalkEventManager.instance.EventReservation(data.deletePath);
                }
            }
        }
    }
}
