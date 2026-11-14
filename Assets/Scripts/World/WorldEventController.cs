using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Player; // 실제 뱀과 충돌 처리
using ProjectEpsilon.Recording; // 실제 피해 원인
using ProjectEpsilon.UI; // 기존 위험 예고 화면
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.World // 맵 영역
{ // 영역 시작
    public sealed class WorldEventController : MonoBehaviour // 청크 맵의 공통 위험 이벤트
    { // 클래스 시작
        private PrototypeChunkMap map; // 실제 청크 관리자
        private SnakeMovement player; // 실제 플레이어 머리
        private SnakeBodyManager body; // 실제 플레이어 몸통
        private SnakeHealth health; // 실제 공용 체력
        private SnakeExternalCollision resolver; // 기존 충돌 우선순위 처리기
        private HUDController hud; // 기존 전투 화면
        private GameObject visual; // 실제 경고와 이동 이미지 묶음
        private LineRenderer outline; // 실제 위험 경계
        private Material outlineMaterial; // 실제 경계 재질
        private Transform swarm; // 실제 황충떼 이미지 묶음
        private Transform fallingRock; // 실제 낙석 이미지
        private Vector3 origin; // 황충떼 출발 위치
        private Vector3 direction; // 황충떼 이동 방향
        private Vector3 impact; // 실제 낙석 착탄 지점
        private float elapsed; // 실제 전투 경과 시간
        private float nextEvent; // 다음 위험 시작 시각
        private float phaseTime; // 현재 단계 경과 시간
        private bool nextRockfall; // 다음 위험 종류
        private bool damageQueued; // 같은 위험의 중복 피해 방지
        private int phase; // 대기 0, 황충 예고 1, 이동 2, 낙석 예고 3, 하강 4
        public bool IsWarning => phase == 1 || phase == 3; // 실제 위험 예고 상태
        public bool IsActive => phase == 2 || phase == 4; // 실제 위험 진행 상태
        public float WarningProgress => IsWarning && map != null && map.Profile != null ? Mathf.Clamp01(phaseTime / map.Profile.WorldEventWarning) : 0f; // 실제 예고 게이지 비율

        private void Awake() // 기존 맵과 플레이어 연결
        { // 함수 시작
            map = GetComponent<PrototypeChunkMap>(); // 실제 청크 관리자
            player = FindFirstObjectByType<SnakeMovement>(); // 실제 머리
            body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통
            health = player == null ? null : player.GetComponent<SnakeHealth>(); // 실제 공용 체력
            resolver = player == null ? null : player.GetComponent<SnakeExternalCollision>(); // 실제 충돌 처리기
            hud = FindFirstObjectByType<HUDController>(); // 실제 전투 화면
        } // 함수 종료

        private void Update() // 실제 전투 중 위험 진행
        { // 함수 시작
            if (map == null || map.Profile == null || player == null || body == null || health == null || resolver == null || GameManager.Instance == null) // 실제 구성 확인
            { // 조건 시작
                return; // 초기화 전 위험 생략
            } // 조건 종료
            if (GameManager.Instance.IsFinalBossPhase) // 기존 보스전 확인
            { // 조건 시작
                if (phase != 0) // 진행 중인 일반 맵 위험 확인
                { // 조건 시작
                    ClearEvent(); // 보스 경고와 출구 보호
                } // 조건 종료
                return; // 보스전 신규 맵 위험 생략
            } // 조건 종료
            if (!GameManager.Instance.IsPlaying) // 실제 전투 상태 확인
            { // 조건 시작
                return; // 일시정지 중 진행 중단
            } // 조건 종료
            elapsed += Time.deltaTime; // 실제 전투 시간 누적
            if (nextEvent <= 0f) // 첫 일정 초기화 확인
            { // 조건 시작
                nextEvent = map.Profile.WorldEventFirstTime; // 맵별 첫 위험 시각
            } // 조건 종료
            if (phase == 0 && elapsed >= nextEvent) // 실제 다음 위험 시각 확인
            { // 조건 시작
                bool began = nextRockfall ? TryBeginRockfall() : TryBeginLocust(); // 두 위험의 교대 시작
                nextEvent = elapsed + map.Profile.WorldEventInterval; // 다음 위험 간격
                if (began) // 실제 위험 시작 확인
                { // 조건 시작
                    nextRockfall = !nextRockfall; // 다음 위험 종류 전환
                } // 조건 종료
            } // 조건 종료
            if (phase == 0) // 대기 상태 확인
            { // 조건 시작
                return; // 진행할 위험 없음
            } // 조건 종료
            phaseTime += Time.deltaTime; // 실제 단계 시간 누적
            if (IsWarning) // 실제 위험 예고 확인
            { // 조건 시작
                hud?.SetWorldWarning(phase == 1 ? "황충떼 주의" : RockfallWarningText(), WarningProgress); // 위험 방향과 충전 게이지
                if (phaseTime >= map.Profile.WorldEventWarning) // 예고 완료 확인
                { // 조건 시작
                    StartActivePhase(); // 실제 이동 또는 하강 시작
                } // 조건 종료
                return; // 예고 중 피해 방지
            } // 조건 종료
            if (phase == 2) // 실제 황충떼 이동 확인
            { // 조건 시작
                float distance = Mathf.Min(30f, phaseTime * map.Profile.LocustSpeed); // 실제 직선 이동 거리
                swarm.position = origin + direction * distance; // 실제 황충 무리 이동
                if (distance >= 30f) // 이동 종료 확인
                { // 조건 시작
                    ClearEvent(); // 종료된 황충떼 정리
                } // 조건 종료
            } // 조건 종료
            else if (phase == 4) // 실제 낙석 하강 확인
            { // 조건 시작
                float progress = Mathf.Clamp01(phaseTime / map.Profile.RockfallDuration); // 실제 하강 비율
                fallingRock.position = impact + Vector3.up * (1f - progress) * 4f; // 실제 낙석 하강 위치
            } // 조건 종료
        } // 함수 종료

        private string RockfallWarningText() // 실제 화면 기준 낙석 방향 안내
        { // 함수 시작
            Camera camera = Camera.main; // 실제 전투 화면 카메라
            if (camera == null) // 실제 카메라 부재 확인
            { // 조건 시작
                return "낙석 주의"; // 방향을 알 수 없는 기본 경고
            } // 조건 종료
            Vector3 viewport = camera.WorldToViewportPoint(impact); // 실제 착탄 지점의 화면 좌표
            if (viewport.z <= 0f) // 카메라 뒤쪽 위치 확인
            { // 조건 시작
                return "낙석 주의"; // 방향을 알 수 없는 기본 경고
            } // 조건 종료
            string horizontal = viewport.x < 0f ? "왼쪽" : viewport.x > 1f ? "오른쪽" : ""; // 화면 밖 가로 방향
            string vertical = viewport.y < 0f ? "아래" : viewport.y > 1f ? "위" : ""; // 화면 밖 세로 방향
            if (horizontal.Length == 0 && vertical.Length == 0) // 실제 화면 안 위치 확인
            { // 조건 시작
                return "낙석 주의"; // 화면 안의 기본 경고
            } // 조건 종료
            string directionText = horizontal.Length == 0 ? vertical : vertical.Length == 0 ? horizontal : horizontal + " " + vertical; // 실제 가로와 세로 방향 조합
            return "낙석 주의 · " + directionText; // 화면 밖 착탄 방향 표시
        } // 함수 종료

        private void FixedUpdate() // 기존 물리 스텝의 위험 피해
        { // 함수 시작
            if (map == null || map.Profile == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || GameManager.Instance.IsFinalBossPhase) // 실제 전투 상태 확인
            { // 조건 시작
                return; // 일시정지와 보스 전환 중 피해 차단
            } // 조건 종료
            if (phase == 2 && swarm != null && !damageQueued) // 실제 이동 황충떼 접촉 확인
            { // 조건 시작
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // 실제 이동 방향 각도
                Collider2D[] contacts = Physics2D.OverlapBoxAll(swarm.position, new Vector2(1.5f, map.Profile.LocustWidth), angle); // 실제 이동 폭의 충돌 대상
                QueueDamage(contacts, map.Profile.LocustDamage); // 기존 충돌 우선순위에 피해 예약
            } // 조건 종료
            if (phase == 4 && phaseTime >= map.Profile.RockfallDuration) // 실제 낙석 착탄 확인
            { // 조건 시작
                QueueDamage(Physics2D.OverlapCircleAll(impact, 2f), map.Profile.RockfallDamage); // 착탄 범위 공용 체력 피해
                map.TryCreateRockfallDebris(impact); // 중앙 잔해의 현재 안전 확인
                map.TryCreateRockfallDebris(impact + Vector3.left * 1.4f); // 왼쪽 잔해의 현재 안전 확인
                map.TryCreateRockfallDebris(impact + Vector3.right * 1.4f); // 오른쪽 잔해의 현재 안전 확인
                ClearEvent(); // 종료된 낙석 정리
            } // 조건 종료
        } // 함수 종료

        public bool TryBeginLocust() // 황충떼 예고 시작
        { // 함수 시작
            if (!CanBegin()) // 실제 전투와 중복 확인
            { // 조건 시작
                return false; // 새 위험 시작 차단
            } // 조건 종료
            direction = Random.value < 0.5f ? Vector3.right : Vector3.up; // 실제 가로 또는 세로 이동 방향
            if (Random.value < 0.5f) // 반대 방향 추첨
            { // 조건 시작
                direction = -direction; // 실제 진행 방향 반전
            } // 조건 종료
            origin = player.transform.position - direction * 15f; // 실제 직선 이동 시작점
            CreateOutline(false, player.transform.position, direction, map.Profile.LocustWidth); // 실제 직선 위험 경계
            phase = 1; // 황충떼 예고 시작
            phaseTime = 0f; // 실제 예고 시간 초기화
            damageQueued = false; // 실제 피해 상태 초기화
            return true; // 실제 예고 시작 완료
        } // 함수 종료

        public bool TryBeginRockfall() // 낙석 예고 시작
        { // 함수 시작
            if (!CanBegin() || !TryFindRockfallSite(out impact)) // 실제 전투와 안전한 착탄 지점 확인
            { // 조건 시작
                return false; // 안전한 위치가 없으면 낙석 생략
            } // 조건 종료
            CreateOutline(true, impact, Vector3.up, 4f); // 실제 착탄 위험 경계
            phase = 3; // 낙석 예고 시작
            phaseTime = 0f; // 실제 예고 시간 초기화
            damageQueued = false; // 실제 피해 상태 초기화
            return true; // 실제 예고 시작 완료
        } // 함수 종료

        private bool CanBegin() // 실제 위험 시작 조건
        { // 함수 시작
            return phase == 0 && map != null && map.Profile != null && player != null && body != null && health != null && resolver != null && GameManager.Instance != null && GameManager.Instance.IsPlaying && !GameManager.Instance.IsFinalBossPhase; // 실제 전투와 필수 구성 확인
        } // 함수 종료

        private bool TryFindRockfallSite(out Vector3 result) // 안전 통로 밖의 착탄 지점 검색
        { // 함수 시작
            result = Vector3.zero; // 기본 후보 초기화
            float best = float.PositiveInfinity; // 가장 가까운 안전 후보 거리
            Camera camera = Camera.main; // 실제 전투 화면 카메라
            if (camera == null) // 실제 전투 카메라 확인
            { // 조건 시작
                return false; // 화면 밖 예고 방지
            } // 조건 종료
            Vector2Int center = new Vector2Int(Mathf.FloorToInt(player.transform.position.x / map.ChunkSize), Mathf.FloorToInt(player.transform.position.y / map.ChunkSize)); // 실제 플레이어 청크
            for (int x = -1; x <= 1; x++) // 가로 청크 후보 순회
            { // 반복 시작
                for (int y = -1; y <= 1; y++) // 세로 청크 후보 순회
                { // 반복 시작
                    GameObject chunk = map.GetChunk(center + new Vector2Int(x, y)); // 실제 활성 청크
                    if (chunk == null) // 활성 청크 확인
                    { // 조건 시작
                        continue; // 없는 청크 생략
                    } // 조건 종료
                    for (int quadrant = 0; quadrant < 4; quadrant++) // 연결 통로 밖 사분면 순회
                    { // 반복 시작
                        float localX = map.ChunkSize * ((quadrant & 1) == 0 ? 0.25f : 0.75f); // 사분면 가로 중심
                        float localY = map.ChunkSize * ((quadrant & 2) == 0 ? 0.25f : 0.75f); // 사분면 세로 중심
                        for (int offsetX = -1; offsetX <= 1; offsetX++) // 실제 사분면 가로 후보 순회
                        { // 반복 시작
                            for (int offsetY = -1; offsetY <= 1; offsetY++) // 실제 사분면 세로 후보 순회
                            { // 반복 시작
                                Vector3 candidate = chunk.transform.position + new Vector3(localX + offsetX * 1.4f, localY + offsetY * 2f, 0f); // 실제 착탄 후보
                                Vector3 viewport = camera.WorldToViewportPoint(candidate); // 실제 전투 화면 좌표
                                float distance = Vector2.Distance(candidate, player.transform.position); // 실제 플레이어와 거리
                                if (viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f && distance < best && map.CanPlaceRockfallDebris(candidate) && map.CanPlaceRockfallDebris(candidate + Vector3.left * 1.4f) && map.CanPlaceRockfallDebris(candidate + Vector3.right * 1.4f)) // 화면과 세 잔해 후보 확인
                                { // 조건 시작
                                    best = distance; // 실제 가장 가까운 안전 거리
                                    result = candidate; // 실제 안전 착탄 후보 저장
                                } // 조건 종료
                            } // 반복 종료
                        } // 반복 종료
                    } // 반복 종료
                } // 반복 종료
            } // 반복 종료
            return !float.IsPositiveInfinity(best); // 안전 착탄 후보 존재 여부
        } // 함수 종료

        private void CreateOutline(bool circular, Vector3 center, Vector3 travel, float width) // 위험 영역 경고 표시
        { // 함수 시작
            visual = new GameObject("WorldHazardVisual"); // 실제 위험 표시 묶음
            visual.transform.SetParent(transform, false); // 실제 맵 수명에 연결
            outline = visual.AddComponent<LineRenderer>(); // 실제 위험 경계선
            outlineMaterial = new Material(Shader.Find("Sprites/Default")); // 기존 방식의 표시 재질
            outline.material = outlineMaterial; // 실제 경계 재질 연결
            outline.startWidth = outline.endWidth = 0.12f; // 실제 경계 두께
            outline.startColor = outline.endColor = Color.yellow; // 실제 예고 색상
            outline.sortingOrder = 40; // 적과 포탑 위 경고 표시
            outline.loop = true; // 경계 끝 연결
            outline.positionCount = circular ? 32 : 4; // 원형 또는 직선 구역 좌표 수
            Vector3 side = new Vector3(-travel.y, travel.x, 0f); // 실제 이동 방향의 수직축
            for (int index = 0; index < outline.positionCount; index++) // 실제 경계 좌표 순회
            { // 반복 시작
                float angle = index * Mathf.PI * 2f / outline.positionCount; // 실제 원형 각도
                Vector3 position = circular ? center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * (width * 0.5f) : center + travel * (index == 0 || index == 3 ? -15f : 15f) + side * (index < 2 ? -width * 0.5f : width * 0.5f); // 실제 예고 경계 위치
                outline.SetPosition(index, position); // 실제 경계 좌표 적용
            } // 반복 종료
        } // 함수 종료

        private void StartActivePhase() // 예고 후 실제 위험 표시 시작
        { // 함수 시작
            int warning = phase; // 실제 완료한 위험 종류
            phase = warning == 1 ? 2 : 4; // 실제 이동 또는 하강 단계
            phaseTime = 0f; // 실제 활성 시간 초기화
            hud?.SetWorldWarning(null, 0f); // 완료된 예고 게이지 숨김
            outline.startColor = outline.endColor = Color.red; // 실제 활성 위험 경계 색상
            if (phase == 2) // 실제 황충떼 이동 확인
            { // 조건 시작
                Sprite sprite = Resources.Load<Sprite>("WorldEvents/LocustSwarm_Casual"); // 생성한 황충 이미지
                swarm = new GameObject("LocustSwarm").transform; // 실제 황충 무리 묶음
                swarm.SetParent(visual.transform, false); // 실제 위험 수명 연결
                swarm.position = origin; // 실제 황충 이동 출발점
                Vector3 side = new Vector3(-direction.y, direction.x, 0f); // 실제 이동 방향의 수직축
                for (int index = 0; index < 10; index++) // 실제 열 마리 황충 생성
                { // 반복 시작
                    GameObject insect = new GameObject("Locust_" + index); // 실제 황충 표시
                    insect.transform.SetParent(swarm, false); // 실제 무리 이동 연결
                    insect.transform.localPosition = side * ((index % 5 - 2) * map.Profile.LocustWidth / 5f + (index / 5 == 0 ? 0f : map.Profile.LocustWidth / 10f)) - direction * (index / 5 * 1.2f); // 두 줄로 엇갈린 무리 배치
                    insect.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction); // 실제 이동 방향으로 황충 회전
                    insect.transform.localScale = Vector3.one * 0.8f; // 실제 황충 이미지 크기
                    SpriteRenderer renderer = insect.AddComponent<SpriteRenderer>(); // 실제 황충 표시기
                    renderer.sprite = sprite; // 생성한 황충 이미지 연결
                    renderer.sortingOrder = 25; // 전투 요소 위와 경고 아래 표시
                } // 반복 종료
            } // 조건 종료
            else // 실제 낙석 하강 확인
            { // 대체 시작
                string folder = map.Profile.ObstaclePattern == 1 ? "Ruins" : map.Profile.ObstaclePattern == 2 ? "Swamp" : "Plains"; // 실제 맵 이미지 폴더
                string name = map.Profile.ObstaclePattern == 1 ? "무너진_벽체" : map.Profile.ObstaclePattern == 2 ? "암반_섬" : "바위_군락"; // 실제 맵별 잔해 이미지
                fallingRock = new GameObject("FallingRock").transform; // 실제 낙석 표시
                fallingRock.SetParent(visual.transform, false); // 실제 위험 수명 연결
                fallingRock.position = impact + Vector3.up * 4f; // 실제 하강 시작 위치
                SpriteRenderer renderer = fallingRock.gameObject.AddComponent<SpriteRenderer>(); // 실제 낙석 표시기
                renderer.sprite = Resources.Load<Sprite>("MapElements/" + folder + "/" + name); // 기존 맵 잔해 이미지 재사용
                renderer.sortingOrder = 25; // 전투 요소 위와 경고 아래 표시
            } // 대체 종료
        } // 함수 종료

        private void QueueDamage(Collider2D[] contacts, int damage) // 실제 머리 또는 몸통 피해 예약
        { // 함수 시작
            if (damageQueued || damage <= 0) // 중복 또는 무효 피해 확인
            { // 조건 시작
                return; // 실제 피해 예약 생략
            } // 조건 종료
            foreach (Collider2D contact in contacts) // 실제 접촉 대상 순회
            { // 반복 시작
                SnakeSegment segment = contact.GetComponent<SnakeSegment>(); // 실제 뱀 몸통 조회
                bool isHead = contact.transform == player.transform; // 실제 머리 접촉 확인
                bool isBody = segment != null && segment.transform.IsChildOf(body.transform); // 실제 뱀 몸통 접촉 확인
                if (isHead || isBody) // 실제 뱀 접촉 확인
                { // 조건 시작
                    resolver.QueueHealthDamage(health, damage, DamageOrigin.SpecialZone); // 기존 무적과 충돌 우선순위 사용
                    damageQueued = true; // 같은 위험의 중복 피해 방지
                    return; // 추가 접촉 처리 종료
                } // 조건 종료
            } // 반복 종료
        } // 함수 종료

        private void ClearEvent() // 완료된 위험 표시 정리
        { // 함수 시작
            hud?.SetWorldWarning(null, 0f); // 실제 예고판 숨김
            if (visual != null) // 실제 위험 표시 확인
            { // 조건 시작
                visual.SetActive(false); // 지연 파괴 전 표시 중단
                Destroy(visual); // 실제 위험 표시 제거
            } // 조건 종료
            if (outlineMaterial != null) // 실제 표시 재질 확인
            { // 조건 시작
                Destroy(outlineMaterial); // 실제 생성 재질 제거
            } // 조건 종료
            visual = null; // 이전 표시 참조 제거
            swarm = null; // 이전 황충 무리 참조 제거
            fallingRock = null; // 이전 낙석 참조 제거
            phase = 0; // 실제 위험 대기 상태
            phaseTime = 0f; // 실제 단계 시간 초기화
        } // 함수 종료

        private void OnDisable() // 맵 비활성화 처리
        { // 함수 시작
            ClearEvent(); // 남은 위험과 예고 정리
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
