using System; // 기본 형식
using System.Collections; // 코루틴과 목록
using System.Collections.Generic; // 시험 보석 목록
using System.Reflection; // 실제 관리자 설정과 계측
using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Enemies; // 적 출현 제어
using ProjectEpsilon.Player; // 플레이어 이동 제어
using ProjectEpsilon.Progression; // 실제 경험치와 보석
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 편집기 영역
{
    [InitializeOnLoad] // 도메인 갱신 연결
    public static class AutomatedExperienceMergePartialVerification // 부분 그룹 실제 프레임 시험
    {
        private const string ActiveKey = "EpsilonExperienceMergePartialVerification"; // 시험 상태 표식
        private static double started; // 실제 시작 시각
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 차단

        static AutomatedExperienceMergePartialVerification() // 도메인 연결
        {
            if (SessionState.GetBool(ActiveKey, false)) // 배치 진행 확인
            {
                EditorApplication.update += Tick; // 실행 감시 연결
            }
        }

        public static void Run() // 배치 진입점
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 배치 시작 기록
            EditorApplication.update -= Tick; // 중복 연결 해제
            EditorApplication.update += Tick; // 플레이 진입 감시
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        }

        private static void Tick() // 시작 선택과 시간 제한
        {
            if (started <= 0d) // 최초 갱신 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 120d) // 전체 실행 시간 확인
            {
                Finish(1, "Partial-group test timed out."); // 시간 초과 실패
                return; // 추가 처리 중단
            }
            if (!EditorApplication.isPlaying || StartingWeaponBatchHelper.HandleStartingSelection()) // 실제 시작 버튼 선택
            {
                return; // 게임 시작 대기
            }
            if (injected || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 씬 초기화 상태
            {
                return; // 초기화 또는 결과 대기
            }
            SnakeExperience experience = UnityEngine.Object.FindFirstObjectByType<SnakeExperience>(); // 실제 경험치 관리자
            if (experience == null) // 실제 수신자 확인
            {
                return; // 구성 대기
            }
            injected = true; // 중복 실행 방지
            experience.gameObject.AddComponent<ExperienceMergePartialRunner>(); // 실제 씬 실행기 추가
        }

        public static void Finish(int code, string message) // 종료 결과 기록
        {
            if (finished) // 중복 종료 확인
            {
                return; // 중복 처리 차단
            }
            finished = true; // 종료 상태 저장
            SessionState.SetBool(ActiveKey, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 감시 연결 해제
            Debug.Log($"[ExperienceMergePartial] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // Unity 종료 코드
        }
    }

    public sealed class ExperienceMergePartialRunner : MonoBehaviour // 실제 다중 프레임 시험
    {
        private const int Budget = 17; // 검사와 확정의 낮은 예산
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 설정 조회
        private readonly List<ExperiencePickup> tracked = new List<ExperiencePickup>(); // 생성된 시험 보석
        private Type managerType; // 실제 병합 관리자 형식
        private Behaviour manager; // 실제 씬 병합 관리자
        private SnakeExperience experience; // 실제 경험치 수신자
        private ExperiencePickup[] ordered; // 고유 번호 순서 보석
        private Vector3 anchorPosition; // 통제 그룹 위치

        private IEnumerator Start() // 통제 시험 순서
        {
            managerType = Type.GetType("ProjectEpsilon.Progression.ExperienceGemMergeController, Assembly-CSharp"); // 제품 관리자 형식
            Require(managerType != null, "Actual scene has no merge controller type."); // 이전 제품의 기능 부재
            manager = UnityEngine.Object.FindFirstObjectByType(managerType) as Behaviour; // 실제 씬 관리자
            Require(manager != null, "Actual scene has no active merge controller."); // 씬 연결 확인
            experience = GetComponent<SnakeExperience>(); // 실제 경험치 수신자
            Require(experience != null, "Actual experience receiver is missing."); // 수신자 연결 확인
            SnakeMovement movement = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 이동 제어기
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            Require(movement != null && spawner != null, "Controlled scene fixtures are missing."); // 통제 구성 확인
            movement.enabled = false; // 시험 중 플레이어 이동 중지
            spawner.enabled = false; // 시험 중 자연 적 출현 중지
            experience.Configure(10000, 1f); // 레벨업 UI 간섭 방지
            experience.ResetProgression(); // 시험 경험치 초기화
            yield return ResetFixture(); // 시작 상태 준비
            yield return VerifyInsertion(); // 부분 그룹 중 동적 삽입
            yield return ResetFixture(); // 다음 시험 초기화
            yield return VerifyMovement(); // 부분 재료 이동 재검증
            yield return ResetFixture(); // 다음 시험 초기화
            yield return VerifyCollection(); // 부분 재료 실제 수집
            AutomatedExperienceMergePartialVerification.Finish(0, "Actual Game scene low-budget partial groups, insertion, movement and collection preserved XP."); // 성공 기록
        }

        private IEnumerator ResetFixture() // 실제 보석과 검사 상태 초기화
        {
            manager.enabled = false; // 부분 검사 상태 초기화
            foreach (ExperiencePickup pickup in UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) // 실제 활성 보석 순회
            {
                if (pickup != null) // 보석 존재 확인
                {
                    Destroy(pickup.gameObject); // 이전 시험 보석 제거
                }
            }
            yield return null; // 지연 파괴 프레임 완료
            tracked.Clear(); // 시험 보석 참조 초기화
            experience.ResetProgression(); // 시험 경험치 초기화
            SetField("mergeThreshold", 5); // 실제 병합 임계값 통제
            SetField("mergeDistance", 1f); // 실제 인접 거리 통제
            SetField("mergeInterval", 3600f); // 자동 검사 중복 방지
            SetField("maximumGroupsPerPass", 1); // 검사당 그룹 한도
            SetField("maximumAnchorsPerPass", 1); // 검사당 기준 보석 한도
            SetField("maximumNeighborChecksPerPass", Budget); // 검사당 실제 연산 예산
            anchorPosition = experience.transform.position + Vector3.right * 80f; // 흡수 범위 밖 기준 위치
            ordered = new ExperiencePickup[24]; // 동적 순서 시험 보석
            for (int index = 0; index < ordered.Length; index++) // 실제 보석 생성
            {
                ordered[index] = Spawn(experience.transform.position + Vector3.right * (10f + index * 2f)); // 흩어진 소형 보석
            }
            Array.Sort(ordered, (left, right) => left.GetInstanceID().CompareTo(right.GetInstanceID())); // 실제 Unity 번호순 정렬
            ordered[0].transform.position = anchorPosition; // 가장 먼 기준 보석
            ordered[9].transform.position = anchorPosition - Vector3.right * 0.1f; // 첫 인접 재료
            ordered[15].transform.position = anchorPosition - Vector3.right * 0.2f; // 둘째 인접 재료
            ordered[21].transform.position = anchorPosition - Vector3.right * 0.3f; // 셋째 인접 재료
            ordered[23].transform.position = anchorPosition - Vector3.right * 0.4f; // 넷째 인접 재료
            manager.enabled = true; // 실제 검사 허용
            Require(TotalExperience() == 24, "Fixture did not preserve 24 XP."); // 초기 경험치 합 확인
        }

        private IEnumerator PreparePartial() // 여러 프레임에 걸친 부분 그룹 생성
        {
            int previousFrame = Time.frameCount; // 첫 검사 프레임
            Require(Pass() == 0, "Partial group merged in the first low-budget pass."); // 첫 예산 분할 확인
            Require(HasPartialAnchor() && PartialMemberCount() == 1, "First pass did not retain its incomplete anchor."); // 미완성 기준 확인
            yield return null; // 실제 다음 프레임 진행
            Require(Time.frameCount > previousFrame, "Partial-group passes did not span frames."); // 프레임 경계 확인
            Require(Pass() == 0, "Partial group merged before dynamic mutation."); // 둘째 예산 분할 확인
            Require(HasPartialAnchor() && PartialMemberCount() > 1 && PartialMemberCount() < 5, "Second pass did not retain selected incomplete materials."); // 부분 재료 보존 확인
            Debug.Log($"[ExperienceMergePartial] partial members={PartialMemberCount()} frame={Time.frameCount} XP={TotalExperience()}."); // 실제 부분 상태 기록
        }

        private IEnumerator VerifyInsertion() // 부분 진행 중 새 보석 삽입
        {
            yield return PreparePartial(); // 실제 부분 그룹 준비
            Spawn(anchorPosition - Vector3.right * 0.5f); // 부분 그룹 중 새로운 소형 보석
            Require(TotalExperience() == 25, "Insertion changed total XP unexpectedly."); // 새 보석 포함 총량
            yield return FinishGroup(25); // 실제 후속 검사와 보존 확인
            Debug.Log("[ExperienceMergePartial] dynamic insertion completed across actual frames without XP loss."); // 삽입 결과 기록
        }

        private IEnumerator VerifyMovement() // 이미 선택된 재료의 이동
        {
            yield return PreparePartial(); // 실제 부분 그룹 준비
            ExperiencePickup selected = FindSelectedMember(); // 기존 부분 재료 조회
            Vector3 prior = selected.transform.position; // 기존 위치 저장
            selected.transform.position = experience.transform.position + Vector3.right * 5f; // 흡수 범위 밖 다른 위치
            yield return null; // 실제 변경 프레임 진행
            Require(Pass() == 0 && CountValue(5) == 0 && TotalExperience() == 24, "Moved partial material merged or changed total XP."); // 이동 재검증 확인
            selected.transform.position = prior; // 유효 위치 복귀
            yield return FinishGroup(24); // 실제 재탐색과 보존 확인
            Debug.Log("[ExperienceMergePartial] moved partial material was rejected, then valid materials merged."); // 이동 결과 기록
        }

        private IEnumerator VerifyCollection() // 이미 선택된 재료의 실제 수집
        {
            yield return PreparePartial(); // 실제 부분 그룹 준비
            ExperiencePickup selected = FindSelectedMember(); // 기존 부분 재료 조회
            selected.transform.position = experience.transform.position; // 실제 머리 획득 위치
            float deadline = Time.realtimeSinceStartup + 3f; // 실제 수집 대기 제한
            while (experience.CurrentExperience == 0 && Time.realtimeSinceStartup < deadline) // 실제 수집 대기
            {
                yield return null; // 실제 보석 Update 진행
            }
            Require(experience.CurrentExperience == 1 && TotalExperience() == 24, "Actual partial-material collection lost or duplicated XP."); // 수집과 총량 확인
            Require(Pass() == 0 && CountValue(5) == 0 && TotalExperience() == 24, "Collected partial material merged or changed XP."); // 수집 재검증 확인
            Spawn(anchorPosition - Vector3.right * 0.5f); // 수집 재료 대체 보석
            Require(TotalExperience() == 25, "Replacement pickup XP was not added exactly once."); // 대체 보석 총량 확인
            yield return FinishGroup(25); // 실제 재탐색과 보존 확인
            Debug.Log("[ExperienceMergePartial] collected partial material was excluded; replacement merged without duplicate XP."); // 수집 결과 기록
        }

        private IEnumerator FinishGroup(int expectedTotal) // 검사 반복과 최종 경험치 확인
        {
            bool merged = false; // 병합 완료 여부
            for (int pass = 0; pass < 200 && !merged; pass++) // 제한된 실제 프레임 순회
            {
                yield return null; // 실제 다음 프레임 진행
                merged = Pass() > 0; // 실제 병합 여부 확인
                Require(TotalExperience() == expectedTotal, "Partial-group pass lost or duplicated XP."); // 매 검사 후 총량 확인
            }
            Require(merged && CountValue(5) == 1, "Valid partial group did not eventually merge."); // 최종 병합 확인
            yield return null; // 실제 지연 파괴 완료
            Require(TotalExperience() == expectedTotal, "Destroy boundary changed total XP."); // 프레임 경계 총량 확인
        }

        private int Pass() // 실제 병합 검사와 예산 확인
        {
            int merged = (int)managerType.GetMethod("ProcessMergePass").Invoke(manager, null); // 실제 제품 검사 호출
            int anchors = Metric("LastAnchorInspections"); // 실제 기준 검사 수
            int neighbors = Metric("LastNeighborInspections"); // 실제 이웃 검사 수
            int distances = Metric("LastBudgetedDistanceChecks"); // 실제 재료 거리 계산 수
            Require(anchors <= 1 && neighbors <= 8 && distances <= Budget, "A low-budget pass exceeded its configured limits."); // 검사와 확정 예산 확인
            Debug.Log($"[ExperienceMergePartial] frame={Time.frameCount} merged={merged} anchors={anchors} neighbors={neighbors} distances={distances} headChecks={Metric("LastCandidateHeadDistanceChecks")}."); // 실제 검사 계측 기록
            return merged; // 병합 결과 반환
        }

        private ExperiencePickup Spawn(Vector3 position) // 실제 소형 보석 생성
        {
            ExperiencePickup pickup = ExperiencePickup.Spawn(position, 1, null, experience); // 제품 보석 생성 경로
            tracked.Add(pickup); // 시험 대상 기록
            return pickup; // 생성 보석 반환
        }

        private int TotalExperience() // 실제 수집값과 보석값 합계
        {
            int total = experience.CurrentExperience; // 플레이어가 받은 경험치
            FieldInfo value = typeof(ExperiencePickup).GetField("experienceValue", Private); // 제품 보석값 필드
            FieldInfo collected = typeof(ExperiencePickup).GetField("collected", Private); // 수집 및 병합 소비 필드
            foreach (ExperiencePickup pickup in tracked) // 생성된 보석 순회
            {
                if (pickup != null && pickup.isActiveAndEnabled && !(bool)collected.GetValue(pickup)) // 미소비 보석 확인
                {
                    total += (int)value.GetValue(pickup); // 실제 보석값 합산
                }
            }
            return total; // 전체 경험치 반환
        }

        private int CountValue(int desired) // 유효 보석값별 개수
        {
            int count = 0; // 결과 수 초기화
            FieldInfo value = typeof(ExperiencePickup).GetField("experienceValue", Private); // 제품 보석값 필드
            FieldInfo collected = typeof(ExperiencePickup).GetField("collected", Private); // 소비 상태 필드
            foreach (ExperiencePickup pickup in tracked) // 시험 보석 순회
            {
                if (pickup != null && pickup.isActiveAndEnabled && !(bool)collected.GetValue(pickup) && (int)value.GetValue(pickup) == desired) // 활성 및 값 확인
                {
                    count++; // 보석 수 증가
                }
            }
            return count; // 보석 수 반환
        }

        private object ScanState() // 실제 부분 검사 상태 조회
        {
            return managerType.GetField("scan", Private).GetValue(manager); // 관리자 부분 상태
        }

        private bool HasPartialAnchor() // 부분 기준 보석 확인
        {
            object scan = ScanState(); // 실제 부분 상태
            return (bool)scan.GetType().GetField("HasAnchor", Private).GetValue(scan); // 기준 유지 여부
        }

        private IList PartialMembers() // 실제 선택 재료 목록
        {
            object scan = ScanState(); // 실제 부분 상태
            return (IList)scan.GetType().GetField("Members", Private).GetValue(scan); // 재료 식별자 목록
        }

        private int PartialMemberCount() // 실제 선택 재료 수
        {
            return PartialMembers().Count; // 부분 목록 크기
        }

        private ExperiencePickup FindSelectedMember() // 기준을 제외한 부분 재료 조회
        {
            int id = (int)PartialMembers()[1]; // 선택된 두 번째 재료 번호
            foreach (ExperiencePickup pickup in tracked) // 실제 시험 보석 순회
            {
                if (pickup != null && pickup.GetInstanceID() == id) // 같은 보석 확인
                {
                    return pickup; // 선택 보석 반환
                }
            }
            Require(false, "Selected partial material is missing."); // 구성 오류 기록
            return null; // 오류 뒤 반환값
        }

        private void SetField(string name, object value) // 관리자 직렬화 설정
        {
            FieldInfo field = managerType.GetField(name, Private); // 실제 필드 조회
            Require(field != null, "Controller field is missing: " + name); // 계약 확인
            field.SetValue(manager, value); // 시험 조건 적용
        }

        private int Metric(string name) // 실제 계측값 조회
        {
            PropertyInfo property = managerType.GetProperty(name); // 제품 계측 속성 조회
            Require(property != null, "Controller metric is missing: " + name); // 계측 계약 확인
            return (int)property.GetValue(manager); // 실제 계측값 반환
        }

        private static void Require(bool condition, string message) // 실패 조건 처리
        {
            if (!condition) // 실패 확인
            {
                AutomatedExperienceMergePartialVerification.Finish(1, message); // 실패 결과와 종료
                throw new InvalidOperationException(message); // 코루틴 중단
            }
        }
    }
}
