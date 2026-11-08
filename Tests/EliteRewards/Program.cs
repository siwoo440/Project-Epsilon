using System; // 기본 형식
using System.Reflection; // 업데이트 호출
using ProjectEpsilon.Combat; // 무기 슬롯
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Player; // 플레이어 상태
using ProjectEpsilon.Progression; // 실제 보상 처리기
using ProjectEpsilon.UI; // 보상 화면

public static class Program // 행동 검증 실행기
{ // 클래스 시작
    public static int Main() // 검증 시작
    { // 메서드 시작
        try // 검증 실패 처리
        { // 보호 시작
            VerifyCandidateRules(); // 후보 추첨 규칙 검증
            VerifyQueueAndHealing(); // 대기열과 회복 검증
            VerifyBodyRepair(); // 몸통 복구 검증
            VerifyBossRepairRestriction(); // 보스전 몸통 복구 제한 검증
            VerifyWeaponAndReplacement(); // 무기와 교체 검증
            VerifyWeaponGradeLimits(); // 무기 등급 범위 검증
            VerifyMergeCancelAndSuccess(); // 합성 취소와 완료 검증
            VerifyLevelUpAndDefeat(); // 레벨업 인계와 패배 검증
            VerifySettingsBlocking(); // 설정 화면 입력 차단 검증
            VerifyUnavailableCandidates(); // 무효 후보 자동 대기 검증
            Console.WriteLine("Elite controller behavior passed."); // 성공 기록
            return 0; // 성공 종료
        } // 보호 끝
        catch (Exception exception) // 실패 예외 확인
        { // 예외 처리 시작
            Console.Error.WriteLine(exception.Message); // 실패 내용 기록
            return 1; // 실패 종료
        } // 예외 처리 끝
    } // 메서드 끝

    private static void VerifyCandidateRules() // 후보 추첨 규칙 검증
    { // 메서드 시작
        for (int mask = 1; mask < 16; mask++) // 모든 적용 가능 상태
        { // 반복 시작
            bool[] available = new bool[4]; // 적용 가능 목록
            for (int index = 0; index < 4; index++) // 네 보상 순회
            { // 반복 시작
                available[index] = (mask & (1 << index)) != 0; // 적용 가능 상태 설정
            } // 반복 끝
            for (int seed = 0; seed < 20; seed++) // 여러 추첨값 확인
            { // 반복 시작
                int[] selected = EliteRewardRules.Select(available, seed); // 실제 후보 추첨
                Require(selected.Length == 3, "후보 세 개 미생성"); // 후보 수 확인
                Require(selected[0] != selected[1] && selected[0] != selected[2] && selected[1] != selected[2], "중복 후보 생성"); // 중복 방지 확인
                Require(available[selected[0]] || available[selected[1]] || available[selected[2]], "선택 가능한 후보 누락"); // 선택 가능 후보 확인
            } // 반복 끝
        } // 반복 끝
        Require(EliteRewardRules.Select(new bool[4], 0).Length == 0, "적용 불가 보상의 대기 실패"); // 전부 불가능한 상태 확인
    } // 메서드 끝

    private static void VerifyQueueAndHealing() // 대기열과 체력 회복
    { // 메서드 시작
        Fixture fixture = new Fixture(); // 시험 환경 생성
        fixture.Health.CurrentHealth = 60; // 손상 체력 설정
        fixture.Experience.IsLevelUpPending = true; // 연속 레벨업 대기
        fixture.Controller.QueueReward(); // 첫 보상 요청
        fixture.Controller.QueueReward(); // 둘째 보상 요청
        Require(!fixture.Controller.ProcessPendingReward() && fixture.Controller.PendingRewardCount == 2, "레벨업 대기 중 보상 표시"); // 레벨업 우선 확인
        fixture.Experience.IsLevelUpPending = false; // 레벨업 종료
        Require(fixture.Controller.ProcessPendingReward() && fixture.Manager.IsPaused, "보상 중 게임 정지 실패"); // 보상 표시 확인
        fixture.Panel.Click(Find(fixture.Controller, 0)); // 체력 회복 버튼 선택
        Require(fixture.Health.CurrentHealth == 75 && fixture.Controller.PendingRewardCount == 1 && fixture.Manager.IsPlaying, "첫 회복 또는 대기열 오류"); // 회복 및 보상 소비 확인
        Require(fixture.Controller.ProcessPendingReward(), "두 번째 보상 표시 실패"); // 연속 보상 표시
        Require(fixture.Controller.TrySelectCandidate(Find(fixture.Controller, 0)), "두 번째 회복 선택 실패"); // 둘째 회복 선택
        Require(fixture.Health.CurrentHealth == 90 && fixture.Controller.PendingRewardCount == 0 && fixture.Manager.IsPlaying, "연속 보상 종료 오류"); // 연속 보상 종료 확인
    } // 메서드 끝

