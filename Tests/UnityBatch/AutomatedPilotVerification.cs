using System; // 예외 형식
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
{
    [InitializeOnLoad] // 도메인 갱신 연결
    public static class AutomatedPilotVerification // 가상 입력 생존 검증
    {
        private const string ActiveKey = "EpsilonPilotVerification"; // 검증 진행 표식
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

        static AutomatedPilotVerification() // 도메인 연결
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
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 시작 무기 화면 처리
            {
                return; // 전투 시작 대기
            }
            if (startWallTime == 0d) // 최초 갱신 확인
            {
                startWallTime = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
            }
            try // 검증 오류 처리
            {
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
                Debug.Log("[PilotVerification] Virtual Keyboard A/D/Space; in-memory InputSettings clone routes unfocused batch input to game; no invulnerability or stat changes; automatic UI choices."); // 시험 조건 기록
            }
            float elapsed = (float)typeof(EarlyEnemyWaveSpawner).GetField("elapsed", PrivateInstance).GetValue(spawner); // 전투 경과 시간
            ObserveState(player, body, health, elapsed); // 손실 및 피해 상태 기록
            if (!checkedStart && elapsed >= 0.25f) // 첫 물리 갱신 이후 확인
            {
                checkedStart = true; // 시작 검증 기록
                if (body.CurrentBodyCount != 3) // 기획 시작 몸통 확인
                {
                    Fail($"initial body={body.CurrentBodyCount}, expected 3"); // 시작 손실 기록
                    return; // 추가 처리 중단
                }
                Debug.Log("[PilotVerification] Starting three bodies retained after physics."); // 시작 상태 기록
            }
            EarlyEnemyBehaviour[] enemies = UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None); // 현재 적 목록
            ObserveEnemies(enemies); // 출현 종류 기록
            SnakeLevelUpController level = UnityEngine.Object.FindFirstObjectByType<SnakeLevelUpController>(); // 레벨업 처리기
            EliteRewardController elite = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 엘리트 보상 처리기
            SnakeWeaponMergeController merge = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponMergeController>(); // 합성 처리기
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
                if (elapsed < 0.75f) // 초기 왼쪽 입력 시험
                {
                    SetKeys(elapsed >= 0.3f ? 1 : 0); // 첫 물리 확인 후 왼쪽 입력
                }
                else if (elapsed < 1.2f) // 초기 오른쪽 입력 시험
                {
                    SetKeys(2); // 오른쪽 입력
                }
                else if (elapsed < 1.65f) // 초기 가속 입력 시험
                {
                    SetKeys(4); // 가속 입력
                }
                else // 위험 회피 입력
                {
                    if (elapsed >= nextDecision) // 새 조향 시각 확인
                    {
                        nextKeys = ChooseKeys(player, body, enemies, stamina); // 위험 회피 조향 계산
                        nextDecision = elapsed + 0.12f; // 다음 조향 시각 저장
                    }
                    SetKeys(nextKeys); // 가상 조향 입력
                }
            }
            if (elapsed >= nextReport) // 상태 기록 간격 확인
            {
                Debug.Log($"[PilotVerification] time={elapsed:F1} wall={EditorApplication.timeSinceStartup - startWallTime:F1} state={game.CurrentState} body={body.CurrentBodyCount} hp={health.CurrentHealth} level={experience.CurrentLevel} xp={experience.CurrentExperience} enemies={spawner.ActiveEnemies} levelChoices={levelSelections} eliteChoices={eliteSelections} seen={sawChaser},{sawPredator},{sawShooter},{sawElite} inputs={leftObserved},{rightObserved},{boostObserved} motion={leftRotationObserved},{rightRotationObserved},{boostMovementObserved} scale={Time.timeScale:F1}"); // 게임 및 입력 상태 기록
                nextReport += 30f; // 다음 기록 시각
            }
            if (game.CurrentState == GameState.Defeated) // 패배 상태 확인
            {
                Fail($"defeated at {elapsed:F1} seconds; level={experience.CurrentLevel}; hp={health.CurrentHealth}; body={body.CurrentBodyCount}"); // 패배 결과 기록
                return; // 추가 처리 중단
            }
            if (elapsed >= 3f && (!leftObserved || !rightObserved || !boostObserved || !leftRotationObserved || !rightRotationObserved || !boostMovementObserved)) // 초기 입력 및 실제 이동 확인
            {
                Fail($"virtual input failed: actions={leftObserved},{rightObserved},{boostObserved}, motion={leftRotationObserved},{rightRotationObserved},{boostMovementObserved}, focused={Application.isFocused}, keyboardEnabled={keyboard.enabled}"); // 입력 미반영 실패
                return; // 추가 처리 중단
            }
            if (game.IsPlaying && (merge == null || !merge.IsMergeOpen) && !Mathf.Approximately(Time.timeScale, 1f)) // 일반 게임 속도 확인
            {
                Fail($"unexpected time scale {Time.timeScale}"); // 속도 차이 기록
                return; // 추가 처리 중단
            }
            if (elapsed >= 300f) // 5분 경과 확인
            {
                if (!sawChaser || !sawPredator || !sawShooter || !sawElite || !leftObserved || !rightObserved || !boostObserved || !leftRotationObserved || !rightRotationObserved || !boostMovementObserved) // 출현 및 입력 확인
                {
                    Fail($"missing enemies or inputs: {sawChaser},{sawPredator},{sawShooter},{sawElite}; {leftObserved},{rightObserved},{boostObserved}"); // 검증 누락 기록
                    return; // 추가 처리 중단
                }
                Debug.Log($"[PilotVerification] PASS 300s: level={experience.CurrentLevel}, body={body.CurrentBodyCount}, hp={health.CurrentHealth}, elapsed real={EditorApplication.timeSinceStartup - startWallTime:F1}s."); // 자동 생존 성공 기록
                Finish(0); // 성공 종료
                return; // 처리 종료
            }
            if (EditorApplication.timeSinceStartup - startWallTime > 480d) // 실제 시간 제한 확인
            {
                Fail($"timed out at {elapsed:F1} seconds"); // 시간 초과 기록
            }
        }

        private static int ChooseKeys(SnakeMovement player, SnakeBodyManager body, EarlyEnemyBehaviour[] enemies, SnakeStamina stamina) // 회피 조향 선택
        {
            Vector2 origin = player.transform.position; // 플레이어 위치
            Vector2 heading = player.transform.up; // 현재 전진 방향
            int bestTurn = 0; // 기본 직진 선택
            float bestScore = float.PositiveInfinity; // 최소 위험 점수
            for (int turn = -1; turn <= 1; turn++) // 왼쪽 직진 오른쪽 비교
            {
                float score = ScoreTrajectory(origin, heading, player, body, enemies, turn); // 후보 경로 위험 계산
                if (score < bestScore) // 더 안전한 경로 확인
                {
                    bestScore = score; // 최소 위험 저장
                    bestTurn = turn; // 조향 방향 저장
                }
            }
            int keys = bestTurn < 0 ? 1 : bestTurn > 0 ? 2 : 0; // 조향 방향 키 변환
            if (stamina != null && stamina.CurrentStamina > 40f && ThreatBehind(origin, heading, enemies)) // 뒤쪽 위험과 자원 확인
            {
                keys |= 4; // 가속 입력 추가
            }
            return keys; // 선택 입력 반환
        }

        private static float ScoreTrajectory(Vector2 origin, Vector2 heading, SnakeMovement player, SnakeBodyManager body, EarlyEnemyBehaviour[] enemies, int turn) // 회피 경로 평가
        {
            Vector2 point = origin; // 경로 위치 초기화
            Vector2 forward = heading; // 경로 방향 초기화
            float score = turn == 0 ? 0f : 0.015f; // 불필요한 회전 억제
            const float step = 0.16f; // 예상 경로 시간 간격
            for (int tick = 1; tick <= 8; tick++) // 짧은 미래 경로 비교
            {
                float angle = -turn * player.TurnSpeed * step * Mathf.Deg2Rad; // 회전 각도 계산
                float cosine = Mathf.Cos(angle); // 회전 코사인
                float sine = Mathf.Sin(angle); // 회전 사인
                forward = new Vector2(forward.x * cosine - forward.y * sine, forward.x * sine + forward.y * cosine); // 예상 전진 방향
                point += forward * player.MoveSpeed * step; // 예상 머리 위치
                foreach (EarlyEnemyBehaviour enemy in enemies) // 적 위험 순회
                {
                    if (enemy == null) // 소멸 적 확인
                    {
                        continue; // 다음 적 확인
                    }
                    Vector2 enemyPosition = enemy.transform.position; // 현재 적 위치
                    Vector2 chase = origin - enemyPosition; // 플레이어 방향
                    Vector2 predicted = enemyPosition + chase.normalized * player.MoveSpeed * 0.8f * (step * tick); // 단순 추적 위치 예측
                    float distance = Vector2.Distance(point, predicted); // 예상 접근 거리
                    if (distance < 4f) // 위험 범위 확인
                    {
                        score += 1f / (distance * distance + 0.15f); // 근접 위험 누적
                    }
                    if (distance < 0.9f) // 충돌 위험 확인
                    {
                        score += 30f; // 충돌 위험 가중
                    }
                }
                for (int index = 2; index < body.BodySegments.Count; index++) // 뒤쪽 몸통 순회
                {
                    SnakeSegment segment = body.BodySegments[index]; // 몸통 조회
                    if (segment != null && Vector2.Distance(point, segment.transform.position) < 0.65f) // 자기 충돌 위험 확인
                    {
                        score += 20f; // 자기 충돌 위험 가중
                    }
                }
            }
            return score; // 예상 위험 반환
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
                Debug.Log($"[PilotVerification] change time={elapsed:F2} body={lastBodies}->{body.CurrentBodyCount} hp={lastHealth}->{health.CurrentHealth} fullInvincible={(invulnerability != null && invulnerability.IsInvulnerable)} directProtected={(collision != null && collision.EnemyContactProtected)}"); // 피해와 보호 상태 기록
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
                string name = enemy.gameObject.name; // 적 이름 조회
                sawChaser |= name == "EarlyEnemy_Chaser"; // 추적자 기록
                sawPredator |= name == "EarlyEnemy_Predator"; // 포식자 기록
                sawShooter |= name == "EarlyEnemy_Shooter"; // 사수 기록
                sawElite |= name == "EarlyEnemy_Elite"; // 엘리트 기록
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

        private static void ClickFirstAvailableMergeButton() // 합성 재료 및 강화 선택
        {
            WeaponMergePanelController panel = UnityEngine.Object.FindFirstObjectByType<WeaponMergePanelController>(FindObjectsInactive.Include); // 합성 화면 조회
            if (panel == null) // 화면 부재 확인
            {
                return; // 선택 생략
            }
            string[] names = { "candidateButton02", "candidateButton01", "candidateButton03" }; // 재료 확정 우선 버튼 순서
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

        private static void Fail(string message) // 실패 결과 기록
        {
            Debug.LogError("[PilotVerification] " + message); // 실패 로그
            Finish(1); // 실패 종료
        }

        private static void Finish(int code) // 검증 종료
        {
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
