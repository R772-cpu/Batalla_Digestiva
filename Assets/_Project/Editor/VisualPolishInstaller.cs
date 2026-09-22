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
        [MenuItem("Batalla Digestiva/Aplicar pulido visual")]
        public static void ApplyVisualPolish()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { Debug.LogWarning("Sal de Play antes de aplicar el pulido."); return; }
            var scene = SceneManager.GetActiveScene();
            var controller = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<GameController>(true)).FirstOrDefault();
            if (controller == null) { Debug.LogError("Abre BatallaPrototype primero."); return; }
            AddQuestionsToScene();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Undo.RecordObject(controller, "Conectar feedback visual");
            if (controller.feedback == null)
            {
                controller.feedback = Undo.AddComponent<GameplayFeedback>(controller.gameObject);
                var layer = Rect("FeedbackLayer", controller.spawner.playArea);
                var template = Label("PlantillaPuntos", layer, "+10", 34, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                template.rectTransform.sizeDelta = new Vector2(140, 60);
                template.fontStyle = FontStyle.Bold;
                var shadow = template.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0.1f, 0, 0, 0.9f); shadow.effectDistance = new Vector2(2, -2);
                controller.feedback.pointsTemplate = template;
                template.gameObject.SetActive(false);
                controller.feedback.medalNotice = Label("AvisoMedalla", controller.gamePanel.transform, "", 27,
                    new Vector2(0.08f, 0.015f), new Vector2(0.92f, 0.095f));
                controller.feedback.medalNotice.color = new Color(1, 0.88f, 0.2f);
                Undo.RegisterCreatedObjectUndo(layer.gameObject, "Crear puntos flotantes");
                Undo.RegisterCreatedObjectUndo(controller.feedback.medalNotice.gameObject, "Crear aviso de medalla");
                EditorUtility.SetDirty(controller.feedback);
            }
            var questions = controller.questions;
            if (questions != null && questions.answerMark == null)
            {
                Undo.RecordObject(questions, "Conectar feedback de respuesta");
                var card = Image("TarjetaPregunta", questions.panel.transform, new Color(1, 0.94f, 0.81f),
                    new Vector2(0.075f, 0.605f), new Vector2(0.925f, 0.84f));
                card.transform.SetAsFirstSibling(); card.raycastTarget = false;
                var panelSprite = LoadSprite(Root + "/Art/UI/panel_pregunta.png");
                if (panelSprite != null) { card.sprite = panelSprite; card.color = Color.white; }
                Undo.RecordObject(questions.questionText, "Ajustar contraste de pregunta");
                questions.questionText.color = new Color(0.2f, 0.12f, 0.08f);
                Move(questions.questionText.rectTransform, new Vector2(0.12f, 0.62f), new Vector2(0.88f, 0.825f));
                var markRect = Rect("MarcaRespuesta", questions.panel.transform, new Vector2(0.1f, 0.175f), new Vector2(0.18f, 0.275f));
                questions.answerMark = markRect.gameObject.AddComponent<AnswerMark>();
                questions.answerMark.raycastTarget = false;
                markRect.gameObject.SetActive(false);
                Move(questions.feedbackText.rectTransform, new Vector2(0.2f, 0.165f), new Vector2(0.92f, 0.285f));
                Undo.RegisterCreatedObjectUndo(card.gameObject, "Crear tarjeta de pregunta");
                Undo.RegisterCreatedObjectUndo(markRect.gameObject, "Crear marca de respuesta");
                EditorUtility.SetDirty(questions);
            }
            foreach (var root in scene.GetRootGameObjects())
                foreach (var button in root.GetComponentsInChildren<Button>(true))
                {
                    if (controller.powerUps != null && button == controller.powerUps.button) continue;
                    if (button.GetComponent<ButtonPressFeedback>() == null) Undo.AddComponent<ButtonPressFeedback>(button.gameObject);
                }
            EditorUtility.SetDirty(controller);
            ConnectVisualAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Pulido listo: puntos flotantes, avisos, preguntas, botones y tipografia disponible. Guarda con Ctrl+S y prueba en Play.");
        }
    }
}
