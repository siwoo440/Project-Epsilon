using System;
using System.Collections.Generic;
using ProjectEpsilon.Core; // 보스전 상태 참조
using UnityEngine;

namespace ProjectEpsilon.Player
{
    [DefaultExecutionOrder(50)]
    public sealed class SnakeBodyManager : MonoBehaviour
    {
        [SerializeField] private SnakePathRecorder pathRecorder;
        [SerializeField] private SnakeBodyFollower bodyFollower;
        [SerializeField] private Sprite bodySprite;
        [SerializeField] private Sprite tailSprite; // 별도 꼬리 이미지
        [SerializeField] private int startingBodyCount = 3;
        [SerializeField] private int maximumBodyCount = 20;

        [Header("Progression")]
        [SerializeField] private int earnedBodyCount = 3;

        [SerializeField] private float segmentSpacing = 0.58f;

        [SerializeField] private Vector3 bodyScale =
            new Vector3(0.62f, 0.62f, 1f);

        [SerializeField] private Vector3 tailScale =
            new Vector3(0.48f, 0.48f, 1f);

        [SerializeField] private Color bodyColor =
            new Color(0.62f, 0.48f, 1f, 1f);

        [SerializeField] private Color tailColor =
            new Color(0.42f, 0.32f, 0.78f, 1f);

        [SerializeField] private float bodyColliderRadius = 0.36f;

        private readonly List<SnakeSegment> bodySegments =
            new List<SnakeSegment>();

        private SnakeSegment tailSegment;
        private bool initialized;

        public event Action<int, int> BodyCountChanged;

        public int CurrentBodyCount
        {
            get
            {
                EnsureInitialized();
                return bodySegments.Count;
            }
        }

        public int MaximumBodyCount => maximumBodyCount;
        public int StartingBodyCount => startingBodyCount;

        public int EarnedBodyCount
        {
            get
            {
                EnsureInitialized();
                NormalizeEarnedBodyCount();
                return earnedBodyCount;
            }
        }

        public int MissingRepairableBodyCount
        {
            get
            {
                EnsureInitialized();
                NormalizeEarnedBodyCount();

                return Mathf.Max(
                    0,
                    earnedBodyCount - bodySegments.Count
                );
            }
        }

        public IReadOnlyList<SnakeSegment> BodySegments
        {
            get
            {
                EnsureInitialized();
                return bodySegments;
            }
        }

        public SnakeSegment TailSegment
        {
            get
            {
                EnsureInitialized();
                return tailSegment;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
            EnsureStartingBodyCount();
            NormalizeEarnedBodyCount();
            EnsureTail();
            RefreshFollower();
        }

        private void Start()
        {
            NotifyBodyCountChanged();
        }

        public void Configure(
            SnakePathRecorder recorder,
            SnakeBodyFollower follower,
            Sprite segmentSprite,
            int startingCount,
            int maximumCount,
            float spacing
        )
        {
            pathRecorder = recorder;
            bodyFollower = follower;
            bodySprite = segmentSprite;
            maximumBodyCount = Mathf.Clamp(maximumCount, 1, 20); // 절대 몸통 상한 적용
            startingBodyCount = Mathf.Clamp(
                startingCount,
                0,
                maximumBodyCount
            );

            segmentSpacing = Mathf.Max(0.05f, spacing);

            initialized = false;
            EnsureInitialized();
            TrimToMaximumBodyCount();

            earnedBodyCount = Mathf.Clamp(
                Mathf.Max(
                    startingBodyCount,
                    bodySegments.Count
                ),
                0,
                maximumBodyCount
            );

            EnsureTail();
            RefreshFollower();
            NotifyBodyCountChanged();
        }

        // 기존 Day05~Day12 호출과의 호환용이다.
        // Level Up에서 Body를 얻는 의미로 유지한다.
        public bool TryAddBody()
        {
            return TryGainBodyFromLevelUp();
        }

        public bool TryGainBodyFromLevelUp()
        {
            EnsureInitialized();
            NormalizeEarnedBodyCount();

            if (bodySegments.Count >= maximumBodyCount)
            {
                return false;
            }

            if (earnedBodyCount < maximumBodyCount)
            {
                earnedBodyCount++;
            }

            CreateBodySegment();

            earnedBodyCount = Mathf.Clamp(
                Mathf.Max(
                    earnedBodyCount,
                    bodySegments.Count
                ),
                0,
                maximumBodyCount
            );

            RefreshFollower();
            NotifyBodyCountChanged();
            return true;
        }

        public bool TryRepairBody()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsFinalBossPhase) // 별도 복구 제한 확인
            { // 조건 시작
                return false; // 보스전 몸통 복구 차단
            } // 조건 끝
            EnsureInitialized();
            NormalizeEarnedBodyCount();

            if (bodySegments.Count >= earnedBodyCount ||
                bodySegments.Count >= maximumBodyCount)
            {
                return false;
            }

            CreateBodySegment();
            RefreshFollower();
            NotifyBodyCountChanged();
            return true;
        }

