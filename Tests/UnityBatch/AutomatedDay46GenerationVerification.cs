using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 HUD 필드 조회
using ProjectEpsilon.Combat; // 무기와 표적
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어와 체력
using ProjectEpsilon.Progression; // 실제 경험치
using ProjectEpsilon.UI; // 실제 HUD
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.UI; // 실제 HUD 텍스트

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedDay46GenerationVerification // 이동 중 경험치 흡수 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay46GenerationVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedDay46GenerationVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 진행 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 플레이 감시 연결
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 씬 시험 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 씬 열기
            SessionState.SetBool(Key, true); // 시험 진행 기록
            EditorApplication.update -= Tick; // 중복 감시 제거
            EditorApplication.update += Tick; // 플레이 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        } // 메서드 끝
        private static void Tick() // 실제 초기화 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 플레이 대기
            } // 조건 끝
            if (started <= 0d) // 시험 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 시작 시각 저장
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 120d) // 시험 제한 시간 확인
            { // 조건 시작
                Finish(1, "Timed out."); // 시간 초과 실패
                return; // 시험 중단
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 시작 선택과 실행 여부 확인
            { // 조건 시작
                return; // 시작 또는 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 실행 중복 방지
            player.gameObject.AddComponent<Day46GenerationVerificationRunner>(); // 실제 시험 실행
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 시험 종료
        { // 메서드 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[Day46GenerationVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        } // 메서드 끝
    } // 클래스 끝
    public sealed class Day46GenerationVerificationRunner : MonoBehaviour // 실제 청크 내용 검증
    { // 클래스 시작
        private IEnumerator Start() // 실제 확률 경계와 지역 거리 검증
        { // 코루틴 시작
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어
            EarlyEnemyWaveSpawner wave = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 출현기
            ProjectEpsilon.World.PrototypeChunkMap map = UnityEngine.Object.FindFirstObjectByType<ProjectEpsilon.World.PrototypeChunkMap>(); // 실제 청크 맵
            ProjectEpsilon.World.PrototypeMapSelection selection = UnityEngine.Object.FindFirstObjectByType<ProjectEpsilon.World.PrototypeMapSelection>(); // 실제 맵 선택기
            Require(player != null && wave != null && map != null && selection != null && selection.SelectedProfile != null, "Generation fixtures missing."); // 실제 시험 구성 확인
            player.enabled = false; // 실제 이동 통제
            wave.enabled = false; // 실제 적 출현 중지
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기
            if (follower != null) // 실제 추적기 확인
            { // 조건 시작
                follower.enabled = false; // 충돌 시험의 몸통 이동 차단
            } // 조건 끝
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌기
            if (self != null) // 실제 충돌기 확인
            { // 조건 시작
                self.enabled = false; // 장애물 시험의 자기 충돌 차단
            } // 조건 끝
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 기존 적 순회
            { // 반복 시작
                enemy.gameObject.SetActive(false); // 장애물 시험의 적 충돌 차단
            } // 반복 끝
            Type profileType = selection.SelectedProfile.GetType(); // 실제 맵 데이터 형식
            FieldInfo obstacle = profileType.GetField("obstacleChance", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 장애물 확률
            FieldInfo zone = profileType.GetField("specialZoneChance", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 지역 확률
            Require(obstacle != null && zone != null, "Generation probabilities missing."); // 실제 확률 계약 확인
            obstacle.SetValue(selection.SelectedProfile, 1f); // 시험용 장애물 생성 확정
            zone.SetValue(selection.SelectedProfile, 1f); // 시험용 특수 지역 생성 확정
            player.transform.position = new Vector3(map.ChunkSize * 30f, map.ChunkSize * 30f, 0f); // 실제 새 청크 위치
            yield return null; // 실제 청크 생성 대기
            yield return null; // 실제 지연 제거 완료 대기
            Require(map.ActiveCount == 9, "Generated map does not contain nine chunks."); // 실제 청크 수 확인
            int obstacleCount = 0; // 실제 장애물 수
            int zoneCount = 0; // 실제 특수 지역 수
            var zones = new System.Collections.Generic.List<Transform>(); // 실제 지역 위치 목록
            for (int x = 29; x <= 31; x++) // 가로 청크 순회
            { // 반복 시작
                for (int y = 29; y <= 31; y++) // 세로 청크 순회
                { // 반복 시작
                    GameObject chunk = map.GetChunk(new Vector2Int(x, y)); // 실제 활성 청크
                    Require(chunk != null, "Expected generated chunk missing."); // 실제 청크 확인
                    foreach (SnakeContactHazard hazard in chunk.GetComponentsInChildren<SnakeContactHazard>()) // 실제 장애물 순회
                    { // 반복 시작
                        Require(hazard.HazardType == SnakeContactHazardType.Obstacle && hazard.GetComponent<Collider2D>() != null, "Generated obstacle lacks its actual hazard collider."); // 실제 충돌 연결 확인
                        obstacleCount++; // 실제 장애물 수 증가
                    } // 반복 끝
                    Type zoneType = typeof(SnakeMovement).Assembly.GetType("ProjectEpsilon.World.PrototypeSpecialZone"); // 실제 지역 형식
                    Require(zoneType != null, "Actual special zone type missing."); // 실제 지역 형식 확인
                    foreach (Component item in chunk.GetComponentsInChildren(zoneType)) // 실제 지역 순회
                    { // 반복 시작
                        zones.Add(item.transform); // 실제 지역 위치 저장
                        zoneCount++; // 실제 지역 수 증가
                    } // 반복 끝
                } // 반복 끝
            } // 반복 끝
            Require(obstacleCount > 0 && zoneCount == 9, $"Probability one did not create patterns and zones: obstacles={obstacleCount}, zones={zoneCount}."); // 확률 일 경계 확인
            GameObject generatedCenter = map.GetChunk(new Vector2Int(30, 30)); // 실제 중앙 청크
            SnakeContactHazard generatedObstacle = generatedCenter.GetComponentInChildren<SnakeContactHazard>(); // 실제 생성 장애물
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeInvulnerability invulnerability = GetComponent<SnakeInvulnerability>(); // 실제 무적 상태
            Require(generatedObstacle != null && health != null && invulnerability != null, "Generated obstacle collision fixtures missing."); // 실제 충돌 구성 확인
            invulnerability.ClearInvulnerability(); // 기존 무적 제거
            health.ResetHealth(); // 실제 공용 체력 복구
            int before = health.CurrentHealth; // 실제 충돌 전 체력
            player.transform.position = generatedObstacle.transform.position; // 실제 생성 장애물과 겹치기
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 장애물 충돌 대기
            yield return null; // 실제 충돌 우선순위 처리 대기
            Require(health.CurrentHealth < before && invulnerability.IsInvulnerable, "Generated obstacle did not cause actual damage and invulnerability."); // 실제 생성 장애물 피해와 무적 확인
            for (int left = 0; left < zones.Count; left++) // 실제 지역 간 거리 순회
            { // 반복 시작
                for (int right = left + 1; right < zones.Count; right++) // 나머지 지역 순회
                { // 반복 시작
                    Require(Vector3.Distance(zones[left].position, zones[right].position) >= 7f, "Adjacent generated zones overlap."); // 실제 지역 최소 간격 확인
                } // 반복 끝
            } // 반복 끝
            obstacle.SetValue(selection.SelectedProfile, 0f); // 시험용 장애물 생성 금지
            zone.SetValue(selection.SelectedProfile, 0f); // 시험용 특수 지역 생성 금지
            player.transform.position = new Vector3(map.ChunkSize * 50f, map.ChunkSize * 50f, 0f); // 실제 새 청크 위치
            yield return null; // 실제 청크 생성 대기
            yield return null; // 실제 지연 제거 대기
            GameObject center = map.GetChunk(new Vector2Int(50, 50)); // 실제 새 중앙 청크
            Require(center != null && center.GetComponentsInChildren<SnakeContactHazard>().Length == 0 && center.transform.childCount == 1, "Probability zero generated a special object."); // 확률 영 경계 확인
            AutomatedDay46GenerationVerification.Finish(0, $"Actual nine chunks generated {obstacleCount} obstacle colliders and {zoneCount} separated zones; probability zero generated neither."); // 실제 시험 성공
        } // 코루틴 끝
        private static void Require(bool condition, string message) // 실제 시험 조건
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedDay46GenerationVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
