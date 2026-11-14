using ProjectEpsilon.Core; // 대상 기능 연결
using ProjectEpsilon.Player; // 플레이어 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{
    public enum RecoveryPickupType // 회복·형식 자료 구조
    {
        Heal, // 회복 항목 전달
        BodyRepair // 몸통 처리
    }

    [RequireComponent(typeof(SpriteRenderer))] // 구성 요소·이미지 설정
    [DefaultExecutionOrder(10)] // 실제 자동 전진 이후 회복 아이템 처리
    public sealed class RecoveryPickup : MonoBehaviour // 회복·행동 자료 구조
    {
        [SerializeField] private Transform receiver; // 필드·위치 설정
        [SerializeField] private SnakeHealth health; // 필드·체력 설정
        [SerializeField] private SnakeBodyManager bodyManager; // 필드·몸통 설정
        [SerializeField] private RecoveryPickupType pickupType = // 필드·회복 설정
            RecoveryPickupType.Heal; // 회복·형식 처리

        [Min(1)] // 최소 설정
        [SerializeField] private int value = 15; // 필드·값 설정

        [Min(0.1f)] // 최소 설정
        [SerializeField] private float attractionRange = 2.6f; // 필드·범위 설정

        [Min(0.1f)] // 최소 설정
        [SerializeField] private float attractionSpeed = 6f; // 필드·속도 설정

        [Min(0.01f)] // 최소 설정
        [SerializeField] private float collectDistance = 0.22f; // 필드·거리 설정

        private static Sprite healArtwork; // 체력 회복 전용 이미지
        private static Sprite bodyArtwork; // 몸통 복구 전용 이미지
        private SpriteRenderer spriteRenderer; // 이미지·렌더러 구성 선언
        private SnakeMovement receiverMovement; // 실제 수신자의 자동 전진 경로
        private bool collected; // 대상 구성 선언
        public bool IsBodyRepair => pickupType == RecoveryPickupType.BodyRepair; // 몸통 복구 아이템 여부

        private void Awake() // 초기화 기능 선언
        {
            spriteRenderer = // 이미지·렌더러 값 설정
                GetComponent<SpriteRenderer>(); // 조회·구성 요소 기능 실행
        }

        public void Configure( // 구성 호출 시작
            Transform target, // 위치·대상 항목 전달
            SnakeHealth snakeHealth, // 체력 항목 전달
            SnakeBodyManager manager, // 몸통·관리자 항목 전달
            RecoveryPickupType type, // 회복·형식 항목 전달
            int amount, // 수량 항목 전달
            Sprite visual // 이미지·시각 효과 처리
        ) // 앞선 항목 종료
        {
            receiver = target; // 대상 값 설정
            receiverMovement = receiver == null ? null : receiver.GetComponent<SnakeMovement>(); // 실제 수신자의 이동 경로 연결
            health = snakeHealth; // 체력 값 설정
            bodyManager = manager; // 몸통·관리자 값 설정
            pickupType = type; // 형식 값 설정
            value = Mathf.Max(1, amount); // 값·최대 값 설정
            collected = false; // 대상 값 설정

            if (spriteRenderer == null) // 이미지·렌더러 조건 확인
            {
                spriteRenderer = // 이미지·렌더러 값 설정
                    GetComponent<SpriteRenderer>(); // 조회·구성 요소 기능 실행
            }

            if (pickupType == RecoveryPickupType.Heal && healArtwork == null) // 체력 이미지 로드 여부 확인
            { // 조건 시작
                healArtwork = Resources.Load<Sprite>("Pickups/HealthPickup"); // 체력 회복 이미지 로드
            } // 조건 종료
            if (pickupType == RecoveryPickupType.BodyRepair && bodyArtwork == null) // 몸통 이미지 로드 여부 확인
            { // 조건 시작
                bodyArtwork = Resources.Load<Sprite>("Pickups/BodyRepairPickup"); // 몸통 복구 이미지 로드
            } // 조건 종료
            Sprite artwork = pickupType == RecoveryPickupType.Heal ? healArtwork : bodyArtwork; // 종류에 맞는 이미지 선택
            spriteRenderer.sprite = artwork != null ? artwork : visual; // 이미지 누락 시 기존 스프라이트 유지
            spriteRenderer.sortingOrder = 17; // 기존 아이템 표시 순서 유지
            spriteRenderer.color = artwork != null ? Color.white : pickupType == RecoveryPickupType.Heal ? new Color(0.35f, 1f, 0.45f, 1f) : new Color(0.35f, 0.85f, 1f, 1f); // 원본 색상과 대체 색상
            float originalScale = pickupType == RecoveryPickupType.Heal ? 0.27f : 0.31f; // 기존 종류별 표시 크기
            float referenceWidth = visual == null ? 26f / 32f : visual.bounds.size.x; // 기존 아이템 이미지 너비
            float artworkWidth = spriteRenderer.sprite == null ? referenceWidth : spriteRenderer.sprite.bounds.size.x; // 실제 표시 이미지 너비
            float scale = artworkWidth > 0f ? referenceWidth * originalScale * 4f / artworkWidth : originalScale * 4f; // 기존 표시 너비의 네 배
            transform.localScale = new Vector3(scale, scale, 1f); // 비율을 유지한 아이템 표시 크기
        }

        private void Update() // 갱신 기능 선언
        {
            if (collected || // 대상 조건 확인
                !CanApply()) // 가능 여부·적용 기능 실행
            {
                return; // 현재 기능 종료
            }

            ResolveReceiver(); // 대상 기능 실행

            if (receiver == null) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            if (receiverMovement == null || receiverMovement.transform != receiver) // 실제 수신자 변경 여부 확인
            { // 조건 시작
                receiverMovement = receiver.GetComponent<SnakeMovement>(); // 현재 수신자의 이동 경로 조회
            } // 조건 종료

            GameManager game = GameManager.Instance; // 실제 게임 상태 조회
            if (game != null && !game.IsPlaying) // 실제 정지 또는 종료 확인
            { // 조건 시작
                SnakeExperience experience = receiver.GetComponent<SnakeExperience>(); // 실제 수신자의 레벨업 상태
                if (game.IsPaused && experience != null && experience.IsLevelUpPending && receiverMovement != null && receiverMovement.CrossedPositionThisFrame(transform.position, collectDistance)) // 레벨업을 연 같은 자동 전진의 아이템 확인
                { // 조건 시작
                    Collect(); // 이미 지나간 아이템의 기존 회복 처리
                } // 조건 종료
                return; // 다른 정지 프레임과 종료 중 획득 차단
            } // 조건 종료

            Vector3 targetPosition = // 벡터·대상 값 설정
                receiver.position; // 위치 처리

            Vector3 delta = // 벡터·변화량 값 설정
                targetPosition - // 대상·위치 처리
                transform.position; // 위치 처리

            float collectDistanceSquared = // 거리 값 설정
                collectDistance * // 거리 처리
                collectDistance; // 거리 처리

            if (delta.sqrMagnitude <= // 변화량 조건 확인
                collectDistanceSquared) // 거리 처리
            {
                Collect(); // 대상 기능 실행
                return; // 현재 기능 종료
            }

            if (receiverMovement != null && receiverMovement.CrossedPositionThisFrame(transform.position, collectDistance)) // 현재 프레임 실제 자동 전진 경로의 아이템 확인
            { // 조건 시작
                Collect(); // 통과한 아이템의 기존 회복 처리
                return; // 흡수와 중복 획득 방지
            } // 조건 종료

            float attractionRangeSquared = // 범위 값 설정
                attractionRange * // 범위 처리
                attractionRange; // 범위 처리

            if (delta.sqrMagnitude > // 변화량 조건 확인
                attractionRangeSquared) // 범위 처리
            {
                return; // 현재 기능 종료
            }

            transform.position = // 위치 값 설정
                Vector3.MoveTowards( // 벡터 호출 시작
                    transform.position, // 위치 항목 전달
                    targetPosition, // 대상·위치 항목 전달
                    attractionSpeed * // 속도 처리
                    Time.deltaTime // 시간·변화량 처리
                ); // 앞선 항목 종료

            if ((targetPosition - // 대상·위치 조건 확인
                transform.position) // 위치 처리
                .sqrMagnitude <= // 대상 처리
                collectDistanceSquared) // 거리 처리
            {
                Collect(); // 대상 기능 실행
            }
        }

        private bool CanApply() // 가능 여부·적용 기능 선언
        {
            ResolveSystems(); // 대상 기능 실행

            if (pickupType == // 형식 조건 확인
                RecoveryPickupType.Heal) // 회복·형식 처리
            {
                return // 대상 결과 반환
                    health != null && // 체력 처리
                    health.CurrentHealth < // 체력·현재 처리
                        health.MaximumHealth && // 체력·최대 처리
                    bodyManager != null && // 몸통·관리자 처리
                    bodyManager.CurrentBodyCount > 0; // 몸통·관리자 처리
            }

            return // 대상 결과 반환
                (GameManager.Instance == null || !GameManager.Instance.IsFinalBossPhase) && // 보스전 별도 복구 제한
                bodyManager != null && // 몸통·관리자 처리
                bodyManager.MissingRepairableBodyCount > 0; // 몸통·관리자 처리
        }

        private void Collect() // 대상 기능 선언
        {
            if (collected) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            bool applied; // 대상 처리

            if (pickupType == // 형식 조건 확인
                RecoveryPickupType.Heal) // 회복·형식 처리
            {
                applied = // 대상 값 설정
                    health != null && // 체력 처리
                    health.Heal(value); // 체력·회복 기능 실행
            }
            else // 다른 조건 처리
            {
                applied = // 대상 값 설정
                    bodyManager != null && // 몸통·관리자 처리
                    bodyManager.TryRepairBody(); // 몸통·관리자 기능 실행
            }

            if (!applied) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            collected = true; // 대상 값 설정
            Destroy(gameObject); // 제거·게임 기능 실행
        }

        private void ResolveReceiver() // 대상 기능 선언
        {
            if (receiver != null) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            ResolveSystems(); // 대상 기능 실행

            if (health != null) // 체력 조건 확인
            {
                receiver = // 대상 값 설정
                    health.transform; // 체력·위치 처리
            }
        }

        private void ResolveSystems() // 대상 기능 선언
        {
            if (health == null) // 체력 조건 확인
            {
                health = // 체력 값 설정
                    Object.FindFirstObjectByType<SnakeHealth>(); // 객체·검색 기능 실행
            }

            if (bodyManager == null) // 몸통·관리자 조건 확인
            {
                bodyManager = // 몸통·관리자 값 설정
                    Object.FindFirstObjectByType<SnakeBodyManager>(); // 객체·검색 기능 실행
            }
        }

        public static RecoveryPickup Spawn( // 회복·생성 호출 시작
            Vector3 position, // 벡터·위치 항목 전달
            Transform receiver, // 위치 항목 전달
            SnakeHealth health, // 체력 항목 전달
            SnakeBodyManager bodyManager, // 몸통·관리자 항목 전달
            RecoveryPickupType type, // 회복·형식 항목 전달
            int value, // 값 항목 전달
            Sprite visual // 이미지·시각 효과 처리
        ) // 앞선 항목 종료
        {
            GameObject pickupObject = // 게임·객체 값 설정
                new GameObject( // 게임·객체 객체 생성
                    type == // 형식 처리
                    RecoveryPickupType.Heal // 회복·형식 처리
                        ? "Pickup_Heal" // 회복 처리
                        : "Pickup_BodyRepair" // 몸통 처리
                ); // 앞선 항목 종료

            pickupObject.transform.position = // 객체·위치 값 설정
                position; // 위치 처리

            RecoveryPickup pickup = // 회복 값 설정
                pickupObject.AddComponent<RecoveryPickup>(); // 객체·추가 기능 실행

            pickup.Configure( // 구성 호출 시작
                receiver, // 대상 항목 전달
                health, // 체력 항목 전달
                bodyManager, // 몸통·관리자 항목 전달
                type, // 형식 항목 전달
                value, // 값 항목 전달
                visual // 시각 효과 처리
            ); // 앞선 항목 종료

            return pickup; // 대상 결과 반환
        }
    }
}
