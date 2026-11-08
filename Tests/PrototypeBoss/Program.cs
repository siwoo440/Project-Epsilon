using System; // 기본 형식
using System.Reflection; // 관리자 수명주기 조회
using ProjectEpsilon.Combat; // 실제 사망 이벤트 대체
using ProjectEpsilon.Core; // 실제 게임 관리자
using ProjectEpsilon.Enemies; // 실제 보스 단계
using ProjectEpsilon.Player; // 실제 소진 이벤트 대체
using UnityEngine; // 게임 시간 대체

internal static class Program // 시험 보스 규칙 검증
{ // 클래스 시작
    private static int failures; // 실패 시험 수
    private static int Main() // 시험 진입점
    { // 메서드 시작
        Check("생존 보스 HP 50% 단계", VerifyPhase); // 실제 단계 경계 검증
        Check("등록 보스 사망만 승리", VerifyVictory); // 실제 사망 이벤트 검증
        Check("패배 후 승리 차단", VerifyDefeat); // 기존 패배 보존 검증
        Check("보스 등록과 중복 방지", VerifyRegistration); // 실제 등록 경계 검증
        return failures == 0 ? 0 : 1; // 실제 종료 코드
    } // 메서드 끝
    private static void Check(string name, Action test) // 독립 시험 실행
    { // 메서드 시작
        try // 시험 시작
        { // 시도 시작
            test(); // 실제 제품 코드 실행
            Console.WriteLine("PASS: " + name); // 통과 결과 기록
        } // 시도 끝
        catch (Exception error) // 실제 실패 확인
        { // 예외 시작
            failures++; // 실패 수 증가
            Console.WriteLine("FAIL: " + name + ": " + error.GetBaseException().Message); // 실패 원인 기록
        } // 예외 끝
    } // 메서드 끝
    private static void VerifyPhase() // 실제 체력 단계 경계
    { // 메서드 시작
        PrototypeBossPhase phase = new PrototypeBossPhase(); // 실제 단계 상태
        phase.Observe(51f, 100f, true); // 절반 초과 생존 체력
        Require(!phase.IsPhaseTwo, "절반 초과 단계 오류"); // 첫 단계 유지 확인
        phase.Observe(50f, 100f, true); // 정확한 절반 생존 체력
        Require(phase.IsPhaseTwo, "절반 체력 단계 누락"); // 두 번째 단계 확인
        phase.Observe(90f, 100f, true); // 이후 체력 복구 상태
        Require(phase.IsPhaseTwo, "두 번째 단계 유지 실패"); // 단계 단조 유지 확인
        PrototypeBossPhase dead = new PrototypeBossPhase(); // 별도 사망 대상 단계
        dead.Observe(0f, 100f, false); // 실제 사망 체력
        Require(!dead.IsPhaseTwo, "사망 이후 단계 전환 오류"); // 사망 경계 확인
    } // 메서드 끝
    private static void VerifyVictory() // 실제 대상 등록과 사망 검증
    { // 메서드 시작
        GameManager manager = Fixture(out SnakeHealth health); // 실제 게임 관리자 구성
        WeaponTarget boss = new WeaponTarget(); // 실제 보스 대상 대체
        WeaponTarget ordinary = new WeaponTarget(); // 별도 일반 적 대체
        Require(manager.RegisterFinalBoss(boss), "보스 등록 실패"); // 실제 등록 확인
        ordinary.Kill(); // 일반 적 사망 이벤트
        Require(manager.IsPlaying, "일반 적 사망 시 승리 오류"); // 일반 적 사망 무시 확인
        boss.Kill(); // 실제 등록 보스 사망 이벤트
        Require(manager.CurrentState == GameState.Victory && Time.timeScale == 0f, "보스 사망 승리 누락"); // 승리와 정지 확인
    } // 메서드 끝
    private static void VerifyDefeat() // 기존 패배 상태 보존
    { // 메서드 시작
        GameManager manager = Fixture(out SnakeHealth health); // 실제 관리자 구성
        WeaponTarget boss = new WeaponTarget(); // 실제 보스 대상 대체
        Require(manager.RegisterFinalBoss(boss), "보스 등록 실패"); // 실제 등록 확인
        health.Deplete(); // 실제 몸통 소진 알림
        boss.Kill(); // 이후 실제 보스 사망
        Require(manager.CurrentState == GameState.Defeated, "기존 패배가 승리로 변경됨"); // 패배 상태 유지 확인
    } // 메서드 끝
    private static void VerifyRegistration() // 실제 중복 등록 경계
    { // 메서드 시작
        GameManager manager = Fixture(out SnakeHealth health); // 실제 관리자 구성
        WeaponTarget boss = new WeaponTarget(); // 첫 보스 대상
        WeaponTarget other = new WeaponTarget(); // 별도 보스 대상
        Require(manager.RegisterFinalBoss(boss) && manager.RegisterFinalBoss(boss), "동일 대상 등록 실패"); // 같은 대상 멱등 확인
        Require(!manager.RegisterFinalBoss(other), "다른 보스 중복 등록 오류"); // 다른 대상 중복 차단
        other.Kill(); // 등록하지 않은 대상 사망
        Require(manager.IsPlaying, "미등록 대상 사망 승리 오류"); // 잘못된 승리 방지
        boss.Kill(); // 실제 등록 대상 사망
        Require(manager.CurrentState == GameState.Victory, "등록 대상 사망 승리 누락"); // 실제 승리 확인
    } // 메서드 끝
    private static GameManager Fixture(out SnakeHealth health) // 실제 관리자 시험 구성
    { // 메서드 시작
        typeof(GameManager).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null); // 이전 전역 관리자 제거
        UnityEngine.Object.Found.Clear(); // 이전 시험 객체 제거
        health = new SnakeHealth(); // 몸통 소진 이벤트 대체
        UnityEngine.Object.Found[typeof(SnakeHealth)] = health; // 실제 검색 결과 연결
        GameManager manager = new GameManager(); // 실제 관리자 생성
        typeof(GameManager).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, null); // 실제 초기화 호출
        typeof(GameManager).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, null); // 실제 시작과 이벤트 연결
        Require(manager.IsPlaying, "실제 게임 시작 실패"); // 실제 시작 상태 확인
        return manager; // 실제 관리자 반환
    } // 메서드 끝
    private static void Require(bool condition, string message) // 실제 시험 조건 확인
    { // 메서드 시작
        if (!condition) // 실패 조건 확인
        { // 조건 시작
            throw new InvalidOperationException(message); // 실제 실패 기록
        } // 조건 끝
    } // 메서드 끝
} // 영역 끝
