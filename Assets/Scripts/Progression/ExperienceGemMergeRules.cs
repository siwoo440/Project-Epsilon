using System; // 수학과 비교 도구
using System.Collections.Generic; // 후보 목록

namespace ProjectEpsilon.Progression // 진행 영역
{ // 영역 시작
    public readonly struct ExperienceMergePoint // 보석 병합 후보 정보
    { // 구조체 시작
        public int Id { get; } // 보석 고유 식별자
        public int Value { get; } // 보석 경험치
        public float X { get; } // 보석 가로 위치
        public float Y { get; } // 보석 세로 위치
        public float DistanceSquared { get; } // 머리와 거리 제곱
        public bool Offscreen { get; } // 화면 밖 여부
        public bool Available { get; } // 병합 가능 여부
        public object Receiver { get; } // 경험치 수신자 식별
        public ExperienceMergePoint(int id, int value, float x, float y, float distanceSquared, bool offscreen, bool available, object receiver) // 후보 정보 구성
        { // 생성자 시작
            Id = id; // 고유 식별자 저장
            Value = value; // 경험치 저장
            X = x; // 가로 위치 저장
            Y = y; // 세로 위치 저장
            DistanceSquared = distanceSquared; // 머리 거리 저장
            Offscreen = offscreen; // 화면 상태 저장
            Available = available; // 병합 가능 상태 저장
            Receiver = receiver; // 수신자 저장
        } // 생성자 끝
    } // 구조체 끝

    public readonly struct ExperienceMergeGroup // 경험치 보존 병합 그룹
    { // 구조체 시작
        public int[] Indices { get; } // 기준 및 소모 보석 위치
        public int ResultValue { get; } // 결과 경험치
        public ExperienceMergeGroup(int[] indices, int resultValue) // 병합 그룹 구성
        { // 생성자 시작
            Indices = indices; // 대상 위치 저장
            ResultValue = resultValue; // 결과 경험치 저장
        } // 생성자 끝
    } // 구조체 끝

    public sealed class ExperienceMergeScanState // 검사 간 부분 그룹 상태
    { // 클래스 시작
        internal readonly List<int> PendingAnchors = new List<int>(); // 현재 순회할 기준 보석 번호
        internal int NextAnchor; // 다음 기준 보석 순서
        internal bool HasAnchor; // 기준 보석 보관 여부
        internal int AnchorId; // 기준 보석 식별자
        internal readonly List<int> PendingNeighbors = new List<int>(); // 기준 선택 시 고정한 이웃 번호
        internal int NextNeighbor; // 다음 인접 보석 순서
        internal readonly List<int> Members = new List<int>(); // 선택한 보석 식별자
        internal void ClearAnchor() // 부분 그룹 초기화
        { // 메서드 시작
            HasAnchor = false; // 기준 보석 해제
            PendingNeighbors.Clear(); // 인접 후보 목록 해제
            NextNeighbor = 0; // 인접 검사 위치 초기화
            Members.Clear(); // 선택 보석 해제
        } // 메서드 끝
        public void Reset() // 전체 검사 상태 초기화
        { // 메서드 시작
            PendingAnchors.Clear(); // 기준 순회 목록 초기화
            NextAnchor = 0; // 기준 순서 초기화
            ClearAnchor(); // 부분 그룹 초기화
        } // 메서드 끝
    } // 클래스 끝

