using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;


public class MetaTextParser
{
    [HideInInspector]
    public readonly List<List<string>> MetaTextData = new List<List<string>>();

    public MetaTextParser(string metaText)
    {
        Start(metaText);
    }

    private void Start(string formale)
    {

        string _formale = Regex.Replace(formale, @"\s", "");

        Convert_Value(ref _formale);
        Convert_Value_Float(ref _formale);
        Convert_Text(ref _formale);

        Debug.LogFormat("êîílïœêîÅF{0}", _formale);


        List<string> orderList = new List<string>();
        Split_MetaOrder(orderList, _formale);
        Split_Parms(orderList);

        /*
        for (int i=0; i<MetaTextData.Count;i++)
        //foreach(var v in MetaTextData)
        {
            var v = MetaTextData[i];
            for (int l=0;l<v.Count; l++)
            //foreach(var v2 in v)
            {
                var v2 = v[l];
                Debug.LogFormat("[{0}] v={1}", i, v2);
            }
        }
        */
    }

    private void Split_MetaOrder(List<string> orderList, string formale)
    {
        StringBuilder texts = new StringBuilder("");
        bool incase = false;

        foreach (char t in formale)
        {
            switch (t)
            {
                case '<':
                    incase = true;
                    break;
                case '>':
                    orderList.Add(texts.ToString());
                    texts = new StringBuilder("");
                    incase = false;
                    break;
                default:
                    if (incase)
                    {
                        //Debug.Log(texts.ToString());
                        texts.Append(t.ToString());
                    }
                    break;
            }
        }
    }

    private void Split_Parms(List<string> orderList)
    {
        foreach(string s in orderList)
        {
            var ss = Regex.Split(s, @":|,");
            MetaTextData.Add(new List<string>(ss));
        }
    }

    private void Convert_Value(ref string _formale)
    {
        while (true)
        {
            Match match = Regex.Match(_formale, @"\\v\[\d{1,4}\]");
            if (match.Success)
            {
                Debug.Log("ê¨å˜");
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

    List<List<string>> Eval(string formale)
    {
        if (MetaTextData != null)
        {
            return MetaTextData;
        }
        return null;
    }
}

