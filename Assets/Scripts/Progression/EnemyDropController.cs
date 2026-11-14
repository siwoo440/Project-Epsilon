using ProjectEpsilon.Combat; // 전투 기능 연결
using ProjectEpsilon.Player; // 플레이어 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{
    [RequireComponent(typeof(WeaponTarget))] // 구성 요소·포탑 설정
    public sealed class EnemyDropController : MonoBehaviour // 적·드롭 자료 구조
    {
        [SerializeField] private WeaponTarget target; // 필드·포탑 설정

        [Header("Experience")] // 경험치 설정
        [Min(1)] // 최소 설정
        [SerializeField] private int experienceValue = 1; // 필드·경험치 설정
        [SerializeField] private int experienceGemCount = 1; // 경험치 보석 개수

        [Header("Recovery")] // 회복 설정
        [Range(0f, 1f)] // 범위 설정
        [SerializeField] private float healDropChance = 0.25f; // 필드·회복 설정

        [Range(0f, 1f)] // 범위 설정
        [SerializeField] private float bodyRepairDropChance = 0.10f; // 필드·몸통 설정
        [Range(0f, 1f)] [SerializeField] private float recoveryDropMultiplier = 0.5f; // 회복 아이템 공통 확률 배율

        [Min(1)] // 최소 설정
        [SerializeField] private int healAmount = 15; // 필드·회복 설정

        [SerializeField] private Sprite pickupSprite; // 필드·이미지 설정

        private bool subscribed; // 대상 구성 선언

        private void Awake() // 초기화 기능 선언
        {
            EnsureTarget(); // 확인·대상 기능 실행
        }

        private void OnEnable() // 활성 기능 선언
        {
            Subscribe(); // 구독 기능 실행
        }

        private void OnDisable() // 비활성 기능 선언
        {
            Unsubscribe(); // 구독 해제 기능 실행
        }

        public void Configure( // 구성 호출 시작
            int experience, // 경험치 항목 전달
            int healing, // 회복 항목 전달
            float healChance, // 회복·확률 항목 전달
            float repairChance, // 확률 항목 전달
            Sprite visual, // 보석 시각 자료
            int gemCount = 1 // 경험치 보석 개수
        ) // 앞선 항목 종료
        {
            experienceValue = // 경험치·값 값 설정
                Mathf.Max( // 최대 호출 시작
                    1, // 대상 항목 전달
                    experience // 경험치 처리
                ); // 앞선 항목 종료

            healAmount = // 회복·수량 값 설정
                Mathf.Max( // 최대 호출 시작
                    1, // 대상 항목 전달
                    healing // 회복 처리
                ); // 앞선 항목 종료

            healDropChance = // 회복·드롭 값 설정
                Mathf.Clamp01( // 대상 호출 시작
                    healChance // 회복·확률 처리
                ); // 앞선 항목 종료

            bodyRepairDropChance = // 몸통·드롭 값 설정
                Mathf.Clamp01( // 대상 호출 시작
                    repairChance // 확률 처리
                ); // 앞선 항목 종료

            pickupSprite = // 이미지 값 설정
                visual; // 시각 효과 처리
            experienceGemCount = Mathf.Clamp(gemCount, 1, experienceValue); // 경험치 보석 수 제한

            EnsureTarget(); // 확인·대상 기능 실행
            Subscribe(); // 구독 기능 실행
        }

        private void EnsureTarget() // 확인·대상 기능 선언
        {
            if (target == null) // 대상 조건 확인
            {
                target = // 대상 값 설정
                    GetComponent<WeaponTarget>(); // 조회·구성 요소 기능 실행
            }
        }

        private void Subscribe() // 구독 기능 선언
        {
            EnsureTarget(); // 확인·대상 기능 실행

            if (subscribed || // 대상 조건 확인
                target == null) // 대상 처리
            {
                return; // 현재 기능 종료
            }

            target.Died += // 대상 값 설정
                HandleTargetDied; // 대상 처리

            subscribed = true; // 대상 값 설정
        }

        private void Unsubscribe() // 구독 해제 기능 선언
        {
            if (!subscribed || // 대상 조건 확인
                target == null) // 대상 처리
            {
                subscribed = false; // 대상 값 설정
                return; // 현재 기능 종료
            }

            target.Died -= // 대상 값 설정
                HandleTargetDied; // 대상 처리

            subscribed = false; // 대상 값 설정
        }

        private void HandleTargetDied( // 대상 호출 시작
            WeaponTarget deadTarget // 포탑·대상 처리
        ) // 앞선 항목 종료
        {
            if (deadTarget == null) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            SnakeExperience experience = // 경험치 값 설정
                SnakeExperience.Current; // 경험치·현재 처리

            if (experience == null) // 경험치 조건 확인
            {
                experience = // 경험치 값 설정
                    Object.FindFirstObjectByType<SnakeExperience>(); // 객체·검색 기능 실행
            }

            SnakeHealth health = // 체력 값 설정
                Object.FindFirstObjectByType<SnakeHealth>(); // 객체·검색 기능 실행

            SnakeBodyManager bodyManager = // 몸통·관리자 값 설정
                Object.FindFirstObjectByType<SnakeBodyManager>(); // 객체·검색 기능 실행

            Transform receiver = // 위치 값 설정
                health == null // 체력 처리
                    ? null // 대상 처리
                    : health.transform; // 체력·위치 처리

            Vector3 position = // 벡터·위치 값 설정
                deadTarget.transform.position; // 대상·위치 처리

            int count = Mathf.Clamp(experienceGemCount, 1, experienceValue); // 유효 보석 수 계산
            for (int index = 0; index < count; index++) // 경험치 보석 생성 반복
            {
                int gemValue = experienceValue / count + (index < experienceValue % count ? 1 : 0); // 보석별 경험치 분배
                float offset = (index - (count - 1) * 0.5f) * 0.3f; // 보석 간 간격 계산
                ExperiencePickup.Spawn(position + Vector3.right * offset, gemValue, pickupSprite, experience); // 경험치 보석 생성
            }

            if (health != null && // 체력 조건 확인
                Random.value < // 난수·값 처리
                healDropChance * Mathf.Clamp01(recoveryDropMultiplier)) // 조정된 체력 아이템 확률
            {
                RecoveryPickup.Spawn( // 회복·생성 호출 시작
                    position + // 위치 처리
                    Vector3.right * // 벡터 처리
                    0.22f, // 대상 항목 전달
                    receiver, // 대상 항목 전달
                    health, // 체력 항목 전달
                    bodyManager, // 몸통·관리자 항목 전달
                    RecoveryPickupType.Heal, // 회복·형식 항목 전달
                    healAmount, // 회복·수량 항목 전달
                    pickupSprite // 이미지 처리
                ); // 앞선 항목 종료
            }

            if (bodyManager != null && // 몸통·관리자 조건 확인
                Random.value < // 난수·값 처리
                bodyRepairDropChance * Mathf.Clamp01(recoveryDropMultiplier)) // 조정된 몸통 아이템 확률
            {
                RecoveryPickup.Spawn( // 회복·생성 호출 시작
                    position + // 위치 처리
                    Vector3.left * // 벡터 처리
                    0.22f, // 대상 항목 전달
                    receiver, // 대상 항목 전달
                    health, // 체력 항목 전달
                    bodyManager, // 몸통·관리자 항목 전달
                    RecoveryPickupType.BodyRepair, // 회복·형식 항목 전달
                    1, // 대상 항목 전달
                    pickupSprite // 이미지 처리
                ); // 앞선 항목 종료
            }
        }
    }
}
