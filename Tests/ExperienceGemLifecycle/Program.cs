using System; // 기본 형식
using System.Reflection; // 내부 상태 설정
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Progression; // 실제 보석과 관리자
using UnityEngine; // Unity 대체

internal static class Program // 보석 병합 수명주기 시험
{ // 클래스 시작
    private static int failures; // 실패 횟수
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 메서드 범위

    private static int Main() // 시험 시작
    { // 메서드 시작
        Check("소형·중형 가치 보존과 지연 파괴", VerifyConservationAndDestroy); // 경험치 보존 검사
        Check("일시정지와 검사 간격", VerifyPauseAndInterval); // 게임 상태 검사
        Check("흡수·수집과 병합 경합", VerifyCollectionRace); // 수집 경합 검사
        Check("비활성·재활성과 수신자 격리", VerifyActiveAndReceiver); // 객체 수명 검사
        Check("화면 밖 우선과 처리량", VerifyPriorityAndBudget); // 병합 순서 검사
        Check("파괴 예약 보석 재설정 차단", VerifyPendingReconfiguration); // 지연 파괴 경계 검사
        Check("실제 거리 예산과 관리자 비활성", VerifyStrictBudget); // 연산 및 비활성 검사
        Check("부분 그룹의 수명주기 변경", VerifyPartialLifecycle); // 부분 그룹 재검증
        Check("정규 등급 외 경험치 보존", VerifyOtherValues); // 비정규 값 검사
        Check("씬 전환 부분 그룹 정리", VerifySceneChange); // 씬 전환 검사
        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL: {failures}"); // 최종 결과
        return failures == 0 ? 0 : 1; // 종료 코드
    } // 메서드 끝

    private static void Check(string name, Action action) // 개별 시험
    { // 메서드 시작
        try // 시험 실행
        { // 시도 시작
            action(); // 시험 호출
            Console.WriteLine($"PASS: {name}"); // 성공 기록
        } // 시도 끝
        catch (Exception error) // 실패 처리
        { // 예외 시작
            failures++; // 실패 횟수 증가
            Console.WriteLine($"FAIL: {name}: {error.GetBaseException().Message}"); // 실패 출력
        } // 예외 끝
    } // 메서드 끝

    private static (ExperienceGemMergeController manager, SnakeExperience receiver) Create(int threshold = 5, int groups = 4) // 시험 환경 구성
    { // 메서드 시작
        UnityEngine.Object.Pickups.Clear(); // 기존 보석 제거
        GameManager.Instance = new GameManager(); // 진행 중 게임 생성
        Camera.main = new Camera(); // 시험 카메라 생성
        SnakeExperience receiver = new SnakeExperience(); // 경험치 수신자 생성
        SnakeExperience.Current = receiver; // 현재 수신자 등록
        GameObject game = new GameObject("MergeManager"); // 병합 관리자 객체
        ExperienceGemMergeController manager = game.AddComponent<ExperienceGemMergeController>(); // 실제 병합 관리자
        Set(manager, "mergeThreshold", threshold); // 발동 개수 설정
        Set(manager, "mergeDistance", 1f); // 인접 거리 설정
        Set(manager, "mergeInterval", 1f); // 검사 간격 설정
        Set(manager, "maximumGroupsPerPass", groups); // 그룹 한도 설정
        Set(manager, "maximumAnchorsPerPass", 32); // 기준 검사 한도 설정
        Set(manager, "maximumNeighborChecksPerPass", 512); // 인접 검사 한도 설정
        return (manager, receiver); // 시험 객체 반환
    } // 메서드 끝

    private static void Set(object value, string name, object setting) // 직렬화 설정 주입
    { // 메서드 시작
        value.GetType().GetField(name, Private).SetValue(value, setting); // 실제 필드 설정
    } // 메서드 끝

