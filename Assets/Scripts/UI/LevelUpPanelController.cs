using System; // C# 기본 기능 연결
using System.Collections.Generic; // C# 기본 기능 연결
using ProjectEpsilon.Progression; // 대상 기능 연결
using ProjectEpsilon.Data; // 시작 캐릭터 데이터
using ProjectEpsilon.Combat; // 무기 슬롯 사용
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.UI; // Unity 엔진 기능 연결

namespace ProjectEpsilon.UI // 프로젝트 기능 영역
{
    public sealed class LevelUpPanelController : MonoBehaviour // 레벨·화면 자료 구조
    {
        [SerializeField] private Text titleText; // 필드·문구 설정
        [SerializeField] private Text levelText; // 필드·문구 설정
        [SerializeField] private Text growthText; // 필드·문구 설정

        [Header("Weapon Candidates")] // 포탑·후보 설정
        [SerializeField] private Button candidateButton01; // 필드·버튼 설정
        [SerializeField] private Button candidateButton02; // 필드·버튼 설정
        [SerializeField] private Button candidateButton03; // 필드·버튼 설정

        [SerializeField] private Text candidateLabel01; // 필드·문구 설정
        [SerializeField] private Text candidateLabel02; // 필드·문구 설정
        [SerializeField] private Text candidateLabel03; // 필드·문구 설정

        private bool buttonsBound; // 대상 구성 선언

        public event Action<int> CandidateSelected; // 이벤트·동작 구성 선언

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
            Text level, // 문구·레벨 항목 전달
            Text growth, // 문구 항목 전달
            Button button01, // 버튼 항목 전달
            Text label01, // 문구 항목 전달
            Button button02, // 버튼 항목 전달
            Text label02, // 문구 항목 전달
            Button button03, // 버튼 항목 전달
            Text label03 // 문구 처리
        ) // 앞선 항목 종료
        {
            UnbindButtons(); // 대상 기능 실행

            titleText = title; // 문구 값 설정
            levelText = level; // 레벨·문구 값 설정
            growthText = growth; // 문구 값 설정

            candidateButton01 = button01; // 후보·버튼 값 설정
            candidateLabel01 = label01; // 후보·문구 값 설정

            candidateButton02 = button02; // 후보·버튼 값 설정
            candidateLabel02 = label02; // 후보·문구 값 설정

            candidateButton03 = button03; // 후보·버튼 값 설정
            candidateLabel03 = label03; // 후보·문구 값 설정

            if (Application.isPlaying) // 대상 조건 확인
            {
                BindButtons(); // 연결 기능 실행
            }
        }

