using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Text;

namespace FLS.Message
{
    
    public sealed class EventWindowManager : MonoBehaviour
    {
        public static EventWindowManager instance;
        private TalkEventManager TeManager;

        public enum MS_State
        {
            STANDBY, WAITING, DISPLAYING, DISPLAYED, DISPLAYED2, INTERVALSKIP, NEXT,
        }

        private List<string> list_endcode = new List<string>();

        /// <summary> メッセージシステムの状態 </summary>
        public MS_State MS_state = MS_State.STANDBY;
        /// <summary> 現在表示しているメッセージ </summary>　
        private string messageDisplay = "";
        /// <summary> 名前表示 </summary>
        private string message_name = "";
        /// <summary> 表示させる文字群 </summary>
        private string[] messageSplit;

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

        //========================================================================================

        public GameObject MessageObject;
        public GameObject MS_message_tO;
        public GameObject MS_message_nO;
        public GameObject MS_BackWindow;
        //public Sprite[] sprite_back;

        public Toggle button_skip;
        public Toggle button_auto;

        private Animator messAnimator;
        private Text MS_message_text;
        private Text MS_message_name;
        private Image MS_back_image;

        //private Char_Config char_Config;

        //========================================================================================

        private bool orderFinish = false;

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

            MS_message_text = MS_message_tO.GetComponentInChildren<Text>();
            MS_message_name = MS_message_nO.GetComponentInChildren<Text>();
            MS_back_image = MS_BackWindow.GetComponent<Image>();
            messAnimator = MessageObject.GetComponentInParent<Animator>();

            SetUp_Order();
            NoDisplayWindow(1);

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
                        par.parInt.Add(-1);
                        return true;
                    }

                    if (arg.Length == 3)
                    {
                        par.parString.Add(arg[1]);
                        par.parString.Add(arg[2]);
                        par.parInt.Add(-1);
                        return true;
                    }

