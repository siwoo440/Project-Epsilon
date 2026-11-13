using System; // 예외와 형식
using System.Collections.Generic; // 실제 적 목록
using System.Reflection; // 실제 회차와 행동 조회
using ProjectEpsilon.Combat; // 실제 적 전투 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 설정
using ProjectEpsilon.Player; // 실제 몸통과 체력
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 전투 씬 열기
using UnityEngine; // Unity 기능
namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay81EliteVerification // 몸통 압박형과 강화형 시험
    { // 시험 시작
        private const string Key = "EpsilonDay81Elite"; // 시험 진행 표식
        private static bool started; // 중복 실행 방지
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 경로 조회
        static AutomatedDay81EliteVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투 준비 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 전투 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실제 전투 준비 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            try // 실제 엘리트 시험
            { // 시험 시작
                Verify(); // 실제 회차와 행동 확인
                Finish(0, "Body pressure elite schedule, nearest body targeting and contact damage passed."); // 성공 결과 기록
            } // 시험 종료
            catch (Exception error) // 실제 실패 확인
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 몸통 압박 확인
        { // 함수 시작
            Require(Enum.TryParse("BodyPressure", out ElitePattern pattern), "몸통 압박 행동 누락"); // 실제 행동 종류 확인
            EnemyWaveSettings wave = Resources.Load<EnemyWaveSettings>("EnemyWaveSettings"); // 실제 웨이브 데이터
            Require(wave != null, "실제 출현 데이터 누락"); // 실제 웨이브 설정 확인
            typeof(EnemyWaveSettings).GetField("enhancedEliteChance", Private).SetValue(wave, 0f); // 전용 회차를 확인할 시험 확률
            Require(wave != null && wave.AdditionalEliteRounds != null && wave.AdditionalEliteRounds.Length >= 3, "16분 엘리트 데이터 누락"); // 실제 회차 설정 확인
            EarlyEnemySettings settings = wave.AdditionalEliteRounds[2]; // 실제 몸통 압박형 데이터
            Require(settings != null && settings.kind == EarlyEnemyKind.Elite && settings.elitePattern == pattern && settings.enemySprite != null, "몸통 압박형 설정 또는 이미지 누락"); // 실제 행동과 원본 이미지 확인
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            Require(spawner != null && body != null && body.BodySegments.Count >= 2, "실제 전투와 몸통 구성 누락"); // 실제 전투 구성 확인
            spawner.enabled = false; // 자연 출현 통제
            typeof(EarlyEnemyWaveSpawner).GetField("nextEliteRound", Private).SetValue(spawner, 3); // 실제 16분 회차 준비
            typeof(EarlyEnemyWaveSpawner).GetField("elapsed", Private).SetValue(spawner, 960f); // 실제 16분 경계
            typeof(EarlyEnemyWaveSpawner).GetMethod("ProcessEliteSchedule", Private).Invoke(spawner, null); // 실제 엘리트 회차 실행
            List<WeaponTarget> targets = (List<WeaponTarget>)typeof(EarlyEnemyWaveSpawner).GetField("targets", Private).GetValue(spawner); // 실제 생존 적 목록
            WeaponTarget elite = targets.Find(item => item != null && item.GetComponent<EarlyEnemyBehaviour>() != null && item.GetComponent<SpriteRenderer>().sprite == settings.enemySprite); // 실제 몸통 압박형 조회
            Require(elite != null, "16분 몸통 압박형 출현 누락"); // 실제 회차 출현 확인
            body.BodySegments[0].transform.position = new Vector3(20f, 20f, 0f); // 첫 몸통 위치 통제
            body.BodySegments[1].transform.position = new Vector3(25f, 25f, 0f); // 두 번째 몸통 위치 통제
            elite.transform.position = new Vector3(25.2f, 25f, 0f); // 두 번째 몸통 근처 엘리트 위치
            Rigidbody2D rigidbody = elite.GetComponent<Rigidbody2D>(); // 실제 엘리트 물리체
            rigidbody.position = elite.transform.position; // 실제 물리 위치 동기화
            EarlyEnemyBehaviour behaviour = elite.GetComponent<EarlyEnemyBehaviour>(); // 실제 몸통 압박 행동
            Vector2 target = (Vector2)typeof(EarlyEnemyBehaviour).GetMethod("TargetPosition", Private).Invoke(behaviour, null); // 실제 추적 목표
            Require(Vector2.Distance(target, body.BodySegments[1].transform.position) < 0.01f, "최근접 몸통 추적 실패"); // 실제 몸통 목표 확인
            Collider2D collider = body.BodySegments[1].GetComponent<Collider2D>(); // 실제 몸통 충돌체
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어
            SnakeHealth health = player.GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeExternalCollision resolver = player.GetComponent<SnakeExternalCollision>(); // 실제 충돌 처리기
            int before = health.CurrentHealth; // 피해 전 실제 체력
            typeof(EarlyEnemyBehaviour).GetField("nextAttack", Private).SetValue(behaviour, 0f); // 실제 접촉 공격 준비
            typeof(EarlyEnemyBehaviour).GetMethod("OnTriggerStay2D", Private).Invoke(behaviour, new object[] { collider }); // 실제 몸통 접촉 처리
            typeof(SnakeExternalCollision).GetMethod("ResolvePendingContacts", Private).Invoke(resolver, null); // 실제 충돌 우선순위 처리
            Require(health.CurrentHealth < before, "몸통 압박 공용 피해 누락"); // 실제 몸통 접촉 피해 확인
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
            Debug.Log("[Day81Elite] " + (code == 0 ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
