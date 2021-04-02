using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class ConditionParser : MonoBehaviour
{
    // |[ Ver. 2 ]|

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(3<<1);


        float[] vs =
        {
            10, 40, 8, 7, 6, 5, 4, 3, 2, 1
        };
        
        string puttern = "";
        //puttern = "2 + ( 1 + [5] * ( 4 + 2 * -3 ) ) - (10 + 8 % [7] / (9))";
        //puttern = "2 * 6 + (2 * ([1] + 7)) / (7 - [8])";
        //puttern = "(2 * 4 + 5) + (7 - 2) + 2"; //20
        //puttern = "3 + 2 * 5 + 4 * 6 - 10 + 1"; //28
        //puttern = "1+(5 + 10 / 5) * (2 + (6 - 4) * 3) - 6"; //51
        //puttern = "-1";
        //puttern = "57";
        //puttern = "c(0.6)";
        //puttern = "3 << 1";

        //puttern = "(8*10 > 0+10 && 1 <= 8) && (5==5 || -6 + 6 * 8 != 6) && -[6]==-4"; //True
        //puttern = "0<=6 && 8<9 || 8!=2 ";
        //puttern = "0==0";
        //puttern = "f(40 / 6) % 6 == 0";
        //puttern = "([1]+(-5)) <= 6*10 && (0==10 || -7/[2]+2*[3] != 9)";
        //puttern = "[121] != 5";
        puttern = "18 <= f([1] / 6) && f([1] / 6) < 24";


        //var s = new Parser(ParserType.Number , puttern);
        var s = new Parser(ParserType.Bool, puttern);


        //Debug.Log(s.Eval_Value_outFlaot(vs));
        //Debug.Log(s.Eval(vs));
    }

    // Update is called once per frame
    void Update()
    {

    }
}

public enum ParserType
{
    Bool, Number, NumberToInt
}

public sealed class Parser
{
    public Parser()
    {

    }

    public Parser(string formale)
    {
        Start(formale);
    }

    public Parser(ParserType type, string formale)
    {
        if (type == ParserType.Bool)
        {
            Start(formale);
        }
        else if (type == ParserType.Number)
        {
            Start_Value(formale);
        }
    }

    Node root;
    Node backup;
    public bool errorCode = false;
    public bool isParsered = false;
    public bool isParsered_value = false;

    //◇======================◇
    //|| public使用用メソッド ||
    //◇======================◇

    public void Start(ParserType type, string formale)
    {
        if (type == ParserType.Bool)
        {
            Start(formale);
        }
        else if (type == ParserType.Number)
        {
            Start_Value(formale);
        }
    }

    /// <summary>
    /// 比較式の初期化メソッド
    /// </summary>
    /// <param name="formale"></param>
    public void Start(string formale)
    {
        root = new Node();
        isParsered = true;

        //空白文字を消す・指定文字置き換え
        string fixed_formale = ConvertFormale(EraseSpace(formale));

        //空白の場合強制的にtrue
        if (fixed_formale == "") fixed_formale = "0==0";

        //ノードを構築する
        SettingNode(root, fixed_formale);

        //Test(root, 0);
    }

    /// <summary>
    /// 数値式の初期化メソッド
    /// </summary>
    /// <param name="formale"></param>
    public void Start_Value(string formale)
    {
        root = new Node();
        isParsered_value = true;

        //空白文字を消す・指定文字置き換え
        string fixed_formale = ConvertFormale(EraseSpace(formale));

        //空白の場合強制的にtrue
        if (fixed_formale == "") fixed_formale = "0==0";

        //ノードを構築する
        SettingNode(root, fixed_formale);

        //Test(root, 0);
    }

    /// <summary>
    /// 取得メソッド(条件式)
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public bool Eval(float[] values)
    {
        if (!errorCode && isParsered && !isParsered_value)
        {
            backup = new Node(root);

            //Test("解析前", backup, 0);

            var a = Analysis_Formale(backup, values);

            //Test("解析後", backup, 2);
            //Debug.Log(a);
            //Debug.Log("Analysis: " + a);
            return bool.Parse(a);
        }

        return false;
    }

