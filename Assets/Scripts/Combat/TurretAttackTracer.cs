using ProjectEpsilon.Core; // 게임 진행 상태
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Combat // 전투 영역
{
    public sealed class TurretAttackTracer : MonoBehaviour // 즉시 공격의 시각 전용 비행 탄
    {
        private Vector3 start; // 실제 몸통 발사 위치
        private Vector3 destination; // 발사 시점 표적 위치
        private float duration; // 실제 표시 시간
        private float elapsed; // 현재 비행 시간
        public static void Spawn(Vector3 origin, Vector3 target, Sprite artwork, Color tint) // 실제 발사 표시 생성
        {
            if (artwork == null || (target - origin).sqrMagnitude < 0.0001f) // 이미지와 비행 거리 확인
            {
                return; // 무효 표시 생성 방지
            }
            GameObject item = new GameObject("TurretAttackTracer"); // 실제 시각 탄 객체
            item.transform.position = origin; // 실제 몸통 발사 위치
            Vector2 direction = target - origin; // 실제 표적 방향
            item.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f); // 실제 탄 방향
            item.transform.localScale = Vector3.one * 0.6f; // 실제 탄 표시 크기
            SpriteRenderer renderer = item.AddComponent<SpriteRenderer>(); // 실제 탄 표시기
            renderer.sprite = artwork; // 실제 탄 이미지
            renderer.color = tint; // 포탑 속성 색상
            renderer.sortingOrder = 21; // 적과 바닥 위 표시
            TurretAttackTracer tracer = item.AddComponent<TurretAttackTracer>(); // 실제 비행 연출
            tracer.start = origin; // 발사 위치 저장
            tracer.destination = target; // 발사 시점 표적 저장
            tracer.duration = Mathf.Clamp(direction.magnitude / 18f, 0.12f, 1.2f); // 실제 거리별 표시 시간
        }
        private void Update() // 실제 시각 탄 비행
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 실제 게임 정지 확인
            {
                return; // 정지 중 비행 중단
            }
            elapsed += Time.deltaTime; // 실제 게임 시간 누적
            transform.position = Vector3.Lerp(start, destination, Mathf.Clamp01(elapsed / duration)); // 발사 지점부터 대상까지 이동
            if (elapsed >= duration) // 실제 비행 완료 확인
            {
                Destroy(gameObject); // 시각 탄 제거
            }
        }
    }
}
