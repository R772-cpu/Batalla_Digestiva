using System.IO;
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
        [MenuItem("Batalla Digestiva/Actualizar interfaz e instrucciones")]
        public static void UpgradePresentation() { AddQuestionsToScene(); }

        private static void InstallPresentation(GameController controller)
        {
            if (controller.GetComponent<HowToPlayPanel>() != null) return;
            var navigation = Undo.AddComponent<HowToPlayPanel>(controller.gameObject);
            navigation.game = controller;
            Move((RectTransform)controller.startButton.transform, new Vector2(0.3f, 0.17f), new Vector2(0.7f, 0.29f));
            navigation.openButton = Button("CÓMO JUGAR", controller.menuPanel.transform, new Vector2(0.3f, 0.025f), new Vector2(0.7f, 0.145f));
            Undo.RegisterCreatedObjectUndo(navigation.openButton.gameObject, "Crear boton de instrucciones");
            var panel = Image("HowToPlayPanel", controller.menuPanel.transform.parent, new Color(0.12f, 0.01f, 0.02f, 0.98f));
            navigation.panel = panel.gameObject;
            Label("Titulo", panel.transform, "CÓMO JUGAR", 48, new Vector2(0.08f, 0.83f), new Vector2(0.92f, 0.97f));
            string[] steps =
            {
                "1. Toca los personajes\nCada eliminación suma 10 puntos. Tienes 60 segundos.",
                "2. Gana medallas\nCada 5 del mismo producto ganas otra medalla.",
                "3. Aprovecha los productos\nTócalos antes de que desaparezcan: eliminan su grupo con burbujas.",
                "4. Responde al final\nUna pregunta: +20 si aciertas, sin descuento si fallas."
            };
            for (int i = 0; i < steps.Length; i++)
            {
                float top = 0.81f - i * 0.15f;
                Label("Paso" + (i + 1), panel.transform, steps[i], 28,
                    new Vector2(0.08f, top - 0.13f), new Vector2(0.92f, top));
            }
            navigation.backButton = Button("VOLVER", panel.transform, new Vector2(0.08f, 0.04f), new Vector2(0.42f, 0.17f));
            navigation.playButton = Button("JUGAR", panel.transform, new Vector2(0.58f, 0.04f), new Vector2(0.92f, 0.17f));
            panel.gameObject.SetActive(false);
            Undo.RegisterCreatedObjectUndo(panel.gameObject, "Crear instrucciones");
            EditorUtility.SetDirty(navigation);
        }

        [MenuItem("Batalla Digestiva/Conectar recursos visuales")]
        public static void ConnectVisualAssets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Sal de Play antes de conectar los recursos.");
                return;
            }
            var scene = SceneManager.GetActiveScene();
            var controller = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<GameController>(true)).FirstOrDefault();
            if (controller == null) { Debug.LogError("Abre BatallaPrototype para conectar sus recursos."); return; }
            foreach (var folder in new[] { "Products", "Medals", "Effects", "UI", "Branding", "Fonts" })
                Directory.CreateDirectory(Root + "/Art/" + folder);
            AssetDatabase.Refresh();
            int count = 0;
            foreach (var data in controller.spawner.characters.Where(data => data != null).Distinct())
            {
                var product = LoadSprite(Root + "/Art/Products/" + data.productId + "_producto.png");
                var medal = LoadSprite(Root + "/Art/Medals/" + data.productId + "_medalla.png");
                if (product == null && medal == null) continue;
                Undo.RecordObject(data, "Conectar producto y medalla");
                if (product != null) { data.productSprite = product; count++; }
                if (medal != null) { data.medalSprite = medal; count++; }
                EditorUtility.SetDirty(data);
            }
            var bubble = LoadSprite(Root + "/Art/Effects/burbuja.png");
            if (bubble != null)
            {
                Undo.RecordObject(controller.config, "Conectar burbuja");
                controller.config.bubbleSprite = bubble;
                EditorUtility.SetDirty(controller.config); count++;
            }
            var buttonSprite = LoadSprite(Root + "/Art/UI/boton_azul.png");
            if (buttonSprite != null)
            {
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var button in root.GetComponentsInChildren<Button>(true))
                    {
                        if (button.image == null || (controller.powerUps != null && button == controller.powerUps.button)) continue;
                        Undo.RecordObject(button.image, "Aplicar imagen de boton");
                        button.image.sprite = buttonSprite;
                        button.image.color = Color.white;
                        button.image.type = buttonSprite.border.sqrMagnitude > 0 ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
                    }
                if (controller.questions != null)
                {
                    Undo.RecordObject(controller.questions, "Ajustar color de respuestas");
                    controller.questions.normalAnswerColor = Color.white;
                }
                count++;
            }
            var logo = LoadSprite(Root + "/Art/Branding/logo_batalla_digestiva.png");
            if (logo != null)
            {
                var title = controller.menuPanel.transform.Find("Titulo");
                var existing = controller.menuPanel.transform.Find("LogoBatalla");
                var logoImage = existing != null ? existing.GetComponent<UnityEngine.UI.Image>() :
                    Image("LogoBatalla", controller.menuPanel.transform, Color.white, new Vector2(0.15f, 0.58f), new Vector2(0.85f, 0.95f));
                if (existing == null) Undo.RegisterCreatedObjectUndo(logoImage.gameObject, "Crear logo");
                Undo.RecordObject(logoImage, "Asignar logo");
                logoImage.sprite = logo; logoImage.preserveAspect = true; logoImage.raycastTarget = false;
                if (title != null) { Undo.RecordObject(title.gameObject, "Ocultar titulo provisional"); title.gameObject.SetActive(false); }
                count++;
            }
            foreach (var view in new[] { controller.medalsHUD, controller.resultsProducts, controller.missionMedals })
            {
                if (view == null) continue;
                for (int i = 0; i < view.characters.Length; i++)
                {
                    var data = view.characters[i];
                    Undo.RecordObject(view.icons[i], "Actualizar vista previa");
                    view.icons[i].sprite = view.resultsMode && view.useProductPortraits && data.productSprite != null ? data.productSprite :
                        data.medalSprite != null ? data.medalSprite : data.normalSprite;
                }
            }
            var questionCard = controller.questions != null ? controller.questions.panel.transform.Find("TarjetaPregunta") : null;
            var questionCardSprite = LoadSprite(Root + "/Art/UI/panel_pregunta.png");
            if (questionCard != null && questionCardSprite != null)
            {
                var cardImage = questionCard.GetComponent<UnityEngine.UI.Image>();
                Undo.RecordObject(cardImage, "Aplicar arte de pregunta");
                cardImage.sprite = questionCardSprite; cardImage.color = Color.white; count++;
            }
            var fontPath = AssetDatabase.FindAssets("t:Font", new[] { Root + "/Art/Fonts" })
                .Select(AssetDatabase.GUIDToAssetPath).OrderBy(path => path, System.StringComparer.Ordinal).FirstOrDefault();
            if (AssetDatabase.LoadAssetAtPath<Font>(Root + "/Art/Fonts/Montserrat-Regular.ttf") != null)
                fontPath = Root + "/Art/Fonts/Montserrat-Regular.ttf";
            if (fontPath != null)
            {
                var brandFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
                if (brandFont != null)
                {
                    foreach (var root in scene.GetRootGameObjects())
                        foreach (var text in root.GetComponentsInChildren<Text>(true))
                        {
                            Undo.RecordObject(text, "Aplicar tipografia");
                            if (text.font != null && text.font.name.Contains("Bold")) text.fontStyle = FontStyle.Bold;
                            text.font = brandFont;
                        }
                    count++;
                }
            }
            ConnectReferenceArt(controller);
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Recursos encontrados y conectados: " + count + ". Los que faltan conservan su aspecto actual. Guarda la escena con Ctrl+S.");
        }
    }
}
