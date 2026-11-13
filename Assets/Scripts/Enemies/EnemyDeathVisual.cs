using UnityEngine; // Unity 표시와 시간
namespace ProjectEpsilon.Enemies // 적 시각 영역
{ // 영역 시작
    public sealed class EnemyDeathVisual : MonoBehaviour // 충돌 없는 임시 사망 표시
    { // 표시 시작
        private SpriteRenderer artwork; // 실제 사망 이미지
        private Color initialColor; // 실제 사망 색상
        private Vector3 initialScale; // 실제 사망 크기
        private float duration; // 임시 사망 표시 시간
        private float elapsed; // 경과 표시 시간
        public void Configure(SpriteRenderer sprite, float seconds) // 실제 사망 표시 설정
        { // 함수 시작
            artwork = sprite; // 실제 사망 이미지 저장
            initialColor = sprite.color; // 실제 사망 색상 저장
            initialScale = transform.localScale; // 실제 사망 크기 저장
            duration = Mathf.Max(0.01f, seconds); // 안전한 표시 시간
        } // 함수 종료
        private void Update() // 사망 이미지 축소와 제거
        { // 함수 시작
            if (artwork == null) // 실제 이미지 확인
            { // 조건 시작
                Destroy(gameObject); // 무효 표시 정리
                return; // 실제 갱신 중단
            } // 조건 종료
            elapsed += Time.unscaledDeltaTime; // 승패 정지 중에도 표시 시간 진행
            float progress = Mathf.Clamp01(elapsed / duration); // 실제 표시 진행률
            artwork.color = new Color(initialColor.r, initialColor.g, initialColor.b, initialColor.a * (1f - progress)); // 실제 사망 색상 투명화
            transform.localScale = initialScale * (1f - 0.35f * progress); // 실제 사망 이미지 축소
            if (progress >= 1f) // 실제 표시 종료 확인
            { // 조건 시작
                Destroy(gameObject); // 종료된 사망 표시 제거
            } // 조건 종료
        } // 함수 종료
    } // 표시 종료
} // 영역 종료
