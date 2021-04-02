using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class FadeManager : MonoBehaviour
{
    public static FadeManager instance;

    [SerializeField]
    private Image image_mid;
    private bool onFade_Coroutine = false;
    private TalkEventManager TeManager;

    private bool orderFinish = false;

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
        TeManager = TalkEventManager.instance;
    }

    private void Order_fade(ref int count, OrderParametor par)
    {
        float seconds = par.parFloat[0];
        Color color = par.parColor[0];
        int mode = par.parInt[0];

        Fade(seconds, color, mode);

        if (orderFinish)
        {
            orderFinish = false;
            count++;
        }
    }

    public void Fade(float seconds, Color color, int mode)
    {
        if (!onFade_Coroutine)
        {
            //Debug.Log(eo);
            StartCoroutine(C_Fade(seconds, image_mid.color, color, mode));

            if (mode == 1)
            {
                orderFinish = true;
            }
        }
    }

    private IEnumerator C_Fade(float second, Color start, Color goal, int mode)
    {
        onFade_Coroutine = true;
        if (mode > 1) mode = 0;

        if (second > 0)
        {
            //Debug.Log("start: " + start);
            //Debug.Log("goal: " + goal);
            for (int i = 0; i <= second * 100; i++)
            {
                //Debug.Log("i: "+i/(second*100));
                image_mid.color = Color.Lerp(start, goal, i / (second * 100));
                yield return new WaitForSeconds(0.01f);
            }

            if (mode == 0)
            {
                orderFinish = true;
            }
        }
        else
        {
            image_mid.color = goal;
            orderFinish = true;
        }

        onFade_Coroutine = false;
    }

    public Image Get_FadeImage()
    {
        return image_mid;
    }
}
