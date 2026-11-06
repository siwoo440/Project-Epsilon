using System.Collections.Generic;
using ProjectEpsilon.Combat;
using ProjectEpsilon.Core;
using ProjectEpsilon.Player;
using ProjectEpsilon.UI;
using UnityEngine;
using UnityEngine.InputSystem; // 레벨업 취소 입력 사용

namespace ProjectEpsilon.Progression
{
    [DefaultExecutionOrder(140)]
    public sealed class SnakeLevelUpController : MonoBehaviour
    {
        [SerializeField] private SnakeExperience experience;
        [SerializeField] private SnakeBodyManager bodyManager;
        [SerializeField] private SnakeHealth health;
        [SerializeField] private SnakeWeaponManager weaponManager;
        [SerializeField] private WeaponRewardPool rewardPool;
        [SerializeField] private LevelUpPanelController levelUpPanel;

        private readonly List<WeaponRewardCandidate> currentCandidates =
            new List<WeaponRewardCandidate>();

        private bool subscribed;
        private bool presentingLevelUp;
        private bool ownsGamePause;
        private bool selectingReplacement; // 슬롯 교체 선택 상태
        private int replacementIndex; // 현재 선택 슬롯
        private WeaponRewardCandidate pendingWeapon; // 미확정 무기 보상
        private int escapeConsumedFrame = -1; // 취소 입력 처리 프레임

        public bool ConsumedEscapeThisFrame => escapeConsumedFrame == Time.frameCount; // 같은 프레임 취소 여부
        public bool IsPresentingLevelUp =>
            presentingLevelUp;

        public IReadOnlyList<WeaponRewardCandidate>
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

            if (levelUpPanel != null)
            {
                levelUpPanel.Hide();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();

            if (Application.isPlaying &&
                ownsGamePause &&
                GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }

            ownsGamePause = false;
            presentingLevelUp = false;
            currentCandidates.Clear();
        }

