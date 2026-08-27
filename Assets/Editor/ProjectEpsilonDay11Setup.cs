using System.Collections.Generic;
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
using UnityEngine.UI;

namespace ProjectEpsilon.Editor
{
    [InitializeOnLoad]
    public static class ProjectEpsilonDay11Setup
    {
        private const string GameScenePath =
            "Assets/Scenes/Game.unity";

        private const string RewardPoolPath =
            "Assets/Data/Progression/DebugWeaponRewardPool.asset";

        private const string SessionKey =
            "ProjectEpsilon.Day11.AutoSetup";

        private static readonly string[] LegacyAssets =
        {
            "Assets/Editor/ProjectEpsilonDay10Setup.cs",
            "Assets/Editor/ProjectEpsilonDay10Setup.cs.meta"
        };

        private static readonly string[] WeaponPaths =
        {
            "Assets/Data/Weapons/DebugBlade.asset",
            "Assets/Data/Weapons/DebugBlaster.asset",
            "Assets/Data/Weapons/DebugPulse.asset",
            "Assets/Data/Weapons/DebugCutter.asset",
            "Assets/Data/Weapons/DebugNeedle.asset",
            "Assets/Data/Weapons/DebugNova.asset"
        };

        static ProjectEpsilonDay11Setup()
        {
            EditorApplication.delayCall +=
                RunAutoSetup;
        }

        [MenuItem("Project Epsilon/Day 11/Run Setup")]
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

            if (SessionState.GetBool(
                SessionKey,
                false
            ))
            {
                return;
            }

            SessionState.SetBool(
                SessionKey,
                true
            );

            RunSetup(false);
        }

        private static void RunSetup(bool force)
        {
            if (!File.Exists(GameScenePath))
            {
                Debug.LogWarning(
                    "[Project Epsilon] Game Scene이 없어 Day 11 자동 구성을 건너뜁니다."
                );
                return;
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    GameScenePath,
                    OpenSceneMode.Single
                );

            if (!force &&
                IsDay11Configured())
            {
                CleanupLegacyAssets();
                return;
            }

            GameObject gameplayRoot =
                GameObject.Find("===Gameplay===");

            GameObject uiRoot =
                GameObject.Find("===UI===");

            if (gameplayRoot == null ||
                uiRoot == null)
            {
                Debug.LogError(
                    "[Project Epsilon] Gameplay 또는 UI Root를 찾지 못해 Day 11 구성을 중단합니다."
                );
                return;
            }

            Transform playerTransform =
                gameplayRoot.transform.Find("Player");

            Transform bodyRoot =
                gameplayRoot.transform.Find("SnakeBody");

            Transform hudCanvas =
                uiRoot.transform.Find("HUDCanvas");

            if (playerTransform == null ||
                bodyRoot == null ||
                hudCanvas == null)
            {
                Debug.LogError(
                    "[Project Epsilon] Player, SnakeBody 또는 HUDCanvas를 찾지 못해 Day 11 구성을 중단합니다."
                );
                return;
            }

            SnakeBodyManager bodyManager =
                bodyRoot.GetComponent<SnakeBodyManager>();

            SnakeWeaponManager weaponManager =
                bodyRoot.GetComponent<SnakeWeaponManager>();

            SnakeHealth health =
                playerTransform.GetComponent<SnakeHealth>();

            SnakeExperience experience =
                playerTransform.GetComponent<SnakeExperience>();

            SnakeLevelUpController levelUpController =
                playerTransform.GetComponent<SnakeLevelUpController>();

            LevelUpPanelController levelUpPanel =
                hudCanvas.GetComponentInChildren<LevelUpPanelController>(
                    true
                );

            if (bodyManager == null ||
                weaponManager == null ||
                health == null ||
                experience == null ||
                levelUpController == null ||
                levelUpPanel == null)
            {
                Debug.LogError(
                    "[Project Epsilon] Day10 핵심 컴포넌트를 찾지 못해 Day 11 구성을 중단합니다."
                );
                return;
            }

            List<WeaponData> weapons =
                EnsureDebugWeapons();

            WeaponRewardPool rewardPool =
                EnsureRewardPool(
                    weapons
                );

            ConfigureLevelUpPanel(
                levelUpPanel,
                hudCanvas
            );

            levelUpController.Configure(
                experience,
                bodyManager,
                health,
                weaponManager,
                rewardPool,
                levelUpPanel
            );

            EditorUtility.SetDirty(
                levelUpController
            );

            EditorUtility.SetDirty(
                levelUpPanel
            );

            EditorUtility.SetDirty(
                rewardPool
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene,
                GameScenePath
            );

            CleanupLegacyAssets();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                levelUpPanel.gameObject;

            Debug.Log(
                "[Project Epsilon] Day 11 weapon reward candidates setup complete."
            );
        }