    private static ExperiencePickup[] SpawnCluster(SnakeExperience receiver, int count, int value, float x) // 보석 묶음 생성
    { // 메서드 시작
        ExperiencePickup[] pickups = new ExperiencePickup[count]; // 생성 보석 목록
        for (int index = 0; index < count; index++) // 보석 생성 순회
        { // 반복 시작
            pickups[index] = ExperiencePickup.Spawn(new Vector3(x + index * 0.1f, 0f, 0f), value, new Sprite(), receiver); // 실제 생성 경로
        } // 반복 끝
        return pickups; // 생성 결과 반환
    } // 메서드 끝

    private static int AvailableValue() // 남은 보석 경험치 합
    { // 메서드 시작
        int total = 0; // 합계 초기화
        foreach (ExperiencePickup pickup in UnityEngine.Object.Pickups) // 전체 보석 순회
        { // 반복 시작
            if (pickup.IsMergeAvailable) // 활성 및 미소비 확인
            { // 조건 시작
                total += pickup.ExperienceValue; // 경험치 합산
            } // 조건 끝
        } // 반복 끝
        return total; // 남은 경험치 반환
    } // 메서드 끝

    private static void VerifyConservationAndDestroy() // 경험치와 지연 파괴 확인
    { // 메서드 시작
        var fixture = Create(5); // 다섯 개 임계값
        ExperiencePickup[] small = SpawnCluster(fixture.receiver, 5, 1, 10f); // 소형 다섯 개 생성
        Require(fixture.manager.ProcessMergePass() == 1, "소형 병합 누락"); // 첫 병합 확인
        Require(AvailableValue() == 5, "소형 병합 시 경험치 변화"); // 경험치 보존 확인
        ExperiencePickup result = Array.Find(small, pickup => pickup.IsMergeAvailable); // 병합 결과 조회
        Require(result != null && result.gameObject.name == "XP_Gem_5" && Math.Abs(result.transform.localScale.x - 0.26f) < 0.0001f, "중형 보석 이름 또는 크기 오류"); // 결과 표현 확인
        Require(fixture.manager.ProcessMergePass() == 0 && AvailableValue() == 5, "파괴 전 재료 중복 병합"); // 지연 파괴 중복 방지
        int pending = 0; // 제거 예약 수
        foreach (ExperiencePickup pickup in small) // 원래 보석 순회
        { // 반복 시작
            if (pickup.gameObject.PendingDestroy) // 제거 예약 확인
            { // 조건 시작
                pending++; // 예약 수 증가
            } // 조건 끝
        } // 반복 끝
        Require(pending == 4, "소형 병합 제거 수 오류"); // 재료 제거 확인
        SpawnCluster(fixture.receiver, 3, 5, 10f); // 추가 중형 세 개
        Set(fixture.manager, "mergeThreshold", 4); // 중형 발동 개수 설정
        Require(fixture.manager.ProcessMergePass() == 1 && AvailableValue() == 20, "중형 네 개 병합 또는 경험치 오류"); // 대형 결과 확인
    } // 메서드 끝

    private static void VerifySceneChange() // 씬 전환 시 대기 후보 초기화
    { // 메서드 시작
        var fixture = Create(); // 시험 환경 구성
        typeof(ExperienceGemMergeController).GetMethod("OnEnable", Private).Invoke(fixture.manager, null); // 씬 변경 감시 연결
        Set(fixture.manager, "maximumNeighborChecksPerPass", 17); // 작은 검사 예산
        SpawnCluster(fixture.receiver, 4, 1, 100f); // 부분 그룹 생성
        for (int index = 0; index < 30; index++) // 고립 보석 생성
        { // 반복 시작
            SpawnCluster(fixture.receiver, 1, 1, 10f + index * 3f); // 원거리 후보
        } // 반복 끝
        fixture.manager.ProcessMergePass(); // 부분 그룹 검사
        ExperienceMergeScanState scan = (ExperienceMergeScanState)typeof(ExperienceGemMergeController).GetField("scan", Private).GetValue(fixture.manager); // 검사 상태 조회
        Require(scan.HasAnchor, "씬 전환 전 부분 그룹 부재"); // 검사 상태 확인
        UnityEngine.SceneManagement.SceneManager.SimulateLoad(); // 새 씬 로드
        Require(!scan.HasAnchor && scan.PendingAnchors.Count == 0, "씬 전환 뒤 오래된 보석 상태 유지"); // 이전 씬 상태 정리 확인
        typeof(ExperienceGemMergeController).GetMethod("OnDisable", Private).Invoke(fixture.manager, null); // 씬 감시 해제
    } // 메서드 끝

