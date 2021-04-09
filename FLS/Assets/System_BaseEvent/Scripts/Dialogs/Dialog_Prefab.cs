using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;
using FLS.Message;

public class Dialog_Prefab : MonoBehaviour
{
    public GameObject[] button_objects;
    public Text mainText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Set()
    {

    }

    public void Set_Default_Dialog(string b1, string b2, string text)
    {
        int mode = 0;
        Set_Text(text);

        if (b1 == "") { mode++; }
        if (b2 == "") { mode++; }

        if (mode == 0) //ボタン二つ
        {
            button_objects[0].GetComponentInChildren<Text>().text = EventWindowManager.ReplaceCode_Dialog(b1);
            button_objects[1].GetComponentInChildren<Text>().text = EventWindowManager.ReplaceCode_Dialog(b2);
        }
        else if (mode == 1) //ボタン一つ
        {
            if (b1 != "")
            {
                button_objects[0].GetComponentInChildren<Text>().text = EventWindowManager.ReplaceCode_Dialog(b1);
                button_objects[1].SetActive(false);
            }

            if (b2 != "")
            {
                button_objects[0].GetComponentInChildren<Text>().text = EventWindowManager.ReplaceCode_Dialog(b2);
                button_objects[1].SetActive(false);
            }
        }
    }

    private void Set_Text(string text)
    {
        string t = text;
        Convert_Char(ref t);
        Convert_Value(ref t);
        Convert_Value_Float(ref t);
        Convert_Text(ref t);

        if (t == "NONE")
        {
            mainText.transform.gameObject.SetActive(false);
        }

        mainText.text = t;
    }

    private void Convert_Char(ref string _formale)
    {
        while (true)
        {
            Match match = Regex.Match(_formale, @"\\n");
            if (match.Success)
            {
                //_formale = Regex.Replace(_formale, new StringBuilder(@"\\v\[").Append(sb.ToString()).Append(@"\]").ToString(), value.ToString());
                _formale = Regex.Replace(_formale, @"\\n", '\n'.ToString());

            }
            else
            {
                break;
            }
        }
    }

    private void Convert_Value(ref string _formale)
    {
        while (true)
        {
            Match match = Regex.Match(_formale, @"\\v\[\d{1,4}\]|<v=\d{1,4}>");
            if (match.Success)
            {
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
}
