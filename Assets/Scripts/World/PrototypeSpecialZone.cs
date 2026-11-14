using System.Collections.Generic; // 활성 지역 목록
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 기존 일반 적 출현기
using ProjectEpsilon.Player; // 실제 머리와 공용 체력
using ProjectEpsilon.Progression; // 기존 경험치 보석
using UnityEngine; // Unity 기능

using ProjectEpsilon.Recording; // 실제 피해 원인 기록

namespace ProjectEpsilon.World // 맵 영역
{
    public enum PrototypeZoneKind // 시험용 특수 지역 종류
    {
        Slow, // 이동 감속
        Experience, // 경험치 증가
        Danger, // 지속 위험
        Healing, // 기존 공용 체력 회복
        Wind, // 평원 바람길
        Pollen, // 평원 꽃가루 들판
        Flower, // 평원 회복 꽃밭
        Mud, // 습지 진흙 웅덩이
        Bridge, // 습지 수초 다리
        Purify, // 습지 정화 꽃
        ElectricFloor, // 폐허 방전 바닥
        CircuitSwitch, // 폐허 회로 스위치
        ToxicSpore, // 습지 독성 포자
        GemTrail, // 평원 분산 보석 꽃길
        Supply, // 폐허 보급 창고
        AntHill, // 평원 개미굴
        Collapse // 폐허 붕괴 잔해
    }
    public sealed class PrototypeSpecialZone : MonoBehaviour // 실제 시험용 특수 지역
    {
        private static readonly List<PrototypeSpecialZone> Active = new List<PrototypeSpecialZone>(); // 현재 활성 지역
        private PrototypeMapProfile profile; // 지역 임시 수치 데이터
        private PrototypeZoneKind kind; // 실제 지역 종류
        private SnakeMovement player; // 실제 플레이어 머리
        private SnakeHealth health; // 실제 공용 체력
        private SnakeBodyManager body; // 실제 몸통 관리자
        private Material material; // 시험용 원형 표시 재질
        private float nextTick; // 다음 효과 시각
        private PrototypeChunkMap map; // 같은 런의 청크 관리자
        private bool wasInside; // 머리의 이전 지역 진입 상태
        private bool localClaimed; // 청크 관리자 없는 일회성 대체 상태
        private float localReadyAt; // 청크 관리자 없는 재사용 대기
        private float localSuppressedUntil; // 독립 장치의 비활성 종료 시각
        private float warningUntil; // 위험 예고 종료 시각
        private float activeUntil; // 위험 활성 종료 시각
        private float nextDamageAt; // 다음 위험 피해 시각
        private int hazardPhase; // 대기 0, 예고 1, 활성 2
        private int antsRemaining; // 순차 소환 대기 수
        private float nextAntAt; // 다음 개미 소환 시각
        private GameObject collapseObstacle; // 일시적으로 나타난 기존 장애물
        private LineRenderer outline; // 위험 상태 표시선
        public float Radius => profile == null ? 0f : kind == PrototypeZoneKind.CircuitSwitch ? Mathf.Min(1f, profile.ZoneRadius) : profile.ZoneRadius; // 현재 실제 반경
        public PrototypeZoneKind Kind => kind; // 현재 실제 종류
        public bool IsWarning => hazardPhase == 1; // 위험 예고 상태
        public bool IsHazardActive => hazardPhase == 2; // 위험 활성 상태
        public bool IsSuppressed => Time.time < Mathf.Max(localSuppressedUntil, map == null ? 0f : map.FeatureSuppressedUntil(transform.position, kind)); // 장치 비활성 상태
        public static PrototypeSpecialZone Create(Transform parent, Vector3 position, PrototypeZoneKind zoneKind, PrototypeMapProfile data) // 실제 특수 지역 생성
        {
            if (data == null) // 실제 맵 데이터 확인
            {
                return null; // 무효 지역 생성 차단
            }
            GameObject zone = new GameObject("PrototypeZone_" + zoneKind); // 실제 지역 객체
            zone.transform.SetParent(parent, false); // 청크와 수명 연결
            zone.transform.position = position; // 실제 지역 위치
            PrototypeSpecialZone result = zone.AddComponent<PrototypeSpecialZone>(); // 실제 지역 구성
            result.Configure(zoneKind, data); // 실제 지역 효과와 표시 설정
            return result; // 실제 지역 반환
        }
        private void Configure(PrototypeZoneKind zoneKind, PrototypeMapProfile data) // 실제 지역 설정
        {
            kind = zoneKind; // 실제 지역 종류 저장
            profile = data; // 임시 수치 연결
            player = FindFirstObjectByType<SnakeMovement>(); // 실제 머리 조회
            health = player == null ? null : player.GetComponent<SnakeHealth>(); // 실제 공용 체력 조회
            body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 조회
            map = GetComponentInParent<PrototypeChunkMap>(); // 같은 런의 지역 기록 관리자
            CreateArtwork(); // 맵별 실제 지역 이미지 연결
            nextTick = Time.time + profile.ZoneTickSeconds; // 첫 효과 시각
            LineRenderer line = gameObject.AddComponent<LineRenderer>(); // 실제 원형 표시
            outline = line; // 위험 상태 표시선 보관
            material = new Material(Shader.Find("Sprites/Default")); // 지역 표시 재질
            line.material = material; // 실제 재질 연결
            line.useWorldSpace = false; // 청크 이동 기준 좌표
            line.loop = true; // 원형 끝 연결
            line.positionCount = 32; // 원형 표시 점 수
            line.startWidth = 0.12f; // 원형 시작 두께
            line.endWidth = 0.12f; // 원형 끝 두께
            line.sortingOrder = 3; // 바닥 위 표시
            Color color = kind == PrototypeZoneKind.Mud ? new Color(0.55f, 0.35f, 0.15f) : kind == PrototypeZoneKind.Wind || kind == PrototypeZoneKind.Bridge || kind == PrototypeZoneKind.Slow ? Color.cyan : kind == PrototypeZoneKind.Pollen || kind == PrototypeZoneKind.Experience ? Color.yellow : kind == PrototypeZoneKind.Danger ? Color.red : Color.green; // 지역별 기능 구분 색상
            line.startColor = color; // 시작 색상
            line.endColor = color; // 끝 색상
            for (int index = 0; index < line.positionCount; index++) // 원형 점 순회
            {
                float angle = index * Mathf.PI * 2f / line.positionCount; // 실제 원형 각도
                line.SetPosition(index, new Vector3(Mathf.Cos(angle) * Radius, Mathf.Sin(angle) * Radius, 0f)); // 실제 원형 좌표
            }
            if (map != null && (kind == PrototypeZoneKind.Flower || kind == PrototypeZoneKind.Purify || kind == PrototypeZoneKind.GemTrail || kind == PrototypeZoneKind.Supply) && map.IsFeatureClaimed(transform.position, kind)) // 같은 런의 사용된 보상 확인
            {
                gameObject.SetActive(false); // 청크 재생성 후에도 사용한 보상 숨김
            }
        }
        private void OnEnable() // 활성 지역 등록
        {
            if (!Active.Contains(this)) // 중복 등록 확인
            {
                Active.Add(this); // 실제 지역 등록
            }
        }
        private void OnDisable() // 활성 지역 해제
        {
            Active.Remove(this); // 효과 대상에서 제외
            wasInside = false; // 재활성 시 머리 진입 상태 초기화
        }
        private void OnDestroy() // 지역 재질 정리
        {
            if (material != null) // 생성 재질 확인
            {
                Destroy(material); // 지역 전용 재질 제거
            }
        }
        private bool Contains(Vector3 position) // 실제 위치의 지역 포함 여부
        {
            return profile != null && isActiveAndEnabled && ((Vector2)(position - transform.position)).sqrMagnitude <= Radius * Radius; // 활성 원형 범위 확인
        }
        public static float SpeedMultiplierAt(Vector3 position) // 현재 위치의 이동 배율
        {
            float multiplier = 1f; // 기본 이동 배율
            foreach (PrototypeSpecialZone zone in Active) // 실제 지역 순회
            {
                if (zone != null && zone.Contains(position)) // 실제 위치의 활성 지역 확인
                {
                    if (zone.kind == PrototypeZoneKind.Slow) // 기존 감속 지역 확인
                    {
                        multiplier = Mathf.Min(multiplier, zone.profile.ZoneSlowMultiplier); // 기존 감속 배율 유지
                    }
                    else if (zone.kind == PrototypeZoneKind.Mud) // 습지 진흙 지역 확인
                    {
                        multiplier = Mathf.Min(multiplier, zone.profile.MudSpeedMultiplier); // 겹친 진흙의 단일 감속 적용
                    }
                }
            }
            return multiplier; // 실제 이동 배율 반환
        }
        public static float ExperienceMultiplierAt(Vector3 position) // 현재 위치의 경험치 배율
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 실제 전투 상태 확인
            {
                return 1f; // 기존 획득 배율 유지
            }
            float multiplier = 1f; // 기본 경험치 배율
            foreach (PrototypeSpecialZone zone in Active) // 실제 지역 순회
            {
                if (zone != null && zone.kind == PrototypeZoneKind.Experience && zone.Contains(position)) // 활성 경험치 지역 확인
                {
                    multiplier = Mathf.Max(multiplier, zone.profile.ZoneExperienceMultiplier); // 가장 높은 배율 적용
                }
            }
            return multiplier; // 실제 경험치 배율 반환
        }
        private void CreateArtwork() // 실제 맵별 지역 이미지 표시
        {
            string path = kind == PrototypeZoneKind.Wind ? "Plains/바람길" : kind == PrototypeZoneKind.Pollen ? "Plains/꽃가루_들판" : kind == PrototypeZoneKind.Flower ? "Plains/회복_꽃밭" : kind == PrototypeZoneKind.GemTrail ? "Plains/분산_보석_꽃길" : kind == PrototypeZoneKind.AntHill ? "Plains/개미굴" : kind == PrototypeZoneKind.Mud ? "Swamp/진흙_웅덩이" : kind == PrototypeZoneKind.Bridge ? "Swamp/수초_다리" : kind == PrototypeZoneKind.Purify ? "Swamp/정화_꽃" : kind == PrototypeZoneKind.ElectricFloor ? "Ruins/방전_바닥" : kind == PrototypeZoneKind.CircuitSwitch ? "Ruins/회로_스위치" : kind == PrototypeZoneKind.Supply ? "Ruins/보급_창고" : kind == PrototypeZoneKind.Collapse ? "Ruins/붕괴_잔해" : kind == PrototypeZoneKind.ToxicSpore ? "Swamp/독성_포자" : null; // 실제 지역별 이미지 경로
            if (string.IsNullOrEmpty(path)) // 기존 시험 지역 확인
            {
                return; // 기존 원형 표시 유지
            }
            Sprite artwork = Resources.Load<Sprite>("MapElements/" + path); // 실제 맵 지역 이미지
            if (artwork == null) // 이미지 부재 확인
            {
                return; // 기존 원형 표시 유지
            }
            GameObject visual = new GameObject("MapFeatureArtwork"); // 충돌 범위와 분리된 지역 이미지
            visual.transform.SetParent(transform, false); // 실제 지역 아래 연결
            visual.transform.localScale = Vector3.one * Radius; // 지역 반경에 맞춘 이미지 크기
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>(); // 실제 지역 표시기
            renderer.sprite = artwork; // 실제 지역 이미지 적용
            renderer.sortingOrder = 1; // 적과 위험 예고 아래 표시
        }

