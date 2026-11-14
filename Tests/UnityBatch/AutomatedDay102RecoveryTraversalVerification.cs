using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 시험용 이동 속도 설정
using ProjectEpsilon.Combat; // 실제 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 자연 적 생성기
using ProjectEpsilon.Player; // 실제 플레이어 이동과 체력
using ProjectEpsilon.Progression; // 실제 회복 아이템
using ProjectEpsilon.World; // 실제 맵 관리자
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay102RecoveryTraversalVerification // 회복 아이템 통과 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay102RecoveryTraversal"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay102RecoveryTraversalVerification() // 시험 복구
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
            new GameObject("Day102RecoveryTraversalRunner").AddComponent<Day102RecoveryTraversalRunner>(); // 실제 이동 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day102RecoveryTraversalVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day102RecoveryTraversalRunner : MonoBehaviour // 실제 회복 통과 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 자동 전진과 회복 확인
        { // 함수 시작
            PrototypeChunkMap map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 관리자
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeHealth health = player == null ? null : player.GetComponent<SnakeHealth>(); // 실제 공용 체력
            Require(map != null && player != null && body != null && health != null, "실제 맵·플레이어·체력 구성 누락"); // 실제 시험 구성 확인
            map.enabled = false; // 격리 시험 중 청크 이동 중지
            player.enabled = false; // 시험 준비 중 자동 이동 중지
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
            foreach (RecoveryPickup pickup in FindObjectsByType<RecoveryPickup>(FindObjectsSortMode.None)) // 기존 회복 아이템 순회
            { // 반복 시작
                pickup.gameObject.SetActive(false); // 기존 아이템의 시험 간섭 방지
            } // 반복 종료
            VerifyDrops(); // 실제 적 처치의 반감된 드롭 검증
            Require(body.TryRemoveBody() && body.MissingRepairableBodyCount > 0, "실제 몸통 손실 구성 실패"); // 실제 복구 가능 몸통 확인
            Require(health.TakeDamage(20), "실제 체력 피해 구성 실패"); // 실제 회복 필요 체력 구성
            int damagedHealth = health.CurrentHealth; // 실제 손상 체력
            int damagedBodies = body.CurrentBodyCount; // 실제 손실 후 몸통 수
            Time.captureFramerate = 20; // 재현 가능한 시험 프레임 간격
            typeof(SnakeMovement).GetField("moveSpeed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, 200f); // 한 프레임 통과용 시험 속도
            player.transform.rotation = Quaternion.Euler(0f, 0f, -90f); // 실제 오른쪽 자동 전진 방향
            yield return null; // 실제 시험 프레임 간격 적용 대기
            float step = player.CurrentMoveSpeed * Time.deltaTime; // 실제 한 프레임 자동 전진 거리
            Require(step > 6f, "회복 아이템 양쪽 흡수 범위를 넘는 시험 속도 부족"); // 실제 통과 시험 조건 확인
            Vector3 center = new Vector3(500f, 500f, 0f); // 다른 전투와 분리된 아이템 위치
            player.transform.position = center + Vector3.left * (step * 0.5f); // 실제 아이템 앞 시작 위치
            RecoveryPickup heal = RecoveryPickup.Spawn(center, player.transform, health, body, RecoveryPickupType.Heal, 15, null); // 실제 회복 아이템
            RecoveryPickup repair = RecoveryPickup.Spawn(center, player.transform, health, body, RecoveryPickupType.BodyRepair, 1, null); // 실제 몸통 복구 아이템
            RecoveryPickup outside = RecoveryPickup.Spawn(center + Vector3.up, player.transform, health, body, RecoveryPickupType.Heal, 15, null); // 실제 이동 경로 밖 회복 아이템
            yield return null; // 실제 첫 아이템 상태 갱신
            Require(heal != null && repair != null, "자동 전진 전 회복 아이템이 먼저 획득됨"); // 실제 시작 조건 확인
            player.enabled = true; // 실제 자동 전진 시작
            yield return null; // 실제 한 프레임 이동과 획득 대기
            player.enabled = false; // 시험 이동 종료
            Require(player.transform.position.x > center.x + 2.6f, "실제 자동 전진이 회복 아이템을 통과하지 못함"); // 실제 통과 경로 확인
            yield return null; // 실제 아이템 제거와 회복 대기
            Require(heal == null && repair == null && outside != null && health.CurrentHealth > damagedHealth && body.CurrentBodyCount == damagedBodies + 1, "자동 전진 중 회복·몸통 복구 또는 경로 밖 판정 실패"); // 실제 회복과 복구 확인
            health.ResetHealth(); // 실제 공용 체력 완전 회복
            Vector3 invalidCenter = center + Vector3.up * 30f; // 실제 이동 경로와 분리된 위치
            player.transform.position = invalidCenter + Vector3.left * (step * 0.5f); // 실제 회복 불가 아이템 앞 위치
            RecoveryPickup invalid = RecoveryPickup.Spawn(invalidCenter, player.transform, health, body, RecoveryPickupType.Heal, 15, null); // 완전 회복 상태의 실제 아이템
            RecoveryPickup invalidRepair = RecoveryPickup.Spawn(invalidCenter, player.transform, health, body, RecoveryPickupType.BodyRepair, 1, null); // 복구할 몸통이 없는 실제 아이템
            MetaProgressionController score = MetaProgressionController.Instance; // 실제 전투 점수 관리자
            Require(score != null, "점수 관리자 누락"); // 실제 점수 구성 확인
            int beforePickupScore = score.CurrentScore; // 아이템 획득 전 점수
            yield return null; // 실제 아이템 갱신 대기
            player.enabled = true; // 실제 자동 전진 시작
            yield return null; // 실제 회복 불가 아이템 통과
            player.enabled = false; // 실제 시험 이동 종료
            yield return null; // 실제 아이템 상태 갱신
            Require(invalid == null && invalidRepair == null && health.CurrentHealth == health.MaximumHealth && score.CurrentScore == beforePickupScore + 200, "가득 찬 회복·복구 아이템 점수 또는 소비 오류"); // 두 아이템의 점수와 소비 확인
            Require(health.TakeDamage(5), "강제 이동 시험 체력 피해 구성 실패"); // 실제 회복 가능 상태 구성
            Vector3 teleportCenter = center + Vector3.up * 60f; // 실제 이동 경로와 분리된 위치
            player.transform.position = teleportCenter + Vector3.left * 5f; // 실제 강제 이동 전 위치
            RecoveryPickup untouched = RecoveryPickup.Spawn(teleportCenter, player.transform, health, body, RecoveryPickupType.Heal, 15, null); // 실제 강제 이동 시험 아이템
            yield return null; // 실제 아이템 갱신 대기
            player.transform.position = teleportCenter + Vector3.right * 5f; // 자동 전진 없이 실제 위치 변경
            yield return null; // 실제 아이템 갱신 대기
            Require(untouched != null, "강제 위치 변경 경로의 회복 아이템을 잘못 획득"); // 실제 비이동 경로 제외 확인
            Time.captureFramerate = 0; // 기존 프레임 시간 복원
            AutomatedDay102RecoveryTraversalVerification.Finish(true, "Fast healing and body repair, eligibility and teleport exclusion passed."); // 실제 시험 결과
        } // 함수 종료
        private static void VerifyDrops() // 실제 적 처치 드롭 확률 확인
        { // 함수 시작
            FieldInfo multiplier = typeof(EnemyDropController).GetField("recoveryDropMultiplier", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 공통 확률 배율 필드
            EnemyDropController[] configuredDrops = FindObjectsByType<EnemyDropController>(FindObjectsInactive.Include, FindObjectsSortMode.None); // 씬에 저장된 실제 드롭 구성
            Require(multiplier != null && configuredDrops.Length > 0, "실제 씬의 회복 드롭 구성 누락"); // 기존 씬의 드롭 구성 확인
            foreach (EnemyDropController configured in configuredDrops) // 기존 씬의 적 드롭 순회
            { // 반복 시작
                Require(Mathf.Approximately((float)multiplier.GetValue(configured), 0.5f), "기존 씬의 회복 확률 배율 오류"); // 기존 직렬화 설정의 실제 반감 확인
            } // 반복 종료
            UnityEngine.Random.State previous = UnityEngine.Random.state; // 기존 무작위 상태 보관
            for (int seed = 1; seed <= 16; seed++) // 재현 가능한 처치 표본 순회
            { // 반복 시작
                UnityEngine.Random.InitState(seed); // 실제 확률 표본 초기화
                float healRoll = UnityEngine.Random.value; // 예상 체력 아이템 추첨
                float repairRoll = UnityEngine.Random.value; // 예상 몸통 아이템 추첨
                UnityEngine.Random.InitState(seed); // 실제 처치의 동일 표본 복구
                GameObject enemy = new GameObject("RecoveryDropTestEnemy"); // 독립 시험 적 생성
                enemy.transform.position = new Vector3(1000f, 1000f, 0f); // 플레이어와 분리된 드롭 위치
                WeaponTarget target = enemy.AddComponent<WeaponTarget>(); // 실제 피해 대상 생성
                target.Configure(1f); // 실제 시험 적 체력
                EnemyDropController drop = enemy.AddComponent<EnemyDropController>(); // 실제 적 드롭 구성
                drop.Configure(1, 15, 0.8f, 0.4f, null); // 원본 체력과 몸통 확률
                int beforeExperience = FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length; // 기존 경험치 보석 수
                target.TakeDamage(10f); // 실제 적 처치와 드롭 실행
                RecoveryPickup[] items = FindObjectsByType<RecoveryPickup>(FindObjectsSortMode.None); // 실제 드롭된 회복 아이템
                bool foundHeal = false; // 실제 체력 아이템 여부
                bool foundRepair = false; // 실제 몸통 아이템 여부
                foreach (RecoveryPickup item in items) // 실제 회복 아이템 순회
                { // 반복 시작
                    if (item.IsBodyRepair) // 실제 몸통 아이템 확인
                    { // 조건 시작
                        foundRepair = true; // 몸통 아이템 발견
                    } // 조건 종료
                    else // 실제 체력 아이템 확인
                    { // 대체 시작
                        foundHeal = true; // 체력 아이템 발견
                    } // 대체 종료
                    item.gameObject.SetActive(false); // 다음 표본의 드롭 격리
                    Destroy(item.gameObject); // 실제 시험 아이템 정리
                } // 반복 종료
                Require(foundHeal == (healRoll < 0.4f) && foundRepair == (repairRoll < 0.2f), "회복 아이템의 절반 드롭 확률 오류"); // 실제 두 확률 반감 확인
                Require(FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length == beforeExperience + 1, "경험치 보석 드롭 변경"); // 기존 경험치 보석 유지 확인
            } // 반복 종료
            UnityEngine.Random.state = previous; // 기존 무작위 상태 복구
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay102RecoveryTraversalVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
