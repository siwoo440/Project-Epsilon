using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using ProjectEpsilon.Combat; // 실제 적 상태
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 자연 적 생성기
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay93StatusIndicatorVerification // 냉기와 독 표시 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay93StatusIndicator"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay93StatusIndicatorVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 게임 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실제 게임 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시작 방지
            new GameObject("Day93StatusIndicatorRunner").AddComponent<Day93StatusIndicatorRunner>(); // 실제 상태 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day93StatusIndicatorVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 배치 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day93StatusIndicatorRunner : MonoBehaviour // 실제 상태 표시 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 냉기와 독 표시 확인
        { // 함수 시작
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            GameObject enemy = new GameObject("Day93Target"); // 격리된 실제 적
            enemy.transform.position = new Vector3(500f, 500f, 0f); // 다른 전투와 분리된 위치
            WeaponTarget target = enemy.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(100f); // 시험 체력 설정
            WeaponTargetColdStatus cold = enemy.AddComponent<WeaponTargetColdStatus>(); // 실제 냉기 상태
            WeaponTargetPoisonStatus poison = enemy.AddComponent<WeaponTargetPoisonStatus>(); // 실제 독 상태
            Require(cold.Apply(target, 2) && poison.Apply(target, 2), "실제 냉기 또는 독 적용 실패"); // 실제 상태 적용 확인
            yield return null; // 실제 표시 갱신 대기
            yield return null; // 지연 갱신 완료 대기
            Transform coldIcon = enemy.transform.Find("StatusColdIndicator"); // 실제 냉기 표시
            Transform poisonIcon = enemy.transform.Find("StatusPoisonIndicator"); // 실제 독 표시
            Require(coldIcon != null && poisonIcon != null && coldIcon.gameObject.activeSelf && poisonIcon.gameObject.activeSelf, "냉기와 독의 동시 지속 표시 누락"); // 실제 중첩 표시 확인
            Require(coldIcon.GetComponent<SpriteRenderer>().sprite != null && poisonIcon.GetComponent<SpriteRenderer>().sprite != null, "실제 상태 이미지 누락"); // 실제 이미지 확인
            Vector3 previous = coldIcon.position; // 실제 표시 위치 기록
            enemy.transform.position += Vector3.right * 3f; // 실제 적 이동
            Require(Mathf.Approximately(coldIcon.position.x - previous.x, 3f), "상태 표시가 적을 따라가지 않음"); // 실제 적 추적 확인
            GameManager.Instance.PauseGame(); // 실제 게임 일시정지
            float remaining = poison.RemainingDuration; // 정지 직전 독 시간
            yield return new WaitForSecondsRealtime(0.1f); // 실제 정지 시간 대기
            Require(Mathf.Approximately(poison.RemainingDuration, remaining) && poisonIcon.gameObject.activeSelf, "정지 중 상태 또는 표시 변경"); // 실제 일시정지 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            cold.Clear(); // 실제 냉기 해제
            yield return null; // 실제 표시 갱신 대기
            yield return null; // 지연 갱신 완료 대기
            Require(!coldIcon.gameObject.activeSelf && poisonIcon.gameObject.activeSelf, "냉기 해제 시 독 표시 보존 실패"); // 독립 상태 해제 확인
            poison.Clear(); // 실제 독 해제
            yield return null; // 실제 표시 갱신 대기
            yield return null; // 지연 제거 완료 대기
            Require(enemy.transform.Find("StatusColdIndicator") == null && enemy.transform.Find("StatusPoisonIndicator") == null, "상태 해제 후 표시 잔류"); // 실제 표시 정리 확인
            GameObject[] enemies = new GameObject[50]; // 표시 상한 시험 대상
            for (int index = 0; index < enemies.Length; index++) // 실제 다수 적 생성
            { // 반복 시작
                GameObject item = new GameObject("Day93LimitTarget"); // 실제 적 객체
                WeaponTarget limitedTarget = item.AddComponent<WeaponTarget>(); // 실제 공격 대상
                limitedTarget.Configure(100f); // 시험 체력 설정
                Require(item.AddComponent<WeaponTargetPoisonStatus>().Apply(limitedTarget, 2), "표시 상한 시험 독 적용 실패"); // 실제 독 적용
                enemies[index] = item; // 실제 적 보관
            } // 반복 종료
            yield return null; // 실제 다수 표시 갱신
            yield return null; // 지연 갱신 완료 대기
            Require(CountVisible(enemies) == 48, "동시 상태 표시 상한 미적용"); // 실제 동시 표시 수 확인
            enemies[0].SetActive(false); // 실제 첫 적 비활성화
            yield return null; // 표시 자리 반환 대기
            yield return null; // 대기 적의 표시 갱신 대기
            Require(CountVisible(enemies) == 48, "비활성 적의 표시 자리 재사용 실패"); // 실제 표시 자리 재사용 확인
            for (int index = 0; index < enemies.Length; index++) // 실제 시험 적 정리
            { // 반복 시작
                Destroy(enemies[index]); // 실제 적과 표시 제거
            } // 반복 종료
            yield return null; // 실제 적 제거 대기
            WeaponTargetPoisonStatus expiry = enemy.AddComponent<WeaponTargetPoisonStatus>(); // 만료 시험의 실제 독
            Require(expiry.Apply(target, 2), "만료 시험 독 적용 실패"); // 실제 독 적용
            typeof(WeaponTargetPoisonStatus).GetField("remainingDuration", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(expiry, 0.05f); // 짧은 만료 시험 시간
            yield return new WaitForSeconds(0.2f); // 실제 독 시간 만료 대기
            Require(enemy.transform.Find("StatusPoisonIndicator") == null, "만료된 독의 표시 잔류"); // 실제 시간 만료 정리 확인
            AutomatedDay93StatusIndicatorVerification.Finish(true, "Overlap, movement, pause, cleanup, expiry and display limit passed."); // 실제 시험 결과
        } // 함수 종료
        private static int CountVisible(GameObject[] enemies) // 실제 독 표시 수 집계
        { // 함수 시작
            int count = 0; // 실제 표시 수 초기화
            foreach (GameObject enemy in enemies) // 실제 시험 적 순회
            { // 반복 시작
                Transform icon = enemy == null ? null : enemy.transform.Find("StatusPoisonIndicator"); // 실제 독 표시 조회
                if (icon != null && icon.gameObject.activeInHierarchy) // 실제 화면 표시 여부
                { // 조건 시작
                    count++; // 실제 표시 수 증가
                } // 조건 종료
            } // 반복 종료
            return count; // 실제 표시 수 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay93StatusIndicatorVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
