using UnityEngine; // Unity 데이터 기능

namespace ProjectEpsilon.Progression // 성장 영역
{ // 영역 시작
    [CreateAssetMenu(fileName = "ExperiencePickupSettings", menuName = "Project Epsilon/Experience Pickup Settings")] // 설정 에셋 생성 메뉴
    public sealed class ExperiencePickupSettings : ScriptableObject // 경험치 보석 임시 설정
    { // 클래스 시작
        [Min(0.1f)] [SerializeField] private float attractionRange = 2.6f; // 보석 흡수 범위
        [Min(0.1f)] [SerializeField] private float attractionSpeed = 6f; // 보석 흡수 속도
        [Min(0.01f)] [SerializeField] private float collectDistance = 0.22f; // 보석 획득 거리

        public float AttractionRange => Mathf.Max(0.1f, attractionRange); // 안전한 흡수 범위
        public float AttractionSpeed => Mathf.Max(0.1f, attractionSpeed); // 안전한 흡수 속도
        public float CollectDistance => Mathf.Max(0.01f, collectDistance); // 안전한 획득 거리
    } // 클래스 끝
} // 영역 끝
