using System; // 예외 처리
using System.IO; // 화면 이미지 저장
using System.Reflection; // 실제 처치 이벤트 시험
using ProjectEpsilon.Core; // 전투 상태
using ProjectEpsilon.Data; // 실제 포탑 데이터
using ProjectEpsilon.Progression; // 실제 포탑 선택과 점수
using ProjectEpsilon.UI; // 실제 보상 화면
using UnityEditor; // 유니티 배치 실행
using UnityEditor.SceneManagement; // 실제 전투 씬
using UnityEngine; // 유니티 화면 기능

namespace ProjectEpsilon.Editor // 배치 확인 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay139ScreenCapture // 실제 UI 화면 이미지 저장
    { // 시험 시작
        private const string Key = "EpsilonDay139ScreenCapture"; // 시험 진행 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay139ScreenCapture() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투 준비 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 실제 전투 씬 실행
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 전투 씬 열기
            SessionState.SetBool(Key, true); // 시험 진행 기록
            EditorApplication.update += Tick; // 전투 준비 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection()) // 전투 진입 확인
            { // 조건 시작
                return; // 시작 선택 대기
            } // 조건 종료
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 전투 초기화 확인
            { // 조건 시작
                return; // 실제 전투 대기
            } // 조건 종료
            started = true; // 중복 캡처 방지
            try // 실제 화면 캡처
            { // 처리 시작
                MetaProgressionController meta = MetaProgressionController.Instance; // 실제 점수 관리자
                typeof(MetaProgressionController).GetMethod("HandleEnemyDied", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(meta, new object[] { true }); // 엘리트 처치 점수 표시
                Capture("day139-hud.png"); // 점수 HUD 화면 저장
                LevelUpPanelController panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 포탑 선택 화면
                WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 포탑 데이터
                panel.ShowRewardAction(new WeaponRewardCandidate(weapon, 1), true); // 실제 장착과 합성 선택 화면
                Capture("day139-choices.png"); // 선택지 화면 저장
                SessionState.SetBool(Key, false); // 캡처 진행 표식 제거
                EditorApplication.update -= Tick; // 캡처 감시 해제
                Debug.Log("[Day139Capture] PASS: HUD and choice images saved."); // 실제 저장 결과 기록
                new GameObject("BatchRewardLayoutRunner").AddComponent<RewardLayoutRunner>(); // 실제 보상 배치 시험 연결
            } // 처리 종료
            catch (Exception error) // 캡처 실패 처리
            { // 실패 시작
                Finish(1, error.ToString()); // 실제 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Capture(string name) // 게임 카메라와 UI 렌더링
        { // 함수 시작
            Camera gameCamera = Camera.main; // 실제 게임 카메라
            if (gameCamera == null || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) // 실제 그래픽 장치 확인
            { // 조건 시작
                throw new InvalidOperationException("Game camera or graphics device unavailable."); // 렌더링 불가 보고
            } // 조건 종료
            RenderTexture target = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32); // 실제 화면 렌더 대상
            RenderTexture previous = RenderTexture.active; // 이전 활성 렌더 대상
            RenderTexture originalTarget = gameCamera.targetTexture; // 기존 게임 카메라 대상
            Rect originalRect = gameCamera.rect; // 기존 게임 카메라 영역
            Canvas[] canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None); // 실제 활성 캔버스
            int[] originalOrders = new int[canvases.Length]; // 기존 캔버스 표시 순서
            for (int index = 0; index < canvases.Length; index++) // 실제 화면 캔버스 순회
            { // 순회 시작
                Canvas canvas = canvases[index]; // 현재 캔버스
                originalOrders[index] = canvas.sortingOrder; // 기존 캔버스 표시 순서 보존
                if (canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay) // 화면 겹침 캔버스 확인
                { // 조건 시작
                    canvas.renderMode = RenderMode.ScreenSpaceCamera; // 배치 카메라에 UI 연결
                    canvas.worldCamera = gameCamera; // 장면과 같은 카메라 사용
                    canvas.planeDistance = 0.31f; // 카메라에 가까운 UI 거리
                    canvas.sortingOrder = short.MaxValue; // 게임 오브젝트 앞의 UI 순서
                } // 조건 종료
            } // 순회 종료
            gameCamera.rect = new Rect(0f, 0f, 1f, 1f); // 전체 화면 UI를 포함한 캡처 영역
            gameCamera.targetTexture = target; // 실제 게임 화면 렌더 대상
            Canvas.ForceUpdateCanvases(); // 실제 화면 배치 갱신
            gameCamera.Render(); // 실제 게임 장면과 UI 렌더링
            RenderTexture.active = target; // 이미지 읽기 대상 설정
            Texture2D image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false); // 화면 이미지 생성
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); // 실제 화면 픽셀 복사
            image.Apply(); // 픽셀 데이터 반영
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..")); // 격리 시험 결과 폴더
            File.WriteAllBytes(Path.Combine(folder, name), image.EncodeToPNG()); // 실제 화면 이미지 저장
            for (int index = 0; index < canvases.Length; index++) // 임시 캔버스 연결 복구
            { // 순회 시작
                Canvas canvas = canvases[index]; // 현재 캔버스
                if (canvas.isRootCanvas && canvas.worldCamera == gameCamera) // 임시 연결 확인
                { // 조건 시작
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay; // 원래 화면 표시 복구
                    canvas.worldCamera = null; // 임시 카메라 연결 해제
                    canvas.sortingOrder = originalOrders[index]; // 기존 캔버스 표시 순서 복구
                } // 조건 종료
            } // 순회 종료
            gameCamera.targetTexture = originalTarget; // 원래 게임 렌더 대상 복구
            gameCamera.rect = originalRect; // 원래 게임 카메라 영역 복구
            RenderTexture.active = previous; // 이전 렌더 대상 복구
            UnityEngine.Object.DestroyImmediate(image); // 임시 이미지 정리
            UnityEngine.Object.DestroyImmediate(target); // 임시 렌더 대상 정리
            Canvas.ForceUpdateCanvases(); // 원래 화면 배치 복구
        } // 함수 종료
        private static void Finish(int code, string message) // 실제 배치 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 진행 표식 제거
            EditorApplication.update -= Tick; // 전투 감시 해제
            Debug.Log($"[Day139Capture] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 결과 기록
            EditorApplication.Exit(code); // 실제 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
