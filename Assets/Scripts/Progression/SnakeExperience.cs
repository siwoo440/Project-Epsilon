using System; // C# 기본 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{
    [DefaultExecutionOrder(120)] // 대상 설정
    public sealed class SnakeExperience : MonoBehaviour // 경험치·행동 자료 구조
    {
        [SerializeField] private int currentLevel = 1; // 필드·현재 설정
        [SerializeField] private int currentExperience; // 필드·현재 설정
        [SerializeField] private int baseRequiredExperience = 10; // 필드·경험치 설정
        [SerializeField] private float experienceGrowthMultiplier = 1.12f; // 필드·경험치 설정
        [SerializeField] private bool levelUpPending; // 필드·레벨 설정
        [SerializeField] private ExperiencePickupSettings pickupSettings; // 보석 흡수 설정
        private decimal zoneExperienceBonusRemainder; // 지역 경험치 소수 보너스

        public static SnakeExperience Current { get; private set; } // 경험치·현재 구성 선언

        public event Action<int, int> ExperienceChanged; // 이벤트·동작 구성 선언
        public event Action<int> ExperienceGained; // 실제 획득 경험치 알림
        public event Action<int> LevelChanged; // 이벤트·동작 구성 선언
        public event Action<int> LevelUpRequested; // 이벤트·동작 구성 선언

        public int BaseRequirement => baseRequiredExperience; // 실제 경험치 기본 요구량
        public float GrowthMultiplier => experienceGrowthMultiplier; // 실제 경험치 요구량 증가율
        public int CurrentLevel => currentLevel; // 현재·레벨 값 반환
        public int CurrentExperience => currentExperience; // 현재·경험치 값 반환
        public ExperiencePickupSettings PickupSettings => pickupSettings; // 새 보석의 흡수 설정

        public void ConfigurePickupSettings(ExperiencePickupSettings settings) // 새 보석 설정 연결
        {
            pickupSettings = settings; // 흡수 설정 저장
        }
        public int RequiredExperience => // 경험치 값 반환
            CalculateRequiredExperience( // 계산·경험치 호출 시작
                baseRequiredExperience, // 경험치 항목 전달
                experienceGrowthMultiplier, // 경험치·배율 항목 전달
                currentLevel // 현재·레벨 처리
            ); // 앞선 항목 종료

        // Day09 코드와의 호환용 별칭이다.
        public int PreviewRequiredExperience => RequiredExperience; // 경험치 값 반환
        public bool IsLevelUpPending => levelUpPending; // 레벨 값 반환

        private void OnEnable() // 활성 기능 선언
        {
            if (Application.isPlaying) // 대상 조건 확인
            {
                Current = this; // 현재 값 설정
            }
        }

        private void Start() // 시작 기능 선언
        {
            NormalizeValues(); // 대상 기능 실행
            NotifyLevelChanged(); // 알림·레벨 기능 실행
            NotifyExperienceChanged(); // 알림·경험치 기능 실행
        }

        private void OnDisable() // 비활성 기능 선언
        {
            if (Current == this) // 현재 조건 확인
            {
                Current = null; // 현재 값 설정
            }
        }

        public void Configure( // 구성 호출 시작
            int baseRequirement, // 대상 항목 전달
            float growthMultiplier // 배율 처리
        ) // 앞선 항목 종료
        {
            baseRequiredExperience = Mathf.Max(1, baseRequirement); // 경험치·최대 값 설정
            experienceGrowthMultiplier = Mathf.Max(1f, growthMultiplier); // 경험치·배율 값 설정

            NormalizeValues(); // 대상 기능 실행
            NotifyLevelChanged(); // 알림·레벨 기능 실행
            NotifyExperienceChanged(); // 알림·경험치 기능 실행
        }

        // Day09 Setup 또는 외부 코드가 기존 시그니처를 사용해도 깨지지 않게 유지한다.
        public void Configure(int baseRequirement) // 구성 기능 선언
        {
            Configure(baseRequirement, 1.12f); // 구성 기능 실행
        }

        public void ResetProgression() // 초기화 기능 선언
        {
            zoneExperienceBonusRemainder = 0m; // 새 성장의 지역 소수 초기화
            currentLevel = 1; // 현재·레벨 값 설정
            currentExperience = 0; // 현재·경험치 값 설정
            levelUpPending = false; // 레벨 값 설정

            NotifyLevelChanged(); // 알림·레벨 기능 실행
            NotifyExperienceChanged(); // 알림·경험치 기능 실행
        }

        public void ResetExperience() // 초기화·경험치 기능 선언
        {
            zoneExperienceBonusRemainder = 0m; // 경험치 초기화의 소수 제거
            currentExperience = 0; // 현재·경험치 값 설정
            levelUpPending = false; // 레벨 값 설정
            NotifyExperienceChanged(); // 알림·경험치 기능 실행
        }

        public void AddExperience(int amount) // 추가·경험치 기능 선언
        {
            if (amount <= 0) // 수량 조건 확인
            {
                return; // 현재 기능 종료
            }

            float zoneMultiplier = ProjectEpsilon.World.PrototypeSpecialZone.ExperienceMultiplierAt(transform.position) * MetaModifiers.ExperienceMultiplier; // 지역과 영구 성장 경험치 배율
            decimal zoneBonus = amount * ((decimal)zoneMultiplier - 1m) + zoneExperienceBonusRemainder; // 실제 소수 보너스 누적
            int wholeBonus = zoneBonus >= int.MaxValue ? int.MaxValue : (int)decimal.Floor(zoneBonus); // 정수 범위 내 추가 경험치
            zoneExperienceBonusRemainder = zoneBonus >= int.MaxValue ? 0m : zoneBonus - wholeBonus; // 다음 획득의 소수 보존
            amount = wholeBonus > int.MaxValue - amount ? int.MaxValue : amount + wholeBonus; // 정수 범위 내 최종 획득량

            int applied = Math.Min(amount, int.MaxValue - currentExperience); // 실제 반영할 경험치
            if (currentExperience > int.MaxValue - amount) // 현재·경험치 조건 확인
            {
                currentExperience = int.MaxValue; // 현재·경험치 값 설정
            }
            else // 다른 조건 처리
            {
                currentExperience += amount; // 현재·경험치 값 설정
            }

            ExperienceGained?.Invoke(applied); // 실제 반영 경험치 알림
            TryRequestNextLevelUp(); // 시도·다음 값 기능 실행
            NotifyExperienceChanged(); // 알림·경험치 기능 실행
        }

        public bool CompletePendingLevelUp() // 완료·레벨 기능 선언
        {
            if (!levelUpPending) // 레벨 조건 확인
            {
                return false; // 실패 결과 반환
            }

            levelUpPending = false; // 레벨 값 설정

            bool requestedNext = // 다음 값 값 설정
                TryRequestNextLevelUp(); // 시도·다음 값 기능 실행

            NotifyExperienceChanged(); // 알림·경험치 기능 실행
            return requestedNext; // 다음 값 결과 반환
        }

        private bool TryRequestNextLevelUp() // 시도·다음 값 기능 선언
        {
            if (levelUpPending) // 레벨 조건 확인
            {
                return false; // 실패 결과 반환
            }

            int required = RequiredExperience; // 경험치 값 설정

            if (currentExperience < required) // 현재·경험치 조건 확인
            {
                return false; // 실패 결과 반환
            }

            currentExperience -= required; // 현재·경험치 값 설정
            currentLevel++; // 현재·레벨 처리
            levelUpPending = true; // 레벨 값 설정

            NotifyLevelChanged(); // 알림·레벨 기능 실행
            LevelUpRequested?.Invoke(currentLevel); // 레벨·현재 기능 실행

            return true; // 성공 결과 반환
        }

        private void NormalizeValues() // 대상 기능 선언
        {
            currentLevel = Mathf.Max(1, currentLevel); // 현재·레벨 값 설정
            currentExperience = Mathf.Max(0, currentExperience); // 현재·경험치 값 설정
            baseRequiredExperience = // 경험치 값 설정
                Mathf.Max(1, baseRequiredExperience); // 최대·경험치 기능 실행
            experienceGrowthMultiplier = // 경험치·배율 값 설정
                Mathf.Max(1f, experienceGrowthMultiplier); // 최대·경험치 기능 실행
        }

        private void NotifyExperienceChanged() // 알림·경험치 기능 선언
        {
            ExperienceChanged?.Invoke( // 경험치 호출 시작
                currentExperience, // 현재·경험치 항목 전달
                RequiredExperience // 경험치 처리
            ); // 앞선 항목 종료
        }

        private void NotifyLevelChanged() // 알림·레벨 기능 선언
        {
            LevelChanged?.Invoke(currentLevel); // 레벨·현재 기능 실행
        }

        public static int CalculateRequiredExperience( // 계산·경험치 호출 시작
            int baseRequirement, // 대상 항목 전달
            float growthMultiplier, // 배율 항목 전달
            int level // 레벨 처리
        ) // 앞선 항목 종료
        {
            int safeBase = Mathf.Max(1, baseRequirement); // 최대 값 설정
            float safeGrowth = Mathf.Max(1f, growthMultiplier); // 최대·배율 값 설정
            int safeLevel = Mathf.Max(1, level); // 레벨·최대 값 설정

            double calculated = // 대상 값 설정
                safeBase * // 대상 처리
                Math.Pow( // 대상 호출 시작
                    safeGrowth, // 대상 항목 전달
                    safeLevel - 1 // 레벨 처리
                ); // 앞선 항목 종료

            if (calculated >= int.MaxValue) // 최대·값 조건 확인
            {
                return int.MaxValue; // 최대·값 결과 반환
            }

            return Mathf.Max( // 최대 결과 반환
                1, // 대상 항목 전달
                Mathf.RoundToInt((float)calculated) // 기획 기준 반올림
            ); // 앞선 항목 종료
        }
    }
}
