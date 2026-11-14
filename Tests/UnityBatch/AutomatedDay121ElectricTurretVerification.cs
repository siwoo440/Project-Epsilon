using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 실제 발사 정보 조회
using ProjectEpsilon.Combat; // 실제 포탑과 전기 연쇄
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
    public static class AutomatedDay121ElectricTurretVerification // 실제 전기 포탑 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay121ElectricTurrets"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay121ElectricTurretVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 실제 전기 포탑 시험
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
            new GameObject("Day121ElectricRunner").AddComponent<Day121ElectricRunner>(); // 실제 포탑 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day121ElectricTurretVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day121ElectricRunner : MonoBehaviour // 실제 전기 포탑 시험 실행기
    { // 클래스 시작
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 발사 정보 조회
        private IEnumerator Start() // 실제 포탑별 연쇄와 공격 확인
        { // 함수 시작
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            WeaponAttributeCombatEffects effects = weapons == null ? null : weapons.AttributeCombatEffects; // 실제 속성 전투 처리기
            Require(player != null && weapons != null && spawner != null && effects != null, "실제 전기 포탑 시험 구성 누락"); // 실제 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 자동 공격 간섭 방지
            foreach (WeaponTarget old in FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 적 순회
            { // 반복 시작
                old.gameObject.SetActive(false); // 기존 적의 시험 간섭 방지
            } // 반복 종료
            WeaponData blade = Load("Blade"); // 실제 전기 근접 포탑
            WeaponData aura = Load("Aura"); // 실제 전기 범위 포탑
            WeaponData dagger = Load("Dagger"); // 실제 전기 속사 근접 포탑
            WeaponData bolt = Load("Bolt"); // 실제 전기 직사 포탑
            WeaponData spear = Load("Spear"); // 실제 전기 관통 포탑
            WeaponData wave = Load("Wave"); // 실제 전기 파동 포탑
            Require(blade != null && aura != null && dagger != null && bolt != null && spear != null && wave != null, "실제 전기 포탑 데이터 누락"); // 실제 데이터 확인
            CheckChain(effects, blade, 2, 0, 0); // 실제 낮은 등급 무시너지 연쇄
            CheckChain(effects, blade, 3, 0, 1); // 실제 삼 등급 단독 연쇄
            CheckChain(effects, blade, 5, 0, 2); // 실제 오 등급 단독 연쇄
            CheckChain(effects, blade, 3, 2, 2); // 실제 삼 등급과 시너지 연쇄 합산
            CheckChain(effects, aura, 3, 0, 1); // 실제 전기 범위 단독 연쇄
            CheckRange(effects, aura, 3, 2.5f, true); // 실제 삼 등급 확장 연쇄 범위
            CheckRange(effects, aura, 3, 2.8f, false); // 실제 삼 등급 연쇄 범위 바깥
            CheckRange(effects, aura, 5, 2.8f, true); // 실제 오 등급 확장 연쇄 범위
            CheckChain(effects, dagger, 5, 0, 1); // 실제 전기 속사 오 등급 연쇄
            CheckChain(effects, bolt, 5, 0, 1); // 실제 전기 직사 오 등급 연쇄
            CheckChain(effects, spear, 5, 0, 1); // 실제 전기 관통 오 등급 연쇄
            CheckChain(effects, wave, 5, 0, 2); // 실제 전기 파동 오 등급 연쇄
            Require(spear.GetProjectileHitLimit(3) == 3 && spear.GetProjectileHitLimit(5) == 4, "실제 전기 관통 횟수 오류"); // 실제 등급별 관통 확인
            SnakeWeaponSlot slot = weapons.Slots[0]; // 실제 첫 몸통 슬롯
            Require(weapons.TryEquipAt(0, dagger, 3), "실제 전기 속사 장착 실패"); // 실제 삼 등급 장착
            slot.StartCooldown(100f); // 실제 공격 주기 시작
            Require(!slot.IsReady(100.49f) && slot.IsReady(100.51f), "삼 등급 전기 속사 주기 오류"); // 실제 삼 등급 속도 확인
            Require(weapons.TryEquipAt(0, dagger, 5), "실제 오 등급 전기 속사 장착 실패"); // 실제 오 등급 장착
            slot.StartCooldown(100f); // 실제 오 등급 공격 주기
            Require(!slot.IsReady(100.45f) && slot.IsReady(100.47f), "오 등급 전기 속사 주기 오류"); // 실제 오 등급 속도 확인
            CheckCadence(weapons, slot, bolt, 3, 4, 1.2f); // 실제 삼 등급 전기 직사 주기
            CheckCadence(weapons, slot, bolt, 5, 3, 1.35f); // 실제 오 등급 전기 직사 주기
            CheckCadence(weapons, slot, wave, 3, 3, 1.2f); // 실제 삼 등급 전기 파동 주기
            CheckCadence(weapons, slot, wave, 5, 2, 1.3f); // 실제 오 등급 전기 파동 주기
            CheckVictory(effects, blade); // 실제 등급 연쇄의 승리 경계
            yield return null; // 실제 지연 객체 정리 대기
            AutomatedDay121ElectricTurretVerification.Finish(true, "Actual electric turret chains, cadence and piercing effects passed."); // 실제 시험 결과
        } // 함수 종료
        private static void CheckChain(WeaponAttributeCombatEffects effects, WeaponData weapon, int grade, int stage, int secondaryCount) // 실제 등급별 연쇄 공격
        { // 함수 시작
            WeaponTarget[] targets = new WeaponTarget[4]; // 실제 연쇄 표적 목록
            for (int index = 0; index < targets.Length; index++) // 실제 연쇄 표적 생성
            { // 반복 시작
                GameObject item = new GameObject("Day121Target"); // 실제 격리 대상
                item.transform.position = new Vector3(500f + index * 0.5f, 500f, 0f); // 실제 연쇄 거리의 대상
                targets[index] = item.AddComponent<WeaponTarget>(); // 실제 공격 대상
                targets[index].Configure(100f); // 실제 대상 체력
            } // 반복 종료
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, weapon.Attribute, stage, stage, grade, targets[0].transform.position, 10f); // 실제 등급과 시너지 공격 정보
            effects.ApplyHit(attack, targets[0], targets[0].transform.position); // 실제 전기 명중과 연쇄
            for (int index = 0; index < targets.Length; index++) // 실제 연쇄 피해 확인
            { // 반복 시작
                Require(Mathf.Abs(targets[index].CurrentHealth - (index <= secondaryCount ? 90f : 100f)) < 0.001f, "실제 전기 연쇄 대상 오류: " + weapon.DisplayName + "/" + grade + "/" + stage + "/" + index); // 실제 연쇄 대상 수 확인
                targets[index].gameObject.SetActive(false); // 다음 시험의 대상 간섭 방지
                Destroy(targets[index].gameObject); // 실제 시험 대상 정리
            } // 반복 종료
        } // 함수 종료
        private static void CheckCadence(SnakeWeaponManager weapons, SnakeWeaponSlot slot, WeaponData weapon, int grade, int interval, float multiplier) // 실제 등급별 주기 공격
        { // 함수 시작
            MethodInfo snapshot = typeof(SnakeWeaponManager).GetMethod("CreateAttackSnapshot", Private); // 실제 공격 정보 생성
            Require(snapshot != null && weapons.TryEquipAt(0, weapon, grade), "실제 전기 포탑 장착 실패"); // 실제 장착 확인
            float first = ((WeaponAttributeAttackSnapshot)snapshot.Invoke(weapons, new object[] { slot, Vector3.zero })).DirectDamage; // 실제 첫 공격 피해
            float last = first; // 실제 주기 공격 피해 초기화
            for (int index = 1; index < interval; index++) // 실제 남은 주기 공격 순회
            { // 반복 시작
                last = ((WeaponAttributeAttackSnapshot)snapshot.Invoke(weapons, new object[] { slot, Vector3.zero })).DirectDamage; // 실제 다음 공격 피해
            } // 반복 종료
            Require(Mathf.Approximately(last, first * multiplier), "실제 전기 포탑 주기 피해 오류: " + weapon.DisplayName + "/" + grade); // 실제 주기 강화 확인
        } // 함수 종료
        private static void CheckRange(WeaponAttributeCombatEffects effects, WeaponData weapon, int grade, float distance, bool expected) // 실제 등급별 연쇄 범위
        { // 함수 시작
            GameObject firstObject = new GameObject("Day121RangeFirst"); // 실제 첫 표적 객체
            firstObject.transform.position = new Vector3(600f, 600f, 0f); // 다른 전투와 분리된 위치
            WeaponTarget first = firstObject.AddComponent<WeaponTarget>(); // 실제 첫 공격 대상
            first.Configure(100f); // 실제 첫 대상 체력
            GameObject otherObject = new GameObject("Day121RangeOther"); // 실제 연쇄 표적 객체
            otherObject.transform.position = firstObject.transform.position + Vector3.right * distance; // 실제 등급 범위 경계 거리
            WeaponTarget other = otherObject.AddComponent<WeaponTarget>(); // 실제 연쇄 대상
            other.Configure(100f); // 실제 연쇄 대상 체력
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, weapon.Attribute, 0, 0, grade, first.transform.position, 10f); // 실제 등급 공격 정보
            effects.ApplyHit(attack, first, first.transform.position); // 실제 전기 명중과 연쇄
            Require(Mathf.Abs(other.CurrentHealth - (expected ? 90f : 100f)) < 0.001f, "실제 등급별 연쇄 범위 오류: " + grade + "/" + distance); // 실제 연쇄 범위 경계 확인
            firstObject.SetActive(false); // 다음 시험의 첫 대상 간섭 방지
            otherObject.SetActive(false); // 다음 시험의 연쇄 대상 간섭 방지
            Destroy(firstObject); // 실제 첫 대상 정리
            Destroy(otherObject); // 실제 연쇄 대상 정리
        } // 함수 종료
        private static void CheckVictory(WeaponAttributeCombatEffects effects, WeaponData weapon) // 실제 등급 연쇄의 승리 경계
        { // 함수 시작
            GameObject bossObject = new GameObject("Day121Boss"); // 실제 최종 보스 객체
            bossObject.transform.position = new Vector3(700f, 700f, 0f); // 다른 전투와 분리된 위치
            WeaponTarget boss = bossObject.AddComponent<WeaponTarget>(); // 실제 최종 보스 대상
            boss.Configure(1f); // 실제 마지막 공격의 보스 체력
            GameObject otherObject = new GameObject("Day121AfterVictory"); // 실제 후속 연쇄 대상
            otherObject.transform.position = bossObject.transform.position + Vector3.right * 0.5f; // 실제 연쇄 범위 위치
            WeaponTarget other = otherObject.AddComponent<WeaponTarget>(); // 실제 후속 공격 대상
            other.Configure(100f); // 실제 후속 대상 체력
            Require(GameManager.Instance.RegisterFinalBoss(boss), "실제 최종 보스 등록 실패"); // 실제 승리 조건 연결
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, weapon.Attribute, 0, 0, 5, boss.transform.position, 10f); // 실제 무시너지 오 등급 공격
            effects.ApplyHit(attack, boss, boss.transform.position); // 실제 보스 처치와 등급 연쇄
            Require(GameManager.Instance.CurrentState == GameState.Victory && Mathf.Approximately(other.CurrentHealth, 100f), "승리 확정 뒤 등급 연쇄 피해 발생"); // 실제 승리와 후속 피해 차단 확인
        } // 함수 종료
        private static WeaponData Load(string name) // 실제 전기 포탑 데이터 조회
        { // 함수 시작
            return AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/PrototypeElectric" + name + ".asset"); // 실제 데이터 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay121ElectricTurretVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
