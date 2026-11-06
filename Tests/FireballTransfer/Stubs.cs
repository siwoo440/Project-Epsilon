using System; // 기본 형식
using System.Collections.Generic; // 대상 목록
using ProjectEpsilon.Data; // 무기 데이터

namespace UnityEngine // Unity 대체 형식
{ // 영역 시작
    public class Object // Unity 객체 대체
    { // 클래스 시작
        public static void Destroy(GameObject value) // 파괴 예약
        { // 메서드 시작
            value.PendingDestroy = true; // 파괴 예약 기록
        } // 메서드 끝
    } // 클래스 끝
    public class MonoBehaviour : Object // 구성 요소 대체
    { // 클래스 시작
        public readonly GameObject gameObject = new GameObject(); // 소속 객체
        public bool isActiveAndEnabled = true; // 구성 활성 상태
        public Transform transform => gameObject.transform; // 객체 위치
        public T GetComponent<T>() where T : new() => new T(); // 필수 구성 조회
    } // 클래스 끝
    public sealed class GameObject // 씬 객체 대체
    { // 클래스 시작
        public bool PendingDestroy; // 파괴 예약 여부
        public Transform transform = new Transform(); // 객체 위치
    } // 클래스 끝
    public sealed class Transform // 위치 대체
    { // 클래스 시작
        public Vector3 position; // 월드 위치
        public Vector3 localScale; // 객체 크기
        public Quaternion rotation; // 객체 회전
    } // 클래스 끝
    public struct Vector2 // 평면 방향
    { // 구조체 시작
        public float x; // 가로 방향
        public float y; // 세로 방향
        public static Vector2 up => new Vector2 { x = 0f, y = 1f }; // 위 방향
        public float sqrMagnitude => x * x + y * y; // 크기 제곱
        public Vector2 normalized => this; // 시험용 정규화
        public static Vector2 operator *(Vector2 value, float scale) => new Vector2 { x = value.x * scale, y = value.y * scale }; // 방향 배율
        public static explicit operator Vector3(Vector2 value) => new Vector3(value.x, value.y, 0f); // 평면 위치 변환
    } // 구조체 끝
    public struct Vector3 // 월드 위치
    { // 구조체 시작
        public float x; // 가로 위치
        public float y; // 세로 위치
        public float z; // 깊이 위치
        public Vector3(float a, float b, float c) // 위치 생성
        { // 생성자 시작
            x = a; // 가로 저장
            y = b; // 세로 저장
            z = c; // 깊이 저장
        } // 생성자 끝
        public float sqrMagnitude => x * x + y * y + z * z; // 거리 제곱
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); // 위치 차이
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); // 위치 합계
    } // 구조체 끝
    public struct Quaternion // 회전 대체
    { // 구조체 시작
        public static Quaternion Euler(float x, float y, float z) => new Quaternion(); // 회전 생성
    } // 구조체 끝
    public sealed class Sprite // 스프라이트 대체
    { // 클래스 시작
    } // 클래스 끝
    public sealed class SpriteRenderer // 렌더러 대체
    { // 클래스 시작
        public Sprite sprite; // 이미지
        public Color color; // 색상
        public int sortingOrder; // 표시 순서
    } // 클래스 끝
    public readonly struct Color // 색상 대체
    { // 구조체 시작
        public Color(float r, float g, float b, float a) // 색상 생성
        { // 생성자 시작
        } // 생성자 끝
    } // 구조체 끝
    public class Collider2D // 충돌체 대체
    { // 클래스 시작
        public ProjectEpsilon.Combat.WeaponTarget Target; // 충돌 표적
        public T GetComponent<T>() where T : class => Target as T; // 표적 조회
    } // 클래스 끝
    public sealed class CircleCollider2D : Collider2D // 원형 충돌체
    { // 클래스 시작
        public bool isTrigger; // 트리거 여부
        public float radius; // 충돌 반경
    } // 클래스 끝
    public enum RigidbodyType2D // 물리체 종류
    { // 열거 시작
        Kinematic // 운동학 물리체
    } // 열거 끝
    public enum RigidbodyConstraints2D // 물리 제약
    { // 열거 시작
        FreezeRotation // 회전 고정
    } // 열거 끝
    public sealed class Rigidbody2D // 물리체 대체
    { // 클래스 시작
        public RigidbodyType2D bodyType; // 물리체 종류
        public float gravityScale; // 중력 배율
        public RigidbodyConstraints2D constraints; // 물리 제약
    } // 클래스 끝
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)] // 중복 필수 구성 허용
    public sealed class RequireComponent : Attribute // 필수 구성 표시
    { // 클래스 시작
        public RequireComponent(Type type) // 필수 구성 설정
        { // 생성자 시작
        } // 생성자 끝
    } // 클래스 끝
    public static class Time // 시간 상태
    { // 클래스 시작
        public static float deltaTime; // 프레임 경과 시간
    } // 클래스 끝
    public static class Mathf // 수학 도구
    { // 클래스 시작
        public const float Rad2Deg = 57.29578f; // 라디안 각도 변환
        public static float Max(float a, float b) => Math.Max(a, b); // 실수 최댓값
        public static float Atan2(float y, float x) => MathF.Atan2(y, x); // 방향 각도
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Core // 게임 상태 대체
{ // 영역 시작
    public sealed class GameManager // 게임 관리자
    { // 클래스 시작
        public static GameManager Instance; // 현재 게임
        public bool IsPlaying = true; // 진행 상태
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Data // 무기 데이터 대체
{ // 영역 시작
    public enum WeaponAttribute // 무기 속성
    { // 열거 시작
        Physical, // 물리
        Fire // 화염
    } // 열거 끝
    public sealed class WeaponData // 시험 무기
    { // 클래스 시작
        public WeaponAttribute Attribute = WeaponAttribute.Fire; // 무기 속성
        public int ProjectileHitLimit = 1; // 투사체 관통 수
        public float GradeThreeTransferRange; // 임시 전달 범위
        public float GradeThreeTransferDamageMultiplier = 1f; // 임시 전달 피해 배율
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Combat // 공격 대상 대체
{ // 영역 시작
    public readonly struct WeaponAttributeAttackSnapshot // 발사 시점 공격 정보
    { // 구조체 시작
        public WeaponData Weapon { get; } // 무기 데이터
        public WeaponAttribute Attribute { get; } // 공격 속성
        public int AttributeCount { get; } // 속성 무기 개수
        public int SynergyStage { get; } // 속성 시너지 단계
        public int Grade { get; } // 발사 무기 등급
        public UnityEngine.Vector3 Origin { get; } // 몸통 발사 위치
        public float DirectDamage { get; } // 직접 피해
        public WeaponAttributeAttackSnapshot(WeaponData weapon, WeaponAttribute attribute, int count, int stage, int grade, UnityEngine.Vector3 origin, float damage) // 공격 정보 생성
        { // 생성자 시작
            Weapon = weapon; // 무기 저장
            Attribute = attribute; // 속성 저장
            AttributeCount = count; // 속성 개수 저장
            SynergyStage = stage; // 시너지 단계 저장
            Grade = grade; // 등급 저장
            Origin = origin; // 발사 위치 저장
            DirectDamage = damage; // 피해 저장
        } // 생성자 끝
    } // 구조체 끝
    public sealed class WeaponAttributeCombatEffects // 속성 효과 대체
    { // 클래스 시작
        public readonly List<WeaponAttributeAttackSnapshot> Hits = new List<WeaponAttributeAttackSnapshot>(); // 명중 정보 기록
        public void ApplyHit(WeaponAttributeAttackSnapshot attack, WeaponTarget target, UnityEngine.Vector3 position) // 기존 통합 명중 경로
        { // 메서드 시작
            Hits.Add(attack); // 발사 정보 기록
            target.TakeDamage(attack.DirectDamage); // 직접 피해 적용
            if (attack.Attribute == WeaponAttribute.Fire && target.IsAlive) // 생존 화염 대상 확인
            { // 조건 시작
                target.Burns++; // 화상 경로 기록
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
    public sealed class WeaponTarget // 공격 대상 대체
    { // 클래스 시작
        public static readonly List<WeaponTarget> Active = new List<WeaponTarget>(); // 활성 대상 목록
        public UnityEngine.Transform transform = new UnityEngine.Transform(); // 대상 위치
        public bool isActiveAndEnabled = true; // 활성 상태
        public float Health = 100f; // 남은 체력
        public bool IsAlive => Health > 0f; // 생존 상태
        public int Hits; // 명중 횟수
        public int Burns; // 화상 적용 횟수
        public Action OnHit; // 첫 명중 재진입 효과
        public static WeaponTarget FindClosest(UnityEngine.Vector3 origin, float range, WeaponTarget excluded) // 최근접 대상 검색
        { // 메서드 시작
            WeaponTarget best = null; // 최근접 대상
            float maxDistance = range * range; // 최대 거리 제곱
            foreach (WeaponTarget target in Active) // 활성 대상 순회
            { // 반복 시작
                if (target == excluded || !target.isActiveAndEnabled || !target.IsAlive) // 제외 및 활성 확인
                { // 조건 시작
                    continue; // 다음 대상 검색
                } // 조건 끝
                float distance = (target.transform.position - origin).sqrMagnitude; // 명중 위치 거리
                if (distance <= maxDistance) // 최근접 거리 확인
                { // 조건 시작
                    maxDistance = distance; // 최근접 거리 갱신
                    best = target; // 최근접 대상 저장
                } // 조건 끝
            } // 반복 끝
            return best; // 검색 결과 반환
        } // 메서드 끝
        public void TakeDamage(float damage) // 피해 적용
        { // 메서드 시작
            Hits++; // 명중 횟수 증가
            Health -= damage; // 체력 감소
            OnHit?.Invoke(); // 재진입 효과 호출
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
