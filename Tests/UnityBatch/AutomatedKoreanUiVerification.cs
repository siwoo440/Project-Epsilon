using System; // 실제 예외 형식
using System.Collections; // 실제 프레임 대기
using System.Collections.Generic; // 실제 영문 표시 목록
using System.Reflection; // 실제 비공개 표시 규칙 조회
using System.Text.RegularExpressions; // 실제 영단어 검사
using ProjectEpsilon.Combat; // 실제 포탑 장착
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 포탑 데이터
using ProjectEpsilon.Enemies; // 실제 보스 체력 표시
using ProjectEpsilon.Progression; // 실제 경험치와 합성
using ProjectEpsilon.UI; // 실제 화면과 씬 역할
using UnityEditor; // 실제 배치 제어
using UnityEditor.SceneManagement; // 실제 시작 씬 열기
using UnityEngine; // Unity 화면 기능
using UnityEngine.SceneManagement; // 실제 씬 상태
using UnityEngine.UI; // 실제 글자와 버튼

namespace ProjectEpsilon.Editor // 실제 배치 시험 영역
{
    [InitializeOnLoad] // 실제 플레이 모드 재연결
    public static class AutomatedKoreanUiVerification // 실제 화면별 한국어 검증
    {
        private const string Key = "EpsilonKoreanUiVerification"; // 실제 시험 진행 표식
        private static bool started; // 실제 시험 실행 여부
        static AutomatedKoreanUiVerification() // 실제 도메인 재시작 연결
        {
            if (SessionState.GetBool(Key, false)) // 실제 시험 진행 확인
            {
                EditorApplication.update += Tick; // 실제 시작 감시 연결
            }
        }
        public static void Run() // 실제 Unity 배치 진입점
        {
            EditorSceneManager.OpenScene(SceneFrontendContext.MainMenuPath); // 실제 메인 메뉴 씬 열기
            SessionState.SetBool(Key, true); // 실제 시험 진행 기록
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 모드 시작
        }
        private static void Tick() // 실제 플레이 시작 확인
        {
            if (!EditorApplication.isPlaying || started) // 실제 시작과 중복 확인
            {
                return; // 실제 플레이 시작 대기
            }
            started = true; // 실제 시험 중복 방지
            GameObject runner = new GameObject("KoreanUiVerification"); // 실제 화면 시험 객체
            UnityEngine.Object.DontDestroyOnLoad(runner); // 실제 씬 간 시험 유지
            runner.AddComponent<KoreanUiVerificationRunner>(); // 실제 화면별 문구 검사 실행
        }
        public static void Finish(int code, string message) // 실제 배치 종료
        {
            if (code == 0) // 실제 시험 성공 확인
            {
                Debug.Log("[KoreanUiVerification] PASS: " + message); // 실제 성공 결과
            }
            else // 실제 시험 실패 확인
            {
                Debug.LogError("[KoreanUiVerification] FAIL: " + message); // 실제 실패 결과
            }
            SessionState.SetBool(Key, false); // 실제 시험 표식 제거
            EditorApplication.update -= Tick; // 실제 감시 연결 해제
            EditorApplication.Exit(code); // 실제 시험 종료 코드
        }
    }
    public sealed class KoreanUiVerificationRunner : MonoBehaviour // 실제 화면 시험 실행기
    {
        private IEnumerator Start() // 실제 예외를 포함한 시험 실행
        {
            IEnumerator steps = RunSteps(); // 실제 화면별 시험 단계
            while (true) // 실제 시험 완료까지 반복
            {
                bool moved; // 실제 다음 단계 여부
                try // 실제 시험 단계 실행
                {
                    moved = steps.MoveNext(); // 실제 다음 단계 진행
                }
                catch (Exception error) // 실제 시험 오류 확인
                {
                    AutomatedKoreanUiVerification.Finish(1, error.ToString()); // 실제 오류와 실패 종료
                    yield break; // 실제 시험 중단
                }
                if (!moved) // 실제 시험 완료 확인
                {
                    yield break; // 실제 실행 종료
                }
                yield return steps.Current; // 실제 다음 프레임 대기
            }
        }
        private IEnumerator RunSteps() // 실제 화면 전환과 문구 검사
        {
            Require(Application.productName == "Project-Epsilon-KoreanUI-Isolated", "User save profile is not isolated."); // 실제 사용자 저장 격리 확인
            yield return null; // 실제 메뉴 초기화 대기
            string[] nodes = (string[])typeof(MetaProgressionMenu).GetField("Labels", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null); // 실제 영구 성장 항목
            foreach (string node in nodes) // 실제 영구 성장 항목 순회
            {
                Require(!Regex.IsMatch(node, "[A-Za-z]{2,}"), "English permanent growth label: " + node); // 실제 성장 항목 한국어 확인
            }
            MethodInfo keyLabel = typeof(InputBindingManager).GetMethod("LocalizeBindingName", BindingFlags.NonPublic | BindingFlags.Static); // 실제 키 이름 변환기
            object[] keyArguments = // 실제 숫자 키패드 확인 인수
            {
                "Numpad 1", "<Keyboard>/numpad1" // 실제 키패드 이름과 입력 경로
            };
            Require(keyLabel != null && (string)keyLabel.Invoke(null, keyArguments) == "숫자 키패드 1", "Keypad name was not localized."); // 실제 숫자 키패드 한국어 확인
            MethodInfo bossLabel = typeof(PrototypeBossEncounter).GetMethod("FormatHealthLabel", BindingFlags.NonPublic | BindingFlags.Static); // 실제 보스 체력 문구 생성기
            object[] bossArguments = // 실제 보스 체력 확인 인수
            {
                7f, 10f // 실제 현재와 최대 체력
            };
            Require(bossLabel != null && (string)bossLabel.Invoke(null, bossArguments) == "보스 7 / 10", "Boss health label was not localized."); // 실제 보스 체력 한국어 확인
            CheckVisibleText("main"); // 실제 메인 화면 영문 확인
            Click("FrontendSettingsButton"); // 실제 메뉴 설정 열기
            yield return null; // 실제 설정 화면 대기
            CheckVisibleText("settings"); // 실제 설정 화면 영문 확인
            Click("FrontendInputSettingsButton"); // 실제 키 설정 열기
            yield return null; // 실제 키 설정 화면 대기
            CheckVisibleText("input"); // 실제 키 설정 영문 확인
            Click("InputClose"); // 실제 키 설정 닫기
            yield return null; // 실제 설정 화면 복귀 대기
            Click("FrontendSettingsBackButton"); // 실제 메인 메뉴 복귀
            yield return null; // 실제 메뉴 화면 대기
            Click("FrontendStartButton"); // 실제 출격 준비 이동
            yield return null; // 실제 캐릭터 선택 대기
            CheckVisibleText("characters"); // 실제 캐릭터 화면 영문 확인
            Click("FrontendCharactersNextButton"); // 실제 맵 선택 이동
            yield return null; // 실제 맵 화면 대기
            CheckVisibleText("maps"); // 실제 맵 화면 영문 확인
            Click("FrontendMapsNextButton"); // 실제 시작 포탑 선택 이동
            yield return null; // 실제 포탑 화면 대기
            CheckVisibleText("starting turrets"); // 실제 시작 포탑 화면 영문 확인
            Click("FrontendStartingWeaponButton0"); // 실제 첫 포탑 선택과 출격
            for (int frame = 0; frame < 120 && (GameManager.Instance == null || !GameManager.Instance.IsPlaying); frame++) // 실제 전투 시작 대기
            {
                yield return null; // 실제 전투 초기화 프레임 대기
            }
            yield return null; // 실제 전투 HUD 갱신 대기
            CheckVisibleText("combat"); // 실제 전투 HUD 영문 확인
            SnakeExperience experience = FindFirstObjectByType<SnakeExperience>(); // 실제 경험치 관리자
            Require(experience != null, "Experience manager missing."); // 실제 경험치 구성 확인
            experience.AddExperience(experience.RequiredExperience); // 실제 레벨업 요청
            yield return null; // 실제 보상 화면 생성 대기
            CheckVisibleText("level reward"); // 실제 레벨업 화면 영문 확인
            Click("CandidateButton_01"); // 실제 첫 보상 선택
            yield return null; // 실제 보상 완료 대기
            LevelUpPanelController rewardPanel = FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 공용 보상 패널
            EliteRewardController elite = FindFirstObjectByType<EliteRewardController>(); // 실제 정예 보상 관리자
            MethodInfo eliteLabel = typeof(EliteRewardController).GetMethod("BuildLabel", BindingFlags.NonPublic | BindingFlags.Instance); // 실제 정예 보상 문구 생성기
            Require(rewardPanel != null && elite != null && eliteLabel != null, "Elite reward panel unavailable."); // 실제 정예 보상 구성 확인
            string[] eliteLabels = new string[3]; // 실제 정예 보상 문구 목록
            bool[] available = new bool[3]; // 실제 정예 보상 선택 상태
            for (int index = 0; index < eliteLabels.Length; index++) // 실제 세 보상 문구 순회
            {
                object[] labelArguments = // 실제 보상 문구 생성 인수
                {
                    index, true // 실제 보상 종류와 가능 상태
                };
                eliteLabels[index] = (string)eliteLabel.Invoke(elite, labelArguments); // 실제 보상 문구 생성
                available[index] = true; // 실제 보상 선택 활성
            }
            rewardPanel.ShowEliteRewards(eliteLabels, available); // 실제 정예 보상 패널 표시
            CheckVisibleText("elite reward"); // 실제 정예 보상 화면 영문 확인
            rewardPanel.Hide(); // 실제 시험용 정예 화면 닫기
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            WeaponData blade = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 물리 포탑
            Require(weapons != null && blade != null && weapons.TryEquipAt(0, blade) && weapons.TryEquipAt(1, blade), "Merge materials unavailable."); // 실제 같은 포탑 두 개 장착
            SnakeWeaponMergeController merge = FindFirstObjectByType<SnakeWeaponMergeController>(); // 실제 합성 관리자
            Require(merge != null && merge.OpenMergePanel(), "Could not open merge panel."); // 실제 합성 화면 열기
            yield return null; // 실제 합성 화면 갱신 대기
            CheckVisibleText("merge"); // 실제 합성 화면 영문 확인
            WeaponMergePanelController mergePanel = FindFirstObjectByType<WeaponMergePanelController>(); // 실제 합성 화면
            Require(mergePanel != null, "Merge display missing."); // 실제 합성 화면 연결 확인
            mergePanel.ShowInheritanceSelection(weapons.Slots[0], 0, weapons.Slots[1], 1); // 실제 강화 계승 화면 표시
            CheckVisibleText("merge inheritance"); // 실제 강화 계승 화면 영문 확인
            mergePanel.ShowRewardUpgradeSelection(blade); // 실제 합성 강화 화면 표시
            mergePanel.ShowSynergyPreview(blade.Attribute, 2, 1, 2, 0); // 실제 속성 단계 미리보기
            CheckVisibleText("merge upgrade"); // 실제 합성 강화 화면 영문 확인
            merge.CloseMergePanel(); // 실제 합성 화면 닫기
            yield return null; // 실제 전투 복귀 대기
            GameManager.Instance.PauseGame(); // 실제 전투 일시정지
            RunFrontendController.Instance.ShowManualPause(); // 실제 일시정지 화면 표시
            yield return null; // 실제 정지 화면 대기
            CheckVisibleText("pause"); // 실제 일시정지 화면 영문 확인
            Click("FrontendPauseSettingsButton"); // 실제 전투 중 설정 열기
            yield return null; // 실제 설정 화면 대기
            Click("FrontendInputSettingsButton"); // 실제 전투 키 설정 열기
            yield return null; // 실제 키 설정 화면 대기
            CheckVisibleText("combat input"); // 실제 전투 키 설정 영문 확인
            FindFirstObjectByType<SettingsMenuController>().CloseSettings(); // 실제 키 설정 닫기
            yield return null; // 실제 설정 화면 복귀 대기
            Click("FrontendSettingsBackButton"); // 실제 정지 화면 복귀
            yield return null; // 실제 정지 화면 대기
            Click("FrontendResumeButton"); // 실제 전투 재개
            yield return null; // 실제 전투 복귀 대기
            GameManager.Instance.AbandonRun(); // 실제 런 포기와 결과 이동
            yield return null; // 실제 결과 씬 전환 대기
            yield return null; // 실제 결과 화면 생성 대기
            Require(SceneManager.GetActiveScene().path == SceneFrontendContext.ResultsPath, "Result scene missing."); // 실제 결과 씬 확인
            CheckVisibleText("results"); // 실제 결과 화면 영문 확인
            AutomatedKoreanUiVerification.Finish(0, "Main, settings, preparation, combat, reward, merge, pause and results text passed."); // 실제 전체 시험 성공
        }
        private static void CheckVisibleText(string stage) // 실제 표시 글자에서 영단어 검색
        {
            List<string> errors = new List<string>(); // 실제 영문 문구 목록
            foreach (Text label in FindObjectsByType<Text>(FindObjectsSortMode.None)) // 실제 활성 UI 글자 순회
            {
                if (!label.isActiveAndEnabled || label.canvas == null || !label.canvas.enabled) // 실제 표시 화면 확인
                {
                    continue; // 숨겨진 글자 제외
                }
                Require(label.font != null, stage + " missing font: " + label.name); // 실제 표시 글꼴 연결 확인
                string plain = Regex.Replace(label.text ?? "", "<[^>]+>", ""); // 실제 글자 태그 제외
                if (Regex.IsMatch(plain, "[A-Za-z]{2,}")) // 실제 두 글자 이상 영단어 확인
                {
                    errors.Add(label.name + ": " + plain.Replace("\n", " / ")); // 실제 화면과 남은 문구 기록
                }
            }
            Require(errors.Count == 0, stage + " English text: " + string.Join("; ", errors)); // 실제 영문 부재 확인
        }
        private static void Click(string name) // 실제 화면 버튼 선택
        {
            GameObject item = GameObject.Find(name); // 실제 활성 버튼 객체
            Button button = item == null ? null : item.GetComponent<Button>(); // 실제 버튼 컴포넌트
            Require(button != null && button.interactable, "Button unavailable: " + name); // 실제 버튼 활성 확인
            button.onClick.Invoke(); // 실제 버튼 이벤트 실행
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 조건 실패 확인
            {
                throw new InvalidOperationException(message); // 실제 시험 중단
            }
        }
    }
}
