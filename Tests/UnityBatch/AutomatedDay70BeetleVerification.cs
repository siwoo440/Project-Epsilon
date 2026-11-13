using System; // 예외 처리
using System.Collections; // 프레임 대기
using System.Collections.Generic; // 적 설정 목록
using System.Reflection; // 실제 출현과 행동 조회
using ProjectEpsilon.Combat; // 적 체력 대상
using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Enemies; // 적 출현과 행동
using ProjectEpsilon.Player; // 플레이어와 몸통
using ProjectEpsilon.Progression; // 경험치 보상
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 편집기 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedDay70BeetleVerification // 쇠똥구리 실제 씬 시험
    { // 시험 시작
        private const string Key = "EpsilonDay70Beetle"; // 시험 진행 표식
        private static bool injected; // 실행기 중복 방지
        private static double started; // 시험 시작 시각
        static AutomatedDay70BeetleVerification() // 재시작 후 시험 복원
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 진행 중 시험 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 시작 감시 연결
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 진입 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 전투 씬
            SessionState.SetBool(Key, true); // 진행 상태 기록
            EditorApplication.update += Tick; // 시작 감시 연결
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 진입 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 감시 시작
            if (!EditorApplication.isPlaying) return; // 플레이 진입 대기
            if (StartingWeaponBatchHelper.HandleStartingSelection()) return; // 시작 포탑 선택 처리
            if (started <= 0d) started = EditorApplication.timeSinceStartup; // 제한 시간 기준
            if (EditorApplication.timeSinceStartup - started > 90d) // 제한 시간 확인
            { // 조건 시작
                Finish(false, "Timed out."); // 제한 시간 실패
                return; // 후속 실행 중단
            } // 조건 종료
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어
            if (!injected && player != null && GameManager.Instance != null && GameManager.Instance.IsPlaying && Time.time > 0.35f) // 전투 초기화 확인
            { // 조건 시작
                injected = true; // 실행기 중복 방지
                player.gameObject.AddComponent<Day70BeetleVerificationRunner>(); // 실제 시험 실행
            } // 조건 종료
        } // 감시 종료
        public static void Finish(bool success, string message) // 결과와 종료 처리
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 감시 연결 해제
            Debug.Log("[Day70Beetle] " + (success ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(success ? 0 : 1); // 결과 코드 종료
        } // 함수 종료
    } // 시험 종료
    public sealed class Day70BeetleVerificationRunner : MonoBehaviour // 실제 쇠똥구리 동작 시험
    { // 실행기 시작
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 필드 조회 조건
        private IEnumerator Start() // 실제 시험 순서
        { // 코루틴 시작
            yield return null; // 씬 초기화 대기
            EnemyWaveSettings settings = Resources.Load<EnemyWaveSettings>("EnemyWaveSettings"); // 실제 웨이브 데이터
            PropertyInfo extraProperty = typeof(EnemyWaveSettings).GetProperty("AdditionalEnemies"); // 추가 적 설정 조회
            if (!Check(settings != null && extraProperty != null, "Additional enemy configuration is missing.")) yield break; // 추가 데이터 확인
            IReadOnlyList<EarlyEnemySettings> extra = extraProperty.GetValue(settings) as IReadOnlyList<EarlyEnemySettings>; // 실제 추가 적 목록
            if (!Check(extra != null && extra.Count >= 1 && extra[0] != null, "Beetle data is missing.")) // 실제 쇠똥구리 설정 확인
            { // 조건 시작
                yield break; // 설정 누락 시 시험 종료
            } // 조건 종료
            EarlyEnemySettings beetle = extra[0]; // 실제 쇠똥구리 데이터
            if (!Check(beetle.kind.ToString() == "Charger" && beetle.health == 30f && beetle.damage == 14 && Mathf.Approximately(beetle.speedRatio, 0.85f) && Mathf.Approximately(beetle.attackInterval, 2.4f) && beetle.experience == 2 && beetle.firstAppearance == 300f && beetle.enemySprite != null, "Beetle data differs from the provisional design.")) yield break; // 실제 기획값과 이미지 확인
            EarlyEnemyWaveSpawner wave = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            if (!Check(wave != null && body != null && body.BodySegments.Count > 0, "Actual enemy or body fixture missing.")) yield break; // 실제 씬 구성 확인
            bool recorded = false; // 새 적의 기록 대상 여부
            foreach (EarlyEnemySettings item in wave.RecordingEnemies) // 실제 런 기록용 적 설정 순회
            { // 반복 시작
                if (item == beetle) recorded = true; // 추가 적 기록 확인
            } // 반복 종료
            if (!Check(recorded, "Beetle was omitted from run recording settings.")) yield break; // 밸런스 기록의 추가 적 누락 방지
            wave.enabled = false; // 자동 출현과 시간 갱신 통제
            GetComponent<SnakeMovement>().enabled = false; // 실제 플레이어 이동 통제
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 자동 공격
            if (weapons != null) weapons.enabled = false; // 시험 적의 자동 처치 방지
            foreach (EarlyEnemyBehaviour prior in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 시험 전 생성된 적 순회
            { // 반복 시작
                prior.gameObject.SetActive(false); // 기존 적 행동 차단
                UnityEngine.Object.Destroy(prior.gameObject); // 기존 적 제거
            } // 반복 종료
            ((IList)typeof(EarlyEnemyWaveSpawner).GetField("targets", Private).GetValue(wave)).Clear(); // 기존 생존 목록 초기화
            ((IDictionary)typeof(EarlyEnemyWaveSpawner).GetField("enemyScales", Private).GetValue(wave)).Clear(); // 기존 능력치 목록 초기화
            typeof(EarlyEnemyWaveSpawner).GetField("activeEnemies", Private).SetValue(wave, 0); // 기존 생존 적 수 초기화
            FieldInfo elapsed = typeof(EarlyEnemyWaveSpawner).GetField("elapsed", Private); // 실제 경과 시간
            MethodInfo selectNormal = typeof(EarlyEnemyWaveSpawner).GetMethod("SelectNormalEnemy", Private); // 실제 일반 적 추첨 경로
            if (!Check(elapsed != null && selectNormal != null, "Weighted normal enemy selection is missing.")) yield break; // 실제 추첨 경로 확인
            UnityEngine.Random.State originalRandom = UnityEngine.Random.state; // 기존 난수 상태 보관
            float[] times = { 0f, 60f, 120f, 300f }; // 초반 순차 해금과 5분 시험 시각
            float[,] expected = { { 1f, 0f, 0f, 0f }, { 60f / 85f, 25f / 85f, 0f, 0f }, { 0.60f, 0.25f, 0.15f, 0f }, { 0.48f, 0.26f, 0.18f, 0.08f } }; // 해금된 적을 기준으로 정규화한 가중치
            for (int stage = 0; stage < times.Length; stage++) // 실제 두 구간 순회
            { // 반복 시작
                elapsed.SetValue(wave, times[stage]); // 실제 출현 구간 설정
                UnityEngine.Random.InitState(70440 + stage); // 재현 가능한 추첨 조건
                int[] counts = new int[4]; // 적 종류별 출현 횟수
                for (int draw = 0; draw < 5000; draw++) // 실제 출현 추첨 반복
                { // 반복 시작
                    EarlyEnemySettings selected = selectNormal.Invoke(wave, null) as EarlyEnemySettings; // 실제 출현 후보 추첨
                    if (!Check(selected != null && (int)selected.kind != 3, "Weighted selection returned no enemy or an elite.")) yield break; // 유효한 일반 적 확인
                    int index = (int)selected.kind == 4 ? 3 : (int)selected.kind; // 추가 적을 네 번째로 변환
                    if (!Check(index >= 0 && index < counts.Length, "Unknown normal enemy was selected.")) yield break; // 시험 중 적 종류 확인
                    counts[index]++; // 실제 추첨 횟수 집계
                } // 반복 종료
                for (int index = 0; index < counts.Length; index++) // 적 종류별 비율 확인
                { // 반복 시작
                    if (!Check(Mathf.Abs(counts[index] / 5000f - expected[stage, index]) < 0.025f, "Weighted spawn distribution differs at " + times[stage] + " seconds.")) yield break; // 기획 비율의 허용 오차 확인
                } // 반복 종료
            } // 반복 종료
            UnityEngine.Random.state = originalRandom; // 기존 난수 상태 복원
            typeof(EarlyEnemyWaveSpawner).GetField("enemies", Private).SetValue(wave, Array.Empty<EarlyEnemySettings>()); // 추가 적만 남긴 시험 조건
            typeof(EarlyEnemyWaveSpawner).GetField("previousClockTime", Private).SetValue(wave, Time.timeAsDouble); // 실제 시간 기준 동기화
            FieldInfo nextSpawn = typeof(EarlyEnemyWaveSpawner).GetField("nextSpawn", Private); // 다음 적 출현 시각
            MethodInfo tick = typeof(EarlyEnemyWaveSpawner).GetMethod("LateUpdate", Private); // 실제 출현 처리 경로
            if (!Check(elapsed != null && nextSpawn != null && tick != null, "Wave schedule path missing.")) yield break; // 실제 출현 경로 확인
            elapsed.SetValue(wave, 299f); // 5분 전 시간
            nextSpawn.SetValue(wave, 0f); // 즉시 출현 검사
            tick.Invoke(wave, null); // 실제 웨이브 처리
            if (!Check(wave.CurrentNormalEnemyCount == 0, "Beetle appeared before five minutes.")) yield break; // 조기 출현 차단 확인
            elapsed.SetValue(wave, 300f); // 5분 경계 시간
            nextSpawn.SetValue(wave, 0f); // 즉시 출현 검사
            tick.Invoke(wave, null); // 실제 웨이브 처리
            EarlyEnemyBehaviour enemy = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyBehaviour>(); // 실제 생성 적
            if (!Check(wave.CurrentNormalEnemyCount == 1 && enemy != null && enemy.GetComponent<EnemyDropController>() != null && enemy.GetComponent<SpriteRenderer>().sprite == beetle.enemySprite, "Five-minute beetle or normal reward path missing.")) yield break; // 실제 출현과 일반 보상 확인
            WeaponTarget target = enemy.GetComponent<WeaponTarget>(); // 실제 적 체력
            if (!Check(target != null && Mathf.Approximately(target.MaximumHealth, 34f), "Beetle health did not use the five-minute multiplier.")) yield break; // 시간별 체력 배율 확인
            EarlyEnemySettings runtime = typeof(EarlyEnemyBehaviour).GetField("settings", Private).GetValue(enemy) as EarlyEnemySettings; // 실제 생성 적의 복사된 능력치
            if (!Check(runtime != null && runtime.damage == 15 && Mathf.Approximately(runtime.speedRatio, 0.85f) && Mathf.Approximately(runtime.chargeWarningDuration, 0.6f), "Beetle damage, speed or warning data mismatch.")) yield break; // 시간별 공격력과 기본 속도 확인
            FieldInfo attack = typeof(EarlyEnemyBehaviour).GetField("nextAttack", Private); // 실제 다음 돌진 시간
            FieldInfo phase = typeof(EarlyEnemyBehaviour).GetField("chargePhase", Private); // 실제 돌진 단계
            FieldInfo pendingCut = typeof(EarlyEnemyBehaviour).GetField("chargeCutPending", Private); // 실제 절단 예약 상태
            MethodInfo contact = typeof(EarlyEnemyBehaviour).GetMethod("OnTriggerStay2D", Private); // 실제 접촉 처리 경로
            if (!Check(attack != null && phase != null && pendingCut != null && contact != null, "Beetle charge path missing.")) yield break; // 실제 돌진 구성 확인
            attack.SetValue(enemy, 0f); // 즉시 돌진 예고 시작
            yield return new WaitForFixedUpdate(); // 실제 물리 단계
            if (!Check((int)phase.GetValue(enemy) == 1 && enemy.GetComponent<LineRenderer>() != null && enemy.GetComponent<LineRenderer>().enabled, "Beetle did not telegraph its charge.")) yield break; // 돌진 예고 확인
            yield return new WaitForSeconds(0.3f); // 예고 도중 대기
            if (!Check((int)phase.GetValue(enemy) == 1, "Beetle warning ended before 0.6 seconds.")) yield break; // 예고 지속 확인
            yield return new WaitForSeconds(0.4f); // 예고 종료 대기
            if (!Check((int)phase.GetValue(enemy) == 2, "Beetle did not charge after warning.")) yield break; // 실제 돌진 시작 확인
            Collider2D bodyCollider = body.BodySegments[0].GetComponent<Collider2D>(); // 실제 몸통 충돌체
            if (!Check(bodyCollider != null, "Player body collider missing.")) yield break; // 몸통 구성 확인
            int count = body.CurrentBodyCount; // 충돌 전 몸통 수
            contact.Invoke(enemy, new object[] { bodyCollider }); // 실제 일반 적 접촉 처리
            yield return new WaitForFixedUpdate(); // 예약 피해와 절단 처리 대기
            if (!Check(!(bool)pendingCut.GetValue(enemy) && body.CurrentBodyCount == count, "Normal beetle charge cut a body segment.")) yield break; // 일반 돌진의 절단 방지 확인
            Material warningMaterial = enemy.GetComponent<LineRenderer>().sharedMaterial; // 실제 돌진 예고 재질
            int gemsBefore = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length; // 처치 전 보석 수
            target.TakeDamage(10000f); // 기존 전투 경로로 일반 적 처치
            yield return null; // 적 제거 처리 대기
            yield return null; // 예고 재질 제거 처리 대기
            if (!Check(UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length == gemsBefore + 2, "Beetle did not drop two experience gems.")) yield break; // 경험치 보석 두 개 생성 확인
            if (!Check(warningMaterial == null, "Destroyed enemy retained its warning material.")) yield break; // 반복 출현 시 재질 누수 방지 확인
            AutomatedDay70BeetleVerification.Finish(true, "Weighted spawn, five-minute beetle, warning, reward, no cut and material cleanup passed."); // 전체 시험 성공
        } // 코루틴 종료
        private static bool Check(bool condition, string message) // 실제 시험 조건
        { // 검사 시작
            if (!condition) AutomatedDay70BeetleVerification.Finish(false, message); // 실패 기록
            return condition; // 실제 결과
        } // 검사 종료
    } // 실행기 종료
} // 영역 종료
