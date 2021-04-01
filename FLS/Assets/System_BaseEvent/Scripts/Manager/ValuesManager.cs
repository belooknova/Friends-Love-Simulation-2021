using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ValuesManager : MonoBehaviour
{
    public static ValuesManager instance;

    private float[] values;
    private string[] texts;
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

        int[] vs = { 256, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
        Set_Values(Startup(255));
        string[] ts = { "愚者", "魔術師", "戦車", "女教皇", "皇帝" };
        Set_Texts(Startup_str(64));
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
                    par.parInt.Add(int.Parse(arg[1]));
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
                    par.parInt.Add(int.Parse(arg[1]));
                    par.parString.Add(arg[2]);

                    return true;
                }

                return false;
            },

            //実行内容
            (ref int count, OrderParametor par) => Order_set_text(ref count, par)
            ) ;

    }

    /// <summary>
    /// トークイベント命令用・数値変数代入
    /// </summary>
    /// <param name="eo"></param>
    public void Oreder_set_value(ref int count, OrderParametor par)
    {
        string formale = par.parString[0];
        int index = par.parInt[0];

        Parser parser = new Parser(ParserType.Number, formale);
        float v = parser.Eval_Value(values);

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
        string text = par.parString[0];
        int index = par.parInt[0];

        //Debug.LogFormat("[ValusManager] {0}番の文字列変数に{1}を代入", index, text);

        if (!Set_Text(index, text))
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
        values = vs;
    }

    public bool Set_Value(int index, float value)
    {
        if (index < values.Length)
        {
            Debug.LogWarningFormat("[ValusManager] {0}番の変数に{1}を代入", index, value);

            values[index] = value;
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
        return values;
    }

    public int Get_Value(int index)
    {
        if (index < values.Length)
        {
            return (int)values[index];
        }
        else
        {
            return 0;
        }
    }

    public float Get_Value_Float(int index)
    {
        if (index < values.Length)
        {
            return values[index];
        }
        else
        {
            return 0;
        }
    }

    public void Set_Texts(string[] texts)
    {
        this.texts = texts;
    }

    public bool Set_Text(int index, string text)
    {
        if (index < texts.Length)
        {
            Debug.LogFormat("[ValusManager] {0}番の文字列変数に{1}を代入", index, text);

            texts[index] = text;
            return true;
        }
        return false;
    }

    public string[] Get_Texts()
    {
        return texts;
    }

    public string Get_Text(int index)
    {
        if (index < texts.Length)
        {
            return texts[index];
        }
        return "";
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

