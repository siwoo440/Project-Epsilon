using System; // 예외 형식
using System.Collections.Generic; // 처치 대상 기록
using System.Reflection; // 선택 UI 상태 조회
using ProjectEpsilon.Combat; // 무기 및 전투 대상
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Enemies; // 적 행동
using ProjectEpsilon.Player; // 플레이어 상태
using ProjectEpsilon.Progression; // 성장 및 보상
using ProjectEpsilon.UI; // 선택 화면
using UnityEditor; // 편집기 제어
using UnityEditor.SceneManagement; // 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.InputSystem; // 가상 키보드
using UnityEngine.InputSystem.LowLevel; // 키보드 상태
using UnityEngine.UI; // 버튼 입력

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 갱신 연결
    public static class AutomatedNaturalCombatVerification // 가상 입력 생존 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonNaturalCombatVerification"; // 검증 진행 표식
        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 멤버 조회 설정
        private static Keyboard keyboard; // 가상 키보드
        private static InputSettings originalInputSettings; // 원래 입력 설정
        private static InputSettings testInputSettings; // 시험용 임시 입력 설정
        private static bool originalRunInBackground; // 원래 백그라운드 실행 상태
        private static int currentKeys = -1; // 이전 입력 조합
        private static bool checkedStart; // 시작 몸통 확인
        private static int lastBodies = -1; // 이전 몸통 수
        private static int lastHealth = -1; // 이전 체력
        private static float nextReport; // 다음 상태 기록 시각
        private static float nextDecision; // 다음 회피 판단 시각
        private static float nextInputReport; // 다음 입력 진단 시각
        private static int nextKeys; // 다음 가상 입력
        private static double startWallTime; // 실제 시작 시간
        private static bool leftObserved; // 왼쪽 입력 확인
        private static bool rightObserved; // 오른쪽 입력 확인
        private static bool boostObserved; // 가속 입력 확인
        private static bool leftRotationObserved; // 실제 왼쪽 회전 확인
        private static bool rightRotationObserved; // 실제 오른쪽 회전 확인
        private static bool boostMovementObserved; // 실제 가속 이동 확인
        private static float previousAngle; // 이전 머리 회전
        private static float previousInputTime = -1f; // 이전 동작 기록 시각
        private static Vector3 previousPosition; // 이전 머리 위치
        private static bool sawChaser; // 추적자 출현 확인
        private static bool sawPredator; // 포식자 출현 확인
        private static bool sawShooter; // 사수 출현 확인
        private static bool sawElite; // 엘리트 출현 확인
        private static int levelSelections; // 레벨업 선택 횟수
        private static int eliteSelections; // 엘리트 보상 선택 횟수
        private static readonly HashSet<WeaponTarget> monitoredTargets = new HashSet<WeaponTarget>(); // 실제 적 처치 구독 목록
        private static readonly Dictionary<WeaponTarget, float> targetHealth = new Dictionary<WeaponTarget, float>(); // 관찰 대상별 체력 기록
        private static WeaponTarget focusTarget; // 지속 추적 중인 적
        private static bool selectedCurve; // 최근 곡선 예측 선택
        private static float selectedTrajectoryScore; // 최근 선택 경로 점수
        private static int naturalKills; // 자연 출현 적 처치 수
        private static bool experienceObserved; // 자연 경험치 획득 여부
        private static SnakeExperience trackedExperience; // 경험치 이벤트 대상
        private static int previousExperience; // 이전 경험치 값
        private static int previousLevel; // 이전 레벨 값
        private static float nextTacticalReport; // 다음 전투 거리 기록 시각

        static AutomatedNaturalCombatVerification() // 도메인 연결
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

        private static void Tick() // 생존 관찰과 가상 입력
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 시작 확인
            { // 조건 시작
                return; // 플레이 진입 대기
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 시작 무기 화면 처리
            { // 조건 시작
                return; // 전투 시작 대기
            } // 조건 끝
            if (startWallTime == 0d) // 최초 갱신 확인
            { // 조건 시작
                startWallTime = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
            } // 조건 끝
            try // 검증 오류 처리
            { // 보호 시작
                TickPlaying(); // 실제 게임 상태 처리
            } // 보호 끝
            catch (Exception exception) // 예외 발생 확인
            { // 예외 시작
                Fail(exception.ToString()); // 예외 기록과 종료
            } // 예외 끝
        } // 메서드 끝

        private static void TickPlaying() // 게임 상태 검증
        { // 메서드 시작
            GameManager game = GameManager.Instance; // 게임 관리자
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 적 출현기
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 플레이어 이동
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 몸통 관리자
            SnakeHealth health = UnityEngine.Object.FindFirstObjectByType<SnakeHealth>(); // 체력 관리자
            SnakeExperience experience = UnityEngine.Object.FindFirstObjectByType<SnakeExperience>(); // 경험치 관리자
            if (game == null || spawner == null || player == null || body == null || health == null || experience == null) // 필수 참조 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            if (trackedExperience == null) // 경험치 이벤트 연결 확인
            { // 조건 시작
                trackedExperience = experience; // 실제 경험치 관리자 저장
                previousExperience = experience.CurrentExperience; // 초기 경험치 저장
                previousLevel = experience.CurrentLevel; // 초기 레벨 저장
                experience.ExperienceChanged += HandleExperienceChanged; // 경험치 변경 구독
            } // 조건 끝
            if (keyboard == null) // 가상 키보드 확인
            { // 조건 시작
                originalInputSettings = InputSystem.settings; // 원래 입력 설정 저장
                testInputSettings = UnityEngine.Object.Instantiate(originalInputSettings); // 저장되지 않는 설정 사본 생성
                testInputSettings.hideFlags = HideFlags.HideAndDontSave; // 시험 설정 저장 방지
                testInputSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; // 배치 키보드 게임 전달
                testInputSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // 배치 포커스 무시
                InputSystem.settings = testInputSettings; // 시험 설정 메모리 적용
                originalRunInBackground = Application.runInBackground; // 기존 백그라운드 설정 저장
                Application.runInBackground = true; // 비활성 배치 입력 처리
                keyboard = InputSystem.AddDevice<Keyboard>(); // 가상 키보드 생성
                Debug.Log("[NaturalCombatVerification] Virtual Keyboard A/D/Space; in-memory focus settings; natural spawns; no invulnerability, stat, XP, or time-scale changes; automatic UI choices."); // 시험 조건 기록
            } // 조건 끝
            float elapsed = (float)typeof(EarlyEnemyWaveSpawner).GetField("elapsed", PrivateInstance).GetValue(spawner); // 전투 경과 시간
            ObserveState(player, body, health, elapsed); // 손실 및 피해 상태 기록
            if (!checkedStart && elapsed >= 0.25f) // 첫 물리 갱신 이후 확인
            { // 조건 시작
                checkedStart = true; // 시작 검증 기록
                if (body.CurrentBodyCount != 3) // 기획 시작 몸통 확인
                { // 조건 시작
                    Fail($"initial body={body.CurrentBodyCount}, expected 3"); // 시작 손실 기록
                    return; // 추가 처리 중단
                } // 조건 끝
                Debug.Log("[NaturalCombatVerification] Starting three bodies retained after physics."); // 시작 상태 기록
            } // 조건 끝
            EarlyEnemyBehaviour[] enemies = UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None); // 현재 적 목록
            ObserveEnemies(enemies); // 출현 종류 기록
            UpdateFocus(enemies); // 실제 적 추적 대상 갱신
            if (elapsed >= nextTacticalReport) // 전투 거리 기록 간격 확인
            { // 조건 시작
                LogTacticalState(enemies, elapsed); // 실제 무기와 적 거리 기록
                nextTacticalReport = elapsed + 5f; // 다음 기록 시각 저장
            } // 조건 끝
            experienceObserved |= experience.CurrentExperience > 0 || experience.CurrentLevel > 1; // 자연 경험치 확인
            SnakeLevelUpController level = UnityEngine.Object.FindFirstObjectByType<SnakeLevelUpController>(); // 레벨업 처리기
            EliteRewardController elite = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 엘리트 보상 처리기
            SnakeWeaponMergeController merge = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponMergeController>(); // 합성 처리기
            if (level != null && level.IsPresentingLevelUp) // 레벨업 선택 상태
            { // 조건 시작
                SetKeys(0); // 이동 입력 해제
                bool replacement = (bool)typeof(SnakeLevelUpController).GetField("selectingReplacement", PrivateInstance).GetValue(level); // 교체 선택 단계 확인
                ClickLevelButton(replacement ? 1 : 0); // 첫 후보 또는 교체 확정
                levelSelections++; // 선택 횟수 증가
            } // 조건 끝
            else if (elite != null && elite.IsPresentingReward) // 엘리트 보상 선택 상태
            { // 조건 시작
                SetKeys(0); // 이동 입력 해제
                bool replacement = (bool)typeof(EliteRewardController).GetField("selectingReplacement", PrivateInstance).GetValue(elite); // 교체 선택 단계 확인
                if (replacement) // 무기 교체 단계
                { // 조건 시작
                    ClickLevelButton(1); // 현재 교체 확정
                } // 조건 끝
                else // 일반 보상 선택 단계
                { // 대안 시작
                    ClickFirstAvailableLevelButton(); // 활성 보상 선택
                } // 대안 끝
                eliteSelections++; // 선택 횟수 증가
            } // 조건 끝
            else if (merge != null && merge.IsMergeOpen) // 보상 합성 화면 상태
            { // 조건 시작
                SetKeys(0); // 이동 입력 해제
                ClickFirstAvailableMergeButton(); // 첫 합성 또는 강화 선택
            } // 조건 끝
            else if (game.IsPlaying) // 일반 플레이 상태
            { // 조건 시작
                SnakeStamina stamina = player.GetComponent<SnakeStamina>(); // 가속 자원 조회
                InputBindingManager bindings = InputBindingManager.Instance; // 실제 입력 상태 조회
                leftObserved |= bindings != null && bindings.TurnLeftPressed; // 왼쪽 입력 반영 확인
                rightObserved |= bindings != null && bindings.TurnRightPressed; // 오른쪽 입력 반영 확인
                boostObserved |= stamina != null && stamina.IsBoosting; // 가속 반영 확인
                ObserveMotion(player, bindings, stamina, elapsed); // 실제 회전 및 가속 이동 확인
                if (elapsed < 2.2f && elapsed >= nextInputReport) // 초기 입력 진단 간격 확인
                { // 조건 시작
                    LogInputDiagnostics(bindings, stamina, elapsed); // 키 상태와 입력 액션 기록
                    nextInputReport = elapsed + 0.15f; // 다음 진단 시각 저장
                } // 조건 끝
                if (elapsed < 0.3f) // 초기 몸통 상태 확인
                { // 조건 시작
                    SetKeys(0); // 첫 물리 갱신 대기
                } // 조건 끝
                else // 위험 회피 입력
                { // 대안 시작
                    if (elapsed >= nextDecision) // 새 조향 시각 확인
                    { // 조건 시작
                        nextKeys = ChooseKeys(player, body, enemies, stamina); // 위험 회피 조향 계산
                        nextDecision = elapsed + 0.12f; // 다음 조향 시각 저장
                    } // 조건 끝
                    SetKeys(nextKeys); // 가상 조향 입력
                } // 조건 끝
            } // 조건 끝
            if (elapsed >= nextReport) // 상태 기록 간격 확인
            { // 조건 시작
                Debug.Log($"[NaturalCombatVerification] time={elapsed:F1} wall={EditorApplication.timeSinceStartup - startWallTime:F1} state={game.CurrentState} body={body.CurrentBodyCount} hp={health.CurrentHealth} level={experience.CurrentLevel} xp={experience.CurrentExperience} kills={naturalKills} gems={UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length} enemies={spawner.ActiveEnemies} levelChoices={levelSelections} eliteChoices={eliteSelections} inputs={leftObserved},{rightObserved},{boostObserved} motion={leftRotationObserved},{rightRotationObserved},{boostMovementObserved} scale={Time.timeScale:F1}"); // 전투 및 성장 상태 기록
                nextReport += 30f; // 다음 기록 시각
            } // 조건 끝
            if (game.CurrentState == GameState.Defeated) // 패배 상태 확인
            { // 조건 시작
                Fail($"defeated at {elapsed:F1} seconds; level={experience.CurrentLevel}; hp={health.CurrentHealth}; body={body.CurrentBodyCount}"); // 패배 결과 기록
                return; // 추가 처리 중단
            } // 조건 끝
            if (game.IsPlaying && (merge == null || !merge.IsMergeOpen) && !Mathf.Approximately(Time.timeScale, 1f)) // 일반 게임 속도 확인
            { // 조건 시작
                Fail($"unexpected time scale {Time.timeScale}"); // 속도 차이 기록
                return; // 추가 처리 중단
            } // 조건 끝
            if (naturalKills > 0 && experienceObserved && experience.CurrentLevel >= 2 && levelSelections > 0 && game.IsPlaying) // 자연 처치와 보상 완료 확인
            { // 조건 시작
                Debug.Log($"[NaturalCombatVerification] PASS natural kills={naturalKills} level={experience.CurrentLevel} xp={experience.CurrentExperience} choices={levelSelections} body={body.CurrentBodyCount} hp={health.CurrentHealth} game={elapsed:F1}s wall={EditorApplication.timeSinceStartup - startWallTime:F1}s."); // 자연 전투 성장 확인
                Finish(0); // 성공 종료
                return; // 처리 종료
            } // 조건 끝
            if (elapsed >= 300f) // 자연 전투 관찰 종료
            { // 조건 시작
                Fail($"natural progression absent at 300s: kills={naturalKills} level={experience.CurrentLevel} xp={experience.CurrentExperience} choices={levelSelections}"); // 성장 미발생 기록
                return; // 추가 처리 중단
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - startWallTime > 480d) // 실제 시간 제한 확인
            { // 조건 시작
                Fail($"timed out at {elapsed:F1} seconds"); // 시간 초과 기록
            } // 조건 끝
        } // 메서드 끝

        private static int ChooseKeys(SnakeMovement player, SnakeBodyManager body, EarlyEnemyBehaviour[] enemies, SnakeStamina stamina) // 회피 조향 선택
        { // 메서드 시작
            Vector2 origin = player.transform.position; // 플레이어 위치
            Vector2 heading = player.transform.up; // 현재 전진 방향
            int bestKeys = 0; // 기본 직진 선택
            float bestScore = float.PositiveInfinity; // 최소 위험 점수
            bool bestCurve = false; // 기본 경로 선택
            Vector2 focusOffset = focusTarget == null ? Vector2.zero : (Vector2)focusTarget.transform.position - origin; // 추적 적의 상대 위치
            bool canReacquire = focusTarget != null && focusTarget.IsAlive && focusTarget.CurrentHealth < focusTarget.MaximumHealth && focusOffset.magnitude > 2.5f && Vector2.Dot(heading, focusOffset.normalized) < -0.3f; // 뒤처진 부상 적 확인
            for (int boost = 0; boost <= 1; boost++) // 가속 사용 여부 비교
            { // 반복 시작
                if (boost == 1 && (stamina == null || stamina.CurrentStamina < 30f)) // 가속 자원 확인
                { // 조건 시작
                    continue; // 가속 후보 제외
                } // 조건 끝
                for (int turn = -1; turn <= 1; turn++) // 왼쪽 직진 오른쪽 비교
                { // 반복 시작
                    int curveCount = canReacquire && turn != 0 ? 2 : 1; // 재접근 곡선 후보 수
                    for (int curve = 0; curve < curveCount; curve++) // 일반 및 곡선 예측 비교
                    { // 반복 시작
                        float score = ScoreTrajectory(origin, heading, player, body, enemies, turn, boost == 1, curve == 1); // 후보 경로 위험 계산
                        if (score < bestScore) // 더 안전한 경로 확인
                        { // 조건 시작
                            bestScore = score; // 최소 위험 저장
                            bestKeys = (turn < 0 ? 1 : turn > 0 ? 2 : 0) | (boost == 1 ? 4 : 0); // 조향과 가속 저장
                            bestCurve = curve == 1; // 곡선 예측 선택 저장
                        } // 조건 끝
                    } // 반복 끝
                } // 반복 끝
            } // 반복 끝
            selectedCurve = bestCurve; // 최근 곡선 선택 기록
            selectedTrajectoryScore = bestScore; // 최근 경로 점수 기록
            return bestKeys; // 선택 입력 반환
        } // 메서드 끝

        private static float ScoreTrajectory(Vector2 origin, Vector2 heading, SnakeMovement player, SnakeBodyManager body, EarlyEnemyBehaviour[] enemies, int turn, bool boost, bool reacquireCurve) // 회피 경로 평가
        { // 메서드 시작
            Vector2 point = origin; // 경로 위치 초기화
            Vector2 forward = heading; // 경로 방향 초기화
            float score = (turn == 0 ? 0f : 1f) + (boost ? 0.35f : 0f); // 불필요한 회전과 가속 억제
            float expectedSpeed = player.MoveSpeed * (boost ? 1.5f : 1f); // 후보별 예상 이동 속도
            Vector2[] predictedEnemies = new Vector2[enemies.Length]; // 적별 예상 위치
            for (int index = 0; index < enemies.Length; index++) // 초기 적 위치 순회
            { // 반복 시작
                predictedEnemies[index] = enemies[index] == null ? origin : (Vector2)enemies[index].transform.position; // 실제 위치 저장
            } // 반복 끝
            int focusIndex = -1; // 추적 대상 위치 초기화
            for (int index = 0; index < enemies.Length; index++) // 실제 적 목록 순회
            { // 반복 시작
                if (enemies[index] != null && enemies[index].GetComponent<WeaponTarget>() == focusTarget) // 추적 대상 일치 확인
                { // 조건 시작
                    focusIndex = index; // 대상 위치 저장
                    break; // 추가 탐색 종료
                } // 조건 끝
            } // 반복 끝
            Vector2 initialOffset = focusIndex < 0 ? Vector2.zero : predictedEnemies[focusIndex] - origin; // 초기 적 방향
            Vector2 flank = initialOffset.sqrMagnitude < 0.001f ? Vector2.zero : new Vector2(-initialOffset.y, initialOffset.x).normalized * turn; // 선택 회전의 측면 방향
            ExperiencePickup[] gems = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None); // 자연 드롭 보석 조회
            const float step = 0.16f; // 예상 경로 시간 간격
            for (int tick = 1; tick <= 8; tick++) // 짧은 미래 경로 비교
            { // 반복 시작
                float angle = -turn * player.TurnSpeed * step * Mathf.Deg2Rad; // 회전 각도 계산
                if (reacquireCurve && focusIndex >= 0 && tick > 1) // 곡선 예측 후속 단계 확인
                { // 조건 시작
                    Vector2 waypoint = predictedEnemies[focusIndex] + flank; // 예측 적의 측면 지점
                    Vector2 desired = waypoint - point; // 측면 접근 방향
                    if (desired.sqrMagnitude > 0.001f) // 목표 방향 확인
                    { // 조건 시작
                        float desiredAngle = Vector2.SignedAngle(forward, desired); // 목표까지 회전 각도
                        angle = Mathf.Clamp(desiredAngle, -player.TurnSpeed * step, player.TurnSpeed * step) * Mathf.Deg2Rad; // 실제 회전 속도 범위 적용
                    } // 조건 끝
                } // 조건 끝
                float cosine = Mathf.Cos(angle); // 회전 코사인
                float sine = Mathf.Sin(angle); // 회전 사인
                forward = new Vector2(forward.x * cosine - forward.y * sine, forward.x * sine + forward.y * cosine); // 예상 전진 방향
                point += forward * expectedSpeed * step; // 예상 머리 위치
                float nearestEnemy = 12f; // 가장 가까운 적 거리
                for (int enemyIndex = 0; enemyIndex < enemies.Length; enemyIndex++) // 적 위험 순회
                { // 반복 시작
                    EarlyEnemyBehaviour enemy = enemies[enemyIndex]; // 현재 적 조회
                    if (enemy == null) // 소멸 적 확인
                    { // 조건 시작
                        continue; // 다음 적 확인
                    } // 조건 끝
                    Vector2 enemyPosition = predictedEnemies[enemyIndex]; // 이전 적 예상 위치
                    Vector2 chase = point - enemyPosition; // 예상 머리 방향
                    Vector2 predicted = enemyPosition + chase.normalized * player.MoveSpeed * 0.8f * step; // 적 추적 위치 갱신
                    predictedEnemies[enemyIndex] = predicted; // 다음 예상 위치 저장
                    float distance = Vector2.Distance(point, predicted); // 예상 접근 거리
                    nearestEnemy = Mathf.Min(nearestEnemy, distance); // 최단 적 거리 갱신
                    if (distance < 1.6f) // 근접 위험 범위 확인
                    { // 조건 시작
                        score += 0.3f / (distance * distance + 0.15f); // 근접 위험 누적
                    } // 조건 끝
                    if (distance < 0.75f) // 머리 직접 충돌 위험 확인
                    { // 조건 시작
                        score += 40f; // 충돌 위험 가중
                    } // 조건 끝
                    Vector2 expectedWeapon = point - forward * 0.58f; // 예상 첫 몸통 위치
                    float weaponDistance = Vector2.Distance(expectedWeapon, predicted); // 적과 첫 무기 거리
                    for (int index = 0; index < body.BodySegments.Count; index++) // 실제 몸통별 위험 확인
                    { // 반복 시작
                        SnakeSegment segment = body.BodySegments[index]; // 현재 몸통 조회
                        if (segment == null) // 제거된 몸통 확인
                        { // 조건 시작
                            continue; // 다음 몸통 확인
                        } // 조건 끝
                        Vector2 futureSegment = point - forward * (0.58f * (index + 1)); // 예측 몸통 위치
                        float travelTime = 0.58f * (index + 1) / Mathf.Max(0.1f, player.MoveSpeed); // 몸통 이동 지연
                        float travelFraction = Mathf.Clamp01(step * tick / travelTime); // 예상 경로 반영 비율
                        Vector2 estimatedSegment = Vector2.Lerp(segment.transform.position, futureSegment, travelFraction); // 현재 몸통과 예상 몸통 보간
                        float bodyDistance = Vector2.Distance(estimatedSegment, predicted); // 적과 예상 몸통 거리
                        if (bodyDistance < 1.3f) // 몸통 근접 위험 확인
                        { // 조건 시작
                            score += 1.2f / (bodyDistance * bodyDistance + 0.15f); // 근접 공격 위험 반영
                        } // 조건 끝
                        if (bodyDistance < 0.8f) // 몸통 직접 접촉 위험 확인
                        { // 조건 시작
                            score += 25f; // 몸통 피해 위험 가중
                        } // 조건 끝
                    } // 반복 끝
                    if (weaponDistance < 1.35f && weaponDistance > 0.95f && distance > 1.1f) // 안전한 무기 사거리 확인
                    { // 조건 시작
                        score -= 1.8f; // 안전 사거리의 지속 공격 선호
                        if (enemyIndex == focusIndex) // 추적 대상 확인
                        { // 조건 시작
                            score -= 1.2f; // 같은 적의 연속 공격 선호
                        } // 조건 끝
                    } // 조건 끝
                    if (enemyIndex == focusIndex) // 추적 대상 거리 확인
                    { // 조건 시작
                        score += Mathf.Min(8f, Mathf.Max(0f, weaponDistance - 1.2f)) * 0.35f; // 사거리 밖 대상 접근 유도
                    } // 조건 끝
                } // 반복 끝
                score += Mathf.Min(nearestEnemy, 8f) * 0.02f; // 가까운 적의 안전한 접근 유도
                float nearestGem = 15f; // 가장 가까운 보석 거리
                foreach (ExperiencePickup gem in gems) // 자연 드롭 보석 순회
                { // 반복 시작
                    if (gem != null) // 유효한 보석 확인
                    { // 조건 시작
                        nearestGem = Mathf.Min(nearestGem, Vector2.Distance(point, gem.transform.position)); // 최단 보석 거리 갱신
                    } // 조건 끝
                } // 반복 끝
                if (gems.Length > 0 && nearestEnemy > 2f) // 안전한 보석 접근 확인
                { // 조건 시작
                    score += Mathf.Min(nearestGem, 10f) * 0.28f; // 보석 접근 유도
                } // 조건 끝
                for (int index = 2; index < body.BodySegments.Count; index++) // 뒤쪽 몸통 순회
                { // 반복 시작
                    SnakeSegment segment = body.BodySegments[index]; // 몸통 조회
                    if (segment != null && Vector2.Distance(point, segment.transform.position) < 0.65f) // 자기 충돌 위험 확인
                    { // 조건 시작
                        score += 20f; // 자기 충돌 위험 가중
                    } // 조건 끝
                } // 반복 끝
            } // 반복 끝
            return score; // 예상 위험 반환
        } // 메서드 끝

        private static bool ThreatBehind(Vector2 origin, Vector2 heading, EarlyEnemyBehaviour[] enemies) // 뒤쪽 근접 위험 확인
        { // 메서드 시작
            foreach (EarlyEnemyBehaviour enemy in enemies) // 적 순회
            { // 반복 시작
                if (enemy == null) // 소멸 적 확인
                { // 조건 시작
                    continue; // 다음 적 확인
                } // 조건 끝
                Vector2 offset = (Vector2)enemy.transform.position - origin; // 적까지 거리
                if (offset.sqrMagnitude < 9f && Vector2.Dot(heading, offset.normalized) < -0.3f) // 뒤쪽 근접 확인
                { // 조건 시작
                    return true; // 가속 필요 반환
                } // 조건 끝
            } // 반복 끝
            return false; // 가속 불필요 반환
        } // 메서드 끝

        private static void SetKeys(int keys) // 가상 키 상태 전송
        { // 메서드 시작
            if (keyboard == null || keys == currentKeys) // 키보드 및 상태 변경 확인
            { // 조건 시작
                return; // 중복 전송 생략
            } // 조건 끝
            currentKeys = keys; // 현재 입력 저장
            KeyboardState state = new KeyboardState(); // 키 상태 초기화
            if ((keys & 1) != 0) // 왼쪽 입력 확인
            { // 조건 시작
                state.Press(Key.A); // A 키 상태 설정
            } // 조건 끝
            if ((keys & 2) != 0) // 오른쪽 입력 확인
            { // 조건 시작
                state.Press(Key.D); // D 키 상태 설정
            } // 조건 끝
            if ((keys & 4) != 0) // 가속 입력 확인
            { // 조건 시작
                state.Press(Key.Space); // Space 키 상태 설정
            } // 조건 끝
            InputSystem.QueueStateEvent(keyboard, state); // 실제 입력 시스템에 상태 전달
        } // 메서드 끝

        private static void ObserveState(SnakeMovement player, SnakeBodyManager body, SnakeHealth health, float elapsed) // 체력과 몸통 변화 기록
        { // 메서드 시작
            if (lastBodies >= 0 && (body.CurrentBodyCount != lastBodies || health.CurrentHealth != lastHealth)) // 몸통 또는 체력 변화 확인
            { // 조건 시작
                SnakeInvulnerability invulnerability = player.GetComponent<SnakeInvulnerability>(); // 전체 무적 상태
                SnakeExternalCollision collision = player.GetComponent<SnakeExternalCollision>(); // 직접 충돌 상태
                Debug.Log($"[NaturalCombatVerification] change time={elapsed:F2} body={lastBodies}->{body.CurrentBodyCount} hp={lastHealth}->{health.CurrentHealth} fullInvincible={(invulnerability != null && invulnerability.IsInvulnerable)} directProtected={(collision != null && collision.EnemyContactProtected)}"); // 피해와 보호 상태 기록
            } // 조건 끝
            lastBodies = body.CurrentBodyCount; // 현재 몸통 저장
            lastHealth = health.CurrentHealth; // 현재 체력 저장
        } // 메서드 끝

        private static void LogInputDiagnostics(InputBindingManager bindings, SnakeStamina stamina, float elapsed) // 초기 입력 상태 진단
        { // 메서드 시작
            InputAction left = bindings == null ? null : (InputAction)typeof(InputBindingManager).GetField("turnLeftAction", PrivateInstance).GetValue(bindings); // 왼쪽 입력 액션 조회
            InputAction right = bindings == null ? null : (InputAction)typeof(InputBindingManager).GetField("turnRightAction", PrivateInstance).GetValue(bindings); // 오른쪽 입력 액션 조회
            InputAction boost = bindings == null ? null : (InputAction)typeof(InputBindingManager).GetField("boostAction", PrivateInstance).GetValue(bindings); // 가속 입력 액션 조회
            string leftPath = left == null || left.bindings.Count == 0 ? "-" : left.bindings[0].effectivePath; // 적용된 왼쪽 키 경로
            string rightPath = right == null || right.bindings.Count == 0 ? "-" : right.bindings[0].effectivePath; // 적용된 오른쪽 키 경로
            string boostPath = boost == null || boost.bindings.Count == 0 ? "-" : boost.bindings[0].effectivePath; // 적용된 가속 키 경로
            Debug.Log($"[PilotInput] time={elapsed:F2} requested={currentKeys} focused={Application.isFocused} keyboardEnabled={keyboard.enabled} keyboardCurrent={Keyboard.current == keyboard} raw={keyboard.aKey.isPressed},{keyboard.dKey.isPressed},{keyboard.spaceKey.isPressed} actions={bindings != null && bindings.TurnLeftPressed},{bindings != null && bindings.TurnRightPressed},{bindings != null && bindings.BoostPressed} motion={leftRotationObserved},{rightRotationObserved},{boostMovementObserved} boosting={stamina != null && stamina.IsBoosting} actionEnabled={left != null && left.enabled},{right != null && right.enabled},{boost != null && boost.enabled} paths={leftPath},{rightPath},{boostPath}"); // 실제 키 및 액션 상태 기록
        } // 메서드 끝

        private static void ObserveMotion(SnakeMovement player, InputBindingManager bindings, SnakeStamina stamina, float elapsed) // 실제 회전과 이동 확인
        { // 메서드 시작
            float angle = player.transform.eulerAngles.z; // 현재 머리 회전
            Vector3 position = player.transform.position; // 현재 머리 위치
            if (previousInputTime >= 0f && elapsed > previousInputTime) // 유효 시간 간격 확인
            { // 조건 시작
                float rotation = Mathf.DeltaAngle(previousAngle, angle); // 실제 회전 변화량
                float movementSpeed = Vector3.Distance(previousPosition, position) / (elapsed - previousInputTime); // 실제 이동 속도
                leftRotationObserved |= bindings != null && bindings.TurnLeftPressed && rotation > 0.01f; // 왼쪽 입력의 반시계 회전 확인
                rightRotationObserved |= bindings != null && bindings.TurnRightPressed && rotation < -0.01f; // 오른쪽 입력의 시계 회전 확인
                boostMovementObserved |= stamina != null && stamina.IsBoosting && movementSpeed > player.MoveSpeed * 1.25f; // 가속 입력의 실제 속도 확인
            } // 조건 끝
            previousAngle = angle; // 현재 회전 저장
            previousPosition = position; // 현재 위치 저장
            previousInputTime = elapsed; // 현재 시각 저장
        } // 메서드 끝

        private static void ObserveEnemies(EarlyEnemyBehaviour[] enemies) // 적 출현 종류 기록
        { // 메서드 시작
            foreach (EarlyEnemyBehaviour enemy in enemies) // 적 순회
            { // 반복 시작
                if (enemy == null) // 제거된 적 확인
                { // 조건 시작
                    continue; // 다음 적 검사
                } // 조건 끝
                string name = enemy.gameObject.name; // 적 이름 조회
                sawChaser |= name == "EarlyEnemy_Chaser"; // 추적자 기록
                sawPredator |= name == "EarlyEnemy_Predator"; // 포식자 기록
                sawShooter |= name == "EarlyEnemy_Shooter"; // 사수 기록
                sawElite |= name == "EarlyEnemy_Elite"; // 엘리트 기록
                WeaponTarget target = enemy.GetComponent<WeaponTarget>(); // 실제 전투 대상 조회
                if (target != null && monitoredTargets.Add(target)) // 신규 적 처치 추적 확인
                { // 조건 시작
                    target.Died += HandleNaturalKill; // 자연 처치 이벤트 연결
                    targetHealth[target] = target.CurrentHealth; // 초기 적 체력 저장
                } // 조건 끝
                if (target != null && targetHealth.TryGetValue(target, out float previous) && target.CurrentHealth < previous) // 실제 적 피해 확인
                { // 조건 시작
                    Debug.Log($"[NaturalCombatVerification] target damage id={target.GetInstanceID()} type={target.name} hp={previous:F1}->{target.CurrentHealth:F1} time={Time.time:F2}"); // 무기 피해 관찰 기록
                    targetHealth[target] = target.CurrentHealth; // 적 체력 갱신
                    if (focusTarget == null || !focusTarget.IsAlive || focusTarget.CurrentHealth >= focusTarget.MaximumHealth) // 유효한 부상 대상 우선 확인
                    { // 조건 시작
                        focusTarget = target; // 실제 피해 대상 추적
                        Debug.Log($"[NaturalCombatVerification] focus id={target.GetInstanceID()} reason=damage hp={target.CurrentHealth:F1}"); // 추적 대상 변경 기록
                    } // 조건 끝
                } // 조건 끝
            } // 반복 끝
        } // 메서드 끝

        private static void LogTacticalState(EarlyEnemyBehaviour[] enemies, float elapsed) // 실제 공격 거리 기록
        { // 메서드 시작
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            if (weapons == null || weapons.Slots.Count == 0 || weapons.Slots[0].IsEmpty || weapons.Slots[0].Origin == null) // 첫 무기 존재 확인
            { // 조건 시작
                return; // 무기 부재 시 생략
            } // 조건 끝
            SnakeWeaponSlot slot = weapons.Slots[0]; // 첫 무기 슬롯
            float nearest = float.PositiveInfinity; // 최근접 적 거리 초기화
            int inRange = 0; // 무기 사거리 안의 적 수
            foreach (EarlyEnemyBehaviour enemy in enemies) // 실제 적 순회
            { // 반복 시작
                if (enemy == null) // 제거된 적 확인
                { // 조건 시작
                    continue; // 다음 적 확인
                } // 조건 끝
                float distance = Vector2.Distance(slot.Origin.position, enemy.transform.position); // 실제 무기와 적 거리
                nearest = Mathf.Min(nearest, distance); // 최단 거리 갱신
                inRange += distance <= slot.EffectiveRange ? 1 : 0; // 사거리 안 적 계수
            } // 반복 끝
            float focusDistance = focusTarget == null ? float.PositiveInfinity : Vector2.Distance(slot.Origin.position, focusTarget.transform.position); // 추적 적의 실제 거리
            int focusId = focusTarget == null ? 0 : focusTarget.GetInstanceID(); // 추적 적 구분값
            float focusHealth = focusTarget == null ? 0f : focusTarget.CurrentHealth; // 추적 적 현재 체력
            Debug.Log($"[NaturalCombatVerification] tactical time={elapsed:F1} weaponNearest={nearest:F2} weaponRange={slot.EffectiveRange:F2} inRange={inRange} weaponReady={slot.IsReady(Time.time)} keys={currentKeys} focusId={focusId} focusDistance={focusDistance:F2} focusHp={focusHealth:F1} curve={selectedCurve} score={selectedTrajectoryScore:F2}"); // 거리 및 추적 상태 기록
        } // 메서드 끝

        private static void UpdateFocus(EarlyEnemyBehaviour[] enemies) // 자연 출현 적 추적 대상 선택
        { // 메서드 시작
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            if (weapons == null || weapons.Slots.Count == 0 || weapons.Slots[0].IsEmpty || weapons.Slots[0].Origin == null) // 첫 무기 존재 확인
            { // 조건 시작
                focusTarget = null; // 무기 부재 시 추적 해제
                return; // 대상 선택 생략
            } // 조건 끝
            Vector2 weaponOrigin = weapons.Slots[0].Origin.position; // 실제 무기 위치
            if (focusTarget != null && (!focusTarget.IsAlive || !focusTarget.isActiveAndEnabled || Vector2.Distance(weaponOrigin, focusTarget.transform.position) > 9f)) // 기존 적 상태 확인
            { // 조건 시작
                Debug.Log($"[NaturalCombatVerification] focus id={focusTarget.GetInstanceID()} reason=inactive-or-distant"); // 추적 해제 사유 기록
                focusTarget = null; // 유효하지 않은 대상 해제
            } // 조건 끝
            if (focusTarget != null) // 기존 대상 유지 확인
            { // 조건 시작
                return; // 단일 적 추적 유지
            } // 조건 끝
            float bestDistance = 7f; // 신규 적 선택 거리 상한
            foreach (EarlyEnemyBehaviour enemy in enemies) // 실제 적 순회
            { // 반복 시작
                if (enemy == null) // 소멸된 적 확인
                { // 조건 시작
                    continue; // 다음 적 확인
                } // 조건 끝
                WeaponTarget target = enemy.GetComponent<WeaponTarget>(); // 실제 공격 대상 조회
                if (target == null || !target.IsAlive || !target.isActiveAndEnabled) // 유효 적 확인
                { // 조건 시작
                    continue; // 다음 적 확인
                } // 조건 끝
                float distance = Vector2.Distance(weaponOrigin, target.transform.position); // 무기와 대상 거리
                if (distance < bestDistance) // 더 가까운 적 확인
                { // 조건 시작
                    bestDistance = distance; // 최단 거리 갱신
                    focusTarget = target; // 추적 대상 저장
                } // 조건 끝
            } // 반복 끝
            if (focusTarget != null) // 신규 대상 확인
            { // 조건 시작
                Debug.Log($"[NaturalCombatVerification] focus id={focusTarget.GetInstanceID()} reason=nearest distance={bestDistance:F2} hp={focusTarget.CurrentHealth:F1}"); // 신규 대상 기록
            } // 조건 끝
        } // 메서드 끝

        private static void HandleNaturalKill(WeaponTarget target) // 실제 적 처치 기록
        { // 메서드 시작
            naturalKills++; // 처치 수 증가
            Debug.Log($"[NaturalCombatVerification] natural kill={naturalKills} type={target.name} time={Time.time:F2}"); // 실제 처치 기록
        } // 메서드 끝

        private static void HandleExperienceChanged(int current, int required) // 실제 경험치 변경 기록
        { // 메서드 시작
            int level = trackedExperience == null ? previousLevel : trackedExperience.CurrentLevel; // 현재 레벨 조회
            if (current != previousExperience || level != previousLevel) // 실제 성장 상태 변경 확인
            { // 조건 시작
                Debug.Log($"[NaturalCombatVerification] experience level={level} xp={current}/{required} time={Time.time:F2}"); // 자연 경험치 기록
                previousExperience = current; // 현재 경험치 저장
                previousLevel = level; // 현재 레벨 저장
            } // 조건 끝
        } // 메서드 끝

        private static void ClickLevelButton(int index) // 레벨업 또는 보상 버튼 선택
        { // 메서드 시작
            LevelUpPanelController panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 선택 화면 조회
            string[] names = { "candidateButton01", "candidateButton02", "candidateButton03" }; // 버튼 필드 이름
            Button button = panel == null ? null : (Button)typeof(LevelUpPanelController).GetField(names[index], PrivateInstance).GetValue(panel); // 실제 버튼 조회
            if (button != null && button.IsActive() && button.interactable) // 선택 가능 버튼 확인
            { // 조건 시작
                button.onClick.Invoke(); // 실제 버튼 이벤트 실행
            } // 조건 끝
        } // 메서드 끝

        private static void ClickFirstAvailableLevelButton() // 사용 가능한 첫 보상 선택
        { // 메서드 시작
            LevelUpPanelController panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 보상 화면 조회
            if (panel == null) // 화면 부재 확인
            { // 조건 시작
                return; // 선택 생략
            } // 조건 끝
            string[] names = { "candidateButton01", "candidateButton02", "candidateButton03" }; // 버튼 필드 이름
            foreach (string name in names) // 후보 버튼 순회
            { // 반복 시작
                Button button = (Button)typeof(LevelUpPanelController).GetField(name, PrivateInstance).GetValue(panel); // 버튼 조회
                if (button != null && button.IsActive() && button.interactable) // 선택 가능 확인
                { // 조건 시작
                    button.onClick.Invoke(); // 실제 버튼 이벤트 실행
                    return; // 중복 선택 차단
                } // 조건 끝
            } // 반복 끝
        } // 메서드 끝

        private static void ClickFirstAvailableMergeButton() // 사용 가능한 첫 합성 선택
        { // 메서드 시작
            WeaponMergePanelController panel = UnityEngine.Object.FindFirstObjectByType<WeaponMergePanelController>(FindObjectsInactive.Include); // 합성 화면 조회
            if (panel == null) // 화면 부재 확인
            { // 조건 시작
                return; // 선택 생략
            } // 조건 끝
            string[] names = { "candidateButton01", "candidateButton02", "candidateButton03" }; // 버튼 필드 이름
            foreach (string name in names) // 후보 버튼 순회
            { // 반복 시작
                Button button = (Button)typeof(WeaponMergePanelController).GetField(name, PrivateInstance).GetValue(panel); // 버튼 조회
                if (button != null && button.IsActive() && button.interactable) // 선택 가능 확인
                { // 조건 시작
                    button.onClick.Invoke(); // 실제 버튼 이벤트 실행
                    return; // 중복 선택 차단
                } // 조건 끝
            } // 반복 끝
        } // 메서드 끝

        private static void Fail(string message) // 실패 결과 기록
        { // 메서드 시작
            Debug.LogError("[NaturalCombatVerification] " + message); // 실패 로그
            Finish(1); // 실패 종료
        } // 메서드 끝

        private static void Finish(int code) // 검증 종료
        { // 메서드 시작
            if (trackedExperience != null) // 경험치 관리자 연결 확인
            { // 조건 시작
                trackedExperience.ExperienceChanged -= HandleExperienceChanged; // 경험치 이벤트 연결 해제
            } // 조건 끝
            if (keyboard != null) // 가상 키보드 확인
            { // 조건 시작
                InputSystem.RemoveDevice(keyboard); // 시험 장치 제거
            } // 조건 끝
            if (originalInputSettings != null) // 원래 입력 설정 확인
            { // 조건 시작
                InputSystem.settings = originalInputSettings; // 원래 설정 복구
                Application.runInBackground = originalRunInBackground; // 원래 백그라운드 상태 복구
            } // 조건 끝
            if (testInputSettings != null) // 시험 설정 사본 확인
            { // 조건 시작
                UnityEngine.Object.Destroy(testInputSettings); // 시험 설정 사본 제거
            } // 조건 끝
            SessionState.SetBool(ActiveKey, false); // 검증 표식 해제
            EditorApplication.update -= Tick; // 갱신 연결 해제
            EditorApplication.Exit(code); // 종료 결과 반환
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
