using System; // 예외 형식 사용
using System.Reflection; // 내부 입력 설정 조회
using ProjectEpsilon.Core; // 입력 관리자 사용
using ProjectEpsilon.Combat; // 무기 및 합성 사용
using ProjectEpsilon.Data; // 무기 데이터 사용
using ProjectEpsilon.Player; // 이동 컴포넌트 사용
using ProjectEpsilon.Progression; // 레벨업 보상 사용
using ProjectEpsilon.UI; // 레벨업 화면 사용
using UnityEditor; // 편집기 메뉴 사용
using UnityEngine; // Unity 기능 사용
using UnityEngine.InputSystem; // 입력 동작 사용

namespace ProjectEpsilon.Editor // 편집기 영역
{
    public static class ProjectEpsilonDay19MergeVerification // 합성 입력 검증
    {
        [MenuItem("Project Epsilon/Day 19/Verify Merge Controls")] // 검증 메뉴
        public static void Run() // 검증 실행
        {
            VerifyMergeMovementSpeed(); // 합성 이동 속도 검증
            VerifyDefaultBoostBinding(); // 가속 입력 검증
            VerifyMergeUpgradeAndSynergy(); // 합성 강화 및 시너지 검증
            VerifyFullSlotSelection(); // 가득 찬 슬롯 선택 검증
            Debug.Log("[Project Epsilon] Day19 merge controls verification passed."); // 검증 성공 기록
        }

        private static void VerifyMergeMovementSpeed() // 합성 이동 중복 감속 검증
        {
            GameObject subject = new GameObject("MergeMovementVerification"); // 임시 플레이어 생성
            try // 정리 보장
            {
                SnakeMovement movement = subject.AddComponent<SnakeMovement>(); // 이동 컴포넌트 생성
                movement.SetMergeMovementMode(true, 0.7f); // 합성 이동 모드 설정
                Require(Mathf.Approximately(movement.CurrentMoveSpeed, movement.MoveSpeed), "합성 중 이동 속도 중복 감속"); // 전체 시간 배율과 중복 적용 방지
            }
            finally // 임시 오브젝트 정리
            {
                UnityEngine.Object.DestroyImmediate(subject); // 임시 오브젝트 제거
            }
        }

        private static void VerifyDefaultBoostBinding() // 기본 가속 입력 검증
        {
            GameObject subject = new GameObject("BoostBindingVerification"); // 임시 입력 관리자 생성
            try // 정리 보장
            {
                InputBindingManager manager = subject.AddComponent<InputBindingManager>(); // 입력 관리자 추가
                MethodInfo create = typeof(InputBindingManager).GetMethod("CreateInputActions", BindingFlags.NonPublic | BindingFlags.Instance); // 입력 생성 메서드 조회
                create.Invoke(manager, null); // 기본 입력 생성
                FieldInfo boostField = typeof(InputBindingManager).GetField("boostAction", BindingFlags.NonPublic | BindingFlags.Instance); // 가속 입력 필드 조회
                InputAction boost = (InputAction)boostField.GetValue(manager); // 가속 입력 조회
                Require(boost.bindings[0].path == "<Keyboard>/space", "기본 가속 입력이 Space가 아님"); // 기본 키 확인
            }
            finally // 임시 오브젝트 정리
            {
                UnityEngine.Object.DestroyImmediate(subject); // 임시 오브젝트 제거
            }
        }

        private static void VerifyMergeUpgradeAndSynergy() // 합성 강화 및 속성 집계 검증
        {
            GameObject subject = new GameObject("MergeUpgradeVerification"); // 임시 플레이어 생성
            WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>(); // 임시 무기 생성
            try // 정리 보장
            {
                SnakeBodyManager bodies = subject.AddComponent<SnakeBodyManager>(); // 몸통 관리자 생성
                bodies.Configure(null, null, null, 0, 20, 0.58f); // 빈 몸통 구성
                bodies.TryGainBodyFromLevelUp(); // 첫 몸통 생성
                bodies.TryGainBodyFromLevelUp(); // 둘째 몸통 생성
                bodies.TryGainBodyFromLevelUp(); // 셋째 몸통 생성
                SnakeWeaponManager weapons = subject.AddComponent<SnakeWeaponManager>(); // 무기 관리자 생성
                weapons.Configure(bodies, null, null); // 몸통 슬롯 연결
                weapons.TryEquipAt(0, weapon); // 첫 무기 장착
                weapons.TryEquipAt(1, weapon); // 둘째 무기 장착
                WeaponAttributeSynergyManager synergy = subject.AddComponent<WeaponAttributeSynergyManager>(); // 속성 관리자 생성
                synergy.Configure(weapons); // 무기 변경 구독
                Require(synergy.GetCount(weapon.Attribute) == 2, "합성 전 속성 개수 오류"); // 합성 전 속성 집계
                Require(weapons.TryMergeSlots(0, 1, WeaponMergeUpgrade.Damage), "합성 실패"); // 공격력 강화 합성
                Require(weapons.Slots[0].Grade == 2, "합성 등급 오류"); // 상위 등급 확인
                Require(Mathf.Approximately(weapons.Slots[0].DamageMultiplier, 1.05f), "공격력 강화 오류"); // 공격력 강화 확인
                Require(synergy.GetCount(weapon.Attribute) == 1, "합성 후 속성 개수 미갱신"); // 즉시 속성 집계 확인
                weapons.AcquireWeapon(weapon); // 새 무기로 슬롯 이동
                Require(Mathf.Approximately(weapons.Slots[1].DamageMultiplier, 1.05f), "슬롯 이동 시 강화 손실"); // 무기별 강화 유지 확인
            }
            finally // 임시 오브젝트 정리
            {
                UnityEngine.Object.DestroyImmediate(subject); // 임시 플레이어 제거
                UnityEngine.Object.DestroyImmediate(weapon); // 임시 무기 제거
            }
        }

