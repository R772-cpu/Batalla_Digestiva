using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva
{
    public sealed class MedalHUD : MonoBehaviour
    {
        public CharacterData[] characters;
        public Image[] icons;
        public Text[] labels;
        public bool resultsMode;
        public bool useProductPortraits = true;
        public Text[] productTitles;
        private int[] previous;
        private float[] pulse;
        public void Refresh(MedalProgress progress)
        {
            if (previous == null || previous.Length != characters.Length)
            { previous = new int[characters.Length]; pulse = new float[characters.Length]; }
            for (int i = 0; i < characters.Length; i++)
            {
                var data = characters[i];
                int medals = progress.Medals(data.productId);
                if (medals > previous[i]) pulse[i] = 0.65f;
                previous[i] = medals;
                icons[i].sprite = resultsMode && useProductPortraits && data.productSprite != null ? data.productSprite :
                    data.medalSprite != null ? data.medalSprite : data.normalSprite;
                icons[i].color = !resultsMode || medals > 0 || useProductPortraits ? Color.white : new Color(0.55f, 0.55f, 0.55f, 0.6f);
                bool separateTitle = productTitles != null && productTitles.Length == characters.Length;
                if (separateTitle) productTitles[i].text = data.displayName;
                labels[i].text = resultsMode ? (separateTitle ? "Medallas x" : data.displayName + " · Medallas x") + medals + "\n" + data.shortDescription :
                    (separateTitle ? "x" : data.displayName + "\nx") + medals + " · " + progress.TowardsNext(data.productId) + "/" + progress.Required;
            }
        }
        private void Update()
        {
            if (pulse == null) return;
            for (int i = 0; i < pulse.Length; i++)
            {
                pulse[i] = Mathf.Max(0, pulse[i] - Time.unscaledDeltaTime);
                icons[i].transform.localScale = Vector3.one * (1 + Mathf.Sin(pulse[i] / 0.65f * Mathf.PI) * 0.22f);
            }
        }
    }
}
