using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BatallaDigestiva
{
    [RequireComponent(typeof(Image))]
    public sealed class TargetView : MonoBehaviour, IPointerDownHandler, ICanvasRaycastFilter
    {
        public enum TargetState { Spawning, Active, Hit, Expired }
        public TargetState State { get; private set; }
        public RectTransform Rect => (RectTransform)transform;
        public bool IsFinished => State == TargetState.Expired;
        public CharacterData Character => character;
        private Graphic bubble;
        private bool powerHit;
        private Image image;
        private CharacterData character;
        private float age, lifetime, hitAge;
        private float hitHoldDuration, hitFadeDuration;
        private Func<TargetView, bool> onHit;
        private readonly List<Vector2[]> hitShapes = new List<Vector2[]>();

        public void Initialize(CharacterData data, float activeLifetime, Func<TargetView, bool> hit, float holdDuration = 0.5f, float fadeDuration = 0.4f)
        {
            image = GetComponent<Image>();
            character = data;
            image.sprite = data.normalSprite;
            image.preserveAspect = true;
            image.raycastTarget = true;
            image.color = Color.white;
            hitShapes.Clear();
            if (data.useSpritePhysicsShape && data.normalSprite != null)
            {
                var points = new List<Vector2>();
                for (int i = 0; i < data.normalSprite.GetPhysicsShapeCount(); i++)
                {
                    points.Clear();
                    data.normalSprite.GetPhysicsShape(i, points);
                    if (points.Count >= 3) hitShapes.Add(points.ToArray());
                }
            }
            lifetime = activeLifetime;
            hitHoldDuration = Mathf.Max(0, holdDuration);
            hitFadeDuration = Mathf.Max(0.05f, fadeDuration);
            onHit = hit;
            age = hitAge = 0;
            State = TargetState.Spawning;
            powerHit = false;
            transform.localScale = Vector3.one * 0.1f;
        }

        // Lo llama el spawner solo mientras la ronda avanza; pausa tambien la expiracion.
        public void Tick(float delta)
        {
            if (State == TargetState.Expired) return;
            if (State == TargetState.Hit)
            {
                hitAge += delta;
                float fade = powerHit ? 1 - Mathf.Clamp01(hitAge / 0.6f) :
                    1 - Mathf.Clamp01((hitAge - hitHoldDuration) / hitFadeDuration);
                image.color = new Color(1, 1, 1, fade);
                if (powerHit)
                {
                    transform.localScale = Vector3.one * (1 + hitAge * 0.35f);
                    Rect.anchoredPosition += Vector2.up * delta * 35;
                    if (bubble != null) bubble.color = new Color(0.75f, 0.94f, 1, fade);
                }
                if (fade <= 0) State = TargetState.Expired;
                return;
            }
            age += delta;
            if (State == TargetState.Spawning)
            {
                transform.localScale = Vector3.one * Mathf.Lerp(0.1f, 1, age / 0.3f);
                if (age >= 0.3f) { State = TargetState.Active; age = 0; }
            }
            else
            {
                transform.localScale = Vector3.one * (1 + Mathf.Sin(age * 2) * 0.025f);
                if (age >= lifetime) { State = TargetState.Expired; image.raycastTarget = false; }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            TryHit(false, null);
        }

        public bool TryHit(bool byProduct, Sprite bubbleSprite)
        {
            bool hittable = State == TargetState.Active || (byProduct && State == TargetState.Spawning);
            if (!hittable || onHit == null || !onHit(this)) return false;
            State = TargetState.Hit;
            image.raycastTarget = false;
            powerHit = byProduct;
            var audio = GameAudio.Instance;
            if (audio != null) audio.Play(byProduct ? audio.bubble : audio.squash, byProduct ? .5f : 1);
            if (byProduct)
            {
                transform.localScale = Vector3.one;
                var obj = new GameObject("Burbuja", typeof(RectTransform));
                var rect = (RectTransform)obj.transform;
                rect.SetParent(transform, false);
                rect.anchorMin = new Vector2(-0.08f, -0.08f); rect.anchorMax = new Vector2(1.08f, 1.08f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                if (bubbleSprite != null)
                {
                    var visual = obj.AddComponent<Image>(); visual.sprite = bubbleSprite; visual.preserveAspect = true; bubble = visual;
                }
                else bubble = obj.AddComponent<BubbleGraphic>();
                bubble.raycastTarget = false;
                bubble.color = new Color(0.75f, 0.94f, 1, 1);
            }
            else if (character.hitSprite != null)
            {
                image.sprite = character.hitSprite;
                transform.localScale = Vector3.one;
            }
            else transform.localScale = new Vector3(1.1f, 0.55f, 1);
            return true;
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (State != TargetState.Active || image == null) return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, screenPoint, eventCamera, out var local)) return false;
            var sprite = character.normalSprite;
            if (sprite == null) return false;
            // Image.preserveAspect centra el sprite en el rectangulo usando su pivot.
            var bounds = Rect.rect;
            float scale = Mathf.Min(bounds.width / sprite.rect.width, bounds.height / sprite.rect.height);
            if (scale <= 0) return false;
            Vector2 size = sprite.rect.size * scale;
            Vector2 origin = bounds.min + Vector2.Scale(bounds.size - size, Rect.pivot);
            Vector2 pixelPoint = (local - origin) / scale;
            if (pixelPoint.x < 0 || pixelPoint.y < 0 || pixelPoint.x > sprite.rect.width || pixelPoint.y > sprite.rect.height) return false;
            if (hitShapes.Count == 0) return true;
            Vector2 shapePoint = (pixelPoint - sprite.pivot) / sprite.pixelsPerUnit;
            foreach (var polygon in hitShapes)
                if (ContainsPoint(polygon, shapePoint)) return true;
            return false;
        }

        private static bool ContainsPoint(Vector2[] polygon, Vector2 point)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                var a = polygon[j];
                var b = polygon[i];
                // Incluir el borde evita rechazar toques exactamente sobre un segmento.
                var segment = b - a;
                float t = segment.sqrMagnitude > 0 ? Mathf.Clamp01(Vector2.Dot(point - a, segment) / segment.sqrMagnitude) : 0;
                if ((point - (a + segment * t)).sqrMagnitude < 0.000001f) return true;
                if ((a.y > point.y) != (b.y > point.y) &&
                    point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
            }
            return inside;
        }
    }
}
