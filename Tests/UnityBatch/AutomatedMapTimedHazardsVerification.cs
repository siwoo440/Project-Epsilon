using System; // 예외 형식
using System.Collections; // 코루틴 형식
using ProjectEpsilon.Combat; // 실제 속성 효과
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어
using ProjectEpsilon.Progression; // 실제 경험치 보석
using ProjectEpsilon.World; // 실제 맵과 특수 지역
using System.Reflection; // 실제 범위 조회
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedMapTimedHazardsVerification // 실제 신성과 암흑 시험
    {
        private const string Key = "EpsilonMapTimedHazardsVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedMapTimedHazardsVerification() // 도메인 재시작 처리
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
            player.gameObject.AddComponent<MapTimedHazardsRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[MapTimedHazardsVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class MapTimedHazardsRunner : MonoBehaviour // 실제 신성 및 암흑 시험 실행기
    {
        private IEnumerator Start() // 실제 경고·피해·스위치 시험
        {
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeInvulnerability invulnerability = GetComponent<SnakeInvulnerability>(); // 실제 공통 무적
            PrototypeChunkMap map = UnityEngine.Object.FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 관리자
            Require(player != null && health != null && invulnerability != null && map != null && map.Profile != null, "Actual timed hazard dependencies missing."); // 실제 구성 확인
            PrototypeZoneKind electric = Parse("ElectricFloor"); // 폐허 방전 바닥 종류
            PrototypeZoneKind control = Parse("CircuitSwitch"); // 폐허 회로 스위치 종류
            PrototypeZoneKind spore = Parse("ToxicSpore"); // 습지 독성 포자 종류
            player.enabled = false; // 실제 머리 이동 중지
            map.enabled = false; // 자연 청크 생성 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 중지
            foreach (PrototypeSpecialZone existing in UnityEngine.Object.FindObjectsByType<PrototypeSpecialZone>(FindObjectsSortMode.None)) // 기존 지역 순회
            {
                existing.gameObject.SetActive(false); // 다른 지역 효과 중지
            }
            Vector3 point = new Vector3(500f, 500f, 0f); // 충돌이 없는 시험 위치
            player.transform.position = point; // 실제 머리 위치
            health.ResetHealth(); // 실제 공용 체력 복구
            invulnerability.ClearInvulnerability(); // 기존 무적 제거
            PrototypeSpecialZone floor = PrototypeSpecialZone.Create(map.transform, point, electric, map.Profile); // 실제 방전 바닥 생성
            int initial = health.CurrentHealth; // 최초 체력
            yield return new WaitForFixedUpdate(); // 실제 머리 진입 판정
            yield return new WaitForSeconds(0.6f); // 방전 예고 중 대기
            Require(ReadFlag(floor, "IsWarning") && health.CurrentHealth == initial, "Electric floor damaged before its warning ended."); // 경고 중 피해 없음 확인
            yield return new WaitForSeconds(0.5f); // 실제 방전 활성 경계
            Require(ReadFlag(floor, "IsHazardActive") && health.CurrentHealth == initial, "Electric floor did not activate after one second."); // 방전 활성과 첫 틱 전 체력 확인
            yield return new WaitForSeconds(1.1f); // 실제 첫 피해 주기
            Require(health.CurrentHealth == initial - 8, "Electric floor did not deal eight damage after one active second."); // 방전 주기와 피해 확인
            invulnerability.StartInvulnerability(1.5f); // 실제 공통 무적 적용
            int protectedHealth = health.CurrentHealth; // 무적 시작 체력
            yield return new WaitForSeconds(1f); // 다음 피해 주기와 종료 대기
            Require(health.CurrentHealth == protectedHealth, "Electric floor bypassed existing invulnerability."); // 공통 무적 보존 확인
            floor.gameObject.SetActive(false); // 첫 방전 시험 종료
            invulnerability.ClearInvulnerability(); // 다음 시험의 무적 해제
            Vector3 next = new Vector3(600f, 600f, 0f); // 분리된 장치 위치
            PrototypeSpecialZone switched = PrototypeSpecialZone.Create(map.transform, next + Vector3.right * 8f, electric, map.Profile); // 스위치 주변 방전 바닥
            player.transform.position = next; // 스위치 위치의 실제 머리
            PrototypeSpecialZone circuit = PrototypeSpecialZone.Create(map.transform, next, control, map.Profile); // 실제 회로 스위치
            yield return new WaitForFixedUpdate(); // 실제 스위치 진입
            Require(ReadFlag(switched, "IsSuppressed"), "Switch did not suppress its nearby electric floor."); // 인접 방전 비활성 확인
            player.transform.position = next + Vector3.right * 8f; // 비활성 방전 바닥 진입
            yield return new WaitForSeconds(1.3f); // 기존 경고 시간 경과
            Require(!ReadFlag(switched, "IsWarning") && !ReadFlag(switched, "IsHazardActive"), "Suppressed electric floor started a hazard."); // 비활성 중 위험 차단 확인
            player.transform.position = next + Vector3.up * 15f; // 스위치와 장치 이탈
            yield return new WaitForSeconds(9f); // 실제 10초 비활성 경과
            Require(!ReadFlag(switched, "IsSuppressed"), "Switch suppression did not expire after ten seconds."); // 실제 비활성 만료 확인
            player.transform.position = next; // 스위치 재진입
            yield return new WaitForFixedUpdate(); // 재진입 판정
            Require(!ReadFlag(switched, "IsSuppressed"), "Switch retriggered before its twenty-second cooldown."); // 실제 스위치 대기 확인
            player.transform.position = next + Vector3.up * 15f; // 스위치 재사용 전 이탈
            yield return new WaitForSeconds(10.2f); // 실제 20초 재사용 대기 경과
            player.transform.position = next; // 재사용 가능 스위치 진입
            yield return new WaitForFixedUpdate(); // 실제 스위치 재발동 판정
            Require(ReadFlag(switched, "IsSuppressed"), "Switch did not retrigger after its twenty-second cooldown."); // 실제 스위치 재사용 만료 확인
            circuit.gameObject.SetActive(false); // 스위치 시험 종료
            switched.gameObject.SetActive(false); // 방전 장치 시험 종료
            Vector3 swamp = new Vector3(800f, 800f, 0f); // 독립된 습지 시험 위치
            player.transform.position = swamp; // 포자 지역 머리 진입
            health.ResetHealth(); // 실제 체력 복구
            invulnerability.ClearInvulnerability(); // 실제 무적 해제
            PrototypeSpecialZone spores = PrototypeSpecialZone.Create(map.transform, swamp, spore, map.Profile); // 실제 독성 포자
            initial = health.CurrentHealth; // 포자 진입 체력
            yield return new WaitForFixedUpdate(); // 실제 포자 진입 판정
            yield return new WaitForSeconds(0.8f); // 포자 예고 중 대기
            Require(ReadFlag(spores, "IsWarning") && health.CurrentHealth == initial, "Spores damaged before warning ended."); // 포자 예고와 무피해 확인
            yield return new WaitForSeconds(0.5f); // 실제 포자 활성 경계
            Require(ReadFlag(spores, "IsHazardActive") && health.CurrentHealth == initial, "Spores did not activate after their warning."); // 포자 활성과 첫 틱 전 체력 확인
            yield return new WaitForSeconds(2.1f); // 실제 2초 피해 주기
            Require(health.CurrentHealth == initial - 5, "Spores did not deal five damage at their two-second interval."); // 포자 피해량과 주기 확인
            spores.gameObject.SetActive(false); // 독성 포자 시험 종료
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 기존 일반 적 출현기
            typeof(EarlyEnemyWaveSpawner).GetField("elapsed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(spawner, 300f); // 개미굴 개방 시간 통제
            invulnerability.StartInvulnerability(30f); // 실제 적 접촉의 시험 간섭 방지
            Vector3 antPoint = new Vector3(1000f, 1000f, 0f); // 독립된 개미굴 시험 위치
            player.transform.position = antPoint; // 실제 머리의 개미굴 진입
            int enemiesBefore = spawner.CurrentNormalEnemyCount; // 소환 전 일반 적 수
            PrototypeSpecialZone anthill = PrototypeSpecialZone.Create(map.transform, antPoint, PrototypeZoneKind.AntHill, map.Profile); // 실제 개미굴 생성
            yield return new WaitForFixedUpdate(); // 개미굴 진입 판정
            yield return new WaitForSeconds(0.6f); // 개미굴 예고 중 대기
            Require(anthill.IsWarning && spawner.CurrentNormalEnemyCount == enemiesBefore, "Ant hill spawned before its warning."); // 예고 중 무소환 확인
            yield return new WaitForSeconds(3.2f); // 세 마리부터 다섯 마리까지 순차 소환 대기
            int summoned = spawner.CurrentNormalEnemyCount - enemiesBefore; // 실제 소환된 일반 적 수
            Require(summoned >= 3 && summoned <= 5, "Ant hill did not spawn three to five normal enemies."); // 실제 순차 소환 수 확인
            foreach (EnemyDropController drop in UnityEngine.Object.FindObjectsByType<EnemyDropController>(FindObjectsSortMode.None)) // 실제 일반 적 보상 구성 순회
            {
                Require(drop.enabled, "Map chaser did not retain its normal reward component."); // 일반 적 경험치 보상 구성 확인
            }
            player.transform.position = antPoint + Vector3.right * 10f; // 개미굴 이탈
            yield return new WaitForFixedUpdate(); // 이탈 판정
            player.transform.position = antPoint; // 재사용 대기 중 재진입
            yield return new WaitForFixedUpdate(); // 재진입 판정
            Require(!anthill.IsWarning && spawner.CurrentNormalEnemyCount == enemiesBefore + summoned, "Ant hill retriggered before cooldown."); // 25초 이전 재발동 차단
            player.transform.position = antPoint + Vector3.right * 10f; // 재사용 만료 전 개미굴 이탈
            yield return new WaitForSeconds(22f); // 실제 25초 재사용 대기 경과
            player.transform.position = antPoint; // 재사용 가능 개미굴 재진입
            yield return new WaitForFixedUpdate(); // 실제 재발동 진입 판정
            Require(anthill.IsWarning, "Ant hill did not retrigger after its twenty-five-second cooldown."); // 실제 재사용 만료 확인
            anthill.gameObject.SetActive(false); // 첫 개미굴 시험 종료
            WeaponTarget mapEnemy = null; // 실제 개미굴 일반 적 대상
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 일반 적 순회
            {
                if (Vector2.Distance(enemy.transform.position, antPoint) < 10f) // 개미굴 주변 소환 적 확인
                {
                    mapEnemy = enemy.GetComponent<WeaponTarget>(); // 실제 공격 대상 연결
                    break; // 첫 소환 적 선택
                }
            }
            Require(mapEnemy != null, "Actual ant hill enemy missing."); // 실제 소환 적 확인
            int gemsBefore = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length; // 처치 전 실제 보석 수
            mapEnemy.TakeDamage(100000f); // 실제 일반 적 처치
            Require(UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length > gemsBefore, "Ant hill enemy did not drop its normal experience."); // 실제 일반 경험치 보상 확인
            typeof(EarlyEnemyWaveSpawner).GetField("activeEnemies", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(spawner, spawner.CurrentNormalEnemyLimit); // 현재 일반 적 상한 통제
            Vector3 cappedPoint = new Vector3(1200f, 1200f, 0f); // 독립된 상한 시험 위치
            player.transform.position = cappedPoint; // 상한 시험 개미굴 진입
            PrototypeSpecialZone.Create(map.transform, cappedPoint, PrototypeZoneKind.AntHill, map.Profile); // 실제 상한 시험 개미굴
            yield return new WaitForSeconds(4f); // 예고와 순차 소환 시간 경과
            Require(spawner.CurrentNormalEnemyCount == spawner.CurrentNormalEnemyLimit, "Ant hill exceeded the existing normal enemy cap."); // 기존 일반 적 상한 유지 확인
            Vector3 collapsePoint = new Vector3(1400f, 1400f, 0f); // 독립된 붕괴 잔해 시험 위치
            player.transform.position = collapsePoint; // 실제 붕괴 지역 머리 진입
            PrototypeSpecialZone collapse = PrototypeSpecialZone.Create(map.transform, collapsePoint, PrototypeZoneKind.Collapse, map.Profile); // 실제 붕괴 지역 생성
            yield return new WaitForSeconds(1.2f); // 플레이어가 지역에 남은 상태로 예고 경과
            Require(collapse.GetComponentInChildren<SnakeContactHazard>() == null, "Collapse created an obstacle over the player."); // 플레이어 위의 즉시 장애물 생성 차단 확인
            player.transform.position = collapsePoint + Vector3.right * 10f; // 붕괴 지역 이탈
            yield return new WaitForFixedUpdate(); // 실제 이탈 판정
            player.transform.position = collapsePoint; // 붕괴 지역 재진입
            yield return new WaitForSeconds(0.3f); // 실제 붕괴 예고 시작
            Require(collapse.IsWarning, "Collapse did not show its warning."); // 실제 예고 상태 확인
            player.transform.position = collapsePoint + Vector3.right * 10f; // 장애물 생성 전 안전 이탈
            yield return new WaitForSeconds(0.9f); // 실제 일 초 예고 경과
            Require(collapse.IsHazardActive && collapse.GetComponentInChildren<SnakeContactHazard>() != null, "Collapse did not create its temporary existing obstacle."); // 예고 후 기존 장애물 생성 확인
            yield return new WaitForSeconds(2.6f); // 실제 잔해 지속 시간 경과
            Require(!collapse.IsHazardActive && collapse.GetComponentInChildren<SnakeContactHazard>() == null, "Collapse obstacle did not expire."); // 일시 장애물 제거 확인
            AutomatedMapTimedHazardsVerification.Finish(0, "Electric, switch, spores, capped rewardable ant spawning and safe temporary collapse passed."); // 실제 검증 성공 기록
        }

        private static bool ReadFlag(PrototypeSpecialZone zone, string name) // 실제 위험 지역 상태 조회
        {
            PropertyInfo property = typeof(PrototypeSpecialZone).GetProperty(name); // 실제 공개 상태 조회
            Require(property != null, "Hazard state missing: " + name); // 실제 상태 존재 확인
            return (bool)property.GetValue(zone); // 실제 위험 상태 반환
        }
        private static PrototypeZoneKind Parse(string name) // 실제 지역 종류 조회
        {
            Require(Enum.TryParse(name, out PrototypeZoneKind kind), "Map feature kind missing: " + name); // 실제 종류 존재 확인
            return kind; // 실제 지역 종류 반환
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
                AutomatedMapTimedHazardsVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
