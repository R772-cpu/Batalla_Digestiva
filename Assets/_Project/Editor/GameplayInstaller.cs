using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva.Editor
{
    public static partial class PrototypeBuilder
    {
        [MenuItem("Batalla Digestiva/Actualizar medallas y potenciadores")]
        public static void UpgradeGameplay() { AddQuestionsToScene(); }

        private static void InstallGameplay(GameController controller)
        {
            var characters = controller.spawner.characters.Where(data => data != null)
                .GroupBy(data => data.productId).Select(group => group.First()).ToArray();
            if (controller.medalsHUD == null)
            {
                var row = Rect("Medallas", controller.gamePanel.transform, new Vector2(0.015f, 0.68f), new Vector2(0.985f, 0.82f));
                controller.medalsHUD = CreateCards(row, characters, false);
                Undo.RegisterCreatedObjectUndo(row.gameObject, "Crear medallas");
                // PlayArea belongs to the scene author. Upgrades must preserve its
                // anchors, offsets, pivot and scale, even when recreating the medal HUD.
            }
            if (controller.powerUps == null)
            {
                controller.powerUps = Undo.AddComponent<PowerUpController>(controller.gameObject);
                var button = Button("PRODUCTO", controller.gamePanel.transform, new Vector2(0.3f, 0.015f), new Vector2(0.7f, 0.125f));
                button.name = "Potenciador";
                controller.powerUps.button = button;
                controller.powerUps.label = button.GetComponentInChildren<Text>();
                var textRect = controller.powerUps.label.rectTransform;
                textRect.anchorMin = new Vector2(0.2f, 0.05f); textRect.anchorMax = new Vector2(0.98f, 0.95f);
                controller.powerUps.label.resizeTextMaxSize = 22;
                var icon = Image("Producto", button.transform, Color.white, new Vector2(0.01f, 0.02f), new Vector2(0.19f, 0.98f));
                icon.preserveAspect = true; icon.raycastTarget = false;
                controller.powerUps.productImage = icon;
                button.gameObject.SetActive(false);
                Undo.RegisterCreatedObjectUndo(button.gameObject, "Crear potenciador");
                EditorUtility.SetDirty(controller.powerUps);
            }
            if (controller.resultsProducts == null)
            {
                var grid = Rect("ProductosYMedallas", controller.resultsPanel.transform, new Vector2(0.04f, 0.22f), new Vector2(0.96f, 0.73f));
                controller.resultsProducts = CreateCards(grid, characters, true);
                Undo.RegisterCreatedObjectUndo(grid.gameObject, "Crear resumen de productos");
                Move(controller.resultText.rectTransform, new Vector2(0.04f, 0.75f), new Vector2(0.96f, 0.98f));
                Move((RectTransform)controller.restartButton.transform, new Vector2(0.1f, 0.035f), new Vector2(0.58f, 0.16f));
                Move((RectTransform)controller.resultsHomeButton.transform, new Vector2(0.65f, 0.035f), new Vector2(0.9f, 0.16f));
            }
            var instructions = controller.menuPanel.transform.Find("Instrucciones");
            if (instructions != null && instructions.TryGetComponent<Text>(out var text))
            {
                Undo.RecordObject(text, "Actualizar instrucciones");
                text.text = "Elimina personajes y gana medallas cada 5.\nToca los productos para eliminar su grupo.\n60 segundos + una pregunta final.";
            }
            EditorUtility.SetDirty(controller);
            InstallPresentation(controller);
            InstallMissionSummary(controller, characters);
        }

        [MenuItem("Batalla Digestiva/Actualizar pantallas finales")]
        public static void UpgradeFinalScreens() { AddQuestionsToScene(); }

        private static void InstallMissionSummary(GameController controller, CharacterData[] characters)
        {
            if (controller.missionPanel != null) return;
            var panel = Image("MissionPanel", controller.resultsPanel.transform.parent, new Color(0.12f, 0.01f, 0.02f, 0.94f));
            controller.missionPanel = panel.gameObject;
            controller.missionTitle = Label("Titulo", panel.transform, "MEDALLAS OBTENIDAS", 44,
                new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.98f));
            var grid = Rect("MedallasConPersonajes", panel.transform, new Vector2(0.04f, 0.21f), new Vector2(0.96f, 0.8f));
            controller.missionMedals = CreateCards(grid, characters, true);
            controller.missionMedals.useProductPortraits = false;
            for (int i = 0; i < characters.Length; i++)
                controller.missionMedals.icons[i].sprite = characters[i].medalSprite != null ? characters[i].medalSprite : characters[i].normalSprite;
            controller.missionContinueButton = Button("CONTINUAR", panel.transform, new Vector2(0.3f, 0.04f), new Vector2(0.7f, 0.16f));
            var sprite = LoadSprite(Root + "/Art/UI/boton_azul.png");
            if (sprite != null)
            {
                controller.missionContinueButton.image.sprite = sprite;
                controller.missionContinueButton.image.color = Color.white;
                controller.missionContinueButton.image.type = sprite.border.sqrMagnitude > 0 ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            }
            panel.gameObject.SetActive(false);
            Undo.RegisterCreatedObjectUndo(panel.gameObject, "Crear resumen de medallas");
            EditorUtility.SetDirty(controller);
        }

        private static void Move(RectTransform rect, Vector2 min, Vector2 max)
        {
            Undo.RecordObject(rect, "Ajustar resumen");
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static MedalHUD CreateCards(RectTransform parent, CharacterData[] characters, bool results)
        {
            var view = parent.gameObject.AddComponent<MedalHUD>();
            view.characters = characters; view.resultsMode = results;
            view.icons = new Image[characters.Length]; view.labels = new Text[characters.Length];
            int columns = results ? 4 : Mathf.Max(1, characters.Length);
            int rows = Mathf.CeilToInt((float)characters.Length / columns);
            for (int i = 0; i < characters.Length; i++)
            {
                int x = i % columns, y = i / columns;
                var card = Image(characters[i].productId, parent, new Color(0.15f, 0.015f, 0.02f, 0.8f),
                    new Vector2((float)x / columns + 0.003f, 1 - (float)(y + 1) / rows + 0.008f),
                    new Vector2((float)(x + 1) / columns - 0.003f, 1 - (float)y / rows - 0.008f));
                card.raycastTarget = false;
                var icon = Image("Imagen", card.transform, Color.white, new Vector2(0.12f, 0.42f), new Vector2(0.88f, 0.98f));
                icon.preserveAspect = true; icon.raycastTarget = false;
                var data = characters[i];
                icon.sprite = results && data.productSprite != null ? data.productSprite : data.medalSprite != null ? data.medalSprite : data.normalSprite;
                view.icons[i] = icon;
                view.labels[i] = Label("Progreso", card.transform, data.displayName + "\nx0 · 0/5", results ? 20 : 18,
                    new Vector2(0.02f, 0.01f), new Vector2(0.98f, 0.42f));
            }
            return view;
        }
    }
}
