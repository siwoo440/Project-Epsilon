using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using ProjectEpsilon.Combat; // 실제 대상과 화상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 표시
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay94BurnFlashVerification // 화상과 피격 표시 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay94BurnFlash"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay94BurnFlashVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실제 전투 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시작 방지
            new GameObject("Day94BurnFlashRunner").AddComponent<Day94BurnFlashRunner>(); // 실제 색상 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day94BurnFlashVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day94BurnFlashRunner : MonoBehaviour // 실제 색상 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 적의 화상과 피격 확인
        { // 함수 시작
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            GameObject enemy = new GameObject("Day94Target"); // 격리된 실제 적
            SpriteRenderer original = enemy.AddComponent<SpriteRenderer>(); // 실제 원본 표시기
            Color baseColor = new Color(0.8f, 0.9f, 0.7f, 1f); // 복원 확인용 실제 색상
            original.color = baseColor; // 실제 원래 색상 적용
            WeaponTarget target = enemy.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(100f); // 시험 체력 설정
            enemy.AddComponent<EnemyHitFlash>(); // 실제 빨간 피격 표시
            enemy.AddComponent<EnemyVisualMotion>(); // 실제 표시 이미지와 모션
            target.TakeDamage(1f); // 실제 직접 피해
            WeaponTargetBurnStatus burn = enemy.AddComponent<WeaponTargetBurnStatus>(); // 실제 화상 상태
            Require(burn.Apply(target, 0, 3f, 1f), "실제 화상 적용 실패"); // 실제 화상 적용
            yield return null; // 실제 화상과 색상 갱신 대기
            yield return null; // 실제 이미지 동기화 대기
            Require(IsRed(original.color), "화상 지속 피해가 직접 피격의 빨간색을 덮음"); // 실제 피격 색상 확인
            yield return new WaitForSeconds(0.3f); // 실제 피격 시간 종료 대기
            Require(!IsRed(original.color) && original.color != baseColor, "화상 중 피격색 종료 또는 화상 표시 실패"); // 실제 화상 색상 확인
            target.TakeDamage(1f); // 화상 중 실제 직접 피해
            burn.Clear(); // 실제 피격 표시 중 화상 해제
            yield return null; // 실제 색상 갱신 대기
            Require(IsRed(original.color), "화상 해제가 진행 중인 빨간 피격 표시를 제거"); // 실제 피격 우선 확인
            yield return new WaitForSeconds(0.3f); // 실제 피격 시간 종료 대기
            Require(original.color == baseColor, "화상 해제 후 원래 적 색상 복원 실패"); // 실제 원래 색상 확인
            WeaponTarget nested = new GameObject("Day94NestedTarget").AddComponent<WeaponTarget>(); // 중첩 피해 시험 대상
            nested.Configure(100f); // 실제 시험 체력
            int calls = 0; // 실제 피해 알림 수
            nested.Damaged += damage => // 실제 중첩 피해 알림
            { // 알림 시작
                calls++; // 실제 알림 수 증가
                if (calls == 1) // 외부 지속 피해 확인
                { // 조건 시작
                    Require(nested.IsDamageOverTime && nested.IsReactionDamage, "지속 반응 피해의 알림 상태 누락"); // 실제 반응과 지속 상태 확인
                    nested.TakeDamage(1f); // 알림 도중 실제 직접 피해
                    Require(nested.IsDamageOverTime && nested.IsReactionDamage, "중첩 피해 뒤 외부 알림 상태 복구 실패"); // 실제 외부 상태 복구 확인
                } // 조건 종료
                else // 중첩 직접 피해 확인
                { // 조건 시작
                    Require(!nested.IsDamageOverTime && nested.IsReactionDamage, "중첩 직접 피해 종류 구분 실패"); // 실제 중첩 종류 확인
                } // 조건 종료
            }; // 알림 종료
            nested.TakeDamageOverTime(1f, true); // 실제 반응 지속 피해
            Require(calls == 2 && Mathf.Approximately(nested.CurrentHealth, 98f) && !nested.IsDamageOverTime && !nested.IsReactionDamage, "중첩 피해 결과 또는 상태 정리 실패"); // 실제 피해와 상태 복구 확인
            AutomatedDay94BurnFlashVerification.Finish(true, "Direct flash, burn tint, color restoration and nested damage passed."); // 실제 시험 결과
        } // 함수 종료
        private static bool IsRed(Color color) // 실제 피격 색상 판별
        { // 함수 시작
            return Mathf.Abs(color.r - 1f) < 0.001f && Mathf.Abs(color.g - 0.05f) < 0.001f && Mathf.Abs(color.b - 0.05f) < 0.001f; // 기존 빨간 피격색 확인
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay94BurnFlashVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
