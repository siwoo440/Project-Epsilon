using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 실제 보스 전환 조회
using ProjectEpsilon.Combat; // 실제 보스 공격 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 출현기와 보스
using ProjectEpsilon.Player; // 실제 플레이어와 장애물
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay116BossSafeSpawnVerification // 실제 보스 안전 출현 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay116BossSafeSpawn"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay116BossSafeSpawnVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void RunBlocked() // 모든 후보 차단 시험
        { // 함수 시작
            Begin(true, false); // 실제 안전 후보 재시도 시험
        } // 함수 종료
        public static void RunAlternate() // 기존 지점 장애물 시험
        { // 함수 시작
            Begin(false, false); // 실제 대체 후보 시험
        } // 함수 종료
        public static void RunTail() // 기존 지점 꼬리 시험
        { // 함수 시작
            Begin(false, true); // 실제 꼬리 보호 시험
        } // 함수 종료
        private static void Begin(bool blocked, bool tail) // 실제 게임 씬 시험 준비
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool("EpsilonDay116Blocked", blocked); // 실제 시험 종류 저장
            SessionState.SetBool("EpsilonDay116Tail", tail); // 실제 꼬리 시험 여부 저장
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실제 전투 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시작 방지
            new GameObject("Day116BossSafeSpawnRunner").AddComponent<Day116BossSafeSpawnRunner>(); // 실제 보스 출현 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day116BossSafeSpawnVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day116BossSafeSpawnRunner : MonoBehaviour // 실제 보스 출현 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 장애물과 보스 전환 확인
        { // 함수 시작
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            PrototypeBossEncounter encounter = FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 관리자
            Require(player != null && weapons != null && spawner != null && encounter != null, "실제 보스 출현 구성 누락"); // 실제 구성 확인
            player.enabled = false; // 시험 중 플레이어 이동 중지
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 자동 공격 간섭 방지
            foreach (SnakeContactHazard old in FindObjectsByType<SnakeContactHazard>(FindObjectsSortMode.None)) // 기존 장애물과 적 접촉 순회
            { // 반복 시작
                if (old.HazardType == SnakeContactHazardType.Obstacle) // 기존 맵 장애물 확인
                { // 조건 시작
                    old.gameObject.SetActive(false); // 시험 후보의 장애물 간섭 제거
                } // 조건 종료
            } // 반복 종료
            bool blocked = SessionState.GetBool("EpsilonDay116Blocked", false); // 실제 전체 차단 시험 여부
            bool tail = SessionState.GetBool("EpsilonDay116Tail", false); // 실제 꼬리 시험 여부
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            if (tail) // 실제 꼬리 위치 시험 확인
            { // 조건 시작
                SnakeBodyFollower follower = body == null ? null : body.GetComponent<SnakeBodyFollower>(); // 실제 몸통 추적기
                Require(body != null && body.TailSegment != null && follower != null, "실제 꼬리 시험 구성 누락"); // 실제 꼬리 구성 확인
                follower.enabled = false; // 시험 중 꼬리 추적 중지
                body.TailSegment.transform.position = player.transform.position + Vector3.up * 8f; // 기존 보스 위치의 실제 꼬리
            } // 조건 종료
            GameObject[] obstacles = new GameObject[blocked ? 16 : tail ? 0 : 1]; // 실제 후보별 시험 장애물
            for (int index = 0; index < obstacles.Length; index++) // 실제 후보 장애물 생성
            { // 반복 시작
                float radians = index * Mathf.PI * 2f / 16f; // 실제 후보 각도
                Vector3 direction = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0f); // 기존 위쪽부터의 실제 후보 방향
                GameObject obstacle = new GameObject("Day116Obstacle"); // 실제 장애물 객체
                obstacle.transform.position = player.transform.position + direction * 8f; // 실제 기존 출현 거리의 장애물
                CircleCollider2D shape = obstacle.AddComponent<CircleCollider2D>(); // 실제 장애물 충돌체
                shape.isTrigger = true; // 기존 장애물과 같은 접촉 방식
                shape.radius = 0.6f; // 실제 후보 겹침 크기
                obstacle.AddComponent<SnakeContactHazard>().Configure(SnakeContactHazardType.Obstacle); // 기존 장애물 판정
                obstacles[index] = obstacle; // 실제 생성 장애물 저장
            } // 반복 종료
            Physics2D.SyncTransforms(); // 실제 장애물의 물리 위치 반영
            typeof(EarlyEnemyWaveSpawner).GetMethod("EnterBossTransition", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(spawner, null); // 실제 보스 전환
            WeaponTarget boss = encounter.Boss; // 실제 생성된 보스
            if (blocked) // 전체 후보 차단 시험 확인
            { // 조건 시작
                Require(boss == null, "모든 후보가 막혔는데 보스가 겹쳐 출현"); // 실제 안전 대기 확인
                obstacles[0].SetActive(false); // 실제 한 후보의 장애물 제거
                Physics2D.SyncTransforms(); // 실제 제거 상태 반영
                yield return new WaitForSeconds(0.6f); // 실제 안전 후보 재시도 대기
                boss = encounter.Boss; // 재시도 후 실제 보스
                Require(boss != null && GameManager.Instance.IsFinalBossPhase, "안전 후보 복구 후 보스 재출현 실패"); // 실제 재시도와 보스 등록 확인
            } // 조건 종료
            else if (tail) // 실제 꼬리 위치 차단 시험
            { // 조건 시작
                Require(boss != null && GameManager.Instance.IsFinalBossPhase, "꼬리 회피 후 보스 출현 실패"); // 실제 보스 등록 확인
                Require(Vector2.Distance(boss.transform.position, body.TailSegment.transform.position) > 1.5f, "실제 꼬리 위에 보스 출현"); // 실제 꼬리 이격 확인
            } // 조건 종료
            else // 기존 출현 지점 차단 시험
            { // 대체 시작
                Require(boss != null && GameManager.Instance.IsFinalBossPhase, "대체 후보의 실제 보스 출현 실패"); // 실제 보스 등록 확인
                Require(Vector2.Distance(boss.transform.position, obstacles[0].transform.position) > 2f, "기존 장애물 위에 보스 출현"); // 실제 장애물 이격 확인
            } // 대체 종료
            AutomatedDay116BossSafeSpawnVerification.Finish(true, "Actual boss transition avoided occupied spawn points and retried safely."); // 실제 시험 결과
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay116BossSafeSpawnVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
