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
        [MenuItem("Batalla Digestiva/Conectar sonidos y volumen")]
        public static void ConnectSounds()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogWarning("Sal de Play primero."); return; }
            var scene = SceneManager.GetActiveScene();
            var game = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<GameController>(true)).FirstOrDefault();
            if (game == null) { Debug.LogError("Abre BatallaPrototype primero."); return; }
            InstallAudio(game);
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Sonidos conectados. El boton de sonido abre musica, efectos y mute. Guarda la escena con Ctrl+S.");
        }

        private static void InstallAudio(GameController game)
        {
            var audio = game.GetComponent<GameAudio>();
            if (audio == null) audio = Undo.AddComponent<GameAudio>(game.gameObject);
            Undo.RecordObject(audio, "Conectar sonidos");
            audio.spawn = Sound("aparición_monstruo.wav");
            audio.squash = Sound("Aplastar_monstruo.wav");
            audio.buttons = Sound("Botones.wav");
            audio.bubble = Sound("Burbuja.wav");
            audio.music = Sound("Canción_loop.mp3");
            audio.powerUp = Sound("Power_Up.mp3");
            audio.victory = Sound("Victoria.mp3");
            audio.correct = Sound("Correct.mp3"); audio.wrong = Sound("Wrong.mp3");
            font = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Art/Fonts/Montserrat-Bold.ttf") ?? game.scoreText.font;
            var parent = game.menuPanel.transform.parent;
            var overlay = EnsureImage("AjustesSonido", parent, Vector2.zero, Vector2.one, new Color(0, 0, 0, .8f));
            overlay.raycastTarget = true;
            audio.settingsPanel = overlay.gameObject;
            var card = EnsureImage("Panel", overlay.transform, new Vector2(.23f, .22f), new Vector2(.77f, .78f), new Color(.20f, .025f, .035f));
            EnsureLabel("Titulo", card.transform, "SONIDO", 36, new Vector2(.1f, .81f), new Vector2(.9f, .97f));
            EnsureLabel("Musica", card.transform, "MÚSICA", 24, new Vector2(.08f, .66f), new Vector2(.38f, .79f));
            EnsureLabel("Efectos", card.transform, "EFECTOS", 24, new Vector2(.08f, .42f), new Vector2(.38f, .55f));
            audio.musicSlider = AudioSlider("VolumenMusica", card.transform, .69f, .5f);
            audio.effectsSlider = AudioSlider("VolumenEfectos", card.transform, .45f, 1);
            audio.muteButton = AudioButton("Silenciar", card.transform, new Vector2(.08f, .08f), new Vector2(.56f, .28f));
            audio.closeButton = AudioButton("Cerrar", card.transform, new Vector2(.61f, .08f), new Vector2(.92f, .28f));
            audio.muteLabel = audio.muteButton.GetComponentInChildren<Text>(true);
            foreach (var button in parent.GetComponentsInChildren<Button>(true))
                if ((game.powerUps == null || button != game.powerUps.button) && button.GetComponent<UIButtonAudio>() == null)
                    Undo.AddComponent<UIButtonAudio>(button.gameObject);
            overlay.gameObject.SetActive(false);
            EditorUtility.SetDirty(audio);
        }

        private static AudioClip Sound(string file)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Sounds/" + file);
            if (clip == null) Debug.LogWarning("No se pudo cargar el sonido: " + file);
            return clip;
        }

        private static Button AudioButton(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<Button>();
            var button = Button(name.ToUpperInvariant(), parent, min, max);
            button.name = name;
            Undo.RegisterCreatedObjectUndo(button.gameObject, "Crear control de audio");
            return button;
        }

        private static Slider AudioSlider(string name, Transform parent, float y, float initial)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<Slider>();
            var root = Rect(name, parent, new Vector2(.40f, y - .04f), new Vector2(.91f, y + .08f));
            Undo.RegisterCreatedObjectUndo(root.gameObject, "Crear volumen");
            var slider = root.gameObject.AddComponent<Slider>();
            var background = Image("Pista", root, new Color(.4f, .25f, .26f), new Vector2(0, .35f), new Vector2(1, .65f));
            var fill = Image("Nivel", root, new Color(1, .7f, .04f), new Vector2(0, .35f), new Vector2(1, .65f));
            fill.raycastTarget = false;
            var handle = Image("Control", root, Color.white, Vector2.zero, Vector2.one);
            handle.rectTransform.sizeDelta = new Vector2(24, 0);
            slider.fillRect = fill.rectTransform; slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight; slider.minValue = 0; slider.maxValue = 1; slider.value = initial;
            return slider;
        }
    }
}
