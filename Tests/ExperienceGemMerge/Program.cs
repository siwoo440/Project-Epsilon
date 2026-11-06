using System; // 기본 형식
using System.Collections.Generic; // 후보 목록
using ProjectEpsilon.Progression; // 병합 규칙

internal static class Program // 경험치 병합 규칙 시험
{ // 클래스 시작
    private static int failures; // 실패 횟수
    private static int nextId = 1; // 시험용 고유 번호

    private static int Main(string[] args) // 시험 시작
    { // 메서드 시작
        if (args.Length > 0 && args[0] == "benchmark") // 성능 측정 요청
        { // 조건 시작
            Benchmark(); // 후보 수별 측정
            return 0; // 측정 종료
        } // 조건 끝
        Check("소형 다섯 개와 중형 네 개", VerifyTierConservation); // 경험치 보존 검사
        Check("임계값 및 거리 경계", VerifyThresholdAndDistance); // 발동 조건 검사
        Check("화면 밖과 원거리 우선", VerifyPriority); // 우선순위 검사
        Check("수신자와 흡수 중 보석 제외", VerifyEligibility); // 후보 안전 검사
        Check("처리량과 회전 검사", VerifyBudgetAndFairness); // 제한과 공정성 검사
        Check("그룹 내 중복 방지", VerifyNoDuplicate); // 중복 사용 검사
        Check("거리 비교 예산", VerifyComparisonBudget); // 연산 한도 검사
        Check("흩어진 재료의 작은 예산 진행", VerifySparseProgress); // 부분 그룹 보존 검사
        Check("부분 그룹 재검증", VerifyPartialInvalidation); // 수집 및 비활성 검사
        Check("우선 후보 동적 삽입 기아 방지", VerifyDynamicInsertion); // 동적 정렬 검사
        Check("인접 후보 동적 삽입 기아 방지", VerifyDynamicNeighbors); // 동적 이웃 검사
        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL: {failures}"); // 최종 결과
        return failures == 0 ? 0 : 1; // 종료 코드
    } // 메서드 끝

    private static void Check(string name, Action action) // 개별 시험 실행
    { // 메서드 시작
        try // 시험 시도
        { // 시도 시작
            action(); // 시험 호출
            Console.WriteLine($"PASS: {name}"); // 성공 기록
        } // 시도 끝
        catch (Exception error) // 실패 처리
        { // 예외 시작
            failures++; // 실패 횟수 증가
            Console.WriteLine($"FAIL: {name}: {error.GetBaseException().Message}"); // 실패 기록
        } // 예외 끝
    } // 메서드 끝

    private static List<ExperienceMergePoint> Cluster(int count, int value, float x, bool offscreen, object receiver, bool available = true) // 인접 보석 구성
    { // 메서드 시작
        List<ExperienceMergePoint> result = new List<ExperienceMergePoint>(); // 후보 목록
        for (int index = 0; index < count; index++) // 보석 수만큼 순회
        { // 반복 시작
            result.Add(new ExperienceMergePoint(nextId++, value, x + index * 0.1f, 0f, x * x, offscreen, available, receiver)); // 후보 추가
        } // 반복 끝
        return result; // 후보 목록 반환
    } // 메서드 끝

    private static void VerifyTierConservation() // 소형 및 중형 합산
    { // 메서드 시작
        object receiver = new object(); // 공통 수신자
        List<ExperienceMergePoint> points = Cluster(5, 1, 20f, true, receiver); // 소형 보석 다섯 개
        points.AddRange(Cluster(4, 5, 30f, true, receiver)); // 중형 보석 네 개
        ExperienceMergeScanState scan = new ExperienceMergeScanState(); // 검사 상태
        List<ExperienceMergeGroup> groups = ExperienceGemMergeRules.Plan(points, 1, 1f, 2, 20, 1000, scan, out _, out _, out _); // 병합 계획
        Require(groups.Count == 2, "두 등급 병합 누락"); // 그룹 수 확인
        foreach (ExperienceMergeGroup group in groups) // 각 병합 그룹 검사
        { // 반복 시작
            int total = 0; // 원래 경험치 합
            foreach (int index in group.Indices) // 그룹 보석 순회
            { // 반복 시작
                total += points[index].Value; // 경험치 누적
            } // 반복 끝
            Require(total == group.ResultValue && (total == 5 || total == 20), "병합 전후 경험치 불일치"); // 정확한 가치 확인
        } // 반복 끝
    } // 메서드 끝

