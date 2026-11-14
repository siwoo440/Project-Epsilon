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
        [Range(0f, 1f)] [SerializeField] private float startingBackgroundOpacity = 0.65f; // 시작 선택 맵 배경 불투명도 시험값

        [Header("Weapon Candidates")] // 포탑·후보 설정
        [SerializeField] private Button candidateButton01; // 필드·버튼 설정
        [SerializeField] private Button candidateButton02; // 필드·버튼 설정
        [SerializeField] private Button candidateButton03; // 필드·버튼 설정

        [SerializeField] private Text candidateLabel01; // 필드·문구 설정
        [SerializeField] private Text candidateLabel02; // 필드·문구 설정
        [SerializeField] private Text candidateLabel03; // 필드·문구 설정

        private bool buttonsBound; // 대상 구성 선언
        private Image startingBackground; // 실제 시작 선택 패널 배경
        private float originalBackgroundOpacity; // 실제 기존 보상 배경 불투명도
        private bool startingBackgroundChanged; // 시작 선택 배경 변경 상태

        public event Action<int> CandidateSelected; // 이벤트·동작 구성 선언

        public void ConfigureGameplayViewport() // 분리된 전투 영역의 보상 카드 크기
        {
            RectTransform rect = transform as RectTransform; // 실제 보상 패널 영역
            if (rect == null) // 실제 UI 영역 확인
            {
                return; // 잘못된 화면 구성 생략
            }
            rect.anchorMin = new Vector2(0f, rect.anchorMin.y); // 실제 전투 영역 왼쪽 기준
            rect.anchorMax = new Vector2(1f, rect.anchorMax.y); // 실제 전투 영역 오른쪽 기준
            rect.offsetMin = new Vector2(0f, rect.offsetMin.y); // 실제 왼쪽 여백 제거
            rect.offsetMax = new Vector2(0f, rect.offsetMax.y); // 실제 오른쪽 여백 제거
            SetCardHorizontalAnchors(candidateButton01, 0.04f, 0.32f); // 실제 첫 카드 가로 범위
            SetCardHorizontalAnchors(candidateButton02, 0.36f, 0.64f); // 실제 가운데 카드 가로 범위
            SetCardHorizontalAnchors(candidateButton03, 0.68f, 0.96f); // 실제 마지막 카드 가로 범위
        }

        private static void SetCardHorizontalAnchors(Button button, float left, float right) // 실제 카드의 가로 위치 설정
        {
            RectTransform rect = button == null ? null : button.transform as RectTransform; // 실제 카드 UI 영역
            if (rect == null) // 실제 카드 확인
            {
                return; // 누락 카드 설정 생략
            }
            rect.anchorMin = new Vector2(left, rect.anchorMin.y); // 실제 카드 왼쪽 기준
            rect.anchorMax = new Vector2(right, rect.anchorMax.y); // 실제 카드 오른쪽 기준
            rect.offsetMin = new Vector2(0f, rect.offsetMin.y); // 실제 카드 왼쪽 여백 제거
            rect.offsetMax = new Vector2(0f, rect.offsetMax.y); // 실제 카드 오른쪽 여백 제거
        }

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
            ApplyCardSkin(); // 실제 보상 카드 프레임 적용
            BindButtons(); // 연결 기능 실행
        }

        private void ApplyCardSkin() // 공용 보상 카드 디자인
        { // 함수 시작
            ProjectUiSkin.ApplyButton(candidateButton01, true); // 첫 카드 프레임
            ProjectUiSkin.ApplyButton(candidateButton02, true); // 둘째 카드 프레임
            ProjectUiSkin.ApplyButton(candidateButton03, true); // 셋째 카드 프레임
        } // 함수 종료

        private void OnDisable() // 비활성 기능 선언
        {
            RestoreStartingBackground(); // 기존 보상 배경 복구
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

            ApplyCardSkin(); // 새로 연결된 보상 카드 프레임 적용

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
            RestoreStartingBackground(); // 일반 레벨업의 기존 배경 복구
            gameObject.SetActive(true); // 게임·객체 기능 실행

            if (titleText != null) // 문구 조건 확인
            {
                titleText.text = "레벨 상승!"; // 문구·레벨 값 설정
            }

            if (levelText != null) // 레벨·문구 조건 확인
            {
                levelText.text = // 레벨·문구 값 설정
                    $"레벨 {Mathf.Max(1, level)}"; // 최대·레벨 기능 실행
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
                growthText.text += "   R: 다시 선택"; // 실제 재선택 키 표시
            }
        }

        public void ShowStartingWeapons(IReadOnlyList<WeaponRewardCandidate> candidates) // 시작 무기 후보 표시
        {
            gameObject.SetActive(true); // 시작 후보 화면 표시
            ApplyStartingBackground(); // 실제 맵이 보이는 시작 선택 배경
            if (titleText != null) // 제목 연결 확인
            {
                titleText.text = "시작 포탑 선택"; // 시작 무기 선택 제목
            }
            if (levelText != null) // 부제 연결 확인
            {
                levelText.text = "전투 전에 하나를 선택하세요"; // 시작 전 선택 안내
            }
            if (growthText != null) // 조작 안내 연결 확인
            {
                growthText.text = "1 / 2 / 3: 선택"; // 선택 키 안내
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
                levelText.text = $"{character.DisplayName} | 몸통 {character.StartingBodyCount} / 체력 {character.BodyMaxHealth:0}"; // 실제 캐릭터 시작 능력치
            }
            if (growthText != null) // 실제 조작 안내 확인
            {
                growthText.text = "← / →: 캐릭터    1 / 2 / 3: 포탑"; // 캐릭터와 무기 선택 안내
            }
        }

        public void SetStartingMap(ProjectEpsilon.World.PrototypeMapProfile map) // 시작 화면 맵 정보 표시
        {
            if (growthText != null && map != null) // 실제 안내와 맵 확인
            {
                growthText.text = SceneFrontendContext.UsesSeparateScenes ? $"맵: {map.DisplayName}    1 / 2 / 3: 포탑 선택" : $"← / →: 캐릭터    ↑ / ↓: 맵 ({map.DisplayName})    1 / 2 / 3: 포탑"; // 실제 화면 방식별 시작 조작 안내
            }
        }

        public void ShowEliteRewards(string[] labels, bool[] available, IReadOnlyList<int> kinds = null) // 엘리트 보상 후보 표시
        {
            RestoreStartingBackground(); // 정예 보상의 기존 배경 복구
            gameObject.SetActive(true); // 선택 화면 표시
            if (titleText != null) // 제목 연결 확인
            {
                titleText.text = "정예 보상"; // 보상 제목 표시
            }
            if (levelText != null) // 부제 연결 확인
            {
                levelText.text = "하나를 선택하세요"; // 단일 선택 안내
            }
            if (growthText != null) // 안내 문구 연결 확인
            {
                growthText.text = "1 / 2 / 3: 선택"; // 입력 안내
            }
            ConfigureEliteButton(candidateButton01, candidateLabel01, labels, available, kinds, 0); // 첫 후보 표시
            ConfigureEliteButton(candidateButton02, candidateLabel02, labels, available, kinds, 1); // 둘째 후보 표시
            ConfigureEliteButton(candidateButton03, candidateLabel03, labels, available, kinds, 2); // 셋째 후보 표시
            BindButtons(); // 버튼 입력 연결
        }

        private static void ConfigureEliteButton(Button button, Text label, string[] labels, bool[] available, IReadOnlyList<int> kinds, int index) // 보상 버튼 구성
        {
            bool exists = labels != null && available != null && index < labels.Length && index < available.Length; // 후보 존재 확인
            Sprite icon = exists && kinds != null && index < kinds.Count ? ProjectUiSkin.EliteRewardIcon(kinds[index]) : null; // 실제 보상 종류 이미지
            WeaponCandidateArtwork.Set(button, label, icon); // 기존 후보 이미지 표시 경로 재사용
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
            RestoreStartingBackground(); // 보상 처리의 기존 배경 복구
            gameObject.SetActive(true); // 레벨업 화면 표시
            if (titleText != null) // 제목 연결 확인
            {
                titleText.text = "포탑 처리 선택"; // 처리 방식 제목
            }
            if (levelText != null) // 무기 문구 연결 확인
            {
                levelText.text = $"{candidate.Weapon.DisplayName} ★{candidate.Grade}"; // 선택한 보상 무기
            }
            if (growthText != null) // 안내 문구 연결 확인
            {
                growthText.text = "1 / 2 / 3: 선택"; // 선택 키 안내
            }
            SetReplacementButton(candidateButton01, candidateLabel01, "장착 / 교체"); // 장착 및 교체 선택
            SetReplacementButton(candidateButton02, candidateLabel02, canMerge ? "보상 합성" : "합성 (재료 없음)"); // 합성 가능 상태 안내
            if (candidateButton02 != null) // 합성 버튼 연결 확인
            {
                candidateButton02.interactable = canMerge; // 재료가 있을 때만 합성 허용
            }
            SetReplacementButton(candidateButton03, candidateLabel03, "뒤로"); // 기존 후보 재선택
            BindButtons(); // 버튼 이벤트 연결
        }

        public void ShowReplacementSelection(IReadOnlyList<SnakeWeaponSlot> slots, WeaponRewardCandidate candidate, int index) // 교체 슬롯 화면 표시
        {
            RestoreStartingBackground(); // 교체 선택의 기존 배경 복구
            gameObject.SetActive(true); // 교체 화면 표시
            if (titleText != null) // 제목 연결 확인
            {
                titleText.text = "교체할 몸통 선택"; // 교체 선택 제목
            }
            if (levelText != null) // 보상 문구 연결 확인
            {
                levelText.text = $"새 포탑: {candidate.Weapon.DisplayName} ★{candidate.Grade}"; // 선택 무기와 등급 표시
            }
            if (growthText != null) // 안내 문구 연결 확인
            {
                growthText.text = "1: 이전   2: 선택   3: 다음   이스케이프: 거절"; // 선택 및 거절 안내
            }
            string selected = index >= slots.Count ? "새 포탑 거절" : $"교체할 몸통 {index + 1}\n{slots[index].Weapon.DisplayName}  ★{slots[index].Grade}"; // 현재 선택 항목
            SetReplacementButton(candidateButton01, candidateLabel01, "< 이전"); // 이전 항목 버튼
            SetReplacementButton(candidateButton02, candidateLabel02, selected); // 선택 항목 버튼
            SetReplacementButton(candidateButton03, candidateLabel03, "다음 >"); // 다음 항목 버튼
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
            RestoreStartingBackground(); // 시작 선택의 기존 배경 복구
            if (gameObject.activeSelf) // 게임·객체 조건 확인
            {
                gameObject.SetActive(false); // 게임·객체 기능 실행
            }
        }

        private void ApplyStartingBackground() // 시작 선택의 맵 배경 표시
        { // 함수 시작
            if (!SceneFrontendContext.UsesSeparateScenes) // 실제 맵에 진입한 선택 화면 확인
            { // 조건 시작
                return; // 기존 단일 씬 배경 유지
            } // 조건 종료
            if (!startingBackgroundChanged) // 실제 기존 배경 저장 확인
            { // 조건 시작
                startingBackground = GetComponent<Image>(); // 실제 공용 보상 패널 배경
                if (startingBackground == null) // 실제 배경 이미지 확인
                { // 조건 시작
                    return; // 이미지 없는 화면 생략
                } // 조건 종료
                originalBackgroundOpacity = startingBackground.color.a; // 실제 기존 보상 배경 저장
                startingBackgroundChanged = true; // 실제 배경 변경 기록
            } // 조건 종료
            Color color = startingBackground.color; // 실제 배경 색상
            color.a = Mathf.Min(originalBackgroundOpacity, Mathf.Clamp01(startingBackgroundOpacity)); // 기존 값보다 어둡지 않은 시험 불투명도
            startingBackground.color = color; // 실제 선택 맵 배경 표시
        } // 함수 종료

        private void RestoreStartingBackground() // 시작 선택 전의 보상 배경 복원
        { // 함수 시작
            if (!startingBackgroundChanged) // 실제 변경 상태 확인
            { // 조건 시작
                return; // 복원할 배경 없음
            } // 조건 종료
            if (startingBackground != null) // 실제 배경 이미지 생존 확인
            { // 조건 시작
                Color color = startingBackground.color; // 실제 현재 배경 색상
                color.a = originalBackgroundOpacity; // 실제 기존 보상 불투명도
                startingBackground.color = color; // 실제 기존 보상 배경 복원
            } // 조건 종료
            startingBackground = null; // 이전 배경 참조 제거
            startingBackgroundChanged = false; // 실제 변경 상태 해제
        } // 함수 종료

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

            WeaponCandidateArtwork.Set(button, label, hasCandidate ? candidates[candidateIndex].Weapon.Icon : null, hasCandidate ? candidates[candidateIndex].Weapon.ArtworkDisplayTint : Color.white); // 포탑 아이콘 후보 표시

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
                    $"몸통 {previous} → {safeCurrent} / {safeMaximum}\n" + // 몸통·이전 값 처리
                    "포탑 하나를 선택하세요"; // 포탑 경로 또는 문구 값
            }

            if (healthRestored) // 체력 조건 확인
            {
                return // 대상 결과 반환
                    $"몸통 최대 {safeMaximum} / {safeMaximum}\n" + // 몸통·최대 처리
                    "체력 전체 회복 + 포탑 하나 선택"; // 체력·포탑 경로 또는 문구 값
            }

            return // 대상 결과 반환
                $"포탑 하나를 선택하세요"; // 포탑 처리
        }
    }
}

