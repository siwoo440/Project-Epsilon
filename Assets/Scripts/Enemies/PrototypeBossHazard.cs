using ProjectEpsilon.Combat; // 보스 전투 상태
using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Player; // 플레이어 구성
using UnityEngine; // 유니티 기능

using ProjectEpsilon.Recording; // 실제 피해 원인 기록

namespace ProjectEpsilon.Enemies // 적 영역
{
    public sealed class PrototypeBossHazard : MonoBehaviour // 시험 보스 임시 위험 구역
    {
        private SnakeMovement player; // 실제 플레이어 머리
        private SnakeBodyManager body; // 실제 플레이어 몸통
        private SnakeHealth health; // 실제 공용 체력
        private WeaponTarget boss; // 실제 보스 상태
        private LineRenderer outline; // 예고와 위험 표시선
        private Material outlineMaterial; // 표시선 임시 재질
        private float duration; // 위험 지속 시간
        private float damageInterval; // 피해 간격
        private float radius; // 위험 반경
        private int damage; // 확정 피해
        private bool rectangular; // 폐허 구획형 위험 여부
        private bool toxicSpore; // 습지 독성 포자 위험 여부
        private float phaseEndsAt; // 현재 단계 종료 시각
        private float nextDamageAt; // 다음 피해 시각
        public bool IsWarning { get; private set; } // 실제 예고 상태
        public bool IsActive { get; private set; } // 실제 위험 상태

        public void Configure(SnakeMovement head, SnakeBodyManager segments, SnakeHealth sharedHealth, WeaponTarget owner, float warningTime, float activeTime, float hitInterval, float range, int hitDamage, int pattern = 0) // 실제 위험 구역 설정
        {
            player = head; // 실제 머리 저장
            body = segments; // 실제 몸통 저장
            health = sharedHealth; // 실제 체력 저장
            boss = owner; // 실제 보스 저장
            duration = Mathf.Max(0.1f, activeTime); // 안전한 위험 시간
            damageInterval = Mathf.Max(0.1f, hitInterval); // 안전한 피해 간격
            radius = Mathf.Max(0.1f, range); // 안전한 위험 반경
            damage = Mathf.Max(0, hitDamage); // 안전한 확정 피해
            rectangular = pattern == 1; // 폐허 구획형 위험 설정
            toxicSpore = pattern == 2; // 습지 독성 포자 위험 설정
            phaseEndsAt = Time.time + Mathf.Max(0.1f, warningTime); // 예고 종료 시각
            IsWarning = true; // 실제 예고 시작
            outline = gameObject.AddComponent<LineRenderer>(); // 실제 원형 표시선
            outlineMaterial = new Material(Shader.Find("Sprites/Default")); // 시험용 표시 재질
            outline.material = outlineMaterial; // 실제 표시 재질 연결
            outline.startWidth = 0.1f; // 표시선 시작 두께
            outline.endWidth = 0.1f; // 표시선 끝 두께
            outline.sortingOrder = 30; // 전투 위 표시 순서
            outline.loop = true; // 원형 표시 연결
            outline.positionCount = rectangular ? 4 : 32; // 구획 또는 원형 표시 좌표 수
            for (int index = 0; index < outline.positionCount; index++) // 실제 표시 좌표 순회
            { // 영역 시작
                if (rectangular) // 폐허 구획형 예고 확인
                { // 영역 시작
                    float x = index == 0 || index == 3 ? -radius : radius; // 실제 구획 가로 경계
                    float y = index < 2 ? -radius : radius; // 실제 구획 세로 경계
                    outline.SetPosition(index, transform.position + new Vector3(x, y, 0f)); // 실제 구획 모서리
                } // 영역 종료
                else // 기존 원형 예고 확인
                { // 영역 시작
                    float angle = index * Mathf.PI * 2f / outline.positionCount; // 실제 원형 각도
                    outline.SetPosition(index, transform.position + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f)); // 실제 원형 좌표
                } // 영역 종료
            } // 영역 종료
            SetColor(toxicSpore ? new Color(0.35f, 1f, 0.35f) : Color.yellow); // 맵별 예고 색상 표시
        }

