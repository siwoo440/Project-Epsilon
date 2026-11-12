using System; // 예외 형식
using System.Collections.Generic; // 처치 대상 기록
using System.Reflection; // 선택 UI 상태 조회
using ProjectEpsilon.Combat; // 무기 및 전투 대상
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Enemies; // 적 행동
using ProjectEpsilon.Player; // 플레이어 상태
using ProjectEpsilon.Progression; // 성장 및 보상
using ProjectEpsilon.Recording; // 선택적 전투 기록
using ProjectEpsilon.UI; // 선택 화면
using UnityEditor; // 편집기 제어
using UnityEditor.SceneManagement; // 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.InputSystem; // 가상 키보드
using UnityEngine.InputSystem.LowLevel; // 키보드 상태
using UnityEngine.UI; // 버튼 입력

namespace ProjectEpsilon.Editor // 편집기 영역
{
    [InitializeOnLoad] // 도메인 갱신 연결
    public static class AutomatedNaturalCombatVerification // 가상 입력 생존 검증
    {
        private const string ActiveKey = "EpsilonNaturalCombatVerification"; // 검증 진행 표식
        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 멤버 조회 설정
        private static Keyboard keyboard; // 가상 키보드
        private static InputSettings originalInputSettings; // 원래 입력 설정
        private static InputSettings testInputSettings; // 시험용 임시 입력 설정
        private static bool originalRunInBackground; // 원래 백그라운드 실행 상태
        private static int currentKeys = -1; // 이전 입력 조합
        private static bool checkedStart; // 시작 몸통 확인
        private static bool startingChoiceObserved; // 실제 시작 버튼 선택 확인
        private static int startingOptionIndex = 1; // 기본 시작 포탑 후보 번호
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
        private static int heldTurn; // 유지 중인 회전 방향
        private static int heldTargetId; // 회전 대상 구분값
        private static float holdEnd; // 회전 유지 종료 시각
        private static float nextHoldAllowed; // 다음 유지 회전 허용 시각
        private static int lockedTurn; // 여러 주기에 유지할 회전 방향
        private static int lockedTargetId; // 방향 유지 대상 구분값
        private static float directionLockEnd; // 방향 유지 최대 시각
        private static int naturalKills; // 자연 출현 적 처치 수
        private static bool experienceObserved; // 자연 경험치 획득 여부
        private static bool naturalProgressObserved; // 실제 성장과 보상 완료 여부
        private static SnakeExperience trackedExperience; // 경험치 이벤트 대상
        private static SnakeBodyManager trackedBody; // 몸통 변경 이벤트 대상
        private static int trackedBodyCount; // 직전 몸통 수
        private static int previousExperience; // 이전 경험치 값
        private static int previousLevel; // 이전 레벨 값
        private static float nextTacticalReport; // 다음 전투 거리 기록 시각
        private static bool randomSeedInitialized; // 실제 난수 시드 적용 여부
        private static float targetSurvivalSeconds = 300f; // 실제 생존 관찰 목표
        private static bool fiveMinuteCaptured; // 실제 5분 표본 저장 여부
        private static bool tenMinuteCaptured; // 실제 10분 표본 저장 여부
        private static bool fifteenMinuteCaptured; // 실제 15분 표본 저장 여부
        private static bool twentyMinuteCaptured; // 실제 20분 표본 저장 여부
        private static bool bossFightRequested; // 실제 보스전 연속 관찰 요청
        private static bool bossPhaseTwoObserved; // 실제 보스 2단계 관찰 여부
        private static double victoryObservedAt; // 실제 승리 화면 대기 시작 시각
        private static bool recordingLogged; // 실제 기록 중복 출력 여부
        private static bool automaticMergeRequested; // 자동 합성 시험 요청
        private static SnakeWeaponMergeController trackedMerge; // 실제 합성 결과 구독 대상
        private static float nextMergeScan; // 다음 합성 가능 검사 시각
        private static bool pilotMergeOpen; // 자동 조종기가 연 합성 여부
        private static int pilotMergeAttempts; // 자동 합성 시작 횟수
        private static int pilotMergeCompleted; // 자동 합성 완료 횟수
        private static int pilotMergeCancelled; // 자동 합성 취소 횟수

        static AutomatedNaturalCombatVerification() // 도메인 연결
        {
            if (SessionState.GetBool(ActiveKey, false)) // 진행 상태 확인
            {
                EditorApplication.update += Tick; // 검증 갱신 연결
            }
        }

        public static void Run() // 배치 진입점
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 검증 시작 기록
            EditorApplication.update -= Tick; // 중복 연결 해제
            EditorApplication.update += Tick; // 검증 갱신 연결
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        }

