using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BatallaDigestiva.Editor
{
    public static partial class PrototypeBuilder
    {
        [MenuItem("Batalla Digestiva/Actualizar área de juego y avisos")]
        public static void ApplyClientReview()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogWarning("Sal de Play primero."); return; }
            var scene = SceneManager.GetActiveScene();
            var game = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<GameController>(true)).FirstOrDefault();
            if (game == null) { Debug.LogError("Abre BatallaPrototype primero."); return; }
            ConnectVisualAssets();
            // Only this explicit command changes PlayArea. Other menus preserve user edits.
            Move(game.spawner.playArea, new Vector2(.025f, .10f), new Vector2(.975f, .80f));
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Area ampliada, botones ajustados y aviso con medalla conectado. Guarda con Ctrl+S.");
        }

        private static void InstallClientReviewLayout(GameController game)
        {
            if (game.questions != null)
            {
                var q = game.questions;
                Move((RectTransform)q.continueButton.transform, new Vector2(.36f, .015f), new Vector2(.64f, .145f));
                for (int i = 0; i < q.answerButtons.Length; i++)
                {
                    float left = .035f + i * .32f;
                    Move((RectTransform)q.answerButtons[i].transform, new Vector2(left, .16f), new Vector2(left + .30f, .29f));
                }
                Move(q.feedbackText.rectTransform, new Vector2(.15f, .295f), new Vector2(.85f, .35f));
                Undo.RecordObject(q.feedbackText, "Ajustar respuesta final");
                q.feedbackText.resizeTextMinSize = 12; q.feedbackText.resizeTextMaxSize = 24;
            }
            if (game.missionContinueButton != null)
                Move((RectTransform)game.missionContinueButton.transform, new Vector2(.35f, .04f), new Vector2(.65f, .16f));
            if (game.feedback != null)
            {
                var notice = game.feedback.medalNotice;
                Move(notice.rectTransform, new Vector2(.45f, .012f), new Vector2(.68f, .085f));
                var icon = EnsureImage("ImagenAvisoMedalla", game.gamePanel.transform,
                    new Vector2(.36f, .008f), new Vector2(.45f, .098f), Color.white);
                icon.preserveAspect = true; icon.raycastTarget = false;
                Undo.RecordObject(game.feedback, "Conectar imagen del aviso");
                game.feedback.medalNoticeImage = icon;
                icon.gameObject.SetActive(false);
            }
            var excluded = new List<RectTransform>();
            var hud = game.gamePanel.transform.Find("HUD");
            if (hud != null)
                foreach (var name in new[] { "MarcoPuntos", "MarcoTiempo" })
                {
                    var rect = hud.Find(name) as RectTransform;
                    if (rect != null) excluded.Add(rect);
                }
            if (game.pauseButton != null) excluded.Add((RectTransform)game.pauseButton.transform);
            if (game.homeButton != null) excluded.Add((RectTransform)game.homeButton.transform);
            var sound = game.gamePanel.transform.Find("ControlSonido") as RectTransform;
            if (sound != null) excluded.Add(sound);
            Undo.RecordObject(game.spawner, "Reservar espacio del HUD");
            game.spawner.excludedUI = excluded.ToArray();
        }
    }
}
