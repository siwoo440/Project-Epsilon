using System; // 기본 형식
using System.Collections.Generic; // 객체 목록

namespace UnityEngine // 유니티 대체 형식
{
    public sealed class SerializeField : Attribute // 직렬화 표시
    {
    }
    public sealed class HeaderAttribute : Attribute // 편집기 제목 표시
    {
        public HeaderAttribute(string title) // 제목 설정
        {
        }
    }
    public sealed class DefaultExecutionOrder : Attribute // 실행 순서 표시
    {
        public int Order { get; } // 실행 순서 값
        public DefaultExecutionOrder(int order) // 실행 순서 설정
        {
            Order = order; // 순서 보존
        }
    }
    public class Object // 유니티 객체
    {
        public static void Destroy(Object value) // 런타임 제거 대체
        {
        }
        public static void DestroyImmediate(Object value) // 즉시 제거 대체
        {
        }
    }
    public class Component : Object // 컴포넌트 대체
    {
        public GameObject gameObject; // 소속 객체
        public Transform transform => gameObject.transform; // 소속 위치
        public string name // 객체 이름
        {
            get => gameObject.name; // 이름 조회
            set => gameObject.name = value; // 이름 변경
        }
        public T GetComponent<T>() where T : Component => gameObject.GetComponent<T>(); // 소속 컴포넌트 조회
    }
    public class MonoBehaviour : Component // 행동 컴포넌트
    {
        public bool isActiveAndEnabled = true; // 실제 활성 상태 대체
    }
    public class GameObject : Object // 게임 객체 대체
    {
        private readonly Dictionary<Type, Component> components = new Dictionary<Type, Component>(); // 컴포넌트 보관
        public string name; // 객체 이름
        public Transform transform; // 객체 위치
        public GameObject(string value) // 객체 생성
        {
            name = value; // 이름 저장
            transform = new Transform // 위치 생성
            {
                gameObject = this // 소속 객체 연결
            };
        }
        public T AddComponent<T>() where T : Component, new() // 컴포넌트 추가
        {
            T component = new T // 컴포넌트 생성
            {
                gameObject = this // 소속 객체 연결
            };
            components[typeof(T)] = component; // 컴포넌트 저장
            return component; // 추가 결과
        }
        public T GetComponent<T>() where T : Component // 컴포넌트 조회
        {
            return components.TryGetValue(typeof(T), out Component value) ? (T)value : null; // 저장 항목 반환
        }
    }
    public class Transform : Component // 위치 대체
    {
        private readonly List<Transform> children = new List<Transform>(); // 자식 목록
        private Transform parent; // 부모 위치
        public Vector3 position; // 월드 위치
        public Vector3 up = Vector3.up; // 전진 방향
        public Vector3 localScale; // 크기
        public Quaternion rotation; // 회전 값
        public int childCount => children.Count; // 자식 수
        public Transform GetChild(int index) => children[index]; // 자식 조회
        public int GetSiblingIndex() => parent == null ? 0 : parent.children.IndexOf(this); // 자식 순서
        public void SetParent(Transform value, bool worldPositionStays) // 부모 설정
        {
            parent = value; // 부모 저장
            value.children.Add(this); // 자식 연결
        }
    }
    public class Sprite : Object // 스프라이트 대체
    {
    }
    public class SpriteRenderer : Component // 스프라이트 표시 대체
    {
        public Sprite sprite; // 표시 그림
        public Color color; // 표시 색상
        public int sortingOrder; // 표시 순서
    }
    public class CircleCollider2D : Component // 원형 충돌체 대체
    {
        public bool isTrigger; // 감지 여부
        public float radius; // 충돌 반경
    }
    public struct Color // 색상 대체
    {
        public Color(float red, float green, float blue, float alpha) // 색상 생성
        {
        }
    }
    public struct Quaternion // 회전 대체
    {
        public static Quaternion Euler(float x, float y, float z) => new Quaternion(); // 회전 생성
    }
    public struct Vector3 // 벡터 대체
    {
        public float x; // 가로 좌표
        public float y; // 세로 좌표
        public float z; // 깊이 좌표
        public Vector3(float xValue, float yValue, float zValue) // 좌표 생성
        {
            x = xValue; // 가로 저장
            y = yValue; // 세로 저장
            z = zValue; // 깊이 저장
        }
        public static Vector3 zero => new Vector3(0f, 0f, 0f); // 영점
        public static Vector3 up => new Vector3(0f, 1f, 0f); // 위쪽 방향
        public float sqrMagnitude => x * x + y * y + z * z; // 길이 제곱
        public Vector3 normalized // 단위 방향
        {
            get // 값 조회
            {
                Vector3 result = this; // 값 복사
                result.Normalize(); // 길이 정규화
                return result; // 단위 방향 반환
            }
        }
        public void Normalize() // 벡터 정규화
        {
            float magnitude = MathF.Sqrt(sqrMagnitude); // 벡터 길이
            if (magnitude > 0f) // 영벡터 제외
            {
                x /= magnitude; // 가로 정규화
                y /= magnitude; // 세로 정규화
                z /= magnitude; // 깊이 정규화
            }
        }
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); // 벡터 합
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); // 벡터 차
        public static Vector3 operator *(Vector3 a, float b) => new Vector3(a.x * b, a.y * b, a.z * b); // 벡터 배율
        public static float Distance(Vector3 a, Vector3 b) => MathF.Sqrt((a - b).sqrMagnitude); // 두 위치 거리
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a + (b - a) * Math.Clamp(t, 0f, 1f); // 선형 보간
    }
    public static class Mathf // 수학 대체
    {
        public const float Rad2Deg = 57.29578f; // 라디안 변환
        public static float Atan2(float y, float x) => MathF.Atan2(y, x); // 방향 각도
        public static float Max(float a, float b) => Math.Max(a, b); // 실수 최댓값
        public static int Max(int a, int b) => Math.Max(a, b); // 정수 최댓값
        public static int Max(int a, int b, int c) => Math.Max(a, Math.Max(b, c)); // 세 정수 최댓값
        public static int Min(int a, int b) => Math.Min(a, b); // 정수 최솟값
        public static int FloorToInt(float value) => (int)Math.Floor(value); // 정수 내림
        public static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max); // 정수 범위 제한
        public static float Clamp01(float value) => Math.Clamp(value, 0f, 1f); // 비율 범위 제한
    }
    public static class Application // 실행 상태 대체
    {
        public static bool isPlaying; // 플레이 상태
    }
    public static class Time // 게임 시간 대체
    {
        public static float time; // 현재 게임 시간
        public static float deltaTime = 0.02f; // 실제 프레임 간격 대체
    }
}

namespace UnityEngine.UI // 문구 표시 대체
{
    public sealed class Text // UI 텍스트
    {
        public string text; // 표시 문구
    }
}

namespace ProjectEpsilon.Core // 게임 상태 대체
{
    public sealed class GameManager // 게임 관리자 대체
    {
        public static GameManager Instance => null; // 관리자 없음
        public bool IsPlaying => true; // 진행 상태
        public bool IsFinalBossPhase => false; // 보스전 상태 대체
    }
}