        private static void Tick() // 생존 관찰과 가상 입력
        {
            if (!EditorApplication.isPlaying) // 플레이 시작 확인
            {
                return; // 플레이 진입 대기
            }
            try // 검증 오류 처리
            {
                GameManager game = GameManager.Instance; // 현재 시작 상태 조회
                if (bossFightRequested && game != null && game.CurrentState == GameState.Victory) // 실제 보스전 승리 확인
                {
                    ObserveBossVictory(game); // 실제 결과 화면과 승리 기록 확인
                    return; // 일반 전투 관찰 중단
                }
                if (game != null && game.CurrentState == GameState.Starting) // 시작 선택 상태 확인
                {
                    if (!Mathf.Approximately(Time.timeScale, 0f)) // 시작 전 정지 확인
                    {
                        throw new InvalidOperationException("Starting selection did not freeze game time."); // 시작 시간 오류
                    }
                    RunFrontendController frontend = RunFrontendController.Instance; // 실제 시작 화면
                    if (frontend != null && frontend.Stage != FrontendStage.Weapons) // 포탑 선택 이전 단계 확인
                    {
                        StartingWeaponBatchHelper.HandleStartingSelection(); // 실제 메뉴 버튼으로 출격 단계 이동
                        return; // 다음 시작 단계 대기
                    }
                    if (!randomSeedInitialized) // 최초 시작 상태 확인
                    {
                        int seed = 4501; // 재현용 기본 난수 시드
                        string[] arguments = Environment.GetCommandLineArgs(); // 실제 배치 인수
                        automaticMergeRequested = Array.IndexOf(arguments, "-epsilonAutoMerge") >= 0; // 자동 합성 시험 옵션 확인
                        bossFightRequested = Array.IndexOf(arguments, "-epsilonFightBoss") >= 0; // 보스전 연속 관찰 옵션 확인
                        for (int index = 0; index + 1 < arguments.Length; index++) // 실제 배치 인수 순회
                        {
                            if (arguments[index] == "-epsilonSeed" && int.TryParse(arguments[index + 1], out int requestedSeed)) // 요청된 시드 확인
                            {
                                seed = requestedSeed; // 시험 시드 적용
                            }
                            if (arguments[index] == "-epsilonStartOption" && int.TryParse(arguments[index + 1], out int requestedOption) && requestedOption >= 1 && requestedOption <= 3) // 시작 포탑 후보 번호 확인
                            {
                                startingOptionIndex = requestedOption - 1; // 실제 배열 순서 저장
                            }
                            if (arguments[index] == "-epsilonSurvivalSeconds" && float.TryParse(arguments[index + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float requestedSeconds) && requestedSeconds >= 300f && requestedSeconds <= 1200f) // 실제 관찰 시간 범위 확인
                            {
                                targetSurvivalSeconds = requestedSeconds; // 실제 관찰 목표 적용
                            }
                        }
                        if (bossFightRequested) // 실제 보스전 관찰 요청 확인
                        {
                            targetSurvivalSeconds = 1200f; // 확정된 20분 보스 진입 목표
                        }
                        UnityEngine.Random.InitState(seed); // 실제 게임 난수 초기화
                        randomSeedInitialized = true; // 중복 시드 적용 방지
                        Debug.Log($"[NaturalCombatVerification] seed={seed}, fixedDelta={Time.fixedDeltaTime:F3}, pilot=range-aware, target={targetSurvivalSeconds:F0}s."); // 재현 조건 기록
                    }
                    StartingWeaponSelection selection = UnityEngine.Object.FindFirstObjectByType<StartingWeaponSelection>(); // 실제 시작 선택기
                    StartingWeaponOptions options = selection == null ? null : (StartingWeaponOptions)typeof(StartingWeaponSelection).GetField("options", PrivateInstance).GetValue(selection); // 실제 후보 데이터
                    if (selection != null && selection.IsSelecting) // 실제 시작 후보 선택 상태 확인
                    {
                        ClickLevelButton(startingOptionIndex); // 지정된 실제 시작 후보 버튼 선택
                    }
                    if (game.IsPlaying) // 버튼 선택 완료 확인
                    {
                        SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 장착 관리자
                        if (options == null || options.Weapons == null || options.Weapons.Length <= startingOptionIndex || options.Weapons[startingOptionIndex] == null || (startingOptionIndex == 1 && options.Weapons[startingOptionIndex].Id != "debug_needle") || weapons == null || weapons.Slots.Count == 0 || weapons.Slots[0].Weapon != options.Weapons[startingOptionIndex] || weapons.OccupiedSlotCount != 1) // 지정된 후보와 기존 기본 후보의 실제 장착 확인
                        {
                            throw new InvalidOperationException("Requested starting weapon button did not equip exactly one turret."); // 시작 장착 오류
                        }
                        startingChoiceObserved = true; // 시작 선택 검증 기록
                        Debug.Log($"[NaturalCombatVerification] starting button={startingOptionIndex + 1} weapon={weapons.Slots[0].Weapon.DisplayName} grade={weapons.Slots[0].Grade} occupied={weapons.OccupiedSlotCount} range={weapons.Slots[0].EffectiveRange:F2} scale={Time.timeScale:F1}"); // 실제 버튼 선택 결과 기록
                    }
                    return; // 다음 갱신부터 전투 관찰
                }
                if (game != null && game.IsPlaying && !startingChoiceObserved) // 시작 버튼 선택 누락 확인
                {
                    throw new InvalidOperationException("Game started without observing the starting weapon button."); // 선택 누락 오류
                }
                if (startWallTime == 0d) // 최초 전투 갱신 확인
                {
                    startWallTime = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
                }
                TickPlaying(); // 실제 게임 상태 처리
            }
            catch (Exception exception) // 예외 발생 확인
            {
                Fail(exception.ToString()); // 예외 기록과 종료
            }
        }

        private static void TickPlaying() // 게임 상태 검증
        {
            GameManager game = GameManager.Instance; // 게임 관리자
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 적 출현기
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 플레이어 이동
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 몸통 관리자
            SnakeHealth health = UnityEngine.Object.FindFirstObjectByType<SnakeHealth>(); // 체력 관리자
            SnakeExperience experience = UnityEngine.Object.FindFirstObjectByType<SnakeExperience>(); // 경험치 관리자
            if (game == null || spawner == null || player == null || body == null || health == null || experience == null) // 필수 참조 확인
            {
                return; // 초기화 대기
            }
            if (trackedExperience == null) // 경험치 이벤트 연결 확인
            {
                trackedExperience = experience; // 실제 경험치 관리자 저장
                previousExperience = experience.CurrentExperience; // 초기 경험치 저장
                previousLevel = experience.CurrentLevel; // 초기 레벨 저장
                experience.ExperienceChanged += HandleExperienceChanged; // 경험치 변경 구독
            }
            if (trackedBody == null) // 몸통 이벤트 연결 확인
            {
                trackedBody = body; // 실제 몸통 관리자 저장
                trackedBodyCount = body.CurrentBodyCount; // 초기 몸통 수 저장
                body.BodyCountChanged += HandleBodyCountChanged; // 실제 몸통 변경 구독
            }
            if (keyboard == null) // 가상 키보드 확인
            {
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
            }
            float elapsed = (float)typeof(EarlyEnemyWaveSpawner).GetField("elapsed", PrivateInstance).GetValue(spawner); // 전투 경과 시간
            if (bossFightRequested && game.IsFinalBossPhase) // 실제 보스전 진행 확인
            {
                PrototypeBossEncounter bossEncounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 출현기 조회
                bossPhaseTwoObserved |= bossEncounter != null && bossEncounter.IsPhaseTwo; // 실제 보스 2단계 관찰
            }
            ObserveState(player, body, health, elapsed); // 손실 및 피해 상태 기록
            if (!checkedStart && elapsed >= 0.25f) // 첫 물리 갱신 이후 확인
            {
                checkedStart = true; // 시작 검증 기록
                if (body.CurrentBodyCount != 3) // 기획 시작 몸통 확인
                {
                    Fail($"initial body={body.CurrentBodyCount}, expected 3"); // 시작 손실 기록
                    return; // 추가 처리 중단
                }
                Debug.Log("[NaturalCombatVerification] Starting three bodies retained after physics."); // 시작 상태 기록
            }
            EarlyEnemyBehaviour[] enemies = UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None); // 현재 적 목록
            ObserveEnemies(enemies); // 출현 종류 기록
            UpdateFocus(enemies); // 실제 적 추적 대상 갱신
            if (elapsed >= nextTacticalReport) // 전투 거리 기록 간격 확인
            {
                LogTacticalState(enemies, elapsed); // 실제 무기와 적 거리 기록
                LogGemState(player, enemies, elapsed); // 보석별 실제 좌표와 입력 기록
                nextTacticalReport = elapsed + 5f; // 다음 기록 시각 저장
            }
            experienceObserved |= experience.CurrentExperience > 0 || experience.CurrentLevel > 1; // 자연 경험치 확인
            SnakeLevelUpController level = UnityEngine.Object.FindFirstObjectByType<SnakeLevelUpController>(); // 레벨업 처리기
            EliteRewardController elite = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 엘리트 보상 처리기
            SnakeWeaponMergeController merge = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponMergeController>(); // 합성 처리기
            if (automaticMergeRequested && trackedMerge == null && merge != null) // 실제 합성 결과 구독 확인
            {
                trackedMerge = merge; // 실제 합성 처리기 저장
                trackedMerge.MergeClosed += HandlePilotMergeClosed; // 실제 합성 완료와 취소 관찰
            }
            bool mergeReady = false; // 현재 자동 합성 가능 여부
            if (automaticMergeRequested && merge != null && elapsed >= nextMergeScan) // 실제 합성 가능 검사 시각 확인
            {
                nextMergeScan = elapsed + 2f; // 다음 합성 가능 검사 시각
                mergeReady = merge.HasMergeCandidates(); // 실제 같은 포탑과 등급 확인
            }
            if (level != null && level.IsPresentingLevelUp) // 레벨업 선택 상태
            {
                SetKeys(0); // 이동 입력 해제
                bool replacement = (bool)typeof(SnakeLevelUpController).GetField("selectingReplacement", PrivateInstance).GetValue(level); // 교체 선택 단계 확인
                ClickLevelButton(replacement ? 1 : 0); // 첫 후보 또는 교체 확정
                levelSelections++; // 선택 횟수 증가
            }
            else if (elite != null && elite.IsPresentingReward) // 엘리트 보상 선택 상태
            {
                SetKeys(0); // 이동 입력 해제
                bool replacement = (bool)typeof(EliteRewardController).GetField("selectingReplacement", PrivateInstance).GetValue(elite); // 교체 선택 단계 확인
                if (replacement) // 무기 교체 단계
                {
                    ClickLevelButton(1); // 현재 교체 확정
                }
                else // 일반 보상 선택 단계
                {
                    ClickFirstAvailableLevelButton(); // 활성 보상 선택
                }
                eliteSelections++; // 선택 횟수 증가
            }
            else if (merge != null && merge.IsMergeOpen) // 보상 합성 화면 상태
            {
                SetKeys(0); // 이동 입력 해제
                ClickFirstAvailableMergeButton(); // 첫 합성 또는 강화 선택
            }
            else if (game.IsPlaying && mergeReady && TryStartAutomaticMerge(merge)) // 실제 자동 합성 진입
            {
                SetKeys(0); // 합성 중 가상 이동 입력 해제
                pilotMergeOpen = true; // 자동 조종기의 합성 진행 기록
                pilotMergeAttempts++; // 실제 합성 시작 횟수 증가
                Debug.Log($"[NaturalCombatVerification] automatic merge started time={elapsed:F2} attempt={pilotMergeAttempts}."); // 실제 자동 합성 진입 기록
            }
            else if (game.IsPlaying) // 일반 플레이 상태
            {
                SnakeStamina stamina = player.GetComponent<SnakeStamina>(); // 가속 자원 조회
                InputBindingManager bindings = InputBindingManager.Instance; // 실제 입력 상태 조회
                leftObserved |= bindings != null && bindings.TurnLeftPressed; // 왼쪽 입력 반영 확인
                rightObserved |= bindings != null && bindings.TurnRightPressed; // 오른쪽 입력 반영 확인
                boostObserved |= stamina != null && stamina.IsBoosting; // 가속 반영 확인
                ObserveMotion(player, bindings, stamina, elapsed); // 실제 회전 및 가속 이동 확인
                if (elapsed < 2.2f && elapsed >= nextInputReport) // 초기 입력 진단 간격 확인
                {
                    LogInputDiagnostics(bindings, stamina, elapsed); // 키 상태와 입력 액션 기록
                    nextInputReport = elapsed + 0.15f; // 다음 진단 시각 저장
                }
                if (elapsed < 0.3f) // 초기 몸통 상태 확인
                {
                    SetKeys(0); // 첫 물리 갱신 대기
                }
                else // 위험 회피 입력
                {
                    if (lockedTurn != 0 && (focusTarget == null || !focusTarget.IsAlive || focusTarget.GetInstanceID() != lockedTargetId || elapsed >= directionLockEnd)) // 대상 또는 방향 유지 시간 확인
                    {
                        Debug.Log($"[NaturalCombatVerification] direction lock end time={elapsed:F2} reason=target-or-time"); // 방향 유지 종료 기록
                        lockedTurn = 0; // 기존 회전 방향 해제
                        heldTurn = 0; // 진행 중 회전 해제
                        nextDecision = elapsed; // 즉시 새 경로 판단
                    }
                    if (heldTurn != 0) // 유지 중인 회전 확인
                    {
                        bool targetLost = focusTarget == null || !focusTarget.IsAlive || focusTarget.GetInstanceID() != heldTargetId; // 기존 대상 유지 확인
                        bool danger = !targetLost && IsCommittedTurnUnsafe(player, body, enemies, heldTurn); // 예측 접촉 위험 확인
                        if (targetLost || danger || elapsed >= holdEnd) // 유지 종료 조건 확인
                        {
                            Debug.Log($"[NaturalCombatVerification] turn hold end time={elapsed:F2} reason={(targetLost ? "target" : danger ? "danger" : "complete")}"); // 유지 종료 사유 기록
                            heldTurn = 0; // 유지 회전 해제
                            if (targetLost || danger) // 중단 사유 확인
                            {
                                lockedTurn = 0; // 위험 또는 대상 변경 시 방향 해제
                            }
                            nextHoldAllowed = elapsed + 0.65f; // 다음 회전 대기
                            nextDecision = elapsed; // 즉시 새 경로 판단
                        }
                    }
                    if (heldTurn != 0) // 안전한 유지 회전 확인
                    {
                        SetKeys(heldTurn < 0 ? 1 : 2); // 기존 방향과 기본 전진 유지
                    }
                    else if (lockedTurn != 0 && elapsed < nextHoldAllowed && !IsCommittedTurnUnsafe(player, body, enemies, 0)) // 안전한 회전 간 직진 확인
                    {
                        SetKeys(0); // 가속 없는 직진 유지
                    }
                    else if (elapsed >= nextDecision || lockedTurn != 0 && elapsed < nextHoldAllowed) // 새 조향 또는 직진 위험 확인
                    {
                        if (lockedTurn != 0 && elapsed < nextHoldAllowed) // 직진 위험 확인
                        {
                            Debug.Log($"[NaturalCombatVerification] direction lock end time={elapsed:F2} reason=straight-danger"); // 위험 종료 기록
                            lockedTurn = 0; // 위험 시 방향 유지 해제
                        }
                        nextKeys = ChooseKeys(player, body, enemies, stamina, elapsed); // 위험 회피 조향 계산
                        nextDecision = elapsed + 0.12f; // 다음 조향 시각 저장
                        SetKeys(nextKeys); // 새 가상 조향 입력
                    }
                    else // 기존 판단 유지
                    {
                        SetKeys(nextKeys); // 이전 가상 조향 입력
                    }
                }
            }
            if (elapsed >= nextReport) // 상태 기록 간격 확인
            {
                Debug.Log($"[NaturalCombatVerification] time={elapsed:F1} wall={EditorApplication.timeSinceStartup - startWallTime:F1} state={game.CurrentState} body={body.CurrentBodyCount} hp={health.CurrentHealth} level={experience.CurrentLevel} xp={experience.CurrentExperience} kills={naturalKills} gems={UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length} enemies={spawner.ActiveEnemies} levelChoices={levelSelections} eliteChoices={eliteSelections} inputs={leftObserved},{rightObserved},{boostObserved} motion={leftRotationObserved},{rightRotationObserved},{boostMovementObserved} scale={Time.timeScale:F1}"); // 전투 및 성장 상태 기록
                nextReport += 30f; // 다음 기록 시각
            }
            if (game.CurrentState == GameState.Defeated) // 패배 상태 확인
            {
                Fail($"defeated at {elapsed:F1} seconds; level={experience.CurrentLevel}; hp={health.CurrentHealth}; body={body.CurrentBodyCount}"); // 패배 결과 기록
                return; // 추가 처리 중단
            }
            if (game.IsPlaying && (merge == null || !merge.IsMergeOpen) && !Mathf.Approximately(Time.timeScale, 1f)) // 일반 게임 속도 확인
            {
                Fail($"unexpected time scale {Time.timeScale}"); // 속도 차이 기록
                return; // 추가 처리 중단
            }
            if (!naturalProgressObserved && naturalKills > 0 && experienceObserved && experience.CurrentLevel >= 2 && levelSelections > 0 && game.IsPlaying) // 자연 처치와 보상 완료 확인
            {
                naturalProgressObserved = true; // 실제 성장 완료 기록
                Debug.Log($"[NaturalCombatVerification] progression observed kills={naturalKills} level={experience.CurrentLevel} xp={experience.CurrentExperience} choices={levelSelections} body={body.CurrentBodyCount} hp={health.CurrentHealth} game={elapsed:F1}s wall={EditorApplication.timeSinceStartup - startWallTime:F1}s."); // 자연 전투 성장 확인
            }
            if (!fiveMinuteCaptured && elapsed >= 300f && game.IsPlaying && (merge == null || !merge.IsMergeOpen)) // 실제 5분 표본 시점 확인
            {
                RunTelemetry telemetry = RunTelemetry.Instance; // 선택적 실제 전투 기록기
                if (telemetry != null && telemetry.Current != null) // 실제 기록 활성 확인
                {
                    telemetry.Capture("five_minutes"); // 실제 5분 상태 표본
                }
                fiveMinuteCaptured = true; // 실제 5분 표본 중복 방지
            }
            if (!tenMinuteCaptured && elapsed >= 600f && game.IsPlaying && (merge == null || !merge.IsMergeOpen)) // 실제 10분 표본 시점 확인
            {
                RunTelemetry telemetry = RunTelemetry.Instance; // 선택적 실제 전투 기록기
                if (telemetry != null && telemetry.Current != null) // 실제 기록 활성 확인
                {
                    telemetry.Capture("ten_minutes"); // 실제 10분 상태 표본
                }
                tenMinuteCaptured = true; // 실제 10분 표본 중복 방지
            }
            if (!fifteenMinuteCaptured && elapsed >= 900f && game.IsPlaying && (merge == null || !merge.IsMergeOpen)) // 실제 15분 표본 시점 확인
            {
                RunTelemetry telemetry = RunTelemetry.Instance; // 선택적 실제 전투 기록기
                if (telemetry != null && telemetry.Current != null) // 실제 기록 활성 확인
                {
                    telemetry.Capture("fifteen_minutes"); // 실제 15분 상태 표본
                }
                fifteenMinuteCaptured = true; // 실제 15분 표본 중복 방지
            }
            if (elapsed >= targetSurvivalSeconds && game.IsPlaying && (merge == null || !merge.IsMergeOpen)) // 실제 전투 재개 및 목표 생존 시간 확인
            {
                if (targetSurvivalSeconds >= 1200f && (!spawner.HasEnteredBossTransition || !game.IsFinalBossPhase)) // 실제 20분 보스 전환 확인
                {
                    Fail("Twenty-minute survival reached without the final boss transition."); // 보스 전환 누락 기록
                    return; // 성공 처리 중단
                }
                RunTelemetry telemetry = RunTelemetry.Instance; // 선택적 실제 전투 기록기
                if (!twentyMinuteCaptured && targetSurvivalSeconds >= 1200f) // 실제 20분 표본 중복 확인
                {
                    if (telemetry != null && telemetry.Current != null) // 실제 기록 활성 확인
                    {
                        telemetry.Capture("twenty_minutes"); // 실제 20분 상태 표본
                    }
                    twentyMinuteCaptured = true; // 실제 20분 표본 중복 방지
                }
                if (bossFightRequested) // 실제 보스전 연속 관찰 확인
                {
                    if (elapsed > 1320f) // 실제 보스전 추가 시간 제한
                    {
                        Fail($"boss fight timed out at {elapsed:F1} seconds; phaseTwo={bossPhaseTwoObserved}"); // 실제 보스전 미종료 기록
                    }
                    return; // 실제 보스전 계속 진행
                }
                if (telemetry != null && telemetry.Current != null) // 실제 기록 활성 확인
                {
                    LogRecording(); // 실제 관측 기록 출력
                }
                if (!naturalProgressObserved) // 실제 성장 누락 확인
                {
                    Fail($"natural progression absent at {targetSurvivalSeconds:F0}s: kills={naturalKills} level={experience.CurrentLevel} xp={experience.CurrentExperience} choices={levelSelections}"); // 성장 미발생 기록
                    return; // 추가 처리 중단
                }
                if (!sawElite) // 실제 엘리트 출현 누락 확인
                {
                    Fail("No elite observed by five minutes."); // 출현 누락 기록
                    return; // 성공 처리 중단
                }
                Debug.Log($"[NaturalCombatVerification] PASS survival target={targetSurvivalSeconds:F0}s kills={naturalKills} level={experience.CurrentLevel} xp={experience.CurrentExperience} choices={levelSelections} sawElite={sawElite} eliteChoices={eliteSelections} mergeAttempts={pilotMergeAttempts} merges={pilotMergeCompleted} mergeCancellations={pilotMergeCancelled} body={body.CurrentBodyCount} hp={health.CurrentHealth} game={elapsed:F1}s wall={EditorApplication.timeSinceStartup - startWallTime:F1}s."); // 자연 성장과 합성 기록
                Finish(0); // 성공 종료
                return; // 추가 처리 중단
            }
            if (EditorApplication.timeSinceStartup - startWallTime > targetSurvivalSeconds * 2d + 180d) // 병렬 시험을 포함한 실제 시간 제한 확인
            {
                Fail($"timed out at {elapsed:F1} seconds"); // 시간 초과 기록
            }
        }