    private static void VerifyOtherValues() // 정규 등급 외 보석 보호
    { // 메서드 시작
        var fixture = Create(); // 시험 환경 구성
        SpawnCluster(fixture.receiver, 4, 1, 10f); // 소형 네 개 생성
        ExperiencePickup[] unusual = SpawnCluster(fixture.receiver, 1, 2, 10f); // 비정규 경험치 보석 생성
        Require(fixture.manager.ProcessMergePass() == 0 && AvailableValue() == 6, "비정규 보석을 재료로 사용"); // 다른 값 제외 확인
        SpawnCluster(fixture.receiver, 1, 1, 10f); // 소형 한 개 추가
        Require(fixture.manager.ProcessMergePass() == 1 && AvailableValue() == 7 && unusual[0].IsMergeAvailable && unusual[0].ExperienceValue == 2, "혼합 경험치 손실 또는 비정규 보석 변경"); // 나머지 경험치 보존
    } // 메서드 끝

    private static void VerifyPartialLifecycle() // 부분 그룹 검사 중 재료 비활성
    { // 메서드 시작
        var fixture = Create(); // 시험 환경 구성
        Set(fixture.manager, "maximumAnchorsPerPass", 1); // 기준 보석 한 개만 검사
        Set(fixture.manager, "maximumNeighborChecksPerPass", 17); // 최소 거리 검사 예산
        ExperiencePickup[] partial = SpawnCluster(fixture.receiver, 4, 1, 100f); // 우선 그룹 일부 생성
        for (int index = 0; index < 30; index++) // 인접하지 않은 보석 생성
        { // 반복 시작
            SpawnCluster(fixture.receiver, 1, 1, 10f + index * 3f); // 다른 위치의 보석
        } // 반복 끝
        Require(fixture.manager.ProcessMergePass() == 0, "부족한 부분 그룹 병합"); // 첫 검사 보존 확인
        ExperienceMergeScanState scan = (ExperienceMergeScanState)typeof(ExperienceGemMergeController).GetField("scan", Private).GetValue(fixture.manager); // 부분 그룹 상태 조회
        Require(scan.HasAnchor && scan.Members.Count >= 2, "부분 그룹 저장 누락"); // 부분 탐색 상태 확인
        partial[1].gameObject.activeInHierarchy = false; // 부분 그룹 재료 비활성
        SpawnCluster(fixture.receiver, 2, 1, 100f); // 대체 재료 추가
        int total = AvailableValue(); // 현재 활성 경험치
        bool merged = false; // 병합 완료 여부
        for (int pass = 0; pass < 100; pass++) // 예산 제한 검사 반복
        { // 반복 시작
            merged |= fixture.manager.ProcessMergePass() > 0; // 병합 결과 기록
            Require(!partial[1].gameObject.PendingDestroy && AvailableValue() == total, "비활성 부분 재료 사용 또는 경험치 손실"); // 수명과 총량 확인
            Require(fixture.manager.LastBudgetedDistanceChecks <= 17, "부분 그룹 재검증 예산 초과"); // 거리 검사 상한 확인
            if (merged) // 병합 완료 확인
            { // 조건 시작
                break; // 검사 종료
            } // 조건 끝
        } // 반복 끝
        Require(merged, "재료 변경 뒤 유효 그룹 병합 누락"); // 재탐색 진행 확인
    } // 메서드 끝

