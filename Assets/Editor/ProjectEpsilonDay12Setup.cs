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
    public static class ProjectEpsilonDay12Setup
    {
        private const string GameScenePath =
            "Assets/Scenes/Game.unity";

        private const string DebugBladePath =
            "Assets/Data/Weapons/DebugBlade.asset";

        private const string SessionKey =
            "ProjectEpsilon.Day12.AutoSetup";

        private const string LegacySetupPath =
            "Assets/Editor/ProjectEpsilonDay11Setup.cs";

        static ProjectEpsilonDay12Setup()
        {
            EditorApplication.delayCall +=
                RunAutoSetup;
        }

        [MenuItem("Project Epsilon/Day 12/Run Setup")]
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
                    "[Project Epsilon] Game Scene이 없어 Day 12 자동 구성을 건너뜁니다."
                );

                return;
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    GameScenePath,
                    OpenSceneMode.Single
                );

            if (!force &&
                IsDay12Configured())
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
                    "[Project Epsilon] Gameplay 또는 UI Root를 찾지 못해 Day 12 구성을 중단합니다."
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
                    "[Project Epsilon] Player, SnakeBody 또는 HUDCanvas를 찾지 못해 Day 12 구성을 중단합니다."
                );

                return;
            }

            SnakeWeaponManager weaponManager =
                snakeBody.GetComponent<SnakeWeaponManager>();

            SnakeMovement movement =
                player.GetComponent<SnakeMovement>();

            SnakeStamina stamina =
                player.GetComponent<SnakeStamina>();

            if (weaponManager == null ||
                movement == null ||
                stamina == null)
            {
                Debug.LogError(
                    "[Project Epsilon] Day12 핵심 컴포넌트를 찾지 못해 구성을 중단합니다."
                );

                return;
            }

            WeaponMergePanelController mergePanel =
                EnsureMergePanel(
                    hudCanvas
                );

            SnakeWeaponMergeController mergeController =
                player.GetComponent<SnakeWeaponMergeController>();

            if (mergeController == null)
            {
                mergeController =
                    player.gameObject.AddComponent<SnakeWeaponMergeController>();
            }

            mergeController.Configure(
                weaponManager,
                movement,
                stamina,
                mergePanel,
                0.7f
            );

            WeaponData debugBlade =
                AssetDatabase.LoadAssetAtPath<WeaponData>(
                    DebugBladePath
                );

            SnakeWeaponMergeDebugControls debugControls =
                player.GetComponent<SnakeWeaponMergeDebugControls>();

            if (debugControls == null)
            {
                debugControls =
                    player.gameObject.AddComponent<SnakeWeaponMergeDebugControls>();
            }

            debugControls.Configure(
                weaponManager,
                debugBlade
            );

            EnsureMergeHint(
                hudCanvas
            );

            EditorUtility.SetDirty(
                movement
            );

            EditorUtility.SetDirty(
                stamina
            );

            EditorUtility.SetDirty(
                mergeController
            );

            EditorUtility.SetDirty(
                debugControls
            );

            EditorUtility.SetDirty(
                mergePanel
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
                mergePanel.gameObject;

            Debug.Log(
                "[Project Epsilon] Day 12 merge + grade damage setup complete."
            );
        }

        private static bool IsDay12Configured()
        {
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
                return false;
            }

            Transform player =
                gameplayRoot.transform.Find(
                    "Player"
                );

            Transform hudCanvas =
                uiRoot.transform.Find(
                    "HUDCanvas"
                );

            if (player == null ||
                hudCanvas == null)
            {
                return false;
            }

            Transform mergePanel =
                hudCanvas.Find(
                    "MergePanel"
                );

            return
                player.GetComponent<SnakeWeaponMergeController>() != null &&
                player.GetComponent<SnakeWeaponMergeDebugControls>() != null &&
                mergePanel != null &&
                mergePanel.GetComponent<WeaponMergePanelController>() != null;
        }

        private static WeaponMergePanelController
            EnsureMergePanel(
                Transform hudCanvas
            )
        {
            Transform existing =
                hudCanvas.Find(
                    "MergePanel"
                );

            GameObject panelObject;

            if (existing == null)
            {
                panelObject =
                    new GameObject(
                        "MergePanel",
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image)
                    );

                panelObject.transform.SetParent(
                    hudCanvas,
                    false
                );
            }
            else
            {
                panelObject =
                    existing.gameObject;
            }

            RectTransform panelRect =
                panelObject.GetComponent<RectTransform>();

            panelRect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            panelRect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            panelRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            panelRect.anchoredPosition =
                Vector2.zero;

            panelRect.sizeDelta =
                new Vector2(
                    720f,
                    390f
                );

            Image panelImage =
                panelObject.GetComponent<Image>();

            panelImage.color =
                new Color(
                    0.05f,
                    0.07f,
                    0.08f,
                    0.94f
                );

            Font font =
                ResolveUIFont(
                    hudCanvas
                );

            Text title =
                EnsureText(
                    panelObject.transform,
                    "Title",
                    "WEAPON MERGE",
                    font,
                    32,
                    new Vector2(
                        0f,
                        145f
                    ),
                    new Vector2(
                        620f,
                        42f
                    )
                );

            Text state =
                EnsureText(
                    panelObject.transform,
                    "StateText",
                    "REAL-TIME / AUTO FORWARD / SPEED 70%",
                    font,
                    17,
                    new Vector2(
                        0f,
                        104f
                    ),
                    new Vector2(
                        620f,
                        32f
                    )
                );

            Button button01 =
                EnsureButton(
                    panelObject.transform,
                    "MergeCandidate_01",
                    font,
                    new Vector2(
                        -220f,
                        -15f
                    ),
                    new Vector2(
                        195f,
                        160f
                    ),
                    "Merge Candidate"
                );

            Button button02 =
                EnsureButton(
                    panelObject.transform,
                    "MergeCandidate_02",
                    font,
                    new Vector2(
                        0f,
                        -15f
                    ),
                    new Vector2(
                        195f,
                        160f
                    ),
                    "Merge Candidate"
                );

            Button button03 =
                EnsureButton(
                    panelObject.transform,
                    "MergeCandidate_03",
                    font,
                    new Vector2(
                        220f,
                        -15f
                    ),
                    new Vector2(
                        195f,
                        160f
                    ),
                    "Merge Candidate"
                );

            Button closeButton =
                EnsureButton(
                    panelObject.transform,
                    "CloseButton",
                    font,
                    new Vector2(
                        0f,
                        -145f
                    ),
                    new Vector2(
                        150f,
                        42f
                    ),
                    "CLOSE [M]"
                );

            Text label01 =
                button01.transform.Find(
                    "Label"
                ).GetComponent<Text>();

            Text label02 =
                button02.transform.Find(
                    "Label"
                ).GetComponent<Text>();

            Text label03 =
                button03.transform.Find(
                    "Label"
                ).GetComponent<Text>();

            WeaponMergePanelController controller =
                panelObject.GetComponent<WeaponMergePanelController>();

            if (controller == null)
            {
                controller =
                    panelObject.AddComponent<WeaponMergePanelController>();
            }

            controller.Configure(
                title,
                state,
                button01,
                label01,
                button02,
                label02,
                button03,
                label03,
                closeButton
            );

            controller.Hide();

            return controller;
        }

        private static void EnsureMergeHint(
            Transform hudCanvas
        )
        {
            EnsureText(
                hudCanvas,
                "MergeHintText",
                "N: Debug Merge Pair   M: Merge",
                ResolveUIFont(
                    hudCanvas
                ),
                16,
                new Vector2(
                    0f,
                    -210f
                ),
                new Vector2(
                    420f,
                    30f
                )
            );
        }

        private static Button EnsureButton(
            Transform parent,
            string objectName,
            Font font,
            Vector2 anchoredPosition,
            Vector2 size,
            string labelValue
        )
        {
            Transform existing =
                parent.Find(
                    objectName
                );

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

            Image image =
                buttonObject.GetComponent<Image>();

            image.color =
                new Color(
                    0.20f,
                    0.34f,
                    0.31f,
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
                    labelValue,
                    font,
                    18,
                    Vector2.zero,
                    size
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
