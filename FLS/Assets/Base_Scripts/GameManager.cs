using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FLS.ImageObj;

public sealed class SaveableData
{
    public string cullentScene = "";
    public float[] values;
    public string[] texts;
    public List<ImageObject_Data> imageObjectList;
    public Savealbe_EventData eventData;
    //�����̃X�P�W���[��
    //�S�̂̃X�P�W���[��
} 

public sealed class GameManager : MonoBehaviour
{
    public static GameManager instance;

    /// <summary> �V�X�e���R�}���h�L���[ </summary>
    private readonly Queue<string> SystemCommandQueue = new Queue<string>();
    /// <summary> ���݂̃V�[���e�L�X�g </summary>
    private SaveableData saveData = new SaveableData();

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
    private IEnumerator Start()
    {
        Debug.Log("[GameManager] Start");

        yield return null;
        /*
        ValuesManager.instance.Set_Value(2, 27.6f);
        ValuesManager.instance.Set_Value(3, 42);
        ValuesManager.instance.Set_Value(4, 31);
        ValuesManager.instance.Set_Text(2, "Boot");

        MetaTextParser meta = new MetaTextParser("<sssdd\\v[2]> <huronos> jur <test2: \\v[3],\\v[4],> <test1:234> <yteuhu: \\t[2], jijiji, \\f[2]>");
        */

    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    #region �Z�[�u�֌W

    public SaveableData Get_SaveData()
    {
        return saveData;
    }


    #endregion
}
