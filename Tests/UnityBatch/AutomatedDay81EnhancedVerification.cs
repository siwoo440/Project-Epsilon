using System; // 예외와 형식
using System.Collections.Generic; // 실제 적 집합
using System.Reflection; // 실제 강화형 생성 경로
using ProjectEpsilon.Combat; // 실제 적 전투 대상
using ProjectEpsilon.Core; // 실제 전투 상태
using ProjectEpsilon.Enemies; // 실제 적 설정
using ProjectEpsilon.Progression; // 실제 엘리트 보상
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 전투 씬 열기
using UnityEngine; // Unity 기능
namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay81EnhancedVerification // 일반 적 강화형 시험
    { // 시험 시작
        private const string Key = "EpsilonDay81Enhanced"; // 시험 진행 표식
        private static bool started; // 중복 실행 방지
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 경로 조회
        static AutomatedDay81EnhancedVerification() // 도메인 재시작 처리
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
            try // 실제 강화형 시험
            { // 시험 시작
                Verify(); // 실제 강화형 생성과 보상 확인
                Finish(0, "Eight enhanced variants, original behavior, scaling, rewards and scheduled selection passed."); // 성공 결과 기록
            } // 시험 종료
            catch (Exception error) // 실제 실패 확인
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 강화형 생성과 보상 확인
        { // 함수 시작
            EnemyWaveSettings wave = Resources.Load<EnemyWaveSettings>("EnemyWaveSettings"); // 실제 웨이브 데이터
            Require(wave != null, "실제 웨이브 데이터 누락"); // 실제 설정 확인
            PropertyInfo chance = typeof(EnemyWaveSettings).GetProperty("EnhancedEliteChance"); // 실제 강화형 확률
            Require(chance != null && Mathf.Approximately((float)chance.GetValue(wave), 0.25f), "강화형 확률 데이터 누락"); // 실제 시험 확률 확인
            MethodInfo sprite = typeof(EnemyWaveSettings).GetMethod("GetEnhancedSprite"); // 종류별 원본 이미지 조회
            Require(sprite != null, "강화형 이미지 설정 누락"); // 실제 이미지 설정 확인
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            Require(spawner != null, "실제 적 출현기 누락"); // 실제 전투 구성 확인
            spawner.enabled = false; // 자연 출현 통제
            typeof(EarlyEnemyWaveSpawner).GetField("elapsed", Private).SetValue(spawner, 1080f); // 모든 일반 적 해금 시각
            EarlyEnemySettings[] originals = (EarlyEnemySettings[])typeof(EarlyEnemyWaveSpawner).GetField("enemies", Private).GetValue(spawner); // 실제 씬 기본 적
            List<EarlyEnemySettings> sources = new List<EarlyEnemySettings> { originals[0], originals[1], originals[2] }; // 기존 세 적 종류
            sources.AddRange(wave.AdditionalEnemies); // 후속 다섯 일반 적 종류
            Require(sources.Count == 8, "일반 적 여덟 종류 누락"); // 실제 원본 적 수 확인
            MethodInfo make = typeof(EarlyEnemyWaveSpawner).GetMethod("CreateEnhancedElite", Private); // 실제 강화형 생성 경로
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", Private); // 실제 적 출현 경로
            FieldInfo enhanced = typeof(EarlyEnemySettings).GetField("isEnhancedElite"); // 실제 강화형 분류
            Require(make != null && spawn != null && enhanced != null, "강화형 출현 경로 누락"); // 실제 구현 확인
            HashSet<WeaponTarget> elites = (HashSet<WeaponTarget>)typeof(EarlyEnemyWaveSpawner).GetField("eliteTargets", Private).GetValue(spawner); // 실제 엘리트 상한 집합
            EliteRewardController rewards = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 실제 선택 보상
            Require(rewards != null, "실제 엘리트 보상 누락"); // 실제 보상 구성 확인
            foreach (EarlyEnemySettings source in sources) // 실제 여덟 종류 순회
            { // 반복 시작
                Sprite artwork = (Sprite)sprite.Invoke(wave, new object[] { source.kind }); // 실제 강화형 원본 이미지
                Require(artwork != null && artwork.rect.width == 1254f && artwork.rect.height == 1254f, "강화형 원본 이미지 누락 또는 잘림"); // 실제 전체 이미지 확인
                EarlyEnemySettings variant = (EarlyEnemySettings)make.Invoke(spawner, new object[] { source }); // 실제 기존 적 강화형 생성
                Require(variant != null && variant.kind == source.kind && (bool)enhanced.GetValue(variant) && variant.enemySprite == artwork && variant.experience == 20, "기존 적 행동 또는 강화형 보상 설정 오류"); // 실제 행동과 보상 확인
                Require(Mathf.Approximately(variant.health, Mathf.Ceil(source.health * 3f)) && variant.damage == Mathf.CeilToInt(source.damage * 1.5f), "조정 가능한 강화형 능력치 오류"); // 실제 기본 강화 배율 확인
                WeaponTarget target = (WeaponTarget)spawn.Invoke(spawner, new object[] { variant, Vector3.right * 10f, false }); // 실제 강화형 적 출현
                Require(target != null && elites.Contains(target) && target.GetComponent<EnemyDropController>() != null, "강화형 엘리트 상한 또는 드롭 누락"); // 실제 엘리트 분류 확인
                EarlyEnemyBehaviour behaviour = target.GetComponent<EarlyEnemyBehaviour>(); // 실제 강화형 행동
                EarlyEnemySettings actual = (EarlyEnemySettings)typeof(EarlyEnemyBehaviour).GetField("settings", Private).GetValue(behaviour); // 실제 강화형 실행 설정
                Require(actual.kind == source.kind && (bool)enhanced.GetValue(actual) && target.GetComponent<SpriteRenderer>().sprite == artwork && Mathf.Approximately(target.MaximumHealth, Mathf.Ceil(variant.health * wave.ResolveMultiplier(1080f, true))) && actual.damage == Mathf.CeilToInt(variant.damage * wave.ResolveMultiplier(1080f, false)), "강화형 실행 행동 또는 시간별 능력치 오류"); // 실제 원본 행동과 시간 성장 확인
                if (source.kind == EarlyEnemyKind.Shooter || source.kind == EarlyEnemyKind.Turret) // 실제 사격 적 확인
                { // 조건 시작
                    int previousShots = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None).Length; // 기존 투사체 수
                    typeof(EarlyEnemyBehaviour).GetField("nextAttack", Private).SetValue(behaviour, 0f); // 실제 사격 준비
                    typeof(EarlyEnemyBehaviour).GetMethod("FixedUpdate", Private).Invoke(behaviour, null); // 실제 강화형 사격
                    Require(UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None).Length - previousShots == source.projectileCount, "강화형 기존 사격 행동 누락"); // 원본 사격 행동 확인
                } // 조건 종료
                if (source.kind == EarlyEnemyKind.Charger) // 실제 돌진 적 확인
                { // 조건 시작
                    typeof(EarlyEnemyBehaviour).GetField("nextAttack", Private).SetValue(behaviour, 0f); // 실제 돌진 준비
                    typeof(EarlyEnemyBehaviour).GetMethod("FixedUpdate", Private).Invoke(behaviour, null); // 실제 강화형 돌진 예고
                    Require((int)typeof(EarlyEnemyBehaviour).GetField("chargePhase", Private).GetValue(behaviour) == 1, "강화형 기존 돌진 예고 누락"); // 원본 돌진 행동 확인
                } // 조건 종료
                if (source.kind == EarlyEnemyKind.Disruptor) // 실제 감속 적 확인
                { // 조건 시작
                    typeof(EarlyEnemyBehaviour).GetField("nextSlowZone", Private).SetValue(behaviour, 0f); // 실제 지대 생성 준비
                    typeof(EarlyEnemyBehaviour).GetMethod("FixedUpdate", Private).Invoke(behaviour, null); // 실제 강화형 지대 생성
                    Require(EnemySlowZone.SpeedMultiplierAt(target.transform.position) < 1f, "강화형 기존 감속 지대 누락"); // 원본 감속 행동 확인
                } // 조건 종료
                if (source.kind == EarlyEnemyKind.Armored) // 실제 장갑 적 확인
                { // 조건 시작
                    WeaponTargetKnockbackController knockback = target.GetComponent<WeaponTargetKnockbackController>(); // 실제 강화형 넉백
                    Require(knockback.Apply(Vector2.right, 2f, 1f) && Mathf.Approximately(knockback.RemainingDistance, 1f), "강화형 기존 넉백 저항 누락"); // 원본 넉백 저항 확인
                } // 조건 종료
                int previousRewards = rewards.PendingRewardCount; // 기존 보상 대기 수
                int previousGems = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length; // 기존 경험치 보석 수
                target.TakeDamage(100000f); // 실제 강화형 처치
                Require(rewards.PendingRewardCount == previousRewards + 1 && !elites.Contains(target), "강화형 처치 선택 보상 또는 상한 해제 실패"); // 실제 보상과 정리 확인
                ExperiencePickup[] gems = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None); // 실제 처치 보석 목록
                Require(gems.Length == previousGems + 1 && Array.Exists(gems, gem => gem.ExperienceValue == 20), "강화형 대형 경험치 보석 누락"); // 실제 엘리트 경험치 보상 확인
            } // 반복 종료
            typeof(EnemyWaveSettings).GetField("enhancedEliteChance", Private).SetValue(wave, 1f); // 실제 강화형 선택 강제
            typeof(EarlyEnemyWaveSpawner).GetField("elapsed", Private).SetValue(spawner, 240f); // 실제 첫 엘리트 경계
            typeof(EarlyEnemyWaveSpawner).GetField("nextEliteRound", Private).SetValue(spawner, 0); // 실제 첫 회차
            typeof(EarlyEnemyWaveSpawner).GetMethod("ProcessEliteSchedule", Private).Invoke(spawner, null); // 실제 첫 회차 실행
            Require(elites.Count == 1, "첫 회차 엘리트 출현 누락"); // 실제 첫 엘리트 확인
            WeaponTarget first = null; // 실제 첫 엘리트 보관
            foreach (WeaponTarget target in elites) // 실제 첫 회차 엘리트 확인
            { // 반복 시작
                EarlyEnemySettings actual = (EarlyEnemySettings)typeof(EarlyEnemyBehaviour).GetField("settings", Private).GetValue(target.GetComponent<EarlyEnemyBehaviour>()); // 실제 첫 회차 설정
                Require(actual.kind == EarlyEnemyKind.Elite && actual.elitePattern == ElitePattern.Charge && !(bool)enhanced.GetValue(actual), "첫 회차 돌진 엘리트 변경"); // 첫 회차 고정 규칙 확인
                first = target; // 실제 첫 엘리트 저장
            } // 반복 종료
            first.TakeDamage(100000f); // 실제 첫 엘리트 처치
            typeof(EarlyEnemyWaveSpawner).GetField("elapsed", Private).SetValue(spawner, 480f); // 실제 8분 경계
            typeof(EarlyEnemyWaveSpawner).GetMethod("ProcessEliteSchedule", Private).Invoke(spawner, null); // 실제 회차 추첨 실행
            Require(elites.Count == 1, "강화형 실제 회차 출현 누락"); // 실제 단일 엘리트 출현 확인
            foreach (WeaponTarget target in elites) // 실제 회차 엘리트 확인
            { // 반복 시작
                EarlyEnemyBehaviour behaviour = target.GetComponent<EarlyEnemyBehaviour>(); // 실제 엘리트 행동
                EarlyEnemySettings actual = (EarlyEnemySettings)typeof(EarlyEnemyBehaviour).GetField("settings", Private).GetValue(behaviour); // 실제 출현 설정
                Require((bool)enhanced.GetValue(actual) && actual.firstAppearance <= 480f, "회차 강화형 해금 조건 오류"); // 실제 해금 적 강화 확인
            } // 반복 종료
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
            Debug.Log("[Day81Enhanced] " + (code == 0 ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
