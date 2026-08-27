using System.IO;
using ProjectEpsilon.Combat;
using ProjectEpsilon.Data;
using ProjectEpsilon.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectEpsilon.Editor
{
    [InitializeOnLoad]
    public static class ProjectEpsilonDay8Setup
    {
        private const string GameScenePath =
            "Assets/Scenes/Game.unity";

        private const string BodySpritePath =
            "Assets/Art/Sprites/DebugSnakeBody.png";

        private const string DebugWeaponPath =
            "Assets/Data/Weapons/DebugBlaster.asset";

        private const string SessionKey =
            "ProjectEpsilon.Day8.AutoSetup";

        private static readonly string[] LegacyAssets =
        {
            "Assets/Editor/ProjectEpsilonDay7Setup.cs",
            "Assets/Editor/ProjectEpsilonDay7Setup.cs.meta"
        };

        static ProjectEpsilonDay8Setup()
        {
            EditorApplication.delayCall += RunAutoSetup;
        }

        [MenuItem("Project Epsilon/Day 8/Run Setup")]
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
                    "[Project Epsilon] Game Scene이 없어 Day 8 자동 구성을 건너뜁니다."
                );
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(
                GameScenePath,
                OpenSceneMode.Single
            );

            if (!force && IsDay8Configured())
            {
                CleanupLegacyAssets();
                return;
            }

            GameObject gameplayRoot =
                EnsureRoot("===Gameplay===");

            GameObject environmentRoot =
                EnsureRoot("===Environment===");

            SnakeBodyManager bodyManager =
                FindBodyManager(gameplayRoot.transform);

            if (bodyManager == null)
            {
                Debug.LogError(
                    "[Project Epsilon] SnakeBodyManager를 찾지 못해 Day 8 구성을 중단합니다."
                );
                return;
            }

            WeaponData debugWeapon =
                EnsureDebugWeapon();

            Sprite debugSprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(
                    BodySpritePath
                );

            SnakeWeaponManager weaponManager =
                EnsureWeaponManager(
                    bodyManager,
                    debugWeapon,
                    debugSprite
                );

            RemoveDay7TestHazards(
                environmentRoot.transform
            );

            EnsureTestTargets(
                environmentRoot.transform,
                debugSprite
            );

            EditorUtility.SetDirty(weaponManager);
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
                "[Project Epsilon] Day 8 body weapon slot + auto attack setup complete."
            );
        }

        private static bool IsDay8Configured()
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

            SnakeBodyManager bodyManager =
                FindBodyManager(gameplayRoot.transform);

            if (bodyManager == null)
            {
                return false;
            }

            SnakeWeaponManager weaponManager =
                bodyManager.GetComponent<SnakeWeaponManager>();

            Transform targets =
                environmentRoot.transform.Find(
                    "Day08_TestTargets"
                );

            WeaponData debugWeapon =
                AssetDatabase.LoadAssetAtPath<WeaponData>(
                    DebugWeaponPath
                );

            return weaponManager != null &&
                targets != null &&
                debugWeapon != null;
        }

        private static GameObject EnsureRoot(
            string rootName
        )
        {
            GameObject existing =
                GameObject.Find(rootName);

            if (existing != null &&
                existing.transform.parent == null)
            {
                return existing;
            }

            GameObject root =
                new GameObject(rootName);

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
            Transform bodyRoot =
                gameplayRoot.Find("SnakeBody");

            if (bodyRoot == null)
            {
                return null;
            }

            return bodyRoot.GetComponent<SnakeBodyManager>();
        }

        private static SnakeWeaponManager EnsureWeaponManager(
            SnakeBodyManager bodyManager,
            WeaponData startingWeapon,
            Sprite projectileSprite
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
                projectileSprite
            );

            return manager;
        }

        private static WeaponData EnsureDebugWeapon()
        {
            EnsureFolder("Assets", "Data");
            EnsureFolder("Assets/Data", "Weapons");

            WeaponData weapon =
                AssetDatabase.LoadAssetAtPath<WeaponData>(
                    DebugWeaponPath
                );

            if (weapon == null)
            {
                weapon =
                    ScriptableObject.CreateInstance<WeaponData>();

                AssetDatabase.CreateAsset(
                    weapon,
                    DebugWeaponPath
                );
            }

            SerializedObject serializedWeapon =
                new SerializedObject(weapon);

            serializedWeapon.FindProperty("id").stringValue =
                "debug_blaster";

            serializedWeapon.FindProperty("displayName").stringValue =
                "Debug Blaster";

            serializedWeapon.FindProperty("attribute").enumValueIndex =
                (int)WeaponAttribute.Physical;

            serializedWeapon.FindProperty("attackType").enumValueIndex =
                (int)WeaponAttackType.StraightProjectile;

            serializedWeapon.FindProperty("baseDamage").floatValue =
                10f;

            serializedWeapon.FindProperty("attackInterval").floatValue =
                1f;

            serializedWeapon.FindProperty("range").floatValue =
                6f;

            serializedWeapon.FindProperty("maxGrade").intValue =
                5;

            serializedWeapon.FindProperty("projectileSpeed").floatValue =
                8f;

            serializedWeapon.FindProperty("projectileLifetime").floatValue =
                3f;

            serializedWeapon.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(weapon);

            return weapon;
        }

        private static void EnsureFolder(
            string parent,
            string child
        )
        {
            string fullPath =
                $"{parent}/{child}";

            if (AssetDatabase.IsValidFolder(fullPath))
            {
                return;
            }

            AssetDatabase.CreateFolder(
                parent,
                child
            );
        }

        private static void RemoveDay7TestHazards(
            Transform environmentRoot
        )
        {
            Transform oldRoot =
                environmentRoot.Find(
                    "Day07_TestHazards"
                );

            if (oldRoot != null)
            {
                Object.DestroyImmediate(
                    oldRoot.gameObject
                );
            }
        }

        private static void EnsureTestTargets(
            Transform environmentRoot,
            Sprite sprite
        )
        {
            Transform existing =
                environmentRoot.Find(
                    "Day08_TestTargets"
                );

            GameObject targetRoot =
                existing == null
                    ? new GameObject(
                        "Day08_TestTargets"
                    )
                    : existing.gameObject;

            targetRoot.transform.SetParent(
                environmentRoot,
                false
            );

            EnsureTarget(
                targetRoot.transform,
                "Target_01",
                new Vector3(-2.2f, 4.2f, 0f),
                new Color(0.25f, 1f, 0.55f, 1f),
                sprite
            );

            EnsureTarget(
                targetRoot.transform,
                "Target_02",
                new Vector3(0f, 5.2f, 0f),
                new Color(0.35f, 0.8f, 1f, 1f),
                sprite
            );

            EnsureTarget(
                targetRoot.transform,
                "Target_03",
                new Vector3(2.2f, 4.2f, 0f),
                new Color(1f, 0.45f, 0.75f, 1f),
                sprite
            );
        }

        private static void EnsureTarget(
            Transform parent,
            string targetName,
            Vector3 position,
            Color color,
            Sprite sprite
        )
        {
            Transform existing =
                parent.Find(targetName);

            GameObject target =
                existing == null
                    ? new GameObject(targetName)
                    : existing.gameObject;

            target.transform.SetParent(parent, false);
            target.transform.localPosition = position;
            target.transform.localScale =
                new Vector3(0.85f, 0.85f, 1f);

            SpriteRenderer renderer =
                target.GetComponent<SpriteRenderer>();

            if (renderer == null)
            {
                renderer =
                    target.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = 4;

            CircleCollider2D collider =
                target.GetComponent<CircleCollider2D>();

            if (collider == null)
            {
                collider =
                    target.AddComponent<CircleCollider2D>();
            }

            collider.isTrigger = true;
            collider.radius = 0.45f;

            WeaponTarget weaponTarget =
                target.GetComponent<WeaponTarget>();

            if (weaponTarget == null)
            {
                weaponTarget =
                    target.AddComponent<WeaponTarget>();
            }

            weaponTarget.Configure(30f);
        }

        private static void CleanupLegacyAssets()
        {
            foreach (string assetPath in LegacyAssets)
            {
                if (AssetDatabase.LoadAssetAtPath<Object>(
                    assetPath
                ) != null)
                {
                    AssetDatabase.DeleteAsset(
                        assetPath
                    );
                }
            }
        }
    }
}
