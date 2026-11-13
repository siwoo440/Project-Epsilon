using System; // 기본 예외와 인수
using System.Collections; // 실제 프레임 대기
using System.Reflection; // 제품 계약 조회
using ProjectEpsilon.Combat; // 실제 보스 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 보스와 적
using ProjectEpsilon.Player; // 실제 플레이어
using ProjectEpsilon.Progression; // 격리된 맵 해금 시험
using ProjectEpsilon.World; // 실제 청크 맵
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 로드
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 갱신 연결
    public static class AutomatedDay46MapsVerification // 실제 맵과 보스 검증
    {
        private const string Key = "EpsilonDay46Maps"; // 시험 진행 표식
        private static bool prepared; // 시작 맵 선택 완료
        private static bool injected; // 실행기 중복 방지
        private static int selected; // 선택할 맵 번호
        private static double started; // 실제 시험 시작
        static AutomatedDay46MapsVerification() // 도메인 갱신 복구
        {
            if (SessionState.GetBool(Key, false)) // 활성 시험 확인
            {
                EditorApplication.update += Tick; // 시험 감시 연결
            }
        }
        public static void Run() // 실제 배치 진입
        {
            string[] args = Environment.GetCommandLineArgs(); // 배치 인수 조회
            for (int i = 0; i + 1 < args.Length; i++) // 배치 인수 순회
            {
                if (args[i] == "-epsilonMapIndex") // 맵 번호 인수 확인
                {
                    int.TryParse(args[i + 1], out selected); // 시험 맵 번호 저장
                }
            }
            SessionState.SetInt("EpsilonDay46MapIndex", selected); // 도메인 갱신 간 맵 번호 보존
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 진행 표식
            EditorApplication.update += Tick; // 시험 감시 시작
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }
        private static void Tick() // 실제 시작 상태 감시
        {
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            {
                return; // 진입 대기
            }
            if (started <= 0d) // 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 시험 제한 확인
            {
                Finish(1, "Timed out."); // 제한 시간 실패
                return; // 추가 처리 중단
            }
            if (!prepared) // 시작 전 맵 구성 확인
            {
                Type type = typeof(PrototypeChunkMap).Assembly.GetType("ProjectEpsilon.World.PrototypeMapSelection"); // 실제 맵 선택 형식
                Component selection = type == null ? null : UnityEngine.Object.FindFirstObjectByType(type) as Component; // 실제 선택 구성
                if (selection == null) // 맵 선택 부재 확인
                {
                    Finish(1, "Actual map selection is missing."); // 선행 실패 기록
                    return; // 선택 불가 처리
                }
                PropertyInfo count = type.GetProperty("ProfileCount"); // 실제 맵 수
                MethodInfo choose = type.GetMethod("SelectMap"); // 실제 맵 선택
                if (count == null || choose == null || (int)count.GetValue(selection) != 3) // 세 맵 계약 확인
                {
                    Finish(1, "Three map profiles or selection path are missing."); // 맵 구성 실패
                    return; // 선택 불가 처리
                }
                selected = SessionState.GetInt("EpsilonDay46MapIndex", 0); // 저장된 맵 번호
                if (selected > 0 && Application.productName.StartsWith("Project-Epsilon-MapVerification-", StringComparison.Ordinal)) // 격리된 시험의 추가 맵 확인
                {
                    MetaProgressionService service = MetaProgressionController.Instance == null ? null : MetaProgressionController.Instance.Service; // 실제 격리된 진행 데이터
                    if (service == null || !service.TryCompleteRun("map-test-plains", 0d, 0, 0, true, 0) || !service.TryCompleteRun("map-test-ruins", 0d, 0, 0, true, 1)) // 실제 해금 조건 준비
                    {
                        Finish(1, "Could not prepare isolated map unlocks."); // 격리된 해금 실패
                        return; // 실제 맵 선택 중단
                    }
                }
                if (!(bool)choose.Invoke(selection, new object[] { selected })) // 실제 선택 적용
                {
                    Finish(1, "Selected map was rejected before the run."); // 선택 실패
                    return; // 추가 처리 중단
                }
                prepared = true; // 실제 맵 선택 완료
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작 후보 처리
            {
                return; // 시작 또는 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 실제 전투 시작 확인
            {
                return; // 초기화 대기
            }
            injected = true; // 중복 실행 방지
            player.gameObject.AddComponent<Day46MapsRunner>(); // 실제 시나리오 시작
        }
        public static void Finish(int code, string message) // 실제 결과 기록
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시험 감시 해제
            Debug.Log($"[Day46MapsVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class Day46MapsRunner : MonoBehaviour // 실제 맵별 전투 시나리오
    {
        private IEnumerator Start() // 실제 씬 검증
        {
            int index = SessionState.GetInt("EpsilonDay46MapIndex", 0); // 선택 맵 번호
            PrototypeChunkMap map = UnityEngine.Object.FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 맵
            PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 출현기
            EarlyEnemyWaveSpawner wave = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            Require(map != null && encounter != null && wave != null, "Actual scene fixtures missing."); // 실제 필수 구성 확인
            GetComponent<SnakeMovement>().enabled = false; // 실제 이동 통제
            wave.enabled = false; // 자연 적 출현 통제
            foreach (EarlyEnemyBehaviour existing in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 기존 적 순회
            {
                existing.gameObject.SetActive(false); // 시험 중 자연 적 전투 차단
            }
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            if (weapons != null) // 무기 관리자 확인
            {
                weapons.enabled = false; // 통제 시험의 자동 공격 차단
            }
            yield return null; // 실제 청크 생성 대기
            Require(map.ActiveCount == 9, "Selected map did not create nine chunks."); // 세 맵의 실제 청크 수
            GameObject center = map.GetChunk(Vector2Int.zero); // 실제 중앙 청크
            Require(center != null && center.transform.Find("Ground") != null, "Selected map ground missing."); // 실제 바닥 확인
            SpriteRenderer ground = center.transform.Find("Ground").GetComponent<SpriteRenderer>(); // 실제 바닥 렌더러
            string[] names = { "PlainsGround_Casual", "RuinsGround_Prototype", "SwampGround_Prototype" }; // 맵별 실제 바닥 이름
            Require(ground != null && ground.sprite != null && ground.sprite.name == names[index], "Selected map ground does not match its profile."); // 실제 맵 배경 연결
            FieldInfo obstacleChance = map.Profile.GetType().GetField("obstacleChance", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 장애물 확률 필드
            FieldInfo zoneChance = map.Profile.GetType().GetField("specialZoneChance", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 지역 확률 필드
            Require(obstacleChance != null && zoneChance != null, "Selected map generation data missing."); // 실제 맵 생성 설정 확인
            obstacleChance.SetValue(map.Profile, 1f); // 시험용 장애물 생성 확정
            zoneChance.SetValue(map.Profile, 1f); // 시험용 지역 생성 확정
            transform.position = new Vector3(map.ChunkSize * 30f, map.ChunkSize * 30f, 0f); // 실제 새 청크 위치
            yield return null; // 실제 새 청크 생성 대기
            yield return null; // 실제 지연 제거 대기
            GameObject generated = map.GetChunk(new Vector2Int(30, 30)); // 실제 맵별 청크
            SnakeContactHazard[] obstacles = map.GetComponentsInChildren<SnakeContactHazard>(); // 아홉 청크의 실제 장애물
            PrototypeSpecialZone[] zones = generated == null ? null : generated.GetComponentsInChildren<PrototypeSpecialZone>(); // 실제 특수 지역
            Require(obstacles != null && obstacles.Length > 0 && zones != null && zones.Length >= 1 && zones.Length == (zones[0].Kind == PrototypeZoneKind.ElectricFloor ? 2 : 1), "Selected map did not generate its obstacles and special zone."); // 실제 맵별 확률 적용 확인
            string[] folders = { "Plains", "Ruins", "Swamp" }; // 실제 맵별 이미지 폴더
            foreach (SnakeContactHazard obstacle in obstacles) // 실제 생성 장애물 순회
            {
                SpriteRenderer artwork = obstacle.GetComponent<SpriteRenderer>(); // 실제 장애물 표시기
                Require(artwork != null && artwork.sprite != null && AssetDatabase.GetAssetPath(artwork.sprite).StartsWith("Assets/Resources/MapElements/" + folders[index] + "/", StringComparison.Ordinal), "Selected map obstacle artwork is wrong."); // 실제 선택 맵의 장애물 이미지 확인
            }
            SetBossValue(map.Profile, "bossDamage", 20 + index); // 실제 맵별 투사체와 돌진 피해 시험값
            SetBossValue(map.Profile, "bossPhaseTwoSpeedMultiplier", 1.4f); // 실제 맵별 두 번째 단계 이동 배율
            SetBossValue(map.Profile, "bossPhaseTwoIntervalMultiplier", 0.6f); // 실제 맵별 두 번째 단계 사격 간격
            SetBossValue(map.Profile, "bossChargeInterval", 0.8f); // 실제 맵별 돌진 간격
            SetBossValue(map.Profile, "bossChargeWarningDuration", 0.6f); // 실제 맵별 돌진 예고 시간
            SetBossValue(map.Profile, "bossChargeDuration", 0.5f); // 실제 맵별 돌진 지속 시간
            SetBossValue(map.Profile, "bossChargeSpeed", 7f); // 실제 맵별 돌진 속도
            SetBossValue(map.Profile, "bossPhaseTwoChargeSpeedMultiplier", 1.3f); // 실제 맵별 두 번째 단계 돌진 배율
            SetBossValue(map.Profile, "bossHazardInterval", 3f); // 실제 맵별 위험 구역 간격
            SetBossValue(map.Profile, "bossPhaseTwoHazardIntervalMultiplier", index == 0 ? 1f : 0.5f); // 맵별 두 번째 단계 간격 시험값
            SetBossValue(map.Profile, "bossHazardWarningDuration", 0.7f); // 실제 맵별 위험 구역 예고
            SetBossValue(map.Profile, "bossHazardDuration", 1.5f); // 실제 맵별 위험 구역 지속
            SetBossValue(map.Profile, "bossHazardDamageInterval", 0.3f); // 실제 맵별 위험 피해 간격
            SetBossValue(map.Profile, "bossHazardRadius", 3f); // 실제 맵별 위험 반경
            SetBossValue(map.Profile, "bossHazardDamage", 6 + index); // 실제 맵별 위험 피해
            SetBossValue(map.Profile, "bossSummonInterval", 3f); // 실제 맵별 소환 간격
            SetBossValue(map.Profile, "bossSummonMaximum", 2); // 실제 맵별 소환 상한
            SetBossValue(map.Profile, "bossSummonRadius", 3f); // 실제 맵별 소환 거리
            Require(encounter.ApplyProfile(map.Profile), "Actual boss map settings could not be applied."); // 실제 선택 맵의 보스 설정 적용
            MethodInfo spawn = typeof(PrototypeBossEncounter).GetMethod("TrySpawnBoss", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 보스 생성
            Require(spawn != null, "Boss spawn path missing."); // 실제 생성 경로 확인
            spawn.Invoke(encounter, null); // 실제 보스 강제 생성
            WeaponTarget boss = encounter.Boss; // 실제 보스 조회
            Require(boss != null && boss.IsAlive && boss.GetComponent<EarlyEnemyBehaviour>() != null, "Selected map boss did not spawn with combat."); // 실제 보스 전투 구성
            float[] expectedHealth = { 600f, 700f, 650f }; // 맵별 실제 임시 체력
            int[] expectedFirstShots = { 3, 6, 4 }; // 맵별 첫 단계 전체 발사 수
            int[] expectedSecondShots = { 5, 10, 6 }; // 맵별 두 번째 단계 전체 발사 수
            Require(Mathf.Approximately(boss.MaximumHealth, expectedHealth[index]), "Selected map boss health does not match its data."); // 실제 맵별 보스 데이터 확인
            EarlyEnemyBehaviour behavior = boss.GetComponent<EarlyEnemyBehaviour>(); // 실제 보스 사격 행동
            behavior.enabled = false; // 통제 중 자동 사격 중지
            PrototypeBossChargeAttack charge = boss.GetComponent<PrototypeBossChargeAttack>(); // 실제 보스 돌진 구성
            PrototypeBossHazardSpawner hazard = boss.GetComponent<PrototypeBossHazardSpawner>(); // 실제 위험 구역 출현기
            PrototypeBossSummoner summon = boss.GetComponent<PrototypeBossSummoner>(); // 실제 보스 소환기
            Require(charge != null && hazard != null && summon != null, "Actual boss attack components missing."); // 실제 보스 공격 구성 확인
            hazard.enabled = false; // 돌진 시험 중 위험 구역 중지
            summon.enabled = false; // 돌진 시험 중 잡몹 소환 중지
            Vector3 chargeStart = boss.transform.position; // 실제 돌진 전 보스 위치
            yield return new WaitForSeconds(0.9f); // 실제 맵별 돌진 간격 경과
            Require(charge.IsWarning && !charge.IsCharging, "Map-specific charge did not begin its warning after the configured interval."); // 맵별 돌진 예고 시작 확인
            yield return new WaitForSeconds(0.35f); // 실제 예고 중 대기
            Require(charge.IsWarning && !charge.IsCharging, "Map-specific charge warning ended too early."); // 맵별 예고 지속 확인
            yield return new WaitForSeconds(0.3f); // 실제 예고 종료 경과
            Require(charge.IsCharging, "Map-specific charge did not start after its warning."); // 맵별 실제 돌진 시작 확인
            yield return new WaitForSeconds(0.55f); // 실제 돌진 지속 경과
            Require(!charge.IsCharging, "Map-specific charge exceeded its configured duration."); // 맵별 실제 돌진 종료 확인
            float chargeDistance = Vector2.Distance(chargeStart, boss.transform.position); // 실제 보스 돌진 거리
            Require(chargeDistance > 1.3f && chargeDistance < 2.2f, $"Actual charge distance differs from map speed, duration and enemy speed multiplier: distance={chargeDistance:F3}, start={chargeStart}, end={boss.transform.position}."); // 전체 적 속도 절반을 반영한 실제 돌진 거리 확인
            charge.enabled = false; // 후속 사격과 피해 시험의 돌진 중지
            MethodInfo fire = typeof(EarlyEnemyBehaviour).GetMethod("FireProjectile", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 부채꼴 사격 경로
            Require(fire != null, "Actual boss firing path missing."); // 실제 사격 경로 확인
            fire.Invoke(behavior, new object[] { Vector2.right }); // 실제 첫 단계 사격
            EnemyTestProjectile[] firstShots = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None); // 실제 첫 단계 투사체
            Require(firstShots.Length == expectedFirstShots[index], "Selected map boss first-phase firing count differs from its data."); // 실제 첫 단계 발사 확인
            if (index == 1) // 폐허 양방향 사격 확인
            { // 영역 시작
                FieldInfo direction = typeof(EnemyTestProjectile).GetField("direction", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 투사체 방향 필드
                Require(direction != null, "Actual projectile direction missing."); // 실제 투사체 방향 확인
                int forward = 0; // 실제 전방 발사 수
                int backward = 0; // 실제 후방 발사 수
                foreach (EnemyTestProjectile shot in firstShots) // 실제 첫 단계 탄환 순회
                { // 영역 시작
                    Vector2 value = (Vector2)direction.GetValue(shot); // 실제 탄환 이동 방향
                    forward += value.x > 0f ? 1 : 0; // 실제 전방 탄환 집계
                    backward += value.x < 0f ? 1 : 0; // 실제 후방 탄환 집계
                } // 영역 종료
                Require(forward == 3 && backward == 3, "Ruins boss did not fire three projectiles in both directions."); // 실제 양방향 부채꼴 확인
            } // 영역 종료
            FieldInfo projectileDamage = typeof(EnemyTestProjectile).GetField("damage", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 발사된 투사체 피해
            Require(projectileDamage != null && (int)projectileDamage.GetValue(firstShots[0]) == 20 + index, "Actual boss projectile did not use the selected map damage."); // 실제 맵별 사격 피해 확인
            foreach (EnemyTestProjectile shot in firstShots) // 실제 첫 단계 투사체 순회
            {
                shot.gameObject.SetActive(false); // 다음 단계 시험 간섭 차단
            }
            boss.TakeDamage(boss.MaximumHealth * 0.5f); // 실제 두 번째 단계 경계
            yield return null; // 실제 보스 단계 갱신
            Require(encounter.IsPhaseTwo, "Selected map boss did not enter its second phase."); // 실제 두 번째 단계 확인
            FieldInfo settingsField = typeof(EarlyEnemyBehaviour).GetField("settings", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 보스 행동 능력치
            EarlyEnemySettings settings = settingsField == null ? null : settingsField.GetValue(behavior) as EarlyEnemySettings; // 실제 두 번째 단계 능력치
            Require(settings != null && Mathf.Abs(settings.speedRatio - map.Profile.BossSpeedRatio * 1.4f) < 0.001f && Mathf.Abs(settings.attackInterval - map.Profile.BossAttackInterval * 0.6f) < 0.001f, "Actual second phase ignored map multipliers."); // 실제 맵별 두 번째 단계 이동과 사격 간격 확인
            fire.Invoke(behavior, new object[] { Vector2.right }); // 실제 두 번째 단계 사격
            EnemyTestProjectile[] secondShots = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None); // 실제 두 번째 단계 투사체
            Require(secondShots.Length == expectedSecondShots[index], "Selected map boss second-phase firing count differs from its data."); // 실제 두 번째 단계 발사 확인
            foreach (EnemyTestProjectile shot in secondShots) // 실제 두 번째 단계 투사체 순회
            { // 영역 시작
                shot.gameObject.SetActive(false); // 위험 구역 시험 간섭 차단
            } // 영역 종료
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            health.ResetHealth(); // 실제 체력 복구
            GetComponent<SnakeInvulnerability>().ClearInvulnerability(); // 기존 무적 제거
            int beforeHazard = health.CurrentHealth; // 실제 위험 예고 전 체력
            FieldInfo nextHazard = typeof(PrototypeBossHazardSpawner).GetField("nextHazardAt", BindingFlags.Instance | BindingFlags.NonPublic); // 첫 구역 즉시 출현 설정
            Require(nextHazard != null, "Boss hazard schedule missing."); // 실제 출현 일정 확인
            nextHazard.SetValue(hazard, Time.time); // 시험 시작 시각 지정
            hazard.enabled = true; // 실제 위험 구역 출현 시작
            yield return new WaitForFixedUpdate(); // 실제 위험 구역 생성 대기
            yield return null; // 실제 구성 반영 대기
            PrototypeBossHazard actualHazard = hazard.CurrentHazard; // 실제 생성된 위험 구역
            Require(actualHazard != null && actualHazard.IsWarning && health.CurrentHealth == beforeHazard, "Map-specific boss hazard warning missing or damaged early."); // 실제 위험 예고와 무피해 확인
            LineRenderer hazardOutline = actualHazard.GetComponent<LineRenderer>(); // 실제 위험 예고선
            Require(hazardOutline != null, "Actual boss hazard warning outline missing."); // 실제 위험 예고 표시 확인
            if (index == 1) // 폐허 구획 경고 확인
            { // 영역 시작
                Require(hazardOutline.loop && hazardOutline.positionCount == 4, "Ruins boss did not show a rectangular sector warning."); // 실제 구획형 위험 예고 확인
            } // 영역 종료
            if (index == 2) // 습지 포자 경고 확인
            { // 영역 시작
                Require(hazardOutline.startColor.g > 0.5f && hazardOutline.startColor.r < 0.9f, "Swamp boss spore warning was not visually distinguished."); // 실제 독성 포자 경고 구분 확인
            } // 영역 종료
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기
            Require(body != null && follower != null, "Boss hazard body fixture missing."); // 실제 몸통 구성 확인
            follower.enabled = false; // 범위 시험 중 몸통 위치 고정
            foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 순회
            { // 영역 시작
                segment.transform.position = actualHazard.transform.position + Vector3.right * 20f; // 실제 위험 반경 밖의 몸통 위치
            } // 영역 종료
            transform.position = actualHazard.transform.position + (index == 1 ? new Vector3(2.4f, 2.4f, 0f) : Vector3.right * 2f); // 실제 구획 모서리 또는 변경 반경 안의 머리
            Physics2D.SyncTransforms(); // 실제 머리와 몸통 위치 반영
            yield return new WaitForSeconds(0.4f); // 실제 위험 예고 중 대기
            Require(actualHazard.IsWarning && health.CurrentHealth == beforeHazard, "Boss hazard warning ended too early."); // 실제 예고 지속 확인
            yield return new WaitForSeconds(0.4f); // 실제 위험 활성 경과
            Require(actualHazard.IsActive && health.CurrentHealth == beforeHazard - (6 + index), "Actual boss hazard ignored map warning or damage."); // 실제 맵별 위험 피해 확인
            yield return new WaitForSeconds(0.3f); // 실제 위험 피해 간격 경과
            Require(actualHazard.IsActive && health.CurrentHealth == beforeHazard - 2 * (6 + index), "Actual boss hazard ignored its map damage interval."); // 실제 맵별 반복 피해 확인
            actualHazard.gameObject.SetActive(false); // 실제 위험 피해 중지
            UnityEngine.Object.Destroy(actualHazard.gameObject); // 첫 구역 제거
            yield return new WaitForSeconds(0.55f); // 단계별 다음 출현 간격 경과
            if (index == 0) // 평원 기본 간격 확인
            { // 조건 시작
                Require(hazard.CurrentHazard == null, "Plains hazard appeared before its unchanged interval."); // 기존 간격 유지 확인
            } // 조건 종료
            else // 폐허와 습지 두 번째 단계 확인
            { // 조건 시작
                Require(hazard.CurrentHazard != null && hazard.CurrentHazard.IsWarning, "Second-phase hazard did not use the shorter map interval."); // 단축된 구역 재출현 확인
                hazard.CurrentHazard.gameObject.SetActive(false); // 후속 시험의 추가 피해 방지
            } // 조건 종료
            hazard.enabled = false; // 추가 위험 구역 생성 중지
            GetComponent<SnakeInvulnerability>().StartInvulnerability(30f); // 소환 적 접촉의 시험 간섭 방지
            summon.enabled = true; // 실제 보스 소환 시작
            yield return new WaitForSeconds(0.3f); // 첫 실제 소환 대기
            Require(summon.ActiveCount == 1, "Actual boss did not use its map summon interval."); // 맵별 첫 소환 간격 확인
            FieldInfo summonField = typeof(PrototypeBossSummoner).GetField("summons", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 소환 적 목록
            IList summoned = summonField == null ? null : summonField.GetValue(summon) as IList; // 실제 소환 적 집합
            Require(summoned != null && summoned.Count == 1, "Actual boss summon target missing."); // 실제 소환 대상 확인
            WeaponTarget summonedEnemy = summoned[0] as WeaponTarget; // 실제 첫 소환 적
            float summonDistance = summonedEnemy == null ? 0f : Vector2.Distance(summonedEnemy.transform.position, boss.transform.position); // 실제 보스와 소환 적 거리
            Require(summonDistance > 2.5f && summonDistance < 3.5f, "Actual boss did not use the map summon radius."); // 실제 맵별 소환 거리 확인
            yield return new WaitForSeconds(3.1f); // 실제 다음 소환 간격 경과
            Require(summon.ActiveCount == 2, "Actual boss did not summon its second enemy."); // 맵별 두 번째 소환 확인
            yield return new WaitForSeconds(3.1f); // 실제 상한 상태의 소환 간격 경과
            Require(summon.ActiveCount == 2, "Actual boss exceeded the map summon maximum."); // 맵별 소환 상한 확인
            summon.enabled = false; // 두 번째 단계 돌진 시험의 추가 소환 중지
            transform.position = boss.transform.position + Vector3.down * 20f; // 실제 보스와 충분히 떨어진 머리
            Physics2D.SyncTransforms(); // 실제 머리 위치 반영
            Vector3 secondStart = boss.transform.position; // 실제 두 번째 단계 돌진 시작 위치
            charge.enabled = true; // 실제 두 번째 단계 돌진 재개
            yield return new WaitForSeconds(0.1f); // 실제 절단 돌진 예고 시작
            Require(charge.IsWarning && encounter.IsPhaseTwo, "Second-phase charge warning missing."); // 두 번째 단계 예고 확인
            if (index == 2) // 습지 추적 예고 확인
            { // 영역 시작
                LineRenderer warning = boss.GetComponent<LineRenderer>(); // 실제 돌진 예고선
                Require(warning != null && warning.enabled, "Swamp charge warning line missing."); // 실제 돌진 예고 표시 확인
                transform.position += Vector3.right * 10f; // 실제 머리의 예고 중 이동
                Physics2D.SyncTransforms(); // 실제 머리 위치 반영
                yield return new WaitForSeconds(0.15f); // 실제 추적 예고 갱신 대기
                Vector2 shown = ((Vector2)(warning.GetPosition(1) - warning.GetPosition(0))).normalized; // 실제 화면상 예고 방향
                Vector2 targetDirection = ((Vector2)(transform.position - boss.transform.position)).normalized; // 실제 이동한 머리 방향
                Require(Vector2.Dot(shown, targetDirection) > 0.99f, "Swamp charge warning did not track the moving player."); // 실제 머리를 추적하는 예고 확인
            } // 영역 종료
            yield return new WaitForSeconds(index == 2 ? 0.5f : 0.65f); // 실제 두 번째 단계 예고 경과
            Require(charge.IsCharging, "Second-phase charge did not begin."); // 두 번째 단계 돌진 확인
            yield return new WaitForSeconds(0.6f); // 실제 두 번째 단계 돌진 종료
            float secondDistance = Vector2.Distance(secondStart, boss.transform.position); // 실제 두 번째 단계 돌진 거리
            Require(!charge.IsCharging && secondDistance > 1.8f && secondDistance < 2.7f, "Second-phase charge did not use the map speed multiplier."); // 실제 두 번째 단계 돌진 배율 확인
            AutomatedDay46MapsVerification.Finish(0, $"Map {index}: map profile, boss firing, charge, hazard and two-phase settings passed."); // 맵별 시험 성공
        }
        private static void SetBossValue(PrototypeMapProfile profile, string name, object value) // 실제 맵 보스 시험값 설정
        { // 영역 시작
            FieldInfo field = typeof(PrototypeMapProfile).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic); // 실제 맵 보스 데이터 필드
            Require(field != null, "Map-specific boss setting missing: " + name); // 조정 가능한 보스 데이터 확인
            field.SetValue(profile, value); // 실제 선택 맵의 시험값 적용
        } // 영역 종료
        private static void Require(bool condition, string message) // 실제 시험 조건
        {
            if (!condition) // 실제 실패 확인
            {
                AutomatedDay46MapsVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 잘못된 후속 시험 중단
            }
        }
    }
}
