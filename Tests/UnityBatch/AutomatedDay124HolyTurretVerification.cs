using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 실제 발사 정보 조회
using ProjectEpsilon.Combat; // 실제 포탑과 독
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 포탑 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어 이동
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay124HolyTurretVerification // 실제 신성 포탑 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay124HolyTurrets"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay124HolyTurretVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 실제 신성 포탑 시험
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실제 전투 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시작 방지
            new GameObject("Day124HolyRunner").AddComponent<Day124HolyRunner>(); // 실제 포탑 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day124HolyTurretVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day124HolyRunner : MonoBehaviour // 실제 신성 포탑 시험 실행기
    { // 클래스 시작
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 발사 정보 조회
        private IEnumerator Start() // 실제 포탑별 독와 공격 확인
        { // 함수 시작
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            WeaponAttributeCombatEffects effects = weapons == null ? null : weapons.AttributeCombatEffects; // 실제 속성 전투 처리기
            Require(player != null && weapons != null && spawner != null && effects != null, "실제 신성 포탑 시험 구성 누락"); // 실제 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 자동 공격 간섭 방지
            foreach (WeaponTarget old in FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 적 순회
            { // 반복 시작
                old.gameObject.SetActive(false); // 기존 적의 시험 간섭 방지
            } // 반복 종료
            WeaponData blade = Load("Blade"); // 실제 독 근접 포탑
            WeaponData aura = Load("Aura"); // 실제 독 범위 포탑
            WeaponData dagger = Load("Dagger"); // 실제 폭발 속사 근접 포탑
            WeaponData bolt = Load("Bolt"); // 실제 폭발 직사 포탑
            WeaponData spear = Load("Spear"); // 실제 폭발 관통 포탑
            WeaponData wave = Load("Wave"); // 실제 폭발 파동 포탑
            Require(blade != null && aura != null && dagger != null && bolt != null && spear != null && wave != null, "실제 신성 포탑 데이터 누락"); // 실제 데이터 확인
            WeaponAttributePlayerEffects playerEffects = effects.PlayerEffects; // 실제 신성 효과 처리기
            SnakeHealth health = playerEffects.SnakeHealth; // 실제 공용 체력
            SnakeShieldController shield = playerEffects.ShieldController; // 실제 공용 보호막
            Require(health != null && shield != null, "실제 신성 생존 구성 누락"); // 실제 생존 구성 확인
            CheckHeal(playerEffects, health, blade, 2, 1); // 실제 기본 회복
            CheckHeal(playerEffects, health, blade, 3, 2); // 실제 삼 등급 회복
            CheckHeal(playerEffects, health, blade, 5, 3); // 실제 오 등급 회복
            CheckHeal(playerEffects, health, wave, 5, 3); // 실제 오 등급 파동 회복
            CheckShield(playerEffects, shield, aura, 2, 15); // 실제 기본 보호막
            CheckShield(playerEffects, shield, aura, 3, 17); // 실제 삼 등급 보호막
            CheckShield(playerEffects, shield, aura, 5, 19); // 실제 오 등급 보호막
            CheckShield(playerEffects, shield, wave, 5, 17); // 실제 오 등급 파동 보호막
            Require(spear.GetProjectileHitLimit(3) == 3 && spear.GetProjectileHitLimit(5) == 4, "실제 신성 관통 횟수 오류"); // 실제 등급별 관통 확인
            SnakeWeaponSlot slot = weapons.Slots[0]; // 실제 첫 몸통 슬롯
            Require(weapons.TryEquipAt(0, dagger, 3), "실제 신성 속사 장착 실패"); // 실제 삼 등급 장착
            slot.StartCooldown(100f); // 실제 공격 주기 시작
            Require(!slot.IsReady(100.49f) && slot.IsReady(100.51f), "삼 등급 신성 속사 주기 오류"); // 실제 삼 등급 속도 확인
            Require(weapons.TryEquipAt(0, dagger, 5), "실제 오 등급 신성 속사 장착 실패"); // 실제 오 등급 장착
            slot.StartCooldown(100f); // 실제 오 등급 공격 주기
            Require(!slot.IsReady(100.45f) && slot.IsReady(100.47f), "오 등급 신성 속사 주기 오류"); // 실제 오 등급 속도 확인
            CheckCadence(weapons, slot, bolt, 3, 4, 1.2f); // 실제 삼 등급 신성 직사 주기
            CheckCadence(weapons, slot, bolt, 5, 3, 1.35f); // 실제 오 등급 신성 직사 주기
            yield return null; // 실제 지연 객체 정리 대기
            AutomatedDay124HolyTurretVerification.Finish(true, "Actual holy healing, shielding, cadence and piercing passed."); // 실제 시험 결과
        } // 함수 종료
        private static void CheckHeal(WeaponAttributePlayerEffects effects, SnakeHealth health, WeaponData weapon, int grade, int expected) // 실제 신성 확률 회복 확인
        { // 함수 시작
            health.ResetHealth(); // 실제 체력 초기화
            Require(health.TakeDamage(20), "실제 회복 시험 피해 실패"); // 실제 회복 필요 상태
            int before = health.CurrentHealth; // 실제 회복 전 체력
            UnityEngine.Random.InitState(124); // 재현 가능한 확률 준비
            UnityEngine.Random.State state; // 실제 회복 확률 상태
            do // 회복에 해당하는 난수 탐색
            { // 반복 시작
                state = UnityEngine.Random.state; // 난수 사용 전 상태
            } while (UnityEngine.Random.value >= WeaponAttributeHolyRules.HealChance); // 회복 가능 난수 확인
            UnityEngine.Random.state = state; // 실제 명중에 동일 난수 전달
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, weapon.Attribute, 4, 4, grade, Vector3.zero, 10f); // 실제 신성 시너지와 등급
            effects.HandleHolyHit(attack); // 실제 신성 명중 처리
            Require(health.CurrentHealth == before + expected, "실제 등급별 신성 회복 오류: " + weapon.DisplayName + "/" + grade); // 실제 회복량 확인
        } // 함수 종료
        private static void CheckShield(WeaponAttributePlayerEffects effects, SnakeShieldController shield, WeaponData weapon, int grade, int expected) // 실제 신성 보호막 확인
        { // 함수 시작
            shield.Clear(); // 실제 보호막 초기화
            WeaponAttributeAttackSnapshot reset = new WeaponAttributeAttackSnapshot(weapon, weapon.Attribute, 0, 0, grade, Vector3.zero, 10f); // 실제 초기 시너지
            effects.HandleHolyHit(reset); // 기존 명중 횟수 초기화
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, weapon.Attribute, 6, 6, grade, Vector3.zero, 10f); // 실제 보호막 시너지
            for (int index = 0; index < 5; index++) // 실제 명중 주기 순회
            { // 반복 시작
                effects.HandleHolyHit(attack); // 실제 신성 명중 처리
            } // 반복 종료
            Require(shield.CurrentShield == expected, "실제 등급별 신성 보호막 오류: " + weapon.DisplayName + "/" + grade); // 실제 보호막 확인
        } // 함수 종료
        private static void CheckCadence(SnakeWeaponManager weapons, SnakeWeaponSlot slot, WeaponData weapon, int grade, int interval, float multiplier) // 실제 등급별 주기 공격
        { // 함수 시작
            MethodInfo snapshot = typeof(SnakeWeaponManager).GetMethod("CreateAttackSnapshot", Private); // 실제 공격 정보 생성
            Require(snapshot != null && weapons.TryEquipAt(0, weapon, grade), "실제 신성 포탑 장착 실패"); // 실제 장착 확인
            float first = ((WeaponAttributeAttackSnapshot)snapshot.Invoke(weapons, new object[] { slot, Vector3.zero })).DirectDamage; // 실제 첫 공격 피해
            float last = first; // 실제 주기 공격 피해 초기화
            for (int index = 1; index < interval; index++) // 실제 남은 주기 공격 순회
            { // 반복 시작
                last = ((WeaponAttributeAttackSnapshot)snapshot.Invoke(weapons, new object[] { slot, Vector3.zero })).DirectDamage; // 실제 다음 공격 피해
            } // 반복 종료
            Require(Mathf.Approximately(last, first * multiplier), "실제 신성 포탑 주기 피해 오류: " + weapon.DisplayName + "/" + grade); // 실제 주기 강화 확인
        } // 함수 종료
        private static WeaponData Load(string name) // 실제 신성 포탑 데이터 조회
        { // 함수 시작
            return AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/PrototypeHoly" + name + ".asset"); // 실제 데이터 반환
        } // 함수 종료
        private static WeaponTarget CreateTarget() // 실제 전투 대상 생성
        { // 함수 시작
            GameObject item = new GameObject("Day122Target"); // 실제 격리 대상
            item.transform.position = new Vector3(500f, 500f, 0f); // 다른 전투와 분리된 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(100f); // 실제 대상 체력
            return target; // 실제 대상 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay124HolyTurretVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
