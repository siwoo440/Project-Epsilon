using System.Collections; // 프레임 대기
using System.Reflection; // 실제 보상 버튼 조회
using ProjectEpsilon.Combat; // 포탑과 속성
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Data; // 포탑 데이터
using ProjectEpsilon.Player; // 몸통 관리
using ProjectEpsilon.Progression; // 합성 처리
using ProjectEpsilon.UI; // 기존 합성 화면
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 게임 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.InputSystem; // 가상 키보드
using UnityEngine.InputSystem.LowLevel; // 키보드 상태
using UnityEngine.UI; // 실제 보상 버튼

namespace ProjectEpsilon.Editor // 편집기 시험 영역
{ // 이름 공간 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedInstantMergeVerification // 즉시 합성 배치 시험
    { // 시험 클래스 시작
        private const string Key = "EpsilonInstantMergeVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 중복 방지
        private static double started; // 시험 시작 시각
        static AutomatedInstantMergeVerification() // 재시작 후 시험 복원
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 진행 중 시험 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 시험 감시 연결
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 진입점 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 전투 씬 열기
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 시작 감시 연결
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        } // 진입점 종료
        private static void Tick() // 시험 실행 준비
        { // 감시 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 다음 프레임 대기
            } // 조건 종료
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 시작 포탑 선택 처리
            { // 조건 시작
                return; // 실제 전투 대기
            } // 조건 종료
            if (started <= 0d) // 최초 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 제한 시간 기준
            } // 조건 종료
            if (EditorApplication.timeSinceStartup - started > 90d) // 제한 시간 확인
            { // 조건 시작
                Finish(false, "Timed out."); // 시간 초과 실패
                return; // 실행 중단
            } // 조건 종료
            SnakeMovement player = Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (!injected && player != null && GameManager.Instance != null && GameManager.Instance.IsPlaying && Time.time > 0.35f) // 전투 초기화 확인
            { // 조건 시작
                injected = true; // 실행기 중복 방지
                player.gameObject.AddComponent<InstantMergeVerificationRunner>(); // 실제 씬 시험 실행
            } // 조건 종료
        } // 감시 종료
        public static void Finish(bool success, string message) // 시험 결과 기록
        { // 결과 처리 시작
            SessionState.SetBool(Key, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 감시 해제
            Debug.Log("[InstantMerge] " + (success ? "PASS: " : "FAIL: ") + message); // 시험 결과 출력
            EditorApplication.Exit(success ? 0 : 1); // 결과 코드 종료
        } // 결과 처리 종료
    } // 시험 클래스 종료
    public sealed class InstantMergeVerificationRunner : MonoBehaviour // 실제 합성 시험
    { // 실행기 시작
        private IEnumerator Start() // 시험 순서 실행
        { // 코루틴 시작
            yield return null; // 씬 초기화 대기
            SnakeWeaponManager weapons = Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            SnakeWeaponMergeController merge = Object.FindFirstObjectByType<SnakeWeaponMergeController>(); // 실제 합성 관리자
            WeaponAttributeSynergyManager synergy = Object.FindFirstObjectByType<WeaponAttributeSynergyManager>(); // 실제 속성 관리자
            SnakeBodyManager body = Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            WeaponMergePanelController panel = Object.FindFirstObjectByType<WeaponMergePanelController>(FindObjectsInactive.Include); // 이전 합성 화면
            if (!Check(weapons != null && merge != null && synergy != null && body != null && panel != null, "Missing components.")) yield break; // 필수 구성 확인
            WeaponData weapon = weapons.Slots[0].Weapon; // 시작 포탑 보관
            if (!Check(weapon != null && weapon.MaxGrade >= 2 && weapons.SlotCount >= 2, "Missing merge materials.")) yield break; // 시험 재료 확인
            foreach (SnakeWeaponSlot slot in weapons.Slots) slot.Clear(); // 기존 장착 제거
            if (!Check(weapons.TryEquipAt(0, weapon, 1) && weapons.TryEquipAt(1, weapon, 1), "Could not equip materials.")) yield break; // 같은 포탑 두 개 장착
            weapons.Slots[0].Equip(weapon, 1, 2, 0, 0); // 첫 재료의 기존 강화 기록
            synergy.Recalculate(); // 초기 속성 수량 갱신
            int completionCount = 0; // 합성 완료 알림 횟수
            merge.MergeClosed += merged => completionCount += merged ? 1 : 0; // 기존 보상 경로의 완료 알림 관측
            if (!Check(synergy.GetCount(weapon.Attribute) == 2 && merge.HasMergeCandidates(), "Initial pair or synergy invalid.")) yield break; // 재료와 시너지 확인
            InputSettings originalSettings = InputSystem.settings; // 기존 입력 설정 보관
            InputSettings testSettings = Object.Instantiate(originalSettings); // 배치 전용 입력 설정
            testSettings.hideFlags = HideFlags.HideAndDontSave; // 시험 설정 저장 방지
            testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; // 배치 입력 전달
            testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // 배치 포커스 무시
            InputSystem.settings = testSettings; // 시험 입력 설정 적용
            bool originalBackground = Application.runInBackground; // 기존 백그라운드 설정
            Application.runInBackground = true; // 배치 키 입력 허용
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>(); // 가상 키보드 생성
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E)); // 실제 E 입력 전송
            yield return null; // 입력 장치 갱신 대기
            yield return null; // 실제 게임 입력 처리 대기
            InputSystem.RemoveDevice(keyboard); // 가상 키보드 제거
            if (originalSettings != null) // 기존 설정이 남아 있는 경우
            { // 조건 시작
                InputSystem.settings = originalSettings; // 기존 입력 설정 복원
                Object.Destroy(testSettings); // 시험 입력 설정 제거
            } // 조건 종료
            Application.runInBackground = originalBackground; // 기존 백그라운드 설정 복원
            if (!Check(weapons.OccupiedSlotCount == 1, "E input did not trigger instant merge.")) yield break; // 실제 합성 키 결과 확인
            if (!Check(completionCount == 1, "Instant merge did not notify the reward flow.")) yield break; // 동기 완료 이벤트 확인
            if (!Check(!merge.IsMergeOpen && !panel.gameObject.activeInHierarchy && weapons.OccupiedSlotCount == 1, "Merge opened a selection or did not consume a turret.")) yield break; // 선택 화면 부재와 소비 확인
            if (!Check(weapons.Slots[0].Weapon == weapon && weapons.Slots[0].Grade == 2 && weapons.Slots[0].DamageUpgrades == 2 && weapons.Slots[0].SpeedUpgrades == 0 && weapons.Slots[0].RangeUpgrades == 0, "Merged grade or retained upgrades incorrect.")) yield break; // 결과 등급과 기존 강화 확인
            if (!Check(synergy.GetCount(weapon.Attribute) == 1 && synergy.GetStage(weapon.Attribute) == 0, "Synergy did not update immediately.")) yield break; // 합성 직후 시너지 확인
            if (!Check(weapons.TryEquipAt(1, weapon, 1) && !merge.HasMergeCandidates() && !merge.OpenMergePanel(), "Different grades were merged.")) yield break; // 서로 다른 등급 차단
            if (!Check(weapons.TryMergeReward(1, weapon, 1, true, WeaponMergeUpgrade.None), "Reward could not merge without an upgrade choice.")) yield break; // 보상 포탑 즉시 합성
            if (!Check(weapons.Slots[0].Weapon == weapon && weapons.Slots[0].Grade == 2, "Reward merge result incorrect.")) yield break; // 보상 합성 결과 확인
            EliteRewardController elite = GetComponent<EliteRewardController>(); // 실제 엘리트 보상 제어기
            if (!Check(elite != null && merge.HasMergeCandidates(), "Elite merge fixture missing.")) yield break; // 엘리트 합성 재료 확인
            elite.QueueReward(); // 실제 엘리트 보상 대기
            FieldInfo eliteCandidates = typeof(EliteRewardController).GetField("candidates", BindingFlags.NonPublic | BindingFlags.Instance); // 보상 종류 고정 필드
            if (!Check(eliteCandidates != null, "Elite reward candidates missing.")) yield break; // 실제 보상 후보 확인
            eliteCandidates.SetValue(elite, new[] { 3, 0, 1 }); // 첫 보상을 합성으로 고정
            if (!Check(elite.ProcessPendingReward() && elite.TrySelectCandidate(0), "Elite reward instant merge failed.")) yield break; // 실제 보상 화면과 합성 선택
            if (!Check(elite.PendingRewardCount == 0 && !elite.IsAwaitingMerge && GameManager.Instance.IsPlaying && weapons.Slots[0].Grade == 3, "Elite reward did not finish exactly once.")) yield break; // 동기 완료와 보상 소비 확인
            SnakeExperience experience = GetComponent<SnakeExperience>(); // 실제 경험치 관리자
            SnakeLevelUpController level = GetComponent<SnakeLevelUpController>(); // 실제 레벨업 제어기
            LevelUpPanelController rewardPanel = Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 레벨업 화면
            if (!Check(experience != null && level != null && rewardPanel != null, "Level reward components missing.")) yield break; // 보상 구성 확인
            experience.Configure(10, 1f); // 연속 레벨업 시험 기준
            experience.ResetProgression(); // 경험치 초기화
            experience.AddExperience(20); // 실제 두 단계 경험치 지급
            if (!Check(level.IsPresentingLevelUp && experience.IsLevelUpPending && GameManager.Instance.IsPaused && level.CurrentCandidates.Count > 0, "Controlled level-up did not start.")) yield break; // 첫 레벨업 확인
            WeaponRewardCandidate reward = level.CurrentCandidates[0]; // 실제 첫 보상 후보
            if (!Check(reward.IsValid && reward.Grade < reward.Weapon.MaxGrade, "First reward is not mergeable.")) yield break; // 실제 보상 유효성
            foreach (SnakeWeaponSlot slot in weapons.Slots) slot.Clear(); // 기존 포탑 제거
            if (!Check(weapons.TryEquipAt(0, reward.Weapon, reward.Grade), "Could not equip matching reward material.")) yield break; // 보상과 같은 포탑 장착
            weapons.Slots[0].Equip(reward.Weapon, reward.Grade, 2, 0, 0); // 기존 강화 이력 준비
            FieldInfo firstField = typeof(LevelUpPanelController).GetField("candidateButton01", BindingFlags.NonPublic | BindingFlags.Instance); // 실제 첫 후보 버튼 필드
            FieldInfo secondField = typeof(LevelUpPanelController).GetField("candidateButton02", BindingFlags.NonPublic | BindingFlags.Instance); // 실제 합성 버튼 필드
            Button firstButton = firstField == null ? null : firstField.GetValue(rewardPanel) as Button; // 실제 첫 후보 버튼
            Button secondButton = secondField == null ? null : secondField.GetValue(rewardPanel) as Button; // 실제 두 번째 버튼
            if (!Check(firstButton != null && secondButton != null, "Reward buttons missing.")) yield break; // 실제 버튼 확인
            firstButton.onClick.Invoke(); // 실제 첫 보상 선택
            if (!Check(secondButton.interactable, "Reward merge action unavailable.")) yield break; // 보상 합성 가능 확인
            secondButton.onClick.Invoke(); // 실제 보상 즉시 합성
            if (!Check(!merge.IsMergeOpen && weapons.Slots[0].Weapon == reward.Weapon && weapons.Slots[0].Grade == reward.Grade + 1 && weapons.Slots[0].DamageUpgrades == 2, "Level reward merge result incorrect.")) yield break; // 보상 합성과 기존 강화 확인
            if (!Check(experience.CurrentLevel == 3 && experience.IsLevelUpPending && level.IsPresentingLevelUp && GameManager.Instance.IsPaused, "Immediate merge did not advance to the next queued level.")) yield break; // 다음 레벨업의 정지와 대기 확인
            AutomatedInstantMergeVerification.Finish(true, "Immediate merge, synergy, elite reward and consecutive level reward flow passed."); // 전체 시험 성공
        } // 코루틴 종료
        private static bool Check(bool condition, string message) // 시험 조건 확인
        { // 검사 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedInstantMergeVerification.Finish(false, message); // 실패 기록과 종료
            } // 조건 종료
            return condition; // 검사 결과 반환
        } // 검사 종료
    } // 실행기 종료
} // 이름 공간 종료
