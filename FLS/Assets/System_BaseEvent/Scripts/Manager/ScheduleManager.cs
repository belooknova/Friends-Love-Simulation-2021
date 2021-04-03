using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FLS.Schedule
{
    /// <summary>
    /// スケジュールとカレンダー
    /// </summary>

    public class ScheduleManager : MonoBehaviour
    {
        
        public static ScheduleManager instance;

        private readonly List<SchedulingEventData> eventDatas = new List<SchedulingEventData>();
        private List<SchedulingEventData> onedayDatas = new List<SchedulingEventData>();

        private ValuesManager vm;
        private TalkEventManager talk;

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
            vm = ValuesManager.instance;
            talk = TalkEventManager.instance;
            SetUp_Order();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void SetUp_Order()
        {
            talk.Order_Registration("SCH_SET", Order_Set_Schedule, Exec_Set_Schedule);
        }

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "SCH_SET dateIndex(p) eventPath statFormale environFormale metaDataText"
        //  "SCH_SET date(p) time(p) eventPath statFormale environFormale metaDataText"
        private bool Order_Set_Schedule(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 6)
            {
                par.parString.Add(arg[1]);
                par.parString.Add(arg[2]);
                par.parString.Add(arg[3]);
                par.parString.Add(arg[4]);
                par.parString.Add(arg[5]);
                par.parInt.Add(0);

                return true;
            }

            if (arg.Length == 7)
            {
                par.parString.Add(arg[1]);
                par.parString.Add(arg[3]);
                par.parString.Add(arg[4]);
                par.parString.Add(arg[5]);
                par.parString.Add(arg[6]);
                par.parString.Add(arg[2]);
                par.parInt.Add(1);
            }
            return false;
        }
        private void Exec_Set_Schedule(ref int count, OrderParametor par)
        {
            int dateIndex;
            if (par.parInt[0] == 0)
            {
                dateIndex = new Parser(ParserType.Number, par.parString[1]).Eval_Value(vm.Get_Values());
            }
            else
            {
                int date = new Parser(ParserType.Number, par.parString[1]).Eval_Value(vm.Get_Values());
                int time = new Parser(ParserType.Number, par.parString[6]).Eval_Value(vm.Get_Values());
                dateIndex = Mathf.Clamp(date, 0, 31) * 11 + Mathf.Clamp(time, 0, 10);
            }

            string path = par.parString[2];
            string stat_formale = par.parString[3];
            string environ_formale = par.parString[4];
            string metaTexts = par.parString[5];

            Set_Schedule(dateIndex, path, stat_formale, environ_formale, metaTexts);

            count++;
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        #region ScheduleSystem

        /// <summary>
        /// 本日のイベントリストを参照してイベントを実行する
        /// </summary>
        public void EventChack()
        {
            foreach(var data in onedayDatas)
            {
                if (new Parser(ParserType.Bool, data.environ_formale).Eval(vm.Get_Values())) //環境
                {
                    talk.EventReservation(data.path);
                }
            }
        }

        /// <summary>
        /// 本日のイベントリストを作成する
        /// </summary>
        /// <param name="dateIndex"></param>
        public void Chack_OneDayEvent(int dateIndex)
        {
            onedayDatas.Clear();
            List<SchedulingEventData> outlist = new List<SchedulingEventData>();

            foreach(var data in eventDatas)
            {
                if (new Parser(ParserType.Bool, data.environ_formale).Eval(vm.Get_Values()))
                {
                    if (new Parser(ParserType.Bool, data.stat_formale).Eval(vm.Get_Values()))
                    {
                        onedayDatas.Add(data);
                    }
                }
            }
        }

        private void Set_Schedule(int dateIndex, string path, string stat_f, string environ_f, string metaText)
        {
            SchedulingEventData data = new SchedulingEventData()
            {
                dateIndex = dateIndex,
                path = path,
                stat_formale = stat_f,
                environ_formale = environ_f
            };

            MetaTextParser metaParser = new MetaTextParser(metaText);
            foreach (var metas in metaParser.Eval())
            {
                switch (metas[0])
                {
                    case "decscription":
                        if (metas.Count == 2)
                            data.description = metas[1];
                        break;
                    case "displaying":
                        data.specialFlag &= SchedulingEventData.FlagType.Displaying;
                        break;
                    case "not_active":
                        data.specialFlag &= SchedulingEventData.FlagType.NotActive;
                        break;
                    case "is_important":
                        data.specialFlag &= SchedulingEventData.FlagType.IsImportant;
                        break;
                    case "hiroineId": //Hiroin 5 3 2
                        if (metas.Count > 1)
                        {
                            for (int i = 1; i < metas.Count; i++)
                            {
                                int m = int.Parse(metas[i]);
                                switch (m)
                                {
                                    case 0:
                                        data.hiroineID &= SchedulingEventData.HiroineType.Hiroine1;
                                        break;
                                    case 1:
                                        data.hiroineID &= SchedulingEventData.HiroineType.Hiroine2;
                                        break;
                                    case 2:
                                        data.hiroineID &= SchedulingEventData.HiroineType.Hiroine3;
                                        break;
                                    case 3:
                                        data.hiroineID &= SchedulingEventData.HiroineType.Hiroine4;
                                        break;
                                    case 4:
                                        data.hiroineID &= SchedulingEventData.HiroineType.Hiroine5;
                                        break;
                                }
                            }
                        }
                        break;
                }
            }

            Set_Schedule(data);
        }

        public void Set_Schedule(SchedulingEventData data)
        {
            eventDatas.Add(data);
        }

        #endregion
        #region CalenderSystem



        #endregion



    }

    public sealed class SchedulingEventData
    {
        public string path;
        public string description;

        public int dateIndex = 0;
        public int Date { get { return dateIndex / 11; } }//日付
        public int Time { get { return dateIndex % 11; } }//時間帯
        public string environ_formale = "True";  //環境的起動条件
        public string stat_formale    = "True";     //状態的起動条件

        public HiroineType hiroineID = 0;
        public FlagType specialFlag = 0;

        //[Flags]
        public enum FlagType
        {
            NotActive    = 1<<0,
            Displaying   = 1<<1,
            IsImportant  = 1<<2,

        }
        
        //[Flags]
        public enum HiroineType
        {
            /// <summary> 暮音 </summary>
            Hiroine1        =1<<0,
            /// <summary> 沙苗 </summary>
            Hiroine2        =1<<1,
            /// <summary> あずき </summary>
            Hiroine3        =1<<2,
            /// <summary> 千鈴 </summary>
            Hiroine4        =1<<3,
            /// <summary> ヒロイン5 </summary>
            Hiroine5        =1<<4,
        }

    }

}