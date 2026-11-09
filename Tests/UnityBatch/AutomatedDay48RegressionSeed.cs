using ProjectEpsilon.Progression; // 실제 영구 성장
using UnityEditor; // 실제 배치 종료
using UnityEngine; // 실제 격리 프로젝트

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    public static class AutomatedDay48RegressionSeed // 기존 회귀 전용 해금
    { // 클래스 시작
        public static void Run() // 실제 배치 진입
        { // 메서드 시작
            if (Application.productName != "Project-Epsilon-Day48-Isolated") // 실제 사용자 저장 격리 확인
            { // 조건 시작
                Debug.LogError("[Day48RegressionSeed] FAIL: Project is not isolated."); // 격리 오류 기록
                EditorApplication.Exit(1); // 실제 시험 중단
                return; // 추가 처리 차단
            } // 조건 끝
            MetaProgressionService service = new MetaProgressionService(new PlayerPrefsMetaStorage("ProjectEpsilon.MetaProgress.v1")); // 격리된 진행 로드
            bool valid = service.IsAvailable && service.TryCompleteRun("regression-plains", 0d, 0, 0, true, 0) && service.TryCompleteRun("regression-ruins", 0d, 0, 0, true, 1); // 기존 맵과 캐릭터 해금
            for (int index = 0; index < 8 && valid; index++) // 실제 시험용 포탑 순회
            { // 반복 시작
                valid = service.TryUnlockTurretAttribute(index); // 기존 회귀용 포탑 해금
            } // 반복 끝
            Debug.Log($"[Day48RegressionSeed] {(valid ? "PASS" : "FAIL")}: Isolated regression unlocks."); // 실제 시험 준비 결과
            EditorApplication.Exit(valid ? 0 : 1); // 실제 종료 코드
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
