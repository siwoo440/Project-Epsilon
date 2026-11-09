using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 보스 출현기
using ProjectEpsilon.Progression; // 실제 맵 해금 진행
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.World // 맵 영역
{ // 영역 시작
    [DefaultExecutionOrder(150)] // 시작 화면보다 먼저 기본 맵 연결
    public sealed class PrototypeMapSelection : MonoBehaviour // 실제 시작 맵 선택
    { // 클래스 시작
        [SerializeField] private PrototypeMapProfile[] profiles; // 맵별 시험 데이터 목록
        private int selectedIndex; // 현재 맵 번호
        private bool hasSelection; // 실제 초기 맵 선택 여부
        public int ProfileCount => profiles == null ? 0 : profiles.Length; // 실제 맵 개수
        public int SelectedIndex => selectedIndex; // 현재 맵 번호
        public PrototypeMapProfile GetProfile(int index) // 실제 맵 후보 조회
        { // 메서드 시작
            return profiles != null && index >= 0 && index < profiles.Length ? profiles[index] : null; // 안전한 맵 반환
        } // 메서드 끝
        public PrototypeMapProfile SelectedProfile => profiles != null && selectedIndex >= 0 && selectedIndex < profiles.Length ? profiles[selectedIndex] : null; // 현재 맵 데이터
        private void Start() // 기본 맵 연결
        { // 메서드 시작
            if (!hasSelection) // 이전에 적용한 재도전 맵 확인
            { // 조건 시작
                SelectMap(0); // 첫 번째 맵 기본 적용
            } // 조건 끝
        } // 메서드 끝
        public bool SelectMap(int index) // 시작 전 실제 맵 선택
        { // 메서드 시작
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Starting || profiles == null || index < 0 || index >= profiles.Length || profiles[index] == null) // 시작 상태와 데이터 확인
            { // 조건 시작
                return false; // 잘못된 선택 차단
            } // 조건 끝
            if (MetaProgressionController.Instance != null && !MetaProgressionController.Instance.IsMapUnlocked(index)) // 실제 맵 해금 확인
            { // 조건 시작
                return false; // 미해금 맵 선택 차단
            } // 조건 끝
            PrototypeChunkMap map = GetComponent<PrototypeChunkMap>(); // 같은 관리자의 청크 맵
            PrototypeBossEncounter boss = GetComponent<PrototypeBossEncounter>(); // 같은 관리자의 보스 출현기
            if (map == null || boss == null || !map.ApplyProfile(profiles[index]) || !boss.ApplyProfile(profiles[index])) // 실제 맵과 보스 연결
            { // 조건 시작
                return false; // 필수 구성 누락 차단
            } // 조건 끝
            selectedIndex = index; // 선택한 맵 저장
            hasSelection = true; // 실제 맵 선택 완료 기록
            return true; // 실제 맵 선택 완료
        } // 메서드 끝
        public bool CycleMap(int direction) // 시작 화면 맵 순환
        { // 메서드 시작
            if (direction == 0 || ProfileCount == 0) // 유효 입력과 목록 확인
            { // 조건 시작
                return false; // 잘못된 순환 차단
            } // 조건 끝
            for (int offset = 1; offset <= ProfileCount; offset++) // 실제 해금 맵 순회
            { // 반복 시작
                int step = direction > 0 ? offset : -offset; // 실제 순환 방향
                int next = (selectedIndex + step + ProfileCount) % ProfileCount; // 순환 맵 번호
                if (SelectMap(next)) // 실제 맵 선택 확인
                { // 조건 시작
                    return true; // 실제 해금 맵 적용
                } // 조건 끝
            } // 반복 끝
            return false; // 사용 가능한 맵 부재
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
