using System.Collections.Generic; // 후보 목록
using ProjectEpsilon.Combat; // 무기 슬롯
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Player; // 체력과 몸통
using ProjectEpsilon.UI; // 보상 화면
using UnityEngine; // 유니티 기능
using UnityEngine.InputSystem; // 숫자 입력

namespace ProjectEpsilon.Progression // 진행 영역
{
    [DefaultExecutionOrder(160)] // 레벨업과 합성 이후 실행
    public sealed class EliteRewardController : MonoBehaviour // 엘리트 보상 처리기
    {
        [SerializeField] private SnakeExperience experience; // 경험치 상태
        [SerializeField] private SnakeLevelUpController levelUp; // 레벨업 화면 상태
        [SerializeField] private SnakeHealth health; // 공용 체력
        [SerializeField] private SnakeBodyManager body; // 몸통 관리자
        [SerializeField] private SnakeWeaponManager weapons; // 무기 관리자
        [SerializeField] private WeaponRewardPool weaponPool; // 무기 후보 풀
        [SerializeField] private SnakeWeaponMergeController merge; // 합성 화면
        [SerializeField] private LevelUpPanelController panel; // 보상 선택 화면
        [SerializeField] private SettingsMenuController settingsMenu; // 설정 화면 상태
        [Min(1)] [SerializeField] private int healingAmount = 15; // 임시 회복량
        [Range(0f, 1f)] [SerializeField] private float gradeTwoChance = 0.25f; // 임시 이성 무기 확률
        private int pendingRewards; // 남은 엘리트 보상 수
        private int[] candidates = new int[0]; // 현재 보상 종류
        private bool presenting; // 보상 선택 상태
        private bool selectingReplacement; // 무기 교체 선택 상태
        private bool awaitingMerge; // 보상 합성 진행 상태
        private bool ownsPause; // 보상 일시정지 소유 상태
        private bool subscribed; // 입력 이벤트 연결 상태
        private int replacementIndex; // 선택 중인 교체 슬롯
        private WeaponRewardCandidate pendingWeapon; // 교체 대기 무기
        public int PendingRewardCount => pendingRewards; // 남은 보상 수 조회
        public bool IsPresentingReward => presenting; // 보상 화면 상태 조회
        public bool IsAwaitingMerge => awaitingMerge; // 보상 합성 상태 조회
        public IReadOnlyList<int> Candidates => candidates; // 보상 종류 조회

        private void OnEnable() // 컴포넌트 활성화
        {
            Subscribe(); // 화면 이벤트 연결
        }

        private void Start() // 플레이 초기화
        {
            Subscribe(); // 화면 이벤트 재확인
        }

        private void OnDisable() // 컴포넌트 비활성화
        {
            Unsubscribe(); // 화면 이벤트 해제
            if (presenting && panel != null) // 열린 보상 화면 확인
            {
                panel.Hide(); // 보상 화면 닫기
            }
            presenting = false; // 보상 표시 종료
            selectingReplacement = false; // 교체 선택 종료
            awaitingMerge = false; // 합성 대기 종료
            ReleasePause(); // 보상 소유 정지 복구
        }

        public void Configure(SnakeExperience xp, SnakeLevelUpController level, SnakeHealth sharedHealth, SnakeBodyManager segments, SnakeWeaponManager weaponManager, WeaponRewardPool pool, SnakeWeaponMergeController merger, LevelUpPanelController rewardPanel, int heal, float gradeChance, SettingsMenuController settings = null) // 구성 참조 연결
        {
            Unsubscribe(); // 이전 이벤트 해제
            experience = xp; // 경험치 참조 저장
            levelUp = level; // 레벨업 참조 저장
            health = sharedHealth; // 체력 참조 저장
            body = segments; // 몸통 참조 저장
            weapons = weaponManager; // 무기 참조 저장
            weaponPool = pool; // 무기 풀 저장
            merge = merger; // 합성 참조 저장
            panel = rewardPanel; // 보상 화면 저장
            settingsMenu = settings; // 설정 화면 저장
            healingAmount = Mathf.Max(1, heal); // 임시 회복량 저장
            gradeTwoChance = Mathf.Clamp01(gradeChance); // 임시 등급 확률 저장
            Subscribe(); // 새 이벤트 연결
        }

