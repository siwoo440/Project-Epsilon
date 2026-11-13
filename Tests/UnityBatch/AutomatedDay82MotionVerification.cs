using System; // 예외와 형식
using System.Reflection; // 실제 적 생성과 시각 경로
using ProjectEpsilon.Combat; // 실제 적 전투 대상
using ProjectEpsilon.Core; // 실제 전투 상태
using ProjectEpsilon.Enemies; // 실제 적과 보스
using ProjectEpsilon.Progression; // 실제 경험치 보상
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 전투 씬 열기
using UnityEngine; // Unity 기능
namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay82MotionVerification // 적과 보스 모션 시험
    { // 시험 시작
        private const string Key = "EpsilonDay82Motion"; // 시험 진행 표식
        private static bool started; // 중복 실행 방지
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 경로 조회
        static AutomatedDay82MotionVerification() // 도메인 재시작 처리
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
            try // 실제 적과 보스 모션 시험
            { // 시험 시작
                Verify(); // 실제 모션과 충돌 확인
                Finish(0, "Enemy and boss visual motion, hit color, attack trigger, detached death and collider preservation passed."); // 성공 결과 기록
            } // 시험 종료
            catch (Exception error) // 실제 실패 확인
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 모션과 충돌 확인
        { // 함수 시작
            Type motionType = Type.GetType("ProjectEpsilon.Enemies.EnemyVisualMotion, Assembly-CSharp"); // 실제 시각 모션 형식
            Type deathType = Type.GetType("ProjectEpsilon.Enemies.EnemyDeathVisual, Assembly-CSharp"); // 실제 사망 시각 형식
            Require(motionType != null && deathType != null, "적 시각 모션 구성 누락"); // 실제 구현 확인
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            Require(spawner != null, "실제 적 출현기 누락"); // 실제 전투 구성 확인
            spawner.enabled = false; // 자연 출현 통제
            EarlyEnemySettings[] originals = (EarlyEnemySettings[])typeof(EarlyEnemyWaveSpawner).GetField("enemies", Private).GetValue(spawner); // 실제 기본 적 설정
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", Private); // 실제 적 출현 경로
            WeaponTarget target = (WeaponTarget)spawn.Invoke(spawner, new object[] { originals[0], Vector3.right * 10f, false }); // 실제 일반 적 출현
            Component motion = target.GetComponent(motionType); // 실제 적 시각 모션
            SpriteRenderer original = target.GetComponent<SpriteRenderer>(); // 기존 적 표시기
            Transform artwork = target.transform.Find("EnemyArtwork"); // 충돌체와 분리된 실제 표시
            SpriteRenderer visual = artwork == null ? null : artwork.GetComponent<SpriteRenderer>(); // 실제 표시용 이미지
            Require(motion != null && original != null && visual != null && !original.enabled && visual.enabled && visual.sprite == original.sprite, "적 표시와 충돌체 분리 실패"); // 실제 시각 분리 확인
            Vector3 rootScale = target.transform.localScale; // 기존 충돌체 크기
            target.transform.position += Vector3.right; // 실제 적 위치 변경
            motionType.GetMethod("LateUpdate", Private).Invoke(motion, null); // 실제 이동 표시 갱신
            Require((float)motionType.GetField("movingUntil", Private).GetValue(motion) > Time.time && target.transform.localScale == rootScale, "이동 모션 또는 충돌체 크기 유지 실패"); // 실제 이동 표시와 충돌 크기 확인
            motionType.GetMethod("PlayAttack").Invoke(motion, null); // 실제 공격 모션 시작
            motionType.GetMethod("LateUpdate", Private).Invoke(motion, null); // 실제 시각 갱신
            Require(target.transform.localScale == rootScale && artwork.localScale != Vector3.one, "공격 모션이 충돌체를 변경하거나 표시되지 않음"); // 실제 공격 표시와 충돌 크기 확인
            ((Behaviour)motion).enabled = false; // 실제 모션 비활성화
            Require(original.enabled && !visual.enabled, "모션 비활성화 후 기존 표시 복구 실패"); // 기존 표시기 복구 확인
            ((Behaviour)motion).enabled = true; // 실제 모션 재활성화
            Require(!original.enabled && visual.enabled, "모션 재활성화 후 표시 분리 실패"); // 실제 표시 재연결 확인
            target.TakeDamage(1f); // 실제 적 피격
            motionType.GetMethod("LateUpdate", Private).Invoke(motion, null); // 실제 시각 색상 동기화
            Require(visual.color == original.color && visual.color.r > visual.color.g, "기존 빨간 피격 표시 동기화 실패"); // 실제 피격 표시 확인
            WeaponTargetBurnStatus burn = target.gameObject.AddComponent<WeaponTargetBurnStatus>(); // 기존 화상 상태
            Require(burn.Apply(target, 0, 2f, 1f), "기존 화상 상태 적용 실패"); // 실제 화상 적용 확인
            Require((SpriteRenderer)typeof(WeaponTargetBurnStatus).GetField("targetRenderer", Private).GetValue(burn) == original, "화상 상태가 기존 표시기를 찾지 못함"); // 기존 화상 표시 참조 확인
            typeof(WeaponTargetBurnStatus).GetMethod("Update", Private).Invoke(burn, null); // 실제 화상 색상 갱신
            motionType.GetMethod("LateUpdate", Private).Invoke(motion, null); // 실제 화상 색상 동기화
            Require(visual.color == original.color, "기존 화상 표시 동기화 실패"); // 실제 화상 표시 확인
            GameManager.Instance.PauseGame(); // 실제 전투 일시정지
            Vector3 pausedScale = artwork.localScale; // 일시정지 시 실제 표시 크기
            motionType.GetMethod("LateUpdate", Private).Invoke(motion, null); // 일시정지 중 표시 갱신
            Require(artwork.localScale == pausedScale, "일시정지 중 모션 진행"); // 실제 정지 상태 확인
            GameManager.Instance.ResumeGame(); // 실제 전투 재개
            int beforeGems = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length; // 기존 경험치 보석 수
            target.TakeDamage(100000f); // 실제 일반 적 처치
            Require(!target.IsAlive && UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length > beforeGems, "사망 모션이 기존 즉시 처치나 보상을 지연"); // 실제 즉시 처치와 보상 확인
            Component death = (Component)UnityEngine.Object.FindFirstObjectByType(deathType); // 실제 분리된 사망 시각
            Require(death != null && death.GetComponent<Collider2D>() == null && death.GetComponent<WeaponTarget>() == null, "사망 시각에 충돌 또는 전투 대상 잔류"); // 실제 무충돌 사망 표시 확인
            typeof(EnemyDeathVisual).GetField("elapsed", Private).SetValue(death, 1f); // 실제 사망 표시 시간 종료
            typeof(EnemyDeathVisual).GetMethod("Update", Private).Invoke(death, null); // 실제 사망 표시 종료 처리
            Require(death.GetComponent<SpriteRenderer>().color.a == 0f, "사망 표시 종료 후 이미지 잔류"); // 실제 사망 표시 종료 확인
            WeaponTarget shooter = (WeaponTarget)spawn.Invoke(spawner, new object[] { originals[2], Vector3.right * 10f, false }); // 실제 사수 출현
            EarlyEnemyBehaviour shooterBehaviour = shooter.GetComponent<EarlyEnemyBehaviour>(); // 실제 사수 행동
            Component shooterMotion = shooter.GetComponent(motionType); // 실제 사수 시각 모션
            typeof(EarlyEnemyBehaviour).GetField("nextAttack", Private).SetValue(shooterBehaviour, 0f); // 실제 사격 준비
            typeof(EarlyEnemyBehaviour).GetMethod("FixedUpdate", Private).Invoke(shooterBehaviour, null); // 실제 사격 실행
            Require((float)motionType.GetField("attackUntil", Private).GetValue(shooterMotion) > Time.time, "실제 사격 모션 연결 누락"); // 실제 공격 이벤트 확인
            typeof(EarlyEnemyWaveSpawner).GetMethod("EnterBossTransition", Private).Invoke(spawner, null); // 실제 보스 전환
            PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 관리자
            WeaponTarget boss = encounter == null ? null : (WeaponTarget)typeof(PrototypeBossEncounter).GetField("boss", Private).GetValue(encounter); // 실제 최종 보스
            Require(boss != null && boss.GetComponent(motionType) != null && boss.transform.Find("EnemyArtwork") != null, "실제 보스 모션 누락"); // 실제 보스 시각 구성 확인
            PrototypeBossChargeAttack charge = boss.GetComponent<PrototypeBossChargeAttack>(); // 실제 보스 돌진 행동
            typeof(PrototypeBossChargeAttack).GetMethod("BeginWarning", Private).Invoke(charge, null); // 실제 보스 돌진 예고
            typeof(PrototypeBossChargeAttack).GetField("phaseEndsAt", Private).SetValue(charge, 0f); // 실제 예고 종료 시각
            typeof(PrototypeBossChargeAttack).GetMethod("FixedUpdate", Private).Invoke(charge, null); // 실제 보스 돌진 시작
            Component bossMotion = boss.GetComponent(motionType); // 실제 보스 시각 모션
            FieldInfo attackUntil = motionType.GetField("attackUntil", Private); // 실제 공격 표시 종료 시각
            Require((float)attackUntil.GetValue(bossMotion) > Time.time, "실제 보스 돌진 모션 연결 누락"); // 실제 보스 공격 모션 확인
            attackUntil.SetValue(bossMotion, 0f); // 이전 보스 공격 표시 초기화
            PrototypeBossSummoner summoner = boss.GetComponent<PrototypeBossSummoner>(); // 실제 보스 소환 행동
            typeof(PrototypeBossSummoner).GetField("nextSummonAt", Private).SetValue(summoner, 0f); // 실제 소환 시각 도달
            typeof(PrototypeBossSummoner).GetMethod("FixedUpdate", Private).Invoke(summoner, null); // 실제 보스 소환 실행
            Require(summoner.ActiveCount > 0 && (float)attackUntil.GetValue(bossMotion) > Time.time, "실제 보스 소환 모션 연결 누락"); // 실제 보스 소환 표시 확인
            attackUntil.SetValue(bossMotion, 0f); // 이전 보스 공격 표시 초기화
            PrototypeBossHazardSpawner hazard = boss.GetComponent<PrototypeBossHazardSpawner>(); // 실제 보스 위험 구역 행동
            typeof(PrototypeBossHazardSpawner).GetField("nextHazardAt", Private).SetValue(hazard, 0f); // 실제 위험 구역 시각 도달
            typeof(PrototypeBossHazardSpawner).GetMethod("FixedUpdate", Private).Invoke(hazard, null); // 실제 위험 구역 생성
            Require(hazard.CurrentHazard != null && (float)attackUntil.GetValue(bossMotion) > Time.time, "실제 보스 위험 구역 모션 연결 누락"); // 실제 보스 위험 공격 표시 확인
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
            Debug.Log("[Day82Motion] " + (code == 0 ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
