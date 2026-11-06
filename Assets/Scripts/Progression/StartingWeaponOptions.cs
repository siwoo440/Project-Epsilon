using ProjectEpsilon.Data; // 무기 데이터
using UnityEngine; // 유니티 구성

namespace ProjectEpsilon.Progression // 게임 진행 영역
{ // 영역 시작
    [CreateAssetMenu(fileName = "StartingWeaponOptions", menuName = "Project Epsilon/Progression/Starting Weapon Options")] // 시작 후보 데이터 생성
    public sealed class StartingWeaponOptions : ScriptableObject // 시작 후보 데이터
    { // 클래스 시작
        [SerializeField] private WeaponData[] weapons = new WeaponData[3]; // 임시 무기 후보
        public WeaponData[] Weapons => weapons; // 시작 후보 조회
        public void Configure(WeaponData[] values) // 시험용 후보 설정
        { // 메서드 시작
            weapons = values; // 후보 목록 저장
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
