using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectEpsilon.Player
{
    [DefaultExecutionOrder(50)]
    public sealed class SnakeBodyManager : MonoBehaviour
    {
        [SerializeField] private SnakePathRecorder pathRecorder;
        [SerializeField] private SnakeBodyFollower bodyFollower;
        [SerializeField] private Sprite bodySprite;
        [SerializeField] private int startingBodyCount = 3;
        [SerializeField] private int maximumBodyCount = 20;
        [SerializeField] private float segmentSpacing = 0.58f;

        [SerializeField] private Vector3 bodyScale = new Vector3(0.62f, 0.62f, 1f);
        [SerializeField] private Vector3 tailScale = new Vector3(0.48f, 0.48f, 1f);
        [SerializeField] private Color bodyColor = new Color(0.62f, 0.48f, 1f, 1f);
        [SerializeField] private Color tailColor = new Color(0.42f, 0.32f, 0.78f, 1f);
        [SerializeField] private float bodyColliderRadius = 0.36f;

        private readonly List<SnakeSegment> bodySegments = new List<SnakeSegment>();
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
            maximumBodyCount = Mathf.Max(1, maximumCount);
            startingBodyCount = Mathf.Clamp(startingCount, 0, maximumBodyCount);
            segmentSpacing = Mathf.Max(0.05f, spacing);

            initialized = false;
            EnsureInitialized();
            TrimToMaximumBodyCount();
            EnsureTail();
            RefreshFollower();
            NotifyBodyCountChanged();
        }

        public bool TryAddBody()
        {
            EnsureInitialized();

            if (bodySegments.Count >= maximumBodyCount)
            {
                return false;
            }

            CreateBodySegment();
            RefreshFollower();
            NotifyBodyCountChanged();
            return true;
        }

        public bool TryRemoveBody()
        {
            return RemoveBodies(1) > 0;
        }

        public int RemoveBodies(int count)
        {
            EnsureInitialized();

            int requestedCount = Mathf.Max(0, count);
            int removedCount = Mathf.Min(requestedCount, bodySegments.Count);

            if (removedCount <= 0)
            {
                return 0;
            }

            for (int removed = 0; removed < removedCount; removed++)
            {
                int lastIndex = bodySegments.Count - 1;
                SnakeSegment segment = bodySegments[lastIndex];
                bodySegments.RemoveAt(lastIndex);

                if (segment != null)
                {
                    DestroySegmentObject(segment.gameObject);
                }
            }

            ReindexBodySegments();
            RefreshFollower();
            NotifyBodyCountChanged();
            return removedCount;
        }

        public void ResetBody()
        {
            EnsureInitialized();

            for (int index = bodySegments.Count - 1; index >= 0; index--)
            {
                SnakeSegment segment = bodySegments[index];

                if (segment != null)
                {
                    DestroySegmentObject(segment.gameObject);
                }
            }

            bodySegments.Clear();

            for (int index = 0; index < startingBodyCount; index++)
            {
                CreateBodySegment();
            }

            EnsureTail();
            RefreshFollower();
            NotifyBodyCountChanged();
        }

        public void RefreshFollower()
        {
            EnsureInitialized();

            if (bodyFollower == null)
            {
                bodyFollower = GetComponent<SnakeBodyFollower>();
            }

            if (bodyFollower == null)
            {
                return;
            }

            Transform[] transforms = new Transform[bodySegments.Count];

            for (int index = 0; index < bodySegments.Count; index++)
            {
                transforms[index] = bodySegments[index] == null
                    ? null
                    : bodySegments[index].transform;
            }

            Transform tailTransform = tailSegment == null ? null : tailSegment.transform;
            bodyFollower.Bind(pathRecorder, transforms, tailTransform, segmentSpacing);
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            bodySegments.Clear();
            tailSegment = null;

            for (int childIndex = 0; childIndex < transform.childCount; childIndex++)
            {
                Transform child = transform.GetChild(childIndex);

                if (child == null)
                {
                    continue;
                }

                if (child.name.StartsWith("Body_", StringComparison.Ordinal))
                {
                    SnakeSegment body = GetOrAddSegment(child.gameObject);
                    bodySegments.Add(body);
                    continue;
                }

                if (child.name == "Tail")
                {
                    tailSegment = GetOrAddSegment(child.gameObject);
                }
            }

            bodySegments.Sort(
                (left, right) => left.transform.GetSiblingIndex()
                    .CompareTo(right.transform.GetSiblingIndex())
            );

            ReindexBodySegments();

            if (tailSegment != null)
            {
                tailSegment.Configure(SnakeSegmentType.Tail, -1);
            }
        }

        private void EnsureStartingBodyCount()
        {
            TrimToMaximumBodyCount();

            while (bodySegments.Count < startingBodyCount)
            {
                CreateBodySegment();
            }
        }

        private void TrimToMaximumBodyCount()
        {
            while (bodySegments.Count > maximumBodyCount)
            {
                int lastIndex = bodySegments.Count - 1;
                SnakeSegment segment = bodySegments[lastIndex];
                bodySegments.RemoveAt(lastIndex);

                if (segment != null)
                {
                    DestroySegmentObject(segment.gameObject);
                }
            }

            ReindexBodySegments();
        }

        private SnakeSegment CreateBodySegment()
        {
            int index = bodySegments.Count;
            GameObject segmentObject = new GameObject($"Body_{index + 1:00}");
            segmentObject.transform.SetParent(transform, false);
            segmentObject.transform.localScale = bodyScale;

            SpriteRenderer renderer = segmentObject.AddComponent<SpriteRenderer>();
            renderer.sprite = bodySprite;
            renderer.color = bodyColor;
            renderer.sortingOrder = 6 - index;

            SnakeSegment segment = segmentObject.AddComponent<SnakeSegment>();
            segment.Configure(SnakeSegmentType.Body, index);
            EnsureBodyCollider(segmentObject);
            bodySegments.Add(segment);

            return segment;
        }

        private void EnsureTail()
        {
            if (tailSegment == null)
            {
                GameObject tailObject = new GameObject("Tail");
                tailObject.transform.SetParent(transform, false);
                tailObject.transform.localScale = tailScale;

                SpriteRenderer renderer = tailObject.AddComponent<SpriteRenderer>();
                renderer.sprite = bodySprite;
                renderer.color = tailColor;
                renderer.sortingOrder = -20;

                tailSegment = tailObject.AddComponent<SnakeSegment>();
            }

            tailSegment.name = "Tail";
            tailSegment.Configure(SnakeSegmentType.Tail, -1);

            SpriteRenderer tailRenderer = tailSegment.GetComponent<SpriteRenderer>();

            if (tailRenderer != null && bodySprite != null)
            {
                tailRenderer.sprite = bodySprite;
            }

            tailSegment.transform.localScale = tailScale;
        }

        private void ReindexBodySegments()
        {
            for (int index = 0; index < bodySegments.Count; index++)
            {
                SnakeSegment segment = bodySegments[index];

                if (segment == null)
                {
                    continue;
                }

                segment.name = $"Body_{index + 1:00}";
                segment.Configure(SnakeSegmentType.Body, index);

                SpriteRenderer renderer = segment.GetComponent<SpriteRenderer>();

                if (renderer != null)
                {
                    if (bodySprite != null)
                    {
                        renderer.sprite = bodySprite;
                    }

                    renderer.color = bodyColor;
                    renderer.sortingOrder = 6 - index;
                }

                segment.transform.localScale = bodyScale;
                EnsureBodyCollider(segment.gameObject);
            }
        }

        private void EnsureBodyCollider(GameObject target)
        {
            CircleCollider2D collider = target.GetComponent<CircleCollider2D>();

            if (collider == null)
            {
                collider = target.AddComponent<CircleCollider2D>();
            }

            collider.isTrigger = true;
            collider.radius = Mathf.Max(0.05f, bodyColliderRadius);
        }

        private static SnakeSegment GetOrAddSegment(GameObject target)
        {
            SnakeSegment segment = target.GetComponent<SnakeSegment>();

            if (segment == null)
            {
                segment = target.AddComponent<SnakeSegment>();
            }

            return segment;
        }

        private void NotifyBodyCountChanged()
        {
            BodyCountChanged?.Invoke(bodySegments.Count, maximumBodyCount);
        }

        private static void DestroySegmentObject(GameObject target)
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
