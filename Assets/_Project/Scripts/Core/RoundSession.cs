using System;

namespace BatallaDigestiva
{
    // Logica independiente de Unity: el reloj y el puntaje tienen un solo propietario.
    public sealed class RoundSession
    {
        public int Score { get; private set; }
        public int Hits { get; private set; }
        public int EliminationScore { get; private set; }
        public float Remaining { get; private set; }
        public bool Playing { get; private set; }
        public bool Paused { get; private set; }
        private bool finalBonusGranted;

        public void Start(float duration)
        {
            Score = Hits = EliminationScore = 0;
            Remaining = Math.Max(0, duration);
            Playing = Remaining > 0;
            Paused = false;
            finalBonusGranted = false;
        }

        public void Tick(float delta)
        {
            if (!Playing || Paused) return;
            Remaining = Math.Max(0, Remaining - Math.Max(0, delta));
            if (Remaining <= 0) Playing = false;
        }

        public bool RegisterHit(int points)
        {
            if (!Playing || Paused) return false;
            Score += Math.Max(0, points);
            EliminationScore += Math.Max(0, points);
            Hits++;
            return true;
        }

        public void SetPaused(bool paused) { Paused = Playing && paused; }
        public bool AwardFinalBonus(int points)
        {
            if (Playing || Remaining > 0 || finalBonusGranted) return false;
            finalBonusGranted = true;
            Score += Math.Max(0, points);
            return true;
        }
        public void Stop() { Playing = false; Paused = false; }
    }
}
