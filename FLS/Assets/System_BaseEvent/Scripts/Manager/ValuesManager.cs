using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

public sealed class ValuesManager : MonoBehaviour
{
    public static ValuesManager instance;

    private SaveableData data;
    private float[] Values { get{ return data.values; } }
    private string[] Texts { get{ return data.texts; } }
    private TalkEventManager TeManager;

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
        data = GameManager.instance.Get_SaveData();

        int[] vs = { 256, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
        Set_Values(Startup(513));
        string[] ts = { "愚者", "魔術師", "戦車", "女教皇", "皇帝" };
        Set_Texts(Startup_str(255));
        SetUp_Order();

    }

    private void SetUp_Order()
    {
        TalkEventManager talk = TalkEventManager.instance;

        talk.Order_Registration("VALUE_SET", //『n秒待機する』
                                            //デコード時の設定
            (int count, OrderParametor par, string[] arg) =>
            {
                if (arg.Length == 3)
                {
                    par.parString.Add(arg[1]);
                    par.parString.Add(arg[2]);

                    return true;
                }

                return false;
            },

            //実行内容
            (ref int count, OrderParametor par) => Oreder_set_value(ref count, par)
            ) ;


        talk.Order_Registration("TEXT_SET", //『n秒待機する』
            //デコード時の設定
            (int count, OrderParametor par, string[] arg) =>
            {
                if (arg.Length == 3)
                {
                    par.parString.Add(arg[1]);
                    par.parString.Add(arg[2]);

                    return true;
                }

                return false;
            },

            //実行内容
            (ref int count, OrderParametor par) => Order_set_text(ref count, par)
            ) ;

        talk.Order_Registration("TEXTC_SET", //デコード時の設定
            (int count, OrderParametor par, string[] arg) =>
            {
                if (arg.Length == 3)
                {
                    par.parInt.Add(int.Parse(arg[1]));
                    par.parInt.Add(int.Parse(arg[2]));

                    return true;
                }

                return false;
            },

            //実行内容
            (ref int count, OrderParametor par) => Order_set_textc(ref count, par)
            );

            }

    /// <summary>
    /// トークイベント命令用・数値変数代入
    /// </summary>
    /// <param name="eo"></param>
    public void Oreder_set_value(ref int count, OrderParametor par)
    {
        string formale = par.parString[1];
        int index = new Parser(ParserType.Number, par.parString[0]).Eval_Value(Values);

        Parser parser = new Parser(ParserType.Number, formale);
        float v = parser.Eval_Value(Values);

        if (!Set_Value(index, v))
        {
            Debug.LogErrorFormat("[ValusManager] 代入に失敗");
        }

        count++;
    }

    /// <summary>
    /// トークイベント命令用・文字列変数代入
    /// </summary>
    /// <param name="eo"></param>
    public void Order_set_text(ref int count, OrderParametor par)
    {
        string text = par.parString[1];
        int index = new Parser(par.parString[0]).Eval_Value(Values);

        //Debug.LogFormat("[ValusManager] {0}番の文字列変数に{1}を代入", index, text);

        if (!Set_Text(index, text))
        {
            Debug.LogErrorFormat("[ValusManager] 代入に失敗");
        }

        count++;
    }

    public void Order_set_textc(ref int count, OrderParametor par)
    {
        int oindex = par.parInt[0];
        int index = par.parInt[1];

        //Debug.LogFormat("[ValusManager] {0}番の文字列変数に{1}を代入", index, text);

        if (!Set_Text_Convert(oindex, index))
        {
            Debug.LogErrorFormat("[ValusManager] 代入に失敗");
        }

        count++;
    }

    /// <summary>
    /// 数値の初期化
    /// </summary>
    public void StartSetting(int valueMax, int stringMax)
    {
        Set_Values(Startup(valueMax));
        Set_Texts(Startup_str(stringMax));
    }


