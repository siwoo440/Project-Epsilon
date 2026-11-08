using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 시간 필드 조회
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 전투 시간
using ProjectEpsilon.Player; // 실제 플레이어 조회
using ProjectEpsilon.UI; // 실제 시간 표시
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedRunClockVerification // 실제 전투 시계 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonRunClockVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지
        private static double started; // 실제 시작 시각
        static AutomatedRunClockVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 시험 진행 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 시작 감시
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 씬 배치 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 메서드 끝
        private static void Tick() // 실제 시작 화면과 결과 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 진입 대기
            } // 조건 끝
            if (started <= 0d) // 시험 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 시간 제한
            { // 조건 시작
                Finish(1, "Timed out waiting for actual run clock test."); // 제한 시간 실패
                return; // 감시 종료
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 시작 선택과 실행 상태 확인
            { // 조건 시작
                return; // 시작 또는 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 씬 초기화 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 중복 실행 방지
            player.gameObject.AddComponent<RunClockVerificationRunner>(); // 실제 시계 시험 실행
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 결과 기록과 종료
        { // 메서드 시작
            if (finished) // 중복 종료 확인
            { // 조건 시작
                return; // 중복 처리 방지
            } // 조건 끝
            finished = true; // 종료 상태 저장
            SessionState.SetBool(ActiveKey, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log($"[RunClockVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 결과 기록
            EditorApplication.Exit(code); // 실제 프로세스 종료
        } // 메서드 끝
    } // 클래스 끝

    [DefaultExecutionOrder(-200)] // 출현기 이전 정지 요청
    public sealed class RunClockPauseProbe : MonoBehaviour // 같은 프레임 정지 관찰기
    { // 클래스 시작
        public EarlyEnemyWaveSpawner Spawner; // 실제 전투 시계
        public bool Requested; // 정지 요청 상태
        public bool Captured; // 실제 정지 기록 여부
        public float PreviousElapsed; // 이전 전투 경과 시간
        public double FrameDelta; // 정지 전에 진행한 게임 시간
        private void Update() // 실제 프레임 초반 일시정지
        { // 메서드 시작
            if (!Requested || Captured) // 요청과 중복 확인
            { // 조건 시작
                return; // 실제 정지 대기
            } // 조건 끝
            FieldInfo field = typeof(EarlyEnemyWaveSpawner).GetField("previousClockTime", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 시계 표본 조회
            PreviousElapsed = Spawner.ElapsedSeconds; // 이전 프레임 전투 시간
            FrameDelta = Time.timeAsDouble - (double)field.GetValue(Spawner); // 이번 프레임 실제 시간 차이
            Captured = true; // 실제 정지 기록 완료
            GameManager.Instance.PauseGame(); // 실제 게임 일시정지
        } // 메서드 끝
    } // 클래스 끝

    public sealed class RunClockVerificationRunner : MonoBehaviour // 실제 시계 시나리오
    { // 클래스 시작
        private static readonly FieldInfo DisplayField = typeof(GameTimerUI).GetField("elapsedSeconds", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 HUD 시간 조회
        private IEnumerator Start() // 실제 정지와 표시 시간 시험
        { // 코루틴 시작
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 전투 시계 조회
            GameTimerUI display = UnityEngine.Object.FindFirstObjectByType<GameTimerUI>(); // 실제 시간 표시 조회
            Require(spawner != null && display != null && DisplayField != null, "Actual scene clock fixtures missing."); // 실제 씬 구성 확인
            GetComponent<SnakeMovement>().enabled = false; // 실제 플레이어 이동 통제
            RunClockPauseProbe probe = gameObject.AddComponent<RunClockPauseProbe>(); // 실제 정지 관찰기 생성
            probe.Spawner = spawner; // 실제 전투 시계 연결
            yield return null; // 이전 시계 프레임 완료 대기
            probe.Requested = true; // 다음 프레임 정지 요청
            while (!probe.Captured) // 실제 정지 대기
            { // 반복 시작
                yield return null; // 실제 프레임 진행
            } // 반복 끝
            yield return null; // 실제 후반 시간 표시 완료 대기
            float stopped = spawner.ElapsedSeconds; // 실제 정지 경과 시간
            float displayed = (float)DisplayField.GetValue(display); // 실제 HUD 경과 시간
            Require(probe.FrameDelta > 0d && Math.Abs(stopped - probe.PreviousElapsed - probe.FrameDelta) < 0.002d, "Frame time preceding pause was lost."); // 같은 프레임 경과 시간 보존 확인
            Require(Math.Abs(stopped - displayed) < 0.001f && GameManager.Instance.IsPaused, "HUD and actual clock diverged at pause."); // 같은 프레임 HUD 일치 확인
            yield return new WaitForSecondsRealtime(0.3f); // 실제 시간 경과 대기
            Require(Math.Abs(spawner.ElapsedSeconds - stopped) < 0.001f && Math.Abs((float)DisplayField.GetValue(display) - displayed) < 0.001f, "Clock advanced during actual pause."); // 실제 정지 지속 확인
            display.ResetTimer(); // 실제 표시 전용 초기화
            Require(Math.Abs(spawner.ElapsedSeconds - stopped) < 0.001f && Math.Abs((float)DisplayField.GetValue(display)) < 0.001f, "Display reset changed the run clock."); // 실제 표시 전용 초기화 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            yield return new WaitForSecondsRealtime(0.3f); // 실제 재개 시간 진행
            yield return null; // 실제 후반 표시 완료 대기
            float resumed = spawner.ElapsedSeconds; // 실제 재개 경과 시간
            float afterReset = (float)DisplayField.GetValue(display); // 실제 초기화 이후 표시 시간
            Require(resumed > stopped && Math.Abs(afterReset - (resumed - stopped)) < 0.002f, "Actual resume or display offset failed."); // 실제 재개와 표시 오프셋 확인
            Debug.Log($"[RunClockVerification] frameDelta={probe.FrameDelta:F6}, paused={stopped:F6}, resumed={resumed:F6}, displayAfterReset={afterReset:F6}."); // 실제 측정 결과
            AutomatedRunClockVerification.Finish(0, "Actual same-frame pause, frozen clock, display reset and resume passed."); // 실제 전체 결과
        } // 코루틴 끝
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실제 실패 조건 확인
            { // 조건 시작
                AutomatedRunClockVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 실행 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
