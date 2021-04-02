using System.Collections;
using UnityEngine;

namespace FLS.FadeControll
{
    using UnityEngine.UI;

    public sealed class FadeManager : MonoBehaviour
    {
        public static FadeManager instance;

        [SerializeField]
        private Image image_mid;
        [SerializeField]
        private float change_secound = 0.6f;

        private bool onFade_Coroutine = false;
        private TalkEventManager talk;
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
            talk = TalkEventManager.instance;
            SetUp_Order();
        }

        private void SetUp_Order()
        {
            talk.Order_Registration("FADEIN", Order_FadeIn, Exec_FadeIn);
            talk.Order_Registration("FADEOUT", Order_FadeOut, Exec_FadeOut);
            talk.Order_Registration("FADE", Order_Fade, Exec_Fade);
        }


        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "FADEIN"
        private bool Order_FadeIn(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 1)
            {
                return true;
            }

            return false;
        }

        private void Exec_FadeIn(ref int count, OrderParametor par)
        {
            //Debug.Log("フェード");
            Fade(change_secound, Color.clear, 0);

            if (orderFinish)
            {
                orderFinish = false;
                //Debug.Log("フェード終わり");
                count++;
            }
        }

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "FADEOUT"
        private bool Order_FadeOut(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 1)
            {
                return true;
            }

            return false;
        }

        private void Exec_FadeOut(ref int count, OrderParametor par)
        {

            Fade(change_secound, new Color(0, 0, 0, 1), 0);

            if (orderFinish)
            {
                orderFinish = false;
                count++;
            }
        }

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "FADE time r b g r mode"
        //  "FADE_time color mode"
        private bool Order_Fade(int count, OrderParametor par, string[] arg)
        {
            if (7 == arg.Length)
            {
                par.parFloat.Add(float.Parse(arg[1]));
                float r = float.Parse(arg[2]) / 255f;
                float g = float.Parse(arg[3]) / 255f;
                float b = float.Parse(arg[4]) / 255f;
                float a = float.Parse(arg[5]) / 255f;
                par.parColor.Add(new Color(r, g, b, a));

                switch (arg[6])
                {
                    case "WAIT":
                        par.parInt.Add(0);
                        break;
                    case "NOWAIT":
                        par.parInt.Add(1);
                        break;
                }

                return true;
            }

            if (4 == arg.Length)
            {
                par.parFloat.Add(float.Parse(arg[1]));

                switch (arg[2])
                {
                    case "BLACK":
                        par.parColor.Add(Color.black);
                        break;
                    case "WHITE":
                        par.parColor.Add(Color.white);
                        break;
                }

                switch (arg[3])
                {
                    case "WAIT":
                        par.parInt.Add(0);
                        break;
                    case "NOWAIT":
                        par.parInt.Add(1);
                        break;
                }

                return true;
            }

            return false;
        }

        private void Exec_Fade(ref int count, OrderParametor par)
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
}
