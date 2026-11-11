using ProjectEpsilon.Combat; // 전투 기능 연결
using ProjectEpsilon.Core; // 대상 기능 연결
using ProjectEpsilon.Data; // 데이터 기능 연결
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.InputSystem; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{
    public sealed class SnakeWeaponMergeDebugControls : MonoBehaviour // 포탑·합성 자료 구조
    {
        [SerializeField] private SnakeWeaponManager weaponManager; // 필드·포탑 설정
        [SerializeField] private WeaponData debugWeapon; // 필드·포탑 설정

        public void Configure( // 구성 호출 시작
            SnakeWeaponManager manager, // 포탑·관리자 항목 전달
            WeaponData weapon // 포탑·데이터 처리
        ) // 앞선 항목 종료
        {
            weaponManager = manager; // 포탑·관리자 값 설정
            debugWeapon = weapon; // 포탑 값 설정
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

            if (keyboard == null || // 키보드 조건 확인
                !keyboard.nKey.wasPressedThisFrame) // 키보드·키 처리
            {
                return; // 현재 기능 종료
            }

            PrepareDebugMergePair(); // 합성 기능 실행
        }

        public bool PrepareDebugMergePair() // 합성 기능 선언
        {
            if (weaponManager == null || // 포탑·관리자 조건 확인
                debugWeapon == null || // 포탑 처리
                weaponManager.SlotCount < 2) // 포탑·관리자 처리
            {
                return false; // 실패 결과 반환
            }

            bool first = // 대상 값 설정
                weaponManager.TryEquipAt( // 포탑·관리자 호출 시작
                    0, // 대상 항목 전달
                    debugWeapon, // 포탑 항목 전달
                    1 // 대상 처리
                ); // 앞선 항목 종료

            bool second = // 대상 값 설정
                weaponManager.TryEquipAt( // 포탑·관리자 호출 시작
                    1, // 대상 항목 전달
                    debugWeapon, // 포탑 항목 전달
                    1 // 대상 처리
                ); // 앞선 항목 종료

            return first && second; // 대상 결과 반환
        }
    }
}
