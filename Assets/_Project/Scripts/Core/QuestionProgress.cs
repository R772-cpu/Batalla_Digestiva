using System;

namespace BatallaDigestiva
{
    // Una pregunta final: una respuesta y una transicion a resultados por ronda.
    public sealed class QuestionProgress
    {
        public int Asked { get; private set; }
        public int Correct { get; private set; }
        public bool IsOpen { get; private set; }
        public bool Answered { get; private set; }
        private bool hasQuestion;

        public void Reset(bool available)
        {
            hasQuestion = available;
            Asked = Correct = 0;
            IsOpen = Answered = false;
        }

        public bool TryOpen()
        {
            if (IsOpen || Asked > 0 || !hasQuestion) return false;
            Asked++;
            IsOpen = true;
            Answered = false;
            return true;
        }

        public bool TryAnswer(bool correct)
        {
            if (!IsOpen || Answered) return false;
            Answered = true;
            if (correct) Correct++;
            return true;
        }

        public bool TryContinue()
        {
            if (!IsOpen || !Answered) return false;
            IsOpen = false;
            return true;
        }
    }
}