    public static class ExperienceGemMergeRules // 보석 병합 선택 규칙
    { // 클래스 시작
        public const int MinimumNeighborChecks = 8; // 부분 그룹 재검증을 포함한 최소 검사 예산
        public static List<ExperienceMergeGroup> Plan(IReadOnlyList<ExperienceMergePoint> points, int threshold, float radius, int maxGroups, int maxAnchors, int maxNeighborChecks, ExperienceMergeScanState state, out int inspected, out int compared, out int anchorsInspected) // 병합 그룹 계획
        { // 메서드 시작
            anchorsInspected = 0; // 기준 보석 검사 횟수
            inspected = 0; // 인접 후보 검사 횟수
            compared = 0; // 실제 거리 계산 횟수
            List<ExperienceMergeGroup> groups = new List<ExperienceMergeGroup>(); // 결과 그룹 목록
            if (state == null || points == null || points.Count < Math.Max(1, threshold) || radius <= 0f || maxGroups <= 0 || maxAnchors <= 0 || maxNeighborChecks < MinimumNeighborChecks) // 검사 가능 상태 확인
            { // 조건 시작
                state?.Reset(); // 중단된 검사 초기화
                return groups; // 병합 없음
            } // 조건 끝
            List<int> priority = new List<int>(); // 기준 보석 우선순위
            List<int> neighbors = new List<int>(); // 고유 번호순 후보
            Dictionary<int, int> byId = new Dictionary<int, int>(); // 고유 번호와 위치
            for (int index = 0; index < points.Count; index++) // 모든 보석 순회
            { // 반복 시작
                ExperienceMergePoint point = points[index]; // 후보 정보 조회
                if (point.Available && point.Receiver != null && (point.Value == 1 || point.Value == 5) && !byId.ContainsKey(point.Id)) // 수신자·등급·고유 번호 확인
                { // 조건 시작
                    priority.Add(index); // 기준 후보 추가
                    neighbors.Add(index); // 인접 후보 추가
                    byId.Add(point.Id, index); // 고유 번호 등록
                } // 조건 끝
            } // 반복 끝
            if (priority.Count == 0) // 유효 후보 부재 확인
            { // 조건 시작
                state.Reset(); // 이전 검사 초기화
                return groups; // 병합 없음
            } // 조건 끝
            neighbors.Sort((left, right) => points[left].Id.CompareTo(points[right].Id)); // 안정적인 고유 번호순 정렬
            bool[] used = new bool[points.Count]; // 같은 검사 내 중복 병합 방지
            float radiusSquared = radius * radius; // 인접 거리 제곱
            int anchors = 0; // 검사한 기준 보석 수
            while (groups.Count < maxGroups && anchors < maxAnchors && inspected < maxNeighborChecks) // 처리 예산 내 반복
            { // 반복 시작
                if (!state.HasAnchor) // 기존 부분 그룹 부재 확인
                { // 조건 시작
                    if (state.NextAnchor >= state.PendingAnchors.Count) // 기존 기준 순회 완료 확인
                    { // 조건 시작
                        priority.Sort((left, right) => ComparePriority(points, left, right)); // 새 순회의 화면 밖과 원거리 우선
                        state.PendingAnchors.Clear(); // 이전 기준 목록 제거
                        foreach (int index in priority) // 현재 우선순위 순회
                        { // 반복 시작
                            state.PendingAnchors.Add(points[index].Id); // 안정적인 보석 번호 저장
                        } // 반복 끝
                        state.NextAnchor = 0; // 새 순회 시작 위치
                    } // 조건 끝
                    int anchorId = state.PendingAnchors[state.NextAnchor++]; // 순회 목록의 다음 기준 번호
                    anchors++; // 기준 보석 검사 횟수 증가
                    if (!byId.TryGetValue(anchorId, out int anchorIndex) || used[anchorIndex]) // 제거되거나 사용한 기준 확인
                    { // 조건 시작
                        continue; // 잘못된 기준 건너뛰기
                    } // 조건 끝
                    state.HasAnchor = true; // 부분 그룹 시작
                    state.AnchorId = anchorId; // 기준 식별자 저장
                    state.Members.Add(state.AnchorId); // 기준 보석 추가
                    foreach (int index in neighbors) // 현재 인접 후보 순회
                    { // 반복 시작
                        state.PendingNeighbors.Add(points[index].Id); // 안정적인 이웃 번호 저장
                    } // 반복 끝
                } // 조건 끝
                else // 기존 부분 그룹 확인
                { // 대안 시작
                    anchors++; // 재개한 기준 검사 횟수 증가
                } // 대안 끝
                if (!byId.TryGetValue(state.AnchorId, out int currentAnchor) || used[currentAnchor]) // 기준 보석 유효성 확인
                { // 조건 시작
                    state.ClearAnchor(); // 사라진 기준 초기화
                    continue; // 다음 기준 검사
                } // 조건 끝
                ExperienceMergePoint anchor = points[currentAnchor]; // 현재 기준 정보
                bool invalid = false; // 부분 그룹 무효 상태
                for (int index = 1; index < state.Members.Count; index++) // 이전에 선택한 재료 순회
                { // 반복 시작
                    inspected++; // 재검증 후보 횟수 증가
                    if (!byId.TryGetValue(state.Members[index], out int memberIndex) || used[memberIndex]) // 수집 및 비활성 확인
                    { // 조건 시작
                        invalid = true; // 부분 그룹 무효화
                        break; // 재검증 종료
                    } // 조건 끝
                    ExperienceMergePoint member = points[memberIndex]; // 기존 재료 정보
                    if (member.Value != anchor.Value || !ReferenceEquals(member.Receiver, anchor.Receiver)) // 등급 및 수신자 확인
                    { // 조건 시작
                        invalid = true; // 부분 그룹 무효화
                        break; // 재검증 종료
                    } // 조건 끝
                    compared++; // 실제 거리 계산 횟수 증가
                    float dx = member.X - anchor.X; // 재료 가로 거리
                    float dy = member.Y - anchor.Y; // 재료 세로 거리
                    if (dx * dx + dy * dy > radiusSquared) // 재료 이동 여부 확인
                    { // 조건 시작
                        invalid = true; // 부분 그룹 무효화
                        break; // 재검증 종료
                    } // 조건 끝
                } // 반복 끝
                if (invalid) // 기존 재료 무효 확인
                { // 조건 시작
                    state.ClearAnchor(); // 부분 그룹 초기화
                    continue; // 다음 기준 검사
                } // 조건 끝
                int required = anchor.Value == 1 ? 5 : 4; // 임시 등급별 필요 개수
                while (state.NextNeighbor < state.PendingNeighbors.Count && inspected < maxNeighborChecks && state.Members.Count < required) // 인접 후보 예산 내 탐색
                { // 반복 시작
                    int candidateId = state.PendingNeighbors[state.NextNeighbor++]; // 다음 이웃 식별자
                    inspected++; // 인접 후보 검사 횟수 증가
                    if (!byId.TryGetValue(candidateId, out int candidateIndex)) // 제거된 후보 확인
                    { // 조건 시작
                        continue; // 사라진 후보 제외
                    } // 조건 끝
                    ExperienceMergePoint candidate = points[candidateIndex]; // 후보 정보
                    if (used[candidateIndex] || state.Members.Contains(candidate.Id) || candidate.Value != anchor.Value || !ReferenceEquals(candidate.Receiver, anchor.Receiver)) // 중복·등급·수신자 확인
                    { // 조건 시작
                        continue; // 호환되지 않는 후보 제외
                    } // 조건 끝
                    compared++; // 실제 거리 계산 횟수 증가
                    float dx = candidate.X - anchor.X; // 후보 가로 거리
                    float dy = candidate.Y - anchor.Y; // 후보 세로 거리
                    if (dx * dx + dy * dy <= radiusSquared) // 인접 거리 확인
                    { // 조건 시작
                        state.Members.Add(candidate.Id); // 인접 보석 보관
                    } // 조건 끝
                } // 반복 끝
                if (state.Members.Count == required) // 완성된 병합 그룹 확인
                { // 조건 시작
                    int[] indices = new int[required]; // 실제 보석 위치 목록
                    for (int index = 0; index < required; index++) // 그룹 보석 순회
                    { // 반복 시작
                        indices[index] = byId[state.Members[index]]; // 현재 배열 위치 조회
                        used[indices[index]] = true; // 동일 검사 내 재사용 방지
                    } // 반복 끝
                    groups.Add(new ExperienceMergeGroup(indices, anchor.Value == 1 ? 5 : 20)); // 경험치 보존 결과 추가
                    state.ClearAnchor(); // 다음 그룹 준비
                    continue; // 남은 예산 처리
                } // 조건 끝
                if (state.NextNeighbor >= state.PendingNeighbors.Count) // 고정 후보 검색 완료 확인
                { // 조건 시작
                    state.ClearAnchor(); // 부족한 그룹 종료
                    continue; // 다음 기준 검사
                } // 조건 끝
                break; // 예산 소진 시 부분 그룹 보존
            } // 반복 끝
            anchorsInspected = anchors; // 실제 기준 검사 수 기록
            return groups; // 계획된 그룹 반환
        } // 메서드 끝

        private static int ComparePriority(IReadOnlyList<ExperienceMergePoint> points, int left, int right) // 병합 우선순위 비교
        { // 메서드 시작
            ExperienceMergePoint a = points[left]; // 첫 후보 정보
            ExperienceMergePoint b = points[right]; // 둘째 후보 정보
            if (a.Offscreen != b.Offscreen) // 화면 밖 우선 확인
            { // 조건 시작
                return a.Offscreen ? -1 : 1; // 화면 밖 보석 우선
            } // 조건 끝
            int distance = b.DistanceSquared.CompareTo(a.DistanceSquared); // 먼 보석 우선 비교
            return distance != 0 ? distance : a.Id.CompareTo(b.Id); // 동일 거리의 안정적 순서
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