    /// <summary>
    /// 取得メソッド(数値)
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public int Eval_Value(float[] values)
    {
        if (!errorCode && !isParsered && isParsered_value)
        {
            backup = new Node(root);

            var a = Analysis_Formale(backup, values);
            //Debug.Log("Analysis: " + a);
            return (int)float.Parse(a);
        }

        return 0;
    }

    /// <summary>
    /// 取得メソッド(実数)
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public float Eval_Value_outFlaot(float[] values)
    {
        if (!errorCode && !isParsered && isParsered_value)
        {
            backup = new Node(root);

            var a = Analysis_Formale(backup, values);
            //Debug.Log("Analysis: " + a);
            return float.Parse(a);
        }

        return 0;
    }

    //◇==============◇
    //|| 解析メソッド ||
    //◇==============◇

    /// <summary>
    /// スペースを消去する
    /// </summary>
    /// <param name="test"></param>
    /// <returns></returns>
    private string EraseSpace(string test)
    {
        string outText = "";

        foreach (char c in test)
        {
            if (!Regex.IsMatch(c.ToString(), @"\s"))
            {
                outText += c.ToString();
            }
        }

        return outText;
    }

    /// <summary>
    /// 特殊な文字を入れ替える
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private string ConvertFormale(string text)
    {
        string s = text;

        s = Regex.Replace(s, @"\\rv", "-1"); //符号応急措置・互換性

        return s;
    }

