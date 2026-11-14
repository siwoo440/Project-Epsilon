using ProjectEpsilon.Data; // 기존 속성 구분
using UnityEngine; // 이미지와 객체 기능

namespace ProjectEpsilon.Combat // 전투 표시 영역
{ // 영역 시작
    [RequireComponent(typeof(WeaponTarget))] // 실제 공격 대상 필수
    public sealed class WeaponTargetStatusIndicators : MonoBehaviour // 적의 지속 상태 표시
    { // 클래스 시작
        [Min(1)] [SerializeField] private int simultaneousLimit = 48; // 동시 표시 적 수 시험값
        private static int activeCount; // 현재 표시 중인 적 수
        private WeaponTarget target; // 실제 공격 대상
        private WeaponTargetColdStatus cold; // 실제 냉기 상태
        private WeaponTargetPoisonStatus poison; // 실제 독 상태
        private WeaponTargetDarkCurseStatus dark; // 실제 암흑 저주 상태
        private WeaponTargetCorrosiveCurseStatus corrosive; // 실제 부식 저주 상태
        private WeaponTargetCorrosiveBurnStatus corrosiveBurn; // 실제 부식 연소 상태
        private GameObject coldIcon; // 실제 냉기 이미지
        private GameObject poisonIcon; // 실제 독 이미지
        private GameObject darkIcon; // 실제 암흑 저주 이미지
        private GameObject corrosiveIcon; // 실제 부식 저주 이미지
        private GameObject corrosiveBurnIcon; // 실제 부식 연소 이미지
        private bool hasSlot; // 실제 표시 자리 보유 여부
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 새 플레이 세션 초기화
        private static void ResetSession() // 이전 세션 표시 수 정리
        { // 함수 시작
            activeCount = 0; // 이전 표시 자리 초기화
        } // 함수 종료
        public static void Ensure(WeaponTarget target) // 실제 상태 적용 대상의 표시 준비
        { // 함수 시작
            if (target != null && target.GetComponent<WeaponTargetStatusIndicators>() == null) // 유효 대상과 기존 표시 확인
            { // 조건 시작
                target.gameObject.AddComponent<WeaponTargetStatusIndicators>(); // 실제 대상에 표시 연결
            } // 조건 종료
        } // 함수 종료
        private void Awake() // 실제 대상 확보
        { // 함수 시작
            target = GetComponent<WeaponTarget>(); // 같은 객체의 공격 대상
        } // 함수 종료
        private void LateUpdate() // 실제 상태와 표시 동기화
        { // 함수 시작
            if (target == null || !target.IsAlive) // 대상 부재 또는 사망 확인
            { // 조건 시작
                Destroy(this); // 불필요한 표시 정리
                return; // 후속 표시 중단
            } // 조건 종료
            if (cold == null) // 실제 냉기 상태 재조회 필요 여부
            { // 조건 시작
                cold = GetComponent<WeaponTargetColdStatus>(); // 현재 냉기 상태 조회
            } // 조건 종료
            if (poison == null) // 실제 독 상태 재조회 필요 여부
            { // 조건 시작
                poison = GetComponent<WeaponTargetPoisonStatus>(); // 현재 독 상태 조회
            } // 조건 종료
            if (dark == null) // 실제 암흑 저주 재조회 필요 여부
            { // 조건 시작
                dark = GetComponent<WeaponTargetDarkCurseStatus>(); // 현재 암흑 저주 조회
            } // 조건 종료
            if (corrosive == null) // 실제 부식 저주 재조회 필요 여부
            { // 조건 시작
                corrosive = GetComponent<WeaponTargetCorrosiveCurseStatus>(); // 현재 부식 저주 조회
            } // 조건 종료
            if (corrosiveBurn == null) // 실제 부식 연소 재조회 필요 여부
            { // 조건 시작
                corrosiveBurn = GetComponent<WeaponTargetCorrosiveBurnStatus>(); // 현재 부식 연소 조회
            } // 조건 종료
            bool showCold = cold != null && cold.isActiveAndEnabled && cold.RemainingDuration > 0f; // 실제 냉기 지속 여부
            bool showPoison = poison != null && poison.isActiveAndEnabled && poison.IsActive; // 실제 독 지속 여부
            bool showDark = dark != null && dark.isActiveAndEnabled && dark.IsActive; // 실제 암흑 저주 지속 여부
            bool showCorrosive = corrosive != null && corrosive.isActiveAndEnabled && corrosive.IsActive; // 실제 부식 저주 지속 여부
            bool showCorrosiveBurn = corrosiveBurn != null && corrosiveBurn.isActiveAndEnabled && corrosiveBurn.IsActive; // 실제 부식 연소 지속 여부
            if (!showCold && !showPoison && !showDark && !showCorrosive && !showCorrosiveBurn) // 모든 상태의 해제 여부
            { // 조건 시작
                Destroy(this); // 표시와 자리 정리
                return; // 후속 표시 중단
            } // 조건 종료
            if (!hasSlot && activeCount < Mathf.Max(1, simultaneousLimit)) // 남은 동시 표시 자리 확인
            { // 조건 시작
                hasSlot = true; // 실제 자리 확보
                activeCount++; // 동시 표시 수 증가
            } // 조건 종료
            if (!hasSlot) // 표시 상한으로 대기 여부
            { // 조건 시작
                return; // 다음 프레임 자리 재확인
            } // 조건 종료
            if (showCold && coldIcon == null) // 실제 냉기 표시 생성 여부
            { // 조건 시작
                coldIcon = CreateIcon("StatusColdIndicator", CombatVfx.GetAttributeSprite(WeaponAttribute.Cold), -0.19f, 0.7f); // 기존 냉기 이미지 연결
            } // 조건 종료
            if (showPoison && poisonIcon == null) // 실제 독 표시 생성 여부
            { // 조건 시작
                poisonIcon = CreateIcon("StatusPoisonIndicator", CombatVfx.GetAttributeSprite(WeaponAttribute.Poison), 0.19f, 0.7f); // 기존 독 이미지 연결
            } // 조건 종료
            if (showDark && darkIcon == null) // 실제 암흑 저주 표시 생성 여부
            { // 조건 시작
                darkIcon = CreateIcon("StatusDarkIndicator", CombatVfx.GetAttributeSprite(WeaponAttribute.Dark), -0.19f, 1.05f); // 기존 암흑 이미지 연결
            } // 조건 종료
            if (showCorrosive && corrosiveIcon == null) // 실제 부식 저주 표시 생성 여부
            { // 조건 시작
                corrosiveIcon = CreateIcon("StatusCorrosiveCurseIndicator", CombatVfx.GetReactionSprite(WeaponAttributeReaction.CorrosiveCurse), 0.19f, 1.05f); // 기존 반응 이미지 연결
            } // 조건 종료
            if (showCorrosiveBurn && corrosiveBurnIcon == null) // 실제 부식 연소 표시 생성 여부
            { // 조건 시작
                corrosiveBurnIcon = CreateIcon("StatusCorrosiveBurnIndicator", CombatVfx.GetReactionSprite(WeaponAttributeReaction.CorrosiveBurn), 0f, 1.4f); // 기존 연소 반응 이미지 연결
            } // 조건 종료
            if (coldIcon != null) // 실제 냉기 표시 존재 여부
            { // 조건 시작
                coldIcon.SetActive(showCold); // 실제 냉기 지속 표시
            } // 조건 종료
            if (poisonIcon != null) // 실제 독 표시 존재 여부
            { // 조건 시작
                poisonIcon.SetActive(showPoison); // 실제 독 지속 표시
            } // 조건 종료
            if (darkIcon != null) // 실제 암흑 저주 표시 존재 여부
            { // 조건 시작
                darkIcon.SetActive(showDark); // 실제 암흑 저주 지속 표시
            } // 조건 종료
            if (corrosiveIcon != null) // 실제 부식 저주 표시 존재 여부
            { // 조건 시작
                corrosiveIcon.SetActive(showCorrosive); // 실제 부식 저주 지속 표시
            } // 조건 종료
            if (corrosiveBurnIcon != null) // 실제 부식 연소 표시 존재 여부
            { // 조건 시작
                corrosiveBurnIcon.SetActive(showCorrosiveBurn); // 실제 부식 연소 지속 표시
            } // 조건 종료
        } // 함수 종료
        private GameObject CreateIcon(string name, Sprite sprite, float x, float y) // 기존 속성과 반응 이미지 재사용
        { // 함수 시작
            GameObject icon = new GameObject(name); // 적에 연결할 표시 객체
            icon.transform.SetParent(transform, false); // 실제 적 위치 추적
            icon.transform.localPosition = new Vector3(x, y, 0f); // 실제 적 위 표시 위치
            icon.transform.localScale = Vector3.one * 0.32f; // 실제 표시 크기 시험값
            SpriteRenderer renderer = icon.AddComponent<SpriteRenderer>(); // 실제 이미지 표시기
            renderer.sprite = sprite; // 기존 속성 또는 반응 이미지
            renderer.sortingOrder = 17; // 적 위와 공격 경고 아래 표시
            return icon; // 실제 표시 반환
        } // 함수 종료
        private void OnDisable() // 비활성화 시 자리 반환
        { // 함수 시작
            if (hasSlot) // 실제 보유 자리 확인
            { // 조건 시작
                hasSlot = false; // 표시 자리 해제
                activeCount = Mathf.Max(0, activeCount - 1); // 실제 동시 표시 수 감소
            } // 조건 종료
            if (coldIcon != null) // 실제 냉기 표시 확인
            { // 조건 시작
                coldIcon.SetActive(false); // 비활성 상태의 표시 숨김
            } // 조건 종료
            if (poisonIcon != null) // 실제 독 표시 확인
            { // 조건 시작
                poisonIcon.SetActive(false); // 비활성 상태의 표시 숨김
            } // 조건 종료
            if (darkIcon != null) // 실제 암흑 저주 표시 확인
            { // 조건 시작
                darkIcon.SetActive(false); // 비활성 상태의 표시 숨김
            } // 조건 종료
            if (corrosiveIcon != null) // 실제 부식 저주 표시 확인
            { // 조건 시작
                corrosiveIcon.SetActive(false); // 비활성 상태의 표시 숨김
            } // 조건 종료
            if (corrosiveBurnIcon != null) // 실제 부식 연소 표시 확인
            { // 조건 시작
                corrosiveBurnIcon.SetActive(false); // 비활성 상태의 표시 숨김
            } // 조건 종료
        } // 함수 종료
        private void OnDestroy() // 실제 생성 표시 제거
        { // 함수 시작
            if (coldIcon != null) // 실제 냉기 표시 확인
            { // 조건 시작
                Destroy(coldIcon); // 생성한 냉기 표시 제거
            } // 조건 종료
            if (poisonIcon != null) // 실제 독 표시 확인
            { // 조건 시작
                Destroy(poisonIcon); // 생성한 독 표시 제거
            } // 조건 종료
            if (darkIcon != null) // 실제 암흑 저주 표시 확인
            { // 조건 시작
                Destroy(darkIcon); // 생성한 암흑 저주 표시 제거
            } // 조건 종료
            if (corrosiveIcon != null) // 실제 부식 저주 표시 확인
            { // 조건 시작
                Destroy(corrosiveIcon); // 생성한 부식 저주 표시 제거
            } // 조건 종료
            if (corrosiveBurnIcon != null) // 실제 부식 연소 표시 확인
            { // 조건 시작
                Destroy(corrosiveBurnIcon); // 생성한 부식 연소 표시 제거
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
