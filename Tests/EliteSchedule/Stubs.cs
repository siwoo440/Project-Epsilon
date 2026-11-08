using System; // 기본 형식
using System.Collections.Generic; // 객체 목록
using System.Linq; // 객체 조회

namespace UnityEngine // Unity 대체
{ // 영역 시작
    [AttributeUsage(AttributeTargets.All)] public sealed class SerializeField : Attribute { } // 직렬화 표시
    [AttributeUsage(AttributeTargets.Class)] public sealed class DefaultExecutionOrder : Attribute { public readonly int Order; public DefaultExecutionOrder(int value) { Order = value; } } // 실행 순서
    public class Object // 객체 기반
    { // 클래스 시작
        public static T FindFirstObjectByType<T>() where T : class => GameObject.Objects.SelectMany(x => x.Components).OfType<T>().FirstOrDefault(); // 첫 객체 조회
        public static T[] FindObjectsByType<T>(FindObjectsSortMode mode) where T : class => GameObject.Objects.Where(item => item.activeInHierarchy).SelectMany(item => item.Components).OfType<T>().ToArray(); // 활성 객체 목록 조회
        public static void Destroy(object item) { if (item is GameObject target) { target.destroyed = true; } } // 제거 대체
    } // 클래스 끝
    public enum FindObjectsSortMode // 객체 정렬 방식 대체
    { // 열거 시작
        None // 정렬 생략
    } // 열거 끝
    public class Component : Object // 구성 요소
    { // 클래스 시작
        public GameObject gameObject; // 소속 객체
        public Transform transform => gameObject.transform; // 위치 조회
        public T GetComponent<T>() where T : class => gameObject.GetComponent<T>(); // 구성 조회
        public T GetComponentInChildren<T>() where T : class => GetComponent<T>(); // 하위 구성 조회
    } // 클래스 끝
    public class MonoBehaviour : Component // 행동 기반
    { // 클래스 시작
        public bool isActiveAndEnabled = true; // 활성 상태
    } // 클래스 끝
    public sealed class GameObject : Object // 게임 객체
    { // 클래스 시작
        public static readonly List<GameObject> Objects = new List<GameObject>(); // 생성 객체 목록
        public readonly List<Component> Components = new List<Component>(); // 구성 목록
        public Transform transform; // 위치 정보
        public string name; // 객체 이름
        public bool activeInHierarchy = true; // 객체 활성 상태
        public bool destroyed; // 제거 기록
        public GameObject(string value) { name = value; transform = new Transform { gameObject = this }; Objects.Add(this); } // 객체 생성
        public T AddComponent<T>() where T : Component, new() { T value = new T { gameObject = this }; Components.Add(value); return value; } // 구성 추가
        public T GetComponent<T>() where T : class => Components.OfType<T>().FirstOrDefault(); // 구성 조회
        public void SetActive(bool value) { activeInHierarchy = value; if (!value) { foreach (Component item in Components) { if (item is ProjectEpsilon.Progression.EnemyDropController drop) { drop.Disable(); } } } } // 활성 변경과 보상 해제
    } // 클래스 끝
    public sealed class Transform : Component // 위치와 크기
    { // 클래스 시작
        public Vector3 position; // 월드 위치
        public Vector3 localScale; // 크기
        public bool IsChildOf(Transform other) => this == other; // 부모 확인
    } // 클래스 끝
    public struct Vector2 // 평면 벡터
    { // 구조 시작
        public float x; // 가로 좌표
        public float y; // 세로 좌표
        public Vector2(float a, float b) { x = a; y = b; } // 좌표 생성
        public float sqrMagnitude => x * x + y * y; // 거리 제곱
        public float magnitude => MathF.Sqrt(sqrMagnitude); // 벡터 길이
        public Vector2 normalized => magnitude > 0f ? this * (1f / magnitude) : new Vector2(); // 정규 방향
        public static Vector2 right => new Vector2(1f, 0f); // 오른쪽 방향
        public static Vector2 up => new Vector2(0f, 1f); // 위쪽 방향
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y); // 덧셈
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y); // 뺄셈
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.x, -a.y); // 방향 반전
        public static Vector2 operator *(Vector2 a, float b) => new Vector2(a.x * b, a.y * b); // 크기 배율
        public static implicit operator Vector3(Vector2 value) => new Vector3(value.x, value.y, 0f); // 삼차원 변환
        public static implicit operator Vector2(Vector3 value) => new Vector2(value.x, value.y); // 평면 변환
    } // 구조 끝
    public struct Vector3 // 월드 벡터
    { // 구조 시작
        public float x; // 가로 좌표
        public float y; // 세로 좌표
        public float z; // 깊이 좌표
        public Vector3(float a, float b, float c) { x = a; y = b; z = c; } // 좌표 생성
        public float sqrMagnitude => x * x + y * y + z * z; // 거리 제곱
        public static Vector3 one => new Vector3(1f, 1f, 1f); // 단위 크기
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); // 덧셈
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); // 뺄셈
        public static Vector3 operator *(Vector3 a, float b) => new Vector3(a.x * b, a.y * b, a.z * b); // 크기 배율
    } // 구조 끝
    public static class Mathf // 수학 기능
    { // 클래스 시작
        public static float Max(float a, float b) => MathF.Max(a, b); // 실수 최댓값
        public static float Min(float a, float b) => MathF.Min(a, b); // 실수 최솟값
        public static float Sqrt(float value) => MathF.Sqrt(value); // 제곱근
        public static int Max(int a, int b) => Math.Max(a, b); // 정수 최댓값
        public static int CeilToInt(float value) => (int)MathF.Ceiling(value); // 올림
        public const float Deg2Rad = MathF.PI / 180f; // 각도에서 라디안 변환
        public static float Cos(float value) => MathF.Cos(value); // 코사인 계산
        public static float Sin(float value) => MathF.Sin(value); // 사인 계산
        public static float Clamp(float value, float minimum, float maximum) => Math.Clamp(value, minimum, maximum); // 실수 범위 제한
        public static int Clamp(int value, int minimum, int maximum) => Math.Clamp(value, minimum, maximum); // 정수 범위 제한
    } // 클래스 끝
    public static class Time // 시험 시간
    { // 클래스 시작
        public static float time; // 경과 시간
        public static float fixedDeltaTime = 0.02f; // 물리 간격
        public static float deltaTime = 0.02f; // 프레임 간격
        public static double timeAsDouble; // 실제 게임 시간 대체
    } // 클래스 끝
    public static class Random // 난수 대체
    { // 클래스 시작
        public static Vector2 insideUnitCircle = new Vector2(8f, 4.12f); // 시험 방향
        public static int Selected; // 시험 출현 순번
        public static int Range(int min, int max) => Math.Clamp(Selected, min, max - 1); // 지정 후보 선택
    } // 클래스 끝
    public sealed class Camera : Component // 시험 카메라
    { // 클래스 시작
        public static Camera main; // 주 카메라
        public bool orthographic = true; // 직교 투영
        public float orthographicSize = 5f; // 세로 절반 크기
        public float aspect = 16f / 9f; // 종횡비
        public Vector3 WorldToViewportPoint(Vector3 position) => new Vector3(0.5f + (position.x - transform.position.x) / (2f * orthographicSize * aspect), 0.5f + (position.y - transform.position.y) / (2f * orthographicSize), 0f); // 뷰포트 좌표
    } // 클래스 끝
    public struct Color // 표시 색상
    { // 구조 시작
        public Color(float r, float g, float b, float a = 1f) { } // 색상 생성
        public static Color white => new Color(); // 기본 색상
        public static Color red => new Color(); // 예고 색상
    } // 구조 끝
    public sealed class Sprite : Object { } // 스프라이트
    public sealed class SpriteRenderer : Component { public Sprite sprite; public Color color; public int sortingOrder; } // 표시 정보
    public class Collider2D : Component { public bool isTrigger; } // 충돌체
    public sealed class CircleCollider2D : Collider2D { } // 원형 충돌체
    public enum RigidbodyType2D { Kinematic } // 물리 이동 방식
    public sealed class Rigidbody2D : Component { public RigidbodyType2D bodyType; public float gravityScale; public bool useFullKinematicContacts; public Vector2 position; public void MovePosition(Vector2 value) { position = value; } } // 물리체
    public sealed class Shader : Object { public static Shader Find(string name) => new Shader(); } // 셰이더 조회
    public sealed class Material : Object { public Material(Shader shader) { } } // 표시 재질
    public sealed class LineRenderer : Component { public Material material; public Color startColor; public Color endColor; public float startWidth; public float endWidth; public int sortingOrder; public int positionCount; public bool enabled; public void SetPosition(int index, Vector3 value) { } } // 예고선
    public static class Debug { public static void LogError(string value) { } } // 오류 기록
} // 영역 끝

