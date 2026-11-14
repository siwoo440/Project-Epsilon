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
    public static class AutomatedDay139ScoreVerification // 전투 HUD 배치 시험
    { // 시험 시작
        private const string Key = "EpsilonDay139Score"; // 시험 진행 표식
        private static bool started; // 중복 실행 방지
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 필드 조회
        static AutomatedDay139ScoreVerification() // 도메인 재시작 처리
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
                Finish(0, "Score accumulation, configurable thresholds and victory finalization verified."); // 성공 기록
            } // 시험 종료
            catch (Exception error) // 시험 실패 처리
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 점수 집계 검사
        { // 함수 시작
            ProjectEpsilon.Progression.MetaProgressionController meta = ProjectEpsilon.Progression.MetaProgressionController.Instance; // 실제 전투 점수 관리자
            ProjectEpsilon.Progression.SnakeExperience experience = ProjectEpsilon.Progression.SnakeExperience.Current; // 실제 경험치 관리자
            Require(meta != null && experience != null, "점수 또는 경험치 관리자 누락"); // 실제 관리자 확인
            ProjectEpsilon.Progression.RunScoreSettings settings = Resources.Load<ProjectEpsilon.Progression.RunScoreSettings>("RunScoreSettings"); // 실제 점수 설정
            Require(settings != null && settings.ExperienceTarget == 0 && settings.NormalKillTarget == 0, "미정 점수 기준 확인 실패"); // 미정 기준 유지 확인
            experience.Configure(1000000, 1f); // 시험 중 레벨업 방지
            experience.AddExperience(25); // 실제 경험치 획득
            MethodInfo enemyDied = meta.GetType().GetMethod("HandleEnemyDied", Private); // 실제 처치 이벤트 처리기
            enemyDied.Invoke(meta, new object[] { false }); // 일반 적 처치 처리
            Require(meta.TotalExperience >= 25 && meta.CurrentScore == 0, "미정 기준에서 점수 반영"); // 경험치 집계와 미정 점수 확인
            enemyDied.Invoke(meta, new object[] { true }); // 엘리트 처치 처리
            Require(meta.CurrentScore == 2500, "엘리트 점수 오류"); // 엘리트 점수 확인
            Text score = GameObject.Find("CurrentScoreText").GetComponent<Text>(); // 실제 현재 점수 글자
            Text gain = GameObject.Find("ScoreGainText").GetComponent<Text>(); // 실제 증가량 글자
            Require(score.text.Contains("2,500") && gain.text.Contains("2,500"), "점수 또는 증가량 표시 오류"); // 실제 HUD 반영 확인
            typeof(ProjectEpsilon.Progression.RunScoreSettings).GetField("experienceTarget", Private).SetValue(settings, 1000); // 시험용 경험치 기준 설정
            typeof(ProjectEpsilon.Progression.RunScoreSettings).GetField("normalKillTarget", Private).SetValue(settings, 100); // 시험용 일반 처치 기준 설정
            experience.AddExperience(25); // 실제 경험치 획득과 기준 반영
            int expected = (int)Math.Floor(Math.Min(1d, meta.TotalExperience / 1000d) * 20000d) + 100 + 2500; // 시험 기준의 현재 점수
            Require(meta.CurrentScore == expected, "경험치 또는 일반 처치 점수 오류"); // 실제 누적 점수 확인
            ProjectEpsilon.Player.SnakeHealth health = UnityEngine.Object.FindFirstObjectByType<ProjectEpsilon.Player.SnakeHealth>(); // 실제 공용 체력
            ProjectEpsilon.Player.SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<ProjectEpsilon.Player.SnakeBodyManager>(); // 실제 몸통
            int bonus = 40000 + (int)Math.Floor(10000d * health.CurrentHealth / health.MaximumHealth) + (int)Math.Floor(10000d * body.CurrentBodyCount / body.EarnedBodyCount); // 실제 승리 보너스
            Require(meta.CompleteRun(true) && meta.ScoreFinalized && meta.CurrentScore == expected + bonus, "승리 점수 확정 오류"); // 실제 승리 점수 확인
            Require(meta.CompleteRun(true) && meta.CurrentScore == expected + bonus, "승리 점수 중복 지급"); // 종료 재시도 중복 방지 확인
            meta.BeginRun(); // 새 전투 점수 초기화
            Require(meta.CurrentScore == 0 && meta.TotalExperience == 0 && !meta.ScoreFinalized, "새 전투 초기화 오류"); // 이전 전투 점수 제거 확인
            Require(meta.CompleteRun(false) && !meta.ScoreFinalized && meta.CurrentScore == 0, "패배에 승리 점수 반영"); // 패배 승리 보너스 제외 확인
        } // 함수 종료
        private static Vector2 GlyphHeight(Text label) // 실제 보이는 글자 높이 범위
        { // 함수 시작
            var vertices = label.cachedTextGenerator.verts; // 실제 생성 글자 꼭짓점
            float minimum = float.PositiveInfinity; // 실제 글자 아래쪽 초기값
            float maximum = float.NegativeInfinity; // 실제 글자 위쪽 초기값
            for (int index = 0; index + 3 < vertices.Count - 4; index += 4) // 실제 글자별 꼭짓점 순회
            { // 반복 시작
                float bottom = float.PositiveInfinity; // 실제 글자 아래쪽
                float top = float.NegativeInfinity; // 실제 글자 위쪽
                for (int corner = 0; corner < 4; corner++) // 실제 글자 꼭짓점 순회
                { // 반복 시작
                    Vector3 local = vertices[index + corner].position / label.pixelsPerUnit; // 실제 글자 로컬 위치
                    float y = label.rectTransform.TransformPoint(local).y; // 실제 화면의 글자 높이
                    bottom = Mathf.Min(bottom, y); // 실제 글자 아래쪽 갱신
                    top = Mathf.Max(top, y); // 실제 글자 위쪽 갱신
                } // 반복 종료
                if (top > bottom) // 공백이 아닌 실제 글자 확인
                { // 조건 시작
                    minimum = Mathf.Min(minimum, bottom); // 전체 글자 아래쪽 갱신
                    maximum = Mathf.Max(maximum, top); // 전체 글자 위쪽 갱신
                } // 조건 종료
            } // 반복 종료
            Require(!float.IsInfinity(minimum) && !float.IsInfinity(maximum), "실제 HUD 글자 꼭짓점 누락: " + label.name); // 실제 보이는 글자 확인
            return new Vector2(minimum, maximum); // 실제 글자 높이 범위 반환
        } // 함수 종료
        private static Vector2 RectHeight(RectTransform rect) // 실제 UI 영역 높이 범위
        { // 함수 시작
            Vector3[] corners = new Vector3[4]; // 실제 UI 영역 모서리
            rect.GetWorldCorners(corners); // 실제 화면 좌표 계산
            return new Vector2(corners[0].y, corners[1].y); // 실제 아래와 위 높이 반환
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
                Debug.Log("[Day139Score] PASS: " + message); // 성공 결과 기록
            } // 조건 종료
            else // 실패 여부 확인
            { // 조건 시작
                Debug.LogError("[Day139Score] FAIL: " + message); // 실패 결과 기록
            } // 조건 종료
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
