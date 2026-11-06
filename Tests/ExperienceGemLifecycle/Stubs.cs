using System; // 기본 형식
using System.Collections.Generic; // 객체 목록
using System.Reflection; // 초기화 호출

namespace UnityEngine // Unity 대체 형식
{ // 영역 시작
    public enum FindObjectsSortMode // 객체 검색 순서
    { // 열거 시작
        None // 정렬 없음
    } // 열거 끝
    public class Object // 객체 대체
    { // 클래스 시작
        private static int nextId = 1; // 다음 객체 번호
        private readonly int id = nextId++; // 객체 고유 번호
        public static readonly List<ProjectEpsilon.Progression.ExperiencePickup> Pickups = new List<ProjectEpsilon.Progression.ExperiencePickup>(); // 생성된 보석
        public int GetInstanceID() => id; // 객체 번호 조회
        public static T[] FindObjectsByType<T>(FindObjectsSortMode sort) where T : class // 활성 객체 검색
        { // 메서드 시작
            List<T> result = new List<T>(); // 검색 결과
            foreach (ProjectEpsilon.Progression.ExperiencePickup pickup in Pickups) // 보석 순회
            { // 반복 시작
                if (pickup.gameObject.activeInHierarchy && pickup.isActiveAndEnabled && pickup is T item) // 활성 객체 확인
                { // 조건 시작
                    result.Add(item); // 검색 결과 추가
                } // 조건 끝
            } // 반복 끝
            return result.ToArray(); // 검색 결과 반환
        } // 메서드 끝
        public static void Destroy(GameObject value) // 지연 제거 대체
        { // 메서드 시작
            value.PendingDestroy = true; // 제거 예약만 기록
        } // 메서드 끝
    } // 클래스 끝
    public class MonoBehaviour : Object // Unity 동작 대체
    { // 클래스 시작
        public GameObject gameObject; // 소속 객체
        public Transform transform => gameObject.transform; // 객체 위치
        public bool isActiveAndEnabled = true; // 활성 상태
        public T GetComponent<T>() where T : class => gameObject.GetComponent<T>(); // 구성 조회
    } // 클래스 끝
    public sealed class GameObject // 씬 객체 대체
    { // 클래스 시작
        public string name; // 객체 이름
        public bool activeInHierarchy = true; // 계층 활성 여부
        public bool PendingDestroy; // 제거 예약 여부
        public Transform transform = new Transform(); // 객체 위치
        private readonly Dictionary<Type, object> components = new Dictionary<Type, object>(); // 객체 구성 목록
        public GameObject(string objectName) // 객체 생성
        { // 생성자 시작
            name = objectName; // 이름 저장
            components[typeof(SpriteRenderer)] = new SpriteRenderer(); // 필수 렌더러 생성
        } // 생성자 끝
        public T AddComponent<T>() where T : new() // 구성 추가
        { // 메서드 시작
            T component = new T(); // 구성 생성
            components[typeof(T)] = component; // 구성 저장
            if (component is MonoBehaviour behavior) // 동작 구성 확인
            { // 조건 시작
                behavior.gameObject = this; // 소속 객체 연결
                typeof(T).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(component, null); // 초기화 호출
            } // 조건 끝
            if (component is ProjectEpsilon.Progression.ExperiencePickup pickup) // 보석 생성 확인
            { // 조건 시작
                Object.Pickups.Add(pickup); // 보석 검색 목록 추가
            } // 조건 끝
            return component; // 구성 반환
        } // 메서드 끝
        public T GetComponent<T>() where T : class => components.TryGetValue(typeof(T), out object value) ? value as T : null; // 구성 조회
    } // 클래스 끝
    public sealed class Transform // 위치 대체
    { // 클래스 시작
        public Vector3 position; // 월드 위치
        public Vector3 localScale; // 표시 크기
    } // 클래스 끝
    public struct Vector3 // 좌표 대체
    { // 구조체 시작
        public float x; // 가로 좌표
        public float y; // 세로 좌표
        public float z; // 깊이 좌표
        public Vector3(float a, float b, float c) // 좌표 생성
        { // 생성자 시작
            x = a; // 가로 저장
            y = b; // 세로 저장
            z = c; // 깊이 저장
        } // 생성자 끝
        public static Vector3 zero => new Vector3(0f, 0f, 0f); // 원점
        public float sqrMagnitude => x * x + y * y + z * z; // 거리 제곱
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); // 위치 차이
        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float distance) // 목표 방향 이동
        { // 메서드 시작
            Vector3 delta = target - current; // 목표 거리
            float length = (float)Math.Sqrt(delta.sqrMagnitude); // 실제 거리
            if (length <= distance || length == 0f) // 도착 여부
            { // 조건 시작
                return target; // 목표 위치 반환
            } // 조건 끝
            return new Vector3(current.x + delta.x * distance / length, current.y + delta.y * distance / length, current.z + delta.z * distance / length); // 이동한 위치 반환
        } // 메서드 끝
    } // 구조체 끝
    public sealed class Camera // 카메라 대체
    { // 클래스 시작
        public static Camera main; // 현재 카메라
        public Vector3 WorldToViewportPoint(Vector3 position) => new Vector3(position.x / 10f + 0.5f, position.y / 10f + 0.5f, 1f); // 단순 화면 좌표
    } // 클래스 끝
    public sealed class Sprite // 스프라이트 대체
    { // 클래스 시작
    } // 클래스 끝
    public sealed class SpriteRenderer // 렌더러 대체
    { // 클래스 시작
        public Sprite sprite; // 이미지
        public Color color; // 표시 색상
        public int sortingOrder; // 표시 순서
    } // 클래스 끝
    public readonly struct Color // 색상 대체
    { // 구조체 시작
        public Color(float r, float g, float b, float a) // 색상 구성
        { // 생성자 시작
        } // 생성자 끝
        public Color(float r, float g, float b) // 불투명 색상 구성
        { // 생성자 시작
        } // 생성자 끝
    } // 구조체 끝
    public sealed class RequireComponent : Attribute // 필수 구성 표시
    { // 클래스 시작
        public RequireComponent(Type type) // 필수 구성 설정
        { // 생성자 시작
        } // 생성자 끝
    } // 클래스 끝
    public sealed class SerializeField : Attribute // 직렬화 표시
    { // 클래스 시작
    } // 클래스 끝
    public sealed class MinAttribute : Attribute // 최소값 표시
    { // 클래스 시작
        public MinAttribute(float value) // 최소값 설정
        { // 생성자 시작
        } // 생성자 끝
    } // 클래스 끝
    public static class Application // 실행 상태 대체
    { // 클래스 시작
        public static bool isPlaying = true; // 플레이 상태
    } // 클래스 끝
    public static class Time // 시간 대체
    { // 클래스 시작
        public static float deltaTime; // 게임 프레임 경과 시간
    } // 클래스 끝
    public static class Mathf // 수학 대체
    { // 클래스 시작
        public static int Max(int a, int b) => Math.Max(a, b); // 정수 최댓값
        public static int Min(int a, int b) => Math.Min(a, b); // 정수 최솟값
        public static float Max(float a, float b) => Math.Max(a, b); // 실수 최댓값
    } // 클래스 끝
} // 영역 끝

