using System; // 예외 형식
using System.Collections; // 코루틴 형식
using ProjectEpsilon.Combat; // 실제 속성 효과
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어
using ProjectEpsilon.World; // 실제 맵과 지역
using System.Reflection; // 실제 생성 경로
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedMapFeatureGenerationVerification // 실제 신성과 암흑 시험
    {
        private const string Key = "EpsilonMapFeatureGenerationVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedMapFeatureGenerationVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(Key, false)) // 진행 상태 확인
            {
                EditorApplication.update += Tick; // 플레이 감시 연결
            }
        }
        public static void Run() // 실제 씬 시험 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 씬 열기
            SessionState.SetBool(Key, true); // 시험 진행 기록
            EditorApplication.update -= Tick; // 중복 감시 제거
            EditorApplication.update += Tick; // 플레이 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }
        private static void Tick() // 실제 초기화 감시
        {
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            {
                return; // 플레이 대기
            }
            if (started <= 0d) // 시험 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 시험 제한 시간 확인
            {
                Finish(1, "Timed out."); // 시간 초과 실패
                return; // 시험 중단
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 시작 선택과 실행 여부 확인
            {
                return; // 시작 또는 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            {
                return; // 초기화 대기
            }
            injected = true; // 실행 중복 방지
            player.gameObject.AddComponent<MapFeatureGenerationRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[MapFeatureGenerationVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class MapFeatureGenerationRunner : MonoBehaviour // 실제 신성 및 암흑 시험 실행기
    {
        private IEnumerator Start() // 실제 맵별 특수 지역 생성 확인
        {
            GetComponent<SnakeMovement>().enabled = false; // 시험 중 플레이어 이동 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            string[] maps = { "Plains", "Ruins", "Swamp" }; // 실제 세 맵 목록
            string[][] allowed = { new[] { "Wind", "Pollen", "Flower", "GemTrail", "AntHill" }, new[] { "ElectricFloor", "Supply", "Collapse" }, new[] { "Mud", "Bridge", "Purify", "ToxicSpore" } }; // 맵별 실제 지역 종류
            for (int index = 0; index < maps.Length; index++) // 실제 맵 순회
            {
                PrototypeMapProfile source = AssetDatabase.LoadAssetAtPath<PrototypeMapProfile>("Assets/Data/World/PrototypeMap" + maps[index] + ".asset"); // 실제 맵 데이터
                Require(source != null, "Map profile missing: " + maps[index]); // 실제 데이터 확인
                PrototypeMapProfile profile = UnityEngine.Object.Instantiate(source); // 실제 데이터의 시험 복사
                typeof(PrototypeMapProfile).GetField("obstacleChance", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(profile, 1f); // 장애물 생성 확정
                typeof(PrototypeMapProfile).GetField("specialZoneChance", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(profile, 1f); // 지역 생성 확정
                typeof(PrototypeMapProfile).GetField("decorationChance", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(profile, 1f); // 바닥 장식 생성 확정
                GameObject owner = new GameObject("FeatureMapFixture"); // 실제 맵 시험 객체
                PrototypeChunkMap map = owner.AddComponent<PrototypeChunkMap>(); // 기존 청크 관리자
                Require(map.ApplyProfile(profile), "Map profile could not be applied."); // 실제 맵 데이터 연결
                MethodInfo populate = typeof(PrototypeChunkMap).GetMethod("PopulateChunk", BindingFlags.NonPublic | BindingFlags.Instance); // 기존 지역 생성 경로
                Require(populate != null, "Actual chunk population missing."); // 실제 생성 경로 확인
                for (int seed = 1; seed <= 32; seed++) // 실제 여러 생성 시드
                {
                    UnityEngine.Random.InitState(seed); // 재현 가능한 실제 난수
                    GameObject chunk = new GameObject("FeatureChunk"); // 실제 시험 청크
                    chunk.transform.SetParent(owner.transform, false); // 기존 맵 아래 연결
                    populate.Invoke(map, new object[] { chunk.transform }); // 실제 청크 생성
                    Transform decoration = chunk.transform.Find("MapDecoration"); // 실제 맵 바닥 장식
                    Require(decoration != null && decoration.GetComponent<Collider2D>() == null && decoration.GetComponent<SpriteRenderer>() != null && decoration.GetComponent<SpriteRenderer>().sortingOrder < 3, "Map decoration must not collide or hide combat warnings."); // 비충돌과 낮은 표시 순서 확인
                    string decorationName = decoration.GetComponent<SpriteRenderer>().sprite.name; // 실제 바닥 장식 이미지 이름
                    if (decorationName == "얕은_수로" || decorationName == "징검돌_길") // 습지 안전 수로 확인
                    {
                        Require(Mathf.Abs(decoration.localPosition.x - map.ChunkSize * 0.5f) < 0.001f, "Swamp safe path was not placed on a protected corridor."); // 연결 안전 통로 위의 경로 표시 확인
                    }
                    PrototypeSpecialZone[] zones = chunk.GetComponentsInChildren<PrototypeSpecialZone>(); // 실제 생성 지역
                    Require(zones.Length >= 1 && Array.IndexOf(allowed[index], zones[0].Kind.ToString()) >= 0 && zones.Length == (zones[0].Kind == PrototypeZoneKind.ElectricFloor ? 2 : 1), "Unexpected map-specific zone: " + maps[index]); // 실제 맵별 종류 확인
                    if (zones[0].Kind == PrototypeZoneKind.ElectricFloor) // 폐허 장치 연결 확인
                    {
                        Require(zones[1].Kind == PrototypeZoneKind.CircuitSwitch && Vector2.Distance(zones[0].transform.position, zones[1].transform.position) <= profile.CircuitRange && Vector2.Distance(zones[0].transform.position, zones[1].transform.position) > zones[0].Radius + zones[1].Radius, "Electric floor switch is missing or unsafe."); // 장치 밖의 인접 스위치 확인
                    }
                    SpriteRenderer artwork = zones[0].GetComponentInChildren<SpriteRenderer>(); // 실제 지역 이미지
                    Require(artwork != null && artwork.sprite != null && AssetDatabase.GetAssetPath(artwork.sprite).StartsWith("Assets/Resources/MapElements/" + maps[index] + "/", StringComparison.Ordinal), "Map feature artwork missing: " + maps[index]); // 실제 맵별 이미지 확인
                    Vector3 local = zones[0].transform.localPosition; // 실제 청크 내 지역 위치
                    Require(Mathf.Abs(local.x - map.ChunkSize * 0.5f) >= zones[0].Radius + 2f && Mathf.Abs(local.y - map.ChunkSize * 0.5f) >= zones[0].Radius + 2f, "Map feature blocked a protected corridor."); // 중앙 안전 통로 보존 확인
                    foreach (SnakeContactHazard hazard in chunk.GetComponentsInChildren<SnakeContactHazard>()) // 실제 장애물 순회
                    {
                        Require(Vector2.Distance(zones[0].transform.position, hazard.transform.position) >= zones[0].Radius + 1.2f, "Map feature overlaps an obstacle."); // 장애물과 효과 분리 확인
                    }
                    chunk.SetActive(false); // 같은 프레임 재생성의 중복 차단
                    Destroy(chunk); // 시험 청크 제거
                }
                owner.SetActive(false); // 이전 맵 지역의 중복 차단
                Destroy(owner); // 시험 맵 제거
                Destroy(profile); // 시험 데이터 제거
            }
            yield return null; // 실제 지연 정리 대기
            AutomatedMapFeatureGenerationVerification.Finish(0, "Actual three-map features, paired switch, artwork, obstacle separation and safe corridors passed for 32 seeds."); // 실제 검증 성공 기록
        }
        private static WeaponTarget CreateTarget(string name, Vector3 position) // 실제 공격 대상 생성
        {
            GameObject item = new GameObject(name); // 통제 표적 객체
            item.transform.position = position; // 다른 적과 떨어진 표적 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(1000f); // 실제 표적 체력
            return target; // 실제 표적 반환
        }
        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedMapFeatureGenerationVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
