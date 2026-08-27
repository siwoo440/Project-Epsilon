using System.IO;
using System.Linq;
using ProjectEpsilon.Core;
using ProjectEpsilon.Player;
using ProjectEpsilon.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectEpsilon.Editor
{
    [InitializeOnLoad]
    public static class ProjectEpsilonDay4Setup
    {
        private const string GameScenePath = "Assets/Scenes/Game.unity";
        private const string HeadSpritePath = "Assets/Art/Sprites/DebugSnakeHead.png";
        private const string BodySpritePath = "Assets/Art/Sprites/DebugSnakeBody.png";
        private const string SessionKey = "ProjectEpsilon.Day4.AutoSetup";

        private static readonly string[] LegacyAssets =
        {
            "Assets/Scripts/Player/PlayerDebugMovement.cs",
            "Assets/Scripts/Player/PlayerDebugMovement.cs.meta",
            "Assets/Editor/ProjectEpsilonDay2Setup.cs",
            "Assets/Editor/ProjectEpsilonDay2Setup.cs.meta",
            "Assets/Editor/ProjectEpsilonDay3Setup.cs",
            "Assets/Editor/ProjectEpsilonDay3Setup.cs.meta",
            "Assets/Editor/ProjectEpsilonManagerRootFix.cs",
            "Assets/Editor/ProjectEpsilonManagerRootFix.cs.meta"
        };

        static ProjectEpsilonDay4Setup()
        {
            EditorApplication.delayCall += RunAutoSetup;
        }

        [MenuItem("Project Epsilon/Day 4/Run Setup")]
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
                Debug.LogWarning("[Project Epsilon] Game Scene이 없어 Day 4 자동 구성을 건너뜁니다.");
                return;
            }

            EnsureDebugSprites();

            Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

            if (!force && IsDay4Configured())
            {
                CleanupLegacyAssets();
                return;
            }

            GameObject managersRoot = EnsureRoot("===Managers===");
            GameObject gameplayRoot = EnsureRoot("===Gameplay===");
            GameObject uiRoot = EnsureRoot("===UI===");
            EnsureRoot("===Environment===");

            EnsureManagers(managersRoot);

            GameObject player = EnsureSnakePlayer(gameplayRoot.transform);
            SnakePathRecorder recorder = player.GetComponent<SnakePathRecorder>();
            EnsureSnakeBody(gameplayRoot.transform, recorder);
            EnsureCameraFollow(player.transform);
            EnsureHUD(uiRoot.transform);
            EnsureEventSystem(uiRoot.transform);
            EnsureSettingsUI(uiRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameScenePath);

            Selection.activeGameObject = player;

            CleanupLegacyAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Project Epsilon] Day 4 Snake movement + body follow setup complete.");
        }

        private static bool IsDay4Configured()
        {
            GameObject gameplayRoot = GameObject.Find("===Gameplay===");
            GameObject managersRoot = GameObject.Find("===Managers===");
            GameObject uiRoot = GameObject.Find("===UI===");

            if (gameplayRoot == null || managersRoot == null || uiRoot == null)
            {
                return false;
            }

            Transform player = gameplayRoot.transform.Find("Player");
            Transform snakeBody = gameplayRoot.transform.Find("SnakeBody");
            Transform hudCanvas = uiRoot.transform.Find("HUDCanvas");

            if (player == null || snakeBody == null || hudCanvas == null)
            {
                return false;
            }

            return player.GetComponent<SnakeMovement>() != null &&
                player.GetComponent<SnakePathRecorder>() != null &&
                snakeBody.GetComponent<SnakeBodyFollower>() != null &&
                managersRoot.GetComponent<GameManager>() != null &&
                managersRoot.GetComponent<InputBindingManager>() != null &&
                hudCanvas.GetComponent<HUDController>() != null &&
                hudCanvas.GetComponent<SettingsMenuController>() != null;
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

        private static void EnsureManagers(GameObject managersRoot)
        {
            GameManager rootManager = managersRoot.GetComponent<GameManager>();
            GameManager[] hierarchyManagers = managersRoot.GetComponentsInChildren<GameManager>(true);
            GameManager legacyManager = hierarchyManagers.FirstOrDefault(
                manager => manager != null && manager.gameObject != managersRoot
            );

            if (rootManager == null)
            {
                rootManager = managersRoot.AddComponent<GameManager>();

                if (legacyManager != null)
                {
                    SerializedObject source = new SerializedObject(legacyManager);
                    SerializedObject destination = new SerializedObject(rootManager);
                    SerializedProperty sourceState = source.FindProperty("currentState");
                    SerializedProperty destinationState = destination.FindProperty("currentState");

                    if (sourceState != null && destinationState != null)
                    {
                        destinationState.enumValueIndex = sourceState.enumValueIndex;
                        destination.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
            }

            foreach (GameManager manager in hierarchyManagers)
            {
                if (manager == null || manager.gameObject == managersRoot)
                {
                    continue;
                }

                GameObject legacyObject = manager.gameObject;
                Object.DestroyImmediate(manager);

                Component[] remaining = legacyObject.GetComponents<Component>();

                if (legacyObject.name == "GameManager" &&
                    legacyObject.transform.childCount == 0 &&
                    remaining.Length == 1)
                {
                    Object.DestroyImmediate(legacyObject);
                }
            }

            if (managersRoot.GetComponent<InputBindingManager>() == null)
            {
                managersRoot.AddComponent<InputBindingManager>();
            }
        }

        private static GameObject EnsureSnakePlayer(Transform gameplayRoot)
        {
            Transform existing = gameplayRoot.Find("Player");
            GameObject player;

            if (existing != null)
            {
                player = existing.gameObject;
            }
            else
            {
                player = new GameObject("Player");
                player.transform.SetParent(gameplayRoot, false);
            }

            player.transform.position = Vector3.zero;
            player.transform.rotation = Quaternion.identity;
            player.transform.localScale = new Vector3(0.72f, 0.72f, 1f);

            SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                spriteRenderer = player.AddComponent<SpriteRenderer>();
            }

            Sprite headSprite = AssetDatabase.LoadAssetAtPath<Sprite>(HeadSpritePath);

            if (headSprite != null)
            {
                spriteRenderer.sprite = headSprite;
            }

            spriteRenderer.color = new Color(0.32f, 0.88f, 1f, 1f);
            spriteRenderer.sortingOrder = 10;

            if (player.GetComponent<PlayerInputReader>() == null)
            {
                player.AddComponent<PlayerInputReader>();
            }

            RemoveLegacyDebugMovement(player);

            if (player.GetComponent<SnakeMovement>() == null)
            {
                player.AddComponent<SnakeMovement>();
            }

            SnakePathRecorder recorder = player.GetComponent<SnakePathRecorder>();

            if (recorder == null)
            {
                recorder = player.AddComponent<SnakePathRecorder>();
            }

            return player;
        }

        private static void RemoveLegacyDebugMovement(GameObject player)
        {
            MonoBehaviour[] behaviours = player.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    continue;
                }

                if (behaviour.GetType().FullName == "ProjectEpsilon.Player.PlayerDebugMovement")
                {
                    Object.DestroyImmediate(behaviour);
                }
            }
        }

        private static void EnsureSnakeBody(Transform gameplayRoot, SnakePathRecorder recorder)
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

            Transform[] segments = new Transform[3];

            for (int index = 0; index < segments.Length; index++)
            {
                string segmentName = $"Body_{index + 1:00}";
                Transform existingSegment = bodyRoot.transform.Find(segmentName);
                GameObject segment;

                if (existingSegment != null)
                {
                    segment = existingSegment.gameObject;
                }
                else
                {
                    segment = new GameObject(segmentName);
                    segment.transform.SetParent(bodyRoot.transform, false);
                }

                segment.transform.localScale = new Vector3(0.62f, 0.62f, 1f);

                SpriteRenderer renderer = segment.GetComponent<SpriteRenderer>();

                if (renderer == null)
                {
                    renderer = segment.AddComponent<SpriteRenderer>();
                }

                Sprite bodySprite = AssetDatabase.LoadAssetAtPath<Sprite>(BodySpritePath);

                if (bodySprite != null)
                {
                    renderer.sprite = bodySprite;
                }

                renderer.color = new Color(0.62f, 0.48f, 1f, 1f);
                renderer.sortingOrder = 6 - index;
                segments[index] = segment.transform;
            }

            SnakeBodyFollower follower = bodyRoot.GetComponent<SnakeBodyFollower>();

            if (follower == null)
            {
                follower = bodyRoot.AddComponent<SnakeBodyFollower>();
            }

            recorder.ResetHistory();
            follower.Bind(recorder, segments, 0.58f);
        }

        private static void EnsureCameraFollow(Transform player)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0f, 0f, -10f);
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 5f;
            }

            CameraFollow2D follow = mainCamera.GetComponent<CameraFollow2D>();

            if (follow == null)
            {
                follow = mainCamera.gameObject.AddComponent<CameraFollow2D>();
            }

            follow.SetTarget(player);
        }

        private static void EnsureHUD(Transform uiRoot)
        {
            Transform existingCanvas = uiRoot.Find("HUDCanvas");
            GameObject canvasObject;

            if (existingCanvas != null)
            {
                canvasObject = existingCanvas.gameObject;
            }
            else
            {
                canvasObject = new GameObject(
                    "HUDCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster)
                );

                canvasObject.transform.SetParent(uiRoot, false);
            }

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Text health = EnsureHUDText(canvasObject.transform, "BodyHP", new Vector2(30f, -30f), TextAnchor.UpperLeft, new Vector2(0f, 1f), 26);
            Text bodyCount = EnsureHUDText(canvasObject.transform, "BodyCount", new Vector2(30f, -70f), TextAnchor.UpperLeft, new Vector2(0f, 1f), 26);
            Text level = EnsureHUDText(canvasObject.transform, "Level", new Vector2(30f, -110f), TextAnchor.UpperLeft, new Vector2(0f, 1f), 26);
            Text experience = EnsureHUDText(canvasObject.transform, "Experience", new Vector2(30f, -150f), TextAnchor.UpperLeft, new Vector2(0f, 1f), 26);
            Text stamina = EnsureHUDText(canvasObject.transform, "Stamina", new Vector2(30f, 30f), TextAnchor.LowerLeft, new Vector2(0f, 0f), 26);
            Text timer = EnsureHUDText(canvasObject.transform, "GameTimer", new Vector2(0f, -30f), TextAnchor.UpperCenter, new Vector2(0.5f, 1f), 34);

            HUDController hud = canvasObject.GetComponent<HUDController>();

            if (hud == null)
            {
                hud = canvasObject.AddComponent<HUDController>();
            }

            hud.Bind(health, bodyCount, experience, level, stamina);

            GameTimerUI timerUI = canvasObject.GetComponent<GameTimerUI>();

            if (timerUI == null)
            {
                timerUI = canvasObject.AddComponent<GameTimerUI>();
            }

            timerUI.Bind(timer);
        }

        private static Text EnsureHUDText(
            Transform parent,
            string objectName,
            Vector2 anchoredPosition,
            TextAnchor alignment,
            Vector2 anchor,
            int fontSize
        )
        {
            Text text = EnsureText(
                parent,
                objectName,
                string.Empty,
                anchoredPosition,
                anchor,
                anchor,
                anchor,
                new Vector2(420f, 44f),
                fontSize,
                alignment
            );

            return text;
        }

        private static void EnsureEventSystem(Transform uiRoot)
        {
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();

            if (eventSystem != null)
            {
                StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();

                if (legacyModule != null)
                {
                    Object.DestroyImmediate(legacyModule);
                }

                if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
                {
                    eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                }

                return;
            }

            GameObject eventSystemObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule)
            );

            eventSystemObject.transform.SetParent(uiRoot, false);
        }

        private static void EnsureSettingsUI(Transform uiRoot)
        {
            Transform hudCanvasTransform = uiRoot.Find("HUDCanvas");

            if (hudCanvasTransform == null)
            {
                return;
            }

            GameObject hudCanvas = hudCanvasTransform.gameObject;

            Button openButton = EnsureButton(
                hudCanvas.transform,
                "SettingsButton",
                "Settings",
                new Vector2(-30f, -30f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(220f, 54f)
            );

            GameObject panel = EnsurePanel(hudCanvas.transform);

            EnsureText(panel.transform, "Title", "Settings", new Vector2(0f, -35f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(520f, 50f), 32, TextAnchor.MiddleCenter);
            EnsureText(panel.transform, "TurnLeftLabel", "Turn Left", new Vector2(65f, -125f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 48f), 25, TextAnchor.MiddleLeft);
            Button leftButton = EnsureButton(panel.transform, "TurnLeftButton", "A", new Vector2(-65f, -125f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(220f, 48f));
            EnsureText(panel.transform, "TurnRightLabel", "Turn Right", new Vector2(65f, -195f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 48f), 25, TextAnchor.MiddleLeft);
            Button rightButton = EnsureButton(panel.transform, "TurnRightButton", "D", new Vector2(-65f, -195f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(220f, 48f));
            EnsureText(panel.transform, "BoostLabel", "Boost", new Vector2(65f, -265f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 48f), 25, TextAnchor.MiddleLeft);
            Button boostButton = EnsureButton(panel.transform, "BoostButton", "Left Shift", new Vector2(-65f, -265f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(220f, 48f));
            Text statusText = EnsureText(panel.transform, "Status", string.Empty, new Vector2(0f, -335f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(600f, 48f), 20, TextAnchor.MiddleCenter);
            Button resetButton = EnsureButton(panel.transform, "ResetButton", "Reset", new Vector2(-130f, 40f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(230f, 52f));
            Button closeButton = EnsureButton(panel.transform, "CloseButton", "Close", new Vector2(130f, 40f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(230f, 52f));

            SettingsMenuController controller = hudCanvas.GetComponent<SettingsMenuController>();

            if (controller == null)
            {
                controller = hudCanvas.AddComponent<SettingsMenuController>();
            }

            controller.Bind(
                panel,
                openButton,
                closeButton,
                resetButton,
                leftButton,
                rightButton,
                boostButton,
                leftButton.GetComponentInChildren<Text>(),
                rightButton.GetComponentInChildren<Text>(),
                boostButton.GetComponentInChildren<Text>(),
                statusText
            );

            panel.SetActive(false);
        }

        private static GameObject EnsurePanel(Transform parent)
        {
            Transform existing = parent.Find("SettingsPanel");
            GameObject panel;

            if (existing != null)
            {
                panel = existing.gameObject;
            }
            else
            {
                panel = new GameObject(
                    "SettingsPanel",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                );
                panel.transform.SetParent(parent, false);
            }

            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(720f, 520f);

            Image image = panel.GetComponent<Image>();
            image.color = new Color(0.05f, 0.07f, 0.1f, 0.94f);

            return panel;
        }

        private static Button EnsureButton(
            Transform parent,
            string objectName,
            string label,
            Vector2 anchoredPosition,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 size
        )
        {
            Transform existing = parent.Find(objectName);
            GameObject buttonObject;

            if (existing != null)
            {
                buttonObject = existing.gameObject;
            }
            else
            {
                buttonObject = new GameObject(
                    objectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(Button)
                );
                buttonObject.transform.SetParent(parent, false);
            }

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.22f, 0.28f, 0.96f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            EnsureText(
                buttonObject.transform,
                "Label",
                label,
                Vector2.zero,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                22,
                TextAnchor.MiddleCenter
            );

            return button;
        }

        private static Text EnsureText(
            Transform parent,
            string objectName,
            string value,
            Vector2 anchoredPosition,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 size,
            int fontSize,
            TextAnchor alignment
        )
        {
            Transform existing = parent.Find(objectName);
            GameObject textObject;

            if (existing != null)
            {
                textObject = existing.gameObject;
            }
            else
            {
                textObject = new GameObject(
                    objectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Text)
                );
                textObject.transform.SetParent(parent, false);
            }

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }

        private static void EnsureDebugSprites()
        {
            EnsureSprite(HeadSpritePath, true);
            EnsureSprite(BodySpritePath, false);
        }

        private static void EnsureSprite(string path, bool isHead)
        {
            if (File.Exists(path))
            {
                return;
            }

            string directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool filled;

                    if (isHead)
                    {
                        float normalizedY = y / (float)(size - 1);
                        float halfWidth = Mathf.Lerp(11f, 2f, normalizedY);
                        filled = Mathf.Abs(x - (size - 1) * 0.5f) <= halfWidth && y >= 3 && y <= 29;
                    }
                    else
                    {
                        float dx = x - (size - 1) * 0.5f;
                        float dy = y - (size - 1) * 0.5f;
                        filled = dx * dx + dy * dy <= 12f * 12f;
                    }

                    pixels[y * size + x] = filled ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32f;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
        }

        private static void CleanupLegacyAssets()
        {
            foreach (string assetPath in LegacyAssets)
            {
                if (File.Exists(assetPath) || AssetDatabase.LoadMainAssetAtPath(assetPath) != null)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }
        }
    }
}
