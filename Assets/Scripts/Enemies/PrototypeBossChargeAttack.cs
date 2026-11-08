using ProjectEpsilon.Combat; // 적 상태와 공격 배율
using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Player; // 플레이어 체력과 충돌
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Enemies // 적 영역
{ // 영역 시작
    [DefaultExecutionOrder(-20)] // 기존 사수 이동 이전 실행
    public sealed class PrototypeBossChargeAttack : MonoBehaviour // 시험 보스 예고 돌진
    { // 클래스 시작
        private PrototypeBossEncounter encounter; // 실제 보스 단계
        private SnakeMovement player; // 실제 플레이어 머리
        private SnakeBodyManager body; // 실제 플레이어 몸통
        private SnakeHealth health; // 실제 공용 체력
        private EarlyEnemyBehaviour ordinaryMovement; // 기존 사수 행동
        private Rigidbody2D physicsBody; // 실제 보스 물리체
        private WeaponTarget target; // 실제 보스 전투 상태
        private WeaponTargetStatusController statuses; // 실제 이동 상태 효과
        private LineRenderer warning; // 실제 돌진 예고선
        private Material warningMaterial; // 예고선 임시 재질
        private float chargeInterval; // 다음 돌진 간격 임시값
        private float warningDuration; // 돌진 예고 시간 임시값
        private float chargeDuration; // 돌진 지속 시간 임시값
        private float chargeSpeed; // 돌진 속도 임시값
        private float phaseTwoSpeedMultiplier; // 두 번째 단계 속도 임시 배율
        private int damage; // 첫 단계 HP 피해 임시값
        private float nextChargeAt; // 다음 돌진 시작 시각
        private float phaseEndsAt; // 예고 또는 돌진 종료 시각
        private Vector2 direction; // 예고한 실제 돌진 방향
        private bool warningActive; // 실제 예고 단계
        private bool charging; // 실제 돌진 단계
        private bool cutAttack; // 예고 시 확정한 절단 여부
        private bool hitHandled; // 돌진당 성공 처리 여부
        private bool cutPending; // 같은 스텝 절단 대기
        private bool previousMovementEnabled; // 기존 행동 활성 상태
        public bool IsWarning => warningActive; // 실제 예고 상태 조회
        public bool IsCharging => charging; // 실제 돌진 상태 조회

        public void Configure(PrototypeBossEncounter owner, SnakeMovement head, SnakeBodyManager segments, SnakeHealth sharedHealth, EarlyEnemyBehaviour movement, float interval, float warningTime, float duration, float speed, float secondPhaseSpeed, int hitDamage) // 실제 보스 돌진 구성
        { // 메서드 시작
            encounter = owner; // 실제 보스 단계 저장
            player = head; // 실제 머리 저장
            body = segments; // 실제 몸통 저장
            health = sharedHealth; // 실제 공용 체력 저장
            ordinaryMovement = movement; // 기존 사수 행동 저장
            physicsBody = GetComponent<Rigidbody2D>(); // 실제 보스 물리체 조회
            target = GetComponent<WeaponTarget>(); // 실제 보스 대상 조회
            statuses = GetComponent<WeaponTargetStatusController>(); // 실제 상태 효과 조회
            chargeInterval = Mathf.Max(0.1f, interval); // 안전한 공격 간격
            warningDuration = Mathf.Max(0.1f, warningTime); // 안전한 예고 시간
            chargeDuration = Mathf.Max(0.1f, duration); // 안전한 돌진 시간
            chargeSpeed = Mathf.Max(0.1f, speed); // 안전한 돌진 속도
            phaseTwoSpeedMultiplier = Mathf.Max(1f, secondPhaseSpeed); // 안전한 강화 배율
            damage = Mathf.Max(0, hitDamage); // 안전한 HP 피해
            nextChargeAt = Time.time + chargeInterval; // 첫 돌진 시각
        } // 메서드 끝

        private void FixedUpdate() // 실제 예고와 직선 돌진
        { // 메서드 시작
            if (encounter == null || player == null || physicsBody == null || target == null || !target.IsAlive || GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 실제 전투 가능 상태 확인
            { // 조건 시작
                return; // 전투 정지 중 동작 중단
            } // 조건 끝
            if (!warningActive && !charging && Time.time >= nextChargeAt) // 다음 돌진 준비 확인
            { // 조건 시작
                BeginWarning(); // 실제 돌진 예고 시작
            } // 조건 끝
            if (warningActive) // 실제 예고 단계 확인
            { // 조건 시작
                if (Time.time >= phaseEndsAt) // 실제 예고 종료 확인
                { // 조건 시작
                    warningActive = false; // 예고 단계 종료
                    charging = true; // 실제 돌진 단계 시작
                    phaseEndsAt = Time.time + Mathf.Max(0.1f, chargeDuration); // 실제 돌진 종료 시각
                    warning.enabled = false; // 종료된 예고선 숨김
                } // 조건 끝
                return; // 예고 또는 돌진 준비 대기
            } // 조건 끝
            if (!charging) // 실제 돌진 상태 확인
            { // 조건 시작
                return; // 기존 사수 행동 유지
            } // 조건 끝
            if (Time.time >= phaseEndsAt) // 실제 돌진 완료 확인
            { // 조건 시작
                FinishCharge(); // 기존 사수 행동 복구
                return; // 완료 프레임 이동 생략
            } // 조건 끝
            float statusMultiplier = statuses == null ? 1f : statuses.CurrentMovementMultiplier; // 실제 상태 이동 배율
            float speed = Mathf.Max(0.1f, chargeSpeed) * (cutAttack ? Mathf.Max(1f, phaseTwoSpeedMultiplier) : 1f); // 단계별 돌진 속도
            physicsBody.MovePosition(physicsBody.position + direction * speed * statusMultiplier * Time.fixedDeltaTime); // 예고한 방향의 실제 물리 이동
        } // 메서드 끝

        private void BeginWarning() // 실제 돌진 방향 예고
        { // 메서드 시작
            Vector2 offset = (Vector2)player.transform.position - physicsBody.position; // 실제 머리 방향
            direction = offset.sqrMagnitude > 0.001f ? offset.normalized : Vector2.up; // 안전한 돌진 방향
            cutAttack = encounter.IsPhaseTwo; // 예고 시 절단 유형 확정
            hitHandled = false; // 이전 돌진 명중 초기화
            cutPending = false; // 이전 절단 대기 초기화
            previousMovementEnabled = ordinaryMovement != null && ordinaryMovement.enabled; // 기존 사수 활성 상태 보존
            if (ordinaryMovement != null) // 기존 사수 행동 확인
            { // 조건 시작
                ordinaryMovement.enabled = false; // 예고 중 사격과 추적 중지
            } // 조건 끝
            if (warning == null) // 실제 예고선 부재 확인
            { // 조건 시작
                warning = gameObject.AddComponent<LineRenderer>(); // 실제 예고선 생성
                warningMaterial = new Material(Shader.Find("Sprites/Default")); // 시험용 예고 재질
                warning.material = warningMaterial; // 실제 예고 재질 연결
                warning.startColor = cutAttack ? Color.magenta : Color.red; // 첫 예고 색상
                warning.endColor = warning.startColor; // 끝 예고 색상
                warning.startWidth = 0.18f; // 예고선 시작 두께 임시값
                warning.endWidth = 0.18f; // 예고선 끝 두께 임시값
                warning.sortingOrder = 30; // 전투 위 예고 순서
                warning.positionCount = 2; // 실제 예고선 좌표 수
            } // 조건 끝
            warning.startColor = cutAttack ? Color.magenta : Color.red; // 단계별 실제 예고 색상
            warning.endColor = warning.startColor; // 실제 예고 끝 색상
            float distance = Mathf.Max(0.1f, chargeSpeed) * Mathf.Max(0.1f, chargeDuration) * (cutAttack ? Mathf.Max(1f, phaseTwoSpeedMultiplier) : 1f); // 예고한 최대 돌진 거리
            warning.SetPosition(0, transform.position); // 실제 예고 시작점
            warning.SetPosition(1, transform.position + (Vector3)(direction * distance)); // 실제 예고 종료점
            warning.enabled = true; // 실제 예고선 표시
            warningActive = true; // 실제 예고 단계 시작
            phaseEndsAt = Time.time + Mathf.Max(0.1f, warningDuration); // 실제 예고 종료 시각
        } // 메서드 끝

        private void FinishCharge() // 실제 돌진 종료
        { // 메서드 시작
            charging = false; // 실제 돌진 상태 종료
            nextChargeAt = Time.time + Mathf.Max(0.1f, chargeInterval); // 다음 돌진 시각
            if (ordinaryMovement != null) // 기존 사수 행동 확인
            { // 조건 시작
                ordinaryMovement.enabled = previousMovementEnabled; // 이전 사수 활성 상태 복구
            } // 조건 끝
        } // 메서드 끝

        private void OnTriggerEnter2D(Collider2D other) // 실제 돌진 진입 접촉
        { // 메서드 시작
            HandleContact(other); // 실제 공통 접촉 판정
        } // 메서드 끝
        private void OnTriggerStay2D(Collider2D other) // 실제 돌진 지속 접촉
        { // 메서드 시작
            HandleContact(other); // 실제 공통 접촉 판정
        } // 메서드 끝
        private void HandleContact(Collider2D other) // 실제 돌진 피해와 절단 예약
        { // 메서드 시작
            if (!isActiveAndEnabled || !charging || hitHandled || cutPending || other == null || player == null || health == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 실제 접촉 가능 상태 확인
            { // 조건 시작
                return; // 중복 또는 비활성 접촉 무시
            } // 조건 끝
            SnakeSegment segment = other.GetComponent<SnakeSegment>(); // 실제 접촉 몸통 조회
            bool isHead = other.transform == player.transform; // 실제 머리 접촉 여부
            bool isBody = segment != null && body != null && segment.transform.IsChildOf(body.transform); // 실제 플레이어 몸통 여부
            if (!isHead && !isBody) // 실제 플레이어 접촉 확인
            { // 조건 시작
                return; // 다른 충돌 무시
            } // 조건 끝
            SnakeExternalCollision resolver = player.GetComponent<SnakeExternalCollision>(); // 실제 물리 충돌 처리기
            if (resolver == null || !resolver.isActiveAndEnabled) // 실제 처리기 누락 확인
            { // 조건 시작
                Debug.LogError("[Project Epsilon] 보스 돌진 충돌 처리기가 없습니다."); // 충돌 처리 누락 기록
                return; // 처리기 없는 피해 방지
            } // 조건 끝
            if (cutAttack) // 예고된 절단 돌진 확인
            { // 조건 시작
                cutPending = true; // 같은 스텝 중복 예약 방지
                resolver.QueueEliteCut(HandleCutResolved); // 기존 물리 우선순위에 절단 예약
                return; // HP 피해 생략
            } // 조건 끝
            float multiplier = target == null ? 1f : target.OutgoingDamageMultiplier; // 실제 적 공격 약화 배율
            hitHandled = true; // 같은 돌진 중복 피해 방지
            resolver.QueueHealthDamage(health, Mathf.CeilToInt(damage * multiplier)); // 기존 물리 우선순위에 HP 피해 예약
        } // 메서드 끝
        private void HandleCutResolved(bool applied) // 실제 절단 결과 처리
        { // 메서드 시작
            cutPending = false; // 실제 절단 대기 해제
            hitHandled |= applied; // 성공한 절단만 기록
        } // 메서드 끝
        private void OnDisable() // 비활성화 중 돌진 정리
        { // 메서드 시작
            if ((warningActive || charging) && ordinaryMovement != null) // 중단된 돌진과 기존 행동 확인
            { // 조건 시작
                ordinaryMovement.enabled = previousMovementEnabled; // 이전 사수 활성 상태 복구
            } // 조건 끝
            warningActive = false; // 예고 상태 해제
            charging = false; // 돌진 상태 해제
            cutPending = false; // 절단 대기 해제
            if (warning != null) // 실제 예고선 확인
            { // 조건 시작
                warning.enabled = false; // 중단된 예고선 숨김
            } // 조건 끝
        } // 메서드 끝
        private void OnDestroy() // 실제 예고 재질 정리
        { // 메서드 시작
            if (warningMaterial != null) // 실제 재질 존재 확인
            { // 조건 시작
                Destroy(warningMaterial); // 생성한 재질 제거
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
