using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameDebuger : EditorWindow
{
    private const float MIN_WINDOWSIZE_W = 420.0f;          //ウィンドウサイズ横幅
    private const float MIN_WINDOWSIZE_H = 200.0f;          //ウィンドウサイズ縦幅
    private const float MAX_WINDOWSIZE_W = 840.0f;          //ウィンドウサイズ横幅
    private const float MAX_WINDOWSIZE_H = 2000.0f;          //ウィンドウサイズ縦幅

    public static GameDebugSetting setting;
    private const string path = "Assets/Editor/ItemEditorSetting.asset";

    [MenuItem("Window/Editor/Create_GameDebuger")]
    public static void Create()
    {
        // ウィンドウを表示！
        var window = EditorWindow.GetWindow<GameDebuger>();
        window.minSize = new Vector2(MIN_WINDOWSIZE_W, MIN_WINDOWSIZE_H);
        window.maxSize = new Vector2(MAX_WINDOWSIZE_W, MAX_WINDOWSIZE_H);


        // 設定を保存(読み込み)するパス ScriptableObjectは.assetを付けないと正しく読んでくれません

        setting = AssetDatabase.LoadAssetAtPath<GameDebugSetting>(path);

        if (setting == null)
        { // ロードしてnullだったら存在しないので生成
            setting = ScriptableObject.CreateInstance<GameDebugSetting>(); // ScriptableObjectはnewではなくCreateInstanceを使います
            AssetDatabase.CreateAsset(setting, path);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (setting == null)
        {
            setting = AssetDatabase.LoadAssetAtPath<GameDebugSetting>(path);
        }

        Repaint(); // Undo.RecordObjectを使うときは入れたほうが更新描画が正しく動く



        Update_valuesCount();
    }

    private void Update_valuesCount()
    {
        //数値変数の初期化
        if (setting.values == null || setting.values.Length ==0 || setting.dumpValue.Length != setting.values.Length)
        {
            Clear_Values(setting.valueCount, ref setting.values);
        }

        //文字列変数の初期化
        if (setting.texts == null || setting.texts.Length == 0 || setting.dumpText.Length != setting.texts.Length)
        {
            Clear_Texts(setting.textValueCount, ref setting.texts);
        }

        
        //比較用数値変数の初期化
        if (setting.dumpValue == null || setting.dumpValue.Length == 0 || setting.dumpValue.Length != setting.values.Length)
        {
            Clear_Values(setting.valueCount, ref setting.dumpValue);
        }

        //比較用文字列変数の初期化
        if (setting.dumpText == null || setting.dumpText.Length == 0 || setting.dumpText.Length != setting.texts.Length)
        {
            Clear_Texts(setting.textValueCount, ref setting.dumpText);
        }

        if (EditorApplication.isPlaying)
        {
            var vm = ValuesManager.instance;
            if (vm != null)
            {
                if (vm.Get_Values().Length != setting.values.Length)
                {
                    setting.valueCount = vm.Get_Values().Length;
                    Clear_Values(setting.valueCount, ref setting.values);
                }

                if (vm.Get_Texts().Length != setting.texts.Length)
                {
                    setting.textValueCount = vm.Get_Texts().Length;
                    Clear_Texts(setting.textValueCount, ref setting.texts);
                }
            }
        }

        return;
        static void Clear_Values(int count, ref float[] vs)
        {
            Debug.Log("チェック");
            vs = new float[count];
            for (int i = 0; i < vs.Length; i++)
            {
                vs[i] = 0;
            }
        }

        static void Clear_Texts(int count, ref string[] vs)
        {
            Debug.Log("チェック");
            vs = new string[count];
            for (int i = 0; i < vs.Length; i++)
            {
                vs[i] = "";
            }
        }
    }

    private Vector2 lscrollPos_d; //アイテムスクロールの位置
    private Vector2 lscrollPos_s; //アイテムスクロールの位置
    private bool loadFolder = false;
    private bool itemCreateFolder = false;
    private string message_load = "";

    private void OnGUI()
    {

        string[] tabs = { "メイン", "変数", "アイテムDB" ,"設定"};


        if (setting == null) return;

        //EditorGUILayout.BeginHorizontal();
        setting.tabs_index = GUILayout.Toolbar(setting.tabs_index, tabs);

        if (setting.tabs_index == 0) //メインメニュー
        {
            EditorGUILayout.LabelField("メインメニュー");
        }

        if (setting.tabs_index == 2)
        {
            if (GUILayout.Button("MemoryAdd"))
            {
                //setting.memorySrots__.Add(new MemorySrot());
            }
        }

        if (setting.tabs_index == 1) //変数操作
        {
            ValueChackerWindow();
        }

        if (setting.tabs_index == 3) //設定
        {
            SettingWindow();
        }

        //EditorGUILayout.EndHorizontal();
    }

    bool itemSettingFolder = true;
    bool valueSettingFolder = true;

    /// <summary>
    /// 設定の描画
    /// </summary>
    private void SettingWindow()
    {
        

        valueSettingFolder = EditorGUILayout.Foldout(valueSettingFolder, "--- 変数操作 ---", true);
        if (valueSettingFolder)
        {
            setting.folderCount = EditorGUILayout.IntField("1フォルダに対する変数の数", setting.folderCount);
            if (setting.folderCount != setting.folderCount_dmp)
            {
                setting.folderCount_dmp = setting.folderCount;
                setting.foldList.Clear();
            }

            setting.memorySwitch = EditorGUILayout.Toggle("変数保存機能を有効にする", setting.memorySwitch);
            if (setting.memorySwitch)
            {
                for (int i = 0; i < setting.memorySrots.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(32);
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);


                    string label = new StringBuilder(i.ToString()).Append("スロット").ToString();
                    EditorGUILayout.LabelField(label, GUILayout.Width(100));
                    if (GUILayout.Button("リセット"))
                    {
                        setting.memorySrots[i].Reset();
                        message_load = "";
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        EditorGUILayout.Space();

    }

    /// <summary>
    /// アイテム一覧の描画
    /// </summary>
    private void ItemWindow()
    {
        string[] tabs_value = { "数値", "文字列" };

        EditorGUILayout.BeginVertical();
        /*
        EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("変数操作", EditorStyles.boldLabel);
        if (setting.memorySrotCount > 0)
        {
            loadFolder = EditorGUILayout.Foldout(loadFolder, "保存/呼び出し");
        }
        EditorGUILayout.EndHorizontal();

        LoadFolder();

        setting.tabs_value_index = GUILayout.Toolbar(setting.tabs_value_index, tabs_value, GUILayout.Height(32));

        if (setting.tabs_value_index == 0)
        {
            NumberValueGroup();
        }

        if (setting.tabs_value_index == 1)
        {
            TextValueGroup();
        }
        */
        EditorGUILayout.EndVertical();
    }


    /// <summary>
    /// 変数操作の描画
    /// </summary>
    private void ValueChackerWindow()
    {
        string[] tabs_value = { "数値", "文字列" };

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("変数操作", EditorStyles.boldLabel);
        if (setting.memorySwitch)
        {
            loadFolder = EditorGUILayout.Foldout(loadFolder, "保存/呼び出し", true);
        }
        EditorGUILayout.EndHorizontal();

        LoadFolder();

        setting.tabs_value_index = GUILayout.Toolbar(setting.tabs_value_index, tabs_value, GUILayout.Height(32));

        if (setting.tabs_value_index == 0)
        {
            NumberValueGroup();
        }

        if (setting.tabs_value_index == 1)
        {
            TextValueGroup();
        }

        EditorGUILayout.EndVertical();
    }

    //========================
    //         NUMBER
    //========================

        /// <summary>
        /// 数値変数操作の描画
        /// </summary>
    private void NumberValueGroup()
    {
        if (setting.values == null || setting.values.Length == 0) return;

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUILayout.LabelField("番号", GUILayout.Width(40));
        EditorGUILayout.LabelField("|数値", GUILayout.Width(60));
        EditorGUILayout.LabelField("|メモ");

        EditorGUILayout.EndHorizontal();

        lscrollPos_d = EditorGUILayout.BeginScrollView(lscrollPos_d, GUI.skin.textArea);
        for (int i = 0; i < setting.valueCount; i++)
        {
            string lable = new StringBuilder("[").Append(i.ToString()).Append("]").ToString();
            string memokey = new StringBuilder("v_").Append(i).ToString();

            if (VariableFolderField(i, setting.values.Length))
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                EditorGUILayout.LabelField(lable, GUILayout.Width(40));

                var style = new GUIStyle(EditorStyles.numberField);
                
                setting.values[i] = EditorGUILayout.FloatField(setting.values[i],style, GUILayout.Width(60));
                Update_ValuesMaanager_Value(i);

                MemoField(memokey);
                EditorGUILayout.EndHorizontal();
                
            }
        }
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// ValueManager・ValueChackerの更新を監視して反映する
    /// </summary>
    /// <param name="index"></param>
    private void Update_ValuesMaanager_Value(int index)
    {
        if (EditorApplication.isPlaying)
        {
            var vm = ValuesManager.instance;
            if (vm != null)
            {
                if (setting.dumpValue[index] != setting.values[index])
                {
                    setting.dumpValue[index] = setting.values[index];
                    vm.Set_Value(index, setting.dumpValue[index]);
                }

                if (setting.dumpValue[index] != vm.Get_Value(index))
                {
                    setting.dumpValue[index] = vm.Get_Value(index);
                    setting.values[index] = setting.dumpValue[index];
                }
            }
        }
        else
        {
            setting.dumpValue[index] = setting.values[index];
        }
    }

    //=======================
    //          TEXT
    //=======================

    /// <summary>
    /// 文字列変数操作の描画
    /// </summary>
    private void TextValueGroup()
    {
        if (setting.texts == null || setting.texts.Length == 0) return;

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUILayout.LabelField("番号", GUILayout.Width(40));
        EditorGUILayout.LabelField("|文字列", GUILayout.Width(150));
        EditorGUILayout.LabelField("|メモ");

        EditorGUILayout.EndHorizontal();

        lscrollPos_s = EditorGUILayout.BeginScrollView(lscrollPos_s, GUI.skin.textArea);
        for (int i = 0; i < setting.textValueCount; i++)
        {
            string lable = new StringBuilder("[").Append(i.ToString()).Append("]").ToString();
            string memokey = new StringBuilder("t_").Append(i).ToString();

            if (VariableFolderField(i, setting.texts.Length))
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                EditorGUILayout.LabelField(lable, GUILayout.Width(40));

                var style = new GUIStyle(EditorStyles.numberField);

                setting.texts[i] = EditorGUILayout.TextField(setting.texts[i], style, GUILayout.Width(150));
                Update_ValuesMaanager_Text(i);

                MemoField(memokey);
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// ValueManager・ValueChackerの更新を監視して反映する(文字列)
    /// </summary>
    /// <param name="index"></param>
    private void Update_ValuesMaanager_Text(int index)
    {
        if (EditorApplication.isPlaying)
        {
            var vm = ValuesManager.instance;
            if (vm != null)
            {
                if (setting.dumpText[index] != setting.texts[index])
                {
                    setting.dumpText[index] = setting.texts[index];
                    vm.Set_Text(index, setting.dumpText[index]);
                }

                if (setting.dumpText[index] != vm.Get_Text(index))
                {
                    setting.dumpText[index] = vm.Get_Text(index);
                    setting.texts[index] = setting.dumpText[index];
                }
            }
        }
        else
        {
            setting.dumpValue[index] = setting.values[index];
        }
    }

    //=======================
    //         BATH
    //=======================

    /// <summary>
    /// 可変フォルダField
    /// </summary>
    /// <param name="index"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    private bool VariableFolderField(int index, int maxLength)
    {
        if (setting.folderCount == 0) return true;

        int key = Mathf.FloorToInt(index / setting.folderCount);
        
        if (setting.foldList.Count <= key)
        {
            setting.foldList.Add(true);
        }

        int minValue = key * setting.folderCount;
        int maxValue = key * setting.folderCount + setting.folderCount - 1;
        if (maxValue >= maxLength) maxValue = maxLength - 1;
        string lable = new StringBuilder(minValue.ToString()).Append("-").Append(maxValue.ToString()).ToString();

        if (index % setting.folderCount == 0)
        {
            EditorGUILayout.BeginVertical(GUI.skin.button);
            setting.foldList[key] = EditorGUILayout.Foldout(setting.foldList[key], lable, true);
            EditorGUILayout.EndVertical();
        }


        return setting.foldList[key];
    }

    /// <summary>
    /// 変数のロード機能
    /// </summary>
    private void LoadFolder()
    {
        if (!setting.memorySwitch) return;

        if (loadFolder)
        {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);
            if (message_load != "")
            {
                EditorGUILayout.LabelField(message_load, EditorStyles.colorField);
            }
            EditorGUILayout.Space();

            //Debug.Log(setting.memorySrots.Count);
            for (int i = 0; i < setting.memorySrots.Count; i++) {
                var list = setting.memorySrots;
                string mark = "";
                if (list[i].isSaved)
                {
                    mark = "(済)";
                }

                string srotName = new StringBuilder("【スロット").Append(i.ToString()).Append("】").Append(mark).ToString();

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(srotName);
                if (GUILayout.Button("保存"))
                {
                    Save_Values(i, srotName);
                }

                if (GUILayout.Button("呼び出し"))
                {
                    Load_Values(i, srotName);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// 変数保存
    /// </summary>
    /// <param name="i"></param>
    /// <param name="srotName"></param>
    void Save_Values(int i, string srotName)
    {
        message_load = new StringBuilder(srotName).Append("に現在の変数を保存しました").ToString();
        setting.memorySrots[i].Set(setting.values, setting.texts);
    }

    /// <summary>
    /// 変数呼び出し
    /// </summary>
    /// <param name="i"></param>
    /// <param name="srotName"></param>
    void Load_Values(int i, string srotName)
    {
        if (setting.memorySrots[i].isSaved)
        {
            message_load = new StringBuilder(srotName).Append("の変数を呼び出しました").ToString();

            if (setting.memorySrots[i].values != null && setting.memorySrots[i].values.Length != 0)
            {
                //setting.values.CopyTo(setting.memorySrots[i].values, 0);
                //setting.texts.CopyTo(setting.memorySrots[i].texts, 0);
                setting.memorySrots[i].values.CopyTo(setting.values, 0);
                setting.memorySrots[i].texts.CopyTo(setting.texts, 0);
            }
            else
            {
                message_load = "保存されているデータが不適切です";
            }
        }
        else
        {
            message_load = "データが保存されていません";
        }
    }

    /// <summary>
    /// メモField
    /// </summary>
    /// <param name="key"></param>
    private void MemoField(string key)
    {
        string dmpMemo = "";

        if (setting.memoDict.ContainsKey(key))
        {
            setting.memoDict[key] = EditorGUILayout.TextField(setting.memoDict[key]);
            if(setting.memoDict[key] == "")
            {
                setting.memoDict.Remove(key);
                Debug.LogFormat("{0}keyをリセット", key);
            }
        }
        else
        {
            dmpMemo = EditorGUILayout.TextField(dmpMemo);
            if (dmpMemo != "")
            {
                setting.memoDict.Add(key, dmpMemo);
                Debug.LogFormat("{0}keyを登録", key);
            }
        }
    }
}
