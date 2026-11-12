using UnityEngine; // 실제 UI 객체 조회
using UnityEngine.UI; // 실제 기존 HUD 글자

namespace ProjectEpsilon.UI // 실제 한국어 화면 영역
{
    public static class HudTextLocalizer // 기존 씬의 고정 문구와 글꼴 정리
    {
        public static void Apply(Transform root, Font font) // 실제 HUD와 키 설정 문구 적용
        {
            if (root == null) // 실제 기존 HUD 확인
            {
                return; // 누락된 HUD 처리 생략
            }
            foreach (Text label in root.GetComponentsInChildren<Text>(true)) // 실제 비활성 화면까지 순회
            {
                if (font != null) // 실제 한글 글꼴 확인
                {
                    label.font = font; // 실제 HUD 한글 글꼴 적용
                }
                switch (label.name) // 실제 고정 안내 이름 확인
                {
                    case "TurnLeftLabel": label.text = "왼쪽 회전"; break; // 실제 왼쪽 조작 안내
                    case "TurnRightLabel": label.text = "오른쪽 회전"; break; // 실제 오른쪽 조작 안내
                    case "BoostLabel": label.text = "가속"; break; // 실제 가속 조작 안내
                    case "MergeHintText": label.text = "N: 시험 합성 재료   E: 합성"; break; // 실제 시험과 합성 키 안내
                    case "PrototypeHintText": label.text = "P: 피해  [: 몸통 손실  H: 회복 보석  J: 복구 보석  3/5: 등급 시험"; break; // 실제 시험 키 안내
                }
                switch (label.text) // 실제 기존 씬의 고정 영문 확인
                {
                    case "Settings": label.text = "설정"; break; // 실제 설정 제목
                    case "Close": label.text = "닫기"; break; // 실제 설정 닫기
                    case "Reset": label.text = "초기화"; break; // 실제 키 초기화
                    case "CLOSE [M]": label.text = "닫기 [E]"; break; // 실제 합성 닫기 키
                    case "Weapon Candidate": label.text = "포탑 후보"; break; // 실제 포탑 후보 기본값
                    case "Merge Candidate": label.text = "합성 후보"; break; // 실제 합성 후보 기본값
                }
            }
        }
    }
}