    private static void VerifyStrictBudget() // 병합 전후 거리 연산과 비활성 관리
    { // 메서드 시작
        var fixture = Create(); // 시험 환경 구성
        SpawnCluster(fixture.receiver, 5, 1, 10f); // 인접 보석 생성
        Set(fixture.manager, "maximumNeighborChecksPerPass", 16); // 최소 예산 미달 설정
        Require(fixture.manager.ProcessMergePass() == 0 && fixture.manager.LastBudgetedDistanceChecks == 0, "최소 예산보다 많은 거리 검사"); // 잘못된 예산 차단
        Set(fixture.manager, "maximumNeighborChecksPerPass", 17); // 유효한 최소 예산 설정
        fixture.manager.isActiveAndEnabled = false; // 병합 관리자 비활성
        int passes = fixture.manager.CompletedPassCount; // 직전 검사 횟수
        Require(fixture.manager.ProcessMergePass() == 0 && fixture.manager.CompletedPassCount == passes, "비활성 관리자 병합 실행"); // 비활성 처리 차단
        fixture.manager.isActiveAndEnabled = true; // 병합 관리자 재활성
        bool merged = false; // 병합 완료 여부
        for (int pass = 0; pass < 10; pass++) // 예산 제한 검사 반복
        { // 반복 시작
            merged |= fixture.manager.ProcessMergePass() == 1; // 병합 여부 저장
            Require(fixture.manager.LastBudgetedDistanceChecks <= 17 && fixture.manager.LastNeighborInspections <= 8, "병합 확정 재검증을 포함한 예산 초과"); // 전체 예산 확인
            if (merged) // 병합 완료 확인
            { // 조건 시작
                break; // 반복 종료
            } // 조건 끝
        } // 반복 끝
        Require(merged && AvailableValue() == 5, "최소 예산에서 보존 병합 누락"); // 최소 예산 진행 확인
    } // 메서드 끝

    private static void VerifyPendingReconfiguration() // 파괴 예약 보석의 재설정
    { // 메서드 시작
        var fixture = Create(); // 시험 환경 구성
        ExperiencePickup[] small = SpawnCluster(fixture.receiver, 5, 1, 10f); // 다섯 보석 생성
        Require(fixture.manager.ProcessMergePass() == 1, "병합 준비 실패"); // 병합 완료 확인
        ExperiencePickup consumed = Array.Find(small, pickup => pickup.gameObject.PendingDestroy); // 소모된 보석 조회
        Require(consumed != null, "파괴 예약 재료 부재"); // 소모 보석 확인
        consumed.Configure(fixture.receiver, 1, new Sprite()); // 파괴 전 재설정 시도
        Require(!consumed.IsMergeAvailable && AvailableValue() == 5, "파괴 예약 보석 재활성으로 경험치 중복"); // 중복 사용 방지
    } // 메서드 끝

    private static void VerifyPauseAndInterval() // 정지 및 경과 시간
    { // 메서드 시작
        var fixture = Create(); // 시험 환경 구성
        SpawnCluster(fixture.receiver, 5, 1, 10f); // 인접 보석 생성
        GameManager.Instance.IsPlaying = false; // 게임 일시정지
        Time.deltaTime = 2f; // 테스트 프레임 시간
        typeof(ExperienceGemMergeController).GetMethod("Update", Private).Invoke(fixture.manager, null); // 정지 중 갱신
        Require(fixture.manager.ProcessMergePass() == 0 && AvailableValue() == 5, "정지 중 병합 발생"); // 정지 차단 확인
        GameManager.Instance.IsPlaying = true; // 게임 재개
        Time.deltaTime = 0.4f; // 기준 미달 시간
        typeof(ExperienceGemMergeController).GetMethod("Update", Private).Invoke(fixture.manager, null); // 첫 시간 갱신
        Require(UnityEngine.Object.Pickups[0].ExperienceValue == 1, "검사 간격 전 병합"); // 간격 확인
        Time.deltaTime = 0.6f; // 나머지 게임 시간
        typeof(ExperienceGemMergeController).GetMethod("Update", Private).Invoke(fixture.manager, null); // 검사 시각 도달
        Require(UnityEngine.Object.Pickups.Exists(pickup => pickup.ExperienceValue == 5 && pickup.IsMergeAvailable), "검사 간격 후 병합 누락"); // 시간 경계 확인
    } // 메서드 끝

