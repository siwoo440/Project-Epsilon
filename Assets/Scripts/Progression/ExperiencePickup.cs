using ProjectEpsilon.Core;
using UnityEngine;

namespace ProjectEpsilon.Progression
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ExperiencePickup : MonoBehaviour
    {
        [SerializeField] private SnakeExperience receiver;
        [Min(1)] [SerializeField] private int experienceValue = 1;
        [Min(0.1f)] [SerializeField] private float attractionRange = 2.6f;
        [Min(0.1f)] [SerializeField] private float attractionSpeed = 6f;
        [Min(0.01f)] [SerializeField] private float collectDistance = 0.22f;

        private SpriteRenderer spriteRenderer;
        private bool collected;
        private bool destructionScheduled; // 파괴 예약 여부
        private Sprite fallbackSprite; // 이미지 설정이 없는 씬의 기존 표시

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Configure(
            SnakeExperience experienceReceiver,
            int value,
            Sprite visual
        )
        {
            if (destructionScheduled) // 파괴 예약 상태 확인
            { // 조건 시작
                return; // 제거 예정 보석 재설정 방지
            } // 조건 끝
            receiver = experienceReceiver;
            ExperiencePickupSettings settings = receiver != null ? receiver.PickupSettings : SnakeExperience.Current != null ? SnakeExperience.Current.PickupSettings : null; // 실제 수신자의 흡수 설정
            if (settings != null) // 공통 설정 존재 확인
            { // 조건 시작
                attractionRange = settings.AttractionRange; // 흡수 범위 적용
                attractionSpeed = settings.AttractionSpeed; // 흡수 속도 적용
                collectDistance = settings.CollectDistance; // 획득 거리 적용
            } // 조건 끝
            fallbackSprite = visual; // 기존 대체 이미지 저장
            experienceValue = Mathf.Max(1, value);
            collected = false;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = visual;
            spriteRenderer.color = new Color(1f, 0.9f, 0.25f, 1f);
            RefreshArtwork(); // 실제 보석 등급 이미지 반영
            spriteRenderer.sortingOrder = 16;

            float scale = ResolveVisualScale(experienceValue);
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void Update()
        {
            if (collected)
            {
                return;
            }

            if (GameManager.Instance != null &&
                !GameManager.Instance.IsPlaying)
            {
                return;
            }

            if (receiver == null)
            {
                receiver = SnakeExperience.Current;

                if (receiver == null)
                {
                    return;
                }
            }

            Vector3 targetPosition = receiver.transform.position;
            Vector3 delta = targetPosition - transform.position;
            float collectDistanceSquared =
                collectDistance * collectDistance;

            if (delta.sqrMagnitude <= collectDistanceSquared)
            {
                Collect();
                return;
            }

            float attractionRangeSquared =
                MergeAttractionRange * MergeAttractionRange; // 영구 성장 흡수 범위 적용

            if (delta.sqrMagnitude > attractionRangeSquared)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                attractionSpeed * Time.deltaTime
            );

            if ((targetPosition - transform.position).sqrMagnitude <=
                collectDistanceSquared)
            {
                Collect();
            }
        }

        private void Collect()
        {
            if (collected || receiver == null)
            {
                return;
            }

            collected = true;
            destructionScheduled = true; // 획득 중 재설정 및 중복 사용 방지
            receiver.AddExperience(experienceValue);
            Destroy(gameObject);
        }

        public int ExperienceValue => experienceValue; // 보석 경험치 조회
        internal bool IsMergeAvailable => !collected && isActiveAndEnabled && gameObject.activeInHierarchy; // 병합 가능 상태
        internal SnakeExperience MergeReceiver => receiver != null ? receiver : SnakeExperience.Current; // 실제 경험치 수신자
        internal float MergeAttractionRange => Mathf.Max(0f, attractionRange * MetaModifiers.PickupRangeMultiplier); // 영구 성장의 안전한 흡수 거리

        internal bool IsOutsideAttraction() // 자동 흡수 범위 밖 확인
        { // 메서드 시작
            SnakeExperience target = MergeReceiver; // 실제 수신자 조회
            if (target == null) // 수신자 부재 확인
            { // 조건 시작
                return false; // 보석 유지
            } // 조건 끝
            float safeRange = MergeAttractionRange; // 안전한 흡수 범위
            return (target.transform.position - transform.position).sqrMagnitude > safeRange * safeRange; // 흡수 범위 밖 여부
        } // 메서드 끝

        internal void PromoteForMerge(int value) // 병합 결과 값 반영
        { // 메서드 시작
            experienceValue = value; // 보존한 경험치 저장
            RefreshArtwork(); // 병합된 보석 등급 이미지 갱신
            gameObject.name = $"XP_Gem_{value}"; // 결과 등급 이름 갱신
            float scale = ResolveVisualScale(value); // 결과 등급 크기
            transform.localScale = new Vector3(scale, scale, 1f); // 보석 크기 갱신
        } // 메서드 끝

        internal void ConsumeForMerge() // 병합 재료 보석 제거
        { // 메서드 시작
            collected = true; // 같은 프레임 중복 수집 방지
            destructionScheduled = true; // 파괴 예약 중 재설정 방지
            Destroy(gameObject); // 병합 재료 제거 예약
        } // 메서드 끝

        private void RefreshArtwork() // 실제 경험치별 이미지 설정
        { // 메서드 시작
            if (spriteRenderer == null) // 실제 표시기 확인
            { // 조건 시작
                return; // 표시기 없는 갱신 방지
            } // 조건 끝
            ExperienceGemVisualPalette palette = FindFirstObjectByType<ExperienceGemVisualPalette>(); // 실제 씬 보석 이미지 설정
            Sprite selected = palette != null && palette.isActiveAndEnabled ? palette.Resolve(experienceValue) : null; // 실제 등급별 이미지
            spriteRenderer.sprite = selected != null ? selected : fallbackSprite; // 전용 이미지 또는 기존 대체 이미지
            spriteRenderer.color = selected != null ? Color.white : new Color(1f, 0.9f, 0.25f, 1f); // 원본 색상 또는 기존 임시 색상
        } // 메서드 끝

        private static float ResolveVisualScale(int value)
        {
            if (value >= 20)
            {
                return 1.02f; // 대형 보석 세 배 크기
            }

            if (value >= 5)
            {
                return 0.78f; // 중형 보석 세 배 크기
            }

            return 0.54f; // 소형 보석 세 배 크기
        }

        public static ExperiencePickup Spawn(
            Vector3 position,
            int value,
            Sprite visual,
            SnakeExperience receiver
        )
        {
            GameObject pickupObject =
                new GameObject($"XP_Gem_{Mathf.Max(1, value)}");

            pickupObject.transform.position = position;

            ExperiencePickup pickup =
                pickupObject.AddComponent<ExperiencePickup>();

            pickup.Configure(
                receiver,
                value,
                visual
            );

            return pickup;
        }
    }
}
