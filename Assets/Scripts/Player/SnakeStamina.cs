using System; // C# 기본 기능 연결
using ProjectEpsilon.Core; // 대상 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{
    [DefaultExecutionOrder(-50)] // 대상 설정
    [RequireComponent(typeof(PlayerInputReader))] // 구성 요소·플레이어 설정
    public sealed class SnakeStamina : MonoBehaviour // 스태미나·행동 자료 구조
    {
        [SerializeField] private PlayerInputReader inputReader; // 필드·플레이어 설정
        [SerializeField] private float maximumStamina = 100f; // 필드·최대 설정
        [SerializeField] private float currentStamina = 100f; // 필드·현재 설정
        [SerializeField] private float boostMultiplier = 1.5f; // 필드·배율 설정
        [SerializeField] private float drainPerSecond = 25f; // 필드 설정
        [SerializeField] private float recoveryPerSecond = 20f; // 필드·회복 설정
        [SerializeField] private float recoveryDelay = 1f; // 필드·회복 설정

        [Header("External State")] // 상태 설정
        [SerializeField] private bool boostBlocked; // 필드 설정

        private float recoveryBlockedUntil; // 회복 구성 선언
        private bool exhaustedUntilRelease; // 대상 구성 선언
        private bool isBoosting; // 대상 구성 선언

        public event Action<int, int> StaminaChanged; // 이벤트·동작 구성 선언
        public event Action<bool> BoostStateChanged; // 이벤트·동작 구성 선언

        public float CurrentStamina => currentStamina; // 현재·스태미나 값 반환
        public float MaximumStamina => maximumStamina * ProjectEpsilon.Progression.MetaModifiers.StaminaMultiplier; // 영구 성장 최대 스태미나
        public float EffectiveRecoveryPerSecond => recoveryPerSecond * CharacterRunInitializer.StaminaRecoveryMultiplier * 0.5f; // 캐릭터 배율을 유지한 절반 회복률
        public bool IsBoosting => isBoosting; // 대상 값 반환
        public bool IsBoostBlocked => boostBlocked; // 대상 값 반환

        public float CurrentSpeedMultiplier => // 현재·속도 값 반환
            isBoosting // 대상 처리
                ? boostMultiplier // 배율 처리
                : 1f; // 대상 처리

        private void Awake() // 초기화 기능 선언
        {
            if (inputReader == null) // 입력 조건 확인
            {
                inputReader = // 입력 값 설정
                    GetComponent<PlayerInputReader>(); // 조회·구성 요소 기능 실행
            }

            NormalizeValues(); // 대상 기능 실행
        }

        private void Start() // 시작 기능 선언
        {
            if (Mathf.Approximately(currentStamina, maximumStamina)) // 기존 시작 최대치 확인
            {
                currentStamina = MaximumStamina; // 영구 성장 시작 최대치 적용
            }
            NotifyStaminaChanged(); // 알림·스태미나 기능 실행
            SetBoosting(false); // 설정 기능 실행
        }

        private void Update() // 갱신 기능 선언
        {
            if (GameManager.Instance != null && // 게임·관리자 조건 확인
                !GameManager.Instance.IsPlaying) // 게임·관리자 처리
            {
                SetBoosting(false); // 설정 기능 실행
                return; // 현재 기능 종료
            }

            if (boostBlocked) // 대상 조건 확인
            {
                SetBoosting(false); // 설정 기능 실행
                TickRecovery(); // 회복 기능 실행
                return; // 현재 기능 종료
            }

            bool boostHeld = // 대상 값 설정
                inputReader != null && // 입력 처리
                inputReader.BoostPressed; // 입력 처리

            if (!boostHeld) // 대상 조건 확인
            {
                exhaustedUntilRelease = false; // 대상 값 설정
            }

            bool canBoost = // 가능 여부 값 설정
                boostHeld && // 대상 처리
                !exhaustedUntilRelease && // 대상 처리
                currentStamina > 0.0001f; // 현재·스태미나 처리

            if (canBoost) // 가능 여부 조건 확인
            {
                TickBoost(); // 대상 기능 실행
                return; // 현재 기능 종료
            }

            SetBoosting(false); // 설정 기능 실행
            TickRecovery(); // 회복 기능 실행
        }

        public void Configure( // 구성 호출 시작
            PlayerInputReader reader, // 플레이어·입력 항목 전달
            float maximum, // 최대 항목 전달
            float multiplier, // 배율 항목 전달
            float drainRate, // 비율 항목 전달
            float recoveryRate, // 회복·비율 항목 전달
            float delay // 대상 처리
        ) // 앞선 항목 종료
        {
            inputReader = reader; // 입력 값 설정
            maximumStamina = Mathf.Max(1f, maximum); // 최대·스태미나 값 설정
            boostMultiplier = Mathf.Max(1f, multiplier); // 배율·최대 값 설정
            drainPerSecond = Mathf.Max(0.01f, drainRate); // 최대·비율 값 설정
            recoveryPerSecond = Mathf.Max(0f, recoveryRate); // 회복·최대 값 설정
            recoveryDelay = Mathf.Max(0f, delay); // 회복·최대 값 설정

            ResetStamina(); // 초기화·스태미나 기능 실행
        }

        public void ResetStamina() // 초기화·스태미나 기능 선언
        {
            NormalizeValues(); // 대상 기능 실행

            currentStamina = MaximumStamina; // 실제 영구 성장 최대치 복구
            recoveryBlockedUntil = 0f; // 회복 값 설정
            exhaustedUntilRelease = false; // 대상 값 설정
            boostBlocked = false; // 대상 값 설정

            SetBoosting(false); // 설정 기능 실행
            NotifyStaminaChanged(); // 알림·스태미나 기능 실행
        }

        public void SetBoostBlocked(bool blocked) // 설정 기능 선언
        {
            boostBlocked = blocked; // 대상 값 설정

            if (boostBlocked) // 대상 조건 확인
            {
                SetBoosting(false); // 설정 기능 실행
            }
        }

        private void TickBoost() // 대상 기능 선언
        {
            SetBoosting(true); // 설정 기능 실행

            recoveryBlockedUntil = // 회복 값 설정
                Time.time + // 시간 처리
                recoveryDelay; // 회복 처리

            float previous = // 이전 값 값 설정
                currentStamina; // 현재·스태미나 처리

            currentStamina = // 현재·스태미나 값 설정
                Mathf.Max( // 최대 호출 시작
                    0f, // 대상 항목 전달
                    currentStamina - // 현재·스태미나 처리
                    (drainPerSecond * // 대상 기능 실행
                    ProjectEpsilon.Progression.MetaModifiers.DrainMultiplier(currentStamina, MaximumStamina) * // 낮은 스태미나 소모 감소
                    Time.deltaTime) // 시간·변화량 처리
                ); // 앞선 항목 종료

            if (!Mathf.Approximately( // 대상 조건 확인
                previous, // 이전 값 항목 전달
                currentStamina // 현재·스태미나 처리
            )) // 대상 처리
            {
                NotifyStaminaChanged(); // 알림·스태미나 기능 실행
            }

            if (currentStamina <= 0.0001f) // 현재·스태미나 조건 확인
            {
                currentStamina = 0f; // 현재·스태미나 값 설정
                exhaustedUntilRelease = true; // 대상 값 설정

                SetBoosting(false); // 설정 기능 실행
                NotifyStaminaChanged(); // 알림·스태미나 기능 실행
            }
        }

        private void TickRecovery() // 회복 기능 선언
        {
            if (Time.time < // 시간 조건 확인
                recoveryBlockedUntil) // 회복 처리
            {
                return; // 현재 기능 종료
            }

            if (currentStamina >= MaximumStamina || // 실제 영구 성장 최대치 확인
                recoveryPerSecond <= 0f) // 회복 처리
            {
                return; // 현재 기능 종료
            }

            float previous = // 이전 값 값 설정
                currentStamina; // 현재·스태미나 처리

            currentStamina = // 현재·스태미나 값 설정
                Mathf.Min( // 최소 호출 시작
                    MaximumStamina, // 실제 영구 성장 최대치
                    currentStamina + // 현재·스태미나 처리
                    (EffectiveRecoveryPerSecond * // 캐릭터별 실제 회복률
                    Time.deltaTime) // 시간·변화량 처리
                ); // 앞선 항목 종료

            if (!Mathf.Approximately( // 대상 조건 확인
                previous, // 이전 값 항목 전달
                currentStamina // 현재·스태미나 처리
            )) // 대상 처리
            {
                NotifyStaminaChanged(); // 알림·스태미나 기능 실행
            }
        }

        private void NormalizeValues() // 대상 기능 선언
        {
            maximumStamina = // 최대·스태미나 값 설정
                Mathf.Max( // 최대 호출 시작
                    1f, // 대상 항목 전달
                    maximumStamina // 최대·스태미나 처리
                ); // 앞선 항목 종료

            currentStamina = // 현재·스태미나 값 설정
                Mathf.Clamp( // 대상 호출 시작
                    currentStamina, // 현재·스태미나 항목 전달
                    0f, // 대상 항목 전달
                    MaximumStamina // 실제 영구 성장 최대치
                ); // 앞선 항목 종료

            boostMultiplier = // 배율 값 설정
                Mathf.Max( // 최대 호출 시작
                    1f, // 대상 항목 전달
                    boostMultiplier // 배율 처리
                ); // 앞선 항목 종료

            drainPerSecond = // 대상 값 설정
                Mathf.Max( // 최대 호출 시작
                    0.01f, // 대상 항목 전달
                    drainPerSecond // 대상 처리
                ); // 앞선 항목 종료

            recoveryPerSecond = // 회복 값 설정
                Mathf.Max( // 최대 호출 시작
                    0f, // 대상 항목 전달
                    recoveryPerSecond // 회복 처리
                ); // 앞선 항목 종료

            recoveryDelay = // 회복 값 설정
                Mathf.Max( // 최대 호출 시작
                    0f, // 대상 항목 전달
                    recoveryDelay // 회복 처리
                ); // 앞선 항목 종료
        }

        private void SetBoosting(bool value) // 설정·값 기능 선언
        {
            if (isBoosting == value) // 값 조건 확인
            {
                return; // 현재 기능 종료
            }

            isBoosting = value; // 값 값 설정

            BoostStateChanged?.Invoke( // 상태 호출 시작
                isBoosting // 대상 처리
            ); // 앞선 항목 종료
        }

        private void NotifyStaminaChanged() // 알림·스태미나 기능 선언
        {
            StaminaChanged?.Invoke( // 스태미나 호출 시작
                Mathf.RoundToInt( // 대상 호출 시작
                    currentStamina // 현재·스태미나 처리
                ), // 대상 항목 전달
                Mathf.RoundToInt( // 대상 호출 시작
                    MaximumStamina // 실제 HUD 최대치
                ) // 앞선 항목 종료
            ); // 앞선 항목 종료
        }
    }
}
