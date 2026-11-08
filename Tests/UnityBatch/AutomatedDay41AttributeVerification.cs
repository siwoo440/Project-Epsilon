using System; // 예외 형식
using System.Collections; // 코루틴 형식
using ProjectEpsilon.Combat; // 실제 속성 효과
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedDay41AttributeVerification // 실제 신성과 암흑 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay41AttributeVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedDay41AttributeVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 진행 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 플레이 감시 연결
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 씬 시험 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 씬 열기
            SessionState.SetBool(Key, true); // 시험 진행 기록
            EditorApplication.update -= Tick; // 중복 감시 제거
            EditorApplication.update += Tick; // 플레이 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        } // 메서드 끝
        private static void Tick() // 실제 초기화 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 플레이 대기
            } // 조건 끝
            if (started <= 0d) // 시험 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 시작 시각 저장
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 90d) // 시험 제한 시간 확인
            { // 조건 시작
                Finish(1, "Timed out."); // 시간 초과 실패
                return; // 시험 중단
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 시작 선택과 실행 여부 확인
            { // 조건 시작
                return; // 시작 또는 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 실행 중복 방지
            player.gameObject.AddComponent<Day41AttributeRunner>(); // 실제 시험 실행
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 시험 종료
        { // 메서드 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[Day41AttributeVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        } // 메서드 끝
    } // 클래스 끝
    public sealed class Day41AttributeRunner : MonoBehaviour // 실제 신성 및 암흑 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 속성 명중 시험
        { // 코루틴 시작
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
            { // 반복 시작
                effects.ApplyHit(holyAttack, holyTarget, holyTarget.transform.position); // 실제 신성 명중
            } // 반복 끝
            Require(shield.CurrentShield == 0, "Holy shield activated before five hits."); // 보호막 조기 발동 방지 확인
            effects.ApplyHit(holyAttack, holyTarget, holyTarget.transform.position); // 다섯 번째 실제 신성 명중
            Require(shield.CurrentShield == WeaponAttributeHolyRules.ShieldAmount, "Holy shield did not activate after five hits."); // 실제 보호막 발동 확인
            WeaponTarget darkTarget = CreateTarget("DarkTarget", new Vector3(200f, 100f, 0f)); // 실제 암흑 표적
            WeaponAttributeAttackSnapshot darkAttack = new WeaponAttributeAttackSnapshot(dark, WeaponAttribute.Dark, 2, 2, 1, darkTarget.transform.position, 1f); // 암흑 이 단계 공격 정보
            for (int index = 0; index < 8; index++) // 최대 저주 초과 명중 순회
            { // 반복 시작
                effects.ApplyHit(darkAttack, darkTarget, darkTarget.transform.position); // 실제 암흑 명중
            } // 반복 끝
            WeaponTargetDarkCurseStatus curse = darkTarget.GetComponent<WeaponTargetDarkCurseStatus>(); // 실제 저주 상태 조회
            Require(curse != null && curse.Stacks == 5 && curse.IsActive, "Dark curse did not cap at five stacks."); // 실제 저주 상한 확인
            float before = darkTarget.CurrentHealth; // 강화 명중 전 체력
            WeaponAttributeAttackSnapshot boosted = new WeaponAttributeAttackSnapshot(dark, WeaponAttribute.Dark, 4, 4, 1, darkTarget.transform.position, 10f); // 암흑 사 단계 공격 정보
            effects.ApplyHit(boosted, darkTarget, darkTarget.transform.position); // 실제 저주 강화 명중
            Require(Mathf.Abs(before - darkTarget.CurrentHealth - 13f) < 0.001f, "Dark curse damage amplification mismatch."); // 실제 다섯 중첩 피해 증가 확인
            yield return null; // 실제 프레임 대기
            AutomatedDay41AttributeVerification.Finish(0, "Actual holy shield threshold and dark curse cap and damage."); // 전체 성공 기록
        } // 코루틴 끝
        private static WeaponTarget CreateTarget(string name, Vector3 position) // 실제 공격 대상 생성
        { // 메서드 시작
            GameObject item = new GameObject(name); // 통제 표적 객체
            item.transform.position = position; // 다른 적과 떨어진 표적 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(1000f); // 실제 표적 체력
            return target; // 실제 표적 반환
        } // 메서드 끝
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedDay41AttributeVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
