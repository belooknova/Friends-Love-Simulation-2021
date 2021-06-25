using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace FLS.StatusBar
{

    public class ManeyTagManager : MonoBehaviour
    {
        [SerializeField]
        private Text subtext;
        [SerializeField]
        private Text textMain;

        private int maney = 0;

        public bool DisplayManey = false;

        private bool moving=false;
        [SerializeField]
        private Transform subTran;

        private void Start()
        {
        }

        private void Update()
        {
            int value = ValuesManager.instance.Get_Value((int)VariableType.Money);

            if (DisplayManey && !moving)
            {
                if (value != maney)
                {
                    if (value - maney > 0) //増えた
                    {
                        StartCoroutine(Upper(maney, value));
                        //StartCoroutine(UpdateManey(value, maney, true));
                        moving = true;
                    }
                    else //減った
                    {
                        StartCoroutine(Downer(maney, value));
                        //StartCoroutine(UpdateManey(value, maney, false));
                        moving = true;
                    }
                }
            }

            textMain.text = maney.ToString();

            if (moving)
            {
                subTran.gameObject.SetActive(true);
            }
            else
            {
                subTran.gameObject.SetActive(false);
            }

        }

        private IEnumerator Upper(int startManey, int lastManey)
        {
            moving = true;
            ViewPulsManey(lastManey - startManey);

            int m = 20;

            if (Mathf.Abs(lastManey - startManey) > 100)
            {
                for (int i = 0; i <= m; i++)
                {
                    yield return null;
                    maney += Mathf.FloorToInt(Mathf.Abs((lastManey - startManey) / (float)m));

                }
            }
            maney = lastManey;

            yield return new WaitForSeconds(1);
            moving = false;
        }

        private IEnumerator Downer(int startManey, int lastManey)
        {
            moving = true;
            ViewPulsManey(lastManey - startManey);

            int m = 20;
            if (Mathf.Abs(lastManey - startManey) > 100)
            {
                for (int i = 0; i <= m; i++)
                {
                    yield return null;
                    maney -= Mathf.FloorToInt(Mathf.Abs((lastManey - startManey) / (float)m));
                }
            }
            maney = lastManey;

            yield return new WaitForSeconds(1);
            moving = false;
        }

        private void ViewPulsManey(int value)
        {
            if (value > 0)
            {
                subtext.color = Temp.Color.PermitGreen;
                subtext.text = "+" + value.ToString();
            }
            else
            {
                subtext.color = Temp.Color.BanRed;
                subtext.text = value.ToString();
            }
        }

        public void Show()
        {
            transform.DOLocalMoveY(384, 0.2f).OnKill(()=> 
            {
                DisplayManey = true;
            });
        }

        public void Hide()
        {
            transform.DOLocalMoveY(480, 0.2f).OnKill(()=>
            {
                DisplayManey = false;
            });
        }
    }
}
