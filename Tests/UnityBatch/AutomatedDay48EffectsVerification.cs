using System; // 실제 예외와 열거형
using System.Reflection; // 실제 피해 계산 접근
using ProjectEpsilon.Combat; // 실제 포탑과 등급
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 포탑 데이터
using ProjectEpsilon.Player; // 실제 플레이어
using ProjectEpsilon.Progression; // 실제 영구 성장
using UnityEditor; // 실제 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 실제 도메인 갱신 처리
    public static class AutomatedDay48EffectsVerification // 실제 성장 효과 검증
    {
        private const string Key = "EpsilonDay48Effects"; // 실제 시험 표식
        private static double started; // 실제 시험 시작 시각
        static AutomatedDay48EffectsVerification() // 실제 도메인 갱신
        {
            if (SessionState.GetBool(Key, false)) // 실제 진행 여부 확인
            {
                EditorApplication.update += Tick; // 실제 시험 감시
            }
        }
        public static void Run() // 실제 배치 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(Key, true); // 실제 시험 진행 기록
            EditorApplication.update -= Tick; // 이전 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }
        private static void Tick() // 실제 씬 대기와 시험
        {
            if (!EditorApplication.isPlaying) // 실제 플레이 상태 확인
            {
                return; // 실제 진입 대기
            }
            if (started <= 0d) // 실제 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            }
            if (EditorApplication.timeSinceStartup - started > 60d) // 실제 제한 시간 확인
            {
                Finish(1, "Timed out."); // 실제 시간 초과
                return; // 추가 검사 중단
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 실제 시작 무기 선택
            {
                return; // 실제 시작 대기
            }
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 런 준비 확인
            {
                return; // 실제 초기화 대기
            }
            try // 실제 효과 시험
            {
                CheckEffects(); // 실제 능력과 재선택 검사
                Finish(0, "All 12 nodes, actual damage, cooldown, healing, stamina, movement, experience and reroll passed."); // 실제 시험 성공
            }
            catch (Exception error) // 실제 시험 실패 확인
            {
                Finish(1, error.ToString()); // 실제 실패 기록
            }
        }
        private static void CheckEffects() // 실제 성장 효과 검사
        {
            Require(Application.productName == "Project-Epsilon-Day48-Isolated", "Non-isolated project."); // 실제 저장 격리 확인
            Day48MemoryStorage storage = new Day48MemoryStorage(); // 실제 메모리 저장소
            MetaProgressionService service = new MetaProgressionService(storage); // 실제 성장 서비스
            Require(service.GetNextCost(MetaNode.Health) == 3, "Initial node cost mismatch."); // 실제 초기 성장 비용 확인
            Require(service.TryCompleteRun("bank", 120000d, 0, 0, false, 0), "Could not fund tree."); // 실제 성장 재화 준비
            for (int index = 0; index < 12; index++) // 실제 열두 노드 순회
            {
                Require(service.TryPurchase((MetaNode)index), "Could not purchase node " + index); // 실제 단계 구매
            }
            Require(service.Cores == 876, "Tree purchase cost mismatch."); // 실제 구매 비용 확인
            Require(service.GetNextCost(MetaNode.Health) == 5 && service.GetNextCost(MetaNode.LastBodySpeed) < 0, "Purchased node cost or maximum mismatch."); // 실제 다음 단계 비용과 최대 확인
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 메타 관리자
            typeof(MetaProgressionController).GetProperty("Service").SetValue(meta, service); // 실제 메모리 성장 적용
            SnakeMovement movement = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 이동기
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeStamina stamina = UnityEngine.Object.FindFirstObjectByType<SnakeStamina>(); // 실제 스태미나
            SnakeHealth health = UnityEngine.Object.FindFirstObjectByType<SnakeHealth>(); // 실제 체력
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            SnakeExperience xp = UnityEngine.Object.FindFirstObjectByType<SnakeExperience>(); // 실제 경험치
            SnakeLevelUpController levels = UnityEngine.Object.FindFirstObjectByType<SnakeLevelUpController>(); // 실제 레벨업
            Require(movement != null && body != null && stamina != null && health != null && weapons != null && xp != null && levels != null, "Actual scene components missing."); // 실제 씬 구성 확인
            movement.enabled = false; // 실제 이동 통제
            weapons.enabled = false; // 실제 자동 공격 통제
            Require(Near(MetaModifiers.HealthMultiplier, 1.04f) && Near(MetaModifiers.HealingMultiplier, 1.05f) && Near(MetaModifiers.TurnMultiplier, 1.03f) && Near(MetaModifiers.StaminaMultiplier, 1.05f) && Near(MetaModifiers.AttackSpeedMultiplier, 1.02f) && Near(MetaModifiers.ExperienceMultiplier, 1.03f) && Near(MetaModifiers.PickupRangeMultiplier, 1.05f), "Basic node multipliers incorrect."); // 실제 일반 성장 배율 확인
            Require(Near(MetaModifiers.DrainMultiplier(25f, 100f), 0.8f) && Near(MetaModifiers.DrainMultiplier(26f, 100f), 1f), "Emergency stamina threshold incorrect."); // 실제 긴급 가속 경계 확인
            stamina.ResetStamina(); // 실제 성장 스태미나 초기화
            Require(Near(stamina.MaximumStamina, 105f) && Near(stamina.CurrentStamina, 105f), "Actual maximum stamina incorrect."); // 실제 스태미나 적용 확인
            Require(Near(movement.TurnSpeed, 145f * CharacterRunInitializer.TurnSpeedMultiplier * 1.03f), "Actual turn speed incorrect."); // 실제 회전 적용 확인
            while (body.CurrentBodyCount > 1) // 실제 마지막 몸통 구성
            {
                Require(body.TryRemoveBody(), "Could not reduce body count."); // 실제 몸통 제거
            }
            Require(Near(MetaModifiers.LastBodySpeedMultiplier(body), 1.1f) && Near(movement.CurrentMoveSpeed, movement.MoveSpeed * 1.1f), "Actual last-body speed incorrect."); // 실제 마지막 몸통 속도 확인
            Require(health.TakeDamage(50), "Could not apply damage."); // 실제 체력 피해
            int before = health.CurrentHealth; // 실제 피해 후 체력
            Require(health.Heal(10) && health.Heal(10) && health.CurrentHealth == before + 21, "Actual healing and fractional carry incorrect."); // 실제 회복 소수 누적 확인
            WeaponData blade = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 시험 포탑
            Require(blade != null && weapons.TryEquipAt(0, blade, 3), "Could not equip grade-three turret."); // 실제 삼등급 장착
            SnakeWeaponSlot slot = weapons.Slots[0]; // 실제 장착 슬롯
            float damage = (float)typeof(SnakeWeaponManager).GetMethod("CalculateSlotDamage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(weapons, new object[] { slot }); // 실제 포탑 피해 계산
            Require(Near(damage, blade.BaseDamage * 1.5f * 1.02f * 1.05f), "Actual grade-three damage incorrect."); // 실제 피해 성장 확인
            slot.StartCooldown(100f); // 실제 공격 대기 시작
            float ready = 100f + blade.AttackInterval / 1.02f; // 실제 기대 공격 시각
            Require(!slot.IsReady(ready - 0.001f) && slot.IsReady(ready + 0.001f), "Actual turret cooldown incorrect."); // 실제 공격속도 적용 확인
            xp.Configure(10000, 1f); // 실제 충분한 경험치 기준
            xp.ResetProgression(); // 실제 경험치 초기화
            xp.AddExperience(100); // 실제 경험치 획득
            Require(xp.CurrentExperience == 103, "Actual experience multiplier incorrect."); // 실제 경험치 성장 확인
            xp.ResetProgression(); // 실제 소수 보너스 초기화
            for (int count = 0; count < 100; count++) // 실제 작은 보석 반복 획득
            {
                xp.AddExperience(1); // 실제 소수 보너스 누적
            }
            Require(xp.CurrentExperience == 103, "Repeated experience fractional carry incorrect."); // 실제 소수 누적 확인
            xp.Configure(10, 1f); // 실제 다음 레벨 기준
            xp.ResetProgression(); // 실제 성장 초기화
            xp.AddExperience(10); // 실제 레벨업 요청
            Require(levels.IsPresentingLevelUp && meta.CanReroll && levels.TryRerollCandidates() && !levels.TryRerollCandidates() && meta.RerollUsed, "Once-per-run actual reward reroll failed."); // 실제 일회 재선택 확인
            Require(service.TryRefundAll() && service.Cores == 1000, "Initial full refund failed."); // 실제 첫 전체 환불 확인
            Require(service.GetNextCost(MetaNode.Health) == 3, "Refund did not restore initial purchase cost."); // 초기화 후 첫 강화 비용 확인
            for (int index = 0; index < 12; index++) // 실제 전체 성장 순회
            {
                int maximum = index % 3 == 2 ? 1 : 5; // 실제 노드별 최대 단계
                for (int rank = 0; rank < maximum; rank++) // 실제 성장 단계 순회
                {
                    Require(service.TryPurchase((MetaNode)index), "Full tree purchase failed."); // 실제 최대 단계 구매
                }
                Require(!service.TryPurchase((MetaNode)index), "Maximum node level exceeded."); // 실제 최대 단계 제한
            }
            Require(service.Cores == 532 && service.GetNextCost(MetaNode.Health) < 0 && service.TryRefundAll() && service.Cores == 1000 && service.GetNextCost(MetaNode.Health) == 3, "Full tree cost or refund incorrect."); // 전체 구매와 초기화 후 비용 확인
        }
        private static bool Near(float actual, float expected) // 실제 수치 오차 확인
        {
            return Mathf.Abs(actual - expected) < 0.001f; // 부동소수 허용 오차
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 실패 확인
            {
                throw new InvalidOperationException(message); // 실제 시험 중단
            }
        }
        private static void Finish(int code, string message) // 실제 시험 결과 기록
        {
            SessionState.SetBool(Key, false); // 실제 시험 표식 제거
            EditorApplication.update -= Tick; // 실제 감시 해제
            Debug.Log($"[Day48EffectsVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public static class AutomatedDay48FreshProcessVerification // 새 프로세스 저장 검증
    {
        public static void Run() // 실제 새 프로세스 진입
        {
            bool isolated = Application.productName == "Project-Epsilon-Day48-Isolated"; // 실제 저장 격리 확인
            MetaProgressionService service = new MetaProgressionService(new PlayerPrefsMetaStorage("ProjectEpsilon.MetaProgress.v1")); // 실제 저장 재로드
            bool valid = isolated && service.IsAvailable && service.FinishedFirstRun && service.Cores == 7 && service.IsMapUnlocked(1) && !service.IsMapUnlocked(2) && service.IsCharacterUnlocked(0) && service.IsTurretAttributeUnlocked(0); // 실제 프로세스 간 진행 확인
            Debug.Log($"[Day48FreshProcessVerification] {(valid ? "PASS" : "FAIL")}: isolated={isolated}, cores={service.Cores}, ruins={service.IsMapUnlocked(1)}."); // 실제 시험 결과
            EditorApplication.Exit(valid ? 0 : 1); // 실제 종료 코드
        }
    }
}
