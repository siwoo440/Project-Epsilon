using ProjectEpsilon.Core; // 대상 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Combat // 프로젝트 기능 영역
{
    [RequireComponent(typeof(SpriteRenderer))] // 구성 요소·이미지 설정
    public sealed class WeaponAttackPulse : MonoBehaviour // 포탑·공격 자료 구조
    {
        private SpriteRenderer spriteRenderer; // 이미지·렌더러 구성 선언
        private Color startColor = Color.white; // 색상·시작 구성 선언
        private float duration = 0.15f; // 지속 시간 구성 선언
        private float remainingTime = 0.15f; // 남은 값·시간 구성 선언

        private void Awake() // 초기화 기능 선언
        {
            spriteRenderer = GetComponent<SpriteRenderer>(); // 이미지·렌더러 값 설정
        }

        public void Configure( // 구성 호출 시작
            Sprite visual, // 이미지·시각 효과 항목 전달
            float radius, // 반경 항목 전달
            Color color, // 색상 항목 전달
            float lifetime // 대상 처리
        ) // 앞선 항목 종료
        {
            if (spriteRenderer == null) // 이미지·렌더러 조건 확인
            {
                spriteRenderer = GetComponent<SpriteRenderer>(); // 이미지·렌더러 값 설정
            }

            float safeRadius = Mathf.Max(0.05f, radius); // 반경·최대 값 설정

            startColor = color; // 시작·색상 값 설정
            duration = Mathf.Max(0.05f, lifetime); // 지속 시간·최대 값 설정
            remainingTime = duration; // 남은 값·시간 값 설정

            spriteRenderer.sprite = visual; // 이미지·렌더러 값 설정
            spriteRenderer.color = startColor; // 이미지·렌더러 값 설정
            spriteRenderer.sortingOrder = 18; // 이미지·렌더러 값 설정

            float diameter = safeRadius * 2f; // 반경 값 설정

            transform.localScale = new Vector3( // 위치·크기 호출 시작
                diameter, // 대상 항목 전달
                diameter, // 대상 항목 전달
                1f // 대상 처리
            ); // 앞선 항목 종료
        }

        private void Update() // 갱신 기능 선언
        {
            if (GameManager.Instance != null && // 게임·관리자 조건 확인
                !GameManager.Instance.IsPlaying) // 게임·관리자 처리
            {
                return; // 현재 기능 종료
            }

            remainingTime -= Time.deltaTime; // 남은 값·시간 값 설정

            if (remainingTime <= 0f) // 남은 값·시간 조건 확인
            {
                Destroy(gameObject); // 제거·게임 기능 실행
                return; // 현재 기능 종료
            }

            if (spriteRenderer == null) // 이미지·렌더러 조건 확인
            {
                return; // 현재 기능 종료
            }

            float alpha = Mathf.Clamp01(remainingTime / duration); // 남은 값·시간 값 설정
            Color fadedColor = startColor; // 색상·시작 값 설정
            fadedColor.a *= alpha; // 색상 값 설정
            spriteRenderer.color = fadedColor; // 이미지·렌더러 값 설정
        }
    }
}
