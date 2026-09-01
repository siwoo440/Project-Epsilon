using ProjectEpsilon.Combat; // 대상 전투 정보 사용
using ProjectEpsilon.Core; // 게임 상태 사용
using ProjectEpsilon.Data; // Enemy 데이터 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Enemies // Enemy 영역
{ // 네임스페이스 시작
    [RequireComponent(typeof(WeaponTarget))] // 공격 대상 필수
    [RequireComponent(typeof(Rigidbody2D))] // 물리 이동 필수
    public sealed class EnemyMovementController : MonoBehaviour // Enemy 추적 이동
    { // 클래스 시작
        [SerializeField] private EnemyData enemyData; // Enemy 데이터 참조
        [SerializeField] private Transform chaseTarget; // 추적 대상 참조
        [SerializeField] private WeaponTargetStatusController statusController; // 상태 관리자 참조
        [SerializeField] private Rigidbody2D movementBody; // 이동 물리 참조
        [Min(0f)] [SerializeField] private float fallbackMoveSpeed = 1f; // 대체 이동속도
        [Min(0f)] [SerializeField] private float stopDistance = 0.05f; // 최소 접근 거리

        private WeaponTarget weaponTarget; // 생존 대상 참조

        public EnemyData EnemyData => enemyData; // Enemy 데이터 반환
        public Transform ChaseTarget => chaseTarget; // 추적 대상 반환
        public WeaponTargetStatusController StatusController => statusController; // 상태 관리자 반환
        public Rigidbody2D MovementBody => movementBody; // 이동 물리 반환
        public float BaseMoveSpeed => enemyData == null ? Mathf.Max(0f, fallbackMoveSpeed) : Mathf.Max(0f, enemyData.MoveSpeed); // 기본 이동속도 반환
        public float CurrentMoveSpeed => BaseMoveSpeed * (statusController == null ? 1f : statusController.CurrentMovementMultiplier); // 최종 이동속도 반환

        private void Awake() // 참조 초기화
        { // 메서드 시작
            EnsureReferences(); // 필수 참조 확보
        } // 메서드 끝

        private void FixedUpdate() // 물리 이동 갱신
        { // 메서드 시작
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 전투 진행 여부 확인
            { // 조건 시작
                return; // 이동 중단
            } // 조건 끝

            EnsureReferences(); // 참조 상태 확인

            if (movementBody == null || chaseTarget == null || weaponTarget == null || !weaponTarget.IsAlive) // 이동 가능 여부 확인
            { // 조건 시작
                return; // 이동 생략
            } // 조건 끝

            float speed = CurrentMoveSpeed; // 현재 이동속도 조회

            if (speed <= 0f) // 정지 상태 확인
            { // 조건 시작
                return; // 이동 생략
            } // 조건 끝

            Vector2 offset = (Vector2)chaseTarget.position - movementBody.position; // 대상 방향 계산
            float distance = offset.magnitude; // 대상 거리 계산

            if (distance <= stopDistance || distance <= 0.0001f) // 최소 거리 확인
            { // 조건 시작
                return; // 접근 이동 생략
            } // 조건 끝

            float travel = Mathf.Min(speed * Time.fixedDeltaTime, distance - stopDistance); // 이동 거리 제한
            Vector2 nextPosition = movementBody.position + offset.normalized * travel; // 다음 위치 계산
            movementBody.MovePosition(nextPosition); // 물리 위치 이동
        } // 메서드 끝

        public void Configure(EnemyData data, Transform target, WeaponTargetStatusController statuses, Rigidbody2D body) // 이동 참조 구성
        { // 메서드 시작
            enemyData = data; // Enemy 데이터 저장
            chaseTarget = target; // 추적 대상 저장
            statusController = statuses; // 상태 관리자 저장
            movementBody = body; // 이동 물리 저장
            EnsureReferences(); // 누락 참조 보완
        } // 메서드 끝

        private void EnsureReferences() // 필수 참조 확보
        { // 메서드 시작
            if (weaponTarget == null) // 공격 대상 없음 확인
            { // 조건 시작
                weaponTarget = GetComponent<WeaponTarget>(); // 공격 대상 조회
            } // 조건 끝

            if (statusController == null) // 상태 관리자 없음 확인
            { // 조건 시작
                statusController = GetComponent<WeaponTargetStatusController>(); // 상태 관리자 조회
            } // 조건 끝

            if (movementBody == null) // 이동 물리 없음 확인
            { // 조건 시작
                movementBody = GetComponent<Rigidbody2D>(); // 이동 물리 조회
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
