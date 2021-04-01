using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class Message_Button : MonoBehaviour
{
    private EventWindowManager EwManager;
    public enum State { Auto, Skip, Log, OffLog, Menu }
    public State state;

    [SerializeField]
    private Toggle toggle;

    private void Start()
    {
        EwManager = EventWindowManager.instance;
    }

    private void Update()
    {
        /*
        switch (state)
        {
            case State.Auto:
                if (EwManager.message_auto != toggle.isOn)
                {
                    toggle.isOn = EwManager.message_auto;
                }
                break;
            case State.Skip:
                if (EwManager.message_skip != toggle.isOn)
                {
                    toggle.isOn = EwManager.message_skip;
                }
                break;
        }*/
    }

    public void Auto_Button()
    {
        EwManager.message_auto = !EwManager.message_auto;
        EwManager.message_skip = false;
        //if (toggle.isOn) { toggle.isOn = false; GetComponent<Toggle>().isOn = true; }
        
        //Debug.Log("Auto");
    }

    public void Skip_Button()
    {
        
        EwManager.message_skip = !EwManager.message_skip;
        EwManager.message_auto = false;
        //if (toggle.isOn) { toggle.isOn = false; GetComponent<Toggle>().isOn = true; }

        //Debug.Log("Skip");
    }

    public void Log_Button()
    {
        //Debug.Log("Log");
        LogManager.instance.Display_Log();
    }

    public void LogOff_Button()
    {
        //Debug.Log("OFFLog");
        LogManager.instance.NoDisplay_Log();
    }

    public void Menu_Button()
    {
        Debug.Log("Menu");
    }
}
