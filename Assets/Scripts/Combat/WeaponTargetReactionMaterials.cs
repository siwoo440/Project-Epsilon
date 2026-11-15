using ProjectEpsilon.Combat; // 대상별 반응 재료 상태
using UnityEngine; // Unity 구성 요소

namespace ProjectEpsilon.Combat // 전투 영역
{ // 이름 공간 시작
    public sealed class WeaponTargetReactionMaterials : MonoBehaviour // 대상별 반응 재료 보관
    { // 형식 시작
        private readonly WeaponAttributeReactionState state = new WeaponAttributeReactionState(); // 대상별 재료 상태
        public WeaponAttributeReactionState State => state; // 반응 재료 상태 조회
    } // 형식 종료
} // 이름 공간 종료
