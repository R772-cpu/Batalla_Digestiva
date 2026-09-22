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
        [MenuItem("Batalla Digestiva/Alinear diseño de referencia")]
        public static void AlignReferenceDesign()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogWarning("Sal de Play primero."); return; }
            var scene = SceneManager.GetActiveScene();
            var game = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<GameController>(true)).FirstOrDefault();
            if (game == null) { Debug.LogError("Abre BatallaPrototype primero."); return; }
            ApplyVisualPolish();
            font = game.scoreText.font;
            AlignQuestion(game.questions);
            AlignMenu(game);
            AlignInstructions(game);
            AlignCards(game.missionMedals);
            AlignCards(game.resultsProducts);
            // Reservar los mismos limites del tablero aprobado. Solo redistribuir el HUD exterior.
            var hud = game.gamePanel.transform.Find("HUD");
            if (hud != null)
            {
                Move((RectTransform)hud, Vector2.zero, Vector2.one);
                Undo.RecordObject(hud.GetComponent<UnityEngine.UI.Image>(), "Quitar franja del HUD");
                hud.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
                hud.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
                Move((RectTransform)game.pauseButton.transform, new Vector2(0.025f, 0.875f), new Vector2(0.12f, 0.98f));
                Move((RectTransform)game.homeButton.transform, new Vector2(0.13f, 0.875f), new Vector2(0.225f, 0.98f));
                Move(game.scoreText.rectTransform, new Vector2(0.81f, 0.75f), new Vector2(0.98f, 0.82f));
                Move(game.timerText.rectTransform, new Vector2(0.81f, 0.67f), new Vector2(0.98f, 0.74f));
                var scoreBox = EnsureImage("MarcoPuntos", hud, new Vector2(0.80f, 0.75f), new Vector2(0.98f, 0.82f), new Color(0.23f, 0.02f, 0.035f));
                var timerBox = EnsureImage("MarcoTiempo", hud, new Vector2(0.80f, 0.67f), new Vector2(0.98f, 0.74f), new Color(0.23f, 0.02f, 0.035f));
                scoreBox.transform.SetAsFirstSibling(); timerBox.transform.SetAsFirstSibling();
            }
            if (game.medalsHUD != null)
                Move((RectTransform)game.medalsHUD.transform, new Vector2(0.27f, 0.835f), new Vector2(0.985f, 0.99f));
            ConnectVisualAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Diseño alineado: respuestas en fila, cuadro con cinta, HUD y fichas finales. Guarda con Ctrl+S. Los recursos no entregados usan formas provisionales.");
        }

        private static void AlignQuestion(QuestionController q)
        {
            Undo.RecordObject(q, "Alinear pregunta"); q.referenceLayout = true;
            var parent = q.panel.transform;
            Undo.RecordObject(q.panel.GetComponent<UnityEngine.UI.Image>(), "Fondo de pregunta");
            q.panel.GetComponent<UnityEngine.UI.Image>().color = new Color(0.12f, 0.01f, 0.02f, 0.28f);
            var card = EnsureImage("TarjetaPregunta", parent, new Vector2(0.22f, 0.32f), new Vector2(0.78f, 0.72f), new Color(1, 0.94f, 0.81f));
            card.transform.SetAsFirstSibling();
            var ribbon = EnsureImage("CintaPregunta", parent, new Vector2(0.18f, 0.59f), new Vector2(0.82f, 0.71f), new Color(1, 0.68f, 0));
            q.ribbonText = EnsureLabel("TituloCinta", ribbon.transform, "Responde la\npregunta", 38, new Vector2(0.06f, 0.05f), new Vector2(0.94f, 0.95f));
            q.questionScoreText = EnsureLabel("PuntajePregunta", parent, "0", 76, new Vector2(0.3f, 0.78f), new Vector2(0.66f, 0.96f));
            var star = EnsureImage("EstrellaPuntaje", parent, new Vector2(0.66f, 0.8f), new Vector2(0.76f, 0.94f), Color.clear);
            star.preserveAspect = true;
            Move(q.questionText.rectTransform, new Vector2(0.27f, 0.38f), new Vector2(0.73f, 0.57f));
            Undo.RecordObject(q.questionText, "Texto de pregunta");
            q.questionText.fontStyle = FontStyle.Bold; q.questionText.resizeTextMaxSize = 32;
            q.answerLetters = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                var button = q.answerButtons[i];
                float left = 0.035f + i * 0.32f;
                Move((RectTransform)button.transform, new Vector2(left, 0.095f), new Vector2(left + 0.30f, 0.225f));
                q.answerLetters[i] = EnsureLabel("Letra", button.transform, ((char)('A' + i)).ToString(), 42,
                    new Vector2(0.025f, 0.2f), new Vector2(0.19f, 0.85f));
                Move(q.answerLabels[i].rectTransform, new Vector2(0.22f, 0.13f), new Vector2(0.95f, 0.87f));
                Undo.RecordObject(q.answerLabels[i], "Texto de alternativa");
                q.answerLabels[i].resizeTextMinSize = 14; q.answerLabels[i].resizeTextMaxSize = 22;
                var colors = button.colors; colors.disabledColor = Color.white;
                Undo.RecordObject(button, "Conservar colores de respuesta"); button.colors = colors;
            }
            if (q.answerMark != null) Move(q.answerMark.rectTransform, new Vector2(0.42f, 0.38f), new Vector2(0.58f, 0.57f));
            var artwork = EnsureImage("ArteRespuesta", parent, new Vector2(0.42f, 0.38f), new Vector2(0.58f, 0.57f), Color.white);
            artwork.preserveAspect = true; artwork.gameObject.SetActive(false); q.answerArtwork = artwork;
            Move(q.feedbackText.rectTransform, new Vector2(0.15f, 0.24f), new Vector2(0.85f, 0.31f));
            Move((RectTransform)q.continueButton.transform, new Vector2(0.35f, 0.015f), new Vector2(0.65f, 0.08f));
            q.progressText.text = "";
            EditorUtility.SetDirty(q);
        }

        private static void AlignMenu(GameController game)
        {
            var menu = game.menuPanel.transform;
            var logo = menu.Find("LogoBatalla");
            if (logo != null) Move((RectTransform)logo, new Vector2(0.23f, 0.43f), new Vector2(0.77f, 0.79f));
            var instructions = menu.Find("Instrucciones");
            if (instructions != null) { Undo.RecordObject(instructions.gameObject, "Ocultar instrucciones duplicadas"); instructions.gameObject.SetActive(false); }
            Move((RectTransform)game.startButton.transform, new Vector2(0.36f, 0.30f), new Vector2(0.64f, 0.405f));
            var how = game.GetComponent<HowToPlayPanel>();
            if (how != null) Move((RectTransform)how.openButton.transform, new Vector2(0.36f, 0.16f), new Vector2(0.64f, 0.265f));
        }

        private static readonly string[] ProductOrder = { "modulex", "moperid", "rogastrilplus", "drenabyl", "metrozin", "pamox", "quanox", "mixel" };
        private static void AlignInstructions(GameController game)
        {
            var how = game.GetComponent<HowToPlayPanel>(); if (how == null) return;
            string[] captions = { "Toca los parásitos e intrusos apenas aparezcan.", "Elimínalos para acumular puntos.",
                "Tienes 60 segundos para conseguir las medallas.", "Elimina 5 del mismo producto y gana su medalla. ¡Puedes repetirla!" };
            for (int i = 0; i < 4; i++)
            {
                float left = 0.035f + i * 0.242f;
                var card = EnsureImage("FichaPaso" + (i + 1), how.panel.transform, new Vector2(left, 0.4f), new Vector2(left + 0.225f, 0.78f), new Color(0.24f, 0.015f, 0.03f));
                card.transform.SetAsFirstSibling();
                var art = EnsureImage("Ilustracion", card.transform, new Vector2(0.05f, 0.38f), new Vector2(0.95f, 0.98f), Color.clear);
                art.preserveAspect = true;
                EnsureLabel("Numero", card.transform, (i + 1).ToString(), 35, new Vector2(0, 0.8f), new Vector2(0.2f, 1));
                var step = how.panel.transform.Find("Paso" + (i + 1));
                if (step != null)
                {
                    Move((RectTransform)step, new Vector2(left + 0.01f, 0.415f), new Vector2(left + 0.215f, 0.535f));
                    var text = step.GetComponent<Text>(); Undo.RecordObject(text, "Actualizar paso"); text.text = captions[i];
                }
            }
            var footer = EnsureImage("ReglasFinales", how.panel.transform, new Vector2(0.08f, 0.2f), new Vector2(0.92f, 0.36f), new Color(0.24f, 0.015f, 0.03f));
            EnsureLabel("Texto", footer.transform, "Al final, responde una pregunta para ganar +20 puntos extra.\nToca los productos brillantes para eliminar a todos los bichos de ese producto.", 26, new Vector2(0.03f, 0.03f), new Vector2(0.97f, 0.97f));
        }
        private static void AlignCards(MedalHUD view)
        {
            if (view == null) return;
            Undo.RecordObject(view, "Etiquetas de productos");
            view.productTitles = new Text[view.characters.Length];
            for (int i = 0; i < view.characters.Length; i++)
            {
                var data = view.characters[i];
                int slot = System.Array.IndexOf(ProductOrder, data.productId); if (slot < 0) slot = i;
                var card = view.icons[i].transform.parent;
                Move((RectTransform)card, new Vector2((slot % 4) / 4f + 0.008f, 1 - (slot / 4 + 1) / 2f + 0.012f),
                    new Vector2((slot % 4 + 1) / 4f - 0.008f, 1 - (slot / 4) / 2f - 0.012f));
                Undo.RecordObject(card.GetComponent<UnityEngine.UI.Image>(), "Quitar fondo de retrato");
                card.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
                var box = EnsureImage("PanelDescripcion", card, new Vector2(0, 0.01f), new Vector2(1, 0.37f), new Color(0.25f, 0.025f, 0.04f));
                box.transform.SetAsFirstSibling();
                var title = EnsureImage("EtiquetaProducto", card, new Vector2(0.12f, 0.34f), new Vector2(0.88f, 0.47f), new Color(1, 0.68f, 0));
                view.productTitles[i] = EnsureLabel("Nombre", title.transform, data.displayName, 21, Vector2.zero, Vector2.one);
                Move(view.icons[i].rectTransform, new Vector2(0.12f, 0.44f), new Vector2(0.88f, 0.99f));
                Move(view.labels[i].rectTransform, new Vector2(0.035f, 0.02f), new Vector2(0.965f, 0.32f));
            }
            EditorUtility.SetDirty(view);
        }

        private static UnityEngine.UI.Image EnsureImage(string name, Transform parent, Vector2 min, Vector2 max, Color fallback)
        {
            var child = parent.Find(name);
            if (child != null) { Move((RectTransform)child, min, max); return child.GetComponent<UnityEngine.UI.Image>(); }
            var image = Image(name, parent, fallback, min, max); image.raycastTarget = false;
            Undo.RegisterCreatedObjectUndo(image.gameObject, "Crear " + name); return image;
        }
        private static Text EnsureLabel(string name, Transform parent, string value, int size, Vector2 min, Vector2 max)
        {
            var child = parent.Find(name);
            if (child != null) { Move((RectTransform)child, min, max); return child.GetComponent<Text>(); }
            var label = Label(name, parent, value, size, min, max);
            Undo.RegisterCreatedObjectUndo(label.gameObject, "Crear " + name); return label;
        }

        private static void ConnectReferenceArt(GameController game)
        {
            var q = game.questions;
            if (q != null)
            {
                Undo.RecordObject(q, "Conectar estados de respuestas");
                var blue = LoadSprite(Root + "/Art/UI/respuesta_azul.png") ?? LoadSprite(Root + "/Art/UI/boton_azul.png");
                var green = LoadSprite(Root + "/Art/UI/respuesta_verde.png");
                var red = LoadSprite(Root + "/Art/UI/respuesta_roja.png");
                if (blue != null) { q.blueAnswerSprite = blue; foreach (var b in q.answerButtons) { Undo.RecordObject(b.image, "Respuesta azul"); b.image.sprite = blue; b.image.color = Color.white; } }
                if (green != null) q.greenAnswerSprite = green;
                if (red != null) q.redAnswerSprite = red;
                var check = LoadSprite(Root + "/Art/UI/check.png"); var cross = LoadSprite(Root + "/Art/UI/equis.png");
                if (check != null) q.correctArtwork = check; if (cross != null) q.incorrectArtwork = cross;
                Bind(q.panel.transform, "CintaPregunta", "UI/cinta_pregunta.png");
                Bind(q.panel.transform, "EstrellaPuntaje", "UI/estrella.png");
                EditorUtility.SetDirty(q);
            }
            foreach (var view in new[] { game.missionMedals, game.resultsProducts })
                if (view != null) foreach (var icon in view.icons)
                { Bind(icon.transform.parent, "PanelDescripcion", "UI/panel_descripcion.png"); Bind(icon.transform.parent, "EtiquetaProducto", "UI/etiqueta_producto.png"); }
            var hud = game.gamePanel.transform.Find("HUD");
            if (hud != null) { Bind(hud, "MarcoPuntos", "UI/marco_contador.png"); Bind(hud, "MarcoTiempo", "UI/marco_contador.png"); }
            var how = game.GetComponent<HowToPlayPanel>();
            if (how != null)
                for (int i = 1; i <= 4; i++)
                {
                    var card = how.panel.transform.Find("FichaPaso" + i);
                    if (card != null) Bind(card, "Ilustracion", "UI/instruccion_" + i + ".png");
                }
            Bind(game.menuPanel.transform.parent, game.menuPanel.name, "Backgrounds/menu_fondo.png");
            if (q != null) Bind(q.panel.transform.parent, q.panel.name, "Backgrounds/preguntas_fondo.png");
            InstallArtDetails(game);
        }

        private static void Bind(Transform parent, string name, string path)
        {
            var child = parent.Find(name); if (child == null) return;
            var sprite = LoadSprite(Root + "/Art/" + path); if (sprite == null) return;
            var image = child.GetComponent<UnityEngine.UI.Image>(); if (image == null) return;
            Undo.RecordObject(image, "Conectar " + name); image.sprite = sprite; image.color = Color.white;
        }
    }
}
