using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;


public class OrderParametor
{
    public List<int> parInt = new List<int>();
    public List<float> parFloat = new List<float>();
    public List<string> parString = new List<string>();
    public List<Color> parColor = new List<Color>();
    public List<Vector3> parVector = new List<Vector3>();
}

public delegate void OrderExec(ref int count, OrderParametor par);
public delegate bool OrderDecode(int count, OrderParametor par, string[] arg);

public class TalkEventManager : MonoBehaviour
{
    public static TalkEventManager instance;

    private readonly string basefile = "MassageTexts/";

    /// <summary>　イベント予約中　</summary>
    public bool IsReservation { private set; get; } = false;
    public int CullentIndex { private set; get; } = 0;
    public bool IsNowloding { private set; get; } = false;

    //======《コルーチン》======================================================

    /// <summary> 実行中イベント数 </summary>
    public bool IsRunningEvent { get; private set; } = false;
    private bool isEventParsing = false;


    //======《イベント関係》====================================================


    private readonly Queue<string> pathParserCodes = new Queue<string>();
    /// <summary> 実行イベントリスト </summary>
    private readonly List<TalkEventMasterData> masterDatas = new List<TalkEventMasterData>();

    /// <summary> 命令イベントデータ </summary>
    private readonly Dictionary<string, Talk_OrderData> orderDatas = new Dictionary<string, Talk_OrderData>();

    private TalkEventMasterData setting_masterData = null;

    //======《分岐関係》====================================================
    /// <summary> 分岐設定用イベント </summary>
    private readonly List<OrderParametor> branchEvent = new List<OrderParametor>();


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
    IEnumerator Start()
    {
        SetUp_Order();
        yield return null;


        
        //EventReservation("Test7");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            EventReservation("00TEST");
        }

        //命令解析
        if (pathParserCodes.Count > 0)
        {
            if (!isEventParsing)
            {
                StartCoroutine(C_EventReservation(pathParserCodes.Dequeue()));
            }
        }

