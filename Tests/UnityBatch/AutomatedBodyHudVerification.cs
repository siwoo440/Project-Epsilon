using System; // 예외 형식
using System.Reflection; // HUD 필드 조회
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Player; // 몸통 관리자
using ProjectEpsilon.UI; // HUD 구성
using UnityEditor; // 편집기 제어
using UnityEditor.SceneManagement; // 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.UI; // HUD 텍스트

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 갱신 연결
    public static class AutomatedBodyHudVerification // 실제 씬 몸통 HUD 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonBodyHudVerification"; // 진행 상태 표식
        private static double startWallTime; // 실제 시작 시간

        static AutomatedBodyHudVerification() // 도메인 연결
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 진행 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 검증 갱신 연결
            } // 조건 끝
        } // 생성자 끝

        public static void Run() // 배치 진입점
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 검증 시작 기록
            EditorApplication.update -= Tick; // 중복 연결 해제
            EditorApplication.update += Tick; // 검증 갱신 연결
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        } // 메서드 끝

        private static void Tick() // 상태별 검증 실행
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 시작 확인
            { // 조건 시작
                return; // 플레이 진입 대기
            } // 조건 끝
            if (startWallTime == 0d) // 최초 갱신 확인
            { // 조건 시작
                startWallTime = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
            } // 조건 끝
            try // 검증 오류 처리
            { // 보호 시작
                if (EditorApplication.timeSinceStartup - startWallTime > 45d) // 제한 시간 확인
                { // 조건 시작
                    throw new InvalidOperationException("Body HUD verification timed out."); // 시간 초과 보고
                } // 조건 끝
                GameManager game = GameManager.Instance; // 게임 관리자 조회
                SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 몸통 관리자 조회
                SnakeBodyHUDPresenter presenter = UnityEngine.Object.FindFirstObjectByType<SnakeBodyHUDPresenter>(); // 몸통 HUD 표시기 조회
                HUDController hud = UnityEngine.Object.FindFirstObjectByType<HUDController>(); // HUD 관리자 조회
                if (game == null || body == null || presenter == null || hud == null || !game.IsPlaying || Time.time < 0.35f) // 초기화와 물리 단계 확인
                { // 조건 시작
                    return; // 초기화 대기
                } // 조건 끝
                FieldInfo field = typeof(HUDController).GetField("bodyCountText", BindingFlags.Instance | BindingFlags.NonPublic); // 몸통 텍스트 필드
                Text label = field == null ? null : (Text)field.GetValue(hud); // 실제 화면 텍스트 조회
                Require(label != null && label.gameObject.activeInHierarchy, "Active body HUD label is missing."); // 실제 텍스트 연결 확인
                game.PauseGame(); // 적 간섭 방지를 위한 시험 일시정지
                Require(game.IsPaused && Mathf.Approximately(Time.timeScale, 0f), "Test pause did not activate."); // 시험 정지 확인
                int events = 0; // 몸통 변경 이벤트 수
                int lastCurrent = -1; // 최근 현재 몸통 수
                int lastMaximum = -1; // 최근 절대 상한
                int lastEarned = -1; // 최근 성장 상한
                body.BodyCountChanged += (current, maximum) => // 실제 이벤트 구독
                { // 이벤트 시작
                    events++; // 이벤트 수 증가
                    lastCurrent = current; // 현재 몸통 수 기록
                    lastMaximum = maximum; // 절대 상한 기록
                    lastEarned = body.EarnedBodyCount; // 성장 상한 기록
                }; // 이벤트 끝
                RequireState(body, label, 3, 3, 20, "initial"); // 시작 세 수치 및 HUD 확인
                Require(body.TryGainBodyFromLevelUp(), "Level growth failed."); // 실제 성장 실행
                RequireState(body, label, 4, 4, 20, "growth"); // 성장 후 세 수치 및 HUD 확인
                Require(events == 1 && lastCurrent == 4 && lastEarned == 4 && lastMaximum == 20, "Growth event mismatch."); // 성장 이벤트 확인
                Require(body.RemoveBodies(1) == 1, "Body loss failed."); // 실제 몸통 손실 실행
                RequireState(body, label, 3, 4, 20, "loss"); // 손실 후 세 수치 및 HUD 확인
                Require(events == 2 && lastCurrent == 3 && lastEarned == 4 && lastMaximum == 20, "Loss event mismatch."); // 손실 이벤트 확인
                Require(body.TryRepairBody(), "Body repair failed."); // 실제 몸통 복구 실행
                RequireState(body, label, 4, 4, 20, "repair"); // 복구 후 세 수치 및 HUD 확인
                Require(events == 3 && lastCurrent == 4 && lastEarned == 4 && lastMaximum == 20, "Repair event mismatch."); // 복구 이벤트 확인
                Require(body.RemoveBodies(1) == 1, "Body loss before reset failed."); // 성장 상한 초기화 준비
                RequireState(body, label, 3, 4, 20, "before earned reset"); // 초기화 전 상태 확인
                body.InitializeEarnedBodyCountFromCurrent(); // 현재 몸통 기준 성장 상한 초기화
                RequireState(body, label, 3, 3, 20, "earned reset"); // 성장 상한 초기화 표시 확인
                Require(events == 5 && lastCurrent == 3 && lastEarned == 3 && lastMaximum == 20, "Earned reset event mismatch."); // 성장 상한 변경 이벤트 확인
                body.InitializeEarnedBodyCountFromCurrent(); // 동일 성장 상한 재설정
                Require(events == 5, "Unchanged earned reset emitted an event."); // 불필요한 이벤트 차단 확인
                presenter.enabled = false; // 표시기 비활성화
                Require(body.TryGainBodyFromLevelUp(), "Growth while presenter disabled failed."); // 표시기 비활성 상태 성장
                Require(label.text == "Body 3 / 3 (max 20)", "Disabled presenter unexpectedly changed label."); // 비활성 상태 표시 유지 확인
                presenter.enabled = true; // 표시기 재활성화
                RequireState(body, label, 4, 4, 20, "presenter re-enable"); // 재활성화 즉시 갱신 확인
                Require(events == 6 && lastCurrent == 4 && lastEarned == 4 && lastMaximum == 20, "Disabled presenter growth event mismatch."); // 일반 구독자 이벤트 확인
                VerifyMaximumWidth(label); // 최대 길이 문구 너비 확인
                Debug.Log("[BodyHudVerification] PASS: initial, growth, loss, repair, earned reset, re-enable and events."); // 전체 성공 기록
                Finish(0); // 검증 성공 종료
            } // 보호 끝
            catch (Exception exception) // 검증 예외 확인
            { // 예외 시작
                Debug.LogError("[BodyHudVerification] " + exception); // 실패 상세 기록
                Finish(1); // 검증 실패 종료
            } // 예외 끝
        } // 메서드 끝

        private static void RequireState(SnakeBodyManager body, Text label, int current, int earned, int maximum, string stage) // 몸통 상태 및 HUD 확인
        { // 메서드 시작
            Require(body.CurrentBodyCount == current && body.EarnedBodyCount == earned && body.MaximumBodyCount == maximum, stage + ": body counts mismatch."); // 몸통 세 수치 확인
            string expected = $"Body {current} / {earned} (max {maximum})"; // 기획된 HUD 문구
            Require(label.text == expected, stage + ": expected '" + expected + "', got '" + label.text + "'."); // 실제 HUD 텍스트 확인
            Debug.Log("[BodyHudVerification] " + stage + ": " + label.text); // 단계별 실제 표시 기록
        } // 메서드 끝

        private static void VerifyMaximumWidth(Text label) // 최대 몸통 문구 너비 검증
        { // 메서드 시작
            string previousText = label.text; // 기존 표시 문구 저장
            try // 표시 문구 복구 보장
            { // 보호 시작
                label.text = "Body 20 / 20 (max 20)"; // 최대 길이 시험 문구
                Canvas.ForceUpdateCanvases(); // 화면 배치 강제 갱신
                float availableWidth = label.rectTransform.rect.width; // 실제 텍스트 영역 너비
                float preferredWidth = label.preferredWidth; // 폰트 기준 필요 너비
                Debug.Log($"[BodyHudVerification] max-width available={availableWidth:F2} preferred={preferredWidth:F2} font={(label.font == null ? "none" : label.font.name)}."); // 너비 측정 기록
                if (availableWidth <= 0f || preferredWidth <= 0f || label.font == null) // 배치 또는 폰트 측정 불가 확인
                { // 조건 시작
                    Debug.LogWarning("[BodyHudVerification] Max body text clipping is unverified: batch font or layout measurement unavailable."); // 너비 미검증 기록
                    return; // 측정 불가 종료
                } // 조건 끝
                Require(preferredWidth <= availableWidth, "Maximum body text exceeds HUD width."); // 너비 초과 확인
                Debug.Log("[BodyHudVerification] Max body text preferred width fits the HUD rectangle."); // 너비 비교 통과 기록
            } // 보호 끝
            finally // 텍스트 복구
            { // 정리 시작
                label.text = previousText; // 이전 표시 복구
            } // 정리 끝
        } // 메서드 끝

        private static void Require(bool condition, string message) // 검증 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 전달
            } // 조건 끝
        } // 메서드 끝

        private static void Finish(int code) // 검증 종료
        { // 메서드 시작
            SessionState.SetBool(ActiveKey, false); // 검증 상태 해제
            EditorApplication.update -= Tick; // 갱신 연결 해제
            EditorApplication.Exit(code); // 종료 결과 반환
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
