using System; // 예외와 형식
using System.Reflection; // 실제 적 생성 경로 조회
using ProjectEpsilon.Combat; // 실제 넉백과 전투 대상
using ProjectEpsilon.Core; // 실제 전투 상태
using ProjectEpsilon.Enemies; // 실제 적 설정
using ProjectEpsilon.Player; // 실제 몸통과 공용 체력
using ProjectEpsilon.Progression; // 실제 경험치 보상
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 전투 씬 열기
using UnityEngine; // Unity 기능
namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay79EnemyVerification // 후반 일반 적 시험
    { // 시험 시작
        private const string Key = "EpsilonDay79Enemy"; // 시험 진행 표식
        private static bool started; // 중복 실행 방지
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 생성 경로 조회
        static AutomatedDay79EnemyVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투 준비 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 전투 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실제 전투 준비 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            try // 실제 후반 적 시험
            { // 시험 시작
                Verify(); // 실제 출현과 공격 확인
                Finish(0, "Antlion and armored thresholds, spread projectiles, resistance, normal knockback and scaling passed."); // 성공 기록
            } // 시험 종료
            catch (Exception error) // 실제 실패 확인
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 적 출현과 전투 확인
        { // 함수 시작
            Require(Enum.TryParse("Turret", out EarlyEnemyKind turret), "개미귀신 종류 누락"); // 실제 개미귀신 종류 확인
            Require(Enum.TryParse("Armored", out EarlyEnemyKind armored), "장수풍뎅이 종류 누락"); // 실제 장수풍뎅이 종류 확인
            EnemyWaveSettings wave = Resources.Load<EnemyWaveSettings>("EnemyWaveSettings"); // 실제 웨이브 데이터
            Require(wave != null && wave.AdditionalEnemies != null, "후반 적 데이터 누락"); // 실제 추가 적 설정 확인
            EarlyEnemySettings antlion = Array.Find(wave.AdditionalEnemies, item => item.kind == turret); // 실제 개미귀신 데이터
            EarlyEnemySettings beetle = Array.Find(wave.AdditionalEnemies, item => item.kind == armored); // 실제 장수풍뎅이 데이터
            Require(antlion != null && beetle != null && antlion.firstAppearance == 900f && beetle.firstAppearance == 1080f, "후반 적 최초 출현 시각 오류"); // 실제 출현 시각 확인
            Require(antlion.health == 38f && antlion.damage == 10 && antlion.experience == 3 && beetle.health == 70f && beetle.damage == 16 && beetle.experience == 4, "기획서 후반 적 능력치 오류"); // 실제 기본값 확인
            Require(antlion.enemySprite != null && beetle.enemySprite != null && antlion.enemySprite.rect.width == 1254f && beetle.enemySprite.rect.width == 1254f, "기획서 원본 이미지 누락 또는 잘림"); // 실제 이미지 확인
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 출현기
            Require(spawner != null, "실제 출현기 누락"); // 실제 전투 구성 확인
            spawner.enabled = false; // 자연 출현 중지
            FieldInfo elapsed = typeof(EarlyEnemyWaveSpawner).GetField("elapsed", Private); // 실제 경과 시각
            MethodInfo weight = typeof(EarlyEnemyWaveSpawner).GetMethod("NormalSpawnWeight", Private); // 실제 출현 가중치
            elapsed.SetValue(spawner, 899f); // 개미귀신 등장 직전
            Require((float)weight.Invoke(spawner, new object[] { antlion }) == 0f, "개미귀신 조기 출현"); // 15분 전 차단
            elapsed.SetValue(spawner, 900f); // 개미귀신 등장 시각
            Require((float)weight.Invoke(spawner, new object[] { antlion }) > 0f && (float)weight.Invoke(spawner, new object[] { beetle }) == 0f, "15분 출현 조건 오류"); // 15분 시점 확인
            elapsed.SetValue(spawner, 1080f); // 장수풍뎅이 등장 시각
            Require((float)weight.Invoke(spawner, new object[] { beetle }) > 0f, "18분 장수풍뎅이 출현 누락"); // 18분 시점 확인
            Require(Mathf.Abs(wave.ResolveMultiplier(1080f, true) - 1.49f) < 0.0001f && Mathf.Abs(wave.ResolveMultiplier(1080f, false) - 1.108f) < 0.0001f, "18분 구간이 기존 성장 보간을 변경"); // 기존 능력치 성장 유지 확인
            for (int minute = 15; minute <= 20; minute++) // 기존 15~20분 성장 확인
            { // 반복 시작
                float progress = (minute - 15f) / 5f; // 기존 구간 진행 비율
                Require(Mathf.Abs(wave.ResolveMultiplier(minute * 60f, true) - Mathf.Lerp(1.4f, 1.55f, progress)) < 0.0001f && Mathf.Abs(wave.ResolveMultiplier(minute * 60f, false) - Mathf.Lerp(1.09f, 1.12f, progress)) < 0.0001f, "기존 분 단위 적 성장 변경"); // 매 분의 기존 성장 유지 확인
            } // 반복 종료
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", Private); // 실제 적 출현 경로
            WeaponTarget antlionTarget = (WeaponTarget)spawn.Invoke(spawner, new object[] { antlion, new Vector3(10f, 0f, 0f), false }); // 실제 개미귀신 출현
            EarlyEnemyBehaviour ranged = antlionTarget.GetComponent<EarlyEnemyBehaviour>(); // 실제 사격 행동
            typeof(EarlyEnemyBehaviour).GetMethod("ShowShooterWarning", Private).Invoke(ranged, new object[] { Vector2.right * 4f }); // 실제 사격 예고 표시
            LineRenderer[] warnings = antlionTarget.GetComponentsInChildren<LineRenderer>(); // 실제 중앙과 양쪽 사격 예고
            Require(warnings.Length == 3 && Array.TrueForAll(warnings, line => line.enabled && line.sortingOrder == 40), "개미귀신 부채꼴 예고 누락"); // 실제 사격 경계 표시 확인
            EnemyTestProjectile[] previousShots = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None); // 기존 투사체 목록
            typeof(EarlyEnemyBehaviour).GetField("nextAttack", Private).SetValue(ranged, 0f); // 실제 사격 시각 도달
            typeof(EarlyEnemyBehaviour).GetMethod("FixedUpdate", Private).Invoke(ranged, null); // 실제 사격 행동 실행
            Require(Array.TrueForAll(warnings, line => !line.enabled), "발사 후 사격 예고 잔류"); // 실제 발사 후 예고 정리 확인
            EnemyTestProjectile[] shots = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None); // 실제 사격 후 투사체 목록
            Require(shots.Length - previousShots.Length == 3, "개미귀신 부채꼴 사격 누락"); // 조정 가능한 시험 사격 확인
            Vector2 firstDirection = Vector2.zero; // 첫 신규 투사체 방향
            float widest = 0f; // 실제 최대 부채각
            foreach (EnemyTestProjectile shot in shots) // 실제 투사체 순회
            { // 반복 시작
                if (Array.Exists(previousShots, previous => previous == shot)) // 기존 투사체 확인
                { // 조건 시작
                    continue; // 기존 투사체 제외
                } // 조건 종료
                Vector2 direction = (Vector2)typeof(EnemyTestProjectile).GetField("direction", Private).GetValue(shot); // 실제 발사 방향
                if (firstDirection == Vector2.zero) // 첫 신규 방향 확인
                { // 조건 시작
                    firstDirection = direction; // 실제 첫 방향 저장
                } // 조건 종료
                widest = Mathf.Max(widest, Vector2.Angle(firstDirection, direction)); // 실제 발사 각도 확인
            } // 반복 종료
            Require(widest >= 19f && widest <= 41f, "개미귀신 투사체 부채각 오류"); // 실제 여러 방향 사격 확인
            WeaponTarget armoredTarget = (WeaponTarget)spawn.Invoke(spawner, new object[] { beetle, new Vector3(12f, 0f, 0f), false }); // 실제 장수풍뎅이 출현
            WeaponTargetKnockbackController resistance = armoredTarget.GetComponent<WeaponTargetKnockbackController>(); // 실제 장수풍뎅이 넉백
            Require(resistance != null && resistance.Apply(Vector2.right, 2f, 1f) && Mathf.Approximately(resistance.RemainingDistance, 1f), "장수풍뎅이 부분 넉백 저항 오류"); // 실제 절반 거리 확인
            EnemyDropController drop = armoredTarget.GetComponent<EnemyDropController>(); // 실제 장수풍뎅이 보상
            Require(drop != null && (int)typeof(EnemyDropController).GetField("experienceValue", Private).GetValue(drop) == 4, "장수풍뎅이 경험치 보상 누락"); // 실제 경험치 보상 확인
            WeaponTargetKnockbackController normal = antlionTarget.GetComponent<WeaponTargetKnockbackController>(); // 일반 적 넉백
            Require(normal != null && normal.Apply(Vector2.right, 2f, 1f) && Mathf.Approximately(normal.RemainingDistance, 2f), "기존 일반 적 넉백 변경"); // 기존 넉백 유지 확인
            normal.Configure(true); // 기존 보스 완전 면역 설정
            Require(!normal.Apply(Vector2.right, 2f, 1f), "기존 완전 면역 변경"); // 기존 보스 면역 유지 확인
            EarlyEnemySettings[] originals = (EarlyEnemySettings[])typeof(EarlyEnemyWaveSpawner).GetField("enemies", Private).GetValue(spawner); // 실제 씬의 기존 적 설정
            WeaponTarget oldTarget = (WeaponTarget)spawn.Invoke(spawner, new object[] { originals[0], new Vector3(14f, 0f, 0f), false }); // 기존 병정개미 출현
            WeaponTargetKnockbackController oldKnockback = oldTarget.GetComponent<WeaponTargetKnockbackController>(); // 기존 적 넉백
            Require(oldKnockback.Apply(Vector2.right, 2f, 1f) && Mathf.Approximately(oldKnockback.RemainingDistance, 2f), "기존 씬 적 넉백 저항이 변경됨"); // 기존 직렬화 적 호환 확인
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 원인 전달
            } // 조건 종료
        } // 함수 종료
        private static void Finish(int code, string message) // 배치 결과 처리
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 전투 감시 해제
            Debug.Log("[Day79Enemy] " + (code == 0 ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
