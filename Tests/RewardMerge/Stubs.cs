using System; // 기본 형식
using System.Collections.Generic; // 목록 형식
using ProjectEpsilon.Data; // 무기 데이터

namespace UnityEngine // 유니티 대체
{ // 영역 시작
    public class MonoBehaviour // 컴포넌트 대체
    { // 클래스 시작
        public Transform transform = new Transform(); // 객체 위치
        public T GetComponent<T>() where T : class => null; // 부품 검색
    } // 클래스 끝
    public sealed class SerializeField : Attribute // 직렬화 표시
    { // 클래스 시작
    } // 클래스 끝
    public sealed class DefaultExecutionOrder : Attribute // 순서 표시
    { // 클래스 시작
        public DefaultExecutionOrder(int order) // 실행 순서
        { // 본문 시작
        } // 본문 끝
    } // 클래스 끝
    public sealed class HeaderAttribute : Attribute // 제목 표시
    { // 클래스 시작
        public HeaderAttribute(string title) // 제목 설정
        { // 본문 시작
        } // 본문 끝
    } // 클래스 끝
    public sealed class RangeAttribute : Attribute // 범위 표시
    { // 클래스 시작
        public RangeAttribute(float min, float max) // 범위 설정
        { // 본문 시작
        } // 본문 끝
    } // 클래스 끝
    public static class Mathf // 수학 도구
    { // 클래스 시작
        public static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max); // 정수 제한
        public static int Max(int a, int b) => Math.Max(a, b); // 정수 최댓값
        public static float Max(float a, float b) => Math.Max(a, b); // 소수 최댓값
    } // 클래스 끝
    public static class Time // 시간 정보
    { // 클래스 시작
        public static float time; // 현재 시간
    } // 클래스 끝
    public class Sprite // 이미지 대체
    { // 클래스 시작
    } // 클래스 끝
    public struct Vector3 // 위치 대체
    { // 구조체 시작
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(); // 위치 차이
    } // 구조체 끝
    public struct Vector2 // 방향 대체
    { // 구조체 시작
        public float sqrMagnitude => 1f; // 방향 길이
        public static implicit operator Vector2(Vector3 vector) => new Vector2(); // 평면 변환
    } // 구조체 끝
    public class Transform // 위치 정보
    { // 클래스 시작
        public Vector3 position; // 위치
        public Vector2 up; // 앞 방향
    } // 클래스 끝
    public class GameObject // 객체 대체
    { // 클래스 시작
        public Transform transform = new Transform(); // 위치
        public GameObject(string name) // 객체 생성
        { // 본문 시작
        } // 본문 끝
        public T AddComponent<T>() where T : new() => new T(); // 부품 생성
    } // 클래스 끝
    public struct Color // 색상 대체
    { // 구조체 시작
        public Color(float r, float g, float b, float a) // 색상 생성
        { // 본문 시작
        } // 본문 끝
    } // 구조체 끝
} // 영역 끝