    private static void VerifyBodyRepair() // 몸통 복구 확인
    { // 메서드 시작
        Fixture fixture = new Fixture(); // 시험 환경 생성
        fixture.Body.CurrentBodyCount = 2; // 손실된 몸통 설정
        fixture.Body.MissingRepairableBodyCount = 1; // 복구 가능 수 설정
        fixture.Controller.QueueReward(); // 보상 요청
        Require(fixture.Controller.ProcessPendingReward(), "몸통 보상 화면 실패"); // 보상 표시 확인
        Require(fixture.Controller.TrySelectCandidate(Find(fixture.Controller, 1)), "몸통 복구 선택 실패"); // 몸통 복구 선택
        Require(fixture.Body.CurrentBodyCount == 3 && fixture.Body.MissingRepairableBodyCount == 0 && fixture.Manager.IsPlaying, "몸통 복구 결과 오류"); // 실제 몸통 수 확인
    } // 메서드 끝

    private static void VerifyBossRepairRestriction() // 이미 표시된 보상 후보의 보스전 제한
    { // 메서드 시작
        Fixture fixture = new Fixture(); // 실제 보상 처리기 구성
        fixture.Health.CurrentHealth = 50; // 대체 가능한 회복 보상 구성
        fixture.Body.CurrentBodyCount = 2; // 실제 손실 몸통 대체
        fixture.Body.MissingRepairableBodyCount = 1; // 실제 복구 가능 수 대체
        fixture.Controller.QueueReward(); // 실제 엘리트 보상 요청
        UnityEngine.Random.Counter = 2; // 회복과 복구가 포함된 후보 추첨
        Require(fixture.Controller.ProcessPendingReward(), "보상 선택 화면 미표시"); // 실제 보상 화면 확인
        int repairIndex = Find(fixture.Controller, 1); // 실제 복구 후보 위치
        int healIndex = Find(fixture.Controller, 0); // 실제 체력 회복 후보 위치
        fixture.Manager.IsFinalBossPhase = true; // 표시 이후 보스전 전환 대체
        Require(!fixture.Controller.TrySelectCandidate(repairIndex) && fixture.Body.CurrentBodyCount == 2 && fixture.Controller.PendingRewardCount == 1, "보스전 복구 보상 적용 또는 보상 소실"); // 복구 차단과 기존 보상 유지 확인
        Require(fixture.Controller.TrySelectCandidate(healIndex) && fixture.Health.CurrentHealth == 65 && fixture.Controller.PendingRewardCount == 0, "보스전 대체 보상 선택 실패"); // 기존 체력 회복 선택 확인
    } // 메서드 끝

