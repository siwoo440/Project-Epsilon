using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using ProjectEpsilon.Combat; // 실제 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 투사체
using ProjectEpsilon.Player; // 실제 뱀과 체력
using ProjectEpsilon.World; // 실제 맵 관리자
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay100EnemyProjectileSweepVerification // 적 탄 고속 충돌 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay100EnemyProjectileSweep"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay100EnemyProjectileSweepVerification() // 시험 복구
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
            new GameObject("Day100EnemyProjectileSweepRunner").AddComponent<Day100EnemyProjectileSweepRunner>(); // 실제 투사체 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day100EnemyProjectileSweepVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day100EnemyProjectileSweepRunner : MonoBehaviour // 실제 적 탄 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 고속 탄과 플레이어 접촉 확인
        { // 함수 시작
            PrototypeChunkMap map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 관리자
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 머리
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeHealth health = player == null ? null : player.GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeInvulnerability invulnerability = player == null ? null : player.GetComponent<SnakeInvulnerability>(); // 실제 무적
            Require(map != null && player != null && body != null && health != null && invulnerability != null, "실제 플레이어와 맵 구성 누락"); // 실제 시험 구성 확인
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
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            Vector3 center = new Vector3(500f, 500f, 0f); // 다른 전투와 분리된 위치
            Vector3 safe = center + Vector3.up * 20f; // 실제 몸통 안전 위치
            player.transform.position = center; // 실제 머리 위치
            foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 순회
            { // 반복 시작
                segment.transform.position = safe; // 실제 몸통 안전 위치
            } // 반복 종료
            if (body.TailSegment != null) // 실제 꼬리 존재 확인
            { // 조건 시작
                body.TailSegment.transform.position = safe; // 실제 꼬리 안전 위치
            } // 조건 종료
            health.ResetHealth(); // 실제 공용 체력 복구
            invulnerability.ClearInvulnerability(); // 실제 무적 해제
            int before = health.CurrentHealth; // 실제 최초 체력
            GameObject shot = new GameObject("Day100FastEnemyShot"); // 실제 적 탄 객체
            shot.transform.position = center + Vector3.left; // 실제 머리 앞 발사 위치
            shot.AddComponent<EnemyTestProjectile>().Configure(Vector2.right, 10000f, 1f, 7, health, body, player, null); // 실제 고속 적 탄 설정
            Physics2D.SyncTransforms(); // 실제 물리 위치 갱신
            yield return new WaitForSeconds(0.2f); // 실제 물리 이동과 피해 처리 대기
            Require(health.CurrentHealth == before - 7, "빠른 적 탄의 머리 통과 피해 누락"); // 실제 고속 접촉 확인
            yield return new WaitForSeconds(0.1f); // 실제 중복 피해 대기
            Require(health.CurrentHealth == before - 7, "빠른 적 탄의 머리 중복 피해"); // 실제 단일 피해 확인
            health.ResetHealth(); // 실제 공용 체력 복구
            invulnerability.ClearInvulnerability(); // 실제 무적 해제
            player.transform.position = safe; // 실제 머리 안전 위치
            Require(body.BodySegments.Count > 0, "몸통 충돌 시험 대상 누락"); // 실제 몸통 존재 확인
            body.BodySegments[0].transform.position = center; // 실제 몸통만 탄 경로에 배치
            Physics2D.SyncTransforms(); // 실제 몸통 위치 갱신
            before = health.CurrentHealth; // 실제 몸통 접촉 전 체력
            Fire(center + Vector3.left, health, body, player); // 실제 고속 적 탄 발사
            yield return new WaitForSeconds(0.2f); // 실제 물리 이동과 피해 대기
            Require(health.CurrentHealth == before - 7, "빠른 적 탄의 몸통 통과 피해 누락"); // 실제 몸통 접촉 확인
            health.ResetHealth(); // 실제 공용 체력 복구
            invulnerability.ClearInvulnerability(); // 실제 무적 해제
            body.BodySegments[0].transform.position = safe; // 실제 몸통 안전 위치
            player.transform.position = center; // 실제 머리 위치
            GameObject cover = new GameObject("Day100Cover"); // 실제 엄폐 객체
            cover.transform.position = center + Vector3.left * 0.6f; // 탄과 머리 사이 실제 엄폐 위치
            CircleCollider2D coverCollider = cover.AddComponent<CircleCollider2D>(); // 실제 엄폐 충돌체
            coverCollider.radius = 0.08f; // 실제 경로상의 작은 엄폐 범위
            coverCollider.isTrigger = true; // 실제 엄폐 접촉 방식
            cover.AddComponent<SnakeContactHazard>().SetProjectileCover(true); // 기존 투사체 엄폐 활성화
            Physics2D.SyncTransforms(); // 실제 엄폐 위치 갱신
            before = health.CurrentHealth; // 실제 엄폐 전 체력
            Fire(center + Vector3.left, health, body, player); // 실제 고속 적 탄 발사
            yield return new WaitForSeconds(0.2f); // 실제 엄폐 처리 대기
            Require(health.CurrentHealth == before, "빠른 적 탄이 엄폐 뒤 머리에 피해 적용"); // 실제 엄폐 차단 확인
            cover.SetActive(false); // 다음 시험의 엄폐 간섭 제거
            invulnerability.StartInvulnerability(3f); // 실제 공용 무적 시작
            Physics2D.SyncTransforms(); // 실제 물리 위치 갱신
            Fire(center + Vector3.left, health, body, player); // 무적 중 실제 고속 적 탄
            yield return new WaitForSeconds(0.2f); // 실제 무적 충돌 대기
            Require(health.CurrentHealth == before, "빠른 적 탄이 기존 무적을 무시"); // 실제 무적 유지 확인
            invulnerability.ClearInvulnerability(); // 다음 시험의 무적 해제
            GameObject paused = Fire(center + Vector3.left, health, body, player); // 일시정지 시험의 실제 적 탄
            Physics2D.SyncTransforms(); // 실제 물리 위치 갱신
            GameManager.Instance.PauseGame(); // 실제 게임 일시정지
            yield return new WaitForSecondsRealtime(0.1f); // 실제 정지 시간 대기
            Require(paused != null && Mathf.Approximately(paused.transform.position.x, center.x - 1f) && health.CurrentHealth == before, "일시정지 중 적 탄 이동 또는 피해"); // 실제 정지 상태 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            yield return new WaitForSeconds(0.2f); // 실제 재개와 피해 대기
            Require(health.CurrentHealth == before - 7, "재개 후 빠른 적 탄의 피해 누락"); // 실제 재개 명중 확인
            health.ResetHealth(); // 실제 공용 체력 복구
            invulnerability.ClearInvulnerability(); // 실제 무적 해제
            before = health.CurrentHealth; // 실제 제한 속도 시험 전 체력
            float previousMaximum = Physics2D.maxTranslationSpeed; // 기존 물리 최대 속도 저장
            Physics2D.maxTranslationSpeed = 10f; // 실제 이동 제한 시험 속도
            GameObject limited = Fire(center + Vector3.left * 15f, health, body, player); // 실제 한 물리 단계 이동 범위 밖의 적 탄
            Physics2D.SyncTransforms(); // 실제 물리 위치 갱신
            yield return new WaitForFixedUpdate(); // 실제 첫 물리 단계
            Require(limited != null && health.CurrentHealth == before && limited.transform.position.x < center.x - 1f, "실제 물리 이동 범위 밖의 조기 피해"); // 실제 이동 상한 내 충돌 확인
            yield return new WaitForSeconds(0.2f); // 실제 제한 이동과 피해 대기
            Require(health.CurrentHealth == before - 7, "제한 속도로 도착한 적 탄의 피해 누락"); // 실제 도착 이후 명중 확인
            Physics2D.maxTranslationSpeed = previousMaximum; // 기존 물리 최대 속도 복원
            AutomatedDay100EnemyProjectileSweepVerification.Finish(true, "Fast hits, cover, invulnerability, pause and physics speed limit passed."); // 실제 시험 결과
        } // 함수 종료
        private static GameObject Fire(Vector3 position, SnakeHealth health, SnakeBodyManager body, SnakeMovement player) // 실제 고속 적 탄 발사
        { // 함수 시작
            GameObject shot = new GameObject("Day100FastEnemyShot"); // 실제 적 탄 객체
            shot.transform.position = position; // 실제 적 탄 발사 위치
            shot.AddComponent<EnemyTestProjectile>().Configure(Vector2.right, 10000f, 1f, 7, health, body, player, null); // 실제 고속 적 탄 설정
            return shot; // 실제 적 탄 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay100EnemyProjectileSweepVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
