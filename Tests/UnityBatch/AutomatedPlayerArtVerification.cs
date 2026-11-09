using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 적 설정 조회
using ProjectEpsilon.Combat; // 실제 보스 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어 구성
using UnityEditor; // 배치와 에셋 조회
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedPlayerArtVerification // 실제 플레이어 이미지 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonPlayerArtVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지
        private static double started; // 실제 시작 시각
        static AutomatedPlayerArtVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 시작 감시
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 씬 배치 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 실제 플레이 모드 시작
        } // 메서드 끝
        private static void Tick() // 실제 시작과 시간 제한 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            { // 조건 시작
                return; // 실제 진입 대기
            } // 조건 끝
            if (started <= 0d) // 시험 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 시간 제한
            { // 조건 시작
                Finish(1, "Timed out waiting for player art test."); // 시간 초과 실패
                return; // 감시 종료
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작과 실행 확인
            { // 조건 시작
                return; // 실제 시작 또는 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            { // 조건 시작
                return; // 실제 초기화 대기
            } // 조건 끝
            injected = true; // 실행기 중복 방지
            player.gameObject.AddComponent<PlayerArtVerificationRunner>(); // 실제 이미지 시험 시작
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 결과 기록
        { // 메서드 시작
            if (finished) // 중복 종료 확인
            { // 조건 시작
                return; // 중복 처리 방지
            } // 조건 끝
            finished = true; // 종료 상태 저장
            SessionState.SetBool(ActiveKey, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 실제 감시 해제
            Debug.Log($"[PlayerArtVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 프로세스 종료
        } // 메서드 끝
    } // 클래스 끝

    public sealed class PlayerArtVerificationRunner : MonoBehaviour // 실제 이미지 연결 시나리오
    { // 클래스 시작
        private IEnumerator Start() // 실제 플레이어와 적 이미지 검증
        { // 코루틴 시작
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 머리 이동
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 관리자
            Require(player != null && body != null && spawner != null && encounter != null, "Actual player art fixtures missing."); // 실제 필수 구성 확인
            player.enabled = false; // 실제 머리 이동 통제
            spawner.enabled = false; // 일반 적 추가 출현 통제
            SpriteRenderer head = player.GetComponent<SpriteRenderer>(); // 실제 머리 표시기
            Require(IsSprite(head, "PlayerHead_Casual.png"), "Actual player head sprite was not assigned."); // 실제 머리 이미지 확인
            Require(head.color == Color.white, "Actual player head retained placeholder tint."); // 실제 머리 원본 색상 확인
            Require(body.CurrentBodyCount == 3, "Actual starting body count changed."); // 실제 시작 몸통 수 확인
            SpriteRenderer firstBody = body.BodySegments[0].GetComponent<SpriteRenderer>(); // 실제 첫 몸통 표시기
            float headSize = Mathf.Max(head.sprite.bounds.size.x * Mathf.Abs(head.transform.lossyScale.x), head.sprite.bounds.size.y * Mathf.Abs(head.transform.lossyScale.y)); // 회전 전 머리 최대 표시 크기
            float bodySize = Mathf.Max(firstBody.sprite.bounds.size.x * Mathf.Abs(firstBody.transform.lossyScale.x), firstBody.sprite.bounds.size.y * Mathf.Abs(firstBody.transform.lossyScale.y)); // 회전 전 몸통 최대 표시 크기
            CircleCollider2D headCollider = player.GetComponent<CircleCollider2D>(); // 실제 머리 충돌체
            CircleCollider2D bodyCollider = body.BodySegments[0].GetComponent<CircleCollider2D>(); // 실제 몸통 충돌체
            Require(Mathf.Abs(headSize - 1f) < 0.02f && Mathf.Abs(bodySize - 1f) < 0.02f && headCollider != null && Mathf.Approximately(headCollider.radius, 0.16f) && Mathf.Approximately(player.transform.localScale.x, 1.44f) && bodyCollider != null && Mathf.Approximately(bodyCollider.radius, 0.36f) && Mathf.Approximately(body.BodySegments[0].transform.localScale.x, 0.62f), $"Head or body visual is not one unit or hitboxes changed: head={headSize:F3}, body={bodySize:F3}."); // 두 표시 크기와 기존 충돌 범위 확인
            foreach (SnakeSegment segment in body.BodySegments) // 실제 시작 몸통 순회
            { // 반복 시작
                Require(IsSprite(segment.GetComponent<SpriteRenderer>(), "PlayerBody_Casual.png"), "Actual starting body sprite was not assigned."); // 실제 시작 몸통 이미지 확인
                Require(segment.GetComponent<SpriteRenderer>().color == Color.white && segment.GetComponent<CircleCollider2D>() != null, "Actual body tint or collider changed."); // 실제 몸통 색상과 충돌체 확인
            } // 반복 끝
            Require(IsSprite(body.TailSegment.GetComponent<SpriteRenderer>(), "PlayerTail_Casual.png"), "Actual separate tail sprite was not assigned."); // 실제 꼬리 이미지 확인
            Require(body.TailSegment.GetComponent<SpriteRenderer>().color == Color.white, "Actual tail retained placeholder tint."); // 실제 꼬리 원본 색상 확인
            Require(body.TryGainBodyFromLevelUp() && body.CurrentBodyCount == 4, "Actual body growth failed."); // 실제 몸통 성장 확인
            Require(IsSprite(body.BodySegments[3].GetComponent<SpriteRenderer>(), "PlayerBody_Casual.png"), "Actual grown body did not retain the correct sprite."); // 실제 성장 몸통 이미지 확인
            Require(IsSprite(body.TailSegment.GetComponent<SpriteRenderer>(), "PlayerTail_Casual.png"), "Actual body growth replaced the tail sprite."); // 실제 성장 후 꼬리 확인
            FieldInfo visual = typeof(EarlyEnemyWaveSpawner).GetField("visual", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 적 임시 이미지 필드
            Sprite enemyVisual = visual == null ? null : visual.GetValue(spawner) as Sprite; // 실제 적 출현 이미지
            Require(enemyVisual != null && enemyVisual != head.sprite, "Actual ordinary enemies inherited the player head sprite."); // 실제 적 이미지 분리 확인
            MethodInfo spawnBoss = typeof(PrototypeBossEncounter).GetMethod("TrySpawnBoss", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 보스 생성 경로
            Require(spawnBoss != null, "Actual boss creation path missing."); // 실제 생성 경로 확인
            spawnBoss.Invoke(encounter, null); // 실제 시험 보스 생성
            WeaponTarget boss = encounter.Boss; // 실제 생성 보스
            Require(boss != null && boss.GetComponent<SpriteRenderer>().sprite != head.sprite, "Actual boss inherited the player head sprite."); // 실제 보스 이미지 분리 확인
            yield return null; // 실제 씬 프레임 진행
            AutomatedPlayerArtVerification.Finish(0, "Actual player head, body, tail, growth and enemy placeholders passed."); // 실제 전체 결과
        } // 코루틴 끝
        private static bool IsSprite(SpriteRenderer renderer, string suffix) // 실제 연결 이미지 확인
        { // 메서드 시작
            return renderer != null && renderer.sprite != null && AssetDatabase.GetAssetPath(renderer.sprite).EndsWith(suffix, StringComparison.Ordinal); // 실제 에셋 경로 비교
        } // 메서드 끝
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실제 실패 조건 확인
            { // 조건 시작
                AutomatedPlayerArtVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
