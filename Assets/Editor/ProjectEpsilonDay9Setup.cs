using System.IO;
using ProjectEpsilon.Combat;
using ProjectEpsilon.Data;
using ProjectEpsilon.Player;
using ProjectEpsilon.Progression;
using ProjectEpsilon.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectEpsilon.Editor
{
    [InitializeOnLoad]
    public static class ProjectEpsilonDay9Setup
    {
        private const string GameScenePath =
            "Assets/Scenes/Game.unity";

        private const string DebugSpritePath =
            "Assets/Art/Sprites/DebugSnakeBody.png";

        private const string DebugBladePath =
            "Assets/Data/Weapons/DebugBlade.asset";

        private const string DebugBlasterPath =
            "Assets/Data/Weapons/DebugBlaster.asset";

        private const string DebugPulsePath =
            "Assets/Data/Weapons/DebugPulse.asset";

        private const string SessionKey =
            "ProjectEpsilon.Day9.AutoSetup";

        private static readonly string[] LegacyAssets =
        {
            "Assets/Editor/ProjectEpsilonDay8Setup.cs",
            "Assets/Editor/ProjectEpsilonDay8Setup.cs.meta"
        };

        static ProjectEpsilonDay9Setup()
        {
            EditorApplication.delayCall += RunAutoSetup;
        }

        [MenuItem("Project Epsilon/Day 9/Run Setup")]
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
                    "[Project Epsilon] Game Scene이 없어 Day 9 자동 구성을 건너뜁니다."
                );
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(
                GameScenePath,
                OpenSceneMode.Single
            );

            if (!force && IsDay9Configured())
            {
                CleanupLegacyAssets();
                return;
            }

            GameObject gameplayRoot =
                EnsureRoot("===Gameplay===");

            GameObject environmentRoot =
                EnsureRoot("===Environment===");

            GameObject uiRoot =
                EnsureRoot("===UI===");

            Transform playerTransform =
                gameplayRoot.transform.Find("Player");

            SnakeBodyManager bodyManager =
                FindBodyManager(gameplayRoot.transform);

            if (playerTransform == null || bodyManager == null)
            {
                Debug.LogError(
                    "[Project Epsilon] Player 또는 SnakeBodyManager를 찾지 못해 Day 9 구성을 중단합니다."
                );
                return;
            }

            Sprite debugSprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(
                    DebugSpritePath
                );

            WeaponData debugBlade = EnsureWeapon(
                DebugBladePath,
                "debug_blade",
                "Debug Blade",
                WeaponAttackType.Melee,
                14f,
                0.75f,
                1.35f,
                8f,
                3f
            );

            WeaponData debugBlaster = EnsureWeapon(
                DebugBlasterPath,
                "debug_blaster",
                "Debug Blaster",
                WeaponAttackType.StraightProjectile,
                10f,
                0.9f,
                6f,
                8f,
                3f
            );

            WeaponData debugPulse = EnsureWeapon(
                DebugPulsePath,
                "debug_pulse",
                "Debug Pulse",
                WeaponAttackType.Area,
                8f,
                1.4f,
                1.8f,
                8f,
                3f
            );

            bodyManager.ResetBody();

            SnakeWeaponManager weaponManager =
                EnsureWeaponManager(
                    bodyManager,
                    debugBlaster,
                    debugSprite
                );

            weaponManager.SynchronizeSlots();
            weaponManager.TryEquipAt(0, debugBlade);
            weaponManager.TryEquipAt(1, debugBlaster);
            weaponManager.TryEquipAt(2, debugPulse);

            SnakeExperience experience =
                EnsureExperience(playerTransform.gameObject);

            EnsureExperienceHUD(
                uiRoot.transform,
                experience
            );

            RemoveOldTestTargets(
                environmentRoot.transform
            );

            EnsureTestTargets(
                environmentRoot.transform,
                debugSprite
            );

            EditorUtility.SetDirty(bodyManager);
            EditorUtility.SetDirty(weaponManager);
            EditorUtility.SetDirty(experience);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(
                scene,
                GameScenePath
            );

            CleanupLegacyAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                weaponManager.gameObject;

            Debug.Log(
                "[Project Epsilon] Day 9 attack types + XP pickup setup complete."
            );
        }

        private static bool IsDay9Configured()
        {
            GameObject gameplayRoot =
                GameObject.Find("===Gameplay===");

            GameObject environmentRoot =
                GameObject.Find("===Environment===");

            if (gameplayRoot == null ||
                environmentRoot == null)
            {
                return false;
            }

            Transform player =
                gameplayRoot.transform.Find("Player");

            SnakeBodyManager bodyManager =
                FindBodyManager(gameplayRoot.transform);

            if (player == null || bodyManager == null)
            {
                return false;
            }

            Transform targets =
                environmentRoot.transform.Find(
                    "Day09_TestTargets"
                );

            return bodyManager.GetComponent<SnakeWeaponManager>() != null &&
                player.GetComponent<SnakeExperience>() != null &&
                targets != null &&
                AssetDatabase.LoadAssetAtPath<WeaponData>(
                    DebugBladePath
                ) != null &&
                AssetDatabase.LoadAssetAtPath<WeaponData>(
                    DebugBlasterPath
                ) != null &&
                AssetDatabase.LoadAssetAtPath<WeaponData>(
                    DebugPulsePath
                ) != null;
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

        private static SnakeBodyManager FindBodyManager(
            Transform gameplayRoot
        )
        {
            Transform bodyRoot = gameplayRoot.Find("SnakeBody");

            if (bodyRoot == null)
            {
                return null;
            }

            return bodyRoot.GetComponent<SnakeBodyManager>();
        }

        private static SnakeWeaponManager EnsureWeaponManager(
            SnakeBodyManager bodyManager,
            WeaponData startingWeapon,
            Sprite debugSprite
        )
        {
            SnakeWeaponManager manager =
                bodyManager.GetComponent<SnakeWeaponManager>();

            if (manager == null)
            {
                manager =
                    bodyManager.gameObject.AddComponent<SnakeWeaponManager>();
            }

            manager.Configure(
                bodyManager,
                startingWeapon,
                debugSprite
            );

            return manager;
        }

        private static SnakeExperience EnsureExperience(
            GameObject player
        )
        {
            SnakeExperience experience =
                player.GetComponent<SnakeExperience>();

            if (experience == null)
            {
                experience = player.AddComponent<SnakeExperience>();
            }

            experience.Configure(10);
            experience.ResetExperience();

            return experience;
        }

        private static void EnsureExperienceHUD(
            Transform uiRoot,
            SnakeExperience experience
        )
        {
            Transform hudCanvas = uiRoot.Find("HUDCanvas");

            if (hudCanvas == null)
            {
                Debug.LogWarning(
                    "[Project Epsilon] HUDCanvas가 없어 XP HUD 연결을 건너뜁니다."
                );
                return;
            }

            HUDController hudController =
                hudCanvas.GetComponent<HUDController>();

            if (hudController == null)
            {
                Debug.LogWarning(
                    "[Project Epsilon] HUDController가 없어 XP HUD 연결을 건너뜁니다."
                );
                return;
            }

            SnakeExperienceHUDPresenter presenter =
                hudCanvas.GetComponent<SnakeExperienceHUDPresenter>();

            if (presenter == null)
            {
                presenter =
                    hudCanvas.gameObject.AddComponent<SnakeExperienceHUDPresenter>();
            }

            presenter.Bind(experience, hudController);
            EditorUtility.SetDirty(presenter);
        }

        private static WeaponData EnsureWeapon(
            string assetPath,
            string id,
            string displayName,
            WeaponAttackType attackType,
            float damage,
            float interval,
            float range,
            float projectileSpeed,
            float projectileLifetime
        )
        {
            EnsureFolder("Assets", "Data");
            EnsureFolder("Assets/Data", "Weapons");

            WeaponData weapon =
                AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);

            if (weapon == null)
            {
                weapon = ScriptableObject.CreateInstance<WeaponData>();
                AssetDatabase.CreateAsset(weapon, assetPath);
            }

            SerializedObject serializedWeapon =
                new SerializedObject(weapon);

            serializedWeapon.FindProperty("id").stringValue = id;
            serializedWeapon.FindProperty("displayName").stringValue =
                displayName;

            serializedWeapon.FindProperty("attribute").enumValueIndex =
                (int)WeaponAttribute.Physical;

            serializedWeapon.FindProperty("attackType").enumValueIndex =
                (int)attackType;

            serializedWeapon.FindProperty("baseDamage").floatValue =
                damage;

            serializedWeapon.FindProperty("attackInterval").floatValue =
                interval;

            serializedWeapon.FindProperty("range").floatValue =
                range;

            serializedWeapon.FindProperty("maxGrade").intValue = 5;

            serializedWeapon.FindProperty("projectileSpeed").floatValue =
                projectileSpeed;

            serializedWeapon.FindProperty("projectileLifetime").floatValue =
                projectileLifetime;

            serializedWeapon.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(weapon);

            return weapon;
        }

        private static void EnsureFolder(
            string parent,
            string child
        )
        {
            string fullPath = $"{parent}/{child}";

            if (AssetDatabase.IsValidFolder(fullPath))
            {
                return;
            }

            AssetDatabase.CreateFolder(parent, child);
        }

        private static void RemoveOldTestTargets(
            Transform environmentRoot
        )
        {
            Transform day8Targets =
                environmentRoot.Find("Day08_TestTargets");

            if (day8Targets != null)
            {
                Object.DestroyImmediate(day8Targets.gameObject);
            }

            Transform day9Targets =
                environmentRoot.Find("Day09_TestTargets");

            if (day9Targets != null)
            {
                Object.DestroyImmediate(day9Targets.gameObject);
            }
        }

        private static void EnsureTestTargets(
            Transform environmentRoot,
            Sprite sprite
        )
        {
            GameObject targetRoot =
                new GameObject("Day09_TestTargets");

            targetRoot.transform.SetParent(environmentRoot, false);

            EnsureTarget(
                targetRoot.transform,
                "Target_XP01_A",
                new Vector3(-2.4f, 3.4f, 0f),
                30f,
                1,
                sprite
            );

            EnsureTarget(
                targetRoot.transform,
                "Target_XP01_B",
                new Vector3(1.8f, 4.1f, 0f),
                30f,
                1,
                sprite
            );

            EnsureTarget(
                targetRoot.transform,
                "Target_XP05",
                new Vector3(0f, 5.1f, 0f),
                42f,
                5,
                sprite
            );

            EnsureTarget(
                targetRoot.transform,
                "Target_XP20",
                new Vector3(2.6f, 6.2f, 0f),
                58f,
                20,
                sprite
            );

            EnsureTarget(
                targetRoot.transform,
                "Target_Close_Left",
                new Vector3(-1.25f, 6.8f, 0f),
                36f,
                1,
                sprite
            );

            EnsureTarget(
                targetRoot.transform,
                "Target_Close_Right",
                new Vector3(1.1f, 7.4f, 0f),
                36f,
                1,
                sprite
            );
        }

        private static void EnsureTarget(
            Transform parent,
            string targetName,
            Vector3 position,
            float health,
            int experienceValue,
            Sprite sprite
        )
        {
            GameObject target = new GameObject(targetName);

            target.transform.SetParent(parent, false);
            target.transform.localPosition = position;
            target.transform.localScale =
                new Vector3(0.82f, 0.82f, 1f);

            SpriteRenderer renderer =
                target.AddComponent<SpriteRenderer>();

            renderer.sprite = sprite;
            renderer.color = ResolveTargetColor(experienceValue);
            renderer.sortingOrder = 4;

            CircleCollider2D collider =
                target.AddComponent<CircleCollider2D>();

            collider.isTrigger = true;
            collider.radius = 0.45f;

            WeaponTarget weaponTarget =
                target.AddComponent<WeaponTarget>();

            weaponTarget.Configure(health);

            ExperienceDropper dropper =
                target.AddComponent<ExperienceDropper>();

            dropper.Configure(experienceValue, sprite);
        }

        private static Color ResolveTargetColor(int experienceValue)
        {
            if (experienceValue >= 20)
            {
                return new Color(1f, 0.35f, 0.75f, 1f);
            }

            if (experienceValue >= 5)
            {
                return new Color(0.45f, 0.8f, 1f, 1f);
            }

            return new Color(0.3f, 1f, 0.55f, 1f);
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
