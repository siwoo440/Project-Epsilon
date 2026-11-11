using System; // 합성 결과 이벤트
using System.Collections.Generic; // C# 기본 기능 연결
using ProjectEpsilon.Combat; // 전투 기능 연결
using ProjectEpsilon.Core; // 대상 기능 연결
using ProjectEpsilon.Data; // 데이터 기능 연결
using ProjectEpsilon.Player; // 플레이어 기능 연결
using ProjectEpsilon.UI; // UI 기능 연결
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.InputSystem; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{
    [DefaultExecutionOrder(150)] // 대상 설정
    public sealed class SnakeWeaponMergeController : MonoBehaviour // 포탑·합성 자료 구조
    {
        [SerializeField] private SnakeWeaponManager weaponManager; // 필드·포탑 설정
        [SerializeField] private SnakeMovement movement; // 필드·이동 설정
        [SerializeField] private SnakeStamina stamina; // 필드·스태미나 설정
        [SerializeField] private WeaponMergePanelController mergePanel; // 필드·포탑 설정

        [Header("UI Priority")] // UI 설정
        [SerializeField] private SnakeExperience experience; // 필드·경험치 설정
        [SerializeField] private SnakeLevelUpController levelUpController; // 필드·레벨 설정
        [SerializeField] private SettingsMenuController settingsMenu; // 설정 화면 참조

        [Range(0.1f, 1f)] // 범위 설정
        [SerializeField] private float mergeMoveMultiplier = 0.7f; // 필드·합성 설정

        [Range(1, 3)] // 범위 설정
        [SerializeField] private int maximumDisplayedCandidates = 3; // 필드·최대 설정

        private readonly List<WeaponMergeCandidate> currentCandidates = // 목록·포탑 구성 선언
            new List<WeaponMergeCandidate>(); // 목록·포탑 객체 생성

        private bool subscribed; // 대상 구성 선언
        private bool mergeOpen; // 합성·열기 구성 선언
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
        {
            pauseOwnedByInput = false; // 직접 정지 소유 해제
        }
        public bool IsMergeOpen => mergeOpen; // 합성·열기 값 반환
        public event Action<bool> MergeClosed; // 합성 완료 또는 취소 알림

        public bool HasMergeCandidates() // 합성 가능한 쌍 확인
        {
            if (weaponManager == null || mergePanel == null) // 구성 확인
            {
                return false; // 합성 불가
            }
            IReadOnlyList<SnakeWeaponSlot> slots = weaponManager.Slots; // 현재 무기 슬롯
            for (int first = 0; first < slots.Count; first++) // 첫 무기 순회
            {
                for (int second = first + 1; second < slots.Count; second++) // 둘째 무기 순회
                {
                    if (CanMergePair(slots[first], slots[second])) // 같은 무기와 등급 확인
                    {
                        return true; // 합성 가능
                    }
                }
            }
            return false; // 합성 쌍 부재
        }

        public IReadOnlyList<WeaponMergeCandidate> // 목록·포탑 구성 선언
            CurrentCandidates => // 현재·후보 처리
                currentCandidates; // 현재·후보 처리

        private void OnEnable() // 활성 기능 선언
        {
            if (Application.isPlaying) // 대상 조건 확인
            {
                Subscribe(); // 구독 기능 실행
            }
        }

        private void Start() // 시작 기능 선언
        {
            Subscribe(); // 구독 기능 실행

            if (mergePanel != null) // 합성·화면 조건 확인
            {
                mergePanel.Hide(); // 합성·화면 기능 실행
            }

            ApplyMergeMovementState( // 적용·합성 호출 시작
                false // 대상 처리
            ); // 앞선 항목 종료
        }

        private void OnDisable() // 비활성 기능 선언
        {
            Unsubscribe(); // 구독 해제 기능 실행

            bool wasOpen = mergeOpen; // 열려 있던 합성 상태
            mergeOpen = false; // 합성·열기 값 설정
            rewardMergeOpen = false; // 보상 합성 상태 해제
            currentCandidates.Clear(); // 현재·후보 기능 실행
            if (mergePanel != null) // 합성 화면 객체 확인
            {
                mergePanel.Hide(); // 합성 화면 숨김
            }

            ApplyMergeMovementState( // 적용·합성 호출 시작
                false // 대상 처리
            ); // 앞선 항목 종료
            if (wasOpen) // 진행 중 합성 확인
            {
                MergeClosed?.Invoke(false); // 종료로 인한 취소 알림
            }
        }

        private void Update() // 합성 및 일시정지 입력 처리
        {
            if (IsLevelUpBlocking()) // 레벨업 선택 우선
            {
                if (mergeOpen) // 열린 합성 확인
                {
                    CloseMergePanel(); // 합성 취소
                }
                return; // 레벨업 중 입력 차단
            }
            if (levelUpController != null && levelUpController.ConsumedEscapeThisFrame) // 레벨업 취소 입력 소비 확인
            {
                return; // 같은 ESC로 일시정지 방지
            }
            Keyboard keyboard = Keyboard.current; // 현재 키보드 조회
            if (keyboard == null) // 키보드 부재 확인
            {
                return; // 입력 처리 종료
            }
            GameManager manager = GameManager.Instance; // 게임 관리자 조회
            if (InputBindingManager.Instance != null && InputBindingManager.Instance.IsRebinding) // 키 재설정 중 확인
            {
                return; // 재설정 입력 우선
            }
            if (settingsMenu == null) // 설정 화면 연결 확인
            {
                settingsMenu = FindFirstObjectByType<SettingsMenuController>(); // 설정 화면 검색
            }
            if (settingsMenu != null && settingsMenu.IsOpen) // 설정 화면 표시 확인
            {
                return; // 설정 중 게임 입력 차단
            }
            if (RunFrontendController.Instance != null && RunFrontendController.Instance.Stage == FrontendStage.Settings) // 실제 전용 설정 화면 확인
            {
                return; // 설정 중 ESC와 게임 입력 차단
            }
            if (mergeOpen && Time.frameCount == openedFrame) // 화면 진입 프레임 확인
            {
                return; // 진입 키 중복 선택 방지
            }
            if (keyboard.escapeKey.wasPressedThisFrame) // ESC 입력 확인
            {
                if (mergeOpen) // 합성 우선 확인
                {
                    CloseMergePanel(); // 합성 취소
                    return; // 일시정지 방지
                }
                if (manager != null && manager.IsPlaying) // 플레이 상태 확인
                {
                    manager.PauseGame(); // 게임 일시정지
                    pauseOwnedByInput = true; // 직접 일시정지 기록
                    RunFrontendController.Instance?.ShowManualPause(); // 실제 일시정지 화면 표시
                }
                else if (manager != null && manager.IsPaused && pauseOwnedByInput) // 직접 정지 상태 확인
                {
                    manager.ResumeGame(); // 게임 재개
                    pauseOwnedByInput = false; // 직접 일시정지 해제
                    RunFrontendController.Instance?.HideManualPause(); // 실제 일시정지 화면 닫기
                }
                return; // 중복 입력 방지
            }
            if (manager != null && !manager.IsPlaying) // 플레이 중 여부 확인
            {
                if (mergeOpen) // 미확정 합성 확인
                {
                    CloseMergePanel(); // 플레이 종료 시 합성 취소
                }
                return; // 정지 중 합성 차단
            }
            if (keyboard.eKey.wasPressedThisFrame) // 합성 키 입력 확인
            {
                if (mergeOpen) // 열린 합성 확인
                {
                    CloseMergePanel(); // 합성 취소
                }
                else // 닫힌 합성 확인
                {
                    OpenMergePanel(); // 합성 화면 열기
                }
                return; // 같은 프레임 후보 선택 방지
            }
            if (!mergeOpen) // 합성 화면 상태 확인
            {
                return; // 합성 선택 차단
            }
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) // 첫 선택 키 확인
            {
                HandleCandidateSelected(0); // 첫 합성 후보 선택
            }
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) // 둘째 선택 키 확인
            {
                HandleCandidateSelected(1); // 둘째 합성 후보 선택
            }
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) // 셋째 선택 키 확인
            {
                HandleCandidateSelected(2); // 셋째 합성 후보 선택
            }
        }

        public void Configure( // 구성 호출 시작
            SnakeWeaponManager manager, // 포탑·관리자 항목 전달
            SnakeMovement movementController, // 이동·제어기 항목 전달
            SnakeStamina staminaController, // 스태미나·제어기 항목 전달
            WeaponMergePanelController panel, // 포탑·합성 항목 전달
            SnakeExperience experienceSource, // 경험치·원본 항목 전달
            SnakeLevelUpController levelController, // 레벨·제어기 항목 전달
            float moveMultiplier // 배율 처리
        ) // 앞선 항목 종료
        {
            Unsubscribe(); // 구독 해제 기능 실행

            weaponManager = manager; // 포탑·관리자 값 설정
            movement = movementController; // 이동·제어기 값 설정
            stamina = staminaController; // 스태미나·제어기 값 설정
            mergePanel = panel; // 합성·화면 값 설정
            experience = experienceSource; // 경험치·원본 값 설정
            levelUpController = levelController; // 레벨·제어기 값 설정

            mergeMoveMultiplier = // 합성·배율 값 설정
                Mathf.Clamp( // 대상 호출 시작
                    moveMultiplier, // 배율 항목 전달
                    0.1f, // 대상 항목 전달
                    1f // 대상 처리
                ); // 앞선 항목 종료

            if (Application.isPlaying) // 대상 조건 확인
            {
                Subscribe(); // 구독 기능 실행
            }
        }

        public bool OpenMergePanel() // 열기·합성 기능 선언
        {
            if (IsLevelUpBlocking()) // 레벨 조건 확인
            {
                return false; // 실패 결과 반환
            }

            BuildMergeCandidates(); // 구성·합성 기능 실행

            if (currentCandidates.Count <= 0 || // 현재·후보 조건 확인
                mergePanel == null) // 합성·화면 처리
            {
                return false; // 실패 결과 반환
            }

            mergeOpen = true; // 합성·열기 값 설정
            openedFrame = Time.frameCount; // 합성 화면 진입 기록
            rewardMergeOpen = false; // 일반 장착 무기 합성
            selectionStage = 0; // 첫 재료 단계
            selectionCursor = 0; // 첫 슬롯 선택
            RebuildSelectableSlots(); // 첫 재료 목록 구성

            ApplyMergeMovementState( // 적용·합성 호출 시작
                true // 대상 처리
            ); // 앞선 항목 종료

            ShowSelectedSlot(); // 첫 재료 화면 표시

            return true; // 성공 결과 반환
        }

        public bool OpenRewardMerge(WeaponRewardCandidate reward) // 미장착 보상 합성 열기
        {
            GameManager manager = GameManager.Instance; // 현재 게임 상태
            if (!isActiveAndEnabled || mergeOpen || weaponManager == null || mergePanel == null || levelUpController == null || !levelUpController.IsAwaitingRewardMerge || manager == null || !manager.IsPlaying || !reward.IsValid) // 보상 합성 진입 조건
            {
                return false; // 진입 거절
            }
            pendingReward = reward; // 미장착 보상 저장
            rewardMergeOpen = true; // 보상 합성 모드
            selectionStage = 1; // 장착 재료 선택 단계
            selectionCursor = 0; // 첫 후보 선택
            RebuildSelectableSlots(); // 호환 장착 무기 검색
            if (selectableSlots.Count == 0) // 호환 재료 부재 확인
            {
                rewardMergeOpen = false; // 보상 합성 취소
                return false; // 진입 실패
            }
            mergeOpen = true; // 합성 화면 열기
            openedFrame = Time.frameCount; // 합성 화면 진입 기록
            ApplyMergeMovementState(true); // 직진 및 감속 적용
            ShowSelectedSlot(); // 장착 재료 표시
            return true; // 진입 성공
        }

        public void CloseMergePanel(bool merged = false) // 합성 화면 종료
        {
            bool wasOpen = mergeOpen; // 종료 전 화면 상태
            mergeOpen = false; // 합성·열기 값 설정
            rewardMergeOpen = false; // 보상 합성 상태 해제
            currentCandidates.Clear(); // 현재·후보 기능 실행

            if (mergePanel != null) // 합성·화면 조건 확인
            {
                mergePanel.Hide(); // 합성·화면 기능 실행
            }

            ApplyMergeMovementState( // 적용·합성 호출 시작
                false // 대상 처리
            ); // 앞선 항목 종료
            if (wasOpen) // 실제 화면 종료 확인
            {
                MergeClosed?.Invoke(merged); // 합성 결과 알림
            }
        }

        private void HandleCandidateSelected(int candidateIndex) // 재료 및 강화 선택
        {
            if (!mergeOpen || weaponManager == null || (GameManager.Instance != null && !GameManager.Instance.IsPlaying)) // 선택 가능 상태 확인
            {
                return; // 선택 무시
            }
            if (selectionStage < 2) // 재료 선택 단계 확인
            {
                if (!RefreshSelectedSlots()) // 재료 목록 유효성 확인
                {
                    CloseMergePanel(); // 재료 변경 시 취소
                    return; // 선택 종료
                }
                if (candidateIndex == 0 || candidateIndex == 2) // 이전 및 다음 선택
                {
                    selectionCursor = (selectionCursor + (candidateIndex == 0 ? selectableSlots.Count - 1 : 1)) % selectableSlots.Count; // 선택 위치 순환
                    ShowSelectedSlot(); // 선택 위치 표시
                    return; // 재료 확정 대기
                }
                if (candidateIndex != 1) // 확정 버튼 확인
                {
                    return; // 잘못된 선택 무시
                }
                if (selectionStage == 0) // 첫 재료 단계 확인
                {
                    firstMaterialIndex = selectableSlots[selectionCursor]; // 첫 재료 저장
                    selectionStage = 1; // 둘째 재료 단계
                    selectionCursor = 0; // 둘째 선택 초기화
                    RebuildSelectableSlots(); // 둘째 후보 구성
                    ShowSelectedSlot(); // 둘째 재료 화면
                    return; // 다음 선택 대기
                }
                secondMaterialIndex = selectableSlots[selectionCursor]; // 둘째 재료 저장
                selectionStage = 2; // 계승 선택 단계
                ShowInheritanceSelection(); // 강화 내역 표시
                return; // 계승 선택 대기
            }
            if (!MaterialsStillValid()) // 선택 재료 유효성 확인
            {
                CloseMergePanel(); // 변경된 재료 취소
                return; // 선택 종료
            }
            if (selectionStage == 2) // 강화 계승 단계 확인
            {
                if (candidateIndex < 0 || candidateIndex > 1) // 계승 대상 범위 확인
                {
                    return; // 잘못된 선택 무시
                }
                inheritSecond = candidateIndex == 1; // 선택 강화 기록
                SnakeWeaponSlot equipped = weaponManager.Slots[rewardMergeOpen ? secondMaterialIndex : firstMaterialIndex]; // 기준 장착 무기 확인
                selectionStage = 3; // 공통 강화 단계
                if (rewardMergeOpen) // 미장착 보상 합성 확인
                {
                    mergePanel.ShowRewardUpgradeSelection(pendingReward.Weapon); // 보상 무기 강화 화면
                }
                else // 일반 무기 합성 확인
                {
                    selectedMerge = new WeaponMergeCandidate(equipped.Weapon, equipped.Grade, firstMaterialIndex, secondMaterialIndex); // 선택 재료 보관
                    mergePanel.ShowUpgradeSelection(selectedMerge); // 공통 강화 화면
                }
                ShowSynergyPreview(equipped); // 속성 변화 미리보기
                return; // 강화 확정 대기
            }
            if (candidateIndex < 0 || candidateIndex > 2) // 강화 후보 범위 확인
            {
                return; // 잘못된 선택 무시
            }
            int kept = inheritSecond ? secondMaterialIndex : firstMaterialIndex; // 계승 재료 위치
            int consumed = inheritSecond ? firstMaterialIndex : secondMaterialIndex; // 소모 재료 위치
            WeaponMergeUpgrade upgrade = (WeaponMergeUpgrade)(candidateIndex + 1); // 선택 강화 변환
            bool merged; // 합성 성공 여부
            mergeCommitting = true; // 확정 중 슬롯 변경 허용
            try // 합성 시도
            {
                merged = rewardMergeOpen // 합성 방식 확인
                    ? weaponManager.TryMergeReward(secondMaterialIndex, pendingReward.Weapon, pendingReward.Grade, inheritSecond, upgrade) // 미장착 보상 합성
                    : weaponManager.TryMergeSlots(kept, consumed, upgrade); // 기존 장착 무기 합성
            }
            finally // 확정 상태 정리
            {
                mergeCommitting = false; // 외부 변경 감시 복구
            }
            CloseMergePanel(merged); // 결과에 따른 화면 종료
        }

        private void RebuildSelectableSlots() // 선택 가능한 재료 갱신
        {
            selectableSlots.Clear(); // 기존 후보 제거
            IReadOnlyList<SnakeWeaponSlot> slots = weaponManager.Slots; // 전체 슬롯 조회
            for (int index = 0; index < slots.Count; index++) // 재료 슬롯 순회
            {
                if (rewardMergeOpen) // 미장착 보상 합성 확인
                {
                    SnakeWeaponSlot candidate = slots[index]; // 장착 재료 조회
                    if (CanMergeSlot(candidate) && candidate.Weapon == pendingReward.Weapon && candidate.Grade == pendingReward.Grade) // 같은 보상 무기 및 등급 확인
                    {
                        selectableSlots.Add(index); // 장착 재료 후보 추가
                    }
                    continue; // 다음 슬롯 확인
                }
                if (selectionStage == 1) // 둘째 재료 단계 확인
                {
                    if (index != firstMaterialIndex && firstMaterialIndex < slots.Count && CanMergePair(slots[firstMaterialIndex], slots[index])) // 호환 재료 확인
                    {
                        selectableSlots.Add(index); // 둘째 후보 추가
                    }
                    continue; // 다음 슬롯 확인
                }
                for (int other = 0; other < slots.Count; other++) // 짝 슬롯 순회
                {
                    if (other != index && CanMergePair(slots[index], slots[other])) // 합성 가능 여부 확인
                    {
                        selectableSlots.Add(index); // 첫 재료 후보 추가
                        break; // 중복 추가 방지
                    }
                }
            }
        }

        private bool RefreshSelectedSlots() // 슬롯 변경 확인
        {
            if (selectableSlots.Count == 0 || selectionCursor >= selectableSlots.Count) // 이전 후보 확인
            {
                return false; // 빈 후보 취소
            }
            int previousSlot = selectableSlots[selectionCursor]; // 현재 위치 보존
            RebuildSelectableSlots(); // 현재 후보 갱신
            selectionCursor = selectableSlots.IndexOf(previousSlot); // 이전 위치 검색
            return selectionCursor >= 0; // 기존 선택 유효 여부
        }

        private bool MaterialsStillValid() // 선택된 재료 검사
        {
            IReadOnlyList<SnakeWeaponSlot> slots = weaponManager.Slots; // 전체 슬롯 조회
            if (rewardMergeOpen) // 미장착 보상 합성 확인
            {
                return secondMaterialIndex >= 0 && secondMaterialIndex < slots.Count && CanMergeSlot(slots[secondMaterialIndex]) && slots[secondMaterialIndex].Weapon == pendingReward.Weapon && slots[secondMaterialIndex].Grade == pendingReward.Grade; // 장착 재료 유효성
            }
            return firstMaterialIndex >= 0 && secondMaterialIndex >= 0 && firstMaterialIndex < slots.Count && secondMaterialIndex < slots.Count && firstMaterialIndex != secondMaterialIndex && CanMergePair(slots[firstMaterialIndex], slots[secondMaterialIndex]); // 재료 호환 확인
        }

        private void ShowSelectedSlot() // 재료 화면 표시
        {
            if (selectableSlots.Count > 0 && selectionCursor >= 0 && selectionCursor < selectableSlots.Count) // 선택 위치 확인
            {
                int index = selectableSlots[selectionCursor]; // 선택된 슬롯
                mergePanel.ShowSlotSelection(weaponManager.Slots[index], index, selectionCursor, selectableSlots.Count, selectionStage == 0); // 슬롯 화면 표시
                ShowSynergyPreview(weaponManager.Slots[index]); // 속성 변화 미리보기
            }
        }

        private void ShowInheritanceSelection() // 강화 계승 화면 표시
        {
            if (rewardMergeOpen) // 미장착 보상 합성 확인
            {
                mergePanel.ShowRewardInheritanceSelection(pendingReward.Weapon, pendingReward.Grade, weaponManager.Slots[secondMaterialIndex], secondMaterialIndex); // 보상 및 장착 강화 표시
                ShowSynergyPreview(weaponManager.Slots[secondMaterialIndex]); // 보상 합성 시너지 표시
                return; // 일반 합성 표시 방지
            }
            mergePanel.ShowInheritanceSelection(weaponManager.Slots[firstMaterialIndex], firstMaterialIndex, weaponManager.Slots[secondMaterialIndex], secondMaterialIndex); // 양쪽 강화 표시
            ShowSynergyPreview(weaponManager.Slots[firstMaterialIndex]); // 속성 변화 미리보기
        }

        private void ShowSynergyPreview(SnakeWeaponSlot selected) // 속성 변화 계산
        {
            WeaponAttribute attribute = selected.Weapon.Attribute; // 선택 무기 속성
            int count = 0; // 현재 속성 무기 수
            IReadOnlyList<SnakeWeaponSlot> slots = weaponManager.Slots; // 현재 무기 슬롯
            for (int index = 0; index < slots.Count; index++) // 슬롯 순회
            {
                SnakeWeaponSlot slot = slots[index]; // 현재 슬롯 조회
                if (slot != null && !slot.IsEmpty && slot.Weapon.Attribute == attribute) // 동일 속성 확인
                {
                    count++; // 속성 무기 집계
                }
            }
            int resultCount = rewardMergeOpen ? count : Mathf.Max(0, count - 1); // 합성 후 무기 수
            int stage = WeaponAttributeSynergyRules.ResolveStage(count); // 현재 시너지 단계
            int resultStage = WeaponAttributeSynergyRules.ResolveStage(resultCount); // 합성 후 시너지 단계
            mergePanel.ShowSynergyPreview(attribute, count, resultCount, stage, resultStage); // 단계 변화 표시
        }

        private void HandleSlotsChanged() // 외부 슬롯 변경 처리
        {
            if (mergeOpen && !mergeCommitting) // 합성 화면 및 외부 변경 확인
            {
                CloseMergePanel(); // 바뀐 재료 선택 취소
            }
        }

        private void HandleCloseRequested() // 닫기 기능 선언
        {
            CloseMergePanel(); // 닫기·합성 기능 실행
        }

        private void HandleLevelUpRequested( // 레벨 호출 시작
            int level // 레벨 처리
        ) // 앞선 항목 종료
        {
            if (mergeOpen) // 합성·열기 조건 확인
            {
                CloseMergePanel(); // 닫기·합성 기능 실행
            }
        }

        private bool IsLevelUpBlocking() // 레벨 기능 선언
        {
            if (rewardMergeOpen && levelUpController != null && levelUpController.IsAwaitingRewardMerge) // 레벨업 보상 합성 허용
            {
                return false; // 보상 합성 진행
            }
            return // 대상 결과 반환
                (experience != null && // 경험치 기능 실행
                experience.IsLevelUpPending) || // 경험치·레벨 처리
                (levelUpController != null && // 레벨·제어기 기능 실행
                levelUpController.IsPresentingLevelUp); // 레벨·제어기 처리
        }

        private void BuildMergeCandidates() // 구성·합성 기능 선언
        {
            currentCandidates.Clear(); // 현재·후보 기능 실행

            if (weaponManager == null) // 포탑·관리자 조건 확인
            {
                return; // 현재 기능 종료
            }

            IReadOnlyList<SnakeWeaponSlot> slots = // 목록·포탑 값 설정
                weaponManager.Slots; // 포탑·관리자 처리

            int candidateLimit = // 후보 값 설정
                Mathf.Clamp( // 대상 호출 시작
                    maximumDisplayedCandidates, // 최대·후보 항목 전달
                    1, // 대상 항목 전달
                    3 // 대상 처리
                ); // 앞선 항목 종료

            for (int firstIndex = 0; // 순서 반복 처리
                firstIndex < slots.Count && // 순서·슬롯 처리
                currentCandidates.Count < // 현재·후보 처리
                    candidateLimit; // 후보 처리
                firstIndex++) // 순서 처리
            {
                SnakeWeaponSlot first = // 포탑·슬롯 값 설정
                    slots[firstIndex]; // 슬롯·순서 처리

                if (!CanMergeSlot(first) || // 가능 여부·합성 조건 확인
                    ContainsWeaponGrade( // 포탑·등급 호출 시작
                        first.Weapon, // 포탑 항목 전달
                        first.Grade // 등급 처리
                    )) // 대상 처리
                {
                    continue; // 다음 반복 진행
                }

                for (int secondIndex = // 순서 반복 처리
                    firstIndex + 1; // 순서 처리
                    secondIndex < slots.Count; // 순서·슬롯 처리
                    secondIndex++) // 순서 처리
                {
                    SnakeWeaponSlot second = // 포탑·슬롯 값 설정
                        slots[secondIndex]; // 슬롯·순서 처리

                    if (!CanMergePair( // 가능 여부·합성 조건 확인
                        first, // 대상 항목 전달
                        second // 대상 처리
                    )) // 대상 처리
                    {
                        continue; // 다음 반복 진행
                    }

                    currentCandidates.Add( // 현재·후보 호출 시작
                        new WeaponMergeCandidate( // 포탑·합성 객체 생성
                            first.Weapon, // 포탑 항목 전달
                            first.Grade, // 등급 항목 전달
                            firstIndex, // 순서 항목 전달
                            secondIndex // 순서 처리
                        ) // 앞선 항목 종료
                    ); // 앞선 항목 종료

                    break; // 반복 또는 분기 종료
                }
            }
        }

        private bool ContainsWeaponGrade( // 포탑·등급 호출 시작
            WeaponData weapon, // 포탑·데이터 항목 전달
            int grade // 등급 처리
        ) // 앞선 항목 종료
        {
            for (int index = 0; // 순서 반복 처리
                index < currentCandidates.Count; // 순서·현재 처리
                index++) // 순서 처리
            {
                WeaponMergeCandidate candidate = // 포탑·합성 값 설정
                    currentCandidates[index]; // 현재·후보 처리

                if (candidate.Weapon == // 후보·포탑 조건 확인
                    weapon && // 포탑 처리
                    candidate.CurrentGrade == // 후보·현재 처리
                    grade) // 등급 처리
                {
                    return true; // 성공 결과 반환
                }
            }

            return false; // 실패 결과 반환
        }

        private static bool CanMergeSlot( // 가능 여부·합성 호출 시작
            SnakeWeaponSlot slot // 포탑·슬롯 처리
        ) // 앞선 항목 종료
        {
            return // 대상 결과 반환
                slot != null && // 슬롯 처리
                !slot.IsEmpty && // 슬롯·빈 상태 처리
                slot.Weapon != null && // 슬롯·포탑 처리
                slot.Grade >= 1 && // 슬롯·등급 처리
                slot.Grade < 5 && // 슬롯·등급 처리
                slot.Grade < // 슬롯·등급 처리
                    slot.Weapon.MaxGrade; // 슬롯·포탑 처리
        }

        private static bool CanMergePair( // 가능 여부·합성 호출 시작
            SnakeWeaponSlot first, // 포탑·슬롯 항목 전달
            SnakeWeaponSlot second // 포탑·슬롯 처리
        ) // 앞선 항목 종료
        {
            return // 대상 결과 반환
                CanMergeSlot(first) && // 가능 여부·합성 기능 실행
                CanMergeSlot(second) && // 가능 여부·합성 기능 실행
                first.Weapon == // 포탑 처리
                    second.Weapon && // 포탑 처리
                first.Grade == // 등급 처리
                    second.Grade; // 등급 처리
        }

        private void ApplyMergeMovementState( // 적용·합성 호출 시작
            bool active // 활성 처리
        ) // 앞선 항목 종료
        {
            GameManager manager = GameManager.Instance; // 게임 관리자 조회
            if (manager != null) // 관리자 연결 확인
            {
                manager.SetGameplayTimeScale(active ? mergeMoveMultiplier : 1f); // 전체 게임 속도 적용
            }
            if (movement != null) // 이동 조건 확인
            {
                movement.SetMergeMovementMode( // 이동·설정 호출 시작
                    active, // 활성 항목 전달
                    mergeMoveMultiplier // 합성·배율 처리
                ); // 앞선 항목 종료
            }

            if (stamina != null) // 스태미나 조건 확인
            {
                stamina.SetBoostBlocked( // 스태미나·설정 호출 시작
                    active // 활성 처리
                ); // 앞선 항목 종료
            }
        }

        private void Subscribe() // 구독 기능 선언
        {
            if (subscribed || // 대상 조건 확인
                mergePanel == null) // 합성·화면 처리
            {
                return; // 현재 기능 종료
            }

            if (weaponManager != null) // 무기 관리자 확인
            {
                weaponManager.SlotsChanged += HandleSlotsChanged; // 외부 슬롯 변경 감시
            }

            mergePanel.CandidateSelected += // 합성·화면 값 설정
                HandleCandidateSelected; // 후보·선택 값 처리

            mergePanel.CloseRequested += // 합성·화면 값 설정
                HandleCloseRequested; // 닫기 처리

            if (experience != null) // 경험치 조건 확인
            {
                experience.LevelUpRequested += // 경험치·레벨 값 설정
                    HandleLevelUpRequested; // 레벨 처리
            }

            subscribed = true; // 대상 값 설정
        }

        private void Unsubscribe() // 구독 해제 기능 선언
        {
            if (!subscribed) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            if (weaponManager != null) // 무기 관리자 확인
            {
                weaponManager.SlotsChanged -= HandleSlotsChanged; // 슬롯 변경 감시 해제
            }

            if (mergePanel != null) // 합성·화면 조건 확인
            {
                mergePanel.CandidateSelected -= // 합성·화면 값 설정
                    HandleCandidateSelected; // 후보·선택 값 처리

                mergePanel.CloseRequested -= // 합성·화면 값 설정
                    HandleCloseRequested; // 닫기 처리
            }

            if (experience != null) // 경험치 조건 확인
            {
                experience.LevelUpRequested -= // 경험치·레벨 값 설정
                    HandleLevelUpRequested; // 레벨 처리
            }

            subscribed = false; // 대상 값 설정
        }
    }
}
