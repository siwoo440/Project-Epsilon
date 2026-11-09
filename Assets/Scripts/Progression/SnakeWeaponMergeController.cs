using System; // 합성 결과 이벤트
using System.Collections.Generic;
using ProjectEpsilon.Combat;
using ProjectEpsilon.Core;
using ProjectEpsilon.Data;
using ProjectEpsilon.Player;
using ProjectEpsilon.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectEpsilon.Progression
{
    [DefaultExecutionOrder(150)]
    public sealed class SnakeWeaponMergeController : MonoBehaviour
    {
        [SerializeField] private SnakeWeaponManager weaponManager;
        [SerializeField] private SnakeMovement movement;
        [SerializeField] private SnakeStamina stamina;
        [SerializeField] private WeaponMergePanelController mergePanel;

        [Header("UI Priority")]
        [SerializeField] private SnakeExperience experience;
        [SerializeField] private SnakeLevelUpController levelUpController;
        [SerializeField] private SettingsMenuController settingsMenu; // 설정 화면 참조

        [Range(0.1f, 1f)]
        [SerializeField] private float mergeMoveMultiplier = 0.7f;

        [Range(1, 3)]
        [SerializeField] private int maximumDisplayedCandidates = 3;

        private readonly List<WeaponMergeCandidate> currentCandidates =
            new List<WeaponMergeCandidate>();

        private bool subscribed;
        private bool mergeOpen;
        private bool pauseOwnedByInput; // 직접 일시정지 상태
        private WeaponMergeCandidate selectedMerge; // 미확정 합성 대상
        private readonly List<int> selectableSlots = new List<int>(); // 선택 가능 슬롯
        private int selectionStage; // 재료 및 강화 단계
        private int selectionCursor; // 현재 슬롯 위치
        private int firstMaterialIndex; // 첫 재료 위치
        private int secondMaterialIndex; // 둘째 재료 위치
        private bool inheritSecond; // 둘째 강화 계승 여부
        private bool mergeCommitting; // 합성 확정 진행 여부
        private bool rewardMergeOpen; // 미장착 보상 합성 여부
        private int openedFrame = -1; // 합성 화면 진입 프레임
        private WeaponRewardCandidate pendingReward; // 합성할 미장착 보상

        public void ClearManualPause() // 실제 메뉴 재개 상태 정리
        { // 메서드 시작
            pauseOwnedByInput = false; // 직접 정지 소유 해제
        } // 메서드 끝
        public bool IsMergeOpen => mergeOpen;
        public event Action<bool> MergeClosed; // 합성 완료 또는 취소 알림

        public bool HasMergeCandidates() // 합성 가능한 쌍 확인
        { // 메서드 시작
            if (weaponManager == null || mergePanel == null) // 구성 확인
            { // 조건 시작
                return false; // 합성 불가
            } // 조건 끝
            IReadOnlyList<SnakeWeaponSlot> slots = weaponManager.Slots; // 현재 무기 슬롯
            for (int first = 0; first < slots.Count; first++) // 첫 무기 순회
            { // 반복 시작
                for (int second = first + 1; second < slots.Count; second++) // 둘째 무기 순회
                { // 반복 시작
                    if (CanMergePair(slots[first], slots[second])) // 같은 무기와 등급 확인
                    { // 조건 시작
                        return true; // 합성 가능
                    } // 조건 끝
                } // 반복 끝
            } // 반복 끝
            return false; // 합성 쌍 부재
        } // 메서드 끝

        public IReadOnlyList<WeaponMergeCandidate>
            CurrentCandidates =>
                currentCandidates;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Subscribe();
            }
        }

        private void Start()
        {
            Subscribe();

            if (mergePanel != null)
            {
                mergePanel.Hide();
            }

            ApplyMergeMovementState(
                false
            );
        }

        private void OnDisable()
        {
            Unsubscribe();

            bool wasOpen = mergeOpen; // 열려 있던 합성 상태
            mergeOpen = false;
            rewardMergeOpen = false; // 보상 합성 상태 해제
            currentCandidates.Clear();
            if (mergePanel != null) // 합성 화면 객체 확인
            { // 조건 시작
                mergePanel.Hide(); // 합성 화면 숨김
            } // 조건 끝

            ApplyMergeMovementState(
                false
            );
            if (wasOpen) // 진행 중 합성 확인
            { // 조건 시작
                MergeClosed?.Invoke(false); // 종료로 인한 취소 알림
            } // 조건 끝
        }

        private void Update() // 합성 및 일시정지 입력 처리
        { // 메서드 시작
            if (IsLevelUpBlocking()) // 레벨업 선택 우선
            { // 조건 시작
                if (mergeOpen) // 열린 합성 확인
                { // 조건 시작
                    CloseMergePanel(); // 합성 취소
                } // 조건 끝
                return; // 레벨업 중 입력 차단
            } // 조건 끝
            if (levelUpController != null && levelUpController.ConsumedEscapeThisFrame) // 레벨업 취소 입력 소비 확인
            { // 조건 시작
                return; // 같은 ESC로 일시정지 방지
            } // 조건 끝
            Keyboard keyboard = Keyboard.current; // 현재 키보드 조회
            if (keyboard == null) // 키보드 부재 확인
            { // 조건 시작
                return; // 입력 처리 종료
            } // 조건 끝
            GameManager manager = GameManager.Instance; // 게임 관리자 조회
            if (InputBindingManager.Instance != null && InputBindingManager.Instance.IsRebinding) // 키 재설정 중 확인
            { // 조건 시작
                return; // 재설정 입력 우선
            } // 조건 끝
            if (settingsMenu == null) // 설정 화면 연결 확인
            { // 조건 시작
                settingsMenu = FindFirstObjectByType<SettingsMenuController>(); // 설정 화면 검색
            } // 조건 끝
            if (settingsMenu != null && settingsMenu.IsOpen) // 설정 화면 표시 확인
            { // 조건 시작
                return; // 설정 중 게임 입력 차단
            } // 조건 끝
            if (RunFrontendController.Instance != null && RunFrontendController.Instance.Stage == FrontendStage.Settings) // 실제 전용 설정 화면 확인
            { // 조건 시작
                return; // 설정 중 ESC와 게임 입력 차단
            } // 조건 끝
            if (mergeOpen && Time.frameCount == openedFrame) // 화면 진입 프레임 확인
            { // 조건 시작
                return; // 진입 키 중복 선택 방지
            } // 조건 끝
            if (keyboard.escapeKey.wasPressedThisFrame) // ESC 입력 확인
            { // 조건 시작
                if (mergeOpen) // 합성 우선 확인
                { // 조건 시작
                    CloseMergePanel(); // 합성 취소
                    return; // 일시정지 방지
                } // 조건 끝
                if (manager != null && manager.IsPlaying) // 플레이 상태 확인
                { // 조건 시작
                    manager.PauseGame(); // 게임 일시정지
                    pauseOwnedByInput = true; // 직접 일시정지 기록
                    RunFrontendController.Instance?.ShowManualPause(); // 실제 일시정지 화면 표시
                } // 조건 끝
                else if (manager != null && manager.IsPaused && pauseOwnedByInput) // 직접 정지 상태 확인
                { // 조건 시작
                    manager.ResumeGame(); // 게임 재개
                    pauseOwnedByInput = false; // 직접 일시정지 해제
                    RunFrontendController.Instance?.HideManualPause(); // 실제 일시정지 화면 닫기
                } // 조건 끝
                return; // 중복 입력 방지
            } // 조건 끝
            if (manager != null && !manager.IsPlaying) // 플레이 중 여부 확인
            { // 조건 시작
                if (mergeOpen) // 미확정 합성 확인
                { // 조건 시작
                    CloseMergePanel(); // 플레이 종료 시 합성 취소
                } // 조건 끝
                return; // 정지 중 합성 차단
            } // 조건 끝
            if (keyboard.eKey.wasPressedThisFrame) // 합성 키 입력 확인
            { // 조건 시작
                if (mergeOpen) // 열린 합성 확인
                { // 조건 시작
                    CloseMergePanel(); // 합성 취소
                } // 조건 끝
                else // 닫힌 합성 확인
                { // 대안 시작
                    OpenMergePanel(); // 합성 화면 열기
                } // 대안 끝
                return; // 같은 프레임 후보 선택 방지
            } // 조건 끝
            if (!mergeOpen) // 합성 화면 상태 확인
            { // 조건 시작
                return; // 합성 선택 차단
            } // 조건 끝
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) // 첫 선택 키 확인
            { // 조건 시작
                HandleCandidateSelected(0); // 첫 합성 후보 선택
            } // 조건 끝
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) // 둘째 선택 키 확인
            { // 조건 시작
                HandleCandidateSelected(1); // 둘째 합성 후보 선택
            } // 조건 끝
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) // 셋째 선택 키 확인
            { // 조건 시작
                HandleCandidateSelected(2); // 셋째 합성 후보 선택
            } // 조건 끝
        } // 메서드 끝

        public void Configure(
            SnakeWeaponManager manager,
            SnakeMovement movementController,
            SnakeStamina staminaController,
            WeaponMergePanelController panel,
            SnakeExperience experienceSource,
            SnakeLevelUpController levelController,
            float moveMultiplier
        )
        {
            Unsubscribe();

            weaponManager = manager;
            movement = movementController;
            stamina = staminaController;
            mergePanel = panel;
            experience = experienceSource;
            levelUpController = levelController;

            mergeMoveMultiplier =
                Mathf.Clamp(
                    moveMultiplier,
                    0.1f,
                    1f
                );

            if (Application.isPlaying)
            {
                Subscribe();
            }
        }

        public bool OpenMergePanel()
        {
            if (IsLevelUpBlocking())
            {
                return false;
            }

            BuildMergeCandidates();

            if (currentCandidates.Count <= 0 ||
                mergePanel == null)
            {
                return false;
            }

            mergeOpen = true;
            openedFrame = Time.frameCount; // 합성 화면 진입 기록
            rewardMergeOpen = false; // 일반 장착 무기 합성
            selectionStage = 0; // 첫 재료 단계
            selectionCursor = 0; // 첫 슬롯 선택
            RebuildSelectableSlots(); // 첫 재료 목록 구성

            ApplyMergeMovementState(
                true
            );

            ShowSelectedSlot(); // 첫 재료 화면 표시

            return true;
        }

        public bool OpenRewardMerge(WeaponRewardCandidate reward) // 미장착 보상 합성 열기
        { // 메서드 시작
            GameManager manager = GameManager.Instance; // 현재 게임 상태
            if (!isActiveAndEnabled || mergeOpen || weaponManager == null || mergePanel == null || levelUpController == null || !levelUpController.IsAwaitingRewardMerge || manager == null || !manager.IsPlaying || !reward.IsValid) // 보상 합성 진입 조건
            { // 조건 시작
                return false; // 진입 거절
            } // 조건 끝
            pendingReward = reward; // 미장착 보상 저장
            rewardMergeOpen = true; // 보상 합성 모드
            selectionStage = 1; // 장착 재료 선택 단계
            selectionCursor = 0; // 첫 후보 선택
            RebuildSelectableSlots(); // 호환 장착 무기 검색
            if (selectableSlots.Count == 0) // 호환 재료 부재 확인
            { // 조건 시작
                rewardMergeOpen = false; // 보상 합성 취소
                return false; // 진입 실패
            } // 조건 끝
            mergeOpen = true; // 합성 화면 열기
            openedFrame = Time.frameCount; // 합성 화면 진입 기록
            ApplyMergeMovementState(true); // 직진 및 감속 적용
            ShowSelectedSlot(); // 장착 재료 표시
            return true; // 진입 성공
        } // 메서드 끝

        public void CloseMergePanel(bool merged = false) // 합성 화면 종료
        {
            bool wasOpen = mergeOpen; // 종료 전 화면 상태
            mergeOpen = false;
            rewardMergeOpen = false; // 보상 합성 상태 해제
            currentCandidates.Clear();

            if (mergePanel != null)
            {
                mergePanel.Hide();
            }

            ApplyMergeMovementState(
                false
            );
            if (wasOpen) // 실제 화면 종료 확인
            { // 조건 시작
                MergeClosed?.Invoke(merged); // 합성 결과 알림
            } // 조건 끝
        }

        private void HandleCandidateSelected(int candidateIndex) // 재료 및 강화 선택
        { // 메서드 시작
            if (!mergeOpen || weaponManager == null || (GameManager.Instance != null && !GameManager.Instance.IsPlaying)) // 선택 가능 상태 확인
            { // 조건 시작
                return; // 선택 무시
            } // 조건 끝
            if (selectionStage < 2) // 재료 선택 단계 확인
            { // 조건 시작
                if (!RefreshSelectedSlots()) // 재료 목록 유효성 확인
                { // 조건 시작
                    CloseMergePanel(); // 재료 변경 시 취소
                    return; // 선택 종료
                } // 조건 끝
                if (candidateIndex == 0 || candidateIndex == 2) // 이전 및 다음 선택
                { // 조건 시작
                    selectionCursor = (selectionCursor + (candidateIndex == 0 ? selectableSlots.Count - 1 : 1)) % selectableSlots.Count; // 선택 위치 순환
                    ShowSelectedSlot(); // 선택 위치 표시
                    return; // 재료 확정 대기
                } // 조건 끝
                if (candidateIndex != 1) // 확정 버튼 확인
                { // 조건 시작
                    return; // 잘못된 선택 무시
                } // 조건 끝
                if (selectionStage == 0) // 첫 재료 단계 확인
                { // 조건 시작
                    firstMaterialIndex = selectableSlots[selectionCursor]; // 첫 재료 저장
                    selectionStage = 1; // 둘째 재료 단계
                    selectionCursor = 0; // 둘째 선택 초기화
                    RebuildSelectableSlots(); // 둘째 후보 구성
                    ShowSelectedSlot(); // 둘째 재료 화면
                    return; // 다음 선택 대기
                } // 조건 끝
                secondMaterialIndex = selectableSlots[selectionCursor]; // 둘째 재료 저장
                selectionStage = 2; // 계승 선택 단계
                ShowInheritanceSelection(); // 강화 내역 표시
                return; // 계승 선택 대기
            } // 조건 끝
            if (!MaterialsStillValid()) // 선택 재료 유효성 확인
            { // 조건 시작
                CloseMergePanel(); // 변경된 재료 취소
                return; // 선택 종료
            } // 조건 끝
            if (selectionStage == 2) // 강화 계승 단계 확인
            { // 조건 시작
                if (candidateIndex < 0 || candidateIndex > 1) // 계승 대상 범위 확인
                { // 조건 시작
                    return; // 잘못된 선택 무시
                } // 조건 끝
                inheritSecond = candidateIndex == 1; // 선택 강화 기록
                SnakeWeaponSlot equipped = weaponManager.Slots[rewardMergeOpen ? secondMaterialIndex : firstMaterialIndex]; // 기준 장착 무기 확인
                selectionStage = 3; // 공통 강화 단계
                if (rewardMergeOpen) // 미장착 보상 합성 확인
                { // 조건 시작
                    mergePanel.ShowRewardUpgradeSelection(pendingReward.Weapon); // 보상 무기 강화 화면
                } // 조건 끝
                else // 일반 무기 합성 확인
                { // 대안 시작
                    selectedMerge = new WeaponMergeCandidate(equipped.Weapon, equipped.Grade, firstMaterialIndex, secondMaterialIndex); // 선택 재료 보관
                    mergePanel.ShowUpgradeSelection(selectedMerge); // 공통 강화 화면
                } // 대안 끝
                ShowSynergyPreview(equipped); // 속성 변화 미리보기
                return; // 강화 확정 대기
            } // 조건 끝
            if (candidateIndex < 0 || candidateIndex > 2) // 강화 후보 범위 확인
            { // 조건 시작
                return; // 잘못된 선택 무시
            } // 조건 끝
            int kept = inheritSecond ? secondMaterialIndex : firstMaterialIndex; // 계승 재료 위치
            int consumed = inheritSecond ? firstMaterialIndex : secondMaterialIndex; // 소모 재료 위치
            WeaponMergeUpgrade upgrade = (WeaponMergeUpgrade)(candidateIndex + 1); // 선택 강화 변환
            bool merged; // 합성 성공 여부
            mergeCommitting = true; // 확정 중 슬롯 변경 허용
            try // 합성 시도
            { // 시도 시작
                merged = rewardMergeOpen // 합성 방식 확인
                    ? weaponManager.TryMergeReward(secondMaterialIndex, pendingReward.Weapon, pendingReward.Grade, inheritSecond, upgrade) // 미장착 보상 합성
                    : weaponManager.TryMergeSlots(kept, consumed, upgrade); // 기존 장착 무기 합성
            } // 시도 끝
            finally // 확정 상태 정리
            { // 정리 시작
                mergeCommitting = false; // 외부 변경 감시 복구
            } // 정리 끝
            CloseMergePanel(merged); // 결과에 따른 화면 종료
        } // 메서드 끝

        private void RebuildSelectableSlots() // 선택 가능한 재료 갱신
        { // 메서드 시작
            selectableSlots.Clear(); // 기존 후보 제거
            IReadOnlyList<SnakeWeaponSlot> slots = weaponManager.Slots; // 전체 슬롯 조회
            for (int index = 0; index < slots.Count; index++) // 재료 슬롯 순회
            { // 반복 시작
                if (rewardMergeOpen) // 미장착 보상 합성 확인
                { // 조건 시작
                    SnakeWeaponSlot candidate = slots[index]; // 장착 재료 조회
                    if (CanMergeSlot(candidate) && candidate.Weapon == pendingReward.Weapon && candidate.Grade == pendingReward.Grade) // 같은 보상 무기 및 등급 확인
                    { // 조건 시작
                        selectableSlots.Add(index); // 장착 재료 후보 추가
                    } // 조건 끝
                    continue; // 다음 슬롯 확인
                } // 조건 끝
                if (selectionStage == 1) // 둘째 재료 단계 확인
                { // 조건 시작
                    if (index != firstMaterialIndex && firstMaterialIndex < slots.Count && CanMergePair(slots[firstMaterialIndex], slots[index])) // 호환 재료 확인
                    { // 조건 시작
                        selectableSlots.Add(index); // 둘째 후보 추가
                    } // 조건 끝
                    continue; // 다음 슬롯 확인
                } // 조건 끝
                for (int other = 0; other < slots.Count; other++) // 짝 슬롯 순회
                { // 반복 시작
                    if (other != index && CanMergePair(slots[index], slots[other])) // 합성 가능 여부 확인
                    { // 조건 시작
                        selectableSlots.Add(index); // 첫 재료 후보 추가
                        break; // 중복 추가 방지
                    } // 조건 끝
                } // 반복 끝
            } // 반복 끝
        } // 메서드 끝

        private bool RefreshSelectedSlots() // 슬롯 변경 확인
        { // 메서드 시작
            if (selectableSlots.Count == 0 || selectionCursor >= selectableSlots.Count) // 이전 후보 확인
            { // 조건 시작
                return false; // 빈 후보 취소
            } // 조건 끝
            int previousSlot = selectableSlots[selectionCursor]; // 현재 위치 보존
            RebuildSelectableSlots(); // 현재 후보 갱신
            selectionCursor = selectableSlots.IndexOf(previousSlot); // 이전 위치 검색
            return selectionCursor >= 0; // 기존 선택 유효 여부
        } // 메서드 끝

        private bool MaterialsStillValid() // 선택된 재료 검사
        { // 메서드 시작
            IReadOnlyList<SnakeWeaponSlot> slots = weaponManager.Slots; // 전체 슬롯 조회
            if (rewardMergeOpen) // 미장착 보상 합성 확인
            { // 조건 시작
                return secondMaterialIndex >= 0 && secondMaterialIndex < slots.Count && CanMergeSlot(slots[secondMaterialIndex]) && slots[secondMaterialIndex].Weapon == pendingReward.Weapon && slots[secondMaterialIndex].Grade == pendingReward.Grade; // 장착 재료 유효성
            } // 조건 끝
            return firstMaterialIndex >= 0 && secondMaterialIndex >= 0 && firstMaterialIndex < slots.Count && secondMaterialIndex < slots.Count && firstMaterialIndex != secondMaterialIndex && CanMergePair(slots[firstMaterialIndex], slots[secondMaterialIndex]); // 재료 호환 확인
        } // 메서드 끝

        private void ShowSelectedSlot() // 재료 화면 표시
        { // 메서드 시작
            if (selectableSlots.Count > 0 && selectionCursor >= 0 && selectionCursor < selectableSlots.Count) // 선택 위치 확인
            { // 조건 시작
                int index = selectableSlots[selectionCursor]; // 선택된 슬롯
                mergePanel.ShowSlotSelection(weaponManager.Slots[index], index, selectionCursor, selectableSlots.Count, selectionStage == 0); // 슬롯 화면 표시
                ShowSynergyPreview(weaponManager.Slots[index]); // 속성 변화 미리보기
            } // 조건 끝
        } // 메서드 끝

        private void ShowInheritanceSelection() // 강화 계승 화면 표시
        { // 메서드 시작
            if (rewardMergeOpen) // 미장착 보상 합성 확인
            { // 조건 시작
                mergePanel.ShowRewardInheritanceSelection(pendingReward.Weapon, pendingReward.Grade, weaponManager.Slots[secondMaterialIndex], secondMaterialIndex); // 보상 및 장착 강화 표시
                ShowSynergyPreview(weaponManager.Slots[secondMaterialIndex]); // 보상 합성 시너지 표시
                return; // 일반 합성 표시 방지
            } // 조건 끝
            mergePanel.ShowInheritanceSelection(weaponManager.Slots[firstMaterialIndex], firstMaterialIndex, weaponManager.Slots[secondMaterialIndex], secondMaterialIndex); // 양쪽 강화 표시
            ShowSynergyPreview(weaponManager.Slots[firstMaterialIndex]); // 속성 변화 미리보기
        } // 메서드 끝

        private void ShowSynergyPreview(SnakeWeaponSlot selected) // 속성 변화 계산
        { // 메서드 시작
            WeaponAttribute attribute = selected.Weapon.Attribute; // 선택 무기 속성
            int count = 0; // 현재 속성 무기 수
            IReadOnlyList<SnakeWeaponSlot> slots = weaponManager.Slots; // 현재 무기 슬롯
            for (int index = 0; index < slots.Count; index++) // 슬롯 순회
            { // 반복 시작
                SnakeWeaponSlot slot = slots[index]; // 현재 슬롯 조회
                if (slot != null && !slot.IsEmpty && slot.Weapon.Attribute == attribute) // 동일 속성 확인
                { // 조건 시작
                    count++; // 속성 무기 집계
                } // 조건 끝
            } // 반복 끝
            int resultCount = rewardMergeOpen ? count : Mathf.Max(0, count - 1); // 합성 후 무기 수
            int stage = WeaponAttributeSynergyRules.ResolveStage(count); // 현재 시너지 단계
            int resultStage = WeaponAttributeSynergyRules.ResolveStage(resultCount); // 합성 후 시너지 단계
            mergePanel.ShowSynergyPreview(attribute, count, resultCount, stage, resultStage); // 단계 변화 표시
        } // 메서드 끝

        private void HandleSlotsChanged() // 외부 슬롯 변경 처리
        { // 메서드 시작
            if (mergeOpen && !mergeCommitting) // 합성 화면 및 외부 변경 확인
            { // 조건 시작
                CloseMergePanel(); // 바뀐 재료 선택 취소
            } // 조건 끝
        } // 메서드 끝

        private void HandleCloseRequested()
        {
            CloseMergePanel();
        }

        private void HandleLevelUpRequested(
            int level
        )
        {
            if (mergeOpen)
            {
                CloseMergePanel();
            }
        }

        private bool IsLevelUpBlocking()
        {
            if (rewardMergeOpen && levelUpController != null && levelUpController.IsAwaitingRewardMerge) // 레벨업 보상 합성 허용
            { // 조건 시작
                return false; // 보상 합성 진행
            } // 조건 끝
            return
                (experience != null &&
                experience.IsLevelUpPending) ||
                (levelUpController != null &&
                levelUpController.IsPresentingLevelUp);
        }

        private void BuildMergeCandidates()
        {
            currentCandidates.Clear();

            if (weaponManager == null)
            {
                return;
            }

            IReadOnlyList<SnakeWeaponSlot> slots =
                weaponManager.Slots;

            int candidateLimit =
                Mathf.Clamp(
                    maximumDisplayedCandidates,
                    1,
                    3
                );

            for (int firstIndex = 0;
                firstIndex < slots.Count &&
                currentCandidates.Count <
                    candidateLimit;
                firstIndex++)
            {
                SnakeWeaponSlot first =
                    slots[firstIndex];

                if (!CanMergeSlot(first) ||
                    ContainsWeaponGrade(
                        first.Weapon,
                        first.Grade
                    ))
                {
                    continue;
                }

                for (int secondIndex =
                    firstIndex + 1;
                    secondIndex < slots.Count;
                    secondIndex++)
                {
                    SnakeWeaponSlot second =
                        slots[secondIndex];

                    if (!CanMergePair(
                        first,
                        second
                    ))
                    {
                        continue;
                    }

                    currentCandidates.Add(
                        new WeaponMergeCandidate(
                            first.Weapon,
                            first.Grade,
                            firstIndex,
                            secondIndex
                        )
                    );

                    break;
                }
            }
        }

        private bool ContainsWeaponGrade(
            WeaponData weapon,
            int grade
        )
        {
            for (int index = 0;
                index < currentCandidates.Count;
                index++)
            {
                WeaponMergeCandidate candidate =
                    currentCandidates[index];

                if (candidate.Weapon ==
                    weapon &&
                    candidate.CurrentGrade ==
                    grade)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CanMergeSlot(
            SnakeWeaponSlot slot
        )
        {
            return
                slot != null &&
                !slot.IsEmpty &&
                slot.Weapon != null &&
                slot.Grade >= 1 &&
                slot.Grade < 5 &&
                slot.Grade <
                    slot.Weapon.MaxGrade;
        }

        private static bool CanMergePair(
            SnakeWeaponSlot first,
            SnakeWeaponSlot second
        )
        {
            return
                CanMergeSlot(first) &&
                CanMergeSlot(second) &&
                first.Weapon ==
                    second.Weapon &&
                first.Grade ==
                    second.Grade;
        }

        private void ApplyMergeMovementState(
            bool active
        )
        {
            GameManager manager = GameManager.Instance; // 게임 관리자 조회
            if (manager != null) // 관리자 연결 확인
            { // 조건 시작
                manager.SetGameplayTimeScale(active ? mergeMoveMultiplier : 1f); // 전체 게임 속도 적용
            } // 조건 끝
            if (movement != null)
            {
                movement.SetMergeMovementMode(
                    active,
                    mergeMoveMultiplier
                );
            }

            if (stamina != null)
            {
                stamina.SetBoostBlocked(
                    active
                );
            }
        }

        private void Subscribe()
        {
            if (subscribed ||
                mergePanel == null)
            {
                return;
            }

            if (weaponManager != null) // 무기 관리자 확인
            { // 조건 시작
                weaponManager.SlotsChanged += HandleSlotsChanged; // 외부 슬롯 변경 감시
            } // 조건 끝

            mergePanel.CandidateSelected +=
                HandleCandidateSelected;

            mergePanel.CloseRequested +=
                HandleCloseRequested;

            if (experience != null)
            {
                experience.LevelUpRequested +=
                    HandleLevelUpRequested;
            }

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (weaponManager != null) // 무기 관리자 확인
            { // 조건 시작
                weaponManager.SlotsChanged -= HandleSlotsChanged; // 슬롯 변경 감시 해제
            } // 조건 끝

            if (mergePanel != null)
            {
                mergePanel.CandidateSelected -=
                    HandleCandidateSelected;

                mergePanel.CloseRequested -=
                    HandleCloseRequested;
            }

            if (experience != null)
            {
                experience.LevelUpRequested -=
                    HandleLevelUpRequested;
            }

            subscribed = false;
        }
    }
}
