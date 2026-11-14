using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 실제 소환 단계 설정
using ProjectEpsilon.Combat; // 실제 적과 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 개미 출현기
using ProjectEpsilon.Player; // 실제 뱀과 장애물
using ProjectEpsilon.World; // 실제 맵과 개미굴
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay99AntHillTailVerification // 개미굴 꼬리 보호 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay99AntHillTail"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay99AntHillTailVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
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
            new GameObject("Day99AntHillTailRunner").AddComponent<Day99AntHillTailRunner>(); // 실제 소환 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day99AntHillTailVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day99AntHillTailRunner : MonoBehaviour // 실제 개미굴 시험 실행기
    { // 클래스 시작
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 비공개 시험 접근
        private IEnumerator Start() // 실제 소환 후보와 꼬리 확인
        { // 함수 시작
            PrototypeChunkMap map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 관리자
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 머리
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 개미 출현기
            Require(map != null && map.Profile != null && player != null && body != null && body.TailSegment != null && spawner != null, "실제 맵·개미 출현기·꼬리 구성 누락"); // 실제 시험 구성 확인
            map.enabled = false; // 격리 시험 중 청크 이동 중지
            player.enabled = false; // 시험 중 머리 이동 중지
            SnakeBodyFollower follower = body.GetComponent<SnakeBodyFollower>(); // 실제 몸통 추적기
            if (follower != null) // 실제 추적기 확인
            { // 조건 시작
                follower.enabled = false; // 시험 중 몸통 이동 중지
            } // 조건 종료
            SnakeSelfCollision self = player.GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 처리기
            if (self != null) // 실제 자기 충돌 처리기 확인
            { // 조건 시작
                self.enabled = false; // 격리 위치의 시험 간섭 방지
            } // 조건 종료
            spawner.enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            typeof(PrototypeMapProfile).GetField("antHillStartTime", Private).SetValue(map.Profile, 0f); // 시험용 개미굴 개방 시각
            Vector3 center = new Vector3(500f, 500f, 0f); // 다른 지형과 분리된 개미굴 중심
            Vector3 safe = center + Vector3.right * 20f; // 실제 소환 구역 밖 위치
            player.transform.position = safe; // 실제 머리 안전 위치
            foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 순회
            { // 반복 시작
                segment.transform.position = safe; // 실제 몸통 안전 위치
            } // 반복 종료
            GameObject root = new GameObject("Day99IsolatedChunk"); // 격리된 실제 지역 부모
            PrototypeSpecialZone zone = PrototypeSpecialZone.Create(root.transform, center, PrototypeZoneKind.AntHill, map.Profile); // 실제 개미굴 생성
            Vector3[] candidates = new Vector3[8]; // 실제 여덟 소환 후보
            int seed = FindSeed(center, zone.Radius + 1.5f, candidates); // 마지막 후보가 분리된 실제 난수 시드
            Require(seed > 0, "분리된 실제 소환 후보 생성 실패"); // 실제 시험 후보 확인
            for (int index = 0; index < candidates.Length - 1; index++) // 앞의 일곱 후보 순회
            { // 반복 시작
                GameObject obstacle = new GameObject("Day99CandidateObstacle"); // 실제 장애물 객체
                obstacle.transform.position = candidates[index]; // 실제 소환 후보 위치
                CircleCollider2D collider = obstacle.AddComponent<CircleCollider2D>(); // 실제 장애물 충돌체
                collider.radius = 0.05f; // 마지막 후보와 분리된 충돌 범위
                collider.isTrigger = true; // 실제 장애물 접촉 방식
                obstacle.AddComponent<SnakeContactHazard>().Configure(SnakeContactHazardType.Obstacle); // 기존 장애물 종류 설정
            } // 반복 종료
            body.TailSegment.transform.position = candidates[7]; // 실제 마지막 후보의 꼬리 위치
            Physics2D.SyncTransforms(); // 실제 물리 위치 갱신
            int before = spawner.CurrentNormalEnemyCount; // 실제 일반 적 수
            InvokeSpawn(zone, seed); // 실제 개미굴의 한 차례 소환
            Require(spawner.CurrentNormalEnemyCount == before, "꼬리와 겹친 후보에 개미 소환"); // 실제 꼬리 보호 확인
            body.TailSegment.transform.position = safe; // 실제 꼬리 안전 위치
            Physics2D.SyncTransforms(); // 실제 물리 위치 갱신
            InvokeSpawn(zone, seed); // 동일 후보의 실제 재소환
            Require(spawner.CurrentNormalEnemyCount == before + 1, "안전한 후보에서 개미 소환 실패"); // 실제 정상 소환 확인
            int limit = spawner.CurrentNormalEnemyLimit; // 현재 실제 일반 적 상한
            while (spawner.CurrentNormalEnemyCount < limit) // 실제 상한까지 일반 적 생성
            { // 반복 시작
                Vector3 position = safe + Vector3.up * (100f + spawner.CurrentNormalEnemyCount); // 소환 후보와 분리된 위치
                Require(spawner.SpawnMapChaser(position) != null, "적 상한 시험용 개미 생성 실패"); // 기존 실제 적 생성 경로
            } // 반복 종료
            InvokeSpawn(zone, seed); // 상한 도달 후 실제 개미굴 소환
            Require(spawner.CurrentNormalEnemyCount == limit, "개미굴이 기존 일반 적 수 상한을 초과"); // 실제 적 수 상한 확인
            yield return null; // 실제 생성 프레임 완료
            AutomatedDay99AntHillTailVerification.Finish(true, "Tail overlap blocked, safe ant spawning and enemy cap preserved."); // 실제 시험 결과
        } // 함수 종료
        private static int FindSeed(Vector3 center, float radius, Vector3[] candidates) // 마지막 후보가 분리된 난수 시드 탐색
        { // 함수 시작
            for (int seed = 1; seed <= 2000; seed++) // 재현 가능한 실제 시드 순회
            { // 반복 시작
                UnityEngine.Random.InitState(seed); // 실제 소환 난수 초기화
                for (int index = 0; index < candidates.Length; index++) // 실제 소환 후보 순회
                { // 반복 시작
                    float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f); // 실제 소환 방향 추첨
                    candidates[index] = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius; // 실제 개미굴 후보 위치
                } // 반복 종료
                bool separated = true; // 마지막 후보 분리 여부
                for (int index = 0; index < candidates.Length - 1; index++) // 앞선 실제 후보 순회
                { // 반복 시작
                    separated &= Vector2.Distance(candidates[index], candidates[7]) > 1f; // 마지막 후보와 장애물 분리 확인
                } // 반복 종료
                if (separated) // 실제 후보 분리 확인
                { // 조건 시작
                    return seed; // 실제 재현 시드 반환
                } // 조건 종료
            } // 반복 종료
            return 0; // 실제 유효 시드 부재
        } // 함수 종료
        private static void InvokeSpawn(PrototypeSpecialZone zone, int seed) // 실제 개미굴의 한 소환 단계 실행
        { // 함수 시작
            typeof(PrototypeSpecialZone).GetField("hazardPhase", Private).SetValue(zone, 2); // 실제 소환 단계
            typeof(PrototypeSpecialZone).GetField("antsRemaining", Private).SetValue(zone, 1); // 실제 남은 소환 기회
            typeof(PrototypeSpecialZone).GetField("nextAntAt", Private).SetValue(zone, 0f); // 실제 소환 가능 시각
            UnityEngine.Random.InitState(seed); // 실제 후보 순서 복원
            typeof(PrototypeSpecialZone).GetMethod("UpdateAntHill", Private).Invoke(zone, null); // 실제 개미굴 소환 처리
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay99AntHillTailVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
