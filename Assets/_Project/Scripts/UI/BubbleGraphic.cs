using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva
{
    // Burbuja provisional vectorial. Puede reemplazarse por el PNG del arte final.
    public sealed class BubbleGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = rectTransform.rect;
            float radius = Mathf.Min(rect.width, rect.height) * 0.49f;
            for (int i = 0; i < 64; i++)
            {
                float angle = i * Mathf.PI * 2 / 64;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                vh.AddVert(rect.center + direction * radius, color, Vector2.zero);
                vh.AddVert(rect.center + direction * radius * 0.91f, new Color(color.r, color.g, color.b, color.a * 0.25f), Vector2.zero);
                int next = ((i + 1) % 64) * 2;
                vh.AddTriangle(i * 2, next, i * 2 + 1);
                vh.AddTriangle(i * 2 + 1, next, next + 1);
            }
        }
    }
}
