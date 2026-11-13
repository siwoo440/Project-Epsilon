using System; // 예외 처리
using ProjectEpsilon.Combat; // 실제 공격 대상
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 격리 씬 생성
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 편집기 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 갱신 연결
    public static class AutomatedTargetMutationVerification // 범위 공격 목록 변경 검증
    { // 시험 시작
        private const string Key = "EpsilonTargetMutation"; // 시험 진행 표식
        private static bool injected; // 실행기 중복 방지
        private static double started; // 시험 시작 시각
        static AutomatedTargetMutationVerification() // 재시작 시험 복원
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 진행 중 시험 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실행 감시 연결
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // 저장하지 않는 빈 시험 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실행 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        } // 함수 종료
        private static void Tick() // 실제 플레이 진입 확인
        { // 감시 시작
            if (!EditorApplication.isPlaying) return; // 플레이 진입 대기
            if (started <= 0d) started = EditorApplication.timeSinceStartup; // 제한 시간 시작
            if (EditorApplication.timeSinceStartup - started > 60d) // 제한 시간 확인
            { // 조건 시작
                Finish(false, "Timed out."); // 시험 시간 초과
                return; // 추가 실행 중단
            } // 조건 종료
            if (injected) return; // 중복 실행 방지
            injected = true; // 실행 상태 기록
            new GameObject("TargetMutationTest").AddComponent<TargetMutationVerificationRunner>(); // 실제 시험 실행기
        } // 감시 종료
        public static void Finish(bool success, string message) // 실제 시험 결과
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 실행 감시 해제
            Debug.Log("[TargetMutation] " + (success ? "PASS: " : "FAIL: ") + message); // 결과 기록
            EditorApplication.Exit(success ? 0 : 1); // 결과 코드 종료
        } // 함수 종료
    } // 시험 종료
    public sealed class TargetMutationVerificationRunner : MonoBehaviour // 범위 대상 실제 시험
    { // 실행기 시작
        private void Start() // 실제 시험 실행
        { // 함수 시작
            try // 실제 오류 감지
            { // 예외 처리 시작
                VerifyVisitCascade(); // 방문 중 다수 제거 시험
                VerifyDamageCascade(); // 일괄 피해 중 다수 제거 시험
                VerifyNestedVisit(); // 중첩 범위 공격 시험
                VerifySpawnDuringVisit(); // 방문 중 새 대상 생성 시험
                VerifyExceptionRecovery(); // 방문 실패 후 목록 복구 시험
                AutomatedTargetMutationVerification.Finish(true, "Cascade removal, damage, nested visits, spawn isolation and exception recovery passed."); // 실제 전체 성공
            } // 예외 처리 종료
            catch (Exception error) // 실제 오류 확인
            { // 예외 처리 시작
                AutomatedTargetMutationVerification.Finish(false, error.ToString()); // 실제 실패 기록
            } // 예외 처리 종료
        } // 함수 종료
        private static WeaponTarget MakeTarget(string name) // 실제 공격 대상 생성
        { // 함수 시작
            GameObject item = new GameObject(name); // 실제 대상 객체
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상 구성
            target.Configure(1f); // 단일 명중 처치 체력
            return target; // 생성된 대상 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 실제 결과 확인
        { // 함수 시작
            if (!condition) throw new InvalidOperationException(message); // 실패 원인 기록
        } // 함수 종료
        private static void Deactivate(params WeaponTarget[] targets) // 시험 대상 목록 정리
        { // 함수 시작
            foreach (WeaponTarget target in targets) // 실제 시험 대상 순회
            { // 반복 시작
                if (target != null) target.gameObject.SetActive(false); // 실제 대상 목록에서 제거
            } // 반복 종료
        } // 함수 종료
        private static void VerifyVisitCascade() // 방문 중 연쇄 제거 확인
        { // 함수 시작
            WeaponTarget first = MakeTarget("visit_first"); // 첫 대상
            WeaponTarget second = MakeTarget("visit_second"); // 두 번째 대상
            WeaponTarget third = MakeTarget("visit_third"); // 마지막 대상
            third.Died += ignored => Deactivate(first, second); // 마지막 처치 시 앞선 대상 동시 제거
            int visits = WeaponTarget.VisitAllInRange(Vector3.zero, 5f, target => target.TakeDamage(10f)); // 실제 범위 공격 경로
            Require(visits == 1 && !first.isActiveAndEnabled && !second.isActiveAndEnabled && !third.IsAlive, "Cascade visit result mismatch."); // 제거된 대상과 실제 방문 수 확인
            Deactivate(first, second, third); // 시험 대상 정리
        } // 함수 종료
        private static void VerifyDamageCascade() // 일괄 피해 중 연쇄 제거 확인
        { // 함수 시작
            WeaponTarget first = MakeTarget("damage_first"); // 첫 대상
            WeaponTarget second = MakeTarget("damage_second"); // 두 번째 대상
            WeaponTarget third = MakeTarget("damage_third"); // 마지막 대상
            third.Died += ignored => Deactivate(first, second); // 다른 대상 동시 제거
            int hits = WeaponTarget.DamageAllInRange(Vector3.zero, 5f, 10f); // 실제 일괄 피해 경로
            Require(hits == 1 && !third.IsAlive, "Cascade damage result mismatch."); // 실제 명중 수 확인
            Deactivate(first, second, third); // 시험 대상 정리
        } // 함수 종료
        private static void VerifyNestedVisit() // 중첩 범위 피해 확인
        { // 함수 시작
            WeaponTarget first = MakeTarget("nested_first"); // 첫 대상
            WeaponTarget second = MakeTarget("nested_second"); // 두 번째 대상
            WeaponTarget third = MakeTarget("nested_third"); // 마지막 대상
            first.Died += ignored => first.gameObject.SetActive(false); // 첫 대상 즉시 제거
            second.Died += ignored => second.gameObject.SetActive(false); // 두 번째 대상 즉시 제거
            int nested = 0; // 중첩 방문 수
            third.Died += ignored => nested = WeaponTarget.VisitAllInRange(Vector3.zero, 5f, target => target.TakeReactionDamage(10f)); // 실제 중첩 반응 피해
            int outer = WeaponTarget.VisitAllInRange(Vector3.zero, 5f, target => target.TakeDamage(10f)); // 실제 바깥 범위 피해
            Require(outer == 1 && nested == 2 && !first.IsAlive && !second.IsAlive && !third.IsAlive, "Nested range attacks were not isolated."); // 중첩 방문과 중복 피해 확인
            Deactivate(first, second, third); // 시험 대상 정리
        } // 함수 종료
        private static void VerifySpawnDuringVisit() // 방문 중 신규 대상 제외 확인
        { // 함수 시작
            WeaponTarget first = MakeTarget("spawn_first"); // 첫 대상
            WeaponTarget second = MakeTarget("spawn_second"); // 두 번째 대상
            WeaponTarget created = null; // 새로 생성될 대상
            int visits = WeaponTarget.VisitAllInRange(Vector3.zero, 5f, target => // 실제 범위 대상 방문
            { // 콜백 시작
                if (created == null) // 신규 대상 부재 확인
                { // 조건 시작
                    created = MakeTarget("spawn_new"); // 방문 중 새 대상 생성
                } // 조건 종료
            }); // 콜백 종료
            Require(visits == 2 && created != null && created.IsAlive, "New target was included in an ongoing attack."); // 기존 대상만 방문 확인
            Deactivate(first, second, created); // 시험 대상 정리
        } // 함수 종료
        private static void VerifyExceptionRecovery() // 방문 실패 후 다음 공격 확인
        { // 함수 시작
            WeaponTarget first = MakeTarget("exception_first"); // 첫 대상
            WeaponTarget second = MakeTarget("exception_second"); // 두 번째 대상
            bool caught = false; // 시험 예외 관찰 상태
            try // 실제 콜백 예외 재현
            { // 예외 처리 시작
                WeaponTarget.VisitAllInRange(Vector3.zero, 5f, target => throw new InvalidOperationException("controlled")); // 실제 방문 콜백 예외
            } // 예외 처리 종료
            catch (InvalidOperationException) // 통제된 예외 확인
            { // 예외 처리 시작
                caught = true; // 예상 예외 기록
            } // 예외 처리 종료
            int visits = WeaponTarget.VisitAllInRange(Vector3.zero, 5f, IgnoreTarget); // 예외 직후 새 방문
            Require(caught && visits == 2, "Visitor exception contaminated a later attack."); // 이전 대상 목록과 중복 방지 확인
            Deactivate(first, second); // 시험 대상 정리
        } // 함수 종료
        private static void IgnoreTarget(WeaponTarget target) // 대상 방문만 수행
        { // 함수 시작
        } // 함수 종료
    } // 실행기 종료
} // 영역 종료
