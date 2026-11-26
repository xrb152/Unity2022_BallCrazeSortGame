using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class AudioItem
{
    public string name;
    public float volume = 1f;
    public float MinTimeBetweenCall = 0;
    public bool loop;
    public AudioClip[] clip;
    [HideInInspector]
    public float lastTimePlayed = 0;
}

public class AudioPlayer : Singleton<AudioPlayer>
{
    public AudioItem[] AudioList;
    public GameSettings settings;
    public static AudioSource MusicSource;

    private float musicVolume
    {
        get { return PlayerPrefs.GetFloat("music", settings.MusicVolume); }
        set { PlayerPrefs.SetFloat("music", value); }
    }
    private float sfxVolume
    {
        get { return PlayerPrefs.GetFloat("sfx", settings.SFXVolume); }
        set { PlayerPrefs.SetFloat("sfx", value); }
    }


    public override void Awake()
    {
        GlobalAudioPlayer.audioPlayer = this;
    }
    private void Start()
    {
        playMusic("BGM");
    }
    public void ToggleSound()
    {
        sfxVolume = sfxVolume == 0f ? settings.SFXVolume : 0f;
        playSFX("Button");
    }
    public void ToggleMusic()
    {
        playSFX("Button");
        musicVolume = musicVolume == 0f ? settings.MusicVolume : 0f;

        MusicSource.volume = musicVolume;
    }
    public bool IsSoundOn() => sfxVolume == settings.SFXVolume;
    public bool IsMusicOn() => musicVolume == settings.MusicVolume;
    public void playSFX(string name)
    {

        foreach (AudioItem audioItem in AudioList)
        {
            if (audioItem.name == name)
            {
                if (Time.time - audioItem.lastTimePlayed < audioItem.MinTimeBetweenCall)
                    return;

                audioItem.lastTimePlayed = Time.time;

                AudioSource audiosource = GetPooledAudioSource(name);
                audiosource.clip = audioItem.clip[Random.Range(0, audioItem.clip.Length)];
                audiosource.volume = audioItem.volume * sfxVolume;
                audiosource.loop = audioItem.loop;
                audiosource.Play();

                StartCoroutine(ReturnToPoolAfterPlay(audiosource, name, audiosource.clip.length));
                return;
            }
        }
        Debug.Log("no sfx found with name: " + name);
    }
    public void playLoopSFX(string name, float time, Transform parent = null)
    {
        bool SFXFound = false;
        foreach (AudioItem audioItem in AudioList)
        {
            if (audioItem.name == name)
            {


                //pick a random number
                int rand = Random.Range(0, audioItem.clip.Length);

                //create gameobject for the audioSource
                GameObject audioObj = new GameObject
                {
                    name = name
                };
                AudioSource audiosource = audioObj.AddComponent<AudioSource>();

                if (parent != null) audioObj.transform.parent = parent;
                //audio source settings
                audiosource.clip = audioItem.clip[rand];
                audiosource.volume = audioItem.volume * sfxVolume;
                //  audiosource.outputAudioMixerGroup = source.outputAudioMixerGroup;
                audiosource.loop = audioItem.loop;
                audiosource.Play();

                //Destroy on finish
                if (audioItem.loop && audiosource.clip != null)
                {
                    TimeToLive TTL = audioObj.AddComponent<TimeToLive>();
                    TTL.LifeTime = time;
                }
                SFXFound = true;
            }
        }
        if (!SFXFound) Debug.Log("no sfx found with name: " + name);
    }
    public void playGrowUpSFX(string name, int id)
    {
        bool SFXFound = false;
        foreach (AudioItem audioItem in AudioList)
        {
            if (audioItem.name == name)
            {

                //pick a random number (not same twice)
                // int rand = Random.Range(0, audioItem.clip.Length);
                // source.PlayOneShot(audioItem.clip[id]);
                // source.volume = audioItem.volume * sfxVolume;
                // source.loop = audioItem.loop;

                AudioSource audiosource = GetPooledAudioSource(name);
                if (id >= audioItem.clip.Length)
                {
                    id = audioItem.clip.Length - 1;
                }
                audiosource.clip = audioItem.clip[id];
                audiosource.volume = audioItem.volume * sfxVolume;
                audiosource.loop = audioItem.loop;
                audiosource.Play();

                SFXFound = true;
            }
        }
        if (!SFXFound) Debug.Log("no sfx found with name: " + name);
    }

    public void playSFXAtPosition(string name, Vector3 worldPosition, Transform parent)
    {
        foreach (AudioItem audioItem in AudioList)
        {
            if (audioItem.name == name)
            {
                if (Time.time - audioItem.lastTimePlayed < audioItem.MinTimeBetweenCall)
                    return;

                audioItem.lastTimePlayed = Time.time;

                AudioSource audiosource = GetPooledAudioSource(name);
                audiosource.transform.position = worldPosition;
                audiosource.spatialBlend = 1.0f;
                audiosource.minDistance = 4f;
                audiosource.volume = audioItem.volume * sfxVolume;
                audiosource.loop = audioItem.loop;
                audiosource.Play();

                StartCoroutine(ReturnToPoolAfterPlay(audiosource, name, audiosource.clip.length));
                return;
            }
        }
        Debug.Log("no sfx found with name: " + name);
    }

