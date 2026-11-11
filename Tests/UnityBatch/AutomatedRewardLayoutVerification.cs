using System.Collections.Generic; // 후보 목록 형식
using System.Reflection; // 실제 버튼 조회
using ProjectEpsilon.Progression; // 무기 보상 후보
using ProjectEpsilon.UI; // 보상 화면
using UnityEngine.UI; // 실제 버튼과 이미지
using System; // 예외 형식
using System.Collections; // 코루틴 형식
using ProjectEpsilon.Combat; // 실제 속성 효과
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedRewardLayoutVerification // 실제 보상 카드 배치 시험
    {
        private const string Key = "EpsilonRewardLayoutVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedRewardLayoutVerification() // 도메인 재시작 처리
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
            if (EditorApplication.timeSinceStartup - started > 90d) // 시험 제한 시간 확인
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
            player.gameObject.AddComponent<RewardLayoutRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[RewardLayoutVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class RewardLayoutRunner : MonoBehaviour // 실제 보상 카드 시험 실행기
    {
        private IEnumerator Start() // 실제 보상 배치 시험
        {
            LevelUpPanelController panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 보상 화면 조회
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 시작 무기 조회
            Require(panel != null && weapon != null, "Reward panel or weapon missing."); // 실제 시험 구성 확인
            WeaponData fire = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlaster.asset"); // 실제 화염 포탑 데이터
            Require(fire != null, "Fire turret missing."); // 실제 화염 포탑 확인
            var candidates = new List<WeaponRewardCandidate> { new WeaponRewardCandidate(weapon, 1), new WeaponRewardCandidate(fire, 1), new WeaponRewardCandidate(weapon, 1) }; // 실제 속성별 후보 준비
            panel.ShowStartingWeapons(candidates); // 실제 시작 보상 화면 열기
            Canvas.ForceUpdateCanvases(); // 실제 화면 배치 갱신
            RectTransform panelRect = (RectTransform)panel.transform; // 실제 보상 패널 영역
            Require(Mathf.Abs(panelRect.anchorMin.x - 0.1f) < 0.001f && Mathf.Abs(panelRect.anchorMax.x - 0.9f) < 0.001f, "Reward panel width is not responsive."); // 실제 화면 기준 패널 너비 확인
            Button[] buttons = new Button[3]; // 실제 세 카드 버튼
            for (int index = 0; index < buttons.Length; index++) // 실제 카드 순회
            {
                FieldInfo field = typeof(LevelUpPanelController).GetField("candidateButton0" + (index + 1), BindingFlags.Instance | BindingFlags.NonPublic); // 실제 후보 필드 조회
                buttons[index] = field.GetValue(panel) as Button; // 실제 카드 버튼 저장
                Require(buttons[index] != null && buttons[index].interactable, "Reward button missing or disabled."); // 실제 카드 입력 확인
                RectTransform image = buttons[index].transform.Find("WeaponArtwork") as RectTransform; // 실제 무기 이미지 영역
                Require(image != null && image.gameObject.activeSelf && Mathf.Abs(image.sizeDelta.x - 96f) < 0.001f && Mathf.Abs(image.sizeDelta.y - 96f) < 0.001f, "Reward artwork is not double size."); // 기존 사십팔 대비 두 배 크기 확인
                Require(!image.GetComponent<Image>().raycastTarget, "Reward artwork blocks button input."); // 이미지 입력 차단 방지 확인
            }
            Require(buttons[1].transform.Find("WeaponArtwork").GetComponent<Image>().color == fire.ArtworkTint, "Reward icon did not show turret attribute color."); // 실제 화염 포탑 후보 색상 확인
            RectTransform first = (RectTransform)buttons[0].transform; // 실제 첫 카드 영역
            RectTransform second = (RectTransform)buttons[1].transform; // 실제 둘째 카드 영역
            RectTransform third = (RectTransform)buttons[2].transform; // 실제 셋째 카드 영역
            float width = (panelRect.anchorMax.x - panelRect.anchorMin.x) * (third.anchorMax.x - first.anchorMin.x); // 전체 화면 대비 카드 묶음 너비
            Require(Mathf.Abs(width - 0.704f) < 0.01f, "Reward cards do not span about seventy percent of screen width."); // 전체 화면 대비 약 칠십 퍼센트 확인
            Require(first.anchorMax.x < second.anchorMin.x && second.anchorMax.x < third.anchorMin.x && first.anchorMin.y < first.anchorMax.y, "Reward cards overlap or lack height."); // 실제 카드 간격과 높이 확인
            panel.ShowEliteRewards(new[] { "A", "B", "C" }, new[] { true, true, true }); // 실제 엘리트 보상 전환
            Require(!buttons[0].transform.Find("WeaponArtwork").gameObject.activeSelf, "Weapon artwork remained on elite reward."); // 엘리트 화면의 무기 그림 숨김 확인
            yield return null; // 실제 프레임 대기
            AutomatedRewardLayoutVerification.Finish(0, "Responsive reward card group, doubled images and elite transition."); // 전체 성공 기록
        }
        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedRewardLayoutVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
