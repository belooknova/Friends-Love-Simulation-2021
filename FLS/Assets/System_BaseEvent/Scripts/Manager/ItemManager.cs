using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FLS.Item
{

    public class ItemManager : MonoBehaviour
    {
        public static ItemManager instance;

        [SerializeField]
        private int valueBaseIndex = 130;
        [SerializeField]
        private const int dmpValueCount = 60;
        [SerializeField]
        private ItemDBList itemDBs;
        [SerializeField]
        private ItemCall_Prefab prefab_Item;
        [SerializeField]
        private Transform canves;
        [SerializeField]
        private CanvasGroup canvasBase;
        [SerializeField]
        private Animator anime;

        /// <summary> インベントリを生成済 </summary>
        private bool isStartUped = false;
        /// <summary> 前回の取得アイテム </summary>
        private float[] dmpValues;

        private bool isDisplay = false;

        public string basePath = "/ItemFolder/";
        public string itemDeletePath = "/ItemFolder/delete_item";

        private readonly List<ItemCall_Prefab> call_Prefabs = new List<ItemCall_Prefab>();

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
        }

        private void SetUp_Order()
        {
            //talk.Order_Registration("ITEM_SET", Order_Set_Item, Exec_Set_Item);
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
        /// 初めてInventoryを起動する
        /// </summary>
        public void StartUp_Inventry()
        {
            isStartUped = true;
            Clear_dmp(dmpValueCount); //初期化
            //Copy_dmpValue(); //コピー

            Update_Indentry();

        }

        private void Update_Indentry()
        {
            for (int i = 0; i < dmpValueCount; i++)
            {
                if (vm.Get_Value(IndexForValue(i)) != dmpValues[i]) //異なる
                {
                    if (vm.Get_Value(IndexForValue(i)) > 0)
                    {
                        var data = Search_ItemCall(i);
                        if (data == null)
                        {
                            Set_ItemCell(i); //入手
                        }
                        else
                        {
                            data.UpdateCount(vm.Get_Value(IndexForValue(i))); //増減
                        }
                    }
                    else
                    {
                        Remove_ItemCall(i); //削除
                    }
                }
            }
        }

        private ItemCall_Prefab Search_ItemCall(int itemIndex)
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

        private void Remove_ItemCall(int index)
        {
            var data = Search_ItemCall(index);
            call_Prefabs.Remove(data);
            Destroy(data.gameObject);
        }

        private void Set_ItemCell(int index)
        {
            ItemCall_Prefab call = Instantiate(prefab_Item).GetComponent<ItemCall_Prefab>();
            call.transform.SetParent(canves);
            call.StartUp(this, index);
            call_Prefabs.Add(call);
        }

        /// <summary>
        /// ダンプ配列にアイテムの所持数をコピーする
        /// </summary>
        public void Copy_dmpValue()
        {
            var values = vm.Get_Values();
            for (int i = 0; i < values.Length; i++)
            {
                dmpValues[i] = values[IndexForValue(i)];
            }
        }

        /// <summary>
        /// ダンプ配列を初期化する
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

        private int IndexForItem(int valueIndex)
        {
            return valueIndex - valueBaseIndex;
        }

        private int IndexForValue(int itemIndex)
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