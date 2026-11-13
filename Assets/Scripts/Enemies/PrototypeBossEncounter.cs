using ProjectEpsilon.Combat; // 실제 무기 공격 대상
using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Player; // 플레이어와 공용 체력
using ProjectEpsilon.World; // 맵별 시험 보스 데이터
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
        private EarlyEnemyWaveSpawner spawner; // 실제 20분 전환기
        private WeaponTarget boss; // 실제 공격 가능한 보스
        private EarlyEnemySettings settings; // 실제 보스 행동 설정
        private readonly PrototypeBossPhase phase = new PrototypeBossPhase(); // 보스 체력 단계 상태
        private bool spawned; // 보스 출현 완료 여부
        private int mapBossPattern; // 선택된 맵의 보스 패턴
        public WeaponTarget Boss => boss; // 실제 생성 보스 조회
        public bool IsPhaseTwo => phase.IsPhaseTwo; // 현재 보스 단계 조회

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
            bossTint = profile.BossTint; // 임시 보스 구분 색상
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
            SpriteRenderer source = player.GetComponentInChildren<SpriteRenderer>(); // 플레이어 스프라이트 조회
            Sprite sprite = bossVisual != null ? bossVisual : source == null ? null : source.sprite; // 지정 이미지 또는 기존 대체 이미지
            GameObject instance = new GameObject("PrototypeFinalBoss"); // 실제 보스 객체 생성
            instance.transform.position = player.transform.position + Vector3.up * Mathf.Max(1f, spawnDistance); // 시험용 출현 위치
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
    }
}
