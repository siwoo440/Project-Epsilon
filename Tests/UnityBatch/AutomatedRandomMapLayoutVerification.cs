using System; // 예외 처리
using System.Collections.Generic; // 배치 결과 집합
using System.Reflection; // 실제 청크 내부 경로 조회
using ProjectEpsilon.Player; // 실제 장애물 판정
using ProjectEpsilon.World; // 실제 맵과 청크
using UnityEditor; // Unity 배치 실행
using UnityEngine; // Unity 물리와 난수

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    public static class AutomatedRandomMapLayoutVerification // 실제 무작위 지형 검증
    {
        public static void Run() // 실제 Unity 배치 진입
        {
            GameObject owner = null; // 시험 맵 객체 초기화
            PrototypeMapProfile profile = null; // 시험 맵 데이터 초기화
            try // 실제 여러 시드의 배치 검증
            {
                string[] mapNames = { "Plains", "Ruins", "Swamp" }; // 실제 세 맵 데이터 이름
                foreach (string mapName in mapNames) // 각 맵별 이미지 설정 확인
                {
                    PrototypeMapProfile candidate = AssetDatabase.LoadAssetAtPath<PrototypeMapProfile>("Assets/Data/World/PrototypeMap" + mapName + ".asset"); // 실제 맵 데이터 조회
                    Require(candidate != null, "Actual map profile missing: " + mapName); // 실제 맵 데이터 확인
                    string folder = "Assets/Resources/MapElements/" + mapName; // 해당 맵 이미지 경로
                    Require(AssetDatabase.FindAssets("t:Sprite", new[] { folder }).Length == 11, "Map sprite count mismatch: " + mapName); // 맵별 이미지 수 확인
                }
                foreach (string mapName in mapNames) // 실제 세 맵의 무작위 배치 검증
                {
                    PrototypeMapProfile source = AssetDatabase.LoadAssetAtPath<PrototypeMapProfile>("Assets/Data/World/PrototypeMap" + mapName + ".asset"); // 실제 평원 데이터 조회
                    Require(source != null, "Actual map profile missing: " + mapName); // 실제 맵 데이터 확인
                    profile = UnityEngine.Object.Instantiate(source); // 원본을 보존한 설정 복사
                    typeof(PrototypeMapProfile).GetField("obstacleChance", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(profile, 1f); // 모든 시험 청크에 장애물 허용
                    typeof(PrototypeMapProfile).GetField("specialZoneChance", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(profile, 0f); // 지역 효과와 분리
                    FieldInfo minimum = typeof(PrototypeMapProfile).GetField("obstacleMinimumCount", BindingFlags.NonPublic | BindingFlags.Instance); // 맵별 최소 장애물 수
                    FieldInfo maximum = typeof(PrototypeMapProfile).GetField("obstacleMaximumCount", BindingFlags.NonPublic | BindingFlags.Instance); // 맵별 최대 장애물 수
                    FieldInfo corridor = typeof(PrototypeMapProfile).GetField("safeCorridorWidth", BindingFlags.NonPublic | BindingFlags.Instance); // 맵별 안전 통로 폭
                    Require(minimum != null && maximum != null && corridor != null, "Adjustable obstacle counts or safe corridor width missing."); // 실제 조정 가능한 데이터 확인
                    minimum.SetValue(profile, 2); // 고정 두 장애물의 데이터 경계
                    maximum.SetValue(profile, 2); // 고정 두 장애물의 데이터 경계
                    corridor.SetValue(profile, 6f); // 넓은 안전 통로 시험값
                    owner = new GameObject("RandomLayoutFixture"); // 실제 청크 관리자 객체
                    PrototypeChunkMap map = owner.AddComponent<PrototypeChunkMap>(); // 실제 청크 관리자
                    Require(map.ApplyProfile(profile), "Could not apply actual map profile."); // 실제 설정 적용 확인
                    MethodInfo populate = typeof(PrototypeChunkMap).GetMethod("PopulateChunk", BindingFlags.NonPublic | BindingFlags.Instance); // 실제 청크 배치 경로
                    Require(populate != null, "Actual chunk population missing."); // 실제 배치 경로 확인
                    HashSet<string> layouts = new HashSet<string>(); // 서로 다른 실제 배치 목록
                    int coverCount = 0; // 실제 엄폐 장애물 수
                    int ordinaryCount = 0; // 실제 일반 장애물 수
                    for (int seed = 1; seed <= 32; seed++) // 여러 재현 가능한 난수 시드
                    {
                        UnityEngine.Random.InitState(seed); // 실제 Unity 난수 시드
                        GameObject chunk = new GameObject("LayoutChunk"); // 실제 시험 청크
                        chunk.transform.SetParent(owner.transform, false); // 실제 맵 아래 연결
                        populate.Invoke(map, new object[] { chunk.transform }); // 실제 청크 배치 실행
                        SnakeContactHazard[] hazards = chunk.GetComponentsInChildren<SnakeContactHazard>(); // 실제 장애물 판정 목록
                        Require(hazards.Length == 2, "Configured obstacle count was not applied."); // 장애물 생성 확인
                        string signature = string.Empty; // 실제 위치 서명 초기화
                        foreach (SnakeContactHazard hazard in hazards) // 실제 장애물 순회
                        {
                            BoxCollider2D collider = hazard.GetComponent<BoxCollider2D>(); // 실제 장애물 충돌 범위
                            Require(collider != null && collider.isTrigger && hazard.HazardType == SnakeContactHazardType.Obstacle, "Actual obstacle collision setup missing."); // 기존 충돌 처리 확인
                            SpriteRenderer renderer = hazard.GetComponent<SpriteRenderer>(); // 실제 장애물 표시기
                            Require(renderer != null && renderer.sprite != null && renderer.sprite.texture != Texture2D.whiteTexture && AssetDatabase.GetAssetPath(renderer.sprite).StartsWith("Assets/Resources/MapElements/" + mapName + "/", StringComparison.Ordinal) && renderer.sortingOrder < 8, "Map-specific obstacle artwork missing or above enemies."); // 실제 맵 이미지와 표시 순서 확인
                            bool expectedCover = renderer.sprite.name == "바위_엄폐" || renderer.sprite.name == "엄폐_잔해"; // 실제 기획상 엄폐 이미지 확인
                            Require(hazard.BlocksProjectiles == expectedCover, "Cover collision flag differs from map artwork."); // 엄폐와 일반 장애물 구분 확인
                            if (expectedCover) // 실제 엄폐 장애물 확인
                            {
                                coverCount++; // 엄폐 장애물 수 증가
                            }
                            else // 일반 장애물 확인
                            {
                                ordinaryCount++; // 일반 장애물 수 증가
                            }
                            Vector3 local = hazard.transform.localPosition; // 실제 청크 내 위치
                            Vector3 scale = hazard.transform.localScale; // 실제 충돌 크기 배율
                            float halfX = Mathf.Abs(collider.size.x * scale.x) * 0.5f; // 실제 충돌체 가로 반폭
                            float halfY = Mathf.Abs(collider.size.y * scale.y) * 0.5f; // 실제 충돌체 세로 반폭
                            signature += local.x.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "," + local.y.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + ";"; // 실제 위치 기록
                            Require(local.x - halfX >= 3f && local.y - halfY >= 3f && local.x + halfX <= map.ChunkSize - 3f && local.y + halfY <= map.ChunkSize - 3f, "Obstacle entered a protected chunk boundary."); // 인접 청크 연결 통로 확인
                            Require(Mathf.Abs(local.x - map.ChunkSize * 0.5f) >= 3f + halfX && Mathf.Abs(local.y - map.ChunkSize * 0.5f) >= 3f + halfY, "Obstacle blocked a four-unit cross corridor."); // 중앙의 연결된 회전 통로 확인
                        }
                        layouts.Add(signature); // 실제 배치 결과 저장
                        UnityEngine.Object.DestroyImmediate(chunk); // 시험 청크 제거
                    }
                    Require(layouts.Count > 1, "Different random seeds produced only a fixed obstacle pattern."); // 실제 배치 무작위성 확인
                    Require(ordinaryCount > 0 && (mapName == "Swamp" || coverCount > 0), "Expected cover or ordinary obstacles did not appear."); // 각 맵의 엄폐와 일반 장애물 표본 확인
                    GameObject playerObject = new GameObject("StartingHead"); // 실제 플레이어 보호 시험 객체
                    SnakeMovement player = playerObject.AddComponent<SnakeMovement>(); // 실제 플레이어 이동 구성
                    player.transform.position = new Vector3(6f, 6f, 0f); // 장애물 후보 주변 머리 위치
                    typeof(PrototypeChunkMap).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(map, player); // 실제 청크 관리자에 머리 연결
                    UnityEngine.Random.InitState(9); // 재현 가능한 보호 시험 시드
                    GameObject protectedChunk = new GameObject("ProtectedChunk"); // 머리 주변 시험 청크
                    protectedChunk.transform.SetParent(owner.transform, false); // 실제 맵 아래 연결
                    populate.Invoke(map, new object[] { protectedChunk.transform }); // 실제 머리 보호 배치 실행
                    foreach (SnakeContactHazard hazard in protectedChunk.GetComponentsInChildren<SnakeContactHazard>()) // 실제 생성 장애물 순회
                    {
                        Require(Vector2.Distance(player.transform.position, hazard.transform.position) >= 5.5f, "Obstacle spawned inside head protection radius."); // 실제 머리 안전 거리 확인
                    }
                    UnityEngine.Object.DestroyImmediate(playerObject); // 시험 머리 제거
                    UnityEngine.Object.DestroyImmediate(owner); // 시험 맵 제거
                    UnityEngine.Object.DestroyImmediate(profile); // 시험 설정 제거
                }
                Debug.Log("[RandomMapLayoutVerification] PASS: 32 seeds per map honored adjustable counts, connected six-unit corridors, map artwork and player protection."); // 실제 성공 기록
                EditorApplication.Exit(0); // 성공 종료
            }
            catch (Exception exception) // 실패 상황 확인
            {
                Debug.LogError("[RandomMapLayoutVerification] FAIL: " + exception); // 실제 실패 기록
                if (owner != null) // 시험 맵 존재 확인
                {
                    UnityEngine.Object.DestroyImmediate(owner); // 시험 맵 정리
                }
                if (profile != null) // 시험 설정 존재 확인
                {
                    UnityEngine.Object.DestroyImmediate(profile); // 시험 설정 정리
                }
                EditorApplication.Exit(1); // 실패 종료
            }
        }

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                throw new InvalidOperationException(message); // 실패 원인 전달
            }
        }
    }
}
