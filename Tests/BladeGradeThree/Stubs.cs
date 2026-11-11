using System; // 기본 형식
using System.Collections.Generic; // 목록 형식
using ProjectEpsilon.Data; // 무기 데이터

namespace UnityEngine // 유니티 대체
{
    public class MonoBehaviour // 컴포넌트 대체
    {
        public Transform transform = new Transform(); // 객체 위치
        public T GetComponent<T>() where T : class => null; // 부품 검색
    }
    public sealed class SerializeField : Attribute // 직렬화 표시
    {
    }
    public sealed class DefaultExecutionOrder : Attribute // 순서 표시
    {
        public DefaultExecutionOrder(int order) // 실행 순서
        {
        }
    }
    public sealed class HeaderAttribute : Attribute // 제목 표시
    {
        public HeaderAttribute(string title) // 제목 설정
        {
        }
    }
    public sealed class RangeAttribute : Attribute // 범위 표시
    {
        public RangeAttribute(float min, float max) // 범위 설정
        {
        }
    }
    public static class Mathf // 수학 도구
    {
        public static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max); // 정수 제한
        public static int Max(int a, int b) => Math.Max(a, b); // 정수 최댓값
        public static float Max(float a, float b) => Math.Max(a, b); // 소수 최댓값
    }
    public static class Time // 시간 정보
    {
        public static float time; // 현재 시간
    }
    public class Sprite // 이미지 대체
    {
    }
    public struct Vector3 // 위치 대체
    {
        public float x; // 가로 위치
        public float y; // 세로 위치
        public float z; // 깊이 위치
        public Vector3(float xValue, float yValue, float zValue) // 위치 생성
        {
            x = xValue; // 가로 위치 저장
            y = yValue; // 세로 위치 저장
            z = zValue; // 깊이 위치 저장
        }
        public float sqrMagnitude => x * x + y * y + z * z; // 거리 제곱
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); // 위치 차이
    }
    public struct Vector2 // 방향 대체
    {
        public float sqrMagnitude => 1f; // 방향 길이
        public static implicit operator Vector2(Vector3 vector) => new Vector2(); // 평면 변환
    }
    public class Transform // 위치 정보
    {
        public Vector3 position; // 위치
        public Vector2 up; // 앞 방향
    }
    public class GameObject // 객체 대체
    {
        public Transform transform = new Transform(); // 위치
        public GameObject(string name) // 객체 생성
        {
        }
        public T AddComponent<T>() where T : new() => new T(); // 부품 생성
    }
    public struct Color // 색상 대체
    {
        public Color(float r, float g, float b, float a) // 색상 생성
        {
        }
    }
}

namespace ProjectEpsilon.Core // 게임 상태 대체
{
    public sealed class GameManager // 게임 관리자
    {
        public static GameManager Instance; // 현재 관리자
        public bool IsPlaying = true; // 진행 여부
    }
}

namespace ProjectEpsilon.Data // 무기 정보 대체
{
    public enum WeaponAttackType // 공격 형식
    {
        Melee, // 근접 공격
        StraightProjectile, // 직선 투사체
        Area // 범위 공격
    }
    public sealed class WeaponData // 무기 설정
    {
        public WeaponAttribute Attribute; // 속성
        public WeaponAttackType AttackType; // 공격 종류
        public string DisplayName = "Test"; // 이름
        public int MaxGrade = 5; // 최대 등급
        public float BaseDamage = 1f; // 기본 피해
        public float Range = 1f; // 사거리
        public float AttackInterval = 1f; // 공격 주기
        public float ProjectileSpeed = 1f; // 투사체 속도
        public float ProjectileLifetime = 1f; // 투사체 지속 시간
        public int GradeThreeMeleeTargets = 1; // 삼 등급 최대 대상 수
        public int GetMeleeTargetLimit(int grade) => grade >= 3 ? Math.Clamp(GradeThreeMeleeTargets, 1, 2) : 1; // 등급별 근접 대상 수
    }
}

namespace ProjectEpsilon.Player // 플레이어 대체
{
    public sealed class SnakeSegment // 몸통
    {
        public UnityEngine.Transform transform = new UnityEngine.Transform(); // 몸통 위치
    }
    public sealed class SnakeBodyManager // 몸통 관리자
    {
        public event Action<int, int> BodyCountChanged; // 몸통 변경 알림
        public readonly List<SnakeSegment> BodySegments = new List<SnakeSegment>(); // 몸통 목록
    }
}