    private static void VerifyWeaponAndReplacement() // 무기 보상과 만석 교체
    { // 메서드 시작
        Fixture fixture = new Fixture(); // 시험 환경 생성
        WeaponData reward = new WeaponData(); // 시험 보상 무기
        fixture.Pool.Entries.Add(new WeaponRewardEntry // 무기 후보 등록
        { // 후보 시작
            Weapon = reward // 보상 무기 설정
        }); // 후보 등록 완료
        fixture.Weapons.Slots.Add(new SnakeWeaponSlot()); // 빈 무기 슬롯 생성
        fixture.Controller.QueueReward(); // 보상 요청
        Require(fixture.Controller.ProcessPendingReward(), "무기 보상 화면 실패"); // 무기 보상 표시
        Require(fixture.Controller.TrySelectCandidate(Find(fixture.Controller, 2)), "무기 획득 선택 실패"); // 무기 획득 선택
        Require(fixture.Weapons.Slots[0].Weapon == reward && fixture.Controller.PendingRewardCount == 0, "빈 슬롯 무기 장착 실패"); // 무기 장착 확인
        Fixture full = new Fixture(); // 만석 시험 환경 생성
        WeaponData oldWeapon = new WeaponData(); // 기존 무기 생성
        WeaponData newWeapon = new WeaponData(); // 신규 무기 생성
        full.Weapons.Slots.Add(new SnakeWeaponSlot // 만석 슬롯 생성
        { // 슬롯 시작
            Weapon = oldWeapon // 기존 무기 설정
        }); // 슬롯 등록 완료
        full.Pool.Entries.Add(new WeaponRewardEntry // 새 무기 후보 등록
        { // 후보 시작
            Weapon = newWeapon // 신규 무기 설정
        }); // 후보 등록 완료
        WeaponData alternative = new WeaponData(); // 다른 보상 무기 생성
        full.Pool.Entries.Add(new WeaponRewardEntry // 추가 무기 후보 등록
        { // 후보 시작
            Weapon = alternative // 다른 보상 무기 설정
        }); // 후보 등록 완료
        full.Controller.QueueReward(); // 보상 요청
        Require(full.Controller.ProcessPendingReward(), "만석 무기 보상 화면 실패"); // 보상 표시 확인
        Require(full.Controller.TrySelectCandidate(Find(full.Controller, 2)) && full.Panel.ReplacementVisible, "슬롯 교체 선택 누락"); // 교체 화면 확인
        WeaponData drawnWeapon = full.Panel.LastReplacementCandidate.Weapon; // 최초 추첨 무기
        int drawnGrade = full.Panel.LastReplacementCandidate.Grade; // 최초 추첨 등급
        Require(full.Controller.TrySelectCandidate(0) && full.Controller.TrySelectCandidate(1), "무기 거절 동작 실패"); // 거절 항목 선택
        Require(full.Controller.PendingRewardCount == 1 && full.Controller.IsPresentingReward, "거절 후 보상 보존 실패"); // 보상 재선택 확인
        Require(full.Controller.TrySelectCandidate(Find(full.Controller, 2)), "교체 재선택 실패"); // 무기 재선택
        Require(full.Panel.LastReplacementCandidate.Weapon == drawnWeapon && full.Panel.LastReplacementCandidate.Grade == drawnGrade, "무기 거절 후 무기나 등급 재추첨"); // 추첨 무기 보존 확인
        Require(full.Controller.TrySelectCandidate(1), "슬롯 교체 확정 실패"); // 슬롯 교체 확정
        Require(full.Weapons.Slots[0].Weapon == drawnWeapon && full.Controller.PendingRewardCount == 0 && full.Manager.IsPlaying, "슬롯 교체 결과 오류"); // 교체 결과 확인
    } // 메서드 끝

    private static void VerifyMergeCancelAndSuccess() // 합성 취소와 성공
    { // 메서드 시작
        Fixture fixture = new Fixture(); // 시험 환경 생성
        fixture.Merge.CanMerge = true; // 합성 가능 쌍 설정
        fixture.Controller.QueueReward(); // 보상 요청
        Require(fixture.Controller.ProcessPendingReward(), "합성 보상 화면 실패"); // 보상 화면 확인
        int[] original = new int[3]; // 기존 후보 저장소
        for (int index = 0; index < 3; index++) // 세 후보 순회
        { // 반복 시작
            original[index] = fixture.Controller.Candidates[index]; // 원래 후보 저장
        } // 반복 끝
        Require(fixture.Controller.TrySelectCandidate(Find(fixture.Controller, 3)), "보상 합성 시작 실패"); // 합성 선택
        Require(fixture.Controller.IsAwaitingMerge && fixture.Merge.IsMergeOpen && fixture.Manager.IsPlaying, "합성 화면 전환 오류"); // 합성 진행 상태 확인
        fixture.Merge.CloseMergePanel(false); // 합성 취소
        Require(fixture.Controller.PendingRewardCount == 1 && !fixture.Controller.IsAwaitingMerge, "합성 취소 시 보상 소실"); // 대기열 유지 확인
        Require(fixture.Controller.ProcessPendingReward(), "취소 후 보상 재표시 실패"); // 보상 화면 복귀
        for (int index = 0; index < 3; index++) // 세 후보 순회
        { // 반복 시작
            Require(fixture.Controller.Candidates[index] == original[index], "취소 후 후보 재추첨"); // 기존 후보 유지 확인
        } // 반복 끝
        Require(fixture.Controller.TrySelectCandidate(Find(fixture.Controller, 3)), "합성 재선택 실패"); // 합성 재선택
        fixture.Merge.CloseMergePanel(true); // 합성 성공
        Require(fixture.Controller.PendingRewardCount == 0 && fixture.Manager.IsPlaying, "합성 성공 보상 소비 오류"); // 보상 소비 확인
    } // 메서드 끝

