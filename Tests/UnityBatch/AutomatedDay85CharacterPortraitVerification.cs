using System; // 예외 처리
using System.Reflection; // 실제 화면 갱신 조회
using ProjectEpsilon.Data; // 실제 캐릭터 데이터
using ProjectEpsilon.Progression; // 실제 캐릭터 해금
using ProjectEpsilon.UI; // 실제 선택 화면
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 준비 씬 열기
using UnityEngine; // Unity 기능
using UnityEngine.UI; // 실제 선택 버튼과 이미지

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay85CharacterPortraitVerification // 캐릭터 초상화 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay85Portrait"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay85CharacterPortraitVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 준비 상태 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene(SceneFrontendContext.PreparationPath); // 실제 출격 준비 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 준비 상태 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 캐릭터 선택 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || GameObject.Find("FrontendCharacterButton0") == null) // 실제 선택 화면 확인
            { // 조건 시작
                return; // 실제 화면 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            try // 실제 초상화 시험
            { // 시험 시작
                Verify(); // 여섯 캐릭터와 선택 화면 확인
                Finish(0, "Six selected portraits and existing unlock behavior passed."); // 성공 결과
            } // 시험 종료
            catch (Exception error) // 실패 확인
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 선택 화면 확인
        { // 함수 시작
            Require(Application.productName.EndsWith("-Isolated", StringComparison.Ordinal), "독립 저장 공간 필요"); // 사용자 저장 보호
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 해금 관리자
            if (meta != null && !meta.IsCharacterUnlocked("mobile")) // 잠긴 기동형 확인
            { // 조건 시작
                Require(meta.Service != null && meta.Service.TryCompleteRun("day85-portrait-unlock", 0d, 0, 0, true, 0), "독립 저장의 기동형 해금 실패"); // 독립 시험 해금
            } // 조건 종료
            RunFrontendController frontend = UnityEngine.Object.FindFirstObjectByType<RunFrontendController>(); // 실제 선택 화면 제어기
            MethodInfo show = typeof(RunFrontendController).GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 화면 갱신 경로
            PropertyInfo portrait = typeof(CharacterData).GetProperty("Portrait"); // 실제 캐릭터 초상화
            Require(frontend != null && show != null && portrait != null, "캐릭터별 초상화 연결 누락"); // 실제 데이터 연결 확인
            show.Invoke(frontend, new object[] { FrontendStage.Characters }); // 해금 상태 반영
            string[] names = { "Balanced", "Mobile", "Heavy", "Fusion", "Holy", "Dark" }; // 실제 캐릭터 에셋 순서
            for (int index = 0; index < names.Length; index++) // 실제 여섯 캐릭터 순회
            { // 반복 시작
                CharacterData character = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Data/Characters/Prototype" + names[index] + ".asset"); // 실제 캐릭터 데이터
                Sprite expected = character == null ? null : portrait.GetValue(character) as Sprite; // 실제 기획서 초상화
                Require(expected != null && expected.name == "Portrait_" + names[index], "캐릭터 초상화 누락: " + names[index]); // 맵별 이미지 연결 확인
                GameObject buttonObject = GameObject.Find("FrontendCharacterButton" + index); // 실제 선택 버튼
                Button button = buttonObject == null ? null : buttonObject.GetComponent<Button>(); // 실제 버튼 입력
                Require(button != null && button.interactable, "해금된 선택 버튼 누락: " + names[index]); // 실제 해금과 선택 확인
                button.onClick.Invoke(); // 실제 캐릭터 선택
                GameObject imageObject = GameObject.Find("FrontendCharacterPortrait"); // 실제 선택 초상화
                Image image = imageObject == null ? null : imageObject.GetComponent<Image>(); // 실제 화면 이미지
                Require(image != null && image.sprite == expected && image.preserveAspect && !image.raycastTarget, "선택 화면 초상화 오류: " + names[index]); // 실제 이미지와 버튼 입력 확인
            } // 반복 종료
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실제 실패 전달
            } // 조건 종료
        } // 함수 종료
        private static void Finish(int code, string message) // 배치 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 감시 해제
            Debug.Log("[Day85PortraitVerification] " + (code == 0 ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
