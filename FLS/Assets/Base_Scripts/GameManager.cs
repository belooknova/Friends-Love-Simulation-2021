using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    private IEnumerator Start()
    {
        Debug.Log("[GameManager] Start");

        yield return null;

        ValuesManager.instance.Set_Value(2, 27.6f);
        ValuesManager.instance.Set_Value(3, 42);
        ValuesManager.instance.Set_Value(4, 31);
        ValuesManager.instance.Set_Text(2, "Boot");

        MetaTextParser meta = new MetaTextParser("<sssdd\\v[2]> <huronos> jur <test2: \\v[3],\\v[4],> <test1:234> <yteuhu: \\t[2], jijiji, \\f[2]>");


    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
