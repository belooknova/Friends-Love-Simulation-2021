using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Text;
using System.Text.RegularExpressions;

namespace FLS.Message
{
    public class MessWindowObject : MonoBehaviour
    {
        [SerializeField]
        private Transform on_TargetPoint;
        [SerializeField]
        private Text messTextComponent;
        [SerializeField]
        private Text nameTextComponent;

        [HideInInspector]
        public bool IsNamed
        {
            get { return _isNamed; }
            set { Set_NameDisplay(_isNamed); }
        }

        public bool IsFinish
        {
            get;
            private set;
        }

        public bool IsSkip
        {
            get;
            private set;
        } = false;

        public bool IsAuto
        {
            get;
            private set;
        } = false;

        private bool _isNamed = false;
        private bool _next_flag = false;
        private bool _isAnimation = false;

        public float fadeTime = 0.5f;
        public float textSpeed_base = 5f;



        // Start is called before the first frame update
        void Start()
        {
            NoDisplay_window();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Play(@"<b><s=1><v=2><f=3>\t[1]\v[2]\f[3]アルマジロ</b><speed=40>アルマジロせやねんせやねん\nおひたし美味しいあああああああああああああああああああ", "リス");
            }


            if (Input.GetKeyDown(KeyCode.Space))
            {
                Next();
            }

        }

        public void Play(string _text)
        {
            Play(_text, "");
        }

        public void Play(string _text, string _name)
        {
            //in -> out
            //      "<s=2>"     ->  変数文字列
            //      "\t[2]"     ->  変数文字列
            //      "<v=1>"     ->  変数数値
            //      "\v[1]"     ->  変数数値
            //      "<f=3>"     ->  変数実数
            //      "\f[3]"     ->  変数実数
            //      "\n"        ->  \n

            if (_name != "")
            {
                Set_NameDisplay(true);
                nameTextComponent.text = Conversion_Text(_name);
            }
            else
            {
                Set_NameDisplay(false);
            }

            Display_window();

            var splited = Split_text(Conversion_Text(_text));
            StartCoroutine(Playing(splited));

        }

        private IEnumerator Playing(Queue<string> textQueue)
        {
            IsFinish = false;

            while (_isAnimation)
            {
                yield return null;
            }

            float textSpeed = textSpeed_base;
            StringBuilder texting = new StringBuilder();
            List<string> codeList = new List<string>();

            while (textQueue.Count != 0)
            {
                string code = textQueue.Dequeue();

                //末尾文字を追加
                string endCode = Add_EndCoding(code, codeList);
                bool resign = false;

                if (RealtimeConfig_speed(code, ref textSpeed)) resign = true;

                //文字を追加
                if (!resign) texting.Append(code);
                //メッセージを表示
                messTextComponent.text = new StringBuilder(texting.ToString()).Append(endCode).ToString();

                //----------
                if (!_next_flag && !IsSkip)
                {  
                    //返す
                    yield return new WaitForSeconds(textSpeed / 100f);
                }
            }
            _next_flag = false;


            while (!_next_flag && !IsSkip && !IsAuto)
            {
                yield return null;
            }
            _next_flag = false;

            Debug.Log("終わり");
            IsFinish = true;
        }

        /// <summary>
        /// テキストを一文字づつ区切る
        /// </summary>
        /// <param name="_text"></param>
        /// <returns></returns>
        private Queue<string> Split_text(string _text)
        {
            bool sw = false;
            Queue<string> textQueue = new Queue<string>();
            StringBuilder buf = new StringBuilder();

            foreach(char ms in _text)
            {

                if (sw)
                {
                    buf.Append(ms);
                }
                else
                {
                    if (ms == '<')
                    {
                        buf.Append(ms);
                        sw = true;
                    }
                    else
                    {
                        textQueue.Enqueue(ms.ToString());
                    }
                }

                if (ms == '>' && sw)
                {
                    sw = false;
                    textQueue.Enqueue(buf.ToString());
                    buf.Clear();
                }
            }

            foreach(var v in textQueue)
            {
                //Debug.Log(v);
            }

            return textQueue;
        }

