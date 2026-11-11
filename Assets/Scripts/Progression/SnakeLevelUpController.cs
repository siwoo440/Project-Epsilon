using System.Collections.Generic; // C# 기본 기능 연결
using ProjectEpsilon.Combat; // 전투 기능 연결
using ProjectEpsilon.Core; // 대상 기능 연결
using ProjectEpsilon.Player; // 플레이어 기능 연결
using ProjectEpsilon.UI; // UI 기능 연결
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.InputSystem; // 레벨업 취소 입력 사용

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{
    [DefaultExecutionOrder(140)] // 대상 설정
    public sealed class SnakeLevelUpController : MonoBehaviour // 레벨·제어기 자료 구조
    {
        [SerializeField] private SnakeExperience experience; // 필드·경험치 설정
        [SerializeField] private SnakeBodyManager bodyManager; // 필드·몸통 설정
        [SerializeField] private SnakeHealth health; // 필드·체력 설정
        [SerializeField] private SnakeWeaponManager weaponManager; // 필드·포탑 설정
        [SerializeField] private WeaponRewardPool rewardPool; // 필드·포탑 설정
        [SerializeField] private LevelUpPanelController levelUpPanel; // 필드·레벨 설정
        [SerializeField] private SettingsMenuController settingsMenu; // 설정 화면 참조

        private readonly List<WeaponRewardCandidate> currentCandidates = // 목록·포탑 구성 선언
            new List<WeaponRewardCandidate>(); // 목록·포탑 객체 생성

        private bool subscribed; // 대상 구성 선언
        private bool presentingLevelUp; // 레벨 구성 선언
        private bool ownsGamePause; // 게임·일시정지 구성 선언
        private bool selectingReplacement; // 슬롯 교체 선택 상태
        private int replacementIndex; // 현재 선택 슬롯
        private WeaponRewardCandidate pendingWeapon; // 미확정 무기 보상
        private int escapeConsumedFrame = -1; // 취소 입력 처리 프레임
        private SnakeWeaponMergeController rewardMerge; // 보상 합성 제어기
        private bool selectingRewardAction; // 보상 처리 방식 선택
        private bool awaitingRewardMerge; // 미장착 보상 합성 대기
        private bool restoreRewardOnEnable; // 중단된 보상 복구 대기
        private int displayedLevel; // 표시 중인 레벨
        private bool displayedBodyGrowth; // 표시 중인 몸통 성장
        private bool displayedHealthRestore; // 표시 중인 체력 회복

        public bool ConsumedEscapeThisFrame => escapeConsumedFrame == Time.frameCount; // 같은 프레임 취소 여부
        public bool IsAwaitingRewardMerge => awaitingRewardMerge; // 보상 합성 대기 조회
        public bool IsPresentingLevelUp => // 레벨 값 반환
            presentingLevelUp; // 레벨 처리

        public IReadOnlyList<WeaponRewardCandidate> // 목록·포탑 구성 선언
            CurrentCandidates => // 현재·후보 처리
                currentCandidates; // 현재·후보 처리

        private void OnEnable() // 활성 기능 선언
        {
            if (Application.isPlaying) // 대상 조건 확인
            {
                Subscribe(); // 구독 기능 실행
                TryRestoreInterruptedReward(); // 중단된 보상 복구
            }
        }

        private void Start() // 시작 기능 선언
        {
            Subscribe(); // 구독 기능 실행

            if (levelUpPanel != null) // 레벨·화면 조건 확인
            {
                levelUpPanel.Hide(); // 레벨·화면 기능 실행
            }
        }

        private void OnDisable() // 비활성 시 보상 보존
        {
            bool interrupted = Application.isPlaying && (presentingLevelUp || awaitingRewardMerge || restoreRewardOnEnable); // 진행 중 보상 확인
            Unsubscribe(); // 이벤트 구독 해제
            if (awaitingRewardMerge && rewardMerge != null && rewardMerge.IsMergeOpen) // 진행 중 보상 합성 확인
            {
                rewardMerge.CloseMergePanel(); // 합성 취소 및 속도 복구
            }
            awaitingRewardMerge = false; // 보상 합성 대기 해제
            if (Application.isPlaying && ownsGamePause && GameManager.Instance != null) // 직접 소유한 일시정지 확인
            {
                GameManager.Instance.ResumeGame(); // 일시정지 소유 반환
            }
            ownsGamePause = false; // 정지 소유 해제
            presentingLevelUp = false; // 비활성 중 입력 차단
            if (levelUpPanel != null) // 보상 화면 객체 확인
            {
                levelUpPanel.Hide(); // 보상 화면 숨김
            }
            if (interrupted) // 미확정 보상 확인
            {
                restoreRewardOnEnable = true; // 재활성 시 기존 단계 복구
                return; // 후보 및 선택 보존
            }
            currentCandidates.Clear(); // 종료된 후보 정리
        }

        private void Update() // 레벨업 키보드 입력
        {
            GameManager game = GameManager.Instance; // 현재 게임 상태
            if ((presentingLevelUp || awaitingRewardMerge || restoreRewardOnEnable) && game != null && !game.IsPlaying && !game.IsPaused) // 종료된 게임 확인
            {
                HandlePlayerDepleted(); // 남은 선택 화면 정리
                return; // 패배 중 입력 차단
            }
            TryRestoreInterruptedReward(); // 중단된 보상 복구 시도
            Keyboard keyboard = Keyboard.current; // 현재 키보드 조회
            if (!presentingLevelUp || awaitingRewardMerge || keyboard == null || IsInputBlocked()) // 선택 상태와 입력 차단 확인
            {
                return; // 입력 처리 종료
            }
            if (selectingReplacement && keyboard.escapeKey.wasPressedThisFrame) // 교체 취소 입력 확인
            {
                escapeConsumedFrame = Time.frameCount; // 취소 입력 소비 기록
                FinishLevelUp(); // 새 무기 거절 및 레벨업 완료
                return; // 다른 선택 차단
            }
            if (keyboard.rKey.wasPressedThisFrame && TryRerollCandidates()) // 실제 런당 후보 재선택 입력
            {
                return; // 같은 프레임 후보 선택 차단
            }
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) // 첫 선택 키 확인
            {
                HandleCandidateSelected(0); // 첫 버튼 선택
            }
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) // 둘째 선택 키 확인
            {
                HandleCandidateSelected(1); // 둘째 버튼 선택
            }
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) // 셋째 선택 키 확인
            {
                HandleCandidateSelected(2); // 셋째 버튼 선택
            }
        }

        private bool IsInputBlocked() // 설정 화면 및 키 재설정 확인
        {
            if (InputBindingManager.Instance != null && InputBindingManager.Instance.IsRebinding) // 키 재설정 상태 확인
            {
                return true; // 보상 입력 차단
            }
            if (settingsMenu == null) // 설정 화면 연결 확인
            {
                settingsMenu = FindFirstObjectByType<SettingsMenuController>(); // 설정 화면 검색
            }
            return settingsMenu != null && settingsMenu.IsOpen; // 설정 화면 표시 여부
        }

        public void Configure( // 구성 호출 시작
            SnakeExperience experienceSource, // 경험치·원본 항목 전달
            SnakeBodyManager manager, // 몸통·관리자 항목 전달
            SnakeHealth snakeHealth, // 체력 항목 전달
            SnakeWeaponManager weapons, // 포탑·관리자 항목 전달
            WeaponRewardPool pool, // 포탑·보상 항목 전달
            LevelUpPanelController panel // 레벨·화면 처리
        ) // 앞선 항목 종료
        {
            Unsubscribe(); // 구독 해제 기능 실행

            experience = experienceSource; // 경험치·원본 값 설정
            bodyManager = manager; // 몸통·관리자 값 설정
            health = snakeHealth; // 체력 값 설정
            weaponManager = weapons; // 포탑·관리자 값 설정
            rewardPool = pool; // 보상 값 설정
            levelUpPanel = panel; // 레벨·화면 값 설정

            if (Application.isPlaying) // 대상 조건 확인
            {
                Subscribe(); // 구독 기능 실행
            }
        }

        private void Subscribe() // 구독 기능 선언
        {
            if (subscribed || // 대상 조건 확인
                experience == null || // 경험치 처리
                levelUpPanel == null) // 레벨·화면 처리
            {
                return; // 현재 기능 종료
            }

            rewardMerge = GetComponent<SnakeWeaponMergeController>(); // 같은 플레이어의 합성 제어기
            if (rewardMerge != null) // 합성 제어기 연결 확인
            {
                rewardMerge.MergeClosed += HandleRewardMergeClosed; // 보상 합성 종료 구독
            }

            if (health != null) // 체력 구성 확인
            {
                health.BodyDepleted += HandlePlayerDepleted; // 몸통 소진 감시
            }

            experience.LevelUpRequested += // 경험치·레벨 값 설정
                HandleLevelUpRequested; // 레벨 처리

            levelUpPanel.CandidateSelected += // 레벨·화면 값 설정
                HandleCandidateSelected; // 후보·선택 값 처리

            subscribed = true; // 대상 값 설정
        }

        private void Unsubscribe() // 구독 해제 기능 선언
        {
            if (!subscribed) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            if (rewardMerge != null) // 합성 제어기 연결 확인
            {
                rewardMerge.MergeClosed -= HandleRewardMergeClosed; // 보상 합성 종료 해제
            }

            if (health != null) // 체력 구성 확인
            {
                health.BodyDepleted -= HandlePlayerDepleted; // 몸통 소진 감시 해제
            }

            if (experience != null) // 경험치 조건 확인
            {
                experience.LevelUpRequested -= // 경험치·레벨 값 설정
                    HandleLevelUpRequested; // 레벨 처리
            }

            if (levelUpPanel != null) // 레벨·화면 조건 확인
            {
                levelUpPanel.CandidateSelected -= // 레벨·화면 값 설정
                    HandleCandidateSelected; // 후보·선택 값 처리
            }

            subscribed = false; // 대상 값 설정
        }

        private void HandleLevelUpRequested( // 레벨 호출 시작
            int level // 레벨 처리
        ) // 앞선 항목 종료
        {
            EliteRewardController elite = GetComponent<EliteRewardController>(); // 엘리트 보상 상태 조회
            if (elite != null && elite.YieldPauseToLevelUp()) // 보상 정지 인계 확인
            {
                ownsGamePause = true; // 레벨업 종료까지 정지 소유
            }
            if (presentingLevelUp) // 레벨 조건 확인
            {
                return; // 현재 기능 종료
            }

            ApplyLevelGrowth( // 적용·레벨 호출 시작
                out bool bodyGrew, // 몸통 항목 전달
                out bool healthRestored // 체력 처리
            ); // 앞선 항목 종료

            BuildCurrentCandidates( // 구성·현재 호출 시작
                level // 레벨 처리
            ); // 앞선 항목 종료

            if (currentCandidates.Count <= 0) // 현재·후보 조건 확인
            {
                Debug.LogWarning( // 대상 호출 시작
                    "[Project Epsilon] 사용 가능한 무기 후보가 없어 레벨업 보상을 건너뜁니다." // 프로젝트 경로 또는 문구 값
                ); // 앞선 항목 종료

                experience?.CompletePendingLevelUp(); // 경험치·완료 기능 실행
                ResumeIfFinished(); // 재개 기능 실행
                return; // 현재 기능 종료
            }

            presentingLevelUp = true; // 레벨 값 설정
            selectingReplacement = false; // 슬롯 선택 초기화
            selectingRewardAction = false; // 보상 방식 선택 초기화
            displayedLevel = level; // 현재 레벨 표시 저장
            displayedBodyGrowth = bodyGrew; // 성장 표시 저장
            displayedHealthRestore = healthRestored; // 회복 표시 저장

            GameManager gameManager = // 게임·관리자 값 설정
                GameManager.Instance; // 게임·관리자 처리

            if (!ownsGamePause && // 게임·일시정지 조건 확인
                gameManager != null && // 게임·관리자 처리
                gameManager.IsPlaying) // 게임·관리자 처리
            {
                ownsGamePause = true; // 게임·일시정지 값 설정
                gameManager.PauseGame(); // 게임·관리자 기능 실행
            }

            levelUpPanel.Show( // 레벨·화면 호출 시작
                level, // 레벨 항목 전달
                bodyGrew, // 몸통 항목 전달
                bodyManager == null // 몸통·관리자 처리
                    ? 0 // 대상 처리
                    : bodyManager.CurrentBodyCount, // 몸통·관리자 항목 전달
                bodyManager == null // 몸통·관리자 처리
                    ? 20 // 대상 처리
                    : bodyManager.MaximumBodyCount, // 몸통·관리자 항목 전달
                healthRestored, // 체력 항목 전달
                currentCandidates // 현재·후보 처리
            ); // 앞선 항목 종료
            levelUpPanel.SetRerollAvailable(MetaProgressionController.Instance != null && MetaProgressionController.Instance.CanReroll); // 실제 재선택 안내
        }

        public bool TryRerollCandidates() // 실제 런당 보상 후보 재선택
        {
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 영구 성장 관리자
            if (!presentingLevelUp || selectingReplacement || selectingRewardAction || awaitingRewardMerge || IsInputBlocked() || rewardPool == null || meta == null || !meta.CanReroll) // 실제 후보 선택과 성장 확인
            {
                return false; // 재선택 불가 상태 유지
            }
            List<WeaponRewardCandidate> candidates = rewardPool.BuildCandidates(displayedLevel, weaponManager, 3); // 실제 새 보상 후보
            if (candidates.Count == 0 || !meta.TryUseReroll()) // 후보와 런당 사용 확인
            {
                return false; // 기존 후보 유지
            }
            currentCandidates.Clear(); // 이전 후보 제거
            currentCandidates.AddRange(candidates); // 실제 새 후보 적용
            ShowCandidates(); // 실제 후보 화면 갱신
            return true; // 실제 재선택 완료
        }

        private void HandleCandidateSelected(int candidateIndex) // 보상 및 교체 선택 처리
        {
            if (!presentingLevelUp || awaitingRewardMerge || weaponManager == null || IsInputBlocked()) // 레벨업 및 입력 상태 확인
            {
                return; // 잘못된 선택 무시
            }
            if (selectingRewardAction) // 보상 방식 선택 단계 확인
            {
                HandleRewardAction(candidateIndex); // 처리 방식 선택
                return; // 후보 선택 방지
            }
            if (selectingReplacement) // 슬롯 선택 단계 확인
            {
                HandleReplacementChoice(candidateIndex); // 슬롯 또는 거절 처리
                return; // 무기 후보 선택 방지
            }
            if (candidateIndex < 0 || candidateIndex >= currentCandidates.Count) // 후보 범위 확인
            {
                return; // 잘못된 후보 무시
            }
            WeaponRewardCandidate candidate = currentCandidates[candidateIndex]; // 선택 무기 조회
            if (!candidate.IsValid) // 유효한 무기 확인
            {
                return; // 잘못된 무기 무시
            }
            pendingWeapon = candidate; // 선택 보상 보관
            if (CanMergeReward(candidate)) // 보상 합성 가능 여부
            {
                selectingRewardAction = true; // 처리 방식 선택 진입
                ShowRewardAction(); // 장착 및 합성 선택 표시
                return; // 확정 대기
            }
            AcquirePendingWeapon(); // 기존 장착 및 교체 경로
        }

        private bool CanMergeReward(WeaponRewardCandidate candidate) // 미장착 보상 합성 가능 확인
        {
            if (rewardMerge == null || !rewardMerge.isActiveAndEnabled || candidate.Weapon == null || candidate.Grade < 1 || candidate.Grade >= 5 || candidate.Grade >= candidate.Weapon.MaxGrade) // 합성 제어기 및 등급 확인
            {
                return false; // 합성 불가
            }
            for (int index = 0; index < weaponManager.Slots.Count; index++) // 장착 슬롯 순회
            {
                SnakeWeaponSlot slot = weaponManager.Slots[index]; // 장착 무기 조회
                if (slot != null && !slot.IsEmpty && slot.Weapon == candidate.Weapon && slot.Grade == candidate.Grade) // 같은 무기 및 등급 확인
                {
                    return true; // 합성 가능
                }
            }
            return false; // 일치 재료 없음
        }

        private void HandleRewardAction(int choice) // 보상 처리 방식 선택
        {
            if (choice == 0) // 장착 또는 교체 선택
            {
                selectingRewardAction = false; // 처리 방식 선택 종료
                AcquirePendingWeapon(); // 기존 장착 경로
                return; // 다른 선택 차단
            }
            if (choice == 1) // 보상 합성 선택
            {
                BeginRewardMerge(); // 보상 합성 진입
                return; // 다른 선택 차단
            }
            if (choice == 2) // 후보 재선택
            {
                selectingRewardAction = false; // 처리 방식 선택 종료
                ShowCandidates(); // 기존 후보 표시
            }
        }

        private void AcquirePendingWeapon() // 선택 보상 장착 또는 교체
        {
            if (weaponManager.OccupiedSlotCount >= weaponManager.SlotCount) // 빈 슬롯 부재 확인
            {
                replacementIndex = 0; // 첫 슬롯 선택
                selectingReplacement = true; // 슬롯 선택 단계 진입
                ShowReplacementSelection(); // 슬롯 선택 화면 표시
                return; // 교체 확정 대기
            }
            if (!weaponManager.AcquireWeapon(pendingWeapon.Weapon, pendingWeapon.Grade)) // 빈 슬롯 장착 확인
            {
                Debug.LogWarning("[Project Epsilon] 선택한 무기를 장착하지 못했습니다."); // 장착 실패 알림
                ShowCandidates(); // 후보 재선택
                return; // 확정 방지
            }
            FinishLevelUp(); // 보상 확정 처리
        }

        private void ShowCandidates() // 기존 레벨업 후보 표시
        {
            levelUpPanel.Show(displayedLevel, displayedBodyGrowth, bodyManager == null ? 0 : bodyManager.CurrentBodyCount, bodyManager == null ? 20 : bodyManager.MaximumBodyCount, displayedHealthRestore, currentCandidates); // 저장된 후보 표시
            levelUpPanel.SetRerollAvailable(MetaProgressionController.Instance != null && MetaProgressionController.Instance.CanReroll); // 실제 재선택 안내
        }

        private void ShowRewardAction() // 보상 처리 방식 표시
        {
            levelUpPanel.ShowRewardAction(pendingWeapon, CanMergeReward(pendingWeapon)); // 합성 가능 여부를 포함한 처리 방식
        }

        private void BeginRewardMerge() // 보상 합성 중 플레이 전환
        {
            GameManager manager = GameManager.Instance; // 현재 게임 상태
            if (!CanMergeReward(pendingWeapon) || manager == null || !ownsGamePause || !manager.IsPaused) // 합성 및 정지 소유 확인
            {
                return; // 현재 선택 유지
            }
            awaitingRewardMerge = true; // 레벨업 완료 보류
            levelUpPanel.Hide(); // 레벨업 화면 숨김
            manager.ResumeGame(); // 합성 중 게임 진행
            ownsGamePause = false; // 레벨업 정지 소유 반환
            if (rewardMerge.OpenRewardMerge(pendingWeapon)) // 합성 화면 진입 확인
            {
                return; // 합성 결과 대기
            }
            awaitingRewardMerge = false; // 실패 시 대기 해제
            if (manager.IsPlaying) // 게임 진행 상태 확인
            {
                manager.PauseGame(); // 레벨업 선택 정지 복구
                ownsGamePause = manager.IsPaused; // 정지 소유 복구
                ShowRewardAction(); // 보상 처리 방식 복구
            }
        }

        private void HandleRewardMergeClosed(bool merged) // 보상 합성 종료 처리
        {
            if (!awaitingRewardMerge) // 보상 합성 대기 확인
            {
                return; // 일반 합성 결과 무시
            }
            awaitingRewardMerge = false; // 결과 중복 처리 방지
            if (merged) // 합성 성공 확인
            {
                FinishLevelUp(); // 보상 한 개 완료
                return; // 취소 처리 방지
            }
            escapeConsumedFrame = Time.frameCount; // 같은 프레임 ESC 소비
            GameManager manager = GameManager.Instance; // 현재 게임 상태
            if (manager == null || !manager.IsPlaying) // 패배 및 외부 정지 확인
            {
                restoreRewardOnEnable = manager != null && manager.IsPaused; // 외부 정지 종료 후 복구 대기
                presentingLevelUp = false; // 현재 보상 화면 해제
                ownsGamePause = false; // 정지 소유 해제
                return; // 외부 게임 상태 유지
            }
            manager.PauseGame(); // 보상 선택 일시정지 복구
            ownsGamePause = manager.IsPaused; // 정지 소유 복구
            ShowRewardAction(); // 선택 보상 복귀
        }

        private void HandlePlayerDepleted() // 몸통 소진 시 보상 화면 정리
        {
            awaitingRewardMerge = false; // 보상 합성 결과 처리 차단
            restoreRewardOnEnable = false; // 보상 화면 복구 취소
            presentingLevelUp = false; // 보상 선택 종료
            selectingRewardAction = false; // 보상 방식 선택 종료
            selectingReplacement = false; // 교체 선택 종료
            ownsGamePause = false; // 정지 소유 해제
            if (levelUpPanel != null) // 레벨업 화면 객체 확인
            {
                levelUpPanel.Hide(); // 레벨업 화면 숨김
            }
            if (rewardMerge != null && rewardMerge.IsMergeOpen) // 열려 있는 합성 화면 확인
            {
                rewardMerge.CloseMergePanel(); // 합성 화면 및 속도 정리
            }
        }

        private void TryRestoreInterruptedReward() // 중단된 보상 선택 복구
        {
            if (!restoreRewardOnEnable || !Application.isPlaying || IsInputBlocked()) // 복구 대기 및 입력 상태 확인
            {
                return; // 복구 연기
            }
            GameManager manager = GameManager.Instance; // 현재 게임 상태
            if (manager == null || !manager.IsPlaying) // 게임 진행 확인
            {
                return; // 진행 재개 대기
            }
            manager.PauseGame(); // 레벨업 선택 정지
            if (!manager.IsPaused) // 정지 성공 확인
            {
                return; // 복구 재시도 대기
            }
            ownsGamePause = true; // 레벨업 정지 소유
            restoreRewardOnEnable = false; // 복구 대기 해제
            presentingLevelUp = true; // 보상 선택 재개
            if (selectingReplacement) // 기존 교체 단계 확인
            {
                ShowReplacementSelection(); // 같은 교체 화면 복구
            }
            else if (selectingRewardAction) // 기존 처리 방식 확인
            {
                ShowRewardAction(); // 같은 보상 방식 복구
            }
            else // 일반 후보 단계 확인
            {
                ShowCandidates(); // 같은 후보 화면 복구
            }
        }

        private void HandleReplacementChoice(int choice) // 슬롯 탐색 및 교체 처리
        {
            int optionCount = weaponManager.SlotCount + 1; // 슬롯 및 거절 항목 수
            if (choice == 0) // 이전 항목 선택
            {
                replacementIndex = (replacementIndex + optionCount - 1) % optionCount; // 이전 슬롯 이동
                ShowReplacementSelection(); // 변경 항목 표시
                return; // 확정 방지
            }
            if (choice == 2) // 다음 항목 선택
            {
                replacementIndex = (replacementIndex + 1) % optionCount; // 다음 슬롯 이동
                ShowReplacementSelection(); // 변경 항목 표시
                return; // 확정 방지
            }
            if (choice != 1) // 확정 버튼 확인
            {
                return; // 잘못된 선택 무시
            }
            if (replacementIndex < weaponManager.SlotCount && // 거절 항목 여부 확인
                !weaponManager.TryReplaceWeapon(replacementIndex, pendingWeapon.Weapon, pendingWeapon.Grade)) // 선택 무기 교체 및 머리 뒤 배치
            {
                return; // 교체 실패 시 대기
            }
            FinishLevelUp(); // 교체 또는 거절 확정
        }

        private void ShowReplacementSelection() // 슬롯 선택 화면 갱신
        {
            levelUpPanel.ShowReplacementSelection(weaponManager.Slots, pendingWeapon, replacementIndex); // 선택 항목 표시
        }

        private void FinishLevelUp() // 레벨업 보상 완료
        {
            selectingReplacement = false; // 교체 선택 종료
            selectingRewardAction = false; // 처리 방식 선택 종료
            presentingLevelUp = false; // 현재 레벨업 종료
            currentCandidates.Clear(); // 후보 목록 초기화
            if (levelUpPanel != null) // 화면 연결 확인
            {
                levelUpPanel.Hide(); // 레벨업 화면 닫기
            }
            experience?.CompletePendingLevelUp(); // 연속 레벨업 확인
            ResumeIfFinished(); // 남은 선택 없을 때 재개
        }

        private void BuildCurrentCandidates( // 구성·현재 호출 시작
            int level // 레벨 처리
        ) // 앞선 항목 종료
        {
            currentCandidates.Clear(); // 현재·후보 기능 실행

            if (rewardPool == null) // 보상 조건 확인
            {
                return; // 현재 기능 종료
            }

            List<WeaponRewardCandidate> generated = // 목록·포탑 값 설정
                rewardPool.BuildCandidates( // 보상·구성 호출 시작
                    level, // 레벨 항목 전달
                    weaponManager, // 포탑·관리자 항목 전달
                    3 // 대상 처리
                ); // 앞선 항목 종료

            currentCandidates.AddRange( // 현재·후보 호출 시작
                generated // 대상 처리
            ); // 앞선 항목 종료
        }

        private void ApplyLevelGrowth( // 적용·레벨 호출 시작
            out bool bodyGrew, // 몸통 항목 전달
            out bool healthRestored // 체력 처리
        ) // 앞선 항목 종료
        {
            bodyGrew = false; // 몸통 값 설정
            healthRestored = false; // 체력 값 설정

            if (bodyManager == null) // 몸통·관리자 조건 확인
            {
                return; // 현재 기능 종료
            }

            if (bodyManager.CurrentBodyCount < // 몸통·관리자 조건 확인
                bodyManager.MaximumBodyCount) // 몸통·관리자 처리
            {
                bodyGrew = // 몸통 값 설정
                    bodyManager.TryGainBodyFromLevelUp(); // 몸통·관리자 기능 실행

                return; // 현재 기능 종료
            }

            if (health != null) // 체력 조건 확인
            {
                health.ResetHealth(); // 체력·초기화 기능 실행
                healthRestored = true; // 체력 값 설정
            }
        }

        private void ResumeIfFinished() // 재개 기능 선언
        {
            if (presentingLevelUp || // 레벨 조건 확인
                !ownsGamePause) // 게임·일시정지 처리
            {
                return; // 현재 기능 종료
            }

            GameManager gameManager = // 게임·관리자 값 설정
                GameManager.Instance; // 게임·관리자 처리

            if (gameManager != null) // 게임·관리자 조건 확인
            {
                gameManager.ResumeGame(); // 게임·관리자 기능 실행
            }

            ownsGamePause = false; // 게임·일시정지 값 설정
        }
    }
}
