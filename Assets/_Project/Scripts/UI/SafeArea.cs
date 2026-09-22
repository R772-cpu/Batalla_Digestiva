using UnityEngine;

namespace BatallaDigestiva
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour
    {
        private void Update()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            var area = Screen.safeArea;
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            rect.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
