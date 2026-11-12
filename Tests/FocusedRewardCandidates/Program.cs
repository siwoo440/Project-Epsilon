using System; // 예외와 결과 출력
using System.Collections.Generic; // 실제 포탑 후보 목록
using ProjectEpsilon.Combat; // 실제 장착 슬롯 대체
using ProjectEpsilon.Data; // 실제 포탑 데이터 대체
using ProjectEpsilon.Progression; // 실제 보상 풀

internal static class Program // 실제 후보 추첨 경계 검증
{
    private static int Main() // 실제 시험 진입
    {
        try // 실제 시험 실패 처리
        {
            VerifyWeightedFirstCard(); // 첫 카드 확률과 나머지 카드 검증
            VerifyTiesAndFallback(); // 최다 동률과 비보유 대체 검증
            VerifyIdentityAndCompletedGrade(); // 동일 식별자와 5등급 제외 검증
            VerifyAdditionalCardsAndGrades(); // 추가 카드와 등급 확률 검증
            Console.WriteLine("PASS: focused reward candidate rules"); // 실제 시험 성공 기록
            return 0; // 실제 성공 종료 코드
        }
        catch (Exception error) // 실제 시험 실패 확인
        {
            Console.Error.WriteLine("FAIL: " + error.Message); // 실제 실패 사유 출력
            return 1; // 실제 실패 종료 코드
        }
    }

    private static WeaponData Weapon(string id) // 실제 식별자를 가진 포탑 대체
    {
        return new WeaponData { Id = id }; // 실제 식별자 데이터 반환
    }

    private static SnakeWeaponSlot Slot(WeaponData weapon, int grade) // 실제 장착 슬롯 대체
    {
        return new SnakeWeaponSlot { Weapon = weapon, Grade = grade }; // 실제 포탑과 등급 반환
    }

    private static void VerifyWeightedFirstCard() // 첫 카드 40퍼센트 집중 추첨 검증
    {
        WeaponData a = Weapon("a"); // 실제 최다 보유 포탑
        WeaponData b = Weapon("b"); // 실제 다른 포탑
        WeaponData c = Weapon("c"); // 실제 세 번째 포탑
        WeaponRewardPool pool = new WeaponRewardPool(); // 실제 보상 풀
        pool.Configure(new[] { a, b, c }); // 실제 세 종류 후보 구성
        SnakeWeaponManager manager = new SnakeWeaponManager(); // 실제 장착 관리자 대체
        manager.Slots.AddRange(new[] { Slot(a, 1), Slot(a, 2), Slot(b, 1) }); // 서로 다른 등급의 동일 포탑 합산
        UnityEngine.Random.InitState(6201); // 실제 추첨 재현 시드
        int[] counts = new int[3]; // 실제 포탑 위치별 등장 횟수
        const int attempts = 12000; // 실제 독립 추첨 수
        for (int iteration = 0; iteration < attempts; iteration++) // 실제 후보 추첨 반복
        {
            List<WeaponRewardCandidate> candidates = pool.BuildCandidates(1, manager, 3); // 실제 세 장 후보 추첨
            Require(candidates.Count == 3 && candidates[0].Weapon.Id != candidates[1].Weapon.Id && candidates[0].Weapon.Id != candidates[2].Weapon.Id && candidates[1].Weapon.Id != candidates[2].Weapon.Id, "카드 간 동일 식별자 중복"); // 실제 중복 금지 확인
            for (int index = 0; index < candidates.Count; index++) // 실제 세 카드 순회
            {
                counts[index] += candidates[index].Weapon.Id == "a" ? 1 : 0; // 실제 최다 포탑 위치 집계
                Require(candidates[index].Grade == 1, "초기 레벨의 2등급 등장"); // 실제 초반 등급 확인
            }
        }
        Require(counts[0] > attempts * 0.57 && counts[0] < attempts * 0.63, "첫 카드의 집중 40퍼센트와 일반 추첨 비율 불일치"); // 실제 40퍼센트와 일반 추첨 결과 확인
        Require(counts[1] > attempts * 0.17 && counts[1] < attempts * 0.23 && counts[2] > attempts * 0.17 && counts[2] < attempts * 0.23, "두 번째 이후 카드의 일반 추첨 비율 불일치"); // 실제 나머지 카드의 비중 확인
    }

