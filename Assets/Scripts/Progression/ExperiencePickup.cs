using ProjectEpsilon.Core; // 대상 기능 연결
using ProjectEpsilon.Player; // 플레이어 획득 범위 효과
using ProjectEpsilon.Recording; // 선택적 보석 경험치 기록
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{
    [RequireComponent(typeof(SpriteRenderer))] // 구성 요소·이미지 설정
    [DefaultExecutionOrder(10)] // 실제 자동 전진 이후 보석 획득 처리
    public sealed class ExperiencePickup : MonoBehaviour // 경험치·행동 자료 구조
    {
        [SerializeField] private SnakeExperience receiver; // 필드·경험치 설정
        [Min(1)] [SerializeField] private int experienceValue = 1; // 최소·필드 설정
        [Min(0.1f)] [SerializeField] private float attractionRange = 2.6f; // 최소·필드 설정
        [Min(0.1f)] [SerializeField] private float attractionSpeed = 6f; // 최소·필드 설정
        [Min(0.01f)] [SerializeField] private float collectDistance = 0.22f; // 최소·필드 설정

        private SpriteRenderer spriteRenderer; // 이미지·렌더러 구성 선언
        private bool collected; // 대상 구성 선언
        private bool destructionScheduled; // 파괴 예약 여부
        private Sprite fallbackSprite; // 이미지 설정이 없는 씬의 기존 표시
        private SnakeMovement receiverMovement; // 수신자의 임시 범위 효과

        private void Awake() // 초기화 기능 선언
        {
            spriteRenderer = GetComponent<SpriteRenderer>(); // 이미지·렌더러 값 설정
        }

        public void Configure( // 구성 호출 시작
            SnakeExperience experienceReceiver, // 경험치 항목 전달
            int value, // 값 항목 전달
            Sprite visual // 이미지·시각 효과 처리
        ) // 앞선 항목 종료
        {
            if (destructionScheduled) // 파괴 예약 상태 확인
            {
                return; // 제거 예정 보석 재설정 방지
            }
            receiver = experienceReceiver; // 경험치 값 설정
            receiverMovement = receiver == null ? null : receiver.GetComponent<SnakeMovement>(); // 실제 수신자의 이동 효과 조회
            ExperiencePickupSettings settings = receiver != null ? receiver.PickupSettings : SnakeExperience.Current != null ? SnakeExperience.Current.PickupSettings : null; // 실제 수신자의 흡수 설정
            if (settings != null) // 공통 설정 존재 확인
            {
                attractionRange = settings.AttractionRange; // 흡수 범위 적용
                attractionSpeed = settings.AttractionSpeed; // 흡수 속도 적용
                collectDistance = settings.CollectDistance; // 획득 거리 적용
            }
            fallbackSprite = visual; // 기존 대체 이미지 저장
            experienceValue = Mathf.Max(1, value); // 경험치·값 값 설정
            collected = false; // 대상 값 설정

            if (spriteRenderer == null) // 이미지·렌더러 조건 확인
            {
                spriteRenderer = GetComponent<SpriteRenderer>(); // 이미지·렌더러 값 설정
            }

            spriteRenderer.sprite = visual; // 이미지·렌더러 값 설정
            spriteRenderer.color = new Color(1f, 0.9f, 0.25f, 1f); // 이미지·렌더러 값 설정
            RefreshArtwork(); // 실제 보석 등급 이미지 반영
            spriteRenderer.sortingOrder = 16; // 이미지·렌더러 값 설정

            float scale = ResolveVisualScale(experienceValue); // 크기·시각 효과 값 설정
            transform.localScale = new Vector3(scale, scale, 1f); // 위치·크기 값 설정
        }

        private void Update() // 갱신 기능 선언
        {
            if (collected) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            if (GameManager.Instance != null && // 게임·관리자 조건 확인
                !GameManager.Instance.IsPlaying) // 게임·관리자 처리
            {
                return; // 현재 기능 종료
            }

            if (receiver == null) // 대상 조건 확인
            {
                receiver = SnakeExperience.Current; // 경험치·현재 값 설정

                if (receiver == null) // 대상 조건 확인
                {
                    return; // 현재 기능 종료
                }
                receiverMovement = receiver.GetComponent<SnakeMovement>(); // 새 수신자의 이동 효과 조회
            }

            Vector3 targetPosition = receiver.transform.position; // 벡터·대상 값 설정
            Vector3 delta = targetPosition - transform.position; // 벡터·변화량 값 설정
            float collectDistanceSquared = // 거리 값 설정
                collectDistance * collectDistance; // 거리 처리

            if (delta.sqrMagnitude <= collectDistanceSquared) // 변화량·거리 조건 확인
            {
                Collect(); // 대상 기능 실행
                return; // 현재 기능 종료
            }

            if (receiverMovement != null && receiverMovement.CrossedPositionThisFrame(transform.position, collectDistance)) // 현재 프레임 실제 자동 전진 경로의 보석 확인
            { // 조건 시작
                Collect(); // 통과한 보석의 기존 경험치 지급
                return; // 흡수와 중복 획득 방지
            } // 조건 종료

            float attractionRangeSquared = // 범위 값 설정
                MergeAttractionRange * MergeAttractionRange; // 영구 성장 흡수 범위 적용

            if (delta.sqrMagnitude > attractionRangeSquared) // 변화량·범위 조건 확인
            {
                return; // 현재 기능 종료
            }

            transform.position = Vector3.MoveTowards( // 위치·벡터 호출 시작
                transform.position, // 위치 항목 전달
                targetPosition, // 대상·위치 항목 전달
                attractionSpeed * Time.deltaTime // 속도·시간 처리
            ); // 앞선 항목 종료

            if ((targetPosition - transform.position).sqrMagnitude <= // 대상·위치 조건 확인
                collectDistanceSquared) // 거리 처리
            {
                Collect(); // 대상 기능 실행
            }
        }

        private void Collect() // 대상 기능 선언
        {
            if (collected || receiver == null) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            collected = true; // 대상 값 설정
            destructionScheduled = true; // 획득 중 재설정 및 중복 사용 방지
            RunTelemetry.RecordGemCollection(experienceValue); // 보너스 적용 전 실제 수집량 기록
            receiver.AddExperience(experienceValue); // 추가·경험치 기능 실행
            Destroy(gameObject); // 제거·게임 기능 실행
        }

        public int ExperienceValue => experienceValue; // 보석 경험치 조회
        internal bool IsMergeAvailable => !collected && isActiveAndEnabled && gameObject.activeInHierarchy; // 병합 가능 상태
        internal SnakeExperience MergeReceiver => receiver != null ? receiver : SnakeExperience.Current; // 실제 경험치 수신자
        internal float MergeAttractionRange => Mathf.Max(0f, attractionRange * MetaModifiers.PickupRangeMultiplier * (receiverMovement == null ? 1f : receiverMovement.PickupRangeMultiplier)); // 영구 성장과 꽃가루 획득 범위

        internal bool IsOutsideAttraction() // 자동 흡수 범위 밖 확인
        {
            SnakeExperience target = MergeReceiver; // 실제 수신자 조회
            if (target == null) // 수신자 부재 확인
            {
                return false; // 보석 유지
            }
            float safeRange = MergeAttractionRange; // 안전한 흡수 범위
            return (target.transform.position - transform.position).sqrMagnitude > safeRange * safeRange; // 흡수 범위 밖 여부
        }

        internal void PromoteForMerge(int value) // 병합 결과 값 반영
        {
            experienceValue = value; // 보존한 경험치 저장
            RefreshArtwork(); // 병합된 보석 등급 이미지 갱신
            gameObject.name = $"XP_Gem_{value}"; // 결과 등급 이름 갱신
            float scale = ResolveVisualScale(value); // 결과 등급 크기
            transform.localScale = new Vector3(scale, scale, 1f); // 보석 크기 갱신
        }

        internal void ConsumeForMerge() // 병합 재료 보석 제거
        {
            collected = true; // 같은 프레임 중복 수집 방지
            destructionScheduled = true; // 파괴 예약 중 재설정 방지
            Destroy(gameObject); // 병합 재료 제거 예약
        }

        private void RefreshArtwork() // 실제 경험치별 이미지 설정
        {
            if (spriteRenderer == null) // 실제 표시기 확인
            {
                return; // 표시기 없는 갱신 방지
            }
            ExperienceGemVisualPalette palette = FindFirstObjectByType<ExperienceGemVisualPalette>(); // 실제 씬 보석 이미지 설정
            Sprite selected = palette != null && palette.isActiveAndEnabled ? palette.Resolve(experienceValue) : null; // 실제 등급별 이미지
            spriteRenderer.sprite = selected != null ? selected : fallbackSprite; // 전용 이미지 또는 기존 대체 이미지
            spriteRenderer.color = selected != null ? Color.white : new Color(1f, 0.9f, 0.25f, 1f); // 원본 색상 또는 기존 임시 색상
        }

        private static float ResolveVisualScale(int value) // 시각 효과·크기 기능 선언
        {
            if (value >= 20) // 값 조건 확인
            {
                return 1.02f; // 대형 보석 세 배 크기
            }

            if (value >= 5) // 값 조건 확인
            {
                return 0.78f; // 중형 보석 세 배 크기
            }

            return 0.54f; // 소형 보석 세 배 크기
        }

        public static ExperiencePickup Spawn( // 경험치·생성 호출 시작
            Vector3 position, // 벡터·위치 항목 전달
            int value, // 값 항목 전달
            Sprite visual, // 이미지·시각 효과 항목 전달
            SnakeExperience receiver // 경험치 처리
        ) // 앞선 항목 종료
        {
            GameObject pickupObject = // 게임·객체 값 설정
                new GameObject($"XP_Gem_{Mathf.Max(1, value)}"); // 게임·객체 객체 생성

            pickupObject.transform.position = position; // 객체·위치 값 설정

            ExperiencePickup pickup = // 경험치 값 설정
                pickupObject.AddComponent<ExperiencePickup>(); // 객체·추가 기능 실행

            pickup.Configure( // 구성 호출 시작
                receiver, // 대상 항목 전달
                value, // 값 항목 전달
                visual // 시각 효과 처리
            ); // 앞선 항목 종료

            RunTelemetry.RecordGemSpawn(pickup.ExperienceValue); // 실제 생성 보석 원본량 기록

            return pickup; // 대상 결과 반환
        }
    }
}