        private void Update() // 보스 사망 시 정지 중 구역 정리
        {
            if (boss == null || !boss.IsAlive) // 실제 보스 생존 확인
            {
                RemoveHazard(); // 남은 위험 구역 제거
            }
        }

        private void FixedUpdate() // 실제 예고와 주기 피해
        {
            if (!isActiveAndEnabled || boss == null || !boss.IsAlive || GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 실제 전투 가능 상태
            {
                return; // 정지 중 위험 진행 방지
            }
            if (IsWarning && Time.time >= phaseEndsAt) // 실제 예고 종료 확인
            {
                IsWarning = false; // 예고 종료
                IsActive = true; // 실제 위험 시작
                phaseEndsAt = Time.time + duration; // 위험 종료 시각
                nextDamageAt = Time.time; // 첫 피해 시각
                SetColor(toxicSpore ? new Color(0.7f, 0.2f, 0.85f) : Color.red); // 맵별 활성 위험 색상 표시
            }
            if (!IsActive) // 실제 위험 단계 확인
            {
                return; // 예고 중 피해 방지
            }
            if (Time.time >= phaseEndsAt) // 실제 위험 종료 확인
            {
                RemoveHazard(); // 종료된 구역 제거
                return; // 종료 후 피해 방지
            }
            if (Time.time >= nextDamageAt) // 실제 피해 주기 확인
            {
                nextDamageAt = Time.time + damageInterval; // 다음 피해 시각
                ApplyDamage(); // 실제 접촉 피해 예약
            }
        }

        private void ApplyDamage() // 실제 구역 내 플레이어 피해
        {
            if (player == null || body == null || health == null || damage <= 0) // 실제 피해 대상 확인
            {
                return; // 무효 피해 제외
            }
            Collider2D[] contacts = rectangular ? Physics2D.OverlapBoxAll(transform.position, Vector2.one * (radius * 2f), 0f) : Physics2D.OverlapCircleAll(transform.position, radius); // 실제 구획 또는 원형 접촉
            foreach (Collider2D contact in contacts) // 실제 구역 접촉 순회
            {
                SnakeSegment segment = contact.GetComponent<SnakeSegment>(); // 실제 접촉 몸통 조회
                bool isHead = contact.transform == player.transform; // 실제 머리 접촉 여부
                bool isBody = segment != null && segment.transform.IsChildOf(body.transform); // 실제 플레이어 몸통 여부
                if (!isHead && !isBody) // 실제 플레이어 접촉 확인
                {
                    continue; // 다른 충돌 제외
                }
                SnakeExternalCollision resolver = player.GetComponent<SnakeExternalCollision>(); // 기존 물리 충돌 처리기
                if (resolver == null || !resolver.isActiveAndEnabled) // 실제 처리기 확인
                {
                    Debug.LogError("[Project Epsilon] 보스 위험 구역 충돌 처리기가 없습니다."); // 처리 누락 기록
                    return; // 처리기 없는 피해 방지
                }
                resolver.QueueHealthDamage(health, damage, DamageOrigin.BossHazard); // 기존 물리 우선순위에 피해 예약
                return; // 머리와 몸통 중복 피해 방지
            }
        }

        private void SetColor(Color color) // 실제 단계별 원형 색상
        {
            outline.startColor = color; // 표시선 시작 색상
            outline.endColor = color; // 표시선 끝 색상
        }
        private void RemoveHazard() // 실제 구역 동작과 표시 종료
        {
            IsWarning = false; // 예고 상태 해제
            IsActive = false; // 위험 상태 해제
            gameObject.SetActive(false); // 지연 파괴 전 동작 차단
            Destroy(gameObject); // 실제 구역 제거
        }
        private void OnDestroy() // 실제 표시 재질 정리
        {
            if (outlineMaterial != null) // 실제 재질 확인
            {
                Destroy(outlineMaterial); // 생성한 재질 제거
            }
        }
    }
}
