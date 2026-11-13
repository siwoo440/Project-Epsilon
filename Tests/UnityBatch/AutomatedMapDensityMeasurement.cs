using System; // 예외 형식
using System.Collections.Generic; // 지역 빈도 집계
using System.Reflection; // 청크 생성 경로
using ProjectEpsilon.Player; // 기존 장애물 판정
using ProjectEpsilon.World; // 실제 맵 구성
using UnityEditor; // 유니티 배치 실행
using UnityEngine; // 유니티 자료형

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    public static class AutomatedMapDensityMeasurement // 맵별 장애물 면적 측정
    { // 영역 시작
        public static void Run() // 실제 맵 데이터 측정
        { // 영역 시작
            try // 실제 세 맵 검사
            { // 영역 시작
                foreach (string name in new[] { "Plains", "Ruins", "Swamp" }) // 실제 세 맵 순회
                { // 영역 시작
                    PrototypeMapProfile profile = AssetDatabase.LoadAssetAtPath<PrototypeMapProfile>("Assets/Data/World/PrototypeMap" + name + ".asset"); // 원본 맵 데이터
                    Require(profile != null, "Map profile missing: " + name); // 실제 데이터 확인
                    GameObject owner = new GameObject("DensityMapFixture"); // 실제 맵 관리자 객체
                    PrototypeChunkMap map = owner.AddComponent<PrototypeChunkMap>(); // 실제 청크 생성기
                    Require(map.ApplyProfile(profile), "Could not apply map: " + name); // 맵 데이터 연결 확인
                    MethodInfo populate = typeof(PrototypeChunkMap).GetMethod("PopulateChunk", BindingFlags.NonPublic | BindingFlags.Instance); // 실제 청크 생성 경로
                    Require(populate != null, "Chunk population missing."); // 실제 생성기 확인
                    Dictionary<PrototypeZoneKind, int> zones = new Dictionary<PrototypeZoneKind, int>(); // 실제 지역별 출현 수
                    int obstacles = 0; // 전체 장애물 수
                    int occupiedChunks = 0; // 장애물 보유 청크 수
                    double area = 0d; // 충돌 사각형 합산 면적
                    const int samples = 512; // 각 맵의 난수 표본 수
                    for (int seed = 1; seed <= samples; seed++) // 실제 난수 표본 순회
                    { // 영역 시작
                        UnityEngine.Random.InitState(seed); // 재현 가능한 난수
                        GameObject chunk = new GameObject("DensityChunk"); // 실제 시험 청크
                        chunk.transform.SetParent(owner.transform, false); // 맵 아래 배치
                        populate.Invoke(map, new object[] { chunk.transform }); // 실제 청크 생성
                        SnakeContactHazard[] hazards = chunk.GetComponentsInChildren<SnakeContactHazard>(); // 실제 장애물 목록
                        if (hazards.Length > 0) // 장애물 존재 확인
                        { // 영역 시작
                            occupiedChunks++; // 장애물 보유 청크 증가
                        } // 영역 종료
                        foreach (SnakeContactHazard hazard in hazards) // 실제 장애물 순회
                        { // 영역 시작
                            BoxCollider2D collider = hazard.GetComponent<BoxCollider2D>(); // 실제 장애물 충돌체
                            Require(collider != null, "Actual obstacle collider missing."); // 실제 충돌체 확인
                            area += Math.Abs(collider.size.x * collider.size.y * hazard.transform.lossyScale.x * hazard.transform.lossyScale.y); // 실제 충돌 사각형 면적 합산
                            obstacles++; // 실제 장애물 수 증가
                        } // 영역 종료
                        foreach (PrototypeSpecialZone zone in chunk.GetComponentsInChildren<PrototypeSpecialZone>()) // 실제 특수 지역 순회
                        { // 영역 시작
                            zones.TryGetValue(zone.Kind, out int count); // 현재 지역 종류의 출현 수
                            zones[zone.Kind] = count + 1; // 실제 지역 출현 기록
                        } // 영역 종료
                        UnityEngine.Object.DestroyImmediate(chunk); // 시험 청크 제거
                    } // 영역 종료
                    double percent = area * 100d / (samples * map.ChunkSize * map.ChunkSize); // 전체 청크 면적 대비 충돌체 합산 비율
                    string detail = string.Join(", ", zones); // 실제 지역별 출현 요약
                    Debug.Log($"[MapDensityMeasurement] {name}: samples={samples}, obstacleChunks={occupiedChunks}, obstacles={obstacles}, colliderAreaPercent={percent:F3}, zones={detail}"); // 실제 맵별 표본 기록
                    UnityEngine.Object.DestroyImmediate(owner); // 시험 관리자 제거
                } // 영역 종료
                Debug.Log("[MapDensityMeasurement] PASS: three actual map profiles measured without changing data."); // 실제 측정 완료 기록
                EditorApplication.Exit(0); // 성공 종료
            } // 영역 종료
            catch (Exception exception) // 실제 실패 처리
            { // 영역 시작
                Debug.LogError("[MapDensityMeasurement] FAIL: " + exception); // 실제 실패 원인 기록
                EditorApplication.Exit(1); // 실패 종료
            } // 영역 종료
        } // 영역 종료
        private static void Require(bool condition, string message) // 실제 측정 조건 확인
        { // 영역 시작
            if (!condition) // 실패 조건 확인
            { // 영역 시작
                throw new InvalidOperationException(message); // 실제 실패 전달
            } // 영역 종료
        } // 영역 종료
    } // 영역 종료
} // 영역 종료
