using ProjectEpsilon.Data; // 무기 데이터
using UnityEngine; // 유니티 구성

namespace ProjectEpsilon.Progression // 게임 진행 영역
{
    [CreateAssetMenu(fileName = "StartingWeaponOptions", menuName = "Project Epsilon/Progression/Starting Weapon Options")] // 시작 후보 데이터 생성
    public sealed class StartingWeaponOptions : ScriptableObject // 시작 후보 데이터
    {
        [SerializeField] private WeaponData[] weapons = new WeaponData[3]; // 임시 무기 후보
        public WeaponData[] Weapons => weapons; // 시작 후보 조회
        public void Configure(WeaponData[] values) // 시험용 후보 설정
        {
            weapons = values; // 후보 목록 저장
        }
    }
}
