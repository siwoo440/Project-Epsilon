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
    public static class ProjectEpsilonDay7Setup
    {
        private const string GameScenePath = "Assets/Scenes/Game.unity";
        private const string BodySpritePath = "Assets/Art/Sprites/DebugSnakeBody.png";
        private const string SessionKey = "ProjectEpsilon.Day7.AutoSetup";

        private static readonly string[] LegacyAssets =
        {
            "Assets/Editor/ProjectEpsilonDay6Setup.cs",
            "Assets/Editor/ProjectEpsilonDay6Setup.cs.meta"
        };

        static ProjectEpsilonDay7Setup()
        {
            EditorApplication.delayCall += RunAutoSetup;
        }

        [MenuItem("Project Epsilon/Day 7/Run Setup")]
        public static void RunSetupFromMenu()
        {
            RunSetup(true);
        }

        private static void RunAutoSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling)
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
                Debug.LogWarning(
                    "[Project Epsilon] Game Scene이 없어 Day 7 자동 구성을 건너뜁니다."
                );
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(
                GameScenePath,
                OpenSceneMode.Single
            );

            if (!force && IsDay7Configured())
            {
                CleanupLegacyAssets();
                return;
            }

            GameObject gameplayRoot = EnsureRoot("===Gameplay===");
            GameObject environmentRoot = EnsureRoot("===Environment===");
            GameObject uiRoot = EnsureRoot("===UI===");

            GameObject player = EnsurePlayer(gameplayRoot.transform);
            SnakePathRecorder recorder = player.GetComponent<SnakePathRecorder>();
            recorder.ResetHistory();

            SnakeBodyManager bodyManager = EnsureSnakeBody(
                gameplayRoot.transform,
                recorder
            );

            SnakeInvulnerability invulnerability =
                EnsureInvulnerability(player);

            SnakeHealth health = EnsureHealth(
                player,
                bodyManager,
                invulnerability
            );

            EnsureSelfCollision(
                player,
                bodyManager,
                invulnerability
            );

            SnakeStamina stamina = EnsureStamina(player);
            EnsureExternalCollision(
                player,
                bodyManager,
                health,
                invulnerability
            );

            EnsureHUDBinding(
                uiRoot.transform,
                bodyManager,
                health,
                stamina
            );

            EnsureTestHazards(environmentRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameScenePath);

            CleanupLegacyAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = player;

            Debug.Log(
                "[Project Epsilon] Day 7 collision + boost/stamina setup complete."
            );
        }

        private static bool IsDay7Configured()
        {
            GameObject gameplayRoot = GameObject.Find("===Gameplay===");
            GameObject environmentRoot = GameObject.Find("===Environment===");
            GameObject uiRoot = GameObject.Find("===UI===");

            if (gameplayRoot == null ||
                environmentRoot == null ||
                uiRoot == null)
            {
                return false;
            }

            Transform player = gameplayRoot.transform.Find("Player");
            Transform bodyRoot = gameplayRoot.transform.Find("SnakeBody");
            Transform testHazards = environmentRoot.transform.Find("Day07_TestHazards");
            Transform hudCanvas = uiRoot.transform.Find("HUDCanvas");

            if (player == null ||
                bodyRoot == null ||
                testHazards == null ||
                hudCanvas == null)
            {
                return false;
            }

            return player.GetComponent<SnakeStamina>() != null &&
                player.GetComponent<SnakeExternalCollision>() != null &&
                player.GetComponent<SnakeMovement>() != null &&
                bodyRoot.GetComponent<SnakeBodyManager>() != null &&
                hudCanvas.GetComponent<SnakeStaminaHUDPresenter>() != null;
        }

        private static GameObject EnsureRoot(string rootName)
        {
            GameObject existing = GameObject.Find(rootName);

            if (existing != null && existing.transform.parent == null)
            {
                return existing;
            }

            GameObject root = new GameObject(rootName);
            root.transform.SetPositionAndRotation(
                Vector3.zero,
                Quaternion.identity
            );
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

            CircleCollider2D headCollider =
                player.GetComponent<CircleCollider2D>();

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

            SnakeBodyFollower follower =
                bodyRoot.GetComponent<SnakeBodyFollower>();

            if (follower == null)
            {
                follower = bodyRoot.AddComponent<SnakeBodyFollower>();
            }

            SnakeBodyManager manager =
                bodyRoot.GetComponent<SnakeBodyManager>();

            if (manager == null)
            {
                manager = bodyRoot.AddComponent<SnakeBodyManager>();
            }

            if (bodyRoot.GetComponent<SnakeBodyDebugControls>() == null)
            {
                bodyRoot.AddComponent<SnakeBodyDebugControls>();
            }

            Sprite bodySprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                BodySpritePath
            );

            manager.Configure(
                recorder,
                follower,
                bodySprite,
                3,
                20,
                0.58f
            );

            manager.ResetBody();

            return manager;
        }

        private static SnakeInvulnerability EnsureInvulnerability(
            GameObject player
        )
        {
            SnakeInvulnerability invulnerability =
                player.GetComponent<SnakeInvulnerability>();

            if (invulnerability == null)
            {
                invulnerability =
                    player.AddComponent<SnakeInvulnerability>();
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

            health.Configure(
                bodyManager,
                invulnerability,
                100
            );

            return health;
        }

        private static void EnsureSelfCollision(
            GameObject player,
            SnakeBodyManager bodyManager,
            SnakeInvulnerability invulnerability
        )
        {
            SnakeSelfCollision selfCollision =
                player.GetComponent<SnakeSelfCollision>();

            if (selfCollision == null)
            {
                selfCollision =
                    player.AddComponent<SnakeSelfCollision>();
            }

            selfCollision.Bind(
                bodyManager,
                invulnerability,
                2,
                2f
            );
        }

        private static SnakeStamina EnsureStamina(GameObject player)
        {
            PlayerInputReader inputReader =
                player.GetComponent<PlayerInputReader>();

            SnakeStamina stamina =
                player.GetComponent<SnakeStamina>();

            if (stamina == null)
            {
                stamina = player.AddComponent<SnakeStamina>();
            }

            stamina.Configure(
                inputReader,
                100f,
                1.5f,
                25f,
                20f,
                1f
            );

            SnakeMovement movement =
                player.GetComponent<SnakeMovement>();

            movement?.BindStamina(stamina);

            return stamina;
        }

        private static void EnsureExternalCollision(
            GameObject player,
            SnakeBodyManager bodyManager,
            SnakeHealth health,
            SnakeInvulnerability invulnerability
        )
        {
            SnakeExternalCollision collision =
                player.GetComponent<SnakeExternalCollision>();

            if (collision == null)
            {
                collision =
                    player.AddComponent<SnakeExternalCollision>();
            }

            collision.Bind(
                bodyManager,
                health,
                invulnerability,
                1,
                1f,
                25,
                1.5f
            );
        }

        private static void EnsureHUDBinding(
            Transform uiRoot,
            SnakeBodyManager bodyManager,
            SnakeHealth health,
            SnakeStamina stamina
        )
        {
            Transform hudCanvas = uiRoot.Find("HUDCanvas");

            if (hudCanvas == null)
            {
                Debug.LogWarning(
                    "[Project Epsilon] HUDCanvas가 없어 Day 7 HUD 연결을 건너뜁니다."
                );
                return;
            }

            HUDController hudController =
                hudCanvas.GetComponent<HUDController>();

            if (hudController == null)
            {
                Debug.LogWarning(
                    "[Project Epsilon] HUDController가 없어 Day 7 HUD 연결을 건너뜁니다."
                );
                return;
            }

            SnakeBodyHUDPresenter bodyPresenter =
                hudCanvas.GetComponent<SnakeBodyHUDPresenter>();

            if (bodyPresenter == null)
            {
                bodyPresenter =
                    hudCanvas.gameObject.AddComponent<SnakeBodyHUDPresenter>();
            }

            bodyPresenter.Bind(bodyManager, hudController);

            SnakeHealthHUDPresenter healthPresenter =
                hudCanvas.GetComponent<SnakeHealthHUDPresenter>();

            if (healthPresenter == null)
            {
                healthPresenter =
                    hudCanvas.gameObject.AddComponent<SnakeHealthHUDPresenter>();
            }

            healthPresenter.Bind(health, hudController);

            SnakeStaminaHUDPresenter staminaPresenter =
                hudCanvas.GetComponent<SnakeStaminaHUDPresenter>();

            if (staminaPresenter == null)
            {
                staminaPresenter =
                    hudCanvas.gameObject.AddComponent<SnakeStaminaHUDPresenter>();
            }

            staminaPresenter.Bind(stamina, hudController);
        }

        private static void EnsureTestHazards(Transform environmentRoot)
        {
            Transform existing = environmentRoot.Find("Day07_TestHazards");
            GameObject testRoot;

            if (existing != null)
            {
                testRoot = existing.gameObject;
            }
            else
            {
                testRoot = new GameObject("Day07_TestHazards");
                testRoot.transform.SetParent(environmentRoot, false);
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                BodySpritePath
            );

            EnsureTestEnemy(testRoot.transform, sprite);
            EnsureTestObstacle(testRoot.transform, sprite);
        }

        private static void EnsureTestEnemy(
            Transform parent,
            Sprite sprite
        )
        {
            Transform existing = parent.Find("TestEnemy");
            GameObject target = existing == null
                ? new GameObject("TestEnemy")
                : existing.gameObject;

            target.transform.SetParent(parent, false);
            target.transform.localPosition = new Vector3(-2.1f, 6f, 0f);
            target.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

            SpriteRenderer renderer =
                target.GetComponent<SpriteRenderer>();

            if (renderer == null)
            {
                renderer = target.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = sprite;
            renderer.color = new Color(1f, 0.3f, 0.3f, 1f);
            renderer.sortingOrder = 3;

            CircleCollider2D collider =
                target.GetComponent<CircleCollider2D>();

            if (collider == null)
            {
                collider = target.AddComponent<CircleCollider2D>();
            }

            collider.isTrigger = true;
            collider.radius = 0.48f;

            SnakeContactHazard hazard =
                target.GetComponent<SnakeContactHazard>();

            if (hazard == null)
            {
                hazard = target.AddComponent<SnakeContactHazard>();
            }

            hazard.Configure(SnakeContactHazardType.EnemyDirect);
        }

        private static void EnsureTestObstacle(
            Transform parent,
            Sprite sprite
        )
        {
            Transform existing = parent.Find("TestObstacle");
            GameObject target = existing == null
                ? new GameObject("TestObstacle")
                : existing.gameObject;

            target.transform.SetParent(parent, false);
            target.transform.localPosition = new Vector3(2.1f, 6f, 0f);
            target.transform.localScale = new Vector3(1.35f, 1.35f, 1f);

            SpriteRenderer renderer =
                target.GetComponent<SpriteRenderer>();

            if (renderer == null)
            {
                renderer = target.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = sprite;
            renderer.color = new Color(1f, 0.72f, 0.2f, 1f);
            renderer.sortingOrder = 3;

            BoxCollider2D collider =
                target.GetComponent<BoxCollider2D>();

            if (collider == null)
            {
                collider = target.AddComponent<BoxCollider2D>();
            }

            collider.isTrigger = true;
            collider.size = new Vector2(0.85f, 0.85f);

            SnakeContactHazard hazard =
                target.GetComponent<SnakeContactHazard>();

            if (hazard == null)
            {
                hazard = target.AddComponent<SnakeContactHazard>();
            }

            hazard.Configure(SnakeContactHazardType.Obstacle);
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