namespace ProjectEpsilon.Core // 게임 상태 대체
{ // 영역 시작
    public sealed class GameManager // 게임 관리자
    { // 클래스 시작
        public static GameManager Instance; // 현재 관리자
        public bool IsPlaying = true; // 진행 여부
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Data // 무기 정보 대체
{ // 영역 시작
    public enum WeaponAttackType // 공격 형식
    { // 열거 시작
        Melee, // 근접 공격
        StraightProjectile, // 직선 투사체
        Area // 범위 공격
    } // 열거 끝
    public sealed class WeaponData // 무기 설정
    { // 클래스 시작
        public WeaponAttribute Attribute; // 속성
        public WeaponAttackType AttackType; // 공격 종류
        public string DisplayName = "Test"; // 이름
        public int MaxGrade = 5; // 최대 등급
        public float BaseDamage = 1f; // 기본 피해
        public float Range = 1f; // 사거리
        public float AttackInterval = 1f; // 공격 주기
        public float ProjectileSpeed = 1f; // 투사체 속도
        public float ProjectileLifetime = 1f; // 투사체 지속 시간
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Player // 플레이어 대체
{ // 영역 시작
    public sealed class SnakeSegment // 몸통
    { // 클래스 시작
        public UnityEngine.Transform transform = new UnityEngine.Transform(); // 몸통 위치
    } // 클래스 끝
    public sealed class SnakeBodyManager // 몸통 관리자
    { // 클래스 시작
        public event Action<int, int> BodyCountChanged; // 몸통 변경 알림
        public readonly List<SnakeSegment> BodySegments = new List<SnakeSegment>(); // 몸통 목록
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Combat // 전투 대체
{ // 영역 시작
    public enum WeaponGradeEffectTrigger // 등급 효과 종류
    { // 열거 시작
        Acquired, // 획득 효과
        Merged, // 합성 효과
        Attack // 공격 효과
    } // 열거 끝
    public readonly struct WeaponGradeEffectContext // 등급 효과 정보
    { // 구조체 시작
        public WeaponGradeEffectContext(WeaponData weapon, int grade, UnityEngine.Vector3 origin, float damage, WeaponGradeEffectTrigger trigger) // 효과 정보 생성
        { // 본문 시작
        } // 본문 끝
    } // 구조체 끝
    public sealed class WeaponGradeEffectHooks // 등급 효과 연결
    { // 클래스 시작
        public void Notify(WeaponGradeEffectContext context) // 효과 알림
        { // 본문 시작
        } // 본문 끝
    } // 클래스 끝
    public readonly struct WeaponAttributeAttackSnapshot // 공격 정보
    { // 구조체 시작
        public float DirectDamage => 0f; // 직접 피해
        public WeaponAttributeAttackSnapshot(WeaponData weapon, WeaponAttribute attribute, int count, int stage, int grade, UnityEngine.Vector3 origin, float damage) // 공격 정보 생성
        { // 본문 시작
        } // 본문 끝
    } // 구조체 끝
    public sealed class WeaponAttributeCombatEffects // 속성 전투 효과
    { // 클래스 시작
        public WeaponAttributeAttackSnapshot CreateAttackSnapshot(WeaponData weapon, int grade, UnityEngine.Vector3 origin, float damage) => new WeaponAttributeAttackSnapshot(); // 공격 정보 생성
        public void ApplyHit(WeaponAttributeAttackSnapshot attack, WeaponTarget target, UnityEngine.Vector3 position) // 대상 피해
        { // 본문 시작
        } // 본문 끝
    } // 클래스 끝
    public sealed class WeaponTarget // 공격 대상
    { // 클래스 시작
        public UnityEngine.Transform transform = new UnityEngine.Transform(); // 대상 위치
        public static WeaponTarget FindClosest(UnityEngine.Vector3 origin, float range) => null; // 가까운 대상
        public static int VisitAllInRange(UnityEngine.Vector3 origin, float range, Action<WeaponTarget> visit) => 0; // 범위 대상 방문
        public void TakeDamage(float value) // 피해 적용
        { // 본문 시작
        } // 본문 끝
    } // 클래스 끝
    public sealed class StraightProjectile // 직선 투사체
    { // 클래스 시작
        public void Configure(UnityEngine.Vector2 direction, WeaponAttributeAttackSnapshot attack, WeaponAttributeCombatEffects effects, float speed, float lifetime, UnityEngine.Sprite sprite) // 투사체 설정
        { // 본문 시작
        } // 본문 끝
    } // 클래스 끝
    public sealed class WeaponAttackPulse // 공격 표시
    { // 클래스 시작
        public void Configure(UnityEngine.Sprite sprite, float radius, UnityEngine.Color color, float duration) // 표시 설정
        { // 본문 시작
        } // 본문 끝
    } // 클래스 끝
    public static class WeaponGradeRules // 무기 등급 규칙
    { // 클래스 시작
        public static float CalculateDamage(float damage, int grade) => damage; // 등급 피해 계산
    } // 클래스 끝
} // 영역 끝
