using System; // C# 기본 기능 연결
using System.Collections.Generic; // C# 기본 기능 연결
using ProjectEpsilon.Progression; // 대상 기능 연결
using ProjectEpsilon.Combat; // 무기 슬롯 표시
using ProjectEpsilon.Data; // 무기 속성 표시
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.UI; // Unity 엔진 기능 연결

namespace ProjectEpsilon.UI // 프로젝트 기능 영역
{
    public sealed class WeaponMergePanelController : MonoBehaviour // 포탑·합성 자료 구조
    {
        [SerializeField] private Text titleText; // 필드·문구 설정
        [SerializeField] private Text stateText; // 필드·문구 설정

        [SerializeField] private Button candidateButton01; // 필드·버튼 설정
        [SerializeField] private Button candidateButton02; // 필드·버튼 설정
        [SerializeField] private Button candidateButton03; // 필드·버튼 설정

        [SerializeField] private Text candidateLabel01; // 필드·문구 설정
        [SerializeField] private Text candidateLabel02; // 필드·문구 설정
        [SerializeField] private Text candidateLabel03; // 필드·문구 설정

        [SerializeField] private Button closeButton; // 필드·버튼 설정

        private bool buttonsBound; // 대상 구성 선언

        public event Action<int> CandidateSelected; // 이벤트·동작 구성 선언
        public event Action CloseRequested; // 이벤트·동작 구성 선언

        public void ApplyDisplayFont(Font value) // 실제 포탑과 보상 글꼴 연결
        {
            if (value == null) // 실제 글꼴 존재 확인
            {
                return; // 누락 글꼴 적용 방지
            }
            foreach (Text label in GetComponentsInChildren<Text>(true)) // 실제 비활성 화면 글자까지 순회
            {
                label.font = value; // 실제 한국어 우선 글꼴 적용
            }
        }

        private void Awake() // 초기화 기능 선언
        {
            BindButtons(); // 연결 기능 실행
        }

        private void OnEnable() // 활성 기능 선언
        {
            BindButtons(); // 연결 기능 실행
        }

        private void OnDisable() // 비활성 기능 선언
        {
            UnbindButtons(); // 대상 기능 실행
        }

        private void OnDestroy() // 제거 기능 선언
        {
            UnbindButtons(); // 대상 기능 실행
        }

        public void Configure( // 구성 호출 시작
            Text title, // 문구 항목 전달
            Text state, // 문구·상태 항목 전달
            Button button01, // 버튼 항목 전달
            Text label01, // 문구 항목 전달
            Button button02, // 버튼 항목 전달
            Text label02, // 문구 항목 전달
            Button button03, // 버튼 항목 전달
            Text label03, // 문구 항목 전달
            Button closeControl // 버튼·닫기 처리
        ) // 앞선 항목 종료
        {
            UnbindButtons(); // 대상 기능 실행

            titleText = title; // 문구 값 설정
            stateText = state; // 상태·문구 값 설정

            candidateButton01 = button01; // 후보·버튼 값 설정
            candidateLabel01 = label01; // 후보·문구 값 설정

            candidateButton02 = button02; // 후보·버튼 값 설정
            candidateLabel02 = label02; // 후보·문구 값 설정

            candidateButton03 = button03; // 후보·버튼 값 설정
            candidateLabel03 = label03; // 후보·문구 값 설정

            closeButton = closeControl; // 닫기·버튼 값 설정

            if (Application.isPlaying) // 대상 조건 확인
            {
                BindButtons(); // 연결 기능 실행
            }
        }

