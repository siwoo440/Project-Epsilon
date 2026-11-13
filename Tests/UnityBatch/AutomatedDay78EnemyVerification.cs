using System; // 예외와 형식
using System.Reflection; // 실제 적 생성과 시점 조회
using ProjectEpsilon.Combat; // 실제 전투 대상
using ProjectEpsilon.Core; // 실제 전투 상태
using ProjectEpsilon.Enemies; // 실제 적 설정
using ProjectEpsilon.Player; // 실제 이동과 체력
using ProjectEpsilon.Progression; // 실제 적 처치 보상
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 전투 씬 열기
using UnityEngine; // Unity 기능
namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay78EnemyVerification // 일반 적 확장 시험
    { // 시험 시작
        private const string Key = "EpsilonDay78Enemy"; // 시험 진행 표식
        private static bool started; // 중복 실행 방지
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 생성 경로 조회
        static AutomatedDay78EnemyVerification() // 도메인 재시작 처리
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
            try // 실제 적 확장 시험
            { // 시험 시작
                Verify(); // 실제 출현과 감속 확인
                Finish(0, "Spawn thresholds and selection, sprites, enemy slow creation and overlap, terrain immunity, dragonfly body damage and reward configuration passed."); // 성공 기록
            } // 시험 종료
            catch (Exception error) // 실제 실패 확인
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 출현과 감속 확인
        { // 함수 시작
            Require(Enum.TryParse("Disruptor", out EarlyEnemyKind disruptor), "노린재 종류 누락"); // 실제 노린재 종류 확인
            Require(Enum.TryParse("Flyer", out EarlyEnemyKind flyer), "잠자리 종류 누락"); // 실제 잠자리 종류 확인
            EnemyWaveSettings wave = Resources.Load<EnemyWaveSettings>("EnemyWaveSettings"); // 실제 출현 데이터
            Require(wave != null && wave.AdditionalEnemies != null, "추가 적 데이터 누락"); // 실제 설정 확인
            EarlyEnemySettings stink = Array.Find(wave.AdditionalEnemies, item => item.kind == disruptor); // 실제 노린재 설정
            EarlyEnemySettings dragonfly = Array.Find(wave.AdditionalEnemies, item => item.kind == flyer); // 실제 잠자리 설정
            Require(stink != null && dragonfly != null && stink.firstAppearance == 600f && dragonfly.firstAppearance == 900f, "실제 최초 출현 시각 오류"); // 실제 출현 시각 확인
            Require(stink.health == 28f && stink.damage == 8 && stink.experience == 2 && dragonfly.health == 22f && dragonfly.damage == 9 && dragonfly.experience == 2, "기획서 능력치 오류"); // 실제 기본값 확인
            Require(stink.enemySprite != null && dragonfly.enemySprite != null, "기획서 적 이미지 누락"); // 실제 외형 확인
            Require(stink.enemySprite.rect.width == 1254f && stink.enemySprite.rect.height == 1254f && dragonfly.enemySprite.rect.width == 1254f && dragonfly.enemySprite.rect.height == 1254f, "기획서 원본 적 이미지가 잘림"); // 실제 전체 원본 이미지 확인
            Require(wave.Resolve(600f).GetSpawnWeight(disruptor) == 10f && wave.Resolve(900f).GetSpawnWeight(flyer) == 10f, "신규 적 출현 가중치 오류"); // 실제 가중치 확인
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어
            Require(spawner != null && player != null, "실제 전투 구성 누락"); // 실제 전투 구성 확인
            spawner.enabled = false; // 자연 출현 중지
            FieldInfo elapsed = typeof(EarlyEnemyWaveSpawner).GetField("elapsed", Private); // 실제 전투 시각 필드
            MethodInfo weight = typeof(EarlyEnemyWaveSpawner).GetMethod("NormalSpawnWeight", Private); // 실제 출현 가중치 경로
            elapsed.SetValue(spawner, 599f); // 노린재 등장 직전
            Require((float)weight.Invoke(spawner, new object[] { stink }) == 0f, "노린재 조기 출현"); // 최초 시각 확인
            elapsed.SetValue(spawner, 600f); // 노린재 등장 시각
            Require((float)weight.Invoke(spawner, new object[] { stink }) > 0f && (float)weight.Invoke(spawner, new object[] { dragonfly }) == 0f, "10분 출현 조건 오류"); // 10분 시점 확인
            elapsed.SetValue(spawner, 900f); // 잠자리 등장 시각
            Require((float)weight.Invoke(spawner, new object[] { dragonfly }) > 0f, "15분 잠자리 출현 누락"); // 15분 시점 확인
            UnityEngine.Random.InitState(78); // 재현 가능한 실제 출현 추첨
            MethodInfo choose = typeof(EarlyEnemyWaveSpawner).GetMethod("SelectNormalEnemy", Private); // 실제 일반 적 추첨 경로
            bool selectedStink = false; // 실제 노린재 추첨 여부
            bool selectedFlyer = false; // 실제 잠자리 추첨 여부
            for (int index = 0; index < 200; index++) // 실제 웨이브 추첨 반복
            { // 반복 시작
                EarlyEnemySettings selected = (EarlyEnemySettings)choose.Invoke(spawner, null); // 실제 추첨 결과
                selectedStink |= selected == stink; // 노린재 실제 출현 후보 확인
                selectedFlyer |= selected == dragonfly; // 잠자리 실제 출현 후보 확인
            } // 반복 종료
            Require(selectedStink && selectedFlyer, "실제 웨이브 추첨에서 신규 적 누락"); // 실제 추첨 후보 확인
            Type zone = Type.GetType("ProjectEpsilon.Enemies.EnemySlowZone, Assembly-CSharp"); // 실제 적 감속 지대
            Require(zone != null, "적 감속 지대 누락"); // 실제 구현 확인
            MethodInfo create = zone.GetMethod("Create"); // 실제 지대 생성 경로
            MethodInfo multiplier = zone.GetMethod("SpeedMultiplierAt"); // 실제 지대 배율 조회
            float baseline = player.CurrentMoveSpeed; // 기존 이동 속도
            object firstZone = create.Invoke(null, new object[] { player.transform.position, 1.3f, 0.8f, 3f }); // 첫 번째 적 감속 지대
            object secondZone = create.Invoke(null, new object[] { player.transform.position, 1.3f, 0.8f, 3f }); // 겹친 두 번째 적 지대
            Require(Mathf.Approximately((float)multiplier.Invoke(null, new object[] { player.transform.position }), 0.8f), "적 지대 감속 중첩"); // 가장 강한 단일 감속 확인
            player.IgnoreTerrainSlow(5f); // 기존 지형 감속 면역
            Require(Mathf.Approximately(player.CurrentMoveSpeed, baseline * 0.8f), "지형 면역이 적 감속까지 해제"); // 기존 면역 범위 확인
            FieldInfo expires = zone.GetField("expiresAt", Private); // 실제 지대 종료 시각
            Require((float)expires.GetValue(firstZone) >= Time.time + 2.9f, "적 지대 지속 시간 오류"); // 기획서 지속 시간 확인
            expires.SetValue(firstZone, Time.time - 0.1f); // 첫 지대 지속 시간 종료
            Require(Mathf.Approximately((float)multiplier.Invoke(null, new object[] { player.transform.position }), 0.8f), "남은 적 지대 감속 누락"); // 남은 지대 단일 감속 확인
            expires.SetValue(secondZone, Time.time - 0.1f); // 두 번째 지대 지속 시간 종료
            Require(Mathf.Approximately((float)multiplier.Invoke(null, new object[] { player.transform.position }), 1f), "종료된 적 지대 감속 잔류"); // 지속 시간 종료 확인
            zone.GetMethod("ClearAll").Invoke(null, null); // 기존 적 지대 정리
            Require(Mathf.Approximately((float)multiplier.Invoke(null, new object[] { player.transform.position }), 1f), "적 지대 정리 후 감속 잔류"); // 실제 지대 정리 확인
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", Private); // 실제 적 출현 경로
            WeaponTarget stinkTarget = (WeaponTarget)spawn.Invoke(spawner, new object[] { stink, player.transform.position, false }); // 실제 노린재 출현
            EarlyEnemyBehaviour stinkBehaviour = stinkTarget.GetComponent<EarlyEnemyBehaviour>(); // 실제 노린재 행동
            typeof(EarlyEnemyBehaviour).GetField("nextSlowZone", Private).SetValue(stinkBehaviour, 0f); // 실제 지대 생성 시각 도달
            typeof(EarlyEnemyBehaviour).GetMethod("FixedUpdate", Private).Invoke(stinkBehaviour, null); // 실제 노린재 행동 실행
            Require(Mathf.Approximately((float)multiplier.Invoke(null, new object[] { player.transform.position }), 0.8f), "실제 노린재 감속 지대 생성 실패"); // 실제 공격 지대 확인
            float nextZone = (float)typeof(EarlyEnemyBehaviour).GetField("nextSlowZone", Private).GetValue(stinkBehaviour); // 실제 다음 지대 생성 시각
            Require(nextZone >= Time.time + 2.7f, "노린재 지대 재생성 간격 오류"); // 실제 공격 간격 확인
            WeaponTarget target = (WeaponTarget)spawn.Invoke(spawner, new object[] { dragonfly, player.transform.position + Vector3.right * 5f, false }); // 실제 잠자리 출현
            Require(target != null && target.GetComponent<Collider2D>().isTrigger && target.GetComponent<EarlyEnemyBehaviour>() != null, "잠자리 통과 또는 실제 전투 구성 누락"); // 실제 접촉과 이동 구성 확인
            EnemyDropController drop = target.GetComponent<EnemyDropController>(); // 실제 잠자리 보상 구성
            Require(drop != null && (int)typeof(EnemyDropController).GetField("experienceValue", Private).GetValue(drop) == 2, "잠자리 경험치 보상 누락"); // 기존 경험치 보상 확인
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 플레이어 몸통
            SnakeHealth health = player.GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeExternalCollision resolver = player.GetComponent<SnakeExternalCollision>(); // 실제 피해 처리기
            Collider2D segmentCollider = body.BodySegments[0].GetComponent<Collider2D>(); // 실제 첫 몸통 충돌체
            Require(health != null && resolver != null && segmentCollider != null, "실제 몸통 피해 구성 누락"); // 실제 접촉 구성 확인
            int beforeHealth = health.CurrentHealth; // 실제 피해 전 체력
            EarlyEnemyBehaviour flyerBehaviour = target.GetComponent<EarlyEnemyBehaviour>(); // 실제 잠자리 행동
            typeof(EarlyEnemyBehaviour).GetField("nextAttack", Private).SetValue(flyerBehaviour, 0f); // 실제 접촉 공격 준비
            typeof(EarlyEnemyBehaviour).GetMethod("OnTriggerStay2D", Private).Invoke(flyerBehaviour, new object[] { segmentCollider }); // 실제 몸통 접촉 처리
            typeof(SnakeExternalCollision).GetMethod("ResolvePendingContacts", Private).Invoke(resolver, null); // 기존 충돌 우선순위 처리
            Require(health.CurrentHealth < beforeHealth, "잠자리 몸통 접촉 피해 누락"); // 실제 공용 체력 피해 확인
            typeof(EarlyEnemyWaveSpawner).GetMethod("EnterBossTransition", Private).Invoke(spawner, null); // 실제 보스 전환
            Require(Mathf.Approximately((float)multiplier.Invoke(null, new object[] { player.transform.position }), 1f), "보스 전환 후 적 지대 잔류"); // 실제 보스 전환 지대 정리 확인
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
            Debug.Log("[Day78Enemy] " + (code == 0 ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
