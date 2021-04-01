using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public sealed class EventSelectManager : MonoBehaviour
{
    public static EventSelectManager instance;
    private TalkEventManager talk;

    private enum SELE_State
    {
        STANDBY, WAITING, BUTTON_EFFECT, PUSH_WAITING, NEXT, MODE_SETTING, HPUSH_WAITING,

    }

    private SELE_State SEL_state = SELE_State.WAITING;
    public GameObject CanvasObject;
    public GameObject prefab_Select;

    [SerializeField]
    private GameObject AutoSimble;

    private List<Select_Button_Prefab> select_Button_s;
    private List<string> select_mess;
    private List<OrderParametor> selectLayer = new List<OrderParametor>();

    /// <summary> 自動選択可能 </summary>
    private bool heroSelectMode = true;

    private bool onESM_Coroutine = false;
    private bool orderFinish = false;
    //private int[] testValues = {0,1,2,3,4,5,6,7,8,9 };

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
    void Start()
    {
        talk = TalkEventManager.instance;
        select_Button_s = new List<Select_Button_Prefab>();
        SetUp_Order();
    }

    private void SetUp_Order()
    {
        talk.Order_Registration("SEL_MODE", Decode_ChengeMode, Oreder_ChengeMode);
        talk.Order_Registration("SELECT_set", Decode_select_set_Update, Order_select_set_Update);
        talk.Order_Registration("SELECT", Decode_select, null, Type_TalkEventData.MULTI);
        talk.Order_Registration("SEL_ELSE", Decode_select_else, null, Type_TalkEventData.MULTI);
        talk.Order_Registration("SEL_END", Decode_select_end, null, Type_TalkEventData.MULTI);
        talk.Order_Registration("HERO_set", Decode_hero_select_set, Order_select_set_Update);
        talk.Order_Registration("HEROSEL", Decode_hero_select, null, Type_TalkEventData.MULTI);
        talk.Order_Registration("HEROSEL_ELSE", Decode_hero_select_else, null, Type_TalkEventData.MULTI);
        talk.Order_Registration("HEROSEL_END", Decode_hero_select_end, null, Type_TalkEventData.MULTI);
    }

    private bool Decode_select(int count, OrderParametor par, string[] arg)
    {
        if (arg.Length == 3 || arg.Length == 2)
        {
            var t1 = new StringBuilder("SELECT_set ").Append(arg[1]).Append(" ");

            if (arg.Length == 3)
            {
                t1.Append(arg[2]);
            }
            else
            {
                t1.Append("True");
            }

            talk.EventRegistration(t1.ToString());
            //talk.EventRegistration("IFWAIT [122]!=0");
            talk.EventRegistration("MESSOFF");
            talk.EventRegistration("IF [123]==1");

            return true;
        }

        return false;
    }

    private bool Decode_select_else(int count, OrderParametor par, string[] arg)
    {
        if (arg.Length == 3 || arg.Length == 2)
        {
            if (selectLayer.Count > 0)
            {
                talk.EventRegistration("ENDIF");

                selectLayer[selectLayer.Count - 1].parString.Add(arg[1]);

                if (arg.Length == 3)
                {
                    selectLayer[selectLayer.Count - 1].parString.Add(arg[2]);
                }
                else
                {
                    selectLayer[selectLayer.Count - 1].parString.Add("True");
                }

                int index = 0;

                if (selectLayer[selectLayer.Count - 1].parInt[0] == 0)
                {
                    index = selectLayer[selectLayer.Count - 1].parString.Count / 2;
                }
                else
                {
                    return false;
                }

                string t = new StringBuilder().Append("IF [123]==").Append(index).ToString();
                talk.EventRegistration(t);

                return true;
            }
        }

        return false;
    }

    private bool Decode_select_end(int count, OrderParametor par, string[] arg)
    {
        if (selectLayer.Count > 0)
        {
            talk.EventRegistration("ENDIF");

            selectLayer.RemoveAt(selectLayer.Count - 1);

            return true;
        }

        return false;
    }

    private bool Decode_hero_select(int count, OrderParametor par, string[] arg)
    {
        if (arg.Length == 5 || arg.Length == 4)
        {
            var t1 = new StringBuilder("HERO_set ").Append(arg[1]).Append(" ");

            if (arg.Length == 5)
            {
                t1.Append(arg[2]).Append(" ").Append(arg[3]).Append(" ").Append(arg[4]);
            }
            else
            {
                t1.Append("True").Append(" ").Append(arg[2]).Append(" ").Append(arg[3]);
            }

            talk.EventRegistration(t1.ToString());
            //talk.EventRegistration("IFWAIT [122]!=0");
            talk.EventRegistration("MESSOFF");
            talk.EventRegistration("IF [123]==1");

            return true;
        }

        return false;
    }

    private bool Decode_hero_select_else(int count, OrderParametor par, string[] arg)
    {
        if (arg.Length == 5 || arg.Length == 4)
        {
            if (selectLayer.Count > 0)
            {
                talk.EventRegistration("ENDIF");

                selectLayer[selectLayer.Count - 1].parString.Add(arg[1]);

                if (arg.Length == 5)
                {
                    selectLayer[selectLayer.Count - 1].parString.Add(arg[2]);
                    selectLayer[selectLayer.Count - 1].parString.Add(arg[3]);
                    selectLayer[selectLayer.Count - 1].parString.Add(arg[4]);
                }
                else
                {
                    selectLayer[selectLayer.Count - 1].parString.Add("True");
                    selectLayer[selectLayer.Count - 1].parString.Add(arg[2]);
                    selectLayer[selectLayer.Count - 1].parString.Add(arg[3]);
                }

                int index = 0;

                if (selectLayer[selectLayer.Count - 1].parInt[0] == 1)
                {
                    index = selectLayer[selectLayer.Count - 1].parString.Count / 4;
                }
                else
                {
                    return false;
                }

                string t = new StringBuilder().Append("IF [123]==").Append(index).ToString();
                talk.EventRegistration(t);

                return true;
            }
        }

        return false;
    }

    private bool Decode_hero_select_end(int count, OrderParametor par, string[] arg)
    {
        if (selectLayer.Count > 0)
        {
            talk.EventRegistration("ENDIF");

            selectLayer.RemoveAt(selectLayer.Count - 1);

            return true;
        }

        return false;
    }

    private bool Decode_ChengeMode(int count, OrderParametor par, string[] arg)
    {
        if (arg.Length == 2)
        {
            par.parInt.Add(int.Parse(arg[1]));
            return true;
        }

        return false;
    }

    private void Oreder_ChengeMode(ref int count, OrderParametor par)
    {
        int mode = par.parInt[0];

        if (mode == 0)
        {
            heroSelectMode = true;
        }
        else if (mode == 1)
        {
            heroSelectMode = false;
        }

        count++;
    }

    private bool Decode_hero_select_set(int count, OrderParametor par, string[] arg)
    {
        if (arg.Length == 5)
        {
            par.parInt.Add(1);
            par.parString.Add(arg[1]);
            par.parString.Add(arg[2]);
            par.parString.Add(arg[3]);
            par.parString.Add(arg[4]);

            selectLayer.Add(par);
            return true;
        }

        return false;
    }

    private void Oreder_hero_select_set(ref int count, OrderParametor par)
    {

    }

    private bool Decode_select_set_Update(int count, OrderParametor par, string[] arg)
    {
        if (arg.Length == 3)
        {
            Debug.Log("登録");
            par.parInt.Add(0);
            par.parString.Add(arg[1]);
            par.parString.Add(arg[2]);

            selectLayer.Add(par);
            return true;
        }

        return false;
    }

    private void Order_select_set_Update(ref int count, OrderParametor par)
    {
        switch (SEL_state)
        {
            case SELE_State.WAITING:
                {
                    int mode = par.parInt[0];

                    Talk_Select_Waiting(par, mode);
                }
                break;
            case SELE_State.PUSH_WAITING:
                Talk_SelectPush_Waiting();
                break;
            case SELE_State.HPUSH_WAITING:
                Talk_HeroSelect_Waiting();
                break;
            case SELE_State.BUTTON_EFFECT:
                Talk_select_Effect();
                break;
            case SELE_State.NEXT:
                SEL_Next();
                break;
        }

        if (orderFinish)
        {
            orderFinish = false;
            count++;
        }
    }

    /// <summary>
    /// トークイベント命令用・選択肢準備
    /// </summary>
    /// <param name="eo"></param>
    public void Talk_Select_Waiting(OrderParametor par, int mode)
    {
        int select_id = 0; //ID

        ValuesManager.instance.Set_Value(122, 0);
        ValuesManager.instance.Set_Value(123, 0);

        List<string> formale = new List<string>();
        List<string> condisions = new List<string>();
        List<string> prioritys = new List<string>();

        select_mess = new List<string>();

        if (mode == 1)
        {
            //自動選択の表示
            if (!AutoSimble.activeSelf)
            {
                AutoSimble.SetActive(true);
            }

            select_mess.Clear();
            
            for(int i=0; i < par.parString.Count; i++)
            {
                switch (i % 4)
                {
                    case 0:
                        select_mess.Add(par.parString[i]);
                        break;
                    case 1:
                        formale.Add(par.parString[i]);
                        break;
                    case 2:
                        condisions.Add(par.parString[i]);
                        break;
                    case 3:
                        prioritys.Add(par.parString[i]);
                        break;
                }
            }
        }
        else if(mode == 0)
        {
            for (int i = 0; i < par.parString.Count; i++)
            {
                switch (i % 2)
                {
                    case 0:
                        select_mess.Add(par.parString[i]);
                        break;
                    case 1:
                        formale.Add(par.parString[i]);
                        break;
                }
            }
        }




        for (int i = 0; i < select_mess.Count; i++)
        {
            GameObject o = Instantiate(prefab_Select);
            o.transform.SetParent(CanvasObject.transform);
            Select_Button_Prefab select = o.GetComponent<Select_Button_Prefab>();

            //条件を満たしていない選択肢はロックする
            Parser parser = new Parser(ParserType.Bool, formale[i]);
            bool pushable_flag = parser.Eval(ValuesManager.instance.Get_Values());

            //選択肢ボタンの初期設定
            if (mode == 1 && heroSelectMode)
            {
                select.Set_Prefab(select_mess[i], select_id++, !pushable_flag, condisions[i], prioritys[i]); //初期設定
            }
            else
            {
                select.Set_Prefab(select_mess[i], select_id++, !pushable_flag); //初期設定
            }

            select.Move_Display(1f, select_mess.Count - i - 1);
            select_Button_s.Add(select); //リストに設定
        }

        if (mode == 1 && heroSelectMode)
        {
            SEL_state = SELE_State.HPUSH_WAITING;
        }
        else
        {
            SEL_state = SELE_State.PUSH_WAITING;
        }
    }

    private void Talk_SelectPush_Waiting()
    {
        //Debug.Log("123: "+ ValuesManager.instance.Get_Value(123));

        if ( ValuesManager.instance.Get_Value(123) > 0)
        {
            SEL_state = SELE_State.BUTTON_EFFECT;
            int index = ValuesManager.instance.Get_Value(123);
            //Debug.Log(index);
            select_Button_s[index - 1].PlayAnimation();
        }

    }

    private void Talk_HeroSelect_Waiting()
    {
        if (!onESM_Coroutine)
        {
            StartCoroutine(C_SEL_Hero_Waiting());
        }
    }

    private IEnumerator C_SEL_Hero_Waiting()
    {
        onESM_Coroutine = true;
        yield return new WaitForSeconds(2);

        Select_Button_Prefab selectingButton = select_Button_s[0];
        List<Select_Button_Prefab> buttons = new List<Select_Button_Prefab>();  

        //条件に満たした選択肢
        foreach(var sb in select_Button_s)
        {
            string condition = sb.doCondition;
            Parser parser = new Parser(condition);

            if (parser.Eval(ValuesManager.instance.Get_Values()) && !sb.noPushable)
            {
                buttons.Add(sb);
            }
        }

        //条件に満たした選択肢から、優先値によって決定する
        float max_priorty = 0;
        foreach(var b in buttons)
        {
            Parser parser = new Parser();
            parser.Start_Value(b.priorty);
            float priorty = parser.Eval_Value_outFlaot(ValuesManager.instance.Get_Values());

            if (max_priorty == priorty)
            {
                int r = Random.Range(0, 2);
                if (r == 0)
                {
                    selectingButton = b;
                    max_priorty = priorty;
                }
            }
            else if (max_priorty < priorty)
            {
                selectingButton = b;
                max_priorty = priorty;
            }
        }


        selectingButton.Push_Button();
        selectingButton.PlayAnimation();
        SEL_state = SELE_State.BUTTON_EFFECT;
        //int index = ValuesManager.instance.Get_Value(123);

        onESM_Coroutine = false;
    }

    private void Talk_select_Effect()
    {
        int index = ValuesManager.instance.Get_Value(123);
        //Debug.Log(select_Button_s.Count);
        var sbp = select_Button_s[index-1];
        //Debug.Log("エフェクト開始");

        if (!sbp.Check_AnimeState("button_push_animation"))
        {
            while (select_Button_s.Count > 0)
            {
                var so = select_Button_s[0].gameObject;
                select_Button_s.RemoveAt(0);
                Destroy(so);
            }

            SEL_state = SELE_State.NEXT;
        }
    }

    private void SEL_Next()
    {
        int index = ValuesManager.instance.Get_Value(123);

        LogManager.instance.Add_LogSel(select_mess.ToArray(), index - 1);
        ValuesManager.instance.Set_Value(122, 1);
        //TeManager.Skip_Select(eo, index);
        Reset_SelectM();
        orderFinish = true;
    }

    /// <summary>
    /// EventSelectManagerをリセットする
    /// </summary>
    public void Reset_SelectM()
    {
        select_Button_s.Clear();
        SEL_state = SELE_State.WAITING;
        AutoSimble.SetActive(false);
    }

}
