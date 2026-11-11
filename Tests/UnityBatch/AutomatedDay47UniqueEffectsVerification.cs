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
    public static class AutomatedDay47UniqueEffectsVerification // 실제 오 등급 시험 효과
    {
        private const string Key = "EpsilonDay47UniqueEffectsVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedDay47UniqueEffectsVerification() // 도메인 재시작 처리
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
            player.gameObject.AddComponent<Day47UniqueEffectsRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[Day47UniqueEffectsVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class Day47UniqueEffectsRunner : MonoBehaviour // 실제 고유 효과 검증
    {
        private IEnumerator Start() // 실제 표적과 등급 효과 시험
        {
            GetComponent<SnakeMovement>().enabled = false; // 플레이어 이동 통제
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>().enabled = false; // 몸통 위치 통제
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            WeaponAttributeCombatEffects effects = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 속성 전투 처리기
            WeaponData steel = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 강철 근접 데이터
            WeaponData fire = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlaster.asset"); // 실제 화염탄 데이터
            Require(weapons != null && effects != null && steel != null && fire != null, "Actual turret fixtures missing."); // 실제 시험 구성 확인
            Require(typeof(WeaponData).GetProperty("GradeFiveWideStrikeInterval") != null && typeof(WeaponData).GetProperty("GradeFiveBurnExplosionRadius") != null, "Per-turret grade-five effects missing."); // 실제 고유 설정 계약 확인
            weapons.enabled = false; // 반복 자동 공격 통제
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 기존 포탑 순회
            {
                slot.Clear(); // 기존 포탑 제거
            }
            foreach (WeaponTarget enemy in UnityEngine.Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 적 순회
            {
                enemy.gameObject.SetActive(false); // 시험 외 적 차단
            }
            Vector3 origin = new Vector3(100f, 100f, 0f); // 실제 통제 몸통 위치
            weapons.Slots[2].Origin.position = origin; // 실제 몸통 이동
            Require(weapons.TryEquipAt(2, steel, 5), "Steel grade five equip failed."); // 실제 강철 포탑 장착
            WeaponTarget first = Target(origin + Vector3.right * 10f, 1000f); // 사거리 밖 최근접 적
            WeaponTarget second = Target(origin + Vector3.right * 10.8f, 1000f); // 강타 범위 안 적
            WeaponTarget third = Target(origin + Vector3.right * 11.4f, 1000f); // 세 번째 강타 범위 적
            WeaponTarget outside = Target(origin + Vector3.right * 15f, 1000f); // 강타 범위 밖 적
            MethodInfo melee = typeof(SnakeWeaponManager).GetMethod("TryMeleeAttack", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 근접 공격 경로
            Require(melee != null, "Actual melee attack missing."); // 실제 공격 경로 확인
            for (int index = 0; index < 3; index++) // 최초 세 공격 반복
            {
                Require((bool)melee.Invoke(weapons, new object[] { weapons.Slots[2], steel }), "Steel attack failed."); // 실제 강철 공격
            }
            Require(Mathf.Approximately(first.CurrentHealth, 934f) && Mathf.Approximately(second.CurrentHealth, 934f) && Mathf.Approximately(third.CurrentHealth, 1000f), "Steel ordinary attacks did not preserve two-target behavior."); // 강타 전 두 대상 유지
            melee.Invoke(weapons, new object[] { weapons.Slots[2], steel }); // 실제 네 번째 강타
            Require(Mathf.Approximately(first.CurrentHealth, 901f) && Mathf.Approximately(second.CurrentHealth, 901f) && Mathf.Approximately(third.CurrentHealth, 967f) && Mathf.Approximately(outside.CurrentHealth, 1000f), "Fourth steel attack did not deliver a bounded wide strike."); // 실제 강타 대상과 피해 확인
            Require(weapons.TryEquipAt(2, steel, 5), "Steel re-equip failed."); // 실제 포탑 재장착
            float thirdBefore = third.CurrentHealth; // 실제 셋째 대상 체력
            melee.Invoke(weapons, new object[] { weapons.Slots[2], steel }); // 새 장착의 첫 공격
            Require(Mathf.Approximately(third.CurrentHealth, thirdBefore), "Steel strike count leaked across re-equips."); // 재장착 강타 횟수 초기화 확인
            weapons.Slots[2].ApplyUpgrade(WeaponMergeUpgrade.Range); // 실제 합성 범위 강화
            WeaponTarget edge = Target(origin + Vector3.right * 12.05f, 1000f); // 기본 강타 밖 강화 반경 안 표적
            for (int index = 0; index < 3; index++) // 다음 강타까지 세 공격
            {
                melee.Invoke(weapons, new object[] { weapons.Slots[2], steel }); // 실제 강화 포탑 공격
            }
            Require(Mathf.Approximately(edge.CurrentHealth, 967f), "Steel wide strike ignored the inherited range upgrade."); // 실제 합성 범위 강화 확인
            edge.gameObject.SetActive(false); // 낮은 등급 시험 간섭 차단
            Destroy(edge.gameObject); // 실제 경계 표적 제거
            Require(weapons.TryEquipAt(2, steel, 4), "Steel grade-four equip failed."); // 실제 사 등급 포탑 장착
            thirdBefore = third.CurrentHealth; // 실제 셋째 대상 체력
            for (int index = 0; index < 4; index++) // 사 등급 네 공격 반복
            {
                melee.Invoke(weapons, new object[] { weapons.Slots[2], steel }); // 실제 사 등급 공격
            }
            Require(Mathf.Approximately(third.CurrentHealth, thirdBefore), "Steel wide strike activated below grade five."); // 낮은 등급 강타 방지
            foreach (WeaponTarget item in new[] { first, second, third, outside }) // 강철 표적 순회
            {
                item.gameObject.SetActive(false); // 화염 시험 간섭 차단
                Destroy(item.gameObject); // 실제 표적 제거
            }
            Vector3 fireOrigin = new Vector3(200f, 200f, 0f); // 화염 시험 위치
            WeaponAttributeAttackSnapshot gradeFour = effects.CreateAttackSnapshot(fire, 4, fireOrigin, 1f); // 실제 사 등급 화염 공격
            WeaponTarget fourVictim = Target(fireOrigin, 10f); // 사 등급 화상 대상
            WeaponTarget fourNeighbor = Target(fireOrigin + Vector3.right, 100f); // 사 등급 인접 대상
            effects.ApplyHit(gradeFour, fourVictim, fourVictim.transform.position); // 실제 사 등급 화상 적용
            fourVictim.TakeDamage(100f); // 실제 화상 대상 처치
            Require(Mathf.Approximately(fourNeighbor.CurrentHealth, 100f), "Grade four triggered grade-five explosion."); // 사 등급 폭발 금지
            fourNeighbor.gameObject.SetActive(false); // 다음 시나리오 간섭 차단
            Destroy(fourNeighbor.gameObject); // 실제 표적 제거
            Vector3 secondOrigin = new Vector3(300f, 300f, 0f); // 오 등급 별도 위치
            WeaponAttributeAttackSnapshot gradeFive = effects.CreateAttackSnapshot(fire, 5, secondOrigin, 10f); // 실제 오 등급 화염 공격
            WeaponTarget victim = Target(secondOrigin, 100f); // 실제 화상 대상
            WeaponTarget near = Target(secondOrigin + Vector3.right, 100f); // 실제 폭발 범위 적
            WeaponTarget far = Target(secondOrigin + Vector3.right * 3f, 100f); // 실제 폭발 범위 밖 적
            effects.ApplyHit(gradeFive, victim, victim.transform.position); // 실제 오 등급 화상 적용
            Require(victim.GetComponent<WeaponTargetBurnStatus>() != null, "Actual fireball did not apply burn."); // 실제 화상 확인
            effects.ApplyHit(gradeFive, victim, victim.transform.position); // 같은 적의 실제 화상 재적중
            victim.TakeDamage(1000f); // 실제 화상 대상 처치
            Require(Mathf.Approximately(near.CurrentHealth, 95f) && Mathf.Approximately(far.CurrentHealth, 100f), "Burned enemy death did not create one bounded fireball explosion."); // 실제 폭발 피해와 범위 확인
            Vector3 directOrigin = new Vector3(400f, 400f, 0f); // 화상 없는 즉사 시험 위치
            WeaponTarget direct = Target(directOrigin, 5f); // 화상 없는 즉사 대상
            WeaponTarget directNeighbor = Target(directOrigin + Vector3.right, 100f); // 즉사 대상 주변 적
            effects.ApplyHit(gradeFive, direct, direct.transform.position); // 실제 화상 전 즉사 공격
            Require(Mathf.Approximately(directNeighbor.CurrentHealth, 100f), "Unburned one-hit kill caused a burn explosion."); // 기존 화상 없는 폭발 방지
            Vector3 chainOrigin = new Vector3(500f, 500f, 0f); // 연쇄 폭발 시험 위치
            WeaponTarget chainA = Target(chainOrigin, 100f); // 첫 화상 대상
            WeaponTarget chainB = Target(chainOrigin + Vector3.right * 1.5f, 10.5f); // 폭발로 사망할 화상 대상
            WeaponTarget chainOutside = Target(chainOrigin + Vector3.right * 3f, 100f); // 첫 폭발 밖 실제 적
            effects.ApplyHit(gradeFive, chainA, chainA.transform.position); // 첫 적의 실제 화상
            effects.ApplyHit(gradeFive, chainB, chainB.transform.position); // 둘째 적의 실제 화상
            chainA.TakeDamage(1000f); // 실제 첫 대상 처치
            Require(!chainB.IsAlive && Mathf.Approximately(chainOutside.CurrentHealth, 100f), "Explosion damage recursively triggered another explosion."); // 중첩 연쇄 폭발 방지 확인
            WeaponData shortFire = UnityEngine.Object.Instantiate(fire); // 실제 화염탄 데이터 사본
            SerializedObject shortEditor = new SerializedObject(shortFire); // 시험용 화염 설정
            shortEditor.FindProperty("burnDuration").floatValue = 0.12f; // 짧은 시험용 화상
            shortEditor.FindProperty("burnDamagePerSecond").floatValue = 1f; // 낮은 시험용 화상 피해
            shortEditor.ApplyModifiedPropertiesWithoutUndo(); // 실제 사본 설정 적용
            Vector3 expiredOrigin = new Vector3(600f, 600f, 0f); // 화상 만료 시험 위치
            WeaponAttributeAttackSnapshot shortAttack = effects.CreateAttackSnapshot(shortFire, 5, expiredOrigin, 1f); // 실제 짧은 화상 공격
            WeaponTarget expired = Target(expiredOrigin, 100f); // 실제 화상 만료 대상
            WeaponTarget expiredNeighbor = Target(expiredOrigin + Vector3.right, 100f); // 실제 만료 대상 주변 적
            effects.ApplyHit(shortAttack, expired, expired.transform.position); // 실제 짧은 화상 적용
            yield return new WaitForSeconds(0.25f); // 실제 화상 만료 대기
            expired.TakeDamage(1000f); // 실제 화상 만료 후 처치
            Require(Mathf.Approximately(expiredNeighbor.CurrentHealth, 100f), "Expired burn still caused an explosion."); // 만료 화상 폭발 방지
            shortEditor.FindProperty("burnDamagePerSecond").floatValue = 20f; // 지속 피해 처치 시험값
            shortEditor.ApplyModifiedPropertiesWithoutUndo(); // 실제 사본 설정 적용
            Vector3 burnKillOrigin = new Vector3(650f, 650f, 0f); // 실제 화상 처치 시험 위치
            WeaponTarget burnKill = Target(burnKillOrigin, 2f); // 화상으로 사망할 실제 적
            WeaponTarget burnNeighbor = Target(burnKillOrigin + Vector3.right, 100f); // 실제 화상 폭발 인접 적
            WeaponAttributeAttackSnapshot burnAttack = effects.CreateAttackSnapshot(shortFire, 5, burnKillOrigin, 1f); // 실제 화상 처치 공격 정보
            effects.ApplyHit(burnAttack, burnKill, burnKill.transform.position); // 실제 화염탄 화상 적용
            yield return new WaitForSeconds(0.25f); // 실제 화상 피해 대기
            Require((burnKill == null || !burnKill.IsAlive) && Mathf.Approximately(burnNeighbor.CurrentHealth, 99.5f), "Lethal burn tick did not trigger the configured explosion exactly once."); // 실제 지속 피해 처치 폭발 확인
            Destroy(shortFire); // 시험용 데이터 사본 제거
            Vector3 projectileOrigin = new Vector3(700f, 700f, 0f); // 실제 탄 발사 시험 위치
            weapons.Slots[2].Origin.position = projectileOrigin; // 실제 장착 몸통 위치
            WeaponTarget projectileVictim = Target(projectileOrigin + Vector3.right * 4f, 100f); // 실제 탄 명중 대상
            CircleCollider2D projectileCollider = projectileVictim.gameObject.AddComponent<CircleCollider2D>(); // 실제 표적 충돌체
            projectileCollider.isTrigger = true; // 실제 탄 명중 감지
            Rigidbody2D projectileBody = projectileVictim.gameObject.AddComponent<Rigidbody2D>(); // 실제 표적 물리체
            projectileBody.bodyType = RigidbodyType2D.Kinematic; // 실제 고정 표적
            projectileBody.useFullKinematicContacts = true; // 운동학 충돌 허용
            WeaponTarget projectileNeighbor = Target(projectileVictim.transform.position + Vector3.up * 1.5f, 100f); // 실제 폭발 범위 표적
            Require(weapons.TryEquipAt(2, fire, 5), "Actual grade-five fireball equip failed."); // 실제 화염탄 장착
            Physics2D.SyncTransforms(); // 실제 발사 위치 반영
            weapons.enabled = true; // 실제 자동 발사 시작
            yield return new WaitForSeconds(0.1f); // 첫 발사 대기
            weapons.enabled = false; // 추가 발사 차단
            float deadline = Time.time + 2f; // 실제 탄 명중 제한
            while (projectileVictim != null && projectileVictim.GetComponent<WeaponTargetBurnStatus>() == null && Time.time < deadline) // 실제 화상 대기
            {
                yield return null; // 실제 투사체 비행
            }
            Require(projectileVictim != null && projectileVictim.GetComponent<WeaponTargetBurnStatus>() != null, "Actual fired projectile did not apply grade-five burn."); // 실제 탄 명중과 화상 확인
            float neighborBefore = projectileNeighbor.CurrentHealth; // 폭발 전 실제 주변 체력
            projectileVictim.TakeDamage(1000f); // 실제 화상 표적 처치
            Require(Mathf.Abs(projectileNeighbor.CurrentHealth - (neighborBefore - 7.7f)) < 0.1f, "Actual fired grade-five projectile did not arm its burn explosion."); // 실제 발사 경로의 폭발 확인
            yield return null; // 실제 파괴 프레임 대기
            AutomatedDay47UniqueEffectsVerification.Finish(0, "Steel periodic strike and fireball burn-death explosion passed under controlled conditions."); // 실제 전체 성공
        }
        private static WeaponTarget Target(Vector3 position, float hp) // 실제 표적 생성
        {
            GameObject item = new GameObject("Day47Target"); // 실제 표적 객체
            item.transform.position = position; // 실제 표적 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 피해 대상
            target.Configure(hp); // 실제 표적 체력
            return target; // 실제 표적 반환
        }
        private static void Require(bool condition, string message) // 실제 시험 조건
        {
            if (!condition) // 실제 실패 확인
            {
                AutomatedDay47UniqueEffectsVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 시험 중단
            }
        }
    }
}