        private void Update() // 레벨업 키보드 입력
        { // 메서드 시작
            Keyboard keyboard = Keyboard.current; // 현재 키보드 조회
            if (!presentingLevelUp || keyboard == null) // 선택 상태와 입력 장치 확인
            { // 조건 시작
                return; // 입력 처리 종료
            } // 조건 끝
            if (selectingReplacement && keyboard.escapeKey.wasPressedThisFrame) // 교체 취소 입력 확인
            { // 조건 시작
                escapeConsumedFrame = Time.frameCount; // 취소 입력 소비 기록
                FinishLevelUp(); // 새 무기 거절 및 레벨업 완료
                return; // 다른 선택 차단
            } // 조건 끝
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) // 첫 선택 키 확인
            { // 조건 시작
                HandleCandidateSelected(0); // 첫 버튼 선택
            } // 조건 끝
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) // 둘째 선택 키 확인
            { // 조건 시작
                HandleCandidateSelected(1); // 둘째 버튼 선택
            } // 조건 끝
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) // 셋째 선택 키 확인
            { // 조건 시작
                HandleCandidateSelected(2); // 셋째 버튼 선택
            } // 조건 끝
        } // 메서드 끝

        public void Configure(
            SnakeExperience experienceSource,
            SnakeBodyManager manager,
            SnakeHealth snakeHealth,
            SnakeWeaponManager weapons,
            WeaponRewardPool pool,
            LevelUpPanelController panel
        )
        {
            Unsubscribe();

            experience = experienceSource;
            bodyManager = manager;
            health = snakeHealth;
            weaponManager = weapons;
            rewardPool = pool;
            levelUpPanel = panel;

            if (Application.isPlaying)
            {
                Subscribe();
            }
        }

        private void Subscribe()
        {
            if (subscribed ||
                experience == null ||
                levelUpPanel == null)
            {
                return;
            }

            experience.LevelUpRequested +=
                HandleLevelUpRequested;

            levelUpPanel.CandidateSelected +=
                HandleCandidateSelected;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (experience != null)
            {
                experience.LevelUpRequested -=
                    HandleLevelUpRequested;
            }

            if (levelUpPanel != null)
            {
                levelUpPanel.CandidateSelected -=
                    HandleCandidateSelected;
            }

            subscribed = false;
        }

        private void HandleLevelUpRequested(
            int level
        )
        {
            if (presentingLevelUp)
            {
                return;
            }

            ApplyLevelGrowth(
                out bool bodyGrew,
                out bool healthRestored
            );

            BuildCurrentCandidates(
                level
            );

            if (currentCandidates.Count <= 0)
            {
                Debug.LogWarning(
                    "[Project Epsilon] 사용 가능한 무기 후보가 없어 레벨업 보상을 건너뜁니다."
                );

                experience?.CompletePendingLevelUp();
                ResumeIfFinished();
                return;
            }

            presentingLevelUp = true;
            selectingReplacement = false; // 슬롯 선택 초기화

            GameManager gameManager =
                GameManager.Instance;

            if (!ownsGamePause &&
                gameManager != null &&
                gameManager.IsPlaying)
            {
                ownsGamePause = true;
                gameManager.PauseGame();
            }

            levelUpPanel.Show(
                level,
                bodyGrew,
                bodyManager == null
                    ? 0
                    : bodyManager.CurrentBodyCount,
                bodyManager == null
                    ? 20
                    : bodyManager.MaximumBodyCount,
                healthRestored,
                currentCandidates
            );
        }

        private void HandleCandidateSelected(int candidateIndex) // 보상 및 교체 선택 처리
        { // 메서드 시작
            if (!presentingLevelUp || weaponManager == null) // 레벨업 상태 확인
            { // 조건 시작
                return; // 잘못된 선택 무시
            } // 조건 끝
            if (selectingReplacement) // 슬롯 선택 단계 확인
            { // 조건 시작
                HandleReplacementChoice(candidateIndex); // 슬롯 또는 거절 처리
                return; // 무기 후보 선택 방지
            } // 조건 끝
            if (candidateIndex < 0 || candidateIndex >= currentCandidates.Count) // 후보 범위 확인
            { // 조건 시작
                return; // 잘못된 후보 무시
            } // 조건 끝
            WeaponRewardCandidate candidate = currentCandidates[candidateIndex]; // 선택 무기 조회
            if (!candidate.IsValid) // 유효한 무기 확인
            { // 조건 시작
                return; // 잘못된 무기 무시
            } // 조건 끝
            if (weaponManager.OccupiedSlotCount >= weaponManager.SlotCount) // 빈 슬롯 부재 확인
            { // 조건 시작
                pendingWeapon = candidate; // 선택 무기 임시 보관
                replacementIndex = 0; // 첫 슬롯 선택
                selectingReplacement = true; // 슬롯 선택 단계 진입
                ShowReplacementSelection(); // 슬롯 선택 화면 표시
                return; // 교체 확정 대기
            } // 조건 끝
            if (!weaponManager.AcquireWeapon(candidate.Weapon, candidate.Grade)) // 빈 슬롯 장착 확인
            { // 조건 시작
                Debug.LogWarning("[Project Epsilon] 선택한 무기를 장착하지 못했습니다."); // 장착 실패 알림
                return; // 재선택 대기
            } // 조건 끝
            FinishLevelUp(); // 보상 확정 처리
        } // 메서드 끝

        private void HandleReplacementChoice(int choice) // 슬롯 탐색 및 교체 처리
        { // 메서드 시작
            int optionCount = weaponManager.SlotCount + 1; // 슬롯 및 거절 항목 수
            if (choice == 0) // 이전 항목 선택
            { // 조건 시작
                replacementIndex = (replacementIndex + optionCount - 1) % optionCount; // 이전 슬롯 이동
                ShowReplacementSelection(); // 변경 항목 표시
                return; // 확정 방지
            } // 조건 끝
            if (choice == 2) // 다음 항목 선택
            { // 조건 시작
                replacementIndex = (replacementIndex + 1) % optionCount; // 다음 슬롯 이동
                ShowReplacementSelection(); // 변경 항목 표시
                return; // 확정 방지
            } // 조건 끝
            if (choice != 1) // 확정 버튼 확인
            { // 조건 시작
                return; // 잘못된 선택 무시
            } // 조건 끝
            if (replacementIndex < weaponManager.SlotCount && // 거절 항목 여부 확인
                !weaponManager.TryReplaceWeapon(replacementIndex, pendingWeapon.Weapon, pendingWeapon.Grade)) // 선택 무기 교체 및 머리 뒤 배치
            { // 조건 시작
                return; // 교체 실패 시 대기
            } // 조건 끝
            FinishLevelUp(); // 교체 또는 거절 확정
        } // 메서드 끝

        private void ShowReplacementSelection() // 슬롯 선택 화면 갱신
        { // 메서드 시작
            levelUpPanel.ShowReplacementSelection(weaponManager.Slots, pendingWeapon, replacementIndex); // 선택 항목 표시
        } // 메서드 끝

        private void FinishLevelUp() // 레벨업 보상 완료
        { // 메서드 시작
            selectingReplacement = false; // 교체 선택 종료
            presentingLevelUp = false; // 현재 레벨업 종료
            currentCandidates.Clear(); // 후보 목록 초기화
            if (levelUpPanel != null) // 화면 연결 확인
            { // 조건 시작
                levelUpPanel.Hide(); // 레벨업 화면 닫기
            } // 조건 끝
            experience?.CompletePendingLevelUp(); // 연속 레벨업 확인
            ResumeIfFinished(); // 남은 선택 없을 때 재개
        } // 메서드 끝

        private void BuildCurrentCandidates(
            int level
        )
        {
            currentCandidates.Clear();

            if (rewardPool == null)
            {
                return;
            }

            List<WeaponRewardCandidate> generated =
                rewardPool.BuildCandidates(
                    level,
                    weaponManager,
                    3
                );

            currentCandidates.AddRange(
                generated
            );
        }

        private void ApplyLevelGrowth(
            out bool bodyGrew,
            out bool healthRestored
        )
        {
            bodyGrew = false;
            healthRestored = false;

            if (bodyManager == null)
            {
                return;
            }

            if (bodyManager.CurrentBodyCount <
                bodyManager.MaximumBodyCount)
            {
                bodyGrew =
                    bodyManager.TryGainBodyFromLevelUp();

                return;
            }

            if (health != null)
            {
                health.ResetHealth();
                healthRestored = true;
            }
        }

        private void ResumeIfFinished()
        {
            if (presentingLevelUp ||
                !ownsGamePause)
            {
                return;
            }

            GameManager gameManager =
                GameManager.Instance;

            if (gameManager != null)
            {
                gameManager.ResumeGame();
            }

            ownsGamePause = false;
        }
    }
}
