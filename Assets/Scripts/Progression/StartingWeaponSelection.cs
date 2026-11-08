using System.Collections.Generic; // 시작 후보 목록
using ProjectEpsilon.Combat; // 무기 관리자
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Player; // 시작 캐릭터 선택
using ProjectEpsilon.UI; // 시작 후보 화면
using UnityEngine; // 유니티 구성
using UnityEngine.InputSystem; // 키보드 입력

namespace ProjectEpsilon.Progression // 게임 진행 영역
{ // 영역 시작
    [DefaultExecutionOrder(200)] // 기존 선택 UI 초기화 이후 실행
    public sealed class StartingWeaponSelection : MonoBehaviour // 시작 무기 선택
    { // 클래스 시작
        [SerializeField] private SnakeWeaponManager weaponManager; // 첫 몸통 무기 관리자
        [SerializeField] private LevelUpPanelController panel; // 공용 세 후보 화면
        [SerializeField] private StartingWeaponOptions options; // 시작 무기 데이터
        [SerializeField] private SettingsMenuController settingsMenu; // 설정 화면
        private readonly List<WeaponRewardCandidate> candidates = new List<WeaponRewardCandidate>(); // 표시할 무기 목록
        private bool initialized; // 시작 초기화 여부
        private bool subscribed; // 버튼 구독 여부
        private bool selecting; // 선택 진행 여부
        private bool selected; // 무기 확정 여부
        public bool IsSelecting => selecting; // 시작 선택 상태 조회
        public bool CanChangeStartingOptions => !selected && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Starting; // 시작 후보 변경 가능 여부

        public bool SetStartingOptions(StartingWeaponOptions data) // 캐릭터별 시작 후보 적용
        { // 메서드 시작
            if (data == null || !CanChangeStartingOptions) // 선택과 시작 상태 확인
            { // 조건 시작
                return false; // 진행 중 후보 변경 방지
            } // 조건 끝
            options = data; // 실제 캐릭터 후보 저장
            if (initialized && isActiveAndEnabled) // 이미 표시된 시작 화면 확인
            { // 조건 시작
                ShowIfStarting(); // 실제 후보 화면 갱신
            } // 조건 끝
            return true; // 시작 후보 적용 완료
        } // 메서드 끝

        public void Configure(SnakeWeaponManager weapons, LevelUpPanelController selectionPanel, StartingWeaponOptions data) // 시작 선택 구성
        { // 메서드 시작
            Unsubscribe(); // 기존 버튼 연결 해제
            if (selecting && panel != null) // 기존 선택 화면 확인
            { // 조건 시작
                panel.Hide(); // 이전 선택 화면 숨김
            } // 조건 끝
            selecting = false; // 이전 선택 비활성
            weaponManager = weapons; // 무기 관리자 저장
            panel = selectionPanel; // 후보 화면 저장
            options = data; // 후보 데이터 저장
            if (Application.isPlaying && isActiveAndEnabled) // 플레이 및 선택기 활성 확인
            { // 조건 시작
                Subscribe(); // 버튼 연결
                if (initialized && !selected) // 기존 선택 대기 확인
                { // 조건 시작
                    ShowIfStarting(); // 기존 선택 화면 갱신
                } // 조건 끝
            } // 조건 끝
        } // 메서드 끝

        private void OnEnable() // 선택기 활성 처리
        { // 메서드 시작
            if (!Application.isPlaying) // 플레이 상태 확인
            { // 조건 시작
                return; // 편집기 구독 방지
            } // 조건 끝
            Subscribe(); // 버튼 구독
            if (initialized && !selected) // 이전 선택 대기 확인
            { // 조건 시작
                ShowIfStarting(); // 시작 선택 복구
            } // 조건 끝
        } // 메서드 끝

        private void Start() // 시작 후보 표시
        { // 메서드 시작
            initialized = true; // 초기화 완료 기록
            Subscribe(); // 버튼 구독 보완
            ShowIfStarting(); // 시작 상태 후보 표시
        } // 메서드 끝

        private void OnDisable() // 선택기 비활성 처리
        { // 메서드 시작
            Unsubscribe(); // 버튼 구독 해제
            selecting = false; // 비활성 중 입력 차단
            if (!selected && panel != null) // 미확정 선택 화면 확인
            { // 조건 시작
                panel.Hide(); // 시작 화면 숨김
            } // 조건 끝
        } // 메서드 끝