namespace ProjectEpsilon.Core // 게임 상태
{ // 영역 시작
    public enum GameState // 실제 진행 상태 대체
    { // 열거 시작
        Playing, // 진행 상태
        Paused // 일시정지 상태
    } // 열거 끝
    public sealed class GameManager // 게임 관리자 대체
    { // 클래스 시작
        public static GameManager Instance = new GameManager(); // 전역 관리자
        public bool IsPlaying = true; // 진행 상태
        public GameState CurrentState => IsPlaying ? GameState.Playing : GameState.Paused; // 실제 상태 조회
    } // 클래스 끝
} // 영역 끝
namespace ProjectEpsilon.Player // 플레이어 대체
{ // 영역 시작
    using UnityEngine; // 위치 형식
    public sealed class SnakeMovement : MonoBehaviour { public float MoveSpeed = 3f; } // 이동 상태
    public sealed class SnakeSegment : MonoBehaviour { } // 몸통
    public sealed class SnakeBodyManager : MonoBehaviour { public readonly List<SnakeSegment> BodySegments = new List<SnakeSegment>(); } // 몸통 목록
    public sealed class SnakeHealth : MonoBehaviour { public void TakeDamage(int damage) { } } // 체력
    public enum SnakeContactHazardType { EnemyDirect } // 직접 접촉
    public sealed class SnakeContactHazard : MonoBehaviour { public void Configure(SnakeContactHazardType kind) { } } // 접촉 설정
    public sealed class SnakeExternalCollision : MonoBehaviour { public void QueueEliteCut(Action<bool> result) { } public void QueueHealthDamage(SnakeHealth target, int damage) { } } // 접촉 처리
} // 영역 끝
namespace ProjectEpsilon.Combat // 전투 대체
{ // 영역 시작
    using UnityEngine; // 객체 형식
    public sealed class WeaponTarget : MonoBehaviour { public event Action<WeaponTarget> Died; public float OutgoingDamageMultiplier = 1f; private bool alive = true; public void Configure(float health) { alive = true; } public void Kill() { if (alive) { alive = false; Died?.Invoke(this); gameObject.destroyed = true; } } } // 전투 대상
    public sealed class WeaponTargetStatusController : MonoBehaviour { public float CurrentMovementMultiplier = 1f; } // 이동 상태
    public sealed class WeaponTargetKnockbackController : MonoBehaviour { public bool TryConsumeMovement(float time, out Vector2 displacement) { displacement = new Vector2(); return false; } } // 넉백 대체
} // 영역 끝
namespace ProjectEpsilon.Progression // 보상 대체
{ // 영역 시작
    using UnityEngine; // 객체 형식
    public sealed class SnakeWeaponMergeController : MonoBehaviour // 합성 화면 대체
    { // 클래스 시작
        public bool IsMergeOpen; // 합성 화면 상태
        public void CloseMergePanel() // 합성 보류 대체
        { // 메서드 시작
            IsMergeOpen = false; // 합성 화면 닫기
        } // 메서드 끝
    } // 클래스 끝
    public sealed class RecoveryPickup : MonoBehaviour // 회복 아이템 대체
    { // 클래스 시작
        public bool IsBodyRepair; // 몸통 복구 여부
    } // 클래스 끝
    public sealed class EliteRewardController : MonoBehaviour { public int Count; public void QueueReward() { Count++; } } // 엘리트 보상
    public sealed class EnemyDropController : MonoBehaviour { public static int TotalExperience; private Action<ProjectEpsilon.Combat.WeaponTarget> handler; public void Configure(int xp, int heal, float chance, float repair, Sprite sprite, int count) { handler = target => TotalExperience += xp; GetComponent<ProjectEpsilon.Combat.WeaponTarget>().Died += handler; } public void Disable() { GetComponent<ProjectEpsilon.Combat.WeaponTarget>().Died -= handler; } } // 적 경험치 기록
} // 영역 끝
namespace ProjectEpsilon.Enemies // 적 투사체 대체
{ // 영역 시작
    using ProjectEpsilon.Player; // 플레이어 참조
    using UnityEngine; // 방향 형식
    public sealed class EnemyTestProjectile : MonoBehaviour { public static Vector2 LastDirection; public void Configure(Vector2 direction, float speed, float lifetime, int damage, SnakeHealth health, SnakeBodyManager body, SnakeMovement player, Sprite visual) { LastDirection = direction; } } // 발사 방향 기록
} // 영역 끝
