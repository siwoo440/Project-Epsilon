using System; // 실제 예외 처리
using System.Reflection; // 실제 메뉴 새로고침 접근
using ProjectEpsilon.Core; // 실제 시작 상태
using ProjectEpsilon.Player; // 실제 시작 체력
using ProjectEpsilon.Progression; // 실제 영구 성장
using UnityEditor; // 실제 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 실제 도메인 갱신 처리
    public static class AutomatedDay48StartStatsVerification // 실제 시작 능력치 검증
    { // 클래스 시작
        private const string Key = "EpsilonDay48StartStats"; // 실제 시험 표식
        private static double started; // 실제 시험 시작 시각
        static AutomatedDay48StartStatsVerification() // 실제 도메인 재시작
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 실제 진행 여부 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 시험 감시
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 배치 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(Key, true); // 실제 시험 진행 기록
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        } // 메서드 끝
        private static void Tick() // 실제 시작 화면 검사
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            { // 조건 시작
                return; // 실제 진입 대기
            } // 조건 끝
            if (started <= 0d) // 실제 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 60d) // 실제 제한 시간 확인
            { // 조건 시작
                Finish(1, "Timed out."); // 실제 시간 초과
                return; // 추가 검사 중단
            } // 조건 끝
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 진행 관리자
            CharacterRunInitializer initializer = UnityEngine.Object.FindFirstObjectByType<CharacterRunInitializer>(); // 실제 캐릭터 초기화기
            SnakeHealth health = UnityEngine.Object.FindFirstObjectByType<SnakeHealth>(); // 실제 체력
            SnakeStamina stamina = UnityEngine.Object.FindFirstObjectByType<SnakeStamina>(); // 실제 스태미나
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Starting || meta == null || initializer == null || health == null || stamina == null || CharacterRunInitializer.ActiveCharacter == null) // 실제 시작 구성 확인
            { // 조건 시작
                return; // 실제 초기화 대기
            } // 조건 끝
            try // 실제 능력치 검사
            { // 보호 영역 시작
                if (Application.productName != "Project-Epsilon-Day48-Isolated") // 실제 사용자 저장 격리 확인
                { // 조건 시작
                    throw new InvalidOperationException("Non-isolated project."); // 실제 시험 중단
                } // 조건 끝
                MetaProgressionService service = new MetaProgressionService(new Day48MemoryStorage()); // 실제 메모리 성장 서비스
                if (!service.TryCompleteRun("stats", 12000d, 0, 0, false, 0) || !service.TryPurchase(MetaNode.Health) || !service.TryPurchase(MetaNode.Turn) || !service.TryPurchase(MetaNode.Stamina)) // 실제 시작 성장 구매
                { // 조건 시작
                    throw new InvalidOperationException("Could not buy starting stats."); // 실제 구매 실패
                } // 조건 끝
                typeof(MetaProgressionController).GetProperty("Service").SetValue(meta, service); // 실제 시험 성장 적용
                typeof(MetaProgressionMenu).GetMethod("RefreshStartingStats", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null); // 실제 성장 메뉴 능력치 갱신
                int expected = Mathf.RoundToInt(CharacterRunInitializer.ActiveCharacter.BodyMaxHealth * 1.04f); // 실제 캐릭터 기준 성장 체력
                if (health.MaximumHealth != expected || health.CurrentHealth != expected || Mathf.Abs(stamina.CurrentStamina - 105f) > 0.001f) // 실제 시작 체력과 스태미나 확인
                { // 조건 시작
                    throw new InvalidOperationException($"Start stats incorrect: hp={health.CurrentHealth}/{health.MaximumHealth}, stamina={stamina.CurrentStamina}."); // 실제 적용 실패
                } // 조건 끝
                if (!service.TryRefundAll()) // 실제 전체 성장 환불
                { // 조건 시작
                    throw new InvalidOperationException("Refund failed."); // 실제 환불 실패
                } // 조건 끝
                typeof(MetaProgressionMenu).GetMethod("RefreshStartingStats", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null); // 실제 환불 능력치 갱신
                if (health.MaximumHealth != Mathf.RoundToInt(CharacterRunInitializer.ActiveCharacter.BodyMaxHealth) || Mathf.Abs(stamina.CurrentStamina - 100f) > 0.001f) // 실제 환불 능력치 확인
                { // 조건 시작
                    throw new InvalidOperationException("Refunded start stats incorrect."); // 실제 복원 실패
                } // 조건 끝
                Finish(0, "Actual start-screen purchase refresh and refund stats passed."); // 실제 시험 성공
            } // 보호 영역 끝
            catch (Exception error) // 실제 시험 실패 확인
            { // 예외 시작
                Finish(1, error.ToString()); // 실제 실패 기록
            } // 예외 끝
        } // 메서드 끝
        private static void Finish(int code, string message) // 실제 시험 결과 기록
        { // 메서드 시작
            SessionState.SetBool(Key, false); // 실제 시험 표식 제거
            EditorApplication.update -= Tick; // 실제 감시 해제
            Debug.Log($"[Day48StartStatsVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 종료 코드
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
