using System; // 예외 형식
using System.Collections; // 코루틴 형식
using ProjectEpsilon.Combat; // 실제 속성 효과
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어
using ProjectEpsilon.Progression; // 게임의 포탑 보상 목록
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedDay41AttributeVerification // 실제 신성과 암흑 시험
    {
        private const string Key = "EpsilonDay41AttributeVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedDay41AttributeVerification() // 도메인 재시작 처리
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
            player.gameObject.AddComponent<Day41AttributeRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[Day41AttributeVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class Day41AttributeRunner : MonoBehaviour // 실제 신성 및 암흑 시험 실행기
    {
        private IEnumerator Start() // 실제 속성 명중 시험
        {
            GetComponent<SnakeMovement>().enabled = false; // 플레이어 이동 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 중지
            WeaponAttributeCombatEffects effects = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 속성 효과 조회
            SnakeShieldController shield = effects == null || effects.PlayerEffects == null ? null : effects.PlayerEffects.ShieldController; // 실제 속성 효과에 연결된 보호막 조회
            WeaponData holy = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/PrototypeHolyBlade.asset"); // 실제 신성 시험 무기
            WeaponData dark = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/PrototypeDarkBlade.asset"); // 실제 암흑 시험 무기
            Require(effects != null && shield != null && holy != null && dark != null, "Holy or dark test dependencies missing."); // 실제 시험 구성 확인
            shield.Clear(); // 기존 보호막 제거
            WeaponTarget holyTarget = CreateTarget("HolyTarget", new Vector3(100f, 100f, 0f)); // 실제 신성 표적
            WeaponAttributeAttackSnapshot holyAttack = new WeaponAttributeAttackSnapshot(holy, WeaponAttribute.Holy, 6, 6, 1, holyTarget.transform.position, 1f); // 신성 육 단계 공격 정보
            for (int index = 0; index < 4; index++) // 보호막 이전 명중 순회
            {
                effects.ApplyHit(holyAttack, holyTarget, holyTarget.transform.position); // 실제 신성 명중
            }
            Require(shield.CurrentShield == 0, "Holy shield activated before five hits."); // 보호막 조기 발동 방지 확인
            effects.ApplyHit(holyAttack, holyTarget, holyTarget.transform.position); // 다섯 번째 실제 신성 명중
            Require(shield.CurrentShield == WeaponAttributeHolyRules.ShieldAmount, "Holy shield did not activate after five hits."); // 실제 보호막 발동 확인
            WeaponTarget darkTarget = CreateTarget("DarkTarget", new Vector3(200f, 100f, 0f)); // 실제 암흑 표적
            WeaponAttributeAttackSnapshot darkAttack = new WeaponAttributeAttackSnapshot(dark, WeaponAttribute.Dark, 2, 2, 1, darkTarget.transform.position, 1f); // 암흑 이 단계 공격 정보
            for (int index = 0; index < 8; index++) // 최대 저주 초과 명중 순회
            {
                effects.ApplyHit(darkAttack, darkTarget, darkTarget.transform.position); // 실제 암흑 명중
            }
            WeaponTargetDarkCurseStatus curse = darkTarget.GetComponent<WeaponTargetDarkCurseStatus>(); // 실제 저주 상태 조회
            Require(curse != null && curse.Stacks == 5 && curse.IsActive, "Dark curse did not cap at five stacks."); // 실제 저주 상한 확인
            float before = darkTarget.CurrentHealth; // 강화 명중 전 체력
            WeaponAttributeAttackSnapshot boosted = new WeaponAttributeAttackSnapshot(dark, WeaponAttribute.Dark, 4, 4, 1, darkTarget.transform.position, 10f); // 암흑 사 단계 공격 정보
            effects.ApplyHit(boosted, darkTarget, darkTarget.transform.position); // 실제 저주 강화 명중
            Require(Mathf.Abs(before - darkTarget.CurrentHealth - 13f) < 0.001f, "Dark curse damage amplification mismatch."); // 실제 다섯 중첩 피해 증가 확인
            bool compareAll = Array.IndexOf(Environment.GetCommandLineArgs(), "-epsilonCompareAttributes") >= 0; // 동일 조건 속성 비교 요청 여부
            if (compareAll) // 여덟 속성 비교 분기
            { // 비교 조건 시작
                yield return CompareAttributes(effects); // 여덟 속성의 공격과 방어 지표 수집
            } // 비교 조건 종료
            yield return null; // 실제 프레임 대기
            AutomatedDay41AttributeVerification.Finish(0, compareAll ? "All eight controlled attribute comparisons passed." : "Actual holy shield threshold and dark curse cap and damage."); // 전체 성공 기록
        }
        private static IEnumerator CompareAttributes(WeaponAttributeCombatEffects effects) // 여덟 속성의 통제 비교
        { // 비교 함수 시작
            var pool = AssetDatabase.LoadAssetAtPath<WeaponRewardPool>("Assets/Data/Progression/DebugWeaponRewardPool.asset"); // 게임에서 쓰는 포탑 보상 에셋
            SnakeHealth health = effects.PlayerEffects.SnakeHealth; // 방어와 회복 비교용 공용 체력
            SnakeShieldController shield = effects.PlayerEffects.ShieldController; // 보호막 비교용 관리자
            Require(pool != null && health != null && shield != null, "Attribute comparison fixtures are missing."); // 실제 효과 구성 확인
            foreach (EarlyEnemyBehaviour enemy in FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 시험 전에 이미 생성된 자연 적 확인
            { // 자연 적 반복 시작
                enemy.gameObject.SetActive(false); // 속성별 같은 피해 조건을 위해 자연 공격 중지
            } // 자연 적 반복 종료
            foreach (WeaponAttribute attribute in Enum.GetValues(typeof(WeaponAttribute))) // 물리부터 암흑까지 같은 순서로 시험
            { // 속성 반복 시작
                WeaponData weapon = null; // 현재 속성의 근접 포탑
                foreach (WeaponRewardEntry entry in pool.Entries) // 기존 포탑 데이터 순회
                { // 포탑 반복 시작
                    if (entry?.Weapon != null && entry.Weapon.Attribute == attribute && entry.Weapon.AttackType == WeaponAttackType.Melee) // 같은 공격 유형의 포탑 검색
                    { // 포탑 조건 시작
                        weapon = entry.Weapon; // 속성에 해당하는 실제 포탑 선택
                        break; // 첫 근접 포탑 사용
                    } // 포탑 조건 종료
                } // 포탑 반복 종료
                Require(weapon != null, "Missing melee comparison turret: " + attribute); // 같은 유형 확보 확인
                UnityEngine.Random.InitState(15501); // 각 속성에 같은 난수 조건 적용
                health.ResetHealth(); // 같은 최대 체력으로 초기화
                shield.Clear(); // 이전 보호막 제거
                health.TakeDamage(50); // 회복을 관찰할 동일한 체력 손실 적용
                int startingHealth = health.CurrentHealth; // 회복 전 실제 체력 기록
                Vector3 center = new Vector3(500f + (int)attribute * 50f, 500f); // 두 대상의 독립 비교 위치
                WeaponTarget primary = CreateTarget(attribute + "Primary", center); // 같은 체력의 주 대상
                WeaponTarget secondary = CreateTarget(attribute + "Secondary", center + Vector3.right * 0.35f); // 연쇄나 폭발 비교 대상
                float damage = WeaponAttributeDamageRules.CalculateDirectDamage(10f, attribute, 6); // 동일 기본 피해와 6단계 속성 피해
                var snapshot = new WeaponAttributeAttackSnapshot(weapon, attribute, 6, 6, 1, center, damage); // 동일한 ★1과 시너지 단계
                for (int index = 0; index < 6; index++) // 같은 횟수의 실제 속성 공격
                { // 명중 반복 시작
                    effects.ApplyHit(snapshot, primary, primary.transform.position); // 실제 명중 효과 적용
                    yield return new WaitForSeconds(0.25f); // 폭발 중복 판정과 상태 효과의 자연 진행
                } // 명중 반복 종료
                yield return new WaitForSeconds(1f); // 같은 추가 상태 피해 관측 시간
                float primaryDamage = primary.MaximumHealth - primary.CurrentHealth; // 주 대상 실제 체력 감소
                float secondaryDamage = secondary.MaximumHealth - secondary.CurrentHealth; // 연쇄와 폭발 피해량
                float outgoing = primary.GetComponent<WeaponTargetPoisonStatus>()?.OutgoingDamageMultiplier ?? 1f; // 적 공격 감소 배율
                float movement = primary.GetComponent<WeaponTargetStatusController>()?.CurrentMovementMultiplier ?? 1f; // 적 이동 감소 배율
                Require(primaryDamage > 0f && startingHealth == health.MaximumHealth - 50, "Invalid controlled measurement for " + attribute); // 공통 피해와 기준 체력 확인
                Debug.Log($"[AttributeBalance] {attribute} primary={primaryDamage:F2} secondary={secondaryDamage:F2} healing={health.CurrentHealth - startingHealth} shield={shield.CurrentShield} outgoing={outgoing:F2} movement={movement:F2}"); // 동일 시간의 공격과 방어 지표
                Destroy(primary.gameObject); // 다음 속성 전에 주 대상 제거
                Destroy(secondary.gameObject); // 다음 속성 전에 보조 대상 제거
                yield return null; // 유니티 지연 제거 완료 대기
            } // 속성 반복 종료
        } // 비교 함수 종료
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
                AutomatedDay41AttributeVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
