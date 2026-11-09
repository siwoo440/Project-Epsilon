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
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedDay45PilotVerification // 실제 원거리 포탑 조종 판단 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay45PilotVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedDay45PilotVerification() // 도메인 재시작 처리
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
            if (EditorApplication.timeSinceStartup - started > 120d) // 시험 제한 시간 확인
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
            player.gameObject.AddComponent<Day45PilotVerificationRunner>(); // 실제 시험 실행
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 시험 종료
        { // 메서드 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[Day45PilotVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        } // 메서드 끝
    } // 클래스 끝
    public sealed class Day45PilotVerificationRunner : MonoBehaviour // 실제 조종 경로 검증
    { // 클래스 시작
        private IEnumerator Start() // 실제 조종 경로 시험
        { // 코루틴 시작
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동기
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            SnakeStamina stamina = GetComponent<SnakeStamina>(); // 실제 스태미나
            Require(player != null && body != null && follower != null && spawner != null && weapons != null && stamina != null, "Pilot fixtures missing."); // 실제 시험 구성 확인
            player.enabled = false; // 머리 위치 통제
            follower.enabled = false; // 몸통 위치 통제
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 시험 중 자동 공격 중지
            foreach (WeaponTarget existingTarget in UnityEngine.Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 이전 자연 적 순회
            { // 반복 시작
                existingTarget.gameObject.SetActive(false); // 기존 적 비활성화
            } // 반복 끝
            player.transform.position = Vector3.zero; // 실제 머리 위치
            player.transform.rotation = Quaternion.identity; // 위쪽 전진 방향
            for (int index = 0; index < body.BodySegments.Count; index++) // 실제 몸통 순회
            { // 반복 시작
                body.BodySegments[index].transform.position = new Vector3(100f + index * 5f, 100f, 0f); // 통제 중 자기 충돌 방지
            } // 반복 끝
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 적 내부 필드 범위
            EarlyEnemySettings[] settings = (EarlyEnemySettings[])typeof(EarlyEnemyWaveSpawner).GetField("enemies", flags).GetValue(spawner); // 실제 초반 적 설정
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", flags); // 실제 적 생성 경로
            Require(settings != null && settings.Length > 0 && spawn != null, "Enemy settings missing."); // 실제 적 설정 확인
            WeaponTarget target = (WeaponTarget)spawn.Invoke(spawner, new object[] { settings[0], new Vector3(0f, -6f, 0f), false }); // 뒤쪽의 실제 적
            EarlyEnemyBehaviour behaviour = target.GetComponent<EarlyEnemyBehaviour>(); // 실제 적 행동
            behaviour.enabled = false; // 조종 판단 중 적 이동 중지
            ExperiencePickup gem = ExperiencePickup.Spawn(new Vector3(0f, 5f, 0f), 1, null, SnakeExperience.Current); // 앞쪽의 실제 경험치 보석
            Require(gem != null, "XP gem missing."); // 실제 보석 생성 확인
            Type pilot = typeof(AutomatedNaturalCombatVerification); // 실제 자연 조종기 형식
            FieldInfo focus = pilot.GetField("focusTarget", BindingFlags.Static | BindingFlags.NonPublic); // 기존 집중 적 설정
            MethodInfo choose = pilot.GetMethod("ChooseKeys", BindingFlags.Static | BindingFlags.NonPublic); // 실제 조향 판단 경로
            Require(focus != null && choose != null, "Pilot decision method missing."); // 실제 조종 경로 확인
            focus.SetValue(null, target); // 뒤쪽 적의 기존 집중 상태 재현
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            int keys = (int)choose.Invoke(null, new object[] { player, body, new[] { behaviour }, stamina, 1f }); // 실제 단일 경로 판단
            Require((keys & 3) == 0, "Pilot turned back toward a distant enemy while an XP gem was directly ahead: keys=" + keys); // 무제한 포탑에서 불필요한 재접근 방지 확인
            yield return null; // 실제 프레임 대기
            AutomatedDay45PilotVerification.Finish(0, "Pilot continued toward the forward XP gem without chasing a distant enemy behind."); // 전체 성공 기록
        } // 코루틴 끝
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedDay45PilotVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
