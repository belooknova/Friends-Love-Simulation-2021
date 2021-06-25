using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FLS.Dialog
{

    public class Dialog_Name_ctrl : MonoBehaviour
    {
        public TMP_InputField family;
        public TMP_InputField first;
        public GameObject button;

        private string text1 = "";
        private string text2 = "";

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (family.text != "" && first.text != "")
            {
                button.SetActive(true);
            }
            else
            {
                button.SetActive(false);
            }

            if (text1 != family.text)
            {
                ValuesManager.instance.Set_Text(1, family.text);
            }

            if (text2 != first.text)
            {
                ValuesManager.instance.Set_Text(2, first.text);
            }

            text1 = family.text;
            text2 = first.text;
        }
    }
}
