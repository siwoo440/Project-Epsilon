using System; // 이벤트 형식
using System.Collections.Generic; // 목록 형식

namespace UnityEngine // 유니티 대체 형식
{
    public class MonoBehaviour // 컴포넌트 기본형
    {
    }
    public sealed class SerializeField : Attribute // 직렬화 표시
    {
    }
    public sealed class DefaultExecutionOrder : Attribute // 실행 순서 표시
    {
        public DefaultExecutionOrder(int order) // 실행 순서 지정
        {
        }
    }
    public sealed class Min : Attribute // 최솟값 표시
    {
        public Min(float value) // 최솟값 지정
        {
        }
    }
    public sealed class Range : Attribute // 범위 표시
    {
        public Range(float minimum, float maximum) // 범위 지정
        {
        }
    }
    public static class Mathf // 수학 함수
    {
        public static int Max(int first, int second) => Math.Max(first, second); // 정수 최댓값
        public static float Clamp01(float value) => Math.Clamp(value, 0f, 1f); // 확률 범위 제한
    }
    public static class Random // 추첨 대체기
    {
        public static int Counter; // 추첨 호출 수
        public static int Range(int minimum, int maximum) // 순차 추첨
        {
            int width = maximum - minimum; // 추첨 범위
            return width <= 0 ? minimum : minimum + Counter++ % width; // 순차 추첨 결과
        }
        public static float value => 0.5f; // 고정 확률
    }
}

namespace UnityEngine.InputSystem // 입력 대체 형식
{
    public sealed class KeyControl // 키 상태
    {
        public bool wasPressedThisFrame; // 현재 프레임 입력
    }
    public sealed class Keyboard // 키보드 상태
    {
        public static Keyboard current; // 현재 키보드
        public KeyControl escapeKey = new KeyControl(); // 취소 키
        public KeyControl digit1Key = new KeyControl(); // 숫자 일
        public KeyControl digit2Key = new KeyControl(); // 숫자 이
        public KeyControl digit3Key = new KeyControl(); // 숫자 삼
        public KeyControl numpad1Key = new KeyControl(); // 숫자패드 일
        public KeyControl numpad2Key = new KeyControl(); // 숫자패드 이
        public KeyControl numpad3Key = new KeyControl(); // 숫자패드 삼
    }
}

namespace ProjectEpsilon.Core // 게임 상태 대체 형식
{
    public sealed class InputBindingManager // 입력 설정 대체기
    {
        public static InputBindingManager Instance; // 현재 입력 설정
        public bool IsRebinding; // 키 재설정 상태
    }
    public sealed class GameManager // 테스트 게임 관리자
    {
        public static GameManager Instance; // 현재 관리자
        public bool IsPlaying = true; // 플레이 상태
        public bool IsPaused; // 일시정지 상태
        public bool IsFinalBossPhase; // 보스전 상태 대체
        public void PauseGame() // 게임 정지
        {
            if (IsPlaying) // 플레이 상태 확인
            {
                IsPlaying = false; // 플레이 중지
                IsPaused = true; // 일시정지 적용
            }
        }
        public void ResumeGame() // 게임 재개
        {
            if (IsPaused) // 일시정지 상태 확인
            {
                IsPaused = false; // 일시정지 해제
                IsPlaying = true; // 플레이 재개
            }
        }
    }
}

namespace ProjectEpsilon.Data // 무기 데이터 대체 형식
{
    public sealed class CharacterData // 시험 캐릭터 데이터
    {
    }
    public sealed class WeaponData // 시험 무기
    {
        public string DisplayName = "Weapon"; // 무기 이름
        public int MaxGrade = 2; // 최대 등급
    }
}

namespace ProjectEpsilon.Player // 플레이어 대체 형식
{
    public static class CharacterRunInitializer // 시험 시작 캐릭터
    {
        public static ProjectEpsilon.Data.CharacterData ActiveCharacter; // 현재 캐릭터
    }
    public sealed class SnakeHealth // 시험 체력
    {
        public int CurrentHealth = 100; // 현재 체력
        public int MaximumHealth = 100; // 최대 체력
        public bool Heal(int amount) // 체력 회복
        {
            if (CurrentHealth >= MaximumHealth) // 최대 체력 확인
            {
                return false; // 회복 불가
            }
            CurrentHealth = Math.Min(MaximumHealth, CurrentHealth + amount); // 체력 증가
            return true; // 회복 성공
        }
    }
    public sealed class SnakeBodyManager // 시험 몸통
    {
        public int CurrentBodyCount = 3; // 현재 몸통 수
        public int MissingRepairableBodyCount; // 복구 가능 수
        public bool TryRepairBody() // 몸통 복구
        {
            if (MissingRepairableBodyCount <= 0) // 복구 가능 수 확인
            {
                return false; // 복구 불가
            }
            CurrentBodyCount++; // 현재 몸통 증가
            MissingRepairableBodyCount--; // 복구 가능 수 감소
            return true; // 복구 성공
        }
    }
}

