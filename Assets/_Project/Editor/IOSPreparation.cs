using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BatallaDigestiva.Editor
{
    public static partial class PrototypeBuilder
    {
        [MenuItem("Batalla Digestiva/Preparar proyecto para iPad (iOS)")]
        public static void PrepareForIPad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Sal de Play antes de preparar iOS.");
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                Debug.LogError("Falta la escena BatallaPrototype. Crea o abre el prototipo primero.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            // Keep other entries available but disabled; the game must be the startup scene.
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.path != ScenePath)
                .Select(scene => new EditorBuildSettingsScene(scene.path, false)).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            PlayerSettings.productName = "Batalla Digestiva";
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPadOnly;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            AssetDatabase.SaveAssets();

            Debug.Log("Configuracion guardada: iPad, ambas orientaciones horizontales, SDK de dispositivo, " +
                "IL2CPP y BatallaPrototype como unica escena habilitada. " +
                "Se conservan la version minima de iOS, identificador y firma actuales. " +
                "Si usas un Build Profile con ajustes propios, revisa que no reemplace estos valores ni la lista de escenas.");
            string identifier = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS);
            if (string.IsNullOrWhiteSpace(identifier) || identifier.Contains("DefaultCompany"))
                Debug.LogWarning("Pendiente: define el Bundle Identifier acordado con tu equipo en " +
                    "Player Settings > iOS > Other Settings > Identification. No se ha inventado un identificador del cliente.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
                Debug.LogWarning("Pendiente: instala iOS Build Support para esta version de Unity desde Unity Hub.");
            Debug.Log("Siguiente paso: File > Build Profiles > iOS > Switch Profile. " +
                "Selecciona Build para exportar a una carpeta fuera de Assets. " +
                "La configuracion no equivale a una compilacion ni a una prueba en iPad; Xcode y la firma se completan en Mac.");
        }
    }
}
