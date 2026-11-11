using System; // 이벤트 형식
using System.Collections.Generic; // 시험 객체 목록

namespace UnityEngine // 유니티 대체 영역
{
    public class Object // 유니티 객체 대체
    {
        public static readonly Dictionary<Type, object> Found = new Dictionary<Type, object>(); // 시험 검색 객체 목록
        public static T FindFirstObjectByType<T>() where T : class => Found.TryGetValue(typeof(T), out object item) ? item as T : null; // 시험 객체 검색
        public static T FindFirstObjectByType<T>(FindObjectsInactive ignored) where T : class => FindFirstObjectByType<T>(); // 비활성 검색 대체
        public static void Destroy(object ignored) // 객체 파괴 대체
        {
        }
        public static void DontDestroyOnLoad(object ignored) // 씬 유지 대체
        {
        }
    }
    public class MonoBehaviour : Object // 유니티 컴포넌트 대체
    {
        public object gameObject = new object(); // 시험 오브젝트
    }
    public sealed class SerializeField : Attribute // 직렬화 속성 대체
    {
    }
    public enum FindObjectsInactive // 비활성 검색 설정 대체
    {
        Include // 비활성 객체 포함
    }
    public static class Time // 게임 시간 배율 대체
    {
        public static float timeScale = 1f; // 현재 시험 배율
    }
    public static class Mathf // 유니티 수학 대체
    {
        public static float Clamp(float value, float minimum, float maximum) => Math.Clamp(value, minimum, maximum); // 범위 제한 대체
    }
    public struct Rect // 화면 영역 대체
    {
        public Rect(float x, float y, float width, float height) // 화면 영역 구성
        {
        }
    }
    public static class Screen // 화면 크기 대체
    {
        public static int width = 1920; // 시험 화면 너비
        public static int height = 1080; // 시험 화면 높이
    }
    public static class GUI // 결과 화면 대체
    {
        public static void Box(Rect rect, string text) // 결과 표시 대체
        {
        }
    }
}
namespace ProjectEpsilon.Player // 플레이어 대체 영역
{
    public sealed class SnakeHealth // 몸통 소진 대체
    {
        public event Action BodyDepleted; // 몸통 소진 알림
        public void Deplete() // 실제 몸통 소진 대체
        {
            BodyDepleted?.Invoke(); // 실제 구독 이벤트 전달
        }
    }
}
namespace ProjectEpsilon.Progression // 시작 화면 대체 영역
{
    public sealed class StartingWeaponSelection // 시작 선택기 대체
    {
    }
}
namespace ProjectEpsilon.Combat // 전투 대상 대체 영역
{
    public sealed class WeaponTarget // 실제 보스 사망 이벤트 대체
    {
        public event Action<WeaponTarget> Died; // 사망 알림
        public bool IsAlive { get; private set; } = true; // 대상 생존 상태
        public void Kill() // 실제 사망 이벤트 대체
        {
            if (!IsAlive) // 중복 사망 확인
            {
                return; // 중복 알림 방지
            }
            IsAlive = false; // 사망 상태 저장
            Died?.Invoke(this); // 실제 구독 이벤트 전달
        }
    }
}
