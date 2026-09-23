using System;
using System.Collections.Generic;
using UnityEngine;

namespace BatallaDigestiva
{
    public sealed class TargetSpawner : MonoBehaviour
    {
        public RectTransform playArea;
        public TargetView targetPrefab;
        public CharacterData[] characters;
        public RectTransform[] excludedUI;
        private readonly List<TargetView> targets = new List<TargetView>();
        private readonly List<CharacterData> bag = new List<CharacterData>();
        private float nextSpawn;
        private CharacterData groupedCharacter;
        private int groupRemaining;
        private Func<TargetView, bool> hitHandler;
        public List<TargetView> ActiveTargets => targets.FindAll(target => target.State == TargetView.TargetState.Active);
        public List<TargetView> VisibleTargets => targets.FindAll(target => target.State == TargetView.TargetState.Active || target.State == TargetView.TargetState.Spawning);
        public Rect? PowerUpSpace { get; set; }

        public void Clear()
        {
            foreach (var target in targets)
                if (target != null) { target.gameObject.SetActive(false); Destroy(target.gameObject); }
            targets.Clear(); bag.Clear(); nextSpawn = 0;
            groupRemaining = 0; groupedCharacter = null;
            PowerUpSpace = null;
            hitHandler = null;
        }

        public void Seed(GameConfig config, Func<TargetView, bool> hit)
        {
            hitHandler = hit;
            for (int i = 0; i < Mathf.Min(config.initialTargets, config.maxTargets); i++) Spawn(config, hit);
            nextSpawn = Mathf.Max(0.1f, config.spawnInterval);
        }

        public void Tick(float delta, GameConfig config, Func<TargetView, bool> hit, float roundProgress = 0, Func<bool> tryPowerUp = null)
        {
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                targets[i].Tick(delta);
                if (!targets[i].IsFinished) continue;
                Destroy(targets[i].gameObject);
                targets.RemoveAt(i);
            }
            nextSpawn -= delta;
            if (nextSpawn > 0) return;
            nextSpawn = Mathf.Max(0.1f, config.spawnInterval * Mathf.Lerp(1, config.finalSpawnIntervalMultiplier, roundProgress));
            for (int i = 0; i < config.spawnBatch; i++)
            {
                // Un producto ocupa una oportunidad de aparicion en lugar de un personaje.
                if (tryPowerUp != null && tryPowerUp()) continue;
                if (targets.Count < Mathf.Max(1, config.maxTargets)) Spawn(config, hit);
            }
        }

        public bool TryFindSpace(Vector2 size, out Vector2 position)
        {
            var bounds = playArea.rect;
            position = default;
            if (size.x > bounds.width || size.y > bounds.height) return false;
            var exclusions = new List<Rect>();
            if (excludedUI != null)
            {
                var corners = new Vector3[4];
                foreach (var ui in excludedUI)
                {
                    if (ui == null || !ui.gameObject.activeInHierarchy) continue;
                    ui.GetWorldCorners(corners);
                    var min = new Vector2(float.MaxValue, float.MaxValue);
                    var max = new Vector2(float.MinValue, float.MinValue);
                    foreach (var corner in corners)
                    {
                        Vector2 local = playArea.InverseTransformPoint(corner);
                        min = Vector2.Min(min, local); max = Vector2.Max(max, local);
                    }
                    exclusions.Add(Rect.MinMaxRect(min.x, min.y, max.x, max.y));
                }
            }
            float bestDistance = -1;
            for (int attempt = 0; attempt < 100; attempt++)
            {
                var candidate = new Vector2(UnityEngine.Random.Range(bounds.xMin + size.x / 2, bounds.xMax - size.x / 2),
                    UnityEngine.Random.Range(bounds.yMin + size.y / 2, bounds.yMax - size.y / 2));
                var space = new Rect(candidate - size / 2 - Vector2.one * 24, size + Vector2.one * 48);
                if (PowerUpSpace.HasValue && space.Overlaps(PowerUpSpace.Value)) continue;
                if (exclusions.Exists(exclusion => space.Overlaps(exclusion))) continue;
                bool fits = true;
                float nearest = PowerUpSpace.HasValue ? (candidate - PowerUpSpace.Value.center).sqrMagnitude : float.MaxValue;
                foreach (var other in targets)
                {
                    var otherRect = new Rect(other.Rect.anchoredPosition - other.Rect.sizeDelta / 2, other.Rect.sizeDelta);
                    if (space.Overlaps(otherRect)) { fits = false; break; }
                    nearest = Mathf.Min(nearest, (candidate - other.Rect.anchoredPosition).sqrMagnitude);
                }
                // Compare free positions instead of accepting the first random gap.
                if (fits && nearest > bestDistance)
                {
                    bestDistance = nearest;
                    position = candidate;
                }
            }
            return bestDistance >= 0;
        }

        private void Spawn(GameConfig config, Func<TargetView, bool> hit)
        {
            var bounds = playArea.rect;
            float size = Mathf.Min(Mathf.Max(50, config.targetSize), bounds.width * 0.3f, bounds.height * 0.42f);
            if (size < 20 || targetPrefab == null) return;
            if (!TryFindSpace(Vector2.one * size, out var position)) return;
            if (groupRemaining == 0 && bag.Count == 0 && characters != null)
                foreach (var data in characters)
                    if (data != null && data.normalSprite != null) bag.Add(data);
            if (groupRemaining == 0)
            {
                if (bag.Count == 0) return;
                int index = UnityEngine.Random.Range(0, bag.Count);
                groupedCharacter = bag[index]; bag.RemoveAt(index);
                groupRemaining = Mathf.Max(1, config.sameProductGroup);
            }
            var character = groupedCharacter;
            groupRemaining--;
            CreateTarget(character, config, hit, position, size);
        }

        public bool EnsurePowerUpTarget(CharacterData character, GameConfig config, bool activating)
        {
            if (VisibleTargets.Exists(target => target.Character.productId == character.productId)) return true;
            if (hitHandler == null || targetPrefab == null) return false;
            float size = Mathf.Min(config.targetSize, playArea.rect.width * .3f, playArea.rect.height * .42f);
            if (size < 20) return false;
            Vector2 position;
            if (VisibleTargets.Count >= config.maxTargets || !TryFindSpace(Vector2.one * size, out position))
            {
                // On activation the product frees its reserved footprint. Use that free
                // space for the last reinforcement instead of consuming an empty pickup.
                if (!activating || !PowerUpSpace.HasValue) return false;
                position = PowerUpSpace.Value.center;
                size = Mathf.Min(size, PowerUpSpace.Value.width, PowerUpSpace.Value.height);
            }
            CreateTarget(character, config, hitHandler, position, size);
            return true;
        }

        private void CreateTarget(CharacterData character, GameConfig config, Func<TargetView, bool> hit, Vector2 position, float size)
        {
            var target = Instantiate(targetPrefab, playArea);
            target.Rect.anchorMin = target.Rect.anchorMax = new Vector2(0.5f, 0.5f);
            target.Rect.anchoredPosition = position;
            target.Rect.sizeDelta = Vector2.one * size;
            target.Initialize(character, Mathf.Max(0.2f, config.targetLifetime), hit, config.hitHoldDuration, config.hitFadeDuration);
            targets.Add(target);
            var audio = GameAudio.Instance;
            if (audio != null) audio.Play(audio.spawn, .5f);
        }
    }
}