        public void InitializeEarnedBodyCountFromCurrent()
        {
            EnsureInitialized();
            int previousEarnedBodyCount = earnedBodyCount; // 기존 성장 최대 수

            earnedBodyCount = Mathf.Clamp(
                Mathf.Max(
                    startingBodyCount,
                    bodySegments.Count
                ),
                0,
                maximumBodyCount
            );
            if (earnedBodyCount != previousEarnedBodyCount) // 성장 최대 변경 확인
            { // 조건 시작
                NotifyBodyCountChanged(); // 몸통 상태 변경 알림
            } // 조건 끝
        }

        public bool TryRemoveBody()
        {
            return RemoveBodies(1) > 0;
        }

        public int RemoveBodies(int count)
        {
            EnsureInitialized();

            int requestedCount = Mathf.Max(0, count);

            int removedCount = Mathf.Min(
                requestedCount,
                bodySegments.Count
            );

            if (removedCount <= 0)
            {
                return 0;
            }

            for (int removed = 0;
                removed < removedCount;
                removed++)
            {
                int lastIndex =
                    bodySegments.Count - 1;

                SnakeSegment segment =
                    bodySegments[lastIndex];

                bodySegments.RemoveAt(
                    lastIndex
                );

                if (segment != null)
                {
                    DestroySegmentObject(
                        segment.gameObject
                    );
                }
            }

            ReindexBodySegments();
            RefreshFollower();
            NotifyBodyCountChanged();
            return removedCount;
        }

