using ProjectEpsilon.Core; // 대상 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{
    [RequireComponent(typeof(PlayerInputReader))] // 구성 요소·플레이어 설정
    public sealed class SnakeMovement : MonoBehaviour // 이동·행동 자료 구조
    {
        [SerializeField] private float moveSpeed = 3f; // 필드·속도 설정
        [SerializeField] private float turnSpeed = 145f; // 필드·속도 설정
        [SerializeField] private SnakeStamina stamina; // 필드·스태미나 설정

        [Header("Merge Movement")] // 합성·이동 설정
        [SerializeField] private bool mergeMovementMode; // 필드·합성 설정
        [Range(0.1f, 1f)] // 범위 설정
        [SerializeField] private float mergeSpeedMultiplier = 0.7f; // 필드·합성 설정

        private PlayerInputReader inputReader; // 플레이어·입력 구성 선언
        private SnakeBodyManager bodyManager; // 마지막 몸통의 실제 상태
        private float windUntil; // 바람길 효과 종료 시각
        private float windMultiplier = 1f; // 바람길 이동 배율
        private float pollenUntil; // 꽃가루 효과 종료 시각
        private float pollenMultiplier = 1f; // 꽃가루 획득 범위 배율
        private float terrainImmunityUntil; // 지형 감속 면역 종료 시각
        private bool cleanseUntilTerrainExit; // 현재 지형 이탈까지 정화 유지
        private Vector3 lastMoveStart; // 현재 프레임 자동 전진 시작점
        private Vector3 lastMoveEnd; // 현재 프레임 자동 전진 종료점
        private int lastMoveFrame = -1; // 마지막 자동 전진 프레임

        public float BaseMoveSpeed => moveSpeed; // 캐릭터 보정 전 기본 이동 속도
        public float MoveSpeed => moveSpeed * CharacterRunInitializer.MoveSpeedMultiplier; // 캐릭터 이동 배율 적용
        public float TurnSpeed => turnSpeed * CharacterRunInitializer.TurnSpeedMultiplier * ProjectEpsilon.Progression.MetaModifiers.TurnMultiplier; // 캐릭터와 영구 회전 배율
        public bool IsMergeMovementMode => mergeMovementMode; // 합성·이동 값 반환

        public float CurrentMoveSpeed => // 현재·속도 값 반환
            MoveSpeed * // 캐릭터 이동 속도 적용
            ProjectEpsilon.Progression.MetaModifiers.LastBodySpeedMultiplier(bodyManager) * // 마지막 몸통 영구 효과
            ResolveTerrainMultiplier() * // 지형 전용 감속과 면역 적용
            ProjectEpsilon.Enemies.EnemySlowZone.SpeedMultiplierAt(transform.position) * // 적 감속 지대의 별도 이동 배율
            (Time.time < windUntil ? windMultiplier : 1f) * // 일시적 바람길 이동 배율
            (stamina == null // 스태미나 기능 실행
                ? 1f // 대상 처리
                : stamina.CurrentSpeedMultiplier); // 합성 감속은 전체 시간에만 적용

        public float PickupRangeMultiplier => Time.time < pollenUntil ? pollenMultiplier : 1f; // 꽃가루의 임시 획득 범위 배율

        public bool CrossedPositionThisFrame(Vector3 position, float radius) // 실제 자동 전진 경로의 위치 확인
        { // 함수 시작
            if (!isActiveAndEnabled || lastMoveFrame != Time.frameCount) // 실제 현재 프레임 이동 여부 확인
            { // 조건 시작
                return false; // 강제 위치 변경과 이전 이동 제외
            } // 조건 종료
            Vector3 path = lastMoveEnd - lastMoveStart; // 실제 자동 전진 구간
            float lengthSquared = path.sqrMagnitude; // 실제 이동 거리 제곱
            float fraction = lengthSquared > 0f ? Mathf.Clamp01(Vector3.Dot(position - lastMoveStart, path) / lengthSquared) : 0f; // 구간 안의 가장 가까운 위치 비율
            float safeRadius = Mathf.Max(0f, radius); // 안전한 획득 반경
            return (position - (lastMoveStart + path * fraction)).sqrMagnitude <= safeRadius * safeRadius; // 실제 이동 구간의 보석 접촉 여부
        } // 함수 종료

        public void ApplyWind(float duration, float multiplier) // 바람길 이동 효과 갱신
        {
            windUntil = Time.time + Mathf.Max(0f, duration); // 안전한 바람 효과 종료 시각
            windMultiplier = Mathf.Max(1f, multiplier); // 안전한 바람 이동 배율
        }

        public void ApplyPollen(float duration, float multiplier) // 꽃가루 획득 범위 효과 갱신
        {
            pollenUntil = Time.time + Mathf.Max(0f, duration); // 안전한 꽃가루 종료 시각
            pollenMultiplier = Mathf.Max(1f, multiplier); // 안전한 획득 범위 배율
        }

        public void IgnoreTerrainSlow(float duration) // 지형 감속 면역 적용
        {
            terrainImmunityUntil = Mathf.Max(terrainImmunityUntil, Time.time + Mathf.Max(0f, duration)); // 기존 면역을 보존한 종료 시각
        }

        public void CleanseTerrainSlow() // 현재 지형 감속만 정화
        {
            cleanseUntilTerrainExit = true; // 현재 감속 지형 이탈까지 정화 유지
        }

        public void RefreshTerrainCleanse() // 실제 감속 지형 이탈 확인
        {
            if (cleanseUntilTerrainExit && ProjectEpsilon.World.PrototypeSpecialZone.SpeedMultiplierAt(transform.position) >= 1f) // 모든 감속 지형 이탈 확인
            {
                cleanseUntilTerrainExit = false; // 이전 지형의 정화 상태 해제
            }
        }

        private float ResolveTerrainMultiplier() // 현재 지형 감속과 면역 계산
        {
            float terrain = ProjectEpsilon.World.PrototypeSpecialZone.SpeedMultiplierAt(transform.position); // 실제 지형 감속 배율
            if (terrain >= 1f) // 모든 감속 지형에서 이탈 확인
            {
                cleanseUntilTerrainExit = false; // 현재 지형에 대한 정화 종료
            }
            return Time.time < terrainImmunityUntil || cleanseUntilTerrainExit ? 1f : terrain; // 지형에만 면역 적용
        }

        private void Awake() // 초기화 기능 선언
        {
            bodyManager = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 상태 연결
            inputReader = // 입력 값 설정
                GetComponent<PlayerInputReader>(); // 조회·구성 요소 기능 실행

            if (stamina == null) // 스태미나 조건 확인
            {
                stamina = // 스태미나 값 설정
                    GetComponent<SnakeStamina>(); // 조회·구성 요소 기능 실행
            }
        }

        private void Update() // 갱신 기능 선언
        {
            if (GameManager.Instance != null && // 게임·관리자 조건 확인
                !GameManager.Instance.IsPlaying) // 게임·관리자 처리
            {
                return; // 현재 기능 종료
            }

            if (!mergeMovementMode) // 합성·이동 조건 확인
            {
                Rotate(); // 대상 기능 실행
            }

            MoveForward(); // 대상 기능 실행
        }

        public void BindStamina( // 연결·스태미나 호출 시작
            SnakeStamina staminaController // 스태미나·제어기 처리
        ) // 앞선 항목 종료
        {
            stamina = // 스태미나 값 설정
                staminaController; // 스태미나·제어기 처리
        }

        public void SetMergeMovementMode( // 설정·합성 호출 시작
            bool active, // 활성 항목 전달
            float speedMultiplier = 0.7f // 속도·배율 값 설정
        ) // 앞선 항목 종료
        {
            mergeMovementMode = active; // 합성·이동 값 설정
            mergeSpeedMultiplier = // 합성·속도 값 설정
                Mathf.Clamp( // 대상 호출 시작
                    speedMultiplier, // 속도·배율 항목 전달
                    0.1f, // 대상 항목 전달
                    1f // 대상 처리
                ); // 앞선 항목 종료
        }

        private void Rotate() // 대상 기능 선언
        {
            float turnInput = // 입력 값 설정
                inputReader.TurnInput; // 입력 처리

            float rotationAmount = // 회전·수량 값 설정
                -turnInput * // 입력 처리
                TurnSpeed * // 캐릭터 회전 속도 적용
                Time.deltaTime; // 시간·변화량 처리

            transform.Rotate( // 위치 호출 시작
                0f, // 대상 항목 전달
                0f, // 대상 항목 전달
                rotationAmount // 회전·수량 처리
            ); // 앞선 항목 종료
        }

        private void MoveForward() // 대상 기능 선언
        {
            lastMoveStart = transform.position; // 실제 자동 전진 시작점 기록
            transform.position += // 위치 값 설정
                transform.up * // 위치 처리
                (CurrentMoveSpeed * // 현재·속도 기능 실행
                Time.deltaTime); // 시간·변화량 처리
            lastMoveEnd = transform.position; // 실제 자동 전진 종료점 기록
            lastMoveFrame = Time.frameCount; // 실제 자동 전진 프레임 기록
        }
    }
}
