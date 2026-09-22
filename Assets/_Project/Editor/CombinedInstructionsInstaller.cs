using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BatallaDigestiva.Editor
{
    public static partial class PrototypeBuilder
    {
        private static void InstallCombinedInstructions(HowToPlayPanel how)
        {
            var sprite = LoadSprite(Root + "/Art/UI/instrucciones.png");
            if (sprite == null) return;
            var parent = how.panel.transform;
            for (int i = 1; i <= 4; i++)
            {
                HideInstructionObject(parent.Find("FichaPaso" + i));
                HideInstructionObject(parent.Find("Paso" + i));
            }
            // Fit the entire composition together, so masks and captions stay registered
            // with the artwork on both iPad aspect ratios.
            var holder = parent.Find("InstruccionesCompletas") as RectTransform;
            if (holder == null)
            {
                holder = Rect("InstruccionesCompletas", parent);
                Undo.RegisterCreatedObjectUndo(holder.gameObject, "Crear instrucciones completas");
            }
            Move(holder, new Vector2(.025f, .38f), new Vector2(.975f, .81f));
            var composition = holder.Find("Composicion") as RectTransform;
            if (composition == null)
            {
                composition = Rect("Composicion", holder);
                Undo.RegisterCreatedObjectUndo(composition.gameObject, "Crear composicion");
            }
            var fitter = composition.GetComponent<AspectRatioFitter>();
            if (fitter == null) fitter = Undo.AddComponent<AspectRatioFitter>(composition.gameObject);
            Undo.RecordObject(fitter, "Conservar proporcion de instrucciones");
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
            var crop = composition.Find("Ilustraciones") as RectTransform;
            if (crop == null)
            {
                crop = Rect("Ilustraciones", composition);
                Undo.RegisterCreatedObjectUndo(crop.gameObject, "Crear mascara de textos antiguos");
                Undo.AddComponent<RectMask2D>(crop.gameObject);
            }
            const float cutoff = .345f;
            Move(crop, new Vector2(0, cutoff), Vector2.one);
            // Keep the source PNG untouched. Clip its green boxes and obsolete rules,
            // including behind transparent rounded corners of the new orange artwork.
            var art = EnsureImage("ImagenOriginal", crop, new Vector2(0, -cutoff / (1 - cutoff)), Vector2.one, Color.white);
            Undo.RecordObject(art, "Asignar instrucciones completas");
            art.sprite = sprite; art.color = Color.white; art.preserveAspect = false;
            art.type = UnityEngine.UI.Image.Type.Simple;
            float[] left = { .014f, .275f, .523f, .761f };
            float[] right = { .246f, .494f, .737f, .997f };
            string[] headings = { "TOCA", "SUMA PUNTOS", "TIENES 60 SEGUNDOS", "GANA MEDALLAS" };
            string[] captions = { "Toca los bichos apenas aparezcan.", "Cada eliminación suma 10 puntos.",
                "Consigue las medallas antes de que termine el tiempo.", "Cada 5 del mismo producto ganas una medalla. ¡Puedes repetirla!" };
            for (int i = 0; i < 4; i++)
            {
                var box = EnsureImage("TextoPaso" + (i + 1), composition, new Vector2(left[i], -.015f),
                    new Vector2(right[i], .365f), new Color(1, .65f, .02f));
                Bind(composition, box.name, "UI/marco_instruccion.png");
                var heading = EnsureLabel("Titulo", box.transform, headings[i], 24, new Vector2(.09f, .58f), new Vector2(.91f, .86f));
                var caption = EnsureLabel("Descripcion", box.transform, captions[i], 22, new Vector2(.09f, .15f), new Vector2(.91f, .59f));
                Undo.RecordObject(heading, "Actualizar titulo de paso");
                Undo.RecordObject(caption, "Actualizar regla de paso");
                heading.text = headings[i]; caption.text = captions[i];
                heading.color = caption.color = Color.white;
                heading.fontStyle = FontStyle.Bold; caption.fontStyle = FontStyle.Bold;
                heading.resizeTextMinSize = 11; caption.resizeTextMinSize = 11;
            }
            var title = parent.Find("Titulo") as RectTransform;
            if (title != null) Move(title, new Vector2(.27f, .84f), new Vector2(.95f, .97f));
        }

        private static void HideInstructionObject(Transform child)
        {
            if (child == null) return;
            Undo.RecordObject(child.gameObject, "Ocultar instrucciones individuales");
            child.gameObject.SetActive(false);
        }
    }
}
