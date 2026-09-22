using UnityEngine;

namespace BatallaDigestiva
{
    [CreateAssetMenu(menuName = "Batalla Digestiva/Pregunta")]
    public sealed class QuestionData : ScriptableObject
    {
        public string id;
        [TextArea(2, 5)] public string question;
        public string[] answers = new string[3];
        [Range(0, 2)] public int correctAnswerIndex;

        public bool IsValid => !string.IsNullOrWhiteSpace(question) && answers != null &&
            answers.Length == 3 && correctAnswerIndex >= 0 && correctAnswerIndex < 3 &&
            !string.IsNullOrWhiteSpace(answers[0]) && !string.IsNullOrWhiteSpace(answers[1]) &&
            !string.IsNullOrWhiteSpace(answers[2]);
    }
}
