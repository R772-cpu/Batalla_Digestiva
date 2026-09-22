using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva.Editor
{
    public static partial class PrototypeBuilder
    {
        private static void InstallArtDetails(GameController game)
        {
            font = game.scoreText.font;
            var menu = game.menuPanel.transform;
            EnsureImage("MarcaSiegfried", menu, new Vector2(.40f, .025f), new Vector2(.60f, .12f), Color.clear).preserveAspect = true;
            EnsureImage("MarcaLinea", menu, new Vector2(.025f, .025f), new Vector2(.25f, .12f), Color.clear).preserveAspect = true;
            Bind(menu, "MarcaSiegfried", "Branding/logo_siegfried.png");
            Bind(menu, "MarcaLinea", "Branding/logo_linea.png");
            var how = game.GetComponent<HowToPlayPanel>();
            if (how != null)
            {
                for (int i = 1; i <= 4; i++) Bind(how.panel.transform, "FichaPaso" + i, "UI/marco_instruccion.png");
                Bind(how.panel.transform, "ReglasFinales", "UI/marco_instrucciones_extra.png");
                InstallCombinedInstructions(how);
            }
            var hud = game.gamePanel.transform.Find("HUD");
            if (hud != null)
            {
                Move(game.scoreText.rectTransform, new Vector2(.81f, .75f), new Vector2(.945f, .82f));
                var star = EnsureImage("EstrellaPuntos", hud, new Vector2(.947f, .758f), new Vector2(.975f, .812f), Color.clear);
                star.preserveAspect = true; Bind(hud, "EstrellaPuntos", "UI/estrella.png");
                Move((RectTransform)game.pauseButton.transform, new Vector2(.025f, .78f), new Vector2(.12f, .855f));
                Bind(game.homeButton.transform.parent, game.homeButton.name, "UI/icono_home.png");
                ApplyIconLabel(game.homeButton, LoadSprite(Root + "/Art/UI/icono_home.png"));
            }
            var view = game.medalsHUD;
            if (view != null)
            {
                Undo.RecordObject(view, "Separar nombres del HUD");
                view.productTitles = new Text[view.characters.Length];
                for (int i = 0; i < view.characters.Length; i++)
                {
                    var card = view.icons[i].transform.parent;
                    var tag = EnsureImage("EtiquetaProducto", card, new Vector2(.01f, .21f), new Vector2(.99f, .40f), new Color(1, .68f, 0));
                    view.productTitles[i] = EnsureLabel("Nombre", tag.transform, view.characters[i].displayName, 16, new Vector2(.04f, 0), new Vector2(.96f, 1));
                    view.productTitles[i].resizeTextMinSize = 9;
                    Move(view.icons[i].rectTransform, new Vector2(.12f, .42f), new Vector2(.88f, .99f));
                    Move(view.labels[i].rectTransform, new Vector2(0, 0), new Vector2(1, .20f));
                    view.labels[i].resizeTextMinSize = 9;
                    Bind(card, "EtiquetaProducto", "UI/etiqueta_producto.png");
                }
                EditorUtility.SetDirty(view);
            }
            if (game.resultsPanel != null)
            {
                var frame = EnsureImage("MarcoPuntajeFinal", game.resultsPanel.transform, new Vector2(.26f, .745f), new Vector2(.96f, .985f), Color.clear);
                frame.transform.SetAsFirstSibling();
                Move(game.resultText.rectTransform, new Vector2(.29f, .76f), new Vector2(.93f, .97f));
                Bind(game.resultsPanel.transform, "MarcoPuntajeFinal", "UI/marco_puntaje_final.png");
            }
            if (game.missionTitle != null) Move(game.missionTitle.rectTransform, new Vector2(.26f, .82f), new Vector2(.96f, .98f));
            foreach (var panel in new[] { game.menuPanel, game.gamePanel, how != null ? how.panel : null,
                game.questions != null ? game.questions.panel : null, game.missionPanel, game.resultsPanel })
            {
                if (panel == null) continue;
                CreateCorner(game, panel.transform, true);
                if (panel != game.menuPanel && panel != game.gamePanel) CreateCorner(game, panel.transform, false);
            }
            var sparkles = LoadSprite(Root + "/Art/Effects/sparkles.png");
            if (sparkles != null && game.powerUps != null)
            {
                Undo.RecordObject(game.powerUps, "Conectar sparkles"); game.powerUps.sparkleSprite = sparkles;
            }
            foreach (var button in game.menuPanel.transform.parent.GetComponentsInChildren<Button>(true))
                foreach (var label in button.GetComponentsInChildren<Text>(true)) MakeBold(label);
            MakeBold(game.scoreText); MakeBold(game.timerText); MakeBold(game.resultText);
            if (game.questions != null) MakeBold(game.questions.questionScoreText);
            RefineReferencePresentation(game);
            InstallAudio(game);
        }

        private static void MakeBold(Text label)
        {
            if (label == null) return;
            Undo.RecordObject(label, "Reforzar tipografia"); label.fontStyle = FontStyle.Bold;
        }

        private static void CreateCorner(GameController game, Transform panel, bool sound)
        {
            string name = sound ? "ControlSonido" : "ControlInicio";
            var existing = panel.Find(name);
            var button = existing != null ? existing.GetComponent<Button>() : Button(name, panel,
                new Vector2(sound ? .025f : .13f, .875f), new Vector2(sound ? .12f : .225f, .98f));
            if (existing == null) Undo.RegisterCreatedObjectUndo(button.gameObject, "Crear control de esquina");
            var caption = button.GetComponentInChildren<Text>(true);
            if (caption != null) { Undo.RecordObject(caption, "Texto de control"); caption.text = sound ? "SONIDO" : "INICIO"; }
            var control = button.GetComponent<CornerControl>();
            if (control == null) control = Undo.AddComponent<CornerControl>(button.gameObject);
            Undo.RecordObject(control, "Configurar control"); control.game = game; control.sound = sound;
            var icon = LoadSprite(Root + (sound ? "/Art/UI/icono_sonido.png" : "/Art/UI/icono_home.png"));
            if (icon != null) control.enabledIcon = icon;
            var muted = LoadSprite(Root + "/Art/UI/icono_silencio.png");
            if (muted != null) control.mutedIcon = muted;
            ApplyIconLabel(button, control.enabledIcon);
        }

        private static void ApplyIconLabel(Button button, Sprite icon)
        {
            if (icon == null) return;
            Undo.RecordObject(button.image, "Aplicar icono");
            button.image.sprite = icon; button.image.color = Color.white; button.image.preserveAspect = true;
            button.image.type = UnityEngine.UI.Image.Type.Simple;
            foreach (var label in button.GetComponentsInChildren<Text>(true))
            { Undo.RecordObject(label.gameObject, "Ocultar texto de icono"); label.gameObject.SetActive(false); }
        }
    }
}
