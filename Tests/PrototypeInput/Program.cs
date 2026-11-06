using System; // 기본 자료형
using System.Reflection; // 비공개 입력 처리 호출
using ProjectEpsilon.Combat; // 무기 관리자
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Progression; // 실제 디버그 입력
using UnityEngine.InputSystem; // 가짜 키보드

namespace UnityEngine // 유니티 대체 자료형
{ // 영역 시작
    public sealed class SerializeField : Attribute // 직렬화 표시
    { // 클래스 시작
    } // 클래스 끝
    public class MonoBehaviour // 컴포넌트 대체
    { // 클래스 시작
        public Transform transform = new Transform(); // 위치 참조
        public object TestComponent; // 연결 컴포넌트 대체
        public T GetComponent<T>() where T : class => TestComponent as T; // 컴포넌트 조회 대체
    } // 클래스 끝
    public sealed class Transform // 위치 대체
    { // 클래스 시작
        public Vector3 position; // 현재 위치
        public Vector3 right; // 오른쪽 방향
    } // 클래스 끝
    public struct Vector3 // 좌표 대체
    { // 구조체 시작
        public static Vector3 operator +(Vector3 left, Vector3 right) => new Vector3(); // 좌표 덧셈
        public static Vector3 operator *(Vector3 value, float scalar) => new Vector3(); // 좌표 배율
    } // 구조체 끝
    public sealed class Sprite // 이미지 대체
    { // 클래스 시작
    } // 클래스 끝
} // 영역 끝

