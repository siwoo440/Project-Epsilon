using System; // 예외 형식
using System.Reflection; // 내부 상태 조회
using ProjectEpsilon.Combat; // 전투 대상
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Enemies; // 적 출현기
using ProjectEpsilon.Player; // 몸통 상태
using ProjectEpsilon.Progression; // 보상과 경험치
using ProjectEpsilon.UI; // 선택 화면
using UnityEditor; // 편집기 제어
using UnityEditor.SceneManagement; // 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 갱신 연결
    public static class AutomatedEliteRewardVerification // 엘리트 보상 시험
    { // 클래스 시작
        private const string ActiveKey = "EpsilonEliteRewardVerification"; // 시험 상태 표식
        private static double started; // 실제 시작 시각
        private static int stage; // 검증 단계
        private static int previousWeaponCount; // 보상 전 무기 수

        static AutomatedEliteRewardVerification() // 도메인 연결
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 진행 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 검증 갱신 연결
            } // 조건 끝
        } // 생성자 끝

        public static void Run() // 배치 진입점
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 시작 기록
            EditorApplication.update -= Tick; // 중복 연결 해제
            EditorApplication.update += Tick; // 검증 갱신 연결
            EditorApplication.isPlaying = true; // 플레이 진입
        } // 메서드 끝

        private static void Tick() // 단계별 검증
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 플레이 시작 대기
            } // 조건 끝
            if (started == 0d) // 최초 갱신 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 시작 시간 저장
            } // 조건 끝
            try // 검증 실패 처리
            { // 보호 시작
                if (EditorApplication.timeSinceStartup - started > 45d) // 실행 제한 확인
                { // 조건 시작
                    throw new InvalidOperationException("Elite verification timed out."); // 제한 시간 실패
                } // 조건 끝
                GameManager game = GameManager.Instance; // 게임 관리자
                EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 출현기
                EliteRewardController reward = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 보상 처리기
                SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 무기 관리자
                SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 몸통 관리자
                if (game == null || spawner == null || reward == null || weapons == null || body == null || Time.time < 0.5f) // 초기화 상태 확인
                { // 조건 시작
                    return; // 초기화 대기
                } // 조건 끝
                BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic; // 비공개 멤버 조회
                if (stage == 0) // 강제 출현 단계
                { // 조건 시작
                    Require(body.CurrentBodyCount == 3, "Starting body count is not three."); // 시작 몸통 확인
                    typeof(EarlyEnemyWaveSpawner).GetField("elapsed", flags).SetValue(spawner, 240f); // 인위적인 4분 시점 설정
                    typeof(EarlyEnemyWaveSpawner).GetField("nextSpawn", flags).SetValue(spawner, float.MaxValue); // 일반 출현 잠시 차단
                    Debug.Log("[EliteVerification] Forced spawner elapsed to 240 seconds; no full survival run."); // 인위 조건 기록
                    stage = 1; // 출현 대기
                    return; // 다음 갱신 대기
                } // 조건 끝
                if (stage == 1) // 엘리트 처치 단계
                { // 조건 시작
                    GameObject elite = GameObject.Find("EarlyEnemy_Elite"); // 실제 출현 엘리트 조회
                    if (elite == null) // 아직 출현 전 확인
                    { // 조건 시작
                        return; // 엘리트 출현 대기
                    } // 조건 끝
                    WeaponTarget target = elite.GetComponent<WeaponTarget>(); // 실제 전투 대상 조회
                    Require(target != null && target.IsAlive, "Elite target is missing."); // 생존 적 확인
                    previousWeaponCount = weapons.OccupiedSlotCount; // 보상 전 무기 수 저장
                    target.TakeDamage(100000f); // 인위적인 시험 처치
                    Require(reward.PendingRewardCount == 1, "Elite reward was not queued once."); // 보상 대기열 확인
                    ExperiencePickup[] pickups = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None); // 경험치 보석 조회
                    FieldInfo value = typeof(ExperiencePickup).GetField("experienceValue", flags); // 보석 경험치 필드
                    int largeCount = 0; // 대형 보석 수
                    foreach (ExperiencePickup pickup in pickups) // 보석 순회
                    { // 반복 시작
                        largeCount += (int)value.GetValue(pickup) == 20 ? 1 : 0; // 대형 보석 계수
                    } // 반복 끝
                    Require(largeCount == 1, "Elite did not drop exactly one 20-XP gem."); // 별도 경험치 확인
                    Debug.Log("[EliteVerification] Forced kill produced one queued reward and one 20-XP gem."); // 처치 결과 기록
                    stage = 2; // 선택 화면 대기
                    return; // 다음 갱신 대기
                } // 조건 끝
                if (stage == 2) // 보상 화면 단계
                { // 조건 시작
                    if (!reward.IsPresentingReward) // 보상 표시 확인
                    { // 조건 시작
                        return; // 선택 화면 대기
                    } // 조건 끝
                    Require(game.IsPaused && Mathf.Approximately(Time.timeScale, 0f), "Elite reward did not pause gameplay."); // 보상 정지 확인
                    Require(reward.Candidates.Count == 3, "Elite reward did not present three candidates."); // 후보 세 개 확인
                    Require(reward.Candidates[0] != reward.Candidates[1] && reward.Candidates[1] != reward.Candidates[2] && reward.Candidates[0] != reward.Candidates[2], "Elite reward candidates repeat."); // 후보 중복 확인
                    int weaponIndex = -1; // 무기 후보 위치
                    for (int index = 0; index < reward.Candidates.Count; index++) // 후보 순회
                    { // 반복 시작
                        if (reward.Candidates[index] == 2) // 무기 후보 확인
                        { // 조건 시작
                            weaponIndex = index; // 무기 후보 위치 저장
                        } // 조건 끝
                    } // 반복 끝
                    Require(weaponIndex >= 0, "Usable weapon reward is absent."); // 적용 가능 후보 확인
                    LevelUpPanelController panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 선택 화면 조회
                    Require(panel != null && panel.gameObject.activeInHierarchy, "Elite reward panel is not visible."); // 화면 활성 확인
                    string[] buttonNames = { "candidateButton01", "candidateButton02", "candidateButton03" }; // 후보 버튼 필드
                    UnityEngine.UI.Button button = (UnityEngine.UI.Button)typeof(LevelUpPanelController).GetField(buttonNames[weaponIndex], flags).GetValue(panel); // 실제 무기 버튼 조회
                    Require(button != null && button.interactable, "Weapon reward button is not selectable."); // 버튼 선택 가능 확인
                    button.onClick.Invoke(); // 실제 버튼 이벤트 실행
                    Require(reward.PendingRewardCount == 0 && !reward.IsPresentingReward, "Reward was not consumed once."); // 단일 보상 소비 확인
                    Require(game.IsPlaying && Mathf.Approximately(Time.timeScale, 1f), "Gameplay did not resume normally."); // 게임 재개 확인
                    Require(weapons.OccupiedSlotCount == previousWeaponCount + 1, "Weapon reward was not equipped."); // 무기 장착 확인
                    Debug.Log("[EliteVerification] PASS: visible three distinct candidates, paused game, selected actual weapon button, equipped reward, resumed."); // 성공 기록
                    Finish(0); // 성공 종료
                } // 조건 끝
            } // 보호 끝
            catch (Exception exception) // 예외 확인
            { // 예외 시작
                Debug.LogError("[EliteVerification] " + exception); // 실패 로그
                Finish(1); // 실패 종료
            } // 예외 끝
        } // 메서드 끝

        private static void Require(bool condition, string message) // 검증 조건
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 전달
            } // 조건 끝
        } // 메서드 끝

        private static void Finish(int code) // 검증 종료
        { // 메서드 시작
            SessionState.SetBool(ActiveKey, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 갱신 연결 해제
            EditorApplication.Exit(code); // 종료 결과 반환
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
