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
    public static class ProjectEpsilonDay6Setup
    {
        private const string GameScenePath = "Assets/Scenes/Game.unity";
        private const string BodySpritePath = "Assets/Art/Sprites/DebugSnakeBody.png";
        private const string SessionKey = "ProjectEpsilon.Day6.AutoSetup";

        private static readonly string[] LegacyAssets =
        {
            "Assets/Editor/ProjectEpsilonDay5Setup.cs",
            "Assets/Editor/ProjectEpsilonDay5Setup.cs.meta"
        };

        static ProjectEpsilonDay6Setup()
        {
            EditorApplication.delayCall += RunAutoSetup;
        }

        [MenuItem("Project Epsilon/Day 6/Run Setup")]
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
                Debug.LogWarning("[Project Epsilon] Game Scene이 없어 Day 6 자동 구성을 건너뜁니다.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

            if (!force && IsDay6Configured())
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
            SnakeInvulnerability invulnerability = EnsureInvulnerability(player);
            SnakeHealth health = EnsureHealth(player, bodyManager, invulnerability);
            SnakeSelfCollision selfCollision = EnsureSelfCollision(
                player,
                bodyManager,
                invulnerability
            );

            EnsureDamageDebugControls(player, health, selfCollision);
            EnsureHUDBinding(uiRoot.transform, bodyManager, health);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameScenePath);

            CleanupLegacyAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = player;
            Debug.Log("[Project Epsilon] Day 6 shared HP + self collision setup complete.");
        }

        private static bool IsDay6Configured()
        {
            GameObject gameplayRoot = GameObject.Find("===Gameplay===");
            GameObject uiRoot = GameObject.Find("===UI===");

            if (gameplayRoot == null || uiRoot == null)
            {
                return false;
            }

            Transform player = gameplayRoot.transform.Find("Player");
            Transform bodyRoot = gameplayRoot.transform.Find("SnakeBody");
            Transform hudCanvas = uiRoot.transform.Find("HUDCanvas");

            if (player == null || bodyRoot == null || hudCanvas == null)
            {
                return false;
            }

            return player.GetComponent<SnakeHealth>() != null &&
                player.GetComponent<SnakeInvulnerability>() != null &&
                player.GetComponent<SnakeSelfCollision>() != null &&
                player.GetComponent<SnakeDamageDebugControls>() != null &&
                player.GetComponent<CircleCollider2D>() != null &&
                player.GetComponent<Rigidbody2D>() != null &&
                bodyRoot.GetComponent<SnakeBodyManager>() != null &&
                hudCanvas.GetComponent<SnakeHealthHUDPresenter>() != null;
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

            CircleCollider2D headCollider = player.GetComponent<CircleCollider2D>();

            if (headCollider == null)
            {
                headCollider = player.AddComponent<CircleCollider2D>();
            }

            headCollider.isTrigger = true;
            headCollider.radius = 0.32f;

            Rigidbody2D rigidbody = player.GetComponent<Rigidbody2D>();

            if (rigidbody == null)
            {
                rigidbody = player.AddComponent<Rigidbody2D>();
            }

            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.gravityScale = 0f;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

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

        private static SnakeInvulnerability EnsureInvulnerability(GameObject player)
        {
            SnakeInvulnerability invulnerability =
                player.GetComponent<SnakeInvulnerability>();

            if (invulnerability == null)
            {
                invulnerability = player.AddComponent<SnakeInvulnerability>();
            }

            invulnerability.ClearInvulnerability();
            return invulnerability;
        }

        private static SnakeHealth EnsureHealth(
            GameObject player,
            SnakeBodyManager bodyManager,
            SnakeInvulnerability invulnerability
        )
        {
            SnakeHealth health = player.GetComponent<SnakeHealth>();

            if (health == null)
            {
                health = player.AddComponent<SnakeHealth>();
            }

            health.Configure(bodyManager, invulnerability, 100);
            return health;
        }

        private static SnakeSelfCollision EnsureSelfCollision(
            GameObject player,
            SnakeBodyManager bodyManager,
            SnakeInvulnerability invulnerability
        )
        {
            SnakeSelfCollision selfCollision =
                player.GetComponent<SnakeSelfCollision>();

            if (selfCollision == null)
            {
                selfCollision = player.AddComponent<SnakeSelfCollision>();
            }

            selfCollision.Bind(bodyManager, invulnerability, 2, 2f);
            return selfCollision;
        }

        private static void EnsureDamageDebugControls(
            GameObject player,
            SnakeHealth health,
            SnakeSelfCollision selfCollision
        )
        {
            SnakeDamageDebugControls controls =
                player.GetComponent<SnakeDamageDebugControls>();

            if (controls == null)
            {
                controls = player.AddComponent<SnakeDamageDebugControls>();
            }

            controls.Bind(health, selfCollision);
        }

        private static void EnsureHUDBinding(
            Transform uiRoot,
            SnakeBodyManager bodyManager,
            SnakeHealth health
        )
        {
            Transform hudCanvas = uiRoot.Find("HUDCanvas");

            if (hudCanvas == null)
            {
                Debug.LogWarning("[Project Epsilon] HUDCanvas가 없어 Day 6 HUD 연결을 건너뜁니다.");
                return;
            }

            HUDController hudController = hudCanvas.GetComponent<HUDController>();

            if (hudController == null)
            {
                Debug.LogWarning("[Project Epsilon] HUDController가 없어 Day 6 HUD 연결을 건너뜁니다.");
                return;
            }

            SnakeBodyHUDPresenter bodyPresenter =
                hudCanvas.GetComponent<SnakeBodyHUDPresenter>();

            if (bodyPresenter == null)
            {
                bodyPresenter = hudCanvas.gameObject.AddComponent<SnakeBodyHUDPresenter>();
            }

            bodyPresenter.Bind(bodyManager, hudController);

            SnakeHealthHUDPresenter healthPresenter =
                hudCanvas.GetComponent<SnakeHealthHUDPresenter>();

            if (healthPresenter == null)
            {
                healthPresenter = hudCanvas.gameObject.AddComponent<SnakeHealthHUDPresenter>();
            }

            healthPresenter.Bind(health, hudController);
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