        private static bool IsDay11Configured()
        {
            WeaponRewardPool pool =
                AssetDatabase.LoadAssetAtPath<WeaponRewardPool>(
                    RewardPoolPath
                );

            GameObject uiRoot =
                GameObject.Find("===UI===");

            if (pool == null ||
                uiRoot == null)
            {
                return false;
            }

            Transform hudCanvas =
                uiRoot.transform.Find("HUDCanvas");

            if (hudCanvas == null)
            {
                return false;
            }

            Transform panel =
                hudCanvas.Find("LevelUpPanel");

            return panel != null &&
                panel.Find("CandidateButton_01") != null &&
                panel.Find("CandidateButton_02") != null &&
                panel.Find("CandidateButton_03") != null;
        }

        private static List<WeaponData>
            EnsureDebugWeapons()
        {
            EnsureFolder(
                "Assets",
                "Data"
            );

            EnsureFolder(
                "Assets/Data",
                "Weapons"
            );

            List<WeaponData> weapons =
                new List<WeaponData>();

            weapons.Add(
                EnsureWeapon(
                    WeaponPaths[0],
                    "debug_blade",
                    "Debug Blade",
                    WeaponAttackType.Melee,
                    14f,
                    0.75f,
                    1.35f,
                    8f,
                    3f
                )
            );

            weapons.Add(
                EnsureWeapon(
                    WeaponPaths[1],
                    "debug_blaster",
                    "Debug Blaster",
                    WeaponAttackType.StraightProjectile,
                    10f,
                    0.9f,
                    6f,
                    8f,
                    3f
                )
            );

            weapons.Add(
                EnsureWeapon(
                    WeaponPaths[2],
                    "debug_pulse",
                    "Debug Pulse",
                    WeaponAttackType.Area,
                    8f,
                    1.4f,
                    1.8f,
                    8f,
                    3f
                )
            );

            weapons.Add(
                EnsureWeapon(
                    WeaponPaths[3],
                    "debug_cutter",
                    "Debug Cutter",
                    WeaponAttackType.Melee,
                    18f,
                    1.05f,
                    1.1f,
                    8f,
                    3f
                )
            );

            weapons.Add(
                EnsureWeapon(
                    WeaponPaths[4],
                    "debug_needle",
                    "Debug Needle",
                    WeaponAttackType.StraightProjectile,
                    7f,
                    0.48f,
                    7f,
                    11f,
                    2.5f
                )
            );

            weapons.Add(
                EnsureWeapon(
                    WeaponPaths[5],
                    "debug_nova",
                    "Debug Nova",
                    WeaponAttackType.Area,
                    12f,
                    1.9f,
                    2.25f,
                    8f,
                    3f
                )
            );

            return weapons;
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
            WeaponData weapon =
                AssetDatabase.LoadAssetAtPath<WeaponData>(
                    assetPath
                );

            if (weapon == null)
            {
                weapon =
                    ScriptableObject.CreateInstance<WeaponData>();

                AssetDatabase.CreateAsset(
                    weapon,
                    assetPath
                );
            }

            SerializedObject serializedWeapon =
                new SerializedObject(weapon);

            serializedWeapon.FindProperty("id").stringValue =
                id;

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

            serializedWeapon.FindProperty("maxGrade").intValue =
                5;

            serializedWeapon.FindProperty("projectileSpeed").floatValue =
                projectileSpeed;

            serializedWeapon.FindProperty("projectileLifetime").floatValue =
                projectileLifetime;

            serializedWeapon.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(
                weapon
            );

            return weapon;
        }

        private static WeaponRewardPool EnsureRewardPool(
            IReadOnlyList<WeaponData> weapons
        )
        {
            EnsureFolder(
                "Assets/Data",
                "Progression"
            );

            WeaponRewardPool pool =
                AssetDatabase.LoadAssetAtPath<WeaponRewardPool>(
                    RewardPoolPath
                );

            if (pool == null)
            {
                pool =
                    ScriptableObject.CreateInstance<WeaponRewardPool>();

                AssetDatabase.CreateAsset(
                    pool,
                    RewardPoolPath
                );
            }

            pool.Configure(weapons);
            EditorUtility.SetDirty(pool);

            return pool;
        }

