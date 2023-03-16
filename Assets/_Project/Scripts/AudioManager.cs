using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using _Project.Scripts;


namespace _Project.Scripts{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] AudioMixer mixer;
        [SerializeField] AudioMixerGroup musicMixer;
        [SerializeField] AudioMixerGroup sfxMixer;
        [SerializeField] AnimationCurve fadeCurve;
        [SerializeField] AnimationCurve lowpassCurve;
        [SerializeField] GameObject player;
        [SerializeField] GameObject dummy;
        [SerializeField] float hitCD = 8f;
        public Sound[] sfx;
        public Sound[] music;


        public static AudioManager instance;
        private Vector2 playerInitPos;
        private bool playerMoved = false;
        private bool inBattle = false;
        private float currentCD = 0f;


        void Start(){
            //SetCutoffFreq(300f);
            //StartCoroutine(SlideLowPass(300f, 22000f, 16f));
            StartCoroutine(Fade("zero", 0f, 1f, 2f));
        }
        void Awake () 
        {
            if (instance == null)
                instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            foreach(Sound s in sfx)
            {
                s.source = gameObject.AddComponent<AudioSource>();

                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = 1f;
                s.source.loop = false;
                s.source.outputAudioMixerGroup = sfxMixer;
            }
            foreach(Sound s in music)
            {
                s.source = gameObject.AddComponent<AudioSource>();

                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = 1f;
                s.source.loop = true;
                s.source.outputAudioMixerGroup = musicMixer;
            }
            foreach(Sound s in music)
            {
                s.source.Play();
                s.source.volume = 0f;
            }
            SetMusicVolume("zero", 1f);
            
            //SetCutoffFreq(22000f);
            //StartCoroutine(SwitchTheme("blood", 10f));
            //PlaySFX("slap");
            //StartCoroutine(SlideLowPass(300f, 22000f, 8f));
            playerInitPos = player.transform.position;

            dummy.GetComponent<IDamageable>().OnDamaged += StartCD;
        }
        void StartCD(float damage){
            currentCD = hitCD;
            PlaySFX("oof");
        }

        public void PlayMusic (string name) 
        {
            Sound s = Array.Find(music, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Music " + name + " not found");
                return;
            }
            s.source.Play();
        }

        public void SetMusicVolume (string name, float volume) 
        {
            AudioSource s = GetMusicSource(name);
            s.volume = volume;
        }
        private AudioSource GetMusicSource(string name) 
        {
            Sound s = Array.Find(music, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Music " + name + " not found");
            }
            return s.source;
        }

        public void PlaySFX (string name) 
        {
            List<Sound> variants = new List<Sound>();
            int i = 1;
            foreach (Sound s in sfx)
            {  
                if (s.name == name + i)
                {
                    variants.Add(s);
                    i++;
                }
            }
            Sound sound = variants[UnityEngine.Random.Range(0, variants.Count)];    
            sound.source.Play();
        }

        void SetCutoffFreq(float value)
        {
            mixer.SetFloat("MusicCutoffFreq", value);
        }

        private IEnumerator SlideLowPass(float value1, float value2, float duration)
        {
            float timeElapsed = 0f;   
            while(timeElapsed < duration)
            {
                float value = value1 + (value2 - value1) * lowpassCurve.Evaluate(timeElapsed / duration);
                SetCutoffFreq(value);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            SetCutoffFreq(value2);
        }
        
        private IEnumerator Fade(string name, float volume1, float volume2, float duration)
        {
            float timeElapsed = 0f;
            AudioSource musicSource = GetMusicSource(name);
            while(timeElapsed < duration)
            {
                float volume = volume1 + (volume2 - volume1) * fadeCurve.Evaluate(timeElapsed / duration);
                //SetMusicVolume(name, volume);
                musicSource.volume = volume;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            //SetMusicVolume(name, volume2);
            musicSource.volume = volume2;
        }
        private IEnumerator SwitchTheme(string name, float duration)
        {
            float timeElapsed = 0f;
            AudioSource switchTo = GetMusicSource(name);
            float initialVolume = switchTo.volume;
            while (timeElapsed < duration)
            {
                foreach (Sound m in music)
                {
                    if(m.name != name)
                        m.source.volume = Mathf.Lerp(m.source.volume, 0f, fadeCurve.Evaluate(timeElapsed / duration));         
                }
                switchTo.volume = Mathf.Lerp(switchTo.volume, 1f, fadeCurve.Evaluate(timeElapsed / duration));
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            foreach (Sound m in music)
            {
                m.source.volume = 0f;
            }
            switchTo.volume = 1f;
        }

        void Update() {
            currentCD -= Time.deltaTime;
            if (inBattle && currentCD <= 0)
            {
                inBattle = false;
                StartCoroutine(SwitchTheme("main", 4f));
            }
            if (!inBattle && currentCD > 0)
            {
                inBattle = true;
                StartCoroutine(SwitchTheme("fight", 4f));
            }
            if(!playerMoved)
            {
                //Debug.Log(Mathf.Abs(playerInitPos.x - player.transform.position.x));
            }
            
            if (!playerMoved && Mathf.Abs(playerInitPos.x - player.transform.position.x) > 4f)
            {
                playerMoved = true;
                StartCoroutine(SwitchTheme("main", 4f));
                
            }
        }
        void OnDisable(){
            dummy.GetComponent<IDamageable>().OnDamaged -= StartCD;
        }
    }
}