namespace ProjectEpsilon.Combat // 전투 대체
{
    public enum WeaponGradeEffectTrigger // 등급 효과 종류
    {
        Acquired, // 획득 효과
        Merged, // 합성 효과
        Attack // 공격 효과
    }
    public readonly struct WeaponGradeEffectContext // 등급 효과 정보
    {
        public WeaponGradeEffectContext(WeaponData weapon, int grade, UnityEngine.Vector3 origin, float damage, WeaponGradeEffectTrigger trigger) // 효과 정보 생성
        {
        }
    }
    public sealed class WeaponGradeEffectHooks // 등급 효과 연결
    {
        public int Notifications; // 공격 알림 횟수
        public void Notify(WeaponGradeEffectContext context) // 효과 알림
        {
            Notifications++; // 알림 횟수 증가
        }
    }
    public readonly struct WeaponAttributeAttackSnapshot // 공격 정보
    {
        public float DirectDamage { get; } // 직접 피해
        public UnityEngine.Vector3 Origin { get; } // 몸통 공격 위치
        public WeaponAttributeAttackSnapshot(WeaponData weapon, WeaponAttribute attribute, int count, int stage, int grade, UnityEngine.Vector3 origin, float damage) // 공격 정보 생성
        {
            DirectDamage = damage; // 피해 저장
            Origin = origin; // 공격 위치 저장
        }
    }
    public sealed class WeaponAttributeCombatEffects // 속성 전투 효과
    {
        public int HitCount; // 속성 명중 횟수
        public UnityEngine.Vector3 LastOrigin; // 마지막 명중 원점
        public WeaponAttributeAttackSnapshot CreateAttackSnapshot(WeaponData weapon, int grade, UnityEngine.Vector3 origin, float damage) => new WeaponAttributeAttackSnapshot(weapon, weapon.Attribute, 0, 0, grade, origin, damage); // 공격 정보 생성
        public void ApplyHit(WeaponAttributeAttackSnapshot attack, WeaponTarget target, UnityEngine.Vector3 position) // 대상 피해
        {
            HitCount++; // 명중 횟수 증가
            LastOrigin = attack.Origin; // 몸통 원점 기록
            target.TakeDamage(attack.DirectDamage); // 대상 피해 적용
        }
    }
    public sealed class WeaponTarget // 공격 대상
    {
        public static readonly List<WeaponTarget> Active = new List<WeaponTarget>(); // 활성 시험 대상
        public UnityEngine.Transform transform = new UnityEngine.Transform(); // 대상 위치
        public bool isActiveAndEnabled = true; // 대상 활성 여부
        public bool IsAlive => Health > 0f; // 대상 생존 여부
        public float Health = 100f; // 대상 체력
        public int Hits; // 피해 적용 횟수
        public Action OnHit; // 피해 중 외부 효과
        public static WeaponTarget FindClosest(UnityEngine.Vector3 origin, float range, WeaponTarget excluded = null) // 최근접 대상 탐색
        {
            WeaponTarget closest = null; // 최근접 대상 초기화
            float maximum = range * range; // 최대 거리 제곱
            foreach (WeaponTarget target in Active) // 대상 순회
            {
                if (target == excluded || !target.isActiveAndEnabled || !target.IsAlive) // 제외 대상 검사
                {
                    continue; // 다음 대상 검색
                }
                float distance = (target.transform.position - origin).sqrMagnitude; // 몸통 원점 거리
                if (distance <= maximum) // 최근접 범위 확인
                {
                    maximum = distance; // 최근접 거리 저장
                    closest = target; // 최근접 대상 저장
                }
            }
            return closest; // 최근접 대상 반환
        }
        public static int VisitAllInRange(UnityEngine.Vector3 origin, float range, Action<WeaponTarget> visit) => 0; // 범위 대상 방문
        public void TakeDamage(float value) // 대상 피해
        {
            Hits++; // 명중 횟수 증가
            Health -= value; // 체력 감소
            OnHit?.Invoke(); // 피해 중 외부 효과
        }
        public float OutgoingDamageMultiplier => 1f; // 적 공격 배율
    }
    public sealed class StraightProjectile // 직선 투사체
    {
        public void Configure(UnityEngine.Vector2 direction, WeaponAttributeAttackSnapshot attack, WeaponAttributeCombatEffects effects, float speed, float lifetime, UnityEngine.Sprite sprite) // 투사체 설정
        {
        }
    }
    public sealed class WeaponAttackPulse // 공격 표시
    {
        public void Configure(UnityEngine.Sprite sprite, float radius, UnityEngine.Color color, float duration) // 표시 설정
        {
        }
    }
    public static class WeaponGradeRules // 무기 등급 규칙
    {
        public static float CalculateDamage(float damage, int grade) => damage; // 등급 피해 계산
    }
}
