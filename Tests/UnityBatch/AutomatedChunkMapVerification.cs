using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 청크 구조 조회
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 출현 통제
using ProjectEpsilon.Player; // 플레이어 이동
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedChunkMapVerification // 실제 청크 맵 검증
    {
        private const string ActiveKey = "EpsilonChunkMapVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지
        private static double started; // 실제 시작 시각
        static AutomatedChunkMapVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 시험 진행 확인
            {
                EditorApplication.update += Tick; // 실제 시작 감시
            }
        }
        public static void Run() // 실제 씬 배치 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 실제 플레이 모드 시작
        }
        private static void Tick() // 실제 시작과 시간 제한 감시
        {
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            {
                return; // 실제 진입 대기
            }
            if (started <= 0d) // 시험 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 시간 제한
            {
                Finish(1, "Timed out waiting for chunk map test."); // 시간 초과 실패
                return; // 감시 종료
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작과 실행 확인
            {
                return; // 실제 시작 또는 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            {
                return; // 실제 초기화 대기
            }
            injected = true; // 실행기 중복 방지
            player.gameObject.AddComponent<ChunkMapVerificationRunner>(); // 실제 청크 시험 시작
        }
        public static void Finish(int code, string message) // 실제 결과 기록
        {
            if (finished) // 중복 종료 확인
            {
                return; // 중복 처리 방지
            }
            finished = true; // 종료 상태 저장
            SessionState.SetBool(ActiveKey, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 실제 감시 해제
            Debug.Log($"[ChunkMapVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 프로세스 종료
        }
    }

    public sealed class ChunkMapVerificationRunner : MonoBehaviour // 실제 청크 이동 시나리오
    {
        private IEnumerator Start() // 실제 생성과 제거 검증
        {
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동
            EarlyEnemyWaveSpawner wave = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 일반 출현기
            player.enabled = false; // 실제 플레이어 이동 통제
            wave.enabled = false; // 실제 일반 출현 통제
            Type mapType = typeof(GameManager).Assembly.GetType("ProjectEpsilon.World.PrototypeChunkMap"); // 실제 청크 맵 형식
            Component map = mapType == null ? null : UnityEngine.Object.FindFirstObjectByType(mapType) as Component; // 실제 씬 청크 맵
            Require(map != null, "Actual scene chunk map missing."); // 실제 맵 구성 확인
            PropertyInfo count = mapType.GetProperty("ActiveCount"); // 실제 활성 청크 수
            PropertyInfo size = mapType.GetProperty("ChunkSize"); // 실제 청크 크기
            MethodInfo getChunk = mapType.GetMethod("GetChunk"); // 실제 좌표별 청크 조회
            Require(count != null && size != null && getChunk != null, "Actual chunk map inspection missing."); // 실제 청크 조회 확인
            player.transform.position = Vector3.zero; // 실제 기준 위치 통제
            yield return null; // 실제 청크 갱신 대기
            Require((int)count.GetValue(map) == 9, "Actual initial active chunks were not 3 by 3."); // 실제 초기 청크 수 확인
            GameObject shared = getChunk.Invoke(map, new object[] { new Vector2Int(0, 0) }) as GameObject; // 실제 중앙 청크 조회
            GameObject departed = getChunk.Invoke(map, new object[] { new Vector2Int(-1, 0) }) as GameObject; // 실제 이탈 예정 청크
            float chunkSize = (float)size.GetValue(map); // 실제 설정 크기
            player.transform.position = new Vector3(chunkSize + 0.1f, 0f, 0f); // 실제 인접 청크 이동
            yield return null; // 실제 청크 갱신 대기
            Require((int)count.GetValue(map) == 9 && (GameObject)getChunk.Invoke(map, new object[] { new Vector2Int(0, 0) }) == shared, "Actual shared chunks were not retained."); // 실제 인접 청크 재사용 확인
            Require(getChunk.Invoke(map, new object[] { new Vector2Int(-1, 0) }) == null && (departed == null || !departed.activeSelf), "Actual departed chunk was not removed."); // 실제 이탈 청크 제거 확인
            GameManager.Instance.PauseGame(); // 실제 게임 일시정지
            player.transform.position = new Vector3(-chunkSize - 0.1f, 0f, 0f); // 정지 중 시험 위치 변경
            yield return null; // 실제 정지 프레임 진행
            Require((GameObject)getChunk.Invoke(map, new object[] { new Vector2Int(0, 0) }) == shared, "Actual chunks changed during pause."); // 실제 정지 중 유지 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            yield return null; // 실제 청크 갱신 대기
            Require((int)count.GetValue(map) == 9 && getChunk.Invoke(map, new object[] { new Vector2Int(-2, 0) }) != null && getChunk.Invoke(map, new object[] { new Vector2Int(0, 0) }) == null, "Actual negative chunk coordinates were incorrect."); // 실제 음수 좌표 확인
            player.transform.position = new Vector3(chunkSize * 5f, chunkSize * 5f, 0f); // 실제 먼 청크 이동
            yield return null; // 실제 청크 갱신 대기
            Require((int)count.GetValue(map) == 9 && getChunk.Invoke(map, new object[] { new Vector2Int(5, 5) }) != null, "Actual distant movement did not rebuild 3 by 3 chunks."); // 실제 먼 이동 확인
            AutomatedChunkMapVerification.Finish(0, "Actual 3 by 3 chunks, shared retention, pause and negative coordinates passed."); // 실제 전체 결과
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 실패 조건 확인
            {
                AutomatedChunkMapVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
