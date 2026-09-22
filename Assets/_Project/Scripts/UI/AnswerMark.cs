using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva
{
    public sealed class AnswerMark : MaskableGraphic
    {
        private bool correct;
        public void Show(bool value) { correct = value; color = value ? new Color(1, 0.75f, 0) : new Color(0.8f, 0.12f, 0.2f); SetVerticesDirty(); gameObject.SetActive(true); }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (correct)
            {
                Line(vh, new Vector2(0.15f, 0.5f), new Vector2(0.4f, 0.25f));
                Line(vh, new Vector2(0.4f, 0.25f), new Vector2(0.86f, 0.8f));
            }
            else
            {
                Line(vh, new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.8f));
                Line(vh, new Vector2(0.2f, 0.8f), new Vector2(0.8f, 0.2f));
            }
        }
        private void Line(VertexHelper vh, Vector2 a, Vector2 b)
        {
            var rect = rectTransform.rect;
            a = rect.min + Vector2.Scale(a, rect.size); b = rect.min + Vector2.Scale(b, rect.size);
            var direction = (b - a).normalized;
            var normal = new Vector2(-direction.y, direction.x) * Mathf.Min(rect.width, rect.height) * 0.055f;
            int start = vh.currentVertCount;
            vh.AddVert(a - normal, color, Vector2.zero); vh.AddVert(a + normal, color, Vector2.zero);
            vh.AddVert(b + normal, color, Vector2.zero); vh.AddVert(b - normal, color, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2); vh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
