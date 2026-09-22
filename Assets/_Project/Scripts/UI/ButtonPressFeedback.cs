using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BatallaDigestiva
{
    [RequireComponent(typeof(Button))]
    public sealed class ButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private RectTransform visual;
        private Vector3 original;
        private bool pressed;
        private void Awake()
        {
            var text = GetComponentInChildren<Text>(true);
            if (text != null) { visual = text.rectTransform; original = visual.localScale; }
        }
        public void OnPointerDown(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left && GetComponent<Button>().IsInteractable()) pressed = true;
        }
        public void OnPointerUp(PointerEventData data) { pressed = false; }
        public void OnPointerExit(PointerEventData data) { pressed = false; }
        private void Update()
        {
            if (visual == null) return;
            visual.localScale = Vector3.Lerp(visual.localScale, original * (pressed ? 0.94f : 1), 1 - Mathf.Exp(-25 * Time.unscaledDeltaTime));
        }
        private void OnDisable() { pressed = false; if (visual != null) visual.localScale = original; }
    }
}