        public void Show( // 표시 호출 시작
            IReadOnlyList<WeaponMergeCandidate> candidates // 목록·포탑 처리
        ) // 앞선 항목 종료
        {
            gameObject.SetActive(true); // 게임·객체 기능 실행

            if (titleText != null) // 문구 조건 확인
            {
                titleText.text = // 문구 값 설정
                    "포탑 합성"; // 포탑·합성 경로 또는 문구 값
            }

            if (stateText != null) // 상태·문구 조건 확인
            {
                stateText.text = // 상태·문구 값 설정
                    "실시간 / 자동 전진 / 속도 70%"; // 시간·속도 경로 또는 문구 값
            }

            ConfigureCandidate( // 구성·후보 호출 시작
                0, // 대상 항목 전달
                candidateButton01, // 후보·버튼 항목 전달
                candidateLabel01, // 후보·문구 항목 전달
                candidates // 후보 처리
            ); // 앞선 항목 종료

            ConfigureCandidate( // 구성·후보 호출 시작
                1, // 대상 항목 전달
                candidateButton02, // 후보·버튼 항목 전달
                candidateLabel02, // 후보·문구 항목 전달
                candidates // 후보 처리
            ); // 앞선 항목 종료

            ConfigureCandidate( // 구성·후보 호출 시작
                2, // 대상 항목 전달
                candidateButton03, // 후보·버튼 항목 전달
                candidateLabel03, // 후보·문구 항목 전달
                candidates // 후보 처리
            ); // 앞선 항목 종료

            BindButtons(); // 연결 기능 실행
        }

        public void ShowSlotSelection(SnakeWeaponSlot slot, int slotIndex, int cursor, int count, bool first) // 재료 선택 화면
        {
            gameObject.SetActive(true); // 합성 화면 표시
            if (titleText != null) // 제목 연결 확인
            {
                titleText.text = first ? "첫 번째 재료 선택" : "두 번째 재료 선택"; // 재료 단계 안내
            }
            if (stateText != null) // 상태 문구 연결 확인
            {
                stateText.text = $"{cursor + 1}/{count} | 이스케이프: 취소"; // 위치 및 취소 안내
            }
            ConfigureUpgradeButton(candidateButton01, candidateLabel01, "< 이전"); // 이전 슬롯 버튼
            ConfigureUpgradeButton(candidateButton02, candidateLabel02, FormatSlot(slot, slotIndex)); // 현재 슬롯 버튼
            ConfigureUpgradeButton(candidateButton03, candidateLabel03, "다음 >"); // 다음 슬롯 버튼
            BindButtons(); // 버튼 연결
        }

        public void ShowInheritanceSelection(SnakeWeaponSlot first, int firstIndex, SnakeWeaponSlot second, int secondIndex) // 강화 계승 화면
        {
            if (titleText != null) // 제목 연결 확인
            {
                titleText.text = "계승할 강화 선택"; // 계승 단계 안내
            }
            if (stateText != null) // 상태 문구 연결 확인
            {
                stateText.text = "재료 선택 | 이스케이프: 취소"; // 계승 및 취소 안내
            }
            ConfigureUpgradeButton(candidateButton01, candidateLabel01, FormatSlot(first, firstIndex)); // 첫 재료 강화 표시
            ConfigureUpgradeButton(candidateButton02, candidateLabel02, FormatSlot(second, secondIndex)); // 둘째 재료 강화 표시
            if (candidateButton03 != null) // 남는 버튼 확인
            {
                candidateButton03.gameObject.SetActive(false); // 남는 버튼 숨김
            }
        }

        private static string FormatSlot(SnakeWeaponSlot slot, int slotIndex) // 재료 정보 작성
        {
            return $"몸통 {slotIndex + 1}: {slot.Weapon.DisplayName} ★{slot.Grade}\n피해 +{slot.DamageUpgrades * 5}%  속도 +{slot.SpeedUpgrades * 5}%  범위 +{slot.RangeUpgrades * 5}%"; // 슬롯 및 강화 요약
        }

        public void ShowRewardInheritanceSelection(WeaponData reward, int grade, SnakeWeaponSlot equipped, int equippedIndex) // 보상 및 장착 강화 선택
        {
            if (titleText != null) // 제목 연결 확인
            {
                titleText.text = "계승할 강화 선택"; // 계승 단계 안내
            }
            if (stateText != null) // 상태 문구 연결 확인
            {
                stateText.text = "재료 선택 | 이스케이프: 취소"; // 계승 및 취소 안내
            }
            ConfigureUpgradeButton(candidateButton01, candidateLabel01, $"보상: {reward.DisplayName} ★{grade}\n피해 +0%  속도 +0%  범위 +0%"); // 미장착 보상 강화 표시
            ConfigureUpgradeButton(candidateButton02, candidateLabel02, FormatSlot(equipped, equippedIndex)); // 장착 무기 강화 표시
            if (candidateButton03 != null) // 남는 버튼 확인
            {
                candidateButton03.gameObject.SetActive(false); // 남는 버튼 숨김
            }
        }

