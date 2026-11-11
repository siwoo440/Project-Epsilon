using System; // 기본 형식
using System.Collections; // 코루틴 형식
using System.Collections.Generic; // 시험 기록 목록
using ProjectEpsilon.Combat; // 무기와 공격 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Enemies; // 자연 적 출현 제어
using ProjectEpsilon.Player; // 몸통과 이동
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 편집기 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedSteelSwordGradeVerification // 강철 검 등급별 실제 공격 검증
    {
        private const string ActiveKey = "EpsilonSteelSwordGradeVerification"; // 배치 진행 표식
        private static double started; // 플레이 시작 시각
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지

        static AutomatedSteelSwordGradeVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 배치 진행 확인
            {
                EditorApplication.update -= Tick; // 중복 연결 제거
                EditorApplication.update += Tick; // 플레이 시작 감시
            }
        }

        public static void Run() // Unity 배치 진입점
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 배치 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 플레이 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        }

        private static void Tick() // 시작 선택과 플레이 진입 감시
        {
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            {
                return; // 진입 대기
            }
            if (started <= 0d) // 최초 갱신 확인
            {
                started = EditorApplication.timeSinceStartup; // 시작 시각 기록
            }
            if (EditorApplication.timeSinceStartup - started > 120d) // 전체 시험 제한
            {
                Finish(1, "Timed out waiting for controlled steel sword test."); // 제한 시간 실패
                return; // 감시 중단
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 실제 시작 무기 첫 후보 선택
            {
                return; // 실제 게임 시작 대기
            }
            if (injected) // 시험 실행기 존재 확인
            {
                return; // 시험 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 씬 초기화 확인
            {
                return; // 초기화 대기
            }
            injected = true; // 실행기 중복 생성 방지
            player.gameObject.AddComponent<SteelSwordGradeVerificationRunner>(); // 실제 씬 시험 실행
        }

        public static void Finish(int code, string message) // 결과 기록과 프로세스 종료
        {
            if (finished) // 중복 종료 확인
            {
                return; // 중복 처리 방지
            }
            finished = true; // 종료 상태 기록
            SessionState.SetBool(ActiveKey, false); // 진행 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log($"[SteelSwordVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 배치 결과 기록
            EditorApplication.Exit(code); // 종료 코드 반환
        }
    }

    public sealed class SteelSwordGradeVerificationRunner : MonoBehaviour // 실제 자동 근접 공격 시험
    {
        private sealed class HitRecord // 실제 명중 정보
        {
            public int TargetId; // 명중 대상 식별자
            public float Time; // 명중 게임 시각
            public float Damage; // 직접 피해량
            public int Stage; // 속성 시너지 단계
            public Vector3 Origin; // 실제 공격 원점
        }

        private readonly List<GameObject> objects = new List<GameObject>(); // 시험 대상 객체
        private readonly List<float> attacks = new List<float>(); // 실제 공격 시각
        private readonly List<Vector3> origins = new List<Vector3>(); // 실제 공격 원점
        private readonly List<HitRecord> hits = new List<HitRecord>(); // 실제 명중 기록
        private SnakeMovement movement; // 실제 플레이어 이동
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeBodyFollower follower; // 몸통 경로 추적
        private SnakeWeaponManager weapons; // 실제 무기 관리자
        private WeaponAttributeSynergyManager synergy; // 실제 속성 집계
        private WeaponAttributeEffectHooks hooks; // 실제 공격 및 명중 알림
        private WeaponData sword; // 실제 강철 검 에셋
        private WeaponData otherMelee; // 다른 실제 근접 무기
        private WeaponData physicalSupport; // 물리 시너지용 무기
        private WeaponData currentWeapon; // 현재 시험 무기
        private Vector3 origin; // 실제 세 번째 몸통 위치

        private IEnumerator Start() // 실제 씬 시험 순서
        {
            Prepare(); // 실제 씬과 통제 조건 준비
            int[] expectedCounts = { 1, 1, 2, 2, 2 }; // 등급별 기대 명중 수
            float[] expectedDamage = { 10f, 12.5f, 15f, 18f, 22f }; // 등급별 기대 직접 피해
            for (int grade = 1; grade <= 5; grade++) // 강철 검 모든 등급 순회
            {
                yield return VerifyGrade(sword, grade, expectedCounts[grade - 1], expectedDamage[grade - 1]); // 실제 자동 공격 검증
            }
            yield return VerifyGrade(otherMelee, 3, 1, 12f); // 다른 근접 무기 3등급 단일 공격
            yield return VerifyGrade(otherMelee, 5, 1, 17.6f); // 다른 근접 무기 5등급 단일 공격
            yield return VerifyMutation(false); // 첫 적 사망 중 대상 재등록
            yield return VerifyMutation(true); // 첫 적 사망 중 둘째 대상 비활성
            yield return VerifyPhysicalSynergy(); // 실제 물리 시너지 경로 확인
            ClearObjects(); // 시험 대상 정리
            hooks.AttackTriggered -= OnAttack; // 공격 기록 구독 해제
            hooks.HitTriggered -= OnHit; // 명중 기록 구독 해제
            AutomatedSteelSwordGradeVerification.Finish(0, "Actual automatic melee attacks across grades, nearest targets, one cooldown, death/list mutations, other weapon and physical synergy."); // 전체 성공 기록
        }

        private void Prepare() // 실제 씬과 무기 데이터 준비
        {
            movement = GetComponent<SnakeMovement>(); // 실제 플레이어 이동 조회
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자 조회
            follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기 조회
            weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자 조회
            synergy = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeSynergyManager>(); // 실제 시너지 관리자 조회
            hooks = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeEffectHooks>(); // 실제 공격 알림 조회
            sword = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 강철 검 에셋
            otherMelee = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugCutter.asset"); // 실제 화염 검 에셋
            physicalSupport = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugNeedle.asset"); // 실제 물리 시너지 에셋
            Require(movement != null && body != null && follower != null && weapons != null && synergy != null && hooks != null && hooks.IsConfigured && sword != null && otherMelee != null && physicalSupport != null && weapons.SlotCount == 3, "Actual scene, weapon assets, or combat hooks are missing."); // 실제 씬 구성 확인
            Require(sword.AttackType == WeaponAttackType.Melee && sword.Attribute == WeaponAttribute.Physical && Mathf.Approximately(sword.BaseDamage, 10f) && Mathf.Approximately(sword.AttackInterval, 0.8f), "Steel sword temporary asset values differ from the specification."); // 기획 임시값 확인
            movement.enabled = false; // 실제 플레이어 이동 중지
            follower.enabled = false; // 몸통 시험 위치 고정
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 자연 출현기 조회
            if (spawner != null) // 출현기 존재 확인
            {
                spawner.enabled = false; // 통제 시험 중 자연 출현 중지
            }
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 자기 충돌 조회
            if (self != null) // 자기 충돌 구성 확인
            {
                self.enabled = false; // 몸통 위치 통제 중 자기 충돌 방지
            }
            foreach (WeaponTarget target in UnityEngine.Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 전투 대상 순회
            {
                target.gameObject.SetActive(false); // 기존 대상의 실제 등록 해제
            }
            origin = movement.transform.position + Vector3.right * 20f; // 머리 사거리 밖 몸통 위치
            weapons.Slots[2].Origin.position = origin; // 실제 세 번째 몸통 이동
            Physics2D.SyncTransforms(); // 실제 몸통 물리 위치 동기화
            hooks.AttackTriggered += OnAttack; // 실제 공격 알림 구독
            hooks.HitTriggered += OnHit; // 실제 명중 알림 구독
            Debug.Log("[SteelSwordVerification] Actual Game scene; first starting weapon selected by real button helper; movement, body following and natural spawning disabled; controlled stationary targets and event callbacks."); // 통제 조건 기록
        }

        private IEnumerator VerifyGrade(WeaponData weapon, int grade, int expectedTargets, float expectedDamage) // 등급별 실제 자동 공격
        {
            ConfigureWeapon(weapon, grade, false); // 시험 무기 하나 장착
            WeaponTarget outside = CreateTarget(origin + Vector3.right * (weapon.Range + 1f), 1000f, "Outside"); // 범위 밖 대상 생성
            yield return null; // 실제 자동 공격 갱신
            yield return null; // 추가 프레임 대기
            Require(attacks.Count == 0 && Mathf.Approximately(outside.CurrentHealth, 1000f), "Melee attack selected an out-of-range target."); // 범위 밖 비공격 확인
            outside.gameObject.SetActive(false); // 범위 밖 대상 등록 해제
            WeaponTarget first = CreateTarget(origin + Vector3.right * 0.3f, 1000f, "First"); // 가장 가까운 실제 대상
            WeaponTarget second = CreateTarget(origin + Vector3.right * 0.7f, 1000f, "Second"); // 두 번째 실제 대상
            WeaponTarget third = CreateTarget(origin + Vector3.right * 1.1f, 1000f, "Third"); // 세 번째 실제 대상
            int firstId = first.GetInstanceID(); // 첫 대상 식별자
            int secondId = second.GetInstanceID(); // 둘째 대상 식별자
            int thirdId = third.GetInstanceID(); // 셋째 대상 식별자
            Require(Vector3.Distance(movement.transform.position, first.transform.position) > weapon.Range && Vector3.Distance(origin, first.transform.position) < weapon.Range, "Controlled target does not distinguish head and body range."); // 실제 머리와 몸통 거리 구분
            float deadline = Time.time + 3f; // 두 차례 공격 제한 시각
            while (attacks.Count < 2 && Time.time < deadline) // 실제 두 차례 자동 공격 대기
            {
                yield return null; // 실제 게임 프레임 대기
            }
            Require(attacks.Count >= 2, $"Two automatic attacks did not occur: {weapon.DisplayName} grade {grade}."); // 자동 공격 횟수 확인
            float interval = attacks[1] - attacks[0]; // 실제 두 공격 간격
            Require(interval >= weapon.AttackInterval - 0.01f && interval <= weapon.AttackInterval + 0.15f, $"Attack cooldown mismatch: {weapon.DisplayName} grade {grade}, interval={interval:F3}."); // 공격 주기 확인
            Require(Vector3.Distance(origins[0], origin) < 0.01f && Vector3.Distance(origins[1], origin) < 0.01f, "Automatic attack origin differs from the equipped body."); // 실제 몸통 원점 확인
            HashSet<int> firstTargets = new HashSet<int>(); // 첫 공격의 고유 대상
            int firstHitCount = 0; // 첫 공격의 총 명중 수
            foreach (HitRecord hit in hits) // 실제 명중 기록 순회
            {
                if (hit.Time >= attacks[1]) // 둘째 공격의 명중 확인
                {
                    continue; // 후속 공격 제외
                }
                firstHitCount++; // 첫 공격 명중 수 증가
                firstTargets.Add(hit.TargetId); // 고유 대상 기록
                Require(Mathf.Abs(hit.Damage - expectedDamage) < 0.01f && hit.Stage == 0 && Vector3.Distance(hit.Origin, origin) < 0.01f, "Actual hit damage, synergy or origin mismatch."); // 등급 피해와 원점 확인
            }
            Require(firstHitCount == expectedTargets && firstTargets.Count == expectedTargets && firstTargets.Contains(firstId), $"Wrong first-volley target count: {weapon.DisplayName} grade {grade}, hits={firstHitCount}, unique={firstTargets.Count}."); // 중복 방지와 명중 수 확인
            Require((expectedTargets < 2 || firstTargets.Contains(secondId)) && !firstTargets.Contains(thirdId), "Attack did not choose only the closest eligible targets."); // 가까운 대상 우선 확인
            Debug.Log($"[SteelSwordVerification] {weapon.DisplayName} grade={grade}, targets={firstHitCount}, damage={expectedDamage:F2}, interval={interval:F3}, origin={origin}."); // 실제 등급별 결과
        }

        private IEnumerator VerifyMutation(bool disableSecond) // 첫 사망 중 실제 대상 등록 변경
        {
            ConfigureWeapon(sword, 3, false); // 강철 검 3등급 장착
            WeaponTarget first = CreateTarget(origin + Vector3.right * 0.3f, 1f, "DyingFirst"); // 한 번에 사망할 첫 대상
            WeaponTarget second = CreateTarget(origin + Vector3.right * 0.7f, 1000f, "OriginalSecond"); // 공격 전 둘째 대상
            WeaponTarget third = CreateTarget(origin + Vector3.right * 1.1f, 1000f, "OriginalThird"); // 공격 전 셋째 대상
            WeaponTarget newlyAdded = null; // 사망 이벤트에서 새로 생긴 대상
            int deathEvents = 0; // 실제 사망 이벤트 수
            first.Died += dead => // 첫 대상 실제 사망 이벤트
            {
                deathEvents++; // 사망 횟수 기록
                second.gameObject.SetActive(false); // 실제 대상 등록 해제
                if (!disableSecond) // 둘째 대상 재등록 시험
                {
                    second.gameObject.SetActive(true); // 동일 대상 실제 재등록
                }
                newlyAdded = CreateTarget(origin + Vector3.left * 0.1f, 1000f, "CallbackAdded"); // 콜백 중 새 대상 생성
            };
            float deadline = Time.time + 2f; // 첫 공격 제한 시각
            while (attacks.Count < 1 && Time.time < deadline) // 실제 첫 자동 공격 대기
            {
                yield return null; // 실제 게임 프레임 대기
            }
            Require(attacks.Count == 1 && deathEvents == 1 && newlyAdded != null, "Actual first hit did not trigger a single death callback."); // 사망과 공격 알림 확인
            yield return new WaitForSeconds(0.2f); // 같은 주기 중 중복 공격 확인
            float expectedSecondHealth = disableSecond ? 1000f : 985f; // 실제 둘째 대상 기대 체력
            Require(attacks.Count == 1 && Mathf.Abs(second.CurrentHealth - expectedSecondHealth) < 0.01f && Mathf.Approximately(third.CurrentHealth, 1000f) && Mathf.Approximately(newlyAdded.CurrentHealth, 1000f), $"Target list mutation changed one attack: disabled={disableSecond}, attacks={attacks.Count}, second={second.CurrentHealth:F1}, third={third.CurrentHealth:F1}, added={newlyAdded.CurrentHealth:F1}."); // 스냅샷과 중복 방지 확인
            int secondHits = 0; // 둘째 대상 명중 횟수
            foreach (HitRecord hit in hits) // 실제 명중 기록 순회
            {
                if (hit.TargetId == second.GetInstanceID()) // 둘째 대상 확인
                {
                    secondHits++; // 실제 명중 수 증가
                }
            }
            Require(secondHits == (disableSecond ? 0 : 1), "Selected second target received the wrong number of hits after a callback."); // 재등록 대상 중복 방지
            Debug.Log($"[SteelSwordVerification] death callback: disabledSecond={disableSecond}, deathEvents={deathEvents}, secondHits={secondHits}, thirdHP={third.CurrentHealth:F1}, addedHP={newlyAdded.CurrentHealth:F1}."); // 실제 이벤트 변경 결과
        }

        private IEnumerator VerifyPhysicalSynergy() // 실제 물리 속성 시너지 피해 검증
        {
            ConfigureWeapon(sword, 3, true); // 강철 검과 물리 보조 무기 장착
            Require(synergy.GetCount(WeaponAttribute.Physical) == 2 && synergy.GetStage(WeaponAttribute.Physical) == 2, "Physical synergy fixture is not at stage two."); // 실제 물리 시너지 확인
            WeaponTarget first = CreateTarget(origin + Vector3.right * 0.3f, 1000f, "SynergyFirst"); // 첫 번째 실제 대상
            WeaponTarget second = CreateTarget(origin + Vector3.right * 0.7f, 1000f, "SynergySecond"); // 두 번째 실제 대상
            float deadline = Time.time + 2f; // 첫 공격 제한 시각
            while (attacks.Count < 1 && Time.time < deadline) // 실제 자동 공격 대기
            {
                yield return null; // 실제 프레임 대기
            }
            Require(attacks.Count == 1 && hits.Count == 2 && Mathf.Abs(first.CurrentHealth - 983.5f) < 0.01f && Mathf.Abs(second.CurrentHealth - 983.5f) < 0.01f, "Physical synergy damage did not reach both selected targets."); // 두 대상 실제 시너지 피해 확인
            foreach (HitRecord hit in hits) // 실제 명중 정보 순회
            {
                Require(hit.Stage == 2 && Mathf.Abs(hit.Damage - 16.5f) < 0.01f && Vector3.Distance(hit.Origin, origin) < 0.01f, "Physical synergy snapshot mismatch."); // 실제 시너지와 원점 확인
            }
            Debug.Log("[SteelSwordVerification] physical synergy x2: two targets received 16.50 direct damage from actual body origin."); // 시너지 결과 기록
        }

        private void ConfigureWeapon(WeaponData weapon, int grade, bool physicalSynergy) // 시험용 무기와 실제 슬롯 구성
        {
            ClearObjects(); // 이전 시험 대상 제거
            currentWeapon = weapon; // 실제 시험 무기 설정
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 모든 몸통 슬롯 순회
            {
                slot.Clear(); // 기존 무기 제거
            }
            if (physicalSynergy) // 물리 시너지 시험 확인
            {
                Require(weapons.TryEquipAt(0, physicalSupport, 1), "Could not equip physical synergy support."); // 실제 물리 보조 무기 장착
                weapons.Slots[0].StartCooldown(Time.time + 100f); // 보조 무기의 시험 중 자동 공격 보류
            }
            Require(weapons.TryEquipAt(2, weapon, grade), "Could not equip controlled melee weapon."); // 실제 세 번째 몸통 장착
            synergy.Recalculate(); // 직접 슬롯 초기화 후 실제 속성 집계
            attacks.Clear(); // 이전 공격 시각 제거
            origins.Clear(); // 이전 공격 원점 제거
            hits.Clear(); // 이전 명중 기록 제거
        }

        private WeaponTarget CreateTarget(Vector3 position, float health, string name) // 실제 공격 대상 생성
        {
            GameObject targetObject = new GameObject("SteelSwordVerification_" + name); // 통제 대상 객체
            objects.Add(targetObject); // 시험 객체 등록
            targetObject.transform.position = position; // 실제 대상 위치
            WeaponTarget target = targetObject.AddComponent<WeaponTarget>(); // 실제 공격 대상 컴포넌트
            target.Configure(health); // 실제 체력 설정
            return target; // 실제 대상 반환
        }

        private void OnAttack(WeaponAttributeEffectContext context) // 실제 공격 알림
        {
            if (context.Weapon != currentWeapon) // 현재 시험 무기 확인
            {
                return; // 다른 무기 무시
            }
            attacks.Add(Time.time); // 실제 공격 시각 기록
            origins.Add(context.Origin); // 실제 몸통 원점 기록
        }

        private void OnHit(WeaponAttributeHitContext context) // 실제 명중 알림
        {
            if (context.Attack.Weapon != currentWeapon || context.Target == null) // 시험 무기와 대상 확인
            {
                return; // 다른 명중 무시
            }
            hits.Add(new HitRecord { TargetId = context.Target.GetInstanceID(), Time = Time.time, Damage = context.Attack.DirectDamage, Stage = context.Attack.SynergyStage, Origin = context.Attack.Origin }); // 실제 명중 정보 기록
        }

        private void ClearObjects() // 이전 시험 대상 즉시 비활성 및 제거
        {
            foreach (GameObject item in objects) // 생성한 대상 순회
            {
                if (item != null) // 대상 존재 확인
                {
                    item.SetActive(false); // 실제 공격 대상 등록 해제
                    UnityEngine.Object.Destroy(item); // 시험 대상 제거
                }
            }
            objects.Clear(); // 시험 대상 목록 초기화
        }

        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedSteelSwordGradeVerification.Finish(1, message); // 실패 기록 및 배치 종료
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