    private static void VerifyThresholdAndDistance() // 발동 조건과 거리
    { // 메서드 시작
        object receiver = new object(); // 공통 수신자
        List<ExperienceMergePoint> points = Cluster(5, 1, -10f, true, receiver); // 인접 소형 보석
        ExperienceMergeScanState scan = new ExperienceMergeScanState(); // 검사 상태
        Require(ExperienceGemMergeRules.Plan(points, 6, 1f, 1, 10, 1000, scan, out _, out _, out _).Count == 0, "임계값 전 병합"); // 발동 개수 확인
        Require(ExperienceGemMergeRules.Plan(points, 5, 0.19f, 1, 10, 1000, scan, out _, out _, out _).Count == 0, "인접 거리 밖 병합"); // 거리 제한 확인
        Require(ExperienceGemMergeRules.Plan(points, 5, 1f, 1, 10, 1000, scan, out _, out _, out _).Count == 1, "조건 충족 병합 누락"); // 임계 경계 확인
    } // 메서드 끝

    private static void VerifyPriority() // 화면 밖과 원거리 우선
    { // 메서드 시작
        object receiver = new object(); // 공통 수신자
        List<ExperienceMergePoint> points = Cluster(5, 1, 100f, false, receiver); // 화면 안 먼 그룹
        points.AddRange(Cluster(5, 1, 10f, true, receiver)); // 화면 밖 가까운 그룹
        ExperienceMergeScanState scan = new ExperienceMergeScanState(); // 검사 상태
        List<ExperienceMergeGroup> groups = ExperienceGemMergeRules.Plan(points, 1, 1f, 1, 20, 1000, scan, out _, out _, out _); // 첫 병합 그룹
        Require(groups.Count == 1 && groups[0].Indices[0] >= 5, "화면 밖 그룹 우선 실패"); // 화면 밖 우선 확인
        points = Cluster(5, 1, 10f, true, receiver); // 가까운 화면 밖 그룹
        points.AddRange(Cluster(5, 1, 100f, true, receiver)); // 먼 화면 밖 그룹
        scan.Reset(); // 검사 상태 초기화
        groups = ExperienceGemMergeRules.Plan(points, 1, 1f, 1, 20, 1000, scan, out _, out _, out _); // 첫 병합 그룹
        Require(groups.Count == 1 && groups[0].Indices[0] >= 5, "먼 그룹 우선 실패"); // 원거리 우선 확인
    } // 메서드 끝

    private static void VerifyEligibility() // 수신자와 획득 경합 방지
    { // 메서드 시작
        object receiver = new object(); // 첫 수신자
        object other = new object(); // 다른 수신자
        List<ExperienceMergePoint> points = Cluster(3, 1, 10f, true, receiver); // 첫 수신자 보석
        points.AddRange(Cluster(2, 1, 10f, true, other)); // 다른 수신자 보석
        points.AddRange(Cluster(2, 1, 10f, true, receiver, false)); // 흡수 및 소멸 예약 보석
        ExperienceMergeScanState scan = new ExperienceMergeScanState(); // 검사 상태
        Require(ExperienceGemMergeRules.Plan(points, 1, 1f, 2, 20, 1000, scan, out _, out _, out _).Count == 0, "다른 수신자 또는 흡수 중 보석 병합"); // 후보 격리 확인
        points.AddRange(Cluster(2, 1, 10f, true, receiver)); // 같은 수신자 보석 추가
        Require(ExperienceGemMergeRules.Plan(points, 1, 1f, 2, 20, 1000, scan, out _, out _, out _).Count == 1, "유효 보석 병합 누락"); // 유효 후보 확인
    } // 메서드 끝

    private static void VerifyBudgetAndFairness() // 처리 한도와 순환
    { // 메서드 시작
        object receiver = new object(); // 공통 수신자
        List<ExperienceMergePoint> points = Cluster(5, 1, 1f, false, receiver); // 낮은 우선순위 유효 그룹
        for (int index = 0; index < 8; index++) // 높은 우선순위 고립 후보 생성
        { // 반복 시작
            points.AddRange(Cluster(1, 1, 100f + index * 10f, true, receiver)); // 인접하지 않은 단일 보석
        } // 반복 끝
        ExperienceMergeScanState scan = new ExperienceMergeScanState(); // 검사 상태
        bool found = false; // 병합 발견 여부
        for (int pass = 0; pass < 8; pass++) // 여러 검사 순회
        { // 반복 시작
            List<ExperienceMergeGroup> groups = ExperienceGemMergeRules.Plan(points, 1, 1f, 1, 2, 1000, scan, out _, out _, out _); // 기준 보석 두 개만 검사
            Require(groups.Count <= 1, "검사당 병합 한도 초과"); // 병합 예산 확인
            found |= groups.Count == 1; // 유효 그룹 발견 기록
        } // 반복 끝
        Require(found, "고립된 원거리 보석으로 유효 그룹 영구 기아"); // 순환 공정성 확인
    } // 메서드 끝

