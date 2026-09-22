using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva
{
    [RequireComponent(typeof(Button))]
    public sealed class UIButtonAudio : MonoBehaviour
    {
        private void Awake() { GetComponent<Button>().onClick.AddListener(Play); }
        private void Play()
        {
            var audio = GameAudio.Instance;
            if (audio != null) audio.Play(audio.buttons);
        }
    }
}
