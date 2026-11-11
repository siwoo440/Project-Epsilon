using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 HUD 필드 조회
using ProjectEpsilon.Combat; // 무기와 표적
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어와 체력
using ProjectEpsilon.Progression; // 실제 경험치
using ProjectEpsilon.UI; // 실제 HUD
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.UI; // 실제 HUD 텍스트

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedGradeThreeVerification // 실제 삼 등급 시험 효과
    {
        private const string Key = "EpsilonGradeThreeVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedGradeThreeVerification() // 도메인 재시작 처리
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
            if (EditorApplication.timeSinceStartup - started > 120d) // 시험 제한 시간 확인
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
            player.gameObject.AddComponent<GradeThreeVerificationRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[GradeThreeVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class GradeThreeVerificationRunner : MonoBehaviour // 실제 삼 등급 공격 실행기
    {
        private IEnumerator Start() // 실제 무기 데이터와 공격 검증
        {
            WeaponRewardPool pool = AssetDatabase.LoadAssetAtPath<WeaponRewardPool>("Assets/Data/Progression/DebugWeaponRewardPool.asset"); // 실제 전체 보상 풀
            Require(pool != null && pool.Entries.Count == 48, "Expected 48 weapons."); // 전체 무기 수 확인
            MethodInfo areaMultiplier = typeof(WeaponData).GetMethod("GetAreaRangeMultiplier"); // 범위 등급 설정 조회
            Require(areaMultiplier != null, "Grade-three area multiplier is missing."); // 신규 설정 존재 확인
            foreach (WeaponRewardEntry entry in pool.Entries) // 전체 무기 순회
            {
                WeaponData weapon = entry.Weapon; // 실제 무기 데이터
                Require(weapon != null, "Missing weapon data."); // 무기 데이터 존재 확인
                if (weapon.AttackType == WeaponAttackType.Melee) // 근접 공격 확인
                {
                    Require(weapon.GetMeleeTargetLimit(2) == 1 && weapon.GetMeleeTargetLimit(3) == 2, "Melee grade settings missing: " + weapon.Id); // 등급별 대상 수 확인
                }
                else if (weapon.AttackType == WeaponAttackType.StraightProjectile) // 직선 공격 확인
                {
                    Require(weapon.GradeThreeTransferRange > 0f && weapon.GradeThreeTransferDamageMultiplier > 0f, "Projectile grade settings missing: " + weapon.Id); // 실제 전달 설정 확인
                }
                else if (weapon.AttackType == WeaponAttackType.Area) // 범위 공격 확인
                {
                    Require(Mathf.Approximately((float)areaMultiplier.Invoke(weapon, new object[] { 2 }), 1f) && (float)areaMultiplier.Invoke(weapon, new object[] { 3 }) > 1f, "Area grade settings missing: " + weapon.Id); // 등급별 공격 범위 확인
                }
                else // 지원하지 않는 공격 유형 확인
                {
                    Require(false, "Unexpected attack type: " + weapon.Id); // 미검증 무기 방지
                }
            }
            GetComponent<SnakeMovement>().enabled = false; // 플레이어 이동 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>().enabled = false; // 몸통 위치 고정
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            weapons.enabled = false; // 시험 준비 중 자동 공격 중지
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 무기 슬롯 순회
            {
                slot.Clear(); // 기존 무기 제거
            }
            foreach (WeaponTarget existing in UnityEngine.Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 적 순회
            {
                existing.gameObject.SetActive(false); // 시험 외 대상 비활성화
            }
            Vector3 origin = new Vector3(50f, 50f, 0f); // 머리에서 떨어진 몸통 위치
            weapons.Slots[2].Origin.position = origin; // 실제 몸통 위치 설정
            WeaponTarget first = CreateTarget("GradeFirst", origin + Vector3.right * 0.6f); // 첫 번째 표적
            WeaponTarget second = CreateTarget("GradeSecond", origin + Vector3.up * 1.5f); // 추가 표적
            WeaponData melee = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/PrototypeColdBlade.asset"); // 실제 냉기 근접 무기
            yield return AttackOnce(weapons, melee, 2); // 이 등급 근접 공격
            Require(first.CurrentHealth < 100f && Mathf.Approximately(second.CurrentHealth, 100f), "Grade-two melee hit extra target."); // 이 등급 단일 공격 확인
            first.Configure(100f); // 첫 표적 체력 복구
            second.Configure(100f); // 둘째 표적 체력 복구
            yield return AttackOnce(weapons, melee, 3); // 삼 등급 근접 공격
            Require(first.CurrentHealth < 100f && second.CurrentHealth < 100f, "Grade-three melee failed second target."); // 삼 등급 추가 대상 확인
            first.Configure(100f); // 첫 표적 체력 복구
            second.Configure(100f); // 둘째 표적 체력 복구
            WeaponData projectile = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/PrototypeColdBolt.asset"); // 실제 냉기 직선 무기
            yield return AttackOnce(weapons, projectile, 2); // 이 등급 직선 공격
            yield return new WaitForSeconds(0.4f); // 실제 투사체 명중 대기
            Require(first.CurrentHealth < 100f && Mathf.Approximately(second.CurrentHealth, 100f), "Grade-two projectile transferred damage."); // 이 등급 전달 방지 확인
            first.Configure(100f); // 첫 표적 체력 복구
            second.Configure(100f); // 둘째 표적 체력 복구
            yield return AttackOnce(weapons, projectile, 3); // 삼 등급 직선 공격
            yield return new WaitForSeconds(0.4f); // 실제 투사체 명중 대기
            Require(first.CurrentHealth < 100f && second.CurrentHealth < 100f, "Grade-three projectile failed transfer."); // 삼 등급 추가 전달 확인
            first.Configure(100f); // 첫 표적 체력 복구
            second.Configure(100f); // 둘째 표적 체력 복구
            WeaponData area = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/PrototypeColdAura.asset"); // 실제 냉기 범위 무기
            second.transform.position = origin + Vector3.up * (area.Range * 1.1f); // 기본 범위 밖 확장 범위 안 배치
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return AttackOnce(weapons, area, 2); // 이 등급 범위 공격
            Require(first.CurrentHealth < 100f && Mathf.Approximately(second.CurrentHealth, 100f), "Grade-two area exceeded base range."); // 기본 범위 유지 확인
            first.Configure(100f); // 첫 표적 체력 복구
            second.Configure(100f); // 둘째 표적 체력 복구
            yield return AttackOnce(weapons, area, 3); // 삼 등급 범위 공격
            Require(first.CurrentHealth < 100f && second.CurrentHealth < 100f, "Grade-three area failed expanded range."); // 삼 등급 범위 확대 확인
            first.gameObject.SetActive(false); // 이전 첫 표적 비활성화
            second.gameObject.SetActive(false); // 이전 둘째 표적 비활성화
            Destroy(first.gameObject); // 이전 첫 표적 제거
            Destroy(second.gameObject); // 이전 둘째 표적 제거
            foreach (WeaponRewardEntry entry in pool.Entries) // 실제 보상 무기 전체 순회
            {
                WeaponTarget target = CreateTarget("AllGradeFirst", origin + Vector3.right * 0.6f); // 실제 최초 표적
                WeaponTarget extra = CreateTarget("AllGradeSecond", origin + Vector3.up * 1.5f); // 실제 추가 표적
                yield return AttackOnce(weapons, entry.Weapon, 3); // 실제 삼 등급 자동 공격
                yield return new WaitForSeconds(0.3f); // 직선 투사체 실제 명중 대기
                Require(target.CurrentHealth < 100f && extra.CurrentHealth < 100f, "Grade-three weapon did not hit both controlled targets: " + entry.Weapon.Id); // 실제 무기별 두 대상 명중 확인
                target.gameObject.SetActive(false); // 이전 첫 표적 접촉 차단
                extra.gameObject.SetActive(false); // 이전 둘째 표적 접촉 차단
                Destroy(target.gameObject); // 이전 첫 표적 제거
                Destroy(extra.gameObject); // 이전 둘째 표적 제거
                foreach (StraightProjectile remaining in UnityEngine.Object.FindObjectsByType<StraightProjectile>(FindObjectsSortMode.None)) // 남은 투사체 순회
                {
                    remaining.gameObject.SetActive(false); // 이전 발사 접촉 차단
                    Destroy(remaining.gameObject); // 이전 발사 제거
                }
            }
            AutomatedGradeThreeVerification.Finish(0, "48 actual grade-three attacks and settings; grade-two/three melee, projectile and area comparisons."); // 전체 성공 기록
        }
        private static IEnumerator AttackOnce(SnakeWeaponManager weapons, WeaponData weapon, int grade) // 실제 자동 공격 한 번 실행
        {
            Require(weapon != null && weapons.TryEquipAt(2, weapon, grade), "Could not equip grade test weapon."); // 실제 몸통 무기 장착
            weapons.enabled = true; // 실제 자동 공격 시작
            yield return new WaitForSeconds(0.12f); // 첫 공격 대기
            weapons.enabled = false; // 반복 공격 중지
        }
        private static WeaponTarget CreateTarget(string name, Vector3 position) // 실제 공격 표적 생성
        {
            GameObject item = new GameObject(name); // 통제 표적 객체
            item.transform.position = position; // 실제 표적 위치
            CircleCollider2D collider = item.AddComponent<CircleCollider2D>(); // 실제 표적 충돌체
            collider.radius = 0.25f; // 표적 반경
            collider.isTrigger = true; // 투사체 트리거 설정
            Rigidbody2D physicsBody = item.AddComponent<Rigidbody2D>(); // 실제 표적 물리체
            physicsBody.bodyType = RigidbodyType2D.Kinematic; // 고정 표적 물리 방식
            physicsBody.useFullKinematicContacts = true; // 운동학 접촉 허용
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 무기 피해 대상
            target.Configure(100f); // 표적 체력 설정
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            return target; // 실제 표적 반환
        }
        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedGradeThreeVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
