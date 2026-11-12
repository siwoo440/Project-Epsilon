using System; // 실제 예외 형식
using System.Linq; // 실제 빌드 씬 검색
using UnityEditor; // 실제 씬 에셋과 빌드 설정
using UnityEngine; // 실제 로그 출력

namespace ProjectEpsilon.Editor // 실제 배치 시험 영역
{
    public static class AutomatedSceneAssetsVerification // 실제 분리 씬 구성 시험
    {
        public static void Run() // 실제 배치 진입점
        {
            try // 실제 씬 검증
            {
                string[] paths = // 실제 화면 순서
                {
                    "Assets/Scenes/MainMenu.unity", // 실제 메인 메뉴 씬
                    "Assets/Scenes/Preparation.unity", // 실제 출격 준비 씬
                    "Assets/Scenes/Game.unity", // 실제 기존 전투 씬
                    "Assets/Scenes/Results.unity" // 실제 결과 씬
                };
                foreach (string path in paths) // 실제 씬 순회
                {
                    Require(AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null, "Scene missing: " + path); // 실제 씬 에셋 확인
                }
                string[] enabled = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(); // 실제 빌드 대상 목록
                Require(enabled.SequenceEqual(paths), "Enabled build scene order mismatch."); // 실제 첫 화면과 전투 순서 확인
                Debug.Log("[SceneAssetsVerification] PASS: four actual scenes and build order."); // 실제 시험 성공 기록
                EditorApplication.Exit(0); // 실제 성공 종료
            }
            catch (Exception error) // 실제 시험 실패 확인
            {
                Debug.LogError("[SceneAssetsVerification] FAIL: " + error); // 실제 실패 원인 기록
                EditorApplication.Exit(1); // 실제 실패 종료
            }
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 조건 실패 확인
            {
                throw new InvalidOperationException(message); // 실제 시험 중단
            }
        }
    }
}