namespace ProjectEpsilon.Combat // 무기 대체 형식
{
    public sealed class SnakeWeaponSlot // 시험 무기 슬롯
    {
        public ProjectEpsilon.Data.WeaponData Weapon; // 장착 무기
        public int Grade = 1; // 무기 등급
    }
    public sealed class SnakeWeaponManager // 시험 무기 관리자
    {
        public readonly List<SnakeWeaponSlot> Slots = new List<SnakeWeaponSlot>(); // 슬롯 목록
        public int SlotCount => Slots.Count; // 슬롯 수
        public int OccupiedSlotCount // 장착 슬롯 수
        {
            get // 조회 시작
            {
                int count = 0; // 장착 수 초기화
                foreach (SnakeWeaponSlot slot in Slots) // 슬롯 순회
                {
                    if (slot.Weapon != null) // 무기 존재 확인
                    {
                        count++; // 장착 수 증가
                    }
                }
                return count; // 장착 수 반환
            }
        }
        public bool HasCompletedGradeFive(ProjectEpsilon.Data.WeaponData weapon) => false; // 최고 등급 부재
        public bool AcquireWeapon(ProjectEpsilon.Data.WeaponData weapon, int grade) // 빈 슬롯 장착
        {
            foreach (SnakeWeaponSlot slot in Slots) // 슬롯 순회
            {
                if (slot.Weapon == null) // 빈 슬롯 확인
                {
                    slot.Weapon = weapon; // 무기 장착
                    slot.Grade = grade; // 등급 저장
                    return true; // 장착 성공
                }
            }
            return false; // 빈 슬롯 부재
        }
        public bool TryReplaceWeapon(int index, ProjectEpsilon.Data.WeaponData weapon, int grade) // 선택 슬롯 교체
        {
            Slots[index].Weapon = weapon; // 무기 교체
            Slots[index].Grade = grade; // 등급 저장
            return true; // 교체 성공
        }
    }
}

namespace ProjectEpsilon.Progression // 진행 대체 형식
{
    public sealed class MetaProgressionController // 시험 해금 상태
    {
        public static MetaProgressionController Instance; // 현재 성장 관리자
        public bool IsTurretUnlocked(ProjectEpsilon.Data.WeaponData weapon) => true; // 포탑 해금 상태 대체
    }
    public sealed class SnakeExperience // 시험 경험치
    {
        public bool IsLevelUpPending; // 레벨업 대기 상태
    }
    public sealed class SnakeLevelUpController // 시험 레벨업
    {
        public bool IsPresentingLevelUp; // 레벨업 화면 상태
    }
    public sealed class WeaponRewardEntry // 시험 무기 후보
    {
        public ProjectEpsilon.Data.WeaponData Weapon; // 무기 데이터
        public bool Unlocked = true; // 해금 상태
    }
    public sealed class WeaponRewardPool // 시험 무기 풀
    {
        public readonly List<WeaponRewardEntry> Entries = new List<WeaponRewardEntry>(); // 무기 목록
        public static int SelectWeaponIndex(IReadOnlyList<ProjectEpsilon.Data.WeaponData> available, ProjectEpsilon.Combat.SnakeWeaponManager weapons, ProjectEpsilon.Data.CharacterData character) // 기본 캐릭터 추첨 대체
        {
            return UnityEngine.Random.Range(0, available.Count); // 균등 추첨 대체
        }
    }
    public readonly struct WeaponRewardCandidate // 시험 무기 보상
    {
        public readonly ProjectEpsilon.Data.WeaponData Weapon; // 무기 데이터
        public readonly int Grade; // 무기 등급
        public bool IsValid => Weapon != null; // 무기 후보 유효성
        public WeaponRewardCandidate(ProjectEpsilon.Data.WeaponData weapon, int grade) // 보상 구성
        {
            Weapon = weapon; // 무기 저장
            Grade = grade; // 등급 저장
        }
    }
    public sealed class SnakeWeaponMergeController // 시험 합성 화면
    {
        public event Action<bool> MergeClosed; // 합성 결과 이벤트
        public bool IsMergeOpen; // 합성 화면 상태
        public bool CanMerge; // 합성 가능 여부
        public bool HasMergeCandidates() => CanMerge; // 합성 후보 조회
        public bool OpenMergePanel() // 합성 화면 열기
        {
            IsMergeOpen = CanMerge; // 화면 상태 저장
            return IsMergeOpen; // 열기 결과 반환
        }
        public void CloseMergePanel(bool merged = false) // 합성 화면 닫기
        {
            if (!IsMergeOpen) // 열린 화면 확인
            {
                return; // 중복 종료 방지
            }
            IsMergeOpen = false; // 화면 닫기
            MergeClosed?.Invoke(merged); // 결과 알림
        }
    }
}

namespace ProjectEpsilon.UI // 보상 화면 대체 형식
{
    public sealed class SettingsMenuController // 설정 화면 대체기
    {
        public bool IsOpen; // 설정 화면 상태
    }
    public sealed class LevelUpPanelController // 시험 보상 화면
    {
        public event Action<int> CandidateSelected; // 후보 선택 이벤트
        public bool Visible; // 화면 표시 상태
        public bool ReplacementVisible; // 슬롯 교체 상태
        public ProjectEpsilon.Progression.WeaponRewardCandidate LastReplacementCandidate; // 마지막 교체 후보
        public void ShowEliteRewards(string[] labels, bool[] available) // 보상 후보 표시
        {
            Visible = true; // 화면 표시
            ReplacementVisible = false; // 교체 화면 종료
        }
        public void ShowReplacementSelection(IReadOnlyList<ProjectEpsilon.Combat.SnakeWeaponSlot> slots, ProjectEpsilon.Progression.WeaponRewardCandidate candidate, int index) // 교체 후보 표시
        {
            Visible = true; // 화면 표시
            ReplacementVisible = true; // 교체 화면 표시
            LastReplacementCandidate = candidate; // 교체 무기 저장
        }
        public void Hide() // 화면 숨기기
        {
            Visible = false; // 화면 숨김
            ReplacementVisible = false; // 교체 화면 숨김
        }
        public void Click(int index) // 시험 버튼 선택
        {
            CandidateSelected?.Invoke(index); // 선택 이벤트 전달
        }
    }
}