    /// <summary>
    /// 式を解析する
    /// </summary>
    /// <param name="node"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    private string Analysis_Formale(Node node, float[] values)
    {
        //List<string> numbers = new List<string>(node.numbers);
        //List<string> signs = new List<string>(node.signs);
        //List<Node.NumberType> index_flags = new List<Node.NumberType>(node.index_flags);
        //List<Node> children = new List<Node>(node.children);

        if (node.numbers.Count == 1) //ひとつのみ
        {
            return ConvertNumber(node.index_flags[0], node.numbers[0], node.children, 0, values);
        }

        //Test_Anal("|[ */% 前 ]|", node.signs, node.numbers, node.children);

        //( */%)
        for (int i = 0; i < node.signs.Count; i++)
        {
            string sign = node.signs[i];

            if (sign == "*" || sign == "/" || sign == "%")
            {
                string left = ConvertNumber( node.index_flags[i], node.numbers[i], node.children, i, values);
                string right = ConvertNumber(node.index_flags[i + 1], node.numbers[i + 1], node.children, i + 1, values);

                if (node.index_flags[i] != Node.NumberType.Logical && node.index_flags[i + 1] != Node.NumberType.Logical)
                {
                    switch (sign)
                    {
                        case "*":
                            NumberPack(i, float.Parse(left) * float.Parse(right));
                            i--;
                            break;

                        case "/":
                            NumberPack(i, float.Parse(left) / float.Parse(right));
                            i--;
                            break;

                        case "%":
                            NumberPack(i, float.Parse(left) % float.Parse(right));
                            i--;
                            break;
                    }
                }
            }
        }

        //Test_Anal("|[ +- 前 ]|", node.signs, node.numbers, node.children);

        //( +-)
        for (int i = 0; i < node.signs.Count; i++)
        {
            string sign = node.signs[i];

            if (sign == "+" || sign == "-")
            {
                string left = ConvertNumber(node.index_flags[i], node.numbers[i], node.children, i, values);
                string right = ConvertNumber(node.index_flags[i + 1], node.numbers[i + 1], node.children, i + 1, values);

                if (node.index_flags[i] != Node.NumberType.Logical && node.index_flags[i + 1] != Node.NumberType.Logical)
                {
                    switch (sign)
                    {
                        case "+":
                            NumberPack(i, float.Parse(left) + float.Parse(right));
                            i--;
                            break;

                        case "-":
                            NumberPack(i, float.Parse(left) - float.Parse(right));
                            i--;
                            break;
                    }
                }
            }
        }

        //( << >>)
        for (int i = 0; i < node.signs.Count; i++)
        {
            string sign = node.signs[i];

            if (sign == "<<" || sign == ">>")
            {
                string left = ConvertNumber(node.index_flags[i], node.numbers[i], node.children, i, values);
                string right = ConvertNumber(node.index_flags[i + 1], node.numbers[i + 1], node.children, i + 1, values);

                if (node.index_flags[i] != Node.NumberType.Logical && node.index_flags[i + 1] != Node.NumberType.Logical)
                {
                    switch (sign)
                    {
                        case "<<":
                            NumberPack(i, (int)float.Parse(left) << (int)float.Parse(right));
                            i--;
                            break;

                        case ">>":
                            NumberPack(i, (int)float.Parse(left) >> (int)float.Parse(right));
                            i--;
                            break;
                    }
                }
            }
        }

        //( & | ^ >>)
        for (int i = 0; i < node.signs.Count; i++)
        {
            string sign = node.signs[i];

            if (sign == "&" || sign == "|" || sign == "^")
            {
                string left = ConvertNumber(node.index_flags[i], node.numbers[i], node.children, i, values);
                string right = ConvertNumber(node.index_flags[i + 1], node.numbers[i + 1], node.children, i + 1, values);

                if (node.index_flags[i] != Node.NumberType.Logical && node.index_flags[i + 1] != Node.NumberType.Logical)
                {
                    switch (sign)
                    {
                        case "&":
                            NumberPackToInt(i, (int)float.Parse(left) & (int)float.Parse(right));
                            i--;
                            break;

                        case "|":
                            NumberPackToInt(i, (int)float.Parse(left) | (int)float.Parse(right));
                            i--;
                            break;
                        case "^":
                            NumberPackToInt(i, (int)float.Parse(left) ^ (int)float.Parse(right));
                            i--;
                            break;
                    }
                }
            }
        }

        //Test_Anal("|[ 条件式前 ]|", node.signs, node.numbers, node.children);

        //( == != <= >= < >)
        for (int i = 0; i < node.signs.Count; i++)
        {
            string sign = node.signs[i];

            if (sign == "==" || sign == "!=" || sign == "<=" || sign == ">=" || sign == "<" || sign == ">")
            {
                string left = ConvertNumber(node.index_flags[i], node.numbers[i], node.children, i, values);
                string right = ConvertNumber(node.index_flags[i + 1], node.numbers[i + 1], node.children, i + 1, values);

                if (node.index_flags[i] != Node.NumberType.Logical && node.index_flags[i + 1] != Node.NumberType.Logical)
                {
                    switch (sign)
                    {
                        case "==":
                            LogicPack(i, float.Parse(left) == float.Parse(right));
                            i--;
                            break;
                        case "!=":
                            LogicPack(i, float.Parse(left) != float.Parse(right));
                            i--;
                            break;
                        case "<=":
                            LogicPack(i, float.Parse(left) <= float.Parse(right));
                            i--;
                            break;
                        case ">=":
                            LogicPack(i, float.Parse(left) >= float.Parse(right));
                            i--;
                            break;
                        case "<":
                            LogicPack(i, float.Parse(left) < float.Parse(right));
                            i--;
                            break;
                        case ">":
                            LogicPack(i, float.Parse(left) > float.Parse(right));
                            i--;
                            break;
                    }
                }
            }
        }

        //Test_Anal("|[ && || 前 ]|", node.signs, node.numbers, node.children);

        //( && || )
        for (int i = 0; i < node.signs.Count; i++)
        {
            string sign = node.signs[i];

            if (sign == "&&" || sign == "||")
            {
                string left = ConvertNumber(node.index_flags[i], node.numbers[i], node.children, i, values);
                string right = ConvertNumber(node.index_flags[i + 1], node.numbers[i + 1], node.children, i + 1, values);

                if (node.index_flags[i] == Node.NumberType.Logical && node.index_flags[i + 1] == Node.NumberType.Logical)
                {
                    switch (sign)
                    {
                        case "&&":
                            LogicPack(i, bool.Parse(left) && bool.Parse(right));
                            i--;
                            break;

                        case "||":
                            LogicPack(i, bool.Parse(left) || bool.Parse(right));
                            i--;
                            break;
                    }
                }
            }
        }

        //Test_Anal(node.signs, node.numbers, node.children);

        if (node.parent != null)
        {
            Node p = node.parent;
            int index = p.children.IndexOf(node);

            //Test(string.Format("< parent_index: {0} >", index), p, 0);

            p.index_flags[index] = node.index_flags[0];
        }

        return node.numbers[0];

        void NumberPack(int index, float value)
        {
            //Debug.LogFormat("{0} {1} {2} = {3}", numbers[index], signs[index], numbers[index + 1], value);

            node.numbers[index] = value.ToString();
            node.numbers.RemoveAt(index + 1);
            node.signs.RemoveAt(index);
            node.index_flags[index] = Node.NumberType.Number;
            node.index_flags.RemoveAt(index + 1);

            node.children[index] = null;
            node.children.RemoveAt(index + 1);
        }

        void NumberPackToInt(int index, int value)
        {
            node.numbers[index] = value.ToString();
            node.numbers.RemoveAt(index + 1);
            node.signs.RemoveAt(index);
            node.index_flags[index] = Node.NumberType.Number;
            node.index_flags.RemoveAt(index + 1);

            node.children[index] = null;
            node.children.RemoveAt(index + 1);
        }

        void LogicPack(int index, bool value)
        {
            //Debug.LogFormat("{0} {1} {2} = {3}", numbers[index], signs[index], numbers[index + 1], value);

            node.numbers[index] = value.ToString();
            node.numbers.RemoveAt(index + 1);
            node.signs.RemoveAt(index);
            node.index_flags[index] = Node.NumberType.Logical;
            node.index_flags.RemoveAt(index + 1);

            node.children[index] = null;
            node.children.RemoveAt(index + 1);
        }
    }

