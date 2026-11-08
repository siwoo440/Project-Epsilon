using System; // 기본 형식
using System.Collections; // 코루틴 형식
using System.Collections.Generic; // 시험 기록 목록
using System.Reflection; // 실제 출현 설정 조회
using ProjectEpsilon.Combat; // 무기와 공격 대상
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현
using ProjectEpsilon.Player; // 플레이어와 몸통
using ProjectEpsilon.Progression; // 경험치 보석
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedEarlyWeaponVerification // 초반 무기와 적 경험치 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonEarlyWeaponVerification"; // 배치 진행 표식
        private static double started; // 플레이 시작 시각
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지

        static AutomatedEarlyWeaponVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 진행 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 시작 감시 연결
            } // 조건 끝
        } // 생성자 끝

        public static void Run() // Unity 배치 진입점
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 배치 진행 표식 저장
            EditorApplication.update -= Tick; // 중복 연결 방지
            EditorApplication.update += Tick; // 시작 감시 연결
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        } // 메서드 끝

        private static void Tick() // 플레이 진입과 제한 시간 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 플레이 대기
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 시작 무기 화면 처리
            { // 조건 시작
                return; // 전투 시작 대기
            } // 조건 끝
            if (started <= 0d) // 최초 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 시작 시각 저장
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 120d) // 전체 시험 제한 확인
            { // 조건 시작
                Finish(1, "Timed out waiting for controlled weapon test."); // 시간 초과 기록
                return; // 시험 중단
            } // 조건 끝
            if (injected) // 실행기 생성 확인
            { // 조건 시작
                return; // 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || Time.time < 0.35f) // 씬 초기화 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 실행기 중복 방지
            player.gameObject.AddComponent<EarlyWeaponVerificationRunner>(); // 실제 씬 시험 실행
        } // 메서드 끝

        public static void Finish(int code, string message) // 결과 기록 및 종료
        { // 메서드 시작
            if (finished) // 종료 중복 확인
            { // 조건 시작
                return; // 중복 종료 방지
            } // 조건 끝
            finished = true; // 종료 상태 기록
            SessionState.SetBool(ActiveKey, false); // 진행 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log($"[EarlyWeaponVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 종료 코드 반환
        } // 메서드 끝
    } // 클래스 끝

    public sealed class EarlyWeaponVerificationRunner : MonoBehaviour // 실제 자동 공격 시험 실행기
    { // 클래스 시작
        private sealed class HitRecord // 실제 명중 기록
        { // 클래스 시작
            public WeaponTarget Target; // 실제 명중 대상
            public float Time; // 명중 게임 시각
            public float Damage; // 실제 직접 피해
            public float RemainingHealth; // 명중 직후 실제 체력
            public Vector3 Origin; // 공격 원점
        } // 클래스 끝

        private static readonly BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance; // 내부 설정 조회
        private readonly List<float> attackTimes = new List<float>(); // 실제 발사 시각
        private readonly List<Vector3> attackOrigins = new List<Vector3>(); // 실제 공격 원점
        private readonly List<HitRecord> hits = new List<HitRecord>(); // 실제 명중 목록
        private readonly List<GameObject> objects = new List<GameObject>(); // 통제 대상 목록
        private SnakeMovement movement; // 실제 이동 처리기
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeBodyFollower follower; // 실제 몸통 추적기
        private SnakeWeaponManager weapons; // 실제 무기 관리자
        private WeaponAttributeEffectHooks hooks; // 실제 공격 및 명중 알림
        private EarlyEnemyWaveSpawner spawner; // 실제 적 출현기
        private SnakeExperience experience; // 실제 경험치 관리자
        private WeaponData activeWeapon; // 현재 시험 무기
        private Vector3 origin; // 실제 시험 몸통 위치

        private IEnumerator Start() // 실제 씬 검증 순서
        { // 코루틴 시작
            Prepare(); // 시험 씬과 몸통 준비
            yield return VerifyWeapon("DebugBlade", WeaponAttackType.Melee, WeaponAttribute.Physical, 10f, 0.8f, 1); // 강철 검 시험값
            yield return VerifyWeapon("DebugNeedle", WeaponAttackType.StraightProjectile, WeaponAttribute.Physical, 8f, 1f, 2); // 강철 창 시험값
            yield return VerifyWeapon("DebugNova", WeaponAttackType.Area, WeaponAttribute.Physical, 7f, 1.4f, 3); // 철퇴 시험값
            yield return VerifyWeapon("DebugCutter", WeaponAttackType.Melee, WeaponAttribute.Fire, 8f, 0.9f, 1); // 화염 검 시험값
            yield return VerifyWeapon("DebugBlaster", WeaponAttackType.StraightProjectile, WeaponAttribute.Fire, 7f, 0.8f, 1); // 화염탄 시험값
            yield return VerifyWeapon("DebugPulse", WeaponAttackType.Area, WeaponAttribute.Fire, 5f, 1.2f, 2); // 화염 고리 시험값
            yield return VerifyWeapon("PrototypePhysicalDagger", WeaponAttackType.Melee, WeaponAttribute.Physical, 6f, 0.55f, 1); // 물리 단검 임시 공격 확인
            yield return VerifyWeapon("PrototypePhysicalJavelin", WeaponAttackType.StraightProjectile, WeaponAttribute.Physical, 6f, 0.7f, 1); // 물리 투창 임시 공격 확인
            yield return VerifyWeapon("PrototypePhysicalHammer", WeaponAttackType.Area, WeaponAttribute.Physical, 11f, 1.8f, 3); // 물리 망치 임시 공격 확인
            yield return VerifyWeapon("PrototypeFireBrand", WeaponAttackType.Melee, WeaponAttribute.Fire, 12f, 1.3f, 1); // 화염 곤봉 임시 공격 확인
            yield return VerifyWeapon("PrototypeFireLance", WeaponAttackType.StraightProjectile, WeaponAttribute.Fire, 9f, 1.2f, 2); // 화염 창 임시 공격 확인
            yield return VerifyWeapon("PrototypeFireBurst", WeaponAttackType.Area, WeaponAttribute.Fire, 8f, 1.6f, 3); // 화염 파동 임시 공격 확인
            yield return VerifyWeapon("PrototypeColdBlade", WeaponAttackType.Melee, WeaponAttribute.Cold, 9f, 0.9f, 1); // 냉기 검 임시 공격 확인
            yield return VerifyWeapon("PrototypeColdSpear", WeaponAttackType.StraightProjectile, WeaponAttribute.Cold, 7f, 1f, 2); // 냉기 창 임시 공격 확인
            yield return VerifyWeapon("PrototypeColdAura", WeaponAttackType.Area, WeaponAttribute.Cold, 6f, 1.4f, 3); // 냉기 고리 임시 공격 확인
            yield return VerifyWeapon("PrototypeColdDagger", WeaponAttackType.Melee, WeaponAttribute.Cold, 6f, 0.55f, 1); // 냉기 단검 임시 공격 확인
            yield return VerifyWeapon("PrototypeColdBolt", WeaponAttackType.StraightProjectile, WeaponAttribute.Cold, 8f, 0.85f, 1); // 냉기 탄 임시 공격 확인
            yield return VerifyWeapon("PrototypeColdWave", WeaponAttackType.Area, WeaponAttribute.Cold, 9f, 1.6f, 3); // 냉기 파동 임시 공격 확인
            yield return VerifyWeapon("PrototypeElectricBlade", WeaponAttackType.Melee, WeaponAttribute.Electric, 9f, 0.9f, 1); // 전기 검 임시 공격 확인
            yield return VerifyWeapon("PrototypeElectricSpear", WeaponAttackType.StraightProjectile, WeaponAttribute.Electric, 7f, 1f, 2); // 전기 창 임시 공격 확인
            yield return VerifyWeapon("PrototypeElectricAura", WeaponAttackType.Area, WeaponAttribute.Electric, 6f, 1.4f, 3); // 전기 고리 임시 공격 확인
            yield return VerifyWeapon("PrototypeElectricDagger", WeaponAttackType.Melee, WeaponAttribute.Electric, 6f, 0.55f, 1); // 전기 단검 임시 공격 확인
            yield return VerifyWeapon("PrototypeElectricBolt", WeaponAttackType.StraightProjectile, WeaponAttribute.Electric, 8f, 0.85f, 1); // 전기 탄 임시 공격 확인
            yield return VerifyWeapon("PrototypeElectricWave", WeaponAttackType.Area, WeaponAttribute.Electric, 9f, 1.6f, 3); // 전기 파동 임시 공격 확인
            VerifyRewardPool(); // 실제 보상 풀 무기 확인
            ClearObjects(); // 시험 표적과 투사체 제거
            weapons.enabled = false; // 경험치 시험 중 자동 공격 중지
            yield return null; // 지연 객체 제거 대기
            yield return VerifyEnemyExperience(); // 실제 적 생성과 보석 획득
            hooks.AttackTriggered -= OnAttack; // 공격 기록 구독 해제
            hooks.HitTriggered -= OnHit; // 명중 기록 구독 해제
            AutomatedEarlyWeaponVerification.Finish(0, "Twenty-four weapon assets, reward pool, body origins, direct damage, cooldown, cold status, projectile contacts and three enemy XP drops/collection."); // 전체 성공 기록
        } // 코루틴 끝

        private void VerifyRewardPool() // 실제 레벨업 및 엘리트 공통 무기 풀 확인
        { // 메서드 시작
            WeaponRewardPool pool = AssetDatabase.LoadAssetAtPath<WeaponRewardPool>("Assets/Data/Progression/DebugWeaponRewardPool.asset"); // 실제 보상 풀 데이터
            Require(pool != null && pool.Entries.Count == 24, "Reward pool must contain twenty-four weapons."); // 보상 풀 크기 확인
            HashSet<string> ids = new HashSet<string>(); // 고유 무기 식별자 목록
            int physical = 0; // 물리 무기 수
            int fire = 0; // 화염 무기 수
            int cold = 0; // 냉기 무기 수
            int electric = 0; // 전기 무기 수
            foreach (WeaponRewardEntry entry in pool.Entries) // 실제 보상 후보 순회
            { // 반복 시작
                Require(entry != null && entry.Unlocked && entry.Weapon != null && ids.Add(entry.Weapon.Id), "Reward pool has missing, locked or duplicate weapon."); // 후보 유효성과 식별자 중복 확인
                physical += entry.Weapon.Attribute == WeaponAttribute.Physical ? 1 : 0; // 물리 속성 수 집계
                fire += entry.Weapon.Attribute == WeaponAttribute.Fire ? 1 : 0; // 화염 속성 수 집계
                cold += entry.Weapon.Attribute == WeaponAttribute.Cold ? 1 : 0; // 냉기 속성 수 집계
                electric += entry.Weapon.Attribute == WeaponAttribute.Electric ? 1 : 0; // 전기 속성 수 집계
            } // 반복 끝
            Require(physical == 6 && fire == 6 && cold == 6 && electric == 6, "Reward pool does not contain six weapons per attribute."); // 속성별 무기 수 확인
        } // 메서드 끝

        private void Prepare() // 실제 씬과 통제 조건 준비
        { // 메서드 시작
            movement = GetComponent<SnakeMovement>(); // 실제 플레이어 이동 조회
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 조회
            follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적 조회
            weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자 조회
            hooks = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeEffectHooks>(); // 실제 공격 알림 조회
            spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기 조회
            experience = UnityEngine.Object.FindFirstObjectByType<SnakeExperience>(); // 실제 경험치 조회
            Require(movement != null && body != null && follower != null && weapons != null && hooks != null && hooks.IsConfigured && spawner != null && experience != null && body.CurrentBodyCount == 3 && weapons.SlotCount == 3, "Required initialized Game scene components are missing."); // 필수 씬 구성 확인
            movement.enabled = false; // 플레이어 이동 정지
            follower.enabled = false; // 몸통 시험 위치 고정
            spawner.enabled = false; // 자연 적 출현 중지
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 자기 충돌 구성 조회
            if (self != null) // 자기 충돌 존재 확인
            { // 조건 시작
                self.enabled = false; // 통제 시험의 자기 충돌 방지
            } // 조건 끝
            foreach (WeaponTarget target in UnityEngine.Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 전투 대상 순회
            { // 반복 시작
                target.gameObject.SetActive(false); // 통제 시험 외 대상 비활성화
            } // 반복 끝
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 전체 몸통 슬롯 순회
            { // 반복 시작
                slot.Clear(); // 다른 무기 제거
            } // 반복 끝
            origin = movement.transform.position + Vector3.right * 20f; // 머리 사거리 밖 몸통 위치
            weapons.Slots[2].Origin.position = origin; // 실제 세 번째 몸통 이동
            Physics2D.SyncTransforms(); // 실제 물리 위치 동기화
            hooks.AttackTriggered += OnAttack; // 실제 공격 알림 기록
            hooks.HitTriggered += OnHit; // 실제 명중 알림 기록
            Debug.Log("[EarlyWeaponVerification] Actual Game scene; natural spawning, player movement and body following disabled; third body positioned outside head range; controlled stationary targets and forced enemy deaths."); // 통제 조건 기록
        } // 메서드 끝

        private IEnumerator VerifyWeapon(string assetName, WeaponAttackType type, WeaponAttribute attribute, float damage, float interval, int expectedHits) // 무기 하나의 자동 공격 검증
        { // 코루틴 시작
            ClearObjects(); // 이전 표적과 투사체 제거
            activeWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/" + assetName + ".asset"); // 실제 시험용 무기 데이터
            Require(activeWeapon != null && activeWeapon.AttackType == type && activeWeapon.Attribute == attribute && Mathf.Approximately(activeWeapon.BaseDamage, damage) && Mathf.Approximately(activeWeapon.AttackInterval, interval), "Specification or asset mismatch for " + assetName); // 기획 임시값 및 유형 확인
            if (assetName == "DebugNeedle") // 강철 창 확인
            { // 조건 시작
                Require(activeWeapon.ProjectileHitLimit == 2, "Steel spear piercing setting is not two."); // 관통 임시값 확인
            } // 조건 끝
            Require(weapons.TryEquipAt(2, activeWeapon, 1), "Could not equip test weapon: " + assetName); // 실제 몸통 슬롯 장착
            attackTimes.Clear(); // 이전 공격 시각 초기화
            attackOrigins.Clear(); // 이전 공격 원점 초기화
            hits.Clear(); // 이전 명중 기록 초기화
            WeaponTarget outside = CreateTarget(origin + Vector3.right * (activeWeapon.Range + 1f), "Outside_" + assetName); // 몸통 범위 밖 대상
            Physics2D.SyncTransforms(); // 물리 위치 동기화
            yield return null; // 실제 자동 공격 갱신
            yield return null; // 추가 갱신
            Require(attackTimes.Count == 0 && Mathf.Approximately(outside.CurrentHealth, outside.MaximumHealth), "Weapon attacked while only an out-of-range target existed: " + assetName); // 사거리 밖 비공격 확인
            outside.gameObject.SetActive(false); // 사거리 밖 대상 등록 해제
            WeaponTarget first = CreateTarget(origin + Vector3.right * 0.6f, "First_" + assetName); // 첫 번째 실제 표적
            WeaponTarget second = CreateTarget(origin + Vector3.right * 1.25f, "Second_" + assetName); // 두 번째 실제 표적
            WeaponTarget third = CreateTarget(origin + Vector3.right * 1.9f, "Third_" + assetName); // 세 번째 실제 표적
            Require(Vector3.Distance(movement.transform.position, first.transform.position) > activeWeapon.Range && Vector3.Distance(origin, first.transform.position) < activeWeapon.Range, "Test target does not distinguish head and body origin: " + assetName); // 머리와 몸통 거리 구분
            Physics2D.SyncTransforms(); // 새 충돌체 동기화
            float deadline = Time.time + interval * 3f + 2f; // 두 번 공격 제한 시간
            while (attackTimes.Count < 2 && Time.time < deadline) // 두 차례 자동 공격 대기
            { // 반복 시작
                yield return null; // 실제 게임 프레임 대기
            } // 반복 끝
            Require(attackTimes.Count >= 2, "Two automatic attacks were not observed: " + assetName); // 자동 공격 횟수 확인
            float observedInterval = attackTimes[1] - attackTimes[0]; // 실제 공격 간격
            Require(observedInterval >= interval - 0.01f && observedInterval <= interval + 0.15f, $"Attack interval mismatch for {assetName}: expected={interval:F2}, actual={observedInterval:F3}"); // 기획 주기 확인
            Require(Vector3.Distance(attackOrigins[0], origin) < 0.01f && Vector3.Distance(attackOrigins[1], origin) < 0.01f, "Automatic attack did not originate at the equipped body: " + assetName); // 실제 몸통 원점 확인
            int firstVolley = 0; // 첫 공격 명중 수
            HashSet<WeaponTarget> hitTargets = new HashSet<WeaponTarget>(); // 첫 공격 고유 대상
            foreach (HitRecord hit in hits) // 실제 명중 기록 순회
            { // 반복 시작
                if (hit.Time >= attackTimes[1]) // 두 번째 공격 이후 확인
                { // 조건 시작
                    continue; // 다음 공격 명중 제외
                } // 조건 끝
                firstVolley++; // 첫 공격 명중 증가
                hitTargets.Add(hit.Target); // 고유 명중 대상 기록
                Require(Mathf.Abs(hit.Damage - damage) < 0.01f && Vector3.Distance(hit.Origin, origin) < 0.01f, "Direct damage or hit origin mismatch: " + assetName); // 실제 명중 피해와 원점 확인
                Require(Mathf.Abs(hit.RemainingHealth - (100f - damage)) < 0.05f, "Actual target health did not reflect first direct damage: " + assetName); // 실제 첫 피해 체력 확인
            } // 반복 끝
            Require(firstVolley == expectedHits && hitTargets.Count == expectedHits && hitTargets.Contains(first), $"First automatic attack targets mismatch for {assetName}: expected={expectedHits}, hits={firstVolley}, unique={hitTargets.Count}"); // 단일·관통·범위 대상 확인
            if (expectedHits >= 2) // 복수 대상 공격 확인
            { // 조건 시작
                Require(hitTargets.Contains(second), "Second target was not hit: " + assetName); // 두 번째 실제 표적 확인
            } // 조건 끝
            if (expectedHits == 3) // 세 대상 범위 공격 확인
            { // 조건 시작
                Require(hitTargets.Contains(third), "Third target was not hit: " + assetName); // 세 번째 실제 표적 확인
            } // 조건 끝
            if (attribute == WeaponAttribute.Fire) // 기본 화상 적용 확인
            { // 조건 시작
                WeaponTargetBurnStatus burn = first.GetComponent<WeaponTargetBurnStatus>(); // 실제 화상 상태 조회
                Require(burn != null && burn.SynergyStage == 0 && Mathf.Approximately(burn.DamagePerSecond, 1f) && burn.RemainingDuration > 0f, "Base fire burn was not applied: " + assetName); // 임시 화상 상태 확인
            } // 조건 끝
            if (attribute == WeaponAttribute.Cold) // 기본 냉기 적용 확인
            { // 조건 시작
                WeaponTargetColdStatus cold = first.GetComponent<WeaponTargetColdStatus>(); // 실제 냉기 상태 조회
                Require(cold != null && cold.StackCount >= 1 && cold.RemainingDuration > 0f, "Base cold status was not applied: " + assetName); // 실제 냉기 누적 확인
            } // 조건 끝
            Debug.Log($"[EarlyWeaponVerification] {assetName}: origin={origin}, headDistance={Vector3.Distance(movement.transform.position, first.transform.position):F2}, range={activeWeapon.Range:F2}, damage={damage:F2}, interval={observedInterval:F3}, firstHits={firstVolley}."); // 실제 공격 결과 기록
        } // 코루틴 끝

        private WeaponTarget CreateTarget(Vector3 position, string name) // 실제 자동 공격 표적 생성
        { // 메서드 시작
            GameObject targetObject = new GameObject(name); // 통제 표적 객체
            objects.Add(targetObject); // 시험 객체 등록
            targetObject.transform.position = position; // 실제 표적 위치
            CircleCollider2D collider = targetObject.AddComponent<CircleCollider2D>(); // 실제 2D 충돌체 생성
            collider.radius = 0.25f; // 표적 접촉 반경
            collider.isTrigger = true; // 투사체 트리거 설정
            Rigidbody2D rigidbody = targetObject.AddComponent<Rigidbody2D>(); // 실제 물리체 생성
            rigidbody.bodyType = RigidbodyType2D.Kinematic; // 고정 표적 물리 방식
            rigidbody.useFullKinematicContacts = true; // 운동학 접촉 허용
            WeaponTarget target = targetObject.AddComponent<WeaponTarget>(); // 실제 무기 피해 대상
            target.Configure(100f); // 표적 체력 설정
            return target; // 실제 표적 반환
        } // 메서드 끝

        private void OnAttack(WeaponAttributeEffectContext context) // 실제 무기 공격 알림
        { // 메서드 시작
            if (context.Weapon != activeWeapon) // 시험 무기 확인
            { // 조건 시작
                return; // 다른 무기 무시
            } // 조건 끝
            attackTimes.Add(Time.time); // 실제 게임 시각 기록
            attackOrigins.Add(context.Origin); // 실제 공격 원점 기록
        } // 메서드 끝

        private void OnHit(WeaponAttributeHitContext context) // 실제 무기 명중 알림
        { // 메서드 시작
            if (context.Attack.Weapon != activeWeapon) // 시험 무기 확인
            { // 조건 시작
                return; // 다른 무기 무시
            } // 조건 끝
            hits.Add(new HitRecord { Target = context.Target, Time = Time.time, Damage = context.Attack.DirectDamage, RemainingHealth = context.Target.CurrentHealth, Origin = context.Attack.Origin }); // 실제 명중 정보 기록
        } // 메서드 끝

        private IEnumerator VerifyEnemyExperience() // 세 실제 적의 보석 생성과 흡수
        { // 코루틴 시작
            EarlyEnemySettings[] settings = (EarlyEnemySettings[])typeof(EarlyEnemyWaveSpawner).GetField("enemies", PrivateInstance).GetValue(spawner); // 실제 씬 적 설정 조회
            Require(settings != null && settings.Length >= 3, "Actual early enemy settings are missing."); // 적 설정 확인
            typeof(EarlyEnemyWaveSpawner).GetField("healDropChance", PrivateInstance).SetValue(spawner, 0f); // 시험 중 회복 보상 차단
            typeof(EarlyEnemyWaveSpawner).GetField("bodyRepairDropChance", PrivateInstance).SetValue(spawner, 0f); // 시험 중 복구 보상 차단
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("Spawn", PrivateInstance); // 실제 적 생성 메서드 조회
            Require(spawn != null, "Actual enemy spawn method is missing."); // 출현기 메서드 확인
            EarlyEnemyKind[] kinds = { EarlyEnemyKind.Chaser, EarlyEnemyKind.Predator, EarlyEnemyKind.Shooter }; // 시험 적 순서
            int[] rewards = { 1, 2, 2 }; // 기획 소형 보석 경험치
            float[] healthValues = { 25f, 35f, 20f }; // 기획 적 체력 임시값
            int[] damageValues = { 10, 12, 8 }; // 기획 적 피해 임시값
            float[] speedValues = { 0.8f, 0.9f, 0.7f }; // 기획 적 속도 배율
            float[] periods = { 1.2f, 1.5f, 2f }; // 기획 적 공격 주기
            for (int index = 0; index < kinds.Length; index++) // 세 적 종류 순회
            { // 반복 시작
                EarlyEnemySettings setting = settings[index]; // 실제 씬의 적 설정
                Require(setting != null && setting.kind == kinds[index] && setting.experience == rewards[index] && Mathf.Approximately(setting.health, healthValues[index]) && setting.damage == damageValues[index] && Mathf.Approximately(setting.speedRatio, speedValues[index]) && Mathf.Approximately(setting.attackInterval, periods[index]), "Scene enemy temporary values mismatch: " + kinds[index]); // 기획 임시값과 씬 대조
                int beforeExperience = experience.CurrentExperience; // 기존 경험치 기록
                spawn.Invoke(spawner, new object[] { setting }); // 실제 출현기로 통제 생성
                GameObject enemy = GameObject.Find("EarlyEnemy_" + kinds[index]); // 생성된 실제 적 조회
                Require(enemy != null, "Controlled enemy did not spawn: " + kinds[index]); // 생성 확인
                enemy.transform.position = movement.transform.position + Vector3.right * 0.5f; // 보석 흡수 가능한 위치
                EarlyEnemyBehaviour behaviour = enemy.GetComponent<EarlyEnemyBehaviour>(); // 실제 적 행동 조회
                if (behaviour != null) // 적 행동 존재 확인
                { // 조건 시작
                    behaviour.enabled = false; // 시험 중 공격과 이동 중지
                } // 조건 끝
                WeaponTarget target = enemy.GetComponent<WeaponTarget>(); // 실제 적 전투 대상 조회
                Require(target != null && Mathf.Approximately(target.MaximumHealth, healthValues[index]), "Spawned enemy HP differs from setting: " + kinds[index]); // 생성된 체력 확인
                target.TakeDamage(target.MaximumHealth + 1f); // 통제된 실제 적 처치
                ExperiencePickup[] gems = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None); // 생성된 실제 보석 조회
                Require(gems.Length == rewards[index], "Wrong number of small gems from " + kinds[index]); // 소형 보석 개수 확인
                FieldInfo value = typeof(ExperiencePickup).GetField("experienceValue", PrivateInstance); // 실제 보석 경험치 필드
                foreach (ExperiencePickup gem in gems) // 모든 보석 순회
                { // 반복 시작
                    Require((int)value.GetValue(gem) == 1, "Enemy gem was not small XP 1: " + kinds[index]); // 각 보석 값 확인
                } // 반복 끝
                float deadline = Time.time + 3f; // 실제 흡수 제한 시간
                while (experience.CurrentExperience < beforeExperience + rewards[index] && Time.time < deadline) // 실제 보석 흡수 대기
                { // 반복 시작
                    yield return null; // 실제 경험치 갱신 대기
                } // 반복 끝
                Require(experience.CurrentExperience == beforeExperience + rewards[index] && experience.CurrentLevel == 1, "Actual collected experience mismatch: " + kinds[index]); // 실제 보석 경험치 확인
                Debug.Log($"[EarlyWeaponVerification] {kinds[index]}: hp={healthValues[index]}, gemCount={gems.Length}, eachXP=1, collectedXP={experience.CurrentExperience - beforeExperience}."); // 적과 경험치 결과 기록
                yield return null; // 지연 보석과 적 제거 대기
            } // 반복 끝
            Require(experience.CurrentExperience == 5, "Three enemy types did not yield total XP 5."); // 세 종류 총 경험치 확인
        } // 코루틴 끝

        private void ClearObjects() // 통제 표적과 남은 투사체 제거
        { // 메서드 시작
            foreach (GameObject item in objects) // 시험 객체 순회
            { // 반복 시작
                if (item != null) // 객체 존재 확인
                { // 조건 시작
                    item.SetActive(false); // 실제 대상 등록 즉시 해제
                    UnityEngine.Object.Destroy(item); // 시험 객체 제거
                } // 조건 끝
            } // 반복 끝
            objects.Clear(); // 시험 객체 목록 초기화
            foreach (StraightProjectile projectile in UnityEngine.Object.FindObjectsByType<StraightProjectile>(FindObjectsSortMode.None)) // 남은 실제 투사체 순회
            { // 반복 시작
                projectile.gameObject.SetActive(false); // 이전 투사체 접촉 중지
                UnityEngine.Object.Destroy(projectile.gameObject); // 이전 투사체 제거
            } // 반복 끝
        } // 메서드 끝

        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedEarlyWeaponVerification.Finish(1, message); // 실패 기록과 배치 종료
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