        public void QueueReward() // 엘리트 보상 대기열 추가
        {
            if (pendingRewards < int.MaxValue) // 대기열 한도 확인
            {
                pendingRewards++; // 보상 요청 저장
            }
        }

        private void Update() // 보상 화면과 입력 처리
        {
            if (!presenting) // 보상 화면 부재 확인
            {
                ProcessPendingReward(); // 대기 보상 표시 시도
                return; // 같은 프레임 선택 방지
            }
            if (GameManager.Instance == null || !GameManager.Instance.IsPaused) // 소유 정지 상태 확인
            {
                AbandonPresentation(); // 미확정 보상 대기 복귀
                return; // 입력 차단
            }
            if (IsInputBlocked()) // 설정 화면 또는 키 변경 확인
            {
                return; // 보상 입력 차단
            }
            if (!selectingReplacement) // 보상 후보 화면 확인
            {
                bool[] available = GetAvailability(); // 현재 후보 적용 가능 상태
                if (!HasUsableCandidate(available)) // 남은 유효 후보 확인
                {
                    candidates = new int[0]; // 무효 후보 초기화
                    presenting = false; // 보상 화면 종료
                    panel?.Hide(); // 선택 화면 숨김
                    ReleasePause(); // 다음 보상 추첨을 위한 재개
                    return; // 유효 후보 발생 대기
                }
                ShowCandidates(); // 현재 후보의 선택 가능 상태 갱신
            }
            Keyboard keyboard = Keyboard.current; // 키보드 조회
            if (keyboard == null) // 입력 장치 부재
            {
                return; // 입력 처리 생략
            }
            if (selectingReplacement && keyboard.escapeKey.wasPressedThisFrame) // 무기 교체 취소
            {
                selectingReplacement = false; // 교체 단계 종료
                ShowCandidates(); // 보상 재선택 화면
                return; // 추가 입력 방지
            }
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) // 첫 숫자 확인
            {
                TrySelectCandidate(0); // 첫 후보 선택
            }
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) // 둘째 숫자 확인
            {
                TrySelectCandidate(1); // 둘째 후보 선택
            }
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) // 셋째 숫자 확인
            {
                TrySelectCandidate(2); // 셋째 후보 선택
            }
        }

        public bool ProcessPendingReward() // 대기 보상 표시 시도
        {
            GameManager manager = GameManager.Instance; // 게임 관리자 조회
            if (pendingRewards <= 0 || presenting || awaitingMerge || manager == null || !manager.IsPlaying || panel == null || IsInputBlocked()) // 보상 표시 조건
            {
                return false; // 보상 표시 연기
            }
            if ((experience != null && experience.IsLevelUpPending) || (levelUp != null && levelUp.IsPresentingLevelUp)) // 연속 레벨업 확인
            {
                return false; // 레벨업 종료 대기
            }
            bool[] available = GetAvailability(); // 현재 적용 가능 후보
            int[] selected = HasUsableCandidate(available) ? candidates : EliteRewardRules.Select(available, Random.Range(0, int.MaxValue)); // 후보 유지 또는 새 추첨
            if (selected.Length != 3) // 사용 가능한 후보 확인
            {
                return false; // 유효 보상 발생까지 대기
            }
            if (merge != null && merge.IsMergeOpen) // 일반 합성 화면 확인
            {
                merge.CloseMergePanel(); // 합성 취소 및 속도 복구
            }
            candidates = selected; // 보상 후보 저장
            presenting = true; // 보상 선택 시작
            selectingReplacement = false; // 교체 상태 초기화
            manager.PauseGame(); // 보상 선택 중 정지
            ownsPause = manager.IsPaused; // 정지 소유 확인
            if (!ownsPause) // 정지 실패 확인
            {
                presenting = false; // 화면 상태 복구
                return false; // 보상 보류
            }
            ShowCandidates(); // 보상 후보 표시
            return true; // 보상 표시 완료
        }

        public bool TrySelectCandidate(int index) // 보상 또는 교체 선택
        {
            if (!presenting || index < 0 || index > 2 || GameManager.Instance == null || !GameManager.Instance.IsPaused || IsInputBlocked()) // 선택 조건 확인
            {
                return false; // 잘못된 입력 차단
            }
            if (selectingReplacement) // 교체 화면 확인
            {
                return HandleReplacement(index); // 교체 선택 처리
            }
            if (candidates.Length != 3) // 후보 구성 확인
            {
                return false; // 비정상 후보 차단
            }
            int kind = candidates[index]; // 선택 보상 종류
            bool[] available = GetAvailability(); // 적용 가능 상태 갱신
            if (kind < 0 || kind >= available.Length || !available[kind]) // 유효 보상 확인
            {
                ShowCandidates(); // 최신 선택 상태 표시
                return false; // 적용 불가 선택 차단
            }
            if (kind == 0 && health.Heal(healingAmount)) // 체력 회복 적용
            {
                FinishReward(); // 보상 완료
                return true; // 선택 성공
            }
            if (kind == 1 && body.TryRepairBody()) // 몸통 한 칸 복구
            {
                FinishReward(); // 보상 완료
                return true; // 선택 성공
            }
            if (kind == 2) // 무기 보상 확인
            {
                return BeginWeaponReward(); // 무기 장착 또는 교체
            }
            if (kind == 3) // 즉시 합성 보상 확인
            {
                return BeginMergeReward(); // 합성 화면 전환
            }
            return false; // 적용 실패 반환
        }

        public bool YieldPauseToLevelUp() // 레벨업에 정지 소유권 이양
        {
            if (!presenting || !ownsPause || GameManager.Instance == null || !GameManager.Instance.IsPaused) // 인계 가능 상태 확인
            {
                return false; // 인계 불가
            }
            presenting = false; // 보상 선택 보류
            selectingReplacement = false; // 교체 선택 초기화
            ownsPause = false; // 보상 정지 소유 해제
            panel?.Hide(); // 레벨업 전 보상 화면 숨김
            return true; // 정지 소유권 이양
        }

        private bool[] GetAvailability() // 보상별 적용 가능 상태
        {
            return new bool[] // 네 가지 적용 가능 여부
            {
                health != null && health.CurrentHealth < health.MaximumHealth && body != null && body.CurrentBodyCount > 0, // 손상된 체력 확인
                (GameManager.Instance == null || !GameManager.Instance.IsFinalBossPhase) && body != null && body.MissingRepairableBodyCount > 0, // 보스전 제외 복구 가능 몸통
                weapons != null && weapons.SlotCount > 0 && GetAvailableWeapons().Count > 0, // 무기 후보와 슬롯 확인
                merge != null && merge.HasMergeCandidates() // 합성 가능한 쌍 확인
            };
        }

        private bool IsInputBlocked() // 다른 입력 화면 확인
        {
            return (settingsMenu != null && settingsMenu.IsOpen) || (InputBindingManager.Instance != null && InputBindingManager.Instance.IsRebinding); // 설정 화면 및 키 변경 확인
        }

        private bool HasUsableCandidate(bool[] available) // 기존 후보의 사용 가능 여부
        {
            if (candidates == null || candidates.Length != 3) // 기존 후보 확인
            {
                return false; // 새 추첨 필요
            }
            foreach (int kind in candidates) // 기존 후보 순회
            {
                if (kind >= 0 && kind < available.Length && available[kind]) // 사용 가능한 후보 확인
                {
                    return true; // 기존 후보 유지
                }
            }
            return false; // 사용 가능한 후보 부재
        }

        private List<WeaponData> GetAvailableWeapons() // 보상 가능한 무기 목록
        {
            List<WeaponData> result = new List<WeaponData>(); // 무기 목록 초기화
            if (weaponPool == null) // 무기 풀 부재 확인
            {
                return result; // 빈 후보 반환
            }
            foreach (WeaponRewardEntry entry in weaponPool.Entries) // 무기 풀 순회
            {
                if (entry != null && entry.Unlocked && entry.Weapon != null && (MetaProgressionController.Instance == null || MetaProgressionController.Instance.IsTurretUnlocked(entry.Weapon)) && !weapons.HasCompletedGradeFive(entry.Weapon)) // 실제 해금 및 최고 등급 확인
                {
                    result.Add(entry.Weapon); // 사용 가능한 무기 추가
                }
            }
            return result; // 무기 후보 반환
        }

        private void ShowCandidates() // 보상 후보 화면 갱신
        {
            bool[] availability = GetAvailability(); // 최신 적용 가능 상태
            string[] labels = new string[3]; // 후보 설명 목록
            bool[] enabled = new bool[3]; // 후보 선택 가능 목록
            for (int index = 0; index < candidates.Length; index++) // 선택 후보 순회
            {
                int kind = candidates[index]; // 보상 종류 조회
                enabled[index] = availability[kind]; // 선택 가능 상태 저장
                labels[index] = BuildLabel(kind, enabled[index]); // 보상 안내 생성
            }
            panel.ShowEliteRewards(labels, enabled, candidates); // 실제 보상 종류와 후보 화면 표시
        }

        private string BuildLabel(int kind, bool enabled) // 후보별 안내 문구
        {
            switch (kind) // 보상 종류 분기
            {
                case 0: return enabled ? $"체력 회복 +{healingAmount}" : "체력 회복 (최대)"; // 체력 회복 안내
                case 1: return enabled ? "몸통 1칸 복구" : GameManager.Instance != null && GameManager.Instance.IsFinalBossPhase ? "몸통 복구 (보스전 불가)" : "몸통 복구 (최대)"; // 몸통 복구 제한 안내
                case 2: return enabled ? "무작위 포탑 ★1~2" : "포탑 보상 (선택 불가)"; // 무기 보상 안내
                case 3: return enabled ? "포탑 합성" : "포탑 합성 (재료 없음)"; // 합성 기회 안내
                default: return "선택 불가"; // 알 수 없는 후보
            }
        }

        private bool BeginWeaponReward() // 무기 보상 선택 처리
        {
            List<WeaponData> available = GetAvailableWeapons(); // 무기 후보 조회
            if (available.Count == 0) // 무기 후보 부재 확인
            {
                return false; // 보상 선택 유지
            }
            if (!pendingWeapon.IsValid || !available.Contains(pendingWeapon.Weapon) || pendingWeapon.Grade > pendingWeapon.Weapon.MaxGrade) // 기존 무기 후보 유효성 확인
            {
                WeaponData selected = available[WeaponRewardPool.SelectWeaponIndex(available, weapons, CharacterRunInitializer.ActiveCharacter)]; // 캐릭터별 무기 후보 가중 추첨
                int selectedGrade = selected.MaxGrade >= 2 && Random.value < gradeTwoChance ? 2 : 1; // 임시 등급 추첨
                pendingWeapon = new WeaponRewardCandidate(selected, selectedGrade); // 보상 무기와 등급 고정
            }
            WeaponData weapon = pendingWeapon.Weapon; // 확정된 무기 종류
            int grade = pendingWeapon.Grade; // 확정된 무기 등급
            if (weapons.OccupiedSlotCount < weapons.SlotCount) // 빈 슬롯 확인
            {
                if (!weapons.AcquireWeapon(weapon, grade)) // 새 무기 장착 확인
                {
                    return false; // 보상 선택 유지
                }
                FinishReward(); // 무기 보상 완료
                return true; // 무기 획득 성공
            }
            selectingReplacement = true; // 교체 선택 시작
            replacementIndex = 0; // 첫 슬롯 선택
            ShowReplacement(); // 교체 화면 표시
            return true; // 교체 선택 진입
        }

        private bool HandleReplacement(int choice) // 무기 교체 선택 처리
        {
            int optionCount = weapons.SlotCount + 1; // 교체 및 거절 항목 수
            if (choice == 0 || choice == 2) // 이전 또는 다음 선택
            {
                replacementIndex = (replacementIndex + (choice == 0 ? optionCount - 1 : 1)) % optionCount; // 슬롯 순환 이동
                ShowReplacement(); // 슬롯 화면 갱신
                return true; // 이동 성공
            }
            if (choice != 1) // 확정 입력 확인
            {
                return false; // 잘못된 선택 차단
            }
            if (replacementIndex >= weapons.SlotCount) // 신규 무기 거절 확인
            {
                selectingReplacement = false; // 교체 선택 종료
                ShowCandidates(); // 보상 재선택 화면
                return true; // 재선택 성공
            }
            if (!weapons.TryReplaceWeapon(replacementIndex, pendingWeapon.Weapon, pendingWeapon.Grade)) // 선택 슬롯 교체 확인
            {
                return false; // 교체 실패 시 대기
            }
            FinishReward(); // 무기 교체 완료
            return true; // 교체 성공
        }

        private void ShowReplacement() // 무기 교체 화면 표시
        {
            panel.ShowReplacementSelection(weapons.Slots, pendingWeapon, replacementIndex); // 기존 슬롯 화면 재사용
        }

        private bool BeginMergeReward() // 보상 합성 전환
        {
            if (merge == null || !merge.HasMergeCandidates()) // 합성 가능 상태 확인
            {
                return false; // 보상 선택 유지
            }
            presenting = false; // 보상 화면 종료
            panel.Hide(); // 보상 화면 숨김
            awaitingMerge = true; // 합성 결과 대기
            ReleasePause(); // 합성 중 게임 진행 허용
            if (merge.OpenMergePanel()) // 기존 합성 화면 열기
            {
                return true; // 합성 화면 전환 성공
            }
            awaitingMerge = false; // 합성 대기 해제
            return false; // 다음 프레임 보상 재선택
        }

        private void HandleMergeClosed(bool merged) // 합성 완료 또는 취소 처리
        {
            if (!awaitingMerge) // 보상 합성 여부 확인
            {
                return; // 일반 합성 무시
            }
            awaitingMerge = false; // 합성 결과 대기 종료
            if (merged && pendingRewards > 0) // 합성 성공 확인
            {
                pendingRewards--; // 보상 한 개 소비
                candidates = new int[0]; // 완료된 후보 초기화
                pendingWeapon = default; // 완료된 무기 후보 초기화
            }
        }

        private void FinishReward() // 선택 보상 완료
        {
            if (pendingRewards > 0) // 대기 보상 확인
            {
                pendingRewards--; // 보상 한 개 소비
            }
            presenting = false; // 보상 화면 종료
            selectingReplacement = false; // 교체 화면 종료
            candidates = new int[0]; // 완료된 후보 초기화
            pendingWeapon = default; // 완료된 무기 후보 초기화
            panel?.Hide(); // 선택 화면 숨김
            ReleasePause(); // 보상 정지 복구
        }

        private void AbandonPresentation() // 중단된 선택 화면 정리
        {
            presenting = false; // 선택 상태 해제
            selectingReplacement = false; // 교체 상태 해제
            ownsPause = false; // 외부 상태 변경 시 소유 해제
            panel?.Hide(); // 보상 화면 숨김
        }

        private void ReleasePause() // 보상 소유 정지 복구
        {
            GameManager manager = GameManager.Instance; // 게임 관리자 조회
            if (ownsPause && manager != null && manager.IsPaused) // 보상 소유 일시정지 확인
            {
                manager.ResumeGame(); // 게임 재개
            }
            ownsPause = false; // 정지 소유 해제
        }

        private void Subscribe() // 화면 이벤트 연결
        {
            if (subscribed) // 중복 연결 확인
            {
                return; // 재연결 생략
            }
            if (panel != null) // 보상 화면 참조 확인
            {
                panel.CandidateSelected += HandlePanelSelection; // 보상 버튼 연결
            }
            if (merge != null) // 합성 화면 참조 확인
            {
                merge.MergeClosed += HandleMergeClosed; // 합성 결과 연결
            }
            subscribed = true; // 이벤트 연결 기록
        }

        private void Unsubscribe() // 화면 이벤트 연결 해제
        {
            if (!subscribed) // 연결 상태 확인
            {
                return; // 중복 해제 생략
            }
            if (panel != null) // 보상 화면 참조 확인
            {
                panel.CandidateSelected -= HandlePanelSelection; // 보상 버튼 해제
            }
            if (merge != null) // 합성 화면 참조 확인
            {
                merge.MergeClosed -= HandleMergeClosed; // 합성 결과 해제
            }
            subscribed = false; // 연결 상태 초기화
        }

        private void HandlePanelSelection(int index) // 화면 버튼 선택
        {
            TrySelectCandidate(index); // 공통 선택 경로
        }
    }
}