    private static void VerifyNoDuplicate() // 한 검사 내 중복 방지
    { // 메서드 시작
        object receiver = new object(); // 공통 수신자
        List<ExperienceMergePoint> points = Cluster(10, 1, 10f, true, receiver); // 소형 보석 열 개
        ExperienceMergeScanState scan = new ExperienceMergeScanState(); // 검사 상태
        List<ExperienceMergeGroup> groups = ExperienceGemMergeRules.Plan(points, 1, 2f, 4, 20, 1000, scan, out _, out _, out _); // 여러 그룹 계획
        HashSet<int> used = new HashSet<int>(); // 사용 보석 추적
        foreach (ExperienceMergeGroup group in groups) // 각 그룹 순회
        { // 반복 시작
            foreach (int index in group.Indices) // 각 그룹 보석 순회
            { // 반복 시작
                Require(used.Add(index), "한 보석이 여러 그룹에 중복 사용"); // 중복 여부 확인
            } // 반복 끝
        } // 반복 끝
        Require(groups.Count == 2 && used.Count == 10, "열 개 보석 병합 결과 오류"); // 그룹 수 확인
    } // 메서드 끝

    private static void VerifyComparisonBudget() // 인접 후보 및 거리 계산 제한
    { // 메서드 시작
        object receiver = new object(); // 공통 수신자
        List<ExperienceMergePoint> points = new List<ExperienceMergePoint>(); // 후보 목록
        for (int index = 0; index < 2000; index++) // 멀리 떨어진 보석 생성
        { // 반복 시작
            points.Add(new ExperienceMergePoint(nextId++, 1, index * 3f, 0f, index * 9f, true, true, receiver)); // 고립된 보석 추가
        } // 반복 끝
        ExperienceMergeScanState scan = new ExperienceMergeScanState(); // 검사 상태
        List<ExperienceMergeGroup> groups = ExperienceGemMergeRules.Plan(points, 1, 1f, 8, 100, 37, scan, out int inspected, out int compared, out _); // 제한된 병합 검사
        Require(groups.Count == 0 && inspected <= 37 && compared <= inspected, "후보 또는 거리 비교 예산 초과"); // 실제 연산 상한 확인
        Require(scan.NextAnchor != 0 || scan.HasAnchor, "예산 소진 후 순환 위치 미진행"); // 다음 검사 위치 확인
    } // 메서드 끝

    private static void VerifySparseProgress() // 흩어진 후보의 점진적 병합
    { // 메서드 시작
        object receiver = new object(); // 공통 수신자
        List<ExperienceMergePoint> points = new List<ExperienceMergePoint>(); // 후보 목록
        for (int id = 1; id <= 500; id++) // 고유 번호 순회
        { // 반복 시작
            bool member = id == 1 || id == 100 || id == 200 || id == 300 || id == 400; // 멀리 분산된 그룹 번호
            float x = member ? 10f : 1000f + id * 3f; // 그룹 또는 고립 위치
            points.Add(new ExperienceMergePoint(id, 1, x, 0f, member ? 100000000f : 1f, member, true, receiver)); // 우선 그룹과 고립 후보
        } // 반복 끝
        ExperienceMergeScanState scan = new ExperienceMergeScanState(); // 검사 상태
        bool merged = false; // 그룹 완성 여부
        for (int pass = 0; pass < 200; pass++) // 예산 제한 검사 반복
        { // 반복 시작
            List<ExperienceMergeGroup> groups = ExperienceGemMergeRules.Plan(points, 1, 0.5f, 1, 1, 8, scan, out int inspected, out int compared, out _); // 작은 검사 예산
            Require(inspected <= 8 && compared <= 8, "작은 검사 예산 초과"); // 실제 연산 상한 확인
            if (groups.Count == 1) // 그룹 완성 확인
            { // 조건 시작
                merged = true; // 병합 진행 기록
                break; // 반복 종료
            } // 조건 끝
        } // 반복 끝
        Require(merged, "흩어진 재료의 부분 그룹 진행 실패"); // 안정 후보의 진행 보장 확인
    } // 메서드 끝