        public void ShowRewardUpgradeSelection(WeaponData weapon) // 보상 합성 강화 화면
        {
            if (titleText != null) // 제목 연결 확인
            {
                titleText.text = "합성 강화 선택"; // 강화 선택 제목
            }
            if (stateText != null) // 상태 문구 연결 확인
            {
                stateText.text = "이스케이프: 취소"; // 취소 안내
            }
            ConfigureUpgradeButton(candidateButton01, candidateLabel01, "공격력 +5%"); // 공격력 선택 표시
            ConfigureUpgradeButton(candidateButton02, candidateLabel02, "공격 속도 +5%"); // 공격 속도 선택 표시
            ConfigureUpgradeButton(candidateButton03, candidateLabel03, "사거리 / 범위 +5%"); // 사거리 선택 표시
        }

        public void ShowSynergyPreview(WeaponAttribute attribute, int count, int resultCount, int stage, int resultStage) // 시너지 변화 표시
        {
            if (stateText == null) // 상태 문구 연결 확인
            {
                return; // 표시 생략
            }
            string current = stage == 0 ? "미활성" : $"{stage}단계"; // 현재 단계 문구
            string result = resultStage == 0 ? "미활성" : $"{resultStage}단계"; // 합성 후 단계 문구
            stateText.text += $" | {WeaponDisplayNames.Attribute(attribute)}: {count} → {resultCount} | {current} → {result}"; // 속성 수 및 단계 표시
        }

        public void ShowUpgradeSelection(WeaponMergeCandidate candidate) // 합성 강화 후보 표시
        {
            if (titleText != null) // 제목 연결 확인
            {
                titleText.text = "합성 강화 선택"; // 강화 선택 제목
            }
            if (stateText != null) // 상태 문구 연결 확인
            {
                stateText.text = "이스케이프: 취소"; // 대상과 취소 안내
            }
            ConfigureUpgradeButton(candidateButton01, candidateLabel01, "공격력 +5%"); // 공격력 선택 표시
            ConfigureUpgradeButton(candidateButton02, candidateLabel02, "공격 속도 +5%"); // 공격 속도 선택 표시
            ConfigureUpgradeButton(candidateButton03, candidateLabel03, "사거리 / 범위 +5%"); // 사거리 선택 표시
        }

        private static void ConfigureUpgradeButton(Button button, Text label, string caption) // 강화 버튼 표시
        {
            LevelUpPanelController.EnlargeChoiceText(label); // 합성 선택 문구 확대
            if (button != null) // 버튼 연결 확인
            {
                button.gameObject.SetActive(true); // 후보 버튼 표시
                button.interactable = true; // 후보 선택 허용
            }
            if (label != null) // 문구 연결 확인
            {
                label.text = caption; // 강화 내용 표시
            }
        }

        public void Hide() // 숨김 기능 선언
        {
            if (gameObject.activeSelf) // 게임·객체 조건 확인
            {
                gameObject.SetActive( // 게임·객체 호출 시작
                    false // 대상 처리
                ); // 앞선 항목 종료
            }
        }

        private void HandleCandidate01() // 후보 기능 선언
        {
            CandidateSelected?.Invoke(0); // 후보·선택 값 기능 실행
        }

        private void HandleCandidate02() // 후보 기능 선언
        {
            CandidateSelected?.Invoke(1); // 후보·선택 값 기능 실행
        }

        private void HandleCandidate03() // 후보 기능 선언
        {
            CandidateSelected?.Invoke(2); // 후보·선택 값 기능 실행
        }

        private void HandleClose() // 닫기 기능 선언
        {
            CloseRequested?.Invoke(); // 닫기 기능 실행
        }

        private void BindButtons() // 연결 기능 선언
        {
            if (buttonsBound) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            if (candidateButton01 != null) // 후보·버튼 조건 확인
            {
                candidateButton01.onClick.AddListener( // 후보·버튼 호출 시작
                    HandleCandidate01 // 후보 처리
                ); // 앞선 항목 종료
            }

            if (candidateButton02 != null) // 후보·버튼 조건 확인
            {
                candidateButton02.onClick.AddListener( // 후보·버튼 호출 시작
                    HandleCandidate02 // 후보 처리
                ); // 앞선 항목 종료
            }

            if (candidateButton03 != null) // 후보·버튼 조건 확인
            {
                candidateButton03.onClick.AddListener( // 후보·버튼 호출 시작
                    HandleCandidate03 // 후보 처리
                ); // 앞선 항목 종료
            }

            if (closeButton != null) // 닫기·버튼 조건 확인
            {
                closeButton.onClick.AddListener( // 닫기·버튼 호출 시작
                    HandleClose // 닫기 처리
                ); // 앞선 항목 종료
            }

            buttonsBound = true; // 대상 값 설정
        }

