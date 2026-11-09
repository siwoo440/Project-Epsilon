using System.Collections.Generic; // 활성 지역 목록
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Player; // 실제 머리와 공용 체력
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.World // 맵 영역
{ // 영역 시작
    public enum PrototypeZoneKind // 시험용 특수 지역 종류
    { // 열거형 시작
        Slow, // 이동 감속
        Experience, // 경험치 증가
        Danger, // 지속 위험
        Healing // 공용 체력 회복
    } // 열거형 끝
    public sealed class PrototypeSpecialZone : MonoBehaviour // 실제 시험용 특수 지역
    { // 클래스 시작
        private static readonly List<PrototypeSpecialZone> Active = new List<PrototypeSpecialZone>(); // 현재 활성 지역
        private PrototypeMapProfile profile; // 지역 임시 수치 데이터
        private PrototypeZoneKind kind; // 실제 지역 종류
        private SnakeMovement player; // 실제 플레이어 머리
        private SnakeHealth health; // 실제 공용 체력
        private SnakeBodyManager body; // 실제 몸통 관리자
        private Material material; // 시험용 원형 표시 재질
        private float nextTick; // 다음 효과 시각
        public float Radius => profile == null ? 0f : profile.ZoneRadius; // 현재 실제 반경
        public PrototypeZoneKind Kind => kind; // 현재 실제 종류
        public static PrototypeSpecialZone Create(Transform parent, Vector3 position, PrototypeZoneKind zoneKind, PrototypeMapProfile data) // 실제 특수 지역 생성
        { // 메서드 시작
            if (data == null) // 실제 맵 데이터 확인
            { // 조건 시작
                return null; // 무효 지역 생성 차단
            } // 조건 끝
            GameObject zone = new GameObject("PrototypeZone_" + zoneKind); // 실제 지역 객체
            zone.transform.SetParent(parent, false); // 청크와 수명 연결
            zone.transform.position = position; // 실제 지역 위치
            PrototypeSpecialZone result = zone.AddComponent<PrototypeSpecialZone>(); // 실제 지역 구성
            result.Configure(zoneKind, data); // 실제 지역 효과와 표시 설정
            return result; // 실제 지역 반환
        } // 메서드 끝
        private void Configure(PrototypeZoneKind zoneKind, PrototypeMapProfile data) // 실제 지역 설정
        { // 메서드 시작
            kind = zoneKind; // 실제 지역 종류 저장
            profile = data; // 임시 수치 연결
            player = FindFirstObjectByType<SnakeMovement>(); // 실제 머리 조회
            health = player == null ? null : player.GetComponent<SnakeHealth>(); // 실제 공용 체력 조회
            body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 조회
            nextTick = Time.time + profile.ZoneTickSeconds; // 첫 효과 시각
            LineRenderer line = gameObject.AddComponent<LineRenderer>(); // 실제 원형 표시
            material = new Material(Shader.Find("Sprites/Default")); // 지역 표시 재질
            line.material = material; // 실제 재질 연결
            line.useWorldSpace = false; // 청크 이동 기준 좌표
            line.loop = true; // 원형 끝 연결
            line.positionCount = 32; // 원형 표시 점 수
            line.startWidth = 0.12f; // 원형 시작 두께
            line.endWidth = 0.12f; // 원형 끝 두께
            line.sortingOrder = 3; // 바닥 위 표시
            Color color = kind == PrototypeZoneKind.Slow ? Color.cyan : kind == PrototypeZoneKind.Experience ? Color.yellow : kind == PrototypeZoneKind.Danger ? Color.red : Color.green; // 지역별 임시 색상
            line.startColor = color; // 시작 색상
            line.endColor = color; // 끝 색상
            for (int index = 0; index < line.positionCount; index++) // 원형 점 순회
            { // 반복 시작
                float angle = index * Mathf.PI * 2f / line.positionCount; // 실제 원형 각도
                line.SetPosition(index, new Vector3(Mathf.Cos(angle) * Radius, Mathf.Sin(angle) * Radius, 0f)); // 실제 원형 좌표
            } // 반복 끝
        } // 메서드 끝
        private void OnEnable() // 활성 지역 등록
        { // 메서드 시작
            if (!Active.Contains(this)) // 중복 등록 확인
            { // 조건 시작
                Active.Add(this); // 실제 지역 등록
            } // 조건 끝
        } // 메서드 끝
        private void OnDisable() // 활성 지역 해제
        { // 메서드 시작
            Active.Remove(this); // 효과 대상에서 제외
        } // 메서드 끝
        private void OnDestroy() // 지역 재질 정리
        { // 메서드 시작
            if (material != null) // 생성 재질 확인
            { // 조건 시작
                Destroy(material); // 지역 전용 재질 제거
            } // 조건 끝
        } // 메서드 끝
        private bool Contains(Vector3 position) // 실제 위치의 지역 포함 여부
        { // 메서드 시작
            return profile != null && isActiveAndEnabled && ((Vector2)(position - transform.position)).sqrMagnitude <= Radius * Radius; // 활성 원형 범위 확인
        } // 메서드 끝
        public static float SpeedMultiplierAt(Vector3 position) // 현재 위치의 이동 배율
        { // 메서드 시작
            float multiplier = 1f; // 기본 이동 배율
            foreach (PrototypeSpecialZone zone in Active) // 실제 지역 순회
            { // 반복 시작
                if (zone != null && zone.kind == PrototypeZoneKind.Slow && zone.Contains(position)) // 활성 감속 지역 확인
                { // 조건 시작
                    multiplier = Mathf.Min(multiplier, zone.profile.ZoneSlowMultiplier); // 가장 강한 감속 적용
                } // 조건 끝
            } // 반복 끝
            return multiplier; // 실제 이동 배율 반환
        } // 메서드 끝
        public static float ExperienceMultiplierAt(Vector3 position) // 현재 위치의 경험치 배율
        { // 메서드 시작
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 실제 전투 상태 확인
            { // 조건 시작
                return 1f; // 기존 획득 배율 유지
            } // 조건 끝
            float multiplier = 1f; // 기본 경험치 배율
            foreach (PrototypeSpecialZone zone in Active) // 실제 지역 순회
            { // 반복 시작
                if (zone != null && zone.kind == PrototypeZoneKind.Experience && zone.Contains(position)) // 활성 경험치 지역 확인
                { // 조건 시작
                    multiplier = Mathf.Max(multiplier, zone.profile.ZoneExperienceMultiplier); // 가장 높은 배율 적용
                } // 조건 끝
            } // 반복 끝
            return multiplier; // 실제 경험치 배율 반환
        } // 메서드 끝
        private void FixedUpdate() // 실제 지역의 물리 주기 효과
        { // 메서드 시작
            if (profile == null || player == null || health == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < nextTick) // 전투와 주기 확인
            { // 조건 시작
                return; // 정지 또는 대기 중 효과 차단
            } // 조건 끝
            nextTick = Time.time + profile.ZoneTickSeconds; // 다음 실제 효과 시각
            if (kind == PrototypeZoneKind.Healing && Contains(player.transform.position)) // 머리의 회복 지역 진입 확인
            { // 조건 시작
                health.Heal(profile.ZoneHealing); // 실제 공용 체력 회복
            } // 조건 끝
            else if (kind == PrototypeZoneKind.Danger && TouchesPlayer()) // 머리 또는 몸통의 위험 지역 접촉 확인
            { // 조건 시작
                SnakeExternalCollision resolver = player.GetComponent<SnakeExternalCollision>(); // 기존 충돌 우선순위 처리기
                if (resolver != null && resolver.isActiveAndEnabled) // 실제 처리기 확인
                { // 조건 시작
                    resolver.QueueHealthDamage(health, profile.ZoneDamage); // 같은 물리 단계 피해 예약
                } // 조건 끝
            } // 조건 끝
        } // 메서드 끝
        private bool TouchesPlayer() // 머리와 몸통의 위험 접촉 확인
        { // 메서드 시작
            if (Contains(player.transform.position)) // 실제 머리 확인
            { // 조건 시작
                return true; // 실제 머리 접촉
            } // 조건 끝
            if (body != null) // 실제 몸통 관리자 확인
            { // 조건 시작
                foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 순회
                { // 반복 시작
                    if (segment != null && Contains(segment.transform.position)) // 활성 몸통 위치 확인
                    { // 조건 시작
                        return true; // 실제 몸통 접촉
                    } // 조건 끝
                } // 반복 끝
            } // 조건 끝
            return false; // 지역 접촉 없음
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