namespace UnityEngine.InputSystem // 키보드 대체
{ // 영역 시작
    public sealed class KeyControl // 키 상태
    { // 클래스 시작
        public bool wasPressedThisFrame; // 입력 발생 여부
        public bool isPressed; // 키 유지 여부
    } // 클래스 끝
    public sealed class Keyboard // 키보드 상태
    { // 클래스 시작
        public static Keyboard current; // 현재 키보드
        public KeyControl hKey = new KeyControl(); // 회복 키
        public KeyControl jKey = new KeyControl(); // 복구 키
        public KeyControl digit3Key = new KeyControl(); // 숫자 3 키
        public KeyControl digit5Key = new KeyControl(); // 숫자 5 키
        public KeyControl leftShiftKey = new KeyControl(); // 왼쪽 쉬프트 키
        public KeyControl rightShiftKey = new KeyControl(); // 오른쪽 쉬프트 키
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Core // 게임 상태 대체
{ // 영역 시작
    public sealed class GameManager // 게임 관리자 대체
    { // 클래스 시작
        public static GameManager Instance; // 현재 관리자
        public bool IsPlaying; // 게임 진행 여부
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Data // 데이터 대체
{ // 영역 시작
    public sealed class WeaponData // 무기 데이터 대체
    { // 클래스 시작
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Player // 플레이어 대체
{ // 영역 시작
    public sealed class SnakeHealth // 체력 대체
    { // 클래스 시작
    } // 클래스 끝
    public sealed class SnakeBodyManager // 몸통 대체
    { // 클래스 시작
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Combat // 전투 대체
{ // 영역 시작
    public sealed class SnakeWeaponManager // 무기 관리자 대체
    { // 클래스 시작
        public int SlotCount => 3; // 슬롯 개수
        public int LastGrade; // 마지막 장착 등급
        public bool TryEquipAt(int index, WeaponData weapon, int grade) // 시험용 무기 장착
        { // 메서드 시작
            LastGrade = grade; // 장착 등급 기록
            return true; // 장착 성공
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Progression // 회복 대체
{ // 영역 시작
    public sealed class SnakeWeaponMergeController // 합성 화면 상태 대체
    { // 클래스 시작
        public bool IsMergeOpen; // 합성 화면 열림 여부
    } // 클래스 끝
    public enum RecoveryPickupType // 회복 종류
    { // 열거형 시작
        Heal, // 체력 회복
        BodyRepair // 몸통 복구
    } // 열거형 끝
    public static class RecoveryPickup // 회복 물품 대체
    { // 클래스 시작
        public static int Count; // 생성 횟수 기록
        public static void Spawn(UnityEngine.Vector3 position, UnityEngine.Transform receiver, Player.SnakeHealth health, Player.SnakeBodyManager body, RecoveryPickupType type, int amount, UnityEngine.Sprite sprite) // 회복 물품 생성 대체
        { // 메서드 시작
            Count++; // 생성 횟수 증가
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝

public static class Program // 입력 검증 실행기
{ // 클래스 시작
    public static int Main() // 검증 시작
    { // 메서드 시작
        Keyboard keyboard = new Keyboard(); // 시험용 키보드
        Keyboard.current = keyboard; // 현재 키보드 설정
        SnakeWeaponManager weapons = new SnakeWeaponManager(); // 시험용 무기 관리자
        SnakePrototypeDebugControls controls = new SnakePrototypeDebugControls(); // 실제 입력 컴포넌트
        controls.Configure(null, null, weapons, new WeaponData(), null); // 무기 관리자 연결
        MethodInfo update = typeof(SnakePrototypeDebugControls).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic); // 입력 갱신 조회
        keyboard.digit3Key.wasPressedThisFrame = true; // 숫자 3 단독 입력
        update.Invoke(controls, null); // 입력 처리 실행
        if (weapons.LastGrade != 0) // 단독 입력 부작용 확인
        { // 조건 시작
            Console.Error.WriteLine("숫자 3 단독 입력으로 디버그 무기 변경"); // 실패 기록
            return 1; // 검증 실패
        } // 조건 끝
        keyboard.digit3Key.wasPressedThisFrame = false; // 숫자 3 해제
        keyboard.digit5Key.wasPressedThisFrame = true; // 숫자 5 단독 입력
        keyboard.hKey.wasPressedThisFrame = true; // 회복 디버그 입력
        keyboard.jKey.wasPressedThisFrame = true; // 복구 디버그 입력
        update.Invoke(controls, null); // 입력 처리 실행
        if (weapons.LastGrade != 0 || RecoveryPickup.Count != 2) // 단독 숫자 차단 및 회복 유지 확인
        { // 조건 시작
            Console.Error.WriteLine("숫자 5 입력 또는 회복 디버그 오류"); // 실패 기록
            return 1; // 검증 실패
        } // 조건 끝
        keyboard.digit3Key.wasPressedThisFrame = true; // 숫자 3 재입력
        keyboard.digit5Key.wasPressedThisFrame = false; // 숫자 5 해제
        keyboard.hKey.wasPressedThisFrame = false; // 회복 입력 해제
        keyboard.jKey.wasPressedThisFrame = false; // 복구 입력 해제
        keyboard.leftShiftKey.isPressed = true; // 왼쪽 쉬프트 입력
        SnakeWeaponMergeController merge = new SnakeWeaponMergeController(); // 시험용 합성 화면
        controls.TestComponent = merge; // 합성 화면 연결
        merge.IsMergeOpen = true; // 합성 화면 열기
        update.Invoke(controls, null); // 합성 중 조합 입력
        if (weapons.LastGrade != 0) // 합성 중 디버그 차단 확인
        { // 조건 시작
            Console.Error.WriteLine("합성 중 등급 디버그 입력 적용"); // 실패 기록
            return 1; // 검증 실패
        } // 조건 끝
        merge.IsMergeOpen = false; // 합성 화면 닫기
        update.Invoke(controls, null); // 조합 입력 처리
        if (weapons.LastGrade != 3) // 쉬프트 조합 확인
        { // 조건 시작
            Console.Error.WriteLine("Shift+3 디버그 무기 변경 실패"); // 실패 기록
            return 1; // 검증 실패
        } // 조건 끝
        keyboard.leftShiftKey.isPressed = false; // 왼쪽 쉬프트 해제
        keyboard.rightShiftKey.isPressed = true; // 오른쪽 쉬프트 입력
        keyboard.digit3Key.wasPressedThisFrame = false; // 숫자 3 해제
        keyboard.digit5Key.wasPressedThisFrame = true; // 숫자 5 입력
        update.Invoke(controls, null); // 오른쪽 조합 입력 처리
        if (weapons.LastGrade != 5) // 오른쪽 쉬프트 조합 확인
        { // 조건 시작
            Console.Error.WriteLine("오른쪽 Shift+5 디버그 무기 변경 실패"); // 실패 기록
            return 1; // 검증 실패
        } // 조건 끝
        Console.WriteLine("디버그 무기 조합 입력 검증 통과"); // 성공 기록
        return 0; // 검증 성공
    } // 메서드 끝
} // 클래스 끝
