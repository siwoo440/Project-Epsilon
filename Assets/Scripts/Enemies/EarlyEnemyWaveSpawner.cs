using System; // 직렬화 형식
using System.Collections.Generic; // 생존 적 목록
using ProjectEpsilon.Combat; // 전투 대상
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Player; // 플레이어 구성
using ProjectEpsilon.Progression; // 적 보상
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Enemies // 적 영역
{ // 영역 시작
    public enum EarlyEnemyKind // 적 행동 종류
    { // 열거 시작
        Chaser, // 추적자
        Predator, // 측면 포식자
        Shooter, // 원거리 사수
        Elite // 돌진 엘리트
    } // 열거 끝
    [Serializable] // 편집기 설정 허용
    public sealed class EarlyEnemySettings // 조정 가능한 적 설정
    { // 클래스 시작
        public EarlyEnemyKind kind; // 행동 종류
        public float health; // 체력
        public int damage; // 공격 피해
        public float speedRatio; // 기본 이동 속도 배율
        public float attackInterval; // 공격 간격
        public int experience; // 경험치
        public float firstAppearance; // 최초 출현 시간
        public Color color = Color.white; // 구분 색상
    } // 클래스 끝
    public sealed class EarlyEnemyWaveSpawner : MonoBehaviour // 초반 적 출현기
    { // 클래스 시작
        [SerializeField] private float spawnInterval = 1.5f; // 임시 출현 간격
        [SerializeField] private int maximumEnemies = 40; // 동시 적 상한
        [SerializeField] private float spawnDistance = 9f; // 출현 거리
        [SerializeField] private float despawnDistance = 22f; // 일반 적 정리 거리 임시값
        [SerializeField] private float shooterDistance = 4f; // 사수 유지 거리 임시값
        [SerializeField] private float projectileSpeed = 5f; // 투사체 속도 임시값
        [SerializeField] private float projectileLifetime = 3f; // 투사체 수명 임시값
        [SerializeField] private float eliteWarningDuration = 1f; // 엘리트 예고 시간 가정값
        [SerializeField] private float eliteChargeDuration = 0.6f; // 엘리트 돌진 시간 가정값
        [SerializeField] private float eliteChargeMultiplier = 3f; // 엘리트 돌진 속도 가정값
        [SerializeField] private int healAmount = 15; // 회복량 임시값
        [SerializeField] private float healDropChance = 0.25f; // 회복 보상 확률 임시값
        [SerializeField] private float bodyRepairDropChance = 0.1f; // 몸통 복구 확률 임시값
        [SerializeField] private float elapsed; // 전투 경과 시간
        [SerializeField] private Sprite visual; // 적과 보석 시각 자료
        [SerializeField] private EarlyEnemySettings[] enemies = // 시험용 적 설정
        { // 설정 목록 시작
            new EarlyEnemySettings // 추적자 임시값
            { // 설정 시작
                kind = EarlyEnemyKind.Chaser, // 추적 행동
                health = 25f, // 체력
                damage = 10, // 피해량
                speedRatio = 0.8f, // 속도 배율
                attackInterval = 1.2f, // 공격 간격
                experience = 1, // 경험치
                firstAppearance = 0f, // 출현 시점
                color = new Color(1f, 0.35f, 0.35f) // 표시 색상
            }, // 설정 끝
            new EarlyEnemySettings // 포식자 임시값
            { // 설정 시작
                kind = EarlyEnemyKind.Predator, // 몸통 추적 행동
                health = 35f, // 체력
                damage = 12, // 피해량
                speedRatio = 0.9f, // 속도 배율
                attackInterval = 1.5f, // 공격 간격
                experience = 2, // 경험치
                firstAppearance = 60f, // 출현 시점
                color = new Color(1f, 0.65f, 0.25f) // 표시 색상
            }, // 설정 끝
            new EarlyEnemySettings // 사수 임시값
            { // 설정 시작
                kind = EarlyEnemyKind.Shooter, // 원거리 행동
                health = 20f, // 체력
                damage = 8, // 피해량
                speedRatio = 0.7f, // 속도 배율
                attackInterval = 2f, // 공격 간격
                experience = 2, // 경험치
                firstAppearance = 120f, // 출현 시점
                color = new Color(0.75f, 0.4f, 1f) // 표시 색상
            }, // 설정 끝
            new EarlyEnemySettings // 엘리트 가정값
            { // 설정 시작
                kind = EarlyEnemyKind.Elite, // 예고 돌진 행동
                health = 160f, // 체력
                damage = 15, // 피해량
                speedRatio = 0.9f, // 속도 배율
                attackInterval = 5f, // 공격 간격
                experience = 20, // 경험치
                firstAppearance = 240f, // 출현 시점
                color = new Color(1f, 0.1f, 0.75f) // 표시 색상
            } // 설정 끝
        }; // 설정 목록 끝
        private SnakeMovement player; // 플레이어 이동 참조
        private SnakeBodyManager body; // 플레이어 몸통 참조
        private SnakeHealth health; // 공용 체력 참조
        private float nextSpawn; // 다음 출현 시각
        private int activeEnemies; // 생존 적 수
        private readonly List<WeaponTarget> targets = new List<WeaponTarget>(); // 생존 적 추적
        private WeaponTarget eliteTarget; // 정리 제외 엘리트
        private bool eliteSpawned; // 엘리트 출현 여부
        public int ActiveEnemies => activeEnemies; // 생존 적 수 조회
        private void Start() // 플레이 시작 처리
        { // 메서드 시작
            player = FindFirstObjectByType<SnakeMovement>(); // 플레이어 이동 조회
            body = FindFirstObjectByType<SnakeBodyManager>(); // 몸통 관리자 조회
            health = FindFirstObjectByType<SnakeHealth>(); // 체력 관리자 조회
            if (visual == null && player != null) // 시각 자료 부재 확인
            { // 조건 시작
                SpriteRenderer renderer = player.GetComponentInChildren<SpriteRenderer>(); // 플레이어 렌더러 조회
                visual = renderer == null ? null : renderer.sprite; // 대체 스프라이트 저장
            } // 조건 끝
        } // 메서드 끝
        private void Update() // 출현 시간 갱신
        { // 메서드 시작
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 전투 가능 여부
            { // 조건 시작
                return; // 출현 중지
            } // 조건 끝
            elapsed += Time.deltaTime; // 전투 시간 누적
            CleanupDistantEnemies(); // 먼 일반 적 정리
            if (!eliteSpawned && enemies.Length > 3 && enemies[3] != null && elapsed >= enemies[3].firstAppearance) // 엘리트 출현 조건
            { // 조건 시작
                ReserveEliteSlot(); // 엘리트 출현 자리 확보
                Spawn(enemies[3]); // 엘리트 출현
                eliteSpawned = true; // 단회 출현 저장
            } // 조건 끝
            if (elapsed < nextSpawn || activeEnemies >= Mathf.Max(1, maximumEnemies)) // 일반 출현 대기 조건
            { // 조건 시작
                return; // 일반 출현 중지
            } // 조건 끝
            nextSpawn = elapsed + Mathf.Max(0.1f, spawnInterval); // 다음 출현 시각 저장
            int available = 0; // 출현 가능 종류 수
            for (int index = 0; index < enemies.Length; index++) // 적 설정 순회
            { // 반복 시작
                if (enemies[index] != null && enemies[index].kind != EarlyEnemyKind.Elite && elapsed >= enemies[index].firstAppearance) // 일반 적 해금 확인
                { // 조건 시작
                    available++; // 출현 가능 종류 증가
                } // 조건 끝
            } // 반복 끝
            if (available == 0) // 출현 가능 적 부재
            { // 조건 시작
                return; // 출현 생략
            } // 조건 끝
            int selected = UnityEngine.Random.Range(0, available); // 출현 종류 추첨
            foreach (EarlyEnemySettings setting in enemies) // 해금 적 순회
            { // 반복 시작
                if (setting == null || setting.kind == EarlyEnemyKind.Elite || elapsed < setting.firstAppearance) // 미해금 적 제외
                { // 조건 시작
                    continue; // 다음 설정 이동
                } // 조건 끝
                if (selected-- == 0) // 선택 순번 확인
                { // 조건 시작
                    Spawn(setting); // 일반 적 출현
                    break; // 선택 종료
                } // 조건 끝
            } // 반복 끝
        } // 메서드 끝
        private void Spawn(EarlyEnemySettings setting) // 적 생성
        { // 메서드 시작
            Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized; // 출현 방향 추첨
            if (direction.sqrMagnitude < 0.01f) // 방향 부재 확인
            { // 조건 시작
                direction = Vector2.right; // 대체 방향 설정
            } // 조건 끝
            GameObject enemy = new GameObject("EarlyEnemy_" + setting.kind); // 적 오브젝트 생성
            enemy.transform.position = player.transform.position + (Vector3)(direction * Mathf.Max(2f, spawnDistance)); // 플레이어 주변 배치
            SpriteRenderer renderer = enemy.AddComponent<SpriteRenderer>(); // 적 렌더러 생성
            renderer.sprite = visual; // 시각 자료 적용
            renderer.color = setting.color; // 적 구분 색상 적용
            renderer.sortingOrder = 8; // 적 표시 순서
            enemy.transform.localScale = Vector3.one * (setting.kind == EarlyEnemyKind.Elite ? 0.8f : 0.5f); // 적 크기 적용
            CircleCollider2D collider = enemy.AddComponent<CircleCollider2D>(); // 적 충돌체 생성
            collider.isTrigger = true; // 접촉 감지 설정
            Rigidbody2D rigidbody = enemy.AddComponent<Rigidbody2D>(); // 적 물리체 생성
            rigidbody.bodyType = RigidbodyType2D.Kinematic; // 물리체 이동 방식
            rigidbody.gravityScale = 0f; // 중력 제거
            rigidbody.useFullKinematicContacts = true; // 운동학 충돌 감지
            WeaponTarget target = enemy.AddComponent<WeaponTarget>(); // 공격 가능 대상 생성
            target.Configure(setting.health); // 적 체력 설정
            enemy.AddComponent<WeaponTargetStatusController>(); // 기존 상태 효과 적용
            enemy.AddComponent<WeaponTargetKnockbackController>(); // 기존 넉백 적용
            EnemyDropController drop = enemy.AddComponent<EnemyDropController>(); // 기존 보상 적용
            drop.Configure(setting.experience, healAmount, healDropChance, bodyRepairDropChance, visual, setting.kind != EarlyEnemyKind.Elite && setting.experience == 2 ? 2 : 1); // 임시 보상과 보석 수 설정
            SnakeContactHazard hazard = enemy.AddComponent<SnakeContactHazard>(); // 머리 직접 충돌 판정
            hazard.Configure(SnakeContactHazardType.EnemyDirect); // 적 직접 충돌 종류
            EarlyEnemyBehaviour behaviour = enemy.AddComponent<EarlyEnemyBehaviour>(); // 적 행동 생성
            behaviour.Configure(setting, player, body, health, visual, shooterDistance, projectileSpeed, projectileLifetime, eliteWarningDuration, eliteChargeDuration, eliteChargeMultiplier); // 적 행동 연결
            activeEnemies++; // 생존 적 증가
            targets.Add(target); // 생존 적 목록 추가
            if (setting.kind == EarlyEnemyKind.Elite) // 엘리트 확인
            { // 조건 시작
                eliteTarget = target; // 정리 제외 대상 저장
            } // 조건 끝
            target.Died += HandleEnemyDied; // 처치 시 수량 갱신 연결
        } // 메서드 끝
        private void HandleEnemyDied(WeaponTarget target) // 적 처치 처리
        { // 메서드 시작
            activeEnemies = Mathf.Max(0, activeEnemies - 1); // 생존 적 감소
            targets.Remove(target); // 생존 적 목록 제거
        } // 메서드 끝
        public static bool ShouldDespawn(Vector3 position, Vector3 playerPosition, float distance, bool elite) // 거리 정리 조건
        { // 메서드 시작
            float safeDistance = Mathf.Max(0.1f, distance); // 최소 정리 거리
            return !elite && (position - playerPosition).sqrMagnitude > safeDistance * safeDistance; // 일반 적 거리 초과 여부
        } // 메서드 끝
        private void CleanupDistantEnemies() // 먼 일반 적 정리
        { // 메서드 시작
            for (int index = targets.Count - 1; index >= 0; index--) // 생존 적 역순 확인
            { // 반복 시작
                WeaponTarget target = targets[index]; // 현재 적 조회
                if (target == null) // 외부 제거 적 확인
                { // 조건 시작
                    targets.RemoveAt(index); // 빈 대상 제거
                    activeEnemies = Mathf.Max(0, activeEnemies - 1); // 생존 수 보정
                    continue; // 다음 적 확인
                } // 조건 끝
                if (!ShouldDespawn(target.transform.position, player.transform.position, despawnDistance, target == eliteTarget)) // 정리 조건 확인
                { // 조건 시작
                    continue; // 유지 대상 건너뛰기
                } // 조건 끝
                target.Died -= HandleEnemyDied; // 처치 수량 연결 해제
                targets.RemoveAt(index); // 생존 목록 제거
                activeEnemies = Mathf.Max(0, activeEnemies - 1); // 생존 수 감소
                Destroy(target.gameObject); // 보상 없이 적 제거
            } // 반복 끝
        } // 메서드 끝
        private void ReserveEliteSlot() // 엘리트 출현 자리 확보
        { // 메서드 시작
            if (activeEnemies < Mathf.Max(1, maximumEnemies)) // 빈 자리 확인
            { // 조건 시작
                return; // 자리 확보 생략
            } // 조건 끝
            WeaponTarget farthest = null; // 가장 먼 적 초기화
            float farthestDistance = -1f; // 가장 먼 거리 초기화
            foreach (WeaponTarget target in targets) // 생존 적 순회
            { // 반복 시작
                if (target == null) // 제거된 적 확인
                { // 조건 시작
                    continue; // 다음 적 이동
                } // 조건 끝
                float distance = (target.transform.position - player.transform.position).sqrMagnitude; // 플레이어 거리 계산
                if (distance > farthestDistance) // 가장 먼 적 확인
                { // 조건 시작
                    farthest = target; // 제거 후보 저장
                    farthestDistance = distance; // 가장 먼 거리 저장
                } // 조건 끝
            } // 반복 끝
            if (farthest != null) // 제거 후보 확인
            { // 조건 시작
                targets.Remove(farthest); // 생존 적 목록 제거
                activeEnemies = Mathf.Max(0, activeEnemies - 1); // 생존 적 수 감소
                Destroy(farthest.gameObject); // 먼 적 제거
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
