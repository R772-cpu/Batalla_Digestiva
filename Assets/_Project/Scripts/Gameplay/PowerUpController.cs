using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva
{
    public sealed class PowerUpController : MonoBehaviour
    {
        public Button button;
        public Image productImage;
        public Image characterImage;
        public Text label;
        public Sprite sparkleSprite;
        private Image sparkles;
        private Graphic glow;
        private GameConfig config;
        private TargetSpawner spawner;
        private CharacterData selected;
        private float wait, remaining;
        private bool running;
        private void Awake() { button.onClick.AddListener(Activate); }

        public void ResetRound(GameConfig settings, TargetSpawner source)
        {
            config = settings; spawner = source;
            ConfigureArena(source);
            Clear(); wait = config.powerUpInterval;
        }

        public void Clear()
        {
            selected = null; running = false; button.gameObject.SetActive(false);
            if (spawner != null) spawner.PowerUpSpace = null;
        }
        public void SetRunning(bool value) { running = value; button.interactable = value; }

        public void Tick(float delta)
        {
            if (!running) return;
            if (selected != null)
            {
                remaining -= delta;
                if (remaining <= 0)
                { Hide(); return; }
                spawner.EnsurePowerUpTarget(selected, config, false);
                float pulse = 0.6f + Mathf.Sin((config.powerUpLifetime - remaining) * 7) * 0.25f;
                if (glow != null) glow.color = new Color(1, 0.83f, 0.2f, pulse);
                if (sparkles != null) sparkles.color = new Color(1, 1, 1, pulse);
                return;
            }
            wait -= delta;
        }

        public bool TrySpawnFromSlot()
        {
            if (!running || selected != null || wait > 0) return false;
            var groups = spawner.VisibleTargets.GroupBy(target => target.Character.productId).ToArray();
            if (groups.Length == 0) return false;
            // Preferir grupos de varios, pero permitir uno si el jugador elimina muy rapido.
            var multiple = groups.Where(group => group.Count() >= 2).ToArray();
            var candidates = multiple.Length > 0 ? multiple : groups;
            var rect = (RectTransform)button.transform;
            if (!spawner.TryFindSpace(rect.sizeDelta, out var position)) return false;
            selected = candidates[Random.Range(0, candidates.Length)].First().Character;
            rect.anchoredPosition = position;
            spawner.PowerUpSpace = new Rect(position - rect.sizeDelta / 2, rect.sizeDelta);
            remaining = config.powerUpLifetime;
            productImage.sprite = selected.productSprite;
            productImage.gameObject.SetActive(selected.productSprite != null);
            characterImage.gameObject.SetActive(false);
            label.text = selected.displayName;
            button.gameObject.SetActive(true);
            button.transform.SetAsLastSibling();
            return true;
        }

        private void Activate()
        {
            if (!running || selected == null) return;
            if (!spawner.EnsurePowerUpTarget(selected, config, true)) return;
            string id = selected.productId;
            var audio = GameAudio.Instance;
            if (audio != null) audio.Play(audio.powerUp);
            Hide(); // Invalidar antes de procesar para evitar activaciones dobles.
            foreach (var target in spawner.VisibleTargets)
                if (target.Character.productId == id) target.TryHit(true, config.bubbleSprite);
        }

        private void Hide()
        {
            selected = null; button.gameObject.SetActive(false);
            spawner.PowerUpSpace = null;
            wait = Mathf.Max(1, config.powerUpInterval) * Random.Range(0.85f, 1.15f);
        }

        // Actualiza tambien escenas ya creadas; no depende de reconstruirlas desde el menu.
        public void ConfigureArena(TargetSpawner source)
        {
            var rect = (RectTransform)button.transform;
            rect.SetParent(source.playArea, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(180, 140);
            rect.localScale = Vector3.one;
            button.image.sprite = null;
            button.image.color = Color.clear;
            button.transition = Selectable.Transition.None;
            SetRect(productImage.rectTransform, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.98f));
            if (characterImage == null)
            {
                var obj = new GameObject("PersonajeRelacionado", typeof(RectTransform), typeof(Image));
                obj.transform.SetParent(rect, false);
                characterImage = obj.GetComponent<Image>();
            }
            SetRect(characterImage.rectTransform, new Vector2(0.57f, 0.3f), new Vector2(0.97f, 0.82f));
            characterImage.preserveAspect = productImage.preserveAspect = true;
            characterImage.gameObject.SetActive(false);
            characterImage.raycastTarget = productImage.raycastTarget = false;
            label.raycastTarget = false;
            SetRect(label.rectTransform, new Vector2(0.02f, 0.01f), new Vector2(0.98f, 0.28f));
            label.resizeTextMinSize = 16; label.resizeTextMaxSize = 28;
            label.fontStyle = FontStyle.Bold;
            var glowTransform = rect.Find("Brillo");
            if (glowTransform == null)
            {
                var obj = new GameObject("Brillo", typeof(RectTransform), typeof(BubbleGraphic));
                obj.transform.SetParent(rect, false); obj.transform.SetAsFirstSibling();
                glow = obj.GetComponent<BubbleGraphic>();
            }
            else glow = glowTransform.GetComponent<Graphic>();
            SetRect(glow.rectTransform, Vector2.zero, Vector2.one);
            glow.raycastTarget = false; glow.color = new Color(1, 0.83f, 0.2f, 0.8f);
            var sparkleTransform = rect.Find("Sparkles");
            if (sparkleTransform == null)
            {
                var obj = new GameObject("Sparkles", typeof(RectTransform), typeof(Image));
                obj.transform.SetParent(rect, false);
                sparkles = obj.GetComponent<Image>();
            }
            else sparkles = sparkleTransform.GetComponent<Image>();
            SetRect(sparkles.rectTransform, new Vector2(0, 0.25f), new Vector2(1, 1));
            sparkles.sprite = sparkleSprite; sparkles.preserveAspect = true; sparkles.raycastTarget = false;
            sparkles.gameObject.SetActive(sparkleSprite != null);
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
