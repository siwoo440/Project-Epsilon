using System;
using System.Collections.Generic;
using ProjectEpsilon.Progression;
using ProjectEpsilon.Data; // 시작 캐릭터 데이터
using ProjectEpsilon.Combat; // 무기 슬롯 사용
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEpsilon.UI
{
    public sealed class LevelUpPanelController : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text growthText;

        [Header("Weapon Candidates")]
        [SerializeField] private Button candidateButton01;
        [SerializeField] private Button candidateButton02;
        [SerializeField] private Button candidateButton03;

        [SerializeField] private Text candidateLabel01;
        [SerializeField] private Text candidateLabel02;
        [SerializeField] private Text candidateLabel03;

        private bool buttonsBound;

        public event Action<int> CandidateSelected;

        private void Awake()
        {
            BindButtons();
        }

        private void OnEnable()
        {
            BindButtons();
        }

        private void OnDisable()
        {
            UnbindButtons();
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        public void Configure(
            Text title,
            Text level,
            Text growth,
            Button button01,
            Text label01,
            Button button02,
            Text label02,
            Button button03,
            Text label03
        )
        {
            UnbindButtons();

            titleText = title;
            levelText = level;
            growthText = growth;

            candidateButton01 = button01;
            candidateLabel01 = label01;

            candidateButton02 = button02;
            candidateLabel02 = label02;

            candidateButton03 = button03;
            candidateLabel03 = label03;

            if (Application.isPlaying)
            {
                BindButtons();
            }
        }

        public void Show(
            int level,
            bool bodyGrew,
            int currentBodyCount,
            int maximumBodyCount,
            bool healthRestored,
            IReadOnlyList<WeaponRewardCandidate> candidates
        )
        {
            gameObject.SetActive(true);

            if (titleText != null)
            {
                titleText.text = "LEVEL UP!";
            }

            if (levelText != null)
            {
                levelText.text =
                    $"Lv. {Mathf.Max(1, level)}";
            }

            if (growthText != null)
            {
                growthText.text =
                    BuildGrowthText(
                        bodyGrew,
                        currentBodyCount,
                        maximumBodyCount,
                        healthRestored
                    );
            }

            ConfigureCandidate(
                0,
                candidateButton01,
                candidateLabel01,
                candidates
            );

            ConfigureCandidate(
                1,
                candidateButton02,
                candidateLabel02,
                candidates
            );

            ConfigureCandidate(
                2,
                candidateButton03,
                candidateLabel03,
                candidates
            );

            BindButtons();
        }

        public void SetRerollAvailable(bool available) // 런당 재선택 안내
        { // 메서드 시작
            if (available && growthText != null) // 실제 안내 표시 확인
            { // 조건 시작
                growthText.text += "   R: REROLL"; // 실제 재선택 키 표시
            } // 조건 끝
        } // 메서드 끝

        public void ShowStartingWeapons(IReadOnlyList<WeaponRewardCandidate> candidates) // 시작 무기 후보 표시
        { // 메서드 시작
            gameObject.SetActive(true); // 시작 후보 화면 표시
            if (titleText != null) // 제목 연결 확인
            { // 조건 시작
                titleText.text = "CHOOSE STARTING WEAPON"; // 시작 무기 선택 제목
            } // 조건 끝
            if (levelText != null) // 부제 연결 확인
            { // 조건 시작
                levelText.text = "CHOOSE ONE BEFORE THE RUN"; // 시작 전 선택 안내
            } // 조건 끝
            if (growthText != null) // 조작 안내 연결 확인
            { // 조건 시작
                growthText.text = "1 / 2 / 3: SELECT"; // 선택 키 안내
            } // 조건 끝
            ConfigureCandidate(0, candidateButton01, candidateLabel01, candidates); // 첫 후보 표시
            ConfigureCandidate(1, candidateButton02, candidateLabel02, candidates); // 둘째 후보 표시
            ConfigureCandidate(2, candidateButton03, candidateLabel03, candidates); // 셋째 후보 표시
            BindButtons(); // 후보 버튼 연결
        } // 메서드 끝

        public void SetStartingCharacter(CharacterData character) // 시작 화면 캐릭터 정보 표시
        { // 메서드 시작
            if (character == null) // 캐릭터 데이터 확인
            { // 조건 시작
                return; // 기존 시작 안내 유지
            } // 조건 끝
            if (levelText != null) // 실제 캐릭터 문구 확인
            { // 조건 시작
                levelText.text = $"{character.DisplayName} | BODY {character.StartingBodyCount} / HP {character.BodyMaxHealth:0}"; // 실제 캐릭터 시작 능력치
            } // 조건 끝
            if (growthText != null) // 실제 조작 안내 확인
            { // 조건 시작
                growthText.text = "← / →: CHARACTER    1 / 2 / 3: WEAPON"; // 캐릭터와 무기 선택 안내
            } // 조건 끝
        } // 메서드 끝

        public void SetStartingMap(ProjectEpsilon.World.PrototypeMapProfile map) // 시작 화면 맵 정보 표시
        { // 메서드 시작
            if (growthText != null && map != null) // 실제 안내와 맵 확인
            { // 조건 시작
                growthText.text = $"← / →: CHARACTER    ↑ / ↓: MAP ({map.DisplayName})    1 / 2 / 3: WEAPON"; // 시작 맵과 조작 표시
            } // 조건 끝
        } // 메서드 끝

        public void ShowEliteRewards(string[] labels, bool[] available) // 엘리트 보상 후보 표시
        { // 메서드 시작
            gameObject.SetActive(true); // 선택 화면 표시
            if (titleText != null) // 제목 연결 확인
            { // 조건 시작
                titleText.text = "ELITE REWARD"; // 보상 제목 표시
            } // 조건 끝
            if (levelText != null) // 부제 연결 확인
            { // 조건 시작
                levelText.text = "CHOOSE ONE"; // 단일 선택 안내
            } // 조건 끝
            if (growthText != null) // 안내 문구 연결 확인
            { // 조건 시작
                growthText.text = "1 / 2 / 3: SELECT"; // 입력 안내
            } // 조건 끝
            ConfigureEliteButton(candidateButton01, candidateLabel01, labels, available, 0); // 첫 후보 표시
            ConfigureEliteButton(candidateButton02, candidateLabel02, labels, available, 1); // 둘째 후보 표시
            ConfigureEliteButton(candidateButton03, candidateLabel03, labels, available, 2); // 셋째 후보 표시
            BindButtons(); // 버튼 입력 연결
        } // 메서드 끝

        private static void ConfigureEliteButton(Button button, Text label, string[] labels, bool[] available, int index) // 보상 버튼 구성
        { // 메서드 시작
            WeaponCandidateArtwork.Set(button, label, null); // 이전 무기 후보 이미지 제거
            bool exists = labels != null && available != null && index < labels.Length && index < available.Length; // 후보 존재 확인
            if (button != null) // 버튼 연결 확인
            { // 조건 시작
                button.gameObject.SetActive(exists); // 후보 표시 여부
                button.interactable = exists && available[index]; // 적용 가능 후보만 선택
            } // 조건 끝
            if (label != null && exists) // 문구와 후보 확인
            { // 조건 시작
                label.text = labels[index]; // 후보 내용 표시
            } // 조건 끝
        } // 메서드 끝

        public void ShowRewardAction(WeaponRewardCandidate candidate, bool canMerge) // 보상 처리 방식 선택
        { // 메서드 시작
            gameObject.SetActive(true); // 레벨업 화면 표시
            if (titleText != null) // 제목 연결 확인
            { // 조건 시작
                titleText.text = "CHOOSE WEAPON ACTION"; // 처리 방식 제목
            } // 조건 끝
            if (levelText != null) // 무기 문구 연결 확인
            { // 조건 시작
                levelText.text = $"{candidate.Weapon.DisplayName} ★{candidate.Grade}"; // 선택한 보상 무기
            } // 조건 끝
            if (growthText != null) // 안내 문구 연결 확인
            { // 조건 시작
                growthText.text = "1 / 2 / 3: SELECT"; // 선택 키 안내
            } // 조건 끝
            SetReplacementButton(candidateButton01, candidateLabel01, "EQUIP / REPLACE"); // 장착 및 교체 선택
            SetReplacementButton(candidateButton02, candidateLabel02, canMerge ? "MERGE REWARD" : "MERGE (NO MATERIAL)"); // 합성 가능 상태 안내
            if (candidateButton02 != null) // 합성 버튼 연결 확인
            { // 조건 시작
                candidateButton02.interactable = canMerge; // 재료가 있을 때만 합성 허용
            } // 조건 끝
            SetReplacementButton(candidateButton03, candidateLabel03, "BACK"); // 기존 후보 재선택
            BindButtons(); // 버튼 이벤트 연결
        } // 메서드 끝

        public void ShowReplacementSelection(IReadOnlyList<SnakeWeaponSlot> slots, WeaponRewardCandidate candidate, int index) // 교체 슬롯 화면 표시
        { // 메서드 시작
            gameObject.SetActive(true); // 교체 화면 표시
            if (titleText != null) // 제목 연결 확인
            { // 조건 시작
                titleText.text = "CHOOSE SLOT TO REPLACE"; // 교체 선택 제목
            } // 조건 끝
            if (levelText != null) // 보상 문구 연결 확인
            { // 조건 시작
                levelText.text = $"New: {candidate.Weapon.DisplayName} ★{candidate.Grade}"; // 선택 무기와 등급 표시
            } // 조건 끝
            if (growthText != null) // 안내 문구 연결 확인
            { // 조건 시작
                growthText.text = "1: PREV   2: SELECT   3: NEXT   ESC: REJECT"; // 선택 및 거절 안내
            } // 조건 끝
            string selected = index >= slots.Count ? "REJECT NEW WEAPON" : $"REPLACE SLOT {index + 1}\n{slots[index].Weapon.DisplayName}  ★{slots[index].Grade}"; // 현재 선택 항목
            SetReplacementButton(candidateButton01, candidateLabel01, "< PREV"); // 이전 항목 버튼
            SetReplacementButton(candidateButton02, candidateLabel02, selected); // 선택 항목 버튼
            SetReplacementButton(candidateButton03, candidateLabel03, "NEXT >"); // 다음 항목 버튼
            BindButtons(); // 버튼 입력 연결
        } // 메서드 끝

        private static void SetReplacementButton(Button button, Text label, string caption) // 슬롯 선택 버튼 설정
        { // 메서드 시작
            WeaponCandidateArtwork.Set(button, label, null); // 이전 무기 후보 이미지 제거
            if (button != null) // 버튼 연결 확인
            { // 조건 시작
                button.gameObject.SetActive(true); // 버튼 표시
                button.interactable = true; // 버튼 선택 허용
            } // 조건 끝
            if (label != null) // 문구 연결 확인
            { // 조건 시작
                label.text = caption; // 버튼 내용 표시
            } // 조건 끝
        } // 메서드 끝

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private void HandleCandidate01()
        {
            CandidateSelected?.Invoke(0);
        }

        private void HandleCandidate02()
        {
            CandidateSelected?.Invoke(1);
        }

        private void HandleCandidate03()
        {
            CandidateSelected?.Invoke(2);
        }

        private void BindButtons()
        {
            if (buttonsBound)
            {
                return;
            }

            if (candidateButton01 != null)
            {
                candidateButton01.onClick.AddListener(
                    HandleCandidate01
                );
            }

            if (candidateButton02 != null)
            {
                candidateButton02.onClick.AddListener(
                    HandleCandidate02
                );
            }

            if (candidateButton03 != null)
            {
                candidateButton03.onClick.AddListener(
                    HandleCandidate03
                );
            }

            buttonsBound = true;
        }

        private void UnbindButtons()
        {
            if (!buttonsBound)
            {
                return;
            }

            if (candidateButton01 != null)
            {
                candidateButton01.onClick.RemoveListener(
                    HandleCandidate01
                );
            }

            if (candidateButton02 != null)
            {
                candidateButton02.onClick.RemoveListener(
                    HandleCandidate02
                );
            }

            if (candidateButton03 != null)
            {
                candidateButton03.onClick.RemoveListener(
                    HandleCandidate03
                );
            }

            buttonsBound = false;
        }

        private static void ConfigureCandidate(
            int candidateIndex,
            Button button,
            Text label,
            IReadOnlyList<WeaponRewardCandidate> candidates
        )
        {
            bool hasCandidate =
                candidates != null &&
                candidateIndex >= 0 &&
                candidateIndex < candidates.Count &&
                candidates[candidateIndex].IsValid;

            WeaponCandidateArtwork.Set(button, label, hasCandidate ? candidates[candidateIndex].Weapon.Artwork : null, hasCandidate ? candidates[candidateIndex].Weapon.ArtworkTint : Color.white); // 실제 속성별 포탑 후보 표시

            if (button != null)
            {
                button.gameObject.SetActive(
                    hasCandidate
                );

                button.interactable =
                    hasCandidate;
            }

            if (!hasCandidate || label == null)
            {
                return;
            }

            WeaponRewardCandidate candidate =
                candidates[candidateIndex];

            label.text =
                BuildCandidateLabel(
                    candidate
                );
        }

        private static string BuildCandidateLabel(
            WeaponRewardCandidate candidate
        )
        {
            if (!candidate.IsValid)
            {
                return "-";
            }

            string stars =
                new string(
                    '★',
                    Mathf.Clamp(
                        candidate.Grade,
                        1,
                        5
                    )
                );

            return
                $"{candidate.Weapon.DisplayName} {stars}\n" +
                $"Attribute: {candidate.Weapon.Attribute}\n" + // 무기 속성 표시
                $"{candidate.Weapon.AttackType}\n" +
                $"DMG {candidate.Weapon.BaseDamage:0.#}";
        }

        private static string BuildGrowthText(
            bool bodyGrew,
            int currentBodyCount,
            int maximumBodyCount,
            bool healthRestored
        )
        {
            int safeCurrent =
                Mathf.Max(
                    0,
                    currentBodyCount
                );

            int safeMaximum =
                Mathf.Max(
                    1,
                    maximumBodyCount
                );

            if (bodyGrew)
            {
                int previous =
                    Mathf.Max(
                        0,
                        safeCurrent - 1
                    );

                return
                    $"Body {previous} → {safeCurrent} / {safeMaximum}\n" +
                    "Choose 1 Weapon";
            }

            if (healthRestored)
            {
                return
                    $"Body MAX {safeMaximum} / {safeMaximum}\n" +
                    "HP FULL RESTORE + Choose 1 Weapon";
            }

            return
                $"Choose 1 Weapon";
        }
    }
}

