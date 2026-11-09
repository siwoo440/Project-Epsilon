using System.Collections.Generic; // 활성 청크 저장소
using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Player; // 플레이어 위치
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.World // 맵 영역
{ // 영역 시작
    public sealed class PrototypeChunkMap : MonoBehaviour // 시험용 개방 평원 청크 관리
    { // 클래스 시작
        [SerializeField] private float chunkSize = 24f; // 청크 크기 임시값
        [SerializeField] private Sprite groundSprite; // 개방 평원 바닥 이미지
        private readonly Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>(); // 실제 활성 청크
        private readonly List<Vector2Int> removed = new List<Vector2Int>(); // 이탈 청크 임시 목록
        private SnakeMovement player; // 실제 플레이어 위치
        private Vector2Int center; // 현재 중심 청크
        private bool initialized; // 최초 청크 생성 여부
        private PrototypeMapProfile profile; // 현재 런 맵 데이터
        private static Sprite obstacleVisual; // 장애물 공용 시험 이미지
        public int ActiveCount => chunks.Count; // 실제 활성 청크 수
        public float ChunkSize => Mathf.Max(1f, chunkSize); // 안전한 청크 크기
        public PrototypeMapProfile Profile => profile; // 실제 적용된 맵 데이터

        public bool ApplyProfile(PrototypeMapProfile data) // 시작 전 맵 설정
        { // 메서드 시작
            if (data == null || data.Ground == null || initialized) // 데이터와 청크 생성 상태 확인
            { // 조건 시작
                return false; // 런 중 맵 교체 방지
            } // 조건 끝
            profile = data; // 실제 맵 데이터 저장
            groundSprite = data.Ground; // 실제 맵 바닥 적용
            return true; // 실제 바닥 설정 완료
        } // 메서드 끝

        public GameObject GetChunk(Vector2Int coordinate) // 실제 청크 좌표 조회
        { // 메서드 시작
            return chunks.TryGetValue(coordinate, out GameObject chunk) ? chunk : null; // 좌표별 활성 청크 반환
        } // 메서드 끝
        private void Start() // 실제 플레이어 위치 연결
        { // 메서드 시작
            player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
        } // 메서드 끝
        private void Update() // 실제 플레이어 주변 청크 갱신
        { // 메서드 시작
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 실제 플레이 상태 확인
            { // 조건 시작
                return; // 정지 중 청크 갱신 방지
            } // 조건 끝
            Vector3 position = player.transform.position; // 실제 플레이어 위치
            Vector2Int next = new Vector2Int(Mathf.FloorToInt(position.x / ChunkSize), Mathf.FloorToInt(position.y / ChunkSize)); // 음수를 포함한 중심 좌표
            if (initialized && next == center) // 중심 청크 유지 확인
            { // 조건 시작
                return; // 불필요한 재생성 방지
            } // 조건 끝
            center = next; // 실제 중심 저장
            initialized = true; // 최초 생성 상태 저장
            removed.Clear(); // 이전 이탈 목록 비우기
            foreach (KeyValuePair<Vector2Int, GameObject> pair in chunks) // 기존 청크 순회
            { // 반복 시작
                if (Mathf.Abs(pair.Key.x - center.x) > 1 || Mathf.Abs(pair.Key.y - center.y) > 1) // 실제 활성 범위 확인
                { // 조건 시작
                    removed.Add(pair.Key); // 이탈 좌표 기록
                } // 조건 끝
            } // 반복 끝
            foreach (Vector2Int coordinate in removed) // 이탈 청크 순회
            { // 반복 시작
                GameObject chunk = chunks[coordinate]; // 실제 이탈 청크
                chunks.Remove(coordinate); // 활성 목록 제거
                chunk.SetActive(false); // 지연 파괴 전 상호작용 차단
                Destroy(chunk); // 실제 이탈 청크 제거
            } // 반복 끝
            for (int x = -1; x <= 1; x++) // 가로 활성 좌표 순회
            { // 반복 시작
                for (int y = -1; y <= 1; y++) // 세로 활성 좌표 순회
                { // 반복 시작
                    Vector2Int coordinate = center + new Vector2Int(x, y); // 실제 청크 좌표
                    if (chunks.ContainsKey(coordinate)) // 기존 활성 청크 확인
                    { // 조건 시작
                        continue; // 기존 청크 유지
                    } // 조건 끝
                    GameObject chunk = new GameObject($"PrototypeChunk_{coordinate.x}_{coordinate.y}"); // 시험용 청크 생성
                    chunk.transform.SetParent(transform); // 실제 맵 아래 배치
                    chunk.transform.position = new Vector3(coordinate.x * ChunkSize, coordinate.y * ChunkSize, 0f); // 실제 청크 원점
                    CreateGround(chunk.transform, coordinate); // 실제 청크 바닥 표시
                    chunks.Add(coordinate, chunk); // 실제 활성 청크 등록
                    PopulateChunk(chunk.transform); // 실제 시험 장애물과 지역 생성
                } // 반복 끝
            } // 반복 끝
        } // 메서드 끝
        private void PopulateChunk(Transform chunk) // 맵별 사전 배치 패턴 생성
        { // 메서드 시작
            if (profile == null) // 맵 데이터 부재 확인
            { // 조건 시작
                return; // 기존 빈 청크 유지
            } // 조건 끝
            if (profile.ObstacleChance >= 1f || Random.value < profile.ObstacleChance) // 장애물 임시 확률 확인
            { // 조건 시작
                CreateObstacle(chunk, 0.25f); // 첫 번째 사전 배치 장애물
                CreateObstacle(chunk, 0.75f); // 통로를 남긴 두 번째 장애물
            } // 조건 끝
            if (profile.SpecialZoneChance <= 0f || profile.SpecialZoneChance < 1f && Random.value >= profile.SpecialZoneChance) // 지역 임시 확률 확인
            { // 조건 시작
                return; // 지역 없는 청크 유지
            } // 조건 끝
            Vector3 position = chunk.position + new Vector3(ChunkSize * 0.5f, ChunkSize * 0.5f, 0f); // 실제 청크 중심
            foreach (PrototypeSpecialZone existing in GetComponentsInChildren<PrototypeSpecialZone>()) // 기존 활성 지역 순회
            { // 반복 시작
                if (existing != null && Vector2.Distance(existing.transform.position, position) < existing.Radius + profile.ZoneRadius + 2f) // 지역 간 최소 거리 확인
                { // 조건 시작
                    return; // 인접 지역 중첩 방지
                } // 조건 끝
            } // 반복 끝
            PrototypeZoneKind kind = (PrototypeZoneKind)Random.Range(0, 4); // 시험 지역 종류 선택
            PrototypeSpecialZone.Create(chunk, position, kind, profile); // 실제 청크 지역 생성
        } // 메서드 끝
        private void CreateObstacle(Transform chunk, float offset) // 실제 장애물 하나 생성
        { // 메서드 시작
            GameObject obstacle = new GameObject("PrototypeObstacle"); // 실제 장애물 객체
            obstacle.transform.SetParent(chunk, false); // 실제 청크와 수명 연결
            bool vertical = profile.ObstaclePattern == 1; // 회랑형 사전 패턴 확인
            obstacle.transform.localPosition = vertical ? new Vector3(ChunkSize * 0.5f, ChunkSize * offset, 0f) : new Vector3(ChunkSize * offset, ChunkSize * 0.5f, 0f); // 실제 패턴 좌표
            SpriteRenderer renderer = obstacle.AddComponent<SpriteRenderer>(); // 실제 장애물 표시기
            if (obstacleVisual == null) // 공용 이미지 생성 여부 확인
            { // 조건 시작
                obstacleVisual = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), new Vector2(0.5f, 0.5f), Texture2D.whiteTexture.width); // 시험용 사각 이미지 생성
            } // 조건 끝
            renderer.sprite = obstacleVisual; // 실제 공용 장애물 이미지
            renderer.color = profile.ObstaclePattern == 2 ? new Color(0.17f, 0.34f, 0.27f) : new Color(0.36f, 0.32f, 0.28f); // 맵별 임시 장애물 색상
            renderer.sortingOrder = 4; // 바닥 위 장애물 표시
            BoxCollider2D collider = obstacle.AddComponent<BoxCollider2D>(); // 실제 장애물 충돌체
            collider.isTrigger = true; // 기존 장애물 피해 판정 사용
            obstacle.transform.localScale = Vector3.one * 1.4f; // 실제 장애물 시각 크기
            collider.size = Vector2.one; // 표시 크기와 실제 충돌 일치
            obstacle.AddComponent<SnakeContactHazard>().Configure(SnakeContactHazardType.Obstacle); // 기존 장애물 충돌 처리 연결
        } // 메서드 끝
        private void CreateGround(Transform chunk, Vector2Int coordinate) // 실제 청크 바닥 생성
        { // 메서드 시작
            if (groundSprite == null || groundSprite.bounds.size.x <= 0f || groundSprite.bounds.size.y <= 0f) // 실제 바닥 이미지 확인
            { // 조건 시작
                return; // 미설정 맵의 기존 빈 청크 유지
            } // 조건 끝
            GameObject ground = new GameObject("Ground"); // 실제 바닥 표시 객체
            ground.transform.SetParent(chunk, false); // 실제 청크 아래 연결
            ground.transform.localPosition = new Vector3(ChunkSize * 0.5f, ChunkSize * 0.5f, 0f); // 실제 청크 중심 위치
            ground.transform.localScale = new Vector3(ChunkSize / groundSprite.bounds.size.x, ChunkSize / groundSprite.bounds.size.y, 1f); // 실제 청크 크기 맞춤
            SpriteRenderer renderer = ground.AddComponent<SpriteRenderer>(); // 실제 바닥 표시기
            renderer.sprite = groundSprite; // 실제 평원 이미지 설정
            renderer.sortingOrder = -100; // 전투 요소 아래 표시
            renderer.flipX = (coordinate.x & 1) != 0; // 인접 가로 경계 반전
            renderer.flipY = (coordinate.y & 1) != 0; // 인접 세로 경계 반전
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
