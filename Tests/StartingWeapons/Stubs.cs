using System; // 기본 형식
using System.Collections.Generic; // 목록 형식
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Progression; // 보상 형식

namespace UnityEngine // 유니티 대체 형식
{ // 영역 시작
    public enum FindObjectsInactive // 비활성 객체 검색 설정
    { // 열거 시작
        Include // 비활성 객체 포함
    } // 열거 끝
    public class Object // 유니티 객체 대체
    { // 클래스 시작
        public static void Destroy(object value) // 객체 제거 대체
        { // 본문 시작
        } // 본문 끝
        public static void DontDestroyOnLoad(object value) // 씬 유지 대체
        { // 본문 시작
        } // 본문 끝
        public static T FindFirstObjectByType<T>() where T : class => (MonoBehaviour.Component as T) ?? (ProjectEpsilon.UI.SettingsMenuController.Active as T); // 구성 요소 검색
        public static T FindFirstObjectByType<T>(FindObjectsInactive mode) where T : class => FindFirstObjectByType<T>(); // 비활성 구성 포함 검색
    } // 클래스 끝
    public class MonoBehaviour : Object // 동작 구성 대체
    { // 클래스 시작
        public static object Component; // 시험용 구성 요소
        public bool isActiveAndEnabled = true; // 활성 상태
        public object gameObject = new object(); // 객체 참조
        public T GetComponent<T>() where T : class => Component as T; // 현재 구성 검색
    } // 클래스 끝
    public class ScriptableObject // 데이터 객체 대체
    { // 클래스 시작
    } // 클래스 끝
    public sealed class SerializeField : Attribute // 직렬화 표시
    { // 클래스 시작
    } // 클래스 끝
    public sealed class DefaultExecutionOrder : Attribute // 실행 순서 표시
    { // 클래스 시작
        public DefaultExecutionOrder(int value) // 순서 설정
        { // 본문 시작
        } // 본문 끝
    } // 클래스 끝
    public sealed class CreateAssetMenuAttribute : Attribute // 데이터 생성 표시
    { // 클래스 시작
        public string fileName; // 데이터 파일명
        public string menuName; // 생성 메뉴명
    } // 클래스 끝
    public static class Application // 실행 환경
    { // 클래스 시작
        public static bool isPlaying = true; // 플레이 상태
    } // 클래스 끝
    public static class Time // 시간 상태
    { // 클래스 시작
        public static float timeScale = 1f; // 게임 시간 배율
    } // 클래스 끝
    public static class Mathf // 수학 도구
    { // 클래스 시작
        public static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max); // 범위 제한
    } // 클래스 끝
    public static class Debug // 진단 출력
    { // 클래스 시작
        public static int ErrorCount; // 오류 기록 수
        public static void LogError(string message) => ErrorCount++; // 구성 오류 기록
    } // 클래스 끝
    public struct Rect // 화면 사각형
    { // 구조체 시작
        public Rect(float x, float y, float width, float height) // 사각형 구성
        { // 본문 시작
        } // 본문 끝
    } // 구조체 끝
    public static class Screen // 화면 크기
    { // 클래스 시작
        public static int width = 1920; // 화면 너비
        public static int height = 1080; // 화면 높이
    } // 클래스 끝
    public static class GUI // 화면 출력
    { // 클래스 시작
        public static void Box(Rect rectangle, string text) // 화면 상자 대체
        { // 본문 시작
        } // 본문 끝
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
        public KeyControl digit1Key = new KeyControl(); // 숫자 일
        public KeyControl digit2Key = new KeyControl(); // 숫자 이
        public KeyControl digit3Key = new KeyControl(); // 숫자 삼
        public KeyControl numpad1Key = new KeyControl(); // 숫자패드 일
        public KeyControl numpad2Key = new KeyControl(); // 숫자패드 이
        public KeyControl numpad3Key = new KeyControl(); // 숫자패드 삼
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Data // 데이터 대체 형식
{ // 영역 시작
    public sealed class WeaponData // 무기 데이터
    { // 클래스 시작
        public string Name; // 무기 구분
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Player // 플레이어 대체 형식
{ // 영역 시작
    public sealed class SnakeHealth // 플레이어 체력
    { // 클래스 시작
        public event Action BodyDepleted; // 몸통 소진 알림
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Core // 입력 설정 대체 형식
{ // 영역 시작
    public sealed class InputBindingManager // 입력 설정 관리자
    { // 클래스 시작
        public static InputBindingManager Instance; // 현재 관리자
        public bool IsRebinding; // 재설정 여부
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Progression // 보상 후보 대체
{ // 영역 시작
    public readonly struct WeaponRewardCandidate // 무기 보상 후보
    { // 구조체 시작
        public WeaponData Weapon { get; } // 보상 무기
        public int Grade { get; } // 보상 등급
        public WeaponRewardCandidate(WeaponData weapon, int grade) // 후보 구성
        { // 생성자 시작
            Weapon = weapon; // 무기 저장
            Grade = grade; // 등급 저장
        } // 생성자 끝
    } // 구조체 끝
} // 영역 끝

namespace ProjectEpsilon.Combat // 무기 관리자 대체
{ // 영역 시작
    public sealed class SnakeWeaponManager // 무기 관리자
    { // 클래스 시작
        public WeaponData Weapon; // 첫 슬롯 무기
        public int EquipCount; // 장착 횟수
        public int SlotCount = 3; // 몸통 슬롯 수
        public bool TryEquipAt(int slot, WeaponData weapon, int grade) // 첫 무기 장착
        { // 메서드 시작
            if (slot != 0 || weapon == null) // 잘못된 장착 확인
            { // 조건 시작
                return false; // 장착 실패
            } // 조건 끝
            Weapon = weapon; // 선택 무기 저장
            EquipCount++; // 장착 횟수 증가
            return true; // 장착 성공
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.UI // 화면 대체 형식
{ // 영역 시작
    public sealed class SettingsMenuController // 설정 화면
    { // 클래스 시작
        public static SettingsMenuController Active; // 활성 설정 화면
        public bool IsOpen; // 설정 표시 여부
    } // 클래스 끝
    public sealed class LevelUpPanelController // 세 후보 화면
    { // 클래스 시작
        public event Action<int> CandidateSelected; // 후보 선택 알림
        public bool Visible; // 화면 표시 여부
        public int ShowCount; // 화면 표시 횟수
        public void ShowStartingWeapons(IReadOnlyList<WeaponRewardCandidate> candidates) // 시작 무기 표시
        { // 메서드 시작
            Visible = true; // 선택 화면 표시
            ShowCount++; // 표시 횟수 증가
        } // 메서드 끝
        public void Hide() // 화면 숨김
        { // 메서드 시작
            Visible = false; // 화면 숨김
        } // 메서드 끝
        public void Click(int index) => CandidateSelected?.Invoke(index); // 후보 클릭
    } // 클래스 끝
} // 영역 끝
