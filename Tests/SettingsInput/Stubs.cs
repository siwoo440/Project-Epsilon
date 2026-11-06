using System; // 기본 형식
using System.Collections.Generic; // 목록 형식
using ProjectEpsilon.UI; // 설정 화면

namespace UnityEngine // 유니티 대체 형식
{ // 영역 시작
    public class MonoBehaviour // 컴포넌트 대체
    { // 클래스 시작
        public static T FindFirstObjectByType<T>() where T : class => SettingsMenuController.Active as T; // 설정 화면 검색
    } // 클래스 끝
    public sealed class SerializeField : Attribute // 직렬화 표시
    { // 클래스 시작
    } // 클래스 끝
    public sealed class DefaultExecutionOrder : Attribute // 실행 순서 표시
    { // 클래스 시작
        public DefaultExecutionOrder(int order) // 실행 순서 지정
        { // 생성자 시작
        } // 생성자 끝
    } // 클래스 끝
    public sealed class HeaderAttribute : Attribute // 제목 표시
    { // 클래스 시작
        public HeaderAttribute(string title) // 제목 지정
        { // 생성자 시작
        } // 생성자 끝
    } // 클래스 끝
    public sealed class RangeAttribute : Attribute // 범위 표시
    { // 클래스 시작
        public RangeAttribute(float min, float max) // 범위 지정
        { // 생성자 시작
        } // 생성자 끝
    } // 클래스 끝
    public static class Application // 실행 환경
    { // 클래스 시작
        public static bool isPlaying = true; // 플레이 상태
    } // 클래스 끝
    public static class Mathf // 수학 도구
    { // 클래스 시작
        public static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max); // 정수 범위 제한
        public static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max); // 실수 범위 제한
    } // 클래스 끝
} // 영역 끝