        private static void ConfigureLevelUpPanel(
            LevelUpPanelController controller,
            Transform hudCanvas
        )
        {
            GameObject panelObject =
                controller.gameObject;

            RectTransform panelRect =
                panelObject.GetComponent<RectTransform>();

            panelRect.sizeDelta =
                new Vector2(
                    720f,
                    410f
                );

            Font font =
                ResolveUIFont(
                    hudCanvas
                );

            Text titleText =
                EnsureText(
                    panelObject.transform,
                    "Title",
                    "LEVEL UP!",
                    font,
                    34,
                    new Vector2(0f, 148f),
                    new Vector2(620f, 44f)
                );

            Text levelText =
                EnsureText(
                    panelObject.transform,
                    "LevelText",
                    "Lv. 1",
                    font,
                    28,
                    new Vector2(0f, 103f),
                    new Vector2(580f, 38f)
                );

            Text growthText =
                EnsureText(
                    panelObject.transform,
                    "GrowthText",
                    "Choose 1 Weapon",
                    font,
                    20,
                    new Vector2(0f, 58f),
                    new Vector2(620f, 48f)
                );

            Transform oldContinue =
                panelObject.transform.Find(
                    "ContinueButton"
                );

            if (oldContinue != null)
            {
                Object.DestroyImmediate(
                    oldContinue.gameObject
                );
            }

            Button button01 =
                EnsureCandidateButton(
                    panelObject.transform,
                    "CandidateButton_01",
                    font,
                    new Vector2(-220f, -54f)
                );

            Button button02 =
                EnsureCandidateButton(
                    panelObject.transform,
                    "CandidateButton_02",
                    font,
                    new Vector2(0f, -54f)
                );

            Button button03 =
                EnsureCandidateButton(
                    panelObject.transform,
                    "CandidateButton_03",
                    font,
                    new Vector2(220f, -54f)
                );

            Text label01 =
                button01.transform.Find("Label")
                    .GetComponent<Text>();

            Text label02 =
                button02.transform.Find("Label")
                    .GetComponent<Text>();

            Text label03 =
                button03.transform.Find("Label")
                    .GetComponent<Text>();

            controller.Configure(
                titleText,
                levelText,
                growthText,
                button01,
                label01,
                button02,
                label02,
                button03,
                label03
            );

            controller.Hide();
        }

        private static Button EnsureCandidateButton(
            Transform parent,
            string objectName,
            Font font,
            Vector2 anchoredPosition
        )
        {
            Transform existing =
                parent.Find(objectName);

            GameObject buttonObject;

            if (existing == null)
            {
                buttonObject =
                    new GameObject(
                        objectName,
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image),
                        typeof(Button)
                    );

                buttonObject.transform.SetParent(
                    parent,
                    false
                );
            }
            else
            {
                buttonObject =
                    existing.gameObject;
            }

            RectTransform rect =
                buttonObject.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                anchoredPosition;

            rect.sizeDelta =
                new Vector2(
                    195f,
                    150f
                );

            Image image =
                buttonObject.GetComponent<Image>();

            image.color =
                new Color(
                    0.24f,
                    0.20f,
                    0.42f,
                    1f
                );

            Button button =
                buttonObject.GetComponent<Button>();

            button.targetGraphic =
                image;

            Text label =
                EnsureText(
                    buttonObject.transform,
                    "Label",
                    "Weapon Candidate",
                    font,
                    19,
                    Vector2.zero,
                    new Vector2(
                        175f,
                        135f
                    )
                );

            RectTransform labelRect =
                label.GetComponent<RectTransform>();

            labelRect.anchorMin =
                Vector2.zero;

            labelRect.anchorMax =
                Vector2.one;

            labelRect.offsetMin =
                new Vector2(
                    8f,
                    8f
                );

            labelRect.offsetMax =
                new Vector2(
                    -8f,
                    -8f
                );

            label.raycastTarget =
                false;

            return button;
        }

        private static Text EnsureText(
            Transform parent,
            string objectName,
            string textValue,
            Font font,
            int fontSize,
            Vector2 anchoredPosition,
            Vector2 size
        )
        {
            Transform existing =
                parent.Find(objectName);

            GameObject textObject;

            if (existing == null)
            {
                textObject =
                    new GameObject(
                        objectName,
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Text)
                    );

                textObject.transform.SetParent(
                    parent,
                    false
                );
            }
            else
            {
                textObject =
                    existing.gameObject;
            }

            RectTransform rect =
                textObject.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                anchoredPosition;

            rect.sizeDelta =
                size;

            Text text =
                textObject.GetComponent<Text>();

            text.font =
                font;

            text.text =
                textValue;

            text.fontSize =
                fontSize;

            text.alignment =
                TextAnchor.MiddleCenter;

            text.color =
                Color.white;

            text.raycastTarget =
                false;

            return text;
        }

        private static Font ResolveUIFont(
            Transform hudCanvas
        )
        {
            Text existingText =
                hudCanvas.GetComponentInChildren<Text>(
                    true
                );

            if (existingText != null &&
                existingText.font != null)
            {
                return existingText.font;
            }

            return Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );
        }

        private static void EnsureFolder(
            string parent,
            string child
        )
        {
            string fullPath =
                $"{parent}/{child}";

            if (AssetDatabase.IsValidFolder(
                fullPath
            ))
            {
                return;
            }

            AssetDatabase.CreateFolder(
                parent,
                child
            );
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
