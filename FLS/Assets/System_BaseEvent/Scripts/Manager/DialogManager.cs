using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace FLS.Dialog
{

    public class DialogManager : MonoBehaviour
    {
        public static DialogManager instance;

        public GameObject CanvasObject;

        public DialogSettingData setingData;

        private TalkEventManager talk;
        private Dictionary<int, GameObject> DialogDict = new Dictionary<int, GameObject>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            talk = TalkEventManager.instance;
            SetUp_Order();
        }

        private void Update()
        {
            
        }

        private void SetUp_Order()
        {
            talk.Order_Registration("DIALOG_SET",
                (int count, OrderParametor par, string[] arg) =>
                {
                    if (arg.Length == 3)
                    {
                        par.parInt.Add(int.Parse(arg[1]));
                        par.parInt.Add(int.Parse(arg[2]));
                        par.parVector.Add(Vector3.zero);
                        par.parInt.Add(0);

                        return true;
                    }

                    if (arg.Length == 5)
                    {
                        par.parInt.Add(int.Parse(arg[1]));
                        par.parInt.Add(int.Parse(arg[2]));
                        par.parVector.Add(new Vector3(float.Parse(arg[3]), float.Parse(arg[4]), 0));
                        par.parInt.Add(1);

                        return true;
                    }

                    return false;
                },
                (ref int count, OrderParametor par) =>
                {
                    int id = par.parInt[0];
                    int d_id = par.parInt[1];
                    Vector2 point = par.parVector[0];
                    int mode = par.parInt[2];

                    Set_Dialog_Event(d_id, id, point, mode);

                    count++;
                });

            talk.Order_Registration("DIALOG_REMOVE",
                (int count, OrderParametor par, string[] arg) =>
                {
                    if (arg.Length == 2)
                    {
                        par.parInt.Add(int.Parse(arg[1]));
                        return true;
                    }


                    return false;
                },
                (ref int count, OrderParametor par) =>
                {
                    int id = par.parInt[0];
                    Remove_Dialog_Event(id);

                    count++;
                });

            talk.Order_Registration("DIALOG_default",
                (int count, OrderParametor par, string[] arg) =>
                {
                    if (arg.Length == 3)
                    {
                        par.parString.Add(arg[1]);
                        par.parString.Add(arg[2]);
                        par.parString.Add("");
                        return true;
                    }

                    if (arg.Length == 4)
                    {
                        par.parString.Add(arg[1]);
                        par.parString.Add(arg[2]);
                        par.parString.Add(arg[3]);
                        return true;
                    }

                    return false;
                },

                (ref int count, OrderParametor par) =>
                {
                    int id = 0;
                    string mess = par.parString[0];
                    string choice1 = par.parString[1];
                    string choice2 = par.parString[2];

                    Set_Default_Event(id, mess, choice1, choice2);

                    count++;
                });

            talk.Order_Registration("DIALOG",
                (int count, OrderParametor par, string[] arg) =>
                {
                    if (arg.Length == 4)
                    {
                        string t1 = new StringBuilder("DIALOG_default \"").
                            Append(arg[1]).Append("\" \"").
                            Append(arg[2]).Append("\" \"").
                            Append(arg[3]).Append("\"").ToString();

                        talk.EventRegistration(t1);
                        talk.EventRegistration("VALUE_SET 121 0");
                        talk.EventRegistration("IFWAIT [121]!=0");
                        talk.EventRegistration("DIALOG_REMOVE 0");
                        talk.EventRegistration("IF [121]==1");

                        return true;
                    }

                    if (arg.Length == 3)
                    {
                        string t1 = new StringBuilder("DIALOG_default \"").
                            Append(arg[1]).Append("\" \"").
                            Append(arg[2]).Append("\"").
                            ToString();

                        talk.EventRegistration(t1);
                        talk.EventRegistration("VALUE_SET 121 0");
                        talk.EventRegistration("IFWAIT [121]!=0");
                        talk.EventRegistration("DIALOG_REMOVE 0");
                    }

                    return false;
                },
                null, Type_TalkEventData.MULTI
                );

            talk.Order_Registration("ELSE_DIALOG",
                (int count, OrderParametor par, string[] arg) =>
                {
                    talk.EventRegistration("ENDIF");
                    talk.EventRegistration("IF [121]==2");

                    return false;
                },
                null, Type_TalkEventData.MULTI
                );

            talk.Order_Registration("END_DIALOG",
                (int count, OrderParametor par, string[] arg) =>
                {
                    talk.EventRegistration("ENDIF");

                    return false;
                },
                null, Type_TalkEventData.MULTI
                );

        }

        /// <summary>
        /// トークイベント命令用・ダイアログ設置
        /// </summary>
        /// <param name="eo"></param>
        public void Set_Default_Event(int id, string mess, string choice1, string choice2)
        {
            GameObject o;
            if (!DialogDict.ContainsKey(id))
            {
                o = Instantiate(setingData.prefabs[0].gameObject);

                DialogDict.Add(id, o);
            }
            else
            {
                o = DialogDict[id];
            }

            Dialog_Prefab prefab = o.GetComponent<Dialog_Prefab>();
            o.transform.SetParent(CanvasObject.transform);
            prefab.Set_Default_Dialog(choice1, choice2, mess);


            var r = setingData.prefabs[0].transform.localPosition;
            o.transform.localPosition = new Vector3(r.x, r.y, r.z);
        }

        /// <summary>
        /// トークイベント命令用・ダイアログを設置する
        /// </summary>
        /// <param name="eo"></param>
        public void Set_Dialog_Event(int dialog_id, int a_dialog_id, Vector2 point, int mode)
        {
            GameObject o;

            if (dialog_id < setingData.prefabs.Count)
            {

                if (!DialogDict.ContainsKey(a_dialog_id))
                {
                    o = Instantiate(setingData.prefabs[dialog_id].gameObject);

                    DialogDict.Add(a_dialog_id, o);
                }
                else
                {
                    o = DialogDict[a_dialog_id];
                }

                Dialog_Prefab prefab = o.GetComponent<Dialog_Prefab>();
                o.transform.parent = CanvasObject.transform;
                prefab.Set();

                if (mode == 1)
                {
                    var r = setingData.prefabs[a_dialog_id].transform.localPosition;
                    o.transform.localPosition = new Vector3(point.x, point.y, r.z);
                }
                else
                {
                    var r = setingData.prefabs[a_dialog_id].transform.localPosition;
                    o.transform.localPosition = new Vector3(r.x, r.y, r.z);

                }
            }
        }

        /// <summary>
        /// トークイベント命令用・ダイアログ削除
        /// </summary>
        /// <param name="eo"></param>
        public void Remove_Dialog_Event(int active_id)
        {
            if (DialogDict.ContainsKey(active_id))
            {
                var o = DialogDict[active_id];
                DialogDict.Remove(active_id);
                Destroy(o);
            }
        }

    }
}