        public bool SetStartingBodyCount(int count) // 런 시작 몸통 수 설정
        { // 메서드 시작
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Starting) // 실제 시작 상태 확인
            { // 조건 시작
                return false; // 진행 중 몸통 초기화 방지
            } // 조건 끝
            int safeCount = Mathf.Clamp(count, 1, maximumBodyCount); // 절대 상한 이내 시작 수
            if (startingBodyCount != safeCount || CurrentBodyCount != safeCount || EarnedBodyCount != safeCount) // 기존 시작 구성과 차이 확인
            { // 조건 시작
                startingBodyCount = safeCount; // 새 시작 몸통 수 저장
                ResetBody(); // 실제 몸통과 성장 최대 초기화
            } // 조건 끝
            return true; // 실제 시작 설정 완료
        } // 메서드 끝

        public void ResetBody()
        {
            EnsureInitialized();

            for (int index =
                bodySegments.Count - 1;
                index >= 0;
                index--)
            {
                SnakeSegment segment =
                    bodySegments[index];

                if (segment != null)
                {
                    DestroySegmentObject(
                        segment.gameObject
                    );
                }
            }

            bodySegments.Clear();

            for (int index = 0;
                index < startingBodyCount;
                index++)
            {
                CreateBodySegment();
            }

            earnedBodyCount =
                startingBodyCount;

            EnsureTail();
            RefreshFollower();
            NotifyBodyCountChanged();
        }

        public void RefreshFollower()
        {
            EnsureInitialized();

            if (bodyFollower == null)
            {
                bodyFollower =
                    GetComponent<SnakeBodyFollower>();
            }

            if (bodyFollower == null)
            {
                return;
            }

            Transform[] transforms =
                new Transform[
                    bodySegments.Count
                ];

            for (int index = 0;
                index < bodySegments.Count;
                index++)
            {
                transforms[index] =
                    bodySegments[index] == null
                        ? null
                        : bodySegments[index].transform;
            }

            Transform tailTransform =
                tailSegment == null
                    ? null
                    : tailSegment.transform;

            bodyFollower.Bind(
                pathRecorder,
                transforms,
                tailTransform,
                segmentSpacing
            );
        }

        private void EnsureInitialized()
        {
            maximumBodyCount = Mathf.Clamp(maximumBodyCount, 1, 20); // 직렬화 상한 제한
            startingBodyCount = Mathf.Clamp(startingBodyCount, 0, maximumBodyCount); // 시작 몸통 범위 제한

            if (initialized)
            {
                return;
            }

            initialized = true;
            bodySegments.Clear();
            tailSegment = null;

            for (int childIndex = 0;
                childIndex < transform.childCount;
                childIndex++)
            {
                Transform child =
                    transform.GetChild(
                        childIndex
                    );

                if (child == null)
                {
                    continue;
                }

                if (child.name.StartsWith(
                    "Body_",
                    StringComparison.Ordinal
                ))
                {
                    SnakeSegment body =
                        GetOrAddSegment(
                            child.gameObject
                        );

                    bodySegments.Add(body);
                    continue;
                }

                if (child.name == "Tail")
                {
                    tailSegment =
                        GetOrAddSegment(
                            child.gameObject
                        );
                }
            }

            bodySegments.Sort(
                (left, right) =>
                    left.transform
                        .GetSiblingIndex()
                        .CompareTo(
                            right.transform
                                .GetSiblingIndex()
                        )
            );

            ReindexBodySegments();

            if (tailSegment != null)
            {
                tailSegment.Configure(
                    SnakeSegmentType.Tail,
                    -1
                );
            }

            NormalizeEarnedBodyCount();
        }

        private void EnsureStartingBodyCount()
        {
            TrimToMaximumBodyCount();

            while (bodySegments.Count <
                startingBodyCount)
            {
                CreateBodySegment();
            }

            NormalizeEarnedBodyCount();
        }

        private void TrimToMaximumBodyCount()
        {
            while (bodySegments.Count >
                maximumBodyCount)
            {
                int lastIndex =
                    bodySegments.Count - 1;

                SnakeSegment segment =
                    bodySegments[lastIndex];

                bodySegments.RemoveAt(
                    lastIndex
                );

                if (segment != null)
                {
                    DestroySegmentObject(
                        segment.gameObject
                    );
                }
            }

            ReindexBodySegments();
            NormalizeEarnedBodyCount();
        }

        private void NormalizeEarnedBodyCount()
        {
            earnedBodyCount = Mathf.Clamp(
                Mathf.Max(
                    earnedBodyCount,
                    bodySegments.Count,
                    startingBodyCount
                ),
                0,
                maximumBodyCount
            );
        }

        private SnakeSegment CreateBodySegment()
        {
            int index =
                bodySegments.Count;

            GameObject segmentObject =
                new GameObject(
                    $"Body_{index + 1:00}"
                );

            segmentObject.transform.SetParent(
                transform,
                false
            );

            segmentObject.transform.localScale =
                bodyScale;

            SpriteRenderer renderer =
                segmentObject.AddComponent<SpriteRenderer>();

            renderer.sprite =
                bodySprite;

            renderer.color =
                bodyColor;

            renderer.sortingOrder =
                6 - index;

            SnakeSegment segment =
                segmentObject.AddComponent<SnakeSegment>();

            segment.Configure(
                SnakeSegmentType.Body,
                index
            );

            EnsureBodyCollider(
                segmentObject
            );

            bodySegments.Add(segment);

            return segment;
        }

        private void EnsureTail()
        {
            if (tailSegment == null)
            {
                GameObject tailObject =
                    new GameObject("Tail");

                tailObject.transform.SetParent(
                    transform,
                    false
                );

                tailObject.transform.localScale =
                    tailScale;

                SpriteRenderer renderer =
                    tailObject.AddComponent<SpriteRenderer>();

                renderer.sprite = tailSprite != null ? tailSprite : bodySprite; // 꼬리 이미지와 기존 대체 이미지

                renderer.color =
                    tailColor;

                renderer.sortingOrder =
                    -20;

                tailSegment =
                    tailObject.AddComponent<SnakeSegment>();
            }

            tailSegment.name =
                "Tail";

            tailSegment.Configure(
                SnakeSegmentType.Tail,
                -1
            );

            SpriteRenderer tailRenderer =
                tailSegment.GetComponent<SpriteRenderer>();

            if (tailRenderer != null && (tailSprite != null || bodySprite != null)) // 실제 꼬리 표시와 이미지 확인
            {
                tailRenderer.sprite = tailSprite != null ? tailSprite : bodySprite; // 별도 꼬리 이미지 유지
            }

            tailSegment.transform.localScale =
                tailScale;
        }

        private void ReindexBodySegments()
        {
            for (int index = 0;
                index < bodySegments.Count;
                index++)
            {
                SnakeSegment segment =
                    bodySegments[index];

                if (segment == null)
                {
                    continue;
                }

                segment.name =
                    $"Body_{index + 1:00}";

                segment.Configure(
                    SnakeSegmentType.Body,
                    index
                );

                SpriteRenderer renderer =
                    segment.GetComponent<SpriteRenderer>();

                if (renderer != null)
                {
                    if (bodySprite != null)
                    {
                        renderer.sprite =
                            bodySprite;
                    }

                    renderer.color =
                        bodyColor;

                    renderer.sortingOrder =
                        6 - index;
                }

                segment.transform.localScale =
                    bodyScale;

                EnsureBodyCollider(
                    segment.gameObject
                );
            }
        }

        private void EnsureBodyCollider(
            GameObject target
        )
        {
            CircleCollider2D collider =
                target.GetComponent<CircleCollider2D>();

            if (collider == null)
            {
                collider =
                    target.AddComponent<CircleCollider2D>();
            }

            collider.isTrigger =
                true;

            collider.radius =
                Mathf.Max(
                    0.05f,
                    bodyColliderRadius
                );
        }

        private static SnakeSegment GetOrAddSegment(
            GameObject target
        )
        {
            SnakeSegment segment =
                target.GetComponent<SnakeSegment>();

            if (segment == null)
            {
                segment =
                    target.AddComponent<SnakeSegment>();
            }

            return segment;
        }

        private void NotifyBodyCountChanged()
        {
            BodyCountChanged?.Invoke(
                bodySegments.Count,
                maximumBodyCount
            );
        }

        private static void DestroySegmentObject(
            GameObject target
        )
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
