using System.IO;
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
    public static class ProjectEpsilonDay10Setup
    {
        private const string GameScenePath =
            "Assets/Scenes/Game.unity";

        private const string SessionKey =
            "ProjectEpsilon.Day10.AutoSetup";

        private static readonly string[] LegacyAssets =
        {
            "Assets/Editor/ProjectEpsilonDay9Setup.cs",
            "Assets/Editor/ProjectEpsilonDay9Setup.cs.meta"
        };

        static ProjectEpsilonDay10Setup()
        {
            EditorApplication.delayCall +=
                RunAutoSetup;
        }

        [MenuItem("Project Epsilon/Day 10/Run Setup")]
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
                    "[Project Epsilon] Game Scene이 없어 Day 10 자동 구성을 건너뜁니다."
                );
                return;
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    GameScenePath,
                    OpenSceneMode.Single
                );

            if (!force &&
                IsDay10Configured())
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
                    "[Project Epsilon] Gameplay 또는 UI Root를 찾지 못해 Day 10 구성을 중단합니다."
                );
                return;
            }

            Transform playerTransform =
                gameplayRoot.transform.Find("Player");

            Transform bodyRoot =
                gameplayRoot.transform.Find("SnakeBody");

            if (playerTransform == null ||
                bodyRoot == null)
            {
                Debug.LogError(
                    "[Project Epsilon] Player 또는 SnakeBody를 찾지 못해 Day 10 구성을 중단합니다."
                );
                return;
            }

            SnakeBodyManager bodyManager =
                bodyRoot.GetComponent<SnakeBodyManager>();

            SnakeHealth health =
                playerTransform.GetComponent<SnakeHealth>();

            if (bodyManager == null ||
                health == null)
            {
                Debug.LogError(
                    "[Project Epsilon] SnakeBodyManager 또는 SnakeHealth를 찾지 못해 Day 10 구성을 중단합니다."
                );
                return;
            }

            SnakeExperience experience =
                EnsureExperience(
                    playerTransform.gameObject
                );

            Transform hudCanvas =
                uiRoot.transform.Find("HUDCanvas");

            if (hudCanvas == null)
            {
                Debug.LogError(
                    "[Project Epsilon] HUDCanvas를 찾지 못해 Day 10 구성을 중단합니다."
                );
                return;
            }

            HUDController hudController =
                hudCanvas.GetComponent<HUDController>();

            if (hudController == null)
            {
                Debug.LogError(
                    "[Project Epsilon] HUDController를 찾지 못해 Day 10 구성을 중단합니다."
                );
                return;
            }

            SnakeExperienceHUDPresenter presenter =
                EnsureExperiencePresenter(
                    hudCanvas.gameObject,
                    experience,
                    hudController
                );

            LevelUpPanelController levelUpPanel =
                EnsureLevelUpPanel(
                    hudCanvas
                );

            SnakeLevelUpController levelUpController =
                EnsureLevelUpController(
                    playerTransform.gameObject,
                    experience,
                    bodyManager,
                    health,
                    levelUpPanel
                );

            SnakeProgressionDebugControls debugControls =
                EnsureDebugControls(
                    playerTransform.gameObject,
                    experience
                );

            EditorUtility.SetDirty(experience);
            EditorUtility.SetDirty(presenter);
            EditorUtility.SetDirty(levelUpPanel);
            EditorUtility.SetDirty(levelUpController);
            EditorUtility.SetDirty(debugControls);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(
                scene,
                GameScenePath
            );

            CleanupLegacyAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                playerTransform.gameObject;

            Debug.Log(
                "[Project Epsilon] Day 10 level + body growth setup complete."
            );
        }

        private static bool IsDay10Configured()
        {
            GameObject gameplayRoot =
                GameObject.Find("===Gameplay===");

            GameObject uiRoot =
                GameObject.Find("===UI===");

            if (gameplayRoot == null ||
                uiRoot == null)
            {
                return false;
            }

            Transform player =
                gameplayRoot.transform.Find("Player");

            Transform hudCanvas =
                uiRoot.transform.Find("HUDCanvas");

            if (player == null ||
                hudCanvas == null)
            {
                return false;
            }

            Transform levelUpPanel =
                hudCanvas.Find("LevelUpPanel");

            return
                player.GetComponent<SnakeExperience>() != null &&
                player.GetComponent<SnakeLevelUpController>() != null &&
                player.GetComponent<SnakeProgressionDebugControls>() != null &&
                levelUpPanel != null &&
                levelUpPanel.GetComponent<LevelUpPanelController>() != null;
        }

        private static SnakeExperience EnsureExperience(
            GameObject player
        )
        {
            SnakeExperience experience =
                player.GetComponent<SnakeExperience>();

            if (experience == null)
            {
                experience =
                    player.AddComponent<SnakeExperience>();
            }

            experience.Configure(
                10,
                1.12f
            );

            experience.ResetProgression();

            return experience;
        }

        private static SnakeExperienceHUDPresenter
            EnsureExperiencePresenter(
                GameObject hudCanvas,
                SnakeExperience experience,
                HUDController hudController
            )
        {
            SnakeExperienceHUDPresenter presenter =
                hudCanvas.GetComponent<SnakeExperienceHUDPresenter>();

            if (presenter == null)
            {
                presenter =
                    hudCanvas.AddComponent<SnakeExperienceHUDPresenter>();
            }

            presenter.Bind(
                experience,
                hudController
            );

            return presenter;
        }

        private static SnakeLevelUpController
            EnsureLevelUpController(
                GameObject player,
                SnakeExperience experience,
                SnakeBodyManager bodyManager,
                SnakeHealth health,
                LevelUpPanelController panel
            )
        {
            SnakeLevelUpController controller =
                player.GetComponent<SnakeLevelUpController>();

            if (controller == null)
            {
                controller =
                    player.AddComponent<SnakeLevelUpController>();
            }

            controller.Configure(
                experience,
                bodyManager,
                health,
                panel
            );

            return controller;
        }

        private static SnakeProgressionDebugControls
            EnsureDebugControls(
                GameObject player,
                SnakeExperience experience
            )
        {
            SnakeProgressionDebugControls controls =
                player.GetComponent<SnakeProgressionDebugControls>();

            if (controls == null)
            {
                controls =
                    player.AddComponent<SnakeProgressionDebugControls>();
            }

            controls.Configure(
                experience,
                10
            );

            return controls;
        }

        private static LevelUpPanelController
            EnsureLevelUpPanel(
                Transform hudCanvas
            )
        {
            Transform existing =
                hudCanvas.Find("LevelUpPanel");

            GameObject panelObject;

            if (existing == null)
            {
                panelObject = new GameObject(
                    "LevelUpPanel",
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
                panelObject = existing.gameObject;
            }

            RectTransform panelRect =
                panelObject.GetComponent<RectTransform>();

            panelRect.anchorMin =
                new Vector2(0.5f, 0.5f);

            panelRect.anchorMax =
                new Vector2(0.5f, 0.5f);

            panelRect.pivot =
                new Vector2(0.5f, 0.5f);

            panelRect.anchoredPosition =
                Vector2.zero;

            panelRect.sizeDelta =
                new Vector2(440f, 270f);

            Image panelImage =
                panelObject.GetComponent<Image>();

            panelImage.color =
                new Color(
                    0.055f,
                    0.065f,
                    0.095f,
                    0.96f
                );

            Font font =
                ResolveUIFont(hudCanvas);

            Text titleText = EnsureText(
                panelObject.transform,
                "Title",
                "LEVEL UP!",
                font,
                34,
                new Vector2(0f, 86f),
                new Vector2(380f, 44f)
            );

            Text levelText = EnsureText(
                panelObject.transform,
                "LevelText",
                "Lv. 1",
                font,
                28,
                new Vector2(0f, 38f),
                new Vector2(360f, 40f)
            );

            Text growthText = EnsureText(
                panelObject.transform,
                "GrowthText",
                "Body +1",
                font,
                22,
                new Vector2(0f, -20f),
                new Vector2(380f, 64f)
            );

            Button continueButton =
                EnsureButton(
                    panelObject.transform,
                    "ContinueButton",
                    "Continue",
                    font,
                    new Vector2(0f, -96f),
                    new Vector2(170f, 44f)
                );

            LevelUpPanelController controller =
                panelObject.GetComponent<LevelUpPanelController>();

            if (controller == null)
            {
                controller =
                    panelObject.AddComponent<LevelUpPanelController>();
            }

            controller.Configure(
                titleText,
                levelText,
                growthText,
                continueButton
            );

            controller.Hide();

            return controller;
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
                textObject = new GameObject(
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
                textObject = existing.gameObject;
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

            rect.sizeDelta = size;

            Text text =
                textObject.GetComponent<Text>();

            text.font = font;
            text.text = textValue;
            text.fontSize = fontSize;
            text.alignment =
                TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;

            return text;
        }

        private static Button EnsureButton(
            Transform parent,
            string objectName,
            string label,
            Font font,
            Vector2 anchoredPosition,
            Vector2 size
        )
        {
            Transform existing =
                parent.Find(objectName);

            GameObject buttonObject;

            if (existing == null)
            {
                buttonObject = new GameObject(
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
                buttonObject = existing.gameObject;
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

            rect.sizeDelta = size;

            Image image =
                buttonObject.GetComponent<Image>();

            image.color =
                new Color(
                    0.36f,
                    0.25f,
                    0.72f,
                    1f
                );

            Button button =
                buttonObject.GetComponent<Button>();

            button.targetGraphic = image;

            Text labelText = EnsureText(
                buttonObject.transform,
                "Label",
                label,
                font,
                20,
                Vector2.zero,
                size
            );

            RectTransform labelRect =
                labelText.GetComponent<RectTransform>();

            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return button;
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
