using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Text;
using DG.Tweening;

namespace FLS.Message
{
    
    public sealed class EventWindowManager : MonoBehaviour
    {
        public static EventWindowManager instance;
        private TalkEventManager TeManager;

        /// <summary> メッセージ許可 </summary>
        //public bool massageApproval = false;
        /// <summary> スキップ有効 </summary>
        public bool message_skip = false;
        /// <summary> オート有効 </summary>
        public bool message_auto = false;
        /// <summary> 進行用のフラグ </summary>
        public bool message_nextFlag = false;

        /// <summary> コルーチン管理フラグ </summary>
        private bool onMS_Coroutine = false;

        /// <summary> メッセージを表示済かどうか </summary>
        private bool isDisplayed = false;

        public MessWindowObject MessWindowObject;

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

        private void Start()
        {
            TeManager = TalkEventManager.instance;

            //MS_message_text = MS_message_tO.GetComponentInChildren<Text>();
            //MS_message_name = MS_message_nO.GetComponentInChildren<Text>();
            //MS_back_image = MS_BackWindow.GetComponent<Image>();
            //messAnimator = MessageObject.GetComponentInParent<Animator>();

            SetUp_Order();
            //NoDisplayWindow(1);

            //MS_window_animator = MS_BackWindow.GetComponentInParent<Animator>();
        }

        private void SetUp_Order()
        {
            TalkEventManager talk = TalkEventManager.instance;

            talk.Order_Registration("MESS", //『メッセージを表示する』
                                            //デコード時の設定
                (int count, OrderParametor par, string[] arg) =>
                {
                    if (arg.Length == 2)
                    {
                        par.parString.Add(arg[1]);
                        par.parString.Add("");
                        par.parInt.Add(0);
                        return true;
                    }

                    if (arg.Length == 3)
                    {
                        par.parString.Add(arg[1]);
                        par.parString.Add(arg[2]);
                        par.parInt.Add(0);
                        return true;
                    }

                    if (arg.Length == 4)
                    {
                        par.parString.Add(arg[1]);
                        par.parString.Add(arg[2]);
                        par.parInt.Add(0);
                        return true;
                    }
                    return false;
                }, Update_Mess);


            talk.Order_Registration("MESSON", //『メッセージウインドウを表示する(非推奨)』
                                              //デコード時の設定
                (int count, OrderParametor par, string[] arg) =>
                {
                    if (arg.Length == 1)
                    {
                        return true;
                    }

                    return false;
                },
                (ref int count, OrderParametor par) =>
                {
                    MessWindowObject.Display_window();
                    count++;
                });

            talk.Order_Registration("MESSOFF", //『メッセージウインドウを非表示にする(非推奨)』
                                               //デコード時の設定
                (int count, OrderParametor par, string[] arg) =>
                {
                    if (arg.Length == 1)
                    {
                        return true;
                    }

                    return false;
                },
                (ref int count, OrderParametor par) =>
                {
                    MessWindowObject.NoDisplay_window();
                    count++;
                });
        }

        public void Update_Mess(ref int count, OrderParametor par)
        {
            int stat = par.parInt[0];
            string text = par.parString[0];
            string nameTag = par.parString[1];

            if (stat == 0)
            {
                MessWindowObject.Play(text, nameTag);
                par.parInt[0] = 1;
            }

            if (stat == 1)
            {
                if (MessWindowObject.IsFinish)
                {
                    Next_Mess();
                    count++;
                }
            }
        }

        private void Next_Mess()
        {
            //LogManager.instance.Add_LogMess(messageDisplay, message_name);

            Debug.Log(TeManager.Next_OrderType());

            Debug.LogFormat("N.O.T: {0}", TeManager.Next_OrderType());

            if (TeManager.Next_OrderType() == "EVENT_EXIT") //イベント終了
            {
                //button_skip.isOn = false;
                //message_skip = false;
                MessWindowObject.NoDisplay_window();
            }
            else if (TeManager.Next_OrderType() == "SELECT_set") //メッセージ以外のタイプだった場合
            {
                //button_skip.isOn = false;
                //message_skip = false;
                //NoDisplayWindow(1);

            }
            else if (TeManager.Next_OrderType() != "MESS") //メッセージ以外のタイプだった場合
            {
                //button_skip.isOn = false;
                //message_skip = false;
                MessWindowObject.NoDisplay_window();

            }
        }

    }
}