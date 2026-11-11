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
    public static class AutomatedDay45PickupSettingsVerification // 경험치 획득 설정 시험
    {
        private const string Key = "EpsilonDay45PickupSettingsVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedDay45PickupSettingsVerification() // 도메인 재시작 처리
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
            player.gameObject.AddComponent<Day45PickupSettingsVerificationRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[Day45PickupSettingsVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class Day45PickupSettingsVerificationRunner : MonoBehaviour // 실제 보석 설정 검증
    {
        private IEnumerator Start() // 실제 설정과 보석 시험
        {
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동기
            SnakeExperience experience = GetComponent<SnakeExperience>(); // 실제 경험치 수신기
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            Require(player != null && experience != null && spawner != null, "Pickup settings fixtures missing."); // 실제 시험 구성 확인
            Require(experience.PickupSettings != null && Mathf.Approximately(experience.PickupSettings.AttractionRange, 2.6f), "Game scene pickup settings are missing or changed."); // 실제 씬 연결과 기존 흡수 범위 확인
            player.enabled = false; // 실제 머리 위치 고정
            spawner.enabled = false; // 자연 적 출현 중지
            player.transform.position = new Vector3(1000f, 1000f, 0f); // 다른 객체에서 떨어진 시험 위치
            Type dataType = typeof(SnakeExperience).Assembly.GetType("ProjectEpsilon.Progression.ExperiencePickupSettings"); // 실제 보석 설정 형식 조회
            MethodInfo configure = typeof(SnakeExperience).GetMethod("ConfigurePickupSettings"); // 실제 보석 설정 연결 경로
            Require(dataType != null && configure != null, "Adjustable pickup settings are missing."); // 조정 가능한 설정 구성 확인
            ScriptableObject settings = ScriptableObject.CreateInstance(dataType); // 저장되지 않는 설정 사본
            SerializedObject editor = new SerializedObject(settings); // 실제 설정 필드 조회
            SerializedProperty range = editor.FindProperty("attractionRange"); // 실제 흡수 범위 설정
            SerializedProperty speed = editor.FindProperty("attractionSpeed"); // 실제 흡수 속도 설정
            SerializedProperty distance = editor.FindProperty("collectDistance"); // 실제 획득 거리 설정
            Require(range != null && speed != null && distance != null, "Pickup tuning fields missing."); // 실제 조정 필드 확인
            range.floatValue = 4f; // 시험용 흡수 범위 설정
            speed.floatValue = 1f; // 시험용 흡수 속도 설정
            distance.floatValue = 0.22f; // 기존 획득 거리 유지
            editor.ApplyModifiedPropertiesWithoutUndo(); // 메모리 설정 반영
            configure.Invoke(experience, new object[] { settings }); // 실제 경험치 수신기에 설정 연결
            ExperiencePickup tuned = ExperiencePickup.Spawn(player.transform.position + Vector3.right * 3.5f, 2, null, experience); // 기본 범위 밖 설정 범위 안 보석
            Vector3 initial = tuned.transform.position; // 실제 보석 초기 위치
            yield return new WaitForSeconds(0.25f); // 실제 보석 이동 대기
            Require(tuned != null && tuned.transform.position.x < initial.x - 0.1f && tuned.ExperienceValue == 2, "Configured gem did not attract without changing its XP value."); // 실제 설정 이동과 경험치 보존 확인
            configure.Invoke(experience, new object[] { null }); // 기존 기본값 복구
            ExperiencePickup fallback = ExperiencePickup.Spawn(player.transform.position + Vector3.up * 3.5f, 1, null, experience); // 기본 범위 밖 비교 보석
            Vector3 original = fallback.transform.position; // 기본 보석 위치
            yield return new WaitForSeconds(0.25f); // 실제 비교 시간 경과
            Require(fallback != null && Vector3.Distance(fallback.transform.position, original) < 0.001f, "Missing settings did not preserve the original pickup range."); // 설정 부재 시 기존 기본값 확인
            Destroy(tuned.gameObject); // 시험 보석 제거
            Destroy(fallback.gameObject); // 비교 보석 제거
            Destroy(settings); // 메모리 설정 사본 제거
            AutomatedDay45PickupSettingsVerification.Finish(0, "Actual pickup settings changed attraction without changing XP; missing settings retained existing behavior."); // 실제 전체 성공 기록
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedDay45PickupSettingsVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
