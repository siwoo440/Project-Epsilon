using System; // 기본 형식
using System.Collections; // 코루틴 형식
using System.Collections.Generic; // 시험 기록 목록
using System.Reflection; // 임시 시험 설정 조회
using ProjectEpsilon.Combat; // 무기와 공격 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Enemies; // 자연 적 출현 제어
using ProjectEpsilon.Player; // 몸통과 이동
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedFireballGradeVerification // 화염탄 등급별 실제 공격 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonFireballGradeVerification"; // 배치 진행 표식
        private static double started; // 플레이 시작 시각
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지

        static AutomatedFireballGradeVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 배치 진행 확인
            { // 조건 시작
                EditorApplication.update -= Tick; // 중복 연결 제거
                EditorApplication.update += Tick; // 플레이 시작 감시
            } // 조건 끝
        } // 생성자 끝

        public static void Run() // Unity 배치 진입점
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 배치 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 플레이 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        } // 메서드 끝

        private static void Tick() // 시작 선택과 플레이 진입 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 진입 대기
            } // 조건 끝
            if (started <= 0d) // 최초 갱신 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 120d) // 전체 시험 제한
            { // 조건 시작
                Finish(1, "Timed out waiting for controlled fireball test."); // 제한 시간 실패
                return; // 감시 중단
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 실제 시작 무기 첫 후보 선택
            { // 조건 시작
                return; // 실제 게임 시작 대기
            } // 조건 끝
            if (injected) // 시험 실행기 존재 확인
            { // 조건 시작
                return; // 시험 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 씬 초기화 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 실행기 중복 생성 방지
            player.gameObject.AddComponent<FireballGradeVerificationRunner>(); // 실제 씬 시험 실행
        } // 메서드 끝

        public static void Finish(int code, string message) // 결과 기록과 프로세스 종료
        { // 메서드 시작
            if (finished) // 중복 종료 확인
            { // 조건 시작
                return; // 중복 처리 방지
            } // 조건 끝
            finished = true; // 종료 상태 기록
            SessionState.SetBool(ActiveKey, false); // 진행 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log($"[FireballVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 배치 결과 기록
            EditorApplication.Exit(code); // 종료 코드 반환
        } // 메서드 끝
    } // 클래스 끝

    public sealed class FireballGradeVerificationRunner : MonoBehaviour // 실제 투사체 전달 시험 실행기
    { // 클래스 시작
        private sealed class HitRecord // 실제 속성 명중 기록
        { // 클래스 시작
            public int TargetId; // 실제 대상 고유 번호
            public float Damage; // 실제 명중 직접 피해
            public int Stage; // 실제 속성 시너지 단계
            public float BurnDps; // 실제 적용 화상 피해
            public Vector3 Origin; // 실제 무기 몸통 원점
        } // 클래스 끝

        private readonly List<GameObject> objects = new List<GameObject>(); // 통제 표적 목록
        private readonly List<HitRecord> hits = new List<HitRecord>(); // 실제 명중 기록
        private readonly List<float> attacks = new List<float>(); // 실제 발사 시각
        private SnakeMovement movement; // 실제 플레이어 이동
        private SnakeBodyFollower follower; // 실제 몸통 추적
        private SnakeWeaponManager weapons; // 실제 무기 관리자
        private WeaponAttributeSynergyManager synergy; // 실제 시너지 관리자
        private WeaponAttributeEffectHooks hooks; // 실제 명중 및 공격 이벤트
        private WeaponData fireball; // 실제 시험용 화염탄 에셋
        private WeaponData spear; // 실제 다른 투사체 무기
        private WeaponData fireSupport; // 실제 화염 시너지 보조 무기
        private WeaponData currentWeapon; // 현재 시험 무기
        private Vector3 origin; // 실제 세 번째 몸통 위치

        private IEnumerator Start() // 실제 씬 시험 순서
        { // 코루틴 시작
            Prepare(); // 실제 씬과 통제 조건 준비
            float[] damage = { 7f, 8.75f, 10.5f, 12.6f, 15.4f }; // 기획 임시 기본 피해와 등급 배율
            for (int grade = 1; grade <= 5; grade++) // 실제 화염탄 전 등급 순회
            { // 반복 시작
                yield return VerifyGrade(grade, damage[grade - 1]); // 실제 발사와 전달 검증
            } // 반복 끝
            yield return VerifyInside(); // 전달 반경 안 경계 확인
            yield return VerifyOutside(); // 전달 반경 밖 대상 확인
            yield return VerifyOtherWeapon(); // 다른 무기 비적용 확인
            yield return VerifySynergy(); // 화상 및 시너지 상태 확인
            yield return VerifyDeath(0); // 최초 대상 사망 후 전달 확인
            yield return VerifyDeath(1); // 최초 사망 중 전달 대상 비활성 확인
            yield return VerifyDeath(2); // 최초 사망 중 전달 대상 사망 확인
            yield return VerifyPiercing(); // 임시 관통 투사체 중복 방지 확인
            yield return VerifyTransferSnapshot(true); // 발사 후 임시 설정 감소
            yield return VerifyTransferSnapshot(false); // 발사 후 임시 설정 증가
            yield return VerifyDisabledProjectile(); // 실제 비활성 투사체 물리 접촉 확인
            ClearObjects(); // 통제 표적 제거
            hooks.AttackTriggered -= OnAttack; // 발사 관측 해제
            hooks.HitTriggered -= OnHit; // 명중 관측 해제
            AutomatedFireballGradeVerification.Finish(0, "Actual fireball projectile grades, transfer selection, burn, synergy, death and controlled piercing were verified."); // 전체 성공 결과
        } // 코루틴 끝

        private void Prepare() // 실제 씬과 임시 무기 데이터 확인
        { // 메서드 시작
            movement = GetComponent<SnakeMovement>(); // 실제 플레이어 조회
            follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적 조회
            weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자 조회
            synergy = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeSynergyManager>(); // 실제 시너지 관리자 조회
            hooks = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeEffectHooks>(); // 실제 전투 명중 조회
            fireball = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlaster.asset"); // 실제 화염탄 임시 에셋
            spear = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugNeedle.asset"); // 실제 강철 창 임시 에셋
            fireSupport = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugPulse.asset"); // 실제 화염 시너지 보조 에셋
            Require(movement != null && follower != null && weapons != null && synergy != null && hooks != null && hooks.IsConfigured && fireball != null && spear != null && fireSupport != null && weapons.SlotCount == 3, "Actual Game scene or weapon assets are missing."); // 실제 씬 구성 확인
            Require(fireball.AttackType == WeaponAttackType.StraightProjectile && fireball.Attribute == WeaponAttribute.Fire && Mathf.Approximately(fireball.BaseDamage, 7f) && Mathf.Approximately(fireball.AttackInterval, 0.8f) && fireball.ProjectileHitLimit == 1, "Temporary fireball asset differs from the controlled fixture."); // 기획 무기 임시값 확인
            movement.enabled = false; // 통제 중 플레이어 이동 정지
            follower.enabled = false; // 실제 몸통 위치 고정
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            if (spawner != null) // 출현기 존재 확인
            { // 조건 시작
                spawner.enabled = false; // 자연 적 출현 정지
            } // 조건 끝
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 자기 충돌 조회
            if (self != null) // 자기 충돌 구성 확인
            { // 조건 시작
                self.enabled = false; // 시험 중 자기 충돌 방지
            } // 조건 끝
            foreach (WeaponTarget target in UnityEngine.Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 전투 대상 순회
            { // 반복 시작
                target.gameObject.SetActive(false); // 통제 외 대상 제외
            } // 반복 끝
            origin = movement.transform.position + Vector3.right * 20f; // 머리 사거리 밖 실제 몸통 위치
            weapons.Slots[2].Origin.position = origin; // 실제 세 번째 몸통 이동
            Physics2D.SyncTransforms(); // 실제 물리 위치 동기화
            hooks.AttackTriggered += OnAttack; // 실제 발사 알림 구독
            hooks.HitTriggered += OnHit; // 실제 명중 알림 구독
            Debug.Log("[FireballVerification] Actual Game scene, real starting weapon button; movement, following and natural spawning disabled; controlled stationary targets and real projectile triggers. Transfer range 2.0 and damage multiplier 1.0 are provisional assumptions."); // 시험 조건과 임시 가정 기록
        } // 메서드 끝

        private IEnumerator VerifyGrade(int grade, float damage) // 실제 화염탄 등급별 전달
        { // 코루틴 시작
            ConfigureWeapon(fireball, grade, false); // 화염탄 장착과 표적 초기화
            yield return null; // 이전 객체 파괴 대기
            WeaponTarget first = CreateTarget(origin + Vector3.right * 0.8f, 1000f, "First"); // 실제 최초 충돌 대상
            WeaponTarget nearest = CreateTarget(first.transform.position + Vector3.up * 0.7f, 1000f, "Nearest"); // 최초 대상 기준 최근접 전달 후보
            WeaponTarget other = CreateTarget(first.transform.position + Vector3.up * 1.3f, 1000f, "Other"); // 반경 안 더 먼 후보
            WeaponTarget outside = CreateTarget(first.transform.position + Vector3.down * 2.1f, 1000f, "Outside"); // 임시 전달 반경 밖 후보
            int firstId = first.GetInstanceID(); // 첫 대상 고유 번호
            int nearestId = nearest.GetInstanceID(); // 최근접 후보 고유 번호
            Physics2D.SyncTransforms(); // 실제 트리거 위치 동기화
            int expected = grade >= 3 ? 2 : 1; // 기획 등급별 실제 명중 수
            yield return WaitForVolley(expected); // 첫 투사체 실제 명중 대기
            Require(CountHit(firstId) == 1 && CountHit(nearestId) == (grade >= 3 ? 1 : 0) && hits.Count == expected, "Grade transfer did not hit the first and nearest eligible targets exactly once."); // 등급별 전달 대상 확인
            Require(Mathf.Approximately(other.CurrentHealth, 1000f) && Mathf.Approximately(outside.CurrentHealth, 1000f), "Transfer hit a farther or out-of-range target."); // 불필요한 대상 제외
            foreach (HitRecord hit in hits) // 실제 첫 발사 명중 순회
            { // 반복 시작
                Require(Mathf.Abs(hit.Damage - damage) < 0.01f && hit.Stage == 0 && Mathf.Approximately(hit.BurnDps, 1f) && Vector3.Distance(hit.Origin, origin) < 0.01f, "Actual projectile damage, base burn, synergy or origin mismatch."); // 피해와 화상 및 실제 몸통 원점
            } // 반복 끝
            Debug.Log($"[FireballVerification] grade={grade}, targets={hits.Count}, firstDamage={damage:F2}, transferDamage={(grade >= 3 ? damage : 0f):F2}, range=2.0 provisional, origin={origin}."); // 등급별 실제 명중 기록
        } // 코루틴 끝

        private IEnumerator VerifyInside() // 전달 반경 안 대상만 존재
        { // 코루틴 시작
            ConfigureWeapon(fireball, 3, false); // 삼 등급 화염탄 장착
            yield return null; // 이전 대상 제거 대기
            WeaponTarget first = CreateTarget(origin + Vector3.right * 0.8f, 1000f, "RangeInsideFirst"); // 실제 최초 대상
            WeaponTarget inside = CreateTarget(first.transform.position + Vector3.up * 1.99f, 1000f, "RangeInside"); // 임시 반경 바로 안쪽
            Physics2D.SyncTransforms(); // 실제 충돌체 동기화
            yield return WaitForVolley(2); // 실제 전달 명중 대기
            Require(hits.Count == 2 && CountHit(first.GetInstanceID()) == 1 && CountHit(inside.GetInstanceID()) == 1, "Fireball failed to transfer inside the provisional radius."); // 경계 안 대상 명중 확인
            Debug.Log("[FireballVerification] range 1.99 included for provisional radius 2.0."); // 반경 경계 기록
        } // 코루틴 끝

        private IEnumerator VerifyOutside() // 전달 범위 밖 대상만 존재
        { // 코루틴 시작
            ConfigureWeapon(fireball, 3, false); // 삼 등급 화염탄 장착
            yield return null; // 이전 대상 제거 대기
            WeaponTarget first = CreateTarget(origin + Vector3.right * 0.8f, 1000f, "RangeFirst"); // 실제 최초 대상
            WeaponTarget outside = CreateTarget(first.transform.position + Vector3.up * 2.01f, 1000f, "RangeOutside"); // 임시 반경 바로 바깥
            Physics2D.SyncTransforms(); // 실제 충돌체 동기화
            yield return WaitForVolley(1); // 최초 투사체 명중 대기
            Require(hits.Count == 1 && CountHit(first.GetInstanceID()) == 1 && Mathf.Approximately(outside.CurrentHealth, 1000f), "Fireball transferred outside the provisional radius."); // 경계 바깥 제외 확인
            Debug.Log("[FireballVerification] range 2.01 excluded for provisional radius 2.0."); // 반경 경계 기록
        } // 코루틴 끝

        private IEnumerator VerifyOtherWeapon() // 다른 투사체 무기의 전달 미적용
        { // 코루틴 시작
            ConfigureWeapon(spear, 3, false); // 실제 삼 등급 강철 창 장착
            yield return null; // 이전 대상 제거 대기
            WeaponTarget first = CreateTarget(origin + Vector3.right * 0.8f, 1000f, "SpearFirst"); // 실제 최초 충돌 대상
            WeaponTarget sideways = CreateTarget(first.transform.position + Vector3.up * 0.7f, 1000f, "SpearSide"); // 투사체 경로 밖 인접 대상
            Physics2D.SyncTransforms(); // 실제 물리 위치 동기화
            yield return WaitForVolley(1); // 실제 강철 창 첫 명중
            Require(CountHit(first.GetInstanceID()) == 1 && hits.Count == 1 && Mathf.Approximately(sideways.CurrentHealth, 1000f), "Another projectile unexpectedly transferred to a side target."); // 다른 무기 전달 미적용
            Debug.Log("[FireballVerification] actual grade-three spear did not transfer to a side target."); // 다른 무기 결과
        } // 코루틴 끝

        private IEnumerator VerifySynergy() // 실제 화염 단계와 두 대상 화상
        { // 코루틴 시작
            ConfigureWeapon(fireball, 3, true); // 화염 두 개로 실제 시너지 구성
            yield return null; // 이전 대상 제거 대기
            Require(synergy.GetCount(WeaponAttribute.Fire) == 2 && synergy.GetStage(WeaponAttribute.Fire) == 2, "Controlled fire synergy is not stage two."); // 실제 화염 속성 수와 단계 확인
            WeaponTarget first = CreateTarget(origin + Vector3.right * 0.8f, 1000f, "FireStageFirst"); // 실제 최초 명중 대상
            WeaponTarget second = CreateTarget(first.transform.position + Vector3.up * 0.7f, 1000f, "FireStageSecond"); // 실제 전달 대상
            Physics2D.SyncTransforms(); // 실제 충돌체 위치 동기화
            yield return WaitForVolley(2); // 실제 첫 발사 명중 대기
            Require(hits.Count == 2 && CountHit(first.GetInstanceID()) == 1 && CountHit(second.GetInstanceID()) == 1, "Fire synergy transfer did not hit two unique targets."); // 실제 두 대상 확인
            foreach (HitRecord hit in hits) // 실제 명중 결과 순회
            { // 반복 시작
                Require(hit.Stage == 2 && Mathf.Abs(hit.Damage - 10.5f) < 0.01f && Mathf.Abs(hit.BurnDps - 1.2f) < 0.01f, "Actual fire synergy or burn snapshot mismatch."); // 실제 등급 피해와 화상 단계 확인
            } // 반복 끝
            Debug.Log("[FireballVerification] actual fire synergy x2: two direct hits 10.50, both burns 1.20 DPS."); // 실제 화상 시너지 기록
        } // 코루틴 끝

        private IEnumerator VerifyDeath(int mutation) // 최초 사망 중 선택 대상 변경
        { // 코루틴 시작
            ConfigureWeapon(fireball, 3, false); // 실제 삼 등급 화염탄
            yield return null; // 이전 대상 제거 대기
            WeaponTarget first = CreateTarget(origin + Vector3.right * 0.8f, 1f, "DyingFirst"); // 첫 명중에 사망할 대상
            WeaponTarget second = CreateTarget(first.transform.position + Vector3.up * 0.7f, mutation == 2 ? 1f : 1000f, "DeathSecond"); // 최초 선택될 전달 대상
            WeaponTarget third = CreateTarget(first.transform.position + Vector3.up * 1.3f, 1000f, "DeathThird"); // 대체하지 않을 다른 후보
            int firstId = first.GetInstanceID(); // 사망 대상 고유 번호
            int secondId = second.GetInstanceID(); // 전달 대상 고유 번호
            int deaths = 0; // 실제 사망 이벤트 횟수
            first.Died += ignored => // 실제 사망 콜백 등록
            { // 콜백 시작
                deaths++; // 실제 사망 횟수 증가
                if (mutation == 1) // 전달 후보 비활성 시험 확인
                { // 조건 시작
                    second.gameObject.SetActive(false); // 선택된 전달 후보 비활성화
                } // 조건 끝
                if (mutation == 2) // 전달 후보 사망 시험 확인
                { // 조건 시작
                    second.TakeDamage(2f); // 실제 사망 및 파괴 예약
                } // 조건 끝
            }; // 콜백 끝
            Physics2D.SyncTransforms(); // 실제 물리 위치 동기화
            yield return WaitForVolley(mutation == 0 ? 2 : 1); // 실제 첫 명중과 전달 대기
            Require(deaths == 1 && CountHit(firstId) == 1 && CountHit(secondId) == (mutation == 0 ? 1 : 0) && hits.Count == (mutation == 0 ? 2 : 1) && Mathf.Approximately(third.CurrentHealth, 1000f), "First-target death or invalid selected target handling mismatch."); // 사망 및 선택 후보 수명 확인
            Debug.Log($"[FireballVerification] first target death callbacks={deaths}, second mutation={mutation}, transferred hits={CountHit(secondId)}."); // 실제 사망 콜백 결과
        } // 코루틴 끝

        private IEnumerator VerifyPiercing() // 시험용 관통 설정의 중복 명중 방지
        { // 코루틴 시작
            WeaponData clone = UnityEngine.Object.Instantiate(fireball); // 실제 에셋의 메모리 사본
            FieldInfo hitLimit = typeof(WeaponData).GetField("projectileHitLimit", BindingFlags.NonPublic | BindingFlags.Instance); // 실제 관통 설정 조회
            Require(hitLimit != null, "Projectile hit-limit fixture is unavailable."); // 설정 필드 확인
            hitLimit.SetValue(clone, 2); // 시험 전용 관통 횟수 변경
            ConfigureWeapon(clone, 3, false); // 실제 복제 화염탄 장착
            yield return null; // 이전 대상 제거 대기
            WeaponTarget first = CreateTarget(origin + Vector3.right * 0.8f, 1000f, "PierceFirst"); // 실제 첫 충돌 대상
            WeaponTarget second = CreateTarget(origin + Vector3.right * 1.55f, 1000f, "PierceSecond"); // 전달 후 경로에서 다시 만나는 대상
            WeaponTarget third = CreateTarget(origin + Vector3.right * 2.55f, 1000f, "PierceThird"); // 실제 남은 관통 충돌 대상
            Physics2D.SyncTransforms(); // 실제 물리 위치 동기화
            yield return WaitForVolley(3); // 전달 및 실제 후속 충돌 대기
            Require(hits.Count == 3 && CountHit(first.GetInstanceID()) == 1 && CountHit(second.GetInstanceID()) == 1 && CountHit(third.GetInstanceID()) == 1, "Controlled piercing damaged a transferred target twice or consumed a contact incorrectly."); // 전달과 물리 충돌 중복 방지
            UnityEngine.Object.Destroy(clone); // 시험용 에셋 사본 제거
            Debug.Log("[FireballVerification] memory-only projectile hit limit 2: first, transferred, later physical target each hit once."); // 시험 전용 관통 결과
        } // 코루틴 끝

        private IEnumerator VerifyDisabledProjectile() // 비활성 컴포넌트 실제 물리 접촉
        { // 코루틴 시작
            ConfigureWeapon(fireball, 3, false); // 실제 삼 등급 화염탄 장착
            yield return null; // 이전 대상 제거 대기
            WeaponTarget target = CreateTarget(origin + Vector3.right * 4f, 1000f, "DisabledProjectileTarget"); // 멀리 놓은 실제 물리 표적
            Physics2D.SyncTransforms(); // 실제 물리 위치 동기화
            float deadline = Time.time + 2f; // 실제 첫 발사 제한
            while (attacks.Count < 1 && Time.time < deadline) // 실제 자동 발사 대기
            { // 반복 시작
                yield return null; // 실제 게임 프레임 진행
            } // 반복 끝
            StraightProjectile projectile = UnityEngine.Object.FindFirstObjectByType<StraightProjectile>(); // 실제 발사된 투사체 조회
            Require(attacks.Count == 1 && projectile != null && projectile.enabled, "Actual projectile was not launched for the disabled-component test."); // 실제 발사 확인
            projectile.enabled = false; // 실제 투사체 컴포넌트만 비활성화
            target.transform.position = projectile.transform.position; // 비활성 투사체에 실제 표적 접촉
            Physics2D.SyncTransforms(); // 실제 물리 위치 동기화
            yield return new WaitForFixedUpdate(); // 실제 물리 스텝 진행
            yield return new WaitForFixedUpdate(); // 트리거 전달 처리 대기
            Require(hits.Count == 0 && Mathf.Approximately(target.CurrentHealth, 1000f), "Disabled projectile damaged a target through a real physics trigger."); // 비활성 트리거 피해 방지 확인
            target.transform.position = projectile.transform.position + Vector3.up * 3f; // 실제 접촉을 충분히 해제
            Physics2D.SyncTransforms(); // 실제 물리 위치 동기화
            yield return new WaitForFixedUpdate(); // 실제 접촉 해제 스텝
            yield return new WaitForFixedUpdate(); // 물리 종료 반영 대기
            projectile.enabled = true; // 같은 투사체 컴포넌트 재활성화
            target.transform.position = projectile.transform.position; // 재활성 투사체에 실제 표적 재접촉
            Physics2D.SyncTransforms(); // 실제 물리 위치 동기화
            yield return new WaitForFixedUpdate(); // 실제 물리 접촉 스텝
            yield return new WaitForFixedUpdate(); // 실제 트리거 명중 대기
            Require(hits.Count == 1 && CountHit(target.GetInstanceID()) == 1 && attacks.Count == 1, "Re-enabled projectile did not hit exactly once on real physics recontact."); // 재활성 실제 명중 확인
            Debug.Log("[FireballVerification] disabled projectile caused no damage on real trigger; re-enabled projectile hit once after real separation and recontact."); // 실제 물리 경계 기록
        } // 코루틴 끝

        private IEnumerator VerifyTransferSnapshot(bool initiallyEnabled) // 발사 시 임시 전달 설정 고정
        { // 코루틴 시작
            WeaponData clone = UnityEngine.Object.Instantiate(fireball); // 실제 에셋의 메모리 사본
            FieldInfo range = typeof(WeaponData).GetField("gradeThreeTransferRange", BindingFlags.NonPublic | BindingFlags.Instance); // 임시 전달 거리 필드
            FieldInfo multiplier = typeof(WeaponData).GetField("gradeThreeTransferDamageMultiplier", BindingFlags.NonPublic | BindingFlags.Instance); // 임시 전달 피해 배율 필드
            Require(range != null && multiplier != null, "Provisional transfer setting fields are unavailable."); // 데이터 필드 확인
            range.SetValue(clone, initiallyEnabled ? 2f : 0f); // 발사 전 임시 전달 거리
            multiplier.SetValue(clone, initiallyEnabled ? 1f : 0f); // 발사 전 임시 피해 배율
            ConfigureWeapon(clone, 3, false); // 실제 복제 화염탄 장착
            yield return null; // 이전 표적 제거 대기
            WeaponTarget first = CreateTarget(origin + Vector3.right * 3f, 1000f, "SnapshotFirst"); // 실제 발사 후 도달할 최초 대상
            WeaponTarget second = CreateTarget(first.transform.position + Vector3.up * 0.7f, 1000f, "SnapshotSecond"); // 발사 시 전달 대상
            Physics2D.SyncTransforms(); // 실제 표적 물리 위치 동기화
            float deadline = Time.time + 2f; // 발사 제한 시각
            while (attacks.Count < 1 && Time.time < deadline) // 실제 첫 발사 대기
            { // 반복 시작
                yield return null; // 실제 게임 프레임 진행
            } // 반복 끝
            Require(attacks.Count == 1 && hits.Count == 0, "Projectile hit before the controlled post-launch setting change."); // 발사와 명중 간 시점 확인
            range.SetValue(clone, initiallyEnabled ? 0f : 2f); // 실제 발사 후 임시 거리 변경
            multiplier.SetValue(clone, initiallyEnabled ? 0f : 1f); // 실제 발사 후 임시 배율 변경
            yield return WaitForVolley(initiallyEnabled ? 2 : 1); // 최초 물리 명중 대기
            Require(hits.Count == (initiallyEnabled ? 2 : 1) && CountHit(first.GetInstanceID()) == 1 && CountHit(second.GetInstanceID()) == (initiallyEnabled ? 1 : 0), "Actual projectile did not preserve transfer range from launch time."); // 발사 시 거리 고정 확인
            if (initiallyEnabled) // 발사 시 전달 활성 확인
            { // 조건 시작
                Require(Mathf.Abs(hits[1].Damage - 10.5f) < 0.01f, "Actual projectile did not preserve transfer damage multiplier from launch time."); // 발사 시 배율 고정 확인
            } // 조건 끝
            UnityEngine.Object.Destroy(clone); // 시험용 데이터 사본 제거
            Debug.Log($"[FireballVerification] launch transfer snapshot preserved, initially enabled={initiallyEnabled}."); // 실제 발사 시점 설정 기록
        } // 코루틴 끝

        private void ConfigureWeapon(WeaponData weapon, int grade, bool fireStage) // 실제 몸통 무기 구성
        { // 메서드 시작
            ClearObjects(); // 이전 통제 표적 비활성
            foreach (StraightProjectile projectile in UnityEngine.Object.FindObjectsByType<StraightProjectile>(FindObjectsSortMode.None)) // 이전 실제 투사체 순회
            { // 반복 시작
                projectile.gameObject.SetActive(false); // 이전 물리 접촉 차단
                UnityEngine.Object.Destroy(projectile.gameObject); // 이전 투사체 제거
            } // 반복 끝
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 모든 몸통 슬롯 순회
            { // 반복 시작
                slot.Clear(); // 이전 무기 제거
            } // 반복 끝
            if (fireStage) // 화염 시너지 시험 확인
            { // 조건 시작
                Require(weapons.TryEquipAt(0, fireSupport, 1), "Could not equip fire synergy support."); // 실제 화염 보조 무기 장착
                weapons.Slots[0].StartCooldown(Time.time + 100f); // 보조 무기의 실제 공격 보류
            } // 조건 끝
            Require(weapons.TryEquipAt(2, weapon, grade), "Could not equip controlled projectile weapon."); // 실제 세 번째 몸통 장착
            synergy.Recalculate(); // 직접 슬롯 변경 후 속성 집계
            currentWeapon = weapon; // 현재 시험 무기 기록
            attacks.Clear(); // 이전 발사 기록 제거
            hits.Clear(); // 이전 명중 기록 제거
        } // 메서드 끝

        private IEnumerator WaitForVolley(int expected) // 첫 실제 발사의 명중 대기
        { // 코루틴 시작
            float deadline = Time.time + 3f; // 실제 게임 시간 제한
            while (Time.time < deadline && attacks.Count < 2 && hits.Count < expected) // 실제 첫 투사체 명중 대기
            { // 반복 시작
                yield return null; // 실제 게임 프레임 진행
            } // 반복 끝
            Require(attacks.Count == 1 && hits.Count >= expected, "Expected first-volley projectile hits did not occur before a second attack."); // 두 번째 발사 전 실제 명중 확인
            yield return new WaitForSeconds(0.12f); // 같은 발사의 후속 접촉 대기
            Require(attacks.Count == 1, "Another attack began before first-volley verification."); // 첫 발사 구간 유지
        } // 코루틴 끝

        private WeaponTarget CreateTarget(Vector3 position, float health, string name) // 실제 물리 표적 생성
        { // 메서드 시작
            GameObject targetObject = new GameObject("FireballVerification_" + name); // 통제 표적 객체
            objects.Add(targetObject); // 정리 대상 기록
            targetObject.transform.position = position; // 실제 표적 위치
            CircleCollider2D collider = targetObject.AddComponent<CircleCollider2D>(); // 실제 트리거 충돌체
            collider.radius = 0.25f; // 실제 표적 충돌 반경
            collider.isTrigger = true; // 실제 투사체 트리거
            Rigidbody2D rigidbody = targetObject.AddComponent<Rigidbody2D>(); // 실제 표적 물리체
            rigidbody.bodyType = RigidbodyType2D.Kinematic; // 고정 표적 물리 방식
            rigidbody.useFullKinematicContacts = true; // 운동학 물리 접촉 허용
            WeaponTarget target = targetObject.AddComponent<WeaponTarget>(); // 실제 무기 표적
            target.Configure(health); // 실제 표적 체력
            return target; // 실제 표적 반환
        } // 메서드 끝

        private void OnAttack(WeaponAttributeEffectContext context) // 실제 투사체 발사 이벤트
        { // 메서드 시작
            if (context.Weapon == currentWeapon) // 현재 시험 무기 확인
            { // 조건 시작
                attacks.Add(Time.time); // 실제 발사 시각 기록
            } // 조건 끝
        } // 메서드 끝

        private void OnHit(WeaponAttributeHitContext context) // 실제 투사체 속성 명중 이벤트
        { // 메서드 시작
            if (context.Attack.Weapon != currentWeapon || context.Target == null) // 현재 시험 무기 확인
            { // 조건 시작
                return; // 다른 명중 무시
            } // 조건 끝
            WeaponTargetBurnStatus burn = context.Target.GetComponent<WeaponTargetBurnStatus>(); // 실제 적용된 화상 조회
            hits.Add(new HitRecord { TargetId = context.Target.GetInstanceID(), Damage = context.Attack.DirectDamage, Stage = context.Attack.SynergyStage, BurnDps = burn == null ? 0f : burn.DamagePerSecond, Origin = context.Attack.Origin }); // 실제 명중과 화상 상태 기록
        } // 메서드 끝

        private int CountHit(int targetId) // 특정 대상 실제 명중 횟수
        { // 메서드 시작
            int count = 0; // 일치 명중 수
            foreach (HitRecord hit in hits) // 실제 명중 기록 순회
            { // 반복 시작
                if (hit.TargetId == targetId) // 고유 대상 확인
                { // 조건 시작
                    count++; // 명중 수 증가
                } // 조건 끝
            } // 반복 끝
            return count; // 실제 명중 횟수 반환
        } // 메서드 끝

        private void ClearObjects() // 통제 표적 즉시 비활성 및 제거
        { // 메서드 시작
            foreach (GameObject item in objects) // 실제 시험 표적 순회
            { // 반복 시작
                if (item != null) // 아직 존재하는 대상 확인
                { // 조건 시작
                    item.SetActive(false); // 실제 표적 목록에서 제거
                    UnityEngine.Object.Destroy(item); // 지연 파괴 예약
                } // 조건 끝
            } // 반복 끝
            objects.Clear(); // 이전 표적 목록 초기화
        } // 메서드 끝

        private static void Require(bool condition, string message) // 실제 검증 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedFireballGradeVerification.Finish(1, message); // 실패 기록과 Unity 종료
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