        private bool TryActivate(float cooldown, bool singleUse) // 재방문을 포함한 지역 발동 확인
        {
            if (map != null) // 실제 청크 관리자 확인
            {
                return map.TryActivateFeature(transform.position, kind, cooldown, singleUse); // 같은 런의 재사용 기록 적용
            }
            if (localClaimed || Time.time < localReadyAt) // 독립 지역의 기존 사용 확인
            {
                return false; // 중복 사용 차단
            }
            localClaimed = singleUse; // 독립 일회 보상 기록
            localReadyAt = Time.time + Mathf.Max(0f, cooldown); // 독립 지역 재사용 시각
            return true; // 실제 효과 발동 허용
        }

        private void ApplyMapFeature() // 머리 진입 시 실제 맵 효과
        {
            bool inside = Contains(player.transform.position); // 실제 머리의 지역 포함 여부
            if (!inside) // 지역 이탈 확인
            {
                wasInside = false; // 다음 진입 대기
                if (kind == PrototypeZoneKind.Mud) // 실제 진흙 이탈 확인
                {
                    player.RefreshTerrainCleanse(); // 겹친 지형까지 벗어난 정화 상태 갱신
                }
                return; // 이탈 중 효과 중단
            }
            if (wasInside || kind == PrototypeZoneKind.Mud) // 기존 진입과 지속 지형 확인
            {
                return; // 중복 발동과 지형 틱 방지
            }
            wasInside = true; // 같은 진입의 중복 발동 방지
            if (kind == PrototypeZoneKind.Wind && TryActivate(profile.WindCooldown, false)) // 바람길 재사용 확인
            {
                player.ApplyWind(profile.WindDuration, profile.WindSpeedMultiplier); // 일시적 이동속도 증가
            }
            else if (kind == PrototypeZoneKind.Pollen && TryActivate(0f, false)) // 꽃가루 진입 확인
            {
                player.ApplyPollen(profile.PollenDuration, profile.PollenPickupMultiplier); // 획득 범위만 일시 증가
            }
            else if (kind == PrototypeZoneKind.Bridge && TryActivate(0f, false)) // 수초 다리 진입 확인
            {
                player.IgnoreTerrainSlow(profile.BridgeImmunityDuration); // 지형 감속만 일시 무시
            }
            else if (kind == PrototypeZoneKind.CircuitSwitch && TryActivate(profile.CircuitCooldown, false)) // 스위치 재사용 확인
            {
                foreach (PrototypeSpecialZone zone in Active) // 실제 활성 장치 순회
                {
                    if (zone != null && zone.kind == PrototypeZoneKind.ElectricFloor && Vector2.Distance(transform.position, zone.transform.position) <= profile.CircuitRange) // 인접 방전 바닥 확인
                    {
                        zone.SuppressHazard(profile.CircuitDisableDuration); // 실제 인접 장치 일시 중지
                    }
                }
            }
            else if ((kind == PrototypeZoneKind.GemTrail || kind == PrototypeZoneKind.Supply) && TryActivate(0f, true)) // 런당 일회성 보상 확인
            {
                SnakeExperience receiver = player.GetComponent<SnakeExperience>(); // 기존 경험치 수신자
                for (int index = 0; index < profile.MapGemCount; index++) // 맵 보상 보석 순회
                {
                    float angle = index * Mathf.PI * 2f / profile.MapGemCount; // 보석 분산 각도
                    Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * (Radius * 0.5f); // 지역 내부 분산 위치
                    ExperiencePickup.Spawn(transform.position + offset, profile.MapGemValue, null, receiver); // 기존 경험치 보석 생성
                }
                if (kind == PrototypeZoneKind.Supply) // 보급 창고 확인
                {
                    health.Heal(profile.SupplyHealing); // 기존 공용 체력 회복
                }
                gameObject.SetActive(false); // 획득한 보상 위치 표시 제거
            }
            else if ((kind == PrototypeZoneKind.Flower || kind == PrototypeZoneKind.Purify) && TryActivate(0f, true)) // 일회성 꽃 사용 확인
            {
                health.Heal(profile.MapFlowerHealing); // 실제 공용 체력 회복
                if (kind == PrototypeZoneKind.Purify) // 정화 꽃 확인
                {
                    player.CleanseTerrainSlow(); // 현재 지형 감속만 해제
                }
                gameObject.SetActive(false); // 사용한 일회성 꽃 표시 제거
            }
        }

