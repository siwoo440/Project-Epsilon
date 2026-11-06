using System.Collections.Generic; // 보석 및 후보 목록
using ProjectEpsilon.Core; // 게임 상태
using UnityEngine; // Unity 기능
using UnityEngine.SceneManagement; // 씬 변경 감지

namespace ProjectEpsilon.Progression // 진행 영역
{ // 영역 시작
    public sealed class ExperienceGemMergeController : MonoBehaviour // 보석 자동 병합 관리자
    { // 클래스 시작
        [SerializeField] private int mergeThreshold = 100; // 병합 발동 개수 임시값
        [SerializeField] private float mergeDistance = 1.5f; // 인접 거리 임시값
        [SerializeField] private float mergeInterval = 1f; // 게임 시간 검사 간격 임시값
        [SerializeField] private int maximumGroupsPerPass = 4; // 검사당 병합 수 임시값
        [SerializeField] private int maximumAnchorsPerPass = 32; // 검사당 기준 보석 수 임시값
        [Min(17)] [SerializeField] private int maximumNeighborChecksPerPass = 512; // 검사당 후보 및 거리 연산 임시값
        private readonly ExperienceMergeScanState scan = new ExperienceMergeScanState(); // 부분 그룹 검사 상태
        private float elapsed; // 마지막 검사 이후 게임 시간
        private bool sceneSubscribed; // 씬 변경 구독 여부
        public int CompletedPassCount { get; private set; } // 진행 중 실행한 검사 횟수
        public int LastAnchorInspections { get; private set; } // 마지막 기준 보석 검사 수
        public int LastNeighborInspections { get; private set; } // 마지막 인접 후보 검사 수
        public int LastBudgetedDistanceChecks { get; private set; } // 마지막 예산 대상 거리 계산 수
        public int LastCandidateHeadDistanceChecks { get; private set; } // 마지막 후보 머리 거리 계산 수

        private void OnEnable() // 씬 변경 감시 연결
        { // 메서드 시작
            if (Application.isPlaying && !sceneSubscribed) // 실행 중 중복 구독 확인
            { // 조건 시작
                SceneManager.sceneLoaded += HandleSceneLoaded; // 새 씬 초기화 연결
                sceneSubscribed = true; // 구독 상태 저장
            } // 조건 끝
        } // 메서드 끝

        private void Update() // 게임 시간에 따른 병합 검사
        { // 메서드 시작
            GameManager game = GameManager.Instance; // 현재 게임 관리자
            if (!isActiveAndEnabled || game == null || !game.IsPlaying) // 활성 및 진행 상태 확인
            { // 조건 시작
                return; // 정지 중 병합 금지
            } // 조건 끝
            elapsed += Time.deltaTime; // 경과 게임 시간 누적
            if (elapsed < Mathf.Max(0.1f, mergeInterval)) // 검사 간격 확인
            { // 조건 시작
                return; // 다음 검사 대기
            } // 조건 끝
            elapsed = 0f; // 검사 시간 초기화
            ProcessMergePass(); // 보석 병합 검사
        } // 메서드 끝

        private void OnDisable() // 관리자 비활성 처리
        { // 메서드 시작
            if (sceneSubscribed) // 씬 변경 구독 확인
            { // 조건 시작
                SceneManager.sceneLoaded -= HandleSceneLoaded; // 씬 변경 연결 해제
                sceneSubscribed = false; // 구독 상태 해제
            } // 조건 끝
            scan.Reset(); // 부분 그룹 상태 해제
            elapsed = 0f; // 검사 시간 초기화
        } // 메서드 끝

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) // 새 씬의 보석 상태 초기화
        { // 메서드 시작
            scan.Reset(); // 이전 씬 보석 번호 제거
            elapsed = 0f; // 새 씬 검사 시간 초기화
        } // 메서드 끝

