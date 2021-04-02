using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FLS.AudioControll
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;

        private AudioClip[] audioClip_bgm;
        private AudioClip[] audioClip_se;

        private TalkEventManager talk;
        private AudioSource audioSource_bgm;
        private bool isFade = false;
        private float timeBuff = 0;


        [SerializeField]
        private string ResoucePass = "audio/";

        [SerializeField]
        private string ResouceBGMPass = "bgm/";

        [SerializeField]
        private string ResouceSEPass = "se/";

        //[SerializeField]
        //private AudioDataBase audioDataBase;

        [SerializeField]
        private AudioSource[] audioSources_se;

        /// <summary> 命令終了フラグ </summary>
        private bool orderFinished = false;

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

            //audioClip_bgm = audioDataBase.Get_BGM();
            //audioClip_se = audioDataBase.Get_SE();
        }

        // Start is called before the first frame update
        void Start()
        {
            talk = TalkEventManager.instance;
            audioSource_bgm = GetComponent<AudioSource>();

            SetUp_Order();
        }

        private void SetUp_Order()
        {
            talk.Order_Registration("BGMPLAY", //『BGMをプレイする』
                                               //デコード時の設定
                (int count, OrderParametor par, string[] arg) =>
                {
                    if (arg.Length == 3)
                    {
                        par.parString.Add(arg[1]);
                        par.parFloat.Add(float.Parse(arg[2]));

                        return true;
                    }

                    if (arg.Length == 2)
                    {
                        par.parString.Add(arg[1]);
                        par.parFloat.Add(1.0f);

                        return true;
                    }

                    return false;
                },

                (ref int count, OrderParametor par) =>
                {
                    string path = par.parString[0];
                    float pitch = par.parFloat[0];

                    Play_BGM(path, pitch);

                    count++;
                });


            talk.Order_Registration("BGMFADE", //『BGMをフェードする』
                                               //デコード時の設定
                (int count, OrderParametor par, string[] arg) =>
                {
                    if (arg.Length == 5)
                    {
                        par.parString.Add(arg[1]);
                        par.parFloat.Add(float.Parse(arg[2]));
                        par.parFloat.Add(float.Parse(arg[3]));
                        par.parString.Add(arg[4]);

                        return true;
                    }

                    return false;
                },
                Order_BGM_Fade);

            talk.Order_Registration("BGMSTOP", //『BGMを止める』
                                               //デコード時の設定
                (int count, OrderParametor par, string[] arg) =>
                {
                    if (arg.Length == 1)
                    {
                        par.parFloat.Add(0);
                        par.parString.Add("");

                        return true;
                    }

                    if (arg.Length == 3)
                    {
                        par.parFloat.Add(float.Parse(arg[1]));
                        par.parString.Add(arg[2]);

                        return true;
                    }

                    return false;
                },
                Order_BGM_Stop);

            talk.Order_Registration("BGMCONT", //『BGMをつづきから再生する』
                                               //デコード時の設定
                (int count, OrderParametor par, string[] arg) =>
                {
                    if (arg.Length == 1)
                    {
                        return true;
                    }

                    return false;
                },
                Order_Continue);

            talk.Order_Registration("SOUND", //『サウンドエフェクトを再生する』
                                             //デコード時の設定
                (int count, OrderParametor par, string[] arg) =>
                {
                    if (arg.Length == 4)
                    {
                        par.parString.Add(arg[1]);
                        par.parFloat.Add(float.Parse(arg[2]));
                        par.parFloat.Add(float.Parse(arg[3]));

                        return true;
                    }

                    if (arg.Length == 2)
                    {
                        par.parString.Add(arg[1]);
                        par.parFloat.Add(1);
                        par.parFloat.Add(1);

                        return true;
                    }

                    return false;
                },
                Order_Sound);

        }

        private void Order_BGM_Fade(ref int count, OrderParametor par)
        {
            int mode = 0;

            if (orderFinished)
            {
                count++;
                orderFinished = false;
                return;
            }

            if (!isFade)
            {
                string path = par.parString[0];
                string s_mode = par.parString[1];
                float seconds = par.parFloat[0];
                float picth = par.parFloat[1];

                switch (s_mode)
                {
                    case "WAIT":
                        mode = 0;
                        break;
                    case "NOWAIT":
                        mode = 1;
                        break;
                }

                StartCoroutine(C_Fade_Update(path, picth, seconds, mode));
            }

            if (mode == 1)
            {
                count++;
            }
        }

        private IEnumerator C_Fade_Update(string pass, float pitch, float seconds, int mode)
        {
            isFade = true;

            if (seconds > 0)
            {
                for (int i = 0; i <= seconds * 50; i++)
                {
                    audioSource_bgm.volume = 1.0f - (i / (seconds * 50));
                    yield return new WaitForSeconds(0.01f);
                }

                Play_BGM(pass, pitch);

                for (int i = 0; i <= seconds * 50; i++)
                {
                    audioSource_bgm.volume = i / (seconds * 50);
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else
            {
                Play_BGM(pass, pitch);
                audioSource_bgm.volume = 1;
            }

            orderFinished = true;

            isFade = false;
        }

        private void Order_BGM_Stop(ref int count, OrderParametor par)
        {
            int mode = 0;

            if (orderFinished)
            {
                count++;
                orderFinished = false;
                return;
            }

            if (!isFade)
            {
                orderFinished = false;
                string s_mode = par.parString[0];
                float seconds = par.parFloat[0];

                switch (s_mode)
                {
                    case "WAIT":
                        mode = 0;
                        break;
                    case "NOWAIT":
                        mode = 1;
                        break;
                }

                StartCoroutine(C_Stop_Update(seconds, mode));
            }

            if (mode == 1)
            {
                count++;
            }


        }

        private IEnumerator C_Stop_Update(float seconds, int mode)
        {
            isFade = true;

            if (seconds > 0)
            {

                for (int i = 0; i <= seconds * 100; i++)
                {
                    audioSource_bgm.volume = 1.0f - (i / (seconds * 100));
                    yield return new WaitForSeconds(0.01f);
                }

                timeBuff = audioSource_bgm.time;
                audioSource_bgm.Stop();
                audioSource_bgm.volume = 1;
            }
            else
            {
                timeBuff = audioSource_bgm.time;
                audioSource_bgm.Stop();
                audioSource_bgm.volume = 1;
            }

            if (mode == 0)
            {
                orderFinished = true;
            }

            isFade = false;
        }

        private void Order_Continue(ref int count, OrderParametor par)
        {
            ContinueBGM();
            count++;
        }

        public void ContinueBGM()
        {
            audioSource_bgm.time = timeBuff;
            audioSource_bgm.Play();
        }

        private AudioClip Get_SpriteAsset(string pass)
        {
            AudioClip audio = Resources.Load<AudioClip>(ResoucePass + pass);
            return audio;
        }

        public void Play_BGM(string pass, float pitch)
        {
            AudioClip clip = Get_SpriteAsset(ResouceBGMPass + pass);
            if (clip != null)
            {
                audioSource_bgm.time = 0;
                audioSource_bgm.clip = clip;
                audioSource_bgm.pitch = pitch;
                audioSource_bgm.Play();
            }
        }

        public void Change_BGM(string pass, float seconds, float pitch)
        {
            if (!isFade)
            {
                StartCoroutine(C_Change_BGM(pass, seconds, pitch));
            }
        }

        private IEnumerator C_Change_BGM(string pass, float seconds, float pitch)
        {
            isFade = true;

            if (seconds > 0)
            {

                for (int i = 0; i <= seconds * 500; i++)
                {
                    audioSource_bgm.volume = 1.0f - (i / (seconds * 500));
                    yield return new WaitForSeconds(0.001f);
                }

                Play_BGM(pass, pitch);

                for (int i = 0; i <= seconds * 500; i++)
                {
                    audioSource_bgm.volume = i / (seconds * 500);
                    yield return new WaitForSeconds(0.001f);
                }
            }
            else
            {
                Play_BGM(pass, pitch);
                audioSource_bgm.volume = 1;
            }
            isFade = false;
        }

        private bool Check_Playing(AudioSource audio)
        {
            if (!audio.isPlaying)
            {
                return true;
            }
            return false;
        }

        //======================================================================================
        //=====================================================================================

        private void Order_Sound(ref int count, OrderParametor par)
        {
            orderFinished = false;
            string path = par.parString[0];
            float picth = par.parFloat[0];
            float volume = par.parFloat[1];

            SE_Play(path, picth, volume);
            count++;
        }

        public void SE_Play(string pass, float pitch, float volume)
        {
            AudioSource audioSource = null;
            AudioClip clip = Get_SpriteAsset(ResouceSEPass + pass);


            //使ってないAudioSourceを探索する
            foreach (var audio in audioSources_se)
            {
                if (!audio.isPlaying)
                {
                    audioSource = audio;
                }
            }

            if (audioSource != null)
            {
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(clip, volume);
            }
        }


        private AudioClip SE_clip(int index)
        {
            if (audioClip_se.Length > index)
            {
                return audioClip_se[index];
            }

            return audioClip_se[0];
        }
    }
}