        private void SuppressHazard(float duration) // 스위치의 방전 장치 비활성
        {
            localSuppressedUntil = Mathf.Max(localSuppressedUntil, Time.time + duration); // 현재 장치의 종료 시각
            map?.SuppressFeature(transform.position, kind, duration); // 청크 재생성 이후까지 상태 보관
            hazardPhase = 0; // 진행 중인 위험 취소
            wasInside = player != null && Contains(player.transform.position); // 현재 진입의 중복 재발동 방지
            UpdateHazardOutline(); // 비활성 표시 갱신
        }

        private void UpdateHazardOutline() // 위험 예고와 활성 표시
        {
            if (outline == null) // 표시선 존재 확인
            {
                return; // 표시선 부재 시 종료
            }
            Color color = IsSuppressed ? Color.gray : hazardPhase == 1 ? Color.yellow : hazardPhase == 2 ? Color.red : Color.white; // 상태별 구분 색상
            outline.startColor = color; // 시작 색상 갱신
            outline.endColor = color; // 끝 색상 갱신
            outline.sortingOrder = hazardPhase == 0 ? 3 : 25; // 적과 탄환 위, 보스 예고 아래 표시
            outline.startWidth = hazardPhase == 0 ? 0.12f : 0.24f; // 위험 표시 시작 두께
            outline.endWidth = outline.startWidth; // 위험 표시 끝 두께
        }

