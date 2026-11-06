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
            experienceValue = Mathf.Max(1, value);
            collected = false;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = visual;
            spriteRenderer.color = new Color(1f, 0.9f, 0.25f, 1f);
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
                attractionRange * attractionRange;

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
        internal float MergeAttractionRange => Mathf.Max(0f, attractionRange); // 안전한 자동 흡수 거리

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

        private static float ResolveVisualScale(int value)
        {
            if (value >= 20)
            {
                return 0.34f;
            }

            if (value >= 5)
            {
                return 0.26f;
            }

            return 0.18f;
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
