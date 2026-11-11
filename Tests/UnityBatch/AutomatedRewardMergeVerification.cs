using System; // 기본 형식
using System.Reflection; // 새 API 조회
using System.Text; // 슬롯 상태 기록
using ProjectEpsilon.Combat; // 무기와 시너지
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Enemies; // 시험 출현 제어
using ProjectEpsilon.Player; // 몸통과 이동
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 편집기 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedRewardMergeVerification // 미장착 보상 합성 실제 씬 검증
    {
        private const string ActiveKey = "EpsilonRewardMergeVerification"; // 배치 진행 표식
        private static double started; // 시험 시작 시각
        private static bool finished; // 중복 종료 방지

        static AutomatedRewardMergeVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 배치 진행 여부
            {
                EditorApplication.update += Tick; // 시험 갱신 연결
            }
        }

        public static void Run() // 배치 실행 진입점
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 진행 표식 저장
            EditorApplication.update -= Tick; // 중복 연결 제거
            EditorApplication.update += Tick; // 시험 갱신 연결
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        }

        private static void Tick() // 실제 씬 시험 갱신
        {
            if (!EditorApplication.isPlaying) // 플레이 시작 확인
            {
                return; // 플레이 대기
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 시작 무기 화면 처리
            {
                return; // 전투 시작 대기
            }
            if (started <= 0d) // 시험 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 60d) // 전체 제한 시간 확인
            {
                Finish(1, "Timed out waiting for initialized Game scene."); // 시간 초과 기록
                return; // 시험 중단
            }
            SnakeMovement movement = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 플레이어 조회
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자 조회
            WeaponAttributeSynergyManager synergy = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeSynergyManager>(); // 실제 시너지 조회
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자 조회
            if (movement == null || weapons == null || synergy == null || body == null || GameManager.Instance == null || Time.time < 0.35f) // 씬 초기화 확인
            {
                return; // 초기화 대기
            }
            EditorApplication.update -= Tick; // 중복 시험 중단
            try // 실제 시험 예외 처리
            {
                Execute(movement, weapons, synergy, body); // 실제 합성 검증
                Finish(0, "Reward merge with empty/full slots, both inheritance options, invalid requests, synergy preservation, and equipped merge regression."); // 전체 성공 기록
            }
            catch (Exception error) // 시험 실패 처리
            {
                Finish(1, error.GetBaseException().Message); // 실패 원인 기록
            }
        }

        private static void Execute(SnakeMovement movement, SnakeWeaponManager weapons, WeaponAttributeSynergyManager synergy, SnakeBodyManager body) // 실제 씬 시험 실행
        {
            MethodInfo method = typeof(SnakeWeaponManager).GetMethod("TryMergeReward", new Type[] { typeof(int), typeof(WeaponData), typeof(int), typeof(bool), typeof(WeaponMergeUpgrade) }); // 합성 API 계약 조회
            Require(method != null && method.ReturnType == typeof(bool), "Missing TryMergeReward(int, WeaponData, int, bool, WeaponMergeUpgrade) API."); // 제품 API 존재 확인
            movement.enabled = false; // 머리 이동 중지
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 자연 출현기 조회
            if (spawner != null) // 출현기 존재 확인
            {
                spawner.enabled = false; // 적 자연 출현 중지
            }
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 기존 적 순회
            {
                enemy.gameObject.SetActive(false); // 시험 중 기존 적 비활성화
            }
            Require(body.CurrentBodyCount == 3 && weapons.SlotCount == 3 && weapons.Slots[0].Weapon != null, "Expected three initialized body slots and a starting weapon."); // 초기 씬 구성 확인
            WeaponData original = weapons.Slots[0].Weapon; // 실제 씬 기본 무기
            Require(original.MaxGrade >= 2, "Starting weapon cannot be upgraded."); // 상위 등급 존재 확인
            WeaponData other = UnityEngine.Object.Instantiate(original); // 서로 다른 무기 객체 생성
            Debug.Log("[RewardMerge] Actual Game scene; movement and enemy spawning disabled; equipment and reward grade controlled; no UI interaction."); // 시험 조건 기록
            VerifyEmptySlot(method, weapons, synergy, original, other); // 빈 슬롯 상태 계승
            VerifyFullSlots(method, weapons, synergy, original, other); // 만석 상태 보상 계승
            VerifyInvalidInputs(method, weapons, synergy, original, other); // 잘못된 요청 차단
            VerifyEquippedRegression(weapons, synergy, original); // 기존 장착 합성 회귀
            UnityEngine.Object.Destroy(other); // 시험용 무기 정리
        }

        private static void VerifyEmptySlot(MethodInfo method, SnakeWeaponManager weapons, WeaponAttributeSynergyManager synergy, WeaponData original, WeaponData other) // 빈 슬롯과 장착 강화 계승
        {
            Clear(weapons); // 이전 무기 초기화
            Require(weapons.TryEquipAt(0, other, 1) && weapons.TryEquipAt(1, original, 1), "Could not equip empty-slot fixture."); // 두 무기 장착
            weapons.Slots[1].Equip(original, 1, 2, 3, 1); // 장착 무기 강화 이력 설정
            synergy.Recalculate(); // 직접 이력 설정 후 집계
            int count = synergy.GetCount(original.Attribute); // 기존 속성 수
            int stage = synergy.GetStage(original.Attribute); // 기존 속성 단계
            int notifications = 0; // 슬롯 이벤트 횟수
            Action changed = () => notifications++; // 슬롯 이벤트 기록
            weapons.SlotsChanged += changed; // 이벤트 구독
            bool merged = Invoke(method, weapons, 1, original, 1, true, WeaponMergeUpgrade.Range); // 장착 강화 계승 합성
            weapons.SlotsChanged -= changed; // 이벤트 구독 해제
            Require(merged && notifications == 1 && weapons.OccupiedSlotCount == 2, "Empty-slot reward merge changed equipped count or event count."); // 장착 수 및 알림 확인
            Require(weapons.Slots[0].Weapon == original && weapons.Slots[0].Grade == 2 && weapons.Slots[0].DamageUpgrades == 2 && weapons.Slots[0].SpeedUpgrades == 3 && weapons.Slots[0].RangeUpgrades == 2, "Equipped inheritance or common range upgrade mismatch."); // 장착 강화 및 새 강화 확인
            Require(weapons.Slots[1].Weapon == other && weapons.Slots[2].IsEmpty, "Unrelated weapon order or empty slot changed."); // 다른 무기 순서 유지
            Require(synergy.GetCount(original.Attribute) == count && synergy.GetStage(original.Attribute) == stage, "Empty-slot reward merge changed synergy count or stage."); // 시너지 유지 확인
            Debug.Log("[RewardMerge] Empty slot: equipped inheritance, result placement, count, event, and synergy passed."); // 개별 시험 결과
        }

        private static void VerifyFullSlots(MethodInfo method, SnakeWeaponManager weapons, WeaponAttributeSynergyManager synergy, WeaponData original, WeaponData other) // 만석과 보상 강화 계승
        {
            Clear(weapons); // 이전 무기 초기화
            Require(weapons.TryEquipAt(0, other, 1) && weapons.TryEquipAt(1, original, 1) && weapons.TryEquipAt(2, other, 2), "Could not equip full-slot fixture."); // 세 무기 장착
            weapons.Slots[1].Equip(original, 1, 2, 3, 1); // 기존 강화 이력 설정
            synergy.Recalculate(); // 직접 구성 후 집계
            int count = synergy.GetCount(original.Attribute); // 기존 속성 수
            int stage = synergy.GetStage(original.Attribute); // 기존 속성 단계
            bool merged = Invoke(method, weapons, 1, original, 1, false, WeaponMergeUpgrade.Damage); // 보상 강화 계승 합성
            Require(merged && weapons.OccupiedSlotCount == 3, "Full-slot reward merge changed equipped count."); // 만석 유지 확인
            Require(weapons.Slots[0].Weapon == original && weapons.Slots[0].Grade == 2 && weapons.Slots[0].DamageUpgrades == 1 && weapons.Slots[0].SpeedUpgrades == 0 && weapons.Slots[0].RangeUpgrades == 0, "Reward inheritance retained equipped bonuses or missed common upgrade."); // 보상 강화 및 새 강화 확인
            Require(weapons.Slots[1].Weapon == other && weapons.Slots[1].Grade == 1 && weapons.Slots[2].Weapon == other && weapons.Slots[2].Grade == 2, "Full-slot merge changed unrelated weapon order."); // 다른 무기 순서 유지
            Require(synergy.GetCount(original.Attribute) == count && synergy.GetStage(original.Attribute) == stage, "Full-slot reward merge changed synergy count or stage."); // 시너지 유지 확인
            Debug.Log("[RewardMerge] Full slots: reward inheritance, result placement, other weapons, count, and synergy passed."); // 개별 시험 결과
        }

        private static void VerifyInvalidInputs(MethodInfo method, SnakeWeaponManager weapons, WeaponAttributeSynergyManager synergy, WeaponData original, WeaponData other) // 잘못된 합성 요청
        {
            Clear(weapons); // 이전 무기 초기화
            Require(weapons.TryEquipAt(0, original, 1) && weapons.TryEquipAt(1, other, 1), "Could not equip invalid-input fixture."); // 서로 다른 무기 장착
            Reject(method, weapons, synergy, -1, original, 1, true, WeaponMergeUpgrade.Damage, "negative slot"); // 음수 슬롯 거절
            Reject(method, weapons, synergy, 3, original, 1, true, WeaponMergeUpgrade.Damage, "out-of-range slot"); // 범위 밖 슬롯 거절
            Reject(method, weapons, synergy, 2, original, 1, true, WeaponMergeUpgrade.Damage, "empty slot"); // 빈 슬롯 거절
            Reject(method, weapons, synergy, 0, null, 1, true, WeaponMergeUpgrade.Damage, "null reward"); // 빈 보상 거절
            Reject(method, weapons, synergy, 0, other, 1, true, WeaponMergeUpgrade.Damage, "different weapon"); // 다른 무기 거절
            Reject(method, weapons, synergy, 0, original, 2, true, WeaponMergeUpgrade.Damage, "different grade"); // 다른 등급 거절
            Reject(method, weapons, synergy, 0, original, 1, true, WeaponMergeUpgrade.None, "missing upgrade"); // 강화 미선택 거절
            Reject(method, weapons, synergy, 0, original, 1, true, (WeaponMergeUpgrade)99, "invalid upgrade"); // 잘못된 강화 거절
            Require(weapons.TryEquipAt(0, original, original.MaxGrade), "Could not equip maximum-grade fixture."); // 최대 등급 장착
            Reject(method, weapons, synergy, 0, original, original.MaxGrade, true, WeaponMergeUpgrade.Damage, "maximum grade"); // 최대 등급 거절
            Debug.Log("[RewardMerge] Invalid weapon, grade, slot, and upgrade requests preserved all slots and synergy."); // 개별 시험 결과
        }

        private static void VerifyEquippedRegression(SnakeWeaponManager weapons, WeaponAttributeSynergyManager synergy, WeaponData original) // 기존 장착 무기 2개 합성
        {
            Clear(weapons); // 이전 무기 초기화
            Require(weapons.TryEquipAt(0, original, 1) && weapons.TryEquipAt(1, original, 1), "Could not equip existing-merge fixture."); // 같은 무기 두 개 장착
            Require(synergy.GetCount(original.Attribute) == 2 && synergy.GetStage(original.Attribute) == 2, "Existing-merge initial synergy mismatch."); // 초기 시너지 확인
            Require(weapons.TryMergeSlots(0, 1, WeaponMergeUpgrade.AttackSpeed), "Existing equipped merge failed."); // 기존 장착 합성 실행
            Require(weapons.OccupiedSlotCount == 1 && weapons.Slots[0].Weapon == original && weapons.Slots[0].Grade == 2 && weapons.Slots[0].SpeedUpgrades == 1, "Existing equipped merge no longer consumes one weapon."); // 2대1 결과 확인
            Require(synergy.GetCount(original.Attribute) == 1 && synergy.GetStage(original.Attribute) == 0, "Existing equipped merge did not update synergy."); // 기존 시너지 갱신 확인
            Debug.Log("[RewardMerge] Existing equipped 2->1 merge and synergy regression passed."); // 개별 시험 결과
        }

        private static void Reject(MethodInfo method, SnakeWeaponManager weapons, WeaponAttributeSynergyManager synergy, int index, WeaponData reward, int grade, bool inherit, WeaponMergeUpgrade upgrade, string label) // 실패 요청의 무변경 확인
        {
            string before = Snapshot(weapons); // 기존 슬롯 상태 기록
            WeaponAttribute attribute = weapons.Slots[0].Weapon.Attribute; // 기존 속성 조회
            int count = synergy.GetCount(attribute); // 기존 속성 수
            int stage = synergy.GetStage(attribute); // 기존 시너지 단계
            int notifications = 0; // 슬롯 변경 이벤트 수
            Action changed = () => notifications++; // 이벤트 기록
            weapons.SlotsChanged += changed; // 이벤트 구독
            bool merged = Invoke(method, weapons, index, reward, grade, inherit, upgrade); // 잘못된 합성 요청
            weapons.SlotsChanged -= changed; // 이벤트 구독 해제
            Require(!merged && before == Snapshot(weapons) && notifications == 0 && synergy.GetCount(attribute) == count && synergy.GetStage(attribute) == stage, "Invalid request changed state: " + label); // 요청 거절 및 상태 유지 확인
        }

        private static bool Invoke(MethodInfo method, SnakeWeaponManager weapons, int index, WeaponData reward, int grade, bool inherit, WeaponMergeUpgrade upgrade) // 실제 합성 API 호출
        {
            return (bool)method.Invoke(weapons, new object[] { index, reward, grade, inherit, upgrade }); // 제품 API 결과 반환
        }

        private static void Clear(SnakeWeaponManager weapons) // 모든 시험 슬롯 초기화
        {
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 전체 슬롯 순회
            {
                slot.Clear(); // 무기와 강화 제거
            }
        }

        private static string Snapshot(SnakeWeaponManager weapons) // 무기 및 강화 상태 기록
        {
            StringBuilder value = new StringBuilder(); // 상태 문자열 생성
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 모든 슬롯 순회
            {
                int id = slot.Weapon == null ? 0 : slot.Weapon.GetInstanceID(); // 무기 객체 식별자
                value.Append($"{id}:{slot.Grade}:{slot.DamageUpgrades}:{slot.SpeedUpgrades}:{slot.RangeUpgrades};"); // 등급과 강화 기록
            }
            return value.ToString(); // 슬롯 상태 반환
        }

        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                throw new InvalidOperationException(message); // 실패 상세 전달
            }
        }

        private static void Finish(int code, string message) // 결과 기록과 편집기 종료
        {
            if (finished) // 종료 중복 확인
            {
                return; // 중복 종료 방지
            }
            finished = true; // 종료 상태 기록
            SessionState.SetBool(ActiveKey, false); // 진행 표식 제거
            EditorApplication.update -= Tick; // 시험 갱신 해제
            Debug.Log($"[RewardMerge] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 종료 코드 반환
        }
    }
}
