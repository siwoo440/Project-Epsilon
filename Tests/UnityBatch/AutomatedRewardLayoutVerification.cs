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
            Require(weapon.Artwork != null && fire.Artwork != null && weapon.Artwork != fire.Artwork, "Different turret types share artwork."); // 근접과 직선 포탑 구분 확인
            var candidates = new List<WeaponRewardCandidate> { new WeaponRewardCandidate(weapon, 1), new WeaponRewardCandidate(fire, 1), new WeaponRewardCandidate(weapon, 1) }; // 실제 속성별 후보 준비
            panel.ShowStartingWeapons(candidates); // 실제 시작 보상 화면 열기
            Canvas.ForceUpdateCanvases(); // 실제 화면 배치 갱신
            RectTransform panelRect = (RectTransform)panel.transform; // 실제 보상 패널 영역
            Require(panelRect.parent != null && panelRect.parent.name == "GameplayViewport", "Reward panel is not inside the gameplay area."); // 실제 보상 패널의 전투 영역 확인
            Button[] buttons = new Button[3]; // 실제 세 카드 버튼
            for (int index = 0; index < buttons.Length; index++) // 실제 카드 순회
            {
                FieldInfo field = typeof(LevelUpPanelController).GetField("candidateButton0" + (index + 1), BindingFlags.Instance | BindingFlags.NonPublic); // 실제 후보 필드 조회
                buttons[index] = field.GetValue(panel) as Button; // 실제 카드 버튼 저장
                Require(buttons[index] != null && buttons[index].interactable, "Reward button missing or disabled."); // 실제 카드 입력 확인
                RectTransform image = buttons[index].transform.Find("WeaponArtwork") as RectTransform; // 실제 무기 이미지 영역
                Require(image != null && image.GetComponent<Image>().sprite == candidates[index].Weapon.Icon, "Reward image differs from its turret data."); // 각 보상 아이콘과 데이터 일치 확인
                Require(image != null && image.gameObject.activeSelf && Mathf.Abs(image.sizeDelta.x - 192f) < 0.001f && Mathf.Abs(image.sizeDelta.y - 192f) < 0.001f, "Reward artwork is not double the previous size."); // 기존 구십육 대비 두 배 크기 확인
                Require(image.anchorMin == new Vector2(0.5f, 0.62f) && image.anchorMax == image.anchorMin && image.pivot == new Vector2(0.5f, 0.5f), "Reward artwork is not centered within the card."); // 실제 카드 안쪽 중앙 배치 확인
                FieldInfo labelField = typeof(LevelUpPanelController).GetField("candidateLabel0" + (index + 1), BindingFlags.Instance | BindingFlags.NonPublic); // 실제 후보 문구 필드
                Text label = labelField.GetValue(panel) as Text; // 실제 후보 문구
                Require(label != null, "Reward label missing."); // 실제 후보 문구 확인
                Require(label.fontSize >= 30 && label.resizeTextForBestFit, "Reward text is not enlarged with fitting."); // 큰 선택 글자와 자동 맞춤 확인
                Vector3[] iconCorners = new Vector3[4]; // 실제 아이콘 모서리
                Vector3[] labelCorners = new Vector3[4]; // 실제 문구 영역 모서리
                image.GetWorldCorners(iconCorners); // 실제 아이콘 위치
                label.rectTransform.GetWorldCorners(labelCorners); // 실제 문구 영역 위치
                Require(labelCorners[1].y <= iconCorners[0].y + 0.01f, "Reward artwork overlaps the label."); // 실제 아이콘과 문구 겹침 방지
                Require(!image.GetComponent<Image>().raycastTarget, "Reward artwork blocks button input."); // 이미지 입력 차단 방지 확인
            }
            Require(buttons[1].transform.Find("WeaponArtwork").GetComponent<Image>().color == fire.ArtworkDisplayTint, "Reward icon did not preserve turret artwork colors."); // 포탑 그림의 원본 색상 확인
            RectTransform first = (RectTransform)buttons[0].transform; // 실제 첫 카드 영역
            RectTransform second = (RectTransform)buttons[1].transform; // 실제 둘째 카드 영역
            RectTransform third = (RectTransform)buttons[2].transform; // 실제 셋째 카드 영역
            Vector3[] firstCorners = new Vector3[4]; // 실제 첫 카드 모서리
            Vector3[] thirdCorners = new Vector3[4]; // 실제 마지막 카드 모서리
            first.GetWorldCorners(firstCorners); // 실제 첫 카드 화면 위치
            third.GetWorldCorners(thirdCorners); // 실제 마지막 카드 화면 위치
            float left = RectTransformUtility.WorldToScreenPoint(null, firstCorners[0]).x; // 실제 카드 묶음 왼쪽
            float right = RectTransformUtility.WorldToScreenPoint(null, thirdCorners[2]).x; // 실제 카드 묶음 오른쪽
            float width = (right - left) / Screen.width; // 실제 전체 화면 대비 카드 너비
            Require(Mathf.Abs(width - 0.7f) < 0.02f && right <= Camera.main.pixelRect.xMax, $"Reward cards exceed gameplay or are not about seventy percent of screen: width={width}."); // 실제 화면 너비와 패널 경계 확인
            Require(first.anchorMax.x < second.anchorMin.x && second.anchorMax.x < third.anchorMin.x && first.anchorMin.y < first.anchorMax.y, "Reward cards overlap or lack height."); // 실제 카드 간격과 높이 확인
            GameObject smallCard = new GameObject("SmallRewardCard", typeof(RectTransform), typeof(Image), typeof(Button)); // 작은 화면의 실제 카드
            RectTransform smallRect = (RectTransform)smallCard.transform; // 실제 작은 카드 영역
            smallRect.SetParent(panel.transform, false); // 실제 보상 화면에 연결
            smallRect.anchorMin = smallRect.anchorMax = new Vector2(0.5f, 0.5f); // 실제 중앙 고정 기준
            smallRect.sizeDelta = new Vector2(180f, 240f); // 실제 작은 카드 크기
            GameObject smallLabelObject = new GameObject("Label", typeof(RectTransform), typeof(Text)); // 실제 작은 카드 문구
            RectTransform smallLabel = (RectTransform)smallLabelObject.transform; // 실제 문구 영역
            smallLabel.SetParent(smallRect, false); // 실제 카드에 문구 연결
            smallLabel.anchorMin = Vector2.zero; // 실제 문구 아래 기준
            smallLabel.anchorMax = Vector2.one; // 실제 문구 위 기준
            smallLabel.offsetMin = new Vector2(8f, 8f); // 실제 문구 아래 여백
            smallLabel.offsetMax = new Vector2(-8f, -8f); // 실제 문구 위 여백
            Vector2 originalOffset = smallLabel.offsetMax; // 실제 문구 원래 위치
            WeaponCandidateArtwork.Set(smallCard.GetComponent<Button>(), smallLabelObject.GetComponent<Text>(), weapon.Icon); // 실제 작은 카드 이미지 적용
            Canvas.ForceUpdateCanvases(); // 실제 작은 카드 배치 반영
            RectTransform smallImage = smallRect.Find("WeaponArtwork") as RectTransform; // 실제 작은 카드 이미지
            Require(smallImage != null && smallImage.sizeDelta.x > 0f && smallImage.sizeDelta.x < 192f, "Reward artwork did not fit a small card."); // 실제 작은 카드 크기 제한
            Vector3[] smallIconCorners = new Vector3[4]; // 실제 작은 아이콘 모서리
            Vector3[] smallLabelCorners = new Vector3[4]; // 실제 작은 문구 모서리
            smallImage.GetWorldCorners(smallIconCorners); // 실제 작은 아이콘 위치
            smallLabel.GetWorldCorners(smallLabelCorners); // 실제 작은 문구 위치
            Require(smallLabelCorners[1].y <= smallIconCorners[0].y + 0.01f, "Small-card artwork overlaps the label."); // 실제 작은 카드 겹침 방지
            smallRect.sizeDelta = new Vector2(400f, 600f); // 실제 큰 카드 크기로 변경
            Canvas.ForceUpdateCanvases(); // 실제 카드 크기 변경 반영
            Require(Mathf.Abs(smallImage.sizeDelta.x - 192f) < 0.001f, "Reward artwork did not resize with the card."); // 실제 카드 크기 변경 대응
            WeaponCandidateArtwork.Set(smallCard.GetComponent<Button>(), smallLabelObject.GetComponent<Text>(), null); // 실제 이미지 없는 화면 전환
            Require(!smallImage.gameObject.activeSelf && smallLabel.offsetMax == originalOffset, "Reward label did not restore after hiding the image."); // 실제 기존 문구 영역 복구
            Destroy(smallCard); // 실제 작은 카드 정리
            panel.ShowRewardAction(candidates[0], false); // 합성 재료 없는 보상 처리 선택
            Canvas.ForceUpdateCanvases(); // 실제 선택 카드 배치 갱신
            for (int index = 0; index < buttons.Length; index++) // 실제 세 가지 처리 선택지 순회
            { // 반복 시작
                Transform iconObject = buttons[index].transform.Find("WeaponArtwork"); // 실제 선택지 이미지 객체
                Image icon = iconObject == null ? null : iconObject.GetComponent<Image>(); // 실제 선택지 이미지
                Require(icon != null && icon.gameObject.activeSelf && icon.sprite != null && icon.sprite.texture.name == "RewardActionIcons" && !icon.raycastTarget, "보상 처리 선택지 이미지 누락: " + index); // 실제 이미지와 버튼 입력 확인
                Require(Mathf.Approximately(icon.sprite.rect.x, index * icon.sprite.texture.width / 3), "보상 처리 선택지 이미지 순서 오류: " + index); // 장착과 합성 및 뒤로 이미지 순서 확인
                Text label = (Text)typeof(LevelUpPanelController).GetField("candidateLabel0" + (index + 1), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel); // 실제 선택지 문구
                Require(label.fontSize >= 30 && label.resizeTextForBestFit, "보상 처리 선택지 글자 확대 누락: " + index); // 장착과 합성 글자 크기 확인
                Vector3[] iconCorners = new Vector3[4]; // 실제 이미지 모서리
                Vector3[] labelCorners = new Vector3[4]; // 실제 문구 모서리
                icon.rectTransform.GetWorldCorners(iconCorners); // 실제 이미지 위치
                label.rectTransform.GetWorldCorners(labelCorners); // 실제 문구 위치
                Require(labelCorners[1].y <= iconCorners[0].y + 0.01f, "보상 처리 이미지와 문구 겹침: " + index); // 실제 문구 겹침 방지 확인
            } // 반복 종료
            Require(!buttons[1].interactable && buttons[0].interactable && buttons[2].interactable, "합성 불가 선택 상태 오류"); // 기존 합성 가능 상태 확인
            panel.ShowRewardAction(candidates[0], true); // 합성 재료가 있는 보상 처리 선택
            Require(buttons[1].interactable, "합성 가능 선택 상태 오류"); // 기존 합성 가능 상태 확인
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
