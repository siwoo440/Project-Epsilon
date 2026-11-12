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
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedOverlappingReactionVerification // 실제 신성과 암흑 시험
    {
        private const string Key = "EpsilonOverlappingReactionVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedOverlappingReactionVerification() // 도메인 재시작 처리
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
            if (EditorApplication.timeSinceStartup - started > 90d) // 시험 제한 시간 확인
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
            player.gameObject.AddComponent<OverlappingReactionRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[OverlappingReactionVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class OverlappingReactionRunner : MonoBehaviour // 실제 신성 및 암흑 시험 실행기
    {
        private IEnumerator Start() // 재료가 겹치는 실제 반응 시험
        {
            GetComponent<SnakeMovement>().enabled = false; // 플레이어 이동 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 중지
            WeaponAttributeCombatEffects effects = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 속성 처리기 조회
            Require(effects != null, "Actual attribute combat effects missing."); // 실제 처리기 확인
            WeaponTarget fireFirst = CreateTarget("OlderFireTarget", new Vector3(500f, 500f, 0f)); // 화염을 먼저 받을 대상
            Hit(effects, fireFirst, WeaponAttribute.Fire); // 화염 재료 최초 적용
            yield return null; // 최초 적용 프레임 구분
            Hit(effects, fireFirst, WeaponAttribute.Dark); // 암흑 재료 후속 적용
            yield return null; // 후속 적용 프레임 구분
            float beforeFireReaction = fireFirst.CurrentHealth; // 반응 전 체력
            Hit(effects, fireFirst, WeaponAttribute.Holy); // 두 재료와 가능한 신성 명중
            WeaponTargetBurnStatus fire = fireFirst.GetComponent<WeaponTargetBurnStatus>(); // 화염 재료 상태
            WeaponTargetDarkCurseStatus dark = fireFirst.GetComponent<WeaponTargetDarkCurseStatus>(); // 암흑 재료 상태
            Require(fire != null && fire.RemainingDuration <= 0f && dark != null && dark.IsActive && beforeFireReaction - fireFirst.CurrentHealth > 1f, "Older fire was not consumed first or dark was lost."); // 오래된 화염 우선과 암흑 보존 확인
            WeaponTarget darkFirst = CreateTarget("OlderDarkTarget", new Vector3(700f, 500f, 0f)); // 암흑을 먼저 받을 대상
            Hit(effects, darkFirst, WeaponAttribute.Dark); // 암흑 재료 최초 적용
            yield return null; // 최초 적용 프레임 구분
            Hit(effects, darkFirst, WeaponAttribute.Fire); // 화염 재료 후속 적용
            yield return null; // 후속 적용 프레임 구분
            float beforeDarkReaction = darkFirst.CurrentHealth; // 반응 전 체력
            Hit(effects, darkFirst, WeaponAttribute.Holy); // 두 재료와 가능한 신성 명중
            fire = darkFirst.GetComponent<WeaponTargetBurnStatus>(); // 화염 재료 상태
            dark = darkFirst.GetComponent<WeaponTargetDarkCurseStatus>(); // 암흑 재료 상태
            Require(dark != null && !dark.IsActive && fire != null && fire.RemainingDuration > 0f && beforeDarkReaction - darkFirst.CurrentHealth > 1f, "Older dark was not consumed first or fire was lost."); // 오래된 암흑 우선과 화염 보존 확인
            WeaponTarget tiedTarget = CreateTarget("TiedEffectTarget", new Vector3(800f, 500f, 0f)); // 같은 시점 재료 대상
            Hit(effects, tiedTarget, WeaponAttribute.Dark); // 같은 프레임 암흑 명중
            Hit(effects, tiedTarget, WeaponAttribute.Fire); // 같은 프레임 화염 명중
            Hit(effects, tiedTarget, WeaponAttribute.Holy); // 같은 프레임 신성 명중
            WeaponTargetBurnStatus tiedFire = tiedTarget.GetComponent<WeaponTargetBurnStatus>(); // 동률 화상 상태
            WeaponTargetDarkCurseStatus tiedDark = tiedTarget.GetComponent<WeaponTargetDarkCurseStatus>(); // 동률 저주 상태
            Require(tiedFire != null && tiedFire.RemainingDuration <= 0f && tiedDark != null && tiedDark.IsActive, "Equal-time reactions did not use attribute order."); // 동률 속성 순서 확인
            WeaponTarget spreadTarget = CreateTarget("SpreadCurseTarget", new Vector3(900f, 500f, 0f)); // 실제 전파 저주 대상
            System.Reflection.MethodInfo spread = typeof(WeaponAttributePlayerEffects).GetMethod("ApplySpreadCurse", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic); // 기존 저주 전파 경로 조회
            Require(spread != null, "Actual curse spread method missing."); // 실제 전파 경로 확인
            spread.Invoke(null, new object[] { spreadTarget, 8 }); // 기존 저주 전파 적용
            yield return null; // 전파 시점과 공격 시점 구분
            WeaponTargetDarkCurseStatus spreadCurse = spreadTarget.GetComponent<WeaponTargetDarkCurseStatus>(); // 실제 전파 저주 상태
            float beforeSpreadReaction = spreadTarget.CurrentHealth; // 전파 반응 전 체력
            Hit(effects, spreadTarget, WeaponAttribute.Holy); // 실제 신성 명중
            Require(spreadCurse != null && !spreadCurse.IsActive && beforeSpreadReaction - spreadTarget.CurrentHealth > 1f, "Spread curse did not become a reaction material."); // 전파 저주의 반응과 소비 확인
            AutomatedOverlappingReactionVerification.Finish(0, "Actual overlapping reactions chose the oldest effect and preserved the other."); // 실제 검증 성공 기록
        }

        private static void Hit(WeaponAttributeCombatEffects effects, WeaponTarget target, WeaponAttribute attribute) // 실제 속성 명중
        {
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(null, attribute, 2, 2, 1, target.transform.position, 1f); // 실제 속성 공격 구성
            effects.ApplyHit(attack, target, target.transform.position); // 실제 전투 명중 적용
        }
        private static WeaponTarget CreateTarget(string name, Vector3 position) // 실제 공격 대상 생성
        {
            GameObject item = new GameObject(name); // 통제 표적 객체
            item.transform.position = position; // 다른 적과 떨어진 표적 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(1000f); // 실제 표적 체력
            return target; // 실제 표적 반환
        }
        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedOverlappingReactionVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
