using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace BatallaDigestiva
{
    public sealed class GameController : MonoBehaviour
    {
        public GameConfig config;
        public TargetSpawner spawner;
        public QuestionController questions;
        public GameplayFeedback feedback;
        public PowerUpController powerUps;
        public MedalHUD medalsHUD, resultsProducts;
        public MedalHUD missionMedals;
        public GameObject missionPanel;
        public Text missionTitle;
        public GameObject missionTitleArtwork;
        public Button missionContinueButton;
        public GameObject menuPanel, gamePanel, resultsPanel, pausePanel;
        public Text scoreText, timerText, resultText;
        public Text resultDetailsText;
        public Button startButton, restartButton, homeButton, pauseButton, resumeButton, resultsHomeButton;
        private readonly RoundSession session = new RoundSession();
        private bool manualPause;
        private bool awaitingFinal;
        private bool showingMission;
        private readonly MedalProgress medals = new MedalProgress();
        private const string RecordKey = "BatallaDigestiva.HighScore";

        private void Start()
        {
            startButton.onClick.AddListener(StartRound);
            restartButton.onClick.AddListener(StartRound);
            homeButton.onClick.AddListener(ShowMenu);
            resultsHomeButton.onClick.AddListener(ShowMenu);
            pauseButton.onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            if (missionContinueButton != null) missionContinueButton.onClick.AddListener(ContinueToScore);
            if (questions != null)
            {
                questions.AnswerAccepted += OnAnswer;
                questions.Continued += ContinueAfterQuestion;
            }
            ShowMenu();
        }

        public void StartRound()
        {
            if (feedback != null) feedback.Clear();
            spawner.Clear();
            session.Start(config.roundDuration);
            manualPause = false;
            awaitingFinal = false;
            showingMission = false;
            if (missionPanel != null) missionPanel.SetActive(false);
            medals.Reset(config.targetsPerMedal);
            if (medalsHUD != null) medalsHUD.Refresh(medals);
            if (powerUps != null) powerUps.ResetRound(config, spawner);
            if (questions != null) questions.ResetRound();
            menuPanel.SetActive(false); resultsPanel.SetActive(false);
            pausePanel.SetActive(false); gamePanel.SetActive(true);
            Canvas.ForceUpdateCanvases();
            spawner.Seed(config, RegisterHit);
            if (powerUps != null) powerUps.SetRunning(true);
            RefreshHUD();
        }

        private void Update()
        {
            if (!session.Playing || session.Paused) return;
            session.Tick(Time.deltaTime);
            if (!session.Playing) { FinishRound(); return; }
            if (feedback != null) feedback.Tick(Time.deltaTime);
            if (powerUps != null) powerUps.Tick(Time.deltaTime);
            spawner.Tick(Time.deltaTime, config, RegisterHit, 1 - session.Remaining / Mathf.Max(1, config.roundDuration),
                powerUps != null ? (System.Func<bool>)powerUps.TrySpawnFromSlot : null);
            RefreshHUD();
        }

        private bool RegisterHit(TargetView target)
        {
            if (!session.RegisterHit(config.pointsPerTarget)) return false;
            bool newMedal = medals.Register(target.Character.productId);
            if (feedback != null)
            {
                feedback.ShowPoints(target.Rect.anchoredPosition, config.pointsPerTarget);
                if (newMedal) feedback.Medal(target.Character.displayName, medals.Medals(target.Character.productId));
            }
            if (medalsHUD != null) medalsHUD.Refresh(medals);
            RefreshHUD();
            return true;
        }

        private void OnAnswer(bool correct)
        {
            var audio = GameAudio.Instance;
            if (audio != null) audio.Play(correct ? audio.correct : audio.wrong, .3f);
            if (correct)
            {
                int bonus = Mathf.Max(0, config.correctAnswerBonus);
                session.AwardFinalBonus(bonus);
                questions.ShowBonus(bonus);
                RefreshHUD();
            }
        }

        private void ContinueAfterQuestion() { if (awaitingFinal) ShowResults(); }
        private void UpdatePause()
        {
            session.SetPaused(manualPause);
            if (powerUps != null) powerUps.SetRunning(session.Playing && !manualPause);
        }

        private void RefreshHUD()
        {
            if (questions != null) questions.SetScore(session.Score);
            scoreText.text = session.Score.ToString();
            int seconds = Mathf.CeilToInt(session.Remaining);
            timerText.text = (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
            timerText.color = seconds <= 10 ? new Color(1, 0.65f, 0.25f) : Color.white;
        }

        private void FinishRound()
        {
            if (feedback != null) feedback.Clear();
            spawner.Clear();
            if (powerUps != null) powerUps.Clear();
            awaitingFinal = true;
            RefreshHUD();
            if (questions != null && questions.OpenFinalQuestion()) return;
            ShowResults();
        }

        private void ShowResults()
        {
            awaitingFinal = false;
            gamePanel.SetActive(false); pausePanel.SetActive(false); resultsPanel.SetActive(true);
            bool completed = medals.HasAll(spawner.characters.Where(data => data != null).Select(data => data.productId));
            int record = Mathf.Max(session.Score, PlayerPrefs.GetInt(RecordKey, 0));
            PlayerPrefs.SetInt(RecordKey, record); PlayerPrefs.Save();
            string details = "RÉCORD " + record.ToString("N0");
            if (questions != null) details += " · " + questions.Correct + "/" + questions.Asked + " respuestas correctas";
            if (resultDetailsText != null)
            {
                resultText.text = session.Score.ToString("N0");
                resultDetailsText.text = details;
            }
            else resultText.text = "TU PUNTAJE\n" + session.Score + " PUNTOS\n" + details;
            if (resultsProducts != null) resultsProducts.Refresh(medals);
            if (missionPanel != null && missionMedals != null)
            {
                missionTitle.text = completed ? "MISIÓN COMPLETADA" : "RONDA TERMINADA";
                if (missionTitleArtwork != null)
                {
                    missionTitleArtwork.SetActive(true);
                    missionTitle.gameObject.SetActive(false);
                }
                missionMedals.Refresh(medals);
                resultsPanel.SetActive(false);
                missionPanel.SetActive(true);
                showingMission = true;
            }
            else PlayVictory();
        }

        private void ContinueToScore()
        {
            if (!showingMission) return;
            showingMission = false;
            missionPanel.SetActive(false);
            resultsPanel.SetActive(true);
            PlayVictory();
        }

        private void PlayVictory()
        {
            var audio = GameAudio.Instance;
            if (audio != null) audio.Play(audio.victory);
        }

        public void ShowMenu()
        {
            if (feedback != null) feedback.Clear();
            showingMission = false;
            if (missionPanel != null) missionPanel.SetActive(false);
            session.Stop(); spawner.Clear();
            manualPause = false;
            awaitingFinal = false;
            if (powerUps != null) powerUps.Clear();
            if (questions != null) questions.ResetRound();
            menuPanel.SetActive(true); gamePanel.SetActive(false);
            resultsPanel.SetActive(false); pausePanel.SetActive(false);
        }

        private void Pause()
        {
            if (!session.Playing && (questions == null || !questions.IsOpen)) return;
            manualPause = true; UpdatePause(); pausePanel.SetActive(true);
        }

        private void Resume() { manualPause = false; UpdatePause(); pausePanel.SetActive(false); }
        private void OnDestroy()
        {
            if (questions == null) return;
            questions.AnswerAccepted -= OnAnswer;
            questions.Continued -= ContinueAfterQuestion;
        }
        private void OnApplicationPause(bool paused) { if (paused) Pause(); }
        private void OnApplicationFocus(bool focused) { if (!focused) Pause(); }
    }
}
