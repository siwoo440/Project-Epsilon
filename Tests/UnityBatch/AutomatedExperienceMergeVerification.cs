using System; // 기본 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 시험 설정 조회
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Enemies; // 적 출현 제어
using ProjectEpsilon.Player; // 플레이어 이동 제어
using ProjectEpsilon.Progression; // 실제 경험치와 보석
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 전환 연결
    public static class AutomatedExperienceMergeVerification // 실제 보석 병합 통제 시험
    { // 클래스 시작
        private const string ActiveKey = "EpsilonExperienceMergeVerification"; // 배치 진행 표식
        private static double started; // 실시간 시작 시각
        private static bool injected; // 시험 실행기 생성 여부
        private static bool finished; // 중복 종료 방지

        static AutomatedExperienceMergeVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 배치 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 플레이 진입 감시
            } // 조건 끝
        } // 생성자 끝

        public static void Run() // 배치 진입점
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 배치 진행 저장
            EditorApplication.update -= Tick; // 중복 구독 방지
            EditorApplication.update += Tick; // 플레이 진입 감시
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        } // 메서드 끝

        private static void Tick() // 시작 선택 및 제한 시간 확인
        { // 메서드 시작
            if (started <= 0d) // 실시간 시작 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 120d) // 전체 시험 제한
            { // 조건 시작
                Finish(1, "Timed out waiting for the actual Game scene test."); // 제한 시간 실패
                return; // 후속 처리 중지
            } // 조건 끝
            if (!EditorApplication.isPlaying || StartingWeaponBatchHelper.HandleStartingSelection()) // 실제 시작 버튼 처리
            { // 조건 시작
                return; // 게임 시작 대기
            } // 조건 끝
            if (injected || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 씬 초기화 확인
            { // 조건 시작
                return; // 초기화 또는 결과 대기
            } // 조건 끝
            SnakeExperience experience = UnityEngine.Object.FindFirstObjectByType<SnakeExperience>(); // 실제 경험치 관리자 조회
            if (experience == null) // 경험치 관리자 존재 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 실행기 중복 방지
            experience.gameObject.AddComponent<ExperienceMergeVerificationRunner>(); // 실제 씬 시험 실행
        } // 메서드 끝

        public static void Finish(int code, string message) // 결과와 종료 코드 기록
        { // 메서드 시작
            if (finished) // 중복 종료 확인
            { // 조건 시작
                return; // 중복 종료 방지
            } // 조건 끝
            finished = true; // 종료 상태 기록
            SessionState.SetBool(ActiveKey, false); // 진행 표식 제거
            EditorApplication.update -= Tick; // 감시 해제
            Debug.Log($"[ExperienceMergeVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 결과 기록
            EditorApplication.Exit(code); // Unity 종료
        } // 메서드 끝
    } // 클래스 끝

    public sealed class ExperienceMergeVerificationRunner : MonoBehaviour // 실제 씬 통제 시험 실행기
    { // 클래스 시작
        private static readonly BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic; // 직렬화 설정 조회
        private Type controllerType; // 실제 병합 관리자 형식
        private Behaviour controller; // 실제 병합 관리자
        private SnakeExperience experience; // 실제 경험치 관리자
        private Vector3 playerPosition; // 통제 플레이어 위치

        private IEnumerator Start() // 실제 시험 순서
        { // 코루틴 시작
            controllerType = Type.GetType("ProjectEpsilon.Progression.ExperienceGemMergeController, Assembly-CSharp"); // 제품 관리자 형식 조회
            Require(controllerType != null, "Actual Game scene has no experience gem merge controller type."); // 28일차 기능 부재 확인
            controller = UnityEngine.Object.FindFirstObjectByType(controllerType) as Behaviour; // 실제 씬 관리자 조회
            Require(controller != null, "Actual Game scene has no enabled experience gem merge controller."); // 실제 씬 연결 확인
            experience = GetComponent<SnakeExperience>(); // 실제 경험치 관리자 조회
            experience.Configure(10000, 1f); // 레벨업 시험 간섭 방지
            experience.ResetProgression(); // 시험 경험치 초기화
            SnakeMovement movement = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동 조회
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현 조회
            Require(movement != null && spawner != null, "Controlled Game scene fixtures are missing."); // 필수 통제 구성 확인
            movement.enabled = false; // 플레이어 이동 정지
            spawner.enabled = false; // 자연 적 출현 정지
            playerPosition = experience.transform.position; // 실제 수신자 위치 기록
            controller.enabled = false; // 시험 준비 중 병합 중지
            ClearPickups(); // 기존 보석 정리
            yield return null; // 지연 파괴 완료 대기
            yield return VerifyPriorityAndBudgets(); // 최초 검사 우선순위와 예산
            yield return VerifyConversion(1, 5, 5); // 소형 다섯 개 병합
            yield return VerifyConversion(5, 4, 20); // 중형 네 개 병합
            yield return VerifyPause(); // 실제 일시정지 검증
            yield return VerifyInactiveAndDestroyed(); // 비활성화와 지연 파괴 검증
            yield return VerifyCollection(); // 실제 수집과 병합 경합 검증
            AutomatedExperienceMergeVerification.Finish(0, "Actual automatic gem conversions, priority, budgets, pause, inactive/destroyed pickups and collection preserved experience under controlled conditions."); // 시험 결과 기록
        } // 코루틴 끝

        private IEnumerator VerifyPriorityAndBudgets() // 실제 최초 검사 우선순위와 예산
        { // 코루틴 시작
            controller.enabled = false; // 시험 준비 중 검사 정지
            ClearPickups(); // 이전 보석 정리
            yield return null; // 지연 파괴 완료 대기
            ConfigureController(15); // 세 그룹 발동 설정
            Camera camera = Camera.main; // 실제 게임 카메라
            Require(camera != null && camera.orthographic, "Actual orthographic camera is missing."); // 실제 탑뷰 카메라 확인
            camera.orthographicSize = 20f; // 화면 안과 밖 위치 통제
            Vector3 visible = playerPosition + Vector3.right * 12f; // 화면 안 흡수 범위 밖 위치
            Vector3 farther = playerPosition + Vector3.right * 60f; // 더 먼 화면 밖 위치
            Vector3 nearer = playerPosition + Vector3.right * 45f; // 덜 먼 화면 밖 위치
            Require(IsVisible(camera, visible) && !IsVisible(camera, nearer) && !IsVisible(camera, farther), "Controlled viewport positions are not valid."); // 실제 뷰포트 확인
            SpawnGroup(visible, 1, 5); // 화면 안 소형 그룹
            SpawnGroup(nearer, 1, 5); // 가까운 화면 밖 그룹
            SpawnGroup(farther, 1, 5); // 먼 화면 밖 그룹
            int previous = ReadMetric("CompletedPassCount"); // 검사 횟수 기록
            controller.enabled = true; // 실제 주기 검사 재개
            yield return WaitForPass(previous); // 첫 실제 검사 대기
            controller.enabled = false; // 첫 검사 직후 정지
            yield return null; // 지연 파괴 완료 대기
            Require(CountPickups(5) == 1 && CountPickups(1) == 10, "One-pass group budget was exceeded or no group merged."); // 한 검사당 병합 수 확인
            ExperiencePickup medium = FindPickup(5); // 병합 결과 보석 조회
            Require(medium != null && Vector3.Distance(medium.transform.position, farther) < 1f, "First pass did not prioritize the farthest offscreen group."); // 화면 밖 및 거리 우선 확인
            Require(TotalExperience() == 15, "Priority test lost or duplicated experience."); // 실제 총량 확인
            Require(ReadMetric("LastNeighborInspections") <= 512 && ReadMetric("LastBudgetedDistanceChecks") <= 512, "Distance inspection budget was exceeded."); // 거리 비교 예산 확인
            Debug.Log($"[ExperienceMergeVerification] first pass offscreen/farthest, group budget=1, neighbor inspections={ReadMetric("LastNeighborInspections")}, distance checks={ReadMetric("LastBudgetedDistanceChecks")}, head checks={ReadMetric("LastCandidateHeadDistanceChecks")}."); // 최초 검사 계측 기록
        } // 코루틴 끝

        private IEnumerator VerifyConversion(int value, int count, int expected) // 실제 자동 병합 검증
        { // 코루틴 시작
            controller.enabled = false; // 보석 준비 중 검사 정지
            ClearPickups(); // 이전 보석 정리
            yield return null; // 지연 파괴 완료 대기
            experience.ResetProgression(); // 경험치 초기화
            ConfigureController(count); // 병합 임시 설정
            Vector3 anchor = playerPosition + Vector3.right * 20f; // 실제 흡수 범위 밖 위치
            SpawnGroup(anchor, value, count); // 실제 원본 보석 생성
            Require(TotalExperience() == value * count, "Initial pickup experience mismatch."); // 병합 전 총량 확인
            controller.enabled = true; // 실제 Update 검사 재개
            float deadline = Time.time + 3f; // 병합 대기 제한
            while (Time.time < deadline && CountPickups(expected) != 1) // 실제 주기 검사 대기
            { // 반복 시작
                yield return null; // 실제 프레임 진행
            } // 반복 끝
            controller.enabled = false; // 검증 중 추가 검사 정지
            yield return null; // 지연 파괴 완료 대기
            Require(CountPickups(expected) == 1 && CountAllPickups() == 1, "Actual automatic conversion produced an incorrect pickup count."); // 결과 개수 확인
            Require(TotalExperience() == value * count, "Automatic conversion changed total experience."); // 경험치 보존 확인
            Debug.Log($"[ExperienceMergeVerification] actual automatic conversion {count}x{value} -> {expected}, total={TotalExperience()}."); // 통제 시험 결과
        } // 코루틴 끝

        private IEnumerator VerifyPause() // 실제 일시정지 중 검사 확인
        { // 코루틴 시작
            controller.enabled = false; // 보석 준비 중 검사 정지
            ClearPickups(); // 이전 보석 정리
            yield return null; // 지연 파괴 완료 대기
            ConfigureController(5); // 소형 다섯 개 발동 설정
            SpawnGroup(playerPosition + Vector3.right * 20f, 1, 5); // 흡수 범위 밖 그룹
            GameManager.Instance.PauseGame(); // 실제 게임 일시정지
            int previous = ReadMetric("CompletedPassCount"); // 정지 전 검사 횟수
            controller.enabled = true; // 정지 중 검사 활성화
            yield return new WaitForSecondsRealtime(0.7f); // 검사 간격 이상의 실제 시간 대기
            Require(ReadMetric("CompletedPassCount") == previous && CountPickups(1) == 5, "Paused game advanced or merged pickups."); // 게임 시간 정지 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            yield return WaitForPass(previous); // 다음 실제 검사 대기
            controller.enabled = false; // 추가 검사 정지
            yield return null; // 지연 파괴 완료 대기
            Require(CountPickups(5) == 1 && TotalExperience() == 5, "Resumed game did not merge correctly."); // 재개 후 경험치 확인
            Debug.Log("[ExperienceMergeVerification] actual pause stopped passes; resume preserved five experience."); // 정지 결과 기록
        } // 코루틴 끝

        private IEnumerator VerifyInactiveAndDestroyed() // 비활성과 외부 파괴 검증
        { // 코루틴 시작
            controller.enabled = false; // 보석 준비 중 검사 정지
            ClearPickups(); // 이전 보석 정리
            yield return null; // 지연 파괴 완료 대기
            experience.ResetProgression(); // 시험 경험치 초기화
            ConfigureController(4); // 네 활성 보석도 검사하도록 설정
            Vector3 anchor = playerPosition + Vector3.right * 20f; // 흡수 범위 밖 위치
            SpawnGroup(anchor, 1, 4); // 활성 소형 네 개
            ExperiencePickup fifth = ExperiencePickup.Spawn(anchor + Vector3.up * 0.2f, 1, null, experience); // 다섯째 실제 보석
            fifth.gameObject.SetActive(false); // 다섯째 즉시 비활성화
            int previous = ReadMetric("CompletedPassCount"); // 현재 검사 횟수
            controller.enabled = true; // 실제 주기 검사 재개
            yield return WaitForPass(previous); // 실제 검사 대기
            controller.enabled = false; // 상태 확인 중 검사 정지
            Require(CountPickups(1) == 4 && CountPickups(5) == 0 && TotalExperience() == 4, "Inactive pickup was included in a merge."); // 비활성 후보 제외 확인
            fifth.gameObject.SetActive(true); // 미소비 보석 재활성화
            previous = ReadMetric("CompletedPassCount"); // 재활성 후 검사 횟수
            controller.enabled = true; // 실제 검사 재개
            yield return WaitForPass(previous); // 실제 검사 대기
            controller.enabled = false; // 추가 검사 정지
            yield return null; // 지연 파괴 완료 대기
            Require(CountPickups(5) == 1 && TotalExperience() == 5, "Reactivated pickup did not merge exactly once."); // 재활성화 경험치 확인
            ClearPickups(); // 이전 병합 결과 정리
            yield return null; // 지연 파괴 완료 대기
            SpawnGroup(anchor, 1, 4); // 파괴 경계의 활성 네 개
            fifth = ExperiencePickup.Spawn(anchor + Vector3.up * 0.2f, 1, null, experience); // 외부 파괴 대상
            UnityEngine.Object.Destroy(fifth.gameObject); // 실제 외부 지연 파괴
            yield return null; // Unity 파괴 완료 프레임
            previous = ReadMetric("CompletedPassCount"); // 파괴 후 검사 횟수
            controller.enabled = true; // 실제 검사 재개
            yield return WaitForPass(previous); // 실제 검사 대기
            controller.enabled = false; // 추가 검사 정지
            Require(CountPickups(1) == 4 && CountPickups(5) == 0 && TotalExperience() == 4, "Destroyed pickup remained a merge candidate after its destruction frame."); // 파괴 완료 후보 제외 확인
            Debug.Log("[ExperienceMergeVerification] inactive exclusion, reactivation and external destruction after frame boundary passed."); // 객체 경계 결과 기록
        } // 코루틴 끝

        private IEnumerator VerifyCollection() // 실제 보석 수집과 병합 경합
        { // 코루틴 시작
            controller.enabled = false; // 보석 준비 중 검사 정지
            ClearPickups(); // 이전 보석 정리
            yield return null; // 지연 파괴 완료 대기
            experience.ResetProgression(); // 수집 전 경험치 초기화
            ConfigureController(5); // 다섯 보석 검사 설정
            SpawnGroup(playerPosition, 1, 5); // 실제 수집 범위의 보석
            controller.enabled = true; // 실제 검사 재개
            yield return null; // 실제 수집과 검사 프레임
            yield return null; // 지연 파괴 완료 대기
            controller.enabled = false; // 추가 검사 정지
            Require(experience.CurrentExperience == 5 && CountAllPickups() == 0 && TotalExperience() == 5, "Simultaneous actual collection lost or duplicated experience."); // 실제 수집 총량 확인
            Debug.Log("[ExperienceMergeVerification] actual simultaneous nearby collection: XP=5, remaining pickups=0."); // 수집 결과 기록
        } // 코루틴 끝

        private void ConfigureController(int threshold) // 메모리 내 임시 검사 설정
        { // 메서드 시작
            SetField("mergeThreshold", threshold); // 발동 임계 임시 설정
            SetField("mergeDistance", 1.5f); // 인접 거리 임시 설정
            SetField("mergeInterval", 0.2f); // 게임 시간 검사 간격
            SetField("maximumGroupsPerPass", 1); // 검사당 병합 예산
            SetField("maximumAnchorsPerPass", 32); // 기준 보석 검사 예산
            SetField("maximumNeighborChecksPerPass", 512); // 이웃 거리 검사 예산
        } // 메서드 끝

        private void SpawnGroup(Vector3 anchor, int value, int count) // 실제 보석 그룹 생성
        { // 메서드 시작
            for (int index = 0; index < count; index++) // 그룹 보석 순회
            { // 반복 시작
                ExperiencePickup.Spawn(anchor + Vector3.up * (index * 0.04f), value, null, experience); // 실제 보석 생성
            } // 반복 끝
        } // 메서드 끝

        private IEnumerator WaitForPass(int previous) // 자동 검사 완료 대기
        { // 코루틴 시작
            float deadline = Time.realtimeSinceStartup + 5f; // 실시간 제한 시각
            while (ReadMetric("CompletedPassCount") <= previous && Time.realtimeSinceStartup < deadline) // 실제 검사 대기
            { // 반복 시작
                yield return null; // 실제 프레임 진행
            } // 반복 끝
            Require(ReadMetric("CompletedPassCount") == previous + 1, "A single automatic merge pass did not complete in time."); // 정확한 검사 수 확인
        } // 코루틴 끝

        private int ReadMetric(string name) // 관리자 읽기 전용 계측 조회
        { // 메서드 시작
            PropertyInfo property = controllerType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance); // 실제 계측 속성 조회
            Require(property != null, "Missing merge observation property: " + name); // 관찰 계약 확인
            return Convert.ToInt32(property.GetValue(controller)); // 실제 계측값 반환
        } // 메서드 끝

        private static bool IsVisible(Camera camera, Vector3 position) // 실제 카메라 뷰포트 확인
        { // 메서드 시작
            Vector3 viewport = camera.WorldToViewportPoint(position); // 월드 좌표 변환
            return viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f; // 화면 안 여부 반환
        } // 메서드 끝

        private static ExperiencePickup FindPickup(int value) // 실제 결과 보석 조회
        { // 메서드 시작
            foreach (ExperiencePickup pickup in UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) // 활성 보석 순회
            { // 반복 시작
                if (pickup.isActiveAndEnabled && ReadValue(pickup) == value) // 실제 값 확인
                { // 조건 시작
                    return pickup; // 일치 보석 반환
                } // 조건 끝
            } // 반복 끝
            return null; // 일치 보석 부재
        } // 메서드 끝

        private void SetField(string name, object value) // 직렬화된 임시 설정 변경
        { // 메서드 시작
            FieldInfo field = controllerType.GetField(name, Fields); // 실제 설정 필드 조회
            Require(field != null, "Missing serialized merge setting: " + name); // 설정 계약 확인
            field.SetValue(controller, value); // 시험용 메모리 설정
        } // 메서드 끝

        private static int ReadValue(ExperiencePickup pickup) // 실제 보석 값 조회
        { // 메서드 시작
            FieldInfo field = typeof(ExperiencePickup).GetField("experienceValue", Fields); // 기존 직렬화 값 조회
            Require(field != null, "Actual pickup experience field is missing."); // 값 조회 계약 확인
            return (int)field.GetValue(pickup); // 보석 경험치 반환
        } // 메서드 끝

        private int TotalExperience() // 실제 획득 및 활성 보석 총량
        { // 메서드 시작
            int total = experience.CurrentExperience; // 플레이어 획득 경험치
            foreach (ExperiencePickup pickup in UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) // 활성 보석 순회
            { // 반복 시작
                if (pickup.isActiveAndEnabled) // 유효 보석 확인
                { // 조건 시작
                    total += ReadValue(pickup); // 활성 보석 경험치 합산
                } // 조건 끝
            } // 반복 끝
            return total; // 실제 총량 반환
        } // 메서드 끝

        private static int CountPickups(int value) // 특정 값의 활성 보석 수
        { // 메서드 시작
            int count = 0; // 결과 개수
            foreach (ExperiencePickup pickup in UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) // 실제 보석 순회
            { // 반복 시작
                if (pickup.isActiveAndEnabled && ReadValue(pickup) == value) // 활성 상태와 값 확인
                { // 조건 시작
                    count++; // 일치 개수 증가
                } // 조건 끝
            } // 반복 끝
            return count; // 일치 개수 반환
        } // 메서드 끝

        private static int CountAllPickups() // 모든 활성 보석 수
        { // 메서드 시작
            return UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length; // 활성 보석 개수 반환
        } // 메서드 끝

        private static void ClearPickups() // 실제 보석 정리
        { // 메서드 시작
            foreach (ExperiencePickup pickup in UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) // 기존 보석 순회
            { // 반복 시작
                pickup.gameObject.SetActive(false); // 즉시 시험 후보 제외
                UnityEngine.Object.Destroy(pickup.gameObject); // 지연 파괴 예약
            } // 반복 끝
        } // 메서드 끝

        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedExperienceMergeVerification.Finish(1, message); // 실패 기록과 종료
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
