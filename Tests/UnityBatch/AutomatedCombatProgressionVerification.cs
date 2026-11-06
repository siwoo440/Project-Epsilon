using System.Collections.Generic; // 보석 식별 목록
using System.Reflection; // 기존 화면 참조 조회
using ProjectEpsilon.Combat; // 실제 전투 코드
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 무기 종류
using ProjectEpsilon.Enemies; // 자연 출현 제어
using ProjectEpsilon.Player; // 실제 몸통과 이동
using ProjectEpsilon.Progression; // 적 보상과 레벨업
using ProjectEpsilon.UI; // 실제 레벨업 화면
using UnityEditor; // 배치 편집기 제어
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 실제 유니티 객체
using UnityEngine.UI; // 실제 UI 버튼

namespace ProjectEpsilon.Editor // 편집기 검증 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 전환 후 검증 복구
    public static class AutomatedCombatProgressionVerification // 통제된 전투 성장 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonCombatProgressionVerification"; // 검증 진행 표식
        private static readonly HashSet<int> SeenGems = new HashSet<int>(); // 발견한 보석 식별자
        private static double startedAt; // 실제 시간 시작점
        private static int phase; // 검증 단계
        private static int deaths; // 실제 적 사망 이벤트 수
        private static bool buttonInvoked; // 실제 UI 선택 이벤트 여부
        private static WeaponData expectedWeapon; // 선택할 실제 무기
        private static int expectedGrade; // 선택할 실제 등급

        static AutomatedCombatProgressionVerification() // 도메인 초기화
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 대기 중인 검증 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 검증 갱신 연결
            } // 조건 끝
        } // 생성자 끝

        public static void Run() // 배치 진입점
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 검증 진행 기록
            EditorApplication.update -= Tick; // 중복 연결 제거
            EditorApplication.update += Tick; // 검증 갱신 연결
            EditorApplication.isPlaying = true; // 실제 플레이 모드 진입
        } // 메서드 끝

        private static void Tick() // 프레임별 검증
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 진입 대기
            } // 조건 끝
            if (startedAt == 0d) // 시작 시각 확인
            { // 조건 시작
                startedAt = EditorApplication.timeSinceStartup; // 실제 시작 시간 저장
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - startedAt > 90d) // 실제 실행 시간 제한
            { // 조건 시작
                Fail("Timeout while waiting for the combat progression loop."); // 시간 초과 보고
                return; // 진행 중단
            } // 조건 끝
            GameManager game = GameManager.Instance; // 실제 게임 관리자
            SnakeMovement movement = Object.FindFirstObjectByType<SnakeMovement>(); // 실제 이동 제어기
            SnakeBodyManager body = Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeWeaponManager weapons = Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            SnakeExperience experience = Object.FindFirstObjectByType<SnakeExperience>(); // 실제 경험치 관리자
            SnakeLevelUpController level = Object.FindFirstObjectByType<SnakeLevelUpController>(); // 실제 레벨업 관리자
            if (game == null || movement == null || body == null || weapons == null || experience == null || level == null) // 씬 초기화 확인
            { // 조건 시작
                return; // 필수 객체 대기
            } // 조건 끝
            if (phase == 0) // 시험 구성 단계
            { // 조건 시작
                Prepare(game, movement, body, weapons); // 실제 씬과 표적 준비
                return; // 다음 프레임 대기
            } // 조건 끝
            if (phase == 1) // 이전 적 제거 대기
            { // 조건 시작
                if (Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None).Length != 0) // 기존 표적 정리 확인
                { // 조건 시작
                    return; // 객체 제거 대기
                } // 조건 끝
                SpawnTargets(movement, body, weapons, experience); // 실제 대상과 보상 생성
                return; // 자동 공격 대기
            } // 조건 끝
            if (phase == 2) // 실제 자동 공격과 경험치 수집
            { // 조건 시작
                ObserveGems(); // 실제 보석 생성 기록
                if (experience.CurrentLevel >= 2 || level.IsPresentingLevelUp) // 레벨업 진입 확인
                { // 조건 시작
                    VerifyLevelUp(game, body, weapons, experience, level); // 적 처치와 레벨업 확인
                } // 조건 끝
                else if (!game.IsPlaying || !Mathf.Approximately(Time.timeScale, 1f)) // 정상 진행 상태 확인
                { // 조건 시작
                    Fail("Game stopped or changed speed before level-up."); // 비정상 상태 보고
                } // 조건 끝
                return; // 다음 프레임 대기
            } // 조건 끝
            if (phase == 3) // 실제 버튼 선택 후 상태
            { // 조건 시작
                VerifySelection(game, body, weapons, experience, level); // 보상 장착과 재개 확인
            } // 조건 끝
        } // 메서드 끝

        private static void Prepare(GameManager game, SnakeMovement movement, SnakeBodyManager body, SnakeWeaponManager weapons) // 통제 조건 설정
        { // 메서드 시작
            if (!game.IsPlaying || weapons.SlotCount == 0 || weapons.Slots[0].IsEmpty || weapons.Slots[0].Origin == null) // 플레이 초기화 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            if (body.CurrentBodyCount != 3 || body.EarnedBodyCount != 3) // 시작 몸통 확인
            { // 조건 시작
                Fail($"Unexpected starting bodies: {body.CurrentBodyCount}/{body.EarnedBodyCount}."); // 시작 상태 오류
                return; // 준비 중단
            } // 조건 끝
            if (!Mathf.Approximately(Time.timeScale, 1f)) // 기본 속도 확인
            { // 조건 시작
                Fail($"Unexpected initial time scale: {Time.timeScale}."); // 속도 오류 보고
                return; // 준비 중단
            } // 조건 끝
            EarlyEnemyWaveSpawner spawner = Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 자연 출현기 조회
            if (spawner != null) // 자연 출현기 확인
            { // 조건 시작
                spawner.enabled = false; // 자연 출현 중단
            } // 조건 끝
            movement.enabled = false; // 플레이어 이동만 중단
            foreach (WeaponTarget target in Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 대상 순회
            { // 반복 시작
                Object.Destroy(target.gameObject); // 기존 대상 제거
            } // 반복 끝
            Debug.Log("[CombatProgression] CONTROLLED: player movement and natural spawning disabled; ten stationary collider-free enemies; actual weapon, HP 25, XP 1 each and time scale 1 retained."); // 인위적 시험 조건 기록
            phase = 1; // 정리 대기 단계
        } // 메서드 끝

        private static void SpawnTargets(SnakeMovement movement, SnakeBodyManager body, SnakeWeaponManager weapons, SnakeExperience experience) // 시험 적 생성
        { // 메서드 시작
            SnakeWeaponSlot slot = weapons.Slots[0]; // 첫 실제 무기 슬롯
            if (slot.Weapon.AttackType != WeaponAttackType.Melee || !Mathf.Approximately(slot.Weapon.BaseDamage, 10f) || !Mathf.Approximately(slot.Weapon.AttackInterval, 0.8f)) // 실제 시작 무기 확인
            { // 조건 시작
                Fail("The expected grade-one steel sword is not equipped."); // 시작 무기 오류
                return; // 시험 중단
            } // 조건 끝
            if (experience.CurrentLevel != 1 || experience.CurrentExperience != 0 || body.CurrentBodyCount != 3) // 초기 진행 상태 확인
            { // 조건 시작
                Fail("Unexpected experience or body state before target creation."); // 초기 상태 오류
                return; // 시험 중단
            } // 조건 끝
            Vector3 location = slot.Origin.position - movement.transform.up * 1.1f; // 몸통 뒤 표적 위치
            float bodyDistance = Vector3.Distance(location, slot.Origin.position); // 몸통 기준 거리
            float headDistance = Vector3.Distance(location, movement.transform.position); // 머리 기준 거리
            if (bodyDistance >= slot.EffectiveRange || headDistance <= slot.EffectiveRange) // 몸통 기준 범위 확인
            { // 조건 시작
                Fail($"Target placement invalid: body={bodyDistance:F2}, head={headDistance:F2}, range={slot.EffectiveRange:F2}."); // 위치 오류 보고
                return; // 시험 중단
            } // 조건 끝
            for (int index = 0; index < 10; index++) // 소형 경험치 대상 생성
            { // 반복 시작
                GameObject enemy = new GameObject($"Controlled_Chaser_{index + 1:00}"); // 정지 시험 적 객체
                enemy.transform.position = location; // 몸통 기준 범위 배치
                WeaponTarget target = enemy.AddComponent<WeaponTarget>(); // 실제 공격 가능 대상
                target.Configure(25f); // 기획 추적자 체력 적용
                EnemyDropController drop = enemy.AddComponent<EnemyDropController>(); // 실제 처치 보상 연결
                drop.Configure(1, 15, 0f, 0f, null, 1); // 소형 경험치와 비활성 회복 확률
                target.Died += HandleDeath; // 실제 처치 이벤트 기록
            } // 반복 끝
            Debug.Log($"[CombatProgression] targets=10 hp=25 xp=1 headDistance={headDistance:F2} bodyDistance={bodyDistance:F2} range={slot.EffectiveRange:F2}"); // 시험 구성 기록
            phase = 2; // 자동 공격 관찰 단계
        } // 메서드 끝

        private static void HandleDeath(WeaponTarget target) // 실제 적 사망 이벤트
        { // 메서드 시작
            deaths++; // 사망 횟수 증가
        } // 메서드 끝

        private static void ObserveGems() // 소형 보석 생성 관찰
        { // 메서드 시작
            foreach (ExperiencePickup pickup in Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) // 활성 보석 순회
            { // 반복 시작
                if (pickup.gameObject.name == "XP_Gem_1") // 소형 보석 확인
                { // 조건 시작
                    SeenGems.Add(pickup.GetInstanceID()); // 고유 보석 기록
                } // 조건 끝
            } // 반복 끝
        } // 메서드 끝

        private static void VerifyLevelUp(GameManager game, SnakeBodyManager body, SnakeWeaponManager weapons, SnakeExperience experience, SnakeLevelUpController level) // 실제 레벨업 검증
        { // 메서드 시작
            if (deaths != 10 || SeenGems.Count != 10) // 사망과 소형 보석 수 확인
            { // 조건 시작
                Fail($"Unexpected deaths or observed gems: deaths={deaths}, gems={SeenGems.Count}."); // 처치 또는 보석 누락
                return; // 검증 중단
            } // 조건 끝
            if (experience.CurrentLevel != 2 || experience.CurrentExperience != 0 || !experience.IsLevelUpPending || !level.IsPresentingLevelUp) // 실제 경험치와 화면 확인
            { // 조건 시작
                Fail($"Unexpected level-up state: level={experience.CurrentLevel}, xp={experience.CurrentExperience}, pending={experience.IsLevelUpPending}, panel={level.IsPresentingLevelUp}."); // 레벨업 오류
                return; // 검증 중단
            } // 조건 끝
            if (!game.IsPaused || !Mathf.Approximately(Time.timeScale, 0f) || body.CurrentBodyCount != 4 || body.EarnedBodyCount != 4) // 정지와 몸통 성장 확인
            { // 조건 시작
                Fail($"Unexpected pause or growth: paused={game.IsPaused}, scale={Time.timeScale}, body={body.CurrentBodyCount}/{body.EarnedBodyCount}."); // 성장 오류
                return; // 검증 중단
            } // 조건 끝
            if (level.CurrentCandidates.Count == 0) // 실제 후보 확인
            { // 조건 시작
                Fail("No weapon reward candidate was generated."); // 후보 누락 보고
                return; // 검증 중단
            } // 조건 끝
            WeaponRewardCandidate candidate = level.CurrentCandidates[0]; // 첫 실제 무기 후보
            expectedWeapon = candidate.Weapon; // 선택 무기 기록
            expectedGrade = candidate.Grade; // 선택 등급 기록
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic; // 직렬화 필드 접근 설정
            LevelUpPanelController panel = (LevelUpPanelController)typeof(SnakeLevelUpController).GetField("levelUpPanel", flags).GetValue(level); // 실제 보상 화면 조회
            Button button = panel == null ? null : (Button)typeof(LevelUpPanelController).GetField("candidateButton01", flags).GetValue(panel); // 실제 첫 버튼 조회
            if (panel == null || !panel.gameObject.activeInHierarchy || button == null || !button.IsActive() || !button.interactable) // 버튼 사용 가능 상태 확인
            { // 조건 시작
                Fail("The actual first level-up UI button is unavailable."); // 버튼 오류 보고
                return; // 검증 중단
            } // 조건 끝
            panel.CandidateSelected += HandleButtonSelection; // 실제 UI 이벤트 기록
            Debug.Log($"[CombatProgression] deaths={deaths} smallGems={SeenGems.Count} level=2 body=4 paused=true; invoking actual first UI button programmatically."); // 레벨업 및 자동 선택 기록
            phase = 3; // 보상 선택 확인 단계
            button.onClick.Invoke(); // 실제 UI 버튼 이벤트 호출
        } // 메서드 끝

        private static void HandleButtonSelection(int index) // 실제 UI 이벤트 기록
        { // 메서드 시작
            buttonInvoked = index == 0; // 첫 버튼 선택 확인
        } // 메서드 끝

        private static void VerifySelection(GameManager game, SnakeBodyManager body, SnakeWeaponManager weapons, SnakeExperience experience, SnakeLevelUpController level) // 무기 장착과 재개 확인
        { // 메서드 시작
            if (!buttonInvoked || !game.IsPlaying || !Mathf.Approximately(Time.timeScale, 1f) || experience.IsLevelUpPending || level.IsPresentingLevelUp) // UI 선택과 게임 재개 확인
            { // 조건 시작
                Fail($"Selection or resume failed: event={buttonInvoked}, playing={game.IsPlaying}, scale={Time.timeScale}, pending={experience.IsLevelUpPending}."); // 재개 오류 보고
                return; // 검증 중단
            } // 조건 끝
            if (body.CurrentBodyCount != 4 || weapons.OccupiedSlotCount != 2 || weapons.Slots[0].Weapon != expectedWeapon || weapons.Slots[0].Grade != expectedGrade) // 실제 보상 장착 확인
            { // 조건 시작
                Fail($"Weapon reward was not equipped in the first body slot: bodies={body.CurrentBodyCount}, occupied={weapons.OccupiedSlotCount}."); // 장착 오류 보고
                return; // 검증 중단
            } // 조건 끝
            Debug.Log("[CombatProgression] PASS: actual body-origin auto-attack, ten deaths, ten XP gems, level-up pause, real UI callback, weapon equip and resume."); // 검증 성공 기록
            Finish(0); // 정상 종료
        } // 메서드 끝

        private static void Fail(string message) // 검증 실패 처리
        { // 메서드 시작
            Debug.LogError("[CombatProgression] FAIL: " + message); // 실패 로그 출력
            Finish(1); // 실패 종료
        } // 메서드 끝

        private static void Finish(int code) // 배치 정리
        { // 메서드 시작
            SessionState.SetBool(ActiveKey, false); // 검증 진행 표식 해제
            EditorApplication.update -= Tick; // 갱신 연결 제거
            EditorApplication.Exit(code); // 결과 반환
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