namespace UnityEngine.SceneManagement // 씬 전환 대체
{ // 영역 시작
    public readonly struct Scene // 씬 정보 대체
    { // 구조체 시작
    } // 구조체 끝
    public enum LoadSceneMode // 씬 로드 방식
    { // 열거 시작
        Single // 단일 씬 로드
    } // 열거 끝
    public static class SceneManager // 씬 관리자 대체
    { // 클래스 시작
        public static event Action<Scene, LoadSceneMode> sceneLoaded; // 씬 로드 알림
        public static void SimulateLoad() // 시험용 씬 전환
        { // 메서드 시작
            sceneLoaded?.Invoke(new Scene(), LoadSceneMode.Single); // 씬 변경 이벤트 전달
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Core // 게임 상태 대체
{ // 영역 시작
    public sealed class GameManager // 게임 관리자
    { // 클래스 시작
        public static GameManager Instance; // 현재 관리자
        public bool IsPlaying = true; // 진행 상태
    } // 클래스 끝
} // 영역 끝

namespace ProjectEpsilon.Progression // 경험치 수신자 대체
{ // 영역 시작
    public sealed class SnakeExperience // 경험치 상태
    { // 클래스 시작
        public static SnakeExperience Current; // 현재 수신자
        public UnityEngine.Transform transform = new UnityEngine.Transform(); // 머리 위치
        public int Experience; // 누적 경험치
        public void AddExperience(int amount) // 경험치 획득
        { // 메서드 시작
            Experience += amount; // 경험치 누적
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
