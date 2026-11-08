using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 시험 설정 조회
using ProjectEpsilon.Combat; // 공격 대상
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Enemies; // 보스와 적
using ProjectEpsilon.Player; // 플레이어 구성
using ProjectEpsilon.Progression; // 실제 보상 조회
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedBossSummonVerification // 실제 보스 소환 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonBossSummonVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지
        private static double started; // 실제 시작 시각
        static AutomatedBossSummonVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 시작 감시
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 씬 배치 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 실제 플레이 모드 시작
        } // 메서드 끝
        private static void Tick() // 실제 시작과 시간 제한 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            { // 조건 시작
                return; // 실제 진입 대기
            } // 조건 끝
            if (started <= 0d) // 시험 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 제한
            { // 조건 시작
                Finish(1, "Timed out waiting for boss summon test."); // 시간 초과 실패
                return; // 감시 종료
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작과 실행 확인
            { // 조건 시작
                return; // 실제 시작 또는 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            { // 조건 시작
                return; // 실제 초기화 대기
            } // 조건 끝
            injected = true; // 실행기 중복 방지
            player.gameObject.AddComponent<BossSummonVerificationRunner>(); // 실제 소환 시험 시작
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 결과 기록
        { // 메서드 시작
            if (finished) // 중복 종료 확인
            { // 조건 시작
                return; // 중복 처리 방지
            } // 조건 끝
            finished = true; // 종료 상태 저장
            SessionState.SetBool(ActiveKey, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 실제 감시 해제
            Debug.Log($"[BossSummonVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 프로세스 종료
        } // 메서드 끝
    } // 클래스 끝

    public sealed class BossSummonVerificationRunner : MonoBehaviour // 실제 보스 소환 시나리오
    { // 클래스 시작
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 설정 범위
        private IEnumerator Start() // 실제 소환과 정리 검증
        { // 코루틴 시작
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동
            EarlyEnemyWaveSpawner wave = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 웨이브 출현기
            PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 관리자
            Require(player != null && wave != null && encounter != null, "Actual boss summon fixtures missing."); // 실제 필수 구성 확인
            player.enabled = false; // 플레이어 이동 통제
            wave.enabled = false; // 일반 웨이브 출현 통제
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 조회
            if (self != null) // 실제 자기 충돌 확인
            { // 조건 시작
                self.enabled = false; // 시험 중 자기 충돌 방지
            } // 조건 끝
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 무기 슬롯 순회
            { // 반복 시작
                slot.Clear(); // 자동 공격 통제
            } // 반복 끝
            SetEncounter(encounter, "summonInterval", 0.4f); // 실제 소환 간격 통제
            SetEncounter(encounter, "summonMaximum", 2); // 실제 소환 상한 통제
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("EnterBossTransition", Private); // 실제 보스 전환 경로
            Require(spawn != null, "Actual boss transition path missing."); // 실제 전환 경로 확인
            spawn.Invoke(wave, null); // 시험용 실제 보스 전환
            WeaponTarget boss = encounter.Boss; // 실제 보스 대상
            Type summonerType = typeof(PrototypeBossEncounter).Assembly.GetType("ProjectEpsilon.Enemies.PrototypeBossSummoner"); // 실제 소환기 형식
            Component summoner = boss == null || summonerType == null ? null : boss.GetComponent(summonerType); // 실제 보스 소환기
            PropertyInfo activeCount = summonerType == null ? null : summonerType.GetProperty("ActiveCount"); // 실제 소환 수 조회
            Require(summoner != null && activeCount != null, "Actual boss summoner missing."); // 실제 소환기 확인
            boss.GetComponent<EarlyEnemyBehaviour>().enabled = false; // 실제 보스 사격과 이동 통제
            boss.GetComponent<PrototypeBossChargeAttack>().enabled = false; // 실제 보스 돌진 통제
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 제한 시각
            while ((int)activeCount.GetValue(summoner) < 2 && Time.realtimeSinceStartup < deadline) // 실제 소환 대기
            { // 반복 시작
                yield return null; // 실제 프레임 진행
            } // 반복 끝
            Require((int)activeCount.GetValue(summoner) == 2, "Actual boss summon did not reach configured cap."); // 실제 소환 상한 확인
            int waveCount = wave.ActiveEnemies; // 실제 소환된 적 수
            yield return new WaitForSeconds(0.7f); // 실제 추가 소환 대기
            Require((int)activeCount.GetValue(summoner) == 2 && wave.ActiveEnemies == waveCount, "Actual boss summons exceeded cap."); // 실제 상한 유지 확인
            WeaponTarget victim = null; // 실제 소환 적 선택
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 적 순회
            { // 반복 시작
                WeaponTarget candidate = enemy.GetComponent<WeaponTarget>(); // 실제 적 전투 대상
                if (candidate != boss) // 보스 외 실제 소환 적 확인
                { // 조건 시작
                    victim = candidate; // 처치할 소환 적 저장
                    break; // 대상 검색 종료
                } // 조건 끝
            } // 반복 끝
            Require(victim != null && victim.GetComponent<EnemyDropController>() == null, "Actual boss summon retained ordinary drops."); // 실제 임시 무보상 확인
            victim.TakeDamage(victim.MaximumHealth); // 실제 소환 적 강제 처치
            yield return null; // 실제 사망 처리 진행
            Require(wave.ActiveEnemies == waveCount - 1, "Actual defeated summon remained in wave count."); // 실제 사망 수량 갱신 확인
            deadline = Time.realtimeSinceStartup + 5f; // 실제 재소환 제한 시각
            while (wave.ActiveEnemies < waveCount && Time.realtimeSinceStartup < deadline) // 실제 재소환 대기
            { // 반복 시작
                yield return null; // 실제 프레임 진행
            } // 반복 끝
            Require((int)activeCount.GetValue(summoner) == 2 && wave.ActiveEnemies == waveCount, "Actual defeated summon did not release its slot."); // 실제 소환 자리 복구 확인
            GameManager.Instance.PauseGame(); // 실제 게임 일시정지
            yield return new WaitForSecondsRealtime(0.3f); // 실제 정지 시간 경과
            Require((int)activeCount.GetValue(summoner) == 2, "Actual summons changed during pause."); // 정지 중 소환 유지 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            boss.TakeDamage(boss.MaximumHealth); // 실제 보스 강제 처치
            Require(GameManager.Instance.CurrentState == GameState.Victory, "Actual boss death did not reach victory."); // 실제 승리 확인
            yield return null; // 실제 소환 정리 진행
            yield return null; // 실제 지연 파괴 완료
            Require(wave.ActiveEnemies == 0, "Actual boss summons remained after victory."); // 실제 소환 정리 확인
            AutomatedBossSummonVerification.Finish(0, "Actual boss summons, cap, pause and victory cleanup passed."); // 실제 전체 결과
        } // 코루틴 끝
        private static void SetEncounter(PrototypeBossEncounter encounter, string name, object value) // 실제 시험 설정
        { // 메서드 시작
            FieldInfo field = typeof(PrototypeBossEncounter).GetField(name, Private); // 실제 설정 필드 조회
            Require(field != null, "Actual adjustable boss summon settings missing: " + name); // 실제 설정 존재 확인
            field.SetValue(encounter, value); // 실제 시험 설정 주입
        } // 메서드 끝
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실제 실패 조건 확인
            { // 조건 시작
                AutomatedBossSummonVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
