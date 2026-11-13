using System; // 예외 처리
using System.Reflection; // 실제 사격 단계 조회
using ProjectEpsilon.Combat; // 적 전투 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 행동
using ProjectEpsilon.Player; // 실제 플레이어
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 전투 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay73ShooterWarningVerification // 사격 예고 시험
    { // 시험 시작
        private const string Key = "EpsilonDay73ShooterWarning"; // 시험 진행 표식
        private static bool started; // 중복 실행 방지
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 상태 조회
        static AutomatedDay73ShooterWarningVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투 준비 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 전투 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 전투 준비 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 시작 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection()) // 시작 상태 확인
            { // 조건 시작
                return; // 전투 준비 대기
            } // 조건 종료
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 전투 준비 확인
            { // 조건 시작
                return; // 전투 준비 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            try // 실제 사격 예고 시험
            { // 시험 시작
                Verify(); // 사격 예고와 발사 검증
                Finish(0, "Shooter warning tracks the target, respects projectile reach, disappears on fire and disable, and preserves the shot interval."); // 성공 기록
            } // 시험 종료
            catch (Exception error) // 시험 실패 처리
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 사격 흐름 확인
        { // 함수 시작
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기
            Require(spawner != null && player != null && follower != null && body != null, "필수 전투 구성 누락"); // 시험 구성 확인
            spawner.enabled = false; // 자연 출현 정지
            player.enabled = false; // 플레이어 자동 이동 정지
            follower.enabled = false; // 몸통 목표 고정
            EarlyEnemySettings[] settings = (EarlyEnemySettings[])typeof(EarlyEnemyWaveSpawner).GetField("enemies", Private).GetValue(spawner); // 실제 씬 적 설정
            Require(settings != null && settings.Length > 2 && settings[2].kind == EarlyEnemyKind.Shooter && settings[2].projectileWarningDuration > 0f, "실제 사수 예고 설정 누락"); // 실제 사수 예고 시간 확인
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", Private); // 실제 적 생성 경로
            Vector3 position = player.transform.position + Vector3.right * 20f; // 접촉 밖 사수 위치
            WeaponTarget enemy = (WeaponTarget)spawn.Invoke(spawner, new object[] { settings[2], position, false }); // 실제 사수 생성
            EarlyEnemyBehaviour behaviour = enemy.GetComponent<EarlyEnemyBehaviour>(); // 실제 사수 행동
            MethodInfo update = typeof(EarlyEnemyBehaviour).GetMethod("FixedUpdate", Private); // 실제 물리 단계
            FieldInfo next = typeof(EarlyEnemyBehaviour).GetField("nextAttack", Private); // 다음 실제 사격 시각
            MethodInfo target = typeof(EarlyEnemyBehaviour).GetMethod("ShooterTargetPosition", Private); // 실제 몸통 목표 조회
            next.SetValue(behaviour, Time.time + settings[2].projectileWarningDuration * 0.5f); // 사격 예고 구간 진입
            update.Invoke(behaviour, null); // 실제 예고 단계 실행
            LineRenderer line = enemy.GetComponent<LineRenderer>(); // 실제 사격 예고선
            Require(line != null && line.enabled && line.sortingOrder > 30 && line.startColor.g > 0.5f, "사격 예고 표시 누락"); // 예고 색상과 순서 확인
            Vector2 goal = (Vector2)target.Invoke(behaviour, null); // 현재 실제 사격 목표
            Vector2 shown = line.GetPosition(1) - line.GetPosition(0); // 실제 표시 방향
            Require(Vector2.Dot(shown.normalized, (goal - (Vector2)enemy.transform.position).normalized) > 0.999f, "사격 목표와 예고 방향 불일치"); // 실제 발사 방향 확인
            float speed = (float)typeof(EarlyEnemyBehaviour).GetField("projectileSpeed", Private).GetValue(behaviour); // 실제 투사체 속도
            float lifetime = (float)typeof(EarlyEnemyBehaviour).GetField("projectileLifetime", Private).GetValue(behaviour); // 실제 투사체 수명
            Require(Mathf.Abs(shown.magnitude - Mathf.Min(Vector2.Distance(goal, enemy.transform.position), speed * lifetime)) < 0.01f, "사격 예고 거리 불일치"); // 실제 투사체 도달 거리 확인
            foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 목표 순회
            { // 반복 시작
                segment.transform.position += Vector3.up * 6f; // 실제 몸통 목표 이동
            } // 반복 종료
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            update.Invoke(behaviour, null); // 변경된 예고 방향 갱신
            Vector2 newGoal = (Vector2)target.Invoke(behaviour, null); // 변경 후 실제 사격 목표
            Vector2 newShown = line.GetPosition(1) - line.GetPosition(0); // 갱신된 예고 방향
            Require(Vector2.Dot(newShown.normalized, (newGoal - (Vector2)enemy.transform.position).normalized) > 0.999f && Vector2.Dot(newShown.normalized, shown.normalized) < 0.999f, "이동 후 사격 예고 갱신 실패"); // 변경된 방향 반영 확인
            int before = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None).Length; // 기존 투사체 수
            next.SetValue(behaviour, Time.time - 0.01f); // 실제 사격 시각 도달
            update.Invoke(behaviour, null); // 실제 사격 실행
            int after = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None).Length; // 사격 후 투사체 수
            Require(after == before + 1 && !line.enabled, "실제 사격 또는 예고 종료 실패"); // 발사와 예고 제거 확인
            Require(Mathf.Abs((float)next.GetValue(behaviour) - (Time.time + settings[2].attackInterval)) < 0.001f, "기존 공격 간격 변경"); // 기존 사격 주기 확인
            next.SetValue(behaviour, Time.time + settings[2].projectileWarningDuration * 0.5f); // 예고 단계 재진입
            update.Invoke(behaviour, null); // 예고 다시 표시
            Require(line.enabled, "사격 예고 재사용 실패"); // 반복 사격 예고 확인
            WeaponTargetKnockbackController knockback = enemy.GetComponent<WeaponTargetKnockbackController>(); // 실제 사수 넉백 상태
            Require(knockback != null && knockback.Apply(Vector2.right, 1f, 0.5f), "넉백 시험 설정 실패"); // 실제 넉백 적용
            update.Invoke(behaviour, null); // 실제 넉백 이동 실행
            Require(!line.enabled, "넉백 중 사격 예고 잔류"); // 이동 중 오래된 예고 제거 확인
            knockback.Clear(); // 시험 넉백 종료
            update.Invoke(behaviour, null); // 사격 예고 복구
            Require(line.enabled, "넉백 후 사격 예고 복구 실패"); // 실제 예고 재생성 확인
            behaviour.enabled = false; // 실제 적 행동 중지
            Require(!line.enabled, "중지된 적의 예고 잔류"); // 비활성 예고 제거 확인
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
            if (code == 0) // 성공 여부 확인
            { // 조건 시작
                Debug.Log("[Day73ShooterWarning] PASS: " + message); // 성공 결과 기록
            } // 조건 종료
            else // 실패 여부 확인
            { // 조건 시작
                Debug.LogError("[Day73ShooterWarning] FAIL: " + message); // 실패 결과 기록
            } // 조건 종료
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
