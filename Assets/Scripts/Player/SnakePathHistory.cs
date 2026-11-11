using System.Collections.Generic; // C# 기본 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{
    public sealed class SnakePathHistory // 경로 자료 구조
    {
        private readonly List<Vector3> points = new List<Vector3>(); // 목록·벡터 기능 선언
        private readonly float minimumPointDistance; // 최소·지점 구성 선언
        private readonly float maximumPathLength; // 최대·경로 구성 선언

        private Vector3 lastCommittedPosition; // 벡터·위치 구성 선언

        public int PointCount => points.Count; // 지점·개수 값 반환

        public SnakePathHistory(float minimumPointDistance, float maximumPathLength) // 경로·최소 기능 선언
        {
            this.minimumPointDistance = Mathf.Max(0.001f, minimumPointDistance); // 최소·지점 값 설정
            this.maximumPathLength = Mathf.Max(this.minimumPointDistance, maximumPathLength); // 최대·경로 값 설정
        }

        public void Reset(Vector3 headPosition, Vector3 forward, float initialLength) // 초기화·벡터 기능 선언
        {
            points.Clear(); // 대상 기능 실행

            Vector3 flatForward = new Vector3(forward.x, forward.y, 0f); // 벡터 값 설정

            if (flatForward.sqrMagnitude < 0.0001f) // 대상 조건 확인
            {
                flatForward = Vector3.up; // 벡터 값 설정
            }

            flatForward.Normalize(); // 대상 기능 실행
            lastCommittedPosition = headPosition; // 위치 값 설정
            points.Add(headPosition); // 추가·위치 기능 실행

            float requiredLength = Mathf.Max(initialLength, maximumPathLength); // 길이·최대 값 설정

            for ( // 대상 반복 처리
                float distance = minimumPointDistance; // 거리·최소 값 설정
                distance <= requiredLength + minimumPointDistance; // 거리·길이 처리
                distance += minimumPointDistance // 거리·최소 값 설정
            ) // 앞선 항목 종료
            {
                points.Add(headPosition - flatForward * distance); // 추가·위치 기능 실행
            }
        }

        public void Record(Vector3 headPosition) // 벡터·위치 기능 선언
        {
            if (points.Count == 0) // 개수 조건 확인
            {
                Reset(headPosition, Vector3.up, maximumPathLength); // 초기화·위치 기능 실행
                return; // 현재 기능 종료
            }

            points[0] = headPosition; // 위치 값 설정

            if (Vector3.Distance(lastCommittedPosition, headPosition) < minimumPointDistance) // 벡터·거리 조건 확인
            {
                return; // 현재 기능 종료
            }

            points.Insert(1, lastCommittedPosition); // 위치 기능 실행
            lastCommittedPosition = headPosition; // 위치 값 설정
            TrimToMaximumLength(); // 최대·길이 기능 실행
        }

        public Vector3 SampleDistanceBehind(float distance) // 벡터·거리 기능 선언
        {
            if (points.Count == 0) // 개수 조건 확인
            {
                return Vector3.zero; // 벡터 결과 반환
            }

            if (distance <= 0f) // 거리 조건 확인
            {
                return points[0]; // 대상 결과 반환
            }

            float walkedDistance = 0f; // 거리 값 설정

            for (int index = 0; index < points.Count - 1; index++) // 순서·개수 반복 처리
            {
                Vector3 from = points[index]; // 벡터·순서 값 설정
                Vector3 to = points[index + 1]; // 벡터·순서 값 설정
                float segmentLength = Vector3.Distance(from, to); // 몸통·길이 값 설정

                if (segmentLength <= 0.0001f) // 몸통·길이 조건 확인
                {
                    continue; // 다음 반복 진행
                }

                if (walkedDistance + segmentLength >= distance) // 거리·몸통 조건 확인
                {
                    float remainingDistance = distance - walkedDistance; // 남은 값·거리 값 설정
                    float interpolation = remainingDistance / segmentLength; // 남은 값·거리 값 설정
                    return Vector3.Lerp(from, to, interpolation); // 벡터 결과 반환
                }

                walkedDistance += segmentLength; // 거리·몸통 값 설정
            }

            return points[points.Count - 1]; // 개수 결과 반환
        }

        private void TrimToMaximumLength() // 최대·길이 기능 선언
        {
            if (points.Count < 2) // 개수 조건 확인
            {
                return; // 현재 기능 종료
            }

            float walkedDistance = 0f; // 거리 값 설정

            for (int index = 0; index < points.Count - 1; index++) // 순서·개수 반복 처리
            {
                Vector3 from = points[index]; // 벡터·순서 값 설정
                Vector3 to = points[index + 1]; // 벡터·순서 값 설정
                float segmentLength = Vector3.Distance(from, to); // 몸통·길이 값 설정

                if (segmentLength <= 0.0001f) // 몸통·길이 조건 확인
                {
                    continue; // 다음 반복 진행
                }

                if (walkedDistance + segmentLength < maximumPathLength) // 거리·몸통 조건 확인
                {
                    walkedDistance += segmentLength; // 거리·몸통 값 설정
                    continue; // 다음 반복 진행
                }

                float remainingDistance = maximumPathLength - walkedDistance; // 남은 값·거리 값 설정
                float interpolation = Mathf.Clamp01(remainingDistance / segmentLength); // 남은 값·거리 값 설정
                Vector3 endpoint = Vector3.Lerp(from, to, interpolation); // 벡터 값 설정

                points[index + 1] = endpoint; // 순서 값 설정

                int removeStart = index + 2; // 제거·시작 값 설정

                if (removeStart < points.Count) // 제거·시작 조건 확인
                {
                    points.RemoveRange(removeStart, points.Count - removeStart); // 제거·범위 기능 실행
                }

                return; // 현재 기능 종료
            }
        }
    }
}