        private static void ObserveBossVictory(GameManager game) // 실제 보스전 승리와 결과 화면 확인
        {
            if (victoryObservedAt <= 0d) // 실제 승리 최초 관찰 확인
            {
                victoryObservedAt = EditorApplication.timeSinceStartup; // 실제 결과 화면 대기 시작
            }
            RunFrontendController frontend = RunFrontendController.Instance; // 실제 결과 화면 관리자
            GameObject titleObject = GameObject.Find("FrontendTitle"); // 실제 결과 제목 객체
            Text title = titleObject == null ? null : titleObject.GetComponent<Text>(); // 실제 결과 제목 문구
            if (frontend == null || frontend.Stage != FrontendStage.Results || title == null || title.text != "승리") // 실제 승리 화면 생성 확인
            {
                if (EditorApplication.timeSinceStartup - victoryObservedAt > 10d) // 실제 결과 화면 대기 제한
                {
                    Fail("Victory result screen did not appear."); // 실제 결과 화면 누락 기록
                }
                return; // 실제 결과 화면 대기
            }
            if (!twentyMinuteCaptured || !bossPhaseTwoObserved || !game.IsFinalBossPhase || !naturalProgressObserved || !sawElite) // 실제 성장과 보스전 단계 관찰 확인
            {
                Fail($"victory missed required observation: twenty={twentyMinuteCaptured}, phaseTwo={bossPhaseTwoObserved}, boss={game.IsFinalBossPhase}, progression={naturalProgressObserved}, elite={sawElite}"); // 실제 단계 누락 기록
                return; // 성공 처리 중단
            }
            LogRecording(); // 실제 보스 처치와 결과 기록 출력
            Debug.Log($"[NaturalCombatVerification] PASS boss victory phaseTwo={bossPhaseTwoObserved} kills={naturalKills} eliteChoices={eliteSelections} merges={pilotMergeCompleted} wall={EditorApplication.timeSinceStartup - startWallTime:F1}s."); // 실제 보스전 성공 기록
            Finish(0); // 실제 결과 화면 검증 종료
        }

