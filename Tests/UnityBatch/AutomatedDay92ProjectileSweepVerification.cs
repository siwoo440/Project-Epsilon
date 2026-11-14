using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 시험용 관통 수 설정
using ProjectEpsilon.Combat; // 실제 투사체와 적
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 공격 속성
using ProjectEpsilon.Enemies; // 자연 적 생성기
using ProjectEpsilon.Player; // 엄폐물과 플레이어
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay92ProjectileSweepVerification // 빠른 포탑 탄 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay92ProjectileSweep"; // 실행 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay92ProjectileSweepVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 게임 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 게임 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시작 방지
            new GameObject("Day92ProjectileSweepRunner").AddComponent<Day92ProjectileSweepRunner>(); // 실제 물리 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 배치 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day92ProjectileSweepVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day92ProjectileSweepRunner : MonoBehaviour // 실제 충돌 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 빠른 투사체의 실제 충돌 확인
        { // 함수 시작
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            WeaponTarget target = CreateTarget(new Vector3(505f, 500f, 0f)); // 격리된 실제 적 충돌체
            GameObject shot = new GameObject("Day92FastShot"); // 빠른 포탑 탄
            shot.transform.position = new Vector3(500f, 500f, 0f); // 적 앞 발사 위치
            StraightProjectile projectile = shot.AddComponent<StraightProjectile>(); // 실제 직선 탄
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(null, WeaponAttribute.Physical, 0, 0, 1, shot.transform.position, 10f); // 실제 공격 정보
            projectile.Configure(Vector2.right, attack, null, 10000f, 1f, null); // 한 프레임 통과 시험 속도
            Physics2D.SyncTransforms(); // 실제 충돌 위치 갱신
            yield return new WaitForSeconds(0.2f); // 실제 이동과 물리 처리 대기
            Require(Mathf.Approximately(target.CurrentHealth, 90f), "빠른 포탑 탄의 적 통과 피해 누락"); // 실제 경로 명중 확인
            yield return new WaitForSeconds(0.1f); // 중복 피해 대기
            Require(Mathf.Approximately(target.CurrentHealth, 90f), "빠른 포탑 탄 중복 피해"); // 실제 단일 피해 확인
            GameObject cover = new GameObject("Day92Cover"); // 실제 엄폐 충돌체
            cover.transform.position = new Vector3(503f, 510f, 0f); // 발사체와 적 사이 위치
            BoxCollider2D coverCollider = cover.AddComponent<BoxCollider2D>(); // 실제 엄폐 충돌 범위
            coverCollider.isTrigger = true; // 실제 엄폐 접촉 방식
            cover.AddComponent<SnakeContactHazard>().SetProjectileCover(true); // 기존 투사체 엄폐 활성화
            WeaponTarget covered = CreateTarget(new Vector3(505f, 510f, 0f)); // 엄폐 뒤 실제 적
            Fire(new Vector3(500f, 510f, 0f), null); // 실제 빠른 포탑 탄 발사
            Physics2D.SyncTransforms(); // 실제 충돌 위치 갱신
            yield return new WaitForSeconds(0.2f); // 실제 엄폐 처리 대기
            Require(Mathf.Approximately(covered.CurrentHealth, 100f), "빠른 포탑 탄이 엄폐 뒤 적을 타격"); // 실제 엄폐 차단 확인
            WeaponData piercing = ScriptableObject.CreateInstance<WeaponData>(); // 임시 관통 포탑 데이터
            typeof(WeaponData).GetField("projectileHitLimit", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(piercing, 2); // 실제 두 대상 관통 수
            WeaponTarget near = CreateTarget(new Vector3(503f, 520f, 0f)); // 첫 번째 실제 적
            WeaponTarget middle = CreateTarget(new Vector3(505f, 520f, 0f)); // 두 번째 실제 적
            WeaponTarget far = CreateTarget(new Vector3(507f, 520f, 0f)); // 세 번째 실제 적
            Fire(new Vector3(500f, 520f, 0f), piercing); // 실제 관통 포탑 탄 발사
            Physics2D.SyncTransforms(); // 실제 충돌 위치 갱신
            yield return new WaitForSeconds(0.2f); // 실제 관통 처리 대기
            Require(Mathf.Approximately(near.CurrentHealth, 90f) && Mathf.Approximately(middle.CurrentHealth, 90f) && Mathf.Approximately(far.CurrentHealth, 100f), "빠른 관통 탄의 거리 순서 또는 명중 수 오류"); // 실제 거리 순서와 관통 수 확인
            WeaponTarget paused = CreateTarget(new Vector3(505f, 530f, 0f)); // 정지 시험의 실제 적
            GameObject pausedShot = Fire(new Vector3(500f, 530f, 0f), null); // 정지 시험의 실제 탄
            Physics2D.SyncTransforms(); // 실제 충돌 위치 갱신
            GameManager.Instance.PauseGame(); // 실제 게임 일시정지
            yield return new WaitForSecondsRealtime(0.2f); // 정지 중 실제 시간 대기
            Require(Mathf.Approximately(paused.CurrentHealth, 100f) && pausedShot != null && Mathf.Approximately(pausedShot.transform.position.x, 500f), "일시정지 중 포탑 탄 이동 또는 피해"); // 실제 정지 상태 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            yield return new WaitForSeconds(0.2f); // 실제 이동과 명중 대기
            Require(Mathf.Approximately(paused.CurrentHealth, 90f), "재개 후 포탑 탄 명중 누락"); // 실제 재개 명중 확인
            AutomatedDay92ProjectileSweepVerification.Finish(true, "Fast traversal, cover, ordered piercing and pause passed."); // 실제 결과 기록
        } // 함수 종료
        private static WeaponTarget CreateTarget(Vector3 position) // 실제 적과 동일한 충돌 구성
        { // 함수 시작
            GameObject enemy = new GameObject("Day92Target"); // 격리 표적 생성
            enemy.transform.position = position; // 실제 표적 위치
            CircleCollider2D collider = enemy.AddComponent<CircleCollider2D>(); // 실제 적 충돌체
            collider.isTrigger = true; // 실제 적 접촉 방식
            Rigidbody2D body = enemy.AddComponent<Rigidbody2D>(); // 실제 적 물리체
            body.bodyType = RigidbodyType2D.Kinematic; // 실제 적 이동 방식
            body.useFullKinematicContacts = true; // 실제 운동학 접촉
            WeaponTarget target = enemy.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(100f); // 시험 체력 설정
            return target; // 실제 표적 반환
        } // 함수 종료
        private static GameObject Fire(Vector3 position, WeaponData weapon) // 실제 빠른 포탑 탄 발사
        { // 함수 시작
            GameObject shot = new GameObject("Day92FastShot"); // 실제 투사체 객체
            shot.transform.position = position; // 실제 발사 위치
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, WeaponAttribute.Physical, 0, 0, 1, position, 10f); // 실제 공격 정보
            shot.AddComponent<StraightProjectile>().Configure(Vector2.right, attack, null, 10000f, 1f, null); // 실제 고속 탄 설정
            return shot; // 실제 투사체 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedDay92ProjectileSweepVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
