using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip[] clipsArray;
    private Dictionary<string, AudioClip> clips;
    private AudioSource audioSource;
    public bool IsSoundEnabled = true;

    void Awake()
    {
        IsSoundEnabled = false;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            clips = new Dictionary<string, AudioClip>();
            foreach (var clip in clipsArray)
                clips[clip.name] = clip;

            bool soundOn = PlayerPrefs.GetInt("SoundsEffectsToggle", 1) == 1;
            SetSoundEnabled(soundOn);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Play(string key)
    {
        if (IsSoundEnabled && clips.ContainsKey(key))
            audioSource.PlayOneShot(clips[key]);
    }

    public void SetSoundEnabled(bool enabled)
    {
        IsSoundEnabled = enabled;
    }
    public void BindButtons()
    {
        TryBind("Continue");
        TryBind("NewGame");

        TryBind("BackButton");
        TryBind("BackСolors");
        TryBind("SettingsButton");
        TryBind("TimerPause");
    }

    private void TryBind(string btnName)
    {
        var btnObj = GameObject.Find(btnName);
        if (btnObj != null)
        {
            var btn = btnObj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => Play("switchsound1"));
        }
    }
}