        private static int ChooseKeys(SnakeMovement player, SnakeBodyManager body, EarlyEnemyBehaviour[] enemies, SnakeStamina stamina, float elapsed) // 회피 조향 선택
        {
            Vector2 origin = player.transform.position; // 플레이어 위치
            Vector2 heading = player.transform.up; // 현재 전진 방향
            int bestKeys = 0; // 기본 직진 선택
            float bestScore = float.PositiveInfinity; // 최소 위험 점수
            bool bestCurve = false; // 기본 경로 선택
            for (int boost = 0; boost <= 1; boost++) // 가속 사용 여부 비교
            {
                if (boost == 1 && (stamina == null || stamina.CurrentStamina < 30f)) // 가속 자원 확인
                {
                    continue; // 가속 후보 제외
                }
                for (int turn = -1; turn <= 1; turn++) // 왼쪽 직진 오른쪽 비교
                {
                    int curveCount = 1; // 무제한 사거리의 기본 경로
                    for (int curve = 0; curve < curveCount; curve++) // 일반 및 곡선 예측 비교
                    {
                        float score = ScoreTrajectory(origin, heading, player, body, enemies, turn, boost == 1, curve == 1); // 후보 경로 위험 계산
                        if (score < bestScore) // 더 안전한 경로 확인
                        {
                            bestScore = score; // 최소 위험 저장
                            bestKeys = (turn < 0 ? 1 : turn > 0 ? 2 : 0) | (boost == 1 ? 4 : 0); // 조향과 가속 저장
                            bestCurve = curve == 1; // 곡선 예측 선택 저장
                        }
                    }
                }
            }
            selectedCurve = bestCurve; // 최근 곡선 선택 기록
            selectedTrajectoryScore = bestScore; // 최근 경로 점수 기록
            return bestKeys; // 선택 입력 반환
        }

        private static float EstimateEnemySpeed(EarlyEnemyBehaviour enemy, SnakeMovement player) // 시험용 적 속도 추정
        {
            FieldInfo field = typeof(EarlyEnemyBehaviour).GetField("settings", PrivateInstance); // 적별 실제 설정 필드
            EarlyEnemySettings settings = field == null ? null : field.GetValue(enemy) as EarlyEnemySettings; // 적별 실제 설정
            WeaponTargetStatusController status = enemy.GetComponent<WeaponTargetStatusController>(); // 적 이동 상태
            float ratio = settings == null ? 0.8f : Mathf.Max(0f, settings.speedRatio); // 설정 또는 기본 속도 비율
            float multiplier = status == null ? 1f : status.CurrentMovementMultiplier; // 상태 효과 이동 배율
            return player.BaseMoveSpeed * ratio * multiplier * EnemyMovementRules.SpeedMultiplier; // 실제 기본 속도와 절반 배율
        }

