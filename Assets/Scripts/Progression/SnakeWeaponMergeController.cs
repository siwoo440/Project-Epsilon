using System; // 합성 결과 이벤트
using System.Collections.Generic; // 슬롯 목록
using ProjectEpsilon.Combat; // 포탑과 합성
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Player; // 기존 구성 인수
using ProjectEpsilon.UI; // 설정과 기존 화면
using UnityEngine; // 유니티 기능
using UnityEngine.InputSystem; // 키보드 입력

namespace ProjectEpsilon.Progression // 성장 기능 영역
{ // 영역 시작
    [DefaultExecutionOrder(150)] // 입력 처리 순서
    public sealed class SnakeWeaponMergeController : MonoBehaviour // 즉시 합성 제어기
    { // 제어기 시작
        [SerializeField] private SnakeWeaponManager weaponManager; // 장착 포탑 관리자
        [SerializeField] private WeaponMergePanelController mergePanel; // 기존 화면 숨김용 참조
        [SerializeField] private SnakeExperience experience; // 레벨업 대기 상태
        [SerializeField] private SnakeLevelUpController levelUpController; // 레벨업 처리기
        [SerializeField] private SettingsMenuController settingsMenu; // 설정 화면
        private bool pauseOwnedByInput; // 직접 일시정지 여부
        public bool IsMergeOpen => false; // 별도 합성 화면 없음
        public event Action<bool> MergeClosed; // 합성 완료 알림

        private void Start() // 기존 화면 초기화
        { // 함수 시작
            mergePanel?.Hide(); // 선택 화면 숨김
        } // 함수 종료

        public void ClearManualPause() // 일시정지 소유 해제
        { // 함수 시작
            pauseOwnedByInput = false; // 수동 정지 기록 제거
        } // 함수 종료

        public void Configure(SnakeWeaponManager manager, SnakeMovement movementController, SnakeStamina staminaController, WeaponMergePanelController panel, SnakeExperience experienceSource, SnakeLevelUpController levelController, float moveMultiplier) // 기존 씬 구성 호환
        { // 함수 시작
            weaponManager = manager; // 포탑 관리자 연결
            mergePanel = panel; // 기존 화면 연결
            experience = experienceSource; // 경험치 연결
            levelUpController = levelController; // 레벨업 연결
        } // 함수 종료

        private void Update() // 합성과 일시정지 입력
        { // 함수 시작
            if (IsLevelUpBlocking() || (levelUpController != null && levelUpController.ConsumedEscapeThisFrame)) // 레벨업 입력 우선
            { // 조건 시작
                return; // 전투 입력 차단
            } // 조건 종료
            Keyboard keyboard = Keyboard.current; // 현재 키보드
            if (keyboard == null || (InputBindingManager.Instance != null && InputBindingManager.Instance.IsRebinding)) // 장치와 키 변경 확인
            { // 조건 시작
                return; // 입력 생략
            } // 조건 종료
            if (settingsMenu == null) // 설정 화면 연결 확인
            { // 조건 시작
                settingsMenu = FindFirstObjectByType<SettingsMenuController>(); // 설정 화면 조회
            } // 조건 종료
            if ((settingsMenu != null && settingsMenu.IsOpen) || (RunFrontendController.Instance != null && (RunFrontendController.Instance.Stage == FrontendStage.Settings || RunFrontendController.Instance.Stage == FrontendStage.Tutorial))) // 설정과 안내 화면 확인
            { // 조건 시작
                return; // 설정 입력 우선
            } // 조건 종료
            GameManager manager = GameManager.Instance; // 현재 게임 관리자
            if (keyboard.escapeKey.wasPressedThisFrame) // 일시정지 입력 확인
            { // 조건 시작
                if (manager != null && manager.IsPlaying) // 진행 중 게임 확인
                { // 조건 시작
                    manager.PauseGame(); // 게임 일시정지
                    pauseOwnedByInput = true; // 직접 정지 기록
                    RunFrontendController.Instance?.ShowManualPause(); // 정지 화면 표시
                } // 조건 종료
                else if (manager != null && manager.IsPaused && pauseOwnedByInput) // 직접 정지 확인
                { // 조건 시작
                    manager.ResumeGame(); // 게임 재개
                    pauseOwnedByInput = false; // 정지 기록 제거
                    RunFrontendController.Instance?.HideManualPause(); // 정지 화면 닫기
                } // 조건 종료
                return; // 같은 프레임 중복 입력 방지
            } // 조건 종료
            if (manager != null && manager.IsPlaying && keyboard.eKey.wasPressedThisFrame) // 진행 중 합성 입력
            { // 조건 시작
                OpenMergePanel(); // 첫 유효 쌍 즉시 합성
            } // 조건 종료
        } // 함수 종료