    private string ConvertNumber(Node.NumberType type, string formale, List<Node> children, int childIndex, float[] vs)
    {
        if (formale == "#")
        {
            //Debug.LogFormat("[ConvertNumber]index: {0}  count: {1}  Type: {2}",childIndex, children.Count, type);
            if (children[childIndex] != null)
            {
                string analRisult = Analysis_Formale(children[childIndex], vs);

                if (type == Node.NumberType.Floor) //切り捨て
                {
                    var v = float.Parse(analRisult);

                    return Mathf.Floor(v).ToString();
                }

                if (type == Node.NumberType.Ceil) //切り上げ
                {
                    var v = float.Parse(analRisult);

                    return Mathf.Ceil(v).ToString();
                }

                return analRisult;
            }
        }

        switch (type)
        {
            case Node.NumberType.Number:
                return formale;

            case Node.NumberType.Minus:
                return "-" + formale;

            case Node.NumberType.VariableMinus:
            case Node.NumberType.VariableDenial:
            case Node.NumberType.Variable:
                {
                    //Debug.LogFormat("[{0}]", formale);
                    int index = int.Parse(formale);
                    if (index < vs.Length)
                    {
                        if (type == Node.NumberType.Variable)
                        {
                            return vs[index].ToString();
                        }
                        else if(type == Node.NumberType.VariableMinus)
                        {
                            return (-vs[index]).ToString();
                        }
                        else if(type == Node.NumberType.VariableDenial)
                        {
                            return (~(int)vs[index]).ToString();
                        }
                    }

                    return vs[0].ToString();
                }

            case Node.NumberType.Logical:
                if (formale != "True" && formale != "False")
                {
                    return "False";
                }
                return formale;              

            case Node.NumberType.Denial:
                return (~(int)float.Parse(formale)).ToString();
            default:
                return "0";
        }

    }
    /// <summary>
    /// 式のノードを構築する
    /// </summary>
    /// <param name="node"></param>
    private void SettingNode(Node node, string formale)
    {
        //Debug.Log(formale);

        string puttern = @"(f\(|c\(|\(|\)|&&|\|\||<<|>>|==|!=|<=|>=|<|>|\+|\-|\*|/|%|&|\||\^)";
        string buffFormale = "";
        bool signFlag = false; //前の文字が符号
        int layer = 0;
        
        //リスト作成
        List<string> vs = new List<string>();
        foreach (string s1 in Regex.Split(formale, @"(f\(|c\(|\(|\)|&&|\|\||<<|>>|==|!=|<=|>=|<|>|\+|\-|\*|/|%|&|\||\^)"))
        {
            //Debug.Log("s: " + s1);
            if (s1 != "")
            {
                vs.Add(s1);
            }
        }

        for(int i=0; i<vs.Count; i++)
        {
            string s = vs[i];


            if (s == ")") //カッコ閉じ
            {
                layer--;
                if (layer == 0) //最後のカッコ
                {
                    node.numbers.Add("#");
                    //node.index_flags.Add(Node.NumberType.Number);

                    node.Add(new Node());
                    SettingNode(node.children[node.children.Count - 1], buffFormale);

                    signFlag = false;
                    buffFormale = "";
                    continue;
                }
            }

            if (layer > 0)
            {
                buffFormale += s;
            }

            if (s == "(" || s == "c(" || s == "f(") //カッコの始まり
            {
                layer++;
                switch (s)
                {
                    case "c(":
                        node.index_flags.Add(Node.NumberType.Ceil);
                        break;
                    case "f(":
                        node.index_flags.Add(Node.NumberType.Floor);
                        break;
                    default:
                        node.index_flags.Add(Node.NumberType.Number);
                        break;
                }
            }

            if (layer == 0)
            {

                //Debug.LogFormat("s: {0} flag: {1}", s, signFlag);
                if (Regex.IsMatch(s, puttern)) //符号検出
                {
                    
                    if (s == "+" || s == "-")
                    {
                        if (i == 0 || signFlag)
                        {
                            
                            buffFormale += s;
                            signFlag = false;
                            continue;
                        }
                    }

                    signFlag = true;
                    node.signs.Add(s);
                    
                }
                else
                {
                    buffFormale += s;
                    //Debug.Log("s: " + buffFormale);
                    Register();
                    signFlag = false;
                }
            }

        }


        if (buffFormale != "")
        {
            Register();
        }

        return;

        void Register()
        {
            //Debug.LogFormat("buffFormale: {0}", buffFormale);

            if (buffFormale == "") return;

            if (Regex.IsMatch(buffFormale, @"\+\[\d{1,4}\]")) //プラス変数
            {
                string num = "";
                for (int i = 2; i < buffFormale.Length - 1; i++) //[]を除く
                {
                    num += buffFormale[i];
                }
                node.index_flags.Add(Node.NumberType.Variable);
                node.numbers.Add(num);
                node.children.Add(null);
                buffFormale = "";
            }
            else

            if (Regex.IsMatch(buffFormale, @"\-\[\d{1,4}\]")) //マイナス変数
            {
                string num = "";
                for (int i = 2; i < buffFormale.Length - 1; i++) //[]を除く
                {
                    num += buffFormale[i];
                }
                node.index_flags.Add(Node.NumberType.VariableMinus);
                node.numbers.Add(num);
                node.children.Add(null);
                buffFormale = "";
            }
            else
            /*
            if (Regex.IsMatch(buffFormale, @"~\[\d{1,4}\]")) //否定変数
            {
                string num = "";
                for (int i = 2; i < buffFormale.Length - 1; i++) //[]を除く
                {
                    num += buffFormale[i];
                }
                node.index_flags.Add(Node.NumberType.VariableDenial);
                node.numbers.Add(num);
                node.children.Add(null);
                buffFormale = "";
            }
            else
            */

            if (Regex.IsMatch(buffFormale, @"\[\d{1,4}\]")) //変数
            {

                string num = "";
                for (int i = 1; i < buffFormale.Length - 1; i++) //[]を除く
                {
                    num += buffFormale[i];
                }
                node.index_flags.Add(Node.NumberType.Variable);
                node.numbers.Add(num);
                node.children.Add(null);
                buffFormale = "";
            }
            else

            if (Regex.IsMatch(buffFormale, @"\+\d{1,4}")) //プラス符号
            {
                string num = "";
                for (int i = 1; i < buffFormale.Length; i++) //+を除く
                {
                    num += buffFormale[i];
                }
                node.index_flags.Add(Node.NumberType.Number);
                node.numbers.Add(num);
                node.children.Add(null);
                buffFormale = "";
            }
            else

            if (Regex.IsMatch(buffFormale, @"\-\d{1,4}")) //マイナス符号
            {
                string num = "";
                for (int i = 1; i < buffFormale.Length; i++) //-を除く
                {
                    num += buffFormale[i];
                }
                node.index_flags.Add(Node.NumberType.Minus);
                node.numbers.Add(num);
                node.children.Add(null);
                buffFormale = "";
            }
            else

            if (Regex.IsMatch(buffFormale, @"\^\d{1,4}")) //否定符号
            {
                string num = "";
                for (int i = 1; i < buffFormale.Length; i++) //-を除く
                {
                    num += buffFormale[i];
                }
                node.index_flags.Add(Node.NumberType.Denial);
                node.numbers.Add(num);
                node.children.Add(null);
                buffFormale = "";
            }
            else
            {
                //Debug.Log(buffFormale);
                node.numbers.Add(buffFormale);
                node.index_flags.Add(Node.NumberType.Number);
                node.children.Add(null);
                buffFormale = "";
            }
        }

    }



