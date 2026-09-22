using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva
{
    [RequireComponent(typeof(Button))]
    public sealed class CornerControl : MonoBehaviour
    {
        public GameController game;
        public bool sound;
        public Sprite enabledIcon, mutedIcon;
        private const string MuteKey = "BatallaDigestiva.SoundMuted";
        private static event System.Action Changed;
        private void Awake() { GetComponent<Button>().onClick.AddListener(Click); }
        private void OnEnable() { Changed += Refresh; GameAudio.Changed += Refresh; Refresh(); }
        private void OnDisable() { Changed -= Refresh; GameAudio.Changed -= Refresh; }
        private void Click()
        {
            if (sound)
            {
                if (GameAudio.Instance != null) { GameAudio.Instance.ToggleSettings(); return; }
                PlayerPrefs.SetInt(MuteKey, PlayerPrefs.GetInt(MuteKey, 0) == 0 ? 1 : 0);
                PlayerPrefs.Save(); Changed?.Invoke();
            }
            else
            {
                var how = game.GetComponent<HowToPlayPanel>();
                if (how != null) how.panel.SetActive(false);
                game.ShowMenu();
            }
        }
        private void Refresh()
        {
            if (!sound) return;
            bool muted = PlayerPrefs.GetInt(MuteKey, 0) != 0;
            AudioListener.volume = muted ? 0 : 1;
            var icon = muted && mutedIcon != null ? mutedIcon : enabledIcon;
            var image = GetComponent<Image>();
            if (icon != null) { image.sprite = icon; image.color = muted && mutedIcon == null ? Color.gray : Color.white; }
            var label = GetComponentInChildren<Text>(true);
            if (label != null) { label.gameObject.SetActive(icon == null); label.text = muted ? "SIN AUDIO" : "SONIDO"; }
        }
    }
}
