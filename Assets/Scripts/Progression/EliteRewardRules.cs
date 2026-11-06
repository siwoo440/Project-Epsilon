using System; // 빈 결과 형식

namespace ProjectEpsilon.Progression // 진행 영역
{ // 영역 시작
    public static class EliteRewardRules // 보상 추첨 규칙
    { // 클래스 시작
        public static int[] Select(bool[] eligible, int randomIndex) // 세 후보 추첨
        { // 메서드 시작
            if (eligible == null || eligible.Length < 4) // 후보 목록 확인
            { // 조건 시작
                return Array.Empty<int>(); // 잘못된 목록 제외
            } // 조건 끝
            int[] validExclusions = new int[4]; // 유효한 제외 후보 목록
            int count = 0; // 유효 조합 개수
            for (int excluded = 0; excluded < 4; excluded++) // 제외 후보 순회
            { // 반복 시작
                bool hasEligible = false; // 적용 가능 후보 여부
                for (int candidate = 0; candidate < 4; candidate++) // 나머지 후보 순회
                { // 반복 시작
                    hasEligible |= candidate != excluded && eligible[candidate]; // 적용 가능 후보 확인
                } // 반복 끝
                if (hasEligible) // 유효 조합 확인
                { // 조건 시작
                    validExclusions[count++] = excluded; // 제외 후보 저장
                } // 조건 끝
            } // 반복 끝
            if (count == 0) // 적용 가능 조합 부재
            { // 조건 시작
                return Array.Empty<int>(); // 보상 선택 연기
            } // 조건 끝
            uint safeIndex = unchecked((uint)randomIndex); // 음수 없는 추첨값
            int excludedIndex = validExclusions[safeIndex % (uint)count]; // 제외 후보 추첨
            int rotation = (int)((safeIndex / (uint)count) % 3); // 표시 순서 회전
            int[] result = new int[3]; // 선택 후보 목록
            int resultIndex = 0; // 저장 순서
            for (int candidate = 0; candidate < 4; candidate++) // 전체 후보 순회
            { // 반복 시작
                if (candidate == excludedIndex) // 제외 대상 확인
                { // 조건 시작
                    continue; // 다음 후보 이동
                } // 조건 끝
                result[(resultIndex++ + rotation) % 3] = candidate; // 후보 순서 저장
            } // 반복 끝
            return result; // 추첨 결과 반환
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
