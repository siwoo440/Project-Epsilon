using ProjectEpsilon.Data; // 무기 속성 사용

namespace ProjectEpsilon.Combat // 전투 영역
{ // 네임스페이스 시작
    public static class WeaponAttributeDamageRules // 속성 피해 규칙
    { // 클래스 시작
        public static float CalculateDirectDamage(float gradeDamage, WeaponAttribute attribute, int synergyStage) // 직접 피해 계산
        { // 메서드 시작
            float safeDamage = gradeDamage < 0f ? 0f : gradeDamage; // 음수 피해 제거

            if (attribute != WeaponAttribute.Physical) // 물리 속성 여부 확인
            { // 조건 시작
                return safeDamage; // 원래 피해 반환
            } // 조건 끝

            return safeDamage * GetPhysicalMultiplier(synergyStage); // 물리 증폭 피해 반환
        } // 메서드 끝

        public static float GetPhysicalMultiplier(int synergyStage) // 물리 배율 조회
        { // 메서드 시작
            int stage = NormalizeStage(synergyStage); // 단계 정규화

            switch (stage) // 단계 분기
            { // 분기 시작
                case 8: // 8단계 선택
                    return 1.5f; // 50퍼센트 증가

                case 6: // 6단계 선택
                    return 1.35f; // 35퍼센트 증가

                case 4: // 4단계 선택
                    return 1.2f; // 20퍼센트 증가

                case 2: // 2단계 선택
                    return 1.1f; // 10퍼센트 증가

                default: // 비활성 단계 선택
                    return 1f; // 기본 배율
            } // 분기 끝
        } // 메서드 끝

        public static float GetFireDuration(int synergyStage) // 화상 지속 시간 조회
        { // 메서드 시작
            int stage = NormalizeStage(synergyStage); // 단계 정규화

            switch (stage) // 단계 분기
            { // 분기 시작
                case 8: // 8단계 선택
                    return 5f; // 5초 반환

                case 6: // 6단계 선택
                    return 4f; // 4초 반환

                case 4: // 4단계 선택
                    return 3f; // 3초 반환

                case 2: // 2단계 선택
                    return 2f; // 2초 반환

                default: // 비활성 단계 선택
                    return 0f; // 지속 없음
            } // 분기 끝
        } // 메서드 끝

        public static float CalculateFireDamagePerSecond(float directDamage, int synergyStage) // 화상 초당 피해 계산
        { // 메서드 시작
            float safeDamage = directDamage < 0f ? 0f : directDamage; // 음수 피해 제거
            int stage = NormalizeStage(synergyStage); // 단계 정규화

            switch (stage) // 단계 분기
            { // 분기 시작
                case 8: // 8단계 선택
                    return safeDamage * 0.25f; // 25퍼센트 반환

                case 6: // 6단계 선택
                    return safeDamage * 0.2f; // 20퍼센트 반환

                case 4: // 4단계 선택
                    return safeDamage * 0.15f; // 15퍼센트 반환

                case 2: // 2단계 선택
                    return safeDamage * 0.1f; // 10퍼센트 반환

                default: // 비활성 단계 선택
                    return 0f; // 피해 없음
            } // 분기 끝
        } // 메서드 끝

        public static bool ShouldReplaceBurn(int currentStage, int incomingStage) // 화상 교체 여부 계산
        { // 메서드 시작
            int safeCurrent = NormalizeStage(currentStage); // 현재 단계 정규화
            int safeIncoming = NormalizeStage(incomingStage); // 신규 단계 정규화
            return safeIncoming > 0 && safeIncoming >= safeCurrent; // 동일 이상 단계 허용
        } // 메서드 끝

        private static int NormalizeStage(int synergyStage) // 단계 정규화
        { // 메서드 시작
            if (synergyStage >= 8) // 8단계 경계 확인
            { // 조건 시작
                return 8; // 8단계 반환
            } // 조건 끝

            if (synergyStage >= 6) // 6단계 경계 확인
            { // 조건 시작
                return 6; // 6단계 반환
            } // 조건 끝

            if (synergyStage >= 4) // 4단계 경계 확인
            { // 조건 시작
                return 4; // 4단계 반환
            } // 조건 끝

            if (synergyStage >= 2) // 2단계 경계 확인
            { // 조건 시작
                return 2; // 2단계 반환
            } // 조건 끝

            return 0; // 비활성 반환
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
