using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva
{
    public sealed class GameplayFeedback : MonoBehaviour
    {
        public Text pointsTemplate;
        public Text medalNotice;
        private sealed class Point { public Text text; public float age; public Vector2 origin; public bool active; }
        private readonly List<Point> points = new List<Point>();
        private readonly Queue<string> notices = new Queue<string>();
        private float noticeTime;
        private int next;

        private void Awake()
        {
            pointsTemplate.gameObject.SetActive(false);
            for (int i = 0; i < 24; i++)
            {
                var text = Instantiate(pointsTemplate, pointsTemplate.transform.parent);
                text.name = "PuntosFlotantes";
                text.raycastTarget = false;
                points.Add(new Point { text = text });
            }
            Clear();
        }

        public void ShowPoints(Vector2 position, int amount)
        {
            if (points.Count == 0) return;
            var point = points[next]; next = (next + 1) % points.Count;
            point.age = 0; point.active = true; point.origin = position;
            point.text.text = "+" + amount;
            point.text.color = new Color(1, 0.88f, 0.2f, 1);
            point.text.rectTransform.anchoredPosition = position;
            point.text.transform.parent.SetAsLastSibling();
            point.text.transform.SetAsLastSibling();
            point.text.gameObject.SetActive(true);
        }

        public void Medal(string product, int count)
        {
            notices.Enqueue("¡Medalla de " + product + "!  x" + count);
        }

        // Lo actualiza GameController: la pausa congela los mensajes igual que los personajes.
        public void Tick(float delta)
        {
            foreach (var point in points)
            {
                if (!point.active) continue;
                point.age += delta;
                float progress = Mathf.Clamp01(point.age / 0.8f);
                point.text.rectTransform.anchoredPosition = point.origin + Vector2.up * progress * 55;
                point.text.color = new Color(1, 0.88f, 0.2f, 1 - progress);
                if (progress >= 1) { point.active = false; point.text.gameObject.SetActive(false); }
            }
            noticeTime -= delta;
            if (noticeTime > 0) return;
            medalNotice.text = notices.Count > 0 ? notices.Dequeue() : "";
            noticeTime = medalNotice.text.Length > 0 ? 1.5f : 0;
        }

        public void Clear()
        {
            foreach (var point in points) { point.active = false; point.text.gameObject.SetActive(false); }
            notices.Clear(); noticeTime = 0; next = 0;
            medalNotice.text = "";
        }
    }
}
