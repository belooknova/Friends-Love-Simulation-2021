using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;

namespace FLS.Dialog
{

    public class Dialog_Prefab : MonoBehaviour
    {
        public GameObject[] button_objects;
        public TextMeshProUGUI mainText;

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
                button_objects[0].GetComponentInChildren<TextMeshProUGUI>().text = Conversion_Text(b1);
                button_objects[1].GetComponentInChildren<TextMeshProUGUI>().text = Conversion_Text(b2);
            }
            else if (mode == 1) //ボタン一つ
            {
                if (b1 != "")
                {
                    button_objects[0].GetComponentInChildren<TextMeshProUGUI>().text = Conversion_Text(b1);
                    button_objects[1].SetActive(false);
                }

                if (b2 != "")
                {
                    button_objects[0].GetComponentInChildren<TextMeshProUGUI>().text = Conversion_Text(b2);
                    button_objects[1].SetActive(false);
                }
            }
        }

        private void Set_Text(string text)
        {
            string t = text;
            t = Conversion_Text(t);

            if (t == "NONE")
            {
                mainText.transform.gameObject.SetActive(false);
            }

            mainText.text = t;
        }

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
    }
}