        public void Show( // 표시 호출 시작
            int level, // 레벨 항목 전달
            bool bodyGrew, // 몸통 항목 전달
            int currentBodyCount, // 현재·몸통 항목 전달
            int maximumBodyCount, // 최대·몸통 항목 전달
            bool healthRestored, // 체력 항목 전달
            IReadOnlyList<WeaponRewardCandidate> candidates // 목록·포탑 처리
        ) // 앞선 항목 종료
        {
            gameObject.SetActive(true); // 게임·객체 기능 실행

            if (titleText != null) // 문구 조건 확인
            {
                titleText.text = "LEVEL UP!"; // 문구·레벨 값 설정
            }

            if (levelText != null) // 레벨·문구 조건 확인
            {
                levelText.text = // 레벨·문구 값 설정
                    $"Lv. {Mathf.Max(1, level)}"; // 최대·레벨 기능 실행
            }

            if (growthText != null) // 문구 조건 확인
            {
                growthText.text = // 문구 값 설정
                    BuildGrowthText( // 구성·문구 호출 시작
                        bodyGrew, // 몸통 항목 전달
                        currentBodyCount, // 현재·몸통 항목 전달
                        maximumBodyCount, // 최대·몸통 항목 전달
                        healthRestored // 체력 처리
                    ); // 앞선 항목 종료
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

        public void SetRerollAvailable(bool available) // 런당 재선택 안내
        {
            if (available && growthText != null) // 실제 안내 표시 확인
            {
                growthText.text += "   R: REROLL"; // 실제 재선택 키 표시
            }
        }

        public void ShowStartingWeapons(IReadOnlyList<WeaponRewardCandidate> candidates) // 시작 무기 후보 표시
        {
            gameObject.SetActive(true); // 시작 후보 화면 표시
            if (titleText != null) // 제목 연결 확인
            {
                titleText.text = "CHOOSE STARTING WEAPON"; // 시작 무기 선택 제목
            }
            if (levelText != null) // 부제 연결 확인
            {
                levelText.text = "CHOOSE ONE BEFORE THE RUN"; // 시작 전 선택 안내
            }
            if (growthText != null) // 조작 안내 연결 확인
            {
                growthText.text = "1 / 2 / 3: SELECT"; // 선택 키 안내
            }
            ConfigureCandidate(0, candidateButton01, candidateLabel01, candidates); // 첫 후보 표시
            ConfigureCandidate(1, candidateButton02, candidateLabel02, candidates); // 둘째 후보 표시
            ConfigureCandidate(2, candidateButton03, candidateLabel03, candidates); // 셋째 후보 표시
            BindButtons(); // 후보 버튼 연결
        }

        public void SetStartingCharacter(CharacterData character) // 시작 화면 캐릭터 정보 표시
        {
            if (character == null) // 캐릭터 데이터 확인
            {
                return; // 기존 시작 안내 유지
            }
            if (levelText != null) // 실제 캐릭터 문구 확인
            {
                levelText.text = $"{character.DisplayName} | BODY {character.StartingBodyCount} / HP {character.BodyMaxHealth:0}"; // 실제 캐릭터 시작 능력치
            }
            if (growthText != null) // 실제 조작 안내 확인
            {
                growthText.text = "← / →: CHARACTER    1 / 2 / 3: WEAPON"; // 캐릭터와 무기 선택 안내
            }
        }

        public void SetStartingMap(ProjectEpsilon.World.PrototypeMapProfile map) // 시작 화면 맵 정보 표시
        {
            if (growthText != null && map != null) // 실제 안내와 맵 확인
            {
                growthText.text = $"← / →: CHARACTER    ↑ / ↓: MAP ({map.DisplayName})    1 / 2 / 3: WEAPON"; // 시작 맵과 조작 표시
            }
        }

        public void ShowEliteRewards(string[] labels, bool[] available) // 엘리트 보상 후보 표시
        {
            gameObject.SetActive(true); // 선택 화면 표시
            if (titleText != null) // 제목 연결 확인
            {
                titleText.text = "ELITE REWARD"; // 보상 제목 표시
            }
            if (levelText != null) // 부제 연결 확인
            {
                levelText.text = "CHOOSE ONE"; // 단일 선택 안내
            }
            if (growthText != null) // 안내 문구 연결 확인
            {
                growthText.text = "1 / 2 / 3: SELECT"; // 입력 안내
            }
            ConfigureEliteButton(candidateButton01, candidateLabel01, labels, available, 0); // 첫 후보 표시
            ConfigureEliteButton(candidateButton02, candidateLabel02, labels, available, 1); // 둘째 후보 표시
            ConfigureEliteButton(candidateButton03, candidateLabel03, labels, available, 2); // 셋째 후보 표시
            BindButtons(); // 버튼 입력 연결
        }

        private static void ConfigureEliteButton(Button button, Text label, string[] labels, bool[] available, int index) // 보상 버튼 구성
        {
            WeaponCandidateArtwork.Set(button, label, null); // 이전 무기 후보 이미지 제거
            bool exists = labels != null && available != null && index < labels.Length && index < available.Length; // 후보 존재 확인
            if (button != null) // 버튼 연결 확인
            {
                button.gameObject.SetActive(exists); // 후보 표시 여부
                button.interactable = exists && available[index]; // 적용 가능 후보만 선택
            }
            if (label != null && exists) // 문구와 후보 확인
            {
                label.text = labels[index]; // 후보 내용 표시
            }
        }

        public void ShowRewardAction(WeaponRewardCandidate candidate, bool canMerge) // 보상 처리 방식 선택
        {
            gameObject.SetActive(true); // 레벨업 화면 표시
            if (titleText != null) // 제목 연결 확인
            {
                titleText.text = "CHOOSE WEAPON ACTION"; // 처리 방식 제목
            }
            if (levelText != null) // 무기 문구 연결 확인
            {
                levelText.text = $"{candidate.Weapon.DisplayName} ★{candidate.Grade}"; // 선택한 보상 무기
            }
            if (growthText != null) // 안내 문구 연결 확인
            {
                growthText.text = "1 / 2 / 3: SELECT"; // 선택 키 안내
            }
            SetReplacementButton(candidateButton01, candidateLabel01, "EQUIP / REPLACE"); // 장착 및 교체 선택
            SetReplacementButton(candidateButton02, candidateLabel02, canMerge ? "MERGE REWARD" : "MERGE (NO MATERIAL)"); // 합성 가능 상태 안내
            if (candidateButton02 != null) // 합성 버튼 연결 확인
            {
                candidateButton02.interactable = canMerge; // 재료가 있을 때만 합성 허용
            }
            SetReplacementButton(candidateButton03, candidateLabel03, "BACK"); // 기존 후보 재선택
            BindButtons(); // 버튼 이벤트 연결
        }

        public void ShowReplacementSelection(IReadOnlyList<SnakeWeaponSlot> slots, WeaponRewardCandidate candidate, int index) // 교체 슬롯 화면 표시
        {
            gameObject.SetActive(true); // 교체 화면 표시
            if (titleText != null) // 제목 연결 확인
            {
                titleText.text = "CHOOSE SLOT TO REPLACE"; // 교체 선택 제목
            }
            if (levelText != null) // 보상 문구 연결 확인
            {
                levelText.text = $"새 포탑: {candidate.Weapon.DisplayName} ★{candidate.Grade}"; // 선택 무기와 등급 표시
            }
            if (growthText != null) // 안내 문구 연결 확인
            {
                growthText.text = "1: PREV   2: SELECT   3: NEXT   ESC: REJECT"; // 선택 및 거절 안내
            }
            string selected = index >= slots.Count ? "REJECT NEW WEAPON" : $"교체할 몸통 {index + 1}\n{slots[index].Weapon.DisplayName}  ★{slots[index].Grade}"; // 현재 선택 항목
            SetReplacementButton(candidateButton01, candidateLabel01, "< PREV"); // 이전 항목 버튼
            SetReplacementButton(candidateButton02, candidateLabel02, selected); // 선택 항목 버튼
            SetReplacementButton(candidateButton03, candidateLabel03, "NEXT >"); // 다음 항목 버튼
            BindButtons(); // 버튼 입력 연결
        }

        private static void SetReplacementButton(Button button, Text label, string caption) // 슬롯 선택 버튼 설정
        {
            WeaponCandidateArtwork.Set(button, label, null); // 이전 무기 후보 이미지 제거
            if (button != null) // 버튼 연결 확인
            {
                button.gameObject.SetActive(true); // 버튼 표시
                button.interactable = true; // 버튼 선택 허용
            }
            if (label != null) // 문구 연결 확인
            {
                label.text = caption; // 버튼 내용 표시
            }
        }

        public void Hide() // 숨김 기능 선언
        {
            if (gameObject.activeSelf) // 게임·객체 조건 확인
            {
                gameObject.SetActive(false); // 게임·객체 기능 실행
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

            buttonsBound = false; // 대상 값 설정
        }

        private static void ConfigureCandidate( // 구성·후보 호출 시작
            int candidateIndex, // 후보·순서 항목 전달
            Button button, // 버튼 항목 전달
            Text label, // 문구 항목 전달
            IReadOnlyList<WeaponRewardCandidate> candidates // 목록·포탑 처리
        ) // 앞선 항목 종료
        {
            bool hasCandidate = // 존재 여부·후보 값 설정
                candidates != null && // 후보 처리
                candidateIndex >= 0 && // 후보·순서 처리
                candidateIndex < candidates.Count && // 후보·순서 처리
                candidates[candidateIndex].IsValid; // 후보·순서 처리

            WeaponCandidateArtwork.Set(button, label, hasCandidate ? candidates[candidateIndex].Weapon.Artwork : null, hasCandidate ? candidates[candidateIndex].Weapon.ArtworkTint : Color.white); // 실제 속성별 포탑 후보 표시

            if (button != null) // 버튼 조건 확인
            {
                button.gameObject.SetActive( // 버튼·게임 호출 시작
                    hasCandidate // 존재 여부·후보 처리
                ); // 앞선 항목 종료

                button.interactable = // 버튼 값 설정
                    hasCandidate; // 존재 여부·후보 처리
            }

            if (!hasCandidate || label == null) // 존재 여부·후보 조건 확인
            {
                return; // 현재 기능 종료
            }

            WeaponRewardCandidate candidate = // 포탑·보상 값 설정
                candidates[candidateIndex]; // 후보·순서 처리

            label.text = // 문구 값 설정
                BuildCandidateLabel( // 구성·후보 호출 시작
                    candidate // 후보 처리
                ); // 앞선 항목 종료
        }

        private static string BuildCandidateLabel( // 구성·후보 호출 시작
            WeaponRewardCandidate candidate // 포탑·보상 처리
        ) // 앞선 항목 종료
        {
            if (!candidate.IsValid) // 후보 조건 확인
            {
                return "-"; // 대상 결과 반환
            }

            string stars = // 대상 값 설정
                new string( // 대상 객체 생성
                    '★', // 대상 항목 전달
                    Mathf.Clamp( // 대상 호출 시작
                        candidate.Grade, // 후보·등급 항목 전달
                        1, // 대상 항목 전달
                        5 // 대상 처리
                    ) // 앞선 항목 종료
                ); // 앞선 항목 종료

            return // 대상 결과 반환
                $"{candidate.Weapon.DisplayName} {stars}\n" + // 후보·포탑 처리
                $"속성: {WeaponDisplayNames.Attribute(candidate.Weapon.Attribute)}\n" + // 무기 속성 표시
                $"{WeaponDisplayNames.AttackType(candidate.Weapon.AttackType)}\n" + // 후보·포탑 처리
                $"공격력 {candidate.Weapon.BaseDamage:0.#}"; // 후보·포탑 처리
        }

        private static string BuildGrowthText( // 구성·문구 호출 시작
            bool bodyGrew, // 몸통 항목 전달
            int currentBodyCount, // 현재·몸통 항목 전달
            int maximumBodyCount, // 최대·몸통 항목 전달
            bool healthRestored // 체력 처리
        ) // 앞선 항목 종료
        {
            int safeCurrent = // 현재 값 설정
                Mathf.Max( // 최대 호출 시작
                    0, // 대상 항목 전달
                    currentBodyCount // 현재·몸통 처리
                ); // 앞선 항목 종료

            int safeMaximum = // 최대 값 설정
                Mathf.Max( // 최대 호출 시작
                    1, // 대상 항목 전달
                    maximumBodyCount // 최대·몸통 처리
                ); // 앞선 항목 종료

            if (bodyGrew) // 몸통 조건 확인
            {
                int previous = // 이전 값 값 설정
                    Mathf.Max( // 최대 호출 시작
                        0, // 대상 항목 전달
                        safeCurrent - 1 // 현재 처리
                    ); // 앞선 항목 종료

                return // 대상 결과 반환
                    $"Body {previous} → {safeCurrent} / {safeMaximum}\n" + // 몸통·이전 값 처리
                    "Choose 1 Weapon"; // 포탑 경로 또는 문구 값
            }

            if (healthRestored) // 체력 조건 확인
            {
                return // 대상 결과 반환
                    $"Body MAX {safeMaximum} / {safeMaximum}\n" + // 몸통·최대 처리
                    "HP FULL RESTORE + Choose 1 Weapon"; // 체력·포탑 경로 또는 문구 값
            }

            return // 대상 결과 반환
                $"Choose 1 Weapon"; // 포탑 처리
        }
    }
}

