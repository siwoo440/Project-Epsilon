using ProjectEpsilon.Combat; // 대상 상태 사용
using ProjectEpsilon.Core; // 게임 상태 사용
using UnityEngine; // Unity 기능 사용
using UnityEngine.InputSystem; // 키보드 입력 사용

namespace ProjectEpsilon.Debugging // Debug 영역
{
    public sealed class EnemyStatusDebugControls : MonoBehaviour // Enemy 상태 Debug 입력
    {
        [SerializeField] private Transform searchOrigin; // 검색 중심 참조
        [Min(1f)] [SerializeField] private float searchRange = 100f; // 검색 범위

        public Transform SearchOrigin => searchOrigin; // 검색 중심 반환

        public void Configure(Transform origin, float range = 100f) // Debug 참조 구성
        {
            searchOrigin = origin; // 검색 중심 저장
            searchRange = Mathf.Max(1f, range); // 검색 범위 저장
        }

        private void Update() // 입력 갱신
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 전투 진행 여부 확인
            {
                return; // 입력 중단
            }

            Keyboard keyboard = Keyboard.current; // 현재 키보드 조회

            if (keyboard == null || searchOrigin == null) // 입력 가능 여부 확인
            {
                return; // 입력 생략
            }

            if (keyboard.f5Key.wasPressedThisFrame) // F5 입력 확인
            {
                ApplySlowToClosest(); // 가까운 적 감속
            }
            else if (keyboard.f6Key.wasPressedThisFrame) // F6 입력 확인
            {
                ApplyStopToClosest(); // 가까운 적 정지
            }
            else if (keyboard.f7Key.wasPressedThisFrame) // F7 입력 확인
            {
                ClearAllStatuses(); // 모든 상태 해제
            }
        }

        private void ApplySlowToClosest() // 가까운 적 감속 적용
        {
            WeaponTargetStatusController statuses = FindClosestStatuses(); // 가까운 상태 관리자 조회

            if (statuses == null) // 대상 없음 확인
            {
                Debug.LogWarning("[Project Epsilon] Day16 slow debug target not found."); // 대상 누락 경고
                return; // 적용 중단
            }

            statuses.ApplySlow(1, 0.5f, 3f); // 50퍼센트 감속 적용
            Debug.Log("[Project Epsilon] Day16 slow 50% for 3 seconds applied."); // 적용 로그 출력
        }

        private void ApplyStopToClosest() // 가까운 적 정지 적용
        {
            WeaponTargetStatusController statuses = FindClosestStatuses(); // 가까운 상태 관리자 조회

            if (statuses == null) // 대상 없음 확인
            {
                Debug.LogWarning("[Project Epsilon] Day16 stop debug target not found."); // 대상 누락 경고
                return; // 적용 중단
            }

            statuses.ApplyStop(1, 1f); // 1초 정지 적용
            Debug.Log("[Project Epsilon] Day16 stop for 1 second applied."); // 적용 로그 출력
        }

        private void ClearAllStatuses() // 전체 상태 해제
        {
            WeaponTargetStatusController[] allStatuses = Object.FindObjectsByType<WeaponTargetStatusController>(FindObjectsInactive.Include, FindObjectsSortMode.None); // 전체 상태 관리자 조회

            for (int index = 0; index < allStatuses.Length; index++) // 모든 상태 관리자 순회
            {
                allStatuses[index].ClearAll(); // 대상 상태 해제
            }

            Debug.Log("[Project Epsilon] Day16 cleared statuses from " + allStatuses.Length + " targets."); // 해제 결과 출력
        }

        private WeaponTargetStatusController FindClosestStatuses() // 가까운 상태 관리자 조회
        {
            WeaponTarget target = WeaponTarget.FindClosest(searchOrigin.position, searchRange); // 가까운 공격 대상 조회

            if (target == null) // 대상 없음 확인
            {
                return null; // 상태 없음 반환
            }

            WeaponTargetStatusController statuses = target.GetComponent<WeaponTargetStatusController>(); // 상태 관리자 조회

            if (statuses == null) // 상태 관리자 없음 확인
            {
                statuses = target.gameObject.AddComponent<WeaponTargetStatusController>(); // 상태 관리자 추가
            }

            return statuses; // 상태 관리자 반환
        }

    }
}