    private static void VerifyTiesAndFallback() // 최다 동률과 보유 대상 부재 검증
    {
        WeaponData a = Weapon("a"); // 실제 첫 포탑
        WeaponData b = Weapon("b"); // 실제 두 번째 포탑
        WeaponData c = Weapon("c"); // 실제 미보유 포탑
        WeaponRewardPool pool = new WeaponRewardPool(); // 실제 보상 풀
        pool.Configure(new[] { a, b, c }); // 실제 세 종류 후보 구성
        SnakeWeaponManager manager = new SnakeWeaponManager(); // 실제 장착 관리자 대체
        manager.Slots.AddRange(new[] { Slot(a, 1), Slot(b, 3) }); // 실제 서로 다른 등급 동률 구성
        UnityEngine.Random.InitState(6202); // 실제 동률 추첨 재현 시드
        int[] counts = new int[3]; // 실제 첫 카드 등장 횟수
        const int attempts = 12000; // 실제 독립 추첨 수
        for (int iteration = 0; iteration < attempts; iteration++) // 실제 후보 추첨 반복
        {
            string id = pool.BuildCandidates(1, manager, 3)[0].Weapon.Id; // 실제 첫 카드 포탑
            counts[id == "a" ? 0 : id == "b" ? 1 : 2]++; // 실제 첫 카드 종류 집계
        }
        Require(counts[0] > attempts * 0.37 && counts[0] < attempts * 0.43 && counts[1] > attempts * 0.37 && counts[1] < attempts * 0.43 && counts[2] > attempts * 0.17 && counts[2] < attempts * 0.23, "최다 동률의 균등 집중 추첨 불일치"); // 실제 동률 균등 배분 확인
        manager.Slots.Clear(); // 실제 보유 포탑 제거
        UnityEngine.Random.InitState(6203); // 실제 일반 추첨 재현 시드
        Array.Clear(counts, 0, counts.Length); // 실제 등장 횟수 초기화
        for (int iteration = 0; iteration < attempts; iteration++) // 실제 일반 추첨 반복
        {
            string id = pool.BuildCandidates(1, manager, 3)[0].Weapon.Id; // 실제 첫 일반 카드
            counts[id == "a" ? 0 : id == "b" ? 1 : 2]++; // 실제 일반 카드 종류 집계
        }
        Require(Array.TrueForAll(counts, count => count > attempts * 0.30 && count < attempts * 0.37), "보유 대상이 없을 때 일반 추첨 불일치"); // 실제 일반 추첨 대체 확인
    }

    private static void VerifyIdentityAndCompletedGrade() // 동일 ID와 5등급 제외 검증
    {
        WeaponData a = Weapon("a"); // 실제 첫 동일 포탑
        WeaponData sameA = Weapon("a"); // 다른 데이터의 동일 ID
        WeaponData b = Weapon("b"); // 실제 다른 포탑
        WeaponData c = Weapon("c"); // 실제 세 번째 포탑
        WeaponRewardPool pool = new WeaponRewardPool(); // 실제 보상 풀
        pool.Configure(new[] { a, sameA, b, c }); // 실제 중복 ID 후보 구성
        SnakeWeaponManager manager = new SnakeWeaponManager(); // 실제 장착 관리자 대체
        manager.Slots.Add(Slot(sameA, 5)); // 동일 ID의 5등급 장착
        for (int iteration = 0; iteration < 100; iteration++) // 실제 제외 추첨 반복
        {
            List<WeaponRewardCandidate> candidates = pool.BuildCandidates(20, manager, 4); // 실제 완성 포탑 제외 추첨
            Require(candidates.Count == 2 && candidates[0].Weapon.Id != candidates[1].Weapon.Id && candidates[0].Weapon.Id != "a" && candidates[1].Weapon.Id != "a", "5등급 동일 ID 제외 불일치"); // 실제 완성 종류 제외 확인
        }
        manager.Slots.Clear(); // 실제 완성 포탑 모두 제거
        List<WeaponRewardCandidate> restored = pool.BuildCandidates(20, manager, 4); // 실제 복귀 후보 추첨
        Require(restored.Count == 3 && restored[0].Weapon.Id != restored[1].Weapon.Id && restored[0].Weapon.Id != restored[2].Weapon.Id && restored[1].Weapon.Id != restored[2].Weapon.Id, "동일 ID 중복 제거 또는 후보 복귀 불일치"); // 실제 복귀와 카드 중복 금지 확인
    }

