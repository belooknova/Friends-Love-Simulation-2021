using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public string basePath = "ItemFolder/";
        public string itemDeletePath = "/ItemFolder/_system_delete_item";
        public string itemInventryUpdatePath = "/ItemFolder/_system_inventry_update";

        public bool isItemEventing = false;

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

            if (vm.Get_Value(updateIndex) == 1)
            {
                Update_Indentry();
                vm.Set_Value(updateIndex, 0);
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
                var value = vm.Get_Value(IndexForValue(i));
                if (value != dmpValues[i]) //異なる
                {
                    if (value > 0)
                    {
                        var data = Search_ItemCall(i);
                        Debug.Log(data);
                        if (data == null)
                        {
                            Set_ItemCell(i); //入手
                        }
                        else
                        {
                            data.UpdateCount(value); //増減
                        }
                    }
                    else
                    {
                        Remove_ItemCell(i); //削除

                    }
                }

                dmpValues[i] = value;
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

        private void Remove_ItemCell(int index)
        {
            var data = Search_ItemCall(index);
            call_Prefabs.Remove(data);
            Destroy(data.gameObject);
        }

        private void Set_ItemCell(int index)
        {
            ItemCall_Prefab cell = Instantiate(prefab_Item).GetComponent<ItemCall_Prefab>();
            cell.transform.SetParent(canves);
            cell.StartUp(this, index);
            call_Prefabs.Add(cell);
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