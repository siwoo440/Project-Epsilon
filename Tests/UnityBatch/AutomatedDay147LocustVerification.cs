using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 실제 위험 상태 조회
using ProjectEpsilon.Combat; // 실제 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어와 체력
using ProjectEpsilon.World; // 실제 맵 위험
using UnityEditor; // Unity 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay147LocustVerification // 실제 황충 충돌 시험
    { // 시험 시작
        private const string Key = "EpsilonDay147Locust"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay147LocustVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 준비 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 실제 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(Key, true); // 시험 진행 기록
            EditorApplication.update += Tick; // 실제 전투 준비 감시
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 전투 시작 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 실행 방지
            new GameObject("Day147LocustRunner").AddComponent<Day147LocustRunner>(); // 실제 충돌 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 전투 감시 해제
            Debug.Log("[Day147Locust] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 시험 결과
            EditorApplication.Exit(passed ? 0 : 1); // 실제 배치 종료
        } // 함수 종료
    } // 시험 종료
    public sealed class Day147LocustRunner : MonoBehaviour // 실제 황충 충돌 실행기
    { // 실행기 시작
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 상태 조회
        private IEnumerator Start() // 실제 황충 예고와 이동 검사
        { // 함수 시작
            PrototypeChunkMap map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 맵
            WorldEventController events = map == null ? null : map.GetComponent<WorldEventController>(); // 실제 황충 이벤트
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 머리
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 플레이어 몸통
            SnakeHealth health = player == null ? null : player.GetComponent<SnakeHealth>(); // 실제 공용 체력
            Require(events != null && map.Profile != null && player != null && body != null && health != null, "황충 시험 구성 누락"); // 실제 시험 구성 확인
            player.enabled = false; // 시험 중 자동 이동 방지
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 방지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            foreach (Collider2D collider in body.GetComponentsInChildren<Collider2D>()) // 실제 몸통 충돌체 순회
            { // 반복 시작
                collider.enabled = false; // 머리 접촉만 시험
            } // 반복 종료
            foreach (PrototypeSpecialZone zone in FindObjectsByType<PrototypeSpecialZone>(FindObjectsSortMode.None)) // 기존 특수 지역 순회
            { // 반복 시작
                zone.gameObject.SetActive(false); // 기존 지형 피해 차단
            } // 반복 종료
            foreach (SnakeContactHazard obstacle in FindObjectsByType<SnakeContactHazard>(FindObjectsSortMode.None)) // 기존 장애물 순회
            { // 반복 시작
                obstacle.gameObject.SetActive(false); // 기존 장애물 피해 차단
            } // 반복 종료
            Set(map.Profile, "worldEventWarning", 0.2f); // 짧은 시험용 경고
            Set(map.Profile, "locustSpeed", 30f); // 빠른 시험용 황충
            Set(map.Profile, "locustWaveGap", 0.2f); // 짧은 시험용 대열 간격
            health.ResetHealth(); // 실제 체력 복구
            UnityEngine.Random.State randomState = UnityEngine.Random.state; // 기존 무작위 상태 보관
            int seen = 0; // 실제 추첨한 대열 수의 비트 목록
            for (int seed = 1; seed <= 16; seed++) // 재현 가능한 대열 추첨 순회
            { // 반복 시작
                UnityEngine.Random.InitState(seed); // 실제 대열 추첨 상태 설정
                Require(events.TryBeginLocust(), "대열 무작위 시험의 황충 시작 실패"); // 실제 이벤트 시작 확인
                int count = (int)Get(events, "locustWaveCount"); // 실제 무작위 대열 수
                Require(count >= 2 && count <= 5, "황충 대열 무작위 범위 오류"); // 실제 추첨 범위 확인
                seen |= 1 << count; // 실제 추첨 결과 기록
                typeof(WorldEventController).GetMethod("ClearEvent", Private).Invoke(events, null); // 다음 추첨 전 이벤트 정리
            } // 반복 종료
            UnityEngine.Random.state = randomState; // 기존 무작위 상태 복구
            Require((seen & (seen - 1)) != 0, "황충 대열 수가 매번 동일함"); // 실제 무작위 변동 확인
            Vector3 original = player.transform.position; // 실제 경고 중심 보관
            Require(events.TryBeginLocust(), "황충 경고 시작 실패"); // 실제 황충 경고 확인
            int chosen = (int)Get(events, "locustWaveCount"); // 실제 추첨 대열 수
            Require(chosen >= 2 && chosen <= 5, "황충 대열 무작위 범위 오류"); // 실제 무작위 범위 확인
            Set(events, "locustWaveCount", 2); // 시험용 두 대열 고정
            player.transform.position = original + Vector3.up * 60f; // 시험 준비 중 충돌 범위 이탈
            Physics2D.SyncTransforms(); // 실제 머리 위치 반영
            yield return new WaitForSeconds(0.3f); // 실제 예고 완료 대기
            Transform swarm = events.transform.Find("WorldHazardVisual/LocustSwarm"); // 실제 이동 황충 묶음
            Require(swarm != null && events.IsActive, "황충 대열 시작 실패"); // 실제 황충 이동 확인
            Vector2[] centers = (Vector2[])typeof(WorldEventController).GetField("LocustCenters", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null); // 원본 황충 몸체 중심
            SpriteRenderer[] renderers = swarm.GetComponentsInChildren<SpriteRenderer>(); // 실제 황충 그림 목록
            Collider2D head = player.GetComponent<Collider2D>(); // 실제 머리 충돌체
            Require(head != null && renderers.Length == 6 && centers.Length > 1, "실제 황충 그림 또는 머리 충돌체 누락"); // 실제 판정 구성 확인
            Vector3 direction = (Vector3)Get(events, "direction"); // 실제 황충 이동 방향
            Vector3 side = new Vector3(-direction.y, direction.x, 0f); // 실제 이동 방향의 수직축
            Vector3 gap = Vector3.zero; // 실제 벌레 사이의 빈 위치
            bool found = false; // 빈 위치 발견 여부
            float headRadius = head.bounds.extents.magnitude; // 실제 머리 충돌 여유
            for (float forward = -7f; forward <= 2f && !found; forward += 0.2f) // 기존 넓은 판정의 이동 방향 탐색
            { // 반복 시작
                for (float lateral = -3.5f; lateral <= 3.5f && !found; lateral += 0.2f) // 기존 넓은 판정의 옆 방향 탐색
                { // 반복 시작
                    Vector3 candidate = swarm.position + direction * forward + side * lateral; // 실제 경고 구역 안의 후보
                    bool clear = true; // 모든 황충과 떨어진 상태
                    foreach (SpriteRenderer renderer in renderers) // 각 황충 그림 순회
                    { // 반복 시작
                        float radius = renderer.sprite.bounds.size.x * Mathf.Abs(renderer.transform.lossyScale.x) * (55f / 724f); // 실제 황충 몸체 반경
                        foreach (Vector2 center in centers) // 각 황충 몸체 순회
                        { // 반복 시작
                            if (Vector2.Distance(candidate, Center(renderer, center)) <= radius + headRadius + 0.05f) // 머리와 황충의 겹침 확인
                            { // 조건 시작
                                clear = false; // 실제 벌레 위치 제외
                                break; // 현재 황충 그림 순회 중단
                            } // 조건 종료
                        } // 반복 종료
                        if (!clear) // 실제 벌레와 가까운 후보 확인
                        { // 조건 시작
                            break; // 현재 후보 검사 중단
                        } // 조건 종료
                    } // 반복 종료
                    if (clear) // 실제 벌레 사이 빈 공간 확인
                    { // 조건 시작
                        gap = candidate; // 실제 빈 공간 보관
                        found = true; // 빈 위치 발견
                    } // 조건 종료
                } // 반복 종료
            } // 반복 종료
            Require(found, "기존 위험 영역 안의 황충 빈 공간을 찾지 못함"); // 실제 빈 공간 확보
            player.transform.position = gap; // 실제 벌레 사이에 머리 배치
            Physics2D.SyncTransforms(); // 실제 충돌 위치 반영
            MethodInfo query = typeof(WorldEventController).GetMethod("QueueLocustDamage", Private); // 실제 개별 황충 충돌 검사
            query.Invoke(events, new object[] { swarm.position, swarm.position }); // 정지한 황충 사이의 접촉 검사
            Require(!(bool)Get(events, "damageQueued"), "벌레가 없는 경고 구역에 피해 예약"); // 실제 빈 공간 무피해 확인
            player.transform.position = Center(renderers[0], centers[0]); // 실제 황충 몸체에 머리 배치
            Physics2D.SyncTransforms(); // 실제 충돌 위치 반영
            query.Invoke(events, new object[] { swarm.position, swarm.position }); // 실제 벌레 몸체 접촉 검사
            Require((bool)Get(events, "damageQueued"), "실제 황충 몸체의 피해 누락"); // 실제 벌레 충돌 확인
            int beforeDamage = health.CurrentHealth; // 실제 충돌 전 체력
            player.transform.position = original + Vector3.up * 60f; // 이후 대열의 시험 간섭 방지
            Physics2D.SyncTransforms(); // 실제 머리 위치 반영
            yield return new WaitForFixedUpdate(); // 기존 충돌 우선순위 처리 대기
            yield return null; // 실제 체력 피해 반영 대기
            Require(health.CurrentHealth == beforeDamage - map.Profile.LocustDamage, "실제 황충 접촉의 체력 피해 누락"); // 실제 공용 체력 피해 확인
            float deadline = Time.realtimeSinceStartup + 10f; // 실제 반복 대열 제한 시간
            while ((int)Get(events, "locustWaveIndex") < 1 && Time.realtimeSinceStartup < deadline) // 첫 대열 통과 대기
            { // 반복 시작
                yield return null; // 실제 물리 프레임 대기
            } // 반복 종료
            Require((int)Get(events, "locustWaveIndex") == 1 && events.IsActive, "첫 대열 이후 반복 대기 누락"); // 실제 대열 간격 확인
            while (events.IsActive && Time.realtimeSinceStartup < deadline) // 두 번째 대열 통과 대기
            { // 반복 시작
                yield return null; // 실제 물리 프레임 대기
            } // 반복 종료
            Require(!events.IsActive && (int)Get(events, "locustWaveIndex") == 2, "두 황충 대열의 통과 종료 오류"); // 실제 반복과 정리 확인
            AutomatedDay147LocustVerification.Finish(true, "Locust gaps, body contact and repeated waves passed."); // 실제 성공 기록
        } // 함수 종료
        private static Vector3 Center(SpriteRenderer renderer, Vector2 pixel) // 실제 그림의 황충 몸체 좌표
        { // 함수 시작
            Bounds bounds = renderer.sprite.bounds; // 실제 원본 그림 크기
            float x = (pixel.x / 724f - 0.5f) * bounds.size.x * (renderer.flipX ? -1f : 1f); // 좌우 반전된 몸체 좌표
            float y = (0.5f - pixel.y / 2172f) * bounds.size.y; // 위쪽 기준 몸체 좌표
            return renderer.transform.TransformPoint(bounds.center + new Vector3(x, y, 0f)); // 실제 월드 좌표 반환
        } // 함수 종료
        private static object Get(object target, string name) // 실제 필드 조회
        { // 함수 시작
            return target.GetType().GetField(name, Private).GetValue(target); // 실제 필드 반환
        } // 함수 종료
        private static void Set(object target, string name, object value) // 실제 시험 상태 설정
        { // 함수 시작
            target.GetType().GetField(name, Private).SetValue(target, value); // 실제 필드 변경
        } // 함수 종료
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 확인
            { // 조건 시작
                AutomatedDay147LocustVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 실행기 종료
} // 영역 종료
