using System; // 기본 형식

namespace UnityEngine // 유니티 대체 형식
{ // 영역 시작
    public class MonoBehaviour // 컴포넌트 기본형
    { // 클래스 시작
        public object Component; // 같은 오브젝트 컴포넌트
        public bool isActiveAndEnabled = true; // 활성 상태
        public T GetComponent<T>() where T : class => Component as T; // 연결 컴포넌트 조회
    } // 클래스 끝
    public sealed class SerializeField : Attribute // 직렬화 표시
    { // 클래스 시작
    } // 클래스 끝
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)] // 중복 의존성 허용
    public sealed class RequireComponent : Attribute // 컴포넌트 의존성 표시
    { // 클래스 시작
        public RequireComponent(Type type) // 의존성 지정
        { // 생성자 시작
        } // 생성자 끝
    } // 클래스 끝
    public sealed class CircleCollider2D // 원형 충돌체
    { // 클래스 시작
    } // 클래스 끝
    public sealed class Rigidbody2D // 물리체
    { // 클래스 시작
    } // 클래스 끝
    public sealed class Collider2D // 다른 충돌체
    { // 클래스 시작
        public object Component; // 충돌 대상 컴포넌트
        public T GetComponent<T>() where T : class => Component as T; // 컴포넌트 조회
    } // 클래스 끝
    public static class Mathf // 수학 도구
    { // 클래스 시작
        public static int Max(int a, int b) => Math.Max(a, b); // 정수 최댓값
        public static float Max(float a, float b) => Math.Max(a, b); // 실수 최댓값
    } // 클래스 끝
    public static class Time // 경과 시간
    { // 클래스 시작
        public static float time; // 현재 시간
    } // 클래스 끝
    public static class Application // 플레이 상태 대체
    { // 클래스 시작
        public static bool isPlaying = true; // 플레이 상태
    } // 클래스 끝
    public static class Debug // 오류 기록 대체
    { // 클래스 시작
        public static int ErrorCount; // 오류 기록 횟수
        public static void LogError(string message) // 오류 기록 대체
        { // 메서드 시작
            ErrorCount++; // 오류 기록 증가
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Player // 플레이어 대체 형식
{ // 영역 시작
    public static class SnakePhysicsStepDispatcher // 물리 스텝 대체 처리
    { // 클래스 시작
        public static int Generation = 1; // 시험용 등록 세대
        public static bool CanQueue = true; // 시험용 자동 물리 상태
        public static int RegisterCalls; // 스텝 등록 횟수
        public static bool Register(Action listener) // 스텝 등록
        { // 메서드 시작
            RegisterCalls++; // 등록 횟수 증가
            return true; // 스텝 등록 성공
        } // 메서드 끝
        public static void Unregister(Action listener) // 스텝 등록 해제
        { // 메서드 시작
        } // 메서드 끝
    } // 클래스 끝
    public static class Trace // 충돌 처리 순서
    { // 클래스 시작
        public static string Events = ""; // 처리 순서 기록
    } // 클래스 끝
    public enum SnakeContactHazardType // 충돌 종류
    { // 열거 시작
        EnemyDirect, // 적 직접 충돌
        Obstacle // 장애물 충돌
    } // 열거 끝
    public sealed class SnakeContactHazard // 충돌 위험
    { // 클래스 시작
        public SnakeContactHazardType HazardType; // 충돌 종류
    } // 클래스 끝
    public sealed class SnakeSegment // 몸통 정보
    { // 클래스 시작
        public bool IsBody = true; // 몸통 여부
    } // 클래스 끝
    public sealed class SnakeBodyManager // 몸통 관리자
    { // 클래스 시작
        public int CurrentBodyCount = 5; // 현재 몸통 수
        public Action OnRemoved; // 몸통 손실 시 시험 동작
        public int RemoveBodies(int count) // 몸통 손실
        { // 메서드 시작
            Trace.Events += "B"; // 몸통 처리 기록
            int removed = Math.Min(CurrentBodyCount, count); // 실제 손실량
            CurrentBodyCount -= removed; // 몸통 수 감소
            OnRemoved?.Invoke(); // 몸통 손실 알림
            return removed; // 손실량 반환
        } // 메서드 끝
    } // 클래스 끝
    public sealed class SnakeInvulnerability // 전체 무적 상태
    { // 클래스 시작
        private float until; // 무적 종료 시간
        public bool IsInvulnerable => UnityEngine.Time.time < until; // 무적 여부
        public void StartInvulnerability(float duration) // 무적 시작
        { // 메서드 시작
            until = Math.Max(until, UnityEngine.Time.time + duration); // 종료 시간 갱신
        } // 메서드 끝
    } // 클래스 끝
    public sealed class SnakeHealth // 공유 체력
    { // 클래스 시작
        public int CurrentHealth = 100; // 현재 체력
        public int DamageCalls; // 피해 처리 횟수
        public SnakeInvulnerability Immunity; // 전체 무적 참조
        public bool TakeDamage(int amount) // 체력 피해
        { // 메서드 시작
            if (Immunity != null && Immunity.IsInvulnerable) // 전체 무적 확인
            { // 조건 시작
                return false; // 무적 피해 차단
            } // 조건 끝
            Trace.Events += "H"; // 체력 처리 기록
            DamageCalls++; // 피해 처리 횟수 증가
            CurrentHealth -= amount; // 체력 감소
            return true; // 피해 성공
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
