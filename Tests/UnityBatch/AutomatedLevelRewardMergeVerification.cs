using System; // 기본 형식
using System.Collections; // 코루틴 형식
using System.Collections.Generic; // 보상 후보 목록
using System.Reflection; // 실제 UI 필드 조회
using ProjectEpsilon.Combat; // 무기와 시너지
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Enemies; // 자연 출현 제어
using ProjectEpsilon.Player; // 몸통과 충돌
using ProjectEpsilon.Progression; // 경험치와 보상
using ProjectEpsilon.UI; // 실제 UI 화면
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.InputSystem; // 가상 입력 장치
using UnityEngine.InputSystem.LowLevel; // 가상 키 상태
using UnityEngine.UI; // 실제 UI 버튼

namespace ProjectEpsilon.Editor // 편집기 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedLevelRewardMergeVerification // 레벨업 보상 합성 실제 씬 시험
    {
        private const string ActiveKey = "EpsilonLevelRewardMergeVerification"; // 배치 진행 표식
        private static double started; // 플레이 시작 시각
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지

        static AutomatedLevelRewardMergeVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 진행 상태 확인
            {
                EditorApplication.update += Tick; // 시작 감시 연결
            }
        }

        public static void Run() // Unity 배치 진입점
        {
            RunScenario(0); // 전체 흐름 시험 시작
        }

        public static void RunNumericEntry() // 숫자 입력 중복 시험 진입점
        {
            RunScenario(1); // 숫자 입력 시험 시작
        }

        public static void RunMergeDisable() // 합성 제어기 비활성 시험 진입점
        {
            RunScenario(2); // 합성 제어기 시험 시작
        }

        public static void RunLevelDisable() // 레벨업 제어기 비활성 시험 진입점
        {
            RunScenario(3); // 레벨업 제어기 시험 시작
        }

        public static void RunLastMaterialLoss() // 마지막 재료 손실 시험 진입점
        {
            RunScenario(4); // 재료 손실 시험 시작
        }

        public static void RunRemainingBoundaries() // 남은 경계 통합 시험 진입점
        {
            RunScenario(5); // 개별 결과를 보존하는 통합 시험
        }

        public static void RunAll() // 전체 흐름과 경계 통합 시험 진입점
        {
            RunScenario(6); // 입력과 경계 및 전체 흐름 시험
        }

        private static void RunScenario(int scenario) // 시험 종류와 플레이 시작
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetInt("EpsilonLevelRewardMergeScenario", scenario); // 시험 종류 저장
            SessionState.SetBool(ActiveKey, true); // 진행 표식 저장
            EditorApplication.update -= Tick; // 중복 연결 방지
            EditorApplication.update += Tick; // 시작 감시 연결
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        }

        private static void Tick() // 플레이 시작과 제한 시간 감시
        {
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            {
                return; // 진입 대기
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 시작 무기 화면 처리
            {
                return; // 전투 시작 대기
            }
            if (started <= 0d) // 최초 갱신 확인
            {
                started = EditorApplication.timeSinceStartup; // 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 120d) // 전체 시험 제한 확인
            {
                Finish(1, "Timed out waiting for level reward merge test."); // 시간 초과 보고
                return; // 감시 중단
            }
            if (injected) // 실행기 중복 확인
            {
                return; // 시험 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || Time.time < 0.35f) // 씬 초기화 확인
            {
                return; // 초기화 대기
            }
            injected = true; // 중복 생성 방지
            player.gameObject.AddComponent<LevelRewardMergeVerificationRunner>(); // 실제 씬 시험 실행
        }

        public static void Finish(int code, string message) // 결과 기록과 종료
        {
            if (finished) // 중복 종료 확인
            {
                return; // 중복 처리 방지
            }
            finished = true; // 종료 상태 기록
            SessionState.SetBool(ActiveKey, false); // 진행 표식 제거
            EditorApplication.update -= Tick; // 감시 연결 제거
            Debug.Log($"[LevelRewardMerge] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 종료 코드 반환
        }
    }

    public sealed class LevelRewardCollisionProbe : MonoBehaviour // 실제 물리 접촉 관측
    {
        public int EnemyEntries; // 적 직접 접촉 횟수

        private void OnTriggerEnter2D(Collider2D other) // 물리 엔진 진입 콜백
        {
            SnakeContactHazard hazard = other.GetComponent<SnakeContactHazard>(); // 시험 위험 조회
            if (hazard != null && hazard.HazardType == SnakeContactHazardType.EnemyDirect) // 적 직접 접촉 확인
            {
                EnemyEntries++; // 실제 콜백 기록
            }
        }
    }

    public sealed class LevelRewardMergeVerificationRunner : MonoBehaviour // 실제 보상 합성 시험 실행기
    {
        private static readonly BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance; // 내부 필드 검색
        private SnakeMovement movement; // 실제 이동 처리기
        private SnakeStamina stamina; // 실제 가속 처리기
        private PlayerInputReader input; // 실제 입력 읽기
        private SnakeHealth health; // 실제 공용 체력
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeWeaponManager weapons; // 실제 무기 관리자
        private WeaponAttributeSynergyManager synergy; // 실제 속성 집계
        private SnakeExperience experience; // 실제 경험치 관리자
        private SnakeLevelUpController level; // 실제 레벨업 처리기
        private SnakeWeaponMergeController merge; // 실제 합성 처리기
        private EliteRewardController elite; // 실제 엘리트 보상 처리기
        private LevelUpPanelController rewardPanel; // 실제 레벨업 화면
        private WeaponMergePanelController mergePanel; // 실제 합성 화면
        private SnakeInvulnerability invulnerability; // 실제 무적 관리자
        private LevelRewardCollisionProbe collisionProbe; // 물리 접촉 기록
        private Keyboard keyboard; // 시험 가상 키보드
        private InputSettings originalSettings; // 기존 입력 설정
        private InputSettings testSettings; // 메모리 설정 사본
        private bool originalBackground; // 기존 백그라운드 상태
        private WeaponRewardCandidate pendingCandidate; // 실제 선택 보상
        private WeaponRewardCandidate[] candidates; // 원래 보상 후보 순서
        private int boundaryFailures; // 경계 시험 실패 개수

        private IEnumerator Start() // 실제 씬 시험 순서
        {
            Prepare(); // 씬과 시험 입력 준비
            yield return null; // 장치 갱신 대기
            int scenario = SessionState.GetInt("EpsilonLevelRewardMergeScenario", 0); // 독립 시험 종류 조회
            if (scenario == 6) // 통합 시험 확인
            {
                yield return VerifyNumericEntry(); // 실제 숫자 입력 경계
                ResetBoundaryFixture(); // 다음 시험 상태 초기화
                yield return null; // 초기화 상태 갱신
                yield return VerifyRemainingBoundaries(); // 나머지 경계 개별 검증
                Require(boundaryFailures == 0, "One or more lifecycle boundaries failed: " + boundaryFailures); // 경계 전체 결과 확인
                ResetBoundaryFixture(); // 전체 흐름 전 상태 초기화
                yield return null; // 충돌 객체 제거 대기
            }
            if (scenario != 0 && scenario != 6) // 별도 경계 시험 확인
            {
                if (scenario == 1) // 숫자 입력 경계 확인
                {
                    yield return VerifyNumericEntry(); // 숫자 입력 중복 방지
                }
                else if (scenario == 2) // 합성 제어기 경계 확인
                {
                    yield return VerifyMergeDisable(); // 합성 제어기 비활성 복구
                }
                else if (scenario == 3) // 레벨업 제어기 경계 확인
                {
                    yield return VerifyLevelDisable(); // 선택 단계별 비활성 복구
                }
                else if (scenario == 4) // 마지막 재료 손실 확인
                {
                    yield return VerifyLastMaterialLoss(); // 실제 몸통 손실과 합성 비활성
                }
                else if (scenario == 5) // 여러 경계 결과 확인
                {
                    yield return VerifyRemainingBoundaries(); // 각 경계 기록 후 독립 초기화
                }
                Cleanup(); // 시험 입력 복구
                AutomatedLevelRewardMergeVerification.Finish(boundaryFailures == 0 ? 0 : 1, "Selected boundary " + scenario + "; failures=" + boundaryFailures); // 경계 시험 결과
                yield break; // 전체 흐름 시험 생략
            }
            StartLevel(2); // 연속 두 레벨의 시험 경험치 주입
            SelectControlledCandidate(false); // 빈 슬롯과 합성 가능한 재료 준비
            elite.QueueReward(); // 우선순위 확인용 엘리트 보상 주입
            typeof(EliteRewardController).GetField("candidates", PrivateInstance).SetValue(elite, new int[] { 2, 0, 1 }); // 무기 보상 후보 고정
            yield return KeyTap(Key.E); // 일시정지 중 E 입력
            Require(!merge.IsMergeOpen && GameManager.Instance.IsPaused, "E opened ordinary merge while level-up was paused."); // 레벨업 입력 우선 확인
            ClickLevel(0); // 실제 첫 보상 버튼 선택
            AssertRewardChoice(); // 합성 방식 화면 확인
            ClickLevel(2); // 후보 목록 복귀 버튼
            Require(level.IsPresentingLevelUp && experience.IsLevelUpPending && GameManager.Instance.IsPaused && SameCandidates(), "Returning to candidates changed the pending reward."); // 후보 재선택 보존
            ClickLevel(0); // 같은 보상 재선택
            for (int stage = 0; stage < 3; stage++) // 세 합성 선택 단계 순회
            {
                ClickLevel(1); // 실제 보상 합성 버튼
                Require(merge.IsMergeOpen && GameManager.Instance.IsPlaying && Mathf.Approximately(Time.timeScale, 0.7f), "Reward merge did not start at 70 percent speed."); // 합성 이동 시작 확인
                if (stage == 0) // 첫 합성 열기 확인
                {
                    yield return VerifyStraightMovement(); // 실제 회전과 가속 차단 확인
                }
                if (stage >= 1) // 강화 계승 화면 준비
                {
                    ClickMerge(1); // 장착 재료 선택
                }
                if (stage >= 2) // 공통 강화 화면 준비
                {
                    ClickMerge(1); // 장착 무기 강화 계승
                }
                yield return KeyTap(Key.Escape); // 실제 ESC 합성 취소
                AssertRewardChoice(); // 같은 보상 방식 화면 복귀
                Require(elite.PendingRewardCount == 1 && !elite.IsPresentingReward, "Elite reward interrupted a pending level reward."); // 엘리트 우선순위 보존
            }
            ClickLevel(1); // 보상 합성 재진입
            ClickMerge(1); // 장착 재료 선택
            ClickMerge(1); // 장착 강화 계승
            ClickMerge(1); // 공격 속도 강화 선택
            Require(!merge.IsMergeOpen && experience.IsLevelUpPending && level.IsPresentingLevelUp && GameManager.Instance.IsPaused, "Successful merge did not advance to the next paused level-up."); // 연속 레벨업 전환
            Require(experience.CurrentLevel == 3 && experience.CurrentExperience == 0 && weapons.OccupiedSlotCount == 1, "Reward was consumed more than once or equipped count changed."); // 단일 보상 소비와 무기 수
            Require(weapons.Slots[0].Weapon == pendingCandidate.Weapon && weapons.Slots[0].Grade == pendingCandidate.Grade + 1 && weapons.Slots[0].DamageUpgrades == 2 && weapons.Slots[0].SpeedUpgrades == 1, "Reward merge result or equipped inheritance mismatch."); // 실제 합성 결과 확인
            Require(synergy.GetCount(pendingCandidate.Weapon.Attribute) == 1 && elite.PendingRewardCount == 1 && !elite.IsPresentingReward, "Synergy or queued elite priority changed during consecutive levels."); // 속성과 보상 대기 확인
            FinishNextLevel(); // 둘째 레벨 보상 정상 완료
            yield return null; // 엘리트 보상 표시 대기
            yield return null; // 보상 처리 갱신 대기
            Require(elite.IsPresentingReward && elite.PendingRewardCount == 1 && GameManager.Instance.IsPaused, "Queued elite reward did not appear after consecutive levels."); // 연속 레벨 이후 엘리트 우선순위
            ClickLevel(0); // 고정된 무기 엘리트 보상 선택
            Require(elite.PendingRewardCount == 0 && !elite.IsPresentingReward && GameManager.Instance.IsPlaying, "Elite reward did not complete after the level-up chain."); // 엘리트 보상 완료
            Debug.Log("[LevelRewardMerge] Empty slot, cancellation, straight movement, consecutive levels, and queued elite reward passed."); // 첫 시험 결과
            StartLevel(1); // 만석 교체 시험 레벨업
            SelectControlledCandidate(true); // 만석과 같은 무기 구성
            int fullCount = weapons.OccupiedSlotCount; // 교체 전 무기 수
            ClickLevel(0); // 보상 후보 선택
            AssertRewardChoice(); // 보상 처리 방식 화면
            ClickLevel(0); // 장착 또는 교체 선택
            Require(level.IsPresentingLevelUp && GameManager.Instance.IsPaused, "Full-slot equip did not present replacement selection."); // 교체 화면 유지
            ClickLevel(1); // 첫 슬롯 교체 확정
            Require(!experience.IsLevelUpPending && GameManager.Instance.IsPlaying && weapons.OccupiedSlotCount == fullCount, "Full-slot replacement did not complete exactly once."); // 만석 교체 완료
            StartLevel(1); // 만석 합성 시험 레벨업
            SelectControlledCandidate(true); // 만석과 같은 무기 구성
            fullCount = weapons.OccupiedSlotCount; // 합성 전 무기 수
            int fullSynergy = synergy.GetCount(pendingCandidate.Weapon.Attribute); // 합성 전 속성 수
            ClickLevel(0); // 보상 후보 선택
            ClickLevel(1); // 보상 합성 선택
            ClickMerge(1); // 장착 재료 선택
            ClickMerge(0); // 미장착 보상 기본 강화 계승
            ClickMerge(2); // 공통 사거리 강화 선택
            Require(!experience.IsLevelUpPending && GameManager.Instance.IsPlaying && weapons.OccupiedSlotCount == fullCount, "Full-slot reward merge changed the equipped count or failed to complete."); // 만석 합성 결과
            Require(weapons.Slots[0].Weapon == pendingCandidate.Weapon && weapons.Slots[0].Grade == pendingCandidate.Grade + 1 && weapons.Slots[0].DamageUpgrades == 0 && weapons.Slots[0].RangeUpgrades == 1, "Reward inheritance or common range upgrade mismatch."); // 보상 기본 강화 계승
            Require(synergy.GetCount(pendingCandidate.Weapon.Attribute) == fullSynergy, "Full-slot reward merge changed the attribute count."); // 시너지 개수 유지
            Debug.Log("[LevelRewardMerge] Full-slot replacement and reward merge passed."); // 만석 시험 결과
            yield return VerifyCollisionAndDefeat(); // 실제 충돌과 패배 정리
            Cleanup(); // 시험 입력 복구
            AutomatedLevelRewardMergeVerification.Finish(0, "Actual level reward UI and virtual inputs, cancellation, movement, consecutive levels, elite priority, empty/full slots, collision, and defeat."); // 전체 성공 기록
        }

        private void Prepare() // 실제 씬 구성과 입력 준비
        {
            movement = GetComponent<SnakeMovement>(); // 이동 구성 조회
            stamina = GetComponent<SnakeStamina>(); // 가속 구성 조회
            input = GetComponent<PlayerInputReader>(); // 입력 구성 조회
            health = GetComponent<SnakeHealth>(); // 공용 체력 조회
            invulnerability = GetComponent<SnakeInvulnerability>(); // 무적 처리 조회
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 몸통 관리자 조회
            weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 무기 관리자 조회
            synergy = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeSynergyManager>(); // 시너지 관리자 조회
            experience = UnityEngine.Object.FindFirstObjectByType<SnakeExperience>(); // 경험치 관리자 조회
            level = UnityEngine.Object.FindFirstObjectByType<SnakeLevelUpController>(); // 레벨업 처리기 조회
            merge = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponMergeController>(); // 합성 처리기 조회
            elite = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 엘리트 보상 조회
            rewardPanel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 레벨업 화면 조회
            mergePanel = UnityEngine.Object.FindFirstObjectByType<WeaponMergePanelController>(FindObjectsInactive.Include); // 합성 화면 조회
            Require(movement != null && stamina != null && input != null && health != null && invulnerability != null && body != null && weapons != null && synergy != null && experience != null && level != null && merge != null && elite != null && rewardPanel != null && mergePanel != null, "Required actual scene components are missing."); // 씬 구성 확인
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 자연 출현기 조회
            if (spawner != null) // 출현기 존재 확인
            {
                spawner.enabled = false; // 통제 시험 중 출현 중지
            }
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 기존 적 순회
            {
                enemy.gameObject.SetActive(false); // 통제 시험 중 적 비활성화
            }
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 자기 충돌 구성 조회
            if (self != null) // 자기 충돌 구성 확인
            {
                self.enabled = false; // 시험 외 자기 충돌 방지
            }
            collisionProbe = gameObject.AddComponent<LevelRewardCollisionProbe>(); // 실제 물리 접촉 관측기
            experience.Configure(10, 1f); // 일정한 시험 경험치 기준
            experience.ResetProgression(); // 시험 레벨 초기화
            originalSettings = InputSystem.settings; // 기존 입력 설정 저장
            testSettings = UnityEngine.Object.Instantiate(originalSettings); // 메모리 입력 설정 복사
            testSettings.hideFlags = HideFlags.HideAndDontSave; // 설정 파일 저장 방지
            testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; // 배치 입력 전달
            testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // 배치 포커스 무시
            InputSystem.settings = testSettings; // 시험 설정 적용
            originalBackground = Application.runInBackground; // 기존 백그라운드 상태 저장
            Application.runInBackground = true; // 배치 입력 허용
            keyboard = InputSystem.AddDevice<Keyboard>(); // 가상 키보드 생성
            Debug.Log("[LevelRewardMerge] Actual Game scene; natural spawning and self-collision disabled; controlled XP, equipped weapons, reward and elite candidate; virtual keyboard and actual button events."); // 시험 조건 기록
        }

        private void StartLevel(int levels) // 실제 경험치 주입으로 레벨업 시작
        {
            Require(GameManager.Instance.IsPlaying && !experience.IsLevelUpPending && !level.IsPresentingLevelUp, "Cannot begin controlled level-up in current state."); // 시작 상태 확인
            int needed = experience.RequiredExperience - experience.CurrentExperience + (levels - 1) * 10; // 시험 레벨 필요 경험치
            experience.AddExperience(needed); // 실제 경험치 처리
            Require(experience.IsLevelUpPending && level.IsPresentingLevelUp && GameManager.Instance.IsPaused && Mathf.Approximately(Time.timeScale, 0f), "Injected experience did not start paused level-up."); // 레벨업 화면 확인
        }

        private void SelectControlledCandidate(bool fillAll) // 실제 후보와 호환 장착 무기 구성
        {
            Require(level.CurrentCandidates.Count > 0, "No level reward candidates are available."); // 실제 후보 존재 확인
            pendingCandidate = level.CurrentCandidates[0]; // 실제 첫 후보 보존
            Require(pendingCandidate.IsValid && pendingCandidate.Grade < pendingCandidate.Weapon.MaxGrade, "First reward cannot be merged."); // 합성 가능한 보상 확인
            candidates = new WeaponRewardCandidate[level.CurrentCandidates.Count]; // 후보 순서 저장소
            for (int index = 0; index < candidates.Length; index++) // 실제 후보 순회
            {
                candidates[index] = level.CurrentCandidates[index]; // 후보 객체와 등급 저장
            }
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 모든 무기 슬롯 순회
            {
                slot.Clear(); // 통제 시험용 기존 무기 제거
            }
            int count = fillAll ? weapons.SlotCount : 1; // 시험 장착 무기 개수
            for (int index = 0; index < count; index++) // 장착 슬롯 순회
            {
                Require(weapons.TryEquipAt(index, pendingCandidate.Weapon, pendingCandidate.Grade), "Could not equip matching reward fixture."); // 실제 무기 장착
            }
            weapons.Slots[0].Equip(pendingCandidate.Weapon, pendingCandidate.Grade, 2, 0, 0); // 장착 재료 강화 이력 구성
            synergy.Recalculate(); // 직접 강화 구성 후 속성 집계
            Require(weapons.OccupiedSlotCount == count, "Controlled equipped count mismatch."); // 시험 무기 수 확인
        }

        private bool SameCandidates() // 기존 보상 후보 순서와 내용 비교
        {
            if (level.CurrentCandidates.Count != candidates.Length) // 후보 수 확인
            {
                return false; // 후보 수 변경
            }
            for (int index = 0; index < candidates.Length; index++) // 후보 순서 순회
            {
                if (level.CurrentCandidates[index].Weapon != candidates[index].Weapon || level.CurrentCandidates[index].Grade != candidates[index].Grade) // 무기 또는 등급 변경 확인
                {
                    return false; // 후보 변경 감지
                }
            }
            return true; // 모든 후보 일치
        }

        private void AssertRewardChoice() // 보상 방식 화면과 보류 상태 확인
        {
            WeaponRewardCandidate stored = (WeaponRewardCandidate)typeof(SnakeLevelUpController).GetField("pendingWeapon", PrivateInstance).GetValue(level); // 보류된 보상 조회
            Require(level.IsPresentingLevelUp && experience.IsLevelUpPending && !merge.IsMergeOpen && rewardPanel.gameObject.activeInHierarchy && GameManager.Instance.IsPaused && Mathf.Approximately(Time.timeScale, 0f), "Pending reward choice was not restored paused."); // 보상 화면과 정지 확인
            Require(stored.Weapon == pendingCandidate.Weapon && stored.Grade == pendingCandidate.Grade && SameCandidates(), "Pending reward or candidate list changed."); // 같은 보상과 후보 확인
        }

        private IEnumerator VerifyStraightMovement() // 합성 중 실제 회전과 가속 입력 확인
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.A, Key.Space)); // 회전과 가속 입력
            yield return null; // 입력 시스템 갱신
            yield return null; // 실제 입력 처리
            Require(input.TurnInput < -0.5f && input.BoostPressed, "Virtual movement controls did not reach gameplay."); // 실제 입력 전달 확인
            Vector3 start = movement.transform.position; // 시작 위치
            Vector3 direction = movement.transform.up; // 진행 방향
            Quaternion rotation = movement.transform.rotation; // 시작 회전
            float realStart = Time.realtimeSinceStartup; // 시작 실제 시간
            float gameStart = Time.time; // 시작 게임 시간
            yield return new WaitForSecondsRealtime(0.3f); // 실제 시간 이동 대기
            float realDelta = Time.realtimeSinceStartup - realStart; // 경과 실제 시간
            float gameDelta = Time.time - gameStart; // 경과 게임 시간
            Vector3 delta = movement.transform.position - start; // 실제 이동량
            Require(realDelta > 0f && Mathf.Abs(gameDelta / realDelta - 0.7f) < 0.12f && Quaternion.Angle(rotation, movement.transform.rotation) < 0.1f && Vector3.Dot(delta, direction) > 0.1f && Vector3.Cross(delta, direction).magnitude < 0.03f, "Reward merge did not advance straight at 70 percent speed."); // 직진과 시간 배율 확인
            Require(stamina.IsBoostBlocked && !stamina.IsBoosting, "Boost remained active during reward merge."); // 가속 차단 확인
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); // 이동 입력 해제
            yield return null; // 입력 해제 대기
        }

        private void FinishNextLevel() // 연속 두 번째 레벨 보상 완료
        {
            Require(level.IsPresentingLevelUp && level.CurrentCandidates.Count > 0, "Second level candidates are unavailable."); // 다음 레벨 보상 확인
            ClickLevel(0); // 실제 두 번째 레벨 후보 선택
            if (experience.IsLevelUpPending) // 합성 가능 후보의 방식 선택 확인
            {
                ClickLevel(0); // 새 무기 장착 선택
            }
            Require(!experience.IsLevelUpPending && !level.IsPresentingLevelUp && experience.CurrentLevel == 3, "Consecutive level-up did not complete exactly once."); // 연속 레벨 완료 확인
        }

        private IEnumerator VerifyNumericEntry() // 실제 숫자 키의 단일 단계 진입
        {
            StartLevel(1); // 시험용 레벨업 시작
            SelectControlledCandidate(false); // 첫 호환 장착 재료 구성
            Require(weapons.TryEquipAt(1, pendingCandidate.Weapon, pendingCandidate.Grade), "Could not equip a second matching material."); // 두 번째 재료 추가
            ClickLevel(0); // 실제 보상 후보 선택
            AssertRewardChoice(); // 보상 처리 방식 확인
            yield return KeyTap(Key.Digit2); // 실제 숫자 2로 합성 진입
            Require(merge.IsMergeOpen && GameManager.Instance.IsPlaying && Mathf.Approximately(Time.timeScale, 0.7f), "Digit 2 did not open reward merge."); // 합성 진입 확인
            string title = PanelText(mergePanel, "titleText"); // 실제 합성 화면 제목
            string selected = PanelText(mergePanel, "candidateLabel02"); // 현재 장착 재료 문구
            Debug.Log($"[LevelRewardMerge] Numeric entry title='{title}', selected='{selected}'."); // 실제 키 처리 결과 기록
            Require(title == "두 번째 재료 선택" && selected.Contains("몸통 1:"), "Digit 2 also confirmed a material in the opening frame."); // 첫 재료 선택 보존
            ClickMerge(2); // 실제 다음 재료 버튼
            Require(PanelText(mergePanel, "candidateLabel02").Contains("몸통 2:"), "Second matching material was not selectable after numeric entry."); // 두 번째 재료 접근 확인
            yield return KeyTap(Key.Escape); // 보상 합성 취소
            AssertRewardChoice(); // 같은 보상 선택으로 복귀
        }

        private IEnumerator VerifyMergeDisable() // 합성 제어기만 비활성화하는 경우
        {
            StartLevel(1); // 시험용 레벨업 시작
            SelectControlledCandidate(false); // 빈 슬롯과 장착 재료 준비
            ClickLevel(0); // 보상 후보 선택
            ClickLevel(1); // 보상 합성 진입
            Require(merge.IsMergeOpen && mergePanel.gameObject.activeInHierarchy, "Reward merge panel did not open."); // 합성 화면 표시 확인
            merge.enabled = false; // 합성 제어기만 비활성화
            yield return null; // 실제 비활성 후 갱신
            Debug.Log($"[LevelRewardMerge] Reward merge disabled: panel={mergePanel.gameObject.activeInHierarchy}, rewardPanel={rewardPanel.gameObject.activeInHierarchy}, paused={GameManager.Instance.IsPaused}, pending={experience.IsLevelUpPending}."); // 실제 복구 상태 기록
            AssertRewardChoice(); // 동일 보상 선택 복귀
            Require(!mergePanel.gameObject.activeInHierarchy, "Disabling reward merge left its separate panel visible."); // 합성 패널 정리 확인
            merge.enabled = true; // 합성 제어기 재활성화
            ClickLevel(0); // 보상을 빈 슬롯에 장착
            Require(!experience.IsLevelUpPending && GameManager.Instance.IsPlaying, "Reward could not complete after merge reactivation."); // 보상 완료 확인
            Require(weapons.TryEquipAt(0, pendingCandidate.Weapon, pendingCandidate.Grade) && weapons.TryEquipAt(1, pendingCandidate.Weapon, pendingCandidate.Grade), "Could not prepare ordinary merge fixture."); // 일반 합성 재료 준비
            yield return KeyTap(Key.E); // 실제 E 일반 합성
            Require(merge.IsMergeOpen && mergePanel.gameObject.activeInHierarchy, "Ordinary merge panel did not open."); // 일반 합성 표시 확인
            merge.enabled = false; // 일반 합성 제어기 비활성화
            yield return null; // 화면 갱신 대기
            Require(!merge.IsMergeOpen && !mergePanel.gameObject.activeInHierarchy && GameManager.Instance.IsPlaying && Mathf.Approximately(Time.timeScale, 1f), "Disabling ordinary merge left the panel or slowed time."); // 일반 합성 패널과 시간 복원
            merge.enabled = true; // 제어기 다시 활성화
        }

        private IEnumerator VerifyLevelDisable() // 레벨업 선택 단계별 비활성 복구
        {
            string[] titles = // 단계별 기대 화면
            {
                "레벨 상승!", "포탑 처리 선택", "교체할 몸통 선택", "포탑 처리 선택" // 실제 네 단계 제목
            };
            for (int stage = 0; stage < 4; stage++) // 후보와 방식 및 교체와 합성 순회
            {
                StartLevel(1); // 독립 시험 레벨업
                SelectControlledCandidate(stage == 2); // 교체 단계만 만석 구성
                if (stage >= 1) // 보상 후보 선택 단계
                {
                    ClickLevel(0); // 첫 무기 보상 선택
                }
                if (stage == 2) // 만석 교체 단계
                {
                    ClickLevel(0); // 장착 또는 교체 선택
                }
                if (stage == 3) // 보상 합성 단계
                {
                    ClickLevel(1); // 보상 합성 진입
                }
                level.enabled = false; // 레벨업 제어기만 비활성화
                yield return null; // 비활성 처리 대기
                level.enabled = true; // 레벨업 제어기 재활성화
                yield return null; // 복구 상태 갱신
                yield return null; // 다음 프레임 복구 대기
                string title = PanelText(rewardPanel, "titleText"); // 실제 복구 화면 제목
                Debug.Log($"[LevelRewardMerge] Level controller reenabled stage={stage}, title='{title}', paused={GameManager.Instance.IsPaused}, pending={experience.IsLevelUpPending}, panel={rewardPanel.gameObject.activeInHierarchy}, merge={merge.IsMergeOpen}."); // 단계별 실제 상태
                Require(level.IsPresentingLevelUp && experience.IsLevelUpPending && GameManager.Instance.IsPaused && Mathf.Approximately(Time.timeScale, 0f) && rewardPanel.gameObject.activeInHierarchy && !merge.IsMergeOpen && SameCandidates() && title == titles[stage], "Level controller did not restore the original pending selection at stage " + stage); // 원래 선택과 정지 복구
                if (stage == 0) // 후보 화면 복구
                {
                    ClickLevel(0); // 같은 보상 다시 선택
                    ClickLevel(0); // 빈 슬롯 장착
                }
                else if (stage == 2) // 교체 화면 복구
                {
                    ClickLevel(1); // 기존 선택 슬롯 교체
                }
                else // 처리 방식 화면 복구
                {
                    ClickLevel(0); // 빈 슬롯 장착
                }
                Require(!experience.IsLevelUpPending && !level.IsPresentingLevelUp && GameManager.Instance.IsPlaying, "Restored level-up could not complete at stage " + stage); // 보상 완료 확인
            }
        }

        private IEnumerator VerifyLastMaterialLoss() // 마지막 호환 몸통의 실제 물리 손실
        {
            StartLevel(1); // 시험용 레벨업 시작
            SelectControlledCandidate(false); // 실제 후보와 장착 재료 준비
            weapons.Slots[0].Clear(); // 첫 슬롯 재료 제거
            int last = weapons.SlotCount - 1; // 마지막 몸통 슬롯 위치
            Require(weapons.TryEquipAt(last, pendingCandidate.Weapon, pendingCandidate.Grade), "Could not equip the last material."); // 마지막 몸통에만 재료 장착
            ClickLevel(0); // 같은 무기 보상 선택
            ClickLevel(1); // 보상 합성 진입
            Require(merge.IsMergeOpen && GameManager.Instance.IsPlaying, "Could not start last-material merge."); // 합성 화면 확인
            int before = body.CurrentBodyCount; // 접촉 전 몸통 수
            GameObject hazard = new GameObject("LevelRewardLastMaterialHazard"); // 실제 물리 위험 생성
            hazard.transform.position = movement.transform.position; // 플레이어와 같은 위치
            CircleCollider2D collider = hazard.AddComponent<CircleCollider2D>(); // 2D 접촉 영역 생성
            collider.radius = 0.2f; // 실제 접촉 범위
            collider.isTrigger = true; // 트리거 충돌 설정
            hazard.AddComponent<SnakeContactHazard>().Configure(SnakeContactHazardType.EnemyDirect); // 직접 적 충돌 설정
            Physics2D.SyncTransforms(); // 물리 위치 동기화
            yield return new WaitForFixedUpdate(); // 실제 물리 단계 대기
            yield return null; // 물리 후 처리 대기
            Require(collisionProbe.EnemyEntries > 0 && body.CurrentBodyCount == before - 1, "Last-material physical contact was not observed."); // 실제 충돌 확인
            AssertRewardChoice(); // 보상 방식 화면 복귀 확인
            Button mergeButton = (Button)typeof(LevelUpPanelController).GetField("candidateButton02", PrivateInstance).GetValue(rewardPanel); // 실제 합성 선택 버튼 조회
            Debug.Log($"[LevelRewardMerge] Last material lost: body={before}->{body.CurrentBodyCount}, mergeButton={mergeButton.interactable}, pending={experience.IsLevelUpPending}."); // 재료 손실 후 실제 상태
            Require(!mergeButton.interactable, "Reward merge remained selectable after losing the last matching material."); // 불가능한 합성 비활성 확인
            UnityEngine.Object.Destroy(hazard); // 시험 위험 객체 제거
        }

        private IEnumerator VerifyRemainingBoundaries() // 각 경계를 독립 상태에서 기록
        {
            StartLevel(1); // 보상 합성 중 비활성 시험
            SelectControlledCandidate(false); // 호환 장착 재료 준비
            ClickLevel(0); // 보상 후보 선택
            ClickLevel(1); // 보상 합성 열기
            merge.enabled = false; // 합성 제어기만 비활성화
            yield return null; // 비활성 처리 대기
            bool rewardClosed = !merge.IsMergeOpen && !mergePanel.gameObject.activeInHierarchy && rewardPanel.gameObject.activeInHierarchy && GameManager.Instance.IsPaused && experience.IsLevelUpPending; // 보상 합성 패널 정리와 복귀
            RecordBoundary("reward merge disable", rewardClosed, $"mergePanel={mergePanel.gameObject.activeInHierarchy}, rewardPanel={rewardPanel.gameObject.activeInHierarchy}, paused={GameManager.Instance.IsPaused}, pending={experience.IsLevelUpPending}"); // 독립 결과 기록
            ResetBoundaryFixture(); // 보상 상태 초기화
            Require(weapons.TryEquipAt(0, pendingCandidate.Weapon, pendingCandidate.Grade) && weapons.TryEquipAt(1, pendingCandidate.Weapon, pendingCandidate.Grade), "Could not prepare ordinary merge boundary."); // 일반 합성 재료 준비
            yield return KeyTap(Key.E); // 일반 합성 키 입력
            Require(merge.IsMergeOpen, "Ordinary merge fixture did not open."); // 일반 합성 진입 확인
            merge.enabled = false; // 합성 제어기 비활성화
            yield return null; // 비활성 처리 대기
            bool ordinaryClosed = !merge.IsMergeOpen && !mergePanel.gameObject.activeInHierarchy && GameManager.Instance.IsPlaying && Mathf.Approximately(Time.timeScale, 1f); // 일반 합성 화면과 속도 정리
            RecordBoundary("ordinary merge disable", ordinaryClosed, $"mergePanel={mergePanel.gameObject.activeInHierarchy}, scale={Time.timeScale:F2}"); // 독립 결과 기록
            ResetBoundaryFixture(); // 일반 합성 상태 초기화
            string[] titles = // 복구 화면 기대값
            {
                "레벨 상승!", "포탑 처리 선택", "교체할 몸통 선택", "포탑 처리 선택" // 실제 네 단계 제목
            };
            for (int stage = 0; stage < 4; stage++) // 레벨업 선택 단계 순회
            {
                StartLevel(1); // 독립 레벨업 시작
                SelectControlledCandidate(stage == 2); // 교체 시험만 만석 구성
                if (stage >= 1) // 보상 후보 선택 단계
                {
                    ClickLevel(0); // 보상 후보 선택
                }
                if (stage == 2) // 만석 교체 단계
                {
                    ClickLevel(0); // 교체 방식 선택
                }
                if (stage == 3) // 보상 합성 단계
                {
                    ClickLevel(1); // 보상 합성 선택
                }
                level.enabled = false; // 레벨업 제어기 비활성화
                yield return null; // 비활성 처리 대기
                level.enabled = true; // 레벨업 제어기 재활성화
                yield return null; // 첫 복구 프레임
                yield return null; // 둘째 복구 프레임
                string title = PanelText(rewardPanel, "titleText"); // 실제 복구 화면
                bool restored = level.IsPresentingLevelUp && experience.IsLevelUpPending && GameManager.Instance.IsPaused && Mathf.Approximately(Time.timeScale, 0f) && rewardPanel.gameObject.activeInHierarchy && !merge.IsMergeOpen && SameCandidates() && title == titles[stage]; // 후보와 정지 및 화면 복구
                RecordBoundary("level disable stage " + stage, restored, $"title='{title}', paused={GameManager.Instance.IsPaused}, pending={experience.IsLevelUpPending}, panel={rewardPanel.gameObject.activeInHierarchy}, candidates={SameCandidates()}"); // 단계별 결과 기록
                ResetBoundaryFixture(); // 각 단계 상태 초기화
            }
            StartLevel(1); // 마지막 호환 재료 손실 시험
            SelectControlledCandidate(false); // 호환 무기 구성
            weapons.Slots[0].Clear(); // 첫 슬롯 재료 제거
            int last = weapons.SlotCount - 1; // 마지막 몸통 위치
            Require(weapons.TryEquipAt(last, pendingCandidate.Weapon, pendingCandidate.Grade), "Could not equip last boundary material."); // 마지막 몸통에 재료 장착
            ClickLevel(0); // 실제 보상 후보 선택
            ClickLevel(1); // 보상 합성 선택
            Require(merge.IsMergeOpen && GameManager.Instance.IsPlaying, "Last-material boundary could not start."); // 합성 상태 확인
            int before = body.CurrentBodyCount; // 충돌 전 몸통 수
            GameObject hazard = new GameObject("LevelRewardBoundaryHazard"); // 시험용 물리 위험 생성
            hazard.transform.position = movement.transform.position; // 플레이어 위치 사용
            CircleCollider2D collider = hazard.AddComponent<CircleCollider2D>(); // 실제 2D 충돌체 생성
            collider.radius = 0.2f; // 접촉 영역 설정
            collider.isTrigger = true; // 트리거 충돌 설정
            hazard.AddComponent<SnakeContactHazard>().Configure(SnakeContactHazardType.EnemyDirect); // 실제 적 직접 충돌
            Physics2D.SyncTransforms(); // 물리 위치 갱신
            yield return new WaitForFixedUpdate(); // 실제 물리 단계 대기
            yield return null; // 물리 후 처리 대기
            Button button = (Button)typeof(LevelUpPanelController).GetField("candidateButton02", PrivateInstance).GetValue(rewardPanel); // 실제 합성 버튼 조회
            bool lossHandled = collisionProbe.EnemyEntries > 0 && body.CurrentBodyCount == before - 1 && !merge.IsMergeOpen && rewardPanel.gameObject.activeInHierarchy && GameManager.Instance.IsPaused && experience.IsLevelUpPending && !button.interactable; // 물리 손실과 비활성 확인
            RecordBoundary("last matching material loss", lossHandled, $"entries={collisionProbe.EnemyEntries}, body={before}->{body.CurrentBodyCount}, mergeButton={button.interactable}, paused={GameManager.Instance.IsPaused}"); // 독립 결과 기록
            UnityEngine.Object.Destroy(hazard); // 시험 위험 제거
        }

        private void RecordBoundary(string name, bool passed, string details) // 개별 경계 결과 기록
        {
            if (!passed) // 실패 여부 확인
            {
                boundaryFailures++; // 실패 개수 증가
            }
            Debug.Log($"[LevelRewardBoundary] {(passed ? "PASS" : "FAIL")} {name}: {details}"); // 각 경계 결과 기록
        }

        private void ResetBoundaryFixture() // 각 경계 뒤 통제된 상태 초기화
        {
            if (merge.IsMergeOpen) // 열린 합성 확인
            {
                merge.CloseMergePanel(); // 남은 합성 종료
            }
            level.enabled = false; // 레벨업 구독과 상태 정리
            rewardPanel.Hide(); // 남은 보상 화면 숨김
            mergePanel.Hide(); // 남은 합성 화면 숨김
            experience.ResetProgression(); // 시험 경험치와 대기 초기화
            string[] flags = { "restoreRewardOnEnable", "presentingLevelUp", "selectingRewardAction", "selectingReplacement", "awaitingRewardMerge", "ownsGamePause" }; // 독립 시험 복구 상태 필드
            foreach (string flag in flags) // 남은 보상 상태 순회
            {
                FieldInfo field = typeof(SnakeLevelUpController).GetField(flag, PrivateInstance); // 실제 비공개 상태 조회
                if (field != null) // 해당 버전 상태 존재 확인
                {
                    field.SetValue(level, false); // 통제 시험 상태 초기화
                }
            }
            FieldInfo candidateField = typeof(SnakeLevelUpController).GetField("currentCandidates", PrivateInstance); // 후보 목록 필드 조회
            if (candidateField != null) // 후보 목록 존재 확인
            {
                ((List<WeaponRewardCandidate>)candidateField.GetValue(level)).Clear(); // 시험 간 후보 제거
            }
            GameManager.Instance.StartGame(); // 정상 플레이 상태 복원
            merge.enabled = true; // 합성 제어기 활성화
            level.enabled = true; // 레벨업 제어기 활성화
            invulnerability.ClearInvulnerability(); // 이전 전체 무적 제거
            health.ShieldController?.Clear(); // 이전 보호막 제거
            SnakeExternalCollision external = GetComponent<SnakeExternalCollision>(); // 실제 외부 충돌 처리기
            if (external != null) // 충돌 처리기 존재 확인
            {
                typeof(SnakeExternalCollision).GetField("nextEnemyContactAllowedTime", PrivateInstance).SetValue(external, 0f); // 시험 간 직접 충돌 보호 초기화
            }
            collisionProbe.EnemyEntries = 0; // 시험 간 실제 접촉 기록 초기화
        }

        private static string PanelText(MonoBehaviour panel, string field) // 실제 UI 문구 조회
        {
            Text text = (Text)panel.GetType().GetField(field, PrivateInstance).GetValue(panel); // 직렬화 문구 조회
            Require(text != null, "UI text missing: " + field); // 문구 연결 확인
            return text.text; // 현재 표시 문구 반환
        }

        private IEnumerator VerifyCollisionAndDefeat() // 실제 물리 충돌과 패배 정리
        {
            StartLevel(1); // 피해 시험 레벨업 시작
            SelectControlledCandidate(false); // 빈 슬롯 및 합성 재료 준비
            ClickLevel(0); // 실제 레벨 보상 선택
            ClickLevel(1); // 보상 합성 선택
            Require(merge.IsMergeOpen && GameManager.Instance.IsPlaying, "Could not start collision fixture merge."); // 충돌 시험 상태 확인
            int before = body.CurrentBodyCount; // 충돌 전 몸통 수
            GameObject hazard = new GameObject("LevelRewardMergeCollisionHazard"); // 통제된 위험 객체 생성
            hazard.transform.position = movement.transform.position; // 실제 플레이어 위치 설정
            CircleCollider2D collider = hazard.AddComponent<CircleCollider2D>(); // 실제 2D 충돌체 생성
            collider.radius = 0.2f; // 시험 접촉 범위
            collider.isTrigger = true; // 물리 트리거 설정
            hazard.AddComponent<SnakeContactHazard>().Configure(SnakeContactHazardType.EnemyDirect); // 적 직접 충돌 종류 설정
            Physics2D.SyncTransforms(); // 물리 위치 동기화
            yield return new WaitForFixedUpdate(); // 실제 물리 단계 대기
            yield return null; // 물리 후 처리 대기
            Require(collisionProbe.EnemyEntries > 0 && body.CurrentBodyCount == before - 1, "Actual 2D enemy contact did not remove a body."); // 실제 접촉과 몸통 손실 확인
            AssertRewardChoice(); // 몸통 손실 후 보상 재선택 화면
            Debug.Log($"[LevelRewardMerge] Actual 2D enemy contact entries={collisionProbe.EnemyEntries}, body={before}->{body.CurrentBodyCount}; paused reward restored."); // 실제 물리 접촉 결과 기록
            UnityEngine.Object.Destroy(hazard); // 시험 충돌체 제거
            ClickLevel(1); // 보상 합성 재진입
            Require(merge.IsMergeOpen && GameManager.Instance.IsPlaying, "Could not reopen reward merge after contact."); // 재진입 확인
            invulnerability.ClearInvulnerability(); // 시험용 무적 제거
            health.ShieldController?.Clear(); // 시험용 보호막 제거
            health.TakeDamage(health.MaximumHealth * body.CurrentBodyCount + 1); // 치명적인 공용 체력 피해
            yield return null; // 패배 상태와 화면 갱신
            string defeatState = $"state={GameManager.Instance.CurrentState}, scale={Time.timeScale:F2}, merge={merge.IsMergeOpen}, rewardPanel={rewardPanel.gameObject.activeInHierarchy}, pending={experience.IsLevelUpPending}, body={body.CurrentBodyCount}, hp={health.CurrentHealth}"; // 실제 패배 상태 요약
            Debug.Log("[LevelRewardMerge] Lethal damage outcome: " + defeatState); // 패배 상태 상세 기록
            Require(GameManager.Instance.CurrentState == GameState.Defeated && Mathf.Approximately(Time.timeScale, 0f) && !merge.IsMergeOpen && !rewardPanel.gameObject.activeInHierarchy && experience.IsLevelUpPending, "Incorrect lethal damage cleanup: " + defeatState); // 패배 화면과 보상 보류 확인
            Debug.Log("[LevelRewardMerge] Actual enemy contact cancelled to paused reward; lethal health damage left defeat and pending reward intact."); // 실제 충돌과 피해 경로 구분 기록
        }

        private IEnumerator KeyTap(Key key) // 실제 가상 키 누름과 해제
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); // 키 누름 전달
            yield return null; // 게임 입력 대기
            yield return null; // 후속 처리 대기
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); // 키 해제 전달
            yield return null; // 키 해제 대기
            yield return null; // 다음 프레임 대기
        }

        private void ClickLevel(int index) // 실제 레벨업 또는 엘리트 버튼 이벤트
        {
            string[] names = { "candidateButton01", "candidateButton02", "candidateButton03" }; // 버튼 필드 이름
            Button button = (Button)typeof(LevelUpPanelController).GetField(names[index], PrivateInstance).GetValue(rewardPanel); // 직렬화된 버튼 조회
            Require(button != null && button.interactable && button.gameObject.activeInHierarchy, "Level reward button unavailable: " + index); // 버튼 사용 가능 확인
            button.onClick.Invoke(); // 실제 UI 버튼 이벤트
        }

        private void ClickMerge(int index) // 실제 보상 합성 버튼 이벤트
        {
            string[] names = { "candidateButton01", "candidateButton02", "candidateButton03" }; // 버튼 필드 이름
            Button button = (Button)typeof(WeaponMergePanelController).GetField(names[index], PrivateInstance).GetValue(mergePanel); // 실제 합성 버튼 조회
            Require(button != null && button.interactable && button.gameObject.activeInHierarchy, "Reward merge button unavailable: " + index); // 버튼 사용 가능 확인
            button.onClick.Invoke(); // 실제 UI 버튼 이벤트
        }

        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedLevelRewardMergeVerification.Finish(1, message); // 실패 기록과 종료
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }

        private void Cleanup() // 가상 입력과 설정 정리
        {
            InputSystem.RemoveDevice(keyboard); // 가상 키보드 제거
            if (originalSettings != null) // 원래 설정 존재 확인
            {
                InputSystem.settings = originalSettings; // 기존 입력 설정 복원
            }
            Application.runInBackground = originalBackground; // 기존 백그라운드 상태 복원
            if (testSettings != null && InputSystem.settings != testSettings) // 설정 사본 사용 여부 확인
            {
                UnityEngine.Object.Destroy(testSettings); // 사용하지 않는 사본 제거
            }
        }
    }
}
