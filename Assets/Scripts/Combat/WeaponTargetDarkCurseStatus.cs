using ProjectEpsilon.Core; // 게임 상태 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{ // 네임스페이스 시작
    public sealed class WeaponTargetDarkCurseStatus : MonoBehaviour // 암흑 저주 상태
    { // 클래스 시작
        [SerializeField] private int stacks; // 현재 저주 중첩
        [SerializeField] private float remainingDuration; // 남은 지속 시간

        public int Stacks => WeaponAttributeDarkRules.NormalizeCurseStacks(stacks); // 현재 중첩 반환
        public float RemainingDuration => remainingDuration; // 남은 시간 반환
        public bool IsActive => Stacks > 0 && remainingDuration > 0f; // 활성 상태 반환

        private void Update() // 저주 시간 갱신
        { // 메서드 시작
            if (!IsActive || GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 갱신 가능 여부 확인
            { // 조건 시작
                return; // 시간 갱신 중단
            } // 조건 끝

            remainingDuration = Mathf.Max(0f, remainingDuration - Time.deltaTime); // 남은 시간 감소

            if (remainingDuration <= 0f) // 저주 종료 확인
            { // 조건 시작
                Clear(); // 저주 초기화
            } // 조건 끝
        } // 메서드 끝

        public int Apply(int synergyStage, int amount) // 저주 중첩 적용
        { // 메서드 시작
            int safeAmount = Mathf.Max(0, amount); // 적용 수 보정

            for (int index = 0; index < safeAmount; index++) // 요청 중첩 순회
            { // 반복 시작
                stacks = WeaponAttributeDarkRules.ApplyCurseStack(stacks, synergyStage); // 중첩 한 개 적용
            } // 반복 끝

            if (stacks > 0) // 활성 저주 확인
            { // 조건 시작
                remainingDuration = WeaponAttributeDarkRules.CurseDuration; // 지속 시간 갱신
            } // 조건 끝

            return Stacks; // 최종 중첩 반환
        } // 메서드 끝

        public void Clear() // 저주 초기화
        { // 메서드 시작
            stacks = 0; // 중첩 초기화
            remainingDuration = 0f; // 시간 초기화
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
