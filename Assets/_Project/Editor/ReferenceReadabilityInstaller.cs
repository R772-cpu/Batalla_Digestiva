using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva.Editor
{
    public static partial class PrototypeBuilder
    {
        private static void RefineReferencePresentation(GameController game)
        {
            // The artwork has transparent/glowing margins: position text in its painted center.
            foreach (var view in new[] { game.medalsHUD, game.missionMedals, game.resultsProducts })
            {
                if (view == null) continue;
                bool hud = view == game.medalsHUD;
                for (int i = 0; i < view.characters.Length; i++)
                {
                    var card = view.icons[i].transform.parent;
                    if (hud)
                    {
                        int slot = System.Array.IndexOf(ProductOrder, view.characters[i].productId);
                        if (slot < 0) slot = i;
                        Move((RectTransform)card, new Vector2(slot / 8f, 0), new Vector2((slot + 1) / 8f, 1));
                        var background = card.GetComponent<UnityEngine.UI.Image>();
                        if (background != null) { Undo.RecordObject(background, "Limpiar HUD"); background.color = Color.clear; }
                        Move(view.icons[i].rectTransform, new Vector2(.08f, .13f), new Vector2(.92f, .77f));
                        Move(view.labels[i].rectTransform, new Vector2(0, 0), new Vector2(1, .16f));
                        view.labels[i].resizeTextMaxSize = 14;
                    }
                    var tag = card.Find("EtiquetaProducto") as RectTransform;
                    if (tag == null) continue;
                    Move(tag, hud ? new Vector2(-.15f, .66f) : new Vector2(-.02f, .25f),
                        hud ? new Vector2(1.15f, 1.14f) : new Vector2(1.02f, .62f));
                    tag.SetAsLastSibling();
                    var name = tag.Find("Nombre").GetComponent<Text>();
                    Move(name.rectTransform, new Vector2(.17f, .35f), new Vector2(.84f, .69f));
                    Undo.RecordObject(name, "Ajustar nombre en etiqueta");
                    name.resizeTextMinSize = hud ? 9 : 12; name.resizeTextMaxSize = hud ? 17 : 24;
                    name.fontStyle = FontStyle.Bold;
                }
            }
            var q = game.questions;
            if (q != null)
            {
                var background = q.panel.GetComponent<UnityEngine.UI.Image>();
                Undo.RecordObject(background, "Fondo limpio de pregunta");
                background.sprite = null; background.color = new Color(.10f, .018f, .025f, 1);
                if (q.ribbonText != null)
                {
                    Move(q.ribbonText.rectTransform, new Vector2(.10f, .14f), new Vector2(.90f, .88f));
                    MakeBold(q.ribbonText);
                }
                Undo.RecordObject(q.questionText, "Contraste de pregunta");
                q.questionText.color = new Color(.23f, .045f, .025f); MakeBold(q.questionText);
            }
            RefineFinalScore(game);
            RefineInstructionsHeader(game);
            InstallMissionTitleArtwork(game);
            InstallClientReviewLayout(game);
            ApplyStaticFontWeights(game);
        }

        private static void RefineFinalScore(GameController game)
        {
            var panel = game.resultsPanel.transform;
            var frame = EnsureImage("MarcoPuntajeFinal", panel, new Vector2(.10f, .48f), new Vector2(.90f, 1.20f), Color.clear);
            frame.transform.SetAsFirstSibling();
            Bind(panel, frame.name, "UI/marco_puntaje_final.png");
            // Coordinates inside the supplied PNG: burgundy panel x=.30..70, y=.35..64.
            Undo.SetTransformParent(game.resultText.transform, frame.transform, "Colocar puntaje dentro del marco");
            Move(game.resultText.rectTransform, new Vector2(.32f, .42f), new Vector2(.60f, .565f));
            Undo.RecordObject(game.resultText, "Tamaño del puntaje final");
            game.resultText.resizeTextMinSize = 24; game.resultText.resizeTextMaxSize = 64;
            game.resultText.text = "0";
            var star = EnsureImage("Estrella", frame.transform, new Vector2(.605f, .43f), new Vector2(.675f, .55f), Color.clear);
            star.preserveAspect = true; Bind(frame.transform, star.name, "UI/estrella.png");
            var title = EnsureImage("EtiquetaPuntaje", frame.transform, new Vector2(.275f, .55f), new Vector2(.725f, .80f), Color.clear);
            Bind(frame.transform, title.name, "UI/etiqueta_producto.png");
            MakeBold(EnsureLabel("Titulo", title.transform, "TU PUNTAJE", 30, new Vector2(.17f, .35f), new Vector2(.84f, .69f)));
            Undo.RecordObject(game, "Separar detalles del puntaje");
            game.resultDetailsText = EnsureLabel("Detalles", frame.transform, "RÉCORD 0", 18, new Vector2(.32f, .36f), new Vector2(.68f, .42f));
            game.resultDetailsText.resizeTextMinSize = 10;
        }

        private static void RefineInstructionsHeader(GameController game)
        {
            var how = game.GetComponent<HowToPlayPanel>(); if (how == null) return;
            var parent = how.panel.transform;
            var titleSprite = LoadSprite(Root + "/Art/UI/title_como_jugar.png");
            if (titleSprite != null)
            {
                HideInstructionObject(parent.Find("Titulo"));
                var title = EnsureImage("TituloComoJugar", parent, new Vector2(.30f, .83f), new Vector2(.69f, .99f), Color.white);
                title.sprite = titleSprite; title.preserveAspect = true;
            }
            var logo = EnsureImage("LogoInstrucciones", parent, new Vector2(.75f, .81f), new Vector2(.98f, .99f), Color.clear);
            logo.preserveAspect = true; Bind(parent, logo.name, "Branding/logo_batalla_digestiva.png");
            var composition = parent.Find("InstruccionesCompletas/Composicion");
            if (composition == null) return;
            float[] x = { -.001f, .26f, .508f, .746f };
            for (int i = 0; i < 4; i++)
            {
                var badge = EnsureImage("NumeroNuevo" + (i + 1), composition, new Vector2(x[i], .755f), new Vector2(x[i] + .075f, 1.025f), Color.clear);
                Bind(composition, badge.name, "UI/etiqueta_numero.png");
                MakeBold(EnsureLabel("Numero", badge.transform, (i + 1).ToString(), 46, new Vector2(.16f, .14f), new Vector2(.84f, .85f)));
            }
            var footer = parent.Find("ReglasFinales");
            if (footer != null)
            {
                Move((RectTransform)footer, new Vector2(.08f, .18f), new Vector2(.92f, .34f));
                HideInstructionObject(footer.Find("Texto"));
                var badge = EnsureImage("Numero5", footer, new Vector2(-.015f, .70f), new Vector2(.055f, 1.09f), Color.clear);
                badge.preserveAspect = true; Bind(footer, badge.name, "UI/etiqueta_numero.png");
                MakeBold(EnsureLabel("Numero", badge.transform, "5", 38, new Vector2(.15f, .15f), new Vector2(.85f, .85f)));
                var star = EnsureImage("EstrellaBonus", footer, new Vector2(.045f, .18f), new Vector2(.15f, .76f), Color.clear);
                star.preserveAspect = true; Bind(footer, star.name, "UI/estrella.png");
                var bonus = EnsureLabel("ReglaPregunta", footer, "Al final, responde una pregunta.\n¡Si aciertas, ganas +20 puntos extra!", 24,
                    new Vector2(.16f, .14f), new Vector2(.48f, .86f));
                var divider = EnsureImage("Separador", footer, new Vector2(.50f, .18f), new Vector2(.502f, .82f), new Color(1, 1, 1, .5f));
                var product = EnsureImage("ProductoEjemplo", footer, new Vector2(.52f, .08f), new Vector2(.68f, .92f), Color.clear);
                product.preserveAspect = true; Bind(footer, product.name, "Products/quanox_producto.png");
                var sparkles = EnsureImage("SparklesEjemplo", footer, new Vector2(.51f, .04f), new Vector2(.69f, .96f), Color.clear);
                sparkles.preserveAspect = true; Bind(footer, sparkles.name, "Effects/sparkles.png");
                var power = EnsureLabel("ReglaPotenciador", footer, "Toca el producto brillante para eliminar todos los bichos de ese producto en pantalla.", 24,
                    new Vector2(.70f, .14f), new Vector2(.96f, .86f));
                foreach (var label in new[] { bonus, power })
                {
                    Undo.RecordObject(label, "Texto blanco de instrucciones");
                    label.color = Color.white; label.resizeTextMinSize = 12; MakeBold(label);
                }
            }
        }

        private static void InstallMissionTitleArtwork(GameController game)
        {
            if (game.missionPanel == null) return;
            var sprite = LoadSprite(Root + "/Art/UI/title_mision_completada.png");
            if (sprite == null) return;
            var title = EnsureImage("ArteMisionCompletada", game.missionPanel.transform,
                new Vector2(.25f, .77f), new Vector2(.75f, 1.07f), Color.white);
            Undo.RecordObject(title, "Conectar titulo de mision");
            title.sprite = sprite; title.preserveAspect = true;
            Undo.RecordObject(game, "Asignar titulo de mision");
            game.missionTitleArtwork = title.gameObject;
            Undo.RecordObject(title.gameObject, "Mostrar titulo grafico");
            title.gameObject.SetActive(true);
            if (game.missionTitle != null) HideInstructionObject(game.missionTitle.transform);
        }

        private static void ApplyStaticFontWeights(GameController game)
        {
            var regular = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Art/Fonts/Montserrat-Regular.ttf");
            var bold = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Art/Fonts/Montserrat-Bold.ttf");
            if (regular == null || bold == null) return;
            foreach (var label in game.menuPanel.transform.parent.GetComponentsInChildren<Text>(true))
            {
                bool heavy = label.fontStyle == FontStyle.Bold || label.font == bold;
                Undo.RecordObject(label, "Aplicar peso real de Montserrat");
                label.font = heavy ? bold : regular;
                // Each file already contains its actual weight; no synthetic bold required.
                label.fontStyle = FontStyle.Normal;
            }
        }
    }
}
