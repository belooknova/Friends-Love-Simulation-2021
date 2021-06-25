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
        /// ������
        /// </summary>
        /// <param name="id"></param>
        /// <param name="controll"></param>
        public void Init(int id, ItemShopControll controll)
        {
            isc = controll;

            //�p�����[�^�ݒ�
            idItem = id;
            var data = isc.ItemData(id);
            text_nameItem.text = data.itemName;
            text_money.text = (data.price * isc.descount).ToString();

            Update_Item();
        }

        /// <summary>
        /// �X�V
        /// </summary>
        public void Update_Item()
        {
            //�w���\�Ȃ����
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

            //�w���s�����Ȃ甄��؂�\���ɂ���
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
        /// �֎~�����̊m�F
        /// </summary>
        /// <returns></returns>
        private bool CheckBan()
        {
            var item = ItemManager.instance.Get_ItemData(idItem);
            return new Parser(ParserType.Bool, item.BunbuyFormale).Eval(ValuesManager.instance.Get_Values());
        }

        /// <summary>
        /// �I�����ꂽ�Ƃ�
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
        /// �I������Ă��邱�Ƃ���������
        /// </summary>
        public void ActiveItem()
        {
            subCard.transform.DOLocalMoveX(-40f, 0.1f);
            Card.transform.DOLocalMoveX(10f, 0.2f);
        }

        /// <summary>
        /// �I������Ă��邱�Ƃ�����������~�߂�
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