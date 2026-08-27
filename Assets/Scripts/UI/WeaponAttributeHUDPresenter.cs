using System.Text; // 문자열 조립 사용
using ProjectEpsilon.Combat; // 시너지 관리자 사용
using ProjectEpsilon.Data; // 무기 속성 사용
using UnityEngine; // Unity 기능 사용
using UnityEngine.UI; // UI Text 사용

namespace ProjectEpsilon.UI // UI 영역
{ // 네임스페이스 시작
    public sealed class WeaponAttributeHUDPresenter : MonoBehaviour // 속성 HUD 표시기
    { // 클래스 시작
        [SerializeField] private WeaponAttributeSynergyManager synergyManager; // 시너지 관리자 참조
        [SerializeField] private Text attributeText; // 표시 Text 참조

        private bool subscribed; // 구독 상태

        public bool IsConfigured // 연결 완료 상태
        { // 속성 시작
            get // 상태 조회
            { // 접근자 시작
                return synergyManager != null && attributeText != null; // 필수 연결 여부 반환
            } // 접근자 끝
        } // 속성 끝

        public WeaponAttributeSynergyManager SynergyManager // 연결된 시너지 관리자
        { // 속성 시작
            get // 참조 조회
            { // 접근자 시작
                return synergyManager; // 시너지 관리자 반환
            } // 접근자 끝
        } // 속성 끝

        public Text AttributeText // 연결된 표시 Text
        { // 속성 시작
            get // 참조 조회
            { // 접근자 시작
                return attributeText; // Text 반환
            } // 접근자 끝
        } // 속성 끝

        private void OnEnable() // 활성화 처리
        { // 메서드 시작
            Subscribe(); // 이벤트 구독
            Refresh(); // 즉시 표시 갱신
        } // 메서드 끝

        private void Start() // 시작 처리
        { // 메서드 시작
            Subscribe(); // 누락 구독 보완
            Refresh(); // 시작 표시 갱신
        } // 메서드 끝

        private void OnDisable() // 비활성화 처리
        { // 메서드 시작
            Unsubscribe(); // 이벤트 구독 해제
        } // 메서드 끝

        public void Configure(WeaponAttributeSynergyManager manager, Text text) // 표시 연결
        { // 메서드 시작
            Unsubscribe(); // 기존 연결 해제
            synergyManager = manager; // 관리자 저장
            attributeText = text; // Text 저장
            Subscribe(); // 새 연결 구독
            Refresh(); // 표시 즉시 갱신
        } // 메서드 끝

        public void Refresh() // HUD 갱신
        { // 메서드 시작
            if (attributeText == null) // Text 존재 확인
            { // 조건 시작
                return; // 표시 생략
            } // 조건 끝

            StringBuilder builder = new StringBuilder(); // 문자열 작성기 생성
            builder.AppendLine("ATTRIBUTES"); // 제목 추가

            for (int index = 0; index < WeaponAttributeSynergyRules.AttributeCount; index++) // 모든 속성 순회
            { // 반복 시작
                WeaponAttribute attribute = (WeaponAttribute)index; // 현재 속성 변환
                int count = synergyManager == null ? 0 : synergyManager.GetCount(attribute); // 현재 개수 조회
                int stage = WeaponAttributeSynergyRules.ResolveStage(count); // 현재 단계 계산
                string stageLabel = stage > 0 ? " ×" + stage : string.Empty; // 단계 문구 생성
                builder.Append(attribute); // 속성명 추가
                builder.Append(" "); // 구분 공백 추가
                builder.Append(count); // 개수 추가
                builder.AppendLine(stageLabel); // 단계 추가
            } // 반복 끝

            attributeText.text = builder.ToString().TrimEnd(); // 완성 문자열 표시
        } // 메서드 끝

        private void Subscribe() // 이벤트 구독
        { // 메서드 시작
            if (subscribed || synergyManager == null) // 구독 가능 여부 확인
            { // 조건 시작
                return; // 구독 생략
            } // 조건 끝

            synergyManager.SynergyChanged += Refresh; // 변경 알림 연결
            subscribed = true; // 구독 상태 저장
        } // 메서드 끝

        private void Unsubscribe() // 이벤트 구독 해제
        { // 메서드 시작
            if (!subscribed || synergyManager == null) // 해제 가능 여부 확인
            { // 조건 시작
                subscribed = false; // 상태 초기화
                return; // 해제 생략
            } // 조건 끝

            synergyManager.SynergyChanged -= Refresh; // 변경 알림 해제
            subscribed = false; // 구독 상태 초기화
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