        public int ProcessMergePass() // 활성 보석 병합 한 차례 처리
        { // 메서드 시작
            GameManager game = GameManager.Instance; // 현재 게임 관리자
            if (!isActiveAndEnabled || game == null || !game.IsPlaying) // 활성 및 진행 상태 확인
            { // 조건 시작
                return 0; // 정지 중 병합 금지
            } // 조건 끝
            CompletedPassCount++; // 진행 중 검사 횟수 증가
            LastAnchorInspections = 0; // 이전 기준 검사 횟수 초기화
            LastNeighborInspections = 0; // 이전 후보 검사 횟수 초기화
            LastBudgetedDistanceChecks = 0; // 이전 재료 거리 횟수 초기화
            LastCandidateHeadDistanceChecks = 0; // 이전 머리 거리 횟수 초기화
            if (maximumNeighborChecksPerPass < ExperienceGemMergeRules.MinimumNeighborChecks + 9) // 재검증을 포함한 최소 예산 확인
            { // 조건 시작
                scan.Reset(); // 유효하지 않은 부분 그룹 초기화
                return 0; // 예산보다 많은 거리 검사 방지
            } // 조건 끝
            ExperiencePickup[] found = FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None); // 현재 활성 보석 검색
            List<ExperiencePickup> pickups = new List<ExperiencePickup>(found.Length); // 소비되지 않은 보석 목록
            List<ExperienceMergePoint> points = new List<ExperienceMergePoint>(found.Length); // 검사 후보 스냅샷
            Camera camera = Camera.main; // 현재 게임 카메라
            foreach (ExperiencePickup pickup in found) // 활성 보석 순회
            { // 반복 시작
                if (pickup == null || !pickup.IsMergeAvailable) // 파괴 예약 및 비활성 확인
                { // 조건 시작
                    continue; // 소비된 보석 제외
                } // 조건 끝
                SnakeExperience receiver = pickup.MergeReceiver; // 보석 수신자 조회
                Vector3 position = pickup.transform.position; // 현재 보석 위치
                float distance = receiver == null ? 0f : (receiver.transform.position - position).sqrMagnitude; // 머리 거리 제곱
                if (receiver != null) // 거리 계산 여부 확인
                { // 조건 시작
                    LastCandidateHeadDistanceChecks++; // 머리 거리 검사 횟수 증가
                } // 조건 끝
                float attraction = pickup.MergeAttractionRange; // 자동 흡수 거리
                bool available = receiver != null && distance > attraction * attraction; // 수신자 및 흡수 상태 확인
                bool offscreen = camera != null && IsOutsideCamera(camera, position); // 화면 밖 여부
                pickups.Add(pickup); // 현재 보석 보관
                points.Add(new ExperienceMergePoint(pickup.GetInstanceID(), pickup.ExperienceValue, position.x, position.y, distance, offscreen, available, receiver)); // 보석 스냅샷 저장
            } // 반복 끝
            int neighborBudget = maximumNeighborChecksPerPass; // 재검증을 포함한 거리 검사 예산
            int groupLimit = Mathf.Min(Mathf.Max(1, maximumGroupsPerPass), (neighborBudget - ExperienceGemMergeRules.MinimumNeighborChecks) / 9); // 그룹 재검증 예산 확보
            int planningBudget = neighborBudget - groupLimit * 9; // 계획 단계 후보 검사 예산
            List<ExperienceMergeGroup> groups = ExperienceGemMergeRules.Plan(points, Mathf.Max(1, mergeThreshold), Mathf.Max(0.01f, mergeDistance), groupLimit, Mathf.Max(1, maximumAnchorsPerPass), planningBudget, scan, out int inspected, out int compared, out int anchorsInspected); // 예산 내 병합 계획
            LastAnchorInspections = anchorsInspected; // 실제 기준 검사 수
            LastNeighborInspections = inspected; // 계획 단계 후보 검사 수
            LastBudgetedDistanceChecks = compared; // 계획 단계 거리 계산 수
            int merged = 0; // 완료된 그룹 수
            foreach (ExperienceMergeGroup group in groups) // 계획된 그룹 순회
            { // 반복 시작
                int commitComparisons = 0; // 그룹 재검증 거리 횟수
                bool valid = CanCommit(group, pickups, ref commitComparisons); // 재료 현재 상태 재확인
                LastBudgetedDistanceChecks += commitComparisons; // 실제 재검증 거리 횟수 반영
                if (!valid) // 그룹 유효성 확인
                { // 조건 시작
                    continue; // 변경된 그룹 생략
                } // 조건 끝
                ExperiencePickup survivor = pickups[group.Indices[0]]; // 결과를 보존할 보석
                survivor.PromoteForMerge(group.ResultValue); // 결과 등급 및 경험치 적용
                for (int index = 1; index < group.Indices.Length; index++) // 소모 재료 순회
                { // 반복 시작
                    pickups[group.Indices[index]].ConsumeForMerge(); // 즉시 소비 표식과 제거 예약
                } // 반복 끝
                merged++; // 병합 수 증가
            } // 반복 끝
            return merged; // 완료 그룹 수 반환
        } // 메서드 끝

        private bool CanCommit(ExperienceMergeGroup group, List<ExperiencePickup> pickups, ref int comparisons) // 병합 직전 재료 검증
        { // 메서드 시작
            if (group.Indices == null || group.Indices.Length == 0) // 빈 그룹 확인
            { // 조건 시작
                return false; // 빈 그룹 거절
            } // 조건 끝
            ExperiencePickup first = pickups[group.Indices[0]]; // 기준 보석 조회
            if (first == null || !first.IsMergeAvailable) // 기준 보석 수명 확인
            { // 조건 시작
                return false; // 수집 및 소멸 대상 제외
            } // 조건 끝
            SnakeExperience receiver = first.MergeReceiver; // 기준 경험치 수신자
            if (receiver == null) // 수신자 확인
            { // 조건 시작
                return false; // 수신자 없는 보석 제외
            } // 조건 끝
            int expectedValue = group.ResultValue == 5 ? 1 : 5; // 재료 경험치
            int expectedCount = group.ResultValue == 5 ? 5 : 4; // 재료 개수
            if (group.Indices.Length != expectedCount) // 그룹 크기 확인
            { // 조건 시작
                return false; // 잘못된 그룹 제외
            } // 조건 끝
            float radius = Mathf.Max(0.01f, mergeDistance); // 인접 거리 임시값
            float squaredRadius = radius * radius; // 인접 거리 제곱
            foreach (int index in group.Indices) // 그룹 재료 순회
            { // 반복 시작
                ExperiencePickup pickup = pickups[index]; // 현재 재료 조회
                if (pickup == null || !pickup.IsMergeAvailable || pickup.MergeReceiver != receiver || pickup.ExperienceValue != expectedValue) // 수명·수신자·등급 확인
                { // 조건 시작
                    return false; // 변경된 재료 제외
                } // 조건 끝
                comparisons++; // 머리와 재료 거리 확인 횟수
                if (!pickup.IsOutsideAttraction()) // 현재 흡수 범위 확인
                { // 조건 시작
                    return false; // 흡수 중 재료 제외
                } // 조건 끝
                if (pickup != first) // 기준과 다른 재료 확인
                { // 조건 시작
                    comparisons++; // 재료 간 거리 확인 횟수
                    if ((pickup.transform.position - first.transform.position).sqrMagnitude > squaredRadius) // 현재 인접 거리 확인
                    { // 조건 시작
                        return false; // 이동한 재료 제외
                    } // 조건 끝
                } // 조건 끝
            } // 반복 끝
            return true; // 안전한 병합 허용
        } // 메서드 끝

        private static bool IsOutsideCamera(Camera camera, Vector3 position) // 화면 밖 보석 확인
        { // 메서드 시작
            Vector3 view = camera.WorldToViewportPoint(position); // 화면 좌표 변환
            return view.z < 0f || view.x < 0f || view.x > 1f || view.y < 0f || view.y > 1f; // 카메라 밖 여부
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