        private void UnbindButtons() // 대상 기능 선언
        {
            if (!buttonsBound) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            if (candidateButton01 != null) // 후보·버튼 조건 확인
            {
                candidateButton01.onClick.RemoveListener( // 후보·버튼 호출 시작
                    HandleCandidate01 // 후보 처리
                ); // 앞선 항목 종료
            }

            if (candidateButton02 != null) // 후보·버튼 조건 확인
            {
                candidateButton02.onClick.RemoveListener( // 후보·버튼 호출 시작
                    HandleCandidate02 // 후보 처리
                ); // 앞선 항목 종료
            }

            if (candidateButton03 != null) // 후보·버튼 조건 확인
            {
                candidateButton03.onClick.RemoveListener( // 후보·버튼 호출 시작
                    HandleCandidate03 // 후보 처리
                ); // 앞선 항목 종료
            }

            if (closeButton != null) // 닫기·버튼 조건 확인
            {
                closeButton.onClick.RemoveListener( // 닫기·버튼 호출 시작
                    HandleClose // 닫기 처리
                ); // 앞선 항목 종료
            }

            buttonsBound = false; // 대상 값 설정
        }

        private static void ConfigureCandidate( // 구성·후보 호출 시작
            int index, // 순서 항목 전달
            Button button, // 버튼 항목 전달
            Text label, // 문구 항목 전달
            IReadOnlyList<WeaponMergeCandidate> candidates // 목록·포탑 처리
        ) // 앞선 항목 종료
        {
            LevelUpPanelController.EnlargeChoiceText(label); // 합성 후보 문구 확대
            bool available = // 대상 값 설정
                candidates != null && // 후보 처리
                index >= 0 && // 순서 처리
                index < candidates.Count && // 순서·후보 처리
                candidates[index].IsValid; // 후보·순서 처리

            if (button != null) // 버튼 조건 확인
            {
                button.gameObject.SetActive( // 버튼·게임 호출 시작
                    available // 대상 처리
                ); // 앞선 항목 종료

                button.interactable = // 버튼 값 설정
                    available; // 대상 처리
            }

            if (!available || // 대상 조건 확인
                label == null) // 문구 처리
            {
                return; // 현재 기능 종료
            }

            WeaponMergeCandidate candidate = // 포탑·합성 값 설정
                candidates[index]; // 후보·순서 처리

            label.text = // 문구 값 설정
                BuildCandidateLabel( // 구성·후보 호출 시작
                    candidate // 후보 처리
                ); // 앞선 항목 종료
        }

        private static string BuildCandidateLabel( // 구성·후보 호출 시작
            WeaponMergeCandidate candidate // 포탑·합성 처리
        ) // 앞선 항목 종료
        {
            string currentStars = // 현재 값 설정
                new string( // 대상 객체 생성
                    '★', // 대상 항목 전달
                    Mathf.Clamp( // 대상 호출 시작
                        candidate.CurrentGrade, // 후보·현재 항목 전달
                        1, // 대상 항목 전달
                        5 // 대상 처리
                    ) // 앞선 항목 종료
                ); // 앞선 항목 종료

            string resultStars = // 결과 값 설정
                new string( // 대상 객체 생성
                    '★', // 대상 항목 전달
                    Mathf.Clamp( // 대상 호출 시작
                        candidate.ResultGrade, // 후보·결과 항목 전달
                        1, // 대상 항목 전달
                        5 // 대상 처리
                    ) // 앞선 항목 종료
                ); // 앞선 항목 종료

            return // 대상 결과 반환
                $"{candidate.Weapon.DisplayName}\n" + // 후보·포탑 처리
                $"{currentStars} + {currentStars}\n" + // 현재 처리
                $"→ {resultStars}\n" + // 결과 처리
                "2 → 1"; // 대상 경로 또는 문구 값
        }
    }
}
