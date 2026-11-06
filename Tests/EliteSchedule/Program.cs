using System; // 기본 형식
using System.Linq; // 생성 대상 조회
using System.Collections.Generic; // 대기열 검사
using System.Reflection; // 실제 경로 호출
using ProjectEpsilon.Combat; // 전투 대상 대체
using ProjectEpsilon.Core; // 게임 상태 대체
using ProjectEpsilon.Enemies; // 실제 출현기
using ProjectEpsilon.Player; // 플레이어 대체
using ProjectEpsilon.Progression; // 보상 대체
using UnityEngine; // 위치와 시간 대체

internal static class Program // 엘리트 일정 시험
{ // 클래스 시작
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 비공개 범위
    private static int failures; // 실패 횟수
    private static int Main() // 시험 시작
    { // 메서드 시작
        Check("네 차례 출현과 개별 보상", VerifyFourRounds); // 일정과 보상 검사
        Check("동시 상한과 FIFO 대기", VerifyPendingQueue); // 미처치 대기 검사
        Check("만석 일반 적 정리와 엘리트 보존", VerifyFullCapacity); // 만석과 공존 검사
        Check("일시정지와 재개", VerifyPause); // 정지 시각 검사
        Check("여러 경계 통과 후 FIFO 보존", VerifySkippedBoundaries); // 긴 프레임 대기 검사
        Check("전체 상한이 엘리트로 찬 경우", VerifyEliteOnlyCapacity); // 엘리트 보존 검사
        Check("70퍼센트 축소 시간 경계", VerifySlowTime); // 축소 시간 검사
        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL: {failures}"); // 최종 결과
        return failures == 0 ? 0 : 1; // 종료 코드
    } // 메서드 끝
    private static void Check(string label, Action action) // 개별 시험
    { // 메서드 시작
        try { action(); Console.WriteLine($"PASS: {label}"); } // 성공 기록
        catch (Exception error) { failures++; Console.WriteLine($"FAIL: {label}: {error.GetBaseException().Message}"); } // 실패 기록
    } // 메서드 끝
    private static void Require(bool condition, string message) { if (!condition) { throw new InvalidOperationException(message); } } // 검증 조건
    private static object Invoke(object instance, string name, params object[] args) => instance.GetType().GetMethod(name, Private).Invoke(instance, args); // 실제 내부 경로 실행
    private static void Set(object instance, string name, object value) => instance.GetType().GetField(name, Private).SetValue(instance, value); // 직렬화 값 설정
    private static (EarlyEnemyWaveSpawner spawner, EliteRewardController rewards) Create() // 출현기 시험 구성
    { // 메서드 시작
        GameObject.Objects.Clear(); // 기존 객체 제거
        Camera.main = null; // 카메라 부재 대조
        GameManager.Instance.IsPlaying = true; // 게임 진행 설정
        EnemyDropController.TotalExperience = 0; // 경험치 기록 초기화
        Time.deltaTime = 0.02f; // 물리 경과 시간
        GameObject player = new GameObject("Player"); // 플레이어 객체
        player.AddComponent<SnakeMovement>(); // 플레이어 이동
        player.AddComponent<SnakeBodyManager>(); // 플레이어 몸통
        player.AddComponent<SnakeHealth>(); // 플레이어 체력
        GameObject owner = new GameObject("Spawner"); // 출현기 객체
        EarlyEnemyWaveSpawner spawner = owner.AddComponent<EarlyEnemyWaveSpawner>(); // 실제 출현기
        GameObject panel = new GameObject("Rewards"); // 보상 객체
        EliteRewardController rewards = panel.AddComponent<EliteRewardController>(); // 보상 대기열
        Set(spawner, "eliteRewards", rewards); // 보상 참조 연결
        Set(spawner, "nextSpawn", float.MaxValue); // 일반 자연 출현 차단
        Invoke(spawner, "Start"); // 실제 참조 초기화
        return (spawner, rewards); // 시험 구성 반환
    } // 메서드 끝
    private static void Advance(EarlyEnemyWaveSpawner spawner, float time) // 지정 시점 갱신
    { // 메서드 시작
        Set(spawner, "elapsed", time - 0.01f); // 경계 직전 시간
        Invoke(spawner, "LateUpdate"); // 실제 출현기 갱신
    } // 메서드 끝
    private static WeaponTarget[] Elites() => GameObject.Objects.Where(value => value.name == "EarlyEnemy_Elite").Select(value => value.GetComponent<WeaponTarget>()).ToArray(); // 생성된 엘리트 목록
    private static Queue<int> Pending(EarlyEnemyWaveSpawner spawner) => (Queue<int>)spawner.GetType().GetField("pendingEliteRounds", Private).GetValue(spawner); // 보류 회차 조회
    private static void VerifyFourRounds() // 네 시각의 개별 출현과 보상
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        float[] times = { 240f, 480f, 720f, 960f }; // 기획 출현 시각
        for (int index = 0; index < times.Length; index++) // 출현 시각 순회
        { // 반복 시작
            Advance(fixture.spawner, times[index]); // 현재 시각 진행
            WeaponTarget[] targets = Elites(); // 현재까지 생성된 엘리트
            Require(targets.Length == index + 1, $"{times[index]}초 엘리트 누락 또는 중복"); // 회차 출현 확인
            targets[index].Kill(); // 실제 사망 이벤트 대체
            targets[index].Kill(); // 중복 사망 대조
            Require(fixture.rewards.Count == index + 1 && EnemyDropController.TotalExperience == (index + 1) * 20, "개별 선택 보상 또는 경험치 누락·중복"); // 개별 보상 확인
        } // 반복 끝
    } // 메서드 끝
    private static void VerifyPendingQueue() // 동시 상한과 순차 대기
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        Advance(fixture.spawner, 240f); // 첫 엘리트 출현
        Advance(fixture.spawner, 480f); // 둘째 출현 예정
        Require(Elites().Length == 1, "초반 동시 상한 위반"); // 초반 한 마리 확인
        Require(Pending(fixture.spawner).SequenceEqual(new[] { 1 }), "8분 회차 대기 누락" ); // 첫 대기 회차 확인
        Advance(fixture.spawner, 720f); // 셋째 출현 예정
        Require(Elites().Length == 2, "12분 동시 두 마리와 보류 회차 누락"); // 후반 둘째 자리 확인
        Require(Pending(fixture.spawner).SequenceEqual(new[] { 2 }), "8분 회차의 FIFO 처리 오류" ); // 대기 회차 순서 확인
        Advance(fixture.spawner, 960f); // 넷째 출현 예정
        Require(Elites().Length == 2, "후반 동시 상한 위반"); // 두 마리 상한 확인
        Require(Pending(fixture.spawner).SequenceEqual(new[] { 2, 3 }), "12분·16분 회차 순서 오류" ); // 후속 대기 순서 확인
        Elites()[1].Kill(); // 둘째 실제 사망
        Invoke(fixture.spawner, "LateUpdate"); // 가장 오래된 대기 처리
        Require(Elites().Length == 3 && fixture.spawner.ActiveEnemies == 2, "첫 대기 회차의 복구 누락"); // 첫 대기 출현 확인
        Require(Pending(fixture.spawner).SequenceEqual(new[] { 3 }), "12분 회차 선행 처리 누락" ); // FIFO 선행 확인
        Elites()[0].Kill(); // 첫 엘리트 사망
        Invoke(fixture.spawner, "LateUpdate"); // 남은 대기 처리
        Require(Elites().Length == 4 && fixture.spawner.ActiveEnemies == 2, "둘째 대기 회차의 복구 누락"); // 둘째 대기 출현 확인
        Elites()[2].Kill(); // 셋째 엘리트 사망
        Elites()[3].Kill(); // 넷째 엘리트 사망
        Require(fixture.rewards.Count == 4 && EnemyDropController.TotalExperience == 80, "공존 엘리트의 개별 보상 오류"); // 네 번 보상 확인
    } // 메서드 끝
    private static void VerifyFullCapacity() // 만석에서 일반 적만 제거
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        EarlyEnemySettings ordinary = new EarlyEnemySettings { kind = EarlyEnemyKind.Chaser, health = 25f, experience = 1, attackInterval = 1f }; // 시험용 일반 적
        for (int index = 0; index < 40; index++) { Invoke(fixture.spawner, "Spawn", ordinary); } // 일반 적 만석
        GameObject farthest = GameObject.Objects.First(value => value.name == "EarlyEnemy_Chaser"); // 가장 먼 일반 적
        farthest.transform.position = new Vector3(20f, 0f, 0f); // 기존 정리 범위 내부
        Advance(fixture.spawner, 240f); // 첫 엘리트 출현
        Require(fixture.spawner.ActiveEnemies == 40 && farthest.destroyed && fixture.rewards.Count == 0 && EnemyDropController.TotalExperience == 0, "만석 일반 적 제거 또는 무보상 오류"); // 자리 확보 확인
        farthest.GetComponent<WeaponTarget>().Kill(); // 지연 파괴 중 사망 이벤트 대조
        Require(EnemyDropController.TotalExperience == 0, "자리 확보로 제거한 일반 적의 보상 발생" ); // 제거 대상의 무보상 확인
        Elites()[0].transform.position = new Vector3(21f, 0f, 0f); // 엘리트를 가장 먼 대상으로 배치
        Advance(fixture.spawner, 480f); // 둘째 엘리트 예정
        Advance(fixture.spawner, 720f); // 둘째 자리 해제
        Require(fixture.spawner.ActiveEnemies == 40 && Elites().Length == 2 && !Elites()[0].gameObject.destroyed, "기존 엘리트가 만석 제거 대상이 됨"); // 엘리트 보존 확인
    } // 메서드 끝
    private static void VerifySkippedBoundaries() // 여러 시점 도달 시 FIFO 보존
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        Advance(fixture.spawner, 960f); // 네 시각을 한 번에 통과
        Require(Elites().Length == 1 && Pending(fixture.spawner).SequenceEqual(new[] { 1, 2, 3 }), "첫 회차와 대기 순서 오류"); // 첫 회차 확인
        Invoke(fixture.spawner, "LateUpdate"); // 둘째 동시 자리 처리
        Require(Elites().Length == 2 && Pending(fixture.spawner).SequenceEqual(new[] { 2, 3 }), "둘째 회차와 대기 순서 오류"); // 둘째 회차 확인
        Elites()[0].Kill(); // 첫 엘리트 처치
        Invoke(fixture.spawner, "LateUpdate"); // 셋째 회차 처리
        Elites()[1].Kill(); // 둘째 엘리트 처치
        Invoke(fixture.spawner, "LateUpdate"); // 넷째 회차 처리
        Require(Elites().Length == 4 && Pending(fixture.spawner).Count == 0, "긴 프레임 후 네 회차 보존 오류"); // 모든 회차 확인
        Invoke(fixture.spawner, "LateUpdate"); // 추가 갱신
        Require(Elites().Length == 4, "완료 회차 중복 생성"); // 중복 방지 확인
    } // 메서드 끝
    private static void VerifyEliteOnlyCapacity() // 전체 상한에 일반 적이 없는 경우
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        Set(fixture.spawner, "maximumEnemies", 1); // 전체 적 상한 축소
        Advance(fixture.spawner, 240f); // 첫 엘리트 출현
        Advance(fixture.spawner, 720f); // 동시 상한 해제 시각
        Require(Elites().Length == 1 && fixture.spawner.ActiveEnemies == 1 && Pending(fixture.spawner).Count == 2, "엘리트만 남은 만석에서 대기 실패"); // 기존 엘리트와 대기 확인
        Elites()[0].Kill(); // 기존 엘리트 처치
        Invoke(fixture.spawner, "LateUpdate"); // 빈 자리 대기 처리
        Require(Elites().Length == 2 && fixture.spawner.ActiveEnemies == 1 && Pending(fixture.spawner).Count == 1, "빈 자리에서 보류 회차 누락"); // 보류 재개 확인
    } // 메서드 끝
    private static void VerifySlowTime() // 70퍼센트 시간 흐름 확인
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        Set(fixture.spawner, "elapsed", 239.98f); // 첫 출현 전 시간
        Time.deltaTime = 0.014f; // 70퍼센트 축소 시간
        Invoke(fixture.spawner, "LateUpdate"); // 첫 축소 프레임
        Require(Elites().Length == 0, "축소 시간 경계 전 출현"); // 경계 전 확인
        Invoke(fixture.spawner, "LateUpdate"); // 둘째 축소 프레임
        Require(Elites().Length == 1, "축소 시간 경계 후 출현 누락"); // 경계 후 확인
    } // 메서드 끝
    private static void VerifyPause() // 정지 시간과 미처치 엘리트 유지
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        Set(fixture.spawner, "elapsed", 239.99f); // 첫 출현 직전
        GameManager.Instance.IsPlaying = false; // 게임 일시정지
        Invoke(fixture.spawner, "LateUpdate"); // 정지 중 갱신
        Require(Elites().Length == 0, "정지 중 엘리트 출현"); // 정지 상태 확인
        GameManager.Instance.IsPlaying = true; // 게임 재개
        Invoke(fixture.spawner, "LateUpdate"); // 재개 후 갱신
        Require(Elites().Length == 1, "재개 후 첫 엘리트 누락"); // 재개 출현 확인
    } // 메서드 끝
} // 클래스 끝
