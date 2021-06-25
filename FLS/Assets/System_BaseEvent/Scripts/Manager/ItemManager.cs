using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace FLS.Item
{

    public class ItemManager : MonoBehaviour
    {
        public static ItemManager instance;

        [SerializeField]
        private int updateIndex = 100;
        [SerializeField]
        private int valueBaseIndex = 130;
        [SerializeField]
        private const int dmpValueCount = 60;
        [SerializeField]
        private ItemDBList itemDBs;
        [SerializeField]
        private ItemCell_Prefab prefab_Item;
        [SerializeField]
        private Transform canves;
        [SerializeField]
        private CanvasGroup canvasBase;
        [SerializeField]
        private Animator anime;
        [SerializeField]
        private ItemShopControll shopControll;

        /// <summary> �C���x���g���𐶐��� </summary>
        private bool isStartUped = false;
        /// <summary> �O��̎擾�A�C�e�� </summary>
        private float[] dmpValues;

        private bool isDisplay = false;

        public string basePath = "ItemFolder/";
        public string itemDeletePath = "/ItemFolder/_system_delete_item";
        public string itemInventryUpdatePath = "/ItemFolder/_system_inventry_update";

        public bool isItemEventing = false;

        private readonly List<ItemCell_Prefab> call_Prefabs = new List<ItemCell_Prefab>();

        private TalkEventManager talk;
        private ValuesManager vm;


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            talk = TalkEventManager.instance;
            vm = ValuesManager.instance;
            SetUp_Order();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                StandInventry();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                ExitInventry();
            }

            if (vm.Get_Value(updateIndex) == 1)
            {
                Update_Indentry();
                vm.Set_Value(updateIndex, 0);
            }
        }

        private void SetUp_Order()
        {
            //�A�C�e���C���x���g���B�\��  => ITEM_SHOW
            talk.Order_Registration("ITEM_SHOW", Order_Show_ItemInv, Exec_Show_ItemInv);
            //�A�C�e���C���x���g���B����  => ITEM_HIDE
            talk.Order_Registration("ITEM_HIDE", Order_Hide_ItemInv, Exec_Hide_ItemInv);
            //�A�C�e���C���x���g���B�X�V  => ITEM_UPDATE
            talk.Order_Registration("ITEM_UPDATE", Order_Update_ItemInv, Exec_Update_ItemInv);
            //�A�C�e���ǉ�                => ITEM_ADD �A�C�e��ID(p) ��(p)
            talk.Order_Registration("ITEM_ADD", (int count, OrderParametor par, string[] arg) => 
            {
                if (arg.Length == 3)
                {
                    //VALUE_
                    StringBuilder sb = new StringBuilder("VALUE_INC \"")
                    .Append(arg[1]).Append(" + ").Append(valueBaseIndex).Append("\" \"")
                    .Append(arg[2]).Append("\"");

                    talk.EventRegistration(sb.ToString());
                    talk.EventRegistration("ITEM_UPDATE");

                    return true;
                }

                return false;
            } , null, Type_TalkEventData.MULTI);
            //�A�C�e������
            talk.Order_Registration("ITEM_REMOVE", (int count, OrderParametor par, string[] arg) =>
            {
                if (arg.Length == 3)
                {
                    //VALUE_
                    StringBuilder sb = new StringBuilder("VALUE_DEC \"")
                    .Append(arg[1]).Append(" + ").Append(valueBaseIndex).Append("\" \"")
                    .Append(arg[2]).Append("\"");
                    Debug.Log(sb.ToString());
                    talk.EventRegistration(sb.ToString());
                    talk.EventRegistration("ITEM_UPDATE");

                    return true;
                }

                return false;
            }, null, Type_TalkEventData.MULTI);

            //�A�C�e���V���b�v�Ăяo��
            talk.Order_Registration("ITEM_CALLSHOP", Order_Call_ItemShop, Exec_Call_ItemShop);

        }

        private bool Order_Show_ItemInv(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 1)
            {
                return true;
            }

            return false;
        }

        private void Exec_Show_ItemInv(ref int count, OrderParametor par)
        {
            StandInventry();
            count++;
        }

        private bool Order_Hide_ItemInv(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 1)
            {
                return true;
            }

            return false;
        }

        private void Exec_Hide_ItemInv(ref int count, OrderParametor par)
        {
            ExitInventry();
            count++;
        }

        private bool Order_Update_ItemInv(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 1)
            {
                return true;
            }

            return false;
        }

        private void Exec_Update_ItemInv(ref int count, OrderParametor par)
        {
            Update_Indentry();
            count++;
        }

        private bool Order_Call_ItemShop(int count, OrderParametor par, string[] arg)
        {
            //ITEM_CALLSHOP ���^�f�[�^(items:n, sale:1, saleD:0)

            if (arg.Length == 2)
            {
                par.parString.Add(arg[1]);
                par.parInt.Add(0);

                return true;
            }
            return false;
        }

        private void Exec_Call_ItemShop(ref int count, OrderParametor par)
        {
            if (par.parInt[0] == 0)
            {
                MetaTextParser meta = new MetaTextParser(par.parString[0]);
                shopControll.Init_ItemShop(meta);
                par.parInt[0] = 1;
            }

            if (!shopControll.isActive)
            {
                count++;
            }
        }

        public void StandInventry()
        {
            if (!isDisplay)
            {
                isDisplay = true;
                if (isStartUped)
                {
                    Update_Indentry();
                }
                else
                {
                    StartUp_Inventry();
                }

                anime.SetBool("isDisplay", true);
            }
        }

        public void ExitInventry()
        {
            if (isDisplay)
            {
                isDisplay = false;
                anime.SetBool("isDisplay", false);
            }
        }

        /// <summary>
        /// ���߂�Inventory���N������
        /// </summary>
        public void StartUp_Inventry()
        {
            isStartUped = true;
            Clear_dmp(dmpValueCount); //������
            //Copy_dmpValue(); //�R�s�[

            Update_Indentry();

        }

        private void Update_Indentry()
        {
            for (int i = 0; i < dmpValueCount; i++)
            {
                var value = vm.Get_Value(IndexForValue(i));
                if (value != dmpValues[i]) //�قȂ�
                {
                    if (value > 0)
                    {
                        var data = Search_ItemCall(i);
                        if (data == null)
                        {
                            Set_ItemCell(i); //����
                        }
                        else
                        {
                            data.UpdateCount(value); //����
                        }
                    }
                    else
                    {
                        Remove_ItemCell(i); //�폜

                    }
                }

                dmpValues[i] = value;
            }
        }

        private ItemCell_Prefab Search_ItemCall(int itemIndex)
        {
            foreach(var db in call_Prefabs)
            {
                if (db.index == itemIndex)
                {
                    return db;
                }
            }

            return null;
        }

        private void Remove_ItemCell(int index)
        {
            var data = Search_ItemCall(index);
            call_Prefabs.Remove(data);
            Destroy(data.gameObject);
        }

        private void Set_ItemCell(int index)
        {
            ItemCell_Prefab cell = Instantiate(prefab_Item).GetComponent<ItemCell_Prefab>();
            cell.transform.SetParent(canves);
            cell.StartUp(this, index);
            call_Prefabs.Add(cell);
        }

        /// <summary>
        /// �_���v�z�������������
        /// </summary>
        /// <param name="number"></param>
        private void Clear_dmp(int number)
        {
            dmpValues = new float[number];
            for (int i = 0; i < dmpValues.Length; i++)
            {
                dmpValues[i] = 0;
            }
        }


        public void Use_Item(int itemIndex)
        {
            var data = Get_ItemData(itemIndex);
            talk.EventReservation(data.eventPath);
        }

        public int IndexForItem(int valueIndex)
        {
            return valueIndex - valueBaseIndex;
        }

        public int IndexForValue(int itemIndex)
        {
            return itemIndex + valueBaseIndex;
        }

        public ItemStaticData Get_ItemData(int itemIndex)
        {
            var list = itemDBs.Get_ItemList();

            return list[Mathf.Clamp(itemIndex, 0, list.Count - 1)];
        }

    }
}