    public void Set_Values(float[] vs)
    {

        data.values = vs;
    }

    public bool Set_Value(int index, float value)
    {
        if (index < Values.Length)
        {
            Debug.LogWarningFormat("[ValusManager] {0}番の変数に{1}を代入", index, value);

            Values[index] = value;
            return true;
        }
        return false;
    }

    public bool Set_Value(int index, string value)
    {
        Parser parser = new Parser();
        parser.Start_Value(value);
        if (!parser.errorCode)
        {
            return Set_Value(index, parser.Eval_Value_outFlaot(Get_Values()));
        }

        return false;
    }

    public float[] Get_Values()
    {
        return data.values;
    }

    public int Get_Value(int index)
    {
        if (index < Get_Values().Length)
        {
            return (int)Values[index];
        }
        else
        {
            return 0;
        }
    }

    public float Get_Value_Float(int index)
    {
        if (index < Get_Values().Length)
        {
            return Values[index];
        }
        else
        {
            return 0;
        }
    }

    public void Set_Texts(string[] texts)
    {
        data.texts = texts;
    }

    public bool Set_Text(int index, string text)
    {
        if (index < Texts.Length)
        {
            Debug.LogFormat("[ValusManager] {0}番の文字列変数に{1}を代入", index, text);

            Texts[index] = text;
            return true;
        }
        return false;
    }

    public string[] Get_Texts()
    {
        return Texts;
    }

    public string Get_Text(int index)
    {
        if (index < Texts.Length)
        {
            return Texts[index];
        }
        return "";
    }

    public bool Set_Text_Convert(int inputIndex, int outIndex)
    {
        string _text = Get_Text(inputIndex);

        Convert_Value(ref _text);
        Convert_Value_Float(ref _text);
        Convert_Text(ref _text);
        return Set_Text(outIndex, _text);
    }

    private void Convert_Value(ref string _formale)
    {
        while (true)
        {
            Match match = Regex.Match(_formale, @"\\v\[\d{1,4}\]");
            if (match.Success)
            {
                Debug.Log("成功");
                StringBuilder sb = new StringBuilder(match.Value).Remove(0, 3).Remove(match.Length - 1 - 3, 1);
                int value = ValuesManager.instance.Get_Value(int.Parse(sb.ToString()));
                _formale = Regex.Replace(_formale, new StringBuilder(@"\\v\[").Append(sb.ToString()).Append(@"\]").ToString(), value.ToString());
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
            Match match = Regex.Match(_formale, @"\\t\[\d{1,4}\]");
            if (match.Success)
            {
                StringBuilder sb = new StringBuilder(match.Value).Remove(0, 3).Remove(match.Length - 1 - 3, 1);
                string value = ValuesManager.instance.Get_Text(int.Parse(sb.ToString()));
                _formale = Regex.Replace(_formale, new StringBuilder(@"\\t\[").Append(sb.ToString()).Append(@"\]").ToString(), value);
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
            Match match = Regex.Match(_formale, @"\\f\[\d{1,4}\]");
            if (match.Success)
            {
                StringBuilder sb = new StringBuilder(match.Value).Remove(0, 3).Remove(match.Length - 1 - 3, 1);
                float value = ValuesManager.instance.Get_Value_Float(int.Parse(sb.ToString()));
                _formale = Regex.Replace(_formale, new StringBuilder(@"\\f\[").Append(sb.ToString()).Append(@"\]").ToString(), value.ToString());
            }
            else
            {
                break;
            }
        }
    }

    private float[] Startup(int max)
    {
        float[] vs = new float[max];

        for(int i=0; i < vs.Length - 1; i++)
        {
            vs[i] = 0;
        }

        return vs;
    }

    private string[] Startup_str(int max)
    {
        string[] vs = new string[max];

        for (int i = 0; i < vs.Length - 1; i++)
        {
            vs[i] = "";
        }

        return vs;
    }
}

