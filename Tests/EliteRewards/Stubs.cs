using System; // 이벤트 형식
using System.Collections.Generic; // 목록 형식

namespace UnityEngine // 유니티 대체 형식
{ // 영역 시작
    public class MonoBehaviour // 컴포넌트 기본형
    { // 클래스 시작
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
    public sealed class Min : Attribute // 최솟값 표시
    { // 클래스 시작
        public Min(float value) // 최솟값 지정
        { // 생성자 시작
        } // 생성자 끝
    } // 클래스 끝
    public sealed class Range : Attribute // 범위 표시
    { // 클래스 시작
        public Range(float minimum, float maximum) // 범위 지정
        { // 생성자 시작
        } // 생성자 끝
    } // 클래스 끝
    public static class Mathf // 수학 함수
    { // 클래스 시작
        public static int Max(int first, int second) => Math.Max(first, second); // 정수 최댓값
        public static float Clamp01(float value) => Math.Clamp(value, 0f, 1f); // 확률 범위 제한
    } // 클래스 끝
    public static class Random // 추첨 대체기
    { // 클래스 시작
        public static int Counter; // 추첨 호출 수
        public static int Range(int minimum, int maximum) // 순차 추첨
        { // 메서드 시작
            int width = maximum - minimum; // 추첨 범위
            return width <= 0 ? minimum : minimum + Counter++ % width; // 순차 추첨 결과
        } // 메서드 끝
        public static float value => 0.5f; // 고정 확률
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
    public sealed class InputBindingManager // 입력 설정 대체기
    { // 클래스 시작
        public static InputBindingManager Instance; // 현재 입력 설정
        public bool IsRebinding; // 키 재설정 상태
    } // 클래스 끝
    public sealed class GameManager // 테스트 게임 관리자
    { // 클래스 시작
        public static GameManager Instance; // 현재 관리자
        public bool IsPlaying = true; // 플레이 상태
        public bool IsPaused; // 일시정지 상태
        public bool IsFinalBossPhase; // 보스전 상태 대체
        public void PauseGame() // 게임 정지
        { // 메서드 시작
            if (IsPlaying) // 플레이 상태 확인
            { // 조건 시작
                IsPlaying = false; // 플레이 중지
                IsPaused = true; // 일시정지 적용
            } // 조건 끝
        } // 메서드 끝
        public void ResumeGame() // 게임 재개
        { // 메서드 시작
            if (IsPaused) // 일시정지 상태 확인
            { // 조건 시작
                IsPaused = false; // 일시정지 해제
                IsPlaying = true; // 플레이 재개
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Data // 무기 데이터 대체 형식
{ // 영역 시작
    public sealed class WeaponData // 시험 무기
    { // 클래스 시작
        public string DisplayName = "Weapon"; // 무기 이름
        public int MaxGrade = 2; // 최대 등급
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Player // 플레이어 대체 형식
{ // 영역 시작
    public sealed class SnakeHealth // 시험 체력
    { // 클래스 시작
        public int CurrentHealth = 100; // 현재 체력
        public int MaximumHealth = 100; // 최대 체력
        public bool Heal(int amount) // 체력 회복
        { // 메서드 시작
            if (CurrentHealth >= MaximumHealth) // 최대 체력 확인
            { // 조건 시작
                return false; // 회복 불가
            } // 조건 끝
            CurrentHealth = Math.Min(MaximumHealth, CurrentHealth + amount); // 체력 증가
            return true; // 회복 성공
        } // 메서드 끝
    } // 클래스 끝
    public sealed class SnakeBodyManager // 시험 몸통
    { // 클래스 시작
        public int CurrentBodyCount = 3; // 현재 몸통 수
        public int MissingRepairableBodyCount; // 복구 가능 수
        public bool TryRepairBody() // 몸통 복구
        { // 메서드 시작
            if (MissingRepairableBodyCount <= 0) // 복구 가능 수 확인
            { // 조건 시작
                return false; // 복구 불가
            } // 조건 끝
            CurrentBodyCount++; // 현재 몸통 증가
            MissingRepairableBodyCount--; // 복구 가능 수 감소
            return true; // 복구 성공
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Combat // 무기 대체 형식
{ // 영역 시작
    public sealed class SnakeWeaponSlot // 시험 무기 슬롯
    { // 클래스 시작
        public ProjectEpsilon.Data.WeaponData Weapon; // 장착 무기
        public int Grade = 1; // 무기 등급
    } // 클래스 끝
    public sealed class SnakeWeaponManager // 시험 무기 관리자
    { // 클래스 시작
        public readonly List<SnakeWeaponSlot> Slots = new List<SnakeWeaponSlot>(); // 슬롯 목록
        public int SlotCount => Slots.Count; // 슬롯 수
        public int OccupiedSlotCount // 장착 슬롯 수
        { // 속성 시작
            get // 조회 시작
            { // 조회 범위 시작
                int count = 0; // 장착 수 초기화
                foreach (SnakeWeaponSlot slot in Slots) // 슬롯 순회
                { // 반복 시작
                    if (slot.Weapon != null) // 무기 존재 확인
                    { // 조건 시작
                        count++; // 장착 수 증가
                    } // 조건 끝
                } // 반복 끝
                return count; // 장착 수 반환
            } // 조회 범위 끝
        } // 속성 끝
        public bool HasCompletedGradeFive(ProjectEpsilon.Data.WeaponData weapon) => false; // 최고 등급 부재
        public bool AcquireWeapon(ProjectEpsilon.Data.WeaponData weapon, int grade) // 빈 슬롯 장착
        { // 메서드 시작
            foreach (SnakeWeaponSlot slot in Slots) // 슬롯 순회
            { // 반복 시작
                if (slot.Weapon == null) // 빈 슬롯 확인
                { // 조건 시작
                    slot.Weapon = weapon; // 무기 장착
                    slot.Grade = grade; // 등급 저장
                    return true; // 장착 성공
                } // 조건 끝
            } // 반복 끝
            return false; // 빈 슬롯 부재
        } // 메서드 끝
        public bool TryReplaceWeapon(int index, ProjectEpsilon.Data.WeaponData weapon, int grade) // 선택 슬롯 교체
        { // 메서드 시작
            Slots[index].Weapon = weapon; // 무기 교체
            Slots[index].Grade = grade; // 등급 저장
            return true; // 교체 성공
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Progression // 진행 대체 형식
{ // 영역 시작
    public sealed class SnakeExperience // 시험 경험치
    { // 클래스 시작
        public bool IsLevelUpPending; // 레벨업 대기 상태
    } // 클래스 끝
    public sealed class SnakeLevelUpController // 시험 레벨업
    { // 클래스 시작
        public bool IsPresentingLevelUp; // 레벨업 화면 상태
    } // 클래스 끝
    public sealed class WeaponRewardEntry // 시험 무기 후보
    { // 클래스 시작
        public ProjectEpsilon.Data.WeaponData Weapon; // 무기 데이터
        public bool Unlocked = true; // 해금 상태
    } // 클래스 끝
    public sealed class WeaponRewardPool // 시험 무기 풀
    { // 클래스 시작
        public readonly List<WeaponRewardEntry> Entries = new List<WeaponRewardEntry>(); // 무기 목록
    } // 클래스 끝
    public readonly struct WeaponRewardCandidate // 시험 무기 보상
    { // 구조체 시작
        public readonly ProjectEpsilon.Data.WeaponData Weapon; // 무기 데이터
        public readonly int Grade; // 무기 등급
        public bool IsValid => Weapon != null; // 무기 후보 유효성
        public WeaponRewardCandidate(ProjectEpsilon.Data.WeaponData weapon, int grade) // 보상 구성
        { // 생성자 시작
            Weapon = weapon; // 무기 저장
            Grade = grade; // 등급 저장
        } // 생성자 끝
    } // 구조체 끝
    public sealed class SnakeWeaponMergeController // 시험 합성 화면
    { // 클래스 시작
        public event Action<bool> MergeClosed; // 합성 결과 이벤트
        public bool IsMergeOpen; // 합성 화면 상태
        public bool CanMerge; // 합성 가능 여부
        public bool HasMergeCandidates() => CanMerge; // 합성 후보 조회
        public bool OpenMergePanel() // 합성 화면 열기
        { // 메서드 시작
            IsMergeOpen = CanMerge; // 화면 상태 저장
            return IsMergeOpen; // 열기 결과 반환
        } // 메서드 끝
        public void CloseMergePanel(bool merged = false) // 합성 화면 닫기
        { // 메서드 시작
            if (!IsMergeOpen) // 열린 화면 확인
            { // 조건 시작
                return; // 중복 종료 방지
            } // 조건 끝
            IsMergeOpen = false; // 화면 닫기
            MergeClosed?.Invoke(merged); // 결과 알림
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.UI // 보상 화면 대체 형식
{ // 영역 시작
    public sealed class SettingsMenuController // 설정 화면 대체기
    { // 클래스 시작
        public bool IsOpen; // 설정 화면 상태
    } // 클래스 끝
    public sealed class LevelUpPanelController // 시험 보상 화면
    { // 클래스 시작
        public event Action<int> CandidateSelected; // 후보 선택 이벤트
        public bool Visible; // 화면 표시 상태
        public bool ReplacementVisible; // 슬롯 교체 상태
        public ProjectEpsilon.Progression.WeaponRewardCandidate LastReplacementCandidate; // 마지막 교체 후보
        public void ShowEliteRewards(string[] labels, bool[] available) // 보상 후보 표시
        { // 메서드 시작
            Visible = true; // 화면 표시
            ReplacementVisible = false; // 교체 화면 종료
        } // 메서드 끝
        public void ShowReplacementSelection(IReadOnlyList<ProjectEpsilon.Combat.SnakeWeaponSlot> slots, ProjectEpsilon.Progression.WeaponRewardCandidate candidate, int index) // 교체 후보 표시
        { // 메서드 시작
            Visible = true; // 화면 표시
            ReplacementVisible = true; // 교체 화면 표시
            LastReplacementCandidate = candidate; // 교체 무기 저장
        } // 메서드 끝
        public void Hide() // 화면 숨기기
        { // 메서드 시작
            Visible = false; // 화면 숨김
            ReplacementVisible = false; // 교체 화면 숨김
        } // 메서드 끝
        public void Click(int index) // 시험 버튼 선택
        { // 메서드 시작
            CandidateSelected?.Invoke(index); // 선택 이벤트 전달
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
