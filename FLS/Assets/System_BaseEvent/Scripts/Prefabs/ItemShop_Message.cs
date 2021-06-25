using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class ItemShop_Message : MonoBehaviour
{
    [SerializeField]
    private Text text;
    [SerializeField]
    private CanvasGroup group;

    public void Show()
    {
        StartCoroutine(Show());
        return;

        IEnumerator Show() {
            while (group.alpha < 0.8f) {
                yield return null;
                group.alpha += 0.1f;
            }
            group.alpha = 1;
        }
    }

    public void Hide()
    {
        StartCoroutine(Hide());
        return;

        IEnumerator Hide()
        {
            while (group.alpha >= 0.2f)
            {
                yield return null;
                group.alpha -= 0.1f;
            }
            group.alpha = 0;
        }
    }

    public void Mess(string mess)
    {
        // "いらっしゃいませ"
        // "申し訳ございません\n品切れです"
        // "お買い上げ\nありがとうございます"
        // "お金が足りないようです・・・"
        // "またのご来店\nお待ちしております"

        
        text.text = Conversion_Text(mess);
    }

    /// <summary>
    /// テキストを変換
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public string Conversion_Text(string text)
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
