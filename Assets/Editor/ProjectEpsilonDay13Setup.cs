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
    public static class ProjectEpsilonDay13Setup
    {
        private const string GameScenePath =
            "Assets/Scenes/Game.unity";

        private const string DebugBladePath =
            "Assets/Data/Weapons/DebugBlade.asset";

        private const string SessionKey =
            "ProjectEpsilon.Day13.AutoSetup";

        private const string LegacySetupPath =
            "Assets/Editor/ProjectEpsilonDay12Setup.cs";

        static ProjectEpsilonDay13Setup()
        {
            EditorApplication.delayCall +=
                RunAutoSetup;
        }

        [MenuItem("Project Epsilon/Day 13/Run Setup")]
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

        private static void RunSetup(
            bool force
        )
        {
            if (!File.Exists(
                GameScenePath
            ))
            {
                Debug.LogWarning(
                    "[Project Epsilon] Game Scene이 없어 Day 13 자동 구성을 건너뜁니다."
                );

                return;
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    GameScenePath,
                    OpenSceneMode.Single
                );

            if (!force &&
                IsDay13Configured())
            {
                CleanupLegacySetup();
                return;
            }

            GameObject gameplayRoot =
                GameObject.Find(
                    "===Gameplay==="
                );

            GameObject uiRoot =
                GameObject.Find(
                    "===UI==="
                );

            if (gameplayRoot == null ||
                uiRoot == null)
            {
                Debug.LogError(
                    "[Project Epsilon] Gameplay 또는 UI Root를 찾지 못해 Day 13 구성을 중단합니다."
                );

                return;
            }

            Transform player =
                gameplayRoot.transform.Find(
                    "Player"
                );

            Transform snakeBody =
                gameplayRoot.transform.Find(
                    "SnakeBody"
                );

            Transform hudCanvas =
                uiRoot.transform.Find(
                    "HUDCanvas"
                );

            if (player == null ||
                snakeBody == null ||
                hudCanvas == null)
            {
                Debug.LogError(
                    "[Project Epsilon] Player, SnakeBody 또는 HUDCanvas를 찾지 못해 Day 13 구성을 중단합니다."
                );

                return;
            }

            SnakeHealth health =
                player.GetComponent<SnakeHealth>();

            SnakeMovement movement =
                player.GetComponent<SnakeMovement>();

            SnakeStamina stamina =
                player.GetComponent<SnakeStamina>();

            SnakeExperience experience =
                player.GetComponent<SnakeExperience>();

            SnakeLevelUpController levelUpController =
                player.GetComponent<SnakeLevelUpController>();

            SnakeWeaponMergeController mergeController =
                player.GetComponent<SnakeWeaponMergeController>();

            SnakeBodyManager bodyManager =
                snakeBody.GetComponent<SnakeBodyManager>();

            SnakeWeaponManager weaponManager =
                snakeBody.GetComponent<SnakeWeaponManager>();

            WeaponMergePanelController mergePanel =
                hudCanvas.GetComponentInChildren<WeaponMergePanelController>(
                    true
                );

            if (health == null ||
                movement == null ||
                stamina == null ||
                experience == null ||
                levelUpController == null ||
                mergeController == null ||
                bodyManager == null ||
                weaponManager == null ||
                mergePanel == null)
            {
                Debug.LogError(
                    "[Project Epsilon] Day12 핵심 컴포넌트를 찾지 못해 Day 13 구성을 중단합니다."
                );

                return;
            }

            bodyManager.InitializeEarnedBodyCountFromCurrent();

            WeaponGradeEffectHooks gradeHooks =
                snakeBody.GetComponent<WeaponGradeEffectHooks>();

            if (gradeHooks == null)
            {
                gradeHooks =
                    snakeBody.gameObject.AddComponent<WeaponGradeEffectHooks>();
            }

            weaponManager.BindGradeEffectHooks(
                gradeHooks
            );

            mergeController.Configure(
                weaponManager,
                movement,
                stamina,
                mergePanel,
                experience,
                levelUpController,
                0.7f
            );

            Sprite defaultPickupSprite =
                ResolvePickupSprite(
                    player,
                    snakeBody
                );

            ConfigureEnemyDrops(
                defaultPickupSprite
            );

            WeaponData debugBlade =
                AssetDatabase.LoadAssetAtPath<WeaponData>(
                    DebugBladePath
                );

            SnakePrototypeDebugControls prototypeDebug =
                player.GetComponent<SnakePrototypeDebugControls>();

            if (prototypeDebug == null)
            {
                prototypeDebug =
                    player.gameObject.AddComponent<SnakePrototypeDebugControls>();
            }

            prototypeDebug.Configure(
                health,
                bodyManager,
                weaponManager,
                debugBlade,
                defaultPickupSprite
            );

            EnsurePrototypeHint(
                hudCanvas
            );

            EditorUtility.SetDirty(
                bodyManager
            );

            EditorUtility.SetDirty(
                weaponManager
            );

            EditorUtility.SetDirty(
                gradeHooks
            );

            EditorUtility.SetDirty(
                mergeController
            );

            EditorUtility.SetDirty(
                prototypeDebug
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene,
                GameScenePath
            );

            CleanupLegacySetup();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                player.gameObject;

            Debug.Log(
                "[Project Epsilon] Day 13 prototype recovery + grade hooks setup complete."
            );
        }

        private static bool IsDay13Configured()
        {
            GameObject gameplayRoot =
                GameObject.Find(
                    "===Gameplay==="
                );

            if (gameplayRoot == null)
            {
                return false;
            }

            Transform player =
                gameplayRoot.transform.Find(
                    "Player"
                );

            Transform snakeBody =
                gameplayRoot.transform.Find(
                    "SnakeBody"
                );

            if (player == null ||
                snakeBody == null)
            {
                return false;
            }

            EnemyDropController dropController =
                Object.FindFirstObjectByType<EnemyDropController>();

            return
                player.GetComponent<SnakePrototypeDebugControls>() != null &&
                snakeBody.GetComponent<WeaponGradeEffectHooks>() != null &&
                dropController != null;
        }

        private static void ConfigureEnemyDrops(
            Sprite defaultSprite
        )
        {
            WeaponTarget[] targets =
                Object.FindObjectsByType<WeaponTarget>(
                    FindObjectsSortMode.None
                );

            foreach (WeaponTarget target in targets)
            {
                if (target == null)
                {
                    continue;
                }

                int experienceValue = 1;
                Sprite pickupSprite =
                    defaultSprite;

                ExperienceDropper oldDropper =
                    target.GetComponent<ExperienceDropper>();

                if (oldDropper != null)
                {
                    SerializedObject serializedDropper =
                        new SerializedObject(
                            oldDropper
                        );

                    SerializedProperty experienceProperty =
                        serializedDropper.FindProperty(
                            "experienceValue"
                        );

                    SerializedProperty spriteProperty =
                        serializedDropper.FindProperty(
                            "pickupSprite"
                        );

                    if (experienceProperty != null)
                    {
                        experienceValue =
                            Mathf.Max(
                                1,
                                experienceProperty.intValue
                            );
                    }

                    if (spriteProperty != null &&
                        spriteProperty.objectReferenceValue is Sprite oldSprite)
                    {
                        pickupSprite =
                            oldSprite;
                    }

                    Object.DestroyImmediate(
                        oldDropper
                    );
                }

                EnemyDropController controller =
                    target.GetComponent<EnemyDropController>();

                if (controller == null)
                {
                    controller =
                        target.gameObject.AddComponent<EnemyDropController>();
                }

                float healChance =
                    experienceValue >= 20
                        ? 0.45f
                        : experienceValue >= 5
                            ? 0.30f
                            : 0.20f;

                float repairChance =
                    experienceValue >= 20
                        ? 0.24f
                        : experienceValue >= 5
                            ? 0.12f
                            : 0.06f;

                controller.Configure(
                    experienceValue,
                    15,
                    healChance,
                    repairChance,
                    pickupSprite
                );

                EditorUtility.SetDirty(
                    controller
                );
            }
        }

        private static Sprite ResolvePickupSprite(
            Transform player,
            Transform snakeBody
        )
        {
            SpriteRenderer playerRenderer =
                player.GetComponent<SpriteRenderer>();

            if (playerRenderer != null &&
                playerRenderer.sprite != null)
            {
                return playerRenderer.sprite;
            }

            SpriteRenderer bodyRenderer =
                snakeBody.GetComponentInChildren<SpriteRenderer>(
                    true
                );

            if (bodyRenderer != null)
            {
                return bodyRenderer.sprite;
            }

            return null;
        }

        private static void EnsurePrototypeHint(
            Transform hudCanvas
        )
        {
            Font font =
                ResolveUIFont(
                    hudCanvas
                );

            Text hint =
                EnsureText(
                    hudCanvas,
                    "PrototypeHintText",
                    "P: Damage  [: Lose Body  H: Heal Pickup  J: Repair Pickup  3/5: Grade Test",
                    font,
                    15,
                    new Vector2(
                        0f,
                        -245f
                    ),
                    new Vector2(
                        760f,
                        30f
                    )
                );

            hint.color =
                new Color(
                    0.82f,
                    0.93f,
                    1f,
                    1f
                );
        }

        private static Text EnsureText(
            Transform parent,
            string objectName,
            string value,
            Font font,
            int fontSize,
            Vector2 anchoredPosition,
            Vector2 size
        )
        {
            Transform existing =
                parent.Find(
                    objectName
                );

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
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.anchoredPosition =
                anchoredPosition;

            rect.sizeDelta =
                size;

            Text text =
                textObject.GetComponent<Text>();

            text.font =
                font;

            text.text =
                value;

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

        private static void CleanupLegacySetup()
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(
                LegacySetupPath
            ) != null)
            {
                AssetDatabase.DeleteAsset(
                    LegacySetupPath
                );
            }
        }
    }
}