    private static void VerifyPartialInvalidation() // 검사 사이 재료 소멸
    { // 메서드 시작
        object receiver = new object(); // 공통 수신자
        List<ExperienceMergePoint> points = Cluster(4, 1, 10f, true, receiver); // 일부 유효 보석
        for (int index = 0; index < 30; index++) // 추가 고립 보석
        { // 반복 시작
            points.Add(new ExperienceMergePoint(nextId++, 1, 100f + index * 3f, 0f, 1f, false, true, receiver)); // 인접하지 않은 보석
        } // 반복 끝
        ExperienceMergeScanState scan = new ExperienceMergeScanState(); // 검사 상태
        ExperienceGemMergeRules.Plan(points, 1, 1f, 1, 1, 8, scan, out _, out _, out _); // 부분 그룹 탐색
        int selected = scan.Members.Count > 1 ? scan.Members[1] : -1; // 검사 중 확보한 재료
        Require(selected >= 0, "부분 재료 준비 실패"); // 부분 그룹 형성 확인
        int position = points.FindIndex(point => point.Id == selected); // 해당 후보 위치
        ExperienceMergePoint old = points[position]; // 기존 후보 정보
        points[position] = new ExperienceMergePoint(old.Id, old.Value, old.X, old.Y, old.DistanceSquared, old.Offscreen, false, old.Receiver); // 수집 또는 비활성 처리
        for (int pass = 0; pass < 20; pass++) // 후속 검사 순회
        { // 반복 시작
            List<ExperienceMergeGroup> groups = ExperienceGemMergeRules.Plan(points, 1, 1f, 1, 1, 8, scan, out int inspected, out int compared, out _); // 부분 재료 재검증
            Require(groups.Count == 0 && inspected <= 8 && compared <= 8, "사라진 재료로 병합 또는 검사량 초과"); // 무효 재료 보호 확인
        } // 반복 끝
    } // 메서드 끝

    private static void VerifyDynamicInsertion() // 앞쪽 우선 후보가 계속 생기는 경우
    { // 메서드 시작
        object receiver = new object(); // 공통 수신자
        List<ExperienceMergePoint> points = Cluster(5, 1, 10f, false, receiver); // 뒤쪽의 유효 그룹
        points.Add(new ExperienceMergePoint(nextId++, 1, 100f, 0f, 10000f, true, true, receiver)); // 고립된 우선 후보
        ExperienceMergeScanState scan = new ExperienceMergeScanState(); // 검사 상태
        bool found = false; // 유효 그룹 발견 여부
        for (int pass = 0; pass < 20; pass++) // 우선 후보가 계속 늘어나는 검사
        { // 반복 시작
            List<ExperienceMergeGroup> groups = ExperienceGemMergeRules.Plan(points, 1, 1f, 1, 1, 100, scan, out _, out _, out _); // 기준 보석 한 개 검사
            if (groups.Count > 0) // 유효 그룹 확인
            { // 조건 시작
                found = true; // 병합 기록
                break; // 검사 종료
            } // 조건 끝
            float x = 200f + pass * 10f; // 새 고립 후보 위치
            points.Add(new ExperienceMergePoint(nextId++, 1, x, 0f, x * x, true, true, receiver)); // 정렬 앞쪽에 새 후보 삽입
        } // 반복 끝
        Require(found, "새 우선 후보 삽입으로 기존 그룹 영구 기아"); // 안정적인 진행 보장
    } // 메서드 끝

