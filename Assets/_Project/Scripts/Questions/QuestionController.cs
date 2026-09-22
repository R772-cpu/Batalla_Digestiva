using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva
{
    public sealed class QuestionController : MonoBehaviour
    {
        public QuestionData[] bank;
        public GameObject panel;
        public Text questionText, feedbackText, progressText;
        public Button[] answerButtons;
        public Text[] answerLabels;
        public Button continueButton;
        public AnswerMark answerMark;
        [Header("Diseño de referencia")]
        public bool referenceLayout;
        public Text ribbonText, questionScoreText;
        public Text[] answerLetters;
        public Image answerArtwork;
        public Sprite blueAnswerSprite, greenAnswerSprite, redAnswerSprite, correctArtwork, incorrectArtwork;
        public bool IsOpen => progress.IsOpen;
        public int Correct => progress.Correct;
        public int Asked => progress.Asked;
        public event Action<bool> AnswerAccepted;
        public event Action Continued;
        private readonly QuestionProgress progress = new QuestionProgress();
        private readonly List<QuestionData> roundBank = new List<QuestionData>();
        public Color normalAnswerColor = new Color(0, 0.46f, 0.7f);
        private QuestionData current;
        private const string HistoryKey = "BatallaDigestiva.QuestionHistory.v1";
        [Serializable] private class History { public List<string> used = new List<string>(); public string last; }

        private void Awake()
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                int index = i;
                answerButtons[i].onClick.AddListener(() => Answer(index));
            }
            continueButton.onClick.AddListener(Continue);
        }

        public void ResetRound()
        {
            roundBank.Clear();
            var ids = new HashSet<string>();
            if (bank != null)
                foreach (var question in bank)
                {
                    if (question == null || !question.IsValid) continue;
                    string key = string.IsNullOrWhiteSpace(question.id) ? question.question : question.id;
                    if (ids.Add(key)) roundBank.Add(question);
                }
            for (int i = roundBank.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                var temp = roundBank[i]; roundBank[i] = roundBank[j]; roundBank[j] = temp;
            }
            progress.Reset(roundBank.Count > 0);
            current = null;
            panel.SetActive(false);
        }

        public bool OpenFinalQuestion()
        {
            if (!progress.TryOpen()) return false;
            History history;
            try { history = JsonUtility.FromJson<History>(PlayerPrefs.GetString(HistoryKey, "{}")) ?? new History(); }
            catch (ArgumentException) { history = new History(); }
            var rotation = new QuestionRotation();
            if (history.used != null) foreach (var id in history.used) rotation.Used.Add(id);
            rotation.Last = history.last;
            Func<QuestionData, string> key = data => string.IsNullOrWhiteSpace(data.id) ? data.question : data.id;
            string chosen = rotation.Choose(roundBank.Select(key), UnityEngine.Random.Range(0, int.MaxValue));
            current = roundBank.First(data => key(data) == chosen);
            history.used = rotation.Used.ToList(); history.last = rotation.Last;
            PlayerPrefs.SetString(HistoryKey, JsonUtility.ToJson(history)); PlayerPrefs.Save();
            var question = current;
            questionText.text = question.question;
            questionText.gameObject.SetActive(true);
            if (ribbonText != null) ribbonText.text = "Responde la\npregunta";
            if (answerArtwork != null) answerArtwork.gameObject.SetActive(false);
            if (answerMark != null) answerMark.gameObject.SetActive(false);
            progressText.text = referenceLayout ? "" : "PREGUNTA FINAL";
            feedbackText.text = referenceLayout ? "" : "La ronda termino. Elige una respuesta.";
            for (int i = 0; i < 3; i++)
            {
                bool separateLetter = answerLetters != null && answerLetters.Length == 3;
                answerLabels[i].text = (separateLetter ? "" : ((char)('A' + i)) + ")  ") + question.answers[i];
                answerButtons[i].interactable = true;
                SetAnswerStyle(i, blueAnswerSprite, normalAnswerColor);
            }
            continueButton.gameObject.SetActive(false);
            panel.SetActive(true);
            return true;
        }

        private void Answer(int index)
        {
            if (!progress.IsOpen || index < 0 || index >= 3) return;
            var question = current;
            bool correct = index == question.correctAnswerIndex;
            if (!progress.TryAnswer(correct)) return;
            for (int i = 0; i < 3; i++) answerButtons[i].interactable = false;
            var artwork = correct ? correctArtwork : incorrectArtwork;
            if (answerArtwork != null && artwork != null)
            { answerArtwork.sprite = artwork; answerArtwork.gameObject.SetActive(true); }
            else if (answerMark != null) answerMark.Show(correct);
            if (referenceLayout)
            {
                questionText.gameObject.SetActive(false);
                if (ribbonText != null) ribbonText.text = correct ? "CORRECTO" : "INCORRECTO";
            }
            else
            {
                answerLabels[question.correctAnswerIndex].text += "  · CORRECTA";
                if (!correct) answerLabels[index].text += "  · TU RESPUESTA";
            }
            SetAnswerStyle(question.correctAnswerIndex, greenAnswerSprite, new Color(0.18f, 0.6f, 0.16f));
            if (!correct) SetAnswerStyle(index, redAnswerSprite, new Color(0.72f, 0.1f, 0.18f));
            feedbackText.text = correct ? "CORRECTO" : "INCORRECTO\nLa respuesta correcta es " + ((char)('A' + question.correctAnswerIndex)) + ".";
            AnswerAccepted?.Invoke(correct);
            continueButton.gameObject.SetActive(true);
        }

        public void ShowBonus(int points)
        {
            feedbackText.text = "CORRECTO\n+" + points + " puntos";
        }

        public void SetScore(int score) { if (questionScoreText != null) questionScoreText.text = score.ToString("N0"); }

        private void SetAnswerStyle(int index, Sprite sprite, Color fallback)
        {
            if (sprite != null)
            { answerButtons[index].image.sprite = sprite; answerButtons[index].image.color = Color.white; }
            else answerButtons[index].image.color = fallback;
        }

        private void Continue()
        {
            if (!progress.TryContinue()) return;
            panel.SetActive(false);
            Continued?.Invoke();
        }
    }
}
