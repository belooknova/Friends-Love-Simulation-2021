using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayTagManager : MonoBehaviour
{
    [SerializeField]
    private Text text;

    [SerializeField]
    private Text text2;

    [SerializeField]
    private Text text3;

    private int dayMemory=0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Update_Text();

        //Update_Display();

    }

    private void Update_Text()
    {
        int time = ValuesManager.instance.Get_Value(1);

        if (dayMemory != time)
        {

            int mouth = 7;
            int day = 1 + time / 6;
            int dayweek = day % 7;
            string dayweek_s = "";
            string time_s = "";

            switch (dayweek)
            {
                case 0:
                    dayweek_s = "Sunday";
                    break;
                case 1:
                    dayweek_s = "Monday";
                    break;
                case 2:
                    dayweek_s = "Tuseday";
                    break;
                case 3:
                    dayweek_s = "Wednesday";
                    break;
                case 4:
                    dayweek_s = "Thursday";
                    break;
                case 5:
                    dayweek_s = "Fryday";
                    break;
                case 6:
                    dayweek_s = "Saturday";
                    break;
            }

            switch (time % 6)
            {
                case 0:
                    time_s = "朝";
                    break;
                case 1:
                    time_s = "休み時間";
                    break;
                case 2:
                    time_s = "昼";
                    break;
                case 3:
                    time_s = "午後";
                    break;
                case 4:
                    time_s = "放課後";
                    break;
                case 5:
                    time_s = "夜";
                    break;
            }

            text.text = string.Format("{0} / {1}", mouth, day);
            text2.text = dayweek_s;
            text3.text = time_s;

            dayMemory = time;
        }
    }

}
