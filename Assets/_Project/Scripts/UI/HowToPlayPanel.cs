using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva
{
    public sealed class HowToPlayPanel : MonoBehaviour
    {
        public GameController game;
        public GameObject panel;
        public Button openButton, backButton, playButton;
        private void Start()
        {
            openButton.onClick.AddListener(Open);
            backButton.onClick.AddListener(Close);
            playButton.onClick.AddListener(Play);
            panel.SetActive(false);
        }
        private void Open() { game.menuPanel.SetActive(false); panel.SetActive(true); }
        private void Close() { panel.SetActive(false); game.menuPanel.SetActive(true); }
        private void Play() { panel.SetActive(false); game.StartRound(); }
    }
}
