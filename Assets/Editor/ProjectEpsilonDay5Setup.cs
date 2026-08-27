using System.IO;
using ProjectEpsilon.Player;
using ProjectEpsilon.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectEpsilon.Editor
{
    [InitializeOnLoad]
    public static class ProjectEpsilonDay5Setup
    {
        private const string GameScenePath = "Assets/Scenes/Game.unity";
        private const string BodySpritePath = "Assets/Art/Sprites/DebugSnakeBody.png";
        private const string SessionKey = "ProjectEpsilon.Day5.AutoSetup";

        private static readonly string[] LegacyAssets =
        {
            "Assets/Editor/ProjectEpsilonDay4Setup.cs",
            "Assets/Editor/ProjectEpsilonDay4Setup.cs.meta"
        };

        static ProjectEpsilonDay5Setup()
        {
            EditorApplication.delayCall += RunAutoSetup;
        }

        [MenuItem("Project Epsilon/Day 5/Run Setup")]
        public static void RunSetupFromMenu()
        {
            RunSetup(true);
        }

        private static void RunAutoSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                return;
            }

            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            SessionState.SetBool(SessionKey, true);
            RunSetup(false);
        }

        private static void RunSetup(bool force)
        {
            if (!File.Exists(GameScenePath))
            {
                Debug.LogWarning("[Project Epsilon] Game Scene이 없어 Day 5 자동 구성을 건너뜁니다.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

            if (!force && IsDay5Configured())
            {
                CleanupLegacyAssets();
                return;
            }

            GameObject gameplayRoot = EnsureRoot("===Gameplay===");
            GameObject uiRoot = EnsureRoot("===UI===");

            GameObject player = EnsurePlayer(gameplayRoot.transform);
            SnakePathRecorder recorder = player.GetComponent<SnakePathRecorder>();
            recorder.ResetHistory();

            SnakeBodyManager bodyManager = EnsureSnakeBody(gameplayRoot.transform, recorder);
            EnsureHUDBinding(uiRoot.transform, bodyManager);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameScenePath);

            CleanupLegacyAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = bodyManager.gameObject;
            Debug.Log("[Project Epsilon] Day 5 dynamic body management setup complete.");
        }

        private static bool IsDay5Configured()
        {
            GameObject gameplayRoot = GameObject.Find("===Gameplay===");
            GameObject uiRoot = GameObject.Find("===UI===");

            if (gameplayRoot == null || uiRoot == null)
            {
                return false;
            }

            Transform bodyRoot = gameplayRoot.transform.Find("SnakeBody");
            Transform hudCanvas = uiRoot.transform.Find("HUDCanvas");

            if (bodyRoot == null || hudCanvas == null)
            {
                return false;
            }

            SnakeBodyManager manager = bodyRoot.GetComponent<SnakeBodyManager>();
            SnakeBodyFollower follower = bodyRoot.GetComponent<SnakeBodyFollower>();
            SnakeBodyHUDPresenter presenter = hudCanvas.GetComponent<SnakeBodyHUDPresenter>();

            return manager != null &&
                follower != null &&
                presenter != null &&
                manager.CurrentBodyCount == 3 &&
                manager.MaximumBodyCount == 20 &&
                manager.TailSegment != null;
        }

        private static GameObject EnsureRoot(string rootName)
        {
            GameObject existing = GameObject.Find(rootName);

            if (existing != null && existing.transform.parent == null)
            {
                return existing;
            }

            GameObject root = new GameObject(rootName);
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            root.transform.localScale = Vector3.one;
            return root;
        }

        private static GameObject EnsurePlayer(Transform gameplayRoot)
        {
            Transform existingPlayer = gameplayRoot.Find("Player");
            GameObject player;

            if (existingPlayer != null)
            {
                player = existingPlayer.gameObject;
            }
            else
            {
                player = new GameObject("Player");
                player.transform.SetParent(gameplayRoot, false);
            }

            if (player.GetComponent<PlayerInputReader>() == null)
            {
                player.AddComponent<PlayerInputReader>();
            }

            if (player.GetComponent<SnakeMovement>() == null)
            {
                player.AddComponent<SnakeMovement>();
            }

            if (player.GetComponent<SnakePathRecorder>() == null)
            {
                player.AddComponent<SnakePathRecorder>();
            }

            return player;
        }

        private static SnakeBodyManager EnsureSnakeBody(
            Transform gameplayRoot,
            SnakePathRecorder recorder
        )
        {
            Transform existingBodyRoot = gameplayRoot.Find("SnakeBody");
            GameObject bodyRoot;

            if (existingBodyRoot != null)
            {
                bodyRoot = existingBodyRoot.gameObject;
            }
            else
            {
                bodyRoot = new GameObject("SnakeBody");
                bodyRoot.transform.SetParent(gameplayRoot, false);
            }

            SnakeBodyFollower follower = bodyRoot.GetComponent<SnakeBodyFollower>();

            if (follower == null)
            {
                follower = bodyRoot.AddComponent<SnakeBodyFollower>();
            }

            SnakeBodyManager manager = bodyRoot.GetComponent<SnakeBodyManager>();

            if (manager == null)
            {
                manager = bodyRoot.AddComponent<SnakeBodyManager>();
            }

            if (bodyRoot.GetComponent<SnakeBodyDebugControls>() == null)
            {
                bodyRoot.AddComponent<SnakeBodyDebugControls>();
            }

            Sprite bodySprite = AssetDatabase.LoadAssetAtPath<Sprite>(BodySpritePath);
            manager.Configure(recorder, follower, bodySprite, 3, 20, 0.58f);
            manager.ResetBody();

            return manager;
        }

        private static void EnsureHUDBinding(Transform uiRoot, SnakeBodyManager bodyManager)
        {
            Transform hudCanvas = uiRoot.Find("HUDCanvas");

            if (hudCanvas == null)
            {
                Debug.LogWarning("[Project Epsilon] HUDCanvas가 없어 BodyCount HUD 연결을 건너뜁니다.");
                return;
            }

            HUDController hudController = hudCanvas.GetComponent<HUDController>();

            if (hudController == null)
            {
                Debug.LogWarning("[Project Epsilon] HUDController가 없어 BodyCount HUD 연결을 건너뜁니다.");
                return;
            }

            SnakeBodyHUDPresenter presenter = hudCanvas.GetComponent<SnakeBodyHUDPresenter>();

            if (presenter == null)
            {
                presenter = hudCanvas.gameObject.AddComponent<SnakeBodyHUDPresenter>();
            }

            presenter.Bind(bodyManager, hudController);
        }

        private static void CleanupLegacyAssets()
        {
            foreach (string assetPath in LegacyAssets)
            {
                if (AssetDatabase.LoadAssetAtPath<Object>(assetPath) != null)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }
        }
    }
}