    //◇=================◇
    //|| Debug用メソッド ||
    //◇=================◇

    /// <summary>
    /// ノードを表示する
    /// </summary>
    /// <param name="node"></param>
    /// <param name="i"></param>
    private void Test(string mess, Node node, int i)
    {
        int index = i;
        string memo = "(";

        Debug.LogFormat("{0}[{1}]Node{2}", Tab(index, 1), index, mess);

        foreach (var s in node.numbers) { memo += s + ", "; }

        Debug.LogFormat("{0}numbers: {1})", Tab(index, 0), memo);

        memo = "(";
        foreach (var s in node.signs) { memo += s + ", "; }
        Debug.LogFormat("{0}signs  : {1})", Tab(index, 0), memo);

        memo = "(";
        foreach (var s in node.index_flags) { memo += s + ", "; }
        Debug.LogFormat("{0}flag   : {1})", Tab(index, 0), memo);

        memo = "(";
        foreach (var s in node.children) { memo += (s == null ? "_" : s.ToString()) + ", "; }
        Debug.LogFormat("{0}child  : {1})", Tab(index, 0), memo);

        Debug.LogFormat("{0}=================================", Tab(index, 0));

        index++;
        foreach (var n in node.children)
        {
            if (n != null) Test(n, index);
        }

        return;

        string Tab(int _index, int type)
        {
            string tab = "";
            for (int _i = 0; _i < _index; _i++)
            {
                tab += "   ";
            }

            return tab + "|";
        }
    }

