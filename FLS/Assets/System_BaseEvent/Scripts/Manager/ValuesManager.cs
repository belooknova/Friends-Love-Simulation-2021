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
            );


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
            );

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

        talk.Order_Registration("VALUE_INC", Order_inc, Exec_inc);
        talk.Order_Registration("VALUE_DEC", Order_dec, Exec_dec);
    }




    /// <summary>
    /// トークイベント命令用・数値変数代入
    /// </summary>
    /// <param name="eo"></param>
    private void Oreder_set_value(ref int count, OrderParametor par)
    {
        string formale = par.parString[1];
        int index = new Parser(ParserType.Number, par.parString[0]).Eval_Value(Values);

        Parser parser = new Parser(ParserType.Number, formale);
        float v = parser.Eval_Value_outFlaot(Values);

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
    private void Order_set_text(ref int count, OrderParametor par)
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

    private void Order_set_textc(ref int count, OrderParametor par)
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

    private bool Order_inc(int count, OrderParametor par, string[] arg)
    {
        //INC_VALUE 識別番号 数値

        if (arg.Length == 3)
        {
            par.parString.Add(arg[1]);
            par.parString.Add(arg[2]);

            return true;
        }

        return false;
    }

    private void Exec_inc(ref int count, OrderParametor par)
    {
        string _index_s = par.parString[0];
        string _value_s = par.parString[1];

        IncrementValue(_index_s, _value_s);

        count++;
    }

    private bool Order_dec(int count, OrderParametor par, string[] arg)
    {
        //INC_VALUE 識別番号 数値

        if (arg.Length == 3)
        {
            par.parString.Add(arg[1]);
            par.parString.Add(arg[2]);

            return true;
        }

        return false;
    }

    private void Exec_dec(ref int count, OrderParametor par)
    {
        string _index_s = par.parString[0];
        string _value_s = par.parString[1];

        DecrementValue(_index_s, _value_s);

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
        return Set_Value(index, new Parser(ParserType.Number, value).Eval_Value_outFlaot(Values));
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

        _text = Conversion_Text(_text);
        return Set_Text(outIndex, _text);
    }

    private string Conversion_Text(string text)
    {
        string textout = text;
        {
            Match match = Regex.Match(textout, @"\\t\[\d{1,4}\]");
            if (match.Success)
            {
                int index = int.Parse(match.Value.Substring(3, match.Value.Length - 3 - 1));
                var value = ValuesManager.instance.Get_Text(index);
                textout = Regex.Replace(textout, @"\\t\[\d{1,4}\]", value);
            }
        }

        {
            Match match = Regex.Match(textout, @"\\v\[\d{1,4}\]");
            if (match.Success)
            {
                int index = int.Parse(match.Value.Substring(3, match.Value.Length - 3 - 1));
                var value = ValuesManager.instance.Get_Value(index);
                textout = Regex.Replace(textout, @"\\v\[\d{1,4}\]", value.ToString());
            }
        }

        {
            Match match = Regex.Match(textout, @"\\f\[\d{1,4}\]");
            if (match.Success)
            {
                int index = int.Parse(match.Value.Substring(3, match.Value.Length - 3 - 1));
                var value = ValuesManager.instance.Get_Value_Float(index);
                textout = Regex.Replace(textout, @"\\f\[\d{1,4}\]", value.ToString());
            }
        }

        return textout;
    }

    /// <summary>
    /// 任意の変数をインクリメントする
    /// </summary>
    /// <param name="_index_s"></param>
    /// <param name="_value_s"></param>
    public void IncrementValue(string _index_s, string _value_s)
    {
        int index = new Parser(ParserType.Number, _index_s).Eval_Value(Values);
        float value = new Parser(ParserType.Number, _value_s).Eval_Value_outFlaot(Values);

        Set_Value(index, Get_Value(index) + value);
    }

    /// <summary>
    /// 任意の変数をデクリメントする
    /// </summary>
    /// <param name="_index_s"></param>
    /// <param name="_value_s"></param>
    public void DecrementValue(string _index_s, string _value_s)
    {
        int index = new Parser(ParserType.Number, _index_s).Eval_Value(Values);
        float value = new Parser(ParserType.Number, _value_s).Eval_Value_outFlaot(Values);

        Set_Value(index, Get_Value(index) - value);
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

