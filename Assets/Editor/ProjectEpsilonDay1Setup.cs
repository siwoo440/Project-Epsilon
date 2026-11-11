using System.IO; // 파일 입출력 기능 연결
using System.Linq; // C# 기본 기능 연결
using ProjectEpsilon.Core; // 대상 기능 연결
using UnityEditor; // Unity 에디터 기능 연결
using UnityEditor.SceneManagement; // Unity 에디터 기능 연결
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.SceneManagement; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Editor // 프로젝트 기능 영역
{
    [InitializeOnLoad] // 초기화·불러오기 설정
    public static class ProjectEpsilonDay1Setup // 프로젝트·초기 구성 자료 구조
    {
        private const string GameScenePath = "Assets/Scenes/Game.unity"; // 게임·씬 구성 선언
        private const string SessionKey = "ProjectEpsilon.Day1.AutoSetup"; // 키·프로젝트 구성 선언

        private static readonly string[] RequiredFolders = // 대상 구성 선언
        {
            "Assets/Art", // 에셋·이미지 경로 또는 문구 값
            "Assets/Art/Sprites", // 에셋·이미지 경로 또는 문구 값
            "Assets/Art/Materials", // 에셋·이미지 경로 또는 문구 값
            "Assets/Art/VFX", // 에셋·이미지 경로 또는 문구 값
            "Assets/Audio", // 에셋·오디오 경로 또는 문구 값
            "Assets/Audio/BGM", // 에셋·오디오 경로 또는 문구 값
            "Assets/Audio/SFX", // 에셋·오디오 경로 또는 문구 값
            "Assets/Data", // 에셋·데이터 경로 또는 문구 값
            "Assets/Data/Characters", // 에셋·데이터 경로 또는 문구 값
            "Assets/Data/Enemies", // 에셋·데이터 경로 또는 문구 값
            "Assets/Data/Weapons", // 에셋·데이터 경로 또는 문구 값
            "Assets/Prefabs", // 에셋 경로 또는 문구 값
            "Assets/Prefabs/Characters", // 에셋 경로 또는 문구 값
            "Assets/Prefabs/Enemies", // 에셋·적 경로 또는 문구 값
            "Assets/Prefabs/Environment", // 에셋 경로 또는 문구 값
            "Assets/Prefabs/Items", // 에셋·아이템 경로 또는 문구 값
            "Assets/Prefabs/UI", // 에셋·UI 경로 또는 문구 값
            "Assets/Prefabs/Weapons", // 에셋·포탑 경로 또는 문구 값
            "Assets/Scenes", // 에셋·씬 경로 또는 문구 값
            "Assets/Scripts", // 에셋·스크립트 경로 또는 문구 값
            "Assets/Scripts/Core", // 에셋·스크립트 경로 또는 문구 값
            "Assets/Scripts/Player", // 에셋·스크립트 경로 또는 문구 값
            "Assets/Scripts/Combat", // 에셋·스크립트 경로 또는 문구 값
            "Assets/Scripts/Enemies", // 에셋·스크립트 경로 또는 문구 값
            "Assets/Scripts/Weapons", // 에셋·스크립트 경로 또는 문구 값
            "Assets/Scripts/UI", // 에셋·스크립트 경로 또는 문구 값
            "Assets/Scripts/Data", // 에셋·스크립트 경로 또는 문구 값
            "Assets/Settings" // 에셋·설정 경로 또는 문구 값
        };

        static ProjectEpsilonDay1Setup() // 프로젝트·초기 구성 기능 선언
        {
            EditorApplication.delayCall += RunAutoSetup; // 에디터·실행 값 설정
        }

        [MenuItem("Project Epsilon/Day 1/Run Setup")] // 프로젝트·실행 설정
        public static void RunSetupFromMenu() // 실행·초기 구성 기능 선언
        {
            RunSetup(); // 실행·초기 구성 기능 실행
        }

        private static void RunAutoSetup() // 실행·초기 구성 기능 선언
        {
            // 플레이 모드 진입 중 자동 실행 방지
            if (EditorApplication.isPlayingOrWillChangePlaymode) // 에디터 조건 확인
            {
                return; // 현재 기능 종료
            }

            // 현재 에디터 세션 중복 실행 방지
            if (SessionState.GetBool(SessionKey, false)) // 상태·조회 조건 확인
            {
                return; // 현재 기능 종료
            }

            SessionState.SetBool(SessionKey, true); // 상태·설정 기능 실행
            RunSetup(); // 실행·초기 구성 기능 실행
        }

        private static void RunSetup() // 실행·초기 구성 기능 선언
        {
            EnsureProjectFolders(); // 확인·프로젝트 기능 실행

            // Game Scene 최초 생성
            if (!File.Exists(GameScenePath)) // 파일·게임 조건 확인
            {
                CreateGameScene(); // 생성·게임 기능 실행
            }

            EnsureBuildSettingsIncludesGameScene(); // 확인·구성 기능 실행
            AssetDatabase.SaveAssets(); // 저장·에셋 기능 실행
            AssetDatabase.Refresh(); // 갱신 기능 실행

            Debug.Log("[Project Epsilon] Day 1 setup complete."); // 프로젝트·초기 구성 기능 실행
        }

        private static void EnsureProjectFolders() // 확인·프로젝트 기능 선언
        {
            foreach (string folderPath in RequiredFolders) // 경로 항목 순회
            {
                EnsureFolder(folderPath); // 확인·경로 기능 실행
            }
        }

        private static void EnsureFolder(string folderPath) // 확인·경로 기능 선언
        {
            // 기존 폴더 유지
            if (AssetDatabase.IsValidFolder(folderPath)) // 경로 조건 확인
            {
                return; // 현재 기능 종료
            }

            string parentPath = Path.GetDirectoryName(folderPath)?.Replace('\\', '/'); // 부모·경로 값 설정
            string folderName = Path.GetFileName(folderPath); // 이름·경로 값 설정

            // 상위 폴더 우선 생성
            if (!string.IsNullOrEmpty(parentPath) && !AssetDatabase.IsValidFolder(parentPath)) // 빈 상태·부모 조건 확인
            {
                EnsureFolder(parentPath); // 확인·부모 기능 실행
            }

            if (!string.IsNullOrEmpty(parentPath)) // 빈 상태·부모 조건 확인
            {
                AssetDatabase.CreateFolder(parentPath, folderName); // 생성·부모 기능 실행
            }
        }

        private static void CreateGameScene() // 생성·게임 기능 선언
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // 씬·에디터 값 설정

            GameObject managers = CreateRoot("===Managers==="); // 게임·객체 기능 실행
            CreateRoot("===Gameplay==="); // 생성 기능 실행
            CreateRoot("===Environment==="); // 생성 기능 실행
            CreateRoot("===UI==="); // 생성·UI 기능 실행

            // 루트 관리자에 게임 관리자 연결
            managers.AddComponent<GameManager>(); // 추가·구성 요소 기능 실행

            // 2D 테스트 카메라 생성
            GameObject cameraObject = new GameObject("Main Camera"); // 게임·객체 값 설정
            Camera camera = cameraObject.AddComponent<Camera>(); // 객체·추가 값 설정
            cameraObject.AddComponent<AudioListener>(); // 객체·추가 기능 실행
            cameraObject.tag = "MainCamera"; // 객체 값 설정
            cameraObject.transform.position = new Vector3(0f, 0f, -10f); // 객체·위치 값 설정
            camera.orthographic = true; // 대상 값 설정
            camera.orthographicSize = 5f; // 크기 값 설정

            EditorSceneManager.MarkSceneDirty(scene); // 에디터·씬 기능 실행
            EditorSceneManager.SaveScene(scene, GameScenePath); // 에디터·씬 기능 실행

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(GameScenePath); // 선택·활성 값 설정
        }

        private static GameObject CreateRoot(string objectName) // 게임·객체 기능 선언
        {
            GameObject root = new GameObject(objectName); // 게임·객체 값 설정
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); // 위치·설정 기능 실행
            root.transform.localScale = Vector3.one; // 위치·크기 값 설정
            return root; // 대상 결과 반환
        }

        private static void EnsureBuildSettingsIncludesGameScene() // 확인·구성 기능 선언
        {
            EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes; // 에디터·구성 값 설정

            // 기존 등록 상태 유지
            if (currentScenes.Any(scene => scene.path == GameScenePath)) // 현재·씬 조건 확인
            {
                return; // 현재 기능 종료
            }

            EditorBuildSettings.scenes = currentScenes // 에디터·구성 값 설정
                .Concat(new[] { new EditorBuildSettingsScene(GameScenePath, true) }) // 에디터·구성 기능 실행
                .ToArray(); // 배열 기능 실행
        }
    }
}
