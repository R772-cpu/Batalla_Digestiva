using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva
{
    [DefaultExecutionOrder(-100)]
    public sealed class GameAudio : MonoBehaviour
    {
        public static GameAudio Instance { get; private set; }
        public AudioClip spawn, squash, buttons, bubble, music, powerUp, victory, correct, wrong;
        public GameObject settingsPanel;
        public Slider musicSlider, effectsSlider;
        public Button muteButton, closeButton;
        public Text muteLabel;
        private AudioSource musicSource, effectsSource;
        public const string MuteKey = "BatallaDigestiva.SoundMuted";
        private const string MusicKey = "BatallaDigestiva.MusicVolume";
        private const string EffectsKey = "BatallaDigestiva.EffectsVolume";
        public static event System.Action Changed;
        public static bool Muted => PlayerPrefs.GetInt(MuteKey, 0) != 0;
        private void Awake()
        {
            Instance = this;
            musicSource = gameObject.AddComponent<AudioSource>();
            effectsSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = effectsSource.playOnAwake = false;
            musicSource.spatialBlend = effectsSource.spatialBlend = 0;
            musicSource.loop = true; musicSource.clip = music;
            musicSlider.SetValueWithoutNotify(Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, .5f)));
            effectsSlider.SetValueWithoutNotify(Mathf.Clamp01(PlayerPrefs.GetFloat(EffectsKey, 1)));
            musicSlider.onValueChanged.AddListener(value => { PlayerPrefs.SetFloat(MusicKey, value); Apply(); });
            effectsSlider.onValueChanged.AddListener(value => { PlayerPrefs.SetFloat(EffectsKey, value); Apply(); });
            muteButton.onClick.AddListener(ToggleMute);
            closeButton.onClick.AddListener(CloseSettings);
            settingsPanel.SetActive(false);
            Apply();
            if (music != null) musicSource.Play();
        }
        private void Apply()
        {
            AudioListener.volume = Muted ? 0 : 1;
            musicSource.volume = musicSlider.value;
            effectsSource.volume = effectsSlider.value;
            muteLabel.text = Muted ? "ACTIVAR SONIDO" : "SILENCIAR";
            Changed?.Invoke();
        }
        public void ToggleMute()
        {
            PlayerPrefs.SetInt(MuteKey, Muted ? 0 : 1); Apply(); PlayerPrefs.Save();
        }
        public void ToggleSettings()
        {
            bool open = !settingsPanel.activeSelf;
            settingsPanel.SetActive(open);
            if (open) settingsPanel.transform.SetAsLastSibling();
            else PlayerPrefs.Save();
        }
        public void CloseSettings() { settingsPanel.SetActive(false); PlayerPrefs.Save(); }
        public void Play(AudioClip clip, float volume = 1)
        {
            if (clip != null && !Muted) effectsSource.PlayOneShot(clip, volume);
        }
        private void OnApplicationPause(bool paused) { if (paused) PlayerPrefs.Save(); }
        private void OnDestroy() { if (Instance == this) Instance = null; PlayerPrefs.Save(); }
    }
}
