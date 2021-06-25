using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using FLS.AudioControll;

namespace FLS.Item
{
    public sealed class ItemShop_List_Button : MonoBehaviour
    {
        [SerializeField]
        private Text text_nameItem;
        [SerializeField]
        private Text text_money;
        [SerializeField]
        private Text text_count;
        [SerializeField]
        private Image Card;
        [SerializeField]
        private Image subCard;

        [SerializeField]
        private Button button;


        public int idItem = 0;

        private ItemShopControll isc;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="id"></param>
        /// <param name="controll"></param>
        public void Init(int id, ItemShopControll controll)
        {
            isc = controll;

            //パラメータ設定
            idItem = id;
            var data = isc.ItemData(id);
            text_nameItem.text = data.itemName;
            text_money.text = (data.price * isc.descount).ToString();

            Update_Item();
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update_Item()
        {
            //購入可能なら光る
            if (isc.CanBuyItem(idItem))
            {
                button.interactable = true;
                text_money.color = Temp.Color.PermitGreen;
            }
            else
            {
                text_money.color = Temp.Color.BanRed;
                button.interactable = false;
            }

            //購入不可条件なら売り切れ表示にする
            if (CheckBan())
            {
                button.interactable = false;
            }
            else
            {
                button.interactable = true;
            }
            int index = ItemManager.instance.IndexForValue(idItem);
            text_count.text = ValuesManager.instance.Get_Value(index).ToString();

        }
        /// <summary>
        /// 禁止条件の確認
        /// </summary>
        /// <returns></returns>
        private bool CheckBan()
        {
            var item = ItemManager.instance.Get_ItemData(idItem);
            return new Parser(ParserType.Bool, item.BunbuyFormale).Eval(ValuesManager.instance.Get_Values());
        }

        /// <summary>
        /// 選択されたとき
        /// </summary>
        public void SelectItem()
        {
            if (!CheckBan())
            {
                AudioManager.instance.SE_Play(Temp.Path_SE.Button_OverMouse, 0.8f, 0.1f);
                isc.Send_Cursor(this);
                ActiveItem();
            }
            else
            {
                isc.Get_Message().Mess(ValuesManager.instance.Get_Text((int)ItemShopControll.MessageIndex.BuyItem));
                AudioManager.instance.SE_Play(Temp.Path_SE.Button_ClickErrer, 0.8f, 0.1f);
            }
        }

        /// <summary>
        /// 選択されていることを示す動作
        /// </summary>
        public void ActiveItem()
        {
            subCard.transform.DOLocalMoveX(-40f, 0.1f);
            Card.transform.DOLocalMoveX(10f, 0.2f);
        }

        /// <summary>
        /// 選択されていることを示す動作を止める
        /// </summary>
        public void NegativeItem()
        {
            subCard.transform.DOLocalMoveX(0f, 0.1f);
            Card.transform.DOLocalMoveX(0f, 0.2f);
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