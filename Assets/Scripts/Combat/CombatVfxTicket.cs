using ProjectEpsilon.Core; // 실제 게임 종료 상태
using UnityEngine; // Unity 객체 수명
namespace ProjectEpsilon.Combat // 전투 시각 영역
{ // 영역 시작
    public sealed class CombatVfxTicket : MonoBehaviour // 실제 표시 수 집계
    { // 집계 시작
        private bool registered; // 실제 집계 등록 여부
        private bool priority; // 반응과 플레이어 표시 여부
        public void Register(bool isPriority) // 실제 표시 집계 등록
        { // 함수 시작
            priority = isPriority; // 실제 표시 종류 저장
            registered = true; // 실제 집계 등록
        } // 함수 종료
        private void Update() // 전투 종료 시 표시 정리
        { // 함수 시작
            if (GameManager.Instance != null && (GameManager.Instance.CurrentState == GameState.Defeated || GameManager.Instance.CurrentState == GameState.Victory)) // 실제 전투 종료 확인
            { // 조건 시작
                Destroy(gameObject); // 정지된 종료 화면의 표시 제거
            } // 조건 종료
        } // 함수 종료
        private void OnDisable() // 실제 표시 제거 집계
        { // 함수 시작
            if (registered) // 실제 집계 등록 확인
            { // 조건 시작
                CombatVfx.Release(priority); // 실제 표시 수 반환
                registered = false; // 중복 반환 방지
            } // 조건 종료
        } // 함수 종료
    } // 집계 종료
} // 영역 종료
