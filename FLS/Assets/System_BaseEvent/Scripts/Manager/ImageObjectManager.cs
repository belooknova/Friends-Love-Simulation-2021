using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Manager.Image
{
    public class ImageObjectManager : MonoBehaviour
    {
        public static ImageObjectManager instance;

        [SerializeField]
        private string basePath = "Img/";
        [SerializeField]
        private GameObject prefab_Image;
        [SerializeField]
        private Transform canvas;

        private TalkEventManager talk;
        private readonly Dictionary<string, ImageObject_Data> dict_Imagedata = new Dictionary<string, ImageObject_Data>();

        private ImageObject_Prefab cullent = null;
        private ImageObject_Data poolData;

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
            talk.Order_Registration("IMGO_SET",     Order_Set_ImageObject,      Exec_Set_ImageObject);
            talk.Order_Registration("IMGO_REMOVE",  Order_Remove_ImageObject,   Exec_Remove_ImageObject);
            talk.Order_Registration("IMGO_MOVE",    Order_Move_ImageObject,     Exec_Move_ImageObject);
            talk.Order_Registration("IMGO_ROT",     Order_Rot_ImageObject,      Exec_Rot_ImageObject);
            talk.Order_Registration("IMGO_SIZE",    Order_Size_ImageObject,     Exec_Size_ImageObject);
            talk.Order_Registration("IMGO_COLOR",   Order_Color_ImageObject,    Exec_Color_ImageObject);
            talk.Order_Registration("IMGO_CHANGE",  Order_Sprite_ImageObject,   Exec_Sprite_ImageObject);

            talk.Order_Registration("IMGO_VAL", Order_Value_ImageObject, Exec_Value_ImageObject);
            talk.Order_Registration("IMGO_VAL_POSITION", Order_V_Position_ImageObject, Exec_V_Position_ImageObject);
            talk.Order_Registration("IMGO_VAL_ROTATION", Order_V_Rotation_ImageObject, Exec_V_Rotation_ImageObject);
            talk.Order_Registration("IMGO_VAL_SCALE", Order_V_Scale_ImageObject, Exec_V_Scale_ImageObject);
            talk.Order_Registration("IMGO_VAL_COLOR", Order_V_Color_ImageObject, Exec_V_Color_ImageObject);
            talk.Order_Registration("IMGO_VAL_PATH", Order_V_Path_ImageObject, Exec_V_Path_ImageObject);

            talk.Order_Registration("IMGO_MULTI_MOVE", Order_MultiMove_ImageObject, Exec_MultiMove_ImageObject);
        }

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_SET name path"
        //  "IMGO_SET name"
        private bool Order_Set_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 3)
            {
                par.parString.Add(arg[1]);
                par.parString.Add(arg[2]);

                return true;
            }

            if (arg.Length == 2)
            {
                par.parString.Add(arg[1]);
                par.parString.Add("NONE");

                return true;
            }

            return false;
        }

        private void Exec_Set_ImageObject(ref int count, OrderParametor par)
        {
            string c_name = par.parString[0];
            string path = par.parString[1];

            Set_ImageObject(c_name, path);

            count++;
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_REMOVE name"
        private bool Order_Remove_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 2)
            {
                par.parString.Add(arg[1]);
                return true;
            }
            return false;
        }

        private void Exec_Remove_ImageObject(ref int count, OrderParametor par)
        {
            string c_name = par.parString[0];
            Remove_ImageObject(c_name);

            count++;
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_MOVE name x y z time mode"
        //mode: ADD_WAIT, ADD_NOWAIT, TAR_WAIT, TAR_NOWAIT
        private bool Order_Move_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 7)
            {
                par.parString.Add(arg[1]);
                par.parVector.Add(new Vector3(float.Parse(arg[2]), float.Parse(arg[3]), float.Parse(arg[4])));
                par.parFloat.Add(float.Parse(arg[5]));
                par.parString.Add(arg[6]);

                par.parInt.Add(0);

                return true;
            }
            return false;
        }

        private void Exec_Move_ImageObject(ref int count, OrderParametor par)
        {
            if (par.parInt[0] == 0) //最初のフレーム
            {
                if (Move())
                {
                    par.parInt[0] = 1;
                }
                else
                {
                    count++;
                }
            }

            if (par.parInt[0] != 0)
            {
                var prefab = cullent.data.Get_Prefab();

                if (!prefab.onAction)
                {
                    count++;
                }
            }

            return; 

            bool Move()
            {
                string lable = par.parString[0];
                if (dict_Imagedata.ContainsKey(lable)) //登録されている
                {
                    var _data = dict_Imagedata[lable];
                    bool displayed = _data.displayed;

                    if (displayed) //画像表示済み
                    {
                        string mode = par.parString[1];
                        Vector3 value = par.parVector[0];
                        float time = par.parFloat[0];

                        if (Move_ImageObject(lable, value, time, mode))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_ROT name x y z time mode"
        //mode: ADD_WAIT, ADD_NOWAIT, TAR_WAIT, TAR_NOWAIT
        private bool Order_Rot_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 7)
            {
                par.parString.Add(arg[1]);
                par.parVector.Add(new Vector3(float.Parse(arg[2]), float.Parse(arg[3]), float.Parse(arg[4])));
                par.parFloat.Add(float.Parse(arg[5]));
                par.parString.Add(arg[6]);

                par.parInt.Add(0);

                return true;
            }
            return false;
        }

        private void Exec_Rot_ImageObject(ref int count, OrderParametor par)
        {
            if (par.parInt[0] == 0) //最初のフレーム
            {
                if (Rot())
                {
                    par.parInt[0] = 1;
                }
                else
                {
                    count++;
                }
            }

            if (par.parInt[0] != 0)
            {
                var prefab = cullent.data.Get_Prefab();
                if (!prefab.onAction)
                {
                    count++;
                }
            }

            return;

            bool Rot()
            {
                string lable = par.parString[0];
                if (dict_Imagedata.ContainsKey(lable)) //登録されている
                {
                    var _data = dict_Imagedata[lable];
                    bool displayed = _data.displayed;

                    if (displayed) //画像表示済み
                    {
                        string mode = par.parString[1];
                        Vector3 value = par.parVector[0];
                        float time = par.parFloat[0];

                        Rot_ImageObject(lable, value, time, mode);
                        return true;
                    }
                }
                return false;
            }
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_SIZE name x y z time mode"
        //mode: ADD_WAIT, ADD_NOWAIT, TAR_WAIT, TAR_NOWAIT
        private bool Order_Size_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 7)
            {
                par.parString.Add(arg[1]);
                par.parVector.Add(new Vector3(float.Parse(arg[2]), float.Parse(arg[3]), float.Parse(arg[4])));
                par.parFloat.Add(float.Parse(arg[5]));
                par.parString.Add(arg[6]);

                par.parInt.Add(0);

                return true;
            }
            return false;
        }

        private void Exec_Size_ImageObject(ref int count, OrderParametor par)
        {
            if (par.parInt[0] == 0) //最初のフレーム
            {
                if (Size())
                {
                    par.parInt[0] = 1;
                }
                else
                {
                    count++;
                }
            }

            if (par.parInt[0] != 0)
            {
                var prefab = cullent.data.Get_Prefab();
                if (!prefab.onAction)
                {
                    count++;
                }
            }

            return;

            bool Size()
            {
                string lable = par.parString[0];
                if (dict_Imagedata.ContainsKey(lable)) //登録されている
                {
                    var _data = dict_Imagedata[lable];
                    bool displayed = _data.displayed;

                    if (displayed) //画像表示済み
                    {
                        string mode = par.parString[1];
                        Vector3 value = par.parVector[0];
                        float time = par.parFloat[0];

                        Size_ImageObject(lable, value, time, mode);
                        return true;
                    }
                }
                return false;
            }
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_COLOR name r g b a time mode" rgba = 0~255
        //  "IMGO_COLOR name FFFFFFFF time mode" 
        //mode: WAIT, NOWAIT
        private bool Order_Color_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 8)
            {
                par.parString.Add(arg[1]);

                float r = float.Parse(arg[2]) / 255f;
                float g = float.Parse(arg[3]) / 255f;
                float b = float.Parse(arg[4]) / 255f;
                float a = float.Parse(arg[5]) / 255f;
                
                par.parColor.Add(new Color(r, g, b, a));
                par.parFloat.Add(float.Parse(arg[6]));
                par.parString.Add(arg[7]);

                par.parInt.Add(0);

                return true;
            }

            return false;
        }

        private void Exec_Color_ImageObject(ref int count, OrderParametor par)
        {
            if (par.parInt[0] == 0) //最初のフレーム
            {
                if (Color())
                {
                    par.parInt[0] = 1;
                }
                else
                {
                    count++;
                }
            }

            if (par.parInt[0] != 0)
            {
                var prefab = cullent.data.Get_Prefab();

                //Debug.Log("Colorループ、onAction判定前");
                if (!prefab.onAction)
                {
                    count++;
                }
            }

            return;

            bool Color()
            {
                string lable = par.parString[0];
                if (dict_Imagedata.ContainsKey(lable)) //登録されている
                {
                    var _data = dict_Imagedata[lable];
                    bool displayed = _data.displayed;

                    if (displayed) //画像表示済み
                    {
                        string mode = par.parString[1];
                        Color color = par.parColor[0];
                        float time = par.parFloat[0];

                        Debug.LogFormat("lable: {0}  color: {1}  time: {2}  mode: {3}", lable, color, time, mode);
                        Color_ImageObject(lable, color, time, mode);
                        return true;
                    }
                }
                return false;
            }
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_CHANGE name newpath time mode"
        //mode: WAIT, NOWAIT
        private bool Order_Sprite_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 5)
            {
                par.parString.Add(arg[1]);
                par.parString.Add(arg[2]);
                par.parFloat.Add(float.Parse(arg[3]));
                par.parString.Add(arg[4]);

                par.parInt.Add(0);

                return true;
            }
            return false;
        }

        private void Exec_Sprite_ImageObject(ref int count, OrderParametor par)
        {
            if (par.parInt[0] == 0) //最初のフレーム
            {
                if (ChangeSprite())
                {
                    par.parInt[0] = 1;
                }
                else
                {
                    count++;
                }
            }

            if (par.parInt[0] != 0)
            {
                var prefab = cullent.data.Get_Prefab();
                if (!prefab.onAction)
                {
                    count++;
                }
            }

            return;

            bool ChangeSprite()
            {
                string lable = par.parString[0];
                if (dict_Imagedata.ContainsKey(lable)) //登録されている
                {
                    var _data = dict_Imagedata[lable];
                    bool displayed = _data.displayed;

                    if (displayed) //画像表示済み
                    {
                        string mode = par.parString[2];
                        string path = par.parString[1];
                        float time = par.parFloat[0];

                        Sprite_ImageObject(lable, path, time, mode);
                        return true;
                    }
                }
                return false;
            }
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_VALUE name path"
        private bool Order_Value_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 3)
            {
                par.parString.Add(arg[1]);
                par.parString.Add(arg[2]);

                return true;
            }

            return false;
        }

        private void Exec_Value_ImageObject(ref int count, OrderParametor par)
        {
            string c_name = par.parString[0];
            string path = par.parString[1];

            Value_ImageObject(c_name, path);

            count++;
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_VALUE_POSITION name x y z"
        private bool Order_V_Position_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 5)
            {
                par.parString.Add(arg[1]);
                par.parVector.Add(new Vector3(float.Parse(arg[2]), float.Parse(arg[3]), float.Parse(arg[4])));

                return true;
            }

            return false;
        }

        private void Exec_V_Position_ImageObject(ref int count, OrderParametor par)
        {
            string c_name = par.parString[0];
            Vector3 vector = par.parVector[0];

            V_Position_ImageObject(c_name, vector);

            count++;
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_VALUE_ROTATION name x y z"
        private bool Order_V_Rotation_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 5)
            {
                par.parString.Add(arg[1]);
                par.parVector.Add(new Vector3(float.Parse(arg[2]), float.Parse(arg[3]), float.Parse(arg[4])));

                return true;
            }

            return false;
        }

        private void Exec_V_Rotation_ImageObject(ref int count, OrderParametor par)
        {
            string c_name = par.parString[0];
            Vector3 vector = par.parVector[0];

            V_Rotation_ImageObject(c_name, vector);

            count++;
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_VALUE_SCALE name x y z"
        private bool Order_V_Scale_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 5)
            {
                par.parString.Add(arg[1]);
                par.parVector.Add(new Vector3(float.Parse(arg[2]), float.Parse(arg[3]), float.Parse(arg[4])));

                return true;
            }

            return false;
        }

        private void Exec_V_Scale_ImageObject(ref int count, OrderParametor par)
        {
            string c_name = par.parString[0];
            Vector3 vector = par.parVector[0];

            V_Scale_ImageObject(c_name, vector);

            count++;
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_VALUE_COLOR name r g b a"
        private bool Order_V_Color_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 6)
            {
                par.parString.Add(arg[1]);
                float r = float.Parse(arg[2]) / 255f;
                float g = float.Parse(arg[3]) / 255f;
                float b = float.Parse(arg[4]) / 255f;
                float a = float.Parse(arg[5]) / 255f;

                par.parColor.Add(new Color(r, g, b, a));

                return true;
            }

            return false;
        }

        private void Exec_V_Color_ImageObject(ref int count, OrderParametor par)
        {
            string c_name = par.parString[0];
            Color color = par.parColor[0];

            V_Color_ImageObject(c_name, color);

            count++;
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_VALUE_PATH name newPath"
        private bool Order_V_Path_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 3)
            {
                par.parString.Add(arg[1]);
                par.parString.Add(arg[2]);

                return true;
            }

            return false;
        }

        private void Exec_V_Path_ImageObject(ref int count, OrderParametor par)
        {
            string c_name = par.parString[0];
            string newPath = par.parString[1];

            V_Path_ImageObject(c_name, newPath);

            count++;
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*
        //  "IMGO_MULTI_MOVE names x y time mode"
        //  "IMGO_MULTI_MOVE names mul_value x y z time mode"
        //mode: ADD_WAIT, ADD_NOWAIT, TAR_WAIT, TAR_NOWAIT
        //names: ex... "test1 & test2 & test4"
        //mul_value: ex... "1 & 1.5 & 1" (namesと対応する)

        private bool Order_MultiMove_ImageObject(int count, OrderParametor par, string[] arg)
        {
            if (arg.Length == 6)
            {
                par.parString.Add(arg[1]);
                //par.parVector.Add(new Vector3(float.Parse(arg[2]), float.Parse(arg[3]), float.Parse(arg[4])));
                
                par.parFloat.Add(float.Parse(arg[4]));
                par.parString.Add(arg[5]);

                par.parInt.Add(0);
                par.parString.Add("");
                par.parFloat.Add(float.Parse(arg[2]));
                par.parFloat.Add(float.Parse(arg[3]));

                return true;
            }

            if (arg.Length == 7)
            {
                par.parString.Add(arg[1]);
                //par.parVector.Add(new Vector3(float.Parse(arg[3]), float.Parse(arg[4]), float.Parse(arg[5])));
                par.parFloat.Add(float.Parse(arg[5]));
                par.parString.Add(arg[6]);

                par.parInt.Add(0);
                par.parString.Add(arg[2]);
                par.parFloat.Add(float.Parse(arg[3]));
                par.parFloat.Add(float.Parse(arg[4]));

                return true;
            }

            return false;
        }

        private void Exec_MultiMove_ImageObject(ref int count, OrderParametor par)
        {
            if (par.parInt[0] == 0) //最初のフレーム
            {
                if (Move())
                {
                    par.parInt[0] = 1;
                }
                else
                {
                    count++;
                }
            }

            if (par.parInt[0] != 0)
            {
                var prefab = cullent.data.Get_Prefab();

                if (!prefab.onAction)
                {
                    count++;
                }
            }

            return;

            bool Move()
            {
                int suc_count = 0;
                string lable_name = par.parString[0];
                string[] names = Split_Names(lable_name);
                string[] mulv = { };

                bool custom = false;
                if (par.parString[2] != "") //設定なし
                {
                    mulv = Split_Names(par.parString[2]);

                    //Debug.LogFormat("{0} == {1}", names.Length, mulv.Length);
                    if (names.Length == mulv.Length)
                    {
                        custom = true;
                    }
                }

                //Debug.Log("ループ");
                for (int i = 0; i < names.Length; i++)
                {
                    string lable = names[i];
 
                    if (dict_Imagedata.ContainsKey(lable)) //登録されている
                    {
                        //Debug.Log("登録されている");

                        var _data = dict_Imagedata[lable];
                        bool displayed = _data.displayed;

                        if (displayed) //画像表示済み
                        {
                            //Debug.Log("画像表示済み");

                            string mode = par.parString[1];
                            float time = par.parFloat[0];
                            float z = _data.postion.z;

                            Vector3 value = new Vector3(par.parFloat[1], par.parFloat[2], z);

                            if (custom)
                            {
                                value *= float.Parse(mulv[i]);
                            }

                            //Debug.Log("移動");
                            Move_ImageObject(lable, value, time, mode, true);
                            suc_count++;
                        }
                    }
                }

                if (suc_count > 0) return true;

                return false;
            }
        }
        //===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*===*

        private string[] Split_Names(string names)
        {
            return Regex.Replace(names, @"\s", "").Split('&');
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 画像オブジェクトを設置
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="spritePath"></param>
        public void Set_ImageObject(string configPath, string spritePath)
        {
            //Debug.Log("画像オブジェクト設定");
            ImageObject_Data _Data;

            if (dict_Imagedata.ContainsKey(configPath))
            {
                //Debug.Log("ある");
                _Data = dict_Imagedata[configPath];
            }
            else
            {
                if (spritePath == "NONE" || Regex.IsMatch(spritePath, @"\s"))
                {
                    return;
                }

                //Debug.Log("ない");
                _Data = new ImageObject_Data(configPath, spritePath);
                dict_Imagedata.Add(configPath, _Data);
            }

            //表示設定
            _Data.displayed = true;

            /*
            Debug.LogFormat("オブジェクト名：{0}" +
                "【登録data】label: {1} path: {2} d: {3}\n" +
                "[position: {4}] [rotation: {5}] [scale: {6}] [color: {7}]"
                , gameObject.name, _Data.label, _Data.path, _Data.displayed
                , _Data.postion, _Data.rotation, _Data.scale, _Data.color);
            */

            //オブジェクト生成
            var o = Instantiate(prefab_Image);
            o.transform.SetParent(canvas);
            //座漂指定
            o.transform.localPosition = _Data.postion;
            //角度制定
            o.transform.localRotation = Quaternion.Euler(_Data.rotation);
            //大きさ指定
            o.transform.localScale = _Data.scale;

            var io_p = o.GetComponent<ImageObject_Prefab>();
            //色指定
            io_p.sprite_r.color = _Data.color;
            //オブジェクト初期化
            io_p.SetUp(_Data);

        }

        /// <summary>
        /// 画像オブジェクトを削除
        /// </summary>
        /// <param name="configPath"></param>
        public void Remove_ImageObject(string configPath)
        {
            if (dict_Imagedata.ContainsKey(configPath))
            {
                var i = dict_Imagedata[configPath];
                //辞書から表示している画像データを削除
                dict_Imagedata.Remove(configPath);
                Destroy(i.Get_Prefab().gameObject);
            }
        }

        public void Value_ImageObject(string configPath, string path)
        {
            if (!dict_Imagedata.ContainsKey(configPath))
            {
                var _Data = new ImageObject_Data(configPath, path);
                dict_Imagedata.Add(configPath, _Data);
            }
        }

        public void V_Path_ImageObject(string c_name, string newPath)
        {
            if (dict_Imagedata.ContainsKey(c_name))
            {
                ImageObject_Data _Data = dict_Imagedata[c_name];
                _Data.path = newPath;

                if (_Data.displayed)
                {
                    var sp = Get_SpriteToPath(newPath);
                    if (sp != null)
                    {
                        //pathを変更する
                        _Data.Get_Prefab().sprite_r.sprite = sp;
                    }
                }
            }
        }

        public void V_Position_ImageObject(string c_name, Vector3 vector)
        {
            if (dict_Imagedata.ContainsKey(c_name))
            {
                ImageObject_Data _Data = dict_Imagedata[c_name];
                _Data.postion = vector;

                if (_Data.displayed)
                {
                    _Data.Get_Prefab().transform.localPosition = vector;
                }
            }
        }

        public void V_Rotation_ImageObject(string c_name, Vector3 vector)
        {
            if (dict_Imagedata.ContainsKey(c_name))
            {
                ImageObject_Data _Data = dict_Imagedata[c_name];
                _Data.rotation = vector;

                if (_Data.displayed)
                {
                    _Data.Get_Prefab().transform.localEulerAngles = vector;
                }
            }
        }

        public void V_Scale_ImageObject(string c_name, Vector3 vector)
        {
            if (dict_Imagedata.ContainsKey(c_name))
            {
                ImageObject_Data _Data = dict_Imagedata[c_name];
                _Data.scale = vector;

                if (_Data.displayed)
                {
                    _Data.Get_Prefab().transform.localScale = vector;
                }
            }
        }

        public void V_Color_ImageObject(string c_name, Color color)
        {
            if (dict_Imagedata.ContainsKey(c_name))
            {
                ImageObject_Data _Data = dict_Imagedata[c_name];
                _Data.color = color;

                if (_Data.displayed)
                {
                    _Data.Get_Prefab().sprite_r.color = color;
                }
            }
        }

        /// <summary>
        /// 画像オブジェクトを移動
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="position1"></param>
        /// <param name="position2"></param>
        /// <param name="time"></param>
        /// <param name="onWait"></param>
        /// <returns></returns>
        public bool Move_ImageObject(string configPath, Vector3 position, float time, string mode, bool keepZ)
        {
            if (dict_Imagedata.ContainsKey(configPath))
            {
                var image_data = dict_Imagedata[configPath];
                var image_prefab = image_data.Get_Prefab();
                Image_ActionRes_Mode ia_mode;

                if (!keepZ)
                {
                    switch (mode)
                    {
                        default:
                        case "ADD_WAIT":
                            ia_mode = Image_ActionRes_Mode.Add_wait;
                            break;
                        case "ADD_NOWAIT":
                            ia_mode = Image_ActionRes_Mode.Add_nowait;
                            break;
                        case "TAR_WAIT":
                            ia_mode = Image_ActionRes_Mode.Target_wait;
                            break;
                        case "TAR_NOWAIT":
                            ia_mode = Image_ActionRes_Mode.Target_nowait;
                            break;
                    }

                    image_prefab.MoveResarve(position, time, ia_mode);
                }
                else
                {
                    switch (mode)
                    {
                        default:
                        case "WAIT":
                        case "ADD_WAIT":
                        case "TAR_WAIT":
                            ia_mode = Image_ActionRes_Mode.Add_wait;
                            break;
                        case "NOWAIT":
                        case "ADD_NOWAIT":
                        case "TAR_NOWAIT":
                            ia_mode = Image_ActionRes_Mode.Add_nowait;
                            break;
                    }

                    image_prefab.MoveKeepzResarve(position, time, ia_mode);
                }

                cullent = image_prefab;

                return true;
            }

            return false;
        }

        public bool Move_ImageObject(string configPath, Vector3 position, float time, string mode)
        {
            return Move_ImageObject(configPath, position, time, mode, false);
        }

        /// <summary>
        /// 画像オブジェクトを回転
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="rotation1"></param>
        /// <param name="rotation2"></param>
        /// <param name="time"></param>
        /// <param name="onWait"></param>
        public void Rot_ImageObject(string configPath, Vector3 rotation, float time, string mode)
        {
            if (dict_Imagedata.ContainsKey(configPath))
            {
                var image_data = dict_Imagedata[configPath];
                var image_prefab = image_data.Get_Prefab();
                Image_ActionRes_Mode ia_mode;

                switch (mode)
                {
                    default:
                    case "ADD_WAIT":
                        ia_mode = Image_ActionRes_Mode.Add_wait;
                        break;
                    case "ADD_NOWAIT":
                        ia_mode = Image_ActionRes_Mode.Add_nowait;
                        break;
                    case "TAR_WAIT":
                        ia_mode = Image_ActionRes_Mode.Target_wait;
                        break;
                    case "TAR_NOWAIT":
                        ia_mode = Image_ActionRes_Mode.Target_nowait;
                        break;
                }

                image_prefab.RotResarve(rotation, time, ia_mode);
                cullent = image_prefab;

            }
        }

        /// <summary>
        /// 画像オブジェクトをリサイズ
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="scale1"></param>
        /// <param name="scale2"></param>
        /// <param name="time"></param>
        /// <param name="onWait"></param>
        public void Size_ImageObject(string configPath, Vector3 scale, float time, string mode)
        {
            if (dict_Imagedata.ContainsKey(configPath))
            {
                var image_data = dict_Imagedata[configPath];
                var image_prefab = image_data.Get_Prefab();
                Image_ActionRes_Mode ia_mode;

                switch (mode)
                {
                    default:
                    case "ADD_WAIT":
                        ia_mode = Image_ActionRes_Mode.Add_wait;
                        break;
                    case "ADD_NOWAIT":
                        ia_mode = Image_ActionRes_Mode.Add_nowait;
                        break;
                    case "TAR_WAIT":
                        ia_mode = Image_ActionRes_Mode.Target_wait;
                        break;
                    case "TAR_NOWAIT":
                        ia_mode = Image_ActionRes_Mode.Target_nowait;
                        break;
                }

                image_prefab.SizeResarve(scale, time, ia_mode);
                cullent = image_prefab;

            }
        }

        /// <summary>
        /// 画像オブジェクトの色を変更する
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="time"></param>
        /// <param name="onWait"></param>
        public void Color_ImageObject(string configPath, Color color, float time, string mode)
        {
            if (dict_Imagedata.ContainsKey(configPath))
            {
                var image_data = dict_Imagedata[configPath];
                var image_prefab = image_data.Get_Prefab();
                Image_ActionRes_Mode ia_mode;
                switch (mode)
                {
                    default:
                    case "WAIT":
                    case "ADD_WAIT":
                    case "TAR_WAIT":
                        ia_mode = Image_ActionRes_Mode.Target_wait;
                        break;
                    case "NOWAIT":
                    case "ADD_NOWAIT":
                    case "TAR_NOWAIT":
                        ia_mode = Image_ActionRes_Mode.Target_nowait;
                        break;
                }

                image_prefab.ColorResarve(color, time, ia_mode);
                cullent = image_prefab;
            }
        }

        /// <summary>
        /// 画像オブジェクトのスプライトを変更する
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="newPath"></param>
        /// <param name="time"></param>
        /// <param name="onWait"></param>
        public void Sprite_ImageObject(string configPath, string newPath, float time, string mode)
        {
            if (dict_Imagedata.ContainsKey(configPath))
            {
                var image_data = dict_Imagedata[configPath];
                var image_prefab = image_data.Get_Prefab();
                Image_ActionRes_Mode ia_mode;
                switch (mode)
                {
                    default:
                    case "WAIT":
                    case "TAR_WAIT":
                    case "ADD_WAIT":
                        ia_mode = Image_ActionRes_Mode.Target_wait;
                        break;
                    case "NOWAIT":
                    case "TAR_NOWAIT":
                    case "ADD_NOWAIT":
                        ia_mode = Image_ActionRes_Mode.Target_nowait;
                        break;
                    case "SMH_WAIT":
                        ia_mode = Image_ActionRes_Mode.Add_wait;
                        break;
                    case "SMH_NOWAIT":
                        ia_mode = Image_ActionRes_Mode.Add_nowait;
                        break;
                }

                image_prefab.SpriteChangeResarve(newPath, time, ia_mode);
                cullent = image_prefab;

            }
        }

        /// <summary>
        /// リソースフォルダのスプライトを読み込む
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Sprite Get_SpriteToPath(string path)
        {
            try
            {
                return Resources.Load<Sprite>(basePath + path);
            }
            catch
            {
                return null;
            }
        }
    }

    public sealed class ImageObject_Data
    {
        public string label;
        public string path;
        public bool displayed = false;

        public Vector3 postion = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public Vector3 scale = Vector3.one;
        public Color color = Color.white;

        private ImageObject_Prefab _prefab;

        public ImageObject_Data(string label, string path)
        {
            this.label = label;
            this.path = path;
        }

        public void Set_Prefab(ImageObject_Prefab prefab)
        {
            _prefab = prefab;
        }

        public ImageObject_Prefab Get_Prefab()
        {
            return _prefab;
        }
    }


}