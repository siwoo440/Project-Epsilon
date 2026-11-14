using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 실제 물리 접촉 호출
using ProjectEpsilon.Combat; // 실제 투사체와 속성 효과
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 포탑과 속성
using ProjectEpsilon.Enemies; // 자연 적 생성기
using ProjectEpsilon.Player; // 실제 플레이어 이동
using UnityEditor; // 배치 실행과 포탑 데이터
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay105VictoryProjectileVerification // 승리 후 투사체와 연쇄 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay105VictoryProjectile"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay105VictoryProjectileVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void RunProjectile() // 이미 발사된 두 탄 시험
        { // 함수 시작
            Begin(0); // 실제 접촉 시험 시작
        } // 함수 종료
        public static void RunTransfer() // 삼 등급 전달 시험
        { // 함수 시작
            Begin(1); // 실제 전달 시험 시작
        } // 함수 종료
        public static void RunChain() // 보스 직접 명중의 전기 연쇄 시험
        { // 함수 시작
            Begin(2); // 실제 전기 연쇄 시험 시작
        } // 함수 종료
        public static void RunChainSecondary() // 보스 연쇄 명중의 후속 연쇄 시험
        { // 함수 시작
            Begin(3); // 실제 후속 전기 연쇄 시험 시작
        } // 함수 종료
        public static void RunExplosion() // 폭발의 후속 대상 시험
        { // 함수 시작
            Begin(4); // 실제 폭발 시험 시작
        } // 함수 종료
        public static void RunExplosionBefore() // 보스보다 먼저 처리되는 폭발 대상 시험
        { // 함수 시작
            Begin(5); // 실제 승리 전 폭발 피해 시험 시작
        } // 함수 종료
        public static void RunPause() // 일시정지 중 투사체 접촉 시험
        { // 함수 시작
            Begin(6); // 실제 정지와 재개 시험 시작
        } // 함수 종료
        private static void Begin(int mode) // 실제 게임 씬 시험 준비
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetInt("EpsilonDay105Mode", mode); // 실제 시험 종류 저장
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
            new GameObject("Day105VictoryProjectileRunner").AddComponent<Day105VictoryProjectileRunner>(); // 실제 공격 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day105VictoryProjectileVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day105VictoryProjectileRunner : MonoBehaviour // 실제 승리 경계 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 보스 승리와 후속 명중 확인
        { // 함수 시작
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            WeaponAttributeCombatEffects effects = FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 속성 전투 처리기
            Require(player != null && weapons != null && spawner != null && effects != null, "실제 플레이어·전투 구성 누락"); // 실제 시험 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 자동 공격 간섭 방지
            foreach (WeaponTarget target in FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 실제 적 순회
            { // 반복 시작
                target.gameObject.SetActive(false); // 기존 적의 연쇄 간섭 제거
            } // 반복 종료
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugNeedle.asset"); // 실제 전달 가능한 포탑 데이터
            Require(weapon != null && weapon.GradeThreeTransferRange > 1f, "실제 전달 포탑 데이터 누락"); // 실제 시험 데이터 확인
            int mode = SessionState.GetInt("EpsilonDay105Mode", 0); // 실제 시험 종류
            Vector3 origin = new Vector3(500f, 500f, 0f); // 다른 전투와 분리된 위치
            WeaponTarget first = mode == 3 ? CreateTarget("Day105First", origin, 100f) : null; // 연쇄 중 보스 시험의 첫 적
            WeaponTarget other = mode == 4 ? CreateTarget("Day105Other", origin + Vector3.right, 100f) : null; // 실제 역순 폭발에서 보스 다음 대상
            WeaponTarget boss = CreateTarget("Day105Boss", origin + Vector3.right * (mode == 3 ? 1f : 0f), 1f); // 실제 최종 보스
            if (other == null) // 폭발 외 시험 대상 확인
            { // 조건 시작
                other = CreateTarget("Day105Other", origin + Vector3.right * (mode == 3 ? 2f : 1f), 100f); // 실제 후속 피해 확인 대상
            } // 조건 종료
            if (mode == 6) // 실제 일시정지 접촉 시험 확인
            { // 조건 시작
                StraightProjectile pausedShot = CreateShot(null, 1, origin); // 실제 정지 시험 투사체
                GameManager.Instance.PauseGame(); // 실제 게임 일시정지
                Contact(pausedShot, other.GetComponent<Collider2D>()); // 정지 중 실제 접촉 콜백
                Require(pausedShot != null && pausedShot.isActiveAndEnabled && Mathf.Approximately(other.CurrentHealth, 100f), "일시정지 중 투사체 피해 또는 탄 소비"); // 실제 정지 상태 유지 확인
                GameManager.Instance.ResumeGame(); // 실제 게임 재개
                Contact(pausedShot, other.GetComponent<Collider2D>()); // 재개 후 실제 접촉 콜백
                Require(Mathf.Approximately(other.CurrentHealth, 90f), "일시정지 후 재개된 투사체 명중 실패"); // 실제 재개 명중 확인
                yield return null; // 실제 지연 파괴 완료 대기
                AutomatedDay105VictoryProjectileVerification.Finish(true, "Paused projectile remained available and hit after resume."); // 실제 시험 결과
                yield break; // 승리 경계 시험 생략
            } // 조건 종료
            Require(GameManager.Instance.RegisterFinalBoss(boss), "실제 최종 보스 등록 실패"); // 실제 승리 조건 연결
            if (mode == 0) // 실제 두 투사체 접촉 시험
            { // 조건 시작
                StraightProjectile firstShot = CreateShot(null, 1, origin); // 실제 보스 명중 탄
                StraightProjectile secondShot = CreateShot(null, 1, origin); // 이미 발사된 실제 두 번째 탄
                Contact(firstShot, boss.GetComponent<Collider2D>()); // 실제 보스 접촉 콜백
                Contact(secondShot, other.GetComponent<Collider2D>()); // 승리 직후 실제 두 번째 접촉 콜백
            } // 조건 종료
            else if (mode == 1) // 실제 삼 등급 전달 시험
            { // 조건 시작
                StraightProjectile shot = CreateShot(weapon, 3, origin); // 실제 삼 등급 포탑 탄
                Contact(shot, boss.GetComponent<Collider2D>()); // 실제 보스 접촉과 전달 처리
            } // 조건 종료
            else if (mode == 2 || mode == 3) // 실제 전기 연쇄 시험
            { // 조건 시작
                WeaponTarget initial = mode == 3 ? first : boss; // 실제 최초 명중 대상
                WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, WeaponAttribute.Electric, 4, 4, 1, origin, 10f); // 실제 두 대상 전기 연쇄 공격
                effects.ApplyHit(attack, initial, initial.transform.position); // 실제 전기 명중과 연쇄
            } // 조건 종료
            else // 실제 폭발의 후속 대상 시험
            { // 대체 시작
                WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, WeaponAttribute.Explosion, 0, 0, 1, origin, 10f); // 실제 폭발 공격 정보
                effects.ApplyHit(attack, boss, boss.transform.position); // 실제 범위 폭발 명중
            } // 대체 종료
            Require(GameManager.Instance.CurrentState == GameState.Victory && !boss.IsAlive, "실제 보스 처치와 승리 실패"); // 실제 승리 확정 확인
            Require(mode == 5 ? other.CurrentHealth < 100f : Mathf.Approximately(other.CurrentHealth, 100f), "승리 전 폭발 피해 또는 승리 후 추가 피해 판정 오류"); // 실제 처리 순서의 피해 확인
            yield return null; // 실제 지연 파괴 완료 대기
            AutomatedDay105VictoryProjectileVerification.Finish(true, "Actual victory stopped later projectile, transfer or area-chain damage."); // 실제 시험 결과
        } // 함수 종료
        private static WeaponTarget CreateTarget(string name, Vector3 position, float health) // 실제 공격 대상 생성
        { // 함수 시작
            GameObject item = new GameObject(name); // 실제 격리 대상
            item.transform.position = position; // 실제 대상 위치
            item.AddComponent<CircleCollider2D>().isTrigger = true; // 실제 투사체 접촉 충돌체
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상 구성
            target.Configure(health); // 실제 대상 체력
            return target; // 실제 공격 대상 반환
        } // 함수 종료
        private static StraightProjectile CreateShot(WeaponData weapon, int grade, Vector3 origin) // 실제 포탑 탄 생성
        { // 함수 시작
            GameObject item = new GameObject("Day105Projectile"); // 실제 투사체 객체
            item.transform.position = origin; // 실제 발사 위치
            StraightProjectile shot = item.AddComponent<StraightProjectile>(); // 실제 포탑 탄
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, WeaponAttribute.Physical, 0, 0, grade, origin, 10f); // 실제 공격 정보
            shot.Configure(Vector2.right, attack, null, 1f, 3f, null); // 실제 투사체 설정
            return shot; // 실제 투사체 반환
        } // 함수 종료
        private static void Contact(StraightProjectile shot, Collider2D other) // 실제 투사체 접촉 순서 통제
        { // 함수 시작
            typeof(StraightProjectile).GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(shot, new object[] { other }); // 실제 물리 접촉 처리 호출
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay105VictoryProjectileVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
