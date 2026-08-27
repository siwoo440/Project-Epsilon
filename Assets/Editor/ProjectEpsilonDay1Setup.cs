using System.IO;
using System.Linq;
using ProjectEpsilon.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectEpsilon.Editor
{
    [InitializeOnLoad]
    public static class ProjectEpsilonDay1Setup
    {
        private const string GameScenePath = "Assets/Scenes/Game.unity";
        private const string SessionKey = "ProjectEpsilon.Day1.AutoSetup";

        private static readonly string[] RequiredFolders =
        {
            "Assets/Art",
            "Assets/Art/Sprites",
            "Assets/Art/Materials",
            "Assets/Art/VFX",
            "Assets/Audio",
            "Assets/Audio/BGM",
            "Assets/Audio/SFX",
            "Assets/Data",
            "Assets/Data/Characters",
            "Assets/Data/Enemies",
            "Assets/Data/Weapons",
            "Assets/Prefabs",
            "Assets/Prefabs/Characters",
            "Assets/Prefabs/Enemies",
            "Assets/Prefabs/Environment",
            "Assets/Prefabs/Items",
            "Assets/Prefabs/UI",
            "Assets/Prefabs/Weapons",
            "Assets/Scenes",
            "Assets/Scripts",
            "Assets/Scripts/Core",
            "Assets/Scripts/Player",
            "Assets/Scripts/Combat",
            "Assets/Scripts/Enemies",
            "Assets/Scripts/Weapons",
            "Assets/Scripts/UI",
            "Assets/Scripts/Data",
            "Assets/Settings"
        };

        static ProjectEpsilonDay1Setup()
        {
            EditorApplication.delayCall += RunAutoSetup;
        }

        [MenuItem("Project Epsilon/Day 1/Run Setup")]
        public static void RunSetupFromMenu()
        {
            RunSetup();
        }

        private static void RunAutoSetup()
        {
            // 플레이 모드 진입 중 자동 실행 방지
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            // 현재 에디터 세션 중복 실행 방지
            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            SessionState.SetBool(SessionKey, true);
            RunSetup();
        }

        private static void RunSetup()
        {
            EnsureProjectFolders();

            // Game Scene 최초 생성
            if (!File.Exists(GameScenePath))
            {
                CreateGameScene();
            }

            EnsureBuildSettingsIncludesGameScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Project Epsilon] Day 1 setup complete.");
        }

        private static void EnsureProjectFolders()
        {
            foreach (string folderPath in RequiredFolders)
            {
                EnsureFolder(folderPath);
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            // 기존 폴더 유지
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parentPath = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(folderPath);

            // 상위 폴더 우선 생성
            if (!string.IsNullOrEmpty(parentPath) && !AssetDatabase.IsValidFolder(parentPath))
            {
                EnsureFolder(parentPath);
            }

            if (!string.IsNullOrEmpty(parentPath))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        private static void CreateGameScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject managers = CreateRoot("===Managers===");
            CreateRoot("===Gameplay===");
            CreateRoot("===Environment===");
            CreateRoot("===UI===");

            // 게임 관리자 생성
            GameObject gameManagerObject = new GameObject("GameManager");
            gameManagerObject.transform.SetParent(managers.transform);
            gameManagerObject.AddComponent<GameManager>();

            // 2D 테스트 카메라 생성
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameScenePath);

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(GameScenePath);
        }

        private static GameObject CreateRoot(string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            root.transform.localScale = Vector3.one;
            return root;
        }

        private static void EnsureBuildSettingsIncludesGameScene()
        {
            EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;

            // 기존 등록 상태 유지
            if (currentScenes.Any(scene => scene.path == GameScenePath))
            {
                return;
            }

            EditorBuildSettings.scenes = currentScenes
                .Concat(new[] { new EditorBuildSettingsScene(GameScenePath, true) })
                .ToArray();
        }
    }
}
