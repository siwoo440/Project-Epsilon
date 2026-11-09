using System; // 실제 예외와 식별자
using System.Collections; // 실제 프레임 대기
using System.Reflection; // 실제 제품 상태 조회
using ProjectEpsilon.Combat; // 실제 전투 대상과 포탑
using ProjectEpsilon.Core; // 실제 게임 관리자
using ProjectEpsilon.Data; // 실제 포탑 데이터
using ProjectEpsilon.Enemies; // 실제 적과 보스
using ProjectEpsilon.Player; // 실제 플레이어
using ProjectEpsilon.Progression; // 실제 영구 성장
using ProjectEpsilon.World; // 실제 맵 선택
using UnityEditor; // 실제 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 실제 도메인 갱신 처리
    public static class AutomatedDay48MetaVerification // 실제 영구 성장과 재시작 검증
    { // 클래스 시작
        private const string Key = "EpsilonDay48MetaVerification"; // 실제 시험 표식
        private const string Phase = "EpsilonDay48MetaPhase"; // 실제 재시작 단계
        private static bool injected; // 시험 실행기 생성 여부
        private static double started; // 실제 시험 시작 시각
        static AutomatedDay48MetaVerification() // 실제 도메인 재시작
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 실제 진행 중 시험 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 시험 감시
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 Unity 배치 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(Key, true); // 실제 시험 진행 기록
            SessionState.SetInt(Phase, 0); // 실제 시험 단계 초기화
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        } // 메서드 끝
        private static void Tick() // 실제 시작과 재시작 확인
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            { // 조건 시작
                return; // 실제 진입 대기
            } // 조건 끝
            if (started <= 0d) // 실제 시험 시작 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 시간 제한
            { // 조건 시작
                Finish(1, "Timed out."); // 실제 시간 초과
                return; // 실제 감시 종료
            } // 조건 끝
            if (SessionState.GetInt(Phase, 0) == 1) // 실제 씬 재시작 대기 확인
            { // 조건 시작
                GameManager game = GameManager.Instance; // 새 실제 게임 관리자
                MetaProgressionController meta = MetaProgressionController.Instance; // 새 실제 메타 관리자
                if (game == null || meta == null || game.GetInstanceID() == SessionState.GetInt("EpsilonDay48OldManager", 0) || game.CurrentState != GameState.Starting || meta.Service == null) // 새 시작 씬 준비 확인
                { // 조건 시작
                    return; // 실제 씬 재시작 대기
                } // 조건 끝
                if (!meta.Service.FinishedFirstRun || meta.Service.Cores != 7 || !meta.IsMapUnlocked(1) || meta.IsMapUnlocked(2) || !meta.IsCharacterUnlocked("prototype_mobile") || !meta.Service.IsTurretAttributeUnlocked(0) || InputBindingManager.Instance == null) // 실제 재시작 저장과 입력 확인
                { // 조건 시작
                    Finish(1, $"Restart persistence failed: finished={meta.Service.FinishedFirstRun}, cores={meta.Service.Cores}, ruins={meta.IsMapUnlocked(1)}, mobile={meta.IsCharacterUnlocked("prototype_mobile")}, input={InputBindingManager.Instance != null}."); // 실제 재시작 실패
                    return; // 추가 처리 중단
                } // 조건 끝
                Finish(0, "Isolated storage, real kills and boss rewards, duplicate prevention and scene restart persistence passed."); // 실제 재시작 성공
                return; // 실제 시험 종료
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작 후보 확인
            { // 조건 시작
                return; // 실제 선택 또는 시험 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 런 초기화 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 끝
            injected = true; // 실제 중복 시험 방지
            player.gameObject.AddComponent<Day48MetaRunner>(); // 실제 시험 실행기 생성
        } // 메서드 끝
        public static void ExpectRestart(GameManager old) // 실제 재시작 감시 등록
        { // 메서드 시작
            SessionState.SetInt("EpsilonDay48OldManager", old.GetInstanceID()); // 이전 관리자 식별자
            SessionState.SetInt(Phase, 1); // 실제 재시작 대기 단계
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 시험 결과 기록
        { // 메서드 시작
            SessionState.SetBool(Key, false); // 실제 시험 표식 제거
            EditorApplication.update -= Tick; // 실제 시험 감시 해제
            Debug.Log($"[Day48MetaVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 프로세스 종료 코드
        } // 메서드 끝
    } // 클래스 끝
    public sealed class Day48MemoryStorage : IMetaStorage // 외부 저장과 분리한 시험 저장소
    { // 클래스 시작
        public string Content; // 실제 시험 저장 내용
        public bool FailWrite; // 실제 시험 저장 실패 설정
        public bool TryRead(out string json) // 실제 시험 저장 읽기
        { // 메서드 시작
            json = Content; // 실제 시험 저장 반환
            return true; // 실제 시험 읽기 성공
        } // 메서드 끝
        public bool TryWrite(string json) // 실제 시험 저장 기록
        { // 메서드 시작
            if (FailWrite) // 실제 실패 설정 확인
            { // 조건 시작
                return false; // 실제 쓰기 실패 재현
            } // 조건 끝
            Content = json; // 실제 시험 저장 갱신
            return true; // 실제 시험 쓰기 성공
        } // 메서드 끝
    } // 클래스 끝
    public sealed class Day48MetaRunner : MonoBehaviour // 실제 서비스와 씬 시나리오
    { // 클래스 시작
        private IEnumerator Start() // 실제 저장과 성과 시험
        { // 코루틴 시작
            Require(Application.productName == "Project-Epsilon-Day48-Isolated", "Batch project is not isolated from user save data."); // 실제 사용자 저장 격리 확인
            Day48MemoryStorage store = new Day48MemoryStorage(); // 실제 메모리 시험 저장소
            MetaProgressionService service = new MetaProgressionService(store); // 실제 제품 서비스
            Require(service.IsAvailable && service.Cores == 0 && !service.FinishedFirstRun && !service.IsMapUnlocked(1) && !service.TryPurchase(MetaNode.Health), "Initial progression or first-run lock failed."); // 실제 초기 상태 확인
            Require(service.TryCompleteRun("loss", 239.9d, 199, 2, false, 0) && service.Cores == 4 && service.FinishedFirstRun, "Defeat or abandon reward calculation failed."); // 실제 정수 성과 보상 확인
            Require(service.TryCompleteRun("loss", 239.9d, 199, 2, false, 0) && service.Cores == 4, "Duplicate run awarded twice."); // 실제 런 중복 지급 방지
            Require(service.TryCompleteRun("win", 1200d, 200, 2, true, 0) && service.Cores == 23 && service.IsMapUnlocked(1) && service.IsCharacterUnlocked(0), "Boss reward or plains unlock failed."); // 실제 보스 보상과 해금 확인
            MetaProgressionService reloaded = new MetaProgressionService(store); // 실제 저장 재복원
            Require(reloaded.Cores == 23 && reloaded.FinishedFirstRun && reloaded.IsMapUnlocked(1), "Memory storage reload lost progression."); // 실제 저장 복원 확인
            Require(!reloaded.TryPurchase(MetaNode.Healing) && !reloaded.TryPurchase(MetaNode.LastBodySpeed), "Node prerequisites were bypassed."); // 실제 선행 조건 확인
            Require(reloaded.TryPurchase(MetaNode.Health) && reloaded.Cores == 20 && reloaded.GetLevel(MetaNode.Health) == 1, "First node purchase or cost failed."); // 실제 첫 단계 비용 확인
            Require(reloaded.TryPurchase(MetaNode.Health) && reloaded.Cores == 15 && reloaded.TryPurchase(MetaNode.Healing) && reloaded.Cores == 12, "Second cost or branch prerequisite failed."); // 실제 두 번째 비용과 선행 조건 확인
            store.FailWrite = true; // 실제 저장 실패 재현
            Require(!reloaded.TryPurchase(MetaNode.Health) && reloaded.Cores == 12 && reloaded.GetLevel(MetaNode.Health) == 2, "Failed save committed a purchase."); // 실제 저장 실패 원자성 확인
            store.FailWrite = false; // 실제 시험 저장 복구
            Require(reloaded.TryRefundAll() && reloaded.Cores == 23 && reloaded.GetLevel(MetaNode.Health) == 0 && reloaded.GetLevel(MetaNode.Healing) == 0, "Free reset did not refund exact costs."); // 실제 무료 초기화 확인
            Require(reloaded.TryCompleteRun("ruins", 0d, 0, 0, true, 1) && reloaded.IsMapUnlocked(2) && reloaded.Cores == 28, "Ruins clear did not unlock swamp."); // 실제 두 번째 맵 해금 확인
            Require(reloaded.TryUnlockTurretAttribute(3) && new MetaProgressionService(store).IsTurretAttributeUnlocked(3), "Turret unlock did not persist."); // 실제 속성 포탑 저장 확인
            Day48MemoryStorage bad = new Day48MemoryStorage { Content = "not-json" }; // 실제 손상 저장 재현
            MetaProgressionService unavailable = new MetaProgressionService(bad); // 실제 손상 저장 복원
            Require(!unavailable.IsAvailable && !unavailable.TryCompleteRun("bad", 0d, 0, 0, false, 0) && bad.Content == "not-json", "Corrupt storage was overwritten."); // 실제 손상 원본 보존 확인
            Day48MemoryStorage incomplete = new Day48MemoryStorage { Content = "{}" }; // 실제 필드 누락 저장 재현
            MetaProgressionService incompleteService = new MetaProgressionService(incomplete); // 실제 누락 저장 복원
            Require(!incompleteService.IsAvailable && !incompleteService.TryCompleteRun("bad", 0d, 0, 0, false, 0) && incomplete.Content == "{}", "Incomplete storage was overwritten."); // 실제 누락 원본 보존 확인
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 씬 진행 관리자
            GameManager game = GameManager.Instance; // 실제 게임 관리자
            EarlyEnemyWaveSpawner wave = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            PrototypeBossEncounter bossEncounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 출현기
            Require(meta != null && game != null && wave != null && weapons != null && bossEncounter != null, "Actual scene progression fixtures missing."); // 실제 씬 구성 확인
            PlayerPrefs.DeleteKey("ProjectEpsilon.MetaProgress.v1"); // 격리된 시험 프로젝트의 메타 키 초기화
            PlayerPrefs.Save(); // 실제 격리 저장 반영
            typeof(MetaProgressionController).GetProperty("Service").SetValue(meta, new MetaProgressionService(new PlayerPrefsMetaStorage("ProjectEpsilon.MetaProgress.v1"))); // 실제 격리 저장소 재연결
            Require(!meta.IsMapUnlocked(1) && !meta.IsCharacterUnlocked("prototype_mobile"), "Initial map or character lock failed."); // 실제 시작 해금 제한 확인
            WeaponData gated = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/PrototypePhysicalJavelin.asset"); // 실제 시험 잠금 포탑
            Require(gated != null && !meta.IsTurretUnlocked(gated), "Initial test turret lock failed."); // 실제 시작 포탑 제한 확인
            GetComponent<SnakeMovement>().enabled = false; // 실제 플레이어 이동 중지
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기
            if (follower != null) // 실제 추적기 확인
            { // 조건 시작
                follower.enabled = false; // 실제 몸통 이동 통제
            } // 조건 끝
            wave.enabled = false; // 실제 자연 적 출현 통제
            weapons.enabled = false; // 실제 자동 공격 통제
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 시작 슬롯 순회
            { // 반복 시작
                slot.Clear(); // 실제 시작 포탑 제거
            } // 반복 끝
            WeaponData blade = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 물리 포탑
            WeaponData needle = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugNeedle.asset"); // 실제 두 번째 물리 포탑
            Require(weapons.TryEquipAt(0, blade) && weapons.TryEquipAt(1, needle) && meta.IsTurretUnlocked(gated), "Actual synergy did not unlock its configured turret."); // 실제 속성 해금 확인
            FieldInfo enemies = typeof(EarlyEnemyWaveSpawner).GetField("enemies", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 적 시험 데이터
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 적 생성 경로
            EarlyEnemySettings[] settings = (EarlyEnemySettings[])enemies.GetValue(wave); // 실제 적 출현 설정
            WeaponTarget normal = (WeaponTarget)spawn.Invoke(wave, new object[] { settings[0], new Vector3(500f, 500f, 0f), false }); // 실제 일반 적
            WeaponTarget elite = (WeaponTarget)spawn.Invoke(wave, new object[] { settings[3], new Vector3(510f, 500f, 0f), false }); // 실제 엘리트
            WeaponTarget summon = (WeaponTarget)spawn.Invoke(wave, new object[] { settings[0], new Vector3(520f, 500f, 0f), true }); // 실제 무보상 소환 적
            normal.TakeDamage(10000f); // 실제 일반 적 처치
            elite.TakeDamage(10000f); // 실제 엘리트 처치
            summon.TakeDamage(10000f); // 실제 무보상 소환 적 처치
            Require(meta.Kills == 2 && meta.Elites == 1, $"Actual eligible kills mismatch: kills={meta.Kills}, elites={meta.Elites}."); // 실제 보상 대상 집계 확인
            typeof(EarlyEnemyWaveSpawner).GetField("elapsed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(wave, 120.01f); // 실제 보상 경계 시간 통제
            typeof(EarlyEnemyWaveSpawner).GetField("previousClockTime", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(wave, Time.timeAsDouble); // 실제 경계 시각 통제
            typeof(PrototypeBossEncounter).GetMethod("TrySpawnBoss", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(bossEncounter, null); // 실제 보스 생성 통제
            Require(bossEncounter.Boss != null, "Actual final boss missing."); // 실제 보스 등록 확인
            bossEncounter.Boss.TakeDamage(10000f); // 실제 최종 보스 처치
            Require(game.CurrentState == GameState.Victory && meta.Service.Cores == 7 && meta.Service.FinishedFirstRun && meta.IsMapUnlocked(1) && meta.IsCharacterUnlocked("prototype_mobile"), $"Actual run reward failed: state={game.CurrentState}, cores={meta.Service.Cores}."); // 실제 종료와 보상 확인
            game.SetState(GameState.Victory); // 실제 중복 종료 호출
            Require(meta.Service.Cores == 7, "Actual scene awarded a run twice."); // 실제 중복 지급 방지 확인
            AutomatedDay48MetaVerification.ExpectRestart(game); // 실제 재시작 감시 등록
            Require(game.RestartRun(), "Actual scene restart was rejected."); // 실제 재시작 요청
            yield return null; // 실제 재시작 프레임 대기
        } // 코루틴 끝
        private static void Require(bool condition, string message) // 실제 시험 조건
        { // 메서드 시작
            if (!condition) // 실제 실패 확인
            { // 조건 시작
                AutomatedDay48MetaVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 실제 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
