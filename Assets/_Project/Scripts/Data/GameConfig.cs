using UnityEngine;

namespace BatallaDigestiva
{
    [CreateAssetMenu(menuName = "Batalla Digestiva/Configuracion")]
    public sealed class GameConfig : ScriptableObject
    {
        [Min(1)] public float roundDuration = 60;
        [Min(1)] public int pointsPerTarget = 10;
        [Min(0.1f)] public float spawnInterval = 1.6f;
        [Min(0.2f)] public float targetLifetime = 11;
        [Range(1, 30)] public int maxTargets = 11;
        [Min(50)] public float targetSize = 150;
        [Header("Pregunta final")]
        [Min(0)] public int correctAnswerBonus = 20;
        [Header("Medallas y potenciadores")]
        [Min(1)] public int targetsPerMedal = 5;
        [Range(1, 4)] public int spawnBatch = 2;
        [Range(1, 4)] public int sameProductGroup = 3;
        [Range(0, 20)] public int initialTargets = 4;
        [Range(0.3f, 1)] public float finalSpawnIntervalMultiplier = 1f;
        [Min(1)] public float powerUpInterval = 8;
        [Min(0.5f)] public float powerUpLifetime = 8;
        public Sprite bubbleSprite;
        [Header("Personaje aplastado")]
        [Min(0)] public float hitHoldDuration = 0.5f;
        [Min(0.05f)] public float hitFadeDuration = 0.4f;
    }
}