        private void UpdateTimedHazard() // 실제 예고와 주기 피해 처리
        {
            bool inside = Contains(player.transform.position); // 실제 머리의 위험 구역 진입
            if (IsSuppressed || GameManager.Instance.IsFinalBossPhase) // 비활성과 보스전 안전 확인
            {
                hazardPhase = 0; // 진행 위험 취소
                wasInside = inside; // 종료 후 재진입 대기
                UpdateHazardOutline(); // 비활성 표시 갱신
                return; // 위험 효과 차단
            }
            if (!inside) // 머리 이탈 확인
            {
                wasInside = false; // 다음 진입 허용
            }
            if (inside && !wasInside && hazardPhase == 0) // 대기 장치의 새 진입 확인
            {
                wasInside = true; // 동일 진입의 중복 시작 방지
                warningUntil = Time.time + (kind == PrototypeZoneKind.ElectricFloor ? profile.ElectricWarningDuration : profile.SporeWarningDuration); // 실제 예고 종료 시각
                activeUntil = warningUntil + (kind == PrototypeZoneKind.ElectricFloor ? profile.ElectricActiveDuration : profile.SporeActiveDuration); // 실제 활성 종료 시각
                nextDamageAt = warningUntil + (kind == PrototypeZoneKind.ElectricFloor ? profile.ElectricDamageInterval : profile.SporeDamageInterval); // 첫 활성 피해 시각
                hazardPhase = 1; // 위험 예고 시작
            }
            if (hazardPhase == 1 && Time.time >= warningUntil) // 실제 예고 종료 확인
            {
                hazardPhase = 2; // 실제 위험 활성
            }
            if (hazardPhase == 2 && Time.time >= nextDamageAt && nextDamageAt <= activeUntil) // 활성 중 피해 주기 확인
            {
                if (TouchesPlayer() && !health.IsInvulnerable) // 실제 접촉과 기존 무적 확인
                {
                    SnakeExternalCollision resolver = player.GetComponent<SnakeExternalCollision>(); // 기존 충돌 우선순위 처리기
                    if (resolver != null && resolver.isActiveAndEnabled) // 기존 처리기 활성 확인
                    {
                        resolver.QueueHealthDamage(health, kind == PrototypeZoneKind.ElectricFloor ? profile.ElectricDamage : profile.SporeDamage, DamageOrigin.SpecialZone); // 기존 피해 판정에 예약
                    }
                }
                nextDamageAt += kind == PrototypeZoneKind.ElectricFloor ? profile.ElectricDamageInterval : profile.SporeDamageInterval; // 다음 실제 피해 시각
            }
            if (hazardPhase == 2 && Time.time > activeUntil) // 실제 활성 종료 확인
            {
                hazardPhase = 0; // 다음 진입까지 대기
            }
            UpdateHazardOutline(); // 실제 위험 상태 표시
        }

