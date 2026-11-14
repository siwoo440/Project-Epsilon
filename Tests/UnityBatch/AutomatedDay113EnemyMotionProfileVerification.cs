using System; // 시험 실패 처리
using System.Reflection; // 실제 적 생성과 모션 조회
using ProjectEpsilon.Combat; // 실제 공격 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 행동
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay113EnemyMotionProfileVerification // 실제 적별 모션 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay113EnemyMotionProfiles"; // 시험 상태 표식
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 기능 조회
        private static bool started; // 중복 실행 방지
        static AutomatedDay113EnemyMotionProfileVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 실제 적별 모션 시험
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
            try // 실제 적별 모션 시험
            { // 시험 시작
                Verify(); // 실제 출현 모션 확인
                Finish(true, "Actual enemy movement profiles differed and preserved colliders."); // 실제 성공 결과
            } // 시험 종료
            catch (Exception error) // 실제 실패 확인
            { // 실패 시작
                Finish(false, error.ToString()); // 실제 실패 결과
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 적 출현 경로와 모션 확인
        { // 함수 시작
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            Require(spawner != null, "실제 적 출현기 누락"); // 실제 구성 확인
            spawner.enabled = false; // 자연 적 출현 중지
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", Private); // 실제 적 생성 경로
            FieldInfo frequency = typeof(EnemyVisualMotion).GetField("movementFrequency", Private); // 실제 움직임 속도
            Require(spawn != null && frequency != null, "실제 적 생성 또는 모션 설정 누락"); // 실제 내부 구성 확인
            EnemyVisualMotion chaser = Spawn(spawner, spawn, EarlyEnemyKind.Chaser, Vector3.right * 30f); // 실제 추적자 생성
            EnemyVisualMotion flyer = Spawn(spawner, spawn, EarlyEnemyKind.Flyer, Vector3.right * 40f); // 실제 비행 적 생성
            EnemyVisualMotion armored = Spawn(spawner, spawn, EarlyEnemyKind.Armored, Vector3.right * 50f); // 실제 장갑 적 생성
            float chaserSpeed = (float)frequency.GetValue(chaser); // 실제 추적자 움직임 속도
            float flyerSpeed = (float)frequency.GetValue(flyer); // 실제 비행 적 움직임 속도
            float armoredSpeed = (float)frequency.GetValue(armored); // 실제 장갑 적 움직임 속도
            Require(flyerSpeed > chaserSpeed && armoredSpeed < chaserSpeed, "적 행동 유형별 움직임 차이 누락"); // 실제 유형별 움직임 구분
            Vector3 rootScale = flyer.transform.localScale; // 실제 비행 적 충돌체 크기
            Quaternion rootRotation = flyer.transform.rotation; // 실제 비행 적 충돌체 방향
            flyer.PlayAttack(); // 실제 비행 적 공격 표시
            typeof(EnemyVisualMotion).GetMethod("LateUpdate", Private).Invoke(flyer, null); // 실제 표시 갱신
            Require(flyer.transform.localScale == rootScale && flyer.transform.rotation == rootRotation && flyer.transform.Find("EnemyArtwork").localScale != Vector3.one, "모션 표시 또는 충돌체 보존 실패"); // 실제 이미지와 물리 분리 확인
            typeof(EarlyEnemyWaveSpawner).GetMethod("EnterBossTransition", Private).Invoke(spawner, null); // 실제 보스 전환
            PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 관리자
            WeaponTarget boss = encounter == null ? null : (WeaponTarget)typeof(PrototypeBossEncounter).GetField("boss", Private).GetValue(encounter); // 실제 최종 보스
            EnemyVisualMotion bossMotion = boss == null ? null : boss.GetComponent<EnemyVisualMotion>(); // 실제 보스 모션
            Require(bossMotion != null && (float)frequency.GetValue(bossMotion) < chaserSpeed, "실제 보스 전용 움직임 누락"); // 실제 보스 표시 설정 확인
        } // 함수 종료
        private static EnemyVisualMotion Spawn(EarlyEnemyWaveSpawner spawner, MethodInfo method, EarlyEnemyKind kind, Vector3 position) // 실제 종류별 적 생성
        { // 함수 시작
            EarlyEnemySettings settings = new EarlyEnemySettings(); // 실제 적 행동 설정
            settings.kind = kind; // 실제 적 종류
            settings.health = 100f; // 실제 적 체력
            settings.attackInterval = 2f; // 실제 적 공격 주기
            WeaponTarget target = (WeaponTarget)method.Invoke(spawner, new object[] { settings, position, false }); // 실제 적 생성 경로
            Require(target != null && target.GetComponent<EnemyVisualMotion>() != null, "실제 적 모션 누락"); // 실제 표시 구성 확인
            return target.GetComponent<EnemyVisualMotion>(); // 실제 적 모션 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 원인 전달
            } // 조건 종료
        } // 함수 종료
        private static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day113EnemyMotionProfileVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