    private static void VerifyAdditionalCardsAndGrades() // 추가 카드와 등급 확률 검증
    {
        WeaponData a = Weapon("a"); // 실제 보유 포탑
        WeaponData b = Weapon("b"); // 실제 두 번째 포탑
        WeaponData c = Weapon("c"); // 실제 세 번째 포탑
        WeaponData d = Weapon("d"); // 실제 네 번째 포탑
        WeaponData e = Weapon("e"); // 실제 다섯 번째 포탑
        WeaponRewardPool pool = new WeaponRewardPool(); // 실제 보상 풀
        pool.Configure(new[] { a, b, c, d, e }); // 실제 다섯 종류 후보 구성
        SnakeWeaponManager manager = new SnakeWeaponManager(); // 실제 장착 관리자 대체
        manager.Slots.Add(Slot(a, 1)); // 실제 집중 대상 한 종류 장착
        UnityEngine.Random.InitState(6204); // 실제 추가 카드 추첨 시드
        int[] locations = new int[5]; // 실제 포탑 카드별 등장 횟수
        int[] gradeTwo = new int[5]; // 실제 카드별 2등급 등장 횟수
        const int attempts = 12000; // 실제 독립 추첨 횟수
        for (int iteration = 0; iteration < attempts; iteration++) // 실제 다섯 카드 추첨 반복
        {
            List<WeaponRewardCandidate> candidates = pool.BuildCandidates(20, manager, 5); // 실제 20레벨 다섯 카드
            Require(candidates.Count == 5, "추가 카드 후보 수 불일치"); // 실제 추가 카드 수 확인
            for (int index = 0; index < candidates.Count; index++) // 실제 다섯 카드 순회
            {
                locations[index] += candidates[index].Weapon.Id == "a" ? 1 : 0; // 실제 보유 포탑 위치 집계
                gradeTwo[index] += candidates[index].Grade == 2 ? 1 : 0; // 실제 카드별 2등급 집계
            }
        }
        Require(locations[0] > attempts * 0.49 && locations[0] < attempts * 0.55, "추가 카드가 첫 카드 집중 확률을 변경"); // 실제 첫 카드 집중 비율 확인
        for (int index = 1; index < locations.Length; index++) // 실제 추가 카드 순회
        {
            Require(locations[index] > attempts * 0.10 && locations[index] < attempts * 0.14, "두 번째 이후 카드에 집중 추첨 적용"); // 실제 일반 카드별 동일 비중 확인
        }
        foreach (int count in gradeTwo) // 실제 카드별 2등급 횟수 순회
        {
            Require(count > attempts * 0.17 && count < attempts * 0.23, "카드별 2등급 확률 불일치"); // 실제 20레벨 20퍼센트 등급 확률 확인
        }
    }

    private static void Require(bool condition, string message) // 실제 시험 조건 확인
    {
        if (!condition) // 실제 실패 조건 확인
        {
            throw new InvalidOperationException(message); // 실제 실패 사유 전달
        }
    }
}
