using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BatallaDigestiva.Editor
{
    public static partial class PrototypeBuilder
    {
        private const string Root = "Assets/_Project";
        private const string ScenePath = Root + "/Scenes/BatallaPrototype.unity";
        private static Font font;

        [MenuItem("Batalla Digestiva/Crear o abrir prototipo")]
        public static void CreateOrOpen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Sal del modo Play antes de crear o abrir el prototipo.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
                AddQuestionsToScene();
                return;
            }
            foreach (var folder in new[] { "Data", "Scenes", "Prefabs", "Art/Characters" })
                Directory.CreateDirectory(Root + "/" + folder);
            AssetDatabase.Refresh();
            var background = LoadSprite("Assets/Sprites/Background/backgroundGame.png");
            if (background == null)
            {
                Debug.LogError("Falta un sprite de fondo en Assets/Sprites/Background/backgroundGame.png.");
                return;
            }
            var characters = LoadCharacters();
            if (characters.Length == 0)
            {
                Debug.LogError("No hay personajes disponibles. Coloca PNG con nombre <producto>_normal.png en Assets/_Project/Art/Characters/<Producto>/.");
                return;
            }
            var normal = characters[0].normalSprite;
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(Root + "/Data/PrototypeConfig.asset");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GameConfig>();
                AssetDatabase.CreateAsset(config, Root + "/Data/PrototypeConfig.asset");
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 0, -10);
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.15f, 0.025f, 0.04f);

            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1200, 900);
            scaler.matchWidthOrHeight = 0.5f;
            var back = Image("Background", canvasObject.transform, Color.white);
            back.sprite = background;
            back.raycastTarget = false;
            var fit = back.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.None;
            fit.aspectRatio = background.rect.width / background.rect.height;
            var safe = Rect("SafeArea", canvasObject.transform);
            safe.gameObject.AddComponent<SafeArea>();

            var controller = new GameObject("GameSystems").AddComponent<GameController>();
            controller.config = config;
            controller.spawner = controller.gameObject.AddComponent<TargetSpawner>();
            controller.spawner.characters = characters;
            var game = Rect("GamePanel", safe);
            controller.gamePanel = game.gameObject;
            var area = Rect("PlayArea", game, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.79f));
            controller.spawner.playArea = area;
            var hud = Image("HUD", game, new Color(0.16f, 0.01f, 0.025f, 0.92f), new Vector2(0, 0.83f), Vector2.one);
            controller.homeButton = Button("Inicio", hud.transform, new Vector2(0.02f, 0.2f), new Vector2(0.19f, 0.8f));
            controller.pauseButton = Button("Pausa", hud.transform, new Vector2(0.21f, 0.2f), new Vector2(0.38f, 0.8f));
            controller.scoreText = Label("Puntos", hud.transform, "PUNTOS  0", 30, new Vector2(0.42f, 0.15f), new Vector2(0.78f, 0.85f));
            controller.timerText = Label("Reloj", hud.transform, "01:00", 38, new Vector2(0.8f, 0.15f), new Vector2(0.98f, 0.85f));

            var menu = Image("MenuPanel", safe, new Color(0.12f, 0.01f, 0.02f, 0.82f));
            controller.menuPanel = menu.gameObject;
            Label("Titulo", menu.transform, "BATALLA\nDIGESTIVA", 64, new Vector2(0.1f, 0.57f), new Vector2(0.9f, 0.88f));
            Label("Instrucciones", menu.transform, "Toca los personajes para sumar puntos.\nTienes 60 segundos.\n\nPrototipo: partida basica", 28, new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.56f));
            controller.startButton = Button("JUGAR", menu.transform, new Vector2(0.3f, 0.12f), new Vector2(0.7f, 0.25f));

            var results = Image("ResultsPanel", safe, new Color(0.12f, 0.01f, 0.02f, 0.94f));
            controller.resultsPanel = results.gameObject;
            controller.resultText = Label("Resultado", results.transform, "RONDA TERMINADA", 40, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.85f));
            controller.restartButton = Button("JUGAR DE NUEVO", results.transform, new Vector2(0.26f, 0.23f), new Vector2(0.74f, 0.36f));
            controller.resultsHomeButton = Button("INICIO", results.transform, new Vector2(0.35f, 0.06f), new Vector2(0.65f, 0.19f));

            var pause = Image("PausePanel", safe, new Color(0.08f, 0.01f, 0.02f, 0.95f));
            controller.pausePanel = pause.gameObject;
            Label("Pausa", pause.transform, "PAUSA", 60, new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.8f));
            controller.resumeButton = Button("CONTINUAR", pause.transform, new Vector2(0.3f, 0.25f), new Vector2(0.7f, 0.4f));

            string prefabPath = Root + "/Prefabs/Target.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<TargetView>(prefabPath);
            if (prefab == null)
            {
                var template = new GameObject("Target", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image), typeof(TargetView));
                var targetImage = template.GetComponent<UnityEngine.UI.Image>();
                targetImage.sprite = normal; targetImage.preserveAspect = true;
                ((RectTransform)template.transform).sizeDelta = Vector2.one * config.targetSize;
                prefab = PrefabUtility.SaveAsPrefabAsset(template, prefabPath).GetComponent<TargetView>();
                Object.DestroyImmediate(template);
            }
            controller.spawner.targetPrefab = prefab;
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            game.gameObject.SetActive(false); results.gameObject.SetActive(false); pause.gameObject.SetActive(false);
            InstallQuestions(controller);
            InstallGameplay(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = controller.gameObject;
            Debug.Log("Prototipo creado con " + characters.Length + " personajes. Pulsa Play y JUGAR. Configuracion: Assets/_Project/Data/PrototypeConfig.asset");
        }

        [MenuItem("Batalla Digestiva/Agregar preguntas a la escena actual")]
        public static void AddQuestionsToScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Sal de Play antes de agregar las preguntas.");
                return;
            }
            var scene = SceneManager.GetActiveScene();
            var controller = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<GameController>(true)).FirstOrDefault();
            if (controller == null)
            {
                Debug.LogError("Abre BatallaPrototype antes de agregar las preguntas.");
                return;
            }
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Agregar preguntas de Batalla Digestiva");
            InstallQuestions(controller);
            InstallGameplay(controller);
            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            // Dejar el guardado al usuario mantiene el control sobre sus otros ajustes de escena.
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = controller.gameObject;
            Debug.Log("Escena actualizada: medallas, potenciadores y una pregunta al final. Guarda con Ctrl+S y pulsa Play.");
        }

        private static void InstallQuestions(GameController controller)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var questions = controller.questions != null ? controller.questions : controller.GetComponent<QuestionController>();
            if (questions == null) questions = Undo.AddComponent<QuestionController>(controller.gameObject);
            Undo.RecordObject(controller, "Conectar preguntas");
            Undo.RecordObject(questions, "Configurar preguntas");
            controller.questions = questions;
            if (questions.bank == null || questions.bank.Length == 0) questions.bank = CreateQuestionBank();
            if (questions.panel == null)
            {
                var parent = controller.menuPanel.transform.parent;
                var panel = Image("QuestionPanel", parent, new Color(0.12f, 0.01f, 0.02f, 0.97f));
                questions.panel = panel.gameObject;
                questions.progressText = Label("Progreso", panel.transform, "PREGUNTA", 32, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.96f));
                questions.questionText = Label("Enunciado", panel.transform, "", 36, new Vector2(0.08f, 0.59f), new Vector2(0.92f, 0.85f));
                questions.feedbackText = Label("Feedback", panel.transform, "", 26, new Vector2(0.08f, 0.17f), new Vector2(0.92f, 0.28f));
                questions.answerButtons = new Button[3];
                questions.answerLabels = new Text[3];
                for (int i = 0; i < 3; i++)
                {
                    float top = 0.58f - i * 0.1f;
                    var button = Button(((char)('A' + i)).ToString(), panel.transform,
                        new Vector2(0.1f, top - 0.085f), new Vector2(0.9f, top));
                    var colors = button.colors;
                    colors.disabledColor = Color.white;
                    button.colors = colors;
                    questions.answerButtons[i] = button;
                    questions.answerLabels[i] = button.GetComponentInChildren<Text>();
                }
                questions.continueButton = Button("CONTINUAR", panel.transform, new Vector2(0.3f, 0.035f), new Vector2(0.7f, 0.15f));
                panel.gameObject.SetActive(false);
                Undo.RegisterCreatedObjectUndo(panel.gameObject, "Crear panel de preguntas");
                // La pausa por perder foco debe bloquear tambien las respuestas.
                controller.pausePanel.transform.SetAsLastSibling();
            }
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(questions);
        }

        private static QuestionData[] CreateQuestionBank()
        {
            Directory.CreateDirectory(Root + "/Data/Questions");
            AssetDatabase.Refresh();
            // Contenido transcrito del documento del usuario; banco provisional, no validacion medica.
            return new[]
            {
                Question("gastro_modulex", "¿Cuál es el principio activo de Modulex®?", new[] { "Domperidona", "Lubiprostone", "Pirantel" }, 1),
                Question("gastro_drenabyl", "¿En qué presentación viene Drenabyl®?", new[] { "Jarabe de 120 mL", "Tabletas", "Gotas" }, 0),
                Question("gastro_moperid", "¿Cuál es el principio activo de Moperid®?", new[] { "Lubiprostone", "Domperidona", "Ivermectina" }, 1),
                Question("antiparasitaria_mixel_activo", "¿Cuál es el principio activo de Mixel®?", new[] { "Ivermectina", "Pirantel", "Nitazoxanida" }, 2),
                Question("antiparasitaria_mixel_posologia", "En adultos y niños mayores de 12 años, ¿cuál es la posología de Mixel® tabletas?", new[] { "500 mg una vez al día durante 5 días", "500 mg cada 12 horas durante 3 días", "250 mg cada 8 horas durante 7 días" }, 1)
            };
        }

        private static QuestionData Question(string id, string prompt, string[] answers, int correct)
        {
            string path = Root + "/Data/Questions/" + id + ".asset";
            var question = AssetDatabase.LoadAssetAtPath<QuestionData>(path);
            if (question != null) return question; // Conservar cualquier edicion del usuario.
            question = ScriptableObject.CreateInstance<QuestionData>();
            question.id = id; question.question = prompt; question.answers = answers; question.correctAnswerIndex = correct;
            AssetDatabase.CreateAsset(question, path);
            return question;
        }

        private static CharacterData[] LoadCharacters()
        {
            var result = new List<CharacterData>();
            var paths = Directory.GetFiles(Root + "/Art/Characters", "*_normal.png", SearchOption.AllDirectories)
                .Select(path => path.Replace('\\', '/')).OrderBy(path => path).ToArray();
            foreach (var path in paths)
            {
                string id = Path.GetFileNameWithoutExtension(path);
                id = id.Substring(0, id.Length - "_normal".Length);
                var normal = LoadSprite(path);
                if (normal == null)
                {
                    Debug.LogError("No se pudo importar el personaje como Sprite: " + path);
                    continue;
                }
                string hitPath = path.Substring(0, path.Length - "_normal.png".Length) + "_aplastado.png";
                // Conserva las fichas creadas previamente, incluidas las del primer prototipo.
                var data = AssetDatabase.FindAssets("t:CharacterData", new[] { Root + "/Data" })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid)))
                    .FirstOrDefault(item => item != null && item.productId == id);
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<CharacterData>();
                    data.productId = id;
                    data.displayName = Path.GetFileName(Path.GetDirectoryName(path));
                    AssetDatabase.CreateAsset(data, AssetDatabase.GenerateUniqueAssetPath(Root + "/Data/" + data.displayName + ".asset"));
                }
                data.normalSprite = normal;
                var hit = LoadSprite(hitPath);
                if (hit != null) data.hitSprite = hit;
                else Debug.LogWarning("No se encontro el aplastado en " + hitPath + ". Se usara la animacion de encogimiento.");
                EditorUtility.SetDirty(data);
                result.Add(data);
            }
            if (result.Count == 0)
            {
                var legacy = LoadSprite("Assets/Sprites/Modulex.png");
                if (legacy != null)
                {
                    var data = AssetDatabase.LoadAssetAtPath<CharacterData>(Root + "/Data/Modulex.asset");
                    if (data == null)
                    {
                        data = ScriptableObject.CreateInstance<CharacterData>();
                        AssetDatabase.CreateAsset(data, Root + "/Data/Modulex.asset");
                    }
                    data.normalSprite = legacy;
                    EditorUtility.SetDirty(data);
                    result.Add(data);
                }
            }
            return result.ToArray();
        }

        private static Sprite LoadSprite(string path)
        {
            if (!File.Exists(path)) return null;
            // Un slice automatico puede separar gotas o adornos. Elegir el cuerpo principal.
            var sprite = SelectMainSprite(path);
            if (sprite != null) return sprite;
            // Los PNG recien exportados pueden llegar como Texture en lugar de Sprite.
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return null;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return SelectMainSprite(path);
        }

        private static Sprite SelectMainSprite(string path) => AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>().OrderByDescending(sprite => sprite.rect.width * sprite.rect.height)
            .ThenBy(sprite => sprite.name, System.StringComparer.Ordinal).FirstOrDefault();

        private static RectTransform Rect(string name, Transform parent, Vector2? min = null, Vector2? max = null)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min ?? Vector2.zero; rect.anchorMax = max ?? Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static Image Image(string name, Transform parent, Color color, Vector2? min = null, Vector2? max = null)
        {
            var image = Rect(name, parent, min, max).gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            return image;
        }

        private static Text Label(string name, Transform parent, string content, int size, Vector2 min, Vector2 max)
        {
            var text = Rect(name, parent, min, max).gameObject.AddComponent<Text>();
            text.font = font; text.text = content; text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter; text.color = Color.white;
            text.resizeTextForBestFit = true; text.resizeTextMinSize = 16; text.resizeTextMaxSize = size;
            text.raycastTarget = false;
            return text;
        }

        private static Button Button(string title, Transform parent, Vector2 min, Vector2 max)
        {
            var image = Image(title, parent, new Color(0, 0.46f, 0.7f), min, max);
            var button = image.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            var navigation = button.navigation; navigation.mode = Navigation.Mode.None; button.navigation = navigation;
            Label("Texto", image.transform, title, 30, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f));
            return button;
        }
    }
}