namespace UnityEngine.InputSystem // 입력 대체 형식
{ // 영역 시작
    public sealed class KeyControl // 키 상태
    { // 클래스 시작
        public bool wasPressedThisFrame; // 현재 프레임 입력
    } // 클래스 끝
    public sealed class Keyboard // 키보드 상태
    { // 클래스 시작
        public static Keyboard current; // 현재 키보드
        public KeyControl escapeKey = new KeyControl(); // 취소 키
        public KeyControl eKey = new KeyControl(); // 합성 키
        public KeyControl digit1Key = new KeyControl(); // 숫자 일
        public KeyControl digit2Key = new KeyControl(); // 숫자 이
        public KeyControl digit3Key = new KeyControl(); // 숫자 삼
        public KeyControl numpad1Key = new KeyControl(); // 숫자패드 일
        public KeyControl numpad2Key = new KeyControl(); // 숫자패드 이
        public KeyControl numpad3Key = new KeyControl(); // 숫자패드 삼
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Core // 게임 상태 대체 형식
{ // 영역 시작
    public sealed class GameManager // 게임 상태 관리자
    { // 클래스 시작
        public static GameManager Instance; // 현재 관리자
        public bool IsPlaying = true; // 진행 상태
        public bool IsPaused; // 정지 상태
        public void PauseGame() // 게임 정지
        { // 메서드 시작
            IsPlaying = false; // 진행 종료
            IsPaused = true; // 정지 적용
        } // 메서드 끝
        public void ResumeGame() // 게임 재개
        { // 메서드 시작
            IsPlaying = true; // 진행 복구
            IsPaused = false; // 정지 해제
        } // 메서드 끝
        public void SetGameplayTimeScale(float value) // 시간 배율 대체
        { // 메서드 시작
        } // 메서드 끝
    } // 클래스 끝
    public sealed class InputBindingManager // 입력 설정 관리자
    { // 클래스 시작
        public static InputBindingManager Instance; // 현재 관리자
        public bool IsRebinding; // 키 재설정 상태
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Data // 무기 데이터 대체
{ // 영역 시작
    public sealed class WeaponData // 무기 정보
    { // 클래스 시작
        public int MaxGrade = 5; // 최대 등급
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Combat // 무기 대체 형식
{ // 영역 시작
    public enum WeaponMergeUpgrade // 합성 강화 종류
    { // 열거 시작
        None, // 강화 없음
        Damage, // 공격력
        AttackSpeed, // 공격 속도
        Range // 범위
    } // 열거 끝
    public sealed class SnakeWeaponSlot // 무기 슬롯
    { // 클래스 시작
        public ProjectEpsilon.Data.WeaponData Weapon; // 무기 정보
        public int Grade = 1; // 무기 등급
        public bool IsEmpty => Weapon == null; // 빈 슬롯 여부
    } // 클래스 끝
    public sealed class SnakeWeaponManager // 무기 관리자
    { // 클래스 시작
        public readonly List<SnakeWeaponSlot> Slots = new List<SnakeWeaponSlot>(); // 슬롯 목록
        public bool TryMergeSlots(int first, int second, WeaponMergeUpgrade upgrade) => true; // 합성 결과
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Player // 플레이어 대체 형식
{ // 영역 시작
    public sealed class SnakeMovement // 이동 제어기
    { // 클래스 시작
        public void SetMergeMovementMode(bool active, float multiplier) // 합성 이동 설정
        { // 메서드 시작
        } // 메서드 끝
    } // 클래스 끝
    public sealed class SnakeStamina // 가속 제어기
    { // 클래스 시작
        public void SetBoostBlocked(bool active) // 가속 차단 설정
        { // 메서드 시작
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Progression // 진행 대체 형식
{ // 영역 시작
    public sealed class SnakeExperience // 경험치 상태
    { // 클래스 시작
        public bool IsLevelUpPending; // 레벨업 대기
        public event Action<int> LevelUpRequested; // 레벨업 이벤트
    } // 클래스 끝
    public sealed class SnakeLevelUpController // 레벨업 제어기
    { // 클래스 시작
        public bool IsPresentingLevelUp; // 레벨업 화면 상태
        public bool ConsumedEscapeThisFrame; // 취소 입력 소비 상태
    } // 클래스 끝
    public readonly struct WeaponMergeCandidate // 합성 후보
    { // 구조체 시작
        public readonly ProjectEpsilon.Data.WeaponData Weapon; // 무기 정보
        public readonly int CurrentGrade; // 현재 등급
        public readonly int FirstSlotIndex; // 첫 슬롯
        public readonly int SecondSlotIndex; // 둘째 슬롯
        public bool IsValid => Weapon != null; // 후보 유효성
        public WeaponMergeCandidate(ProjectEpsilon.Data.WeaponData weapon, int grade, int first, int second) // 후보 구성
        { // 생성자 시작
            Weapon = weapon; // 무기 저장
            CurrentGrade = grade; // 등급 저장
            FirstSlotIndex = first; // 첫 슬롯 저장
            SecondSlotIndex = second; // 둘째 슬롯 저장
        } // 생성자 끝
    } // 구조체 끝
} // 영역 끝

namespace ProjectEpsilon.UI // 화면 대체 형식
{ // 영역 시작
    public sealed class SettingsMenuController // 설정 화면
    { // 클래스 시작
        public static SettingsMenuController Active; // 현재 설정
        public bool IsOpen; // 화면 상태
    } // 클래스 끝
    public sealed class WeaponMergePanelController // 합성 화면
    { // 클래스 시작
        public event Action<int> CandidateSelected; // 후보 선택 이벤트
        public event Action CloseRequested; // 닫기 이벤트
        public void Show(IReadOnlyList<ProjectEpsilon.Progression.WeaponMergeCandidate> candidates) // 후보 표시
        { // 메서드 시작
        } // 메서드 끝
        public void ShowUpgradeSelection(ProjectEpsilon.Progression.WeaponMergeCandidate candidate) // 강화 표시
        { // 메서드 시작
        } // 메서드 끝
        public void Hide() // 화면 숨김
        { // 메서드 시작
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