        public bool HasMergeCandidates() // 합성 가능 쌍 확인
        { // 함수 시작
            return TryFindPair(out _, out _); // 첫 유효 쌍 검색
        } // 함수 종료

        public bool OpenMergePanel() // 기존 호출 경로의 즉시 합성
        { // 함수 시작
            GameManager manager = GameManager.Instance; // 현재 게임 상태
            if (!isActiveAndEnabled || manager == null || !manager.IsPlaying || IsLevelUpBlocking() || !TryFindPair(out int first, out int second)) // 실행 조건과 재료 확인
            { // 조건 시작
                return false; // 합성 불가
            } // 조건 종료
            bool merged = weaponManager.TryMergeSlots(first, second, WeaponMergeUpgrade.None); // 선택지 없이 한 쌍 합성
            if (merged) // 실제 성공 확인
            { // 조건 시작
                MergeClosed?.Invoke(true); // 보상 처리에 완료 알림
            } // 조건 종료
            return merged; // 실제 합성 결과
        } // 함수 종료

        public bool OpenRewardMerge(WeaponRewardCandidate reward) // 보상 포탑 즉시 합성
        { // 함수 시작
            GameManager manager = GameManager.Instance; // 현재 게임 상태
            if (!isActiveAndEnabled || weaponManager == null || levelUpController == null || !levelUpController.IsAwaitingRewardMerge || manager == null || !manager.IsPlaying || !reward.IsValid) // 보상 합성 조건
            { // 조건 시작
                return false; // 잘못된 요청 거절
            } // 조건 종료
            IReadOnlyList<SnakeWeaponSlot> slots = weaponManager.Slots; // 장착 슬롯 조회
            for (int index = 0; index < slots.Count; index++) // 몸통 순서 검색
            { // 반복 시작
                SnakeWeaponSlot slot = slots[index]; // 현재 재료
                if (!CanMergeSlot(slot) || slot.Weapon != reward.Weapon || slot.Grade != reward.Grade) // 같은 포탑과 등급 확인
                { // 조건 시작
                    continue; // 다음 슬롯 검사
                } // 조건 종료
                bool merged = weaponManager.TryMergeReward(index, reward.Weapon, reward.Grade, true, WeaponMergeUpgrade.None); // 장착 재료 유지 및 즉시 합성
                if (merged) // 실제 성공 확인
                { // 조건 시작
                    MergeClosed?.Invoke(true); // 레벨업 완료 알림
                } // 조건 종료
                return merged; // 실제 합성 결과
            } // 반복 종료
            return false; // 유효 재료 부재
        } // 함수 종료

        public void CloseMergePanel(bool merged = false) // 기존 화면 정리 호환
        { // 함수 시작
            mergePanel?.Hide(); // 남은 선택 화면 숨김
        } // 함수 종료

        private bool IsLevelUpBlocking() // 레벨업 입력 우선 확인
        { // 함수 시작
            return (experience != null && experience.IsLevelUpPending) || (levelUpController != null && levelUpController.IsPresentingLevelUp); // 보상 선택 또는 대기 여부
        } // 함수 종료

        private bool TryFindPair(out int first, out int second) // 몸통 순서의 첫 유효 쌍 검색
        { // 함수 시작
            first = -1; // 첫 재료 기본값
            second = -1; // 둘째 재료 기본값
            if (weaponManager == null) // 포탑 관리자 확인
            { // 조건 시작
                return false; // 검색 불가
            } // 조건 종료
            IReadOnlyList<SnakeWeaponSlot> slots = weaponManager.Slots; // 전체 슬롯 조회
            for (int left = 0; left < slots.Count; left++) // 앞쪽 재료 순회
            { // 반복 시작
                if (!CanMergeSlot(slots[left])) // 첫 재료 유효성
                { // 조건 시작
                    continue; // 다음 첫 재료 검색
                } // 조건 종료
                for (int right = left + 1; right < slots.Count; right++) // 뒤쪽 재료 순회
                { // 반복 시작
                    if (CanMergeSlot(slots[right]) && slots[left].Weapon == slots[right].Weapon && slots[left].Grade == slots[right].Grade) // 같은 포탑과 등급 확인
                    { // 조건 시작
                        first = left; // 첫 재료 위치
                        second = right; // 둘째 재료 위치
                        return true; // 유효 쌍 발견
                    } // 조건 종료
                } // 반복 종료
            } // 반복 종료
            return false; // 유효 쌍 부재
        } // 함수 종료

        private static bool CanMergeSlot(SnakeWeaponSlot slot) // 재료 합성 가능 확인
        { // 함수 시작
            return slot != null && !slot.IsEmpty && slot.Weapon != null && slot.Grade >= 1 && slot.Grade < 5 && slot.Grade < slot.Weapon.MaxGrade; // 무기와 최대 등급 확인
        } // 함수 종료
    } // 제어기 종료
} // 영역 종료