        private static bool IsCommittedTurnUnsafe(SnakeMovement player, SnakeBodyManager body, EarlyEnemyBehaviour[] enemies, int turn) // 회전 중 접촉 위험 예측
        {
            Vector2 point = player.transform.position; // 예상 머리 시작 위치
            Vector2 forward = player.transform.up; // 예상 전진 방향
            CircleCollider2D headCollider = player.GetComponent<CircleCollider2D>(); // 실제 머리 충돌체
            SnakeBodyFollower follower = body.GetComponent<SnakeBodyFollower>(); // 실제 몸통 간격 조회
            float spacing = follower != null ? follower.SegmentSpacing : 0.58f; // 실제 간격과 기존 대체값
            Vector2[] predicted = new Vector2[enemies.Length]; // 적별 예상 위치
            for (int index = 0; index < enemies.Length; index++) // 실제 적 위치 순회
            {
                predicted[index] = enemies[index] == null ? point : (Vector2)enemies[index].transform.position; // 적 초기 위치 저장
            }
            const float step = 0.07f; // 짧은 예측 간격
            for (int tick = 1; tick <= 5; tick++) // 유지 회전 시간 예측
            {
                float angle = -turn * player.TurnSpeed * step * Mathf.Deg2Rad; // 회전 각도 계산
                float cosine = Mathf.Cos(angle); // 회전 코사인
                float sine = Mathf.Sin(angle); // 회전 사인
                forward = new Vector2(forward.x * cosine - forward.y * sine, forward.x * sine + forward.y * cosine); // 예상 방향 계산
                point += forward * player.MoveSpeed * step; // 기본 속도 예상 위치
                for (int enemyIndex = 0; enemyIndex < enemies.Length; enemyIndex++) // 적 접촉 위험 순회
                {
                    if (enemies[enemyIndex] == null) // 소멸 적 확인
                    {
                        continue; // 다음 적 확인
                    }
                    Vector2 chase = point - predicted[enemyIndex]; // 적의 예상 추적 방향
                    predicted[enemyIndex] += chase.normalized * EstimateEnemySpeed(enemies[enemyIndex], player) * step; // 실제 설정 기준 추적 위치 예측
                    CircleCollider2D enemyCollider = enemies[enemyIndex].GetComponent<CircleCollider2D>(); // 실제 적 충돌체
                    if (Vector2.Distance(point, predicted[enemyIndex]) < Mathf.Max(1f, ContactDistance(headCollider, enemyCollider) + 0.52f)) // 실제 머리 접촉 위험 확인
                    {
                        return true; // 유지 회전 차단
                    }
                    for (int index = 0; index < body.BodySegments.Count; index++) // 몸통 접촉 위험 순회
                    {
                        SnakeSegment segment = body.BodySegments[index]; // 실제 몸통 조회
                        if (segment == null) // 제거된 몸통 확인
                        {
                            continue; // 다음 몸통 확인
                        }
                        Vector2 future = point - forward * (spacing * (index + 1)); // 실제 간격 기준 예상 몸통 위치
                        float delay = spacing * (index + 1) / Mathf.Max(0.1f, player.MoveSpeed); // 실제 간격 기준 몸통 이동 지연
                        Vector2 estimated = Vector2.Lerp(segment.transform.position, future, Mathf.Clamp01(step * tick / delay)); // 기존 위치와 예측 위치 보간
                        if (Vector2.Distance(estimated, predicted[enemyIndex]) < Mathf.Max(0.8f, ContactDistance(segment.GetComponent<CircleCollider2D>(), enemyCollider) + 0.33f)) // 실제 몸통 접촉 위험 확인
                        {
                            return true; // 유지 회전 차단
                        }
                    }
                }
            }
            return false; // 예측 위험 없음
        }