        private static void VerifyFullSlotSelection() // 가득 찬 슬롯 교체 검증
        {
            GameObject subject = new GameObject("FullSlotVerification"); // 임시 플레이어 생성
            GameObject panelObject = new GameObject("FullSlotPanel"); // 임시 패널 생성
            WeaponData oldWeapon = ScriptableObject.CreateInstance<WeaponData>(); // 기존 무기 생성
            WeaponData replacedWeapon = ScriptableObject.CreateInstance<WeaponData>(); // 교체 대상 무기 생성
            WeaponData newWeapon = ScriptableObject.CreateInstance<WeaponData>(); // 신규 무기 생성
            try // 정리 보장
            {
                SnakeBodyManager bodies = subject.AddComponent<SnakeBodyManager>(); // 몸통 관리자 생성
                bodies.Configure(null, null, null, 0, 20, 0.58f); // 빈 몸통 구성
                bodies.TryGainBodyFromLevelUp(); // 첫 몸통 생성
                bodies.TryGainBodyFromLevelUp(); // 둘째 몸통 생성
                SnakeWeaponManager weapons = subject.AddComponent<SnakeWeaponManager>(); // 무기 관리자 생성
                weapons.Configure(bodies, null, null); // 몸통 슬롯 연결
                weapons.TryEquipAt(0, oldWeapon); // 첫 슬롯 장착
                weapons.TryEquipAt(1, replacedWeapon); // 둘째 슬롯 장착
                LevelUpPanelController panel = panelObject.AddComponent<LevelUpPanelController>(); // 레벨업 화면 생성
                SnakeLevelUpController controller = subject.AddComponent<SnakeLevelUpController>(); // 레벨업 관리자 생성
                controller.Configure(null, bodies, null, weapons, null, panel); // 검증 대상 연결
                FieldInfo presenting = typeof(SnakeLevelUpController).GetField("presentingLevelUp", BindingFlags.NonPublic | BindingFlags.Instance); // 레벨업 상태 조회
                presenting.SetValue(controller, true); // 레벨업 선택 상태 설정
                var candidates = (System.Collections.Generic.List<WeaponRewardCandidate>)controller.CurrentCandidates; // 보상 후보 목록 조회
                candidates.Add(new WeaponRewardCandidate(newWeapon, 1)); // 신규 무기 후보 추가
                MethodInfo select = typeof(SnakeLevelUpController).GetMethod("HandleCandidateSelected", BindingFlags.NonPublic | BindingFlags.Instance); // 선택 처리 조회
                select.Invoke(controller, new object[] { 0 }); // 신규 무기 선택
                Require(weapons.Slots[0].Weapon == oldWeapon && weapons.Slots[1].Weapon == replacedWeapon, "교체 슬롯 선택 전 기존 무기 손실"); // 기존 무기 보존 확인
                Require(controller.IsPresentingLevelUp, "교체 선택 전 레벨업 종료"); // 교체 화면 유지 확인
                select.Invoke(controller, new object[] { 2 }); // 둘째 슬롯 선택
                select.Invoke(controller, new object[] { 1 }); // 현재 슬롯 교체 확정
                Require(weapons.Slots[0].Weapon == newWeapon && weapons.Slots[1].Weapon == oldWeapon, "선택 슬롯 교체 오류"); // 선택 슬롯 확인
            }
            finally // 임시 오브젝트 정리
            {
                UnityEngine.Object.DestroyImmediate(subject); // 임시 플레이어 제거
                UnityEngine.Object.DestroyImmediate(panelObject); // 임시 패널 제거
                UnityEngine.Object.DestroyImmediate(oldWeapon); // 기존 무기 제거
                UnityEngine.Object.DestroyImmediate(replacedWeapon); // 교체 대상 제거
                UnityEngine.Object.DestroyImmediate(newWeapon); // 신규 무기 제거
            }
        }

        private static void Require(bool condition, string message) // 검증 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                throw new InvalidOperationException("[Project Epsilon] Day19 merge verification failed: " + message); // 실패 예외 전달
            }
        }
    }
}