        private void UpdateAntHill() // 개미굴 예고와 순차 소환
        {
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 기존 일반 적 출현기
            bool inside = Contains(player.transform.position); // 실제 머리의 경고 원 진입
            if (spawner == null || spawner.ElapsedSeconds < profile.AntHillStartTime || GameManager.Instance.IsFinalBossPhase) // 개방 시간과 보스전 확인
            {
                hazardPhase = 0; // 진행 예고 취소
                wasInside = false; // 개방 이후 진입 허용
                UpdateHazardOutline(); // 대기 표시 갱신
                return; // 소환 중지
            }
            if (!inside) // 경고 원 이탈 확인
            {
                wasInside = false; // 다음 진입 허용
            }
            if (inside && !wasInside && hazardPhase == 0) // 새로운 경고 원 진입 확인
            {
                wasInside = true; // 같은 진입의 중복 발동 방지
                if (TryActivate(profile.AntHillCooldown, false)) // 같은 개미굴의 재사용 대기 확인
                {
                    antsRemaining = Random.Range(3, 6); // 기획 범위의 개미 수 추첨
                    warningUntil = Time.time + profile.AntHillWarning; // 실제 소환 예고 종료 시각
                    nextAntAt = warningUntil; // 첫 개미 소환 시각
                    hazardPhase = 1; // 개미굴 예고 시작
                }
            }
            if (hazardPhase == 1 && Time.time >= warningUntil) // 실제 개미굴 예고 종료 확인
            {
                hazardPhase = 2; // 순차 소환 시작
            }
            if (hazardPhase == 2 && Time.time >= nextAntAt) // 실제 개미 소환 시각 확인
            {
                Vector3 spawn = transform.position; // 실제 소환 후보 위치
                bool safe = false; // 안전한 출현 후보 확인
                for (int attempt = 0; attempt < 8 && !safe; attempt++) // 경고 원 밖의 여러 후보 검사
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f); // 실제 소환 방향 추첨
                    spawn = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * (Radius + 1.5f); // 경고 원 밖의 소환 후보
                    safe = Vector2.Distance(spawn, player.transform.position) >= 2f; // 머리와 즉시 겹침 방지
                    if (body != null) // 실제 몸통 관리자 확인
                    {
                        foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 순회
                        {
                            if (segment != null && Vector2.Distance(spawn, segment.transform.position) < 1.5f) // 몸통과 즉시 겹침 확인
                            {
                                safe = false; // 겹친 소환 후보 제외
                            }
                        }
                    }
                    foreach (Collider2D contact in Physics2D.OverlapCircleAll(spawn, 0.7f)) // 실제 출현 지점의 충돌체 순회
                    {
                        SnakeContactHazard obstacle = contact.GetComponentInParent<SnakeContactHazard>(); // 기존 충돌 위험 구성 조회
                        if (obstacle != null && obstacle.HazardType == SnakeContactHazardType.Obstacle) // 장애물 내부 출현 확인
                        {
                            safe = false; // 장애물과 겹친 소환 후보 제외
                        }
                    }
                }
                if (safe) // 안전한 개미 출현 위치 확인
                {
                    spawner.SpawnMapChaser(spawn); // 기존 적 상한과 일반 보상 적용
                }
                antsRemaining--; // 이번 소환 기회 소비
                nextAntAt = Time.time + profile.AntHillInterval; // 다음 순차 소환 시각
                if (antsRemaining <= 0) // 준비된 소환 종료 확인
                {
                    hazardPhase = 0; // 다음 진입까지 대기
                }
            }
            UpdateHazardOutline(); // 개미굴 경고 표시 갱신
        }

        private void UpdateCollapse() // 붕괴 예고와 안전한 일시 장애물
        {
            bool inside = Contains(player.transform.position); // 실제 머리의 지역 진입
            if (GameManager.Instance.IsFinalBossPhase) // 보스전 겹침 방지
            {
                if (collapseObstacle != null) // 기존 잔해 확인
                {
                    Destroy(collapseObstacle); // 보스전 진입 시 잔해 제거
                }
                hazardPhase = 0; // 진행 예고 취소
                wasInside = inside; // 보스전 이후 새 진입 대기
                UpdateHazardOutline(); // 잔해 표시 갱신
                return; // 보스전 중 추가 위험 중지
            }
            if (!inside) // 지역 이탈 확인
            {
                wasInside = false; // 다음 진입 허용
            }
            if (inside && !wasInside && hazardPhase == 0) // 새 지역 진입 확인
            {
                wasInside = true; // 같은 진입의 중복 발동 방지
                warningUntil = Time.time + profile.CollapseWarning; // 실제 붕괴 예고 종료 시각
                hazardPhase = 1; // 붕괴 예고 시작
            }
            if (hazardPhase == 1 && Time.time >= warningUntil) // 예고 종료 확인
            {
                hazardPhase = 0; // 안전 확인 전 대기 상태
                bool tailInside = body != null && body.TailSegment != null && body.TailSegment.isActiveAndEnabled && Contains(body.TailSegment.transform.position); // 실제 활성 꼬리의 구역 포함 여부
                if (!TouchesPlayer() && !tailInside) // 머리와 몸통 및 꼬리의 붕괴 구역 이탈 확인
                {
                    collapseObstacle = new GameObject("TemporaryCollapseObstacle"); // 실제 일시 장애물 객체
                    collapseObstacle.transform.SetParent(transform, false); // 지역과 수명 연결
                    BoxCollider2D collider = collapseObstacle.AddComponent<BoxCollider2D>(); // 기존 장애물 충돌 범위
                    collider.isTrigger = true; // 기존 장애물 피해 판정 사용
                    collider.size = new Vector2(1f, 0.7f); // 그림보다 작은 안전 충돌 범위
                    collider.offset = new Vector2(0f, -0.15f); // 실제 지면 충돌 위치
                    collapseObstacle.AddComponent<SnakeContactHazard>().Configure(SnakeContactHazardType.Obstacle); // 기존 충돌과 무적 처리 연결
                    activeUntil = Time.time + profile.CollapseDuration; // 일시 잔해 종료 시각
                    hazardPhase = 2; // 일시 잔해 활성
                }
            }
            if (hazardPhase == 2 && Time.time >= activeUntil) // 실제 잔해 지속 종료 확인
            {
                if (collapseObstacle != null) // 실제 잔해 존재 확인
                {
                    collapseObstacle.SetActive(false); // 지연 파괴 전 충돌 차단
                    Destroy(collapseObstacle); // 실제 일시 잔해 제거
                }
                hazardPhase = 0; // 다음 진입까지 대기
            }
            UpdateHazardOutline(); // 실제 붕괴 경고 표시
        }

        private void FixedUpdate() // 실제 지역의 물리 주기 효과
        {
            if (profile == null || player == null || health == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 실제 전투와 참조 확인
            {
                return; // 정지 또는 구성 부재 중 효과 차단
            }
            if (kind == PrototypeZoneKind.ElectricFloor || kind == PrototypeZoneKind.ToxicSpore) // 예고가 필요한 실제 위험 확인
            {
                UpdateTimedHazard(); // 예고와 주기 피해 처리
                return; // 일반 지역 처리 제외
            }
            if (kind == PrototypeZoneKind.AntHill) // 개미굴 지역 확인
            {
                UpdateAntHill(); // 기존 적 출현기에 순차 소환 연결
                return; // 일반 지역 처리 제외
            }
            if (kind == PrototypeZoneKind.Collapse) // 붕괴 잔해 지역 확인
            {
                UpdateCollapse(); // 예고와 안전한 일시 장애물 처리
                return; // 일반 지역 처리 제외
            }
            if (kind >= PrototypeZoneKind.Wind) // 실제 맵별 지역 확인
            {
                ApplyMapFeature(); // 머리 진입과 일회성 효과 처리
                return; // 기존 일반 시험 지역 처리 제외
            }
            if (Time.time < nextTick) // 기존 시험 지역 주기 확인
            {
                return; // 주기 전 효과 차단
            }
            nextTick = Time.time + profile.ZoneTickSeconds; // 다음 실제 효과 시각
            if (kind == PrototypeZoneKind.Healing && Contains(player.transform.position)) // 머리의 회복 지역 진입 확인
            {
                health.Heal(profile.ZoneHealing); // 실제 공용 체력 회복
            }
            else if (kind == PrototypeZoneKind.Danger && TouchesPlayer()) // 머리 또는 몸통의 위험 지역 접촉 확인
            {
                SnakeExternalCollision resolver = player.GetComponent<SnakeExternalCollision>(); // 기존 충돌 우선순위 처리기
                if (resolver != null && resolver.isActiveAndEnabled) // 실제 처리기 확인
                {
                    resolver.QueueHealthDamage(health, profile.ZoneDamage, DamageOrigin.SpecialZone); // 같은 물리 단계 피해 예약
                }
            }
        }
        private bool TouchesPlayer() // 머리와 몸통의 위험 접촉 확인
        {
            if (Contains(player.transform.position)) // 실제 머리 확인
            {
                return true; // 실제 머리 접촉
            }
            if (body != null) // 실제 몸통 관리자 확인
            {
                foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 순회
                {
                    if (segment != null && Contains(segment.transform.position)) // 활성 몸통 위치 확인
                    {
                        return true; // 실제 몸통 접촉
                    }
                }
            }
            return false; // 지역 접촉 없음
        }
    }
}
