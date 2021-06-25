using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FLS.AudioControll;


public class Select_Button_Prefab : MonoBehaviour
{
    [SerializeField] private Text text;
    [SerializeField] int pointFont = 32;

    /// <summary> 選択肢ID </summary>
    public int select_id = -1;

    /// <summary> 押すことが出来ない </summary>
    public bool noPushable = false;

    /// <summary> 自動選択有効 </summary>
    private bool isAuto = false;

    private RectTransform rectTransform;
    private Animator animator;
    private Image image;
    private Button button;
    private EventTrigger eventTrigger;

    /// <summary> 表示させるメッセージ </summary>
    public string displayText = "Text";

    /// <summary> 選ぶ条件 </summary>
    [HideInInspector]
    public string doCondition = "";

    /// <summary> 優先度 </summary>
    [HideInInspector]
    public string priorty = "";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        eventTrigger = GetComponent<EventTrigger>();
    }

    // Update is called once per frame
    void Update()
    {
        if (noPushable)
        {
            button.interactable = false;
        }

        if (isAuto)
        {

        }

        text.text = displayText;
        rectTransform.sizeDelta = new Vector2(text.text.Length * pointFont + 10, rectTransform.sizeDelta.y);
    }

    public void Push_Button_UI()
    {
        if (!isAuto)
        {
            Push_Button();
        }
    }

    public void Push_Button()
    {

        if (!noPushable)
        {
            AudioManager.instance.SE_Play("enter6", 1, 1);
            ValuesManager.instance.Set_Value(123, select_id + 1);
        }
        else
        {
            AudioManager.instance.SE_Play("enter8", 1, 1);
        }
    }

    
    public void Over_Button()
    {
        image.color = new Color(1, 0.5f, 0.5f);
        if (!isAuto)
            AudioManager.instance.SE_Play("enter15", 1, 0.5f);
    }

    public void Exit_Button()
    {
        image.color = new Color(1, 1, 1);
    }

    public void Set_Prefab(string mess, int index, bool isNoPush)
    {
        Set_Prefab(mess, index, isNoPush, "", "");
    }

    public void Set_Prefab(string mess, int index, bool isNoPush, string doCondition, string priorty)
    {
        displayText = mess;
        select_id = index;
        noPushable = isNoPush;
        this.doCondition = doCondition;
        this.priorty = priorty;

        if (doCondition != "" || priorty != "")
        {
            isAuto = true;
        }
    }

    public int GetID()
    {
        return select_id;
    }

    /// <summary>
    /// アニメーションを再生する
    /// </summary>
    public void PlayAnimation()
    {
        if (animator == null) animator = GetComponent<Animator>();

        animator.SetBool("playedAnime", true);
    }

    //"button_push_animation"
    /// <summary>
    /// アニメーションの状態を確認する
    /// </summary>
    /// <param name="stateName"></param>
    /// <returns></returns>
    public bool Check_AnimeState(string stateName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    public void Move_Display(float time, int i)
    {

        StartCoroutine(C_middle(time, i));
    }

    private IEnumerator C_middle(float time, int index)
    {
        rectTransform = GetComponent<RectTransform>();
        float heigth = rectTransform.sizeDelta.y + 5;

        Vector3 target = new Vector3(0, index * heigth, 0);
        transform.localPosition = target + new Vector3(500, 0, 0);

        yield return new WaitForSeconds(index * 0.1f);
        //StartCoroutine(C_Roteto(time));

        for (int i = 0; i <= time * 60; i++)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, i / (time * 60));

            yield return null;
        }

    }

    private IEnumerator C_Displaying(Vector3 target, float time)
    {
        //float rTime = Random.Range(0.5f, 1.5f);
        //Vector3 rVector = new Vector3(Random.Range(-300, 300), Random.Range(-300, 300), 0);
        transform.localPosition = new Vector3(target.x-1000, target.y, target.z);

        for (int i = 0; i < time * 60; i++)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, i / (time * 60));

            yield return null;
        }

    }

    private IEnumerator C_Roteto(float time)
    {
        Vector3 roteto = new Vector3(0 ,0 ,Random.Range(-6, 6));

        for (int i = 0; i < time * 60; i++)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(roteto), i / (time * 60));

            yield return null;
        }
    }

}
