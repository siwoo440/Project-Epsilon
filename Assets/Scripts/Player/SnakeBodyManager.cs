using System; // C# 기본 기능 연결
using System.Collections.Generic; // C# 기본 기능 연결
using ProjectEpsilon.Core; // 보스전 상태 참조
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{ // 이름 공간 시작
    [DefaultExecutionOrder(50)] // 대상 설정
    public sealed class SnakeBodyManager : MonoBehaviour // 몸통·관리자 자료 구조
    { // 형식 시작
        [SerializeField] private SnakePathRecorder pathRecorder; // 필드·경로 설정
        [SerializeField] private SnakeBodyFollower bodyFollower; // 필드·몸통 설정
        [SerializeField] private Sprite bodySprite; // 필드·이미지 설정
        [SerializeField] private Sprite tailSprite; // 별도 꼬리 이미지
        [SerializeField] private int startingBodyCount = 3; // 필드·몸통 설정
        [SerializeField] private int maximumBodyCount = 20; // 필드·최대 설정

        [Header("Progression")] // 대상 설정
        [SerializeField] private int earnedBodyCount = 3; // 필드·몸통 설정

        [SerializeField] private float segmentSpacing = 0.58f; // 필드·몸통 설정

        [SerializeField] private Vector3 bodyScale = // 필드·벡터 설정
            new Vector3(0.62f, 0.62f, 1f); // 벡터 객체 생성

        [SerializeField] private Vector3 tailScale = // 필드·벡터 설정
            new Vector3(0.48f, 0.48f, 1f); // 벡터 객체 생성

        [SerializeField] private Color bodyColor = // 필드·색상 설정
            new Color(0.62f, 0.48f, 1f, 1f); // 색상 객체 생성

        [SerializeField] private Color tailColor = // 필드·색상 설정
            new Color(0.42f, 0.32f, 0.78f, 1f); // 색상 객체 생성

        [SerializeField] private float bodyColliderRadius = 0.36f; // 필드·몸통 설정

        private readonly List<SnakeSegment> bodySegments = // 목록·몸통 구성 선언
            new List<SnakeSegment>(); // 목록·몸통 객체 생성

        private SnakeSegment tailSegment; // 몸통 구성 선언
        private bool initialized; // 초기화 구성 선언
        private Sprite defaultBodySprite; // 기본 몸통 이미지 보관
        private Sprite defaultTailSprite; // 기본 꼬리 이미지 보관
        private bool defaultAppearanceCached; // 기본 외형 보관 여부

        public event Action<int, int> BodyCountChanged; // 이벤트·동작 구성 선언
        public event Action<Vector3> BodyRemoved; // 실제 몸통 손실 위치 알림

        public int CurrentBodyCount // 현재·몸통 구성 선언
        { // 코드 블록 시작
            get // 조회 처리
            { // 코드 블록 시작
                EnsureInitialized(); // 확인·초기화 기능 실행
                return bodySegments.Count; // 몸통·개수 결과 반환
            } // 코드 블록 종료
        } // 코드 블록 종료

        public int MaximumBodyCount => maximumBodyCount; // 최대·몸통 값 반환
        public int StartingBodyCount => startingBodyCount; // 몸통·개수 값 반환

        public int EarnedBodyCount // 몸통·개수 구성 선언
        { // 코드 블록 시작
            get // 조회 처리
            { // 코드 블록 시작
                EnsureInitialized(); // 확인·초기화 기능 실행
                NormalizeEarnedBodyCount(); // 몸통·개수 기능 실행
                return earnedBodyCount; // 몸통·개수 결과 반환
            } // 코드 블록 종료
        } // 코드 블록 종료

        public int MissingRepairableBodyCount // 몸통·개수 구성 선언
        { // 코드 블록 시작
            get // 조회 처리
            { // 코드 블록 시작
                EnsureInitialized(); // 확인·초기화 기능 실행
                NormalizeEarnedBodyCount(); // 몸통·개수 기능 실행

                return Mathf.Max( // 최대 결과 반환
                    0, // 대상 항목 전달
                    earnedBodyCount - bodySegments.Count // 몸통·개수 처리
                ); // 앞선 항목 종료
            } // 코드 블록 종료
        } // 코드 블록 종료

        public IReadOnlyList<SnakeSegment> BodySegments // 목록·몸통 구성 선언
        { // 코드 블록 시작
            get // 조회 처리
            { // 코드 블록 시작
                EnsureInitialized(); // 확인·초기화 기능 실행
                return bodySegments; // 몸통 결과 반환
            } // 코드 블록 종료
        } // 코드 블록 종료

        public SnakeSegment TailSegment // 몸통 구성 선언
        { // 코드 블록 시작
            get // 조회 처리
            { // 코드 블록 시작
                EnsureInitialized(); // 확인·초기화 기능 실행
                return tailSegment; // 몸통 결과 반환
            } // 코드 블록 종료
        } // 코드 블록 종료

        private void Awake() // 초기화 기능 선언
        { // 함수 시작
            EnsureInitialized(); // 확인·초기화 기능 실행
            EnsureStartingBodyCount(); // 확인·몸통 기능 실행
            NormalizeEarnedBodyCount(); // 몸통·개수 기능 실행
            EnsureTail(); // 확인 기능 실행
            RefreshFollower(); // 갱신 기능 실행
        } // 함수 종료

        private void Start() // 시작 기능 선언
        { // 함수 시작
            NotifyBodyCountChanged(); // 알림·몸통 기능 실행
        } // 함수 종료

        public void Configure( // 구성 호출 시작
            SnakePathRecorder recorder, // 경로 항목 전달
            SnakeBodyFollower follower, // 몸통 항목 전달
            Sprite segmentSprite, // 이미지·몸통 항목 전달
            int startingCount, // 개수 항목 전달
            int maximumCount, // 최대·개수 항목 전달
            float spacing // 대상 처리
        ) // 앞선 항목 종료
        { // 함수 시작
            pathRecorder = recorder; // 경로 값 설정
            bodyFollower = follower; // 몸통 값 설정
            bodySprite = segmentSprite; // 몸통·이미지 값 설정
            maximumBodyCount = Mathf.Clamp(maximumCount, 1, 20); // 절대 몸통 상한 적용
            startingBodyCount = Mathf.Clamp( // 몸통·개수 호출 시작
                startingCount, // 개수 항목 전달
                0, // 대상 항목 전달
                maximumBodyCount // 최대·몸통 처리
            ); // 앞선 항목 종료

            segmentSpacing = Mathf.Max(0.05f, spacing); // 몸통·최대 값 설정

            initialized = false; // 초기화 값 설정
            EnsureInitialized(); // 확인·초기화 기능 실행
            TrimToMaximumBodyCount(); // 최대·몸통 기능 실행

            earnedBodyCount = Mathf.Clamp( // 몸통·개수 호출 시작
                Mathf.Max( // 최대 호출 시작
                    startingBodyCount, // 몸통·개수 항목 전달
                    bodySegments.Count // 몸통·개수 처리
                ), // 대상 항목 전달
                0, // 대상 항목 전달
                maximumBodyCount // 최대·몸통 처리
            ); // 앞선 항목 종료

            EnsureTail(); // 확인 기능 실행
            RefreshFollower(); // 갱신 기능 실행
            NotifyBodyCountChanged(); // 알림·몸통 기능 실행
        } // 함수 종료

        // 기존 Day05~Day12 호출과의 호환용이다.
        // Level Up에서 Body를 얻는 의미로 유지한다.
        public bool TryAddBody() // 시도·추가 기능 선언
        { // 함수 시작
            return TryGainBodyFromLevelUp(); // 시도·몸통 결과 반환
        } // 함수 종료

        public bool TryGainBodyFromLevelUp() // 시도·몸통 기능 선언
        { // 함수 시작
            EnsureInitialized(); // 확인·초기화 기능 실행
            NormalizeEarnedBodyCount(); // 몸통·개수 기능 실행

            if (bodySegments.Count >= maximumBodyCount) // 몸통·개수 조건 확인
            { // 조건 시작
                return false; // 실패 결과 반환
            } // 조건 종료

            if (earnedBodyCount < maximumBodyCount) // 몸통·개수 조건 확인
            { // 조건 시작
                earnedBodyCount++; // 몸통·개수 처리
            } // 조건 종료

            CreateBodySegment(); // 생성·몸통 기능 실행

            earnedBodyCount = Mathf.Clamp( // 몸통·개수 호출 시작
                Mathf.Max( // 최대 호출 시작
                    earnedBodyCount, // 몸통·개수 항목 전달
                    bodySegments.Count // 몸통·개수 처리
                ), // 대상 항목 전달
                0, // 대상 항목 전달
                maximumBodyCount // 최대·몸통 처리
            ); // 앞선 항목 종료

            RefreshFollower(); // 갱신 기능 실행
            NotifyBodyCountChanged(); // 알림·몸통 기능 실행
            return true; // 성공 결과 반환
        } // 함수 종료

        public bool TryRepairBody() // 시도·몸통 기능 선언
        { // 함수 시작
            if (GameManager.Instance != null && GameManager.Instance.IsFinalBossPhase) // 별도 복구 제한 확인
            { // 조건 시작
                return false; // 보스전 몸통 복구 차단
            } // 조건 종료
            EnsureInitialized(); // 확인·초기화 기능 실행
            NormalizeEarnedBodyCount(); // 몸통·개수 기능 실행

            if (bodySegments.Count >= earnedBodyCount || // 몸통·개수 조건 확인
                bodySegments.Count >= maximumBodyCount) // 몸통·개수 처리
            { // 조건 시작
                return false; // 실패 결과 반환
            } // 조건 종료

            CreateBodySegment(); // 생성·몸통 기능 실행
            RefreshFollower(); // 갱신 기능 실행
            NotifyBodyCountChanged(); // 알림·몸통 기능 실행
            return true; // 성공 결과 반환
        } // 함수 종료

        public void InitializeEarnedBodyCountFromCurrent() // 초기화·몸통 기능 선언
        { // 함수 시작
            EnsureInitialized(); // 확인·초기화 기능 실행
            int previousEarnedBodyCount = earnedBodyCount; // 기존 성장 최대 수

            earnedBodyCount = Mathf.Clamp( // 몸통·개수 호출 시작
                Mathf.Max( // 최대 호출 시작
                    startingBodyCount, // 몸통·개수 항목 전달
                    bodySegments.Count // 몸통·개수 처리
                ), // 대상 항목 전달
                0, // 대상 항목 전달
                maximumBodyCount // 최대·몸통 처리
            ); // 앞선 항목 종료
            if (earnedBodyCount != previousEarnedBodyCount) // 성장 최대 변경 확인
            { // 조건 시작
                NotifyBodyCountChanged(); // 몸통 상태 변경 알림
            } // 조건 종료
        } // 함수 종료

        public bool TryRemoveBody() // 시도·제거 기능 선언
        { // 함수 시작
            return RemoveBodies(1) > 0; // 제거 결과 반환
        } // 함수 종료

        public int RemoveBodies(int count) // 제거·개수 기능 선언
        { // 함수 시작
            EnsureInitialized(); // 확인·초기화 기능 실행

            int requestedCount = Mathf.Max(0, count); // 개수·최대 값 설정

            int removedCount = Mathf.Min( // 개수·최소 호출 시작
                requestedCount, // 개수 항목 전달
                bodySegments.Count // 몸통·개수 처리
            ); // 앞선 항목 종료

            if (removedCount <= 0) // 개수 조건 확인
            { // 조건 시작
                return 0; // 대상 결과 반환
            } // 조건 종료

            for (int removed = 0; // 대상 반복 처리
                removed < removedCount; // 개수 처리
                removed++) // 대상 처리
            { // 반복 시작
                int lastIndex = // 순서 값 설정
                    bodySegments.Count - 1; // 몸통·개수 처리

                SnakeSegment segment = // 몸통 값 설정
                    bodySegments[lastIndex]; // 몸통·순서 처리

                bodySegments.RemoveAt( // 몸통·제거 호출 시작
                    lastIndex // 순서 처리
                ); // 앞선 항목 종료

                if (segment != null) // 몸통 조건 확인
                { // 조건 시작
                    BodyRemoved?.Invoke(segment.transform.position); // 실제 제거 위치 알림
                    DestroySegmentObject( // 제거·몸통 호출 시작
                        segment.gameObject // 몸통·게임 처리
                    ); // 앞선 항목 종료
                } // 조건 종료
            } // 반복 종료

            ReindexBodySegments(); // 몸통 기능 실행
            RefreshFollower(); // 갱신 기능 실행
            NotifyBodyCountChanged(); // 알림·몸통 기능 실행
            return removedCount; // 개수 결과 반환
        } // 함수 종료

        public void SetAppearance(Sprite segmentSprite, Sprite endSprite, Color tint) // 캐릭터별 몸통 외형 적용
        { // 함수 시작
            EnsureInitialized(); // 기존 몸통 초기화 확인
            if (!defaultAppearanceCached) // 최초 기본 이미지 확인
            { // 조건 시작
                defaultBodySprite = bodySprite; // 기본 몸통 이미지 보관
                defaultTailSprite = tailSprite; // 기본 꼬리 이미지 보관
                defaultAppearanceCached = true; // 기본 외형 보관 완료
            } // 조건 종료
            bodySprite = segmentSprite != null ? segmentSprite : defaultBodySprite; // 선택 또는 기본 몸통 이미지
            tailSprite = endSprite != null ? endSprite : defaultTailSprite; // 선택 또는 기본 꼬리 이미지
            bodyColor = tint; // 기존과 새 몸통 색상
            tailColor = tint; // 기존과 새 꼬리 색상
            ReindexBodySegments(); // 현재 몸통 이미지와 색상 갱신
            EnsureTail(); // 꼬리 이미지 갱신
            SpriteRenderer tailRenderer = tailSegment == null ? null : tailSegment.GetComponent<SpriteRenderer>(); // 꼬리 표시기 조회
            if (tailRenderer != null) // 꼬리 표시기 확인
            { // 조건 시작
                tailRenderer.color = tailColor; // 현재 꼬리 색상 갱신
            } // 조건 종료
        } // 함수 종료

        public bool SetStartingBodyCount(int count) // 런 시작 몸통 수 설정
        { // 함수 시작
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Starting) // 실제 시작 상태 확인
            { // 조건 시작
                return false; // 진행 중 몸통 초기화 방지
            } // 조건 종료
            int safeCount = Mathf.Clamp(count, 1, maximumBodyCount); // 절대 상한 이내 시작 수
            if (startingBodyCount != safeCount || CurrentBodyCount != safeCount || EarnedBodyCount != safeCount) // 기존 시작 구성과 차이 확인
            { // 조건 시작
                startingBodyCount = safeCount; // 새 시작 몸통 수 저장
                ResetBody(); // 실제 몸통과 성장 최대 초기화
            } // 조건 종료
            return true; // 실제 시작 설정 완료
        } // 함수 종료

        public void ResetBody() // 초기화·몸통 기능 선언
        { // 함수 시작
            EnsureInitialized(); // 확인·초기화 기능 실행

            for (int index = // 순서 반복 처리
                bodySegments.Count - 1; // 몸통·개수 처리
                index >= 0; // 순서 처리
                index--) // 순서 처리
            { // 반복 시작
                SnakeSegment segment = // 몸통 값 설정
                    bodySegments[index]; // 몸통·순서 처리

                if (segment != null) // 몸통 조건 확인
                { // 조건 시작
                    DestroySegmentObject( // 제거·몸통 호출 시작
                        segment.gameObject // 몸통·게임 처리
                    ); // 앞선 항목 종료
                } // 조건 종료
            } // 반복 종료

            bodySegments.Clear(); // 몸통 기능 실행

            for (int index = 0; // 순서 반복 처리
                index < startingBodyCount; // 순서·몸통 처리
                index++) // 순서 처리
            { // 반복 시작
                CreateBodySegment(); // 생성·몸통 기능 실행
            } // 반복 종료

            earnedBodyCount = // 몸통·개수 값 설정
                startingBodyCount; // 몸통·개수 처리

            EnsureTail(); // 확인 기능 실행
            RefreshFollower(); // 갱신 기능 실행
            NotifyBodyCountChanged(); // 알림·몸통 기능 실행
        } // 함수 종료

        public void RefreshFollower() // 갱신 기능 선언
        { // 함수 시작
            EnsureInitialized(); // 확인·초기화 기능 실행

            if (bodyFollower == null) // 몸통 조건 확인
            { // 조건 시작
                bodyFollower = // 몸통 값 설정
                    GetComponent<SnakeBodyFollower>(); // 조회·구성 요소 기능 실행
            } // 조건 종료

            if (bodyFollower == null) // 몸통 조건 확인
            { // 조건 시작
                return; // 현재 기능 종료
            } // 조건 종료

            Transform[] transforms = // 위치 값 설정
                new Transform[ // 위치 객체 생성
                    bodySegments.Count // 몸통·개수 처리
                ]; // 대상 처리

            for (int index = 0; // 순서 반복 처리
                index < bodySegments.Count; // 순서·몸통 처리
                index++) // 순서 처리
            { // 반복 시작
                transforms[index] = // 순서 값 설정
                    bodySegments[index] == null // 몸통·순서 처리
                        ? null // 대상 처리
                        : bodySegments[index].transform; // 몸통·순서 처리
            } // 반복 종료

            Transform tailTransform = // 위치 값 설정
                tailSegment == null // 몸통 처리
                    ? null // 대상 처리
                    : tailSegment.transform; // 몸통·위치 처리

            bodyFollower.Bind( // 몸통·연결 호출 시작
                pathRecorder, // 경로 항목 전달
                transforms, // 대상 항목 전달
                tailTransform, // 위치 항목 전달
                segmentSpacing // 몸통 처리
            ); // 앞선 항목 종료
        } // 함수 종료

        private void EnsureInitialized() // 확인·초기화 기능 선언
        { // 함수 시작
            maximumBodyCount = Mathf.Clamp(maximumBodyCount, 1, 20); // 직렬화 상한 제한
            startingBodyCount = Mathf.Clamp(startingBodyCount, 0, maximumBodyCount); // 시작 몸통 범위 제한

            if (initialized) // 초기화 조건 확인
            { // 조건 시작
                return; // 현재 기능 종료
            } // 조건 종료

            initialized = true; // 초기화 값 설정
            bodySegments.Clear(); // 몸통 기능 실행
            tailSegment = null; // 몸통 값 설정

            for (int childIndex = 0; // 자식·순서 반복 처리
                childIndex < transform.childCount; // 자식·순서 처리
                childIndex++) // 자식·순서 처리
            { // 반복 시작
                Transform child = // 위치·자식 값 설정
                    transform.GetChild( // 위치·조회 호출 시작
                        childIndex // 자식·순서 처리
                    ); // 앞선 항목 종료

                if (child == null) // 자식 조건 확인
                { // 조건 시작
                    continue; // 다음 반복 진행
                } // 조건 종료

                if (child.name.StartsWith( // 자식·이름 조건 확인
                    "Body_", // 몸통 경로 또는 문구 값
                    StringComparison.Ordinal // 대상 처리
                )) // 대상 처리
                { // 조건 시작
                    SnakeSegment body = // 몸통 값 설정
                        GetOrAddSegment( // 조회·추가 호출 시작
                            child.gameObject // 자식·게임 처리
                        ); // 앞선 항목 종료

                    bodySegments.Add(body); // 몸통·추가 기능 실행
                    continue; // 다음 반복 진행
                } // 조건 종료

                if (child.name == "Tail") // 자식·이름 조건 확인
                { // 조건 시작
                    tailSegment = // 몸통 값 설정
                        GetOrAddSegment( // 조회·추가 호출 시작
                            child.gameObject // 자식·게임 처리
                        ); // 앞선 항목 종료
                } // 조건 종료
            } // 반복 종료

            bodySegments.Sort( // 몸통 호출 시작
                (left, right) => // 몸통의 자식 순서 비교
                    left.transform // 위치 처리
                        .GetSiblingIndex() // 조회·순서 기능 실행
                        .CompareTo( // 대상 호출 시작
                            right.transform // 위치 처리
                                .GetSiblingIndex() // 조회·순서 기능 실행
                        ) // 앞선 항목 종료
            ); // 앞선 항목 종료

            ReindexBodySegments(); // 몸통 기능 실행

            if (tailSegment != null) // 몸통 조건 확인
            { // 조건 시작
                tailSegment.Configure( // 몸통·구성 호출 시작
                    SnakeSegmentType.Tail, // 몸통·형식 항목 전달
                    -1 // 대상 처리
                ); // 앞선 항목 종료
            } // 조건 종료

            NormalizeEarnedBodyCount(); // 몸통·개수 기능 실행
        } // 함수 종료

        private void EnsureStartingBodyCount() // 확인·몸통 기능 선언
        { // 함수 시작
            TrimToMaximumBodyCount(); // 최대·몸통 기능 실행

            while (bodySegments.Count < // 몸통·개수 반복 조건
                startingBodyCount) // 몸통·개수 처리
            { // 반복 시작
                CreateBodySegment(); // 생성·몸통 기능 실행
            } // 반복 종료

            NormalizeEarnedBodyCount(); // 몸통·개수 기능 실행
        } // 함수 종료

        private void TrimToMaximumBodyCount() // 최대·몸통 기능 선언
        { // 함수 시작
            while (bodySegments.Count > // 몸통·개수 반복 조건
                maximumBodyCount) // 최대·몸통 처리
            { // 반복 시작
                int lastIndex = // 순서 값 설정
                    bodySegments.Count - 1; // 몸통·개수 처리

                SnakeSegment segment = // 몸통 값 설정
                    bodySegments[lastIndex]; // 몸통·순서 처리

                bodySegments.RemoveAt( // 몸통·제거 호출 시작
                    lastIndex // 순서 처리
                ); // 앞선 항목 종료

                if (segment != null) // 몸통 조건 확인
                { // 조건 시작
                    DestroySegmentObject( // 제거·몸통 호출 시작
                        segment.gameObject // 몸통·게임 처리
                    ); // 앞선 항목 종료
                } // 조건 종료
            } // 반복 종료

            ReindexBodySegments(); // 몸통 기능 실행
            NormalizeEarnedBodyCount(); // 몸통·개수 기능 실행
        } // 함수 종료

        private void NormalizeEarnedBodyCount() // 몸통·개수 기능 선언
        { // 함수 시작
            earnedBodyCount = Mathf.Clamp( // 몸통·개수 호출 시작
                Mathf.Max( // 최대 호출 시작
                    earnedBodyCount, // 몸통·개수 항목 전달
                    bodySegments.Count, // 몸통·개수 항목 전달
                    startingBodyCount // 몸통·개수 처리
                ), // 대상 항목 전달
                0, // 대상 항목 전달
                maximumBodyCount // 최대·몸통 처리
            ); // 앞선 항목 종료
        } // 함수 종료

        private SnakeSegment CreateBodySegment() // 몸통·생성 기능 선언
        { // 함수 시작
            int index = // 순서 값 설정
                bodySegments.Count; // 몸통·개수 처리

            GameObject segmentObject = // 게임·객체 값 설정
                new GameObject( // 게임·객체 객체 생성
                    $"Body_{index + 1:00}" // 몸통·순서 처리
                ); // 앞선 항목 종료

            segmentObject.transform.SetParent( // 몸통·객체 호출 시작
                transform, // 위치 항목 전달
                false // 대상 처리
            ); // 앞선 항목 종료

            segmentObject.transform.localScale = // 몸통·객체 값 설정
                bodyScale; // 몸통·크기 처리

            SpriteRenderer renderer = // 이미지·렌더러 값 설정
                segmentObject.AddComponent<SpriteRenderer>(); // 몸통·객체 기능 실행

            renderer.sprite = // 렌더러·이미지 값 설정
                bodySprite; // 몸통·이미지 처리

            renderer.color = // 렌더러·색상 값 설정
                bodyColor; // 몸통·색상 처리

            renderer.sortingOrder = // 렌더러 값 설정
                6 - index; // 순서 처리

            SnakeSegment segment = // 몸통 값 설정
                segmentObject.AddComponent<SnakeSegment>(); // 몸통·객체 기능 실행

            segment.Configure( // 몸통·구성 호출 시작
                SnakeSegmentType.Body, // 몸통·형식 항목 전달
                index // 순서 처리
            ); // 앞선 항목 종료

            EnsureBodyCollider( // 확인·몸통 호출 시작
                segmentObject // 몸통·객체 처리
            ); // 앞선 항목 종료

            bodySegments.Add(segment); // 몸통·추가 기능 실행

            return segment; // 몸통 결과 반환
        } // 함수 종료

        private void EnsureTail() // 확인 기능 선언
        { // 함수 시작
            if (tailSegment == null) // 몸통 조건 확인
            { // 조건 시작
                GameObject tailObject = // 게임·객체 값 설정
                    new GameObject("Tail"); // 게임·객체 객체 생성

                tailObject.transform.SetParent( // 객체·위치 호출 시작
                    transform, // 위치 항목 전달
                    false // 대상 처리
                ); // 앞선 항목 종료

                tailObject.transform.localScale = // 객체·위치 값 설정
                    tailScale; // 크기 처리

                SpriteRenderer renderer = // 이미지·렌더러 값 설정
                    tailObject.AddComponent<SpriteRenderer>(); // 객체·추가 기능 실행

                renderer.sprite = tailSprite != null ? tailSprite : bodySprite; // 꼬리 이미지와 기존 대체 이미지

                renderer.color = // 렌더러·색상 값 설정
                    tailColor; // 색상 처리

                renderer.sortingOrder = // 렌더러 값 설정
                    -20; // 대상 처리

                tailSegment = // 몸통 값 설정
                    tailObject.AddComponent<SnakeSegment>(); // 객체·추가 기능 실행
            } // 조건 종료

            tailSegment.name = // 몸통·이름 값 설정
                "Tail"; // 대상 경로 또는 문구 값

            tailSegment.Configure( // 몸통·구성 호출 시작
                SnakeSegmentType.Tail, // 몸통·형식 항목 전달
                -1 // 대상 처리
            ); // 앞선 항목 종료

            SpriteRenderer tailRenderer = // 이미지·렌더러 값 설정
                tailSegment.GetComponent<SpriteRenderer>(); // 몸통·조회 기능 실행

            if (tailRenderer != null && (tailSprite != null || bodySprite != null)) // 실제 꼬리 표시와 이미지 확인
            { // 조건 시작
                tailRenderer.sprite = tailSprite != null ? tailSprite : bodySprite; // 별도 꼬리 이미지 유지
            } // 조건 종료

            tailSegment.transform.localScale = // 몸통·위치 값 설정
                tailScale; // 크기 처리
        } // 함수 종료

        private void ReindexBodySegments() // 몸통 기능 선언
        { // 함수 시작
            for (int index = 0; // 순서 반복 처리
                index < bodySegments.Count; // 순서·몸통 처리
                index++) // 순서 처리
            { // 반복 시작
                SnakeSegment segment = // 몸통 값 설정
                    bodySegments[index]; // 몸통·순서 처리

                if (segment == null) // 몸통 조건 확인
                { // 조건 시작
                    continue; // 다음 반복 진행
                } // 조건 종료

                segment.name = // 몸통·이름 값 설정
                    $"Body_{index + 1:00}"; // 몸통·순서 처리

                segment.Configure( // 몸통·구성 호출 시작
                    SnakeSegmentType.Body, // 몸통·형식 항목 전달
                    index // 순서 처리
                ); // 앞선 항목 종료

                SpriteRenderer renderer = // 이미지·렌더러 값 설정
                    segment.GetComponent<SpriteRenderer>(); // 몸통·조회 기능 실행

                if (renderer != null) // 렌더러 조건 확인
                { // 조건 시작
                    if (bodySprite != null) // 몸통·이미지 조건 확인
                    { // 조건 시작
                        renderer.sprite = // 렌더러·이미지 값 설정
                            bodySprite; // 몸통·이미지 처리
                    } // 조건 종료

                    renderer.color = // 렌더러·색상 값 설정
                        bodyColor; // 몸통·색상 처리

                    renderer.sortingOrder = // 렌더러 값 설정
                        6 - index; // 순서 처리
                } // 조건 종료

                segment.transform.localScale = // 몸통·위치 값 설정
                    bodyScale; // 몸통·크기 처리

                EnsureBodyCollider( // 확인·몸통 호출 시작
                    segment.gameObject // 몸통·게임 처리
                ); // 앞선 항목 종료
            } // 반복 종료
        } // 함수 종료

        private void EnsureBodyCollider( // 확인·몸통 호출 시작
            GameObject target // 게임·객체 처리
        ) // 앞선 항목 종료
        { // 함수 시작
            CircleCollider2D collider = // 충돌체 값 설정
                target.GetComponent<CircleCollider2D>(); // 대상·조회 기능 실행

            if (collider == null) // 충돌체 조건 확인
            { // 조건 시작
                collider = // 충돌체 값 설정
                    target.AddComponent<CircleCollider2D>(); // 대상·추가 기능 실행
            } // 조건 종료

            collider.isTrigger = // 충돌체·접촉 값 설정
                true; // 대상 처리

            collider.radius = // 충돌체·반경 값 설정
                Mathf.Max( // 최대 호출 시작
                    0.05f, // 대상 항목 전달
                    bodyColliderRadius // 몸통·충돌체 처리
                ); // 앞선 항목 종료
        } // 함수 종료

        private static SnakeSegment GetOrAddSegment( // 몸통·조회 호출 시작
            GameObject target // 게임·객체 처리
        ) // 앞선 항목 종료
        { // 함수 시작
            SnakeSegment segment = // 몸통 값 설정
                target.GetComponent<SnakeSegment>(); // 대상·조회 기능 실행

            if (segment == null) // 몸통 조건 확인
            { // 조건 시작
                segment = // 몸통 값 설정
                    target.AddComponent<SnakeSegment>(); // 대상·추가 기능 실행
            } // 조건 종료

            return segment; // 몸통 결과 반환
        } // 함수 종료

        private void NotifyBodyCountChanged() // 알림·몸통 기능 선언
        { // 함수 시작
            BodyCountChanged?.Invoke( // 몸통·개수 호출 시작
                bodySegments.Count, // 몸통·개수 항목 전달
                maximumBodyCount // 최대·몸통 처리
            ); // 앞선 항목 종료
        } // 함수 종료

        private static void DestroySegmentObject( // 제거·몸통 호출 시작
            GameObject target // 게임·객체 처리
        ) // 앞선 항목 종료
        { // 함수 시작
            if (target == null) // 대상 조건 확인
            { // 조건 시작
                return; // 현재 기능 종료
            } // 조건 종료

            if (Application.isPlaying) // 대상 조건 확인
            { // 조건 시작
                target.SetActive(false); // 제거 예정 몸통의 표시와 충돌 즉시 차단
                Destroy(target); // 실행 중 대상 파괴 예약
            } // 조건 종료
            else // 다른 조건 처리
            { // 조건 시작
                DestroyImmediate(target); // 편집 중 대상 즉시 제거
            } // 조건 종료
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
