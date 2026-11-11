using System; // 기본 형식
using System.Collections.Generic; // 목록 형식
using ProjectEpsilon.UI; // 설정 화면

namespace UnityEngine // 유니티 대체 형식
{
    public class MonoBehaviour // 컴포넌트 대체
    {
        public static T FindFirstObjectByType<T>() where T : class => SettingsMenuController.Active as T; // 설정 화면 검색
        public T GetComponent<T>() where T : class => null; // 보상 제어기 부재
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
    public static class Application // 실행 환경
    {
        public static bool isPlaying = true; // 플레이 상태
    }
    public static class Time // 프레임 정보
    {
        public static int frameCount; // 현재 프레임
    }
    public static class Debug // 진단 출력
    {
        public static void LogWarning(string message) // 경고 무시
        {
        }
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
    public sealed class GameManager // 게임 상태 관리자
    {
        public static GameManager Instance; // 현재 관리자
        public bool IsPlaying = true; // 진행 상태
        public bool IsPaused; // 정지 상태
        public void PauseGame() // 게임 정지
        {
            IsPlaying = false; // 진행 종료
            IsPaused = true; // 정지 적용
        }
        public void ResumeGame() // 게임 재개
        {
            IsPlaying = true; // 진행 복구
            IsPaused = false; // 정지 해제
        }
    }
    public sealed class InputBindingManager // 입력 설정 관리자
    {
        public static InputBindingManager Instance; // 현재 관리자
        public bool IsRebinding; // 키 재설정 상태
    }
}

namespace ProjectEpsilon.Data // 무기 데이터 대체
{
    public sealed class WeaponData // 무기 정보
    {
        public int MaxGrade = 5; // 최대 등급
    }
}

namespace ProjectEpsilon.Combat // 무기 대체 형식
{
    public sealed class SnakeWeaponSlot // 무기 슬롯
    {
        public ProjectEpsilon.Data.WeaponData Weapon; // 장착 무기
        public int Grade = 1; // 장착 등급
        public bool IsEmpty => Weapon == null; // 빈 슬롯 여부
    }
    public sealed class SnakeWeaponManager // 무기 관리자
    {
        public readonly List<SnakeWeaponSlot> Slots = new List<SnakeWeaponSlot>(); // 슬롯 목록
        public int SlotCount = 2; // 전체 슬롯 수
        public int OccupiedSlotCount; // 장착 슬롯 수
        public int AcquisitionCount; // 장착 횟수
        public bool AcquireWeapon(ProjectEpsilon.Data.WeaponData weapon, int grade) // 무기 획득
        {
            AcquisitionCount++; // 장착 횟수 증가
            return true; // 장착 성공
        }
        public bool TryReplaceWeapon(int slot, ProjectEpsilon.Data.WeaponData weapon, int grade) => true; // 무기 교체 결과
    }
}

namespace ProjectEpsilon.Player // 플레이어 대체 형식
{
    public sealed class SnakeBodyManager // 몸통 관리자
    {
        public int CurrentBodyCount = 3; // 현재 몸통 수
        public int MaximumBodyCount = 20; // 최대 몸통 수
        public bool TryGainBodyFromLevelUp() => true; // 몸통 성장 결과
    }
    public sealed class SnakeHealth // 체력 관리자
    {
        public event Action BodyDepleted; // 몸통 소진 알림
        public void Deplete() => BodyDepleted?.Invoke(); // 소진 이벤트 발생
        public void ResetHealth() // 체력 복원
        {
        }
    }
}

namespace ProjectEpsilon.Progression // 진행 대체 형식
{
    public sealed class SnakeWeaponMergeController // 보상 합성 대체
    {
        public event Action<bool> MergeClosed; // 합성 종료 알림
        public bool IsMergeOpen; // 합성 진행 여부
        public bool isActiveAndEnabled = true; // 합성 활성 상태
        public bool OpenRewardMerge(WeaponRewardCandidate reward) => false; // 합성 시작 결과
        public void CloseMergePanel(bool merged = false) // 합성 취소
        {
        }
    }
    public sealed class EliteRewardController // 엘리트 보상 상태
    {
        public bool YieldPauseToLevelUp() => false; // 정지 인계 부재
    }
    public sealed class SnakeExperience // 경험치 상태
    {
        public event Action<int> LevelUpRequested; // 레벨업 이벤트
        public int CompletionCount; // 완료 횟수
        public void Request(int level) // 레벨업 요청
        {
            LevelUpRequested?.Invoke(level); // 요청 이벤트 전달
        }
        public bool CompletePendingLevelUp() // 레벨업 완료
        {
            CompletionCount++; // 완료 횟수 증가
            return false; // 추가 대기 부재
        }
    }
    public readonly struct WeaponRewardCandidate // 무기 보상 후보
    {
        public readonly ProjectEpsilon.Data.WeaponData Weapon; // 무기 정보
        public readonly int Grade; // 무기 등급
        public bool IsValid => Weapon != null; // 후보 유효성
        public WeaponRewardCandidate(ProjectEpsilon.Data.WeaponData weapon, int grade) // 후보 구성
        {
            Weapon = weapon; // 무기 저장
            Grade = grade; // 등급 저장
        }
    }
    public sealed class WeaponRewardPool // 무기 보상 풀
    {
        public List<WeaponRewardCandidate> BuildCandidates(int level, ProjectEpsilon.Combat.SnakeWeaponManager weapons, int count) // 보상 후보 생성
        {
            List<WeaponRewardCandidate> candidates = new List<WeaponRewardCandidate>(); // 후보 목록
            candidates.Add(new WeaponRewardCandidate(new ProjectEpsilon.Data.WeaponData(), 1)); // 시험 후보 추가
            return candidates; // 후보 반환
        }
    }
}

namespace ProjectEpsilon.UI // 화면 대체 형식
{
    public sealed class SettingsMenuController // 설정 화면
    {
        public static SettingsMenuController Active; // 현재 설정
        public bool IsOpen; // 화면 상태
    }
    public sealed class LevelUpPanelController // 레벨업 화면
    {
        public event Action<int> CandidateSelected; // 후보 선택 이벤트
        public void Click(int index) // 마우스 선택
        {
            CandidateSelected?.Invoke(index); // 선택 이벤트 전달
        }
        public void Show(int level, bool grew, int current, int maximum, bool restored, IReadOnlyList<ProjectEpsilon.Progression.WeaponRewardCandidate> candidates) // 후보 표시
        {
        }
        public void ShowRewardAction(ProjectEpsilon.Progression.WeaponRewardCandidate candidate, bool canMerge) // 보상 방식 표시
        {
        }
        public void ShowReplacementSelection(IReadOnlyList<ProjectEpsilon.Combat.SnakeWeaponSlot> slots, ProjectEpsilon.Progression.WeaponRewardCandidate candidate, int index) // 교체 화면 표시
        {
        }
        public void Hide() // 화면 숨김
        {
        }
    }
}
