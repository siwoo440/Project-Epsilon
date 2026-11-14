using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 실제 공격 경로 조회
using ProjectEpsilon.Combat; // 실제 포탑과 대상
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
    public static class AutomatedDay118PhysicalTurretVerification // 실제 물리 포탑 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay118PhysicalTurrets"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay118PhysicalTurretVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 실제 물리 포탑 시험
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
            new GameObject("Day118PhysicalRunner").AddComponent<Day118PhysicalRunner>(); // 실제 포탑 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day118PhysicalTurretVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day118PhysicalRunner : MonoBehaviour // 실제 물리 포탑 시험 실행기
    { // 클래스 시작
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 공격 경로 조회
        private IEnumerator Start() // 실제 포탑별 공격 확인
        { // 함수 시작
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            Require(player != null && weapons != null && spawner != null && weapons.Slots.Count >= 1, "실제 포탑 시험 구성 누락"); // 실제 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 자동 공격 간섭 방지
            foreach (WeaponTarget old in FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 적 순회
            { // 반복 시작
                old.gameObject.SetActive(false); // 기존 적의 시험 간섭 방지
            } // 반복 종료
            WeaponData needle = Load("DebugNeedle"); // 실제 강철 관통 포탑
            WeaponData dagger = Load("PrototypePhysicalDagger"); // 실제 물리 속사 근접 포탑
            WeaponData javelin = Load("PrototypePhysicalJavelin"); // 실제 물리 속사 직사 포탑
            WeaponData nova = Load("DebugNova"); // 실제 강철 충격파 포탑
            WeaponData hammer = Load("PrototypePhysicalHammer"); // 실제 물리 중파동 포탑
            WeaponData blade = Load("DebugBlade"); // 실제 강철 근접 포탑
            Require(needle != null && dagger != null && javelin != null && nova != null && hammer != null && blade != null, "실제 물리 포탑 데이터 누락"); // 실제 데이터 확인
            CheckPiercing(needle, 1, 2); // 실제 일 등급 기본 관통
            CheckPiercing(needle, 3, 3); // 실제 삼 등급 추가 관통
            CheckPiercing(needle, 5, 4); // 실제 오 등급 추가 관통
            SnakeWeaponSlot slot = weapons.Slots[0]; // 실제 첫 몸통 슬롯
            Require(weapons.TryEquipAt(0, dagger, 3), "실제 속사 근접 장착 실패"); // 실제 삼 등급 장착
            slot.StartCooldown(100f); // 실제 등급별 공격 주기 시작
            Require(!slot.IsReady(100.47f) && slot.IsReady(100.49f), "삼 등급 속사 근접 공격 주기 오류"); // 실제 삼 등급 속도 확인
            Require(weapons.TryEquipAt(0, dagger, 5), "실제 오 등급 속사 근접 장착 실패"); // 실제 오 등급 장착
            slot.StartCooldown(100f); // 실제 오 등급 공격 주기 시작
            Require(!slot.IsReady(100.43f) && slot.IsReady(100.45f), "오 등급 속사 근접 공격 주기 오류"); // 실제 오 등급 속도 확인
            MethodInfo snapshot = typeof(SnakeWeaponManager).GetMethod("CreateAttackSnapshot", Private); // 실제 발사 시점 공격 정보
            Require(snapshot != null && weapons.TryEquipAt(0, javelin, 3), "실제 속사 직사 시험 준비 실패"); // 실제 포탑 장착 확인
            float first = ((WeaponAttributeAttackSnapshot)snapshot.Invoke(weapons, new object[] { slot, Vector3.zero })).DirectDamage; // 실제 첫 발사 피해
            snapshot.Invoke(weapons, new object[] { slot, Vector3.zero }); // 실제 둘째 발사
            snapshot.Invoke(weapons, new object[] { slot, Vector3.zero }); // 실제 셋째 발사
            float fourth = ((WeaponAttributeAttackSnapshot)snapshot.Invoke(weapons, new object[] { slot, Vector3.zero })).DirectDamage; // 실제 넷째 발사 피해
            Require(Mathf.Approximately(fourth, first * 1.2f), "삼 등급 속사 직사 주기 강화 오류"); // 실제 삼 등급 주기 피해 확인
            Require(weapons.TryEquipAt(0, javelin, 5), "실제 오 등급 속사 직사 장착 실패"); // 실제 오 등급 장착
            first = ((WeaponAttributeAttackSnapshot)snapshot.Invoke(weapons, new object[] { slot, Vector3.zero })).DirectDamage; // 실제 새 장착 첫 발사
            snapshot.Invoke(weapons, new object[] { slot, Vector3.zero }); // 실제 둘째 발사
            float third = ((WeaponAttributeAttackSnapshot)snapshot.Invoke(weapons, new object[] { slot, Vector3.zero })).DirectDamage; // 실제 셋째 발사 피해
            Require(Mathf.Approximately(third, first * 1.35f), "오 등급 속사 직사 주기 강화 또는 재장착 초기화 오류"); // 실제 오 등급 주기 확인
            Vector3 origin = new Vector3(500f, 500f, 0f); // 다른 전투와 분리된 몸통 위치
            slot.Origin.position = origin; // 실제 몸통 위치 이동
            WeaponTarget target = CreateTarget(origin + Vector3.right); // 실제 범위 공격 대상
            MethodInfo area = typeof(SnakeWeaponManager).GetMethod("TryAreaAttack", Private); // 실제 범위 공격 경로
            Require(area != null && weapons.TryEquipAt(0, hammer, 3), "실제 중파동 포탑 준비 실패"); // 실제 삼 등급 장착
            float before = target.CurrentHealth; // 실제 첫 공격 전 체력
            area.Invoke(weapons, new object[] { slot, hammer }); // 실제 첫 중파동 공격
            float normal = before - target.CurrentHealth; // 실제 기본 공격 피해
            area.Invoke(weapons, new object[] { slot, hammer }); // 실제 둘째 중파동 공격
            before = target.CurrentHealth; // 실제 셋째 공격 전 체력
            area.Invoke(weapons, new object[] { slot, hammer }); // 실제 셋째 중파동 공격
            Require(Mathf.Approximately(before - target.CurrentHealth, normal * 1.2f), "삼 등급 중파동 주기 피해 오류"); // 실제 삼 등급 피해 확인
            Require(weapons.TryEquipAt(0, hammer, 5), "실제 오 등급 중파동 장착 실패"); // 실제 오 등급 장착
            before = target.CurrentHealth; // 실제 새 장착 전 체력
            area.Invoke(weapons, new object[] { slot, hammer }); // 실제 첫 오 등급 공격
            normal = before - target.CurrentHealth; // 실제 첫 오 등급 피해
            before = target.CurrentHealth; // 실제 둘째 공격 전 체력
            area.Invoke(weapons, new object[] { slot, hammer }); // 실제 둘째 오 등급 공격
            Require(Mathf.Abs((before - target.CurrentHealth) - normal * 1.35f) < 0.001f, "오 등급 중파동 주기 피해 오류: " + (before - target.CurrentHealth) + " / " + normal); // 실제 오 등급 피해 확인
            Require(weapons.TryEquipAt(0, nova, 5), "실제 오 등급 충격파 장착 실패"); // 실제 오 등급 장착
            area.Invoke(weapons, new object[] { slot, nova }); // 실제 충격파 공격
            WeaponTargetKnockbackController knockback = target.GetComponent<WeaponTargetKnockbackController>(); // 실제 공격 대상 넉백
            Require(knockback != null && knockback.IsActive, "오 등급 충격파 넉백 누락"); // 실제 일반 적 넉백 확인
            knockback.Configure(true); // 실제 보스 넉백 면역
            area.Invoke(weapons, new object[] { slot, nova }); // 실제 면역 대상 충격파
            Require(!knockback.IsActive, "실제 보스 넉백 면역 위반"); // 실제 넉백 면역 유지 확인
            Require(blade.GradeFiveWideStrikeInterval == 4 && blade.GradeFiveWideStrikeRadius > 0f, "기존 강철 근접 강타 변경"); // 기존 전용 강타 보존 확인
            yield return null; // 실제 지연 객체 정리 대기
            AutomatedDay118PhysicalTurretVerification.Finish(true, "Actual physical turret piercing, cadence, knockback and immunity passed."); // 실제 시험 결과
        } // 함수 종료
        private static void CheckPiercing(WeaponData weapon, int grade, int expected) // 실제 등급별 관통 확인
        { // 함수 시작
            GameObject item = new GameObject("Day118Projectile"); // 실제 투사체 객체
            StraightProjectile projectile = item.AddComponent<StraightProjectile>(); // 실제 직선 투사체
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, weapon.Attribute, 1, 0, grade, Vector3.zero, 1f); // 실제 등급 공격 정보
            projectile.Configure(Vector2.right, attack, null, 8f, 1f, null); // 실제 투사체 발사 설정
            int remaining = (int)typeof(StraightProjectile).GetField("remainingHits", Private).GetValue(projectile); // 실제 남은 관통 횟수
            Require(remaining == expected, "실제 관통 횟수 오류: " + grade); // 실제 등급별 관통 확인
            MethodInfo hit = typeof(StraightProjectile).GetMethod("OnTriggerEnter2D", Private); // 실제 투사체 접촉 경로
            Require(hit != null, "실제 투사체 접촉 경로 누락"); // 실제 물리 접촉 확인
            for (int index = 0; index < 5; index++) // 실제 다섯 대상 접촉
            { // 반복 시작
                WeaponTarget target = CreateTarget(new Vector3(100f + index * 10f, 200f + grade * 20f)); // 전달 범위 밖의 실제 대상
                CircleCollider2D collider = target.gameObject.AddComponent<CircleCollider2D>(); // 실제 접촉 충돌체
                hit.Invoke(projectile, new object[] { collider }); // 실제 투사체 접촉 처리
                Require(Mathf.Approximately(target.CurrentHealth, index < expected ? 999f : 1000f), "실제 등급별 관통 피해 오류: " + grade + "/" + index); // 실제 횟수 내 대상만 피해 확인
                target.gameObject.SetActive(false); // 다른 시험의 대상 간섭 방지
                Destroy(target.gameObject); // 실제 시험 대상 정리
            } // 반복 종료
            item.SetActive(false); // 실제 시험 탄 이동 차단
            Destroy(item); // 실제 시험 탄 정리
        } // 함수 종료
        private static WeaponData Load(string name) // 실제 포탑 데이터 조회
        { // 함수 시작
            return AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/" + name + ".asset"); // 실제 데이터 반환
        } // 함수 종료
        private static WeaponTarget CreateTarget(Vector3 position) // 실제 전투 대상 생성
        { // 함수 시작
            GameObject item = new GameObject("Day118Target"); // 실제 격리 대상
            item.transform.position = position; // 실제 대상 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(1000f); // 실제 대상 체력
            return target; // 실제 대상 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay118PhysicalTurretVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
