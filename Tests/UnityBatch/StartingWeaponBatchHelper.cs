using ProjectEpsilon.Core; // 게임 시작 상태
using ProjectEpsilon.Progression; // 시작 무기 선택
using ProjectEpsilon.UI; // 선택 화면
using UnityEngine; // 씬 객체 검색
using UnityEngine.UI; // 실제 버튼 입력

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    internal static class StartingWeaponBatchHelper // 시작 선택 공통 처리
    {
        public static bool HandleStartingSelection(int index = 0) // 시작 무기 버튼 자동 선택
        {
            GameManager game = GameManager.Instance; // 현재 게임 관리자
            if (game == null || game.CurrentState != GameState.Starting) // 시작 상태 확인
            {
                return false; // 일반 배치 진행
            }
            RunFrontendController frontend = RunFrontendController.Instance; // 실제 전용 시작 화면
            if (frontend != null && frontend.Stage != FrontendStage.Weapons) // 실제 출격 단계 확인
            {
                string name = frontend.Stage == FrontendStage.Main ? "FrontendStartButton" : frontend.Stage == FrontendStage.Characters ? "FrontendCharactersNextButton" : frontend.Stage == FrontendStage.Maps ? "FrontendMapsNextButton" : null; // 실제 다음 단계 버튼
                GameObject next = string.IsNullOrEmpty(name) ? null : GameObject.Find(name); // 실제 다음 단계 조회
                Button navigation = next == null ? null : next.GetComponent<Button>(); // 실제 다음 단계 버튼 조회
                if (navigation != null && navigation.IsActive() && navigation.interactable) // 실제 버튼 사용 가능 확인
                {
                    navigation.onClick.Invoke(); // 실제 시작 단계 이동
                }
                return true; // 실제 화면 전환 대기
            }
            StartingWeaponSelection selection = Object.FindFirstObjectByType<StartingWeaponSelection>(); // 시작 선택기 검색
            if (selection == null || !selection.IsSelecting) // 선택 화면 준비 확인
            {
                return true; // 시작 준비 대기
            }
            LevelUpPanelController panel = Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 공용 선택 화면
            if (panel == null) // 화면 연결 확인
            {
                return true; // 화면 준비 대기
            }
            string buttonName = index == 1 ? "candidateButton02" : "candidateButton01"; // 실제 시작 후보 버튼 이름
            var field = typeof(LevelUpPanelController).GetField(buttonName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); // 실제 후보 버튼 필드
            Button button = field == null ? null : field.GetValue(panel) as Button; // 첫 후보 버튼
            if (button != null && button.IsActive() && button.interactable) // 실제 버튼 활성 확인
            {
                button.onClick.Invoke(); // 실제 시작 무기 선택
            }
            return true; // 다음 프레임부터 배치 진행
        }
    }
}