    private static void VerifyWeaponGradeLimits() // 시험 무기 등급 범위 확인
    { // 메서드 시작
        Fixture high = new Fixture(); // 이성 확정 환경 생성
        high.Weapons.Slots.Add(new SnakeWeaponSlot()); // 빈 무기 슬롯
        high.Pool.Entries.Add(new WeaponRewardEntry // 무기 후보 등록
        { // 후보 시작
            Weapon = new WeaponData() // 이성 가능 무기
        }); // 후보 등록 완료
        high.Controller.Configure(high.Experience, high.LevelUp, high.Health, high.Body, high.Weapons, high.Pool, high.Merge, high.Panel, 15, 1f, high.Settings); // 이성 확률 설정
        high.Controller.QueueReward(); // 보상 요청
        Require(high.Controller.ProcessPendingReward() && high.Controller.TrySelectCandidate(Find(high.Controller, 2)), "이성 무기 선택 실패"); // 무기 보상 선택
        Require(high.Weapons.Slots[0].Grade == 2, "이성 확률 적용 실패"); // 이성 결과 확인
        Fixture low = new Fixture(); // 일성 한도 환경 생성
        low.Weapons.Slots.Add(new SnakeWeaponSlot()); // 빈 무기 슬롯
        WeaponData oneStar = new WeaponData(); // 일성 한도 무기
        oneStar.MaxGrade = 1; // 최대 등급 설정
        low.Pool.Entries.Add(new WeaponRewardEntry // 무기 후보 등록
        { // 후보 시작
            Weapon = oneStar // 일성 한도 무기 설정
        }); // 후보 등록 완료
        low.Controller.Configure(low.Experience, low.LevelUp, low.Health, low.Body, low.Weapons, low.Pool, low.Merge, low.Panel, 15, 1f, low.Settings); // 이성 확률 설정
        low.Controller.QueueReward(); // 보상 요청
        Require(low.Controller.ProcessPendingReward() && low.Controller.TrySelectCandidate(Find(low.Controller, 2)), "일성 무기 선택 실패"); // 무기 보상 선택
        Require(low.Weapons.Slots[0].Grade == 1, "무기 최대 등급 초과"); // 일성 한도 확인
    } // 메서드 끝

