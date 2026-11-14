using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 실제 발사 정보 조회
using ProjectEpsilon.Combat; // 실제 포탑과 화상
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
    public static class AutomatedDay119FireTurretVerification // 실제 화염 포탑 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay119FireTurrets"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay119FireTurretVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 실제 화염 포탑 시험
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
            new GameObject("Day119FireRunner").AddComponent<Day119FireRunner>(); // 실제 포탑 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day119FireTurretVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day119FireRunner : MonoBehaviour // 실제 화염 포탑 시험 실행기
    { // 클래스 시작
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 발사 정보 조회
        private IEnumerator Start() // 실제 포탑별 화상과 공격 확인
        { // 함수 시작
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            WeaponAttributeCombatEffects effects = weapons == null ? null : weapons.AttributeCombatEffects; // 실제 속성 전투 처리기
            Require(player != null && weapons != null && spawner != null && effects != null, "실제 화염 포탑 시험 구성 누락"); // 실제 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 자동 공격 간섭 방지
            foreach (WeaponTarget old in FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 적 순회
            { // 반복 시작
                old.gameObject.SetActive(false); // 기존 적의 시험 간섭 방지
            } // 반복 종료
            WeaponData cutter = Load("DebugCutter"); // 실제 화염 근접 포탑
            WeaponData pulse = Load("DebugPulse"); // 실제 화염 범위 포탑
            WeaponData brand = Load("PrototypeFireBrand"); // 실제 화염 중근접 포탑
            WeaponData lance = Load("PrototypeFireLance"); // 실제 화염 관통 포탑
            WeaponData burst = Load("PrototypeFireBurst"); // 실제 화염 파동 포탑
            WeaponData fireball = Load("DebugBlaster"); // 실제 화염탄 포탑
            Require(cutter != null && pulse != null && brand != null && lance != null && burst != null && fireball != null, "실제 화염 포탑 데이터 누락"); // 실제 데이터 확인
            CheckDamage(effects, cutter, 2, 1f); // 실제 낮은 등급 화상 대상 피해
            CheckDamage(effects, cutter, 3, 1.2f); // 실제 삼 등급 화상 대상 피해
            CheckDamage(effects, cutter, 5, 1.35f); // 실제 오 등급 화상 대상 피해
            CheckUnburned(effects, cutter); // 실제 화상 없는 첫 명중 확인
            CheckBurn(effects, pulse, 2, 3f, 1f); // 실제 기본 화상
            CheckBurn(effects, pulse, 3, 3.6f, 1f); // 실제 삼 등급 화상 시간
            CheckBurn(effects, pulse, 5, 4.2f, 1f); // 실제 오 등급 화상 시간
            CheckBurn(effects, pulse, 3, 4.68f, 1.2f, 4); // 실제 화염 시너지와 등급 결합
            CheckBurn(effects, burst, 3, 3f, 1.15f); // 실제 삼 등급 화상 피해
            CheckBurn(effects, burst, 5, 3f, 1.3f); // 실제 오 등급 화상 피해
            CheckDamage(effects, lance, 5, 1.1f); // 실제 오 등급 화상 관통 피해
            Require(lance.GetProjectileHitLimit(3) == 3 && lance.GetProjectileHitLimit(5) == 4, "실제 화염 관통 횟수 오류"); // 실제 등급별 관통 확인
            MethodInfo snapshot = typeof(SnakeWeaponManager).GetMethod("CreateAttackSnapshot", Private); // 실제 공격 시점 피해 정보
            Require(snapshot != null && weapons.TryEquipAt(0, brand, 3), "실제 화염 중근접 장착 실패"); // 실제 삼 등급 장착
            SnakeWeaponSlot slot = weapons.Slots[0]; // 실제 첫 몸통 슬롯
            float first = ((WeaponAttributeAttackSnapshot)snapshot.Invoke(weapons, new object[] { slot, Vector3.zero })).DirectDamage; // 실제 첫 공격 피해
            snapshot.Invoke(weapons, new object[] { slot, Vector3.zero }); // 실제 둘째 공격
            float third = ((WeaponAttributeAttackSnapshot)snapshot.Invoke(weapons, new object[] { slot, Vector3.zero })).DirectDamage; // 실제 셋째 공격 피해
            Require(Mathf.Approximately(third, first * 1.2f), "삼 등급 화염 중근접 주기 피해 오류"); // 실제 삼 등급 주기 확인
            Require(weapons.TryEquipAt(0, brand, 5), "실제 오 등급 화염 중근접 장착 실패"); // 실제 오 등급 장착
            first = ((WeaponAttributeAttackSnapshot)snapshot.Invoke(weapons, new object[] { slot, Vector3.zero })).DirectDamage; // 실제 새 장착 첫 공격
            float second = ((WeaponAttributeAttackSnapshot)snapshot.Invoke(weapons, new object[] { slot, Vector3.zero })).DirectDamage; // 실제 둘째 공격
            Require(Mathf.Approximately(second, first * 1.3f), "오 등급 화염 중근접 주기 피해 오류"); // 실제 오 등급 주기 확인
            Require(fireball.GradeFiveBurnExplosionRadius > 0f, "기존 화염탄 화상 폭발 변경"); // 실제 기존 전용 효과 확인
            yield return null; // 실제 지연 객체 정리 대기
            AutomatedDay119FireTurretVerification.Finish(true, "Actual fire turret burn, cadence and piercing effects passed."); // 실제 시험 결과
        } // 함수 종료
        private static void CheckDamage(WeaponAttributeCombatEffects effects, WeaponData weapon, int grade, float multiplier) // 실제 화상 대상 직접 피해
        { // 함수 시작
            WeaponTarget target = CreateTarget(); // 실제 공격 대상
            WeaponTargetBurnStatus burn = target.gameObject.AddComponent<WeaponTargetBurnStatus>(); // 실제 화상 상태
            Require(burn.Apply(target, 0, 3f, 1f), "실제 화상 상태 준비 실패"); // 실제 화상 적용 확인
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, weapon.Attribute, 1, 0, grade, target.transform.position, 10f); // 실제 등급 공격 정보
            effects.ApplyHit(attack, target, target.transform.position); // 실제 속성 전투 명중
            Require(Mathf.Abs(target.CurrentHealth - (100f - 10f * multiplier)) < 0.001f, "실제 화상 대상 직접 피해 오류: " + weapon.DisplayName + "/" + grade); // 실제 등급별 피해 확인
            target.gameObject.SetActive(false); // 다음 시험의 대상 간섭 방지
            Destroy(target.gameObject); // 실제 시험 대상 정리
        } // 함수 종료
        private static void CheckUnburned(WeaponAttributeCombatEffects effects, WeaponData weapon) // 실제 화상 없는 첫 명중
        { // 함수 시작
            WeaponTarget target = CreateTarget(); // 실제 공격 대상
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, weapon.Attribute, 1, 0, 5, target.transform.position, 10f); // 실제 오 등급 공격 정보
            effects.ApplyHit(attack, target, target.transform.position); // 실제 화상 전 첫 명중
            Require(Mathf.Abs(target.CurrentHealth - 90f) < 0.001f && target.GetComponent<WeaponTargetBurnStatus>() != null, "화상 없는 첫 명중의 피해 또는 화상 적용 오류"); // 실제 첫 피해와 신규 화상 확인
            target.gameObject.SetActive(false); // 다음 시험의 대상 간섭 방지
            Destroy(target.gameObject); // 실제 시험 대상 정리
        } // 함수 종료
        private static void CheckBurn(WeaponAttributeCombatEffects effects, WeaponData weapon, int grade, float duration, float damage, int stage = 0) // 실제 등급별 화상 효과
        { // 함수 시작
            WeaponTarget target = CreateTarget(); // 실제 공격 대상
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, weapon.Attribute, stage, stage, grade, target.transform.position, 1f); // 실제 등급과 시너지 공격 정보
            effects.ApplyHit(attack, target, target.transform.position); // 실제 화염 명중
            WeaponTargetBurnStatus burn = target.GetComponent<WeaponTargetBurnStatus>(); // 실제 화상 상태
            Require(burn != null && Mathf.Abs(burn.RemainingDuration - duration) < 0.001f && Mathf.Abs(burn.DamagePerSecond - damage) < 0.001f, "실제 화상 시간 또는 피해 오류: " + weapon.DisplayName + "/" + grade); // 실제 화상 시험값 확인
            target.gameObject.SetActive(false); // 다음 시험의 대상 간섭 방지
            Destroy(target.gameObject); // 실제 시험 대상 정리
        } // 함수 종료
        private static WeaponData Load(string name) // 실제 포탑 데이터 조회
        { // 함수 시작
            return AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/" + name + ".asset"); // 실제 데이터 반환
        } // 함수 종료
        private static WeaponTarget CreateTarget() // 실제 전투 대상 생성
        { // 함수 시작
            GameObject item = new GameObject("Day119Target"); // 실제 격리 대상
            item.transform.position = new Vector3(500f, 500f, 0f); // 다른 전투와 분리된 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(100f); // 실제 대상 체력
            return target; // 실제 대상 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay119FireTurretVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
