using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameDebuger : EditorWindow
{
    private const float MIN_WINDOWSIZE_W = 420.0f;          //�E�B���h�E�T�C�Y����
    private const float MIN_WINDOWSIZE_H = 200.0f;          //�E�B���h�E�T�C�Y�c��
    private const float MAX_WINDOWSIZE_W = 840.0f;          //�E�B���h�E�T�C�Y����
    private const float MAX_WINDOWSIZE_H = 2000.0f;          //�E�B���h�E�T�C�Y�c��

    public static GameDebugSetting setting;
    private const string path = "Assets/Editor/ItemEditorSetting.asset";

    [MenuItem("Window/Editor/Create_GameDebuger")]
    public static void Create()
    {
        // �E�B���h�E��\���I
        var window = EditorWindow.GetWindow<GameDebuger>();
        window.minSize = new Vector2(MIN_WINDOWSIZE_W, MIN_WINDOWSIZE_H);
        window.maxSize = new Vector2(MAX_WINDOWSIZE_W, MAX_WINDOWSIZE_H);


        // �ݒ��ۑ�(�ǂݍ���)����p�X ScriptableObject��.asset��t���Ȃ��Ɛ������ǂ�ł���܂���

        setting = AssetDatabase.LoadAssetAtPath<GameDebugSetting>(path);

        if (setting == null)
        { // ���[�h����null�������瑶�݂��Ȃ��̂Ő���
            setting = ScriptableObject.CreateInstance<GameDebugSetting>(); // ScriptableObject��new�ł͂Ȃ�CreateInstance���g���܂�
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

        Repaint(); // Undo.RecordObject���g���Ƃ��͓��ꂽ�ق����X�V�`�悪����������



        Update_valuesCount();
    }

    private void Update_valuesCount()
    {
        //���l�ϐ��̏�����
        if (setting.values == null || setting.values.Length ==0 || setting.dumpValue.Length != setting.values.Length)
        {
            Clear_Values(setting.valueCount, ref setting.values);
        }

        //������ϐ��̏�����
        if (setting.texts == null || setting.texts.Length == 0 || setting.dumpText.Length != setting.texts.Length)
        {
            Clear_Texts(setting.textValueCount, ref setting.texts);
        }

        
        //��r�p���l�ϐ��̏�����
        if (setting.dumpValue == null || setting.dumpValue.Length == 0 || setting.dumpValue.Length != setting.values.Length)
        {
            Clear_Values(setting.valueCount, ref setting.dumpValue);
        }

        //��r�p������ϐ��̏�����
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
            Debug.Log("�`�F�b�N");
            vs = new float[count];
            for (int i = 0; i < vs.Length; i++)
            {
                vs[i] = 0;
            }
        }

        static void Clear_Texts(int count, ref string[] vs)
        {
            Debug.Log("�`�F�b�N");
            vs = new string[count];
            for (int i = 0; i < vs.Length; i++)
            {
                vs[i] = "";
            }
        }
    }

    private Vector2 lscrollPos_d; //�A�C�e���X�N���[���̈ʒu
    private Vector2 lscrollPos_s; //�A�C�e���X�N���[���̈ʒu
    private bool loadFolder = false;
    private bool itemCreateFolder = false;
    private string message_load = "";

    private void OnGUI()
    {

        string[] tabs = { "���C��", "�ϐ�", "�A�C�e��DB" ,"�ݒ�"};


        if (setting == null) return;

        //EditorGUILayout.BeginHorizontal();
        setting.tabs_index = GUILayout.Toolbar(setting.tabs_index, tabs);

        if (setting.tabs_index == 0) //���C�����j���[
        {
            EditorGUILayout.LabelField("���C�����j���[");
        }

        if (setting.tabs_index == 2)
        {
            if (GUILayout.Button("MemoryAdd"))
            {
                //setting.memorySrots__.Add(new MemorySrot());
            }
        }

        if (setting.tabs_index == 1) //�ϐ�����
        {
            ValueChackerWindow();
        }

        if (setting.tabs_index == 3) //�ݒ�
        {
            SettingWindow();
        }

        //EditorGUILayout.EndHorizontal();
    }

    bool itemSettingFolder = true;
    bool valueSettingFolder = true;

    /// <summary>
    /// �ݒ�̕`��
    /// </summary>
    private void SettingWindow()
    {
        

        valueSettingFolder = EditorGUILayout.Foldout(valueSettingFolder, "--- �ϐ����� ---", true);
        if (valueSettingFolder)
        {
            setting.folderCount = EditorGUILayout.IntField("1�t�H���_�ɑ΂���ϐ��̐�", setting.folderCount);
            if (setting.folderCount != setting.folderCount_dmp)
            {
                setting.folderCount_dmp = setting.folderCount;
                setting.foldList.Clear();
            }

            setting.memorySwitch = EditorGUILayout.Toggle("�ϐ��ۑ��@�\��L���ɂ���", setting.memorySwitch);
            if (setting.memorySwitch)
            {
                for (int i = 0; i < setting.memorySrots.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(32);
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);


                    string label = new StringBuilder(i.ToString()).Append("�X���b�g").ToString();
                    EditorGUILayout.LabelField(label, GUILayout.Width(100));
                    if (GUILayout.Button("���Z�b�g"))
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
    /// �A�C�e���ꗗ�̕`��
    /// </summary>
    private void ItemWindow()
    {
        string[] tabs_value = { "���l", "������" };

        EditorGUILayout.BeginVertical();
        /*
        EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("�ϐ�����", EditorStyles.boldLabel);
        if (setting.memorySrotCount > 0)
        {
            loadFolder = EditorGUILayout.Foldout(loadFolder, "�ۑ�/�Ăяo��");
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
    /// �ϐ�����̕`��
    /// </summary>
    private void ValueChackerWindow()
    {
        string[] tabs_value = { "���l", "������" };

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("�ϐ�����", EditorStyles.boldLabel);
        if (setting.memorySwitch)
        {
            loadFolder = EditorGUILayout.Foldout(loadFolder, "�ۑ�/�Ăяo��", true);
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
        /// ���l�ϐ�����̕`��
        /// </summary>
    private void NumberValueGroup()
    {
        if (setting.values == null || setting.values.Length == 0) return;

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUILayout.LabelField("�ԍ�", GUILayout.Width(40));
        EditorGUILayout.LabelField("|���l", GUILayout.Width(60));
        EditorGUILayout.LabelField("|����");

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
    /// ValueManager�EValueChacker�̍X�V���Ď����Ĕ��f����
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
    /// ������ϐ�����̕`��
    /// </summary>
    private void TextValueGroup()
    {
        if (setting.texts == null || setting.texts.Length == 0) return;

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUILayout.LabelField("�ԍ�", GUILayout.Width(40));
        EditorGUILayout.LabelField("|������", GUILayout.Width(150));
        EditorGUILayout.LabelField("|����");

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
    /// ValueManager�EValueChacker�̍X�V���Ď����Ĕ��f����(������)
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
    /// �σt�H���_Field
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
    /// �ϐ��̃��[�h�@�\
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
                    mark = "(��)";
                }

                string srotName = new StringBuilder("�y�X���b�g").Append(i.ToString()).Append("�z").Append(mark).ToString();

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(srotName);
                if (GUILayout.Button("�ۑ�"))
                {
                    Save_Values(i, srotName);
                }

                if (GUILayout.Button("�Ăяo��"))
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
    /// �ϐ��ۑ�
    /// </summary>
    /// <param name="i"></param>
    /// <param name="srotName"></param>
    void Save_Values(int i, string srotName)
    {
        message_load = new StringBuilder(srotName).Append("�Ɍ��݂̕ϐ���ۑ����܂���").ToString();
        setting.memorySrots[i].Set(setting.values, setting.texts);
    }

    /// <summary>
    /// �ϐ��Ăяo��
    /// </summary>
    /// <param name="i"></param>
    /// <param name="srotName"></param>
    void Load_Values(int i, string srotName)
    {
        if (setting.memorySrots[i].isSaved)
        {
            message_load = new StringBuilder(srotName).Append("�̕ϐ����Ăяo���܂���").ToString();

            if (setting.memorySrots[i].values != null && setting.memorySrots[i].values.Length != 0)
            {
                //setting.values.CopyTo(setting.memorySrots[i].values, 0);
                //setting.texts.CopyTo(setting.memorySrots[i].texts, 0);
                setting.memorySrots[i].values.CopyTo(setting.values, 0);
                setting.memorySrots[i].texts.CopyTo(setting.texts, 0);
            }
            else
            {
                message_load = "�ۑ�����Ă���f�[�^���s�K�؂ł�";
            }
        }
        else
        {
            message_load = "�f�[�^���ۑ�����Ă��܂���";
        }
    }

    /// <summary>
    /// ����Field
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
                Debug.LogFormat("{0}key�����Z�b�g", key);
            }
        }
        else
        {
            dmpMemo = EditorGUILayout.TextField(dmpMemo);
            if (dmpMemo != "")
            {
                setting.memoDict.Add(key, dmpMemo);
                Debug.LogFormat("{0}key��o�^", key);
            }
        }
    }
}