    private static void VerifyLevelUpAndDefeat() // 레벨업 인계와 패배 상태
    { // 메서드 시작
        Fixture fixture = new Fixture(); // 시험 환경 생성
        fixture.Health.CurrentHealth = 80; // 회복 가능 상태
        fixture.Controller.QueueReward(); // 보상 요청
        fixture.Manager.PauseGame(); // 사용자 일시정지
        Require(!fixture.Controller.ProcessPendingReward() && fixture.Manager.IsPaused, "사용자 정지 무시"); // 사용자 정지 유지 확인
        fixture.Manager.ResumeGame(); // 사용자 정지 해제
        Require(fixture.Controller.ProcessPendingReward(), "레벨업 전 보상 표시 실패"); // 보상 화면 표시
        Require(fixture.Controller.YieldPauseToLevelUp() && fixture.Manager.IsPaused && fixture.Controller.PendingRewardCount == 1, "레벨업 정지 인계 실패"); // 소유권과 대기열 확인
        fixture.Experience.IsLevelUpPending = true; // 레벨업 선택 대기
        fixture.Manager.ResumeGame(); // 시험용 레벨업 완료
        Require(!fixture.Controller.ProcessPendingReward(), "레벨업 대기 중 보상 표시"); // 레벨업 우선 확인
        fixture.Experience.IsLevelUpPending = false; // 레벨업 종료
        Require(fixture.Controller.ProcessPendingReward(), "레벨업 후 보상 재표시 실패"); // 보상 재표시 확인
        fixture.Manager.IsPaused = false; // 시험용 패배 상태
        fixture.Manager.IsPlaying = false; // 플레이 종료
        typeof(EliteRewardController).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(fixture.Controller, null); // 패배 상태 갱신
        Require(!fixture.Manager.IsPlaying && fixture.Controller.PendingRewardCount == 1 && !fixture.Controller.IsPresentingReward, "패배 중 게임 재개"); // 패배 유지 확인
    } // 메서드 끝

    private static void VerifySettingsBlocking() // 설정 중 입력 차단
    { // 메서드 시작
        Fixture fixture = new Fixture(); // 시험 환경 생성
        fixture.Health.CurrentHealth = 80; // 회복 가능 상태
        fixture.Settings.IsOpen = true; // 설정 화면 열기
        fixture.Controller.QueueReward(); // 보상 요청
        Require(!fixture.Controller.ProcessPendingReward(), "설정 중 보상 표시"); // 설정 우선 확인
        fixture.Settings.IsOpen = false; // 설정 화면 닫기
        Require(fixture.Controller.ProcessPendingReward(), "설정 종료 후 보상 표시 실패"); // 보상 표시 확인
        int healIndex = Find(fixture.Controller, 0); // 체력 보상 위치
        fixture.Settings.IsOpen = true; // 보상 중 설정 열기
        fixture.Panel.Click(healIndex); // 마우스 보상 선택
        Require(fixture.Health.CurrentHealth == 80 && fixture.Manager.IsPaused && fixture.Controller.PendingRewardCount == 1, "설정 중 버튼 선택 허용"); // 클릭 차단 확인
        UnityEngine.InputSystem.Keyboard keyboard = new UnityEngine.InputSystem.Keyboard(); // 시험 키보드 생성
        UnityEngine.InputSystem.Keyboard.current = keyboard; // 현재 키보드 연결
        keyboard.digit1Key.wasPressedThisFrame = healIndex == 0; // 첫 후보 키 입력
        keyboard.digit2Key.wasPressedThisFrame = healIndex == 1; // 둘째 후보 키 입력
        keyboard.digit3Key.wasPressedThisFrame = healIndex == 2; // 셋째 후보 키 입력
        Tick(fixture.Controller); // 설정 중 키 입력 처리
        Require(fixture.Health.CurrentHealth == 80 && fixture.Manager.IsPaused, "설정 중 숫자 키 선택 허용"); // 숫자 입력 차단 확인
        UnityEngine.InputSystem.Keyboard.current = null; // 키보드 참조 해제
        fixture.Settings.IsOpen = false; // 설정 화면 닫기
        InputBindingManager.Instance.IsRebinding = true; // 키 변경 시작
        Require(!fixture.Controller.TrySelectCandidate(healIndex) && fixture.Manager.IsPaused, "키 변경 중 보상 선택 허용"); // 키 변경 차단 확인
        UnityEngine.InputSystem.Keyboard.current = keyboard; // 시험 키보드 재연결
        Tick(fixture.Controller); // 키 변경 중 숫자 입력 처리
        Require(fixture.Health.CurrentHealth == 80 && fixture.Manager.IsPaused, "키 변경 중 숫자 키 선택 허용"); // 숫자 입력 차단 확인
        UnityEngine.InputSystem.Keyboard.current = null; // 키보드 참조 해제
        InputBindingManager.Instance.IsRebinding = false; // 키 변경 종료
        Require(fixture.Controller.TrySelectCandidate(healIndex) && fixture.Health.CurrentHealth == 95, "설정 종료 후 보상 선택 실패"); // 정상 선택 복구 확인
    } // 메서드 끝

