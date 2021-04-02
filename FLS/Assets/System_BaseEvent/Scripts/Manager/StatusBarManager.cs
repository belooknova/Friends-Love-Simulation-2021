using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FLS.StatusBar
{
    public class StatusBarManager : MonoBehaviour
    {
        public static StatusBarManager instance;

        private TalkEventManager TeManager;

        [SerializeField]
        private Animator dayAnimator;
        [SerializeField]
        private Animator maneyAnimator;
        private ManeyTagManager tagManager;

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
            tagManager = maneyAnimator.gameObject.GetComponent<ManeyTagManager>();
        }

        private void Order_daytag(ref int count, OrderParametor par)
        {
            int mode = par.parInt[0];

            if (mode == 0)
            {
                DayTag_In();
            }
            else
            {
                DayTag_Out();
            }

            count++;
        }

        private void Order_Maneytag(ref int count, OrderParametor par)
        {
            int mode = par.parInt[0];

            if (mode == 0)
            {
                ManeyTag_In();
            }
            else
            {
                ManeyTag_Out();
            }

            count++;
        }

        public void DayTag_In()
        {
            if (dayAnimator != null)
            {
                if (dayAnimator)

                    dayAnimator.SetInteger("Stat", 2);
            }
        }

        public void DayTag_Out()
        {
            if (dayAnimator != null)
            {
                dayAnimator.SetInteger("Stat", 1);
            }
        }

        public void ManeyTag_In()
        {
            if (dayAnimator != null)
            {
                maneyAnimator.SetInteger("Stat", 2);
                tagManager.DisplayManey = true; //表示させている
            }
        }

        public void ManeyTag_Out()
        {
            if (dayAnimator != null)
            {
                maneyAnimator.SetInteger("Stat", 1);
                tagManager.DisplayManey = false; //表示させていない
            }
        }
    }
}