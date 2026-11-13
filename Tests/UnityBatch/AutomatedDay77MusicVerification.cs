using System; // 예외와 형식
using System.Reflection; // 실제 맵과 보스 경로 조회
using ProjectEpsilon.Core; // 실제 전투 상태
using ProjectEpsilon.Enemies; // 실제 보스 전환
using ProjectEpsilon.World; // 실제 맵 선택
using ProjectEpsilon.UI; // 실제 분리 씬 경로
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 전투 씬 열기
using UnityEngine; // Unity 기능
using UnityEngine.SceneManagement; // 실제 씬 전환
using UnityEngine.UI; // 실제 메뉴 버튼
namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay77MusicVerification // 배경음 연결 시험
    { // 시험 시작
        private const string Key = "EpsilonDay77Music"; // 시험 진행 표식
        private static int step; // 실제 시험 단계
        private static int frames; // 전환 후 대기 프레임
        private static float deadline; // 실제 화면 생성 제한 시각
        private static MonoBehaviour music; // 실제 배경음 제어기
        private static PrototypeMapSelection map; // 실제 맵 선택기
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 경로 조회
        static AutomatedDay77MusicVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투와 씬 전환 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene(SceneFrontendContext.GamePath); // 실제 전투 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실제 게임 준비 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투와 화면 확인
        { // 함수 시작
            if (!EditorApplication.isPlaying) // 플레이 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            try // 실제 음악 경로 시험
            { // 시험 시작
                Advance(); // 단계별 전환 확인
            } // 시험 종료
            catch (Exception error) // 실제 실패 확인
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Advance() // 실제 음악 전환 시험
        { // 함수 시작
            if (step == 0) // 실제 전투 초기화 대기
            { // 조건 시작
                if (StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 전투 준비 확인
                { // 조건 시작
                    return; // 실제 전투 준비 대기
                } // 조건 종료
                Type type = Type.GetType("ProjectEpsilon.Audio.BackgroundMusicController, Assembly-CSharp"); // 실제 배경음 제어기
                Require(type != null, "배경음 제어기 누락"); // 실제 구현 확인
                music = (MonoBehaviour)UnityEngine.Object.FindFirstObjectByType(type); // 실제 전역 배경음
                map = UnityEngine.Object.FindFirstObjectByType<PrototypeMapSelection>(); // 실제 맵 선택기
                Require(music != null && map != null, "배경음 또는 맵 선택 누락"); // 실제 전투 구성 확인
                FrontendOptions options = new FrontendOptions(); // 기존 음량 설정
                Require(options.Change(25 - options.Volume, 0, 0, false), "기존 전체 음량 설정 실패"); // 실제 저장 설정 반영
                Check("BGM_Plain"); // 기본 평원 음악 확인
                typeof(PrototypeMapSelection).GetField("selectedIndex", Private).SetValue(map, 1); // 폐허 선택 상태 재현
                step = 1; // 다음 단계 이동
                return; // 실제 프레임 대기
            } // 조건 종료
            if (step == 1) // 폐허 음악 확인
            { // 조건 시작
                if (++frames < 3) // 전환 대기 확인
                { // 조건 시작
                    return; // 실제 변경 반영 대기
                } // 조건 종료
                Check("BGM_Ruins"); // 실제 폐허 음악 확인
                typeof(PrototypeMapSelection).GetField("selectedIndex", Private).SetValue(map, 2); // 습지 선택 상태 재현
                frames = 0; // 대기 초기화
                step = 2; // 다음 단계 이동
                return; // 실제 프레임 대기
            } // 조건 종료
            if (step == 2) // 습지 음악 확인
            { // 조건 시작
                if (++frames < 3) // 전환 대기 확인
                { // 조건 시작
                    return; // 실제 변경 반영 대기
                } // 조건 종료
                Check("BGM_Swamp"); // 실제 습지 음악 확인
                EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
                typeof(EarlyEnemyWaveSpawner).GetMethod("EnterBossTransition", Private).Invoke(spawner, null); // 실제 보스 전환
                frames = 0; // 대기 초기화
                step = 3; // 다음 단계 이동
                return; // 실제 프레임 대기
            } // 조건 종료
            if (step == 3) // 보스 음악 확인
            { // 조건 시작
                if (++frames < 3) // 전환 대기 확인
                { // 조건 시작
                    return; // 실제 변경 반영 대기
                } // 조건 종료
                Check("BGM_Boss"); // 실제 보스 음악 확인
                GameObject reloader = new GameObject("Day77SceneReloader"); // 실제 씬 전환 보조 객체
                reloader.AddComponent<RunSceneReloader>().Begin(GameManager.Instance.gameObject, SceneFrontendContext.MainMenuPath); // 기존 관리자 제거 후 메뉴 이동
                deadline = Time.realtimeSinceStartup + 10f; // 메뉴 생성 대기 제한
                frames = 0; // 대기 초기화
                step = 4; // 다음 단계 이동
                return; // 실제 프레임 대기
            } // 조건 종료
            if (++frames < 5) // 화면 생성 대기 확인
            { // 조건 시작
                return; // 실제 화면 생성 대기
            } // 조건 종료
            if (step == 4) // 실제 메뉴 확인
            { // 조건 시작
                if (SceneManager.GetActiveScene().path != SceneFrontendContext.MainMenuPath && Time.realtimeSinceStartup < deadline) // 실제 메뉴 씬 대기
                { // 조건 시작
                    return; // 실제 메뉴 씬 전환 대기
                } // 조건 종료
                Require(SceneManager.GetActiveScene().path == SceneFrontendContext.MainMenuPath, "실제 메뉴 씬 전환 실패"); // 실제 메뉴 씬 확인
                Check("BGM_Menu"); // 실제 메뉴 음악 확인
                Require(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length == 1, "메뉴 오디오 수신기 누락 또는 중복"); // 실제 메뉴 수신기 확인
                GameObject button = GameObject.Find("FrontendStartButton"); // 실제 출격 준비 버튼
                if (button == null && Time.realtimeSinceStartup < deadline) // 실제 버튼 생성 대기
                { // 조건 시작
                    return; // 실제 화면 준비 대기
                } // 조건 종료
                Require(button != null && button.GetComponent<Button>() != null, "실제 메뉴 시작 버튼 누락"); // 실제 메뉴 구성 확인
                button.GetComponent<Button>().onClick.Invoke(); // 실제 준비 씬 이동
                deadline = Time.realtimeSinceStartup + 10f; // 준비 화면 생성 대기 제한
                frames = 0; // 대기 초기화
                step = 5; // 준비 씬 확인 단계
                return; // 실제 씬 생성 대기
            } // 조건 종료
            if (SceneManager.GetActiveScene().path != SceneFrontendContext.PreparationPath && Time.realtimeSinceStartup < deadline) // 실제 준비 화면 대기
            { // 조건 시작
                return; // 실제 준비 씬 전환 대기
            } // 조건 종료
            Require(SceneManager.GetActiveScene().path == SceneFrontendContext.PreparationPath, "실제 준비 씬 전환 실패"); // 실제 준비 씬 확인
            Check("BGM_Menu"); // 준비 씬 메뉴 음악 유지 확인
            Require(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length == 1, "준비 화면 오디오 수신기 누락 또는 중복"); // 실제 준비 화면 수신기 확인
            Finish(0, "Map tracks, boss transition, menu and preparation tracks, listeners, loop and master volume passed."); // 성공 기록
        } // 함수 종료
        private static void Check(string name) // 실제 음악 설정 확인
        { // 함수 시작
            AudioSource source = music.GetComponent<AudioSource>(); // 실제 음악 재생기
            Require(source != null && source.clip != null && source.clip.name == name && source.loop, "배경음 전환 또는 반복 실패: " + name); // 실제 음원과 반복 확인
            Require(Mathf.Approximately(AudioListener.volume, 0.25f), "기존 전체 음량 변경"); // 전체 음량 유지 확인
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 원인 전달
            } // 조건 종료
        } // 함수 종료
        private static void Finish(int code, string message) // 배치 결과 처리
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 전투 감시 해제
            Debug.Log("[Day77Music] " + (code == 0 ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
