using System.Collections.Generic; // 실제 뱀 이미지 색상 보관
using ProjectEpsilon.Combat; // 실제 전투 시각 효과
using UnityEngine; // Unity 이미지와 객체
using UnityEngine.SceneManagement; // 실제 씬 전환
namespace ProjectEpsilon.Player // 플레이어 시각 영역
{ // 영역 시작
    [DefaultExecutionOrder(400)] // 실제 전투 구성 후 연결
    public sealed class PlayerCombatVfx : MonoBehaviour // 플레이어 피격·몸통·무적 표시
    { // 표시 시작
        private SnakeMovement player; // 실제 플레이어 머리
        private SnakeHealth health; // 실제 공용 체력
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeInvulnerability invulnerability; // 실제 무적 상태
        private SnakeExternalCollision collision; // 실제 직접 충돌 보호 상태
        private SpriteRenderer ring; // 실제 무적 표시 이미지
        private SpriteRenderer contactProtection; // 실제 직접 충돌 보호 표시
        [SerializeField, Min(0.01f)] private float damageBlinkDuration = 0.6f; // 실제 피격 점멸 지속 시간
        [SerializeField, Min(0.01f)] private float damageBlinkInterval = 0.12f; // 실제 밝기 전환 간격
        [SerializeField, Range(0f, 1f)] private float damageBlinkAlpha = 0.55f; // 실제 점멸 중 투명도 배율
        private readonly Dictionary<SpriteRenderer, float> blinkAlphas = new Dictionary<SpriteRenderer, float>(); // 실제 뱀 이미지의 원래 투명도
        private float damageBlinkStarted; // 실제 피격 점멸 시작 시각
        private float damageBlinkUntil; // 실제 피격 점멸 종료 시각
        private bool damageBlinking; // 실제 피격 점멸 진행 여부
        private bool initialized; // 최초 연결 여부
        private bool subscribed; // 실제 이벤트 구독 여부
        private void Start() // 실제 플레이어 구성 연결
        { // 함수 시작
            initialized = true; // 최초 초기화 완료
            Bind(); // 실제 전투 이벤트 연결
        } // 함수 종료
        private void OnEnable() // 실제 표시 활성화
        { // 함수 시작
            SceneManager.sceneLoaded += OnSceneLoaded; // 실제 씬 전환 구독
            if (initialized) // 최초 초기화 여부 확인
            { // 조건 시작
                Bind(); // 실제 전투 이벤트 재연결
            } // 조건 종료
        } // 함수 종료
        private void OnDisable() // 실제 표시 비활성화
        { // 함수 시작
            SceneManager.sceneLoaded -= OnSceneLoaded; // 실제 씬 전환 해제
            Unbind(); // 실제 전투 이벤트 해제
        } // 함수 종료
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) // 실제 씬 전환 처리
        { // 함수 시작
            if (!initialized) // 최초 초기화 여부 확인
            { // 조건 시작
                return; // 초기화 전 전환 무시
            } // 조건 종료
            Unbind(); // 이전 씬 전투 이벤트 해제
            Bind(); // 새 씬 전투 이벤트 연결
        } // 함수 종료
        private void Bind() // 실제 전투 이벤트 연결
        { // 함수 시작
            if (subscribed) // 기존 구독 확인
            { // 조건 시작
                return; // 중복 구독 방지
            } // 조건 종료
            player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 머리
            body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            health = player == null ? null : player.GetComponent<SnakeHealth>(); // 실제 공용 체력
            invulnerability = player == null ? null : player.GetComponent<SnakeInvulnerability>(); // 실제 무적 상태
            collision = player == null ? null : player.GetComponent<SnakeExternalCollision>(); // 실제 직접 충돌 보호 상태
            if (health != null) // 실제 체력 확인
            { // 조건 시작
                health.DamageApplied += OnDamage; // 실제 공용 피해 구독
            } // 조건 종료
            if (body != null) // 실제 몸통 확인
            { // 조건 시작
                body.BodyRemoved += OnBodyRemoved; // 실제 몸통 손실 위치 구독
            } // 조건 종료
            if (invulnerability != null) // 실제 무적 상태 확인
            { // 조건 시작
                invulnerability.InvulnerabilityStarted += OnInvulnerability; // 실제 무적 시작 구독
            } // 조건 종료
            if (player != null) // 실제 플레이어 확인
            { // 조건 시작
                GameObject instance = new GameObject("PlayerInvulnerabilityVfx"); // 실제 무적 표시 객체
                instance.transform.SetParent(player.transform, false); // 실제 머리 위치 추적
                instance.transform.localScale = Vector3.one * 1.5f; // 실제 무적 표시 크기
                ring = instance.AddComponent<SpriteRenderer>(); // 실제 무적 이미지 표시기
                ring.sprite = CombatVfx.GetInvulnerabilitySprite(); // 실제 무적 이미지
                ring.color = new Color(1f, 1f, 1f, 0.75f); // 실제 무적 이미지 투명도
                ring.sortingOrder = 2; // 몸통과 경고 아래 표시
                instance.SetActive(invulnerability != null && invulnerability.IsInvulnerable && ring.sprite != null); // 실제 무적 상태 표시
                GameObject marker = new GameObject("PlayerContactProtectionVfx"); // 실제 제한 보호 표시 객체
                marker.transform.SetParent(player.transform, false); // 실제 머리 위치 추적
                marker.transform.localScale = Vector3.one * 0.55f; // 실제 제한 보호 표시 크기
                contactProtection = marker.AddComponent<SpriteRenderer>(); // 실제 제한 보호 이미지 표시기
                contactProtection.sprite = CombatVfx.GetBodyBreakSprite(); // 기존 몸통 손실 이미지 재사용
                contactProtection.color = new Color(1f, 0.7f, 0.2f, 0.95f); // 완전 무적과 구분되는 주황색
                contactProtection.sortingOrder = 9; // 플레이어 위와 경고 아래 표시
                marker.SetActive(false); // 첫 실제 상태 갱신 전 숨김
                RefreshContactProtection(); // 실제 제한 보호 상태 반영
            } // 조건 종료
            subscribed = true; // 실제 이벤트 구독 상태
        } // 함수 종료
        private void Unbind() // 실제 전투 이벤트 해제
        { // 함수 시작
            RestoreBlink(); // 기존 뱀 이미지의 투명도 복구
            if (!subscribed) // 기존 구독 확인
            { // 조건 시작
                return; // 중복 해제 방지
            } // 조건 종료
            if (health != null) // 실제 체력 확인
            { // 조건 시작
                health.DamageApplied -= OnDamage; // 실제 공용 피해 해제
            } // 조건 종료
            if (body != null) // 실제 몸통 확인
            { // 조건 시작
                body.BodyRemoved -= OnBodyRemoved; // 실제 몸통 손실 해제
            } // 조건 종료
            if (invulnerability != null) // 실제 무적 확인
            { // 조건 시작
                invulnerability.InvulnerabilityStarted -= OnInvulnerability; // 실제 무적 이벤트 해제
            } // 조건 종료
            if (ring != null) // 실제 무적 표시 확인
            { // 조건 시작
                ring.gameObject.SetActive(false); // 지연 파괴 전 표시 중단
                Destroy(ring.gameObject); // 이전 씬의 무적 표시 제거
            } // 조건 종료
            ring = null; // 이전 표시 참조 해제
            if (contactProtection != null) // 실제 제한 보호 표시 확인
            { // 조건 시작
                contactProtection.gameObject.SetActive(false); // 지연 파괴 전 표시 중단
                Destroy(contactProtection.gameObject); // 이전 씬의 보호 표시 제거
            } // 조건 종료
            contactProtection = null; // 이전 제한 보호 참조 해제
            collision = null; // 이전 충돌 처리기 참조 해제
            subscribed = false; // 실제 구독 상태 초기화
        } // 함수 종료
        private void OnDamage() // 실제 공용 체력 피해
        { // 함수 시작
            BeginBlink(); // 실제 체력 또는 보호막 피격 점멸
        } // 함수 종료
        private void OnBodyRemoved(Vector3 position) // 실제 몸통 손실 위치
        { // 함수 시작
            CombatVfx.SpawnBodyBreak(position); // 실제 몸통 손실 표시
            BeginBlink(); // 실제 직접 충돌과 몸통 손실 점멸
        } // 함수 종료
        private void OnInvulnerability(float duration) // 실제 무적 시작
        { // 함수 시작
            RefreshInvulnerability(); // 실제 무적 표시 갱신
        } // 함수 종료
        private void Update() // 실제 무적 종료 감시
        { // 함수 시작
            RefreshInvulnerability(); // 실제 무적 상태 표시
            RefreshContactProtection(); // 실제 제한 보호 상태 표시
            RefreshBlink(); // 실제 피격 점멸과 색상 복구
        } // 함수 종료
        private void BeginBlink() // 실제 피격 점멸 시작
        { // 함수 시작
            damageBlinkStarted = Time.time; // 실제 점멸 시작 시각
            damageBlinkUntil = Time.time + Mathf.Max(0.01f, damageBlinkDuration); // 실제 점멸 종료 시각
            damageBlinking = true; // 실제 점멸 진행 상태
            RefreshBlink(); // 첫 피격 프레임의 점멸 표시
        } // 함수 종료
        private void RefreshBlink() // 실제 뱀 이미지의 점멸 갱신
        { // 함수 시작
            if (!damageBlinking) // 실제 점멸 진행 확인
            { // 조건 시작
                return; // 불필요한 색상 갱신 생략
            } // 조건 종료
            if (Time.time >= damageBlinkUntil) // 실제 점멸 종료 확인
            { // 조건 시작
                RestoreBlink(); // 실제 원래 투명도 복구
                return; // 완료된 점멸 갱신 종료
            } // 조건 종료
            bool dim = Mathf.FloorToInt((Time.time - damageBlinkStarted) / Mathf.Max(0.01f, damageBlinkInterval)) % 2 == 0; // 실제 점멸 밝기 단계
            float factor = dim ? Mathf.Clamp01(damageBlinkAlpha) : 1f; // 실제 점멸 투명도 배율
            if (player != null) // 실제 머리 확인
            { // 조건 시작
                ApplyBlink(player.GetComponent<SpriteRenderer>(), factor); // 실제 머리 이미지 점멸
            } // 조건 종료
            if (body == null) // 실제 몸통 관리자 확인
            { // 조건 시작
                return; // 몸통 없는 화면 갱신 종료
            } // 조건 종료
            foreach (SnakeSegment segment in body.BodySegments) // 실제 현재 몸통 순회
            { // 반복 시작
                if (segment != null) // 실제 몸통 이미지 확인
                { // 조건 시작
                    ApplyBlink(segment.GetComponent<SpriteRenderer>(), factor); // 실제 몸통 이미지 점멸
                } // 조건 종료
            } // 반복 종료
            if (body.TailSegment != null) // 실제 꼬리 확인
            { // 조건 시작
                ApplyBlink(body.TailSegment.GetComponent<SpriteRenderer>(), factor); // 실제 꼬리 이미지 점멸
            } // 조건 종료
        } // 함수 종료
        private void ApplyBlink(SpriteRenderer renderer, float factor) // 실제 뱀 이미지의 투명도 조정
        { // 함수 시작
            if (renderer == null) // 실제 이미지 확인
            { // 조건 시작
                return; // 누락 이미지 생략
            } // 조건 종료
            if (!blinkAlphas.TryGetValue(renderer, out float original)) // 실제 원래 투명도 확인
            { // 조건 시작
                original = renderer.color.a; // 실제 원래 투명도 보관
                blinkAlphas.Add(renderer, original); // 실제 이미지별 투명도 저장
            } // 조건 종료
            Color color = renderer.color; // 실제 현재 이미지 색상
            color.a = original * factor; // 실제 원래 투명도 기준 점멸
            renderer.color = color; // 실제 점멸 색상 적용
        } // 함수 종료
        private void RestoreBlink() // 실제 뱀 이미지의 원래 투명도 복구
        { // 함수 시작
            foreach (KeyValuePair<SpriteRenderer, float> entry in blinkAlphas) // 실제 점멸 이미지 순회
            { // 반복 시작
                if (entry.Key == null) // 이미 제거된 몸통 확인
                { // 조건 시작
                    continue; // 제거된 이미지 생략
                } // 조건 종료
                Color color = entry.Key.color; // 실제 현재 이미지 색상
                color.a = entry.Value; // 실제 원래 투명도 복구
                entry.Key.color = color; // 실제 복구 색상 적용
            } // 반복 종료
            blinkAlphas.Clear(); // 실제 이전 이미지 기록 제거
            damageBlinking = false; // 실제 점멸 종료 상태
        } // 함수 종료
        private void RefreshContactProtection() // 실제 적 직접 충돌 보호 표시
        { // 함수 시작
            if (contactProtection == null) // 실제 표시기 확인
            { // 조건 시작
                return; // 표시기 부재 시 갱신 생략
            } // 조건 종료
            bool visible = contactProtection.sprite != null && collision != null && collision.EnemyContactProtected; // 실제 제한 보호 상태
            if (contactProtection.gameObject.activeSelf != visible) // 실제 표시 상태 변경 확인
            { // 조건 시작
                contactProtection.gameObject.SetActive(visible); // 실제 제한 보호 시작과 종료 반영
            } // 조건 종료
            if (visible && player != null) // 실제 표시 위치 확인
            { // 조건 시작
                contactProtection.transform.position = player.transform.position + Vector3.up * 0.85f; // 화면 위쪽의 독립 표시 위치
                contactProtection.transform.rotation = Quaternion.identity; // 머리 회전과 독립된 표시 방향
            } // 조건 종료
        } // 함수 종료
        private void RefreshInvulnerability() // 실제 무적 상태 반영
        { // 함수 시작
            if (ring != null) // 실제 표시기 확인
            { // 조건 시작
                bool visible = ring.sprite != null && invulnerability != null && invulnerability.IsInvulnerable; // 실제 유효 무적 상태
                if (ring.gameObject.activeSelf != visible) // 실제 표시 상태 변경 확인
                { // 조건 시작
                    ring.gameObject.SetActive(visible); // 실제 무적 시작과 종료 반영
                } // 조건 종료
            } // 조건 종료
        } // 함수 종료
    } // 표시 종료
} // 영역 종료
