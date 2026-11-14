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
    public static class AutomatedDay141ResultVerification // 전투 HUD 배치 시험
    { // 시험 시작
        private const string Key = "EpsilonDay141Result"; // 시험 진행 표식
        private static bool started; // 중복 실행 방지
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 필드 조회
        static AutomatedDay141ResultVerification() // 도메인 재시작 처리
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
                Finish(0, "Score details, victory and defeat displays, stamina recovery verified."); // 성공 기록
            } // 시험 종료
            catch (Exception error) // 시험 실패 처리
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 결과 점수와 스태미나 회복 시험
        { // 함수 시작
            ProjectEpsilon.Player.SnakeStamina stamina = UnityEngine.Object.FindFirstObjectByType<ProjectEpsilon.Player.SnakeStamina>(); // 실제 스태미나 관리자
            Require(stamina != null, "스태미나 관리자 누락"); // 실제 스태미나 확인
            float multiplier = ProjectEpsilon.Player.CharacterRunInitializer.StaminaRecoveryMultiplier; // 실제 캐릭터 회복 배율
            Require(Mathf.Abs(stamina.EffectiveRecoveryPerSecond - 10f * multiplier) < 0.01f, "스태미나 회복이 절반이 아님"); // 실제 씬의 절반 회복 확인
            float maximum = stamina.MaximumStamina; // 실제 최대 스태미나 보존
            typeof(ProjectEpsilon.Player.SnakeStamina).GetField("currentStamina", Private).SetValue(stamina, 50f); // 실제 회복 전 스태미나 설정
            typeof(ProjectEpsilon.Player.SnakeStamina).GetMethod("TickRecovery", Private).Invoke(stamina, null); // 실제 회복 처리
            Require(Mathf.Abs(stamina.CurrentStamina - (50f + 10f * multiplier * Time.deltaTime)) < 0.01f && stamina.MaximumStamina == maximum, "실제 회복량 또는 최대치 오류"); // 실제 회복량과 최대치 확인
            Texture2D originalTexture = new Texture2D(32, 32); // 기존 아이템 크기 기준 이미지
            Sprite original = Sprite.Create(originalTexture, new Rect(0f, 0f, 26f, 26f), new Vector2(0.5f, 0.5f), 32f); // 기존 아이템 크기 기준 스프라이트
            ProjectEpsilon.Progression.RecoveryPickup healPickup = ProjectEpsilon.Progression.RecoveryPickup.Spawn(new Vector3(100f, 100f), null, null, null, ProjectEpsilon.Progression.RecoveryPickupType.Heal, 15, original); // 실제 체력 회복 아이템
            ProjectEpsilon.Progression.RecoveryPickup bodyPickup = ProjectEpsilon.Progression.RecoveryPickup.Spawn(new Vector3(100f, 101f), null, null, null, ProjectEpsilon.Progression.RecoveryPickupType.BodyRepair, 1, original); // 실제 몸통 복구 아이템
            SpriteRenderer healImage = healPickup.GetComponent<SpriteRenderer>(); // 실제 체력 아이템 그림
            SpriteRenderer bodyImage = bodyPickup.GetComponent<SpriteRenderer>(); // 실제 몸통 아이템 그림
            Require(healImage.sprite != null && bodyImage.sprite != null && healImage.sprite != original && bodyImage.sprite != original && healImage.sprite != bodyImage.sprite, "회복 아이템별 새 이미지 누락"); // 두 회복 이미지 구분 확인
            Require(Mathf.Abs(healImage.bounds.size.x - original.bounds.size.x * 0.27f * 4f) < 0.01f && Mathf.Abs(bodyImage.bounds.size.x - original.bounds.size.x * 0.31f * 4f) < 0.01f, "회복 아이템 표시 크기가 네 배가 아님"); // 기존 이미지 대비 실제 표시 너비 확인
            Require(healImage.color == Color.white && bodyImage.color == Color.white, "새 아이템 이미지 색상 변형"); // 생성 이미지 원본 색상 유지 확인
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null) // 실제 그래픽 장치 확인
            { // 조건 시작
                ProjectEpsilon.Player.SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<ProjectEpsilon.Player.SnakeMovement>(); // 실제 플레이어 위치
                healPickup.transform.position = player.transform.position + new Vector3(-1.25f, 0.4f); // 체력 아이템의 화면 표시 위치
                bodyPickup.transform.position = player.transform.position + new Vector3(1.25f, 0.4f); // 몸통 아이템의 화면 표시 위치
                typeof(AutomatedDay139ScreenCapture).GetMethod("Capture", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { "day141-pickups.png" }); // 실제 회복 아이템 화면 캡처
            } // 조건 종료
            UnityEngine.Object.Destroy(healPickup.gameObject); // 시험 아이템 제거
            UnityEngine.Object.Destroy(bodyPickup.gameObject); // 시험 아이템 제거
            UnityEngine.Object.Destroy(original); // 시험 스프라이트 제거
            UnityEngine.Object.Destroy(originalTexture); // 시험 이미지 제거
            ProjectEpsilon.Progression.MetaProgressionController meta = ProjectEpsilon.Progression.MetaProgressionController.Instance; // 실제 전투 점수 관리자
            ProjectEpsilon.Progression.SnakeExperience experience = ProjectEpsilon.Progression.SnakeExperience.Current; // 실제 경험치 관리자
            Require(meta != null && experience != null, "점수 또는 경험치 관리자 누락"); // 실제 관리자 확인
            experience.Configure(1000000, 1f); // 시험 중 레벨업 방지
            experience.AddExperience(25); // 실제 경험치 획득
            MethodInfo enemyDied = meta.GetType().GetMethod("HandleEnemyDied", Private); // 실제 처치 처리기
            enemyDied.Invoke(meta, new object[] { false }); // 일반 적 처치 집계
            enemyDied.Invoke(meta, new object[] { true }); // 엘리트 처치 집계
            Require(meta.CurrentScore == 2500, "미정 기준 또는 엘리트 점수 오류"); // 기존 점수 계산 확인
            ProjectEpsilon.Player.SnakeHealth health = UnityEngine.Object.FindFirstObjectByType<ProjectEpsilon.Player.SnakeHealth>(); // 실제 플레이어 체력
            ProjectEpsilon.Player.SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<ProjectEpsilon.Player.SnakeBodyManager>(); // 실제 몸통 관리자
            Require(health.TakeDamage(25), "시험 체력 피해 실패"); // 실제 남은 체력 변경
            int expectedHealth = (int)Math.Floor(10000d * health.CurrentHealth / health.MaximumHealth); // 실제 남은 체력 점수
            int expectedBody = (int)Math.Floor(10000d * body.CurrentBodyCount / body.EarnedBodyCount); // 실제 남은 몸통 점수
            GameObject bossObject = new GameObject("ScoreTestBoss"); // 시험용 최종 보스
            ProjectEpsilon.Combat.WeaponTarget boss = bossObject.AddComponent<ProjectEpsilon.Combat.WeaponTarget>(); // 실제 전투 대상 구성
            boss.Configure(1f); // 시험용 보스 체력
            Require(GameManager.Instance.RegisterFinalBoss(boss), "최종 보스 등록 실패"); // 실제 승리 경로 연결
            boss.TakeDamage(100f); // 실제 최종 보스 처치
            Require(GameManager.Instance.CurrentState == GameState.Victory && meta.ScoreFinalized, "실제 승리 점수 확정 실패"); // 실제 승리 상태 확인
            int finalScore = meta.CurrentScore; // 확정된 실제 점수
            long finalExperience = meta.TotalExperience; // 확정된 실제 경험치
            experience.AddExperience(30); // 종료 후 추가 경험치 시도
            enemyDied.Invoke(meta, new object[] { true }); // 종료 후 추가 처치 시도
            Require(meta.CurrentScore == finalScore && meta.TotalExperience == finalExperience, "종료 뒤 점수 또는 경험치 변경"); // 실제 종료 후 기록 보존
            ProjectEpsilon.Progression.RunScoreSettings settings = Resources.Load<ProjectEpsilon.Progression.RunScoreSettings>("RunScoreSettings"); // 실제 점수 설정
            typeof(ProjectEpsilon.Progression.RunScoreSettings).GetField("experienceTarget", Private).SetValue(settings, 1); // 종료 후 기준 변경 시험
            typeof(ProjectEpsilon.Progression.RunScoreSettings).GetField("normalKillTarget", Private).SetValue(settings, 1); // 종료 후 처치 기준 변경 시험
            string[] properties = { "BossScore", "ExperienceScore", "NormalKillScore", "EliteScore", "HealthScore", "BodyScore" }; // 실제 점수 항목 이름
            int[] expected = { 40000, 0, 0, 2500, expectedHealth, expectedBody }; // 실제 점수 항목 기대값
            int sum = 0; // 상세 점수 합계
            for (int index = 0; index < properties.Length; index++) // 실제 점수 항목 순회
            { // 순회 시작
                PropertyInfo property = meta.GetType().GetProperty(properties[index]); // 실제 점수 항목 조회
                Require(property != null && (int)property.GetValue(meta) == expected[index], "점수 항목 오류: " + properties[index]); // 실제 점수 항목 확인
                sum += expected[index]; // 상세 점수 합계
            } // 순회 종료
            Require(sum == meta.CurrentScore, "항목 합계와 총점 불일치"); // 실제 총점 일치 확인
            RunFrontendController frontend = RunFrontendController.Instance; // 실제 화면 제어기
            typeof(RunFrontendController).GetMethod("Show", Private).Invoke(frontend, new object[] { FrontendStage.Results }); // 실제 결과 화면 구성
            Text detail = GameObject.Find("FrontendScoreDetails")?.GetComponent<Text>(); // 실제 상세 점수 글자
            Require(detail != null && detail.text.Contains("기준 미정") && detail.text.Contains("2,500") && detail.text.Contains("40,000"), "결과 화면 점수 상세 누락"); // 점수 항목과 미정 기준 표시 확인
            Require(detail.text.Contains(meta.TotalExperience.ToString("N0")), "실제 경험치 집계 표시 누락"); // 경험치 획득량 표시 확인
            Canvas.ForceUpdateCanvases(); // 실제 결과 화면 배치 계산
            Text results = GameObject.Find("FrontendResultDetails").GetComponent<Text>(); // 실제 전투 결과 글자
            Vector3[] scoreCorners = new Vector3[4]; // 실제 점수 영역 경계
            Vector3[] resultCorners = new Vector3[4]; // 실제 전투 성과 영역 경계
            detail.rectTransform.GetWorldCorners(scoreCorners); // 실제 점수 영역 위치
            results.rectTransform.GetWorldCorners(resultCorners); // 실제 전투 성과 영역 위치
            Require(resultCorners[2].x <= scoreCorners[0].x + 0.01f, "결과 정보와 점수 영역 겹침"); // 두 정보 영역 겹침 방지
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null) // 실제 그래픽 장치 확인
            { // 조건 시작
                HUDController hud = UnityEngine.Object.FindFirstObjectByType<HUDController>(); // 전투 전용 HUD
                if (hud != null) // 실제 전투 HUD 확인
                { // 조건 시작
                    hud.gameObject.SetActive(false); // 결과 씬처럼 전투 HUD 숨김
                } // 조건 종료
                typeof(AutomatedDay139ScreenCapture).GetMethod("Capture", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { "day141-results.png" }); // 실제 결과 화면 캡처
                if (hud != null) // 숨긴 전투 HUD 확인
                { // 조건 시작
                    hud.gameObject.SetActive(true); // 시험용 HUD 상태 복구
                } // 조건 종료
            } // 조건 종료
            meta.BeginRun(); // 새 전투 초기화
            Require(meta.CompleteRun(false) && !meta.ScoreFinalized, "패배 점수 확정 오류"); // 실제 패배 보너스 제외
            typeof(RunFrontendController).GetMethod("Show", Private).Invoke(frontend, new object[] { FrontendStage.Results }); // 패배 점수 정보 다시 표시
            Text defeat = GameObject.Find("FrontendScoreDetails")?.GetComponent<Text>(); // 실제 패배 점수 글자
            Require(defeat != null && defeat.text.Contains("미확정") && !defeat.text.Contains("40,000"), "패배 승리 점수 표시 오류"); // 패배 화면 허위 점수 방지
            meta.BeginRun(); // 변경된 기준으로 새 전투 시작
            Require(meta.ExperienceScoreTarget == 1 && meta.NormalKillScoreTarget == 1, "새 전투의 점수 기준 초기화 오류"); // 획득 전 실제 기준 확인
            Require(meta.CompleteRun(true) && meta.ScoreFinalized, "변경된 기준의 점수 확정 실패"); // 실제 변경 기준의 승리 결과
            typeof(RunFrontendController).GetMethod("Show", Private).Invoke(frontend, new object[] { FrontendStage.Results }); // 변경 기준의 결과 화면 표시
            Text configured = GameObject.Find("FrontendScoreDetails")?.GetComponent<Text>(); // 실제 변경 기준의 점수 글자
            Require(configured != null && !configured.text.Contains("기준 미정") && configured.text.Contains("0 / 1"), "설정된 점수 기준 표시 오류"); // 실제 설정 기준과 획득량 확인
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
                Debug.Log("[Day141Result] PASS: " + message); // 성공 결과 기록
            } // 조건 종료
            else // 실패 여부 확인
            { // 조건 시작
                Debug.LogError("[Day141Result] FAIL: " + message); // 실패 결과 기록
            } // 조건 종료
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
