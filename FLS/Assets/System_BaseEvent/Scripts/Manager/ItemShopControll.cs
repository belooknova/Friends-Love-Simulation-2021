using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FLS.AudioControll;
using DG.Tweening;

namespace FLS.Item
{
    public sealed class ItemShopControll : MonoBehaviour
    {
        [HideInInspector]
        public bool isActive = false;

        [SerializeField]
        private Transform contents;
        [SerializeField]
        private ItemShop_List_Button button_Prefab;

        private readonly List<ItemShop_List_Button> itemCard_List = new List<ItemShop_List_Button>();
        private ItemShop_List_Button cursor;
        public float descount = 1;


        [SerializeField]
        private ItemShop_Description _description;
        [SerializeField]
        private ItemShop_Message _message;
        [SerializeField]
        Transform itemShop_List;
        [SerializeField]
        GameObject backButton;

        public enum MessageIndex
        {
            InShop = 50,
            BanItemSelected = 51,
            BuyItem = 52,
            NotEnoughManey = 53,
            OutShop = 54
        }


        public void Init_ItemShop(MetaTextParser meta)
        {
            // 'items' �A�C�e���ԍ�
            // 'sale' �{��
            // 'saleD'

            //�A�C�e�����X�g�̐���
            var list = meta.Eval();
            bool no_errer = false;
            List<string> items = new List<string>() ;

            foreach (var lables in list)
            {
                Debug.Log(lables[0]);
                switch (lables[0])
                {
                    case "items":
                        {
                            items = lables;
                        }
                        break;
                    case "sale":
                        float value = new Parser(ParserType.Number, lables[1]).Eval_Value_outFlaot(ValuesManager.instance.Get_Values());
                        descount = value;
                        break;
                    case "saleD":
                        break;
                }
            }

            if (items.Count != 0)
            {
                for (int i = 1; i < items.Count; i++)
                {
                    string lable = items[i];
                    int index = new Parser(ParserType.Number, lable).Eval_Value(ValuesManager.instance.Get_Values());

                    var p = Instantiate(button_Prefab);
                    p.transform.SetParent(contents);
                    p.Init(index, this);

                    itemCard_List.Add(p);
                }

                no_errer = true;
            }

            if (no_errer)
            {
                isActive = true;
                //���X��\��������
                ShopStart();
            }
        }

        private void ShopStart()
        {
            //���X��\��������
            ShowList();
            backButton.SetActive(true);

            //�u��������Ⴂ�܂��v���b�Z�[�W�̕\��
            _message.Mess(ValuesManager.instance.Get_Text((int)MessageIndex.InShop));
            _message.Show();
        }

        private void TryShowDescription()
        {
            if (cursor != null)
            {
                //�����ƍw���{�^����\��������
                //_description.Update_Description(cursor.idItem);

                if (cursor != _description.cullent)
                {
                    //�����ƍw���{�^�����X�V����
                    Update_ItemDescription();
                }
            }
        }

        public void Update_ItemDescription()
        {
            _description.cullent = cursor;
            _description.Update_Description(cursor.idItem);
        }

        public void TryBuyItem()
        {
            if (CanBuyCursorItem())
            {
                //�w������
                BuyItem();
                AudioManager.instance.SE_Play(Temp.Path_SE.BuyItem_Bell, 1f, 0.2f);
                //�������グ���b�Z�[�W
                _message.Mess(ValuesManager.instance.Get_Text((int)MessageIndex.BuyItem));
            }
            else
            {
                //�G���[���b�Z�[�W
                _message.Mess(ValuesManager.instance.Get_Text((int)MessageIndex.NotEnoughManey));
                AudioManager.instance.SE_Play(Temp.Path_SE.Button_ClickErrer, 0.8f, 0.1f);
            }
        }

        private void BuyItem()
        {
            //�������𑝂₷
            int index = ItemManager.instance.IndexForValue(cursor.idItem);
            ValuesManager.instance.IncrementValue(index, 1);

            //���������炷
            int price = PriceItem(cursor.idItem);
            ValuesManager.instance.DecrementValue((int)VariableType.Money, price);

            //���w�����o
            Update_Shop();
        }

        private void Update_Shop()
        {
            Update_ItemDescription();
            foreach (var item in itemCard_List)
            {
                item.Update_Item();
            }
        }

        public bool CanBuyCursorItem()
        {
            return CanBuyItem(cursor.idItem);
        }

        public bool CanBuyItem(int id)
        {
            int price = PriceItem(id);
            return (ValuesManager.instance.Get_Value((int)VariableType.Money) >= price);
        }

        public int PriceItem(int idItem)
        {
            return Mathf.FloorToInt(ItemData(idItem).price * descount);
        }

        public ItemStaticData ItemData(int idItem)
        {
            return ItemManager.instance.Get_ItemData(idItem);
        }

        public void Send_Cursor(ItemShop_List_Button _button)
        {
            if (cursor != null)
            {
                cursor.NegativeItem();
            }
            cursor = _button;
        }

        public void Exit_ItemShop()
        {
            //����ۂ̏���
            StartCoroutine(Exit());

            IEnumerator Exit()
            {
                HideList();
                _description.Hide();
                backButton.SetActive(false);

                //���b�Z�[�W�\��
                _message.Mess(ValuesManager.instance.Get_Text((int)MessageIndex.OutShop));
                //�ҋ@
                yield return new WaitForSeconds(2);

                //��ʂ��\���ɂ���
                _message.Hide();
                isActive = false;
            }
        }

        public ItemShop_Message Get_Message()
        {
            return _message;
        }

        private void ShowList()
        {
            itemShop_List.DOLocalMoveX(290, 0.2f);
        }

        private void HideList()
        {
            itemShop_List.DOLocalMoveX(750, 0.2f);
        }

        
        IEnumerator Start()
        {
            yield return null;
            TalkEventManager.instance.EventReservation("System/sys_varable_setting");
            yield return null;
            //Init_ItemShop(new MetaTextParser("<items:1,2,3,4,5,6,7,8,9><sale:0.5>"));
        }
        

        // Update is called once per frame
        void Update()
        {
            if (isActive)
            {
                TryShowDescription();
            }
        }
    }
}
