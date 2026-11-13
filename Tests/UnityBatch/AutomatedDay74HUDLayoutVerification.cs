using System; // 예외 처리
using System.Reflection; // 실제 HUD 필드 조회
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.UI; // 전투 HUD
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 전투 씬 열기
using UnityEngine; // Unity 기능
using UnityEngine.UI; // 실제 HUD 글자

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay74HUDLayoutVerification // 전투 HUD 배치 시험
    { // 시험 시작
        private const string Key = "EpsilonDay74HUDLayout"; // 시험 진행 표식
        private static bool started; // 중복 실행 방지
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 필드 조회
        static AutomatedDay74HUDLayoutVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투 준비 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 전투 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 전투 준비 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection()) // 시작 상태 확인
            { // 조건 시작
                return; // 전투 준비 대기
            } // 조건 종료
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 전투 준비 확인
            { // 조건 시작
                return; // 전투 준비 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            try // HUD 배치 시험
            { // 시험 시작
                Verify(); // 실제 HUD 배치 확인
                Finish(0, "Actual HUD text, bars and gameplay viewport measured."); // 성공 기록
            } // 시험 종료
            catch (Exception error) // 시험 실패 처리
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 화면 배치 검사
        { // 함수 시작
            HUDController hud = UnityEngine.Object.FindFirstObjectByType<HUDController>(); // 실제 HUD 제어기
            GameTimerUI timer = UnityEngine.Object.FindFirstObjectByType<GameTimerUI>(); // 실제 시간 표시기
            Require(hud != null && timer != null, "HUD 또는 타이머 누락"); // 실제 표시기 확인
            Canvas.ForceUpdateCanvases(); // 실제 UI 영역 계산
            Debug.Log("[Day74HUDLayout] Screen=" + Screen.width + "x" + Screen.height); // 실제 배치 화면 크기
            foreach (string name in new[] { "healthText", "bodyCountText", "experienceText", "levelText", "staminaText" }) // 실제 HUD 글자 순회
            { // 순회 시작
                Text text = typeof(HUDController).GetField(name, Private).GetValue(hud) as Text; // 실제 HUD 글자
                Require(text != null && text.gameObject.activeInHierarchy && text.font != null, "HUD 글자 누락: " + name); // 실제 표시 상태 확인
                Vector3[] corners = new Vector3[4]; // 글자 영역 모서리
                text.rectTransform.GetWorldCorners(corners); // 실제 화면 영역 조회
                Debug.Log("[Day74HUDLayout] " + name + " rect=" + corners[0] + " to " + corners[2] + " font=" + text.fontSize + " alignment=" + text.alignment + " preferred=" + text.preferredWidth + " text=" + text.text); // 실제 글자 배치 기록
            } // 순회 종료
            Text timerText = typeof(GameTimerUI).GetField("timerText", Private).GetValue(timer) as Text; // 실제 시간 글자
            Require(timerText != null && timerText.gameObject.activeInHierarchy, "시간 글자 누락"); // 실제 타이머 확인
            Vector3[] timerCorners = new Vector3[4]; // 시간 영역 모서리
            timerText.rectTransform.GetWorldCorners(timerCorners); // 실제 시간 화면 영역
            Debug.Log("[Day74HUDLayout] timer rect=" + timerCorners[0] + " to " + timerCorners[2] + " font=" + timerText.fontSize); // 실제 타이머 배치 기록
            foreach (string name in new[] { "HealthBar", "ExperienceBar", "StaminaBar" }) // 실제 게이지 순회
            { // 순회 시작
                GameObject bar = GameObject.Find(name); // 실제 게이지 배경
                Require(bar != null && bar.transform.Find("Fill") != null, "HUD 게이지 누락: " + name); // 게이지 구성 확인
            } // 순회 종료
            hud.SetLevel(200); // 높은 레벨 표시
            hud.SetExperience(10000000, 20000000); // 큰 경험치 표시
            Text level = (Text)typeof(HUDController).GetField("levelText", Private).GetValue(hud); // 실제 레벨 글자
            Text experience = (Text)typeof(HUDController).GetField("experienceText", Private).GetValue(hud); // 실제 경험치 글자
            Canvas.ForceUpdateCanvases(); // 변경된 글자 크기 계산
            float required = level.preferredWidth + experience.preferredWidth + 8f; // 양쪽 글자와 간격
            float available = Mathf.Min(level.rectTransform.rect.width, experience.rectTransform.rect.width); // 공통 HUD 너비
            Debug.Log("[Day74HUDLayout] stress required=" + required + " available=" + available + " fonts=" + level.fontSize + "," + experience.fontSize); // 긴 글자 측정 결과
            Require(required <= available + 0.5f, "긴 레벨과 경험치 글자가 겹침"); // 실제 글자 간격 확인
            RectTransform experienceBar = (RectTransform)GameObject.Find("ExperienceBar").transform; // 실제 경험치 막대 영역
            RectTransform fill = (RectTransform)experienceBar.Find("Fill"); // 실제 경험치 채움 영역
            Require(Mathf.Abs(experienceBar.rect.width - available) < 0.1f && Mathf.Abs(fill.anchorMax.x - 0.5f) < 0.001f, "긴 글자 조정 중 경험치 막대 변경"); // 전체 막대 너비와 비율 확인
            hud.SetLevel(1); // 일반 레벨 복귀
            hud.SetExperience(0, 10); // 일반 경험치 복귀
            Require(level.fontSize == 26 && experience.fontSize == 26, "일반 글자 크기 복원 실패"); // 기본 글자 크기 복원 확인
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 원인 전달
            } // 조건 종료
        } // 함수 종료
        private static void Finish(int code, string message) // 배치 결과 처리
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 전투 감시 해제
            if (code == 0) // 성공 여부 확인
            { // 조건 시작
                Debug.Log("[Day74HUDLayout] PASS: " + message); // 성공 결과 기록
            } // 조건 종료
            else // 실패 여부 확인
            { // 조건 시작
                Debug.LogError("[Day74HUDLayout] FAIL: " + message); // 실패 결과 기록
            } // 조건 종료
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
