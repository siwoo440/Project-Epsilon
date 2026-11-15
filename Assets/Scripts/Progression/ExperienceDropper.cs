using ProjectEpsilon.Combat; // 전투 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{ // 이름 공간 시작
    [RequireComponent(typeof(WeaponTarget))] // 구성 요소·포탑 설정
    public sealed class ExperienceDropper : MonoBehaviour // 경험치·행동 자료 구조
    { // 형식 시작
        [SerializeField] private WeaponTarget target; // 필드·포탑 설정
        [Min(1)] [SerializeField] private int experienceValue = 1; // 최소·필드 설정
        [SerializeField] private Sprite pickupSprite; // 필드·이미지 설정

        private bool subscribed; // 대상 구성 선언

        private void Awake() // 초기화 기능 선언
        { // 함수 시작
            EnsureTarget(); // 처치 대상 연결 확인
        } // 함수 종료

        private void OnEnable() // 활성 기능 선언
        { // 함수 시작
            Subscribe(); // 구독 기능 실행
        } // 함수 종료

        private void OnDisable() // 비활성 기능 선언
        { // 함수 시작
            Unsubscribe(); // 구독 해제 기능 실행
        } // 함수 종료

        public void Configure( // 구성 호출 시작
            int value, // 값 항목 전달
            Sprite visual // 이미지·시각 효과 처리
        ) // 앞선 항목 종료
        { // 함수 시작
            experienceValue = Mathf.Max(1, value); // 경험치·값 값 설정
            pickupSprite = visual; // 이미지·시각 효과 값 설정

            EnsureTarget(); // 처치 대상 연결 확인
            Subscribe(); // 구독 기능 실행
        } // 함수 종료

        private void EnsureTarget() // 처치 대상 연결
        { // 함수 시작
            if (target == null) // 대상 조건 확인
            { // 조건 시작
                target = GetComponent<WeaponTarget>(); // 대상·조회 값 설정
            } // 조건 종료
        } // 함수 종료

        private void Subscribe() // 구독 기능 선언
        { // 함수 시작
            EnsureTarget(); // 처치 대상 연결 확인

            if (subscribed || target == null) // 대상 조건 확인
            { // 조건 시작
                return; // 현재 기능 종료
            } // 조건 종료

            target.Died += HandleTargetDied; // 대상 값 설정
            subscribed = true; // 대상 값 설정
        } // 함수 종료

        private void Unsubscribe() // 구독 해제 기능 선언
        { // 함수 시작
            if (!subscribed || target == null) // 대상 조건 확인
            { // 조건 시작
                subscribed = false; // 대상 값 설정
                return; // 현재 기능 종료
            } // 조건 종료

            target.Died -= HandleTargetDied; // 대상 값 설정
            subscribed = false; // 대상 값 설정
        } // 함수 종료

        private void HandleTargetDied(WeaponTarget deadTarget) // 대상·포탑 기능 선언
        { // 함수 시작
            SnakeExperience experience = SnakeExperience.Current; // 경험치·현재 값 설정

            if (experience == null) // 경험치 조건 확인
            { // 조건 시작
                experience = // 경험치 값 설정
                    Object.FindFirstObjectByType<SnakeExperience>(); // 객체·검색 기능 실행
            } // 조건 종료

            ExperiencePickup.Spawn( // 경험치·생성 호출 시작
                deadTarget.transform.position, // 대상·위치 항목 전달
                experienceValue, // 경험치·값 항목 전달
                pickupSprite, // 이미지 항목 전달
                experience // 경험치 처리
            ); // 앞선 항목 종료
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
