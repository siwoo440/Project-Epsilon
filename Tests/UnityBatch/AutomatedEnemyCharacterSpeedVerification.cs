using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 HUD 필드 조회
using ProjectEpsilon.Combat; // 무기와 표적
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어와 체력
using ProjectEpsilon.Progression; // 실제 경험치
using ProjectEpsilon.UI; // 실제 HUD
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.UI; // 실제 HUD 텍스트

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedEnemyCharacterSpeedVerification // 캐릭터별 적 속도 독립 시험
    { // 클래스 시작
        private const string Key = "EpsilonEnemyCharacterSpeedVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedEnemyCharacterSpeedVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 진행 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 플레이 감시 연결
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 씬 시험 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 씬 열기
            SessionState.SetBool(Key, true); // 시험 진행 기록
            EditorApplication.update -= Tick; // 중복 감시 제거
            EditorApplication.update += Tick; // 플레이 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        } // 메서드 끝
        private static void Tick() // 실제 초기화 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 플레이 대기
            } // 조건 끝
            if (started <= 0d) // 시험 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 시작 시각 저장
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 120d) // 시험 제한 시간 확인
            { // 조건 시작
                Finish(1, "Timed out."); // 시간 초과 실패
                return; // 시험 중단
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 시작 선택과 실행 여부 확인
            { // 조건 시작
                return; // 시작 또는 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 실행 중복 방지
            player.gameObject.AddComponent<EnemyCharacterSpeedVerificationRunner>(); // 실제 시험 실행
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 시험 종료
        { // 메서드 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[EnemyCharacterSpeedVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        } // 메서드 끝
    } // 클래스 끝
    public sealed class EnemyCharacterSpeedVerificationRunner : MonoBehaviour // 실제 적 속도 독립 검증
    { // 클래스 시작
        private IEnumerator Start() // 실제 물리 이동 검증
        { // 코루틴 시작
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동기
            player.enabled = false; // 플레이어 위치 고정
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            spawner.enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 시험 중 자동 공격 중지
            foreach (WeaponTarget existing in UnityEngine.Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 자연 적 순회
            { // 반복 시작
                existing.gameObject.SetActive(false); // 시험 외 대상 제거
            } // 반복 끝
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 설정 조회 범위
            EarlyEnemySettings[] settings = (EarlyEnemySettings[])typeof(EarlyEnemyWaveSpawner).GetField("enemies", flags).GetValue(spawner); // 실제 적 데이터
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", flags); // 실제 적 생성 경로
            PropertyInfo active = typeof(CharacterRunInitializer).GetProperty("ActiveCharacter"); // 실제 현재 캐릭터 속성
            MethodInfo setActive = active == null ? null : active.GetSetMethod(true); // 시험용 현재 캐릭터 설정
            Require(settings != null && settings.Length > 0 && spawn != null && setActive != null, "Enemy character-speed fixtures missing."); // 실제 시험 구성 확인
            string[] names = { "Mobile", "Heavy" }; // 실제 속도가 다른 캐릭터
            float[] expectedPlayerSpeeds = { 3.3f, 2.7f }; // 실제 캐릭터별 이동 속도
            for (int index = 0; index < names.Length; index++) // 캐릭터별 실제 적 속도 순회
            { // 반복 시작
                CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Data/Characters/Prototype" + names[index] + ".asset"); // 실제 캐릭터 데이터
                Require(data != null, "Character asset missing: " + names[index]); // 실제 캐릭터 데이터 확인
                setActive.Invoke(null, new object[] { data }); // 테스트용 현재 캐릭터 설정
                Require(Mathf.Abs(player.MoveSpeed - expectedPlayerSpeeds[index]) < 0.01f, "Player speed did not change: " + names[index]); // 실제 캐릭터 속도 보정 확인
                WeaponTarget enemy = (WeaponTarget)spawn.Invoke(spawner, new object[] { settings[0], new Vector3(20f, 0f, 0f), false }); // 실제 추적 적 생성
                yield return new WaitForFixedUpdate(); // 실제 첫 물리 이동 대기
                Vector3 before = enemy.transform.position; // 실제 시작 위치
                float start = Time.fixedTime; // 실제 시작 물리 시각
                for (int step = 0; step < 20; step++) // 실제 이동 프레임 순회
                { // 반복 시작
                    yield return new WaitForFixedUpdate(); // 실제 물리 이동 대기
                } // 반복 끝
                float measured = Vector3.Distance(before, enemy.transform.position) / (Time.fixedTime - start); // 실제 평균 적 속도
                float expected = 3f * settings[0].speedRatio * 0.5f; // 캐릭터와 무관한 기본 속도의 절반
                Require(Mathf.Abs(measured - expected) < 0.05f, "Enemy speed changed with " + names[index] + ": measured=" + measured + ", expected=" + expected); // 캐릭터와 독립된 적 속도 확인
                enemy.gameObject.SetActive(false); // 이전 적 접촉 차단
                Destroy(enemy.gameObject); // 실제 시험 적 제거
            } // 반복 끝
            AutomatedEnemyCharacterSpeedVerification.Finish(0, "Actual chaser speed stayed fixed for mobile and heavy characters."); // 전체 성공 기록
        } // 코루틴 끝
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedEnemyCharacterSpeedVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
