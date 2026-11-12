using System.Collections.Generic; // 활성 청크 저장소
using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Player; // 플레이어 위치
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.World // 맵 영역
{
    public sealed class PrototypeChunkMap : MonoBehaviour // 시험용 개방 평원 청크 관리
    {
        [SerializeField] private float chunkSize = 24f; // 청크 크기 임시값
        [SerializeField] private Sprite groundSprite; // 개방 평원 바닥 이미지
        private readonly Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>(); // 실제 활성 청크
        private readonly List<Vector2Int> removed = new List<Vector2Int>(); // 이탈 청크 임시 목록
        private readonly HashSet<Vector3Int> claimedFeatures = new HashSet<Vector3Int>(); // 런당 획득한 지역 보상
        private readonly Dictionary<Vector3Int, float> featureCooldowns = new Dictionary<Vector3Int, float>(); // 청크 재생성 뒤에도 유지할 대기 시간
        private readonly Dictionary<Vector3Int, float> featureSuppression = new Dictionary<Vector3Int, float>(); // 청크 재생성 뒤에도 유지할 비활성 시간
        private SnakeMovement player; // 실제 플레이어 위치
        private Vector2Int center; // 현재 중심 청크
        private bool initialized; // 최초 청크 생성 여부
        private PrototypeMapProfile profile; // 현재 런 맵 데이터
        private static Sprite obstacleVisual; // 장애물 공용 시험 이미지
        public int ActiveCount => chunks.Count; // 실제 활성 청크 수
        public float ChunkSize => Mathf.Max(1f, chunkSize); // 안전한 청크 크기
        public PrototypeMapProfile Profile => profile; // 실제 적용된 맵 데이터

        public bool ApplyProfile(PrototypeMapProfile data) // 시작 전 맵 설정
        {
            if (data == null || data.Ground == null || initialized) // 데이터와 청크 생성 상태 확인
            {
                return false; // 런 중 맵 교체 방지
            }
            profile = data; // 실제 맵 데이터 저장
            groundSprite = data.Ground; // 실제 맵 바닥 적용
            return true; // 실제 바닥 설정 완료
        }

        public bool TryActivateFeature(Vector3 position, PrototypeZoneKind kind, float cooldown, bool singleUse) // 지역별 재사용과 일회 보상 확인
        {
            Vector3Int key = new Vector3Int(Mathf.FloorToInt(position.x / ChunkSize), Mathf.FloorToInt(position.y / ChunkSize), (int)kind); // 청크 좌표와 지역 종류
            if (claimedFeatures.Contains(key)) // 이미 획득한 일회 보상 확인
            {
                return false; // 같은 런의 중복 획득 차단
            }
            if (featureCooldowns.TryGetValue(key, out float readyAt) && Time.time < readyAt) // 기존 재사용 대기 확인
            {
                return false; // 대기 중 재발동 차단
            }
            if (singleUse) // 일회성 지역 확인
            {
                claimedFeatures.Add(key); // 런 종료까지 획득 기록
            }
            featureCooldowns[key] = Time.time + Mathf.Max(0f, cooldown); // 안전한 다음 사용 시각 저장
            return true; // 지역 효과 발동 허용
        }

        public bool IsFeatureClaimed(Vector3 position, PrototypeZoneKind kind) // 이미 획득한 일회 보상 확인
        {
            Vector3Int key = new Vector3Int(Mathf.FloorToInt(position.x / ChunkSize), Mathf.FloorToInt(position.y / ChunkSize), (int)kind); // 실제 보상의 청크와 종류
            return claimedFeatures.Contains(key); // 같은 런의 획득 여부 반환
        }

        public void SuppressFeature(Vector3 position, PrototypeZoneKind kind, float duration) // 청크 재방문을 포함한 장치 비활성
        {
            Vector3Int key = new Vector3Int(Mathf.FloorToInt(position.x / ChunkSize), Mathf.FloorToInt(position.y / ChunkSize), (int)kind); // 실제 장치의 청크와 종류
            float until = Time.time + Mathf.Max(0f, duration); // 안전한 비활성 종료 시각
            if (featureSuppression.TryGetValue(key, out float previous)) // 기존 비활성 시각 확인
            {
                until = Mathf.Max(until, previous); // 더 긴 기존 비활성 보존
            }
            featureSuppression[key] = until; // 실제 비활성 종료 시각 저장
        }

        public float FeatureSuppressedUntil(Vector3 position, PrototypeZoneKind kind) // 장치의 비활성 종료 시각 조회
        {
            Vector3Int key = new Vector3Int(Mathf.FloorToInt(position.x / ChunkSize), Mathf.FloorToInt(position.y / ChunkSize), (int)kind); // 실제 장치의 청크와 종류
            return featureSuppression.TryGetValue(key, out float until) ? until : 0f; // 기존 비활성 또는 기본 시각
        }

        public GameObject GetChunk(Vector2Int coordinate) // 실제 청크 좌표 조회
        {
            return chunks.TryGetValue(coordinate, out GameObject chunk) ? chunk : null; // 좌표별 활성 청크 반환
        }
        private void Start() // 실제 플레이어 위치 연결
        {
            player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
        }
        private void Update() // 실제 플레이어 주변 청크 갱신
        {
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 실제 플레이 상태 확인
            {
                return; // 정지 중 청크 갱신 방지
            }
            Vector3 position = player.transform.position; // 실제 플레이어 위치
            Vector2Int next = new Vector2Int(Mathf.FloorToInt(position.x / ChunkSize), Mathf.FloorToInt(position.y / ChunkSize)); // 음수를 포함한 중심 좌표
            if (initialized && next == center) // 중심 청크 유지 확인
            {
                return; // 불필요한 재생성 방지
            }
            center = next; // 실제 중심 저장
            initialized = true; // 최초 생성 상태 저장
            removed.Clear(); // 이전 이탈 목록 비우기
            foreach (KeyValuePair<Vector2Int, GameObject> pair in chunks) // 기존 청크 순회
            {
                if (Mathf.Abs(pair.Key.x - center.x) > 1 || Mathf.Abs(pair.Key.y - center.y) > 1) // 실제 활성 범위 확인
                {
                    removed.Add(pair.Key); // 이탈 좌표 기록
                }
            }
            foreach (Vector2Int coordinate in removed) // 이탈 청크 순회
            {
                GameObject chunk = chunks[coordinate]; // 실제 이탈 청크
                chunks.Remove(coordinate); // 활성 목록 제거
                chunk.SetActive(false); // 지연 파괴 전 상호작용 차단
                Destroy(chunk); // 실제 이탈 청크 제거
            }
            for (int x = -1; x <= 1; x++) // 가로 활성 좌표 순회
            {
                for (int y = -1; y <= 1; y++) // 세로 활성 좌표 순회
                {
                    Vector2Int coordinate = center + new Vector2Int(x, y); // 실제 청크 좌표
                    if (chunks.ContainsKey(coordinate)) // 기존 활성 청크 확인
                    {
                        continue; // 기존 청크 유지
                    }
                    GameObject chunk = new GameObject($"PrototypeChunk_{coordinate.x}_{coordinate.y}"); // 시험용 청크 생성
                    chunk.transform.SetParent(transform); // 실제 맵 아래 배치
                    chunk.transform.position = new Vector3(coordinate.x * ChunkSize, coordinate.y * ChunkSize, 0f); // 실제 청크 원점
                    CreateGround(chunk.transform, coordinate); // 실제 청크 바닥 표시
                    chunks.Add(coordinate, chunk); // 실제 활성 청크 등록
                    PopulateChunk(chunk.transform); // 실제 시험 장애물과 지역 생성
                }
            }
        }
        private void PopulateChunk(Transform chunk) // 맵별 사전 배치 패턴 생성
        {
            if (profile == null) // 맵 데이터 부재 확인
            {
                return; // 기존 빈 청크 유지
            }
            CreateDecoration(chunk); // 전투 표시를 가리지 않는 맵 장식
            if (profile.ObstacleChance >= 1f || Random.value < profile.ObstacleChance) // 장애물 임시 확률 확인
            {
                int firstQuadrant = Random.Range(0, 4); // 첫 장애물 사분면 추첨
                int obstacleCount = Random.Range(profile.ObstacleMinimumCount, profile.ObstacleMaximumCount + 1); // 맵 데이터의 장애물 수 추첨
                for (int index = 0; index < obstacleCount; index++) // 서로 다른 사분면 순회
                {
                    CreateObstacle(chunk, (firstQuadrant + index) % 4); // 안전 통로 밖 무작위 장애물 생성
                }
            }
            if (profile.SpecialZoneChance <= 0f || profile.SpecialZoneChance < 1f && Random.value >= profile.SpecialZoneChance) // 지역 임시 확률 확인
            {
                return; // 지역 없는 청크 유지
            }
            PrototypeZoneKind[] kinds = profile.ObstaclePattern == 2 // 습지 지역 확인
                ? new[] { PrototypeZoneKind.Mud, PrototypeZoneKind.Bridge, PrototypeZoneKind.Purify, PrototypeZoneKind.ToxicSpore } // 실제 습지 지역 목록
                : profile.ObstaclePattern == 1 // 폐허 지역 확인
                    ? new[] { PrototypeZoneKind.ElectricFloor, PrototypeZoneKind.Supply, PrototypeZoneKind.Collapse } // 실제 폐허 장치와 보상 목록
                    : new[] { PrototypeZoneKind.Wind, PrototypeZoneKind.Pollen, PrototypeZoneKind.Flower, PrototypeZoneKind.GemTrail, PrototypeZoneKind.AntHill }; // 실제 평원 지역 목록
            PrototypeZoneKind kind = kinds[Random.Range(0, kinds.Length)]; // 실제 맵별 지역 추첨
            if (!TryFindFeaturePosition(chunk, out Vector3 position)) // 안전한 지역 위치 확인
            {
                return; // 안전한 사분면 부재 시 생성 생략
            }
            foreach (PrototypeSpecialZone existing in GetComponentsInChildren<PrototypeSpecialZone>()) // 기존 활성 지역 순회
            {
                if (existing != null && Vector2.Distance(existing.transform.position, position) < existing.Radius + profile.ZoneRadius + 2f) // 지역 간 최소 거리 확인
                {
                    return; // 인접 지역 중첩 방지
                }
            }
            PrototypeSpecialZone.Create(chunk, position, kind, profile); // 실제 청크 지역 생성
            if (kind == PrototypeZoneKind.ElectricFloor) // 방전 바닥의 스위치 배치
            {
                float direction = position.x - chunk.position.x < ChunkSize * 0.5f ? 1f : -1f; // 같은 사분면의 중앙 방향
                Vector3 switchPosition = position + Vector3.right * direction * (profile.ZoneRadius + 1.5f); // 위험 범위 밖의 인접 스위치 위치
                PrototypeSpecialZone.Create(chunk, switchPosition, PrototypeZoneKind.CircuitSwitch, profile); // 실제 회로 스위치 생성
            }
        }
        private void CreateDecoration(Transform chunk) // 충돌 없는 맵별 바닥 장식
        {
            if (Random.value >= profile.DecorationChance) // 데이터의 장식 생성 확률 확인
            {
                return; // 장식 없는 청크 유지
            }
            string folder = profile.ObstaclePattern == 1 ? "Ruins" : profile.ObstaclePattern == 2 ? "Swamp" : "Plains"; // 현재 맵 이미지 폴더
            string[] names = profile.ObstaclePattern == 1 // 폐허 장식 확인
                ? new[] { "균열_경계선", "점멸_경고등_먼지" } // 폐허 바닥 장식 목록
                : profile.ObstaclePattern == 2 // 습지 장식 확인
                    ? new[] { "얕은_수로", "징검돌_길", "발광_포자_물결_안개" } // 습지의 안전 수로와 장식 목록
                    : new[] { "꽃잎_풀결_곤충_그림자" }; // 평원 바닥 장식 목록
            string selectedName = names[Random.Range(0, names.Length)]; // 실제 장식 종류 추첨
            Sprite sprite = Resources.Load<Sprite>("MapElements/" + folder + "/" + selectedName); // 실제 장식 스프라이트
            if (sprite == null) // 장식 이미지 부재 확인
            {
                return; // 다른 맵 요소 유지
            }
            GameObject decoration = new GameObject("MapDecoration"); // 충돌 없는 장식 객체
            decoration.transform.SetParent(chunk, false); // 청크와 수명 연결
            bool safePath = selectedName == "얕은_수로" || selectedName == "징검돌_길"; // 습지 안전 경로 이미지 확인
            float x = safePath ? ChunkSize * 0.5f : Random.Range(3f, ChunkSize - 3f); // 안전 수로의 중앙 연결 통로 위치
            decoration.transform.localPosition = new Vector3(x, Random.Range(3f, ChunkSize - 3f), 0f); // 청크 내부 무작위 위치
            SpriteRenderer renderer = decoration.AddComponent<SpriteRenderer>(); // 실제 바닥 장식 표시기
            renderer.sprite = sprite; // 실제 맵별 이미지 적용
            renderer.sortingOrder = 0; // 전투 요소와 위험 예고 아래 표시
        }
        private bool TryFindFeaturePosition(Transform chunk, out Vector3 position) // 장애물과 연결 통로를 피한 지역 위치
        {
            position = Vector3.zero; // 기본 위치 초기화
            float quarter = ChunkSize * 0.25f; // 사분면 중심 좌표
            float corridorHalf = profile.SafeCorridorWidth * 0.5f; // 안전 통로의 반폭
            if (quarter - profile.ZoneRadius < corridorHalf) // 지역과 통로의 최소 간격 확인
            {
                return false; // 안전 공간이 부족한 청크 제외
            }
            int first = Random.Range(0, 4); // 최초 사분면 추첨
            for (int offset = 0; offset < 4; offset++) // 실제 사분면 후보 순회
            {
                int quadrant = (first + offset) % 4; // 현재 사분면 번호
                float x = (quadrant & 1) == 0 ? quarter : ChunkSize - quarter; // 가로 중심 위치
                float y = (quadrant & 2) == 0 ? quarter : ChunkSize - quarter; // 세로 중심 위치
                Vector3 candidate = chunk.position + new Vector3(x, y, 0f); // 실제 월드 후보 위치
                bool blocked = false; // 장애물 겹침 상태 초기화
                foreach (SnakeContactHazard hazard in chunk.GetComponentsInChildren<SnakeContactHazard>()) // 현재 청크의 장애물 순회
                {
                    if (hazard != null && Vector2.Distance(hazard.transform.position, candidate) < profile.ZoneRadius + 1.2f) // 지역과 실제 장애물 간격 확인
                    {
                        blocked = true; // 장애물과 겹친 후보 제외
                        break; // 나머지 장애물 검사 종료
                    }
                }
                if (!blocked) // 안전한 후보 확인
                {
                    position = candidate; // 실제 지역 위치 저장
                    return true; // 지역 생성 허용
                }
            }
            return false; // 안전한 지역 후보 부재
        }

        private void CreateObstacle(Transform chunk, int quadrant) // 안전한 사분면의 장애물 생성
        {
            float half = ChunkSize * 0.5f; // 청크 중심 위치
            float visualPadding = 1.2f; // 그림과 충돌 범위의 안전 여유
            float corridorHalf = profile.SafeCorridorWidth * 0.5f; // 설정된 안전 통로의 반폭
            float edge = corridorHalf + visualPadding; // 청크 경계 연결 통로 여유
            float inner = half - corridorHalf - visualPadding; // 중앙 연결 통로 바깥 경계
            if (inner <= edge) // 안전한 배치 공간 확인
            {
                return; // 작은 청크의 장애물 생략
            }
            float x = Random.Range(edge, inner); // 사분면의 가로 위치 추첨
            float y = Random.Range(edge, inner); // 사분면의 세로 위치 추첨
            if ((quadrant & 1) != 0) // 오른쪽 사분면 확인
            {
                x = ChunkSize - x; // 좌우 대칭 위치
            }
            if ((quadrant & 2) != 0) // 위쪽 사분면 확인
            {
                y = ChunkSize - y; // 상하 대칭 위치
            }
            Vector3 position = chunk.position + new Vector3(x, y, 0f); // 실제 장애물 위치
            if (player != null && Vector2.Distance(player.transform.position, position) < profile.PlayerObstacleClearance) // 플레이어 주변 안전 영역 확인
            {
                return; // 출현 지점과 현재 머리 주변 보호
            }
            string folder = profile.ObstaclePattern == 1 ? "Ruins" : profile.ObstaclePattern == 2 ? "Swamp" : "Plains"; // 현재 맵의 이미지 폴더
            string[] names = profile.ObstaclePattern == 1 // 폐허 장애물 목록 확인
                ? new[] { "부서진_석주", "녹슨_배관", "무너진_벽체", "정지_톱니_잔해", "엄폐_잔해" } // 폐허 장애물 이미지
                : profile.ObstaclePattern == 2 // 습지 장애물 목록 확인
                    ? new[] { "비틀린_고사목", "거대_버섯_줄기", "노출_뿌리_군락", "암반_섬" } // 습지 장애물 이미지
                    : new[] { "바위_군락", "쓰러진_고목", "울타리_잔해", "개미굴_흙무더기", "바위_엄폐" }; // 평원 장애물 이미지
            string selectedName = names[Random.Range(0, names.Length)]; // 실제 장애물 종류 추첨
            Sprite artwork = Resources.Load<Sprite>("MapElements/" + folder + "/" + selectedName); // 실제 맵별 이미지 조회
            GameObject obstacle = new GameObject("PrototypeObstacle"); // 기존 장애물 객체
            obstacle.transform.SetParent(chunk, false); // 실제 청크와 수명 연결
            obstacle.transform.localPosition = new Vector3(x, y, 0f); // 실제 사분면 위치
            SpriteRenderer renderer = obstacle.AddComponent<SpriteRenderer>(); // 실제 장애물 표시기
            if (obstacleVisual == null) // 공용 대체 이미지 생성 확인
            {
                obstacleVisual = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), new Vector2(0.5f, 0.5f), Texture2D.whiteTexture.width); // 기존 사각 대체 이미지
            }
            renderer.sprite = artwork != null ? artwork : obstacleVisual; // 맵별 이미지 또는 기존 대체 이미지
            renderer.color = artwork != null ? Color.white : new Color(0.36f, 0.32f, 0.28f); // 원본 이미지 색상 유지
            renderer.sortingOrder = 4; // 바닥 위와 적 아래 표시
            BoxCollider2D collider = obstacle.AddComponent<BoxCollider2D>(); // 실제 장애물 충돌체
            collider.isTrigger = true; // 기존 장애물 피해 판정 사용
            collider.size = new Vector2(1f, 0.7f); // 그림 전체보다 작은 충돌 범위
            collider.offset = new Vector2(0f, -0.15f); // 지면에 가까운 충돌 위치
            SnakeContactHazard hazard = obstacle.AddComponent<SnakeContactHazard>(); // 기존 장애물 충돌 처리 생성
            hazard.Configure(SnakeContactHazardType.Obstacle); // 기존 장애물 충돌 처리 연결
            hazard.SetProjectileCover(selectedName == "바위_엄폐" || selectedName == "엄폐_잔해"); // 실제 엄폐 이미지의 투사체 차단 설정
        }
        private void CreateGround(Transform chunk, Vector2Int coordinate) // 실제 청크 바닥 생성
        {
            if (groundSprite == null || groundSprite.bounds.size.x <= 0f || groundSprite.bounds.size.y <= 0f) // 실제 바닥 이미지 확인
            {
                return; // 미설정 맵의 기존 빈 청크 유지
            }
            GameObject ground = new GameObject("Ground"); // 실제 바닥 표시 객체
            ground.transform.SetParent(chunk, false); // 실제 청크 아래 연결
            ground.transform.localPosition = new Vector3(ChunkSize * 0.5f, ChunkSize * 0.5f, 0f); // 실제 청크 중심 위치
            ground.transform.localScale = new Vector3(ChunkSize / groundSprite.bounds.size.x, ChunkSize / groundSprite.bounds.size.y, 1f); // 실제 청크 크기 맞춤
            SpriteRenderer renderer = ground.AddComponent<SpriteRenderer>(); // 실제 바닥 표시기
            renderer.sprite = groundSprite; // 실제 평원 이미지 설정
            renderer.sortingOrder = -100; // 전투 요소 아래 표시
            renderer.flipX = (coordinate.x & 1) != 0; // 인접 가로 경계 반전
            renderer.flipY = (coordinate.y & 1) != 0; // 인접 세로 경계 반전
        }
    }
}