        private static float ScoreTrajectory(Vector2 origin, Vector2 heading, SnakeMovement player, SnakeBodyManager body, EarlyEnemyBehaviour[] enemies, int turn, bool boost, bool reacquireCurve) // 회피 경로 평가
        {
            Vector2 point = origin; // 경로 위치 초기화
            Vector2 forward = heading; // 경로 방향 초기화
            CircleCollider2D headCollider = player.GetComponent<CircleCollider2D>(); // 실제 머리 충돌체
            SnakeBodyFollower follower = body.GetComponent<SnakeBodyFollower>(); // 실제 몸통 간격 조회
            float spacing = follower != null ? follower.SegmentSpacing : 0.58f; // 실제 간격과 기존 대체값
            float score = (turn == 0 ? 0f : 1f) + (boost ? 0.35f : 0f); // 불필요한 회전과 가속 억제
            float expectedSpeed = player.MoveSpeed * (boost ? 1.5f : 1f); // 후보별 예상 이동 속도
            Vector2[] predictedEnemies = new Vector2[enemies.Length]; // 적별 예상 위치
            for (int index = 0; index < enemies.Length; index++) // 초기 적 위치 순회
            {
                predictedEnemies[index] = enemies[index] == null ? origin : (Vector2)enemies[index].transform.position; // 실제 위치 저장
            }
            int focusIndex = -1; // 추적 대상 위치 초기화
            for (int index = 0; index < enemies.Length; index++) // 실제 적 목록 순회
            {
                if (enemies[index] != null && enemies[index].GetComponent<WeaponTarget>() == focusTarget) // 추적 대상 일치 확인
                {
                    focusIndex = index; // 대상 위치 저장
                    break; // 추가 탐색 종료
                }
            }
            Vector2 initialOffset = focusIndex < 0 ? Vector2.zero : predictedEnemies[focusIndex] - origin; // 초기 적 방향
            Vector2 flank = initialOffset.sqrMagnitude < 0.001f ? Vector2.zero : new Vector2(-initialOffset.y, initialOffset.x).normalized * turn; // 선택 회전의 측면 방향
            ExperiencePickup[] gems = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None); // 자연 드롭 보석 조회
            const float step = 0.16f; // 예상 경로 시간 간격
            for (int tick = 1; tick <= 8; tick++) // 짧은 미래 경로 비교
            {
                float angle = -turn * player.TurnSpeed * step * Mathf.Deg2Rad; // 회전 각도 계산
                if (reacquireCurve && focusIndex >= 0 && tick > 1) // 곡선 예측 후속 단계 확인
                {
                    Vector2 waypoint = predictedEnemies[focusIndex] + flank; // 예측 적의 측면 지점
                    Vector2 desired = waypoint - point; // 측면 접근 방향
                    if (desired.sqrMagnitude > 0.001f) // 목표 방향 확인
                    {
                        float desiredAngle = Vector2.SignedAngle(forward, desired); // 목표까지 회전 각도
                        angle = Mathf.Clamp(desiredAngle, -player.TurnSpeed * step, player.TurnSpeed * step) * Mathf.Deg2Rad; // 실제 회전 속도 범위 적용
                    }
                }
                float cosine = Mathf.Cos(angle); // 회전 코사인
                float sine = Mathf.Sin(angle); // 회전 사인
                forward = new Vector2(forward.x * cosine - forward.y * sine, forward.x * sine + forward.y * cosine); // 예상 전진 방향
                point += forward * expectedSpeed * step; // 예상 머리 위치
                for (int enemyIndex = 0; enemyIndex < enemies.Length; enemyIndex++) // 적 위험 순회
                {
                    EarlyEnemyBehaviour enemy = enemies[enemyIndex]; // 현재 적 조회
                    if (enemy == null) // 소멸 적 확인
                    {
                        continue; // 다음 적 확인
                    }
                    Vector2 enemyPosition = predictedEnemies[enemyIndex]; // 이전 적 예상 위치
                    Vector2 chase = point - enemyPosition; // 예상 머리 방향
                    Vector2 predicted = enemyPosition + chase.normalized * EstimateEnemySpeed(enemy, player) * step; // 실제 설정 기준 적 위치 예측
                    predictedEnemies[enemyIndex] = predicted; // 다음 예상 위치 저장
                    float distance = Vector2.Distance(point, predicted); // 예상 접근 거리
                    CircleCollider2D enemyCollider = enemy.GetComponent<CircleCollider2D>(); // 실제 적 충돌체
                    float headContact = ContactDistance(headCollider, enemyCollider); // 실제 머리 접촉 거리
                    if (distance < Mathf.Max(1.6f, headContact + 1.12f)) // 실제 크기 기준 근접 위험 확인
                    {
                        score += 0.3f / (distance * distance + 0.15f); // 근접 위험 누적
                    }
                    if (distance < Mathf.Max(0.75f, headContact + 0.27f)) // 실제 크기 기준 머리 충돌 위험 확인
                    {
                        score += 40f; // 충돌 위험 가중
                    }
                    for (int index = 0; index < body.BodySegments.Count; index++) // 실제 몸통별 위험 확인
                    {
                        SnakeSegment segment = body.BodySegments[index]; // 현재 몸통 조회
                        if (segment == null) // 제거된 몸통 확인
                        {
                            continue; // 다음 몸통 확인
                        }
                        Vector2 futureSegment = point - forward * (spacing * (index + 1)); // 실제 간격 기준 예측 몸통 위치
                        float travelTime = spacing * (index + 1) / Mathf.Max(0.1f, player.MoveSpeed); // 실제 간격 기준 몸통 이동 지연
                        float travelFraction = Mathf.Clamp01(step * tick / travelTime); // 예상 경로 반영 비율
                        Vector2 estimatedSegment = Vector2.Lerp(segment.transform.position, futureSegment, travelFraction); // 현재 몸통과 예상 몸통 보간
                        float bodyDistance = Vector2.Distance(estimatedSegment, predicted); // 적과 예상 몸통 거리
                        float bodyContact = ContactDistance(segment.GetComponent<CircleCollider2D>(), enemyCollider); // 실제 몸통 접촉 거리
                        if (bodyDistance < Mathf.Max(1.3f, bodyContact + 0.83f)) // 실제 크기 기준 몸통 근접 위험 확인
                        {
                            score += 1.2f / (bodyDistance * bodyDistance + 0.15f); // 근접 공격 위험 반영
                        }
                        if (bodyDistance < Mathf.Max(0.8f, bodyContact + 0.33f)) // 실제 크기 기준 몸통 접촉 위험 확인
                        {
                            score += 25f; // 몸통 피해 위험 가중
                        }
                    }
                }
                float nearestGem = 30f; // 가장 가까운 보석 거리
                foreach (ExperiencePickup gem in gems) // 자연 드롭 보석 순회
                {
                    if (gem != null) // 유효한 보석 확인
                    {
                        nearestGem = Mathf.Min(nearestGem, Vector2.Distance(point, gem.transform.position)); // 최단 보석 거리 갱신
                    }
                }
                if (gems.Length > 0 && nearestGem < 30f) // 접근 가능한 보석 확인
                {
                    score += nearestGem * 0.8f; // 적 접촉 위험과 함께 보석 접근 평가
                }
                for (int index = 2; index < body.BodySegments.Count; index++) // 뒤쪽 몸통 순회
                {
                    SnakeSegment segment = body.BodySegments[index]; // 몸통 조회
                    if (segment != null && Vector2.Distance(point, segment.transform.position) < Mathf.Max(0.65f, ContactDistance(headCollider, segment.GetComponent<CircleCollider2D>()) + 0.2f)) // 실제 크기 기준 자기 충돌 위험 확인
                    {
                        score += 20f; // 자기 충돌 위험 가중
                    }
                }
            }
            return score; // 예상 위험 반환
        }

        private static float ContactDistance(CircleCollider2D first, CircleCollider2D second) // 실제 원형 충돌체 접촉 거리
        {
            return WorldRadius(first) + WorldRadius(second); // 두 충돌 반경 합계
        }
        private static float WorldRadius(CircleCollider2D collider) // 실제 배율을 적용한 충돌 반경
        {
            if (collider == null || !collider.enabled) // 유효 충돌체 확인
            {
                return 0f; // 없는 충돌체 제외
            }
            Vector3 scale = collider.transform.lossyScale; // 실제 월드 배율
            return collider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)); // 원형 충돌체 월드 반경
        }
        private static bool ThreatBehind(Vector2 origin, Vector2 heading, EarlyEnemyBehaviour[] enemies) // 뒤쪽 근접 위험 확인
        {
            foreach (EarlyEnemyBehaviour enemy in enemies) // 적 순회
            {
                if (enemy == null) // 소멸 적 확인
                {
                    continue; // 다음 적 확인
                }
                Vector2 offset = (Vector2)enemy.transform.position - origin; // 적까지 거리
                if (offset.sqrMagnitude < 9f && Vector2.Dot(heading, offset.normalized) < -0.3f) // 뒤쪽 근접 확인
                {
                    return true; // 가속 필요 반환
                }
            }
            return false; // 가속 불필요 반환
        }

        private static void SetKeys(int keys) // 가상 키 상태 전송
        {
            if (keyboard == null || keys == currentKeys) // 키보드 및 상태 변경 확인
            {
                return; // 중복 전송 생략
            }
            currentKeys = keys; // 현재 입력 저장
            KeyboardState state = new KeyboardState(); // 키 상태 초기화
            if ((keys & 1) != 0) // 왼쪽 입력 확인
            {
                state.Press(Key.A); // A 키 상태 설정
            }
            if ((keys & 2) != 0) // 오른쪽 입력 확인
            {
                state.Press(Key.D); // D 키 상태 설정
            }
            if ((keys & 4) != 0) // 가속 입력 확인
            {
                state.Press(Key.Space); // Space 키 상태 설정
            }
            InputSystem.QueueStateEvent(keyboard, state); // 실제 입력 시스템에 상태 전달
        }

        private static void ObserveState(SnakeMovement player, SnakeBodyManager body, SnakeHealth health, float elapsed) // 체력과 몸통 변화 기록
        {
            if (lastBodies >= 0 && (body.CurrentBodyCount != lastBodies || health.CurrentHealth != lastHealth)) // 몸통 또는 체력 변화 확인
            {
                SnakeInvulnerability invulnerability = player.GetComponent<SnakeInvulnerability>(); // 전체 무적 상태
                SnakeExternalCollision collision = player.GetComponent<SnakeExternalCollision>(); // 직접 충돌 상태
                Debug.Log($"[NaturalCombatVerification] change time={elapsed:F2} body={lastBodies}->{body.CurrentBodyCount} hp={lastHealth}->{health.CurrentHealth} fullInvincible={(invulnerability != null && invulnerability.IsInvulnerable)} directProtected={(collision != null && collision.EnemyContactProtected)}"); // 피해와 보호 상태 기록
            }
            lastBodies = body.CurrentBodyCount; // 현재 몸통 저장
            lastHealth = health.CurrentHealth; // 현재 체력 저장
        }

        private static void LogInputDiagnostics(InputBindingManager bindings, SnakeStamina stamina, float elapsed) // 초기 입력 상태 진단
        {
            InputAction left = bindings == null ? null : (InputAction)typeof(InputBindingManager).GetField("turnLeftAction", PrivateInstance).GetValue(bindings); // 왼쪽 입력 액션 조회
            InputAction right = bindings == null ? null : (InputAction)typeof(InputBindingManager).GetField("turnRightAction", PrivateInstance).GetValue(bindings); // 오른쪽 입력 액션 조회
            InputAction boost = bindings == null ? null : (InputAction)typeof(InputBindingManager).GetField("boostAction", PrivateInstance).GetValue(bindings); // 가속 입력 액션 조회
            string leftPath = left == null || left.bindings.Count == 0 ? "-" : left.bindings[0].effectivePath; // 적용된 왼쪽 키 경로
            string rightPath = right == null || right.bindings.Count == 0 ? "-" : right.bindings[0].effectivePath; // 적용된 오른쪽 키 경로
            string boostPath = boost == null || boost.bindings.Count == 0 ? "-" : boost.bindings[0].effectivePath; // 적용된 가속 키 경로
            Debug.Log($"[PilotInput] time={elapsed:F2} requested={currentKeys} focused={Application.isFocused} keyboardEnabled={keyboard.enabled} keyboardCurrent={Keyboard.current == keyboard} raw={keyboard.aKey.isPressed},{keyboard.dKey.isPressed},{keyboard.spaceKey.isPressed} actions={bindings != null && bindings.TurnLeftPressed},{bindings != null && bindings.TurnRightPressed},{bindings != null && bindings.BoostPressed} motion={leftRotationObserved},{rightRotationObserved},{boostMovementObserved} boosting={stamina != null && stamina.IsBoosting} actionEnabled={left != null && left.enabled},{right != null && right.enabled},{boost != null && boost.enabled} paths={leftPath},{rightPath},{boostPath}"); // 실제 키 및 액션 상태 기록
        }

        private static void ObserveMotion(SnakeMovement player, InputBindingManager bindings, SnakeStamina stamina, float elapsed) // 실제 회전과 이동 확인
        {
            float angle = player.transform.eulerAngles.z; // 현재 머리 회전
            Vector3 position = player.transform.position; // 현재 머리 위치
            if (previousInputTime >= 0f && elapsed > previousInputTime) // 유효 시간 간격 확인
            {
                float rotation = Mathf.DeltaAngle(previousAngle, angle); // 실제 회전 변화량
                float movementSpeed = Vector3.Distance(previousPosition, position) / (elapsed - previousInputTime); // 실제 이동 속도
                leftRotationObserved |= bindings != null && bindings.TurnLeftPressed && rotation > 0.01f; // 왼쪽 입력의 반시계 회전 확인
                rightRotationObserved |= bindings != null && bindings.TurnRightPressed && rotation < -0.01f; // 오른쪽 입력의 시계 회전 확인
                boostMovementObserved |= stamina != null && stamina.IsBoosting && movementSpeed > player.MoveSpeed * 1.25f; // 가속 입력의 실제 속도 확인
            }
            previousAngle = angle; // 현재 회전 저장
            previousPosition = position; // 현재 위치 저장
            previousInputTime = elapsed; // 현재 시각 저장
        }

        private static void ObserveEnemies(EarlyEnemyBehaviour[] enemies) // 적 출현 종류 기록
        {
            foreach (EarlyEnemyBehaviour enemy in enemies) // 적 순회
            {
                if (enemy == null) // 제거된 적 확인
                {
                    continue; // 다음 적 검사
                }
                string name = enemy.gameObject.name; // 적 이름 조회
                sawChaser |= name == "EarlyEnemy_Chaser"; // 추적자 기록
                sawPredator |= name == "EarlyEnemy_Predator"; // 포식자 기록
                sawShooter |= name == "EarlyEnemy_Shooter"; // 사수 기록
                sawElite |= name == "EarlyEnemy_Elite"; // 엘리트 기록
                WeaponTarget target = enemy.GetComponent<WeaponTarget>(); // 실제 전투 대상 조회
                if (target != null && monitoredTargets.Add(target)) // 신규 적 처치 추적 확인
                {
                    target.Died += HandleNaturalKill; // 자연 처치 이벤트 연결
                    targetHealth[target] = target.CurrentHealth; // 초기 적 체력 저장
                }
                if (target != null && targetHealth.TryGetValue(target, out float previous) && target.CurrentHealth < previous) // 실제 적 피해 확인
                {
                    if (previous - target.CurrentHealth >= 0.1f) // 표시 가능한 실제 체력 차이 확인
                    {
                        Debug.Log($"[NaturalCombatVerification] target damage id={target.GetInstanceID()} type={target.name} hp={previous:F1}->{target.CurrentHealth:F1} time={Time.time:F2}"); // 무기 피해 관찰 기록
                    }
                    targetHealth[target] = target.CurrentHealth; // 적 체력 갱신
                    SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
                    SnakeWeaponSlot slot = weapons != null && weapons.Slots.Count > 0 ? weapons.Slots[0] : null; // 첫 무기 슬롯
                    bool hasOrigin = slot != null && !slot.IsEmpty && slot.Origin != null; // 실제 무기 위치 확인
                    float previousDistance = hasOrigin && focusTarget != null ? Vector2.Distance(slot.Origin.position, focusTarget.transform.position) : float.PositiveInfinity; // 기존 추적 거리
                    float targetDistance = hasOrigin ? Vector2.Distance(slot.Origin.position, target.transform.position) : float.PositiveInfinity; // 피격 대상 거리
                    bool closerHit = hasOrigin && previousDistance > slot.EffectiveRange && targetDistance <= slot.EffectiveRange; // 기존 대상 이탈 및 새 피격 대상 확인
                    if (focusTarget == null || !focusTarget.IsAlive || focusTarget.CurrentHealth >= focusTarget.MaximumHealth || closerHit) // 유효한 부상 대상 우선 확인
                    {
                        int previousId = focusTarget == null ? 0 : focusTarget.GetInstanceID(); // 기존 추적 대상 번호
                        focusTarget = target; // 실제 피해 대상 추적
                        Debug.Log($"[NaturalCombatVerification] focus id={target.GetInstanceID()} reason=damage hp={target.CurrentHealth:F1} previousId={previousId} previousDistance={previousDistance:F2} targetDistance={targetDistance:F2} closerHit={closerHit}"); // 추적 대상 변경 기록
                    }
                }
            }
        }

        private static void LogTacticalState(EarlyEnemyBehaviour[] enemies, float elapsed) // 실제 공격 거리 기록
        {
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            if (weapons == null || weapons.Slots.Count == 0 || weapons.Slots[0].IsEmpty || weapons.Slots[0].Origin == null) // 첫 무기 존재 확인
            {
                return; // 무기 부재 시 생략
            }
            SnakeWeaponSlot slot = weapons.Slots[0]; // 첫 무기 슬롯
            float nearest = float.PositiveInfinity; // 최근접 적 거리 초기화
            int inRange = 0; // 무기 사거리 안의 적 수
            foreach (EarlyEnemyBehaviour enemy in enemies) // 실제 적 순회
            {
                if (enemy == null) // 제거된 적 확인
                {
                    continue; // 다음 적 확인
                }
                float distance = Vector2.Distance(slot.Origin.position, enemy.transform.position); // 실제 무기와 적 거리
                nearest = Mathf.Min(nearest, distance); // 최단 거리 갱신
                inRange += distance <= slot.EffectiveRange ? 1 : 0; // 사거리 안 적 계수
            }
            float focusDistance = focusTarget == null ? float.PositiveInfinity : Vector2.Distance(slot.Origin.position, focusTarget.transform.position); // 추적 적의 실제 거리
            int focusId = focusTarget == null ? 0 : focusTarget.GetInstanceID(); // 추적 적 구분값
            float focusHealth = focusTarget == null ? 0f : focusTarget.CurrentHealth; // 추적 적 현재 체력
            Debug.Log($"[NaturalCombatVerification] tactical time={elapsed:F1} weaponNearest={nearest:F2} weaponRange={slot.EffectiveRange:F2} inRange={inRange} weaponReady={slot.IsReady(Time.time)} keys={currentKeys} focusId={focusId} focusDistance={focusDistance:F2} focusHp={focusHealth:F1} curve={selectedCurve} score={selectedTrajectoryScore:F2}"); // 거리 및 추적 상태 기록
        }

        private static void LogGemState(SnakeMovement player, EarlyEnemyBehaviour[] enemies, float elapsed) // 실제 보석 경로 기록
        {
            Vector2 head = player.transform.position; // 실제 머리 위치
            float nearestEnemy = float.PositiveInfinity; // 가까운 적 거리 초기화
            foreach (EarlyEnemyBehaviour enemy in enemies) // 실제 적 순회
            {
                if (enemy != null) // 유효한 적 확인
                {
                    nearestEnemy = Mathf.Min(nearestEnemy, Vector2.Distance(head, enemy.transform.position)); // 머리와 적 최단 거리
                }
            }
            ExperiencePickup[] gems = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None); // 실제 보석 목록
            foreach (ExperiencePickup gem in gems) // 보석별 경로 순회
            {
                if (gem == null) // 제거된 보석 확인
                {
                    continue; // 다음 보석 확인
                }
                Vector2 position = gem.transform.position; // 실제 보석 위치
                Debug.Log($"[NaturalCombatVerification] gem time={elapsed:F2} id={gem.GetInstanceID()} name={gem.name} head=({head.x:F2},{head.y:F2}) position=({position.x:F2},{position.y:F2}) distance={Vector2.Distance(head, position):F2} nearestEnemy={nearestEnemy:F2} keys={currentKeys} heldTurn={heldTurn} lockedTurn={lockedTurn}"); // 보석과 조종 상태 기록
            }
        }

        private static void UpdateFocus(EarlyEnemyBehaviour[] enemies) // 자연 출현 적 추적 대상 선택
        {
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            if (weapons == null || weapons.Slots.Count == 0 || weapons.Slots[0].IsEmpty || weapons.Slots[0].Origin == null) // 첫 무기 존재 확인
            {
                focusTarget = null; // 무기 부재 시 추적 해제
                return; // 대상 선택 생략
            }
            Vector2 weaponOrigin = weapons.Slots[0].Origin.position; // 실제 무기 위치
            if (focusTarget != null && (!focusTarget.IsAlive || !focusTarget.isActiveAndEnabled || Vector2.Distance(weaponOrigin, focusTarget.transform.position) > 9f)) // 기존 적 상태 확인
            {
                Debug.Log($"[NaturalCombatVerification] focus id={focusTarget.GetInstanceID()} reason=inactive-or-distant"); // 추적 해제 사유 기록
                focusTarget = null; // 유효하지 않은 대상 해제
            }
            if (focusTarget != null) // 기존 대상 유지 확인
            {
                return; // 단일 적 추적 유지
            }
            float bestDistance = 7f; // 신규 적 선택 거리 상한
            foreach (EarlyEnemyBehaviour enemy in enemies) // 실제 적 순회
            {
                if (enemy == null) // 소멸된 적 확인
                {
                    continue; // 다음 적 확인
                }
                WeaponTarget target = enemy.GetComponent<WeaponTarget>(); // 실제 공격 대상 조회
                if (target == null || !target.IsAlive || !target.isActiveAndEnabled) // 유효 적 확인
                {
                    continue; // 다음 적 확인
                }
                float distance = Vector2.Distance(weaponOrigin, target.transform.position); // 무기와 대상 거리
                if (distance < bestDistance) // 더 가까운 적 확인
                {
                    bestDistance = distance; // 최단 거리 갱신
                    focusTarget = target; // 추적 대상 저장
                }
            }
            if (focusTarget != null) // 신규 대상 확인
            {
                Debug.Log($"[NaturalCombatVerification] focus id={focusTarget.GetInstanceID()} reason=nearest distance={bestDistance:F2} hp={focusTarget.CurrentHealth:F1}"); // 신규 대상 기록
            }
        }

        private static void HandleNaturalKill(WeaponTarget target) // 실제 적 처치 기록
        {
            naturalKills++; // 처치 수 증가
            Debug.Log($"[NaturalCombatVerification] natural kill={naturalKills} id={target.GetInstanceID()} type={target.name} time={Time.time:F2}"); // 실제 처치 기록
        }

        private static void HandleBodyCountChanged(int current, int maximum) // 실제 몸통 변경 원인 기록
        {
            if (current < trackedBodyCount) // 실제 몸통 손실 확인
            {
                Debug.Log($"[NaturalCombatVerification] body loss {trackedBodyCount}->{current} time={Time.time:F2} stack={Environment.StackTrace}"); // 손실 호출 경로 기록
            }
            trackedBodyCount = current; // 현재 몸통 수 저장
        }
        private static void HandleExperienceChanged(int current, int required) // 실제 경험치 변경 기록
        {
            int level = trackedExperience == null ? previousLevel : trackedExperience.CurrentLevel; // 현재 레벨 조회
            if (current != previousExperience || level != previousLevel) // 실제 성장 상태 변경 확인
            {
                Debug.Log($"[NaturalCombatVerification] experience level={level} xp={current}/{required} time={Time.time:F2}"); // 자연 경험치 기록
                previousExperience = current; // 현재 경험치 저장
                previousLevel = level; // 현재 레벨 저장
            }
        }

        private static void ClickLevelButton(int index) // 레벨업 또는 보상 버튼 선택
        {
            LevelUpPanelController panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 선택 화면 조회
            string[] names = { "candidateButton01", "candidateButton02", "candidateButton03" }; // 버튼 필드 이름
            Button button = panel == null ? null : (Button)typeof(LevelUpPanelController).GetField(names[index], PrivateInstance).GetValue(panel); // 실제 버튼 조회
            if (button != null && button.IsActive() && button.interactable) // 선택 가능 버튼 확인
            {
                button.onClick.Invoke(); // 실제 버튼 이벤트 실행
            }
        }

        private static void ClickFirstAvailableLevelButton() // 사용 가능한 첫 보상 선택
        {
            LevelUpPanelController panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 보상 화면 조회
            if (panel == null) // 화면 부재 확인
            {
                return; // 선택 생략
            }
            string[] names = { "candidateButton01", "candidateButton02", "candidateButton03" }; // 버튼 필드 이름
            foreach (string name in names) // 후보 버튼 순회
            {
                Button button = (Button)typeof(LevelUpPanelController).GetField(name, PrivateInstance).GetValue(panel); // 버튼 조회
                if (button != null && button.IsActive() && button.interactable) // 선택 가능 확인
                {
                    button.onClick.Invoke(); // 실제 버튼 이벤트 실행
                    return; // 중복 선택 차단
                }
            }
        }

        private static void ClickFirstAvailableMergeButton() // 사용 가능한 첫 합성 선택
        {
            WeaponMergePanelController panel = UnityEngine.Object.FindFirstObjectByType<WeaponMergePanelController>(FindObjectsInactive.Include); // 합성 화면 조회
            if (panel == null) // 화면 부재 확인
            {
                return; // 선택 생략
            }
            SnakeWeaponMergeController merge = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponMergeController>(); // 실제 합성 상태 조회
            FieldInfo stageField = typeof(SnakeWeaponMergeController).GetField("selectionStage", PrivateInstance); // 실제 재료 선택 단계 조회
            int stage = merge == null || stageField == null ? -1 : (int)stageField.GetValue(merge); // 현재 실제 선택 단계
            string[] names = stage >= 0 && stage < 2 ? new[] { "candidateButton02" } : new[] { "candidateButton01", "candidateButton02", "candidateButton03" }; // 재료 확정 또는 강화 버튼 선택
            foreach (string name in names) // 후보 버튼 순회
            {
                Button button = (Button)typeof(WeaponMergePanelController).GetField(name, PrivateInstance).GetValue(panel); // 버튼 조회
                if (button != null && button.IsActive() && button.interactable) // 선택 가능 확인
                {
                    button.onClick.Invoke(); // 실제 버튼 이벤트 실행
                    return; // 중복 선택 차단
                }
            }
        }

        private static bool TryStartAutomaticMerge(SnakeWeaponMergeController merge) // 실제 자동 합성 시작
        {
            return merge != null && !merge.IsMergeOpen && merge.HasMergeCandidates() && merge.OpenMergePanel(); // 실제 합성 가능 상태와 진입 결과
        }

        private static void HandlePilotMergeClosed(bool completed) // 실제 자동 합성 종료 기록
        {
            if (!pilotMergeOpen) // 자동 조종기가 연 합성 확인
            {
                return; // 다른 합성 결과 제외
            }
            pilotMergeOpen = false; // 자동 합성 진행 상태 해제
            if (completed) // 실제 합성 성공 확인
            {
                pilotMergeCompleted++; // 실제 완료 횟수 증가
                RunTelemetry.Instance?.Capture("automatic_merge"); // 실제 합성 결과 상태 표본
            }
            else // 실제 합성 취소 확인
            {
                pilotMergeCancelled++; // 실제 취소 횟수 증가
            }
            Debug.Log($"[NaturalCombatVerification] automatic merge result completed={completed} total={pilotMergeCompleted} cancelled={pilotMergeCancelled}."); // 실제 자동 합성 결과 기록
        }

        private static void Fail(string message) // 실패 결과 기록
        {
            Debug.LogError("[NaturalCombatVerification] " + message); // 실패 로그
            RunTelemetry.Instance?.Capture("interrupted"); // 실제 실패 시점 표본 저장
            LogRecording(); // 실패 전 실제 관측 기록 출력
            Finish(1); // 실패 종료
        }

        private static void LogRecording() // 실제 전투 기록 한 번 출력
        {
            RunTelemetry telemetry = RunTelemetry.Instance; // 실제 전투 기록기
            if (recordingLogged || telemetry == null || telemetry.Current == null) // 출력 여부와 실제 기록 확인
            {
                return; // 중복 또는 빈 기록 출력 방지
            }
            recordingLogged = true; // 실제 기록 출력 상태 저장
            Debug.Log("[NaturalCombatVerification] RECORD " + JsonUtility.ToJson(telemetry.Current)); // 실제 관측 기록 출력
        }

        private static void Finish(int code) // 검증 종료
        {
            if (trackedMerge != null) // 실제 합성 구독 확인
            {
                trackedMerge.MergeClosed -= HandlePilotMergeClosed; // 실제 합성 결과 구독 해제
            }
            if (trackedBody != null) // 실제 몸통 구독 확인
            {
                trackedBody.BodyCountChanged -= HandleBodyCountChanged; // 몸통 이벤트 연결 해제
            }
            if (trackedExperience != null) // 경험치 관리자 연결 확인
            {
                trackedExperience.ExperienceChanged -= HandleExperienceChanged; // 경험치 이벤트 연결 해제
            }
            if (keyboard != null) // 가상 키보드 확인
            {
                InputSystem.RemoveDevice(keyboard); // 시험 장치 제거
            }
            if (originalInputSettings != null) // 원래 입력 설정 확인
            {
                InputSystem.settings = originalInputSettings; // 원래 설정 복구
                Application.runInBackground = originalRunInBackground; // 원래 백그라운드 상태 복구
            }
            if (testInputSettings != null) // 시험 설정 사본 확인
            {
                UnityEngine.Object.Destroy(testInputSettings); // 시험 설정 사본 제거
            }
            SessionState.SetBool(ActiveKey, false); // 검증 표식 해제
            EditorApplication.update -= Tick; // 갱신 연결 해제
            EditorApplication.Exit(code); // 종료 결과 반환
        }
    }
}
