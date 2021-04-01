using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manager.Image {

    public sealed class ImageObject_Prefab : MonoBehaviour
    {
        //リストに登録しているデータ
        public ImageObject_Data data;
        public Queue<Image_ActionRes> queue = new Queue<Image_ActionRes>();

        [SerializeField]
        public SpriteRenderer sprite_r;
        [SerializeField]
        public SpriteRenderer subsprite_r;

        public bool onAction = false;
        

        /// <summary>
        /// 初期準備
        /// </summary>
        /// <param name="data"></param>
        public void SetUp(ImageObject_Data data)
        {
            this.data = data;
            onAction = false;
            data.Set_Prefab(this);
            sprite_r.sprite = ImageObjectManager.instance.Get_SpriteToPath(data.path);

            //StartCoroutine(DebugUpdate());
            //ExecAction_Queue();
            //Debug.LogFormat("SetUp onAction: 「{0}", onAction);
        }

        private void Update()
        {
            ExecAction_Queue();
            
        }

        /*
        private IEnumerator DebugUpdate()
        {
            while (true) {
                Debug.LogFormat("オブジェクト名：{0}" +
                    "【登録data】label: {1} path: {2} d: {3}\n" +
                    "[position: {4}] [rotation: {5}] [scale: {6}] [color: {7}]\n{8}"
                    , gameObject.name, data.label, data.path, data.displayed
                    , data.postion, data.rotation, data.scale, data.color);

                foreach (var data in queue.ToArray())
                {
                    Debug.LogFormat("======Queue======\n" +
                        "[mode: {1}] [type: {2}] [onWait: {3}]\n [time: {4}] [vector: {5}] [color: {6}]" +
                        "================="
                        , (Image_ActionRes_Mode)data.Get_Mode(), data.Get_Type(), data.Get_OnWait(), data.Get_Time(), data.Get_Vector3(), data.Get_Color());
                }

                yield return new WaitForSeconds(1);
            }
        }
        */

        private void ExecAction_Queue()
        {
            //Debug.LogFormat("count: {0}", queue.Count);
            if (queue.Count != 0)
            {
                if (!onAction)
                {
                    var i_ar = queue.Dequeue();

                    //座漂
                    if (i_ar.Get_Type() == Image_ActionRes_Type.Position)
                    {
                        if (i_ar.Get_OnWait())
                        {
                            onAction = true;
                        }

                        Vector3 v1;
                        Vector3 v2;
                        switch (i_ar.Get_Mode())
                        {
                            default:
                            case 1: //ADD
                                v1 = transform.localPosition;
                                v2 = transform.localPosition + i_ar.Get_Vector3();
                                break;
                            case 2: //TARGET
                                v1 = transform.localPosition;
                                v2 = i_ar.Get_Vector3();
                                break;
                        }

                        var time = i_ar.Get_Time();
                        StartCoroutine(C_Position(v1, v2, time, i_ar.Get_OnWait()));

                        data.postion = v2;
                    }

                    //座漂
                    if (i_ar.Get_Type() == Image_ActionRes_Type.Position_KeepZ)
                    {
                        if (i_ar.Get_OnWait())
                        {
                            onAction = true;
                        }

                        //Debug.Log("キープ");
                        float z = transform.localPosition.z;
                        Vector3 gV = i_ar.Get_Vector3();

                        Vector3 v1;
                        Vector3 v2;
                        switch (i_ar.Get_Mode())
                        {
                            default:
                            case 1: //ADD
                                v1 = transform.localPosition;
                                v2 = transform.localPosition + new Vector3(gV.x, gV.y, z);
                                break;
                            case 2: //TARGET
                                v1 = transform.localPosition;
                                v2 = new Vector3(gV.x, gV.y, z);
                                break;
                        }

                        var time = i_ar.Get_Time();
                        StartCoroutine(C_Position(v1, v2, time, i_ar.Get_OnWait()));

                        data.postion = v2;
                    }

                    //回転
                    if (i_ar.Get_Type() == Image_ActionRes_Type.Rotation)
                    {
                        if (i_ar.Get_OnWait())
                        {
                            onAction = true;
                        }

                        Vector3 v1;
                        Vector3 v2;
                        switch (i_ar.Get_Mode())
                        {
                            default:
                            case 1: //ADD
                                v1 = transform.localRotation.eulerAngles;
                                v2 = transform.localRotation.eulerAngles + i_ar.Get_Vector3();
                                break;
                            case 2: //TARGET
                                v1 = transform.localRotation.eulerAngles;
                                v2 = i_ar.Get_Vector3();
                                break;
                        }

                        var time = i_ar.Get_Time();
                        StartCoroutine(C_Rot(v1, v2, time, i_ar.Get_OnWait()));

                        data.rotation = v2;
                    }

                    //拡大
                    if (i_ar.Get_Type() == Image_ActionRes_Type.Scale)
                    {
                        if (i_ar.Get_OnWait())
                        {
                            onAction = true;
                        }

                        Vector3 v1;
                        Vector3 v2;
                        switch (i_ar.Get_Mode())
                        {
                            default:
                            case 1: //ADD
                                v1 = transform.localScale;
                                v2 = transform.localScale + i_ar.Get_Vector3();
                                break;
                            case 2: //TARGET
                                v1 = transform.localScale;
                                v2 = i_ar.Get_Vector3();
                                break;
                        }

                        var time = i_ar.Get_Time();
                        StartCoroutine(C_Size(v1, v2, time, i_ar.Get_OnWait()));

                        data.scale = v2;
                    }

                    //色変更
                    if (i_ar.Get_Type() == Image_ActionRes_Type.Color)
                    {

                        //Debug.LogFormat("onWait: {0}", i_ar.Get_OnWait());
                        if (i_ar.Get_OnWait())
                        {
                            onAction = true;
                        }
                        //Debug.LogFormat("onAction: {0}", onAction);

                        Color v1;
                        Color v2;
                        switch (i_ar.Get_Mode())
                        {
                            default:
                            case 2: //TARGET
                                v1 = sprite_r.color;
                                v2 = i_ar.Get_Color();
                                break;
                        }

                        var time = i_ar.Get_Time();
                        StartCoroutine(C_Color(v1, v2, time, i_ar.Get_OnWait()));

                        data.color = v2;
                    }

                    //スプライト変更
                    if (i_ar.Get_Type() == Image_ActionRes_Type.Sprite)
                    {
                        if (i_ar.Get_OnWait())
                        {
                            onAction = true;
                        }

                        var v1 = i_ar.Get_Path();
                        var time = i_ar.Get_Time();

                        switch (i_ar.Get_Mode())
                        {
                            default:
                            case 1: //ADD (Smooth)
                                StartCoroutine(C_Sprite_ChangeSmooth(v1, time, i_ar.Get_OnWait()));
                                break;
                            case 2: //TARGET (No Smooth)
                                StartCoroutine(C_Sprite_Change(v1, time, i_ar.Get_OnWait()));
                                break;
                        }

                        data.path = v1;
                    }

                }
            }
        }

        private IEnumerator C_Position(Vector3 start, Vector3 goal, float time, bool onWait)
        {
            time = Mathf.Clamp(time, 0, float.MaxValue);

            for (int i = 1; i <= 100 * time; i++) {
                transform.localPosition = Vector3.Lerp(start, goal, (float)i / (100 * time));
                yield return new WaitForSeconds(1.0f / 100.0f);
            }

            if (time == 0)
            {
                transform.localPosition = goal;
            }

            if (onWait) onAction = false;
        }

        private IEnumerator C_Rot(Vector3 start, Vector3 goal, float time, bool onWait)
        {
            time = Mathf.Clamp(time, 0, float.MaxValue);

            //Debug.LogFormat("start: {0}  goal: {1}", start, goal);

            for (int i = 1; i <= 100 * time; i++)
            {
                transform.localRotation = Quaternion.Euler( Vector3.Lerp(start, goal, (float)i / (100 * time)));
                yield return new WaitForSeconds(1.0f / 100.0f);
            }

            if (time == 0)
            {
                transform.localRotation = Quaternion.Euler(goal);
            }

            if (onWait) onAction = false;
        }

        private IEnumerator C_Size(Vector3 start, Vector3 goal, float time, bool onWait)
        {
            time = Mathf.Clamp(time, 0, float.MaxValue);

            for (int i = 1; i <= 100 * time; i++)
            {
                transform.localScale = Vector3.Lerp(start, goal, (float)i / (100 * time));
                yield return new WaitForSeconds(1.0f / 100.0f);
            }

            if (time == 0)
            {
                transform.localScale = goal;
            }

            if (onWait) onAction = false;

        }

        private IEnumerator C_Color(Color start, Color goal, float time, bool onWait)
        {
            time = Mathf.Clamp(time, 0, float.MaxValue);
            var a = start.a;  

            //Debug.LogFormat("色変化({0}) 開始", goal);
            for (int i = 1; i <= 100 * a * time; i++)
            {
                sprite_r.color = Color.Lerp(start, goal, (float)i / (100 * a * time));
                yield return new WaitForSeconds(1.0f / 100.0f);
            }

            if (time == 0)
            {
                sprite_r.color = goal;
            }

            //Debug.LogFormat("色変化({0}) 終了", goal);
            if (onWait) onAction = false;
        }

        private IEnumerator C_Sprite_Change(string path, float time, bool onWait)
        {
            time = Mathf.Clamp(time, 0, float.MaxValue);

            var start = sprite_r.color;
            var goal = new Color(start.r, start.g, start.b, 0);
            
            for (int i = 1; i <= 50 * time; i++)
            {
                sprite_r.color = Color.Lerp(start, goal, (float)i / (50 * time));
                yield return new WaitForSeconds(1.0f / 100.0f);
            }

            var newSprite = ImageObjectManager.instance.Get_SpriteToPath(path);
            sprite_r.sprite = newSprite;

            for (int i = 1; i <= 50 * time; i++)
            {
                sprite_r.color = Color.Lerp(goal, start, (float)i / (50 * time));
                yield return new WaitForSeconds(1.0f / 100.0f);
            }

            if (time == 0)
            {
                sprite_r.color = start;
            }

            if (onWait) onAction = false;
        }

        private IEnumerator C_Sprite_ChangeSmooth(string path, float time, bool onWait)
        {
            //Debug.Log("差分変化");
            time = Mathf.Clamp(time, 0, float.MaxValue);

            var start = sprite_r.color;
            var goal = new Color(start.r, start.g, start.b, 0);

            var newSprite = ImageObjectManager.instance.Get_SpriteToPath(path);
            subsprite_r.sprite = newSprite;
            subsprite_r.color = start;
            var a = start.a; //0.6f

            for (int i = 1; i <= 100 * a * time; i++)
            {
                sprite_r.color = Color.Lerp(start, goal, (float)i / (100 * a * time));
                yield return new WaitForSeconds(1.0f / 100.0f); 
            }

            sprite_r.sprite = newSprite;
            sprite_r.color = start;

            if (onWait) onAction = false;
        }

        /// <summary>
        /// 座漂移動予約
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <param name="secound"></param>
        /// <param name="onWait"></param>
        public void MoveResarve(Vector3 value, float secound, Image_ActionRes_Mode mode)
        {
            var i_ar = new Image_ActionRes();
            i_ar.SetType(1, mode);
            i_ar.Set_Time(secound);
            i_ar.Set_Vector(value);

            queue.Enqueue(i_ar);
            ExecAction_Queue();
        }

        public void MoveKeepzResarve(Vector3 value, float secound, Image_ActionRes_Mode mode)
        {
            //Debug.Log("キープ予約");

            var i_ar = new Image_ActionRes();
            i_ar.SetType(6, mode);
            i_ar.Set_Time(secound);
            i_ar.Set_Vector(value);

            queue.Enqueue(i_ar);
            ExecAction_Queue();
        }

        /// <summary>
        /// 回転動作予約
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <param name="secound"></param>
        /// <param name="onWait"></param>
        public void RotResarve(Vector3 value, float secound, Image_ActionRes_Mode mode)
        {
            var i_ar = new Image_ActionRes();

            i_ar.SetType(2, mode);
            i_ar.Set_Time(secound);
            i_ar.Set_Vector(value);

            queue.Enqueue(i_ar);
            ExecAction_Queue();
        }

        /// <summary>
        /// サイズ動作予約
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <param name="secound"></param>
        /// <param name="onWait"></param>
        public void SizeResarve(Vector3 value, float secound, Image_ActionRes_Mode mode)
        {
            var i_ar = new Image_ActionRes();

            i_ar.SetType(3, mode);
            i_ar.Set_Time(secound);
            i_ar.Set_Vector(value);

            queue.Enqueue(i_ar);
            ExecAction_Queue();
        }

        /// <summary>
        /// 色切替予約
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <param name="secound"></param>
        /// <param name="onWait"></param>
        public void ColorResarve(Color value, float secound, Image_ActionRes_Mode mode)
        {
            var i_ar = new Image_ActionRes();

            i_ar.SetType(4, mode);
            i_ar.Set_Time(secound);
            i_ar.Set_Color(value);

            queue.Enqueue(i_ar);
            ExecAction_Queue();
        }

        /// <summary>
        /// スプライト変更
        /// </summary>
        /// <param name="newSpritePath"></param>
        /// <param name="secound"></param>
        /// <param name="onWait"></param>
        public void SpriteChangeResarve(string newSpritePath, float secound, Image_ActionRes_Mode mode)
        {
            var i_ar = new Image_ActionRes();

            i_ar.SetType(5, mode);
            i_ar.Set_Path(newSpritePath);
            i_ar.Set_Time(secound);

            queue.Enqueue(i_ar);
            ExecAction_Queue();
        }

    }

    public sealed class Image_ActionRes
    {
        // 【type】1:位置, 2:回転, 3:サイズ, 4:色 5:変更
        // 【mode】1:ADD 2:TARGET
        // 【下4~6桁】0:直線移動 

        int type = 0;
        int mode = 0;
        bool onWait = false;

        float time = 0;

        Vector3 vector = Vector3.zero;

        Color color = Color.white;

        string path = "";

        public void Set_Path(string path)
        {
            this.path = path;
        }

        public void Set_Time(float time)
        {
            this.time = time;
        }

        public void SetType(int type, Image_ActionRes_Mode mode)
        {
            this.type = type;

            switch (mode)
            {
                default:
                case Image_ActionRes_Mode.Add_wait:
                    this.mode = 1;
                    onWait = true;
                    break;
                case Image_ActionRes_Mode.Add_nowait:
                    this.mode = 1;
                    onWait = false;
                    break;
                case Image_ActionRes_Mode.Target_wait:
                    this.mode = 2;
                    onWait = true;
                    break;
                case Image_ActionRes_Mode.Target_nowait:
                    this.mode = 2;
                    onWait = false;
                    break;
            }
        }

        public void Set_Vector(Vector3 vector)
        {
            this.vector = vector;
        }

        public void Set_Color(Color color)
        {
            this.color = color;
        }

        public bool Get_OnWait()
        {
            return onWait;
        }

        public float Get_Time()
        {
            return time;
        }

        public string Get_Path()
        {
            return path;
        }

        public Image_ActionRes_Type Get_Type()
        {
            switch (type)
            {
                case 1:
                    return Image_ActionRes_Type.Position;
                case 2:
                    return Image_ActionRes_Type.Rotation;
                case 3:
                    return Image_ActionRes_Type.Scale;
                case 4:
                    return Image_ActionRes_Type.Color;
                case 5:
                    return Image_ActionRes_Type.Sprite;
                case 6:
                    return Image_ActionRes_Type.Position_KeepZ;
                default:
                    return Image_ActionRes_Type.None;
            }
        }

        public int Get_Mode()
        {
            return mode;
        }

        public Vector3 Get_Vector3()
        {
            return vector;
        }

        public Color Get_Color()
        {
            return color;
        }
    }

    public enum Image_ActionRes_Type
    {
        None, Position, Rotation, Scale, Color, Sprite, Position_KeepZ
    }

    public enum Image_ActionRes_Mode
    {
        None, Add_wait, Add_nowait, Target_wait, Target_nowait
    }
}