using System; // 실제 예외 형식
using System.Linq; // 실제 빌드 항목 확인
using ProjectEpsilon.Core; // 실제 입력 관리자
using ProjectEpsilon.Data; // 실제 캐릭터 데이터
using ProjectEpsilon.Progression; // 실제 성장 관리자
using ProjectEpsilon.UI; // 실제 화면 역할
using ProjectEpsilon.World; // 실제 맵 데이터
using UnityEditor; // 실제 씬 설정과 에셋 조회
using UnityEditor.SceneManagement; // 실제 씬 생성과 저장
using UnityEngine; // 실제 씬 객체 생성
using UnityEngine.EventSystems; // 실제 버튼 이벤트
using UnityEngine.InputSystem.UI; // 실제 입력 시스템 UI
using UnityEngine.SceneManagement; // 실제 씬 형식

namespace ProjectEpsilon.Editor // 실제 배치 설정 영역
{
    public static class ConfigureSceneFlow // 실제 화면별 씬 생성기
    {
        public static void Run() // 실제 Unity 배치 진입점
        {
            try // 실제 씬 생성 실패 처리
            {
                CreateScene(SceneFrontendKind.MainMenu, SceneFrontendContext.MainMenuPath); // 실제 메인 메뉴 씬 생성
                CreateScene(SceneFrontendKind.Preparation, SceneFrontendContext.PreparationPath); // 실제 출격 준비 씬 생성
                CreateScene(SceneFrontendKind.Results, SceneFrontendContext.ResultsPath); // 실제 결과 씬 생성
                EditorBuildSettingsScene[] previous = EditorBuildSettings.scenes; // 실제 기존 빌드 목록
                EditorBuildSettingsScene[] ordered = // 실제 기본 화면 순서
                {
                    new EditorBuildSettingsScene(SceneFrontendContext.MainMenuPath, true), // 실제 첫 메뉴
                    new EditorBuildSettingsScene(SceneFrontendContext.PreparationPath, true), // 실제 출격 준비
                    new EditorBuildSettingsScene(SceneFrontendContext.GamePath, true), // 실제 기존 전투
                    new EditorBuildSettingsScene(SceneFrontendContext.ResultsPath, true) // 실제 결과 화면
                };
                EditorBuildSettings.scenes = ordered.Concat(previous.Where(scene => !ordered.Any(item => item.path == scene.path)).Select(scene => new EditorBuildSettingsScene(scene.path, false))).ToArray(); // 기존 기타 씬 보존과 비활성화
                AssetDatabase.SaveAssets(); // 실제 새 에셋 저장
                Debug.Log("[ConfigureSceneFlow] PASS: independent menu, preparation and result scenes created."); // 실제 씬 생성 결과
                EditorApplication.Exit(0); // 실제 성공 종료
            }
            catch (Exception error) // 실제 생성 실패 확인
            {
                Debug.LogError("[ConfigureSceneFlow] FAIL: " + error); // 실제 실패 원인 기록
                EditorApplication.Exit(1); // 실제 실패 종료
            }
        }
        private static void CreateScene(SceneFrontendKind kind, string path) // 실제 화면 역할별 씬 구성
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // 실제 독립 빈 씬 생성
            GameObject root = new GameObject("SceneFrontend"); // 실제 화면 제어 객체
            SceneFrontendContext context = root.AddComponent<SceneFrontendContext>(); // 실제 씬별 데이터
            SerializedObject data = new SerializedObject(context); // 실제 직렬화 필드 편집
            data.FindProperty("kind").enumValueIndex = (int)kind; // 실제 씬 역할 지정
            SetArray(data.FindProperty("characters"), new[] // 실제 여섯 캐릭터 후보
            {
                "Assets/Data/Characters/PrototypeBalanced.asset", // 실제 균형형
                "Assets/Data/Characters/PrototypeMobile.asset", // 실제 기동형
                "Assets/Data/Characters/PrototypeHeavy.asset", // 실제 중장형
                "Assets/Data/Characters/PrototypeFusion.asset", // 실제 합성형
                "Assets/Data/Characters/PrototypeHoly.asset", // 실제 신성형
                "Assets/Data/Characters/PrototypeDark.asset" // 실제 암흑형
            }); // 실제 캐릭터 후보 배열 연결
            SetArray(data.FindProperty("maps"), new[] // 실제 세 맵 후보
            {
                "Assets/Data/World/PrototypeMapPlains.asset", // 실제 평원
                "Assets/Data/World/PrototypeMapRuins.asset", // 실제 폐허
                "Assets/Data/World/PrototypeMapSwamp.asset" // 실제 늪지
            }); // 실제 맵 후보 배열 연결
            data.FindProperty("headSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/PlayerHead_Casual.png"); // 실제 머리 이미지
            data.FindProperty("bodySprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/PlayerBody_Casual.png"); // 실제 몸통 이미지
            data.ApplyModifiedPropertiesWithoutUndo(); // 실제 씬 데이터 반영
            root.AddComponent<RunFrontendController>(); // 실제 기존 화면 제어기
            if (kind != SceneFrontendKind.Results) // 실제 저장과 입력이 필요한 화면
            {
                root.AddComponent<InputBindingManager>(); // 실제 입력 설정 관리자
                MetaProgressionController meta = root.AddComponent<MetaProgressionController>(); // 실제 영구 성장 관리자
                SerializedObject saved = new SerializedObject(meta); // 실제 해금 설정 편집
                saved.FindProperty("unlockConfig").objectReferenceValue = AssetDatabase.LoadAssetAtPath<MetaUnlockConfig>("Assets/Data/Progression/PrototypeMetaUnlockConfig.asset"); // 실제 해금 데이터 연결
                saved.ApplyModifiedPropertiesWithoutUndo(); // 실제 해금 참조 반영
            }
            GameObject events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)); // 실제 UI 버튼 입력 구성
            SceneManager.MoveGameObjectToScene(events, scene); // 실제 대상 씬에 입력 객체 배치
            if (!EditorSceneManager.SaveScene(scene, path)) // 실제 씬 저장 확인
            {
                throw new InvalidOperationException("Scene could not be saved: " + path); // 실제 저장 실패 중단
            }
        }
        private static void SetArray(SerializedProperty property, string[] paths) // 실제 에셋 참조 배열 설정
        {
            property.arraySize = paths.Length; // 실제 후보 수 지정
            for (int index = 0; index < paths.Length; index++) // 실제 후보 경로 순회
            {
                UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(paths[index]); // 실제 에셋 조회
                if (asset == null) // 실제 에셋 누락 확인
                {
                    throw new InvalidOperationException("Asset missing: " + paths[index]); // 실제 구성 누락 중단
                }
                property.GetArrayElementAtIndex(index).objectReferenceValue = asset; // 실제 씬 에셋 연결
            }
        }
    }
}
