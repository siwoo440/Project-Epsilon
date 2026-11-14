using System; // 실제 예외 처리
using System.Collections; // 실제 프레임 대기
using System.Reflection; // 실제 시험 수치 설정
using ProjectEpsilon.Combat; // 기존 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 기존 적 출현기
using ProjectEpsilon.Player; // 실제 뱀과 체력
using ProjectEpsilon.World; // 실제 맵 위험
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay89LocustSweepVerification // 빠른 황충 이동 충돌 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay89LocustSweep"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay89LocustSweepVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투 상태 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 전투 상태 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            new GameObject("Day89LocustSweepRunner").AddComponent<Day89LocustSweepRunner>(); // 실제 프레임 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 배치 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 감시 해제
            Debug.Log("[Day89LocustSweepVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day89LocustSweepRunner : MonoBehaviour // 실제 황충 충돌 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 빠른 이동과 피해 확인
        { // 함수 시작
            PrototypeChunkMap map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 관리자
            WorldEventController events = map == null ? null : map.GetComponent<WorldEventController>(); // 실제 위험 관리자
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 뱀 머리
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 뱀 몸통
            SnakeHealth health = player == null ? null : player.GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeInvulnerability invulnerability = player == null ? null : player.GetComponent<SnakeInvulnerability>(); // 실제 공통 무적
            Require(events != null && map.Profile != null && player != null && body != null && health != null && invulnerability != null, "실제 시험 구성 누락"); // 실제 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            SnakeBodyFollower follower = body.GetComponent<SnakeBodyFollower>(); // 실제 몸통 경로 추적기
            if (follower != null) // 실제 추적기 확인
            { // 조건 시작
                follower.enabled = false; // 시험 중 몸통 이동 중지
            } // 조건 종료
            SnakeSelfCollision self = player.GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 처리기
            if (self != null) // 실제 자기 충돌 처리기 확인
            { // 조건 시작
                self.enabled = false; // 시험용 위치 변경 간섭 방지
            } // 조건 종료
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            foreach (PrototypeSpecialZone zone in FindObjectsByType<PrototypeSpecialZone>(FindObjectsSortMode.None)) // 기존 특수 지역 순회
            { // 반복 시작
                zone.gameObject.SetActive(false); // 기존 지역의 시험 간섭 방지
            } // 반복 종료
            foreach (EarlyEnemyBehaviour enemy in FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 기존 자연 적 순회
            { // 반복 시작
                enemy.gameObject.SetActive(false); // 기존 적의 시험 간섭 방지
            } // 반복 종료
            SetFloat(map.Profile, "worldEventWarning", 0.2f); // 빠른 시험용 예고 시간
            SetFloat(map.Profile, "locustSpeed", 10000f); // 한 프레임에 긴 거리를 지나는 시험 속도
            health.ResetHealth(); // 실제 공용 체력 복구
            invulnerability.ClearInvulnerability(); // 기존 무적 해제
            int before = health.CurrentHealth; // 실제 최초 체력
            Require(events.TryBeginLocust(), "빠른 황충 예고 시작 실패"); // 실제 황충 예고 시작
            yield return new WaitForSeconds(0.7f); // 실제 예고와 전체 이동 대기
            Require(!events.IsWarning && !events.IsActive, "빠른 황충 이동이 종료되지 않음"); // 실제 이동 종료 확인
            Require(health.CurrentHealth == before - map.Profile.LocustDamage, "빠른 황충떼가 머리를 통과했으나 피해 누락"); // 실제 연속 이동 피해 확인
            yield return new WaitForSeconds(0.2f); // 실제 중복 피해 대기
            Require(health.CurrentHealth == before - map.Profile.LocustDamage, "빠른 황충 중복 피해"); // 실제 단일 피해 확인
            invulnerability.StartInvulnerability(3f); // 기존 공통 무적 시작
            before = health.CurrentHealth; // 실제 무적 시작 체력
            Require(events.TryBeginLocust(), "무적 중 빠른 황충 예고 실패"); // 실제 두 번째 황충 예고
            yield return new WaitForSeconds(0.7f); // 실제 빠른 이동 대기
            Require(health.CurrentHealth == before, "빠른 황충떼가 기존 무적을 무시"); // 실제 무적 보존 확인
            invulnerability.ClearInvulnerability(); // 다음 시험의 무적 해제
            Require(events.TryBeginLocust(), "몸통 충돌 황충 예고 실패"); // 실제 몸통 충돌 황충 예고
            Vector3 bodyDirection = (Vector3)typeof(WorldEventController).GetField("direction", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(events); // 실제 황충 진행 방향
            Vector3 bodySide = new Vector3(-bodyDirection.y, bodyDirection.x, 0f); // 실제 진행 방향의 수직축
            Vector3 hitPosition = player.transform.position; // 실제 황충 이동 경로 중심
            Vector3 headSafe = hitPosition + bodySide * (map.Profile.LocustWidth + 5f); // 실제 머리 회피 위치
            player.transform.position = headSafe; // 실제 머리 회피
            foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 순회
            { // 반복 시작
                segment.transform.position = headSafe; // 실제 나머지 몸통 회피
            } // 반복 종료
            if (body.TailSegment != null) // 실제 꼬리 존재 확인
            { // 조건 시작
                body.TailSegment.transform.position = headSafe; // 실제 꼬리 회피
            } // 조건 종료
            body.BodySegments[0].transform.position = hitPosition; // 실제 몸통 하나만 이동 경로에 배치
            Physics2D.SyncTransforms(); // 실제 물리 위치 갱신
            before = health.CurrentHealth; // 실제 몸통 충돌 전 체력
            yield return new WaitForSeconds(0.7f); // 실제 빠른 이동 대기
            Require(health.CurrentHealth == before - map.Profile.LocustDamage, "빠른 황충떼가 몸통을 통과했으나 피해 누락"); // 실제 몸통 이동 충돌 확인
            Require(events.TryBeginLocust(), "범위 밖 황충 예고 실패"); // 실제 세 번째 황충 예고
            Vector3 direction = (Vector3)typeof(WorldEventController).GetField("direction", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(events); // 실제 황충 진행 방향
            Vector3 side = new Vector3(-direction.y, direction.x, 0f); // 실제 이동 방향의 수직축
            Vector3 safe = player.transform.position + side * (map.Profile.LocustWidth + 5f); // 실제 황충 폭 바깥 위치
            player.transform.position = safe; // 실제 머리 회피 위치
            foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 순회
            { // 반복 시작
                segment.transform.position = safe; // 실제 몸통 회피 위치
            } // 반복 종료
            if (body.TailSegment != null) // 실제 꼬리 존재 확인
            { // 조건 시작
                body.TailSegment.transform.position = safe; // 실제 꼬리 회피 위치
            } // 조건 종료
            Physics2D.SyncTransforms(); // 실제 물리 위치 갱신
            before = health.CurrentHealth; // 실제 회피 전 체력
            yield return new WaitForSeconds(0.7f); // 실제 빠른 이동 대기
            Require(health.CurrentHealth == before, "황충 이동 폭 밖의 대상 피해"); // 실제 범위 밖 무피해 확인
            AutomatedDay89LocustSweepVerification.Finish(true, "Fast traversal, single damage, invulnerability and lane exclusion passed."); // 실제 전체 결과
        } // 함수 종료
        private static void SetFloat(PrototypeMapProfile profile, string name, float value) // 독립 시험 수치 적용
        { // 함수 시작
            FieldInfo field = typeof(PrototypeMapProfile).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic); // 실제 맵 수치 필드
            Require(field != null, "맵 위험 수치 누락: " + name); // 실제 데이터 확인
            field.SetValue(profile, value); // 메모리의 시험값 적용
        } // 함수 종료
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedDay89LocustSweepVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
