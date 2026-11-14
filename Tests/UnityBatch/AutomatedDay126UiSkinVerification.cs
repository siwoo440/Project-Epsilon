using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 실제 발사 정보 조회
using ProjectEpsilon.Combat; // 실제 포탑과 독
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 포탑 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어 이동
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능
using UnityEngine.UI; // 실제 UI 구성

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay126UiSkinVerification // 실제 UI 디자인 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay126UiSkinTurrets"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay126UiSkinVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 실제 UI 디자인 시험
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실제 전투 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시작 방지
            new GameObject("Day126UiSkinRunner").AddComponent<Day126UiSkinRunner>(); // 실제 UI 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day126UiSkinVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day126UiSkinRunner : MonoBehaviour // 실제 UI 디자인 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 화면 에셋과 상호작용 확인
        { // 함수 시작
            yield return null; // 실제 HUD와 우측 패널 생성 대기
            ProjectEpsilon.UI.LevelUpPanelController rewards = FindFirstObjectByType<ProjectEpsilon.UI.LevelUpPanelController>(FindObjectsInactive.Include); // 실제 보상 화면
            Require(rewards != null, "실제 보상 화면 누락"); // 보상 구성 확인
            Button card = (Button)typeof(ProjectEpsilon.UI.LevelUpPanelController).GetField("candidateButton01", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(rewards); // 실제 첫 보상 카드
            Require(card != null && card.GetComponent<Image>().sprite != null && card.GetComponent<Image>().sprite.texture.name == "RewardCardFrame", "실제 보상 카드 프레임 누락"); // 실제 카드 이미지 확인
            Require(card.GetComponent<Image>().type == Image.Type.Sliced, "실제 카드 크기 대응 누락"); // 실제 테두리 확장 확인
            Image side = FindImage("BodySidebar"); // 실제 우측 패널
            Image row = FindImage("SidebarRow_1"); // 실제 몸통 행
            Require(side != null && side.sprite != null && side.sprite.texture.name == "RewardCardFrame" && !side.raycastTarget, "실제 우측 패널 프레임 누락"); // 패널 이미지와 입력 보존
            Require(row != null && row.sprite != null && row.sprite.texture.name == "ButtonFrame" && !row.raycastTarget, "실제 몸통 행 프레임 누락"); // 몸통 행 이미지 확인
            for (int index = 0; index < 8; index++) // 실제 여덟 속성 순회
            { // 반복 시작
                Image icon = FindImage("SidebarAttributeIcon_" + index); // 실제 속성 아이콘
                Require(icon != null && icon.sprite != null && icon.sprite.texture.name == "AttributeIcons" && !icon.raycastTarget, "실제 속성 아이콘 누락: " + index); // 실제 이미지와 입력 확인
                Rect area = icon.sprite.rect; // 실제 원본 격자 영역
                Require(Mathf.Approximately(area.x, index % 4 * icon.sprite.texture.width / 4) && Mathf.Approximately(area.y, (1 - index / 4) * icon.sprite.texture.height / 2), "실제 속성 아이콘 순서 오류: " + index + " / " + area + " / " + icon.sprite.texture.width + "x" + icon.sprite.texture.height); // 실제 속성 배열 순서 확인
            } // 반복 종료
            foreach (string name in new[] { "HealthBar", "ExperienceBar", "StaminaBar" }) // 실제 공용 바 순회
            { // 반복 시작
                Image bar = FindImage(name); // 실제 막대 배경
                Require(bar != null && bar.sprite != null && bar.sprite.texture.name == "BarFrame" && !bar.raycastTarget, "실제 막대 프레임 누락: " + name); // 실제 프레임과 입력 보존
                Require(bar.transform.Find("Fill") != null, "실제 막대 채움 누락: " + name); // 기존 채움 유지 확인
            } // 반복 종료
            ProjectEpsilon.UI.RunFrontendController frontend = ProjectEpsilon.UI.RunFrontendController.Instance; // 실제 화면 전환 관리자
            Require(frontend != null, "실제 화면 전환 관리자 누락"); // 메뉴 구성 확인
            MethodInfo show = typeof(ProjectEpsilon.UI.RunFrontendController).GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 화면 전환 경로
            Require(show != null, "실제 화면 전환 경로 누락"); // 메뉴 전환 확인
            show.Invoke(frontend, new object[] { ProjectEpsilon.UI.FrontendStage.Paused }); // 실제 일시정지 화면 구성
            Image modal = FindImage("FrontendContentFrame"); // 실제 일시정지 배경
            Require(modal != null && modal.sprite != null && modal.sprite.texture.name == "RewardCardFrame" && !modal.raycastTarget, "실제 일시정지 프레임 또는 입력 오류"); // 실제 배경과 입력 확인
            FieldInfo pageField = typeof(ProjectEpsilon.UI.RunFrontendController).GetField("tutorialPage", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 안내 단계
            Require(pageField != null, "실제 안내 단계 누락"); // 기존 안내 상태 확인
            for (int page = 0; page < 8; page++) // 실제 여덟 안내 단계 순회
            { // 반복 시작
                pageField.SetValue(frontend, page); // 실제 안내 단계 설정
                show.Invoke(frontend, new object[] { ProjectEpsilon.UI.FrontendStage.Tutorial }); // 실제 안내 화면 구성
                GameObject artworkObject = GameObject.Find("FrontendTutorialArtwork"); // 실제 안내 그림
                Image artwork = artworkObject == null ? null : artworkObject.GetComponent<Image>(); // 실제 안내 이미지
                Require(artwork != null && artwork.sprite != null && artwork.sprite.texture.name == "TutorialIcons" && !artwork.raycastTarget, "실제 안내 그림 누락: " + page); // 실제 안내 이미지와 입력 확인
                Rect area = artwork.sprite.rect; // 실제 안내 원본 영역
                Require(Mathf.Approximately(area.x, page % 4 * artwork.sprite.texture.width / 4) && Mathf.Approximately(area.y, (1 - page / 4) * artwork.sprite.texture.height / 2), "실제 안내 이미지 순서 오류: " + page); // 실제 안내 단계별 이미지 확인
            } // 반복 종료
            show.Invoke(frontend, new object[] { ProjectEpsilon.UI.FrontendStage.Playing }); // 실제 전투 화면 복구
            AutomatedDay126UiSkinVerification.Finish(true, "Actual reward, body sidebar and HUD frame skins are present and non-blocking."); // 실제 시험 결과
        } // 함수 종료
        private static Image FindImage(string name) // 실제 활성 또는 비활성 이미지 조회
        { // 함수 시작
            foreach (Image image in FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None)) // 실제 이미지 순회
            { // 반복 시작
                if (image.name == name) // 실제 이름 확인
                { // 조건 시작
                    return image; // 실제 이미지 반환
                } // 조건 종료
            } // 반복 종료
            return null; // 누락 이미지 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 실제 시험 조건
        { // 함수 시작
            if (!condition) // 실제 실패 확인
            { // 조건 시작
                AutomatedDay126UiSkinVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