                    if (arg.Length == 4)
                    {
                        par.parString.Add(arg[1]);
                        par.parString.Add(arg[2]);
                        par.parInt.Add(int.Parse(arg[3]));
                        return true;
                    }
                    return false;
                }, MessWin_Update);


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
                    if (message_name != "")
                    {
                        MS_DisplayWindow(1);
                    }
                    else
                    {
                        MS_DisplayWindow(0);
                    }

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
                    NoDisplayWindow(0);

                    count++;
                });
        }

        public void MessWin_Update(ref int count, OrderParametor par)
        {
            MS_message_text.text = messageDisplay + Get_EndCode();
            MS_message_name.text = message_name;

            //Debug.Log("name:"+message_name);

            switch (MS_state)
            {
                case MS_State.WAITING:
                    {
                        string m_name = par.parString[1];
                        string message = par.parString[0];
                        Convert_Value(ref message);
                        Convert_Value_Float(ref message);
                        Convert_Text(ref message);
                        Convert_Value(ref m_name);
                        Convert_Value_Float(ref m_name);
                        Convert_Text(ref m_name);

                        int mode = par.parInt[0];

                        MS_Waiting(m_name, message, mode);
                    }
                    break;
                case MS_State.DISPLAYING:
                    MS_Displaying();
                    break;
                case MS_State.DISPLAYED:
                    MS_Displayed();
                    break;
                case MS_State.DISPLAYED2:
                    MS_Displayed2();
                    break;
                case MS_State.INTERVALSKIP:
                    MS_IntervalSkip();
                    break;
                case MS_State.NEXT:
                    MS_Next();
                    break;
            }


            //キー入力
            if (Input.GetKeyDown(KeyCode.Space))
            {
                MS_Push_NextFlag();
            }

            //イベントが終了していたらウインドウを非表示にする
            if (!TeManager.IsReservation)
            {
                //message_auto = false;
                //message_skip = false;

                if (MS_message_tO.activeSelf)
                {
                    NoDisplayWindow(1);
                }
            }

            if (orderFinish)
            {
                orderFinish = false;
                count++;
            }
        }


        /// <summary>
        /// ステイトマシンを進める
        /// </summary>
        public void MS_Push_NextFlag()
        {
            if (MS_state == MS_State.DISPLAYING || MS_state == MS_State.DISPLAYED || MS_state == MS_State.DISPLAYED2)
            {
                message_nextFlag = true;
            }
        }


        /// <summary>
        /// ウインドウを出す
        /// </summary>
        public void MS_DisplayWindow(int state)
        {
            //MessageObject.SetActive(true);

            switch (state)
            {
                case 0:
                    MS_message_nO.SetActive(true);
                    MS_message_tO.SetActive(true);
                    break;
                case 1:
                    MS_message_nO.SetActive(false);
                    MS_message_tO.SetActive(true);
                    break;
            }

            messAnimator.SetInteger("Stat", 1);
        }

        /// <summary>
        /// ウインドウを消す
        /// </summary>
        public void NoDisplayWindow(int state)
        {
            messAnimator.SetInteger("Stat", 2);

            StartCoroutine(C_NDW());

            return;

            IEnumerator C_NDW()
            {
                while (GetStatAnime("Show")) { yield return null; } //アニメが終わるまで待つ

                //MessageObject.SetActive(false);
                if (state == 1)
                {
                    messageDisplay = "";
                    message_name = "";
                }
            }
        }



        /// <summary>
        /// 待機状態
        /// </summary>
        private void MS_Waiting(string m_name, string mess, int m_mode)
        {
            //名前の有無によってウインドウを表示/非表示する
            if (m_name == "")
            {
                MS_DisplayWindow(1);
            }
            else
            {
                MS_DisplayWindow(0);
            }

            //次のイベントオブジェクトを読み込む
            message_name = m_name;
            messageSplit = MS_Split(mess);
            message_nextFlag = false;

            //初期化
            if (messageDisplay != "")
            {
                messageDisplay = "";
            }

            isDisplayed = false;
            MS_state = MS_State.DISPLAYING;
        }

        /// <summary>
        /// 表示中状態
        /// </summary>
        private void MS_Displaying()
        {
            if (!onMS_Coroutine) StartCoroutine(C_MS_Displaying());
        }

        private bool GetStatAnime(string name)
        {
            return messAnimator.GetCurrentAnimatorStateInfo(0).IsName(name);
        }

        private IEnumerator C_MS_Displaying()
        {
            onMS_Coroutine = true;


            while (GetStatAnime("Hide")) { yield return null; } //アニメが終わるまで待つ

            foreach (string ms in messageSplit)
            {
                if (message_skip)
                {
                    break;
                }

                if (message_nextFlag)
                {
                    message_nextFlag = false;
                    break;
                }

                EndCoding(ms);
                messageDisplay += SplitMessage(ms);

                yield return new WaitForSeconds(0.05f);
            }
            MS_state = MS_State.DISPLAYED;
            onMS_Coroutine = false;
        }

        private string SplitMessage(string ms)
        {
            if (ValuesManager.instance != null)
            {
                if (Regex.IsMatch(ms, @"<v\d{1,4}>"))
                {
                    string m = "";
                    for (int i = 2; i < ms.Length - 1; i++)
                    {
                        m += ms[i];
                    }
                    int v = (int)ValuesManager.instance.Get_Value(int.Parse(m));

                    return v.ToString();
                }

                if (Regex.IsMatch(ms, @"<vf\d{1,4}>"))
                {
                    string m = "";
                    for (int i = 2; i < ms.Length - 1; i++)
                    {
                        m += ms[i];
                    }
                    float v = ValuesManager.instance.Get_Value(int.Parse(m));

                    return v.ToString();
                }
            }
            return ms;
        }

        /// <summary>
        /// 表示済み状態
        /// </summary>
        private void MS_Displayed()
        {
            MS_DisplayAll();

            if (!onMS_Coroutine) StartCoroutine(C_MS_Displayed());

            if (message_skip)
            {
                MS_state = MS_State.INTERVALSKIP;
            }

            if (message_nextFlag)
            {
                MS_state = MS_State.NEXT;
            }
        }

        private IEnumerator C_MS_Displayed()
        {
            onMS_Coroutine = true;
            for (int ci = 0; ci < 60; ci++)
            {
                if (MS_state != MS_State.DISPLAYED)
                {
                    onMS_Coroutine = false;
                    yield break;
                }
                yield return null;
            }
            isDisplayed = true;
            MS_state = MS_State.DISPLAYED2;
            onMS_Coroutine = false;
        }

        /// <summary>
        /// メッセージを全表示する
        /// </summary>
        private void MS_DisplayAll()
        {
            if (isDisplayed) return;

            messageDisplay = "";
            foreach (string a in messageSplit)
            {
                messageDisplay += a;
            }

            isDisplayed = true;
            list_endcode.Clear(); //末尾文字を表示しない
        }

        /// <summary>
        /// 表示済2
        /// </summary>
        private void MS_Displayed2()
        {
            if (message_nextFlag || message_auto)
            {
                MS_state = MS_State.NEXT;
            }

        }

        /// <summary>
        /// スキップのための間
        /// </summary>
        private void MS_IntervalSkip()
        {
            if (!onMS_Coroutine) StartCoroutine(C_MS_IntervalSkip());
        }

        private IEnumerator C_MS_IntervalSkip()
        {
            onMS_Coroutine = true;
            yield return null;

            MS_state = MS_State.NEXT;
            onMS_Coroutine = false;
        }

        private void MS_Next()
        {
            LogManager.instance.Add_LogMess(messageDisplay, message_name);

            Debug.Log(TeManager.Next_OrderType());

            Debug.LogFormat("N.O.T: {0}", TeManager.Next_OrderType());

            if (TeManager.Next_OrderType() == "EVENT_EXIT") //イベント終了
            {
                button_skip.isOn = false;
                message_skip = false;
                NoDisplayWindow(1);
            }
            else if (TeManager.Next_OrderType() == "SELECT_set") //メッセージ以外のタイプだった場合
            {
                button_skip.isOn = false;
                message_skip = false;
                //NoDisplayWindow(1);

            }
            else if (TeManager.Next_OrderType() != "MESS") //メッセージ以外のタイプだった場合
            {
                button_skip.isOn = false;
                message_skip = false;
                NoDisplayWindow(1);

            }

            orderFinish = true;

            MS_state = MS_State.WAITING;
        }

        //＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

        private string[] MS_Split(string o_message)
        {
            List<string> outList = new List<string>();
            string mass = o_message;
            bool gmode = false;
            string buff = "";

            foreach (char c in mass)
            {
                if (c == '<')
                {
                    gmode = true;
                    buff += c;
                }
                else if (c == '>')
                {
                    gmode = false;
                    buff += c;
                    outList.Add(EscCode(buff));
                    buff = "";
                }
                else
                {
                    buff += c;
                    if (!gmode)
                    {
                        outList.Add(buff);
                        buff = "";
                    }
                }
            }

            return outList.ToArray();
        }

        private string EscCode(string buff)
        {
            switch (buff)
            {
                case @"<\n>":
                    return "\n";
                default:
                    return buff;
            }
        }

        private void EndCoding(string code)
        {
            //Debug.Log("EndCoding: " + code);
            switch (code)
            {
                case @"<b>":
                    list_endcode.Add("</b>");
                    break;
                case @"<i>":
                    list_endcode.Add("</i>");
                    break;
                case @"</b>":
                    list_endcode.Remove("</b>");
                    break;
                case @"</i>":
                    list_endcode.Remove("</i>");
                    break;
                case @"</size>":
                    list_endcode.Remove("</size>");
                    break;
                case @"</color>":
                    list_endcode.Remove("</color>");
                    break;
            }

            if (Regex.IsMatch(code, @"<size=([0-9]+)"))
            {
                list_endcode.Add("</size>");
            }

            if (Regex.IsMatch(code, @"<color=.+>"))
            {
                list_endcode.Add("</color>");
            }
        }

        private string Get_EndCode()
        {
            string list = "";

            for (int i = 0; i < list_endcode.Count; i++)
            {
                list += list_endcode[list_endcode.Count - 1 - i];
            }
            return list;
        }

        private void Convert_Value(ref string _formale)
        {
            while (true)
            {
                Match match = Regex.Match(_formale, @"\\v\[\d{1,4}\]|<v=\d{1,4}>");
                if (match.Success)
                {
                    Debug.Log("成功");
                    StringBuilder sb = new StringBuilder(match.Value).Remove(0, 3).Remove(match.Length - 1 - 3, 1);
                    int value = ValuesManager.instance.Get_Value(int.Parse(sb.ToString()));
                    _formale = Regex.Replace(_formale, new StringBuilder(@"\\v\[").Append(sb.ToString()).Append(@"\]").ToString(), value.ToString());
                    _formale = Regex.Replace(_formale, new StringBuilder(@"<v=").Append(sb.ToString()).Append(@">").ToString(), value.ToString());

                }
                else
                {
                    break;
                }
            }
        }

        private void Convert_Text(ref string _formale)
        {
            while (true)
            {
                Match match = Regex.Match(_formale, @"\\t\[\d{1,4}\]|<s=\d{1,4}>");
                if (match.Success)
                {
                    StringBuilder sb = new StringBuilder(match.Value).Remove(0, 3).Remove(match.Length - 1 - 3, 1);
                    string value = ValuesManager.instance.Get_Text(int.Parse(sb.ToString()));
                    _formale = Regex.Replace(_formale, new StringBuilder(@"\\t\[").Append(sb.ToString()).Append(@"\]").ToString(), value);
                    _formale = Regex.Replace(_formale, new StringBuilder(@"<s=").Append(sb.ToString()).Append(@">").ToString(), value.ToString());

                }
                else
                {
                    break;
                }
            }
        }

        private void Convert_Value_Float(ref string _formale)
        {
            while (true)
            {
                Match match = Regex.Match(_formale, @"\\f\[\d{1,4}\]|<f=\d{1,4}>");
                if (match.Success)
                {
                    StringBuilder sb = new StringBuilder(match.Value).Remove(0, 3).Remove(match.Length - 1 - 3, 1);
                    float value = ValuesManager.instance.Get_Value_Float(int.Parse(sb.ToString()));
                    _formale = Regex.Replace(_formale, new StringBuilder(@"\\f\[").Append(sb.ToString()).Append(@"\]").ToString(), value.ToString());
                    _formale = Regex.Replace(_formale, new StringBuilder(@"<f=").Append(sb.ToString()).Append(@">").ToString(), value.ToString());

                }
                else
                {
                    break;
                }
            }
        }

        public static string ReplaceCode_Dialog(string mass)
        {
            string outText = mass;
            //Debug.Log(outText);
            outText = Regex.Replace(outText, @"\\n", "\n");

            return ReplaceCode(outText);
        }

        public static string ReplaceCode(string mass)
        {
            string outText = mass;

            

            //値を書き換える
            if (ValuesManager.instance != null)
            {
                MatchCollection match_values = Regex.Matches(mass, @"<v=\d{1,4}>");
                foreach (Match m in match_values)
                {
                    string ms = "";
                    for (int i = 3; i < m.Value.Length - 1; i++)
                    {
                        ms += m.Value[i];
                    }
                    int index = int.Parse(ms);

                    Regex regex = new Regex(m.Value);
                    outText = regex.Replace(outText, ValuesManager.instance.Get_Value(index).ToString());
                }

                MatchCollection match_float = Regex.Matches(mass, @"<f=\d{1,4}>");
                foreach (Match m in match_float)
                {
                    string ms = "";
                    for (int i = 3; i < m.Value.Length - 1; i++)
                    {
                        ms += m.Value[i];
                    }
                    int index = int.Parse(ms);

                    Regex regex = new Regex(m.Value);
                    outText = regex.Replace(outText, ValuesManager.instance.Get_Value_Float(index).ToString());
                }

                MatchCollection match_string = Regex.Matches(mass, @"<s=\d{1,4}>");
                foreach (Match m in match_string)
                {
                    string ms = "";
                    for (int i = 3; i < m.Value.Length - 1; i++)
                    {
                        ms += m.Value[i];
                    }
                    int index = int.Parse(ms);

                    Regex regex = new Regex(m.Value);
                    outText = regex.Replace(outText, ValuesManager.instance.Get_Text(index));
                }
            }

            return outText;
        }


    }
}