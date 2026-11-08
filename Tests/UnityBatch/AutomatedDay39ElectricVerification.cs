using System; // 예외 형식
using System.Collections; // 코루틴 형식
using ProjectEpsilon.Combat; // 실제 속성 효과
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 무기 속성
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedDay39ElectricVerification // 실제 전기 연쇄 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay39ElectricVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedDay39ElectricVerification() // 도메인 재시작 처리
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
            if (EditorApplication.timeSinceStartup - started > 90d) // 시험 제한 시간 확인
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
            player.gameObject.AddComponent<Day39ElectricRunner>(); // 실제 시험 실행
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 시험 종료
        { // 메서드 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[Day39ElectricVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        } // 메서드 끝
    } // 클래스 끝
    public sealed class Day39ElectricRunner : MonoBehaviour // 실제 속성 효과 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 전기 연쇄 시험
        { // 코루틴 시작
            GetComponent<SnakeMovement>().enabled = false; // 플레이어 이동 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자 조회
            weapons.enabled = false; // 자동 공격 중지
            WeaponAttributeCombatEffects effects = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 속성 효과 조회
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/Day14/DebugElectric.asset"); // 기존 전기 시험 데이터
            Require(effects != null && weapon != null, "Electric test dependencies missing."); // 실제 시험 구성 확인
            float distance = weapon.Range * 1.05f; // 기본 범위 밖 시너지 범위 안 거리
            WeaponTarget first = CreateTarget("ElectricFirst", 100f); // 첫 실제 표적
            WeaponTarget second = CreateTarget("ElectricSecond", 100f + distance); // 두 번째 실제 표적
            WeaponTarget third = CreateTarget("ElectricThird", 100f + distance * 2f); // 세 번째 실제 표적
            Hit(effects, weapon, first, 0); // 무시너지 실제 명중
            Require(first.CurrentHealth == 90f && second.CurrentHealth == 100f && third.CurrentHealth == 100f, "Electric stage zero unexpectedly chained."); // 기존 무시너지 동작 확인
            Reset(first, second, third); // 실제 표적 체력 초기화
            Hit(effects, weapon, first, 2); // 이 단계 실제 명중
            Require(first.CurrentHealth == 90f && second.CurrentHealth == 90f && third.CurrentHealth == 100f, "Electric stage two chain or expanded range mismatch."); // 이 단계 한 대상과 범위 증가 확인
            Reset(first, second, third); // 실제 표적 체력 초기화
            Hit(effects, weapon, first, 4); // 사 단계 실제 명중
            Require(first.CurrentHealth == 90f && second.CurrentHealth == 90f && third.CurrentHealth == 90f, "Electric stage four did not hit the additional target."); // 사 단계 추가 대상 확인
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자 조회
            WeaponAttributeSynergyManager synergy = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeSynergyManager>(); // 실제 속성 집계 조회
            WeaponData equipped = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/PrototypeElectricBolt.asset"); // 실제 전기 보상 무기
            Require(body != null && synergy != null && equipped != null, "Actual electric equipment dependencies missing."); // 실제 장착 구성 확인
            weapons.enabled = true; // 몸통 변경 구독 복구
            Require(body.TryGainBodyFromLevelUp() && weapons.SlotCount >= 4, "Could not grow fourth weapon slot."); // 실제 네 번째 몸통과 슬롯 생성
            weapons.enabled = false; // 자동 공격 재중지
            for (int index = 0; index < 4; index++) // 실제 전기 슬롯 순회
            { // 반복 시작
                Require(weapons.TryEquipAt(index, equipped, 1), "Could not equip electric weapon."); // 같은 등급 실제 전기 무기 장착
            } // 반복 끝
            Require(synergy.GetCount(WeaponAttribute.Electric) == 4 && synergy.GetStage(WeaponAttribute.Electric) == 4, "Actual four-weapon synergy mismatch."); // 실제 네 무기 시너지 확인
            Require(weapons.TryMergeSlots(0, 1, WeaponMergeUpgrade.Damage), "Actual electric weapon merge failed."); // 실제 동일 무기 합성
            Require(synergy.GetCount(WeaponAttribute.Electric) == 3 && synergy.GetStage(WeaponAttribute.Electric) == 2, "Electric synergy did not update after merging."); // 합성 직후 전기 단계 하락 확인
            yield return null; // 실제 프레임 대기
            AutomatedDay39ElectricVerification.Finish(0, "Actual stage zero, two and four chain counts, range and post-merge synergy."); // 전체 성공 기록
        } // 코루틴 끝
        private static WeaponTarget CreateTarget(string name, float x) // 실제 공격 대상 생성
        { // 메서드 시작
            GameObject item = new GameObject(name); // 통제 표적 객체
            item.transform.position = new Vector3(x, 100f, 0f); // 다른 적과 떨어진 표적 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(100f); // 실제 표적 체력
            return target; // 실제 표적 반환
        } // 메서드 끝
        private static void Reset(WeaponTarget first, WeaponTarget second, WeaponTarget third) // 표적 체력 초기화
        { // 메서드 시작
            first.Configure(100f); // 첫 표적 회복
            second.Configure(100f); // 두 번째 표적 회복
            third.Configure(100f); // 세 번째 표적 회복
        } // 메서드 끝
        private static void Hit(WeaponAttributeCombatEffects effects, WeaponData weapon, WeaponTarget target, int stage) // 실제 속성 명중
        { // 메서드 시작
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, WeaponAttribute.Electric, stage, stage, 1, Vector3.zero, 10f); // 실제 시너지 공격 정보
            effects.ApplyHit(attack, target, target.transform.position); // 실제 명중과 연쇄 처리
        } // 메서드 끝
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedDay39ElectricVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