        //実行
        if (masterDatas.Count > 0)
        {
            IsReservation = true;
            Run_CullectEvent();

        }
        else
        {
            IsReservation = false;
        }
    }

    /// <summary>
    /// 次のイベント名
    /// </summary>
    /// <returns></returns>
    public string Next_OrderType()
    {
        string output = "NO_EVENT";

        if (masterDatas.Count == 1) //予約中が一つ
        {
            var datas = masterDatas[0].datas;

            if (CullentIndex + 1 < datas.Count)
            {
                output = datas[CullentIndex + 1].tType;
            }
            else
            {
                output = "EVENT_EXIT";
            }

        }
        else if (masterDatas.Count > 1)
        {
            var datas = masterDatas[0].datas;

            if (CullentIndex + 1 < datas.Count)
            {
                output = datas[CullentIndex + 1].tType;
            }
            else
            {
                var datas2 = masterDatas[1].datas;
                output = datas2[0].tType;
            }
        }

        return output;
    }

    /// <summary>
    /// 現在のイベントを実行する
    /// </summary>
    private void Run_CullectEvent()
    {
        if (masterDatas[0].mode == TalkEventMasterData.Mode.Normal)
        {
            if (!IsRunningEvent)
            {

                StartCoroutine(RunEvent(masterDatas[0].datas));
                //masterDatas.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// イベントを予約する
    /// </summary>
    /// <param name="path"></param>
    public void EventReservation(string path)
    {
        pathParserCodes.Enqueue(path);
        //StartCoroutine(C_EventReservation(path));
    }

    private IEnumerator C_EventReservation(string path)
    {
        //yield return null;
        isEventParsing = true;

        if (LoadTextLine(out string[] texts, path))
        {
            TalkEventMasterData masterData = new TalkEventMasterData();
            setting_masterData = masterData;

            int count = 0;
            int c = 0;

            for (int i = 0; i < texts.Length; i++)
            {
                string text = texts[i];

                AnalysisEvent(SplitText(text), masterData, ref count);

                c++;
                if (c == 120)
                {
                    c = 0;
                    yield return null;
                    IsNowloding = true;
                }
            }

            setting_masterData = null;
            Debug.LogFormat("[{0}]を登録", path);
            masterDatas.Add(masterData);
        }

        IsNowloding = false;
        isEventParsing = false;
    }

    /// <summary>
    /// 命令と引数を分ける
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private string[] SplitText(string text)
    {
        string checkText = "";
        bool areaSwitch = true;
        List<string> vs = new List<string>();

        foreach (char c in text)
        {
            if (c == '"') //囲いは無条件で取り入れる
            {
                areaSwitch = !areaSwitch;
                continue;
            }

            if (areaSwitch)
            {
                if (Regex.IsMatch(c.ToString(), @"\s"))
                {
                    if (checkText != "")
                    {
                        vs.Add(checkText);
                    }
                    checkText = "";
                    continue;
                }
            }

            checkText += c;
        }

        if (checkText != "")
        {
            vs.Add(checkText);
        }

        return vs.ToArray();
    }

    /// <summary>
    /// 分けた引数を解析してEventDataに変換する
    /// </summary>
    /// <param name="formale"></param>
    /// <param name="masterData"></param>
    private void AnalysisEvent(string[] formale, TalkEventMasterData masterData, ref int cullentCount)
    {
        foreach (string s in formale)
        {
            //Debug.Log(s);
        }

        var data = Decoding(formale, cullentCount);
        if (data != null)
        {
            var ot = orderDatas[formale[0]].type;

            if (ot == Type_TalkEventData.SINGLE)
            {
                //Debug.Log(data.type);
                masterData.datas.Add(data); //マスターデータに登録
                cullentCount++;
            }
            else if (ot == Type_TalkEventData.MULTI)
            {
                //登録を任せる
            }
        }
    }

    private TalkEventData Decoding(string[] formale, int cullentCount)
    {
        if (formale.Length == 0) return null;

        if (formale.Length > 0)
        {
            if (orderDatas.ContainsKey(formale[0]))
            {
                Debug.LogFormat("命令：{0}", formale[0]);

                string m_name = formale[0];
                Talk_OrderData orderData = orderDatas[m_name];
                OrderParametor parametor = new OrderParametor();

                Debug.LogFormat("cullentCount: {0}  datas.Count: {1}", cullentCount, setting_masterData.datas.Count);

                if (!orderData.decode(setting_masterData.datas.Count, parametor, formale))
                {
                    return null;
                }

                var eventData = new TalkEventData(m_name, parametor, orderData.order);

                return eventData;
            }
        }

        return null;
    }

    /// <summary>
    /// テキストをロードし、一行ごとのテキストデータを読み出す
    /// </summary>
    /// <param name="path"></param>
    private bool LoadTextLine(out string[] texts, string path)
    {
        texts = new string[0];

        try
        {
            TextAsset textasset = Resources.Load(basefile + path, typeof(TextAsset)) as TextAsset;
            string TextLines = textasset.text; //テキスト全体をstring型で入れる変数を用意して入れる
            texts = TextLines.Split('\n'); //Splitで一行づつを代入した1次配列を作成
        }
        catch
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// イベントを実行する
    /// </summary>
    /// <param name="eventDatas"></param>
    /// <returns></returns>
    private IEnumerator RunEvent(List<TalkEventData> eventDatas)
    {
        IsRunningEvent = true;

        int count = 0;
        int max_count = eventDatas.Count;
        int memory;
        Debug.Log("イベント実行");

        while (count < max_count)
        {
            TalkEventData ed = eventDatas[count];
            memory = count;

            CullentIndex = count;

            //Debug.LogFormat("[実行] {0}", ed.tType);
            ed.action?.Invoke(ref count, ed.data); //実行

            if (memory == count) //同じカウントで逃がす
            {
                //Debug.Log("逃げてる");
                yield return null;
            }

            if (count == -1) break; //強制終了
        }

        masterDatas.RemoveAt(0);
        IsRunningEvent = false;
    }

    public void EventRegistration(TalkEventData eventData)
    {
        setting_masterData.datas.Add(eventData); //マスターデータに登録
    }

    public void EventRegistration(string formale)
    {
        if (setting_masterData != null)
        {
            string[] texts = SplitText(formale);
            if (orderDatas.ContainsKey(texts[0]))
            {
                Talk_OrderData orderData = orderDatas[texts[0]];
                OrderParametor parametor = new OrderParametor();
                int count = setting_masterData.datas.Count;

                if (orderData.decode(count, parametor, texts))
                {
                    var eventData = new TalkEventData(texts[0], parametor, orderData.order);
                    setting_masterData.datas.Add(eventData);
                }
            }
        }
    }

    /// <summary>
    /// 命令予約
    /// ()
    /// </summary>
    /// <param name="orderName"></param>
    /// <param name="decode"></param>
    /// <param name="orderExec"></param>
    public void Order_Registration(string orderName, OrderDecode decode, OrderExec orderExec, Type_TalkEventData type)
    {
        var t = new Talk_OrderData()
        {
            tType = orderName,
            decode = decode,
            order = orderExec,
            type = type
        };

        if (!orderDatas.ContainsKey(orderName))
        {
            orderDatas.Add(orderName, t);
        }
        else
        {
            Debug.LogErrorFormat("命令宣言：すでに存在する命令名です。({0}, {1})", orderName, GetType().Name);
        }
    }

    public void Order_Registration(string orderName, OrderDecode decode, OrderExec orderExec)
    {
        Order_Registration(orderName, decode, orderExec, Type_TalkEventData.SINGLE);
    }


    private void SetUp_Order()
    {
        #region システム
        Order_Registration("WAIT", //『n秒待機する』
                                    //デコード時の設定
            (int count, OrderParametor par, string[] arg) =>
                {
                    if (arg.Length == 2)
                    {
                        par.parFloat.Add(float.Parse(arg[1]));
                        par.parFloat.Add(0);

                        return true;
                    }

                    return false;
                },

            //実行内容
            (ref int count, OrderParametor par) =>
                {
                    par.parFloat[1] += Time.deltaTime / Time.timeScale;
                    if (par.parFloat[0] <= par.parFloat[1])
                    {
                        count++;
                    }
                }
            );

        OrderDecode decode_order_ifwait = (int count, OrderParametor par, string[] arg) =>
        {
            if (arg.Length == 2)
            {
                par.parString.Add(arg[1]);

                return true;
            }

            return false;
        };

        OrderExec exec_order_ifwait = (ref int count, OrderParametor par) =>
        {
            string formale = par.parString[0];

            Parser parser = new Parser(ParserType.Bool, formale);

            if (parser.Eval(ValuesManager.instance.Get_Values()))
            {
                count++;
            }
        };

        Order_Registration("IFWAIT", decode_order_ifwait, exec_order_ifwait);
        Order_Registration("DIALOG_WAIT", decode_order_ifwait, exec_order_ifwait);


        Order_Registration("EVENTEND", //『イベントを強制終了する』
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
                count = -1;
            });

        Order_Registration("EVENT_RV", //『イベントを予約する』
            (int count, OrderParametor par, string[] arg) =>
            {
                if (arg.Length == 2)
                {
                    par.parString.Add(arg[1]);

                    return true;
                }

                return false;
            },

            (ref int count, OrderParametor par) =>
            {
                string path = par.parString[0];

                EventReservation(path);

                count++;
            });

        //==============================================================================

        //分岐
        OrderDecode decode_order_branch = (int count, OrderParametor par, string[] arg) =>
        {
            if (arg.Length == 2)
            {
                par.parString.Add(arg[1]);

                branchEvent.Add(par); //分岐をプラス
                                        //branchEvent[branchEvent.Count - 1].parInt

            return true;

            }

            return false;
        };

        OrderExec exec_order_branch = (ref int count, OrderParametor par) =>
        {
            string formale = par.parString[0];

            var v = ValuesManager.instance.Get_Values();
            bool b = new Parser(ParserType.Bool, formale).Eval(v);
        //Debug.LogFormat("count: {0}  tarCount: {1}", count, data.regInt[0]);
        if (b)
            {
                count++;
            }
            else
            {
                count = par.parInt[0] + 1;

            }
        };

        Order_Registration("BRANCH", decode_order_branch, exec_order_branch);
        Order_Registration("IF", decode_order_branch, exec_order_branch);

        //==============================================================================

        OrderDecode decode_order_branchElse = (int count, OrderParametor par, string[] arg) =>
        {
            if (arg.Length == 1)
            {
                par.parInt = branchEvent[branchEvent.Count - 1].parInt;
                branchEvent[branchEvent.Count - 1].parInt.Add(count);

                return true;
            }
            return false;
        };

        OrderExec exec_order_branchElse = (ref int count, OrderParametor par) =>
        {
            count = par.parInt[1] + 1;
        };

        Order_Registration("ELSEBRANCH", decode_order_branchElse, exec_order_branchElse);
        Order_Registration("ELSEIF", decode_order_branchElse, exec_order_branchElse);

        //==============================================================================

        OrderDecode decode_order_branchEnd = (int count, OrderParametor par, string[] arg) =>
        {
            if (arg.Length == 1)
            {
                branchEvent[branchEvent.Count - 1].parInt.Add(count);
                branchEvent.RemoveAt(branchEvent.Count - 1);

                return true;
            }
            return false;
        };

        OrderExec exec_order_branchEnd = (ref int count, OrderParametor par) =>
        {
            count++;
        };

        Order_Registration("ENDBRANCH", decode_order_branchEnd, exec_order_branchEnd);
        Order_Registration("ENDIF", decode_order_branchEnd, exec_order_branchEnd);

        Order_Registration("LABEL", //『ラベル』
            (int count, OrderParametor par, string[] arg) =>
            {
                if (arg.Length == 2)
                {
                    par.parString.Add(arg[1]);

                    return true;
                }

                return false;
            },

            (ref int count, OrderParametor par) =>
            {
                count++;
            });

        Order_Registration("GOTO", //『ラベルにskipする』
            (int count, OrderParametor par, string[] arg) =>
            {
                if (arg.Length == 2)
                {
                    par.parString.Add(arg[1]);

                    return true;
                }

                return false;
            },

            (ref int count, OrderParametor par) =>
            {
                string lable = par.parString[0];

                for (int i = 0; i < masterDatas[0].datas.Count; i++)
                {
                    var md = masterDatas[0].datas[i];

                    if (md.tType == "LABEL")
                    {
                        if (md.data.parString.Count == 1)
                        {
                            if (md.data.parString[0] == lable)
                            {
                                count = i;
                                return;
                            }
                        }
                    }
                }

                count++;
            });

        Order_Registration("Test", //『TEST』
            (int count, OrderParametor par, string[] arg) =>
            {
                if (arg.Length == 2)
                {
                    par.parString.Add(arg[1]);

                    return true;
                }

                return false;
            },

            (ref int count, OrderParametor par) =>
            {
                string s = par.parString[0];

                Debug.LogFormat("[Test] {0}", s);
                count++;
            });


        #endregion


    }

}

/// <summary>
/// イベントデータの大元データ
/// </summary>
public sealed class TalkEventMasterData
{
    public enum Mode
    {
        Normal, Concurrent
    }

    public Mode mode = Mode.Normal;
    public List<TalkEventData> datas = new List<TalkEventData>();
}

public enum Type_TalkEventData
{
    SINGLE, MULTI
}

/// <summary>
/// イベントの命令実体データ
/// </summary>
public sealed class TalkEventData
{
    public string tType;
    public OrderParametor data;
    public OrderExec action;

    public TalkEventData(string type, OrderParametor data, OrderExec action)
    {
        tType = type;
        this.data = data;
        this.action = action;
    }
    //public delegate void ActionEvent();
}

public sealed class Talk_OrderData
{
    public string tType;
    public Type_TalkEventData type;
    public OrderDecode decode;
    public OrderExec order;
}

public sealed class Savealbe_EventData
{
    public string path;
    public int cullentIndex;
}
