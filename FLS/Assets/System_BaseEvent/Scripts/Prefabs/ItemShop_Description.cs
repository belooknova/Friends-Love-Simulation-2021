using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace FLS.Item
{

    public sealed class ItemShop_Description : MonoBehaviour
    {
        [SerializeField]
        private Text text_ItemName;
        [SerializeField]
        private Text text_Money;
        [SerializeField]
        private Text text_Description;
        [SerializeField]
        private Text text_Count;
        [SerializeField]
        private ItemShopControll isc;

        public bool isShow = false;
        public int idItem = 0;

        public ItemShop_List_Button cullent;

        public void Update_Description(int id)
        {
            idItem = id;


            //隠れて、情報更新して、表示する
            StartCoroutine(Conformity());
            //ConformityItem();

            return;

            IEnumerator Conformity()
            {
                //Hide();
                //transform.DOLocalMoveY(-580, 0.2f);
                //yield return new WaitForSeconds(0.2f);

                ConformityItem();

                Show();
                yield return new WaitForSeconds(0.2f);
            }

        }

        public void ConformityItem()
        {
            //アイテムの情報を適応する
            var data = ItemManager.instance.Get_ItemData(idItem);
            text_ItemName.text = data.itemName;
            text_Description.text = data.description;
            text_Money.text = data.price.ToString();
            text_Count.text = ValuesManager.instance.Get_Value(ItemManager.instance.IndexForValue(idItem)).ToString();
        }

        public void Show()
        {
            transform.DOLocalMoveY(-180, 0.2f);
        }

        public void Hide()
        {
            transform.DOLocalMoveY(-580, 0.2f);
        }


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