    private static void VerifyDynamicNeighbors() // 인접 검사 중 고립 후보의 지속 삽입
    { // 메서드 시작
        object receiver = new object(); // 공통 수신자
        List<ExperienceMergePoint> points = new List<ExperienceMergePoint>(); // 후보 목록
        points.Add(new ExperienceMergePoint(1, 1, 10f, 0f, 1000000f, true, true, receiver)); // 최우선 기준 보석
        for (int id = 2; id < 102; id++) // 처음 고립 후보 생성
        { // 반복 시작
            points.Add(new ExperienceMergePoint(id, 1, 1000f + id * 3f, 0f, 1f, false, true, receiver)); // 그룹과 먼 후보
        } // 반복 끝
        foreach (int id in new[] { 100000, 200000, 300000, 400000 }) // 멀리 떨어진 고유 번호
        { // 반복 시작
            points.Add(new ExperienceMergePoint(id, 1, 10f, 0f, 1000000f, true, true, receiver)); // 기준과 인접한 재료
        } // 반복 끝
        ExperienceMergeScanState scan = new ExperienceMergeScanState(); // 검사 상태
        bool merged = false; // 병합 완료 여부
        int nextInserted = 102; // 새 후보 번호
        for (int pass = 0; pass < 50; pass++) // 작은 예산으로 반복 검사
        { // 반복 시작
            List<ExperienceMergeGroup> groups = ExperienceGemMergeRules.Plan(points, 1, 1f, 1, 1, 8, scan, out int inspected, out int compared, out _); // 기준 보석의 이웃 탐색
            Require(inspected <= 8 && compared <= 8, "동적 삽입 중 예산 초과"); // 검사 예산 확인
            if (groups.Count > 0) // 그룹 완성 확인
            { // 조건 시작
                merged = true; // 병합 기록
                break; // 검사 종료
            } // 조건 끝
            for (int index = 0; index < 10; index++) // 검사 예산보다 많은 후보 추가
            { // 반복 시작
                int id = nextInserted++; // 새로운 고유 번호
                points.Add(new ExperienceMergePoint(id, 1, 1000f + id * 3f, 0f, 1f, false, true, receiver)); // 기존 재료보다 앞선 고립 후보
            } // 반복 끝
        } // 반복 끝
        Require(merged, "새 이웃 후보 삽입으로 기존 재료 영구 기아"); // 기존 후보 진행 보장
    } // 메서드 끝

    private static void Benchmark() // 후보 수별 계획 시간 측정
    { // 메서드 시작
        foreach (int count in new[] { 1000, 10000, 100000 }) // 후보 수 순회
        { // 반복 시작
            object receiver = new object(); // 공통 수신자
            List<ExperienceMergePoint> points = new List<ExperienceMergePoint>(count); // 후보 목록
            for (int index = 0; index < count; index++) // 후보 생성
            { // 반복 시작
                points.Add(new ExperienceMergePoint(nextId++, 1, index * 3f, 0f, index * 9f, true, true, receiver)); // 서로 떨어진 보석
            } // 반복 끝
            ExperienceMergeScanState scan = new ExperienceMergeScanState(); // 검사 상태
            ExperienceGemMergeRules.Plan(points, 1, 1f, 8, 64, 4096, scan, out _, out _, out _); // 최초 실행 준비
            System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew(); // 측정 시작
            int inspected = 0; // 인접 검사 횟수
            int compared = 0; // 거리 계산 횟수
            for (int pass = 0; pass < 10; pass++) // 열 차례 계획
            { // 반복 시작
                ExperienceGemMergeRules.Plan(points, 1, 1f, 8, 64, 4096, scan, out inspected, out compared, out _); // 동일 조건 계획
            } // 반복 끝
            timer.Stop(); // 측정 종료
            Console.WriteLine($"고립 N={count}, 평균={timer.Elapsed.TotalMilliseconds / 10d:F3}ms, 후보검사={inspected}, 거리비교={compared}"); // 고립 보석 성능 출력
            List<ExperienceMergePoint> clustered = new List<ExperienceMergePoint>(count); // 밀집 보석 목록
            for (int index = 0; index < count; index++) // 밀집 보석 생성
            { // 반복 시작
                clustered.Add(new ExperienceMergePoint(nextId++, 1, 10f, 0f, 100f, true, true, receiver)); // 같은 위치 보석
            } // 반복 끝
            scan.Reset(); // 검사 상태 초기화
            ExperienceGemMergeRules.Plan(clustered, 1, 1f, 8, 64, 4096, scan, out _, out _, out _); // 밀집 검사 준비
            timer.Restart(); // 밀집 측정 시작
            for (int pass = 0; pass < 10; pass++) // 열 차례 계획
            { // 반복 시작
                ExperienceGemMergeRules.Plan(clustered, 1, 1f, 8, 64, 4096, scan, out inspected, out compared, out _); // 밀집 조건 계획
            } // 반복 끝
            timer.Stop(); // 밀집 측정 종료
            Console.WriteLine($"밀집 N={count}, 평균={timer.Elapsed.TotalMilliseconds / 10d:F3}ms, 후보검사={inspected}, 거리비교={compared}"); // 밀집 보석 성능 출력
        } // 반복 끝
    } // 메서드 끝

    private static void Require(bool condition, string message) // 조건 검사
    { // 메서드 시작
        if (!condition) // 실패 확인
        { // 조건 시작
            throw new InvalidOperationException(message); // 실패 전달
        } // 조건 끝
    } // 메서드 끝
} // 클래스 끝