    public void playSFXAtPosition(string name, Vector3 worldPosition)
    {
        playSFXAtPosition(name, worldPosition, transform.root);
    }
    public void playMultipleSFX(string name)
    {
        bool SFXFound = false;
        foreach (AudioItem audioItem in AudioList)
        {
            if (audioItem.name == name)
            {
                foreach (AudioClip clip in audioItem.clip)
                {
                    GameObject source = new GameObject();
                    source.transform.parent = transform;
                    source.name = name;
                    AudioSource audiosource = source.AddComponent<AudioSource>();


                    audiosource.PlayOneShot(clip);
                    audiosource.volume = audioItem.volume * sfxVolume;
                    audiosource.loop = audioItem.loop;
                }
                SFXFound = true;
            }
        }
        if (!SFXFound) Debug.Log("no sfx found with name: " + name);
    }
    public void playSFXWithAudioItem(AudioItem item)
    {
        bool SFXFound = false;

        if (item != null)
        {
            GameObject source = new GameObject();
            source.transform.parent = transform;
            source.name = name;
            AudioSource audiosource = source.AddComponent<AudioSource>();


            int rand = Random.Range(0, item.clip.Length);
            audiosource.PlayOneShot(item.clip[rand]);
            audiosource.volume = item.volume * sfxVolume;
            audiosource.loop = item.loop;



            SFXFound = true;
        }

        if (!SFXFound) Debug.Log("no sfx found with name: " + name);
    }
    GameObject music;
    public void playMusic(string name)
    {

        //create a separate gameobject designated for playing music
        if (music == null)
        {
            music = new GameObject();
            music.name = "Music";
            MusicSource = music.AddComponent<AudioSource>();
        }
        if (coroutineMusic != null) StopCoroutine(coroutineMusic);

        //get music track from trackList
        foreach (AudioItem audioItem in AudioList)
        {
            if (audioItem.name == name)
            {
                MusicSource.clip = audioItem.clip[0];
                MusicSource.loop = true;
                MusicSource.volume = audioItem.volume * musicVolume;
                MusicSource.ignoreListenerPause = false;
                MusicSource.Play();
            }
        }
    }
    AudioItem tempAudioItem;
    Coroutine coroutineMusic;
    public void PlayMusicFromIntro(string name)
    {
        if (music == null)
        {
            music = new GameObject();
            music.name = "Music";
            MusicSource = music.AddComponent<AudioSource>();
        }


        if (coroutineMusic != null) StopCoroutine(coroutineMusic);
        //get music track from trackList
        foreach (AudioItem audioItem in AudioList)
        {
            if (audioItem.name == name)
            {
                tempAudioItem = audioItem;

            }
        }
        coroutineMusic = StartCoroutine(playMusicFromIntro());

        // MusicSource.loop = true;
        // MusicSource.volume = tempAudioItem.volume * musicVolume;
        // MusicSource.ignoreListenerPause = false;
        // MusicSource.Play();

    }
    IEnumerator playMusicFromIntro()
    {
        MusicSource.clip = tempAudioItem.clip[0];
        MusicSource.loop = true;
        MusicSource.volume = tempAudioItem.volume * musicVolume;
        MusicSource.ignoreListenerPause = false;
        MusicSource.Play();
        yield return new WaitForSeconds(MusicSource.clip.length);
        MusicSource.clip = tempAudioItem.clip[1];
        MusicSource.Play();
    }

    private Dictionary<string, Queue<AudioSource>> audioSourcePool = new Dictionary<string, Queue<AudioSource>>();
    private AudioSource GetPooledAudioSource(string name)
    {
        if (audioSourcePool.TryGetValue(name, out Queue<AudioSource> pool) && pool.Count > 0)
        {
            var source = pool.Dequeue();
            source.gameObject.SetActive(true);
            return source;
        }

        // Create a new AudioSource if none are available in the pool
        GameObject sourceObj = new GameObject(name);
        sourceObj.transform.parent = transform;
        AudioSource newSource = sourceObj.AddComponent<AudioSource>();
        return newSource;
    }
    private void ReturnAudioSourceToPool(string name, AudioSource source)
    {
        source.Stop();
        source.gameObject.SetActive(false);

        if (!audioSourcePool.ContainsKey(name))
        {
            audioSourcePool[name] = new Queue<AudioSource>();
        }

        audioSourcePool[name].Enqueue(source);
    }
    private IEnumerator ReturnToPoolAfterPlay(AudioSource source, string name, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnAudioSourceToPool(name, source);
    }

}
