using System; // 예외와 형식
using System.Collections.Generic; // 실제 적 목록
using System.Reflection; // 실제 엘리트 일정 조회
using ProjectEpsilon.Combat; // 실제 적 전투 대상
using ProjectEpsilon.Core; // 실제 전투 상태
using ProjectEpsilon.Enemies; // 실제 적 출현과 행동
using ProjectEpsilon.Progression; // 실제 처치 보상
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 전투 씬 열기
using UnityEngine; // Unity 기능
namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay80EliteVerification // 전용 엘리트 시험
    { // 시험 시작
        private const string Key = "EpsilonDay80Elite"; // 시험 진행 표식
        private static bool started; // 중복 실행 방지
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 경로 조회
        static AutomatedDay80EliteVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투 준비 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 전투 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실제 전투 준비 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            try // 실제 전용 엘리트 시험
            { // 시험 시작
                Verify(); // 실제 일정과 행동 확인
                Finish(0, "Elite scheduling, ranged fire, capped summons, cleanup and rewards passed."); // 성공 결과 기록
            } // 시험 종료
            catch (Exception error) // 실제 실패 확인
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 엘리트 일정과 공격 확인
        { // 함수 시작
            EnemyWaveSettings wave = Resources.Load<EnemyWaveSettings>("EnemyWaveSettings"); // 실제 출현 데이터
            PropertyInfo roundsProperty = typeof(EnemyWaveSettings).GetProperty("AdditionalEliteRounds"); // 실제 추가 회차 설정
            Require(wave != null && roundsProperty != null, "추가 엘리트 회차 설정 누락"); // 실제 구현 확인
            EarlyEnemySettings[] rounds = (EarlyEnemySettings[])roundsProperty.GetValue(wave); // 실제 추가 회차 데이터
            Require(rounds != null && rounds.Length >= 2 && rounds[0] != null && rounds[1] != null, "전용 엘리트 데이터 누락"); // 실제 전용 엘리트 확인
            Require(rounds[0].enemySprite != null && rounds[1].enemySprite != null, "기획서 엘리트 이미지 누락"); // 실제 원본 외형 확인
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            Require(spawner != null, "실제 출현기 누락"); // 실제 전투 구성 확인
            spawner.enabled = false; // 자연 출현 통제
            typeof(EarlyEnemyWaveSpawner).GetField("nextEliteRound", Private).SetValue(spawner, 0); // 실제 첫 회차 준비
            FieldInfo elapsed = typeof(EarlyEnemyWaveSpawner).GetField("elapsed", Private); // 실제 경과 시각
            MethodInfo schedule = typeof(EarlyEnemyWaveSpawner).GetMethod("ProcessEliteSchedule", Private); // 실제 엘리트 일정
            List<WeaponTarget> targets = (List<WeaponTarget>)typeof(EarlyEnemyWaveSpawner).GetField("targets", Private).GetValue(spawner); // 실제 생존 적 목록
            EarlyEnemySettings[] originals = (EarlyEnemySettings[])typeof(EarlyEnemyWaveSpawner).GetField("enemies", Private).GetValue(spawner); // 실제 기존 엘리트 설정
            elapsed.SetValue(spawner, 240f); // 실제 첫 엘리트 경계
            schedule.Invoke(spawner, null); // 실제 첫 회차 출현
            WeaponTarget first = FindElite(targets, originals[3]); // 실제 기존 돌진 엘리트
            Require(first != null, "4분 기존 엘리트 출현 실패"); // 실제 기존 회차 확인
            elapsed.SetValue(spawner, 480f); // 실제 8분 경계
            schedule.Invoke(spawner, null); // 실제 8분 회차 출현
            Require(FindElite(targets, rounds[0]) == null, "기존 엘리트 생존 중 8분 동시 상한 초과"); // 실제 대기 회차 확인
            first.TakeDamage(100000f); // 기존 엘리트 실제 처치
            schedule.Invoke(spawner, null); // 실제 대기 회차 출현
            WeaponTarget ranged = FindElite(targets, rounds[0]); // 실제 원거리 봉쇄형 조회
            Require(ranged != null, "8분 원거리 엘리트 출현 실패"); // 실제 회차 확인
            EarlyEnemyBehaviour rangedBehaviour = ranged.GetComponent<EarlyEnemyBehaviour>(); // 실제 사격 행동
            int beforeShots = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None).Length; // 기존 적 탄 수
            typeof(EarlyEnemyBehaviour).GetField("nextAttack", Private).SetValue(rangedBehaviour, 0f); // 실제 사격 시각 도달
            typeof(EarlyEnemyBehaviour).GetMethod("FixedUpdate", Private).Invoke(rangedBehaviour, null); // 실제 사격 동작 실행
            Require(UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None).Length - beforeShots == rounds[0].projectileCount, "원거리 엘리트 사격 실패"); // 실제 사격 수 확인
            elapsed.SetValue(spawner, 720f); // 실제 12분 경계
            schedule.Invoke(spawner, null); // 실제 12분 회차 출현
            WeaponTarget summoner = FindElite(targets, rounds[1]); // 실제 소환형 엘리트 조회
            Require(summoner != null, "12분 소환 엘리트 출현 실패"); // 실제 회차 확인
            PrototypeBossSummoner controller = summoner.GetComponent<PrototypeBossSummoner>(); // 기존 소환 행동
            Require(controller != null, "기존 소환 행동 연결 누락"); // 실제 소환 구성 확인
            int beforeNormal = spawner.CurrentNormalEnemyCount; // 기존 일반 적 수
            typeof(PrototypeBossSummoner).GetField("nextSummonAt", Private).SetValue(controller, 0f); // 실제 소환 시각 도달
            typeof(PrototypeBossSummoner).GetMethod("FixedUpdate", Private).Invoke(controller, null); // 실제 소환 행동 실행
            Require(controller.ActiveCount == 1 && spawner.CurrentNormalEnemyCount == beforeNormal + 1, "소환 적의 일반 적 상한 집계 오류"); // 실제 소환과 상한 집계 확인
            List<WeaponTarget> summons = (List<WeaponTarget>)typeof(PrototypeBossSummoner).GetField("summons", Private).GetValue(controller); // 실제 소환 적 목록
            Require(summons[0].GetComponent<EnemyDropController>() == null, "소환 적 무보상 설정 누락"); // 실제 중복 보상 방지 확인
            int previousActive = spawner.ActiveEnemies; // 실제 적 수 보존
            typeof(EarlyEnemyWaveSpawner).GetField("activeEnemies", Private).SetValue(spawner, previousActive + spawner.CurrentNormalEnemyLimit); // 일반 적 상한 도달 재현
            typeof(PrototypeBossSummoner).GetField("nextSummonAt", Private).SetValue(controller, 0f); // 다음 소환 시각 도달
            typeof(PrototypeBossSummoner).GetMethod("FixedUpdate", Private).Invoke(controller, null); // 실제 상한에서 소환 시도
            Require(controller.ActiveCount == 1, "일반 적 상한 초과 소환"); // 실제 일반 적 상한 유지 확인
            typeof(EarlyEnemyWaveSpawner).GetField("activeEnemies", Private).SetValue(spawner, previousActive); // 실제 적 수 복원
            for (int index = 0; index < 3; index++) // 실제 소환 상한 확인
            { // 반복 시작
                typeof(PrototypeBossSummoner).GetField("nextSummonAt", Private).SetValue(controller, 0f); // 다음 소환 시각 도달
                typeof(PrototypeBossSummoner).GetMethod("FixedUpdate", Private).Invoke(controller, null); // 실제 반복 소환 시도
            } // 반복 종료
            Require(controller.ActiveCount == 3 && spawner.CurrentNormalEnemyCount == beforeNormal + 3, "엘리트 동시 소환 상한 오류"); // 실제 엘리트별 소환 상한 확인
            EliteRewardController rewards = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 실제 엘리트 선택 보상
            Require(rewards != null, "기존 엘리트 보상 누락"); // 실제 보상 구성 확인
            int beforeReward = rewards.PendingRewardCount; // 기존 대기 보상 수
            int beforeGems = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length; // 기존 경험치 보석 수
            summoner.TakeDamage(100000f); // 실제 소환 엘리트 처치
            Require(controller.ActiveCount == 0 && rewards.PendingRewardCount == beforeReward + 1, "엘리트 처치 후 소환 정리 또는 선택 보상 누락"); // 실제 소환 정리와 보상 확인
            ExperiencePickup[] gems = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None); // 실제 처치 보석
            Require(gems.Length == beforeGems + 1 && Array.Exists(gems, gem => gem.ExperienceValue == 20), "엘리트 처치 경험치 보석 누락"); // 기존 대형 보석 확인
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", Private); // 실제 적 생성 경로
            WeaponTarget laterSummoner = (WeaponTarget)spawn.Invoke(spawner, new object[] { rounds[1], Vector3.right * 10f, false }); // 보스 전환 전 소환 엘리트
            PrototypeBossSummoner laterController = laterSummoner.GetComponent<PrototypeBossSummoner>(); // 실제 소환 행동
            typeof(PrototypeBossSummoner).GetField("nextSummonAt", Private).SetValue(laterController, 0f); // 실제 소환 시각 도달
            typeof(PrototypeBossSummoner).GetMethod("FixedUpdate", Private).Invoke(laterController, null); // 실제 전환 전 소환
            Require(laterController.ActiveCount == 1, "보스 전환 전 소환 준비 실패"); // 실제 소환 적 확인
            typeof(EarlyEnemyWaveSpawner).GetMethod("EnterBossTransition", Private).Invoke(spawner, null); // 실제 보스 전환
            Require(spawner.ActiveEnemies == 0 && spawner.CurrentNormalEnemyCount == 0 && UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None).Length == 0, "보스 전환 후 적 또는 투사체 잔류"); // 실제 전환 정리 확인
        } // 함수 종료
        private static WeaponTarget FindElite(List<WeaponTarget> targets, EarlyEnemySettings source) // 실제 회차별 엘리트 조회
        { // 함수 시작
            foreach (WeaponTarget target in targets) // 실제 생존 적 순회
            { // 반복 시작
                EarlyEnemyBehaviour behaviour = target == null ? null : target.GetComponent<EarlyEnemyBehaviour>(); // 실제 적 행동
                EarlyEnemySettings settings = behaviour == null ? null : (EarlyEnemySettings)typeof(EarlyEnemyBehaviour).GetField("settings", Private).GetValue(behaviour); // 실제 출현 적 설정
                if (settings != null && settings.kind == EarlyEnemyKind.Elite && settings.enemySprite == source.enemySprite) // 실제 회차 이미지 확인
                { // 조건 시작
                    return target; // 실제 회차 엘리트 반환
                } // 조건 종료
            } // 반복 종료
            return null; // 실제 회차 엘리트 부재
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 원인 전달
            } // 조건 종료
        } // 함수 종료
        private static void Finish(int code, string message) // 배치 결과 처리
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 전투 감시 해제
            Debug.Log("[Day80Elite] " + (code == 0 ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
