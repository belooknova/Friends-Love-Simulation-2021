using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManeyTagManager : MonoBehaviour
{
    [SerializeField]
    private Text text;
    [SerializeField]
    private Text textMain;

    private int maney = 0;
    private bool onCoroutine = false;

    public bool DisplayManey = false;


    private void Update()
    {


        int value = ValuesManager.instance.Get_Value(4);

        if (DisplayManey)
        {
            if (value != maney)
            {
                if (value - maney > 0) //増えた
                {
                    StartCoroutine(UpdateManey(value, maney, true));
                }
                else //減った
                {
                    StartCoroutine(UpdateManey(value, maney, false));
                }
            }
        }
        else
        {
            textMain.text = value.ToString() + "円";
        }

        maney = value;
        
    }

    private IEnumerator UpdateManey(int maney, int buff, bool plus)
    {
        //Debug.Log("お金");
        int a;

        text.gameObject.SetActive(true);
        if (plus)
        {
            a = 1;
            text.color = Color.green;
            text.text = "+" + Mathf.Abs(maney - buff).ToString();
        } else {
            a = -1;
            text.color = Color.red;
            text.text = "-" + Mathf.Abs(maney - buff).ToString();
        }

        onCoroutine = true;


        for(int i=0; i < Mathf.Abs(maney-buff); i+=Mathf.CeilToInt(Mathf.Abs(maney - buff)/10f))
        {
            textMain.text = (buff + i * a).ToString() + "円"; 

            
            yield return null;
        }
        textMain.text = maney.ToString() + "円";


        yield return new WaitForSeconds(1);
        text.gameObject.SetActive(false);

    }
}
