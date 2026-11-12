using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Player; // 실제 시작 능력치
using UnityEngine; // Unity 표시 기능
using ProjectEpsilon.UI; // 실제 메뉴 단계

namespace ProjectEpsilon.Progression // 영구 성장 영역
{
    public sealed class MetaProgressionMenu : MonoBehaviour // 시작 화면 시험용 영구 성장 메뉴
    {
        private static readonly string[] Labels = // 실제 성장 노드 안내
        {
            "생존 체력 +4%", "생존 회복 +5%", "생존 마지막 몸통 속도", // 실제 생존 계열
            "기동 회전 +3%", "기동 스태미나 +5%", "기동 저스태미나 효과", // 실제 기동 계열
            "전투 공격력 +2%", "전투 공격 속도 +2%", "전투 3등급 이상 피해", // 실제 전투 계열
            "성장 경험치 +3%", "성장 획득 범위 +5%", "성장 재선택 1회" // 실제 성장 계열
        };
        private MetaProgressionController controller; // 실제 진행 관리자
        public bool IsOpen { get; private set; } // 실제 메뉴 표시 상태
        public void Configure(MetaProgressionController source) // 실제 관리자 연결
        {
            controller = source; // 실제 성장 관리자 저장
        }
        public void Open() // 실제 메뉴에서 성장 열기
        {
            if (controller != null && controller.Service != null && controller.Service.IsAvailable && controller.Service.FinishedFirstRun) // 실제 해금과 저장 확인
            {
                IsOpen = true; // 실제 성장 메뉴 표시
            }
        }
        private void OnGUI() // 실제 시작 화면 표시
        {
            Font previous = GUI.skin.font; // 실제 이전 공용 글꼴
            Font current = RunFrontendController.Instance == null ? null : RunFrontendController.Instance.DisplayFont; // 실제 화면 한글 글꼴
            if (current != null) // 실제 한글 글꼴 확인
            {
                GUI.skin.font = current; // 실제 성장 화면 글꼴 적용
            }
            try // 실제 화면 표시 중 공용 설정 보호
            {
                DrawMenu(); // 실제 영구 성장 화면 표시
            }
            finally // 실제 화면 오류와 관계없는 복원
            {
                GUI.skin.font = previous; // 실제 다른 화면 글꼴 복원
            }
        }
        private void DrawMenu() // 실제 영구 성장 내용 표시
        {
            GameManager game = GameManager.Instance; // 실제 게임 상태
            bool mainMenu = RunFrontendController.Instance != null && RunFrontendController.Instance.IsMainMenu; // 실제 메인 메뉴 상태
            bool starting = game == null ? mainMenu : game.CurrentState == GameState.Starting; // 실제 독립 메뉴 또는 기존 시작 상태
            if (controller == null || controller.Service == null || !starting || RunFrontendController.Instance != null && !mainMenu) // 실제 시작 상태 확인
            {
                IsOpen = false; // 다른 화면에서 메뉴 닫기
                return; // 실제 시작 외 표시 차단
            }
            if (!controller.Service.IsAvailable) // 실제 저장 오류 확인
            {
                GUI.Label(new Rect(16f, 16f, 400f, 35f), "성장 저장 오류: 원본을 보존했습니다"); // 저장 오류 안내
                return; // 잘못된 저장 변경 차단
            }
            if (!controller.Service.FinishedFirstRun) // 실제 첫 런 종료 여부 확인
            {
                return; // 첫 런 전 성장 메뉴 잠금
            }
            if (!IsOpen) // 실제 메뉴 닫힘 확인
            {
                if (GUI.Button(new Rect(16f, 16f, 180f, 36f), "영구 성장")) // 실제 성장 메뉴 열기
                {
                    IsOpen = true; // 실제 메뉴 표시
                }
                return; // 닫힌 메뉴 표시 종료
            }
            float width = Mathf.Min(760f, Mathf.Max(320f, Screen.width - 32f)); // 실제 화면 너비
            float height = Mathf.Min(620f, Mathf.Max(430f, Screen.height - 32f)); // 실제 화면 높이
            Rect frame = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height); // 실제 메뉴 영역
            GUI.Box(frame, "영구 성장  |  코어 " + controller.Service.Cores); // 실제 보유 재화 표시
            float rowHeight = (height - 95f) / Labels.Length; // 실제 노드 행 높이
            for (int index = 0; index < Labels.Length; index++) // 실제 열두 노드 순회
            {
                MetaNode node = (MetaNode)index; // 실제 노드 종류
                int level = controller.Service.GetLevel(node); // 실제 노드 단계
                int cost = controller.Service.GetNextCost(node); // 실제 다음 비용
                float y = frame.y + 34f + index * rowHeight; // 실제 노드 행 위치
                GUI.Label(new Rect(frame.x + 16f, y, width - 175f, rowHeight), Labels[index] + "  단계 " + level); // 실제 효과와 단계 안내
                bool previous = GUI.enabled; // 기존 버튼 상태 보존
                GUI.enabled = controller.Service.CanPurchase(node); // 실제 구매 가능 여부
                if (GUI.Button(new Rect(frame.xMax - 150f, y, 130f, rowHeight - 2f), cost < 0 ? "최대" : "강화 " + cost)) // 실제 성장 구매 버튼
                {
                    if (controller.Service.TryPurchase(node)) // 실제 저장과 구매 확인
                    {
                        RefreshStartingStats(); // 실제 시작 능력치 갱신
                    }
                }
                GUI.enabled = previous; // 기존 버튼 상태 복구
            }
            if (GUI.Button(new Rect(frame.x + 16f, frame.yMax - 48f, 150f, 32f), "무료 초기화")) // 실제 무료 초기화 버튼
            {
                if (controller.Service.TryRefundAll()) // 실제 저장과 재화 반환 확인
                {
                    RefreshStartingStats(); // 실제 시작 능력치 복구
                }
            }
            if (GUI.Button(new Rect(frame.xMax - 166f, frame.yMax - 48f, 150f, 32f), "닫기")) // 실제 성장 메뉴 닫기
            {
                IsOpen = false; // 실제 메뉴 종료
            }
        }
        private static void RefreshStartingStats() // 실제 시작 능력치 갱신
        {
            CharacterRunInitializer initializer = FindFirstObjectByType<CharacterRunInitializer>(); // 실제 캐릭터 초기화기
            if (initializer != null && CharacterRunInitializer.ActiveCharacter != null) // 실제 시작 캐릭터 확인
            {
                initializer.TryApply(CharacterRunInitializer.ActiveCharacter); // 실제 시작 체력 재적용
            }
            SnakeStamina stamina = FindFirstObjectByType<SnakeStamina>(); // 실제 스태미나 조회
            stamina?.ResetStamina(); // 실제 새 최대치 회복
        }
    }
}