    private static void VerifyUnavailableCandidates() // 모든 후보 무효화 처리
    { // 메서드 시작
        Fixture fixture = new Fixture(); // 시험 환경 생성
        fixture.Health.CurrentHealth = 80; // 회복 가능 상태
        fixture.Controller.QueueReward(); // 보상 요청
        Require(fixture.Controller.ProcessPendingReward(), "무효화 시험 보상 표시 실패"); // 보상 화면 확인
        fixture.Health.CurrentHealth = fixture.Health.MaximumHealth; // 모든 보상 무효화
        Tick(fixture.Controller); // 화면 상태 갱신
        Require(!fixture.Controller.IsPresentingReward && fixture.Controller.PendingRewardCount == 1 && fixture.Manager.IsPlaying, "무효 후보에서 대기 실패"); // 잠금 없는 대기 확인
        Require(!fixture.Controller.ProcessPendingReward(), "사용 불가 보상 즉시 재표시"); // 유효 후보 대기 확인
        fixture.Health.CurrentHealth = 90; // 회복 가능 상태 복원
        Require(fixture.Controller.ProcessPendingReward(), "유효 보상 복원 후 재표시 실패"); // 보상 재표시 확인
    } // 메서드 끝

    private static void Tick(EliteRewardController controller) // 실제 업데이트 실행
    { // 메서드 시작
        typeof(EliteRewardController).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(controller, null); // 프레임 처리 호출
    } // 메서드 끝

    private static int Find(EliteRewardController controller, int kind) // 보상 종류 위치 조회
    { // 메서드 시작
        for (int index = 0; index < controller.Candidates.Count; index++) // 후보 목록 순회
        { // 반복 시작
            if (controller.Candidates[index] == kind) // 보상 종류 일치 확인
            { // 조건 시작
                return index; // 후보 위치 반환
            } // 조건 끝
        } // 반복 끝
        throw new InvalidOperationException("사용 가능한 보상 후보 누락"); // 후보 누락 보고
    } // 메서드 끝

    private static void Require(bool condition, string message) // 검증 조건 확인
    { // 메서드 시작
        if (!condition) // 실패 확인
        { // 조건 시작
            throw new InvalidOperationException(message); // 실패 보고
        } // 조건 끝
    } // 메서드 끝

    private sealed class Fixture // 보상 시험 환경
    { // 클래스 시작
        public readonly GameManager Manager = new GameManager(); // 게임 관리자
        public readonly SnakeExperience Experience = new SnakeExperience(); // 경험치 상태
        public readonly SnakeLevelUpController LevelUp = new SnakeLevelUpController(); // 레벨업 상태
        public readonly SnakeHealth Health = new SnakeHealth(); // 체력 상태
        public readonly SnakeBodyManager Body = new SnakeBodyManager(); // 몸통 상태
        public readonly SnakeWeaponManager Weapons = new SnakeWeaponManager(); // 무기 상태
        public readonly WeaponRewardPool Pool = new WeaponRewardPool(); // 무기 후보 풀
        public readonly SnakeWeaponMergeController Merge = new SnakeWeaponMergeController(); // 합성 상태
        public readonly LevelUpPanelController Panel = new LevelUpPanelController(); // 보상 화면
        public readonly SettingsMenuController Settings = new SettingsMenuController(); // 설정 화면
        public readonly EliteRewardController Controller = new EliteRewardController(); // 실제 보상 처리기
        public Fixture() // 시험 환경 구성
        { // 생성자 시작
            UnityEngine.Random.Counter = 0; // 추첨 상태 초기화
            GameManager.Instance = Manager; // 게임 관리자 연결
            InputBindingManager.Instance = new InputBindingManager(); // 입력 관리자 연결
            Controller.Configure(Experience, LevelUp, Health, Body, Weapons, Pool, Merge, Panel, 15, 0.25f, Settings); // 실제 보상 처리기 구성
        } // 생성자 끝
    } // 클래스 끝
} // 클래스 끝