    private static void VerifyCollectionRace() // 자동 흡수 및 수집 경합
    { // 메서드 시작
        var fixture = Create(); // 시험 환경 구성
        ExperiencePickup[] small = SpawnCluster(fixture.receiver, 5, 1, 10f); // 다섯 보석 생성
        small[4].transform.position = new Vector3(0f, 0f, 0f); // 머리 가까운 보석 이동
        Require(fixture.manager.ProcessMergePass() == 0, "흡수 범위 보석 병합"); // 흡수 범위 제외 확인
        typeof(ExperiencePickup).GetMethod("Update", Private).Invoke(small[4], null); // 실제 획득 갱신
        Require(fixture.receiver.Experience == 1 && small[4].gameObject.PendingDestroy, "보석 획득 오류"); // 획득 및 제거 예약 확인
        Require(fixture.manager.ProcessMergePass() == 0 && fixture.receiver.Experience + AvailableValue() == 5, "수집·병합 경합으로 경험치 중복 또는 손실"); // 총 경험치 확인
    } // 메서드 끝

    private static void VerifyActiveAndReceiver() // 비활성 및 다른 수신자
    { // 메서드 시작
        var fixture = Create(); // 시험 환경 구성
        ExperiencePickup[] small = SpawnCluster(fixture.receiver, 5, 1, 10f); // 다섯 보석 생성
        small[4].gameObject.activeInHierarchy = false; // 마지막 보석 비활성
        Require(fixture.manager.ProcessMergePass() == 0, "비활성 보석 병합"); // 비활성 제외 확인
        small[4].gameObject.activeInHierarchy = true; // 보석 재활성
        Require(fixture.manager.ProcessMergePass() == 1, "재활성 보석 병합 누락"); // 재활성 확인
        fixture = Create(); // 별도 시험 초기화
        SpawnCluster(fixture.receiver, 3, 1, 10f); // 첫 수신자 보석
        SpawnCluster(new SnakeExperience(), 2, 1, 10f); // 다른 수신자 보석
        Require(fixture.manager.ProcessMergePass() == 0, "서로 다른 수신자 보석 병합"); // 수신자 분리 확인
    } // 메서드 끝

    private static void VerifyPriorityAndBudget() // 우선순위와 그룹 처리량
    { // 메서드 시작
        var fixture = Create(5, 1); // 그룹 한 개 제한
        ExperiencePickup[] visible = SpawnCluster(fixture.receiver, 5, 1, 3.5f); // 화면 안 흡수 밖 보석
        ExperiencePickup[] outside = SpawnCluster(fixture.receiver, 5, 1, 10f); // 화면 밖 보석
        Require(fixture.manager.ProcessMergePass() == 1, "우선 그룹 병합 누락"); // 그룹 한도 확인
        bool visibleConsumed = false; // 화면 안 소비 여부
        bool outsideConsumed = false; // 화면 밖 소비 여부
        foreach (ExperiencePickup pickup in visible) // 화면 안 보석 순회
        { // 반복 시작
            visibleConsumed |= pickup.gameObject.PendingDestroy; // 제거 예약 기록
        } // 반복 끝
        foreach (ExperiencePickup pickup in outside) // 화면 밖 보석 순회
        { // 반복 시작
            outsideConsumed |= pickup.gameObject.PendingDestroy; // 제거 예약 기록
        } // 반복 끝
        Require(outsideConsumed && !visibleConsumed && AvailableValue() == 10, "화면 밖 우선 또는 경험치 보존 오류"); // 우선순위 확인
    } // 메서드 끝

    private static void Require(bool condition, string message) // 조건 검사
    { // 메서드 시작
        if (!condition) // 실패 조건 확인
        { // 조건 시작
            throw new InvalidOperationException(message); // 실패 전달
        } // 조건 끝
    } // 메서드 끝
} // 클래스 끝
