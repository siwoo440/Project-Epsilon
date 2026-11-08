using ProjectEpsilon.Combat; // 실제 무기 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 캐릭터 데이터
using ProjectEpsilon.Progression; // 시작 무기 선택
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Player // 플레이어 영역
{ // 영역 시작
    [DefaultExecutionOrder(100)] // 몸통과 무기 초기화 이후 실행
    public sealed class CharacterRunInitializer : MonoBehaviour // 실제 런 시작 캐릭터 적용
    { // 클래스 시작
        [SerializeField] private CharacterData defaultCharacter; // 기본 균형형 데이터
        [SerializeField] private CharacterData[] availableCharacters; // 시작 화면 캐릭터 목록
        public static CharacterData SelectedCharacter { get; private set; } // 다음 런 선택 캐릭터
        public static CharacterData ActiveCharacter { get; private set; } // 현재 런 적용 캐릭터
        public static float MoveSpeedMultiplier => ActiveCharacter == null ? 1f : Mathf.Clamp(ActiveCharacter.MoveSpeedMultiplier, 0.9f, 1.1f); // 안전한 캐릭터 이동 배율
        public static float TurnSpeedMultiplier => ActiveCharacter == null ? 1f : Mathf.Clamp(ActiveCharacter.TurnPowerMultiplier, 0.85f, 1.15f); // 안전한 캐릭터 회전 배율
        public static float StaminaRecoveryMultiplier => ActiveCharacter != null && (ActiveCharacter.Passive == CharacterPassive.BalancedRecovery || ActiveCharacter.Passive == CharacterPassive.MobileRecovery) ? ActiveCharacter.PassiveMultiplier : 1f; // 회복형 캐릭터 스태미나 배율
        public static float DamageReceivedMultiplier => ActiveCharacter != null && ActiveCharacter.Passive == CharacterPassive.HeavyDefense ? ActiveCharacter.PassiveMultiplier : 1f; // 중장형 받는 피해 배율
        public static float HealingMultiplier => ActiveCharacter != null && ActiveCharacter.Passive == CharacterPassive.HolySpecialization ? ActiveCharacter.PassiveMultiplier : 1f; // 신성형 회복 효율 배율
        public static float HolyDamageMultiplier => ActiveCharacter != null && ActiveCharacter.Passive == CharacterPassive.HolySpecialization ? ActiveCharacter.SecondaryMultiplier : 1f; // 신성형 무기 피해 배율
        public static float DarkKillHealMultiplier => ActiveCharacter != null && ActiveCharacter.Passive == CharacterPassive.DarkSpecialization ? ActiveCharacter.PassiveMultiplier : 1f; // 암흑형 저주 처치 회복 배율

        public static void SelectForNextRun(CharacterData data) // 다음 런 캐릭터 선택
        { // 메서드 시작
            SelectedCharacter = data; // 다음 시작 선택 저장
        } // 메서드 끝
        public bool CycleCharacter(int direction) // 시작 화면의 이전 또는 다음 캐릭터
        { // 메서드 시작
            if (direction == 0 || availableCharacters == null || availableCharacters.Length == 0 || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Starting) // 실제 시작과 후보 상태 확인
            { // 조건 시작
                return false; // 유효하지 않은 순환 입력 차단
            } // 조건 끝
            int current = 0; // 현재 캐릭터 기본 위치
            for (int index = 0; index < availableCharacters.Length; index++) // 실제 캐릭터 목록 순회
            { // 반복 시작
                if (availableCharacters[index] == ActiveCharacter) // 현재 적용 캐릭터 확인
                { // 조건 시작
                    current = index; // 현재 위치 저장
                    break; // 추가 검색 종료
                } // 조건 끝
            } // 반복 끝
            for (int offset = 1; offset <= availableCharacters.Length; offset++) // 다음 유효 캐릭터 검색
            { // 반복 시작
                int step = direction > 0 ? offset : -offset; // 실제 순환 방향
                int next = (current + step + availableCharacters.Length) % availableCharacters.Length; // 경계를 넘는 순환 위치
                CharacterData candidate = availableCharacters[next]; // 실제 후보 캐릭터
                if (candidate != null && TryApply(candidate)) // 실제 시작 캐릭터 적용 확인
                { // 조건 시작
                    SelectForNextRun(candidate); // 다음 런 선택 상태 저장
                    return true; // 실제 캐릭터 변경 완료
                } // 조건 끝
            } // 반복 끝
            return false; // 사용 가능한 캐릭터 부재
        } // 메서드 끝

        private void Start() // 실제 런 시작 시 캐릭터 적용
        { // 메서드 시작
            CharacterData chosen = SelectedCharacter != null ? SelectedCharacter : defaultCharacter; // 선택 또는 기본 캐릭터
            if (!TryApply(chosen) && chosen != defaultCharacter) // 선택 데이터 실패 확인
            { // 조건 시작
                TryApply(defaultCharacter); // 기본 캐릭터 대체 적용
            } // 조건 끝
        } // 메서드 끝
        private void OnDestroy() // 현재 런 종료 처리
        { // 메서드 시작
            ActiveCharacter = null; // 이전 런 능력치 제거
        } // 메서드 끝
        public bool TryApply(CharacterData data) // 시작 상태의 실제 캐릭터 적용
        { // 메서드 시작
            if (data == null || data.StartingWeapons == null || data.StartingWeapons.Weapons == null || data.StartingWeapons.Weapons.Length != 3 || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Starting) // 데이터와 실제 시작 상태 확인
            { // 조건 시작
                return false; // 잘못된 데이터와 진행 중 변경 차단
            } // 조건 끝
            WeaponData[] options = data.StartingWeapons.Weapons; // 실제 시작 무기 후보
            if (options[0] == null || options[1] == null || options[2] == null || options[0] == options[1] || options[0] == options[2] || options[1] == options[2]) // 후보 누락과 중복 확인
            { // 조건 시작
                return false; // 불완전한 캐릭터 데이터 차단
            } // 조건 끝
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            StartingWeaponSelection selection = GetComponent<StartingWeaponSelection>(); // 실제 시작 선택 화면
            if (body == null || weapons == null || health == null || selection == null || !selection.CanChangeStartingOptions) // 필수 구성과 미확정 시작 상태 확인
            { // 조건 시작
                return false; // 불완전한 씬 구성 차단
            } // 조건 끝
            ActiveCharacter = data; // 실제 런 패시브와 이동 배율 적용
            body.SetStartingBodyCount(data.StartingBodyCount); // 실제 시작 몸통과 성장 최대 적용
            health.SetStartingMaximumHealth(Mathf.RoundToInt(data.BodyMaxHealth)); // 실제 시작 공용 체력 적용
            weapons.ClearStartingLoadout(); // 이전 시작 무기 제거
            selection.SetStartingOptions(data.StartingWeapons); // 캐릭터별 실제 선택 후보 적용
            return true; // 실제 시작 적용 완료
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