    private void Test(Node node, int i)
    {
        Test("", node, i);
    }

    private void Test_Anal(string mess, List<string> signs, List<string> numbers, List<Node> children)
    {
        Debug.LogFormat("〇======{0}======", mess);

        {
            string s = "";
            foreach (var n in signs) { s += n + ","; }
            Debug.LogFormat("|| sign: {0}", s);
        }

        {
            string s = "";
            foreach (var n in numbers) { s += n + ","; }
            Debug.LogFormat("|| num: {0}", s);
        }

        {
            string s = "";
            foreach (var n in children) { s += (n == null ? "_" : n.ToString()) + ","; }
            Debug.LogFormat("|| child: {0}", s);
        }

        Debug.LogFormat("〇============");
    }

    private void Test_Anal(List<string> signs, List<string> numbers, List<Node> children)
    {
        Test_Anal("", signs, numbers, children);
    }


    //=======================================================


    public class Node
    {
        public List<string> numbers = new List<string>();
        public List<string> signs = new List<string>();
        public List<NumberType> index_flags = new List<NumberType>();
        public Node parent;
        public List<Node> children = new List<Node>();

        public OutSign outSign = OutSign.Bool;

        public enum OutSign { Bool, Number };
        public enum NumberType
        {
            Number, Minus, Denial, //数値
            Variable, VariableMinus, VariableDenial, //変数

            Ceil, Floor, //カッコ専用

            Logical,

        };

        public Node()
        {
            numbers = new List<string>();
            signs = new List<string>();
            index_flags = new List<NumberType>();
            children = new List<Node>();
            parent = null;
        }

        public Node(Node node)
        {
            //Debug.Log(node.numbers.Count);
            numbers = new List<string>(node.numbers);
            signs = new List<string>(node.signs);
            index_flags = new List<NumberType>(node.index_flags);
            children = new List<Node>();

            foreach (Node n in node.children)
            {
                if (n != null)
                {
                    var n1 = new Node(n)
                    {
                        parent = this
                    };
                    children.Add(n1);
                }
                else
                {
                    children.Add(null);
                }
            }
        }

        public void Add(Node node)
        {
            children.Add(node);
            node.parent = this;
        }
    }
}
