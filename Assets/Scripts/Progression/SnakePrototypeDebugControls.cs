using ProjectEpsilon.Combat; // 전투 기능 연결
using ProjectEpsilon.Core; // 대상 기능 연결
using ProjectEpsilon.Data; // 데이터 기능 연결
using ProjectEpsilon.Player; // 플레이어 기능 연결
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.InputSystem; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{
    public sealed class SnakePrototypeDebugControls : MonoBehaviour // 행동 자료 구조
    {
        [SerializeField] private SnakeHealth health; // 필드·체력 설정
        [SerializeField] private SnakeBodyManager bodyManager; // 필드·몸통 설정
        [SerializeField] private SnakeWeaponManager weaponManager; // 필드·포탑 설정
        [SerializeField] private WeaponData debugWeapon; // 필드·포탑 설정
        [SerializeField] private Sprite pickupSprite; // 필드·이미지 설정

        public void Configure( // 구성 호출 시작
            SnakeHealth snakeHealth, // 체력 항목 전달
            SnakeBodyManager manager, // 몸통·관리자 항목 전달
            SnakeWeaponManager weapons, // 포탑·관리자 항목 전달
            WeaponData gradeTestWeapon, // 포탑·데이터 항목 전달
            Sprite visual // 이미지·시각 효과 처리
        ) // 앞선 항목 종료
        {
            health = snakeHealth; // 체력 값 설정
            bodyManager = manager; // 몸통·관리자 값 설정
            weaponManager = weapons; // 포탑·관리자 값 설정
            debugWeapon = gradeTestWeapon; // 포탑·등급 값 설정
            pickupSprite = visual; // 이미지·시각 효과 값 설정
        }

        private void Update() // 갱신 기능 선언
        {
            if (GameManager.Instance != null && // 게임·관리자 조건 확인
                !GameManager.Instance.IsPlaying) // 게임·관리자 처리
            {
                return; // 현재 기능 종료
            }

            Keyboard keyboard = // 키보드 값 설정
                Keyboard.current; // 키보드·현재 처리

            if (keyboard == null) // 키보드 조건 확인
            {
                return; // 현재 기능 종료
            }

            if (keyboard.hKey.wasPressedThisFrame) // 키보드·키 조건 확인
            {
                SpawnRecovery( // 생성·회복 호출 시작
                    RecoveryPickupType.Heal // 회복·형식 처리
                ); // 앞선 항목 종료
            }

            if (keyboard.jKey.wasPressedThisFrame) // 키보드·키 조건 확인
            {
                SpawnRecovery( // 생성·회복 호출 시작
                    RecoveryPickupType.BodyRepair // 회복·형식 처리
                ); // 앞선 항목 종료
            }

            if (!keyboard.leftShiftKey.isPressed && !keyboard.rightShiftKey.isPressed) // 등급 디버그 조합 확인
            {
                return; // 숫자 단독 입력 보호
            }

            SnakeWeaponMergeController mergeController = GetComponent<SnakeWeaponMergeController>(); // 합성 화면 조회
            if (mergeController != null && mergeController.IsMergeOpen) // 합성 선택 중 확인
            {
                return; // 합성 선택 입력 보호
            }

            if (keyboard.digit3Key.wasPressedThisFrame) // 키보드·키 조건 확인
            {
                SetDebugGrade(3); // 설정·등급 기능 실행
            }

            if (keyboard.digit5Key.wasPressedThisFrame) // 키보드·키 조건 확인
            {
                SetDebugGrade(5); // 설정·등급 기능 실행
            }
        }

        private void SpawnRecovery( // 생성·회복 호출 시작
            RecoveryPickupType type // 회복·형식 처리
        ) // 앞선 항목 종료
        {
            RecoveryPickup.Spawn( // 회복·생성 호출 시작
                transform.position + // 위치 처리
                transform.right * // 위치 처리
                (type == // 형식 기능 실행
                RecoveryPickupType.Heal // 회복·형식 처리
                    ? 0.8f // 대상 처리
                    : -0.8f), // 대상 항목 전달
                transform, // 위치 항목 전달
                health, // 체력 항목 전달
                bodyManager, // 몸통·관리자 항목 전달
                type, // 형식 항목 전달
                type == // 형식 처리
                RecoveryPickupType.Heal // 회복·형식 처리
                    ? 15 // 대상 처리
                    : 1, // 대상 항목 전달
                pickupSprite // 이미지 처리
            ); // 앞선 항목 종료
        }

        private void SetDebugGrade( // 설정·등급 호출 시작
            int grade // 등급 처리
        ) // 앞선 항목 종료
        {
            if (weaponManager == null || // 포탑·관리자 조건 확인
                debugWeapon == null || // 포탑 처리
                weaponManager.SlotCount <= 0) // 포탑·관리자 처리
            {
                return; // 현재 기능 종료
            }

            weaponManager.TryEquipAt( // 포탑·관리자 호출 시작
                0, // 대상 항목 전달
                debugWeapon, // 포탑 항목 전달
                grade // 등급 처리
            ); // 앞선 항목 종료
        }
    }
}
