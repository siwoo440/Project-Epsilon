using System; // 예외 형식
using System.Collections.Generic; // 적 목록 형식
using System.Reflection; // 비공개 설정 접근
using ProjectEpsilon.Combat; // 적 체력 구성
using ProjectEpsilon.Enemies; // 초반 적 구성
using ProjectEpsilon.Player; // 플레이어 구성
using ProjectEpsilon.Progression; // 보석 드롭 구성
using UnityEditor; // 편집기 기능
using UnityEditor.SceneManagement; // 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 편집기 영역
{
    public static class ProjectEpsilonEarlyEnemyVerification // 초반 적 검증
    {
        public static void Run() // 검증 실행
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 게임 씬 열기
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 출현기 조회
            Require(spawner != null, "초반 적 출현기가 Game 씬에 연결되지 않음"); // 씬 연결 확인
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 머리 조회
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 몸통 조회
            SnakeHealth health = UnityEngine.Object.FindFirstObjectByType<SnakeHealth>(); // 체력 조회
            Require(player != null && body != null && health != null, "플레이어 연결 누락"); // 씬 구성 확인
            Type type = typeof(EarlyEnemyWaveSpawner); // 출현기 형식 조회
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 구성 접근 설정
            type.GetField("player", flags).SetValue(spawner, player); // 시험용 머리 연결
            type.GetField("body", flags).SetValue(spawner, body); // 시험용 몸통 연결
            type.GetField("health", flags).SetValue(spawner, health); // 시험용 체력 연결
            EarlyEnemySettings fixture = new EarlyEnemySettings // 독립 적 설정
            {
                kind = EarlyEnemyKind.Predator, // 포식자 행동
                health = 42f, // 시험 체력
                damage = 12, // 시험 피해
                speedRatio = 0.9f, // 시험 속도 배율
                attackInterval = 1.5f, // 시험 공격 간격
                experience = 2 // 시험 경험치
            };
            List<WeaponTarget> targets = (List<WeaponTarget>)type.GetField("targets", flags).GetValue(spawner); // 생성 대상 목록 조회
            try // 시험 오브젝트 정리 보장
            {
                object[] arguments = new object[] // 출현 매개변수
                {
                    fixture // 시험 적 설정
                };
                type.GetMethod("Spawn", flags).Invoke(spawner, arguments); // 실제 출현 실행
                Require(targets.Count == 1 && targets[0] != null, "적 생성 누락"); // 생성 결과 확인
                WeaponTarget target = targets[0]; // 생성된 적 조회
                Require(Mathf.Approximately(target.MaximumHealth, 42f), "설정 체력 미적용"); // 체력 설정 확인
                Require(target.GetComponent<EarlyEnemyBehaviour>() != null, "적 행동 누락"); // 적 행동 연결 확인
                Require(target.GetComponent<SnakeContactHazard>() != null, "머리 직접 충돌 판정 누락"); // 직접 충돌 연결 확인
                EnemyDropController drops = target.GetComponent<EnemyDropController>(); // 경험치 드롭 조회
                Require(drops != null, "경험치 드롭 누락"); // 보상 연결 확인
                SerializedObject serializedDrops = new SerializedObject(drops); // 드롭 설정 조회
                Require(serializedDrops.FindProperty("experienceValue").intValue == 2, "경험치 설정 오류"); // 경험치 총량 확인
                Require(serializedDrops.FindProperty("experienceGemCount").intValue == 2, "소형 보석 분할 오류"); // 두 보석 분할 확인
            }
            finally // 시험 결과 정리
            {
                foreach (WeaponTarget target in targets) // 생성 대상 순회
                {
                    if (target != null) // 대상 존재 확인
                    {
                        UnityEngine.Object.DestroyImmediate(target.gameObject); // 시험 대상 제거
                    }
                }
            }
            Debug.Log("초반 적 씬 연결과 생성 검증 완료"); // 검증 성공 표시
        }
        private static void Require(bool condition, string message) // 검증 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                throw new InvalidOperationException(message); // 검증 실패 보고
            }
        }
    }
}
