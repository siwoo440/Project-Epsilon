using System; // 예외 형식
using System.Reflection; // 실제 적 설정 조회
using ProjectEpsilon.Combat; // 실제 적 전투 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어
using ProjectEpsilon.Progression; // 실제 적 보상
using UnityEditor; // 배치와 에셋 조회
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedEnemyArtVerification // 실제 일반 적 이미지 검증
    {
        private const string ActiveKey = "EpsilonEnemyArtVerification"; // 시험 진행 표식
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 설정 범위
        private static bool finished; // 중복 종료 방지
        private static double started; // 실제 시작 시각
        private static WeaponTarget[] spawnedEnemies; // 실제 생성 적 목록
        private static SnakeMovement trackedPlayer; // 실제 목표 플레이어
        private static float orientationCheckAt; // 실제 방향 확인 시각
        private static bool checkingCharge; // 실제 돌진 방향 확인 단계
        private static bool checkingBossCharge; // 실제 보스 예고 방향 확인 단계
        private static WeaponTarget trackedBoss; // 실제 보스 대상
        static AutomatedEnemyArtVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 시험 진행 확인
            {
                EditorApplication.update += Tick; // 실제 시작 감시
            }
        }
        public static void Run() // 실제 씬 배치 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 실제 플레이 모드 시작
        }
        private static void Tick() // 실제 시작과 시험 결과 감시
        {
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            {
                return; // 실제 진입 대기
            }
            if (started <= 0d) // 시험 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 시간 제한
            {
                Finish(1, "Timed out waiting for enemy art test."); // 시간 초과 실패
                return; // 감시 종료
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 실제 시작 선택 확인
            {
                return; // 실제 시작 대기
            }
            if (spawnedEnemies != null) // 실제 방향 확인 대기
            {
                if (Time.time < orientationCheckAt) // 실제 물리 진행 확인
                {
                    return; // 실제 물리 스텝 대기
                }
                try // 실제 방향 오류 감시
                {
                    if (checkingBossCharge) // 실제 보스 예고 확인
                    {
                        PrototypeBossChargeAttack charge = trackedBoss.GetComponent<PrototypeBossChargeAttack>(); // 실제 보스 돌진 행동
                        Require(charge.IsWarning && Vector2.Dot(trackedBoss.transform.up, Vector2.right) > 0.95f, "Actual boss sprite did not face its warned charge direction."); // 실제 보스 예고 방향 확인
                        Finish(0, "Actual ordinary, elite and boss images, target directions, warned charges and reward sprites passed."); // 실제 전체 결과
                        return; // 실제 시험 종료
                    }
                    if (checkingCharge) // 실제 돌진 방향 확인
                    {
                        Require(Vector2.Dot(spawnedEnemies[3].transform.up, Vector2.right) > 0.95f, "Actual elite sprite did not follow its locked charge direction."); // 실제 고정 돌진 방향 확인
                        PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 관리자
                        MethodInfo createBoss = typeof(PrototypeBossEncounter).GetMethod("TrySpawnBoss", Private); // 실제 보스 생성 경로
                        Require(encounter != null && createBoss != null, "Actual boss art fixtures missing."); // 실제 보스 구성 확인
                        createBoss.Invoke(encounter, null); // 실제 보스 생성
                        WeaponTarget boss = encounter.Boss; // 실제 보스 대상
                        SpriteRenderer bossRenderer = boss == null ? null : boss.GetComponent<SpriteRenderer>(); // 실제 보스 표시기
                        Require(bossRenderer != null && bossRenderer.sprite != null && AssetDatabase.GetAssetPath(bossRenderer.sprite).EndsWith("BossPlains_Casual.png", StringComparison.Ordinal) && bossRenderer.color == Color.white, "Actual boss image or tint was incorrect."); // 실제 보스 이미지와 원본 색상 확인
                        FieldInfo shotField = typeof(EarlyEnemyBehaviour).GetField("visual", Private); // 실제 투사체 이미지 필드
                        Sprite shot = shotField == null ? null : shotField.GetValue(boss.GetComponent<EarlyEnemyBehaviour>()) as Sprite; // 실제 보스 투사체 이미지
                        Require(shot != null && shot != bossRenderer.sprite, "Actual boss image unexpectedly replaced its projectile sprite."); // 실제 보스 투사체 표시 유지 확인
                        trackedBoss = boss; // 실제 보스 대상 보관
                        trackedPlayer.transform.position = boss.transform.position + Vector3.right * 8f; // 실제 보스 오른쪽 예고 목표
                        Physics2D.SyncTransforms(); // 실제 물리 위치 반영
                        PrototypeBossChargeAttack charge = boss.GetComponent<PrototypeBossChargeAttack>(); // 실제 보스 돌진 행동
                        MethodInfo beginWarning = typeof(PrototypeBossChargeAttack).GetMethod("BeginWarning", Private); // 실제 예고 시작 경로
                        Require(charge != null && beginWarning != null, "Actual boss warning path missing."); // 실제 예고 경로 확인
                        beginWarning.Invoke(charge, null); // 실제 보스 예고 시작
                        checkingBossCharge = true; // 실제 보스 예고 검사 단계
                        orientationCheckAt = Time.time + 0.15f; // 실제 물리 스텝 이후 확인 시각
                        return; // 실제 예고 진행 대기
                    }
                    foreach (WeaponTarget enemy in spawnedEnemies) // 실제 생성 적 순회
                    {
                        Vector2 towardPlayer = ((Vector2)(trackedPlayer.transform.position - enemy.transform.position)).normalized; // 실제 플레이어 방향
                        Require(Vector2.Dot(enemy.transform.up, towardPlayer) > 0.95f, "Actual enemy sprite did not face its target."); // 실제 추적 방향 표시 확인
                    }
                    EarlyEnemyBehaviour elite = spawnedEnemies[3].GetComponent<EarlyEnemyBehaviour>(); // 실제 엘리트 행동
                    typeof(EarlyEnemyBehaviour).GetField("chargePhase", Private).SetValue(elite, 2); // 실제 돌진 단계 통제
                    typeof(EarlyEnemyBehaviour).GetField("chargeDirection", Private).SetValue(elite, Vector2.right); // 실제 고정 돌진 방향 통제
                    typeof(EarlyEnemyBehaviour).GetField("chargePhaseEnd", Private).SetValue(elite, Time.time + 2f); // 실제 돌진 종료 시각 통제
                    checkingCharge = true; // 실제 돌진 검사 단계 시작
                    orientationCheckAt = Time.time + 0.15f; // 실제 물리 스텝 이후 확인 시각
                    return; // 실제 돌진 진행 대기
                }
                catch (Exception error) // 실제 방향 오류 확인
                {
                    Finish(1, error.ToString()); // 실제 실패 기록
                }
                return; // 추가 적 생성 방지
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기 조회
            if (player == null || spawner == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            {
                return; // 실제 초기화 대기
            }
            try // 실제 시험 오류 감시
            {
                player.enabled = false; // 실제 플레이어 이동 통제
                spawner.enabled = false; // 실제 일반 적 출현 통제
                FieldInfo settingsField = typeof(EarlyEnemyWaveSpawner).GetField("enemies", Private); // 실제 적 데이터 필드
                FieldInfo placeholderField = typeof(EarlyEnemyWaveSpawner).GetField("visual", Private); // 기존 공용 표시 필드
                FieldInfo dropField = typeof(EnemyDropController).GetField("pickupSprite", Private); // 기존 보상 이미지 필드
                MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", Private); // 실제 일반 적 생성 경로
                EarlyEnemySettings[] settings = settingsField == null ? null : settingsField.GetValue(spawner) as EarlyEnemySettings[]; // 실제 적 데이터
                Sprite placeholder = placeholderField == null ? null : placeholderField.GetValue(spawner) as Sprite; // 실제 공용 보상 이미지
                Require(settings != null && settings.Length >= 4 && placeholder != null && dropField != null && spawn != null, "Actual enemy art fixtures missing."); // 실제 필수 구성 확인
                string[] expected = { "EnemyChaser_Casual.png", "EnemyPredator_Casual.png", "EnemyShooter_Casual.png", "EnemyElite_Casual.png" }; // 실제 적 종류별 이미지
                WeaponTarget[] tested = new WeaponTarget[expected.Length]; // 실제 생성 적 보관
                for (int index = 0; index < expected.Length; index++) // 실제 일반 적 종류 순회
                {
                    Vector3 position = player.transform.position + Vector3.right * (20f + index * 5f); // 실제 접촉 밖 출현 위치
                    WeaponTarget enemy = spawn.Invoke(spawner, new object[] { settings[index], position, false }) as WeaponTarget; // 실제 일반 적 생성
                    Require(enemy != null && enemy.IsAlive, "Actual ordinary enemy did not spawn."); // 실제 일반 적 생존 확인
                    tested[index] = enemy; // 실제 방향 검사 대상 저장
                    SpriteRenderer renderer = enemy.GetComponent<SpriteRenderer>(); // 실제 적 표시기
                    Require(renderer != null && renderer.sprite != null && AssetDatabase.GetAssetPath(renderer.sprite).EndsWith(expected[index], StringComparison.Ordinal), "Actual enemy kind did not use its assigned image: " + expected[index]); // 실제 종류별 이미지 확인
                    Require(renderer.color == Color.white && enemy.GetComponent<CircleCollider2D>() != null, "Actual enemy tint or collider was incorrect."); // 실제 이미지 색상과 충돌체 확인
                    EnemyDropController drop = enemy.GetComponent<EnemyDropController>(); // 실제 적 보상
                    Require(drop != null && (Sprite)dropField.GetValue(drop) == placeholder, "Actual enemy image unexpectedly replaced the reward sprite."); // 실제 보상 이미지 유지 확인
                }
                trackedPlayer = player; // 실제 목표 플레이어 저장
                spawnedEnemies = tested; // 실제 생성 적 목록 저장
                orientationCheckAt = Time.time + 0.15f; // 실제 물리 스텝 이후 확인 시각
            }
            catch (Exception error) // 실제 시험 오류 확인
            {
                Finish(1, error.ToString()); // 실제 실패 기록
            }
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 실패 조건 확인
            {
                throw new InvalidOperationException(message); // 실제 실패 전달
            }
        }
        private static void Finish(int code, string message) // 실제 결과 기록
        {
            if (finished) // 중복 종료 확인
            {
                return; // 중복 처리 방지
            }
            finished = true; // 종료 상태 저장
            SessionState.SetBool(ActiveKey, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 실제 감시 해제
            Debug.Log($"[EnemyArtVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 프로세스 종료
        }
    }
}
