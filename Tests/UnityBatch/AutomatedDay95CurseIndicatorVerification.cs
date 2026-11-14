using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using ProjectEpsilon.Combat; // 실제 대상과 상태
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 속성 구분
using ProjectEpsilon.Enemies; // 자연 적 생성기
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay95CurseIndicatorVerification // 저주 표시 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay95CurseIndicator"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay95CurseIndicatorVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
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
            new GameObject("Day95CurseIndicatorRunner").AddComponent<Day95CurseIndicatorRunner>(); // 실제 표시 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day95CurseIndicatorVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day95CurseIndicatorRunner : MonoBehaviour // 실제 저주 표시 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 저주와 기존 상태 표시 확인
        { // 함수 시작
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            GameObject enemy = new GameObject("Day95Target"); // 격리된 실제 적
            WeaponTarget target = enemy.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(100f); // 시험 체력 설정
            WeaponTargetColdStatus cold = enemy.AddComponent<WeaponTargetColdStatus>(); // 실제 냉기 상태
            WeaponTargetPoisonStatus poison = enemy.AddComponent<WeaponTargetPoisonStatus>(); // 실제 독 상태
            WeaponTargetDarkCurseStatus dark = enemy.AddComponent<WeaponTargetDarkCurseStatus>(); // 실제 암흑 저주
            WeaponTargetCorrosiveCurseStatus corrosive = enemy.AddComponent<WeaponTargetCorrosiveCurseStatus>(); // 실제 부식 저주
            Require(cold.Apply(target, 2) && poison.Apply(target, 2) && dark.Apply(2, 1) > 0, "실제 냉기·독·저주 적용 실패"); // 실제 상태 적용
            corrosive.Apply(3f, 0.8f, 1.1f); // 실제 부식 저주 적용
            yield return null; // 실제 표시 갱신 대기
            yield return null; // 지연 갱신 완료 대기
            Transform coldIcon = FindVisible(enemy, "StatusColdIndicator"); // 실제 냉기 표시
            Transform poisonIcon = FindVisible(enemy, "StatusPoisonIndicator"); // 실제 독 표시
            Transform darkIcon = FindVisible(enemy, "StatusDarkIndicator"); // 실제 암흑 저주 표시
            Transform corrosiveIcon = FindVisible(enemy, "StatusCorrosiveCurseIndicator"); // 실제 부식 저주 표시
            Require(coldIcon != null && poisonIcon != null && darkIcon != null && corrosiveIcon != null, "중첩 상태의 저주 지속 표시 누락"); // 실제 네 상태 표시 확인
            Require(darkIcon.GetComponent<SpriteRenderer>().sprite != null && corrosiveIcon.GetComponent<SpriteRenderer>().sprite != null, "실제 저주 이미지 누락"); // 기존 이미지 확인
            Require(darkIcon.localPosition != corrosiveIcon.localPosition && darkIcon.localPosition != coldIcon.localPosition, "중첩 저주 표시 위치 중복"); // 실제 표시 분리 확인
            dark.Clear(); // 실제 암흑 저주 해제
            yield return null; // 실제 표시 갱신 대기
            yield return null; // 지연 갱신 완료 대기
            Require(!darkIcon.gameObject.activeSelf && corrosiveIcon.gameObject.activeSelf && coldIcon.gameObject.activeSelf && poisonIcon.gameObject.activeSelf, "암흑 저주 해제 시 다른 상태 표시 변경"); // 실제 개별 해제 확인
            cold.Clear(); // 실제 냉기 해제
            poison.Clear(); // 실제 독 해제
            corrosive.Apply(0.05f, 0.8f, 1.1f); // 짧은 부식 저주 만료 시간
            yield return new WaitForSeconds(0.2f); // 실제 반응 시간 만료 대기
            Require(enemy.transform.Find("StatusDarkIndicator") == null && enemy.transform.Find("StatusCorrosiveCurseIndicator") == null, "만료된 저주 표시 잔류"); // 실제 표시 정리 확인
            WeaponAttributeCombatEffects effects = FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 속성 전투 처리기
            Require(effects != null, "실제 속성 전투 처리기 누락"); // 실제 전투 구성 확인
            GameObject reactionEnemy = new GameObject("Day95ReactionTarget"); // 실제 반응 시험 대상
            WeaponTarget reactionTarget = reactionEnemy.AddComponent<WeaponTarget>(); // 실제 공격 대상
            reactionTarget.Configure(100f); // 실제 시험 체력
            WeaponAttributeAttackSnapshot darkAttack = new WeaponAttributeAttackSnapshot(null, WeaponAttribute.Dark, 2, 2, 1, reactionEnemy.transform.position, 1f); // 실제 암흑 공격 정보
            WeaponAttributeAttackSnapshot poisonAttack = new WeaponAttributeAttackSnapshot(null, WeaponAttribute.Poison, 2, 2, 1, reactionEnemy.transform.position, 1f); // 실제 독 공격 정보
            effects.ApplyHit(darkAttack, reactionTarget, reactionEnemy.transform.position); // 실제 암흑 명중
            yield return null; // 실제 암흑 표시 대기
            yield return null; // 지연 표시 완료 대기
            Require(FindVisible(reactionEnemy, "StatusDarkIndicator") != null, "실제 암흑 공격의 저주 표시 누락"); // 실제 공격 연결 확인
            effects.ApplyHit(poisonAttack, reactionTarget, reactionEnemy.transform.position); // 실제 독 명중과 복합 반응
            yield return null; // 실제 반응 표시 대기
            yield return null; // 지연 표시 완료 대기
            Require(FindVisible(reactionEnemy, "StatusCorrosiveCurseIndicator") != null && FindVisible(reactionEnemy, "StatusDarkIndicator") == null && FindVisible(reactionEnemy, "StatusPoisonIndicator") == null, "실제 부식 저주 반응의 표시 전환 실패"); // 실제 소비 상태와 반응 표시 확인
            AutomatedDay95CurseIndicatorVerification.Finish(true, "Overlapping curse icons, expiry and real reaction transition passed."); // 실제 시험 결과
        } // 함수 종료
        private static Transform FindVisible(GameObject enemy, string name) // 실제 활성 표시 조회
        { // 함수 시작
            Transform icon = enemy.transform.Find(name); // 실제 자식 표시 조회
            return icon != null && icon.gameObject.activeInHierarchy ? icon : null; // 실제 표시 또는 부재 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay95CurseIndicatorVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
