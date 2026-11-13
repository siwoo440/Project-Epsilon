using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 맵 데이터 설정 조회
using ProjectEpsilon.Combat; // 실제 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어와 몸통
using ProjectEpsilon.World; // 실제 청크와 맵 데이터
using UnityEditor; // Unity 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedMapLongBodyTraversalVerification // 실제 긴 몸통 통과 시험
    { // 영역 시작
        private const string Key = "EpsilonMapLongBodyTraversalVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 상태
        private static double started; // 시험 시작 시각
        static AutomatedMapLongBodyTraversalVerification() // 도메인 재시작 처리
        { // 영역 시작
            if (SessionState.GetBool(Key, false)) // 진행 상태 확인
            { // 영역 시작
                EditorApplication.update += Tick; // 플레이 감시 연결
            } // 영역 종료
        } // 영역 종료
        public static void Run() // 실제 씬 시험 진입
        { // 영역 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(Key, true); // 시험 진행 기록
            EditorApplication.update -= Tick; // 중복 감시 제거
            EditorApplication.update += Tick; // 플레이 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        } // 영역 종료
        private static void Tick() // 실제 초기화 감시
        { // 영역 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 영역 시작
                return; // 플레이 대기
            } // 영역 종료
            if (started <= 0d) // 최초 시작 시각 확인
            { // 영역 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
            } // 영역 종료
            if (EditorApplication.timeSinceStartup - started > 180d) // 실제 제한 시간 확인
            { // 영역 시작
                Finish(1, "Timed out."); // 시간 초과 기록
                return; // 시험 중단
            } // 영역 종료
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 시작 선택과 실행 상태 확인
            { // 영역 시작
                return; // 시작 또는 결과 대기
            } // 영역 종료
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 전투 초기화 확인
            { // 영역 시작
                return; // 초기화 대기
            } // 영역 종료
            injected = true; // 실행 중복 방지
            player.gameObject.AddComponent<MapLongBodyTraversalRunner>(); // 실제 통과 시험 실행
        } // 영역 종료
        public static void Finish(int code, string message) // 실제 시험 종료
        { // 영역 시작
            SessionState.SetBool(Key, false); // 시험 진행 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[MapLongBodyTraversalVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        } // 영역 종료
    } // 영역 종료

    public sealed class MapLongBodyTraversalRunner : MonoBehaviour // 긴 몸통 실제 이동 실행기
    { // 영역 시작
        private IEnumerator Start() // 세 맵의 실제 통로 이동
        { // 영역 시작
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 머리 이동
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakePathRecorder path = GetComponent<SnakePathRecorder>(); // 실제 몸통 경로 기록기
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            Require(player != null && health != null && path != null && self != null && body != null && follower != null && spawner != null && weapons != null, "Long-body traversal dependencies missing."); // 실제 시험 구성 확인
            player.enabled = false; // 시험 준비 중 이동 정지
            self.enabled = false; // 시험 준비 중 자기 충돌 정지
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 자동 공격 중지
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 기존 적 순회
            { // 영역 시작
                enemy.gameObject.SetActive(false); // 통제 시험의 적 접촉 제거
                Destroy(enemy.gameObject); // 기존 적 정리
            } // 영역 종료
            PrototypeChunkMap previous = UnityEngine.Object.FindFirstObjectByType<PrototypeChunkMap>(); // 기존 게임 맵
            if (previous != null) // 기존 맵 존재 확인
            { // 영역 시작
                previous.gameObject.SetActive(false); // 기존 맵의 장애물과 효과 중지
            } // 영역 종료
            while (body.CurrentBodyCount < 20) // 실제 절대 상한까지 성장
            { // 영역 시작
                Require(body.TryGainBodyFromLevelUp(), "Could not prepare twenty body segments."); // 실제 몸통 성장 확인
            } // 영역 종료
            string[] names = { "Plains", "Ruins", "Swamp" }; // 실제 세 맵 이름
            for (int index = 0; index < names.Length; index++) // 실제 맵 순회
            { // 영역 시작
                PrototypeMapProfile source = AssetDatabase.LoadAssetAtPath<PrototypeMapProfile>("Assets/Data/World/PrototypeMap" + names[index] + ".asset"); // 실제 맵 설정
                Require(source != null, "Map profile missing: " + names[index]); // 실제 맵 데이터 확인
                PrototypeMapProfile profile = Instantiate(source); // 원본을 보존한 설정 복사
                typeof(PrototypeMapProfile).GetField("obstacleChance", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(profile, 1f); // 모든 청크의 장애물 생성
                typeof(PrototypeMapProfile).GetField("obstacleMinimumCount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(profile, 3); // 세 장애물 고정
                typeof(PrototypeMapProfile).GetField("obstacleMaximumCount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(profile, 3); // 세 장애물 고정
                typeof(PrototypeMapProfile).GetField("specialZoneChance", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(profile, 0f); // 지형 효과와 분리
                GameObject owner = new GameObject("LongBodyMapFixture"); // 실제 시험 맵 객체
                PrototypeChunkMap map = owner.AddComponent<PrototypeChunkMap>(); // 실제 청크 관리자
                Require(map.ApplyProfile(profile), "Could not apply map: " + names[index]); // 맵 설정 연결 확인
                UnityEngine.Random.InitState(6701 + index); // 맵별 재현 난수
                player.enabled = false; // 위치 설정 중 이동 정지
                self.enabled = false; // 위치 설정 중 자기 충돌 정지
                player.transform.position = new Vector3(12f, -14f, 0f); // 실제 중앙 통로 진입 위치
                player.transform.rotation = Quaternion.identity; // 실제 위쪽 전진 방향
                health.ResetHealth(); // 실제 공용 체력 복구
                path.ResetHistory(); // 현재 위치의 실제 이동 경로 초기화
                follower.SnapToPath(); // 스무 몸통을 실제 초기 경로에 배치
                Physics2D.SyncTransforms(); // 실제 물리 위치 동기화
                yield return null; // 실제 청크 생성 대기
                Require(map.ActiveCount == 9, "Actual chunks did not initialize: " + names[index]); // 실제 청크 생성 확인
                Require(body.CurrentBodyCount == 20 && Vector2.Distance(body.BodySegments[0].transform.position, body.BodySegments[19].transform.position) > 8f, "Actual body train was not arranged along the path."); // 실제 긴 몸통 연결 확인
                self.enabled = true; // 실제 자기 충돌 복구
                player.enabled = true; // 실제 자동 전진 시작
                int initialHealth = health.CurrentHealth; // 실제 이동 전 체력
                while (player.transform.position.y < 10.8f) // 실제 중앙 교차로까지 전진
                { // 영역 시작
                    Require(GameManager.Instance.IsPlaying && body.CurrentBodyCount == 20 && health.CurrentHealth == initialHealth, "Long body collided before the turn: " + names[index]); // 직진 중 실제 충돌 확인
                    yield return null; // 실제 이동 프레임 진행
                } // 영역 종료
                Quaternion target = Quaternion.Euler(0f, 0f, -90f); // 실제 우회전 목표 방향
                while (Quaternion.Angle(player.transform.rotation, target) > 0.1f) // 실제 속도의 직각 회전
                { // 영역 시작
                    player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, target, player.TurnSpeed * Time.deltaTime); // 기존 회전 속도의 통제 입력
                    Require(GameManager.Instance.IsPlaying && body.CurrentBodyCount == 20 && health.CurrentHealth == initialHealth, "Long body collided while turning: " + names[index]); // 회전 중 실제 충돌 확인
                    yield return null; // 실제 이동 프레임 진행
                } // 영역 종료
                while (player.transform.position.x < 27f) // 뒷몸통까지 교차로를 벗어나는 이동
                { // 영역 시작
                    Require(GameManager.Instance.IsPlaying && body.CurrentBodyCount == 20 && health.CurrentHealth == initialHealth, "Long body collided while exiting: " + names[index]); // 탈출 중 실제 충돌 확인
                    yield return null; // 실제 이동 프레임 진행
                } // 영역 종료
                Require(body.BodySegments[19].transform.position.x > 13f && Mathf.Abs(body.BodySegments[19].transform.position.y - 12f) < 2f, "Last body did not follow the actual turn: " + names[index]); // 마지막 몸통의 실제 회전과 탈출 확인
                Debug.Log($"[MapLongBodyTraversalVerification] {names[index]}: twenty connected segments completed a controlled ninety-degree turn and exit."); // 맵별 실제 결과 기록
                player.enabled = false; // 다음 맵 준비 중 이동 정지
                self.enabled = false; // 다음 맵 준비 중 자기 충돌 정지
                owner.SetActive(false); // 이전 맵 충돌 차단
                Destroy(owner); // 실제 시험 맵 제거
                Destroy(profile); // 실제 시험 데이터 제거
                yield return null; // 지연 제거 완료 대기
            } // 영역 종료
            AutomatedMapLongBodyTraversalVerification.Finish(0, "Twenty connected body segments traversed and exited one controlled turn on each actual map."); // 실제 전체 결과 기록
        } // 영역 종료
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 영역 시작
            if (!condition) // 실패 조건 확인
            { // 영역 시작
                AutomatedMapLongBodyTraversalVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 영역 종료
        } // 영역 종료
    } // 영역 종료
} // 영역 종료
