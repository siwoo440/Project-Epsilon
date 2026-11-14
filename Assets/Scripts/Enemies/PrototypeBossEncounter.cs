using ProjectEpsilon.Combat; // 실제 무기 공격 대상
using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Player; // 플레이어와 공용 체력
using ProjectEpsilon.World; // 맵별 시험 보스 데이터
using System.Collections.Generic; // 안전 출현 후보의 겹침 목록
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Enemies // 적 영역
{
    public sealed class PrototypeBossEncounter : MonoBehaviour // 시험용 최종 보스 출현기
    {
        [SerializeField] private float maximumHealth = 600f; // 보스 체력 임시값
        [SerializeField] private int damage = 15; // 투사체 피해 임시값
        [SerializeField] private float speedRatio = 0.75f; // 플레이어 기준 이동 속도 임시값
        [SerializeField] private float attackInterval = 2f; // 사격 간격 임시값
        [SerializeField] private float keepDistance = 4f; // 사격 유지 거리 임시값
        [SerializeField] private float projectileSpeed = 5f; // 투사체 속도 임시값
        [SerializeField] private float projectileLifetime = 3f; // 투사체 수명 임시값
        [SerializeField] private int projectileCount = 3; // 첫 단계 부채꼴 투사체 수 임시값
        [SerializeField] private int phaseTwoProjectileCount = 5; // 두 번째 단계 투사체 수 임시값
        [SerializeField] private float projectileSpreadDegrees = 40f; // 부채꼴 전체 각도 임시값
        [SerializeField] private float phaseTwoSpeedMultiplier = 1.25f; // 두 번째 단계 속도 임시 배율
        [SerializeField] private float phaseTwoIntervalMultiplier = 0.75f; // 두 번째 단계 공격 간격 임시 배율
        [SerializeField] private float chargeInterval = 6f; // 돌진 간격 임시값
        [SerializeField] private float chargeWarningDuration = 1f; // 돌진 예고 시간 임시값
        [SerializeField] private float chargeDuration = 0.8f; // 돌진 지속 시간 임시값
        [SerializeField] private float chargeSpeed = 8f; // 돌진 속도 임시값
        [SerializeField] private float phaseTwoChargeSpeedMultiplier = 1.4f; // 절단 돌진 속도 임시 배율
        [SerializeField] private float coldFreezeSlowMultiplier = 0.5f; // 빙결 대체 보스 감속 임시값
        [SerializeField] private float bodyPushDistance = 0.6f; // 보스 몸통 밀림 거리 임시값
        [SerializeField] private float bodyPushRecoveryDuration = 0.4f; // 밀린 몸통 경로 복귀 시간 임시값
        [SerializeField] private float bodyPushGraceDuration = 0.5f; // 밀린 몸통 자기 충돌 유예 임시값
        [SerializeField] private float hazardInterval = 8f; // 위험 구역 출현 간격 임시값
        [SerializeField] private float hazardWarningDuration = 1f; // 위험 구역 예고 시간 임시값
        [SerializeField] private float hazardDuration = 3f; // 위험 구역 지속 시간 임시값
        [SerializeField] private float hazardDamageInterval = 1f; // 위험 구역 피해 간격 임시값
        [SerializeField] private float hazardRadius = 1.2f; // 위험 구역 반경 임시값
        [SerializeField] private float phaseTwoHazardIntervalMultiplier = 1f; // 두 번째 단계 위험 구역 간격 배율
        [SerializeField] private int hazardDamage = 10; // 위험 구역 피해 임시값
        [SerializeField] private float summonInterval = 12f; // 보스 잡몹 소환 간격 임시값
        [SerializeField] private int summonMaximum = 4; // 보스 잡몹 동시 상한 임시값
        [SerializeField] private float summonRadius = 2f; // 보스 주변 소환 거리 임시값
        [SerializeField] private float spawnDistance = 8f; // 보스 출현 거리 임시값
        [SerializeField] private Sprite bossVisual; // 보스 전용 또는 임시 이미지
        [SerializeField] private Sprite projectileVisual; // 보스 투사체 전용 또는 임시 이미지
        [SerializeField] private Color bossTint = new Color(0.7f, 0.1f, 0.9f); // 기존 보스 구분 색상
        [SerializeField] private bool alignVisualWithTarget; // 보스 이미지의 목표 방향 회전
        private Sprite fallbackBossVisual; // 씬의 기본 보스 이미지
        private bool fallbackVisualAlignment; // 씬의 기본 회전 설정
        private EarlyEnemyWaveSpawner spawner; // 실제 20분 전환기
        private WeaponTarget boss; // 실제 공격 가능한 보스
        private EarlyEnemySettings settings; // 실제 보스 행동 설정
        private readonly PrototypeBossPhase phase = new PrototypeBossPhase(); // 보스 체력 단계 상태
        private bool spawned; // 보스 출현 완료 여부
        private float nextSpawnAttempt; // 다음 안전 출현 재시도 시각
        private readonly List<Collider2D> spawnOverlaps = new List<Collider2D>(); // 실제 출현 후보의 충돌 목록
        private int mapBossPattern; // 선택된 맵의 보스 패턴
        public WeaponTarget Boss => boss; // 실제 생성 보스 조회
        public bool IsPhaseTwo => phase.IsPhaseTwo; // 현재 보스 단계 조회

        private void Awake() // 씬의 기본 이미지 보존
        { // 함수 시작
            fallbackBossVisual = bossVisual; // 기본 보스 이미지 저장
            fallbackVisualAlignment = alignVisualWithTarget; // 기본 이미지 방향 저장
        } // 함수 종료

        public bool ApplyProfile(PrototypeMapProfile profile) // 시작 전 맵별 보스 설정
        {
            if (profile == null || spawned) // 실제 데이터와 출현 상태 확인
            {
                return false; // 출현 후 보스 변경 방지
            }
            maximumHealth = profile.BossHealth; // 맵별 임시 체력
            speedRatio = profile.BossSpeedRatio; // 맵별 임시 이동 비율
            attackInterval = profile.BossAttackInterval; // 맵별 임시 발사 간격
            projectileCount = profile.BossProjectileCount; // 맵별 첫 단계 발사 수
            phaseTwoProjectileCount = profile.BossPhaseTwoProjectileCount; // 맵별 두 번째 단계 발사 수
            damage = profile.BossDamage; // 맵별 투사체와 돌진 피해
            phaseTwoSpeedMultiplier = profile.BossPhaseTwoSpeedMultiplier; // 맵별 두 번째 단계 이동 배율
            phaseTwoIntervalMultiplier = profile.BossPhaseTwoIntervalMultiplier; // 맵별 두 번째 단계 사격 배율
            chargeInterval = profile.BossChargeInterval; // 맵별 돌진 간격
            chargeWarningDuration = profile.BossChargeWarningDuration; // 맵별 돌진 예고 시간
            chargeDuration = profile.BossChargeDuration; // 맵별 돌진 지속 시간
            chargeSpeed = profile.BossChargeSpeed; // 맵별 돌진 속도
            phaseTwoChargeSpeedMultiplier = profile.BossPhaseTwoChargeSpeedMultiplier; // 맵별 두 번째 단계 돌진 배율
            hazardInterval = profile.BossHazardInterval; // 맵별 위험 구역 간격
            phaseTwoHazardIntervalMultiplier = profile.BossPhaseTwoHazardIntervalMultiplier; // 맵별 두 번째 단계 구역 간격 배율
            hazardWarningDuration = profile.BossHazardWarningDuration; // 맵별 위험 구역 예고
            hazardDuration = profile.BossHazardDuration; // 맵별 위험 구역 지속
            hazardDamageInterval = profile.BossHazardDamageInterval; // 맵별 위험 피해 간격
            hazardRadius = profile.BossHazardRadius; // 맵별 위험 반경
            hazardDamage = profile.BossHazardDamage; // 맵별 위험 피해
            summonInterval = profile.BossSummonInterval; // 맵별 소환 간격
            summonMaximum = profile.BossSummonMaximum; // 맵별 소환 적 상한
            summonRadius = profile.BossSummonRadius; // 맵별 소환 거리
            bossVisual = profile.BossArtwork != null ? profile.BossArtwork : fallbackBossVisual; // 맵 이미지 또는 씬 기본 이미지
            alignVisualWithTarget = profile.BossArtwork != null || fallbackVisualAlignment; // 맵 이미지의 이동 방향 정렬
            bossTint = profile.BossTint; // 맵별 이미지 색상
            mapBossPattern = profile.BossPattern; // 선택된 맵의 보스 패턴
            return true; // 맵별 설정 적용 완료
        }

        private void OnEnable() // 실제 전환 알림 연결
        {
            spawner = GetComponent<EarlyEnemyWaveSpawner>(); // 같은 관리자 출현기 조회
            if (spawner == null) // 출현기 부재 확인
            {
                spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 활성 출현기 대체 조회
            }
            if (spawner != null) // 실제 출현기 연결 확인
            {
                spawner.BossTransitionStarted += TrySpawnBoss; // 실제 전환 이벤트 구독
                if (spawner.HasEnteredBossTransition) // 이미 완료된 전환 확인
                {
                    TrySpawnBoss(); // 늦은 활성화 시 보스 출현
                }
            }
        }

        private void OnDisable() // 실제 전환 알림 해제
        {
            if (spawner != null) // 출현기 연결 확인
            {
                spawner.BossTransitionStarted -= TrySpawnBoss; // 중복 구독 방지
            }
        }

        private void Update() // 실제 보스 체력 단계 관찰
        {
            if (!spawned && spawner != null && spawner.HasEnteredBossTransition && GameManager.Instance != null && GameManager.Instance.IsPlaying && Time.time >= nextSpawnAttempt) // 전환 후 안전 후보 재시도 확인
            { // 조건 시작
                TrySpawnBoss(); // 실제 안전 출현 재시도
            } // 조건 종료
            if (boss == null || !boss.IsAlive || settings == null) // 실제 생존 보스 확인
            {
                return; // 단계 갱신 생략
            }
            bool wasPhaseTwo = phase.IsPhaseTwo; // 기존 단계 저장
            phase.Observe(boss.CurrentHealth, boss.MaximumHealth, boss.IsAlive); // 실제 체력 경계 반영
            if (!wasPhaseTwo && phase.IsPhaseTwo) // 두 번째 단계 최초 진입 확인
            {
                settings.speedRatio = Mathf.Max(0f, speedRatio) * Mathf.Max(0f, phaseTwoSpeedMultiplier); // 임시 이동 속도 변경
                settings.attackInterval = Mathf.Max(0.1f, attackInterval * Mathf.Max(0f, phaseTwoIntervalMultiplier)); // 임시 사격 간격 변경
                settings.projectileCount = Mathf.Clamp(phaseTwoProjectileCount, 1, 9); // 두 번째 단계 임시 사격 수
            }
        }

        private void OnGUI() // 시험용 보스 체력 표시
        {
            if (boss == null || !boss.IsAlive || boss.MaximumHealth <= 0f) // 생존 보스와 최대 체력 확인
            {
                return; // 표시 생략
            }
            Rect background = ResolveHealthBarRect(Camera.main); // 실제 전투 화면 안의 체력 바
            GUI.Box(background, GUIContent.none); // 체력 바 배경
            float ratio = Mathf.Clamp01(boss.CurrentHealth / boss.MaximumHealth); // 현재 체력 비율
            Rect fill = new Rect(background.x + 2f, background.y + 2f, Mathf.Max(0f, background.width - 4f) * ratio, background.height - 4f); // 남은 체력 영역
            Color previousColor = GUI.color; // 이전 화면 색상 보존
            GUI.color = new Color(0.85f, 0.15f, 0.25f); // 시험용 체력 색상
            GUI.DrawTexture(fill, Texture2D.whiteTexture); // 남은 체력 표시
            GUI.color = previousColor; // 다른 UI 색상 복구
            Font previousFont = GUI.skin.font; // 실제 이전 화면 글꼴
            Font displayFont = ProjectEpsilon.UI.RunFrontendController.Instance == null ? null : ProjectEpsilon.UI.RunFrontendController.Instance.DisplayFont; // 실제 한글 화면 글꼴
            if (displayFont != null) // 실제 한글 글꼴 확인
            {
                GUI.skin.font = displayFont; // 실제 보스 체력 글꼴 적용
            }
            try // 실제 보스 체력 표시 중 글꼴 보호
            {
                GUI.Label(background, FormatHealthLabel(boss.CurrentHealth, boss.MaximumHealth)); // 실제 한국어 보스 체력 문구
            }
            finally // 실제 다른 화면 글꼴 복원
            {
                GUI.skin.font = previousFont; // 실제 공용 글꼴 복원
            }
        }

        private static string FormatHealthLabel(float current, float maximum) // 실제 한국어 보스 체력 문구
        {
            return $"보스 {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(maximum)}"; // 실제 현재와 최대 체력 표시
        }

        private static Rect ResolveHealthBarRect(Camera camera) // 실제 전투 카메라 기준 체력 표시 영역
        {
            Rect viewport = camera == null ? new Rect(0f, 0f, Screen.width, Screen.height) : camera.pixelRect; // 실제 카메라 또는 전체 화면
            float width = Mathf.Min(400f, Mathf.Max(1f, viewport.width - 40f)); // 실제 전투 영역에 맞춘 체력 바
            return new Rect(viewport.xMin + (viewport.width - width) * 0.5f, Screen.height - viewport.yMax + 48f, width, 22f); // 실제 전투 영역 상단 중앙
        }

        private void TrySpawnBoss() // 실제 최종 보스 출현
        {
            if (spawned || GameManager.Instance == null) // 중복 출현과 관리자 확인
            {
                return; // 출현 생략
            }
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동 조회
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 조회
            SnakeHealth health = FindFirstObjectByType<SnakeHealth>(); // 실제 공용 체력 조회
            if (player == null || body == null || health == null) // 필수 플레이어 구성 확인
            {
                Debug.LogError("[Project Epsilon] 시험 보스 출현에 필요한 플레이어 구성이 없습니다."); // 구성 오류 기록
                return; // 잘못된 출현 방지
            }
            if (!TryFindSafeSpawn(player.transform.position, body, out Vector3 spawnPosition)) // 실제 보스 충돌 범위의 안전 후보 확인
            { // 조건 시작
                nextSpawnAttempt = Time.time + 0.25f; // 안전 후보가 없을 때 다음 재시도
                return; // 장애물이나 몸통 위 출현 차단
            } // 조건 종료
            SpriteRenderer source = player.GetComponentInChildren<SpriteRenderer>(); // 플레이어 스프라이트 조회
            Sprite sprite = bossVisual != null ? bossVisual : source == null ? null : source.sprite; // 지정 이미지 또는 기존 대체 이미지
            GameObject instance = new GameObject("PrototypeFinalBoss"); // 실제 보스 객체 생성
            instance.transform.position = spawnPosition; // 실제 안전 출현 위치
            instance.transform.localScale = Vector3.one * 2.8f; // 시험용 보스 두 배 크기
            SpriteRenderer renderer = instance.AddComponent<SpriteRenderer>(); // 실제 보스 렌더러
            renderer.sprite = sprite; // 임시 보스 시각 자료
            renderer.color = bossTint; // 실제 설정한 보스 색상
            renderer.sortingOrder = 8; // 실제 렌더 순서
            CircleCollider2D collider = instance.AddComponent<CircleCollider2D>(); // 실제 보스 충돌체
            collider.isTrigger = true; // 실제 투사체 접촉 설정
            Rigidbody2D physicsBody = instance.AddComponent<Rigidbody2D>(); // 실제 보스 물리체
            physicsBody.bodyType = RigidbodyType2D.Kinematic; // 기존 적 이동 방식
            physicsBody.gravityScale = 0f; // 중력 제거
            physicsBody.useFullKinematicContacts = true; // 실제 투사체 접촉 허용
            WeaponTarget target = instance.AddComponent<WeaponTarget>(); // 실제 무기 공격 대상
            target.Configure(maximumHealth); // 임시 최대 체력 반영
            instance.AddComponent<EnemyHitFlash>(); // 실제 피격 표시 연결
            instance.AddComponent<EnemyVisualMotion>(); // 충돌체와 독립된 임시 시각 모션
            instance.AddComponent<WeaponTargetStatusController>().ConfigureColdFreezeResistance(true, coldFreezeSlowMultiplier); // 보스 냉기 빙결 저항 설정
            instance.AddComponent<WeaponTargetKnockbackController>().Configure(true); // 보스 넉백 면역
            settings = new EarlyEnemySettings // 기존 사수 행동의 임시 설정
            {
                kind = EarlyEnemyKind.Shooter, // 시험용 사격 행동
                health = maximumHealth, // 임시 체력
                damage = damage, // 임시 투사체 피해
                speedRatio = Mathf.Max(0f, speedRatio), // 임시 이동 속도
                attackInterval = Mathf.Max(0.1f, attackInterval), // 임시 사격 간격
                projectileCount = Mathf.Clamp(projectileCount, 1, 9), // 첫 단계 임시 사격 수
                projectileWarningDuration = 0f, // 기존 보스 사격 예고 유지
                projectileSpreadDegrees = Mathf.Clamp(projectileSpreadDegrees, 0f, 180f), // 임시 부채꼴 각도
                opposedProjectileFan = mapBossPattern == 1, // 폐허 보스의 양방향 부채꼴
                enemySprite = alignVisualWithTarget ? bossVisual : null // 보스 이미지 방향 회전 설정
            };
            EarlyEnemyBehaviour behaviour = instance.AddComponent<EarlyEnemyBehaviour>(); // 기존 적 행동 재사용
            behaviour.Configure(settings, player, body, health, projectileVisual != null ? projectileVisual : sprite, keepDistance, projectileSpeed, projectileLifetime, 1f, 0.6f, 3f); // 실제 투사체와 이동 설정
            instance.GetComponent<EnemyVisualMotion>()?.ConfigureBossMotion(); // 실제 보스 전용 표시 시험값 연결
            if (!GameManager.Instance.RegisterFinalBoss(target)) // 실제 보스 등록 확인
            {
                instance.SetActive(false); // 등록 실패 시 공격 차단
                Destroy(instance); // 등록 실패 보스 제거
                settings = null; // 임시 행동 참조 해제
                return; // 승리 대상 없는 출현 방지
            }
            boss = target; // 실제 보스 참조 저장
            spawned = true; // 중복 출현 방지
            instance.AddComponent<PrototypeBossChargeAttack>().Configure(this, player, body, health, behaviour, chargeInterval, chargeWarningDuration, chargeDuration, chargeSpeed, phaseTwoChargeSpeedMultiplier, damage, alignVisualWithTarget, mapBossPattern == 2); // 습지 예고 추적을 포함한 실제 돌진 구성
            SnakeBodyFollower follower = body.GetComponent<SnakeBodyFollower>(); // 실제 몸통 경로 추적기 조회
            instance.AddComponent<PrototypeBossBodyPush>().Configure(body, follower, bodyPushDistance, bodyPushRecoveryDuration, bodyPushGraceDuration); // 실제 보스 몸통 밀림 구성
            instance.AddComponent<PrototypeBossHazardSpawner>().Configure(player, body, health, target, hazardInterval, hazardWarningDuration, hazardDuration, hazardDamageInterval, hazardRadius, hazardDamage, mapBossPattern, this, phaseTwoHazardIntervalMultiplier); // 맵별 위험 구역과 단계별 간격 구성
            instance.AddComponent<PrototypeBossSummoner>().Configure(spawner, target, summonInterval, summonMaximum, summonRadius); // 실제 보스 잡몹 소환 구성
        }

        private bool TryFindSafeSpawn(Vector3 playerPosition, SnakeBodyManager body, out Vector3 position) // 실제 보스 출현 후보 검색
        { // 함수 시작
            ContactFilter2D filter = ContactFilter2D.noFilter; // 실제 전체 물리 계층 검색
            filter.useTriggers = true; // 장애물과 뱀 접촉체 포함
            float distance = Mathf.Max(1f, spawnDistance); // 기존 보스 출현 거리
            const float radius = 1.5f; // 보스 충돌 반경과 작은 여유
            for (int index = 0; index < 16; index++) // 기존 위쪽부터 열여섯 방향 확인
            { // 반복 시작
                float radians = index * Mathf.PI * 2f / 16f; // 실제 후보 방향 각도
                Vector3 candidate = playerPosition + new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0f) * distance; // 실제 출현 후보 위치
                if (Vector2.Distance(candidate, playerPosition) < radius + 0.5f) // 실제 머리 주변 여유 확인
                { // 조건 시작
                    continue; // 머리와 가까운 후보 제외
                } // 조건 종료
                bool nearBody = false; // 실제 몸통 주변 차단 상태
                foreach (SnakeSegment segment in body.BodySegments) // 실제 활성 몸통 순회
                { // 반복 시작
                    if (segment != null && segment.gameObject.activeInHierarchy && Vector2.Distance(candidate, segment.transform.position) < radius + 0.5f) // 실제 몸통 주변 여유 확인
                    { // 조건 시작
                        nearBody = true; // 안전하지 않은 몸통 주변
                        break; // 나머지 몸통 검사 종료
                    } // 조건 종료
                } // 반복 종료
                if (nearBody || body.TailSegment != null && body.TailSegment.gameObject.activeInHierarchy && Vector2.Distance(candidate, body.TailSegment.transform.position) < radius + 0.5f) // 실제 몸통과 충돌체 없는 꼬리 확인
                { // 조건 시작
                    continue; // 몸통이나 꼬리 주변 후보 제외
                } // 조건 종료
                spawnOverlaps.Clear(); // 이전 후보의 충돌 목록 초기화
                Physics2D.OverlapCircle(candidate, radius, filter, spawnOverlaps); // 실제 보스 반경의 충돌체 조회
                bool blocked = false; // 실제 후보 차단 상태
                foreach (Collider2D overlap in spawnOverlaps) // 실제 겹친 충돌체 순회
                { // 반복 시작
                    if (overlap == null) // 무효 충돌체 확인
                    { // 조건 시작
                        continue; // 다음 충돌체 확인
                    } // 조건 종료
                    SnakeContactHazard hazard = overlap.GetComponentInParent<SnakeContactHazard>(); // 실제 장애물 접촉 정보
                    if (hazard != null && hazard.HazardType == SnakeContactHazardType.Obstacle || overlap.GetComponentInParent<SnakeMovement>() != null || overlap.GetComponentInParent<SnakeSegment>() != null) // 장애물과 뱀 머리·몸통 확인
                    { // 조건 시작
                        blocked = true; // 안전하지 않은 출현 후보
                        break; // 다음 후보 확인
                    } // 조건 종료
                } // 반복 종료
                if (!blocked) // 실제 안전한 후보 확인
                { // 조건 시작
                    position = candidate; // 실제 보스 출현 위치 반환
                    return true; // 안전 후보 발견
                } // 조건 종료
            } // 반복 종료
            position = default; // 안전 후보 부재 표시
            return false; // 현재 보스 출현 보류
        } // 함수 종료
    }
}