        private void Update() // 시작 선택 키 입력
        { // 메서드 시작
            if (!selecting || IsInputBlocked()) // 선택 및 설정 상태 확인
            { // 조건 시작
                return; // 입력 무시
            } // 조건 끝
            Keyboard keyboard = Keyboard.current; // 현재 키보드
            if (keyboard == null) // 키보드 부재 확인
            { // 조건 시작
                return; // 키 입력 생략
            } // 조건 끝
            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame) // 이전 또는 다음 캐릭터 입력
            { // 조건 시작
                CharacterRunInitializer initializer = GetComponent<CharacterRunInitializer>(); // 실제 시작 캐릭터 처리기
                initializer?.CycleCharacter(keyboard.rightArrowKey.wasPressedThisFrame ? 1 : -1); // 실제 캐릭터와 후보 변경
                return; // 같은 프레임 무기 선택 차단
            } // 조건 끝
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) // 첫 후보 입력
            { // 조건 시작
                HandleSelected(0); // 첫 무기 선택
            } // 조건 끝
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) // 둘째 후보 입력
            { // 조건 시작
                HandleSelected(1); // 둘째 무기 선택
            } // 조건 끝
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) // 셋째 후보 입력
            { // 조건 시작
                HandleSelected(2); // 셋째 무기 선택
            } // 조건 끝
        } // 메서드 끝

        private void ShowIfStarting() // 시작 상태에 후보 표시
        { // 메서드 시작
            GameManager game = GameManager.Instance; // 현재 게임 관리자
            if (game == null || game.CurrentState != GameState.Starting || selected) // 시작 상태 확인
            { // 조건 시작
                return; // 다른 상태 무시
            } // 조건 끝
            selecting = false; // 후보 검증 중 입력 차단
            candidates.Clear(); // 이전 후보 제거
            if (panel != null) // 기존 선택 화면 확인
            { // 조건 시작
                panel.Hide(); // 기존 후보 화면 숨김
            } // 조건 끝
            if (weaponManager == null || panel == null || options == null || options.Weapons == null || options.Weapons.Length != 3) // 필수 구성 및 후보 수 확인
            { // 조건 시작
                Debug.LogError("[Project Epsilon] 시작 무기 후보 3종과 선택 화면을 확인하세요."); // 구성 오류 알림
                return; // 시작 상태 유지
            } // 조건 끝
            for (int index = 0; index < options.Weapons.Length; index++) // 후보 데이터 순회
            { // 반복 시작
                WeaponData weapon = options.Weapons[index]; // 후보 무기 조회
                if (weapon == null) // 후보 누락 확인
                { // 조건 시작
                    candidates.Clear(); // 불완전 후보 제거
                    Debug.LogError("[Project Epsilon] 시작 무기 후보가 비어 있습니다."); // 누락 오류 알림
                    return; // 시작 상태 유지
                } // 조건 끝
                for (int previous = 0; previous < index; previous++) // 이전 후보 순회
                { // 반복 시작
                    if (options.Weapons[previous] == weapon) // 중복 무기 확인
                    { // 조건 시작
                        candidates.Clear(); // 잘못된 후보 제거
                        Debug.LogError("[Project Epsilon] 시작 무기 후보가 중복되었습니다."); // 중복 오류 알림
                        return; // 시작 상태 유지
                    } // 조건 끝
                } // 반복 끝
                candidates.Add(new WeaponRewardCandidate(weapon, 1)); // 기본 등급 후보 추가
            } // 반복 끝
            selecting = true; // 시작 선택 활성
            panel.ShowStartingWeapons(candidates); // 세 후보 표시
            panel.SetStartingCharacter(CharacterRunInitializer.ActiveCharacter); // 실제 캐릭터와 선택 안내 표시
        } // 메서드 끝

        private bool IsInputBlocked() // 설정 입력 우선 확인
        { // 메서드 시작
            if (InputBindingManager.Instance != null && InputBindingManager.Instance.IsRebinding) // 입력 재설정 확인
            { // 조건 시작
                return true; // 시작 선택 차단
            } // 조건 끝
            if (settingsMenu == null) // 설정 화면 연결 확인
            { // 조건 시작
                settingsMenu = FindFirstObjectByType<SettingsMenuController>(); // 설정 화면 검색
            } // 조건 끝
            return settingsMenu != null && settingsMenu.IsOpen; // 설정 화면 우선 여부
        } // 메서드 끝

        private void HandleSelected(int index) // 시작 무기 확정
        { // 메서드 시작
            GameManager game = GameManager.Instance; // 현재 게임 관리자
            if (!selecting || selected || IsInputBlocked() || game == null || game.CurrentState != GameState.Starting || index < 0 || index >= candidates.Count) // 선택 가능 상태 확인
            { // 조건 시작
                return; // 잘못된 선택 무시
            } // 조건 끝
            if (weaponManager == null || weaponManager.SlotCount <= 0 || !weaponManager.TryEquipAt(0, candidates[index].Weapon, 1)) // 첫 몸통 장착 확인
            { // 조건 시작
                Debug.LogError("[Project Epsilon] 시작 무기를 첫 몸통에 장착하지 못했습니다."); // 장착 오류 알림
                return; // 시작 상태 유지
            } // 조건 끝
            selected = true; // 중복 확정 방지
            selecting = false; // 선택 상태 종료
            panel.Hide(); // 시작 화면 숨김
            game.StartGame(); // 전투 시작
        } // 메서드 끝

        private void Subscribe() // 후보 버튼 구독
        { // 메서드 시작
            if (subscribed || panel == null) // 기존 구독 및 화면 확인
            { // 조건 시작
                return; // 중복 연결 방지
            } // 조건 끝
            panel.CandidateSelected += HandleSelected; // 후보 선택 연결
            subscribed = true; // 구독 상태 기록
        } // 메서드 끝

        private void Unsubscribe() // 후보 버튼 구독 해제
        { // 메서드 시작
            if (!subscribed) // 기존 구독 여부
            { // 조건 시작
                return; // 해제 생략
            } // 조건 끝
            if (panel != null) // 화면 객체 확인
            { // 조건 시작
                panel.CandidateSelected -= HandleSelected; // 후보 선택 해제
            } // 조건 끝
            subscribed = false; // 구독 상태 해제
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
