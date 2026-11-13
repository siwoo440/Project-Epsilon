using System.Collections.Generic; // 활성 적 감속 지대
using ProjectEpsilon.Core; // 실제 게임 상태
using UnityEngine; // Unity 표시와 시간
namespace ProjectEpsilon.Enemies // 적 전투 영역
{ // 영역 시작
    public sealed class EnemySlowZone : MonoBehaviour // 노린재의 임시 감속 지대
    { // 지대 시작
        private static readonly List<EnemySlowZone> Active = new List<EnemySlowZone>(); // 현재 활성 적 지대
        private float radius; // 실제 감속 반경
        private float multiplier = 1f; // 실제 적 감속 배율
        private float expiresAt; // 실제 지대 종료 시각
        private Material material; // 원형 표시 전용 재질
        public static EnemySlowZone Create(Vector3 position, float zoneRadius, float speedMultiplier, float duration) // 실제 적 감속 지대 생성
        { // 함수 시작
            GameObject instance = new GameObject("EnemySlowZone"); // 실제 지대 객체
            instance.transform.position = position; // 실제 지대 위치
            EnemySlowZone zone = instance.AddComponent<EnemySlowZone>(); // 실제 적 지대 구성
            zone.Configure(zoneRadius, speedMultiplier, duration); // 실제 수치와 표시 연결
            return zone; // 실제 생성 지대 반환
        } // 함수 종료
        private void Configure(float zoneRadius, float speedMultiplier, float duration) // 조정 가능한 지대 설정
        { // 함수 시작
            radius = Mathf.Max(0.1f, zoneRadius); // 안전한 지대 반경
            multiplier = Mathf.Clamp(speedMultiplier, 0.1f, 1f); // 안전한 감속 배율
            expiresAt = Time.time + Mathf.Max(0f, duration); // 실제 지대 종료 시각
            LineRenderer line = gameObject.AddComponent<LineRenderer>(); // 실제 지대 경계선
            material = new Material(Shader.Find("Sprites/Default")); // 지대 표시 재질
            line.material = material; // 실제 재질 적용
            line.useWorldSpace = false; // 지대 중심 기준 좌표
            line.loop = true; // 원형 경계 연결
            line.positionCount = 32; // 원형 경계 점 수
            line.startWidth = 0.1f; // 시작 경계 두께
            line.endWidth = 0.1f; // 끝 경계 두께
            line.startColor = new Color(0.55f, 1f, 0.2f, 0.9f); // 적 감속 지대 시작 색상
            line.endColor = line.startColor; // 적 감속 지대 끝 색상
            line.sortingOrder = 12; // 적 위와 경고선 아래 표시
            for (int index = 0; index < line.positionCount; index++) // 실제 원형 경계 생성
            { // 반복 시작
                float angle = index * Mathf.PI * 2f / line.positionCount; // 현재 원형 각도
                line.SetPosition(index, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f)); // 실제 원형 경계 좌표
            } // 반복 종료
        } // 함수 종료
        private void OnEnable() // 실제 지대 활성화
        { // 함수 시작
            if (!Active.Contains(this)) // 중복 등록 확인
            { // 조건 시작
                Active.Add(this); // 실제 지대 등록
            } // 조건 종료
        } // 함수 종료
        private void OnDisable() // 실제 지대 비활성화
        { // 함수 시작
            Active.Remove(this); // 감속 대상에서 제거
        } // 함수 종료
        private void Update() // 지대 지속 시간 확인
        { // 함수 시작
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 일시정지와 종료 확인
            { // 조건 시작
                return; // 게임 시간 정지 유지
            } // 조건 종료
            if (Time.time >= expiresAt) // 실제 지속 시간 종료 확인
            { // 조건 시작
                Destroy(gameObject); // 종료된 지대 제거
            } // 조건 종료
        } // 함수 종료
        private void OnDestroy() // 표시 자원 정리
        { // 함수 시작
            if (material != null) // 실제 표시 재질 확인
            { // 조건 시작
                Destroy(material); // 지대 전용 재질 제거
            } // 조건 종료
        } // 함수 종료
        public static float SpeedMultiplierAt(Vector3 position) // 현재 머리 위치의 적 감속
        { // 함수 시작
            float result = 1f; // 기본 이동 배율
            foreach (EnemySlowZone zone in Active) // 실제 적 지대 순회
            { // 반복 시작
                if (zone != null && zone.isActiveAndEnabled && Time.time < zone.expiresAt && ((Vector2)(position - zone.transform.position)).sqrMagnitude <= zone.radius * zone.radius) // 유효한 적 지대 포함 확인
                { // 조건 시작
                    result = Mathf.Min(result, zone.multiplier); // 겹친 적 지대의 가장 강한 감속
                } // 조건 종료
            } // 반복 종료
            return result; // 실제 적 감속 배율 반환
        } // 함수 종료
        public static void ClearAll() // 보스 전환 시 적 지대 정리
        { // 함수 시작
            while (Active.Count > 0) // 남은 적 지대 확인
            { // 반복 시작
                EnemySlowZone zone = Active[Active.Count - 1]; // 마지막 활성 지대
                Active.RemoveAt(Active.Count - 1); // 활성 목록에서 제거
                if (zone != null) // 실제 지대 객체 확인
                { // 조건 시작
                    zone.gameObject.SetActive(false); // 지연 파괴 전 감속 해제
                    Destroy(zone.gameObject); // 실제 지대 제거
                } // 조건 종료
            } // 반복 종료
        } // 함수 종료
    } // 지대 종료
} // 영역 종료
