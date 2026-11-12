using UnityEngine; // Unity 데이터 기능

namespace ProjectEpsilon.Progression // 영구 성장 영역
{
    [CreateAssetMenu(fileName = "PrototypeMetaUnlockConfig", menuName = "Project Epsilon/Meta Unlock Config")] // 시험 설정 에셋 메뉴
    public sealed class MetaUnlockConfig : ScriptableObject // 미정 해금 대상 시험 설정
    {
        [SerializeField] private string firstBossCharacterId = "mobile"; // 평원 보상 캐릭터 임시 대상
        [Min(1)] [SerializeField] private int synergyThreshold = 2; // 포탑 해금 시너지 임시 기준
        [SerializeField] private string[] synergyTurretIds = new string[8]; // 속성별 포탑 임시 대상
        public string FirstBossCharacterId => firstBossCharacterId; // 실제 시험 캐릭터 식별자
        public int SynergyThreshold => Mathf.Max(1, synergyThreshold); // 안전한 속성 단계
        public int GetTurretAttribute(string id) // 포탑별 시험 해금 속성 조회
        {
            if (string.IsNullOrEmpty(id) || synergyTurretIds == null) // 실제 식별자와 목록 확인
            {
                return -1; // 별도 해금 없는 포탑
            }
            for (int index = 0; index < synergyTurretIds.Length && index < 8; index++) // 실제 속성별 대상 순회
            {
                if (synergyTurretIds[index] == id) // 실제 시험 대상 확인
                {
                    return index; // 실제 해금 속성 반환
                }
            }
            return -1; // 별도 해금 없는 포탑
        }
    }
}
