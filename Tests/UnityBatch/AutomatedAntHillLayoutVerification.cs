using System; // 예외 형식
using System.Reflection; // 청크 생성 경로 조회
using ProjectEpsilon.Player; // 기존 장애물 판정
using ProjectEpsilon.World; // 실제 맵 구성
using UnityEditor; // 배치 실행
using UnityEngine; // 유니티 자료형

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    public static class AutomatedAntHillLayoutVerification // 개미굴 주변 장애물 시험
    { // 영역 시작
        public static void Run() // 실제 청크 생성 시험
        { // 영역 시작
            GameObject owner = null; // 임시 청크 관리자
            PrototypeMapProfile profile = null; // 임시 맵 데이터
            try // 실제 배치 검사
            { // 영역 시작
                PrototypeMapProfile source = AssetDatabase.LoadAssetAtPath<PrototypeMapProfile>("Assets/Data/World/PrototypeMapPlains.asset"); // 실제 평원 데이터
                Require(source != null, "Plains profile missing."); // 평원 데이터 확인
                profile = UnityEngine.Object.Instantiate(source); // 원본 보존 복사
                typeof(PrototypeMapProfile).GetField("obstacleChance", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(profile, 0f); // 기존 일반 장애물 제외
                typeof(PrototypeMapProfile).GetField("specialZoneChance", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(profile, 1f); // 특수 지역 생성 확정
                typeof(PrototypeMapProfile).GetField("decorationChance", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(profile, 0f); // 장식 제외
                owner = new GameObject("AntHillLayoutFixture"); // 실제 청크 관리자 객체
                PrototypeChunkMap map = owner.AddComponent<PrototypeChunkMap>(); // 실제 청크 생성기
                Require(map.ApplyProfile(profile), "Could not apply plains profile."); // 실제 평원 연결
                MethodInfo populate = typeof(PrototypeChunkMap).GetMethod("PopulateChunk", BindingFlags.NonPublic | BindingFlags.Instance); // 실제 청크 생성 경로
                Require(populate != null, "Chunk population missing."); // 실제 생성기 확인
                int seen = 0; // 개미굴 표본 수
                int sampleSeed = 0; // 보호 시험용 개미굴 시드
                Vector3 sampleMound = Vector3.zero; // 보호 시험용 흙무더기 위치
                for (int seed = 1; seed <= 128; seed++) // 여러 무작위 배치 확인
                { // 영역 시작
                    UnityEngine.Random.InitState(seed); // 재현 가능한 난수
                    GameObject chunk = new GameObject("AntHillChunk"); // 실제 시험 청크
                    chunk.transform.SetParent(owner.transform, false); // 맵 아래 배치
                    populate.Invoke(map, new object[] { chunk.transform }); // 실제 청크 생성
                    PrototypeSpecialZone[] zones = chunk.GetComponentsInChildren<PrototypeSpecialZone>(); // 실제 특수 지역
                    if (zones.Length > 0 && zones[0].Kind == PrototypeZoneKind.AntHill) // 개미굴 생성 확인
                    { // 영역 시작
                        seen++; // 개미굴 표본 증가
                        SnakeContactHazard[] hazards = chunk.GetComponentsInChildren<SnakeContactHazard>(); // 실제 충돌 장애물
                        Require(hazards.Length >= 1, "Ant hill has no surrounding mound obstacle."); // 개미굴 주변 흙무더기 확인
                        bool found = false; // 안전한 흙무더기 확인값
                        foreach (SnakeContactHazard hazard in hazards) // 실제 장애물 순회
                        { // 영역 시작
                            SpriteRenderer visual = hazard.GetComponent<SpriteRenderer>(); // 실제 장애물 이미지
                            BoxCollider2D collider = hazard.GetComponent<BoxCollider2D>(); // 실제 충돌 범위
                            if (visual == null || visual.sprite == null || visual.sprite.name != "개미굴_흙무더기") // 흙무더기 이미지 확인
                            { // 영역 시작
                                continue; // 다른 장애물 제외
                            } // 영역 종료
                            Require(collider != null && collider.isTrigger && hazard.HazardType == SnakeContactHazardType.Obstacle, "Mound lacks the existing obstacle collision."); // 기존 장애물 판정 확인
                            Vector3 local = hazard.transform.localPosition; // 청크 내 위치
                            float halfX = collider.size.x * 0.5f; // 충돌체 가로 반폭
                            float halfY = collider.size.y * 0.5f; // 충돌체 세로 반폭
                            float corridor = profile.SafeCorridorWidth * 0.5f; // 보호 통로 반폭
                            Require(local.x - halfX >= corridor && local.y - halfY >= corridor && local.x + halfX <= map.ChunkSize - corridor && local.y + halfY <= map.ChunkSize - corridor, "Mound entered the boundary corridor."); // 청크 경계 통로 확인
                            Require(Mathf.Abs(local.x - map.ChunkSize * 0.5f) >= corridor + halfX && Mathf.Abs(local.y - map.ChunkSize * 0.5f) >= corridor + halfY, "Mound blocked the central corridor."); // 중앙 연결 통로 확인
                            Require(Vector2.Distance(zones[0].transform.position, hazard.transform.position) >= zones[0].Radius + 1.2f, "Mound blocked the ant hill entrance."); // 개미굴 입구 간격 확인
                            if (sampleSeed == 0) // 첫 안전한 개미굴 표본 확인
                            { // 영역 시작
                                sampleSeed = seed; // 재현용 시드 보관
                                sampleMound = hazard.transform.position; // 실제 흙무더기 위치 보관
                            } // 영역 종료
                            found = true; // 실제 흙무더기 확인
                        } // 영역 종료
                        Require(found, "Ant hill mound artwork missing."); // 실제 흙무더기 표시 확인
                    } // 영역 종료
                    UnityEngine.Object.DestroyImmediate(chunk); // 시험 청크 제거
                } // 영역 종료
                Require(seen >= 5, "Too few ant hill samples."); // 충분한 난수 표본 확인
                GameObject head = new GameObject("ProtectedHead"); // 실제 머리 보호 시험 객체
                SnakeMovement movement = head.AddComponent<SnakeMovement>(); // 실제 플레이어 이동 구성
                head.transform.position = sampleMound; // 흙무더기 후보의 실제 머리
                typeof(PrototypeChunkMap).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(map, movement); // 실제 청크 관리자의 머리 연결
                UnityEngine.Random.InitState(sampleSeed); // 동일한 개미굴 후보 재현
                GameObject protectedChunk = new GameObject("ProtectedAntHillChunk"); // 머리 보호 시험 청크
                protectedChunk.transform.SetParent(owner.transform, false); // 실제 청크 연결
                populate.Invoke(map, new object[] { protectedChunk.transform }); // 실제 보호 배치 실행
                Require(protectedChunk.GetComponentsInChildren<PrototypeSpecialZone>().Length == 1 && protectedChunk.GetComponentsInChildren<SnakeContactHazard>().Length == 0, "Ant hill mound spawned over the player."); // 머리 위 장애물 생성 차단 확인
                UnityEngine.Object.DestroyImmediate(protectedChunk); // 보호 시험 청크 제거
                typeof(PrototypeChunkMap).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(map, null); // 머리 보호 시험 연결 해제
                UnityEngine.Object.DestroyImmediate(head); // 시험 머리 제거
                UnityEngine.Random.InitState(sampleSeed); // 동일한 개미굴 후보 재현
                GameObject occupiedChunk = new GameObject("OccupiedAntHillChunk"); // 장애물 중복 시험 청크
                occupiedChunk.transform.SetParent(owner.transform, false); // 실제 청크 연결
                GameObject prior = new GameObject("ExistingObstacle"); // 실제 기존 장애물
                prior.transform.SetParent(occupiedChunk.transform, false); // 시험 청크 아래 연결
                prior.transform.position = sampleMound; // 흙무더기 후보의 장애물 위치
                prior.AddComponent<SnakeContactHazard>().Configure(SnakeContactHazardType.Obstacle); // 기존 장애물 판정 구성
                populate.Invoke(map, new object[] { occupiedChunk.transform }); // 실제 중복 방지 배치 실행
                Require(occupiedChunk.GetComponentsInChildren<PrototypeSpecialZone>().Length == 1 && occupiedChunk.GetComponentsInChildren<SnakeContactHazard>().Length == 1, "Ant hill mound overlapped an existing obstacle."); // 기존 장애물 중복 차단 확인
                UnityEngine.Object.DestroyImmediate(occupiedChunk); // 중복 시험 청크 제거
                Debug.Log("[AntHillLayoutVerification] PASS: " + seen + " ant hills have safe surrounding obstacle mounds."); // 실제 시험 결과
                EditorApplication.Exit(0); // 성공 종료
            } // 영역 종료
            catch (Exception exception) // 실제 실패 처리
            { // 영역 시작
                Debug.LogError("[AntHillLayoutVerification] FAIL: " + exception); // 실패 원인 기록
                EditorApplication.Exit(1); // 실패 종료
            } // 영역 종료
            finally // 시험 객체 정리
            { // 영역 시작
                if (owner != null) // 시험 맵 존재 확인
                { // 영역 시작
                    UnityEngine.Object.DestroyImmediate(owner); // 시험 맵 정리
                } // 영역 종료
                if (profile != null) // 시험 데이터 존재 확인
                { // 영역 시작
                    UnityEngine.Object.DestroyImmediate(profile); // 시험 데이터 정리
                } // 영역 종료
            } // 영역 종료
        } // 영역 종료
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 영역 시작
            if (!condition) // 실패 조건 확인
            { // 영역 시작
                throw new InvalidOperationException(message); // 실제 실패 전달
            } // 영역 종료
        } // 영역 종료
    } // 영역 종료
} // 영역 종료