        /// <summary>
        /// 末尾文字を追加する
        /// </summary>
        /// <param name="code"></param>
        /// <param name="endCodeList"></param>
        /// <returns></returns>
        private string Add_EndCoding(string code, List<string> endCodeList)
        {
            //Debug.Log("EndCoding: " + code);
            switch (code)
            {
                case @"<b>":
                    endCodeList.Add("</b>");
                    break;
                case @"<i>":
                    endCodeList.Add("</i>");
                    break;
                case @"</b>":
                    endCodeList.Remove("</b>");
                    break;
                case @"</i>":
                    endCodeList.Remove("</i>");
                    break;
                case @"</size>":
                    endCodeList.Remove("</size>");
                    break;
                case @"</color>":
                    endCodeList.Remove("</color>");
                    break;
            }

            if (Regex.IsMatch(code, @"<size=.+>"))
            {
                endCodeList.Add("</size>");
            }

            if (Regex.IsMatch(code, @"<color=.+>"))
            {
                endCodeList.Add("</color>");
            }

            StringBuilder sb = new StringBuilder();
            foreach(string endCode in endCodeList)
            {
                sb.Append(endCode);
            }

            return sb.ToString();
        }

        /// <summary>
        /// スピードの設定
        /// </summary>
        /// <param name="code"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        private bool RealtimeConfig_speed(string code, ref float speed)
        {
            if (Regex.IsMatch(code, @"<speed=.+>"))
            {
                string value = code.Substring(7, code.Length - 7 - 1);
                speed = float.Parse(value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// テキストを変換
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string Conversion_Text(string text)
        {
            string textout = text;
            {
                Match match = Regex.Match(textout, @"\\t\[\d{1,4}\]|<s=\d{1,4}>");
                if (match.Success)
                {
                    int index = int.Parse(match.Value.Substring(3, match.Value.Length - 3 - 1));
                    var value = ValuesManager.instance.Get_Text(index);
                    textout = Regex.Replace(textout, @"\\t\[\d{1,4}\]|<s=\d{1,4}>", value);
                }
            }

            {
                Match match = Regex.Match(textout, @"\\v\[\d{1,4}\]|<v=\d{1,4}>");
                if (match.Success)
                {
                    int index = int.Parse(match.Value.Substring(3, match.Value.Length - 3 - 1));
                    var value = ValuesManager.instance.Get_Value(index);
                    textout = Regex.Replace(textout, @"\\v\[\d{1,4}\]|<v=\d{1,4}>", value.ToString());
                }
            }

            {
                Match match = Regex.Match(textout, @"\\f\[\d{1,4}\]|<f=\d{1,4}>");
                if (match.Success)
                {
                    int index = int.Parse(match.Value.Substring(3, match.Value.Length - 3 - 1));
                    var value = ValuesManager.instance.Get_Value_Float(index);
                    textout = Regex.Replace(textout, @"\\f\[\d{1,4}\]|<f=\d{1,4}>", value.ToString());
                }
            }

            textout = Regex.Replace(textout, @"\\n", '\n'.ToString());

            return textout;
        }

        public void Display_window()
        {
            transform.DOLocalMove(on_TargetPoint.localPosition, fadeTime)
                .OnStart(() =>
                {
                    _isAnimation = true;
                })
                .OnComplete(()=> 
                {
                    _isAnimation = false;
                });
        }

        public void NoDisplay_window()
        {
            transform.DOLocalMove(on_TargetPoint.localPosition + Vector3.down * 400, fadeTime)
                .OnStart(() =>
                {
                    _isAnimation = true;
                })
                .OnComplete(() =>
                {
                    _isAnimation = false;
                });
        }

        private void Set_NameDisplay(bool sw)
        {
            _isNamed = sw;
            nameTextComponent.transform.parent.gameObject.SetActive(sw);
        }

        public void Next()
        {
            _next_flag = true;
        }
    }

}